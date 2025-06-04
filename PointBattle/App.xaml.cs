using System.Globalization;

namespace PointBattle
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Get saved language preference or use default
            var savedLanguage = Preferences.Get("language", "en");
            
            // Set culture based on saved preference
            var culture = new CultureInfo(savedLanguage);
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            
            Console.WriteLine($"App startup - Language set to: {savedLanguage}");
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new MainPage()) { Title = "PointBattle" };
        }
    }
}
