using System.Diagnostics;

namespace PointBattle
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            AppDomain.CurrentDomain.UnhandledException += (sender, error) =>
            {
                Debug.WriteLine($"Unhandled exception: {error.ExceptionObject}");
            };
        }
    }
}
