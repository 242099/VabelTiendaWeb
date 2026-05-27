using Microsoft.Extensions.Configuration;
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
            LibretaVentasService libretaventasService = new LibretaVentasService(connectionString);
            InventarioService inventarioService = new InventarioService(connectionString);
            VentasService ventasService = new VentasService(connectionString);
            TiendaService tiendaService = new TiendaService(connectionString);

            MainViewModel mainViewModel = new MainViewModel(navigationStore, authService, libretaventasService, inventarioService, ventasService, tiendaService);

            // 4. Inicializar y conectar el servicio de Arduino
            // Recuerda modificar "COM3" por el puerto asignado a tu Arduino en el Administrador de dispositivos.
            mainViewModel.DispositivoArduino = new ArduinoService("COM5");
            mainViewModel.DispositivoArduino.Connect();

            MainWindow window = new MainWindow
            {
                DataContext = mainViewModel
            };
            window.Show();
        }
    }
}