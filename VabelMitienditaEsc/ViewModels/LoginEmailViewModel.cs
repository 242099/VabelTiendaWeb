using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VabelMitienditaEsc.Core;
using VabelMitienditaEsc.Services;

namespace VabelMitienditaEsc.ViewModels
{
    public partial class LoginEmailViewModel : ViewModelBase
    {
        private readonly NavigationStore _navigationStore;
        private readonly DatabaseAuthenticationService _authService;
        private readonly MainViewModel _mainViewModel;
        private readonly TiendaService _tiendaService;

        [ObservableProperty]
        private string _email;

        [ObservableProperty]
        private string _errorMessage;

        [ObservableProperty]
        private bool _hasError;

        [ObservableProperty]
        private bool _isLoading;

        public LoginEmailViewModel(NavigationStore navigationStore, DatabaseAuthenticationService authService, MainViewModel mainViewModel, TiendaService tiendaService)
        {
            _navigationStore = navigationStore;
            _authService = authService;
            _mainViewModel = mainViewModel;
            _tiendaService = tiendaService;
        }

        [RelayCommand]
        private async Task Next()
        {
            IsLoading = true;
            HasError = false;

            bool isValid = await _authService.ValidateEmailAsync(Email);

            IsLoading = false;
            if (isValid)
            {
                _mainViewModel.TempEmail = Email;
                _navigationStore.CurrentViewModel = new LoginPinViewModel(_navigationStore, _authService, _mainViewModel, _tiendaService);
            }
            else
            {
                ErrorMessage = "Correo no encontrado en la base de datos o cuenta inactiva.";
                HasError = true;
            }
        }
    }
}
