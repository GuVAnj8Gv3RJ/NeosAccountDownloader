using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace AccountDownloader.Services
{
    public interface ILocaleService
    {
        public void SetLanguage(string code);
        public void SetLanguage(LocaleModel model);
        public LocaleModel CurrentLocale { get; }
        public List<LocaleModel> AvailableLocales { get; }
    }
    // For UIs
    public class LocaleModel
    {
        public string Name { get; }
        public string Code { get; }

        public LocaleModel(string name, string code)
        {
            Name = name;
            Code = code;
        }
    }

    public class LocaleService: ILocaleService
    {
        private ILogger Logger { get; }

        //TODO: is there a way to auto-generate this?
        //TODO: We can also measure %-age completion if we can auto-generate.
        public static readonly HashSet<string> AvailableLocaleCodes = new()
        {
            "en",
            "de",
            "ja",
            "ko",
            "fr",
            "es",
            "ru"
        };
        public static readonly HashSet<string> MachineTranslatedCodes = new() { "ja", "ko" };

        public List<LocaleModel> AvailableLocales { get; }

        public LocaleModel CurrentLocale => new(Thread.CurrentThread.CurrentUICulture.DisplayName, Thread.CurrentThread.CurrentUICulture.Name);

        public void SetLanguage(string code)
        {
            if (string.IsNullOrEmpty(code))
                throw new ArgumentNullException(nameof(code));
            if (!AvailableLocaleCodes.Contains(code))
                throw new InvalidOperationException("Unavailable Locale");

            Logger.LogInformation("Setting App Language to: {code}", code);

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(code);
        }

        public void SetLanguage(LocaleModel model)
        {
            SetLanguage(model.Code);
        }

        public LocaleService(ILogger? logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // TODO: do we need other details here? Is it better to put the entire CultureInfo there?
            AvailableLocales = AvailableLocaleCodes.Select(CreateLocaleModel).ToList();

            Logger.LogInformation("Language set to: {LanguageName}", CurrentLocale.Name);
        }

        private LocaleModel CreateLocaleModel(string code)
        {
            var cultureName = CultureInfo.GetCultureInfo(code).NativeName;
            if (MachineTranslatedCodes.Contains(code))
                cultureName += " (Machine Translated)";

            return new LocaleModel(cultureName, code);
        }
    }
}
