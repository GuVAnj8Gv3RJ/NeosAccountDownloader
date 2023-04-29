using AccountDownloader.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;

namespace AccountDownloader.ViewModels
{
    public class AboutWindowViewModel: ReactiveObject
    {
        [Reactive]
        public string AppVersion { get; set; }

        [Reactive]
        public string AppCompany { get; set; }

        private readonly IAssemblyInfoService _assemblyInfoService;

        public AboutWindowViewModel()
        {
            _assemblyInfoService = Locator.Current.GetService<IAssemblyInfoService>() ?? throw new NullReferenceException("No version info");
            AppVersion = _assemblyInfoService.Version;
            AppCompany = _assemblyInfoService.CompanyName;
        }
    }
}
