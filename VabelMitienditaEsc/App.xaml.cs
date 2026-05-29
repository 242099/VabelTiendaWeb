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

            // Construir la configuración leyendo los User Secrets
            IConfiguration configuration = new ConfigurationBuilder()
                .AddUserSecrets<App>()
                .Build();

            // Extraer la cadena de conexión
            string connectionString = configuration.GetConnectionString("DefaultConnection");

            // Inicializar el servicio real con la base de datos
            DatabaseAuthenticationService authService = new DatabaseAuthenticationService(connectionString);
            NavigationStore navigationStore = new NavigationStore();
            LibretaVentasService libretaventasService = new LibretaVentasService(connectionString);
            InventarioService inventarioService = new InventarioService(connectionString);
            VentasService ventasService = new VentasService(connectionString);
            TiendaService tiendaService = new TiendaService(connectionString);

            // --- SERVICIOS PARA LA VISTA DE GASTOS ---
            GastosOperativosService gastosService = new GastosOperativosService(connectionString);
            ProveedorService proveedorService = new ProveedorService(connectionString);
            FormasPagoService formasPagoService = new FormasPagoService(connectionString);
            CatalogoCuentasService catalogoCuentasService = new CatalogoCuentasService(connectionString);

            // Pasamos todos los servicios al constructor del MainViewModel
            MainViewModel mainViewModel = new MainViewModel(
                navigationStore,
                authService,
                libretaventasService,
                inventarioService,
                ventasService,
                tiendaService,
                gastosService,
                proveedorService,
                formasPagoService,
                catalogoCuentasService);

            // Inicializar y conectar el servicio de Arduino
            mainViewModel.DispositivoArduino = new ArduinoService("COM4");
            mainViewModel.DispositivoArduino.Connect();

            MainWindow window = new MainWindow
            {
                DataContext = mainViewModel
            };
            window.Show();
        }
    }
}
