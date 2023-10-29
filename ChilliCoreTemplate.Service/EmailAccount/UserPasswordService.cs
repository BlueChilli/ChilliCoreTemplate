using ChilliSource.Cloud.Core;
using ChilliCoreTemplate.Data;
using ChilliCoreTemplate.Data.EmailAccount;
using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.EmailAccount;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChilliCoreTemplate.Service.EmailAccount
{
    public partial class AccountService : Service<DataContext>
    {
        public ServiceResult<int> Password_Reset(ResetPasswordViewModel model, bool sendEmail = true)
        {
            var userRequest = User_GetAccountByEmailToken(new UserTokenModel { Email = model.Email, Token = model.Token }, includeDeleted: true);

            if (!userRequest.Success) return ServiceResult<int>.AsError(userRequest.Error);

            var user = userRequest.Result;

            Password_Set(user, model.NewPassword, sendEmail);

            return ServiceResult<int>.AsSuccess(user.Id);
        }

        public ServiceResult Password_Change(ChangePasswordViewModel model)
        {
            var user = GetAccount(model.UserId, includeDeleted: true);

            if (!user.ConfirmPassword(model.CurrentPassword, _config.ProjectId.Value))
            {
                return ServiceResult.AsError("Current password is not correct");
            }

            if (!user.SetPassword(model.NewPassword, _config.ProjectId.Value))
            {
                return ServiceResult.AsError("New password cannot be same as current password");
            }

            Password_Set(user, model.NewPassword);

            return ServiceResult.AsSuccess();
        }

        internal void Password_Set(User user, string password, bool sendEmail = true)
        {
            user.SetPassword(password, _config.ProjectId.Value);

            if (user.Id != 0)
            {
                Invite_Confirm(user);
                if (user.Status == UserStatus.Deleted) user.Status = UserStatus.Activated;

                Context.SaveChanges();

                Activity_Add(new UserActivity { UserId = user.Id, ActivityType = ActivityType.Update, EntityId = user.Id, EntityType = EntityType.Password });

                if (sendEmail && user.Status != UserStatus.Anonymous) QueueMail(RazorTemplates.PasswordChanged, user.Email, new RazorTemplateDataModel<AccountViewModel> { Data = GetSingle<AccountViewModel, User>(user) });
            }
        }

        public ServiceResult Password_ResetRequest(string email)
        {
            var account = GetAccountByEmail(email, includeDeleted: true);
            return Password_ResetRequest(account);
        }

        private ServiceResult Password_ResetRequest(User account)
        {
            var request = Password_SetRequestToken(account, null, out bool wasExpired);

            if (request.Success)
            {
                var emailModel = new ResetPasswordRequestModel { Email = account.Email, Token = request.Result };

                if (wasExpired)
                    QueueMail(RazorTemplates.ResetPassword, account.Email, new RazorTemplateDataModel<ResetPasswordRequestModel>() { Data = emailModel });
                else
                    QueueMail_Distinct(RazorTemplates.ResetPassword, account.Email, new RazorTemplateDataModel<ResetPasswordRequestModel>() { Data = emailModel }, new TimeSpan(0, 15, 0));
            }

            return ServiceResult.AsSuccess();
        }

        public ServiceResult<Guid> Password_SetRequestToken(int userId, TimeSpan? expiryTime = null)
        {
            var account = GetAccount(userId);
            var result = Password_SetRequestToken(account, expiryTime, out _);

            return result;
        }

        private ServiceResult<Guid> Password_SetRequestToken(User account, TimeSpan? expiryTime, out bool wasExpired)
        {
            if (account == null)
            {
                wasExpired = false;
                return ServiceResult<Guid>.AsError("Email address not registered.");
            }

            if (expiryTime == null) expiryTime = TimeSpan.FromMinutes(60);

            var result = Token_Add(account, UserTokenType.Password, expiryTime, out wasExpired);
            Context.SaveChanges();

            return ServiceResult<Guid>.AsSuccess(result);
        }

        public ServiceResult Password_OneTime(User user)
        {
            if (user == null) return ServiceResult.AsError("Account not registered.");
            var token = Token_Add(user, UserTokenType.OneTimePassword, new TimeSpan(0, 10, 0));
            Context.SaveChanges();

            if (!String.IsNullOrEmpty(user.Email))
                QueueMail(RazorTemplates.OneTimePassword, user.Email, new RazorTemplateDataModel<OneTimePasswordModel> { Data = new OneTimePasswordModel(token) });
            else if (!String.IsNullOrEmpty(user.Phone))
                _sms.Queue(RazorTemplates.OneTimePassword_Sms, user.Id, user.Phone, new RazorTemplateDataModel<OneTimePasswordModel> { Data = new OneTimePasswordModel(token) });

            return ServiceResult.AsSuccess();
        }

    }
}
