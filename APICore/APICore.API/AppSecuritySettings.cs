using APICore.Common.ApplicationServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APICore.API
{
    public class AppSecuritySettings : IAppSecuritySettings
    {
        public PublicClientApplicationOptions ApplicationSettings { get; private set; }
        public string MicrosoftGraphBaseEndpoint { get; set; }

        public static AppSecuritySettings ReadConfiguration(IConfiguration configuration)
        {
            IConfigurationRoot ConfigRoot;

            var builder = new ConfigurationBuilder().AddConfiguration(configuration);
            ConfigRoot = builder.Build();

            var settings = new AppSecuritySettings
            {
                ApplicationSettings = new PublicClientApplicationOptions()
            };

            ConfigRoot.Bind("Authentication", settings.ApplicationSettings);
            settings.MicrosoftGraphBaseEndpoint = ConfigRoot.GetValue<String>("WebAPI:MicrosoftGraphBaseEndpoint");

            return settings;
        }
    }
}
