using AutoMapper;
using ChilliCoreTemplate.Data;
using ChilliCoreTemplate.Data.EmailAccount;
using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.Api.OAuth;
using ChilliCoreTemplate.Models.EmailAccount;
using ChilliSource.Cloud.Core;
using ChilliSource.Core.Extensions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
//using AppleAuth;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Service.EmailAccount
{
    public partial class AccountService : Service<DataContext>
    {

        public static void OAuth_AutoMapperConfigure(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<OAuthUserModel, RegistrationViewModel>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => Role.User));
            cfg.CreateMap<OAuthGoogleUserModel, OAuthUserModel>();
            cfg.CreateMap<OAuthFacebookUserModel, OAuthUserModel>()
                .ForMember(dest => dest.ProfilePhotoUrl, opt => opt.MapFrom(src => src.Picture == null ? null : src.Picture.Data.Url));
            //cfg.CreateMap<AppleAuth.TokenObjects.UserInformation, OAuthUserModel>()
            //    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.UserID))
            //    .ForMember(dest => dest.EmailIsVerified, opt => opt.MapFrom(src => src.EmailVerified));
        }

        public ServiceResult<string> OAuth_Url(OAuthUrlApiModel model, OAuthMode mode, string state = "")
        {
            var provider = model.Provider;
            var providerUrl = String.Empty;
            var responseType = "code";
            var responseMode = String.Empty;
            var loginHint = String.Empty;
            var scope = String.Empty;
            var oAuthConfig = _config.OAuthsSettings.OAuths.Where(x => x.Provider == provider).FirstOrDefault();
            switch(provider)
            {
                case OAuthProvider.Google:
                    providerUrl = "https://accounts.google.com/o/oauth2/v2/auth";
                    loginHint = model.Email;
                    scope = "https://www.googleapis.com/auth/userinfo.email https://www.googleapis.com/auth/userinfo.profile";
                    break;
                case OAuthProvider.Facebook:
                    providerUrl = "https://www.facebook.com/v9.0/dialog/oauth";
                    scope = "email";
                    break;
                //case OAuthProvider.Apple:
                //    providerUrl = "https://appleid.apple.com/auth/authorize";
                //    responseType = "code id_token";
                //    responseMode = "form_post";
                //    break;
                default: return ServiceResult<string>.AsError(error: $"Provider {provider} not supported");
            }
            var parameters = new Dictionary<string, string> { { "client_id", oAuthConfig.ClientId }, { "response_type", responseType }, { "redirect_uri", OAuth_Url_Redirect }, { "state", $"{provider}|{mode}|{model.RedirectUrl}|{model.UserKey}|{state}" } };
            if (!String.IsNullOrEmpty(responseMode)) parameters.Add("response_mode", responseMode);
            if (!String.IsNullOrEmpty(loginHint)) parameters.Add("login_hint", loginHint);
            if (!String.IsNullOrEmpty(scope)) parameters.Add("scope", scope);
            var url = QueryHelpers.AddQueryString(providerUrl, parameters);
            return ServiceResult<string>.AsSuccess(url);
        }

        private string OAuth_Url_Redirect => $"{_config.BaseUrl}/api/v1/usersessions/byoauth/auth";

        public async Task<ServiceResult<UserDataPrincipal>> OAuth_Login(OAuthLoginApiModel model, Action<UserDataPrincipal> loginAction)
        {
            var request = await OAuth_Authenticate(model.Provider.Value, OAuthMode.Login, model.Token, model.Code, sessionEmail: User.UserData()?.Email);
            if (!request.Success) return ServiceResult<UserDataPrincipal>.CopyFrom(request);

            var user = request.Result;
            var principal = CreatePrincipal(user, CreateUserDeviceId(user, model.DeviceId));
            loginAction(principal);

            return ServiceResult<UserDataPrincipal>.AsSuccess(principal);
        }

        internal async Task<ServiceResult<User>> OAuth_Authenticate(OAuthProvider provider, OAuthMode mode = OAuthMode.Any, string token = null, string code = null, string sessionEmail = null, Role role = Role.User, string companyName = null)
        {
            var oAuthConfig = _config.OAuthsSettings.OAuths.Where(x => x.Provider == provider).FirstOrDefault();
            ServiceResult<OAuthUserModel> oAuthUserRequest = null;
            switch (provider)
            {
                case OAuthProvider.Google:
                    oAuthUserRequest = await OAuth_Code_Google(token, code, oAuthConfig);
                    break;
                case OAuthProvider.Facebook:
                    oAuthUserRequest = await OAuth_Code_Facebook(token, code, oAuthConfig);
                    break;
                //case OAuthProvider.Apple:
                //    oAuthUserRequest = await OAuth_Code_Apple(model, oAuthConfig);
                //    break;
                default: return ServiceResult<User>.AsError(error: $"Provider {provider} not supported");
            }
            if (!oAuthUserRequest.Success) return ServiceResult<User>.CopyFrom(oAuthUserRequest);
            var oAuthUser = oAuthUserRequest.Result;
            if (oAuthUser == null || String.IsNullOrEmpty(oAuthUser.Email)) return ServiceResult<User>.AsError("There was an error retrieving email address from social account.");

            var existingOAuth = await Context.UserOAuths
                .Include(x => x.User)
                .ThenInclude(x => x.Tokens)
                .Include(x => x.User.UserRoles).ThenInclude(u => u.Company)
                .Where(x => x.OAuthIdHash == oAuthUser.IdHash && x.OAuthId == oAuthUser.Id && x.Provider == provider)
                .FirstOrDefaultAsync();
            var user = existingOAuth == null ? null : existingOAuth.User;
            if (existingOAuth == null)
            {
                var existingUser = GetAccountByEmail(oAuthUser.Email);
                if (existingUser == null)
                {
                    if (mode == OAuthMode.Login) return ServicesLibrary.AsError<User>(error: $"Account with email address {oAuthUser.Email} is not registered.", key: "ACCOUNT_NOTREGISTERED_ERROR");
                    var registerModel = Mapper.Map<RegistrationViewModel>(oAuthUser);
                    registerModel.Roles = role;
                    registerModel.CompanyName = companyName;
                    var newUserRequest = Create(registerModel, sendEmail: !oAuthUser.EmailIsVerified);
                    if (!newUserRequest.Success) return ServiceResult<User>.CopyFrom(newUserRequest);
                    user = GetAccount(newUserRequest.Result.UserId);
                    if (oAuthUser.EmailIsVerified) Activate(user.Id);
                }
                else
                {
                    if (mode == OAuthMode.Register) return ServicesLibrary.AsError<User>(error: $"Account with email address {oAuthUser.Email} is already registered.", key: "ACCOUNT_ALREADYREGISTERED_ERROR");
                    if (!existingUser.Email.Same(sessionEmail))
                        return ServicesLibrary.AsError<User>(error: $"To link account with email address {existingUser.Email}, you must be logged in as this account", key: "ACCOUNT_LINK_ERROR");
                    user = existingUser;
                }
                Context.UserOAuths.Add(new UserOAuth
                {
                    UserId = user.Id,
                    Provider = provider,
                    OAuthId = oAuthUser.Id,
                    Token = oAuthUser.Token
                });
            }
            else if (mode == OAuthMode.Register) 
                return ServicesLibrary.AsError<User>(error: $"Account with email address {oAuthUser.Email} is already registered.", key: "ACCOUNT_ALREADYREGISTERED_ERROR");

            if (String.IsNullOrEmpty(user.FirstName) && !String.IsNullOrEmpty(oAuthUser.FirstName))
                user.FirstName = oAuthUser.FirstName;
            if (String.IsNullOrEmpty(user.LastName) && !String.IsNullOrEmpty(oAuthUser.LastName))
                user.LastName = oAuthUser.LastName;
            if (String.IsNullOrEmpty(user.ProfilePhotoPath) && !String.IsNullOrEmpty(oAuthUser.ProfilePhotoUrl))
            {
                user.ProfilePhotoPath = _fileStorage.Save(new StorageCommand { Folder = ProfilePhotoFolder }.SetUrlSource(oAuthUser.ProfilePhotoUrl));
            }
            await Context.SaveChangesAsync();
            return ServiceResult<User>.AsSuccess(user);
        }

        public async Task<ServiceResult<OAuthCodeResultApiModel>> OAuth_Code(OAuthCodeApiModel model)
        {
            if (!String.IsNullOrEmpty(model.GetError)) return ServicesLibrary.AsError<OAuthCodeResultApiModel>(error: model.GetErrorMessage, key: model.GetError);
            var state = model.State.Split('|');
            var provider = EnumHelper.Parse<OAuthProvider>(state[0]);
            var mode = EnumHelper.Parse<OAuthMode>(state[1]);
            var sessionEmail = User?.UserData()?.Email;
            if (String.IsNullOrEmpty(sessionEmail) && state.Length >= 4)
            {
                var session = _session.GetByUserKey(state[3]);
                sessionEmail = session?.UserData.Email;
            }
            var role = Role.User;
            String companyName = null;
            if (state.Length >= 5)
            {
                var registrationState = state[4].Split(':');
                companyName = registrationState[0];
                role = EnumHelper.Parse<Role>(registrationState[1]);
            }

            var request = await OAuth_Authenticate(provider, mode, code: model.Code, sessionEmail: sessionEmail, role: role, companyName: companyName);
            if (!request.Success) return ServiceResult<OAuthCodeResultApiModel>.CopyFrom(request);

            var user = request.Result;
            var token = Token_Add(user, UserTokenType.Login);
            await Context.SaveChangesAsync();
            return ServiceResult<OAuthCodeResultApiModel>.AsSuccess(new OAuthCodeResultApiModel
            {
                Token = token,
                Email = user.Email
            });
        }

        private async Task<ServiceResult<OAuthUserModel>> OAuth_Code_Google(string token, string code, OAuthsConfigurationElement oAuthConfig)
        {
            var client = new RestClient();
            client.UseNewtonsoftJson();
            if (token == null)
            {
                var request = new RestRequest("https://oauth2.googleapis.com/token", Method.Post);
                request.AddJsonBody(new
                {
                    code = code,
                    client_id = oAuthConfig.ClientId,
                    client_secret = oAuthConfig.ClientSecret,
                    redirect_uri = OAuth_Url_Redirect,
                    grant_type = "authorization_code"
                });
                var result = await client.ExecuteAsync<OAuthTokenGoogleModel>(request);
                if (!result.IsSuccessful) return ServiceResult<OAuthUserModel>.AsError(result.GetError());
                token = result.Data.AccessToken;
            }

            var userRequest = new RestRequest("https://www.googleapis.com/oauth2/v2/userinfo", Method.Get);
            userRequest.AddQueryParameter("access_token", token);
            var userResult = await client.ExecuteAsync<OAuthGoogleUserModel>(userRequest);
            if (!userResult.IsSuccessful) return ServiceResult<OAuthUserModel>.AsError(userResult.GetError());

            var user = Mapper.Map<OAuthUserModel>(userResult.Data);
            user.Token = token;
            return ServiceResult<OAuthUserModel>.AsSuccess(user);
        }

        private async Task<ServiceResult<OAuthUserModel>> OAuth_Code_Facebook(string token, string code, OAuthsConfigurationElement oAuthConfig)
        {
            var client = new RestClient("https://graph.facebook.com/v9.0/");
            client.UseNewtonsoftJson();
            if (token == null)
            {
                var request = new RestRequest("oauth/access_token", Method.Post);
                request.AddJsonBody(new
                {
                    code = code,
                    client_id = oAuthConfig.ClientId,
                    client_secret = oAuthConfig.ClientSecret,
                    redirect_uri = OAuth_Url_Redirect
                });
                var result = await client.ExecuteAsync<OAuthTokenFacebookModel>(request);
                if (!result.IsSuccessful) return result.Data?.HasError ?? false ? ServiceResult<OAuthUserModel>.CopyFrom(result.Data.GetError) : ServiceResult<OAuthUserModel>.AsError(result.GetError());
                token = result.Data.AccessToken;
            }

            var userRequest = new RestRequest("me", Method.Get);
            userRequest.AddQueryParameter("access_token", token);
            userRequest.AddQueryParameter("fields", "id,first_name,last_name,email,picture.type(large)");
            var userResult = await client.ExecuteAsync<OAuthFacebookUserModel>(userRequest);
            if (!userResult.IsSuccessful) return ServiceResult<OAuthUserModel>.AsError(userResult.GetError());

            if (String.IsNullOrEmpty(userResult.Data?.Email))
            {
                var deleteRequest = new RestRequest("me/permissions", Method.Delete);
                deleteRequest.AddQueryParameter("access_token", token);
                _ = client.ExecuteAsync(deleteRequest);
            }

            var user = Mapper.Map<OAuthUserModel>(userResult.Data);
            user.Token = token;
            return ServiceResult<OAuthUserModel>.AsSuccess(user);
        }

        //private async Task<ServiceResult<OAuthUserModel>> OAuth_Code_Apple(OAuthCodeApiModel model, OAuthsConfigurationElement oAuthConfig)
        //{
        //    var provider = new AppleAuthProvider(oAuthConfig.ClientId, oAuthConfig.ClientJWT.Issuer, oAuthConfig.ClientJWT.KeyId, OAuth_Url_Redirect, model.State);
        //    var url = provider.GetButtonHref();
        //    var result = await provider.GetAuthorizationToken(model.Code, oAuthConfig.ClientJWT.Key);

        //    var user = Mapper.Map<OAuthUserModel>(result.UserInformation);
        //    user.Token = result.Token;
        //    return ServiceResult<OAuthUserModel>.AsSuccess(user);
        //}

        public ServiceResult OAuth_Deauth(OAuthDeauthApiModel model)
        {
            var provider = model.Provider;
            var oAuthConfig = _config.OAuthsSettings.OAuths.Where(x => x.Provider == provider).FirstOrDefault();

            switch (provider)
            {
                case OAuthProvider.Facebook:
                    return OAuth_Deauth_Facebook(model, oAuthConfig);
                default: return ServiceResult.AsError(error: $"Provider {provider} not supported");
            }
        }

        private ServiceResult OAuth_Deauth_Facebook(OAuthDeauthApiModel model, OAuthsConfigurationElement oAuthConfig)
        {
            var signed_request = model.SignedRequest;

            //https://stackoverflow.com/questions/19046207/how-to-get-the-facebook-signed-request-in-c-sharp
            if (signed_request.Contains("."))
            {
                string[] split = signed_request.Split('.');

                string signatureRaw = FixBase64String(split[0]);
                string dataRaw = FixBase64String(split[1]);

                // the decoded signature
                byte[] signature = Convert.FromBase64String(signatureRaw);

                byte[] dataBuffer = Convert.FromBase64String(dataRaw);

                // JSON object
                var data = Encoding.UTF8.GetString(dataBuffer).FromJson<OAuthFacebookDeauthModel>();

                byte[] appSecretBytes = Encoding.UTF8.GetBytes(oAuthConfig.ClientSecret);
                var hmac = new HMACSHA256(appSecretBytes);
                byte[] expectedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(split[1]));
                if (expectedHash.SequenceEqual(signature))
                {
                    return OAuth_Deauth(data.UserId);
                }
            }
            return ServiceResult.AsError($"Facebook deauth failed");
        }
        private string FixBase64String(string str)
        {
            while (str.Length % 4 != 0)
            {
                str = str.PadRight(str.Length + 1, '=');
            }
            return str.Replace("-", "+").Replace("_", "/");
        }

        private ServiceResult OAuth_Deauth(string oauthId)
        {
            var hash = CommonLibrary.CalculateHash(oauthId);
            if (hash == null) return ServiceResult.AsError("OAuthId not valid");

            var oauth = Context.UserOAuths.Where(x => x.OAuthIdHash == hash && x.OAuthId == oauthId).FirstOrDefault();
            if (oauth != null)
            {
                Context.UserOAuths.Remove(oauth);
                Context.SaveChanges();
            }
            return ServiceResult.AsSuccess();
        }

    }

}
