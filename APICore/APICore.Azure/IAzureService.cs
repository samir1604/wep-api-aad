using APICore.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APICore.Azure
{
    public interface IAzureService
    {
        Task<(User user, string token, string refreshToken)> GetAADAuthToken(string user, string password);
    }
}
