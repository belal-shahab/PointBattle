using System.Globalization;
using Microsoft.Extensions.Localization;
using PointBattle.Resources;

namespace PointBattle.Services
{
    public class LanguageService
    {
        public event Action LanguageChanged;
        public string CurrentLanguage { get; private set; } = "en";
        
        public bool IsRightToLeft => CurrentLanguage == "ckb";
        
        /// <summary>
        /// Initializes the LanguageService by loading the saved language preference, applying the corresponding culture settings, and logging the initialization details.
        /// </summary>
        public LanguageService()
        {
            // Get the saved language preference or use default
            var savedLanguage = Preferences.Get("language", "en");
            CurrentLanguage = savedLanguage;
            ApplyLanguage(savedLanguage);
            
            Console.WriteLine($"Language service initialized with language: {savedLanguage}, RTL: {IsRightToLeft}");
        }
        
        /// <summary>
        /// Changes the application's language to the specified language code, updates culture settings, persists the preference, and notifies subscribers.
        /// </summary>
        /// <param name="languageCode">The language code to switch to (e.g., "en", "ckb").</param>
        public void ChangeLanguage(string languageCode)
        {
            if (CurrentLanguage == languageCode)
                return;
                
            CurrentLanguage = languageCode;
            
            // Save the language preference
            Preferences.Set("language", languageCode);
            
            // Apply the language change
            ApplyLanguage(languageCode);
            
            Console.WriteLine($"Language changed to: {languageCode}, RTL: {IsRightToLeft}");
            
            // Notify listeners that language has changed
            LanguageChanged?.Invoke();
        }

        /// <summary>
        /// Sets the application's culture and UI culture to the specified language code, updating both current and default thread cultures.
        /// Falls back to English if the specified culture cannot be applied.
        /// </summary>
        /// <param name="languageCode">The language code to apply (e.g., "en", "ckb").</param>
        private void ApplyLanguage(string languageCode)
        {
            try
            {
                // Update the current culture
                var culture = new CultureInfo(languageCode);
                CultureInfo.CurrentCulture = culture;
                CultureInfo.CurrentUICulture = culture;
                CultureInfo.DefaultThreadCurrentCulture = culture;
                CultureInfo.DefaultThreadCurrentUICulture = culture;
                
                // Also update thread cultures to ensure all parts of the app respect the change
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
                
                Console.WriteLine($"Applied language {languageCode} successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error applying language {languageCode}: {ex.Message}");
                // Fallback to English if there was an error
                if (languageCode != "en")
                {
                    try
                    {
                        var fallbackCulture = new CultureInfo("en");
                        CultureInfo.CurrentCulture = fallbackCulture;
                        CultureInfo.CurrentUICulture = fallbackCulture;
                        CultureInfo.DefaultThreadCurrentCulture = fallbackCulture;
                        CultureInfo.DefaultThreadCurrentUICulture = fallbackCulture;
                        Thread.CurrentThread.CurrentCulture = fallbackCulture;
                        Thread.CurrentThread.CurrentUICulture = fallbackCulture;
                        
                        Console.WriteLine("Fallback to English applied");
                    }
                    catch
                    {
                        // Last resort - ignore if even fallback fails
                        Console.WriteLine("Fallback to English failed");
                    }
                }
            }
        }
    }
}