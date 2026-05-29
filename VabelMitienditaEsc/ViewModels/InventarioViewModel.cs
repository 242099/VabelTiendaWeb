using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using VabelMitienditaEsc.Core;
using VabelMitienditaEsc.Models;
using VabelMitienditaEsc.Services;

namespace VabelMitienditaEsc.ViewModels
{
    public partial class InventarioViewModel : ViewModelBase
    {
        private readonly NavigationStore _navigationStore;
        private readonly MainViewModel _mainViewModel;
        private readonly InventarioService _inventarioService;

        // Métricas visibles en el XAML
        [ObservableProperty]
        private ObservableCollection<Producto> _ultimosProductos = new();

        [ObservableProperty]
        private int _totalProductos;

        [ObservableProperty]
        private decimal _valorTotalInventario;

        public InventarioViewModel(
            NavigationStore navigationStore,
            MainViewModel mainViewModel,
            InventarioService inventarioService)
        {
            _navigationStore = navigationStore;
            _mainViewModel = mainViewModel;
            _inventarioService = inventarioService;

            // Cargar datos del dashboard del inventario en background
            _ = CargarInventarioAsync();
        }

        public async Task CargarInventarioAsync()
        {
            try
            {
                int idTienda = _mainViewModel?.CurrentUser?.IdTienda ?? 1;
                var productos = await _inventarioService.GetProductosAsync(idTienda);

                TotalProductos = productos.Count;

                // Calculamos el valor del inventario en base al precio de venta y stock actual
                ValorTotalInventario = productos.Sum(p => p.StockActual * p.PrecioVenta);

                // Últimos 5 productos añadidos
                var ultimos = productos.OrderByDescending(x => x.FechaRegistro).Take(5);
                UltimosProductos = new ObservableCollection<Producto>(ultimos);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando dashboard del inventario: {ex.Message}");
            }
        }

        // Navegación hacia el módulo de control de productos
        [RelayCommand]
        private void VerDetalleProducto()
        {
            // Navega a la vista de detalles del producto inyectando el NavigationStore
            // No se pasa ningún producto por parámetro, por lo que cargará la pantalla de inicio limpia.
            _navigationStore.CurrentViewModel = new DetalleProductoViewModel(
                _inventarioService,
                _mainViewModel,
                _navigationStore);
        }
    }
}