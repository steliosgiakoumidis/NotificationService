using NotificationCommon.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using static NotificationCommon.Models.Enums;

namespace NotificationService.Processing
{
    public class SendoutProcessing
    {
        public IEnumerable<GatewaySendout> ParseTextAndSetUserPreferences(
            IEnumerable<UserGroup> userGroups,
            IEnumerable<User> users, 
            IEnumerable<Template> templates, 
            IEnumerable<Sendout> sendouts)
        {
            List<GatewaySendout> sendoutReadyList = new List<GatewaySendout>();
            foreach (var sendout in sendouts)
            {
                try
                {
                    var userList = new List<User>();
                    var template = templates.FirstOrDefault(x => x.NotificationName == sendout.ReminderName);
                    var user = users.FirstOrDefault(x => x.Username == sendout.Username);
                    var temp = userGroups
                        .FirstOrDefault(x => x.GroupName == sendout.UserGroup)
                        ?.UserIds;
                    var userGroup = temp?.Split(",")
                        .Select(x => Convert.ToInt32(x.Trim()))
                        .Select(x => users.FirstOrDefault(y => y.Id == x));
                    if (user != null) userList.Add(user);
                    if (userGroup != null) userList.AddRange(userGroup);
                    var meanOfCommunication = ExtractPriority(template, userList);
                    var listOfContactDetails = meanOfCommunication == MeansOfCommunication.Email ?
                        userList.Select(x => x.Email) : userList.Select(x => x.SMS);

                    sendoutReadyList.AddRange(listOfContactDetails.Select(x => new GatewaySendout()
                    {
                        Text = GetParsedNotificationText(sendout, template),
                        ContactDetails = x,
                        MeansOfCommunication = meanOfCommunication
                    }));
                }
                catch (Exception ex)
                {
                    Log.Error($"Sendout list can not be processed, please check checkout records. Error thrown: {ex}");
                }
            }
            return sendoutReadyList;
        }

        private MeansOfCommunication ExtractPriority(Template template, IEnumerable<User> user)
        {
            if (Convert.ToInt16(template.NotificationPriority) == (int) MeansOfCommunication.Email &&
               user.All(x => !String.IsNullOrEmpty(x.Email))) return MeansOfCommunication.Email;
            if (Convert.ToInt16(template.NotificationPriority) == (int) MeansOfCommunication.SMS &&
                user.All(x => !String.IsNullOrEmpty(x.SMS))) return MeansOfCommunication.SMS;

            throw new ArgumentException("Not specific priority is set for this notification");
        }

        private string GetParsedNotificationText(Sendout sendout, Template template)
        {
            var parameterSet = sendout.Parameters.Split(",").Select(x => x.Trim()).ToList();
            var dictionaryParam = new Dictionary<string, string>();
            foreach (var line in parameterSet)
            {
                dictionaryParam.Add(line.Split("=")[0], line.Split("=")[1]);
            }
            var squareBracketsFound = true;
            while (squareBracketsFound)
            {
                var openBracketIndex = template.NotificationText.IndexOf('[');
                var closeBracketIndex = template.NotificationText.IndexOf(']');
                if (openBracketIndex == -1 || closeBracketIndex == -1)
                {
                    squareBracketsFound = false;
                    continue;
                }
                var wordWithBrackets = template.NotificationText.Substring(openBracketIndex, closeBracketIndex - openBracketIndex + 1);
                template.NotificationText = dictionaryParam.ContainsKey(wordWithBrackets.Substring(1, wordWithBrackets.Length - 1)) ?
                     template.NotificationText.
                            Replace(wordWithBrackets,
                            dictionaryParam[wordWithBrackets.Substring(1, wordWithBrackets.Length - 1)]) :
                     template.NotificationText.
                            Replace(wordWithBrackets, $"{wordWithBrackets.Substring(1, wordWithBrackets.Length - 2)}");

            }
            return template.NotificationText;
        }       
    }
}
