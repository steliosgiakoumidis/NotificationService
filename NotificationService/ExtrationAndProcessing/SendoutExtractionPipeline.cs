using NotificationCommon.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using NotificationService.Enums;
using static NotificationCommon.Models.Enums;

namespace NotificationService.Processing
{
    public class SendoutExtractAndProcessPipeline
    {
        public async static Task<IEnumerable<GatewaySendout>> GetNotificationsAsync(IHttpClientFactory clientFactory, Config config)
        {
            var sendouts = await GetSendoutList(clientFactory, config);
            var templates = await GetTemplates(clientFactory, config);
            var users = await GetUsers(clientFactory, config);
            var usergroups = await GetUserGroups(clientFactory, config);
            var timeFilteredSendouts = FilterSendouts(sendouts);
            MarkAsSent(sendouts, clientFactory, config);
            return new SendoutProcessing().ParseTextAndSetUserPreferences(usergroups, users, templates,
                    timeFilteredSendouts);
        }

        private async static void MarkAsSent(IEnumerable<Sendout> sendouts, 
            IHttpClientFactory clientFactory, Config config)
        {
            await GatewayClient.GatewayClient.SendToExternalService(sendouts,
                clientFactory, config, ExternalServices.PutToNotificationApi);
        }

        private async static Task<IEnumerable<Sendout>> GetSendoutList(IHttpClientFactory clientFactory, Config config)
        {
            var uri = $"{config.NotificationApiUrl}/regularsendout";
            return await Utilities.ExternalServiceCalls.
                GetAllItems<Sendout>(clientFactory, uri);
        }

        private async static Task<IEnumerable<Template>> GetTemplates(IHttpClientFactory clientFactory, Config config)
        {
            var uri = $"{config.NotificationApiUrl}/template";
            return await Utilities.ExternalServiceCalls.
                GetAllItems<Template>(clientFactory, uri);
        }

        private async static Task<IEnumerable<User>> GetUsers(IHttpClientFactory clientFactory, Config config)
        {
            var uri = $"{config.NotificationApiUrl}/users";
            return await Utilities.ExternalServiceCalls.
                GetAllItems<User>(clientFactory, uri);
        }

        private async static Task<IEnumerable<UserGroup>> GetUserGroups(IHttpClientFactory clientFactory, Config config)
        {
            var uri = $"{config.NotificationApiUrl}/usergroups";
            return await Utilities.ExternalServiceCalls.
                GetAllItems<UserGroup>(clientFactory, uri);
        }

        private static IEnumerable<Sendout> FilterSendouts(IEnumerable<Sendout> sendouts)
        {
            return sendouts.AsParallel().Where(x => FilterSendout(x) == true);

        }

        private static bool FilterSendout(Sendout sendout)
        {
            if (sendout.RepetitionFrequency == RepetitionFrequency.Once &&
                RepetitionFrequencyRequirements.RepetitionFrequencyOnceRequirements(sendout)) return true;

            if (sendout.RepetitionFrequency == RepetitionFrequency.Daily &&
                RepetitionFrequencyRequirements.RepetitionFrequencyDailyRequirements(sendout)) return true;

            if (sendout.RepetitionFrequency == RepetitionFrequency.SecondDay &&
                RepetitionFrequencyRequirements.RepetitionFrequencySecondDayRequirements(sendout)) return true;

            if (sendout.RepetitionFrequency == RepetitionFrequency.SecondWeek &&
                RepetitionFrequencyRequirements.RepetitionFrequencySeconWeekRequirements(sendout,
                WeekOfTheYear(sendout.StartDate))) return true;

            if (sendout.RepetitionFrequency == RepetitionFrequency.ForthWeek &&
                RepetitionFrequencyRequirements.RepetitionFrequencyForthWeekRequirements(sendout,
                    WeekOfTheYear(sendout.StartDate))) return true;

            if (sendout.RepetitionFrequency == RepetitionFrequency.Weekly &&
                RepetitionFrequencyRequirements.RepetitionFrequencyWeeklyRequirements(sendout)) return true;

            if (sendout.RepetitionFrequency == RepetitionFrequency.Monthly &&
                RepetitionFrequencyRequirements.RepetitionFrequencyMonthlyRequirements(sendout)) return true;

            if (sendout.RepetitionFrequency == RepetitionFrequency.SecondMonth &&
                RepetitionFrequencyRequirements.RepetitionFrequencySecondMonthRequirements(sendout)) return true;

            //Notification will arrive at the controller and not through the scheduoed task
            if (sendout.RepetitionFrequency == RepetitionFrequency.Now &&
                RepetitionFrequencyRequirements.RepetitionFrequencySecondMonthRequirements(sendout)) return false;

            return false;
        }
        private static int WeekOfTheYear(DateTime datetime)
        {
            int weekOfYear = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(datetime, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            return weekOfYear;
        }
    }
}
