using ChilliSource.Core.Extensions;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Models.Api
{
    public class PushNotificationSettings
    {
        private readonly IConfigurationSection _section;

        public PushNotificationSettings(IConfiguration configuration)
        {
            _section = configuration.GetSection("PushNotifications");
        }

        public List<PushNotificationApp> Apps => _section.GetChildren().Select(x => new PushNotificationApp(x)).ToList();

    }

    public class PushNotificationApp
    {
        private readonly IConfigurationSection _section;

        public PushNotificationApp(IConfigurationSection section)
        {
            _section = section;
        }

        public PushNotificationAppId AppId => EnumHelper.Parse<PushNotificationAppId>(_section.GetRequiredString("AppId"));

        public AwsSnsConfiguration Aws => _aws.Exists() ? new AwsSnsConfiguration(_aws) : null;
        private IConfigurationSection _aws => _section.GetSection("AWS");

        public AzureSnsConfiguration Azure => _azure.Exists() ? new AzureSnsConfiguration(_azure) : null;
        private IConfigurationSection _azure => _section.GetSection("Azure");

        public FireBaseConfiguration FireBase => _fireBase.Exists() ? new FireBaseConfiguration(_fireBase) : null;
        private IConfigurationSection _fireBase => _section.GetSection("FireBase");

    }

    public class AwsSnsConfiguration
    {
        private readonly IConfigurationSection _section;

        public AwsSnsConfiguration(IConfigurationSection section)
        {
            _section = section;
        }

        public string AccessKey => _section.GetRequiredString("Accesskey");

        public string Secret => _section.GetRequiredString("Secret");

        public string IOSArn => _section.GetRequiredString("IOSArn");

        public string AndroidArn => _section.GetRequiredString("AndroidArn");
    }

    /// <summary>
    /// Main container for Azure sns configuration
    /// </summary>
    public class AzureSnsConfiguration
    {
        private readonly IConfigurationSection _section;

        public AzureSnsConfiguration(IConfigurationSection section)
        {
            _section = section;
        }


        public string ConnectionString => _section.GetRequiredString("ConnectionString");

        public string HubName => _section.GetRequiredString("HubName");

        public bool SandBox => _section.GetRequiredValue<bool>("SandBox");

    }

    /// <summary>
    /// Main container for FireBase configuration
    /// </summary>
    public class FireBaseConfiguration
    {
        private readonly IConfigurationSection _section;

        public FireBaseConfiguration(IConfigurationSection section)
        {
            _section = section;
        }


        public string CredentialPath => _section.GetRequiredString("CredentialPath");

    }

}
