using ChilliSource.Core.Extensions;
using ChilliSource.Cloud.Core;
using ChilliCoreTemplate.Data;
using ChilliCoreTemplate.Data.EmailAccount;
using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.Api;
using ChilliCoreTemplate.Models.EmailAccount;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Threading.Tasks;
using ChilliCoreTemplate.Service.EmailAccount;
using System.Net;
using System.IO;
using ChilliSource.Cloud.Core.LinqMapper;
using System.Security.Principal;
using ChilliSource.Cloud.Web;
using AutoMapper;

namespace ChilliCoreTemplate.Service.Api
{
    public class UserApiMobileService : Service<DataContext>
    {
        AccountService _accountService;
        UserSessionService _session;        
        UserKeyHelper _userKeyHelper;
        IFileStorage _fileStorage;
        PushNotificationConfiguration _push;

        public UserApiMobileService(IPrincipal user, DataContext context, AccountService accountService, UserSessionService session, UserKeyHelper userKeyHelper, IFileStorage fileStorage, PushNotificationConfiguration push)
            : base(user, context)
        {
            _accountService = accountService;
            _accountService.IsApi = true;
            _session = session;
            _userKeyHelper = userKeyHelper;
            _fileStorage = fileStorage;
            _push = push;
        }

        internal static void AutoMapperConfigure(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<PhoneRegistrationApiModel, RegistrationViewModel>()
                .ForMember(dest => dest.MixpanelTempId, opt => opt.MapFrom(src => src.AnonymousUserId));
        }

        public ServiceResult<UserAccountApiModel> Create(PhoneRegistrationApiModel model)
        {
            var registrationModel = Mapper.Map<RegistrationViewModel>(model);
            registrationModel.Roles = Role.User;
            var response = _accountService.Create(registrationModel);
            if (!response.Success) return ServiceResult<UserAccountApiModel>.CopyFrom(response);
            return GetUserInternal(response.Result.UserId);
        }

        private ServiceResult<UserAccountApiModel> GetUserInternal(int id)
        {
            var account = Context.Users.Where(a => a.Id == id)
                            .Materialize<User, UserAccountApiModel>().FirstOrDefault();

            if (account == null)
                return ServiceResult<UserAccountApiModel>.AsError("Profile not found.");

            return ServiceResult<UserAccountApiModel>.AsSuccess(account);
        }

        private User GetAccountByPhone(string phone)
        {
            var phoneHash = Data.EmailAccount.User.GetPhoneHash(phone);
            var account = Context.Users.Where(a => a.PhoneHash == phoneHash && a.Phone == phone && a.Status != UserStatus.Deleted)
                            .Include(a => a.UserRoles)
                            .FirstOrDefault();

            return account;
        }

        public ServiceResult<UserDataPrincipal> LoginWithPhoneNumber(NewSessionByPhoneApiModel model, Action<UserDataPrincipal> loginAction)
        {
            var result = ServiceResult<UserDataPrincipal>.AsError("Unable to verify your code. Please request another code.");

            var account = GetAccountByPhone(model.Phone);

            if (account == null || account.PhoneVerificationCode == null)
            {
                return result;
            }

            if (account.PhoneVerificationToken != _userKeyHelper.UnprotectGuid(model.VerificationToken))
            {
                return result;
            }

            if (account.PhoneVerificationExpiry < DateTime.UtcNow)
            {
                result.Error = "Verification code has expired. Please request another code.";
                return result;
            }

            var codeHash = EncryptionHelper.GenerateSaltedHash(model.VerificationCode, account.PasswordSalt.ToString());
            if (account.PhoneVerificationCode != codeHash)
            {
                if (account.PhoneVerificationRetries > 5)
                {
                    account.PhoneVerificationCode = null;
                }
                else
                {
                    account.PhoneVerificationRetries++;
                }
                Context.SaveChanges();

                return result;
            }

            if (account.Status == UserStatus.Registered)
            {
                account.Status = UserStatus.Activated;
                account.ActivatedDate = DateTime.UtcNow;
            }

            account.PhoneVerificationCode = null;
            account.PhoneVerificationRetries = 0;
            account.LastLoginDate = DateTime.UtcNow;
            account.LoginCount += 1;
            Context.SaveChanges();

            Mixpanel.SendAccountToMixpanel(account, "Login");
            AccountService.Activity_Add(Context, new UserActivity { UserId = account.Id, ActivityType = ActivityType.Create, EntityId = account.Id, EntityType = EntityType.Session });

            var session = _accountService.Session_Create(account, _accountService.CreateUserDeviceId(account, model.DeviceId), TimeSpan.FromDays(365), loginAction);
            
            return ServiceResult<UserDataPrincipal>.AsSuccess(session);
        }

        public ServiceResult<UserDataPrincipal> LoginWithPin(PinLoginPinApiModel model, Action<UserDataPrincipal> loginAction)
        {
            var errorResult = ServiceResult<UserDataPrincipal>.AsError("You entered an incorrect email or pin");

            var pinToken = _userKeyHelper.UnprotectGuid(model.PinToken);
            if (pinToken == null)
            {
                return errorResult;
            }

            var device = Context.UserDevices.Where(s => s.PinToken == pinToken)
                            .Include(s => s.User.UserRoles)
                            .ThenInclude((UserRole r) => r.Company)
                            .FirstOrDefault();

            if (device == null || device.DeviceId != model.DeviceId || device.User.Status == UserStatus.Deleted)
            {
                return errorResult;
            }

            if (device.PinLastRetryDate >= DateTime.UtcNow.AddMinutes(-10) && device.PinRetries >= 3)
            {
                errorResult.StatusCode = System.Net.HttpStatusCode.Forbidden;
                return errorResult;
            }

            var pinHash = model.Pin.SaltedHash(device.PinToken.ToString());
            if (device.PinHash != pinHash)
            {
                device.PinRetries++;
                device.PinLastRetryDate = DateTime.UtcNow;

                Context.SaveChanges();

                return errorResult;
            }

            device.PinRetries = 0;
            var account = device.User;

            account.LastLoginDate = DateTime.UtcNow;
            account.LoginCount += 1;
            Context.SaveChanges();

            Mixpanel.SendAccountToMixpanel(account, "Login");
            AccountService.Activity_Add(Context, new UserActivity { UserId = account.Id, ActivityType = ActivityType.Create, EntityId = account.Id, EntityType = EntityType.Session });

            var session = _accountService.Session_Create(account, _accountService.CreateUserDeviceId(account, model.DeviceId), TimeSpan.FromDays(365), loginAction);

            return ServiceResult<UserDataPrincipal>.AsSuccess(session);
        }


        public ServiceResult<DevicePinResponseApiModel> SaveDevicePin(PersistDevicePinApiModel model)
        {
            Guid sessionId = Guid.Empty;
            Guid.TryParse(User?.Session()?.Id, out sessionId);

            if (sessionId == Guid.Empty)
            {
                return NoAuthDevice<DevicePinResponseApiModel>();
            }

            var device = Context.UserDevices.Where(d => d.UserSessions.Any(s => s.SessionId == sessionId))
                            .FirstOrDefault();

            if (device == null || device.DeviceId != model.DeviceId)
            {
                return NoAuthDevice<DevicePinResponseApiModel>();
            }

            device.PinToken = Guid.NewGuid();
            device.PinHash = model.Pin.SaltedHash(device.PinToken.ToString());
            device.PinRetries = 0;

            Context.SaveChanges();

            var response = new DevicePinResponseApiModel()
            {
                PinToken = _userKeyHelper.ProtectGuid(device.PinToken)
            };

            return ServiceResult<DevicePinResponseApiModel>.AsSuccess(response);
        }

        private ServiceResult<T> NoAuthDevice<T>()
        {
            return ServiceResult<T>.AsError("Device not authenticated.", System.Net.HttpStatusCode.Unauthorized);
        }

        public ServiceResult DeleteDevicePin()
        {
            Guid sessionId = Guid.Empty;
            Guid.TryParse(User?.Session()?.Id, out sessionId);
            
            if (sessionId == Guid.Empty)
                return ServiceResult.AsSuccess();

            var device = Context.UserDevices.Where(d => d.UserSessions.Any(s => s.SessionId == sessionId))
                            .FirstOrDefault();

            if (device == null)
                return ServiceResult.AsSuccess();

            device.PinHash = null;
            Context.SaveChanges();

            return ServiceResult.AsSuccess();
        }

        public async Task<ServiceResult<object>> RegisterPushToken(PushTokenRegistrationApiModel model)
        {
            var sessionId = Guid.Empty;
            Guid.TryParse(User?.Session()?.Id, out sessionId);
            if (sessionId == Guid.Empty)
            {
                return NoAuthDevice<object>();
            }

            var device = await Context.UserDevices
                .Where(d => d.UserSessions.Any(s => s.SessionId == sessionId))
                .FirstOrDefaultAsync();

            if (device == null || device.DeviceId != model.DeviceId)
            {
                return NoAuthDevice<object>();
            }

            var alreadyExists = await Context.UserDevices.AnyAsync(x => x.Id != device.Id && x.PushToken == model.Token);
            if (alreadyExists) return ServiceResult<object>.AsError("Push token is already registered");

            device.PushToken = model.Token;
            device.PushProvider = model.Provider;
            device.PushAppId = model.AppId;

            var tokenRequest = await _push.GetService(model.AppId.Value).RegisterPushTokenToSNSAsync(device.PushToken, model.Provider.Value);
            if (!tokenRequest.Success) return ServiceResult<object>.CopyFrom(tokenRequest);
            device.PushTokenId = tokenRequest.Result;

            await Context.SaveChangesAsync();

            return ServiceResult.AsSuccess();
        }
    }
}
