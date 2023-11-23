using DiscordBot;
using DiscordBot.Common;
using DiscordBot.Extensions;
using DiscordBot.Services.ArtGallery.Source;
using Quartz;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("fatalLog.txt")
    .CreateBootstrapLogger();

try
{
    var builder = Host.CreateApplicationBuilder(args);

    builder.Services.AddSystemd();
    builder.Services.AddSerilog(config => config.ReadFrom.Configuration(builder.Configuration));

    builder.Services.AddHostedService<BotStartup>();
    builder.Services.AddAndValidateOptions<BotConfig>();
    builder.Services.AddAndValidateOptions<SaucenaoConfig>();
    builder.Services.AddAndValidateOptions<QuartzOptions>("Quartz");
    builder.Services.AddBotServices(builder.Configuration);

    var host = builder.Build();
    host.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
