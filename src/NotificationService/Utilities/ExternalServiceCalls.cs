using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace NotificationService.Utilities
{
    public class ExternalServiceCalls
    {
        public async static Task<IEnumerable<T>> GetAllItems<T>(IHttpClientFactory clientFactory, string uri)
        {
            var client = clientFactory.CreateClient();
            var response = await client.GetAsync(uri);
            var result = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<IEnumerable<T>>(result);
        }
    }
}
