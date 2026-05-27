using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VabelMitienditaEsc.Core;
using VabelMitienditaEsc.Models;
using VabelMitienditaEsc.Services;
using VabelMitienditaEsc.Views;

namespace VabelMitienditaEsc.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        private readonly NavigationStore _navigationStore;
        private readonly DatabaseAuthenticationService _authService;
        private readonly LibretaVentasService _libretaventasService;
        private readonly InventarioService _inventarioService;
        private readonly VentasService _ventasService;
        private readonly TiendaService _tiendaService;

        [ObservableProperty]
        private ViewModelBase _currentViewModel;

        [ObservableProperty]
        private bool _showSidebar;

        [ObservableProperty]
        private Usuario _currentUser;

        [ObservableProperty]
        private string _tempEmail; //Se utiliza durante el proceso de login para almacenar el email antes de validar el PIN

        // AGREGA ESTA LÍNEA PARA QUE EL ARDUINO SEA ACCESIBLE DESDE CUALQUIER VISTA
        public ArduinoService DispositivoArduino { get; set; }

        public MainViewModel(
            NavigationStore navigationStore, 
            DatabaseAuthenticationService authService, 
            LibretaVentasService libretaventasService,
            InventarioService inventarioService,
            VentasService ventasService,
            TiendaService tiendaService)
        {
            _navigationStore = navigationStore;
            _authService = authService;
            _libretaventasService = libretaventasService;
            _inventarioService = inventarioService;
            _ventasService = ventasService;
            _tiendaService = tiendaService;

            _navigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;

            _navigationStore.CurrentViewModel = new LoginEmailViewModel(_navigationStore, _authService, this);
        }

        //Este método prácticamente se encarga de actualizar la vista actual y decidir si mostrar o no la barra lateral dependiendo de qué vista se esté mostrando.
        private void OnCurrentViewModelChanged()
        {
            CurrentViewModel = _navigationStore.CurrentViewModel;
            ShowSidebar = CurrentViewModel is not LoginEmailViewModel &&
                          CurrentViewModel is not LoginPinViewModel;
        }

        [RelayCommand]
        private void NavigateToLobby()
        {
            _navigationStore.CurrentViewModel = new LobbyViewModel(_navigationStore, this);
        }
        // Navegación hacia Nueva venta
        [RelayCommand]
        private void NavigateToNuevaVenta()
        {
            _navigationStore.CurrentViewModel = new NuevaVentaViewModel(
                _navigationStore, 
                this, 
                _inventarioService, 
                _ventasService, 
                _libretaventasService,
                _tiendaService);
        }

        [RelayCommand]
        private void NavigateToLibretaVentas()
        {
            // Pasamos 'this' (el MainViewModel) para que la Libreta de Ventas 
            // pueda leer los datos de '_currentUser' y su 'IdRol' real de la base de datos
            _navigationStore.CurrentViewModel = new LibretaVentasViewModel(_navigationStore, this, _libretaventasService);
        }

        [RelayCommand]
        private void Logout()
        {
            CurrentUser = null;
            TempEmail = string.Empty;
            _navigationStore.CurrentViewModel = new LoginEmailViewModel(_navigationStore, _authService, this);
        }
    }
}