using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using VabelMitienditaEsc.Core;
using VabelMitienditaEsc.Models;
using VabelMitienditaEsc.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace VabelMitienditaEsc.ViewModels
{
    public partial class DetalleProductoViewModel : ViewModelBase
    {
        private readonly NavigationStore _navigationStore;
        private readonly InventarioService _inventarioService;
        private readonly MainViewModel _mainViewModel;
        private List<Producto> _catalogoCompleto = new();

        public enum NivelStock { Critico, Advertencia, Optimo }

        // Listas desplegables y controles UI
        [ObservableProperty] private ObservableCollection<Categoria> _categorias = new();
        [ObservableProperty] private ObservableCollection<Producto> _resultadosBusqueda = new();
        [ObservableProperty] private bool _mostrarResultados;
        [ObservableProperty] private Producto? _productoSeleccionado;
        [ObservableProperty] private string _buscarTexto = string.Empty;

        // ==========================================
        // CAMPOS EDITABLES CON NOTIFICACIÓN EN CASCADA
        // ==========================================
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SinProductoSeleccionado), nameof(EstadoReabastecimiento), nameof(RequiereResurtido))]
        private int _productoId;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SinProductoSeleccionado), nameof(EstadoReabastecimiento), nameof(RequiereResurtido))]
        private string _nombre = string.Empty;

        [ObservableProperty] private string _marca = string.Empty;
        [ObservableProperty] private string _codigoBarra = string.Empty;
        [ObservableProperty] private string _descripcion = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PorcentajeGanancia), nameof(MontoUtilidad), nameof(UtilidadTotalEstimada))]
        private decimal _precioCosto; // Mantenemos el nombre PrecioCosto para respetar los bindings del XAML original

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PorcentajeGanancia), nameof(MontoUtilidad), nameof(UtilidadTotalEstimada))]
        private decimal _precioVenta;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(UtilidadTotalEstimada), nameof(NivelStockActual), nameof(EstadoColor), nameof(EstadoBorderColor), nameof(EstadoTextoColor), nameof(EstadoReabastecimiento), nameof(RequiereResurtido))]
        private int _stockActual;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(NivelStockActual), nameof(EstadoColor), nameof(EstadoBorderColor), nameof(EstadoTextoColor), nameof(EstadoReabastecimiento), nameof(RequiereResurtido))]
        private int _stockMinimo;

        [ObservableProperty] private int _categoriaId = 1;

        // ==========================================
        // PROPIEDADES CALCULADAS (Lógica del compañero)
        // ==========================================
        public decimal PorcentajeGanancia => PrecioCosto == 0 ? 0 : ((PrecioVenta - PrecioCosto) / PrecioCosto) * 100;
        public decimal MontoUtilidad => PrecioVenta - PrecioCosto;
        public decimal UtilidadTotalEstimada => MontoUtilidad * (StockActual > 0 ? StockActual : 1);

        public bool SinProductoSeleccionado => ProductoId == 0 && string.IsNullOrWhiteSpace(Nombre);
        public bool RequiereResurtido => !SinProductoSeleccionado && StockActual <= StockMinimo;

        public NivelStock NivelStockActual
        {
            get
            {
                if (SinProductoSeleccionado) return NivelStock.Optimo;
                if (StockActual <= StockMinimo) return NivelStock.Critico;
                if (StockActual <= StockMinimo * 2) return NivelStock.Advertencia;
                return NivelStock.Optimo;
            }
        }

        public string EstadoColor => NivelStockActual switch
        {
            NivelStock.Critico => "#FFF0F0",
            NivelStock.Advertencia => "#FFF8E1",
            _ => "#F0FFF0"
        };

        public string EstadoBorderColor => NivelStockActual switch
        {
            NivelStock.Critico => "#FEB2B2",
            NivelStock.Advertencia => "#FFE082",
            _ => "#C6F6D5"
        };

        public string EstadoTextoColor => NivelStockActual switch
        {
            NivelStock.Critico => "#C53030",
            NivelStock.Advertencia => "#E65100",
            _ => "#276749"
        };

        public string EstadoReabastecimiento => SinProductoSeleccionado
            ? "🔍 Busque o seleccione un producto"
            : NivelStockActual switch
            {
                NivelStock.Critico => $"⚠️⚠️ CRÍTICO: ¡Solo quedan {StockActual} unidades! Reabastezca URGENTE.",
                NivelStock.Advertencia => $"⚠️ ADVERTENCIA: Stock bajo ({StockActual} unidades). Considere reabastecer pronto.",
                _ => $"✅ Stock óptimo ({StockActual} unidades). No requiere compras urgentes."
            };

        // ==========================================
        // CONSTRUCTOR Y LÓGICA DE DATOS
        // ==========================================
        public DetalleProductoViewModel(InventarioService inventarioService, MainViewModel mainViewModel, NavigationStore navigationStore)
        {
            _inventarioService = inventarioService;
            _mainViewModel = mainViewModel;
            _navigationStore = navigationStore;
            _ = InicializarDatosAsync();
        }

        private async Task InicializarDatosAsync()
        {
            try
            {
                var cats = await _inventarioService.GetCategoriasAsync();
                Categorias = new ObservableCollection<Categoria>(cats);

                if (Categorias.Any()) CategoriaId = Categorias.First().IdCategoria;
                await CargarCatalogo();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar catálogos: {ex.Message}");
            }
        }

        private async Task CargarCatalogo()
        {
            int idTienda = _mainViewModel.CurrentUser?.IdTienda ?? 1;
            _catalogoCompleto = await _inventarioService.GetProductosAsync(idTienda);
        }

        // ==========================================
        // COMANDOS Y BÚSQUEDA REACTIVA
        // ==========================================
        partial void OnBuscarTextoChanged(string value)
        {
            BuscarProductos();
        }

        [RelayCommand]
        private void BuscarProductos()
        {
            if (string.IsNullOrWhiteSpace(BuscarTexto))
            {
                ResultadosBusqueda.Clear();
                MostrarResultados = false;
                return;
            }

            string filtro = BuscarTexto.ToLower().Trim();
            var filtrados = _catalogoCompleto.Where(p =>
                p.Nombre.ToLower().Contains(filtro) ||
                (p.CodigoBarra != null && p.CodigoBarra.ToLower().Contains(filtro))
            ).Take(15).ToList();

            ResultadosBusqueda = new ObservableCollection<Producto>(filtrados);
            MostrarResultados = filtrados.Any();
        }

        partial void OnProductoSeleccionadoChanged(Producto? value)
        {
            if (value != null)
            {
                ProductoId = value.ProductoId;
                Nombre = value.Nombre;
                Marca = value.Marca;
                CodigoBarra = value.CodigoBarra;
                Descripcion = value.Descripcion;
                PrecioCosto = value.PrecioCompra;
                PrecioVenta = value.PrecioVenta;
                StockActual = value.StockActual;
                StockMinimo = value.StockMinimo;
                CategoriaId = value.CategoriaId;

                MostrarResultados = false;
                BuscarTexto = string.Empty;
            }
        }

        [RelayCommand]
        private void LimpiarCampos()
        {
            ProductoId = 0;
            Nombre = string.Empty;
            Marca = string.Empty;
            CodigoBarra = string.Empty;
            Descripcion = string.Empty;
            PrecioCosto = 0;
            PrecioVenta = 0;
            StockActual = 0;
            StockMinimo = 0;
            CategoriaId = Categorias.FirstOrDefault()?.IdCategoria ?? 1;

            ProductoSeleccionado = null;
            BuscarTexto = string.Empty;
            MostrarResultados = false;
        }

        [RelayCommand]
        private async Task GuardarCambios()
        {
            if (string.IsNullOrWhiteSpace(Nombre))
            {
                MessageBox.Show("El nombre del producto es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                int idTienda = _mainViewModel.CurrentUser?.IdTienda ?? 1;
                Producto producto = new Producto
                {
                    ProductoId = ProductoId,
                    Nombre = Nombre,
                    Marca = Marca,
                    CodigoBarra = CodigoBarra,
                    Descripcion = Descripcion,
                    PrecioCompra = PrecioCosto,
                    PrecioVenta = PrecioVenta,
                    StockActual = StockActual,
                    StockMinimo = StockMinimo,
                    CategoriaId = CategoriaId,
                    Ubicacion = "General"
                };

                if (ProductoId == 0)
                {
                    // Si el ID es 0, es un artículo nuevo
                    var productoInsertado = await _inventarioService.InsertProducto(producto, idTienda);
                    if (productoInsertado != null && productoInsertado.ProductoId > 0)
                    {
                        ProductoId = productoInsertado.ProductoId;
                        MessageBox.Show("✓ Producto creado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    // De lo contrario actualizamos
                    await _inventarioService.ActualizarProducto(producto, idTienda);
                    MessageBox.Show("✓ Producto actualizado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                await CargarCatalogo();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar: {ex.Message}", "Error Crítico", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task EliminarProducto()
        {
            if (ProductoId == 0) return;

            var result = MessageBox.Show($"¿Eliminar '{Nombre}'?\nEsta acción no se puede deshacer.", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    int idTienda = _mainViewModel.CurrentUser?.IdTienda ?? 1;
                    await _inventarioService.EliminarProducto(ProductoId, idTienda);
                    MessageBox.Show("El producto ha sido eliminado.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                    await CargarCatalogo();
                    LimpiarCampos();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"No se pudo eliminar el producto: {ex.Message}", "Error Crítico", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private void NavigateToInventario()
        {
            _navigationStore.CurrentViewModel = new InventarioViewModel(
                _navigationStore,
                _mainViewModel,
                _inventarioService);
        }
    }
}