using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SlackNet;
using SlackNet.Interaction;
using SlackNet.WebApi;

namespace PiotrekApp
{
    public class CptBombMessageHandler : ISlashCommandHandler
    {
        public const string Commnad = "/bomba";

        //private readonly List<string> quotes;
        private readonly QuotesConfiguration quotesConfiguration;

        public CptBombMessageHandler(IConfiguration configuration)
        {
            quotesConfiguration = configuration.Get<QuotesConfiguration>();
            //quotes = configuration
            //    .GetSection("quotes")
            //    .GetChildren()
            //    .Select(c => c.Value)
            //    .ToList();
        }

        public Task<SlashCommandResponse> Handle(SlashCommand command)
        {
            Console.WriteLine($"{command.UserName} has requested a quote from Cpt Bomb");

            var r = new Random();
            var quoteIndex = r.Next(quotesConfiguration.Quotes.Count);

            return Task.FromResult(new SlashCommandResponse
            {
                Message = new Message
                {
                    Text = quotesConfiguration.Quotes[quoteIndex],
                    ReplyBroadcast = true,
                },
                ResponseType = ResponseType.InChannel
            });
        }
    }

    record QuotesConfiguration
    {
        public List<string> Quotes { get; set; }
    }
}
