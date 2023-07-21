using AccountDownloader.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Collections.Generic;

namespace AccountDownloader.ViewModels
{
    public class AboutWindowViewModel: ReactiveObject
    {
        [Reactive]
        public string AppVersion { get; set; }

        [Reactive]
        public string AppCompany { get; set; }

        private ContributionsService ContributionsService { get; set; }

        public List<Contributor>? Contributors => ContributionsService.Contributors;

        private readonly IAssemblyInfoService _assemblyInfoService;

        public AboutWindowViewModel()
        {
            _assemblyInfoService = Locator.Current.GetService<IAssemblyInfoService>() ?? throw new NullReferenceException("No version info");
            ContributionsService =  Locator.Current.GetService<ContributionsService>() ?? throw new NullReferenceException("No contributor information available");
            AppVersion = _assemblyInfoService.Version;
            AppCompany = _assemblyInfoService.CompanyName;
        }
    }
}
