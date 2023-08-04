using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SlackNet;
using SlackNet.Events;
using SlackNet.WebApi;

namespace PiotrekApp
{
    internal class DirectMessageHandler : IEventHandler<AppMention>
    {
        private readonly OpenAIClient openAIClient;
        private readonly ISlackApiClient slackApiClient;
        private readonly Regex userRegex = new Regex(@"<@(\w+)>", RegexOptions.IgnoreCase);
        private readonly AppSettings slackSettings;

        public DirectMessageHandler(OpenAIClient openAIClient, ISlackApiClient slackApiClient, IConfiguration configuration)
        {
            this.openAIClient = openAIClient;
            this.slackApiClient = slackApiClient;

            slackSettings = configuration.Get<AppSettings>();
        }

        public async Task Handle(AppMention slackEvent)
        {
            if (slackEvent.Type.ToLower().Equals("app_mention"))
            {
                var question = slackEvent.Text;

                foreach (Match userMatch in userRegex.Matches(question))
                {
                    if (userMatch.Groups.Count > 0)
                    {
                        var userId = userMatch.Groups[1].Value;
                        var user = await slackApiClient.Users.Info(userId);
                        var name = user.Name;

                        //@TODO better replace method, this is shitty :D
                        question = question.Replace(userMatch.Groups[0].Value, name == "piotrekbot" ? string.Empty : name);
                    }
                }

                var response = await openAIClient.Ask(question);

                Console.WriteLine($"{response ?? string.Empty}");

                await slackApiClient.Chat.PostMessage(new Message
                {
                    Channel = slackEvent.Channel,
                    Text = response
                });
            }
        }
    }

    record AppSettings
    {
        public string ApiToken { get; set; } = string.Empty;
        public string AppLevelToken { get; set; } = string.Empty;
    }
}
