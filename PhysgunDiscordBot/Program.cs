using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using PhysgunDiscordBot;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services
    .AddDiscordGateway(options => options.Intents = GatewayIntents.GuildMessages)
    .AddApplicationCommands(options => options.UseScopes = true);
builder.Services.AddSingleton<Cfg>();
builder.Services.AddSingleton<OnlineLocator>();
builder.Services.AddSingleton<WaitingPlayersCountEventSender>();

var app = builder.Build();

app.AddSlashCommand("online", "Показывает онлайн на сервере", (OnlineLocator ol, ApplicationCommandContext cntxt) =>
{
    ol.UpdateOnlineCount();
    int online = ol.PlayerCount;
    InteractionMessageProperties msg = new()
    {
        Flags = MessageFlags.Ephemeral
    };
    if (ol.IsServerAvailable) msg.Content = $"Сейчас на сервере {online} человек{(online < 10 ||
                                                        online > 20 &&
                                                        online % 10 > 1 &&
                                                        online % 10 < 5 ? 'а' : string.Empty)}";
    else msg.Content = "Сервер, вероятно, не работает :(";
    return msg;
});
app.AddSlashCommand(
    name: "subscribe",
    description: "Подписывает вас на количество онлайна",
    handler: async (WaitingPlayersCountEventSender eventSender, ApplicationCommandContext cntxt, byte count) =>
    {
        //if (count > 15) return "Больше 10 нельзя)";
        var channel = await cntxt.User.GetDMChannelAsync();
        eventSender.SetWaiting(count, channel.Id);
        InteractionMessageProperties a = new()
        {
            Content = $"Вы удачно подписались!",
            Flags = MessageFlags.Ephemeral,
        };
        return a;
    });

app.UseGatewayHandlers();

app.MapGet("/ping", () => "pong");

app.Run();
