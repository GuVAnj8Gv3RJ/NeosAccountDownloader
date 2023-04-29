using Avalonia;
using Splat;
using AccountDownloader.Services;
using AccountDownloader.ViewModels;
using CloudX.Shared;
using Serilog.Extensions.Logging;
using Serilog;
using System;
using System.IO;
using System.Diagnostics;
using Serilog.Events;

namespace AccountDownloader
{
    public class Config
    {
        public string LogFolder { get; }

        public LogEventLevel LogLevel { get; }
        
        public Config(IAssemblyInfoService? info) {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            LogFolder = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create),info.CompanyName, info.Name);

            LogLevel = LogEventLevel.Information;
#if DEBUG
            LogLevel = LogEventLevel.Debug;
#endif

        }
    }

    public class Boostrapper
    {
        public static void Register(IMutableDependencyResolver services, IReadonlyDependencyResolver resolve)
        {
            services.RegisterConstant<IAssemblyInfoService>(new AssemblyInfoService());
            services.RegisterLazySingleton(() => new Config(resolve.GetService<IAssemblyInfoService>()));
            services.RegisterLazySingleton<ILogger>(() =>
            {
                var machine = Environment.MachineName;

                var info = resolve.GetService<IAssemblyInfoService>();
                var config = resolve.GetService<Config>();
                var folder = config!.LogFolder;
                var level = config!.LogLevel;

                var logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.File(folder + $"/{machine}-{info!.Version}-.log", rollingInterval: RollingInterval.Day, restrictedToMinimumLevel:level )
                    .CreateLogger();

                var listener = new SerilogTraceListener.SerilogTraceListener(logger);
                Trace.Listeners.Add(listener);

                var factory = new SerilogLoggerFactory(logger);

                return factory.CreateLogger("Default");
            });

            services.RegisterConstant<ILocaleService>(new LocaleService(resolve.GetService<ILogger>()));

            var version = resolve.GetService<IAssemblyInfoService>();
            // Registering this as non-lazy because it is quite slow to init.
            services.RegisterConstant(new CloudXInterface(null, version?.NameNoSpaces, version?.Version));

            services.RegisterLazySingleton<IAppCloudService>(() => new AppCloudService(resolve.GetService<CloudXInterface>(), resolve.GetService<ILogger>()));

            services.RegisterLazySingleton(() => new MainWindowViewModel());
            services.Register<IAccountDownloader>(() => new AccountDownloadManager(resolve.GetService<CloudXInterface>(), resolve.GetService<ILogger>()));
            services.Register<IStorageService>(() => new CloudStorageService(resolve.GetService<CloudXInterface>(), resolve.GetService<ILogger>()));
            services.Register<IGroupsService>(() => new GroupsService(resolve.GetService<CloudXInterface>(), resolve.GetService<IStorageService>(), resolve.GetService<ILogger>()));
        }
    }
}
