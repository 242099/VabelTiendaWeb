using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VabelMitienditaEsc.Core;
using VabelMitienditaEsc.Models;

namespace VabelMitienditaEsc.ViewModels
{
    public partial class LibretaVentasViewModel : ViewModelBase
    {
        private readonly NavigationStore _navigationStore;
        private readonly MainViewModel _mainViewModel;

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

        // Filtros de fecha
        [ObservableProperty]
        private DateTime? _fechaInicio;

        [ObservableProperty]
        private DateTime? _fechaFin;

        public LibretaVentasViewModel(NavigationStore navigationStore, MainViewModel mainViewModel)
        {
            _navigationStore = navigationStore;
            _mainViewModel = mainViewModel;
            TopProductosList = new ObservableCollection<TopProducto>();

            // Configuración de Roles basada en la BD (id_rol 1 = Dueño, 2 = Empleado)
            // Se asume que en el modelo Usuario agregaste la propiedad IdRol
            // Si no la tienes aún mapeada, temporalmente se valida si es nulo
            int idRol = _mainViewModel.CurrentUser?.IdRol ?? 2;
            EsDueno = idRol == 1;
            RolUsuarioActual = EsDueno ? "Dueño" : "Empleado";

            // Carga inicial (Histórico general)
            _ = CargarDatosDashboardAsync();
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

        [RelayCommand]
        private void VerHistorialVentas()
        {
            // Nota: Se inyectarán _fechaInicio y _fechaFin al constructor del ViewModel Genérico
            // _navigationStore.CurrentViewModel = new HistorialGenericoViewModel(_navigationStore, "Ventas", _fechaInicio, _fechaFin);
        }

        [RelayCommand]
        private void VerHistorialGastos()
        {
            // _navigationStore.CurrentViewModel = new HistorialGenericoViewModel(_navigationStore, "Gastos", _fechaInicio, _fechaFin);
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
            // _navigationStore.CurrentViewModel = new MetodosPagoViewModel(_navigationStore);
        }
    }
}