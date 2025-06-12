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
        private CultureInfo _currentCulture = new CultureInfo("en");
        private bool _isWebViewReady = false;
        
        public event Action OnLanguageChanged;
        
        public CultureInfo CurrentCulture => _currentCulture;
        public bool IsRightToLeft => _currentCulture.Name.StartsWith("ckb", StringComparison.OrdinalIgnoreCase) || 
                                     _currentCulture.Name.StartsWith("ar", StringComparison.OrdinalIgnoreCase);
        public string CurrentLanguage => _currentCulture.Name;

        // Available languages
        public List<LanguageOption> SupportedLanguages => new List<LanguageOption>
        {
            new LanguageOption { Code = "en", Name = "English", NativeName = "English", Flag = "🇺🇸" },
            new LanguageOption { Code = "ckb-iq", Name = "Kurdish", NativeName = "کوردی", Flag = "image:images/kurdistan_flag.png" },
            new LanguageOption { Code = "ar", Name = "Arabic", NativeName = "العربية", Flag = "🇸🇦" }
        };

        public LocalizationService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
            
            _resourceManager = new ResourceManager(
                typeof(SharedResource).FullName, 
                typeof(SharedResource).Assembly);
            
            var savedLanguage = Preferences.Get("app_language", "en");
            _currentCulture = new CultureInfo(savedLanguage);
            
            ApplyLanguage(savedLanguage);
            
            Console.WriteLine($"LocalizationService initialized with language: {savedLanguage}, RTL: {IsRightToLeft}");
        }

        public async Task InitializeAsync()
        {
            try
            {
                _isWebViewReady = true;
                await _jsRuntime.InvokeVoidAsync("setDirection", IsRightToLeft ? "rtl" : "ltr");
                Console.WriteLine($"WebView initialized with language: {_currentCulture.Name}, RTL: {IsRightToLeft}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing WebView: {ex.Message}");
            }
        }
        
        public string GetString(string key)
        {
            try
            {
                var localizedString = _resourceManager.GetString(key, _currentCulture);
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
            if (_currentCulture.Name.Equals(languageCode, StringComparison.OrdinalIgnoreCase))
                return;
                
            try
            {
                Console.WriteLine($"=== SETTING LANGUAGE TO: {languageCode} ===");
                
                _currentCulture = new CultureInfo(languageCode);
                Preferences.Set("app_language", languageCode);
                ApplyLanguage(languageCode);
                
                Console.WriteLine($"Language changed to: {languageCode}, RTL: {IsRightToLeft}");
                
                if (_isWebViewReady)
                {
                    try
                    {
                        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "app_language", languageCode);
                        await _jsRuntime.InvokeVoidAsync("setDirection", IsRightToLeft ? "rtl" : "ltr");
                        Console.WriteLine("JavaScript calls completed successfully");
                    }
                    catch (Exception jsEx)
                    {
                        Console.WriteLine($"JavaScript call failed (but continuing): {jsEx.Message}");
                    }
                }
                
                OnLanguageChanged?.Invoke();
                Console.WriteLine("OnLanguageChanged event fired");
                
                Console.WriteLine($"=== LANGUAGE CHANGE COMPLETED ===");
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
                var culture = new CultureInfo(languageCode);
                CultureInfo.CurrentCulture = culture;
                CultureInfo.CurrentUICulture = culture;
                CultureInfo.DefaultThreadCurrentCulture = culture;
                CultureInfo.DefaultThreadCurrentUICulture = culture;
                
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
                
                Console.WriteLine($"Applied language {languageCode} successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error applying language {languageCode}: {ex.Message}");
                if (!languageCode.Equals("en", StringComparison.OrdinalIgnoreCase))
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

    public class LanguageOption
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string NativeName { get; set; }
        public string Flag { get; set; }
    }
}