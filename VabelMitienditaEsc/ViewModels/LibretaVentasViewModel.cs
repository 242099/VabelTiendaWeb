using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using VabelMitienditaEsc.Core;
using VabelMitienditaEsc.Models;
using VabelMitienditaEsc.Services;

namespace VabelMitienditaEsc.ViewModels
{
    public partial class LibretaVentasViewModel : ViewModelBase
    {
        private readonly NavigationStore _navigationStore;
        private readonly MainViewModel _mainViewModel;
        private readonly LibretaVentasService _libretaService; // Instancia del nuevo servicio

        [ObservableProperty] private bool _esDueno;
        [ObservableProperty] private string _rolUsuarioActual;
        [ObservableProperty] private decimal _totalGanancias;
        [ObservableProperty] private decimal _totalGastos;
        [ObservableProperty] private ObservableCollection<TopProducto> _topProductosList;
        [ObservableProperty] private ObservableCollection<TransaccionHistorial> _historialFiltrado;

        // Títulos y Estados de control visual
        [ObservableProperty] private string _tituloHistorial;
        [ObservableProperty] private bool _mostrarTablaHistorial;
        [ObservableProperty] private bool _mostrarMetodosPago;
        [ObservableProperty] private bool _mostrarFormularioMetodoPago;
        [ObservableProperty] private string _nuevoMetodoPagoNombre;

        // Filtros de fecha
        [ObservableProperty] private DateTime? _fechaInicio;
        [ObservableProperty] private DateTime? _fechaFin;

        public LibretaVentasViewModel(NavigationStore navigationStore, MainViewModel mainViewModel, LibretaVentasService libretaService)
        {
            _navigationStore = navigationStore;
            _mainViewModel = mainViewModel;
            _libretaService = libretaService;

            TopProductosList = new ObservableCollection<TopProducto>();
            HistorialFiltrado = new ObservableCollection<TransaccionHistorial>();

            // Configuración inicial de fechas (Mes actual por defecto)
            FechaInicio = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            FechaFin = DateTime.Now;

            // ASIGNACIÓN COMPLETAMENTE DINÁMICA DESDE LA BASE DE DATOS
            // Obtenemos el nombre del rol real ("Dueño", "Empleado", etc.)
            string rolDb = _mainViewModel.CurrentUser?.NombreRol;

            // Fallback de seguridad extrema: si por alguna razón la sesión fuese nula, asigna "Empleado"
            RolUsuarioActual = !string.IsNullOrEmpty(rolDb) ? rolDb : "Empleado";

            // Evaluamos permisos basados de manera exacta en la cadena de texto real de la BD
            EsDueno = RolUsuarioActual.Equals("Dueño", StringComparison.OrdinalIgnoreCase);

            // Ejecuta la carga en segundo plano de manera segura
            _ = CargarResumenFinancieroAsync();
        }

        // Interceptores de CommunityToolkit. Se ejecutan automáticamente al cambiar las fechas en la UI
        partial void OnFechaInicioChanged(DateTime? value)
        {
            _ = CargarResumenFinancieroAsync();
        }

        partial void OnFechaFinChanged(DateTime? value)
        {
            _ = CargarResumenFinancieroAsync();
        }

        // CARGA ASÍNCRONA DE INDICADORES PRINCIPALES (KPIs Izquierda)
        public async Task CargarResumenFinancieroAsync()
        {
            try
            {
                // 1. Cargar Totales Dinámicos según los filtros de fecha seleccionados
                var (ganancias, gastos) = await _libretaService.GetTotalesFinancierosAsync(FechaInicio, FechaFin);
                TotalGanancias = ganancias;
                TotalGastos = gastos;

                // 2. Cargar el Top 3 de productos más vendidos globales
                var topProductos = await _libretaService.GetTopProductosAsync();
                TopProductosList.Clear();
                foreach (var prod in topProductos)
                {
                    TopProductosList.Add(prod);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cargar indicadores: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task VerHistorialVentas()
        {
            MostrarMetodosPago = false;
            MostrarFormularioMetodoPago = false;
            HistorialFiltrado.Clear();
            TituloHistorial = "Historial de Ventas";

            try
            {
                var ventasReal = await _libretaService.GetHistorialVentasAsync(FechaInicio, FechaFin);
                foreach (var venta in ventasReal)
                {
                    HistorialFiltrado.Add(venta);
                }
                MostrarTablaHistorial = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cargar ventas: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task VerHistorialGastos()
        {
            MostrarMetodosPago = false;
            MostrarFormularioMetodoPago = false;
            HistorialFiltrado.Clear();
            TituloHistorial = "Historial de Gastos y Compras";

            try
            {
                var gastosReal = await _libretaService.GetHistorialGastosAsync(FechaInicio, FechaFin);
                foreach (var gasto in gastosReal)
                {
                    HistorialFiltrado.Add(gasto);
                }
                MostrarTablaHistorial = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cargar gastos: {ex.Message}");
            }
        }

        [RelayCommand]
        private void VerReportesFinancieros()
        {
            // Pantalla delegada al compañero
            // _navigationStore.CurrentViewModel = new ReportesFinancierosViewModel(_navigationStore);
        }

        [RelayCommand]
        private async Task VerMetodosPago()
        {
            MostrarTablaHistorial = false;
            MostrarFormularioMetodoPago = false;
            HistorialFiltrado.Clear();
            TituloHistorial = "Métodos de Pago Registrados";

            try
            {
                var metodos = await _libretaService.GetMetodosPagoAsync();
                foreach (var met in metodos)
                {
                    HistorialFiltrado.Add(met);
                }
                MostrarMetodosPago = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cargar métodos: {ex.Message}");
            }
        }

        // Al hacer clic en el botón azul "Agregar" con el lápiz:
        [RelayCommand]
        private void MostrarFormularioNuevoMetodo()
        {
            MostrarMetodosPago = false;
            MostrarFormularioMetodoPago = true;
            TituloHistorial = "Nuevo Método de Pago";
            NuevoMetodoPagoNombre = string.Empty;
        }

        [RelayCommand]
        private async Task RegistrarMetodoPagoReal()
        {
            if (string.IsNullOrWhiteSpace(NuevoMetodoPagoNombre)) return;

            try
            {
                // Inserción real en la tabla formas_pago
                await _libretaService.InsertMetodoPagoAsync(NuevoMetodoPagoNombre.Trim());

                // Limpiamos y refrescamos volviendo automáticamente al listado de tarjetas
                NuevoMetodoPagoNombre = string.Empty;
                await VerMetodosPago();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al registrar método de pago: {ex.Message}");
            }
        }

        [RelayCommand]
        private void ToggleMetodoPagoForm()
        {
            MostrarFormularioMetodoPago = !MostrarFormularioMetodoPago;
        }

        [RelayCommand]
        private void VerDetalleTransaccion(TransaccionHistorial transaccion)
        {
            if (transaccion == null) return;

            // Aquí derivarás a la nueva pantalla de detalles pasando el objeto completo
            // _navigationStore.CurrentViewModel = new DetalleTransaccionViewModel(_navigationStore, transaccion);
        }
    }
}