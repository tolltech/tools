using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Tolltech.Core;
using Tolltech.Core.Helpers;
using Tolltech.WhoPrometheus;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Console;
using Vostok.Logging.File;
using Vostok.Logging.File.Configuration;

LogProvider.Configure(
    new CompositeLog(
        new ConsoleLog(),
        new FileLog(
            new FileLogSettings { RollingStrategy = new RollingStrategyOptions { MaxSize = 100 * 1024 * 1024 } })
    )
);

var log = LogProvider.Get();

try
{
    var services = new ServiceCollection();
    IoCResolver.Resolve((x, y) => services.AddSingleton(x, y), null, "Tolltech");

    string settingsStr;
    if (args.Length > 0) settingsStr = args[0];
    else if (File.Exists("args.txt")) settingsStr = File.ReadAllText("args.txt");
    else settingsStr = "[]";
    
    var settings = JsonConvert.DeserializeObject<AppSettings>(settingsStr)!;
    services.AddSingleton(settings);
    services.AddSingleton(new WhoPrometheusSettings(settings.Settings.SafeGet("WhoPrometheus")?.KeyValues));
    
    var serviceProvider = services.BuildServiceProvider();
    
    
    var whoPrometheusListener = serviceProvider.GetRequiredService<IWhoPrometheusListener>();
    await whoPrometheusListener.StartListeningAsync();
}
catch (Exception e)
{
    log.Fatal("Global exception was thrown");
    log.Error(e);
}
finally
{
    //note: time for logger flushing
    await Task.Delay(5000);
}

