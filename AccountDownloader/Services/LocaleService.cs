
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

        public event Action<CultureInfo> LocaleChanged;
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

    //TODO: Can we also measure %-age completion?
    public class LocaleService : ILocaleService
    {
        private ILogger Logger { get; }

        public static readonly HashSet<string> MachineTranslatedCodes = new() { "ko" };

        public List<LocaleModel> AvailableLocales { get; }

        public event Action<CultureInfo> LocaleChanged;

        public LocaleModel CurrentLocale => new(Thread.CurrentThread.CurrentUICulture.DisplayName, Thread.CurrentThread.CurrentUICulture.Name);

        public void SetLanguage(string code)
        {
            if (string.IsNullOrEmpty(code))
                throw new ArgumentNullException(nameof(code));

            if (!AvailableLocales.Any(lm => lm.Code.Equals(code)))
                throw new InvalidOperationException("Unavailable Locale");

            Logger.LogInformation("Setting App Language to: {code}", code);

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(code);

            LocaleChanged?.Invoke(Thread.CurrentThread.CurrentUICulture);
        }

        public void SetLanguage(LocaleModel model)
        {
            SetLanguage(model.Code);
        }

        public LocaleService(ILogger? logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));

            AvailableLocales = AvailableCultures.Select(CreateLocaleModel).ToList();

            Logger.LogInformation("Language set to: {LanguageName}", CurrentLocale.Name);
        }

        private LocaleModel CreateLocaleModel(CultureInfo ci)
        {
            var cultureName = ci.NativeName;
            if (MachineTranslatedCodes.Contains(ci.Name))
                cultureName += " (Machine Translated)";

            return new LocaleModel(cultureName, ci.Name);
        }

        //TODO: this is a bit intensive, I was looking into CodeGenerators but those hate me.
        // Cached result.
        private IEnumerable<CultureInfo>? _avaliableLocales;

        public IEnumerable<CultureInfo> AvailableCultures => _avaliableLocales != null ? _avaliableLocales : GenerateAvailableLocales();


        // Loosly based on: https://stackoverflow.com/a/57683929/2095344
        private IEnumerable<CultureInfo> GenerateAvailableLocales()
        {
            if (_avaliableLocales != null)
                return _avaliableLocales;

            //Loop through all cultures
            _avaliableLocales = CultureInfo.GetCultures(CultureTypes.AllCultures)
            .Where(c =>
            {
                // We always support english(saves a lookup)
                if (c.Name == "en")
                    return true;
                // If its invariant just give up
                if (c.Equals(CultureInfo.InvariantCulture))
                    return false;

                // Get the set with this culture(Will add resource set to memory)
                var set = Res.ResourceManager.GetResourceSet(c, true, false);

                // No set, not available
                if (set == null)
                    return false;

                // If the set, is not for the currently selected language, close it to save memory.
                // Causes language selection to fail
                // TODO
                //if (!c.Equals(Thread.CurrentThread.CurrentUICulture))
                //    set.Close();

                return true;
            });

            return _avaliableLocales;
        }
    }
}
