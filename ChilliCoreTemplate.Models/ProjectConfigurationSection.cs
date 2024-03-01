using ChilliSource.Cloud;
using ChilliSource.Core.Extensions;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;
using ChilliCoreTemplate.Models.EmailAccount;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using ChilliCoreTemplate.Models.Stripe;
using ChilliCoreTemplate.Models.Api.OAuth;
using System.Text.RegularExpressions;
using ChilliSource.Cloud.Core.Email;

namespace ChilliCoreTemplate.Models
{
    public static class ConfigurationSectionExtensions
    {
        public static string GetRequiredString(this IConfigurationSection section, string key)
        {
            var value = section?.GetString(key);
            if (String.IsNullOrEmpty(value))
            {
                throw new ApplicationException($"Configuration value not found or empty: {section.Key}:{key}");
            }

            return value;
        }

        public static string GetString(this IConfigurationSection section, string key)
        {
            return section?.GetValue<string>(key);
        }

        public static T GetRequiredValue<T>(this IConfigurationSection section, string key)
        {
            if (typeof(T) == typeof(string))
                return (T)(object)GetRequiredString(section, key);

            if (section == null)
                return default(T);

            var value = section.GetValue<T>(key);
            if (value == null)
            {
                throw new ApplicationException($"Configuration value not found: {section.Key}:{key}");
            }

            return value;
        }
    }

    public class ProjectSettings
    {
        private readonly IConfigurationSection _baseSection;
        private readonly FileStorageSection _fileStorageSection;
        private readonly EmailTemplateSection _emailTemplateSection;
        private readonly MailConfigurationSection _mailConfigurationSection;
        private readonly ApiConfigurationSection _apiConfigurationSection;
        private readonly HostingSection _hostingSection;
        private readonly GoogleApisSection _googleApisSection;

        public ProjectSettings(IConfiguration configuration)
        {
            _baseSection = configuration.GetSection("ProjectSettings:Base");
            AppSettings = new AppConfigurationSection(configuration.GetSection("ProjectSettings:AppSettings"));
            _fileStorageSection = new FileStorageSection(configuration.GetSection("ProjectSettings:FileStorage"));
            _emailTemplateSection = new EmailTemplateSection(configuration.GetSection("ProjectSettings:EmailTemplate"));
            _mailConfigurationSection = new MailConfigurationSection(configuration.GetSection("MailSettings"));
            SmsSettings = new SmsConfigurationSection(configuration.GetSection("SmsSettings"));
            _apiConfigurationSection = new ApiConfigurationSection(configuration.GetSection("ProjectSettings:Api"));
            _hostingSection = new HostingSection(configuration.GetSection("ProjectSettings:Hosting"));
            MfaSettings = new MfaConfigurationSection(configuration.GetSection("ProjectSettings:Mfa"));
            OAuthsSettings = new OAuthsConfigurationSection(configuration.GetSection("ProjectSettings:OAuth"));
            _googleApisSection = new GoogleApisSection(configuration.GetSection("ProjectSettings:GoogleApis"));
            GoogleTagManager = new GoogleTagManagerSection(configuration.GetSection("ProjectSettings:GoogleTagManager"));
            StripeSettings = new StripeConfigurationSection(configuration.GetSection("ProjectSettings:Stripe"));
            ErrorLogSettings = new ErrorLogConfigurationSection(configuration.GetSection("ErrorLog"));
            SlackSettings = new SlackConfigurationSection(configuration.GetSection("Slack"));
        }

        /// <summary>
        /// Build a url using base url to replace "~"
        /// </summary>
        /// <param name="url">url to build. Pass in the format "~/myurl/tobuild"</param>
        /// <param name="parameters">anonymous object whose properties are turned into querystring key value pairs</param>
        /// <returns></returns>
        public string ResolveUrl(string url, object parameters = null)
        {
            url = url.Replace("~", this.BaseUrl);
            if (parameters != null)
            {
                var queryString = HttpUtility.ParseQueryString("");

                Type t = parameters.GetType();
                foreach (var property in t.GetProperties())
                {
                    queryString[property.Name] = property.GetValue(parameters, null) == null ? "" : property.GetValue(parameters, null).ToString();
                }
                url = String.Format("{0}?{1}", url, queryString.ToString());
            }

            return url;
        }

        public string ResolveAppUrl(string url, object parameters = null)
        {
            return ResolveUrl(url.Replace("~", this.AppUrl), parameters);
        }

        public string ResolveApiUrl(string url, object parameters = null)
        {
            return ResolveUrl(url.Replace("~", this.ApiUrl), parameters);
        }

        /// <summary>
        /// Gets a value of base url for the project configuration.
        /// </summary>
        public string BaseUrl => _baseSection.GetRequiredString("BaseUrl");

        public string AppUrl => _baseSection.GetString("AppUrl").DefaultTo(BaseUrl);

        public string ApiUrl => _baseSection.GetString("ApiUrl").DefaultTo(BaseUrl);

        /// <summary>
        /// Gets a value of host name for the project configuration.
        /// </summary>
        /// <returns></returns>
        public string HostName()
        {
            return new Uri(BaseUrl).Host;
        }

        /// <summary>
        /// Public url is used when public site is hosted elsewhere from the application. Eq squarespace. Defaults to BaseUrl if not entered.
        /// </summary>
        public string PublicUrl => _baseSection.GetString("PublicUrl").DefaultTo(BaseUrl);

        /// <summary>
        /// Gets a value of unique project id for this project, which is used as one of the seeds to some encryption and hashing algorithms.
        /// </summary>
        public Guid? ProjectId => _baseSection.GetRequiredValue<Guid?>("ProjectId");

        /// <summary>
        /// Gets a value of project name, which is used in some html and meta helpers as a default value.
        /// </summary>        
        public string ProjectName => _baseSection.GetRequiredString("ProjectName");

        /// <summary>
        /// Gets a value of project display name.
        /// </summary>
        public string ProjectDisplayName => _baseSection.GetString("ProjectDisplayName").DefaultTo(ProjectName);

        /// <summary>
        /// Legal entity of project
        /// </summary>        
        public string LegalName => _baseSection.GetRequiredString("LegalName");

        /// <summary>
        /// Email to send internal emails to
        /// </summary>
        public string AdminEmail => _baseSection.GetRequiredString("AdminEmail");

        /// <summary>
        /// Session length in hours
        /// </summary>
        public int SessionLength => _baseSection.GetValue<int>("SessionLength");

        /// <summary>
        /// Session length in hours for devices if using cookieless flag
        /// </summary>
        public int SessionLengthDevice => _baseSection.GetValue<int>("SessionLengthDevice");

        public bool UnbundledFiles => _baseSection.GetValue<bool?>("UnbundledFiles") ?? false;

        public bool PurgeOldAnonymousAccounts => _baseSection.GetValue<bool?>("PurgeOldAnonymousAccounts") ?? false;

        public bool HasMasterCompany => _baseSection.GetValue<bool?>("HasMasterCompany") ?? false;

        /// <summary>
        /// Do users use a link (default) or a one time password to confirm their email address?
        /// </summary>
        public UserConfirmationMethod UserConfirmationMethod => _baseSection.GetValue<UserConfirmationMethod?>("UserConfirmationMethod") ?? UserConfirmationMethod.Link;

        public AppConfigurationSection AppSettings;

        /// <summary>
        /// Gets an instance of the FileStorageSection section.
        /// </summary>        
        public FileStorageSection FileStorage => _fileStorageSection;

        /// <summary>
        /// Gets an instance of the Mail settings section.
        /// </summary>
        public MailConfigurationSection MailSettings => _mailConfigurationSection;

        /// <summary>
        /// Gets an instance of the Sms settings section.
        /// </summary>
        public SmsConfigurationSection SmsSettings;

        /// <summary>
        /// Gets an instance of the Api settings section.
        /// </summary>
        public ApiConfigurationSection ApiSettings => _apiConfigurationSection;

        /// <summary>
        /// Gets an instance of the Email Template settings section.
        /// </summary>
        public EmailTemplateSection EmailTemplate => _emailTemplateSection;

        public HostingSection Hosting => _hostingSection;

        public MfaConfigurationSection MfaSettings { get; }

        public OAuthsConfigurationSection OAuthsSettings { get; }

        public GoogleApisSection GoogleApis => _googleApisSection;

        public GoogleTagManagerSection GoogleTagManager;

        public StripeConfigurationSection StripeSettings { get; }

        public ErrorLogConfigurationSection ErrorLogSettings { get; }

        public SlackConfigurationSection SlackSettings { get; }
    }

    #region Custom elements


    /// <summary>
    /// Represents S3 configuration element within a configuration file.
    /// </summary>
    public class S3Element
    {
        IConfigurationSection _section;

        public S3Element(IConfigurationSection section)
        {
            _section = section;
        }

        /// <summary>
        /// Gets a value of S3 access key Id.
        /// </summary>        
        public string AccessKeyId => _section.GetString("S3:AccessKeyId");

        /// <summary>
        /// Gets a value of S3 secret access key.
        /// </summary>        
        public string SecretAccessKey => _section.GetString("S3:SecretAccessKey");

        /// <summary>
        /// Gets a value of S3 bucket name.
        /// </summary>        
        public string Bucket => _section.GetString("S3:Bucket");

        /// <summary>
        /// Gets a value of S3 host name.
        /// </summary>        
        public string Host => _section.GetString("S3:Host") ?? "s3.amazonaws.com";

        public string ImagePrefix => _section.GetRequiredString("S3:ImagePrefix");
    }

    /// <summary>
    /// Represents local storage configuration element within a configuration file.
    /// </summary>
    public class LocalStorageElement
    {
        IConfigurationSection _section;

        public LocalStorageElement(IConfigurationSection section)
        {
            _section = section;
        }

        public string BasePath => _section.GetString("Local:BasePath");

        public string ImagePrefix => _section.GetRequiredString("Local:ImagePrefix");
    }


    public enum FileStorageProvider : int
    {
        S3 = 1,
        Azure,
        Local
    }

    /// <summary>
    /// Represents azure storage configuration element within a configuration file.
    /// </summary>
    public class FileStorageSection
    {
        IConfigurationSection _section;
        S3Element _s3;
        AzureStorageElement _azure;
        LocalStorageElement _local;


        public FileStorageSection(IConfigurationSection section)
        {
            _section = section;
            _s3 = new S3Element(section);
            _azure = new AzureStorageElement(section);
            _local = new LocalStorageElement(section);
        }

        /// <summary>
        /// Gets the default storage provider: S3 or Azure
        /// </summary>        
        public FileStorageProvider DefaultProvider => _section.GetRequiredValue<FileStorageProvider?>("DefaultProvider").Value;

        /// <summary>
        /// Gets a value representing a S3Element from configuration file.
        /// </summary>        
        public S3Element S3 => _s3;

        /// <summary>
        /// Gets a value representing a AzureStorageElement from configuration file.
        /// </summary>        
        public AzureStorageElement Azure => _azure;

        public LocalStorageElement Local => _local;
    }

    /// <summary>
    /// Represents azure storage configuration element within a configuration file.
    /// </summary>
    public class AzureStorageElement
    {
        IConfigurationSection _section;

        public AzureStorageElement(IConfigurationSection section)
        {
            _section = section;
        }

        /// <summary>
        /// Gets a value of azure storage account name.
        /// </summary>        
        public string AccountName => _section.GetString("Azure:AccountName");

        /// <summary>
        /// Gets a value of azure storage account key.
        /// </summary>
        public string AccountKey => _section.GetString("Azure:AccountKey");

        /// <summary>
        /// Gets a value of azure storage container.
        /// </summary>        
        public string Container => _section.GetString("Azure:Container");

        /// <summary>
        /// Gets a value of azure storage container.
        /// </summary>        
        public string ImagePrefix => _section.GetString("Azure:ImagePrefix");
    }


    #endregion

    public class MailAddressConfiguration
    {
        private readonly IConfigurationSection _configuration;
        private readonly string _address;
        private readonly string _displayName;

        public MailAddressConfiguration(IConfigurationSection configuration)
        {
            _configuration = configuration;
            _address = _configuration.GetString("address");
            _displayName = _configuration.GetString("displayName");
        }

        public EmailData_Address EmailAddress => !String.IsNullOrWhiteSpace(_address) ? new EmailData_Address(_address, _displayName) : null;

        public string Address => _address;
        public string DisplayName => _displayName;
    }

    public class MailConfigurationSection
    {
        private readonly IConfigurationSection _section;
        private readonly MailAddressConfiguration _fromAddressConfiguration;
        private readonly MailAddressConfiguration _redirecToAddressConfigiration;
        private readonly MailAddressConfiguration _bccAddressConfiguration;

        public MailConfigurationSection(IConfigurationSection section)
        {
            _section = section;
            _fromAddressConfiguration = new MailAddressConfiguration(_section.GetSection("from"));
            _redirecToAddressConfigiration = new MailAddressConfiguration(_section.GetSection("redirectTo"));
            _bccAddressConfiguration = new MailAddressConfiguration(_section.GetSection("bcc"));
        }

        /// <summary>
        /// Gets the default storage provider: S3 or Azure
        /// </summary>        
        public EmailData_Address From => _fromAddressConfiguration?.EmailAddress;


        public string Host => _section.GetString("network:host");

        public int Port => _section.GetValue<int>("network:port");

        public string UserName => _section.GetString("network:username");

        public string Password => _section.GetString("network:password");

        public bool EnableSsl => _section.GetValue<bool>("network:enableSsl");

        public MailConfigurationQuarantine Quarantine => MailConfigurationQuarantine.FromSection(_section);

        public EmailData_Address Bcc => _bccAddressConfiguration?.EmailAddress;
    }

    public class MailConfigurationQuarantine
    {
        /// <summary>
        /// example.com,test.com
        /// </summary>
        public IEnumerable<string> SafeDomains { get; set; }

        /// <summary>
        /// jim@example.com,jane@test.com
        /// </summary>
        public IEnumerable<string> SafeEmails { get; set; }

        /// <summary>
        /// mailinator.com. Emails not in safe domains will be sent to originalemail@mailinator.com. Where original email will have non alpanumeric characters replaced.
        /// </summary>
        public string QuarantineDomain { get; set; }

        internal static MailConfigurationQuarantine FromSection(IConfigurationSection section)
        {
            var safeDomains = section.GetString("quarantine:safeDomains");
            var safeEmails = section.GetString("quarantine:safeEmails");
            return new MailConfigurationQuarantine
            {
                QuarantineDomain = section.GetString("quarantine:quarantineDomain"),
                SafeDomains = String.IsNullOrEmpty(safeDomains) ? new List<string>() : safeDomains.Split(','),
                SafeEmails = String.IsNullOrEmpty(safeEmails) ? new List<string>() : safeEmails.Split(',')
            };
        }

        public bool ShouldQuarantine(string email)
        {
            if (!SafeDomains.Any()) return false;
            var domain = email.GetEmailAddressDomain();
            return !SafeDomains.Any(x => x.Same(domain)) && !SafeEmails.Any(x => x.Same(email));
        }

        public string Quarantine(string email, bool noDomain = false)
        {
            var sanitised = $"{email.Replace('@', '-').Replace('.', '_')}";
            return noDomain ? sanitised : $"{sanitised}@{QuarantineDomain}";
        }
    }

    public enum SmsProvider
    {
        Email = 1,
        Twilio
    }

    public class SmsConfigurationSection
    {
        private readonly IConfigurationSection _section;

        public SmsConfigurationSection(IConfigurationSection section)
        {
            _section = section;
        }

        public SmsProvider Provider => _section.GetRequiredValue<SmsProvider?>("Provider").Value;

        public string UserName => _section.GetString("Username");

        public string Password => _section.GetString("Password");

        public string SendUrl => _section.GetString("SendUrl");

        public string CallbackUrl => _section.GetString("CallbackUrl");

        public string ServiceId => _section.GetString("ServiceId");

        public string From => _section.GetString("From");

        public Regex SendViaEmailRegex => String.IsNullOrEmpty(_section.GetString("SendViaEmailRegex")) ? null : new Regex(_section.GetString("SendViaEmailRegex"));

    }

    /// <summary>
    /// Place to store one off application setting
    /// </summary>
    public class AppConfigurationSection
    {
        private readonly IConfigurationSection _section;

        public AppConfigurationSection(IConfigurationSection section)
        {
            _section = section;
        }

    }

    public class ApiConfigurationSection
    {
        private readonly IConfigurationSection _section;

        public ApiConfigurationSection(IConfigurationSection section)
        {
            _section = section;
        }

        /// <summary>
        /// Gets the api key value.
        /// </summary>
        public string ApiKey => _section.GetString("ApiKey");

        /// <summary>
        /// Log all api calls. Not recommended for production due to leaking confidential information
        /// </summary>
        public bool LogApiCalls => _section.GetValue<bool?>("LogApiCalls") ?? false;

        /// <summary>
        /// Filter out status codes not interested in for saving space / logging burden (eg 200)
        /// </summary>
        public int[] LogApiIgnore => _section.GetSection("LogApiIgnore").Get<int[]>() ?? Array.Empty<int>();
    }

    public class EmailTemplateSection
    {
        private readonly IConfigurationSection _section;

        public EmailTemplateSection(IConfigurationSection section)
        {
            _section = section;
        }

        /// <summary>
        /// Gets the bucket name
        /// </summary>
        public string Bucket => _section.GetString("Bucket");

        /// <summary>
        /// Gets the bucket name
        /// </summary>
        public string Email => _section.GetString("Email");
    }

    public class HostingSection
    {
        private readonly IConfigurationSection _section;

        public HostingSection(IConfigurationSection section)
        {
            _section = section;
        }

        /// <summary>
        /// Gets the allowed hosts (defaults to *). Comma delimited.
        /// https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.builder.forwardedheadersoptions.allowedhosts
        /// </summary>
        public string[] AllowedHosts => (_section.GetString("AllowedHosts") ?? "*").Split(',');

        /// <summary>
        /// Whether to redirect requests to the index.html page (for react projects)
        /// </summary>
        public bool UseIndexHtml => _section.GetValue<bool>("UseIndexHtml");

        /// <summary>
        /// Traffic from this host overrides setting of UseIndexHtml
        /// </summary>
        public string AdminHost => _section.GetString("AdminHost");

        /// <summary>
        /// Gets the bucket name
        /// </summary>
        public bool UnderELB => _section.GetValue<bool>("UnderELB");

        /// <summary>
        /// Gets the bucket name
        /// </summary>
        public bool HttpsOnly => _section.GetValue<bool>("HttpsOnly");

        public int? HttpsPort => _section.GetValue<int?>("HttpsPort");

        /// <summary>
        /// Gets the bucket name
        /// </summary>
        public bool Hsts => _section.GetValue<bool>("Hsts");
    }

    public class MfaConfigurationSection
    {
        private readonly IConfigurationSection _section;

        public MfaConfigurationSection(IConfigurationSection section)
        {
            _section = section;
        }

        public bool Enabled => _section.GetValue<bool>("Enabled");

        public string Secret => _section.GetRequiredString("Secret");

        public int? TrustDeviceInDays => _section.GetValue<int?>("TrustDeviceInDays");
    }

    public class OAuthsConfigurationSection
    {
        private readonly IConfigurationSection _section;

        public OAuthsConfigurationSection(IConfigurationSection section)
        {
            _section = section;
        }

        public string DefaultUrl => _section.GetString("DefaultUrl");

        public List<OAuthsConfigurationElement> OAuths => _section.GetSection("OAuths").GetChildren().Select(x => new OAuthsConfigurationElement(x)).ToList();

    }

    public class OAuthsConfigurationElement
    {
        private readonly IConfigurationSection _section;

        public OAuthsConfigurationElement(IConfigurationSection section)
        {
            _section = section;
        }

        public OAuthProvider Provider => EnumHelper.Parse<OAuthProvider>(_section.GetString("Provider") ?? "Unknown");

        public string AppBundleId => _section.GetString("AppBundleId");

        public string ClientId => _section.GetString("ClientId");

        public string ClientSecret => _section.GetString("ClientSecret");

        public OAuthJWT ClientJWT => _section.GetSection("ClientJWT").Get<OAuthJWT>();

        public string Scopes => _section.GetString("Scopes");

        public bool AutoLink => _section.GetValue<bool>("AutoLink");

        public bool AutoSignInUp => _section.GetValue<bool>("AutoSignInUp");

    }
    public class GoogleApisSection
    {
        private readonly IConfigurationSection _section;

        public GoogleApisSection(IConfigurationSection section)
        {
            _section = section;
        }

        /// <summary>
        /// Gets the api key value.
        /// </summary>
        public string ApiKey => _section.GetString("ApiKey");

        public string Libraries => _section.GetString("Libraries");

        /// <summary>
        /// Gets the private api key value (server side calls).
        /// </summary>
        public string PrivateApiKey => _section.GetString("PrivateApiKey");

        /// <summary>
        /// Gets the timezone api key value (server side calls).
        /// </summary>
        public string TimezoneApiKey => _section.GetString("TimezoneApiKey");

    }

    public class GoogleTagManagerSection
    {
        private readonly IConfigurationSection _section;

        public GoogleTagManagerSection(IConfigurationSection section)
        {
            _section = section;
        }

        /// <summary>
        /// When enabled component is rendered
        /// </summary>
        public bool Enabled => _section.GetValue<bool>("Enabled");

        /// <summary>
        /// Gets the tag id value.
        /// </summary>
        public string TagId => _section.GetString("TagId");

    }

    public class ErrorLogConfigurationSection
    {
        private readonly IConfigurationSection _section;

        public ErrorLogConfigurationSection(IConfigurationSection section)
        {
            _section = section;
        }

        /// <summary>
        /// When enabled error log emails are sent
        /// </summary>
        public bool Enabled => _section.GetValue<bool>("Enabled");

        /// <summary>
        /// Trigger to send error alert email
        /// </summary>
        public int ErrorCount => _section.GetValue<int>("ErrorCount");

        /// <summary>
        /// Csv list of emails to send errors emails to 
        /// </summary>
        public string EmailTo => _section.GetString("EmailTo");

        /// <summary>
        /// Csv list of days to send error summary emails to 
        /// </summary>
        public string ErrorDays => _section.GetString("ErrorDays");

    }

    public class SlackConfigurationSection
    {
        private readonly IConfigurationSection _section;

        public SlackConfigurationSection(IConfigurationSection section)
        {
            _section = section;
        }

        /// <summary>
        /// Message destination
        /// </summary>
        public string WebhookUrl => _section.GetString("WebhookUrl");

        /// <summary>
        /// Only when enabled will slack service attempt to send messages (defaults to false if not specified)
        /// </summary>
        public bool Enabled => _section.GetValue<bool?>("Enabled") ?? false;

    }
}