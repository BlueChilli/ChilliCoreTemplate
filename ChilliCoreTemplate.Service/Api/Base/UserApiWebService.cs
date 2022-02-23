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

        private int? CompanyId => User?.UserData()?.CompanyId;
        private int? UserId => User?.UserData()?.UserId;

        public UserApiWebService(IPrincipal user, DataContext context, AccountService accountSvc, UserKeyHelper userKeyHelper, FileStoragePath fileStoragePath, UserSessionService session)
            : base(user, context)
        {
            _accountService = accountSvc;
            _userKeyHelper = userKeyHelper;
            _fileStoragePath = fileStoragePath;
            _session = session;
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
            registrationModel.IsApi = true;
            var response = _accountService.Create(registrationModel);
            if (!response.Success) return ServiceResult<UserAccountApiModel>.CopyFrom(response);

            return this.GetAccount(response.Result.UserId, onlyVisible: false);
        }

        public async Task<ServiceResult<UserAccountApiModel>> Create(OAuthRegisterApiModel model)
        {
            var request = await _accountService.OAuth_Authenticate(model.Provider.Value, OAuthMode.Register, model.Token, model.Code);
            if (!request.Success) return ServiceResult<UserAccountApiModel>.CopyFrom(request);

            return await this.GetAccountAsync(request.Result.Id, onlyVisible: false);
        }

        public ServiceResult<UserAccountApiModel> Invite(InviteEditApiModel model)
        {
            var inviteModel = Mapper.Map<InviteEditModel>(model);
            inviteModel.InviteRole = new InviteRoleViewModel { CompanyId = CompanyId, Role = Role.CompanyAdmin };

            var response = _accountService.Invite(inviteModel, true);
            if (!response.Success) return ServiceResult<UserAccountApiModel>.CopyFrom(response);
            return this.GetAccount(response.Result.Id, onlyVisible: false);
        }

        public ServiceResult RequestNewPassword(TokenEditApiModel model)
        {
            var forgotPassword = new ResetPasswordRequestModel()
            {
                Email = model.Email,
                IsWebApi = true
            };

            return _accountService.Password_ResetRequest(forgotPassword);
        }

        public ServiceResult Token_Create(TokenEditApiModel model)
        {
            User user;
            switch (model.Type)
            {
                case UserTokenType.Password:
                    var forgotPassword = new ResetPasswordRequestModel()
                    {
                        Email = model.Email,
                        IsWebApi = true
                    };
                    return _accountService.Password_ResetRequest(forgotPassword);
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
            var user = User.UserData();

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
            var userRequest = _accountService.User_GetByEmailToken(model);
            if (!userRequest.Success) return ServiceResult<UserAccountApiModel>.CopyFrom(userRequest);

            int userId = userRequest.Result.Id;

            if (!String.IsNullOrEmpty(model.FirstName) || !String.IsNullOrEmpty(model.LastName))
            {
                var editRequest = _accountService.GetForEdit(userId);
                if (!editRequest.Success) return ServiceResult<UserAccountApiModel>.CopyFrom(editRequest);

                var editModel = editRequest.Result;
                editModel.FirstName = model.FirstName;
                editModel.LastName = model.LastName;

                var updateResponse = _accountService.Update(editModel, userId, visibleOnly: false);
                if (!updateResponse.Success) return ServiceResult<UserAccountApiModel>.CopyFrom(updateResponse);
            }

            if (!String.IsNullOrEmpty(model.Password))
            {
                var response = _accountService.Password_Reset(new ResetPasswordViewModel()
                {
                    Id = userId,
                    Email = model.Email,
                    Token = model.Token,
                    NewPassword = model.Password,
                    ConfirmPassword = model.Password
                });

                if (!response.Success) return ServiceResult<UserAccountApiModel>.CopyFrom(response);
            }

            return this.GetAccount(userId, onlyVisible: false);
        }

        public async Task<ServiceResult<object>> Delete(DeleteUserApiModel model)
        {
            await _accountService.SoftDeleteAsync(model.Id);
            return ServiceResult<object>.AsSuccess();
        }

    }

}
