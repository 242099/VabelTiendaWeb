using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
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

        // Colección mostrada en la tabla (se adapta a los bindings usados en la vista)
        [ObservableProperty]
        private ObservableCollection<InventarioItem> _listaInventario;

        // Backup de todos los elementos para aplicar filtros localmente
        private List<InventarioItem> _allItems = new List<InventarioItem>();
        [ObservableProperty] private ObservableCollection<Producto> _ultimosProductos;
        [ObservableProperty] private int _totalProductos;
        [ObservableProperty] private decimal _valorTotalInventario;

        // Estados de UI
        [ObservableProperty] private bool _mostrarFormularioAgregarProducto;
        [ObservableProperty] private bool _mostrarDetalleProducto;
        [ObservableProperty] private bool _mostrarBotonAgregar = true;

        // Campos del nuevo producto (simple esqueleto)
        [ObservableProperty] private string _nuevoProductoNombre;
        [ObservableProperty] private string _nuevoProductoMarca;
        [ObservableProperty] private decimal _nuevoProductoPrecioCompra;
        [ObservableProperty] private int _nuevoProductoCategoriaId;
        [ObservableProperty] private string _nuevoProductoDescripcion;
        [ObservableProperty] private int _nuevoProductoStockMinimo;
        [ObservableProperty] private string _nuevoProductoUbicacion;
        [ObservableProperty] private string _nuevoProductoCodigo;
        [ObservableProperty] private int _nuevoProductoStock;
        [ObservableProperty] private decimal _nuevoProductoPrecioVenta;

        // Búsqueda
        [ObservableProperty] private string _filtroBusqueda;
        [ObservableProperty] private InventarioItem _selectedProducto;
        // Modo edición (no es binded en UI directamente)
        [ObservableProperty]
        private bool _isEditMode;

        public InventarioViewModel(NavigationStore navigationStore, MainViewModel mainViewModel, InventarioService inventarioService)
        {
            _navigationStore = navigationStore;
            _mainViewModel = mainViewModel;
            _inventarioService = inventarioService;

            ListaInventario = new ObservableCollection<InventarioItem>();
            UltimosProductos = new ObservableCollection<Producto>();


            // Cargar datos iniciales en background
            _ = CargarInventarioAsync();

        }

        public async Task CargarInventarioAsync()
        {
            try
            {
                ListaInventario.Clear();
                _allItems.Clear();

                int idTienda = _mainViewModel?.CurrentUser?.IdTienda ?? 0;
                var productos = await _inventarioService.GetProductosAsync(idTienda);

                foreach (var p in productos)
                {
                    var item = new InventarioItem
                    {
                        Producto = p,
                        Nombre = p.Nombre,
                        Stock = p.StockActual,
                        Precio = p.PrecioVenta
                    };

                    _allItems.Add(item);
                }

                // Poblamos colecciones visibles
                foreach (var it in _allItems)
                    ListaInventario.Add(it);

                TotalProductos = _allItems.Count;

                // Últimos productos añadidos por fecha (si existe)
                var ultimos = productos.OrderByDescending(x => x.FechaRegistro).Take(5);
                UltimosProductos.Clear();
                foreach (var u in ultimos)
                    UltimosProductos.Add(u);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando inventario: {ex.Message}");
            }
            // Multiplica el stock actual de cada producto por su precio y suma todo el total
            ValorTotalInventario = ListaInventario.Sum(item => item.Stock * item.Precio);
        }

        [RelayCommand]
        private void MostrarFormularioNuevoProducto()
        {
            MostrarFormularioAgregarProducto = true;
            NuevoProductoNombre = string.Empty;
            NuevoProductoCodigo = string.Empty;
            NuevoProductoStock = 0;
            NuevoProductoPrecioVenta = 0m;
            NuevoProductoPrecioCompra = 0m;
        }

        [RelayCommand]
        private void CancelarAgregarProducto()
        {
            MostrarFormularioAgregarProducto = false;
            IsEditMode = false;
        }



        [RelayCommand]
        private async Task RegistrarProducto()
        {
            if (string.IsNullOrWhiteSpace(NuevoProductoNombre) || string.IsNullOrWhiteSpace(NuevoProductoCodigo))
                return;

            try
            {
                int idTienda = _mainViewModel?.CurrentUser?.IdTienda ?? 0;

                // Mapeo COMPLETO para evitar excepciones de campos NOT NULL en la base de datos
                var nuevoProductoModel = new Producto
                {
                    CodigoBarra = NuevoProductoCodigo.Trim(),
                    Nombre = NuevoProductoNombre.Trim(),

                    Marca = NuevoProductoMarca?.Trim() ?? string.Empty,
                    PrecioCompra = NuevoProductoPrecioCompra <= 0 ? NuevoProductoPrecioCompra : NuevoProductoPrecioCompra,
                    PrecioVenta = NuevoProductoPrecioVenta,
                    Descripcion = NuevoProductoDescripcion.Trim() ?? string.Empty,
                    CategoriaId = NuevoProductoCategoriaId > 0 ? NuevoProductoCategoriaId : 2,
                    StockActual = NuevoProductoStock,
                    StockMinimo = NuevoProductoStockMinimo > 0 ? NuevoProductoStockMinimo : 5,
                    Ubicacion = string.IsNullOrWhiteSpace(NuevoProductoUbicacion) ? "Almacén" : NuevoProductoUbicacion.Trim(),
                    FechaRegistro = DateTime.Now,
                    Activo = true
                };

                // 1. Llamar al servicio para insertar en la BD
                var productoInsertado = await _inventarioService.InsertProducto(nuevoProductoModel, idTienda);

                if (productoInsertado != null)
                {
                    // 2. Reflejar en la interfaz localmente
                    var item = new InventarioItem
                    {
                        Producto = productoInsertado,
                        Codigo = productoInsertado.CodigoBarra,
                        Nombre = productoInsertado.Nombre,
                        Stock = productoInsertado.StockActual,
                        Precio = productoInsertado.PrecioVenta
                    };

                    _allItems.Insert(0, item);
                    ListaInventario.Insert(0, item);

                    TotalProductos = _allItems.Count;

                    UltimosProductos.Insert(0, productoInsertado);
                    if (UltimosProductos.Count > 5) UltimosProductos.RemoveAt(UltimosProductos.Count - 1);

                    // Cerrar formulario (AQUÍ ESTABA EL ERROR LÓGICO DE LA UI)
                    MostrarFormularioAgregarProducto = false;
                    IsEditMode = false;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("El servicio no pudo registrar el producto en la BD.");
                }
            }
            catch (Exception ex)
            {
                // Si sigue fallando, revisa la ventana de "Salida/Output" en Visual Studio
                // para leer exactamente este mensaje de error de SQL.
                System.Diagnostics.Debug.WriteLine($"Error registrando producto en BD: {ex.Message}");
            }
        }

        [RelayCommand]
        private void EditarProducto(InventarioItem productoSeleccionado)
        {
            // 1. Si no llegó nada desde la interfaz, cancelamos
            if (productoSeleccionado == null) return;

            // Guardamos el producto en memoria para que el botón verde lo encuentre después.
            SelectedProducto = productoSeleccionado;

            // 3. Activamos el modo edición y mostramos el formulario
            IsEditMode = true;
            MostrarFormularioAgregarProducto = true;

            // 4. Llenamos las cajas de texto del formulario con los datos actuales
            NuevoProductoNombre = productoSeleccionado.Nombre;
            NuevoProductoCodigo = productoSeleccionado.Codigo;
            NuevoProductoMarca = productoSeleccionado.Marca;
            NuevoProductoStock = productoSeleccionado.Stock;
            NuevoProductoPrecioVenta = productoSeleccionado.PrecioVenta;
            NuevoProductoPrecioCompra = productoSeleccionado.PrecioCompra;
            NuevoProductoUbicacion = productoSeleccionado.Ubicacion;
            NuevoProductoStockMinimo = productoSeleccionado.StockMinimo;
        }

        [RelayCommand]
        private async Task ModificarProducto()
        {
            // PRUEBA DE VIDA: Si sale este mensaje, el botón ya funciona perfectamente
            System.Windows.MessageBox.Show("¡El botón verde sí llamó al comando!");

            // 1. Verificamos si se guardó la selección
            if (SelectedProducto == null || SelectedProducto.Producto == null)
            {
                System.Windows.MessageBox.Show("Error: El producto seleccionado se perdió de memoria.");
                return;
            }

            // 2. Verificamos que los campos obligatorios no estén vacíos
            if (string.IsNullOrWhiteSpace(NuevoProductoNombre) || string.IsNullOrWhiteSpace(NuevoProductoCodigo))
            {
                System.Windows.MessageBox.Show("Error: Nombre o Código vacíos.");
                return;
            }
            // Validamos que exista un producto seleccionado y que los campos requeridos no estén vacíos
            if (SelectedProducto == null || SelectedProducto.Producto == null) return;
            if (string.IsNullOrWhiteSpace(NuevoProductoNombre) || string.IsNullOrWhiteSpace(NuevoProductoCodigo))
                return;

            try
            {
                int idTienda = _mainViewModel?.CurrentUser?.IdTienda ?? 0;

                // Recuperamos el modelo original y le mapeamos los nuevos valores del formulario
                var productoAEditar = SelectedProducto.Producto;

                productoAEditar.CodigoBarra = NuevoProductoCodigo.Trim();
                productoAEditar.Nombre = NuevoProductoNombre.Trim();
                productoAEditar.Descripcion = "Sin descripción"; // Cumple con el NOT NULL
                productoAEditar.Marca = string.IsNullOrWhiteSpace(NuevoProductoMarca) ? "Genérico" : NuevoProductoMarca.Trim();
                productoAEditar.PrecioCompra = NuevoProductoPrecioCompra > 0 ? NuevoProductoPrecioCompra : NuevoProductoPrecioCompra;
                productoAEditar.PrecioVenta = NuevoProductoPrecioVenta;
                productoAEditar.CategoriaId = NuevoProductoCategoriaId > 0 ? NuevoProductoCategoriaId : 2;
                productoAEditar.StockActual = NuevoProductoStock;
                productoAEditar.StockMinimo = NuevoProductoStockMinimo > 0 ? NuevoProductoStockMinimo : 5;
                productoAEditar.Ubicacion = string.IsNullOrWhiteSpace(NuevoProductoUbicacion) ? "Almacén" : NuevoProductoUbicacion.Trim();

                // 1. Llamar al servicio para impactar los cambios en MySQL
                bool actualizadoConExito = await _inventarioService.ActualizarProducto(productoAEditar, idTienda);

                if (actualizadoConExito)
                {
                    // 2. Reflejar los cambios inmediatamente en el renglón/tarjeta de la UI
                    SelectedProducto.Codigo = productoAEditar.CodigoBarra;
                    SelectedProducto.Nombre = productoAEditar.Nombre;
                    SelectedProducto.Stock = productoAEditar.StockActual;
                    SelectedProducto.PrecioVenta = productoAEditar.PrecioVenta;

                    // 3. Opcional: Actualizar el producto dentro de la lista de "UltimosProductos" si es que aparece ahí
                    var itemEnUltimos = UltimosProductos.FirstOrDefault(x => x.ProductoId == productoAEditar.ProductoId);
                    if (itemEnUltimos != null)
                    {
                        int index = UltimosProductos.IndexOf(itemEnUltimos);
                        UltimosProductos[index] = productoAEditar;
                    }

                    // 4. Recalcular contadores del Dashboard
                    TotalProductos = _allItems.Count;

                    // 5. Restablecer estados de la UI y limpiar selección
                    MostrarFormularioAgregarProducto = false;
                    IsEditMode = false;
                    SelectedProducto = null;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("El servicio devolvió falso al intentar actualizar.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error modificando producto en BD: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task EliminarProducto(InventarioItem item)
        {
            if (item == null) return;

            try
            {
                int idTienda = _mainViewModel?.CurrentUser?.IdTienda ?? 0;
                // Eliminar en BD
                if (item.Producto?.ProductoId > 0)
                {
                    await _inventarioService.EliminarProducto(item.Producto.ProductoId, idTienda);
                }

                ListaInventario.Remove(item);
                _allItems.Remove(item);

                TotalProductos = _allItems.Count;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error eliminando producto: {ex.Message}");
            }
        }


        [RelayCommand]
        private void VerDetalleProducto(InventarioItem item)
        {

        }

        // Clase auxiliar para adaptar las propiedades de Producto a los bindings XAML
        public class InventarioItem
        {
            public Producto Producto { get; set; }
            public string Codigo { get; set; }
            public string Nombre { get; set; }
            public string Marca { get; }
            public int Stock { get; set; }
            public int StockMinimo { get; set; }
            public decimal Precio { get; set; }
            public decimal PrecioCompra { get; set; }
            public decimal PrecioVenta { get; set; }
            public string Ubicacion { get; set; }
        }
    }
}
