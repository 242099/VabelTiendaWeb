using Microsoft.Extensions.Configuration;
using System.Windows;
using VabelMitienditaEsc.Core;
using VabelMitienditaEsc.Services;
using VabelMitienditaEsc.ViewModels;

namespace VabelMitienditaEsc
{
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

            // 3. Inicializar los servicios
            DatabaseAuthenticationService authService = new DatabaseAuthenticationService(connectionString);
            NavigationStore navigationStore = new NavigationStore();
            LibretaVentasService ventasService = new LibretaVentasService(connectionString);
            GastosOperativosService gastosService = new GastosOperativosService(connectionString);
            ProveedorService proveedorService = new ProveedorService(connectionString);
            FormasPagoService formasPagoService = new FormasPagoService(connectionString);
            CatalogoCuentasService catalogoCuentasService = new CatalogoCuentasService(connectionString);

            MainViewModel mainViewModel = new MainViewModel(navigationStore, authService, ventasService, gastosService, proveedorService, formasPagoService, catalogoCuentasService);

            // 4. Inicializar y conectar el servicio de Arduino
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