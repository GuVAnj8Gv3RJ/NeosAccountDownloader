using Avalonia;
using System;
using Avalonia.ReactiveUI;
using Splat;

using Microsoft.Extensions.Logging;
using AccountDownloader.Services;

namespace AccountDownloader
{
    class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args) {
            Boostrapper.Register(Locator.CurrentMutable, Locator.Current);
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        } 

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .UseReactiveUI()
                .AfterSetup(AfterStartup);

        private static void AfterStartup(AppBuilder obj)
        {
            var logger = Locator.Current.GetService<ILogger>();
            var config = Locator.Current.GetService<Config>();
            var info = Locator.Current.GetService<IAssemblyInfoService>();
            if (logger == null)
                return;

            logger.LogInformation($"Starting up {info!.Name} v{info!.Version}");
            logger.LogInformation($"Log Level: ${config!.LogLevel}");

        }
    }
}
