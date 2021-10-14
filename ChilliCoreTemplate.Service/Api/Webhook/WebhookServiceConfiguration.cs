using System.Configuration;

namespace ChilliCoreTemplate.Service
{
    public class WebhookServiceConfiguration : ConfigurationSection
    {
        public static WebhookServiceConfiguration GetConfig()
        {
            var config = (WebhookServiceConfiguration)System.Configuration.ConfigurationManager.GetSection("webhook");
            return config;
        }

        [ConfigurationProperty("enabled", IsRequired = true)]
        public bool Enabled
        {
            get
            {
                return (bool)this["enabled"];
            }
            set
            {
                this["enabled"] = value;
            }
        }

        /// <summary>
        /// Overrride used when not subscribing/unsubscribing to webhooks
        /// </summary>
        [ConfigurationProperty("targetURL", IsRequired = false)]
        public string TargetURL
        {
            get
            {
                return (string)this["targetURL"];
            }
            set
            {
                this["targetURL"] = value;
            }
        }

    }
}
