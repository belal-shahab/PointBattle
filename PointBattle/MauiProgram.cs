using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using PointBattle.Services;
using System.Globalization;
using Microsoft.Extensions.Localization;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

namespace PointBattle;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Add Blazor WebView
        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        // Add MudBlazor
        builder.Services.AddMudServices();
        
        // Register services
        builder.Services.AddSingleton<DatabaseService>();
        builder.Services.AddSingleton<LocalizationService>();
        builder.Services.AddSingleton<LanguageService>();
        builder.Services.AddSingleton<AppSessionService>(); // NEW: Add session service
        
        // Configure localization
        builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
        
        // Get initial language
        var savedLanguage = Preferences.Get("app_language", "en");
        
        // Configure supported cultures
        var supportedCultures = new[] { 
            new CultureInfo("en"),
            new CultureInfo("ckb-iq"),
            new CultureInfo("ar")
        };
        
        // Set default cultures
        CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(savedLanguage);
        CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(savedLanguage);

        var app = builder.Build();
        
        // Initialize App Center
        AppCenter.Start("android=e61a914d-b8cb-4610-b55f-3778d9aca4cc", 
            typeof(Analytics), typeof(Crashes));
        
        return app;
    }
}