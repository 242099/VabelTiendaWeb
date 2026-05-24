using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using VabelMitienditaEsc.Core;
using VabelMitienditaEsc.Models;

namespace VabelMitienditaEsc.ViewModels
{
    public partial class LibretaVentasViewModel : ViewModelBase
    {
        private readonly NavigationStore _navigationStore;
        private readonly MainViewModel _mainViewModel;

        // Colección original (simularía la Base de Datos completa)
        private List<TransaccionHistorial> _historialCompletoDb = new();

        [ObservableProperty]
        private bool _esDueno;

        [ObservableProperty]
        private string _rolUsuarioActual;

        [ObservableProperty]
        private decimal _totalGanancias;

        [ObservableProperty]
        private decimal _totalGastos;

        [ObservableProperty]
        private ObservableCollection<TopProducto> _topProductosList;

        // Propiedades para agregar Método de Pago rápido
        [ObservableProperty]
        private string _nuevoMetodoPagoNombre;

        [ObservableProperty]
        private bool _mostrarFormularioMetodoPago;

        [ObservableProperty]
        private bool _mostrarMetodosPago; // Controla si se ve la lista de métodos de pago

        // Filtros de fecha
        [ObservableProperty]
        private DateTime? _fechaInicio;

        [ObservableProperty]
        private DateTime? _fechaFin;

        //Area de despliegue
        [ObservableProperty]
        private ObservableCollection<TransaccionHistorial> _historialFiltrado;

        [ObservableProperty]
        private string _tituloHistorial = "Seleccione un historial para visualizar";

        [ObservableProperty]
        private bool _mostrarTablaHistorial = false;
        public LibretaVentasViewModel(NavigationStore navigationStore, MainViewModel mainViewModel)
        {
            _navigationStore = navigationStore;
            _mainViewModel = mainViewModel;
            TopProductosList = new ObservableCollection<TopProducto>();
            HistorialFiltrado = new ObservableCollection<TransaccionHistorial>();

            // ASIGNACIÓN COMPLETAMENTE DINÁMICA DESDE LA BASE DE DATOS
            // Obtenemos el nombre del rol real ("Dueño", "Empleado", etc.)
            string rolDb = _mainViewModel.CurrentUser?.NombreRol;

            // Fallback de seguridad extrema: si por alguna razón la sesión fuese nula, asigna "Empleado"
            RolUsuarioActual = !string.IsNullOrEmpty(rolDb) ? rolDb : "Empleado";

            // Evaluamos permisos basados de manera exacta en la cadena de texto real de la BD
            EsDueno = RolUsuarioActual.Equals("Dueño", StringComparison.OrdinalIgnoreCase);

            // Carga inicial (Histórico general)
            _ = CargarDatosDashboardAsync();
            // Cargar datos dummy iniciales a la simulación de BD
            CargarDatosDummy();
        }

        // Interceptores de CommunityToolkit. Se ejecutan automáticamente al cambiar las fechas en la UI
        partial void OnFechaInicioChanged(DateTime? value)
        {
            _ = CargarDatosDashboardAsync();
        }

        partial void OnFechaFinChanged(DateTime? value)
        {
            _ = CargarDatosDashboardAsync();
        }

        private async Task CargarDatosDashboardAsync()
        {
            // Aquí irá la llamada al servicio que ejecuta las consultas SQL (SUM de ventas, SUM de compras, etc.)
            // pasando _fechaInicio y _fechaFin como parámetros. Si son null, la consulta no filtra por fecha.

            // Simulación de carga de datos para previsualización del diseño
            await Task.Delay(100);

            if (_fechaInicio.HasValue || _fechaFin.HasValue)
            {
                TotalGanancias = 1520.00m; // Dato simulado del filtro
                TotalGastos = 380.50m;
            }
            else
            {
                TotalGanancias = 45500.00m; // Histórico general
                TotalGastos = 12300.00m;
            }

            TopProductosList.Clear();
            TopProductosList.Add(new TopProducto { Posicion = 1, NombreProducto = "Coca Cola 600ml", CantidadVendida = 145 });
            TopProductosList.Add(new TopProducto { Posicion = 2, NombreProducto = "Gansito Marinela", CantidadVendida = 89 });
            TopProductosList.Add(new TopProducto { Posicion = 3, NombreProducto = "Sabritas Sal 40g", CantidadVendida = 76 });
        }

        private void CargarDatosDummy()
        {
            _historialCompletoDb.Add(new TransaccionHistorial
            {
                Id = 1024,
                Fecha = DateTime.Now.AddDays(-1),
                Concepto = "Venta Mostrador - Cliente General",
                Monto = 450.50m,
                Tipo = "Venta",
                MetodoPago = "Efectivo",
                EsVenta = true,
                Icono = "\ue8cc", // Icono de carrito/bolsa
                Observaciones = "Sin observaciones"
            });

            _historialCompletoDb.Add(new TransaccionHistorial
            {
                Id = 201,
                Fecha = DateTime.Now.AddDays(-1),
                Concepto = "Pago Proveedor Sabritas",
                Monto = 1200.00m,
                Tipo = "Gasto",
                MetodoPago = "Transferencia",
                EsVenta = false,
                Icono = "\uf053", // Icono de egreso/pago
                Observaciones = "Factura F-9982"
            });

            _historialCompletoDb.Add(new TransaccionHistorial
            {
                Id = 1025,
                Fecha = DateTime.Now,
                Concepto = "Venta Mostrador - Pedido #1025",
                Monto = 89.00m,
                Tipo = "Venta",
                MetodoPago = "Tarjeta de Débito",
                EsVenta = true,
                Icono = "\ue8cc",
                Observaciones = "Terminal Banamex"
            });

            _historialCompletoDb.Add(new TransaccionHistorial
            {
                Id = 305,
                Fecha = DateTime.Now,
                Concepto = "Compra de empaques y bolsas",
                Monto = 350.00m,
                Tipo = "Gasto",
                MetodoPago = "Efectivo",
                EsVenta = false,
                Icono = "\ue857", // Icono de tienda/insumos
                Observaciones = "Papelería Local"
            });
        }

        // [Ruta: ViewModels/LibretaVentasViewModel.cs]

        [RelayCommand]
        private void VerHistorialVentas()
        {
            // 1. Ocultamos por completo las vistas de métodos de pago
            System.Diagnostics.Debug.WriteLine("Cargando historial de ventas...");
            MostrarMetodosPago = false;
            MostrarFormularioMetodoPago = false;

            // 2. Limpiamos y cargamos el historial
            HistorialFiltrado.Clear();
            TituloHistorial = "Historial de Ventas";

            var ventas = _historialCompletoDb.Where(t => t.EsVenta &&
                (!FechaInicio.HasValue || t.Fecha >= FechaInicio.Value) &&
                (!FechaFin.HasValue || t.Fecha <= FechaFin.Value));

            foreach (var venta in ventas)
            {
                HistorialFiltrado.Add(venta);
            }

            // 3. Mostramos la lista de transacciones
            MostrarTablaHistorial = true;
        }

        [RelayCommand]
        private void VerHistorialGastos()
        {
            // 1. Ocultamos por completo las vistas de métodos de pago
            System.Diagnostics.Debug.WriteLine("Cargando historial de gastos...");
            MostrarMetodosPago = false;
            MostrarFormularioMetodoPago = false;

            // 2. Limpiamos y cargamos el historial
            HistorialFiltrado.Clear();
            TituloHistorial = "Historial de Gastos y Compras";

            var gastos = _historialCompletoDb.Where(t => !t.EsVenta &&
                (!FechaInicio.HasValue || t.Fecha >= FechaInicio.Value) &&
                (!FechaFin.HasValue || t.Fecha <= FechaFin.Value));

            foreach (var gasto in gastos)
            {
                HistorialFiltrado.Add(gasto);
            }

            // 3. Mostramos la lista de transacciones
            MostrarTablaHistorial = true;
        }

        [RelayCommand]
        private void VerReportesFinancieros()
        {
            // Pantalla delegada al compañero
            // _navigationStore.CurrentViewModel = new ReportesFinancierosViewModel(_navigationStore);
        }

        [RelayCommand]
        private void VerMetodosPago()
        {
            // Ocultamos el historial de transacciones y el formulario
            MostrarTablaHistorial = false;
            MostrarFormularioMetodoPago = false;

            // Cambiamos el título y activamos la vista de tarjetas de métodos de pago
            TituloHistorial = "Métodos de Pago Registrados";
            MostrarMetodosPago = true;

            // Aquí en un futuro cargarías desde la BD a una colección, 
            // por ahora podemos reutilizar HistorialFiltrado limpiándolo o mapeando datos dummy
            HistorialFiltrado.Clear();

            // Simulamos los métodos de pago actuales usando el DTO de forma temporal para la UI
            // (En el futuro usarás una ObservableCollection<FormaPago> dedicada si lo prefieres)
            HistorialFiltrado.Add(new TransaccionHistorial { Id = 1, Concepto = "Efectivo", Icono = "\ue8a1", MetodoPago = "Activo" });
            HistorialFiltrado.Add(new TransaccionHistorial { Id = 2, Concepto = "Tarjeta de Débito", Icono = "\uea14", MetodoPago = "Activo" });
            HistorialFiltrado.Add(new TransaccionHistorial { Id = 3, Concepto = "Transferencia Interbancaria", Icono = "\ue63e", MetodoPago = "Activo" });
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

            // SIMULACIÓN BD: Aquí irá el INSERT INTO formas_pago...
            await Task.Delay(300);

            // Regresar automáticamente a la lista de métodos de pago y refrescar
            VerMetodosPago();
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