using System.Configuration;
using System.Data;
using System.Windows;
using VabelMitienditaEsc.Core;
using VabelMitienditaEsc.Services;
using VabelMitienditaEsc.ViewModels;

namespace VabelMitienditaEsc
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Inicialización de dependencias centralizadas
            NavigationStore navigationStore = new NavigationStore();
            MockAuthenticationService authService = new MockAuthenticationService();

            MainViewModel mainViewModel = new MainViewModel(navigationStore, authService);

            MainWindow window = new MainWindow
            {
                DataContext = mainViewModel
            };
            window.Show();
        }
    }

}
