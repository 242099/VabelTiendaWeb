using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;
using VabelMitienditaEsc.Core;
using VabelMitienditaEsc.Models;
using VabelMitienditaEsc.Services;

namespace VabelMitienditaEsc.ViewModels
{
    public partial class ProveedorViewModel : ViewModelBase
    {
        private readonly ProveedorService _proveedorService;
        private readonly NavigationStore _navigationStore;

        // Listados
        [ObservableProperty] private ObservableCollection<ProveedorVistaLista> listaProveedores = new();
        [ObservableProperty] private ObservableCollection<string> listaEmpresas = new();

        // Filtros UI
        [ObservableProperty] private string empresaProveedorSeleccionada;

        [ObservableProperty] private string criterioBusqueda = "";

        // Visibilidad de formulario
        [ObservableProperty] private bool mostrarFormularioProveedor = false;
        [ObservableProperty] private bool esEdicion = false;

        // Propiedades del formulario
        [ObservableProperty] private int idProveedor;
        [ObservableProperty] private string nombreEmpresa;
        [ObservableProperty] private string nombreProveedor;
        [ObservableProperty] private string apellidoPaternoProveedor;
        [ObservableProperty] private string apellidoMaternoProveedor;
        [ObservableProperty] private string telefonoProveedor;
        [ObservableProperty] private string emailProveedor;
        [ObservableProperty] private string rfcEmpresa;
        [ObservableProperty] private string calleEmpresa;
        [ObservableProperty] private string numeroEmpresa;
        [ObservableProperty] private string ciudadEmpresa;
        [ObservableProperty] private DateTime? fechaRegistroProveedor = DateTime.Now;
        private MainViewModel mainViewModel;

        public ProveedorViewModel(NavigationStore navigationStore, ProveedorService proveedorService)
        {
            _proveedorService = proveedorService;
            _ = CargarProveedoresAsync();
        }

        public ProveedorViewModel(NavigationStore navigationStore, MainViewModel mainViewModel)
        {
            _navigationStore = navigationStore;
            this.mainViewModel = mainViewModel;
        }

        [RelayCommand]
        public void AbrirFormularioNuevoProveedor()
        {
            LimpiarFormulario();
            EsEdicion = false;
            MostrarFormularioProveedor = true;
        }

        [RelayCommand]
        public async Task BuscarProveedoresAsync()
        {
            ListaProveedores.Clear();
            if (string.IsNullOrWhiteSpace(CriterioBusqueda))
            {
                var proveedores = await _proveedorService.ListarProveedores();
                foreach (var p in proveedores) ListaProveedores.Add(p);
            }
            else
            {
                var resultado = await _proveedorService.BuscarProveedoresPorNombreOEmpresa(CriterioBusqueda.Trim());
                foreach (var p in resultado) ListaProveedores.Add(p);
            }
        }

        [RelayCommand]
        public async Task LimpiarBusquedaAsync()
        {
            CriterioBusqueda = "";
            await BuscarProveedoresAsync();
        }

        [RelayCommand]
        public async Task CargarProveedoresAsync()
        {
            ListaProveedores.Clear();
            var proveedores = await _proveedorService.ListarProveedores();
            foreach (var prov in proveedores) ListaProveedores.Add(prov);
        }


        [RelayCommand]
        public async Task EditarProveedor(ProveedorVistaLista proveedor)
        {
            var prov = await _proveedorService.BuscarProveedorPorId(proveedor.idProveedor);
            try
            {
                if (prov == null)
                {
                    MessageBox.Show("Proveedor no encontrado", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                IdProveedor = prov.idProveedor;
                NombreEmpresa = prov.nombreEmpresa;
                NombreProveedor = prov.nombre;
                ApellidoPaternoProveedor = prov.aPaterno;
                ApellidoMaternoProveedor = prov.aMaterno;
                TelefonoProveedor = prov.telefono;
                EmailProveedor = string.IsNullOrWhiteSpace(prov.email) ? null : prov.email;
                RfcEmpresa = prov.rfc;
                CalleEmpresa = string.IsNullOrWhiteSpace(prov.calle) ? null : prov.calle;
                NumeroEmpresa = string.IsNullOrWhiteSpace(prov.numero) ? null : prov.numero;
                CiudadEmpresa = string.IsNullOrWhiteSpace(prov.ciudad) ? null : prov.ciudad;
                FechaRegistroProveedor = prov.fechaRegistro.ToDateTime(TimeOnly.MinValue);
                EsEdicion = true;
                MostrarFormularioProveedor = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al editar proveedor: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        public async Task GuardarProveedor()
        {
            if (!ValidarFormulario()) return;

            try
            {
                var prov = new Proveedor
                {
                    idProveedor = EsEdicion ? IdProveedor : 0,
                    nombreEmpresa = NombreEmpresa,
                    nombre = NombreProveedor,
                    aPaterno = ApellidoPaternoProveedor,
                    aMaterno = ApellidoMaternoProveedor,
                    telefono = TelefonoProveedor,
                    email = string.IsNullOrWhiteSpace(EmailProveedor) ? null : EmailProveedor,
                    rfc = RfcEmpresa,
                    calle = string.IsNullOrWhiteSpace(CalleEmpresa) ? null : CalleEmpresa,
                    numero = string.IsNullOrWhiteSpace(NumeroEmpresa) ? null : NumeroEmpresa,
                    ciudad = string.IsNullOrWhiteSpace(CiudadEmpresa) ? null : CiudadEmpresa,
                    fechaRegistro = DateOnly.FromDateTime(FechaRegistroProveedor.Value)
                };
                bool result = EsEdicion
                    ? await _proveedorService.ActualizarProveedor(prov)
                    : await _proveedorService.InsertarProveedor(prov);
                if (result)
                {
                    MessageBox.Show("Proveedor actualizado correctamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("No se pudo guardar", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                if (result)
                {
                    MostrarFormularioProveedor = false;
                    await CargarProveedoresAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar proveedor: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        public async Task EliminarProveedor(ProveedorVistaLista proveedor)
        {
            if (MessageBox.Show("¿Seguro que desea eliminar?", "Confirmar", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var result = await _proveedorService.BorrarProveedor(proveedor.idProveedor);
                if (result)
                {
                    await CargarProveedoresAsync();
                    MessageBox.Show("Proveedor eliminado correctamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("No se pudo eliminar el proveedor", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        public void CancelarProveedor()
        {
            LimpiarFormulario();
            MostrarFormularioProveedor = false;
        }

        private bool ValidarFormulario()
        {
            if (string.IsNullOrWhiteSpace(NombreEmpresa))
            {
                MessageBox.Show("El nombre de la empresa es requerido", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(NombreProveedor))
            {
                MessageBox.Show("El nombre del proveedor es requerido", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(ApellidoPaternoProveedor))
            {
                MessageBox.Show("El apellido paterno del proveedor es requerido", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(ApellidoMaternoProveedor))
            {
                MessageBox.Show("El apellido materno del proveedor es requerido", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(TelefonoProveedor))
            {
                MessageBox.Show("Debe registrar un numero telefonico", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(RfcEmpresa))
            {
                MessageBox.Show("Ingrese RFC de la empresa", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!FechaRegistroProveedor.HasValue)
            {
                MessageBox.Show("Debe de haber una fecha de registro", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }


        private void LimpiarFormulario()
        {
            IdProveedor = 0;
            NombreEmpresa = "";
            NombreProveedor = "";
            ApellidoPaternoProveedor = "";
            ApellidoMaternoProveedor = "";
            TelefonoProveedor = "";
            EmailProveedor = "";
            RfcEmpresa = "";
            CalleEmpresa = "";
            NumeroEmpresa = "";
            CiudadEmpresa = "";
            FechaRegistroProveedor = DateTime.Now;
        }
    }
}