using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NotificationCommon.Models;
using NotificationService.Processing;
using System.Net.Http;
using System.Threading.Tasks;

namespace NotificationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private IHttpClientFactory _clientFactory;
        private Config _config;
        public NotificationController(IHttpClientFactory clientFactory, IOptions<Config> config)
        {
            _clientFactory = clientFactory;
            _config = config.Value;
        }
        [HttpPost]
        public async Task SendNotification([FromBody] Sendout sendout)
        {
            var notificationToSend = await SendoutExtractAndProcessPipeline.
                    GetNotificationsAsync(_clientFactory, _config, sendout);
            await GatewayClient.GatewayClient.SendToExternalService(notificationToSend,
                _clientFactory, _config, Enums.ExternalServices.PostToNotificationGateway);
        }
    }
}