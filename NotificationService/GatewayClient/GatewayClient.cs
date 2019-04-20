using Newtonsoft.Json;
using NotificationCommon.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using NotificationService.Enums;
namespace NotificationService.GatewayClient
{
    public class GatewayClient
    {

        public async static Task SendToExternalService<T>(IEnumerable<T> listOfItems,
            IHttpClientFactory clientFactory, Config config, ExternalServices externalService)
        {
            foreach (var sendout in listOfItems)
            {
                var response = await PrepareClientAndSend(sendout, clientFactory, 
                    config, externalService);
                if (!response) Log.Error($"Communication could not be established, notification {sendout} could not be sent");
            }
        }

        private async static Task<bool> PrepareClientAndSend<T>(T sendout,
            IHttpClientFactory clientFactory, Config config, ExternalServices externalService)
        {         
            try
            {
                var httpContent = new StringContent(JsonConvert.SerializeObject(sendout).ToString());
                httpContent.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                var client = clientFactory.CreateClient();
                var response = externalService == ExternalServices.PutToNotificationApi ?
                    await client.PutAsync(config.NotificationApiUrl, httpContent) : 
                    await client.PostAsync(config.NotificationGatewayUrl, httpContent);
                return response.IsSuccessStatusCode;
            }
            catch(Exception ex)
            {
                Log.Error($"Http client related error caught. Error: {ex}");
            }
            return false;
        }
    }
}
