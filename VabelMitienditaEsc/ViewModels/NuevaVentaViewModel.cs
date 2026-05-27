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
        private readonly LibretaVentasService _libretaVentasService;
        private readonly TiendaService _tiendaService;

        private List<Producto> _todosLosProductos = new();

        [ObservableProperty]
        private ObservableCollection<Producto> _productosCatalogo = new();

        [ObservableProperty]
        private ObservableCollection<Producto> _productosCarrito = new();

        [ObservableProperty]
        private string _filtroBusqueda = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Cambio))]
        private decimal _total;

        [ObservableProperty]
        private int _totalProductosCarrito;

        [ObservableProperty]
        private ObservableCollection<FormaPago> _metodosPago = new();

        [ObservableProperty]
        private FormaPago? _metodoPagoSeleccionado;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Cambio))]
        private string _efectivoTexto = string.Empty;

        public decimal CantidadRecibida => decimal.TryParse(EfectivoTexto, out decimal result) ? result : 0m;

        public decimal Cambio => CantidadRecibida >= Total ? CantidadRecibida - Total : 0m;

        public NuevaVentaViewModel(
            NavigationStore navigationStore, 
            MainViewModel mainViewModel,
            InventarioService inventarioService,
            VentasService ventasService,
            LibretaVentasService libretaVentasService,
            TiendaService tiendaService)
        {
            _navigationStore = navigationStore;
            _mainViewModel = mainViewModel;
            _inventarioService = inventarioService;
            _ventasService = ventasService;
            _libretaVentasService = libretaVentasService;
            _tiendaService = tiendaService;

            _ = RefrescarProductos();
            _ = CargarMetodosPago();
        }

        // Método parcial generado por CommunityToolkit.Mvvm que se ejecuta al cambiar la propiedad FiltroBusqueda
        partial void OnFiltroBusquedaChanged(string value)
        {
            FiltrarProductos();
        }

        public async Task CargarMetodosPago()
        {
            try
            {
                var metodos = await _libretaVentasService.GetFormasPagoAsync();
                MetodosPago = new ObservableCollection<FormaPago>(metodos);
                if (MetodosPago.Any())
                {
                    MetodoPagoSeleccionado = MetodosPago.First();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar métodos de pago: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

            if (MetodoPagoSeleccionado == null)
            {
                MessageBox.Show("Por favor seleccione un método de pago.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool esEfectivo = MetodoPagoSeleccionado.Nombre.Equals("Efectivo", StringComparison.OrdinalIgnoreCase);

            if (esEfectivo && CantidadRecibida < Total)
            {
                MessageBox.Show("El monto en efectivo recibido es menor al total de la compra.", "Monto insuficiente", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                int idTienda = _mainViewModel.CurrentUser?.IdTienda ?? 1;
                int idUsuario = _mainViewModel.CurrentUser?.IdUsuario ?? 1;
                int idFormaPago = MetodoPagoSeleccionado.IdFormaPago;

                decimal montoTotal = Total;
                decimal montoIva = montoTotal * 0.16m;

                int idVenta = await _ventasService.RegistrarVentaAsync(montoTotal, montoIva, ProductosCarrito, idTienda, idUsuario, idFormaPago);

                if (idVenta > 0)
                {
                    // Consultamos los detalles reales de la tienda para generar el ticket
                    Tienda? tiendaFisica = await _tiendaService.GetTiendaByIdAsync(idTienda);

                    string nombreT = (tiendaFisica?.Nombre ?? "TIENDA NO REGISTRADA").ToUpper();
                    string direccionT = (tiendaFisica?.Direccion ?? "DIRECCIÓN NO DISPONIBLE").ToUpper();

                    // Función local para centrar texto a 40 caracteres
                    string CentrarTexto(string texto)
                    {
                        if (texto.Length >= 40) return texto.Substring(0, 40);
                        int espaciosIzquierda = (40 + texto.Length) / 2;
                        return texto.PadLeft(espaciosIzquierda).PadRight(40);
                    }

                    // Estructuración profesional del formato del ticket comercial
                    string sepDouble = "========================================\n";
                    string sepSimple = "----------------------------------------\n";

                    string ticket = sepDouble;
                    ticket += $"{CentrarTexto(nombreT)}\n";
                    ticket += $"{CentrarTexto(direccionT)}\n";
                    ticket += sepDouble;
                    ticket += $"Ticket de Venta: #{idVenta}\n";
                    ticket += $"Fecha:  {DateTime.Now:dd/MM/yyyy HH:mm:ss}\n";
                    ticket += $"Cajero: {_mainViewModel.CurrentUser?.Nombre ?? "Admin"} {_mainViewModel.CurrentUser?.APaterno ?? ""}\n";
                    ticket += sepSimple;
                    ticket += "Cant   Descripción               Importe\n";
                    ticket += sepSimple;

                    foreach (var item in ProductosCarrito)
                    {
                        string nombreProd = item.Nombre.Length > 22 ? item.Nombre.Substring(0, 22) : item.Nombre;
                        ticket += $"{item.CantidadCarrito,-6} {nombreProd,-22} {item.TotalNeto,10:C}\n";
                        ticket += $"       ({item.PrecioVenta:C} c/u)\n";
                    }

                    ticket += sepSimple;
                    ticket += $"Total Artículos: {TotalProductosCarrito}\n";
                    ticket += $"Subtotal:        {montoTotal - montoIva,24:C}\n";
                    ticket += $"IVA (16%):       {montoIva,24:C}\n";
                    ticket += $"TOTAL A PAGAR:   {montoTotal,24:C}\n";
                    ticket += sepSimple;

                    if (esEfectivo)
                    {
                        ticket += $"Forma de Pago:   {MetodoPagoSeleccionado.Nombre}\n";
                        ticket += $"Efectivo Recib.: {CantidadRecibida,24:C}\n";
                        ticket += $"Cambio Entreg.:  {Cambio,24:C}\n";
                    }
                    else
                    {
                        ticket += $"Forma de Pago:   {MetodoPagoSeleccionado.Nombre}\n";
                        ticket += $"Monto Cobrado:   {montoTotal,24:C}\n";
                    }

                    ticket += sepDouble;
                    ticket += "     ¡GRACIAS POR SU PREFERENCIA!     \n";
                    ticket += sepDouble;

                    ProductosCarrito.Clear();
                    EfectivoTexto = string.Empty;
                    RecalcularTotales();
                    await RefrescarProductos();

                    MessageBox.Show(ticket, "Comprobante Comercial de Venta", MessageBoxButton.OK, MessageBoxImage.Information);
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
            EfectivoTexto = string.Empty;
        }
    }
}