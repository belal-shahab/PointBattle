using System.Globalization;

namespace PointBattle
{
    public partial class App : Application
    {
        /// <summary>
        /// Initializes the application and sets the culture based on the saved language preference.
        /// </summary>
        /// <remarks>
        /// Retrieves the user's preferred language from persistent storage (defaulting to English if not set) and configures the application's culture and UI culture accordingly.
        /// </remarks>
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

        /// <summary>
        /// Creates and returns the main application window with the title "PointBattle".
        /// </summary>
        /// <param name="activationState">Optional activation state for window initialization.</param>
        /// <returns>A new Window instance containing the main page.</returns>
        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new MainPage()) { Title = "PointBattle" };
        }
    }
}
