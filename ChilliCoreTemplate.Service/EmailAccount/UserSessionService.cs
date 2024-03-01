using AutoMapper;
using ChilliSource.Core.Extensions;
using ChilliCoreTemplate.Data.EmailAccount;
using ChilliCoreTemplate.Models.EmailAccount;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChilliSource.Cloud.Core;
using ChilliCoreTemplate.Models;
using LazyCache;
using ChilliCoreTemplate.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Security.Principal;
using Microsoft.Extensions.Caching.Memory;
using ChilliSource.Cloud.Core.Distributed;

namespace ChilliCoreTemplate.Service.EmailAccount
{
    public class UserSessionService : IService
    {
        private readonly IAppCache _cache;
        private readonly UserKeyHelper _userKeyHelper;
        private readonly IMapper _mapper;
        private readonly DataContext Context;
        private readonly ProjectSettings _config;

        public UserSessionService(DataContext context, IAppCache cache, ProjectSettings config, UserKeyHelper userKeyHelper, IMapper mapper)
        {
            Context = context;
            _cache = cache;
            _config = config;
            _userKeyHelper = userKeyHelper;
            _mapper = mapper;
        }

        private void SetUserSession(UserSession session, UserData userData, DateTime expiresOn)
        {
            session.UserId = userData.UserId;
            session.UserDeviceId = userData.UserDeviceId;
            session.SessionCreatedOn = DateTime.UtcNow;
            session.SessionExpiryOn = expiresOn;
            session.ImpersonationChain = userData.ImpersonationChain().ToJson();
            session.IsMfaVerified = userData.IsMfaVerified || (userData.Impersonator?.IsMfaVerified ?? false);
        }

        public async Task<UserData> GetCompanySession(Guid guid)
        {
            var admin = await Context.Users
                .Include(a => a.UserRoles)
                .ThenInclude((UserRole r) => r.Company)
                .Where(x => x.Status != UserStatus.Deleted && x.UserRoles.Any(r => r.Role == Role.CompanyAdmin && r.Company.ApiKey == guid && !r.Company.IsDeleted))
                .FirstOrDefaultAsync();

            if (admin != null)
            {
                return _mapper.Map<UserData>(admin);
            }

            return null;
        }

        public async Task<string> CreateAsync(UserData userData, DateTime expiresOn)
        {
            var session = Context.UserSessions.Add(new UserSession { SessionId = Guid.NewGuid() }).Entity;
            SetUserSession(session, userData, expiresOn);
            await Context.SaveChangesAsync();

            var sessionId = session.SessionId.ToString();
            _cache.Add(sessionId, CreateSessionInfo(session, userData), policy: CreatePolicy());

            return sessionId;
        }

        internal UserData MapUserData(User user, int? userDeviceId)
        {
            var userData = _mapper.Map<UserData>(user, opts => opts.Items["DataContext"] = Context);
            userData.UserDeviceId = userDeviceId;
            return userData;
        }

        public async Task<SessionInfo> GetAsync(string sessionId, CancellationToken cancellationToken)
        {
            return await GetInternalAsync(sessionId, cancellationToken, isAsync: true);
        }

        public SessionInfo Get(string sessionId)
        {
            return SyncTaskHelper.ValidateSyncTask(GetInternalAsync(sessionId, CancellationToken.None, isAsync: false));
        }

        internal SessionInfo GetByUserKey(string userKey)
        {
            var sessionId = _userKeyHelper.UnprotectGuid(userKey);
            if (sessionId.HasValue) return Get(sessionId.ToString());
            return null;
        }

        private MemoryCacheEntryOptions CreatePolicy()
        {
            return new MemoryCacheEntryOptions()
            {
                AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(20)
            };
        }

        private async Task<SessionInfo> GetInternalAsync(string id, CancellationToken cancellationToken, bool isAsync)
        {
            var session = isAsync ? await _cache.GetOrAddAsync(id, () => LoadAsync(id, cancellationToken, isAsync: isAsync), policy: CreatePolicy())
                                   : _cache.GetOrAdd(id, () => SyncTaskHelper.ValidateSyncTask(LoadAsync(id, cancellationToken, isAsync: isAsync)), policy: CreatePolicy());

            if (session == null) return null;

            if (session.IsExpired())
            {
                if (isAsync)
                {
                    await DeleteAsync(session.Id);
                }
                else
                {
                    Delete(session.Id);
                }

                return null;
            }

            return session;
        }

        public async Task<bool> ReplaceAsync(string id, UserData userData)
        {
            var sessionGuid = GetGuidFromString(id);
            if (sessionGuid == null)
                return false;

            var session = await Context.UserSessions.Where(s => s.SessionId == sessionGuid)
                            .FirstOrDefaultAsync();

            if (session == null)
                return false;

            SetUserSession(session, userData, session.SessionExpiryOn);
            await Context.SaveChangesAsync();

            var sessionId = session.SessionId.ToString();
            _cache.Remove(sessionId);
            _cache.Add(sessionId, CreateSessionInfo(session, userData), policy: CreatePolicy());

            return true;
        }

        public Task DeleteAsync(string id)
        {
            return DeleteAsync(id, isAsync: true);
        }

        public void Delete(string id)
        {
            SyncTaskHelper.ValidateSyncTask(DeleteAsync(id, isAsync: false));
        }

        private async Task DeleteAsync(string id, bool isAsync)
        {
            var sessionGuid = GetGuidFromString(id);

            var query = Context.UserSessions.Where(x => x.SessionId == sessionGuid);
            var session = isAsync ? await query.FirstOrDefaultAsync()
                                  : query.FirstOrDefault();
            if (session != null)
            {
                Context.UserSessions.Remove(session);
                try
                {
                    _ = isAsync ? await Context.SaveChangesAsync()
                                : Context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                }
            }

            _cache.Remove(id);
        }

        private Guid? GetGuidFromString(string id)
        {
            if (String.IsNullOrEmpty(id))
                return null;

            Guid sessionGuid = Guid.Empty;
            if (!Guid.TryParse(id, out sessionGuid))
                return null;

            return sessionGuid;
        }

        private async Task<SessionInfo> LoadAsync(string id, CancellationToken cancellationToken, bool isAsync)
        {
            var sessionGuid = GetGuidFromString(id);
            if (sessionGuid == null)
                return null;

            var sessionQuery = Context.UserSessions
                                .Where(x => x.SessionId == sessionGuid && x.User.Status != UserStatus.Deleted && x.SessionExpiryOn > DateTime.UtcNow)
                                .Include(x => x.User.UserRoles)
                                .ThenInclude((UserRole r) => r.Company);

            var session = isAsync ? await sessionQuery.FirstOrDefaultAsync(cancellationToken)
                                  : sessionQuery.FirstOrDefault();

            if (session == null) return null;

            var userData = MapUserData(session.User, session.UserDeviceId);
            userData.IsMfaVerified = session.IsMfaVerified;

            if (!String.IsNullOrEmpty(session.ImpersonationChain))
            {
                var impersonationPointer = userData;
                var impersonationIds = session.ImpersonationChain.FromJson<List<int>>();
                impersonationIds.Reverse();
                foreach (var impersonationId in impersonationIds)
                {
                    var impersonationQuery = Context.Users.Where(u => u.Id == impersonationId)
                                                .Include(u => u.UserRoles)
                                                .ThenInclude((UserRole r) => r.Company);

                    var impersonation = isAsync ? await impersonationQuery.FirstOrDefaultAsync(cancellationToken)
                                                : impersonationQuery.FirstOrDefault();

                    var impersonationData = _mapper.Map<UserData>(impersonation);
                    impersonationData.IsMfaVerified = session.IsMfaVerified;
                    impersonationPointer.ImpersonatedBy(impersonationData);
                    impersonationPointer = impersonationData;
                }
            }

            return CreateSessionInfo(session, userData);
        }

        private SessionInfo CreateSessionInfo(UserSession session, UserData userData)
        {
            return new SessionInfo(userData)
            {
                Id = session.SessionId.ToString(),
                SessionExpiryOn = session.SessionExpiryOn
            };
        }

        public void Clean(ITaskExecutionInfo executionInfo)
        {
            //Delete old sessions & tokens
            Context.Database.ExecuteSqlRaw("delete from UserSessions where SessionExpiryOn < SYSUTCDATETIME()");
            Context.Database.ExecuteSqlRaw("delete from UserTokens where Expiry < DATEADD(year, -1, SYSUTCDATETIME())");

            executionInfo.SendAliveSignal();
            if (executionInfo.IsCancellationRequested)
                return;
        }

        public void ClearSessionCache(string id)
        {
            if (String.IsNullOrEmpty(id))
                return;

            var sessionGuid = GetGuidFromString(id);
            if (sessionGuid.HasValue) id = sessionGuid.ToString().ToLower();

            _cache.Remove(id);
        }
    }

    internal static class SyncTaskHelper
    {
        public static void ValidateSyncTask(Task task)
        {
            if (!task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
                throw new ApplicationException("The task was expected to be completed synchronously.");

            task.GetAwaiter().GetResult();
        }

        public static T ValidateSyncTask<T>(Task<T> task)
        {
            if (!task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
                throw new ApplicationException("The task was expected to be completed synchronously.");

            return task.GetAwaiter().GetResult();
        }
    }
}
