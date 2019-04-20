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
                    var user = users.FirstOrDefault(x => sendout.Username == sendout.Username);
                    var userGroup = userGroups
                        .FirstOrDefault(x => x.GroupName == sendout.UserGroup)
                        .UserIds
                        .Split(",")
                        .Select(x => Convert.ToInt32(x.Trim())).ToList()
                        .Select(x => users.FirstOrDefault(y => y.Id == x));
                    if (user != null) userList.Add(user);
                    if (userGroup != null) userList.AddRange(userGroup);
                    var meanOfCommunication = ExtractPriority(sendout, template, userList);
                    var listOfContatDetails = meanOfCommunication == MeansOfCommunication.Email ?
                        userList.Select(x => x.Email) : userList.Select(x => x.Facebook);

                    sendoutReadyList.AddRange(listOfContatDetails.Select(x => new GatewaySendout()
                    {
                        Text = GetParsedNotificationText(sendout, template),
                        ContactDetails = x,
                        MeansOfCommunication = meanOfCommunication
                    }));
                }
                catch (Exception ex)
                {
                    Log.Error($"Sendout list can not be analyzed, please check checkout records. Error thrown: {ex}");
                }
            }
            return sendoutReadyList;
        }

        private MeansOfCommunication ExtractPriority(Sendout sendout, Template template, IEnumerable<User> user)
        {
            var parameterSets = sendout.Parameters.Split(",").Select(x => x.Trim()).ToList();
            if (parameterSets[0] == MeansOfCommunication.Email.ToString() &&
               user.All(x => !String.IsNullOrEmpty(x.Email))) return MeansOfCommunication.Email;

            if (parameterSets[0] == MeansOfCommunication.Facebook.ToString() &&
                user.All(x => !String.IsNullOrEmpty(x.Email))) return MeansOfCommunication.Facebook;

            throw new ArgumentException("Not specific priority is set for this notification");
        }

        private string GetParsedNotificationText(Sendout sendout, Template template)
        {
            var parameterSets = sendout.Parameters.Split(",").Select(x => x.Trim()).ToList();
            var dictionaryParam = new Dictionary<string, string>();
            foreach (var line in parameterSets)
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
                if (dictionaryParam.ContainsKey(wordWithBrackets.Substring(1, wordWithBrackets.Length - 1)))
                    template.NotificationText = template.NotificationText.
                            Replace(wordWithBrackets,
                            dictionaryParam[wordWithBrackets.Substring(1, wordWithBrackets.Length - 1)]);
            }
            return template.NotificationText;
        }       
    }
}
