using AutoMapper;
using ChilliCoreTemplate.Data;
using ChilliCoreTemplate.Data.EmailAccount;
using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.Admin;
using ChilliCoreTemplate.Models.EmailAccount;
using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Core.Distributed;
using ChilliSource.Cloud.Core.LinqMapper;
using ChilliSource.Cloud.Web;
using ChilliSource.Core.Extensions;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Profiling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Principal;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Service.EmailAccount
{
    public partial class AccountService : Service<DataContext>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ProjectSettings _config;
        private readonly IFileStorage _fileStorage;
        private readonly ITemplateViewRenderer _templateViewRenderer;
        private readonly IBackgroundTaskQueue _backgroundTaskQueue;
        private readonly UserSessionService _session;
        private readonly FileStoragePath _fileStoragePath;
        private readonly IMapper _mapper;
        private readonly SmsService _sms;

        public AccountService(IPrincipal user,
            DataContext context,
            UserSessionService session,
            SmsService sms,
            IServiceProvider serviceProvider,
            IFileStorage fileStorage,
            ITemplateViewRenderer templateViewRenderer,
            ProjectSettings config,
            IBackgroundTaskQueue backgroundTaskQueue,
            FileStoragePath fileStoragePath,
            IMapper mapper)
            : base(user, context)
        {
            _session = session;
            _serviceProvider = serviceProvider;
            _fileStorage = fileStorage;
            _templateViewRenderer = templateViewRenderer;
            _config = config;
            _sms = sms;
            _backgroundTaskQueue = backgroundTaskQueue;
            _fileStoragePath = fileStoragePath;
            _mapper = mapper;
        }

        public const string ProfilePhotoFolder = "Profile";

        public bool IsApi { get; set; }

        public IQueryable<User> VisibleUsers()
        {
            return VisibleUsers(this);
        }

        internal static IQueryable<User> VisibleUsers(Service<DataContext> svc, bool ignoreMasterCompany = false)
        {
            var userData = svc.User.UserData();

            if (userData == null)
                return Enumerable.Empty<User>().AsQueryable();

            if (userData.IsInRole(Role.Administrator))
            {
                return svc.Context.Users;
            }

            if (userData.IsInRole(Role.CompanyAdmin))
            {
                var companyId = userData.CompanyId;
                if (ignoreMasterCompany)
                    return svc.Context.Users.Where(u => u.UserRoles.Any(r => r.CompanyId == companyId));
                else
                    return svc.Context.Users.Where(u => u.UserRoles.Any(r => r.CompanyId == companyId || r.Company.MasterCompanyId == companyId));
            }

            return svc.Context.Users.Where(u => u.Id == userData.UserId);
        }

        internal User GetAccountByEmail(string email, bool includeDeleted = false)
        {
            var hash = CommonLibrary.CalculateHash(email);
            var account = Context.Users.Where(a =>
                    a.EmailHash == hash &&
                    a.Email == email &&
                    (includeDeleted || a.Status != UserStatus.Deleted))
                .Include(u => u.Tokens)
                .Include(u => u.UserRoles).ThenInclude(u => u.Company)
                .FirstOrDefault();

            return account;
        }

        internal User GetAccountByPhone(string phone, bool includeDeleted = false)
        {
            var hash = Data.EmailAccount.User.GetPhoneHash(phone);
            var account = Context.Users.Where(a =>
                    a.PhoneHash == hash &&
                    a.Phone == phone &&
                    (includeDeleted || a.Status != UserStatus.Deleted))
                .Include(u => u.Tokens)
                .Include(u => u.UserRoles).ThenInclude(u => u.Company)
                .FirstOrDefault();

            return account;
        }

        internal int? GetIdByEmail(string email)
        {
            var hash = CommonLibrary.CalculateHash(email);
            var id = Context.Users.Where(a =>
                    a.EmailHash == hash &&
                    a.Email == email &&
                    a.Status != UserStatus.Deleted)
                .Select(a => (int?)a.Id)
                .FirstOrDefault();
            return id;
        }

        public ServiceResult<UserDataPrincipal> Login(SessionEditModel viewModel, Action<UserDataPrincipal> loginAction, bool isApi = false)
        {
            var result = ServiceResult<UserDataPrincipal>.AsError("You entered an incorrect email or password");

            User account = GetAccountByEmail(viewModel.Email, includeDeleted: true);

            if (account == null)
            {
                QueueMail_Distinct(RazorTemplates.AccountNotRegistered, viewModel.Email, new RazorTemplateDataModel<string> { Data = viewModel.Email });
                return result;
            }

            if (account.Status == UserStatus.Deleted)
            {
                var request = Password_ResetRequest(account);
                if (request.Success)
                {
                    result.Error = "We sent have you an forgot password email so you can reactivate your account.";
                }
                return result;
            }

            if (account.UserRoles.Count == 1 && account.UserRoles.First().Status == RoleStatus.Invited)
            {
                result.Error = "We have resent your invitation email. Please check your email to complete your registration.";

                Token_Add(account, UserTokenType.Invite, new TimeSpan(7, 0, 0, 0));
                Context.SaveChanges();

                var inviteModel = _mapper.Map<InviteEditModel>(account);
                QueueMail(RazorTemplates.InviteUser, inviteModel.Email, new RazorTemplateDataModel<InviteEditModel> { Data = inviteModel });

                return result;
            }

            if (account.IsTooManyRetries)
            {
                return result;
            }

            if (account.PasswordHash == null)
            {
                result.Error = $"A password has not been set for this account. Please check for an email sent to {viewModel.Email.ToLower()} so a password can be created.";

                Password_ResetRequest(account);

                return result;
            }

            if (!account.ConfirmPassword(viewModel.Password, _config.ProjectId.Value))
            {
                account.NumOfRetries = Math.Max(1, account.NumOfRetries + 1);
                account.LastRetryDate = DateTime.UtcNow;

                Context.SaveChanges();

                return result;
            }

            LoginRecord(account);

            if (account.Status == UserStatus.Registered)
            {
                if (_config.UserConfirmationMethod == UserConfirmationMethod.Link)
                {
                    if (account.CreatedDate < DateTime.UtcNow.AddHours(-1)) SendRegistrationCompleteEmail(account, isApi);
                }
                else
                {
                    if (account.CreatedDate < DateTime.UtcNow.AddMinutes(-5)) Password_OneTime(account);
                }
            }

            result.Success = true;
            result.Error = string.Empty;

            var principal = result.Result = CreatePrincipal(account, CreateUserDeviceId(account, viewModel.DeviceId));
            loginAction(principal);

            return result;
        }

        private void LoginRecord(User user)
        {
            user.NumOfRetries = 0;
            user.LastLoginDate = DateTime.UtcNow;
            user.LoginCount += 1;

            Mixpanel.SendAccountToMixpanel(user, "Login");
            Activity_Add(Context, new UserActivity { UserId = user.Id, ActivityType = ActivityType.Create, EntityId = user.Id, EntityType = EntityType.Session });
        }

        public ServiceResult<UserDataPrincipal> LoginWithToken(UserTokenModel model, Action<UserDataPrincipal> loginAction, string deviceId = null)
        {
            var userRequest = User_GetAccountByEmailToken(model);
            if (!userRequest.Success) return ServiceResult<UserDataPrincipal>.CopyFrom(userRequest);
            var user = userRequest.Result;

            Activate(user);
            LoginRecord(user);

            var session = Session_Create(user, CreateUserDeviceId(user, deviceId), TimeSpan.FromDays(1), loginAction);
            return ServiceResult<UserDataPrincipal>.AsSuccess(session);
        }

        public ServiceResult<UserDataPrincipal> LoginWithCode(UserTokenModel model, Action<UserDataPrincipal> loginAction, string deviceId = null)
        {
            var userRequest = User_GetAccountByOneTimePassword(model);
            if (!userRequest.Success) return ServiceResult<UserDataPrincipal>.CopyFrom(userRequest);
            var user = userRequest.Result;

            Activate(user);
            LoginRecord(user);

            var session = Session_Create(user, CreateUserDeviceId(user, deviceId), TimeSpan.FromDays(1), loginAction);
            return ServiceResult<UserDataPrincipal>.AsSuccess(session);
        }

        public UserDataPrincipal CreatePrincipal(int userId)
        {
            return CreatePrincipal(GetAccount(userId), null);
        }

        public UserDataPrincipal CreatePrincipal(User user, int? userDeviceId)
        {
            var userData = CreateUserData(user, userDeviceId);
            return new UserDataPrincipal(userData);
        }

        private void ValidateAccountRolesLoaded(User user)
        {
            if (user.UserRoles == null)
            {
                throw new ArgumentException("user.AccountRoles not loaded.");
            }
        }

        public Task<string> CreateAsync(User user, int? userDeviceId, DateTime ExpiresOn)
        {
            var userData = MapUserData(user, userDeviceId);

            return _session.CreateAsync(userData, ExpiresOn);
        }

        private UserData MapUserData(User user, int? userDeviceId)
        {
            return _session.MapUserData(user, userDeviceId);
        }

        public UserData CreateUserData(User user, int? userDeviceId)
        {
            ValidateAccountRolesLoaded(user);
            var userData = MapUserData(user, userDeviceId);

            return userData;
        }

        internal int? CreateUserDeviceId(User account, string deviceId)
        {
            if (String.IsNullOrEmpty(deviceId))
                return null;

            return CreateUserDevice(account, deviceId).Id;
        }

        internal UserDevice CreateUserDevice(User account, string deviceId)
        {
            var device = Context.UserDevices.FirstOrDefault(d => d.UserId == account.Id && d.DeviceId == deviceId);
            if (device == null)
            {
                device = Context.UserDevices.Add(new UserDevice
                {
                    UserId = account.Id,
                    PinToken = Guid.NewGuid(), //Setting pin token because it is indexed.
                    DeviceId = deviceId,
                    LastLoginDate = DateTime.Now
                }).Entity;

                Context.SaveChanges();

                //Dealing with concurrency
                device = Context.UserDevices.FirstOrDefault(d => d.UserId == account.Id && d.DeviceId == deviceId);
            }

            return device;
        }

        internal List<PushNotificationDevice> UserDevice_List(List<int> userIds)
        {
            return Context.UserDevices
                .Where(x => userIds.Contains(x.UserId) && x.PushTokenId != null && x.User.Status != UserStatus.Deleted)
                .Select(x => new PushNotificationDevice { UserId = x.Id, TokenId = x.PushTokenId, Provider = x.PushProvider.Value })
                .ToList();
        }

        internal UserDataPrincipal CreateImpersonationTicket(User account, Action<UserDataPrincipal> loginAction)
        {
            var currentUser = User.UserData();
            var impersonatedUser = MapUserData(account, null);
            impersonatedUser.ImpersonatedBy(currentUser);

            var principal = new UserDataPrincipal(impersonatedUser);
            loginAction(principal);

            return principal;
        }

        public ServiceResult<UserDataPrincipal> ImpersonateAccount(int accountId, Action<UserDataPrincipal> loginAction)
        {
            var userData = User.UserData();
            if (userData == null) return ServiceResult<UserDataPrincipal>.AsError("Not found or access denied.");
            if (accountId == userData.UserId) return ServiceResult<UserDataPrincipal>.AsError("You cannot impersonate yourself.");

            var account = this.Get<AccountViewModel>(accountId, visibleOnly: true);
            if (account == null || !userData.CanImpersonate(account))
                return ServiceResult<UserDataPrincipal>.AsError("Not found or access denied.");

            var data = this.GetAccount(accountId);

            var ticket = CreateImpersonationTicket(data, loginAction);

            return ServiceResult<UserDataPrincipal>.AsSuccess(ticket);
        }

        public ServiceResult<UserDataPrincipal> RemoveImpersonation(Action<UserDataPrincipal> loginAction)
        {
            var userData = User?.UserData();
            if (userData == null) return ServiceResult<UserDataPrincipal>.AsError();

            if (!userData.IsImpersonated())
                return ServiceResult<UserDataPrincipal>.AsError("Error, account is not impersonated");

            userData = userData.Clone();
            userData.RemoveImpersonation(_mapper);

            var principal = new UserDataPrincipal(userData);
            loginAction(principal);

            return ServiceResult<UserDataPrincipal>.AsSuccess(principal);
        }

        internal ServiceResult AddNewAccountRoles(User account, params RoleSelectionViewModel[] roleSelections)
        {
            var rolesRequest = CreateNewAccountRoles(account, roleSelections);
            if (!rolesRequest.Success) return ServiceResult.CopyFrom(rolesRequest);

            MergeAccountRoles(account, rolesRequest.Result, deleteUnmatched: false);

            return ServiceResult.AsSuccess();
        }

        internal ServiceResult<List<UserRole>> CreateNewAccountRoles(User account, params RoleSelectionViewModel[] roleSelections)
        {
            return CreateNewAccountRoles(account, (IList<RoleSelectionViewModel>)roleSelections);
        }

        internal ServiceResult<List<UserRole>> CreateNewAccountRoles(User account, IList<RoleSelectionViewModel> roleSelections)
        {
            var accountRoles = new List<UserRole>();

            foreach (var selectedRole in roleSelections)
            {
                if (selectedRole.Role.IsCompanyRole())
                {
                    if (selectedRole.CompanyId == null)
                    {
                        if (!String.IsNullOrEmpty(selectedRole.CompanyName))
                        {
                            using (var scope = _serviceProvider.CreateScope())
                            {
                                var db = scope.ServiceProvider.GetRequiredService<DataContext>();
                                var company = db.Companies.Add(Company.CreateNew(selectedRole.CompanyName)).Entity;
                                db.SaveChanges();
                                selectedRole.CompanyId = company.Id;
                            }
                        }
                        else if (selectedRole.CompanyGuid.HasValue)
                        {
                            selectedRole.CompanyId = Context.Companies.Where(x => x.Guid == selectedRole.CompanyGuid.Value).Select(x => x.Id).FirstOrDefault();
                        }
                        if (selectedRole.CompanyId == null) return ServiceResult<List<UserRole>>.AsError("Company not selected for a company role");
                    }
                }
                else
                {
                    selectedRole.CompanyId = null;
                    selectedRole.CompanyName = null;
                }

                accountRoles.Add(new UserRole()
                {
                    User = account,
                    CreatedAt = DateTime.UtcNow,
                    Role = selectedRole.Role,
                    CompanyId = selectedRole.CompanyId,
                    Status = selectedRole.Status
                });
            }

            return ServiceResult<List<UserRole>>.AsSuccess(accountRoles);
        }

        private void MergeAccountRoles(User account, List<UserRole> newRoles, bool deleteUnmatched)
        {
            if (account.UserRoles == null)
            {
                throw new ApplicationException("Account roles not loaded");
            }

            Expression<Func<UserRole, UserRole, bool>> matchPredicate =
                (a, b) => (a.UserId == b.UserId || a.User == b.User)
                         && a.Role == b.Role
                         && a.CompanyId == b.CompanyId;

            if (deleteUnmatched)
            {
                var removeList = account.UserRoles.AsQueryable().AsExpandable()
                                    .Where(currentRole => !newRoles.Any(r => matchPredicate.Invoke(currentRole, r)))
                                    .ToList();

                removeList.ForEach(r =>
                {
                    account.UserRoles.Remove(r);
                    Context.UserRoles.Remove(r);
                });
            }

            foreach (var newRole in newRoles)
            {
                var existing = account.UserRoles.AsQueryable().AsExpandable()
                                          .Where(currentRole => matchPredicate.Invoke(currentRole, newRole))
                                          .FirstOrDefault();

                if (existing == null)
                {
                    account.UserRoles.Add(newRole);
                }
            }
        }

        public ServiceResult<UserData> Create(RegistrationViewModel model, bool sendEmail = true)
        {
            var createModel = _mapper.Map<UserCreateModel>(model);

            var createAccountRequest = Create(createModel);
            var account = createAccountRequest.Result;
            if (createAccountRequest.Success)
            {
                if (!model.IsAnonymous)
                {
                    if (_config.UserConfirmationMethod == UserConfirmationMethod.Link && sendEmail) SendWelcomeEmail(account);

                    if (model.MixpanelTempId != null)
                    {
                        Mixpanel.CreateAlias(account, model.MixpanelTempId.Value);
                    }

                    Mixpanel.SendAccountToMixpanel(account, "Account created", tempUserId: model.MixpanelTempId);   //Use temp id as mixplanel can take too long to register the alias in previous call.
                }

                return ServiceResult<UserData>.AsSuccess(MapUserData(account, null));
            }
            else
            {
                if (!model.IsAnonymous && sendEmail && !String.IsNullOrEmpty(model.Email))
                    QueueMail_Distinct(RazorTemplates.AccountAlreadyRegistered, model.Email, new RazorTemplateDataModel<string> { Data = model.Email }, new TimeSpan(24, 0, 0));

                return ServiceResult<UserData>.AsError("Already registered");
            }
        }

        public ServiceResult<AccountViewModel> CreateOrGet(RegistrationViewModel model, bool sendEmail = true)
        {
            int? userId = null;
            if (!String.IsNullOrEmpty(model.ExternalId)) userId = Context.Users.HasExternalId(model.ExternalId).Where(x => x.Status != UserStatus.Deleted).Select(x => (int?)x.Id).FirstOrDefault();
            if (userId == null) userId = GetIdByEmail(model.Email);

            if (userId == null)
            {
                var createRequest = Create(model, sendEmail);
                if (!createRequest.Success) return ServiceResult<AccountViewModel>.CopyFrom(createRequest);
                userId = createRequest.Result.UserId;
            }

            return ServiceResult<AccountViewModel>.AsSuccess(Get<AccountViewModel>(userId.Value, false));
        }

        public ServiceResult ChangeAccountRoles(ChangeAccountRoleModel model)
        {
            var userData = User.UserData();
            if (!userData.IsInRole(new UserRoleModel() { Role = Role.Administrator }))
            {
                return ServiceResult.AsError("Not found or access denied.");
            }

            var account = this.VisibleUsers().Where(a => a.Id == model.Id)
                            .Include(a => a.UserRoles)
                            .FirstOrDefault();

            if (account == null)
            {
                return ServiceResult.AsError("Not found or access denied.");
            }

            var role = account.GetLatestUserRole();
            if (role == null)
            {
                role = new UserRole();
                account.UserRoles.Add(role);
            }
            role.Role = model.Role.Value;

            if (role.Role.IsCompanyRole())
            {
                if (model.CompanyId == null) return ServiceResult.AsError("Company must be selected for a company role");
                role.CompanyId = model.CompanyId;
            }
            else
            {
                role.CompanyId = null;
            }

            account.UpdatedDate = DateTime.UtcNow;

            return ServiceResult.AsSuccess();
        }

        private ServiceResult<User> Create(UserCreateModel model)
        {
            if (model.Status == UserStatus.Anonymous && String.IsNullOrEmpty(model.Email))
            {
                model.Email = _config.EmailTemplate.Email.Replace("@", $"+{Guid.NewGuid().ToShortGuid()}@");
            }

            var account = String.IsNullOrEmpty(model.Email) ? GetAccountByPhone(model.Phone, includeDeleted: true) : GetAccountByEmail(model.Email, includeDeleted: true);

            var isInvited = (account == null && model.UserRoles.Any(x => x.Status == RoleStatus.Invited)) || (account != null && account.UserRoles.Count == 1 && account.UserRoles.First().Status == RoleStatus.Invited);
            var inviteCompanyRole = account != null && model.UserRoles.Any(x => x.Role.IsCompanyRole() && !account.UserRoles.Any(x => x.Role.IsIn(Role.Administrator, Role.CompanyAdmin, Role.CompanyUser)));
            if (account == null || isInvited || inviteCompanyRole || account.Status == UserStatus.Deleted || account.Status == UserStatus.Anonymous)
            {
                if (account == null)
                {
                    account = Context.Users.Add(new User() { CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow, UserRoles = new List<UserRole>() }).Entity;
                }

                if (!inviteCompanyRole)
                    _mapper.Map(model, account);

                if (!String.IsNullOrEmpty(model.Password))
                {
                    Password_Set(account, model.Password);
                }

                var newRolesResponse = CreateNewAccountRoles(account, model.UserRoles);
                if (!newRolesResponse.Success)
                    return ServiceResult<User>.CopyFrom(newRolesResponse);

                MergeAccountRoles(account, newRolesResponse.Result, deleteUnmatched: !inviteCompanyRole);

                if (isInvited)
                {
                    account.InvitedDate = DateTime.UtcNow;
                    Token_Add(account, UserTokenType.Invite, new TimeSpan(7, 0, 0, 0));
                }
                else if (inviteCompanyRole)
                {
                    Token_Add(account, UserTokenType.Invite, new TimeSpan(365, 0, 0, 0));
                }
                else if (model.Status == UserStatus.Registered)
                {
                    if (_config.UserConfirmationMethod == UserConfirmationMethod.Link)
                        Token_Add(account, UserTokenType.Activate, new TimeSpan(14, 0, 0, 0));
                    else
                        Password_OneTime(account);
                }

                if (model.ProfilePhotoFile != null) account.ProfilePhotoPath = _fileStorage.Save(new StorageCommand { Folder = ProfilePhotoFolder }.SetHttpPostedFileSource(model.ProfilePhotoFile));
                else if (!String.IsNullOrEmpty(model.ProfilePhotoUrl)) account.ProfilePhotoPath = _fileStorage.Save(new StorageCommand { Folder = ProfilePhotoFolder }.SetUrlSource(model.ProfilePhotoUrl));

                Context.SaveChanges();

                Activity_Add(new UserActivity { UserId = account.Id, ActivityType = ActivityType.Create, EntityId = account.Id, EntityType = EntityType.User });

                return ServiceResult<User>.AsSuccess(account);
            }
            return ServiceResult<User>.AsError(account, String.IsNullOrEmpty(model.Email) ? "Phone is already registered" : "Email is already registered");
        }

        private void SendWelcomeEmail(User account, bool isApi = false)
        {
            if (String.IsNullOrEmpty(account.Email)) return;

            var companyId = account.GetLatestUserRole()?.CompanyId;

            QueueMail(RazorTemplates.WelcomeEmail, account.Email, new RazorTemplateDataModel<RegistrationCompleteViewModel>
            {
                Data = new RegistrationCompleteViewModel
                {
                    FirstName = account.FirstName,
                    Email = account.Email,
                    Token = account.GetToken(UserTokenType.Activate),
                    IsApi = isApi
                }
            });
        }

        public ServiceResult Activate(UserTokenModel model)
        {
            var userRequest = User_GetAccountByEmailToken(model);
            if (!userRequest.Success) return ServiceResult.CopyFrom(userRequest);

            return Activate(userRequest.Result);
        }

        /// <summary>
        /// Activate a user on their behalf
        /// </summary>
        internal ServiceResult Activate(int id)
        {
            var user = Context.Users.Where(x => x.Id == id).FirstOrDefault();
            if (user == null) return ServiceResult.AsError("User not found");
            return Activate(user, onBehalfOf: true);
        }

        internal ServiceResult Activate(User user, bool onBehalfOf = false)
        {
            if (user != null && user.Status == UserStatus.Registered)
            {
                user.Status = UserStatus.Activated;
                user.ActivatedDate = DateTime.UtcNow;
                Context.SaveChanges();

                if (_config.UserConfirmationMethod == UserConfirmationMethod.OneTimePassword) SendWelcomeEmail(user);

                Mixpanel.SendAccountToMixpanel(user, "Account activated");
                Activity_Add(new UserActivity { UserId = user.Id, ActivityType = ActivityType.Activate, EntityId = user.Id, EntityType = EntityType.User });

                if (onBehalfOf || !User.Identity.IsAuthenticated)
                {
                    var sessions = Context.UserSessions.Where(x => x.UserId == user.Id).ToList();
                    foreach (var session in sessions) _session.ClearSessionCache(session.SessionId.ToString());
                }
                else
                {
                    _session.ClearSessionCache(User.Session()?.Id);
                }
            }

            return ServiceResult.AsSuccess();
        }

        public async Task SoftDeleteAsync(int userId)
        {
            var user = await Context.Users
                .Include(x => x.UserRoles)
                .Include(x => x.OAuths)
                .Where(x => x.Id == userId)
                .FirstAsync();

            if (user != null)
            {
                user.Status = UserStatus.Deleted;
                user.ClosedDate = DateTime.UtcNow;
                user.UpdatedDate = DateTime.UtcNow;

                user.FirstName = null;
                user.LastName = null;
                user.Email = $"{Guid.NewGuid()}@{new Uri(_config.PublicUrl).Domain()}";

                if (user.UserRoles.Any()) Context.UserRoles.RemoveRange(user.UserRoles);

                foreach (var oauth in user.OAuths)
                {
                    if (oauth.Provider == Models.Api.OAuth.OAuthProvider.Apple)
                    {
                        await this.OAuth_Revoke(oauth);
                    }
                    Context.UserOAuths.Remove(oauth);
                }

                await Context.SaveChangesAsync();
            }
        }

        public ServiceResult Purge(int userId)
        {
            var user = Context.Users
                .Include(x => x.UserRoles)
                .Include(x => x.Sessions)
                .Include(x => x.Tokens)
                .Include(x => x.Activities)
                .Include(x => x.Devices)
                .Include(x => x.OAuths)
                .Where(x => x.Id == userId)
                .FirstOrDefault();
            if (user != null)
            {
                if (user.UserRoles.Any()) Context.UserRoles.RemoveRange(user.UserRoles);
                if (user.Sessions.Any()) Context.UserSessions.RemoveRange(user.Sessions);
                if (user.Tokens.Any()) Context.UserTokens.RemoveRange(user.Tokens);
                if (user.Activities.Any()) Context.UserActivities.RemoveRange(user.Activities);
                if (user.Devices.Any()) Context.UserDevices.RemoveRange(user.Devices);
                if (user.OAuths.Any()) Context.UserOAuths.RemoveRange(user.OAuths);

                var logs = Context.ErrorLogs.Where(x => x.UserId == userId).ToList();
                if (logs.Any()) Context.ErrorLogs.RemoveRange(logs);

                var emails = Context.Emails.Where(x => x.UserId == userId).ToList();
                if (emails.Any()) Context.Emails.RemoveRange(emails);

                var emailUsers = Context.EmailUsers.Where(x => x.UserId == userId).ToList();
                if (emailUsers.Any()) Context.EmailUsers.RemoveRange(emailUsers);

                var sms = Context.SmsQueue.Where(x => x.UserId == userId).ToList();
                if (sms.Any()) Context.SmsQueue.RemoveRange(sms);
                Context.SaveChanges();

                Context.Users.Remove(user);
                Context.SaveChanges();
            }
            return ServiceResult.AsSuccess();
        }

        public T Get<T>(int id, bool visibleOnly) where T : class, new()
        {
            var query = visibleOnly ? this.VisibleUsers() : Context.Users;

            return query.Where(a => a.Id == id)
                        .Materialize<User, T>()
                        .FirstOrDefault();
        }

        public ServiceResult<AccountDetailsEditModel> GetForEdit(int accountId)
        {
            var record = Context.Users.Where(a => a.Id == accountId)
                .Materialize<User, AccountDetailsEditModel>()
                .FirstOrDefault();

            if (record == null) return ServiceResult<AccountDetailsEditModel>.AsError("Account not found or access denied.");

            return ServiceResult<AccountDetailsEditModel>.AsSuccess(record);
        }

        public ServiceResult Update_Status(int userId, UserStatus status, bool isApi = false)
        {
            var user = GetAccount(userId);

            if (user == null) return ServiceResult.AsError("Not found");

            if (user.Status == UserStatus.Anonymous && status == UserStatus.Registered)
            {
                Token_Add(user, UserTokenType.Activate, new TimeSpan(14, 0, 0, 0));
                SendWelcomeEmail(user, isApi);
            }
            user.Status = status;
            Context.SaveChanges();

            return ServiceResult.AsSuccess();
        }

        public ServiceResult Update(AccountDetailsEditModel model, int userId, bool onBehalfOfUser = false, bool visibleOnly = true)
        {
            var currentSession = this.User?.Session();

            if (Exists(model.Email, userId))  return ServiceResult.AsError("The email address chosen is already registered");

            var query = visibleOnly ? this.VisibleUsers() : Context.Users;

            var user = query
                .Where(a => a.Id == userId)
                .Include(a => a.UserRoles)
                .ThenInclude((UserRole r) => r.Company)
                .FirstOrDefault();

            if (user == null) return ServiceResult.AsError("Not found");

            if (model.ProfilePhotoFile != null)
            {
                model.ProfilePhotoPath = _fileStorage.Save(new StorageCommand { Folder = ProfilePhotoFolder }.SetHttpPostedFileSource(model.ProfilePhotoFile));
            }

            user = _mapper.Map(model, user);
            Context.SaveChanges();
            Mixpanel.SendAccountToMixpanel(user);

            if (!onBehalfOfUser)
            {
                _session.ClearSessionCache(currentSession?.Id);
            }

            return ServiceResult.AsSuccess();
        }

        public async Task<ServiceResult<object>> ProfilePhoto_Add(int id, ImageFileModel model)
        {
            var user = await Context.Users.Where(a => a.Id == id).FirstOrDefaultAsync();

            if (user != null)
            {
                if (!String.IsNullOrEmpty(user.ProfilePhotoPath))
                    await _fileStorage.DeleteAsync(user.ProfilePhotoPath);

                user.ProfilePhotoPath = await _fileStorage.SaveAsync(new StorageCommand { Folder = ProfilePhotoFolder }.SetHttpPostedFileSource(model.ImageFile));

                await Context.SaveChangesAsync();
            }

            return ServiceResult<object>.AsSuccess();
        }

        public async Task<ServiceResult<object>> ProfilePhoto_Delete(int id)
        {
            var user = await Context.Users.Where(a => a.Id == id).FirstOrDefaultAsync();

            if (user != null && !String.IsNullOrEmpty(user.ProfilePhotoPath))
            {
                await _fileStorage.DeleteAsync(user.ProfilePhotoPath);
                user.ProfilePhotoPath = null;
                await Context.SaveChangesAsync();
            }

            return ServiceResult<object>.AsSuccess();
        }

        internal User GetAccount(int? id, bool includeDeleted = false)
        {
            if (id == null)
                return null;

            var query = Context.Users.Where(a => a.Id == id);

            if (!includeDeleted)
                query = query.Where(a => a.Status != UserStatus.Deleted);

            var account = query
                .Include(a => a.Tokens)
                .Include(a => a.UserRoles)
                .ThenInclude((UserRole r) => r.Company)
                .FirstOrDefault();

            return account;
        }

        public bool Exists(string email, int? accountId = null)
        {
            if (String.IsNullOrEmpty(email)) return false;
            var hash = CommonLibrary.CalculateHash(email);
            var query = Context.Users.Where(a => a.EmailHash == hash && a.Email == email);
            if (accountId.HasValue) query = query.Where(a => a.Id != accountId);
            else query = query.Where(a => a.Status != UserStatus.Deleted);
            return query.Any();
        }

        public bool Exists_Phone(string phone, int accountId)
        {
            if (String.IsNullOrEmpty(phone)) return false;
            var hash = Data.EmailAccount.User.GetPhoneHash(phone);
            return Context.Users.Any(a => a.PhoneHash == hash && a.Id != accountId && a.Phone == phone && a.Status != UserStatus.Deleted);
        }

        public void SendRegistrationCompleteEmail(int accountId)
        {
            var account = this.GetAccount(accountId);
            this.SendRegistrationCompleteEmail(account);
        }

        internal void SendRegistrationCompleteEmail(User account, bool isApi = false)
        {
            if (!account.Tokens.Any(t => t.Type == UserTokenType.Activate && t.Expiry > DateTime.UtcNow))
            {
                var token = Token_Add(account, UserTokenType.Activate, new TimeSpan(7, 0, 0, 0));
                QueueMail(RazorTemplates.RegistrationComplete, account.Email, new RazorTemplateDataModel<RegistrationCompleteViewModel> { Data = new RegistrationCompleteViewModel { FirstName = account.FirstName, Token = token.ToShortGuid().ToString(), Email = account.Email, IsApi = isApi } });
            }
        }

        public IList<LoginRoleModel> GetListOfRoles(int id)
        {
            var account = GetAccount(id);
            if (account == null) return ArrayExtensions.EmptyArray<LoginRoleModel>();

            return GetListOfRoles(Context, account);
        }

        internal static List<LoginRoleModel> GetListOfRoles(DataContext context, User account)
        {
            var roles = context.UserRoles.Where(a => a.Id == account.Id)
                                .Materialize<UserRole, LoginRoleModel>()
                                .ToList();

            return roles.OrderBy(r => r.RoleDesc).ToList();
        } 

        public UserDataPrincipal Session_Create(int id, int? userDeviceId, TimeSpan expiry, Action<UserDataPrincipal> loginAction)
        {
            var user = GetAccount(id);
            return Session_Create(user, userDeviceId, expiry, loginAction);
        }

        internal UserDataPrincipal Session_Create(User user, int? userDeviceId, TimeSpan expiry, Action<UserDataPrincipal> loginAction)
        {
            var principal = CreatePrincipal(user, userDeviceId);
            //TODO: await async
            principal.Id = TaskHelper.GetResultSafeSync(() => _session.CreateAsync(principal.UserData, DateTime.UtcNow.Add(expiry)));
            loginAction(principal);

            return principal;
        }

        internal void Session_Replace(string id, User user, int? userDeviceId)
        {
            ValidateAccountRolesLoaded(user);

            var userData = MapUserData(user, userDeviceId);
            TaskHelper.GetResultSafeSync(() => _session.ReplaceAsync(id, userData));
        }

        internal void Session_Clear(string id)
        {
            _session.ClearSessionCache(id);
        }

        public async Task Anonymous_CleanAsync(ITaskExecutionInfo executionInfo)
        {
            executionInfo.SendAliveSignal();
            if (executionInfo.IsCancellationRequested)
                return;

            if (_config.PurgeOldAnonymousAccounts)
            {
                //Delete old anonymous accounts
                var oneMonthAgo = DateTime.UtcNow.AddMonths(-1);
                var users = await Context.Users
                    .Where(x => x.Status == UserStatus.Anonymous && ((x.CreatedDate < oneMonthAgo && x.LastLoginDate == null) || x.LastLoginDate < oneMonthAgo))
                    .Take(50)
                    .Select(x => x.Id)
                    .ToListAsync();

                users.ForEach(x => Purge(x));
            }
        }
    }
}