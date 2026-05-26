using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VabelMitienditaEsc.Core;
using VabelMitienditaEsc.Models;
using VabelMitienditaEsc.Services;

namespace VabelMitienditaEsc.ViewModels
{
    public partial class NuevaVentaViewModel : ViewModelBase
    {
        private readonly NavigationStore _navigationStore;
        private readonly MainViewModel _mainViewModel;
        private readonly InventarioService _inventarioService;
        private readonly VentasService _ventasService;

        private List<Producto> _todosLosProductos = new();

        [ObservableProperty]
        private ObservableCollection<Producto> _productosCatalogo = new();

        [ObservableProperty]
        private ObservableCollection<Producto> _productosCarrito = new();

        [ObservableProperty]
        private string _filtroBusqueda = string.Empty;

        [ObservableProperty]
        private decimal _total;

        [ObservableProperty]
        private int _totalProductosCarrito;

        public NuevaVentaViewModel(
            NavigationStore navigationStore, 
            MainViewModel mainViewModel,
            InventarioService inventarioService,
            VentasService ventasService)
        {
            _navigationStore = navigationStore;
            _mainViewModel = mainViewModel;
            _inventarioService = inventarioService;
            _ventasService = ventasService;

            _ = RefrescarProductos();
        }

        // Método parcial generado por CommunityToolkit.Mvvm que se ejecuta al cambiar la propiedad FiltroBusqueda
        partial void OnFiltroBusquedaChanged(string value)
        {
            FiltrarProductos();
        }

        // Cargar productos de la base de datos según la tienda del usuario logueado
        public async Task RefrescarProductos()
        {
            try
            {
                int idTienda = _mainViewModel.CurrentUser?.IdTienda ?? 1;
                _todosLosProductos = await _inventarioService.GetProductosAsync(idTienda);
                FiltrarProductos();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar catálogo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Ejecuta el filtrado reactivo del catálogo en memoria
        [RelayCommand]
        private void FiltrarProductos()
        {
            if (string.IsNullOrWhiteSpace(FiltroBusqueda))
            {
                ProductosCatalogo = new ObservableCollection<Producto>(_todosLosProductos);
            }
            else
            {
                string filtro = FiltroBusqueda.ToLower().Trim();
                var filtrados = _todosLosProductos.Where(p =>
                    p.Nombre.ToLower().Contains(filtro) ||
                    p.CodigoBarra.Contains(filtro)
                ).ToList();

                ProductosCatalogo = new ObservableCollection<Producto>(filtrados);
            }
        }

        [RelayCommand]
        private void AgregarAlCarrito(Producto producto)
        {
            if (producto == null) return;

            var existente = ProductosCarrito.FirstOrDefault(p => p.ProductoId == producto.ProductoId);
            if (existente != null)
            {
                existente.CantidadCarrito++;
            }
            else
            {
                producto.CantidadCarrito = 1;
                ProductosCarrito.Add(producto);
            }

            RecalcularTotales();
            // Truco para refrescar el bindeo de colecciones de manera limpia
            ProductosCarrito = new ObservableCollection<Producto>(ProductosCarrito);
        }

        [RelayCommand]
        private void QuitarDelCarrito(Producto producto)
        {
            if (producto == null) return;

            var existente = ProductosCarrito.FirstOrDefault(p => p.ProductoId == producto.ProductoId);
            if (existente != null)
            {
                existente.CantidadCarrito--;
                if (existente.CantidadCarrito <= 0)
                {
                    ProductosCarrito.Remove(existente);
                }
            }

            RecalcularTotales();
            ProductosCarrito = new ObservableCollection<Producto>(ProductosCarrito);
        }

        [RelayCommand]
        private void EliminarItemCarrito(Producto producto)
        {
            if (producto == null) return;

            var existente = ProductosCarrito.FirstOrDefault(p => p.ProductoId == producto.ProductoId);
            if (existente != null)
            {
                ProductosCarrito.Remove(existente);
            }

            RecalcularTotales();
        }

        private void RecalcularTotales()
        {
            Total = ProductosCarrito.Sum(p => p.CantidadCarrito * p.PrecioVenta);
            TotalProductosCarrito = ProductosCarrito.Sum(p => p.CantidadCarrito);
        }

        [RelayCommand]
        private async Task CobrarVenta()
        {
            if (!ProductosCarrito.Any())
            {
                MessageBox.Show("El carrito de compras está vacío.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                int idTienda = _mainViewModel.CurrentUser?.IdTienda ?? 1;
                int idUsuario = _mainViewModel.CurrentUser?.IdUsuario ?? 1;
                int idFormaPago = 1; // Asumimos "Efectivo" como forma de pago predeterminada

                decimal montoTotal = Total;
                decimal montoIva = montoTotal * 0.16m;

                // Construcción limpia del ticket informativo
                string detalleVenta = "PRODUCTOS VENDIDOS:\n\n";
                foreach (var item in ProductosCarrito)
                {
                    detalleVenta += $"• {item.Nombre}\n";
                    detalleVenta += $"  Cantidad: {item.CantidadCarrito} x {item.PrecioVenta:C} = {item.CantidadCarrito * item.PrecioVenta:C}\n\n";
                }
                detalleVenta += $"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n";
                detalleVenta += $"TOTAL: {montoTotal:C}\n";
                detalleVenta += $"Total productos: {TotalProductosCarrito}";

                // Guardar en Base de Datos de manera asíncrona
                int idVenta = await _ventasService.RegistrarVentaAsync(montoTotal, montoIva, ProductosCarrito, idTienda, idUsuario, idFormaPago);

                if (idVenta > 0)
                {
                    ProductosCarrito.Clear();
                    RecalcularTotales();
                    await RefrescarProductos();

                    MessageBox.Show(detalleVenta, $"✅ Venta #{idVenta} completada con éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error crítico al procesar la operación: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void CancelarVenta()
        {
            ProductosCarrito.Clear();
            RecalcularTotales();
            FiltroBusqueda = string.Empty;
        }
    }
}