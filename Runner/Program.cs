using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tolltech.Core;
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
    var serviceProvider = services.BuildServiceProvider();

    var whoPrometheusListener = serviceProvider.GetRequiredService<IWhoPrometheusListener>();
    await whoPrometheusListener.StartListeningAsync(9700);
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

