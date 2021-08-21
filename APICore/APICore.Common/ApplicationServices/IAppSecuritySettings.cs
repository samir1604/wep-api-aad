using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APICore.Common.ApplicationServices
{
    public interface IAppSecuritySettings
    {
        PublicClientApplicationOptions ApplicationSettings { get; }
        string MicrosoftGraphBaseEndpoint { get; set; }
    }
}
