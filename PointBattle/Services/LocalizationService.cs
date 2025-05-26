using System.Globalization;
using System.Resources;
using Microsoft.JSInterop;
using PointBattle.Resources;

namespace PointBattle.Services
{
    public class LocalizationService
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly ResourceManager _resourceManager;
        private CultureInfo _currentCulture;
        
        public event Action OnLanguageChanged;
        
        public CultureInfo CurrentCulture => _currentCulture;
        public bool IsRightToLeft => _currentCulture.Name == "ckb";
        public string CurrentLanguage => _currentCulture.Name;

        public LocalizationService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
            
            // Initialize ResourceManager for the resource file
            _resourceManager = new ResourceManager(
                typeof(SharedResource).FullName, 
                typeof(SharedResource).Assembly);
            
            // Get saved language preference or use default
            var savedLanguage = Preferences.Get("language", "en");
            _currentCulture = new CultureInfo(savedLanguage);
            
            // Apply the language
            ApplyLanguage(savedLanguage);
            
            Console.WriteLine($"LocalizationService initialized with language: {savedLanguage}, RTL: {IsRightToLeft}");
        }
        
        public string GetString(string key)
        {
            try
            {
                // Get localized string from resource manager
                var localizedString = _resourceManager.GetString(key, _currentCulture);
                
                // Return the key itself if not found (helps with debugging)
                return localizedString ?? $"[{key}]";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting localized string for key: {key}, Error: {ex.Message}");
                return $"[{key}]";
            }
        }
        
        public async Task SetLanguageAsync(string languageCode)
        {
            if (_currentCulture.Name == languageCode)
                return;
                
            try
            {
                // Update the culture info
                _currentCulture = new CultureInfo(languageCode);
                
                // Save language preference
                Preferences.Set("language", languageCode);
                
                // Apply the language change
                ApplyLanguage(languageCode);
                
                // Set a localStorage value to indicate a language change was made
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "app_language_changed", "true");
                
                // Update document direction
                await _jsRuntime.InvokeVoidAsync("setDirection", IsRightToLeft ? "rtl" : "ltr");
                
                Console.WriteLine($"Language changed to: {languageCode}, RTL: {IsRightToLeft}");
                
                // Notify listeners that language has changed
                OnLanguageChanged?.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting language: {ex.Message}");
                throw;
            }
        }

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
                
                // Also update thread cultures
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
                        Thread.CurrentThread.CurrentCulture = fallbackCulture;
                        Thread.CurrentThread.CurrentUICulture = fallbackCulture;
                        
                        Console.WriteLine("Fallback to English applied");
                    }
                    catch
                    {
                        // Last resort - ignore if even fallback fails
                    }
                }
            }
        }
    }
}
