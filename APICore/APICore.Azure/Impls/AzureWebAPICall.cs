using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace APICore.Azure.Impls
{
    public class AzureWebAPICall
    {
        private readonly HttpClient _httpClient;

        public AzureWebAPICall()
        {
            _httpClient = new HttpClient();
        }

        public async Task<JObject> CallAPI(string apiUrl, string accessToken)
        {
            var defaultRequestHeaders = _httpClient.DefaultRequestHeaders;
            if(defaultRequestHeaders.Accept ==  null || 
                defaultRequestHeaders.Accept.Any(m => m.MediaType == "application/json"))
            {
                _httpClient.DefaultRequestHeaders.Accept.Add(
                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(
                        "application/json"));
            }
            defaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", accessToken);

            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
            string json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject(json) as JObject;
        }
    }
}
