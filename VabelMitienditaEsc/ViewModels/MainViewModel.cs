using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VabelMitienditaEsc.Core;
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
        private string _userEmail;

        [ObservableProperty]
        private string _userName; // Nueva propiedad para almacenar el nombre que viene de la BD

        public MainViewModel(NavigationStore navigationStore, DatabaseAuthenticationService authService)
        {
            _navigationStore = navigationStore;
            _authService = authService;
            _navigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;

            _navigationStore.CurrentViewModel = new LoginEmailViewModel(_navigationStore, _authService, this);
        }

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
            UserEmail = string.Empty;
            UserName = string.Empty; // Limpieza de sesión
            _navigationStore.CurrentViewModel = new LoginEmailViewModel(_navigationStore, _authService, this);
        }
    }
}