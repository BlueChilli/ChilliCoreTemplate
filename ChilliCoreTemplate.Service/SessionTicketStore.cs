using ChilliCoreTemplate.Models.EmailAccount;
using ChilliCoreTemplate.Service.EmailAccount;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Service
{
    public class SingletonTicketStore : ITicketStore
    {
        IServiceProvider _serviceProvider;

        public SingletonTicketStore(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        private ScopedSessionTicketStore GetScopedTicketStore()
        {
            var context = _serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext;
            if (context == null)
            {
                throw new ApplicationException("Http context not found");
            }

            return context.RequestServices.GetRequiredService<ScopedSessionTicketStore>();
        }

        public Task RemoveAsync(string key)
        {
            return GetScopedTicketStore().RemoveAsync(key);
        }

        public Task RenewAsync(string key, AuthenticationTicket ticket)
        {
            return GetScopedTicketStore().RenewAsync(key, ticket);
        }

        public Task<AuthenticationTicket> RetrieveAsync(string key)
        {
            return GetScopedTicketStore().RetrieveAsync(key);
        }

        public Task<string> StoreAsync(AuthenticationTicket ticket)
        {
            return GetScopedTicketStore().StoreAsync(ticket);
        }
    }

    public class ScopedSessionTicketStore : ITicketStore
    {
        UserSessionService _sessionService;
        public ScopedSessionTicketStore(UserSessionService sessionService)
        {
            _sessionService = sessionService;
        }

        private UserDataPrincipal GetPrincipal(AuthenticationTicket ticket)
        {
            if (ticket == null)
                throw new ArgumentNullException(nameof(ticket));

            var principal = ticket.Principal as UserDataPrincipal;
            if (principal == null)
            {
                throw new ApplicationException("This Ticket store only supports UserDataPrincipal principals.");
            }

            return principal;
        }

        public Task RemoveAsync(string key)
        {
            return _sessionService.DeleteAsync(key);
        }

        public async Task RenewAsync(string key, AuthenticationTicket ticket)
        {
            var principal = GetPrincipal(ticket);

            if (await _sessionService.ReplaceAsync(key, principal.UserData))
            {
                principal.Id = key;
            }
        }

        public async Task<AuthenticationTicket> RetrieveAsync(string key)
        {
            var session = await _sessionService.GetAsync(key, CancellationToken.None);
            if (session == null)
                return null;

            var principal = new UserDataPrincipal(session.UserData)
            {
                Id = session.Id
            };

            return new AuthenticationTicket(principal, CookieAuthenticationDefaults.AuthenticationScheme);
        }

        public async Task<string> StoreAsync(AuthenticationTicket ticket)
        {
            var expiryUtc = ticket.Properties.ExpiresUtc.Value.UtcDateTime;

            var principal = GetPrincipal(ticket);
            //Checks if the session has already been created.
            var session = !String.IsNullOrEmpty(principal.Id) ? await _sessionService.GetAsync(principal.Id, CancellationToken.None) : null;

            if (session == null)
            {
                principal.Id = await _sessionService.CreateAsync(principal.UserData, expiryUtc);
            }
            else if (session.SessionExpiryOn != expiryUtc)
            {
                if (!await _sessionService.ReplaceAsync(principal.Id, principal.UserData))
                {
                    principal.Id = await _sessionService.CreateAsync(principal.UserData, expiryUtc);
                }
            }

            return principal.Id;
        }
    }
}
