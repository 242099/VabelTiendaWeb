using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VabelMitienditaEsc.Core;
using VabelMitienditaEsc.Models;
using VabelMitienditaEsc.Services;

namespace VabelMitienditaEsc.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        private readonly NavigationStore _navigationStore;
        private readonly DatabaseAuthenticationService _authService;

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

        public MainViewModel(NavigationStore navigationStore, DatabaseAuthenticationService authService)
        {
            _navigationStore = navigationStore;
            _authService = authService;
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

        [RelayCommand]
        private void Logout()
        {
            CurrentUser = null;
            TempEmail = string.Empty;
            _navigationStore.CurrentViewModel = new LoginEmailViewModel(_navigationStore, _authService, this);
        }
    }
}