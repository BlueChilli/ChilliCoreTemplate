using ChilliCoreTemplate.Data;
using ChilliCoreTemplate.Data.EmailAccount;
using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.EmailAccount;
using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Core.LinqMapper;
using Google.Authenticator;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Service.EmailAccount
{
    public class MfaService : Service<DataContext>
    {
        private readonly ProjectSettings _config;
        private readonly IWebHostEnvironment _environment;
        private readonly UserSessionService _session;

        public MfaService(IPrincipal user, DataContext context, ProjectSettings config, IWebHostEnvironment environment, UserSessionService session)
            : base(user, context)
        {
            _config = config;
            _environment = environment;
            _session = session;
        }

        public static void AddClaim(HttpContext context)
        {
            var claims = context.User.Claims.ToList();
            claims.Add(new Claim("Mfa", "Verified"));
            var identity = new ClaimsIdentity(context.User.Identity, claims);
            context.User = new ClaimsPrincipal(identity);
        }

        public bool IsEnabled()
        {
            return Context.Users.Where(x => x.Id == UserId.Value).Select(x => x.IsMfaEnabled).FirstOrDefault();
        }

        public ServiceResult<MfaSetupModel> Setup()
        {
            var user = Context.Users.Where(x => x.Id == UserId.Value).FirstOrDefault();

            if (user.IsMfaEnabled) return ServiceResult<MfaSetupModel>.AsError("Mfa already enabled");

            var twoFactor = new TwoFactorAuthenticator();

            var name = _environment.IsProduction() ? _config.ProjectDisplayName : $"{_config.ProjectDisplayName} ({_environment.EnvironmentName})";
            var setupInfo = twoFactor.GenerateSetupCode(name, user.Email, TwoFactorKey(user), false, 3);

            var model = new MfaSetupModel
            {
                SetupCode = setupInfo.ManualEntryKey,
                QrCodeImage = setupInfo.QrCodeSetupImageUrl
            };

            return ServiceResult<MfaSetupModel>.AsSuccess(model);
        }

        public async Task<ServiceResult<object>> Setup(MfaSetupModel model)
        {
            var user = await Context.Users.Where(x => x.Id == UserId.Value).FirstOrDefaultAsync();

            if (user == null) return ServiceResult<object>.AsError("User not found");

            if (user.IsMfaEnabled) return ServiceResult.AsError("Mfa already enabled");

            var twoFactor = new TwoFactorAuthenticator();

            if (twoFactor.ValidateTwoFactorPIN(TwoFactorKey(user), model.ConfirmationCode))
            {
                user.IsMfaEnabled = true;
                user.UpdatedDate = DateTime.UtcNow;
                await Context.SaveChangesAsync();

                var userData = User.UserData();
                userData.IsMfaVerified = true;
                await _session.ReplaceAsync(User.Session().Id, userData);

                return ServiceResult.AsSuccess();
            }

            return ServiceResult.AsError("Confirmation code was not valid");
        }

        public async Task<ServiceResult<object>> Confirm(MfaConfirmModel model)
        {
            var user = await Context.Users.Where(x => x.Id == UserId.Value).FirstOrDefaultAsync();

            if (user == null) return ServiceResult<object>.AsError("User not found");

            if (!user.IsMfaEnabled) return ServiceResult.AsError("Mfa not enabled");

            var twoFactor = new TwoFactorAuthenticator();

            if (twoFactor.ValidateTwoFactorPIN(TwoFactorKey(user), model.ConfirmationCode))
            {
                var userData = User.UserData();
                userData.IsMfaVerified = true;
                await _session.ReplaceAsync(User.Session().Id, userData);

                return ServiceResult.AsSuccess();
            }

            return ServiceResult.AsError("Confirmation code was not valid");
        }

        public async Task<bool> ConfirmSkipCode(string code)
        {
            var userData = User.UserData();
            if (MfaConfirmModel.IsValidSkipCode(code, userData, _config))
            {
                userData.IsMfaVerified = true;
                await _session.ReplaceAsync(User.Session().Id, userData);
                return true;
            }
            return false;
        }

        private string TwoFactorKey(User user)
        {
            return $"{_config.MfaSettings.Secret}{user.PasswordSalt}";
        }

        public ServiceResult Remove(int id)
        {
            var user = Context.Users.Where(x => x.Id == id).FirstOrDefault();

            if (user == null) return ServiceResult.AsError("User not found");

            if (user.IsMfaEnabled)
            {
                user.IsMfaEnabled = false;
                Context.SaveChanges();
            }

            return ServiceResult.AsSuccess();
        }
    }
}
