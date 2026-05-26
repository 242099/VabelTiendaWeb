using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VabelMitienditaEsc.Core;
using VabelMitienditaEsc.Models;
using VabelMitienditaEsc.Services;

namespace VabelMitienditaEsc.ViewModels
{
    public partial class GastoViewModel : ViewModelBase
    {
        private readonly NavigationStore _navigationStore;
        private readonly MainViewModel _mainViewModel;
        private readonly GastosOperativosService _gastosService;
        private readonly ProveedorService _proveedorService;
        private readonly FormasPagoService _formasPagoService;
        private readonly CatalogoCuentasService _catalogoCuentasService;

        // Propiedades observables para el listado
        [ObservableProperty] private ObservableCollection<VistaGastoOperativo> _listaGastosOperativos;

        //ComboBox
        [ObservableProperty] private ObservableCollection<ProveedorVistaLista> _listaProveedores;
        [ObservableProperty] private ObservableCollection<FormasPago> _listaFormasPago;
        [ObservableProperty] private ObservableCollection<CatalogoCuentas> _listaCatalogoCuentas;

        [ObservableProperty] private DateTime _fechaGasto;
        [ObservableProperty] private string _descripcionGasto;
        [ObservableProperty] private decimal _montoGasto;
        [ObservableProperty] private decimal _tasaIVAGasto;
        [ObservableProperty] private string _observacionesGasto;
        [ObservableProperty] private int? _proveedorSeleccionado;
        [ObservableProperty] private int _formaPagoSeleccionada;
        [ObservableProperty] private int _cuentaSeleccionada;

        [ObservableProperty] private bool _mostrarListadoGastos;
        [ObservableProperty] private bool _esEdicion;
        [ObservableProperty] private GastoOperativo _gastoActual;

        [ObservableProperty] private DateTime? _fechaInicio;
        [ObservableProperty] private DateTime? _fechaFin;
        [ObservableProperty] private decimal _montoTotal;

        public GastoViewModel(NavigationStore navigationStore, MainViewModel mainViewModel,
            GastosOperativosService gastosService, ProveedorService proveedorService,
            FormasPagoService formasPagoService, CatalogoCuentasService catalogoCuentasService)
        {
            _navigationStore = navigationStore;
            _mainViewModel = mainViewModel;
            _gastosService = gastosService;
            _proveedorService = proveedorService;
            _formasPagoService = formasPagoService;
            _catalogoCuentasService = catalogoCuentasService;

            ListaGastosOperativos = new ObservableCollection<VistaGastoOperativo>();
            ListaProveedores = new ObservableCollection<ProveedorVistaLista>();
            ListaFormasPago = new ObservableCollection<FormasPago>();
            ListaCatalogoCuentas = new ObservableCollection<CatalogoCuentas>();

            FechaGasto = DateTime.Now;
            FechaInicio = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            FechaFin = DateTime.Now;
            MostrarListadoGastos = false;
            EsEdicion = false;

            _ = CargarDatosIniciales();
        }

        private async Task CargarDatosIniciales()
        {
            try
            {
                // Cargar proveedores
                var proveedores = await _proveedorService.ListarProveedores();
                ListaProveedores.Clear();
                if (proveedores != null)
                {
                    foreach (var proveedor in proveedores)
                    {
                        ListaProveedores.Add(proveedor);
                    }
                }

                // Cargar formas de pago
                var formasPago = await _formasPagoService.Listar();
                ListaFormasPago.Clear();
                if (formasPago != null)
                {
                    foreach (var forma in formasPago)
                    {
                        ListaFormasPago.Add(forma);
                    }
                }

                // Cargar catálogo de cuentas
                var cuentas = await _catalogoCuentasService.Listar();
                ListaCatalogoCuentas.Clear();
                if (cuentas != null)
                {
                    foreach (var cuenta in cuentas)
                    {
                        ListaCatalogoCuentas.Add(cuenta);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar datos iniciales: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void AbrirFormularioNuevoGasto()
        {
            MostrarListadoGastos = !MostrarListadoGastos;
            if (MostrarListadoGastos)
            {
                LimpiarFormulario();
                EsEdicion = false;
                _ = CargarGastosAsync();
            }
        }

        [RelayCommand]
        public async Task CargarGastosAsync()
        {
            try
            {
                int idTienda = _mainViewModel.CurrentUser?.IdTienda ?? 0;

                var gastos = await _gastosService.ListarGastos(idTienda);
                ListaGastosOperativos.Clear();

                if (gastos != null)
                {
                    decimal total = 0;
                    foreach (var gasto in gastos)
                    {
                        if (FechaInicio.HasValue && FechaFin.HasValue)
                        {
                            if (gasto.fecha >= FechaInicio.Value && gasto.fecha <= FechaFin.Value)
                            {
                                ListaGastosOperativos.Add(gasto);
                                total += gasto.monto;
                            }
                        }
                        else
                        {
                            ListaGastosOperativos.Add(gasto);
                            total += gasto.monto;
                        }
                    }

                    MontoTotal = total;
                }

                MostrarListadoGastos = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar gastos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task EditarGasto(VistaGastoOperativo gasto)
        {
            if (gasto == null) return;

            try
            {
                var gastoCompleto = await _gastosService.ObtenerGastoPorId(gasto.idGastos);

                if (gastoCompleto == null)
                {
                    MessageBox.Show("No se pudo cargar el gasto seleccionado", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                EsEdicion = true;
                GastoActual = gastoCompleto;

                FechaGasto = gastoCompleto.fecha;
                DescripcionGasto = gastoCompleto.descripcion;
                MontoGasto = gastoCompleto.monto;
                TasaIVAGasto = gastoCompleto.tasaIVA;
                ObservacionesGasto = gastoCompleto.observaciones ?? string.Empty;
                ProveedorSeleccionado = gastoCompleto.idProveedor;
                FormaPagoSeleccionada = gastoCompleto.idFormaPago;
                CuentaSeleccionada = gastoCompleto.idCuenta;

                MostrarListadoGastos = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al editar gasto: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task GuardarGasto()
        {
            if (!ValidarFormulario()) return;

            try
            {
                var gasto = new GastoOperativo
                {
                    idGastos = EsEdicion ? GastoActual?.idGastos ?? 0 : 0,
                    fecha = FechaGasto,
                    descripcion = DescripcionGasto,
                    monto = MontoGasto,
                    tasaIVA = TasaIVAGasto,
                    observaciones = ObservacionesGasto,
                    idUsuario = _mainViewModel.CurrentUser?.IdUsuario ?? 0,
                    idProveedor = ProveedorSeleccionado,
                    idFormaPago = FormaPagoSeleccionada,
                    idCuenta = CuentaSeleccionada,
                    idTienda = _mainViewModel.CurrentUser?.IdTienda ?? 0
                };

                bool resultado = false;

                if (EsEdicion)
                {
                    resultado = await _gastosService.ActualizarGasto(gasto);
                    if (resultado)
                    {
                        MessageBox.Show("Gasto actualizado correctamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    resultado = await _gastosService.InsertarGasto(gasto);
                    if (resultado)
                    {
                        MessageBox.Show("Gasto creado correctamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }

                if (resultado)
                {
                    await CargarGastosAsync();
                    CancelarEdicion();
                }
                else
                {
                    MessageBox.Show("No se pudo guardar el gasto", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar gasto: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task BuscarPorFechas()
        {
            if (!FechaInicio.HasValue || !FechaFin.HasValue)
            {
                MessageBox.Show("Debe especificar ambas fechas", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                int idTienda = _mainViewModel.CurrentUser?.IdTienda ?? 0;

                var gastosEnFecha = await _gastosService.BuscarGastoPorFecha(FechaInicio.Value, FechaFin.Value, idTienda);

                ListaGastosOperativos.Clear();

                if (gastosEnFecha != null && gastosEnFecha.Count > 0)
                {
                    decimal total = 0;
                    foreach (var gasto in gastosEnFecha)
                    {
                        ListaGastosOperativos.Add(gasto);
                        total += gasto.monto;
                    }
                    MontoTotal = total;
                    MostrarListadoGastos = true;
                }
                else
                {
                    MessageBox.Show("No se encontraron gastos en el rango de fechas especificado", "Sin resultados", MessageBoxButton.OK, MessageBoxImage.Information);
                    MontoTotal = 0;
                    MostrarListadoGastos = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error en la búsqueda: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void CancelarEdicion()
        {
            LimpiarFormulario();
            MostrarListadoGastos = false;
            EsEdicion = false;
        }

        private bool ValidarFormulario()
        {
            if (string.IsNullOrWhiteSpace(DescripcionGasto))
            {
                MessageBox.Show("La descripción es requerida", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (MontoGasto <= 0)
            {
                MessageBox.Show("El monto debe ser mayor a 0", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            /*if (ProveedorSeleccionado <= 0)
            {
                MessageBox.Show("Debe seleccionar un proveedor", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }*/

            if (CuentaSeleccionada <= 0)
            {
                MessageBox.Show("Debe seleccionar una cuenta para facilitar su registro contable", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (FormaPagoSeleccionada <= 0)
            {
                MessageBox.Show("Debe seleccionar una forma de pago", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void LimpiarFormulario()
        {
            FechaGasto = DateTime.Now;
            DescripcionGasto = string.Empty;
            MontoGasto = 0;
            TasaIVAGasto = 0;
            ObservacionesGasto = string.Empty;
            ProveedorSeleccionado = 0;
            FormaPagoSeleccionada = 0;
            CuentaSeleccionada = 0;
            GastoActual = null;
        }
    }
}