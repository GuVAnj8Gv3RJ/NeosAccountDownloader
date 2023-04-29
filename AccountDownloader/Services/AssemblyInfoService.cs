using System;
using System.Reflection;

namespace AccountDownloader.Services
{
    public interface IAssemblyInfoService
    {
        string Version { get; }
        string CompanyName { get; }
        string Name { get; }

        string NameNoSpaces { get; }
    }
    public class AssemblyInfoService : IAssemblyInfoService
    {
        public AssemblyInfoService() {
            var info = Assembly.GetExecutingAssembly();
            var company = info.GetCustomAttribute<AssemblyCompanyAttribute>();

            // Dunno why MSBuild isn't adding the regular version but IDC
            var version = info.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            Version = version?.InformationalVersion ?? "Unknown";
            CompanyName = company?.Company ?? "Unknown";
            Name = info.GetName().Name ?? "Unknown";

        }
        public string NameNoSpaces => Name.Replace(" ", "");
        public string Version { get; }

        public string CompanyName { get; }

        public string Name { get; }
    }
}
