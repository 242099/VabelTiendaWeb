using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VabelMitienditaEsc.Core;
using VabelMitienditaEsc.Services;

namespace VabelMitienditaEsc.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        private readonly NavigationStore _navigationStore;
        private readonly MockAuthenticationService _authService;

        [ObservableProperty]
        private ViewModelBase _currentViewModel;

        [ObservableProperty]
        private bool _showSidebar;

        [ObservableProperty]
        private string _userEmail;

        public MainViewModel(NavigationStore navigationStore, MockAuthenticationService authService)
        {
            _navigationStore = navigationStore;
            _authService = authService;
            _navigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;

            // Inicializar en la primera pantalla de Login
            _navigationStore.CurrentViewModel = new LoginEmailViewModel(_navigationStore, _authService, this);
        }

        private void OnCurrentViewModelChanged()
        {
            CurrentViewModel = _navigationStore.CurrentViewModel;

            // Determinar si se muestra la barra lateral según el tipo de ViewModel activo
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
            UserEmail = string.Empty;
            _navigationStore.CurrentViewModel = new LoginEmailViewModel(_navigationStore, _authService, this);
        }
    }
}
