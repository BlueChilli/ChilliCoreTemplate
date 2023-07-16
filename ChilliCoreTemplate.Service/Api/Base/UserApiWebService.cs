using AutoMapper;
using ChilliSource.Core.Extensions;
using ChilliCoreTemplate.Data;
using ChilliCoreTemplate.Data.EmailAccount;
using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.Api;
using ChilliCoreTemplate.Models.EmailAccount;
using ChilliCoreTemplate.Service.EmailAccount;
using ChilliSource.Cloud.Core;
using System;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using ChilliSource.Cloud.Core.LinqMapper;
using System.Security.Principal;
using ChilliCoreTemplate.Models.Api.OAuth;

namespace ChilliCoreTemplate.Service.Api
{
    public class UserApiWebService : Service<DataContext>
    {
        private readonly AccountService _accountService;
        private readonly UserKeyHelper _userKeyHelper;
        private readonly FileStoragePath _fileStoragePath;
        private readonly UserSessionService _session;
        private readonly ProjectSettings _config;

        private int? CompanyId => User?.UserData()?.CompanyId;
        private int? UserId => User?.UserData()?.UserId;

        public UserApiWebService(IPrincipal user, DataContext context, AccountService accountSvc, UserKeyHelper userKeyHelper, FileStoragePath fileStoragePath, UserSessionService session, ProjectSettings config)
            : base(user, context)
        {
            _accountService = accountSvc;
            _accountService.IsApi = true;
            _userKeyHelper = userKeyHelper;
            _fileStoragePath = fileStoragePath;
            _session = session;
            _config = config;
        }

        public IQueryable<User> VisibleAccounts()
        {
            return _accountService.VisibleUsers();
        }

        public ServiceResult<UserAccountApiModel> GetAccount(int? accountId, bool onlyVisible = true)
        {
            var query = onlyVisible ? this.VisibleAccounts() : Context.Users;

            var model = query.Where(a => a.Id == accountId)
                        .Materialize<User, UserAccountApiModel>()
                        .FirstOrDefault();

            if (model == null)
                return ServiceResult<UserAccountApiModel>.AsError("Account not found or access denied.", System.Net.HttpStatusCode.NotFound);

            return ServiceResult<UserAccountApiModel>.AsSuccess(model);
        }

        public async Task<ServiceResult<UserAccountApiModel>> GetAccountAsync(int? accountId, bool onlyVisible = true)
        {
            var query = onlyVisible ? this.VisibleAccounts() : Context.Users;

            if (accountId == null)
                return ServiceResult<UserAccountApiModel>.AsError("Account not found or access denied.", System.Net.HttpStatusCode.NotFound);

            var model = await query.Where(a => a.Id == accountId)
                                .Materialize<User, UserAccountApiModel>()
                                .FirstOrDefaultAsync();

            if (model == null)
                return ServiceResult<UserAccountApiModel>.AsError("Account not found or access denied.", System.Net.HttpStatusCode.NotFound);

            return ServiceResult<UserAccountApiModel>.AsSuccess(model);
        }

        public ServiceResult<UserDataPrincipal> Login(NewSessionApiModel model, Action<UserDataPrincipal> loginAction)
        {
            return _accountService.Login(new SessionEditModel()
            {
                Email = model.Email,
                Password = model.Password,
                DeviceId = model.DeviceId
            }, loginAction, isApi: true);
        }

        public SessionSummaryApiModel GetSessionSummary()
        {
            var session = _session.Get(User.Session().Id);
            return GetSessionSummary(session);
        }

        public SessionSummaryApiModel GetSessionSummary(UserDataPrincipal principal, bool includeUserKey = false)
        {
            Guid.TryParse(principal.Id, out var sessionGuid);
            return GetSessionSummary(principal.UserData, includeUserKey ? (Guid?)sessionGuid : null);
        }

        private SessionSummaryApiModel GetSessionSummary(SessionInfo sessionInfo)
        {
            var session = GetSessionSummary(sessionInfo.UserData, Guid.Parse(sessionInfo.Id));
            session.ExpiresOn = sessionInfo.SessionExpiryOn;
            return session;
        }

        private SessionSummaryApiModel GetSessionSummary(UserData userData, Guid? sessionId)
        {
            return new SessionSummaryApiModel
            {
                UserId = userData.UserId,
                UserKey = sessionId.HasValue ? _userKeyHelper.ProtectGuid(sessionId.Value) : null,
                FirstName = userData.FirstName,
                LastName = userData.LastName,
                FullName = userData.Name,
                ProfilePhotoPath = _fileStoragePath.GetImagePath(userData.ProfilePhotoPath, true),
                Roles = userData.Roles,
                Status = userData.Status,
                Email = userData.Email,
                Phone = userData.Phone,
                Impersonator = userData.Impersonator?.ImpersonatorSummary(userData.Impersonator)
            };
        }
        public ServiceResult<UserAccountApiModel> Create(RegistrationApiModel model)
        {
            var registrationModel = Mapper.Map<RegistrationViewModel>(model);
            registrationModel.Roles = model.GetRole();
            var response = _accountService.Create(registrationModel);
            if (!response.Success) return ServiceResult<UserAccountApiModel>.CopyFrom(response);

            return this.GetAccount(response.Result.UserId, onlyVisible: false);
        }

        public async Task<ServiceResult<UserAccountApiModel>> Create(OAuthRegisterApiModel model)
        {
            var request = await _accountService.OAuth_Authenticate(model.Provider.Value, OAuthMode.Register, model.Platform, model.Token, model.Code, model.User);
            if (!request.Success) return ServiceResult<UserAccountApiModel>.CopyFrom(request);

            return await this.GetAccountAsync(request.Result.Id, onlyVisible: false);
        }

        public ServiceResult<UserAccountApiModel> Invite(InviteEditApiModel model)
        {
            var inviteModel = Mapper.Map<InviteEditModel>(model, opts => opts.Items["CompanyId"] = CompanyId);

            var response = _accountService.Invite(inviteModel, true);
            if (!response.Success) return ServiceResult<UserAccountApiModel>.CopyFrom(response);
            return this.GetAccount(response.Result.Id, onlyVisible: false);
        }

        public ServiceResult Token_Create(TokenEditApiModel model)
        {
            User user;
            switch (model.Type)
            {
                case UserTokenType.Password:
                    return _accountService.Password_ResetRequest(model.Email);
                case UserTokenType.Activate:
                    user = _accountService.GetAccountByEmail(model.Email);
                    _accountService.SendRegistrationCompleteEmail(user);
                    return ServiceResult.AsSuccess();
                case UserTokenType.OneTimePassword:
                    user = _accountService.GetAccountByEmail(model.Email);
                    return _accountService.Password_OneTime(user);
                default:
                    return ServiceResult.AsError($"Token type {model.Type} is not supported");
            }
        }

        public ServiceResult<string> Token_Get(UserTokenModel model)
        {
            var data = _accountService.User_GetToken(model);

            var token = data?.Item2;
            if (token != null) return ServiceResult<string>.AsSuccess(token.Token.ToShortGuid());

            return ServiceResult<string>.AsError(error: data == null ? "Account not found or access denied" : "Code is invalid or has expired");
        }

        public ServiceResult<UserAccountApiModel> GetByToken(UserTokenModel model)
        {
            var accountRequest = _accountService.User_GetAccountByEmailToken(model);
            return GetByX(accountRequest);
        }

        public ServiceResult<UserAccountApiModel> GetByCode(UserTokenModel model)
        {
            var accountRequest = _accountService.User_GetAccountByOneTimePassword(model);
            return GetByX(accountRequest);
        }

        private ServiceResult<UserAccountApiModel> GetByX(ServiceResult<User> request)
        {
            if (!request.Success) return ServiceResult<UserAccountApiModel>.CopyFrom(request);

            _accountService.Activate(request.Result);

            return GetAccount(request.Result.Id, onlyVisible: false);
        }

        public ServiceResult<UserAccountApiModel> Update(PersistUserAccountApiModel model)
        {
            var accountResponse = this.GetAccount(model.Id);
            if (!accountResponse.Success)
                return accountResponse;

            var response = _accountService.Update(new AccountDetailsEditModel()
            {
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Phone = model.Phone,
                ProfilePhotoFile = model.ProfilePhotoFile
            }, model.Id);

            if (!response.Success)
                return ServiceResult<UserAccountApiModel>.CopyFrom(response);

            return this.GetAccount(model.Id);
        }

        public ServiceResult<UserAccountApiModel> PatchUser(PatchAccountApiModel model)
        {
            if (model.PasswordSpecified)
            {
                var passwordRequest = _accountService.Password_Change(new ChangePasswordViewModel()
                {
                    UserId = model.Id,
                    CurrentPassword = model.CurrentPassword,
                    NewPassword = model.Password
                });

                if (!passwordRequest.Success)
                    return ServiceResult<UserAccountApiModel>.CopyFrom(passwordRequest);
            }

            if (model.EmailSpecified || model.PhoneSpecified || model.NameSpecified)
            {
                var account = _accountService.GetAccount(model.Id);
                if (model.EmailSpecified)
                {
                    if (_accountService.Exists(model.Email, model.Id)) return ServiceResult<UserAccountApiModel>.AsError("The email address chosen is already taken");
                    account.Email = model.Email;
                }
                if (model.PhoneSpecified) account.Phone = String.IsNullOrEmpty(model.Phone) ? null : model.Phone;
                if (model.NameSpecified)
                {
                    account.FirstName = model.FirstName;
                    account.LastName = model.LastName;
                }
                Context.SaveChanges();
                _accountService.Session_Clear(User.Session()?.Id);
            }

            if (model.Status != null)
            {
                var updateRequest = _accountService.Update_Status(model.Id, model.Status.Value, isApi: true);
                if (!updateRequest.Success) return ServiceResult<UserAccountApiModel>.CopyFrom(updateRequest);
            }

            return this.GetAccount(model.Id);
        }

        public ServiceResult<UserAccountApiModel> PatchUser(PatchAccountTokenApiModel model)
        {
            var userRequest = _accountService.User_GetAccountByEmailToken(model, includeDeleted: !String.IsNullOrEmpty(model.Password));
            if (!userRequest.Success) return ServiceResult<UserAccountApiModel>.CopyFrom(userRequest);

            int userId = userRequest.Result.Id;

            if (model.SetProperties().Any(x => x.IsIn(nameof(model.FirstName), nameof(model.LastName), nameof(model.Phone))))
            {
                var editRequest = _accountService.GetForEdit(userId);
                if (!editRequest.Success) return ServiceResult<UserAccountApiModel>.CopyFrom(editRequest);

                var editModel = editRequest.Result;
                editModel.FirstName = model.FirstName;
                editModel.LastName = model.LastName;

                var updateResponse = _accountService.Update(editModel, userId, visibleOnly: false);
                if (!updateResponse.Success) return ServiceResult<UserAccountApiModel>.CopyFrom(updateResponse);
            }

            if (model.IsSetProperty(nameof(model.Password)))
            {
                _accountService.Password_Set(userRequest.Result, model.Password);
            }

            return this.GetAccount(userId, onlyVisible: false);
        }

        public async Task<ServiceResult<object>> Delete(DeleteUserApiModel model)
        {
            var user = _accountService.GetAccount(model.Id);

            if (user == null) return ServiceResult.AsError("User not found");

            if (!user.ConfirmPassword(model.Password, _config.ProjectId.Value)) return ServiceResult.AsError("Password was not verified");
            await _accountService.SoftDeleteAsync(model.Id);

            _session.Delete(User.Session().Id);

            return ServiceResult<object>.AsSuccess();
        }

    }

}
