using AutoMapper;
using ChilliCoreTemplate.Data;
using ChilliCoreTemplate.Data.EmailAccount;
using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.Admin;
using ChilliCoreTemplate.Models.EmailAccount;
using ChilliSource.Cloud.Core;
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
            FileStoragePath fileStoragePath)
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
        }

        public const string ProfilePhotoFolder = "Profile";

        public IQueryable<User> VisibleUsers()
        {
            return VisibleUsers(this);
        }

        internal static IQueryable<User> VisibleUsers(Service<DataContext> svc)
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
                return svc.Context.Users.Where(u => u.UserRoles.Any(r => r.CompanyId == companyId));
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
            var result = ServiceResult<UserDataPrincipal>.AsError("Unable to login with supplied credentials");

            User account = GetAccountByEmail(viewModel.Email);

            if (account == null || account.Status == UserStatus.Deleted)
            {
                QueueMail_Distinct(RazorTemplates.AccountNotRegistered, viewModel.Email, new RazorTemplateDataModel<string> { Data = viewModel.Email });
                return result;
            }

            if (account.Status == UserStatus.Invited)
            {
                result.Error = "We have resent your invitation email. Please check your email to complete your registration.";

                Token_Add(account, UserTokenType.Invite, new TimeSpan(7, 0, 0, 0));
                Context.SaveChanges();

                var inviteModel = Mapper.Map<InviteEditModel>(account);
                QueueMail(RazorTemplates.InviteUser, inviteModel.Email, new RazorTemplateDataModel<InviteEditModel> { Data = inviteModel });

                return result;
            }

            if (account.IsTooManyRetries)
            {
                return result;
            }

            if (!account.ConfirmPassword(viewModel.Password))
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

            Mixpanel.SendAccountToMixpanel(account, "Login");
            Activity_Add(Context, new UserActivity { UserId = account.Id, ActivityType = ActivityType.Create, EntityId = account.Id, EntityType = EntityType.Session });

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

        public ServiceResult<UserDataPrincipal> LoginWithToken(EmailTokenModel model, Action<UserDataPrincipal> loginAction)
        {
            var userRequest = User_GetAccountByEmailToken(model);
            if (!userRequest.Success) return ServiceResult<UserDataPrincipal>.CopyFrom(userRequest);
            var user = userRequest.Result;

            Activate(user);
            LoginRecord(user);

            var session = Session_Create(user, null, TimeSpan.FromDays(1), loginAction);
            return ServiceResult<UserDataPrincipal>.AsSuccess(session);
        }

        public ServiceResult<UserDataPrincipal> LoginWithCode(EmailTokenModel model, Action<UserDataPrincipal> loginAction)
        {
            var userRequest = User_GetAccountByOneTimePassword(model);
            if (!userRequest.Success) return ServiceResult<UserDataPrincipal>.CopyFrom(userRequest);
            var user = userRequest.Result;

            Activate(user);
            LoginRecord(user);

            var session = Session_Create(user, null, TimeSpan.FromDays(1), loginAction);
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
            var userData = Mapper.Map<UserData>(user);
            userData.UserDeviceId = userDeviceId;
            return userData;
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
                    DeviceId = deviceId
                }).Entity;

                Context.SaveChanges();

                //Dealing with concurrency
                device = Context.UserDevices.FirstOrDefault(d => d.UserId == account.Id && d.DeviceId == deviceId);
            }

            return device;
        }

        internal UserDataPrincipal CreateImpersonationTicket(User account, Action<UserDataPrincipal> loginAction)
        {
            var currentUser = User.UserData();
            var impersonatedUser = Mapper.Map<UserData>(account);
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

            var account = this.Get(accountId, visibleOnly: true);
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
            userData.RemoveImpersonation();

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
                    CompanyId = selectedRole.CompanyId
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
            var createModel = Mapper.Map<UserCreateModel>(model);

            var createAccountRequest = Create(createModel);
            var account = createAccountRequest.Result;
            if (createAccountRequest.Success)
            {
                if (!model.IsAnonymous)
                {
                    if (_config.UserConfirmationMethod == UserConfirmationMethod.Link) SendWelcomeEmail(account, model.IsApi);

                    if (model.MixpanelTempId != null)
                    {
                        Mixpanel.CreateAlias(account, model.MixpanelTempId.Value);
                    }

                    Mixpanel.SendAccountToMixpanel(account, "Account created", tempUserId: model.MixpanelTempId);   //Use temp id as mixplanel can take too long to register the alias in previous call.
                }

                return ServiceResult<UserData>.AsSuccess(Mapper.Map<UserData>(account));
            }
            else
            {
                if (!model.IsAnonymous && sendEmail && !String.IsNullOrEmpty(model.Email))
                    QueueMail_Distinct(RazorTemplates.AccountAlreadyRegistered, model.Email, new RazorTemplateDataModel<string> { Data = model.Email }, new TimeSpan(24, 0, 0));

                return ServiceResult<UserData>.AsError("Already registered");
            }
        }

        public ServiceResult<AccountViewModel> CreateOrGet(RegistrationViewModel model)
        {
            var userId = GetIdByEmail(model.Email);

            if (userId == null)
            {
                var createRequest = Create(model);
                if (!createRequest.Success) return ServiceResult<AccountViewModel>.CopyFrom(createRequest);
                userId = createRequest.Result.UserId;
            }

            return ServiceResult<AccountViewModel>.AsSuccess(Get(userId.Value, false));
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

            account.UpdatedDate = DateTime.Now;
            Context.SaveChanges();

            return ServiceResult.AsSuccess();
        }

        private ServiceResult<User> Create(UserCreateModel model)
        {
            var account = String.IsNullOrEmpty(model.Email) ? GetAccountByPhone(model.Phone, includeDeleted: true) : GetAccountByEmail(model.Email, includeDeleted: true);

            if (account == null || (account.Status == UserStatus.Invited || account.Status == UserStatus.Deleted))
            {
                if (account == null)
                {
                    account = Context.Users.Add(new User() { CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow, UserRoles = new List<UserRole>() }).Entity;
                }

                var password = model.Password;
                if (string.IsNullOrEmpty(password))
                {
                    password = Password.GenerateHumanReadable(3, 3, 0);
                    account.PasswordAutoGenerated = true;
                }
                Password_Set(account, password);

                Mapper.Map(model, account);

                var newRolesResponse = CreateNewAccountRoles(account, model.UserRoles);
                if (!newRolesResponse.Success)
                    return ServiceResult<User>.CopyFrom(newRolesResponse);

                MergeAccountRoles(account, newRolesResponse.Result, deleteUnmatched: true);

                if (model.Status == UserStatus.Registered)
                {
                    if (_config.UserConfirmationMethod == UserConfirmationMethod.Link)
                        Token_Add(account, UserTokenType.Activate, new TimeSpan(14, 0, 0, 0));
                    else
                        Password_OneTime(account);
                }
                else if (model.Status == UserStatus.Invited)
                {
                    account.InvitedDate = DateTime.UtcNow;
                    Token_Add(account, UserTokenType.Invite, new TimeSpan(7, 0, 0, 0));
                }

                if (model.ProfilePhotoFile != null)
                    account.ProfilePhotoPath = _fileStorage.Save(new StorageCommand { Folder = ProfilePhotoFolder }.SetHttpPostedFileSource(model.ProfilePhotoFile));

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

        public ServiceResult Activate(EmailTokenModel model)
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

                if (!onBehalfOf && User.Identity.IsAuthenticated)
                {
                    _session.ClearSessionCache(User.Session()?.Id);
                }
            }

            return ServiceResult.AsSuccess();
        }

        public async Task SoftDeleteAsync(int accountId)
        {
            User account = Context.Users.Find(accountId);

            if (account != null)
            {
                account.Status = UserStatus.Deleted;
                account.ClosedDate = DateTime.UtcNow;
                account.UpdatedDate = DateTime.UtcNow;

                await Context.SaveChangesAsync();
            }
        }

        public AccountViewModel Get(int id, bool visibleOnly)
        {
            var query = visibleOnly ? this.VisibleUsers() : Context.Users;

            return query.Where(a => a.Id == id)
                        .Materialize<User, AccountViewModel>()
                        .FirstOrDefault();
        }

        public ServiceResult<AccountDetailsEditModel> GetForEdit(int accountId)
        {
            var account = Context.Users.Where(a => a.Id == accountId).FirstOrDefault();
            if (account == null)
                return ServiceResult<AccountDetailsEditModel>.AsError("Account not found or access denied.");

            var mapped = Mapper.Map<User, AccountDetailsEditModel>(account);
            return ServiceResult<AccountDetailsEditModel>.AsSuccess(mapped);
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

            user = Mapper.Map(model, user);
            Context.SaveChanges();
            Mixpanel.SendAccountToMixpanel(user);

            if (!onBehalfOfUser)
            {
                _session.ClearSessionCache(currentSession?.Id);
            }

            return ServiceResult.AsSuccess();
        }

        internal User GetAccount(int? id)
        {
            if (id == null)
                return null;

            var account = Context.Users.Where(a => a.Id == id && a.Status != UserStatus.Deleted)
                            .Include(a => a.Tokens)
                            .Include(a => a.UserRoles)
                            .ThenInclude((UserRole r) => r.Company)
                            .FirstOrDefault();

            return account;
        }

        public bool Exists(string email, int accountId)
        {
            using (MiniProfiler.Current.Step("AccountService.Exists"))
            {
                if (String.IsNullOrEmpty(email)) return false;
                return Context.Users.Where(a => a.Email == email && a.Id != accountId && a.Status != UserStatus.Deleted).Any();
            }
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

        public ServiceResult<OnboardingStep> GetOnboardingStep()
        {
            var userData = User.UserData();
            if (userData == null)
                return ServiceResult<OnboardingStep>.AsError("Not logged in.");

            if (userData.IsInRole(Role.Administrator))
                return ServiceResult<OnboardingStep>.AsSuccess(OnboardingStep.Complete);

            OnboardingStep step = OnboardingStep.Complete;

            if (userData.IsInRole(Role.CompanyAdmin))
            {
                var companyId = userData.GetCompanyIds().FirstOrDefault();
                var company = Context.Companies.Where(c => c.Id == companyId).Select(c => new { c.IsSetup }).FirstOrDefault();
                if (company == null)
                    return ServiceResult<OnboardingStep>.AsError("Company not found for company admin.");

                if (!company.IsSetup)
                {
                    step = OnboardingStep.SetupCompany;
                }
            }

            return ServiceResult<OnboardingStep>.AsSuccess(step);
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
    }
}