using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NotificationService.Enums;
using NotificationService.Processing;
using Serilog;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace NotificationService.Scheduler
{
    public class ScheduledTask : ScheduledProcessor
    {
        private Config _config;
        private IHttpClientFactory _clientFactory;
        public ScheduledTask(IServiceScopeFactory serviceScopeFactory, IOptions<Config> options, IHttpClientFactory clientFactory) : base(serviceScopeFactory)
        {
            _config = options.Value;
            _clientFactory = clientFactory;
        }

        protected override string Schedule => "* * * * *";

        public override async Task ProcessInScopeAsync(IServiceProvider serviceProvider)
        {
            Log.Information("Scheduled task started");
            var notificationToSend = await SendoutExtractAndProcessPipeline
                .GetNotificationsAsync(_clientFactory, _config);
            await GatewayClient.GatewayClient.SendToExternalService(notificationToSend, _clientFactory, _config, ExternalServices.PostToNotificationGateway);
        }
    }
}
