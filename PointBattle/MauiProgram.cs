﻿using Microsoft.Extensions.Logging;
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
        builder.Services.AddMauiBlazorWebView();
#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif
        builder.Services.AddMudServices();
        
        // Register the database service as a singleton
        builder.Services.AddSingleton<DatabaseService>();
        
        // Configure localization with strongly typed resources
        builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
        
        // IMPORTANT: Register both services during transition!
        // This ensures backward compatibility while we migrate components
        builder.Services.AddSingleton<LocalizationService>();
        builder.Services.AddSingleton<LanguageService>();
        // Get initial language
        var savedLanguage = Preferences.Get("app_language", "en");
        
        // Configure supported cultures and set default
        var supportedCultures = new[] { 
            new CultureInfo("en"),
            new CultureInfo("ckb-iq"),
            new CultureInfo("ar") // Added Arabic support
        };
        
        // Set default cultures for the application
        CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(savedLanguage);
        CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(savedLanguage);

        // App Center configuration
        var app = builder.Build();
        
        // Initialize App Center with your secret
        AppCenter.Start("android=e61a914d-b8cb-4610-b55f-3778d9aca4cc", 
            typeof(Analytics), typeof(Crashes));
        
        return app;
    }
}