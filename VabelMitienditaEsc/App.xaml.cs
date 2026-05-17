using Microsoft.Extensions.Configuration;
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

            // 1. Construir la configuración leyendo los User Secrets
            IConfiguration configuration = new ConfigurationBuilder()
                .AddUserSecrets<App>()
                .Build();

            // 2. Extraer la cadena de conexión
            string connectionString = configuration.GetConnectionString("DefaultConnection");

            // 3. Inicializar el servicio real con la base de datos
            DatabaseAuthenticationService authService = new DatabaseAuthenticationService(connectionString);
            NavigationStore navigationStore = new NavigationStore();

            MainViewModel mainViewModel = new MainViewModel(navigationStore, authService);

            MainWindow window = new MainWindow
            {
                DataContext = mainViewModel
            };
            window.Show();
        }
    }

}
