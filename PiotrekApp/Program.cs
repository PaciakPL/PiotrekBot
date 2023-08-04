// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PiotrekApp;
using SlackNet;
using SlackNet.Events;
using SlackNet.Extensions.DependencyInjection;


var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddUserSecrets<Program>()
    .AddJsonFile("appsettings.json", reloadOnChange: true, optional: false)
    .AddJsonFile("appsettings.dev.json", reloadOnChange: true, optional: true)
    .Build();

var slackConfig = config.Get<AppSettings>();

Console.WriteLine($"Starting slack app");

var serviceCollection = new ServiceCollection();
serviceCollection.AddSlackNet(c =>
{
    c.UseAppLevelToken(slackConfig.AppLevelToken);
    c.UseApiToken(slackConfig.ApiToken);
    c.RegisterSlashCommandHandler<CptBombMessageHandler>(CptBombMessageHandler.Commnad);
    c.RegisterEventHandler<AppMention, DirectMessageHandler>();
});
serviceCollection.AddScoped<IConfiguration>(_ => config);
serviceCollection.AddScoped<OpenAIClient>();

var services = serviceCollection.BuildServiceProvider();

var client = services.SlackServices().GetSocketModeClient();

await client.Connect();

Console.WriteLine($"Connection result {client.Connected}");

await Task.Run(Console.ReadKey);

client.Disconnect();

public record AppSettings
{
    public string ApiToken { get; set; } = string.Empty;
    public string AppLevelToken { get; set; } = string.Empty;
}
