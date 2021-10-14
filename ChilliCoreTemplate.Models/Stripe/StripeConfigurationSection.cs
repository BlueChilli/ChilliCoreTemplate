using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChilliCoreTemplate.Models.Stripe
{
    public class StripeConfigurationSection
    {
        private readonly IConfigurationSection _section;

        public StripeConfigurationSection(IConfigurationSection section)
        {
            _section = section;
        }

        /// <summary>
        /// Gets the public api key value.
        /// </summary>
        public string PublicApiKey => _section.GetString("PublicApiKey");

        /// <summary>
        /// Gets the secret api key value.
        /// </summary>
        public string SecretApiKey => _section.GetString("SecretApiKey");
    }
}
