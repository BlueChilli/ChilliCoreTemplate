using Microsoft.AspNetCore.DataProtection;
using System;
using System.Linq;
using System.Security.Principal;

namespace ChilliCoreTemplate.Models.EmailAccount
{
    public class UserDataPrincipal : GenericPrincipal
    {
        public UserDataPrincipal(UserData userData)
            : base(new GenericIdentity(String.IsNullOrEmpty(userData.Name) ? userData.Email : userData.Name), userData.Roles.Select(x => x.ToString()).ToArray())
        {
            if (userData == null)
                throw new ArgumentNullException(nameof(userData));

            this.UserData = userData;
        }

        public string Id { get; set; }

        public UserData UserData { get; private set; }
    }

    public class SessionInfo
    {
        public SessionInfo(UserData userData)
        {
            this.UserData = userData;
        }

        public string Id { get; set; }

        public DateTime SessionExpiryOn { get; set; }

        public UserData UserData { get; private set; }

        public bool IsExpired()
        {
            return this.SessionExpiryOn < DateTime.UtcNow;
        }
    }

    public class UserKeyHelper
    {
        IDataProtector _protector;
        ProjectSettings _settings;

        public UserKeyHelper(IDataProtectionProvider provider, ProjectSettings settings)
        {
            _settings = settings;
            var purpose = $"_{Convert.ToBase64String(settings.ProjectId.Value.ToByteArray())}_UserKeyHelper";
            _protector = provider.CreateProtector(purpose);
        }

        public string ProtectGuid(Guid value)
        {
            var encrypted = _protector.Protect(value.ToByteArray());
            return Convert.ToBase64String(encrypted);
        }

        public Guid? UnprotectGuid(string value)
        {
            if (String.IsNullOrEmpty(value))
                return null;

            try
            {
                var buffer = Convert.FromBase64String(value);
                var decrypted = _protector.Unprotect(buffer);
                return new Guid(decrypted);
            }
            catch
            {
                return null;
            }
        }
    }
}

