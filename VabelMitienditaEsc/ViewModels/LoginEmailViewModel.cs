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
        private readonly MockAuthenticationService _authService;
        private readonly MainViewModel _mainViewModel;

        [ObservableProperty]
        private string _email;

        [ObservableProperty]
        private string _errorMessage;

        [ObservableProperty]
        private bool _hasError;

        [ObservableProperty]
        private bool _isLoading;

        public LoginEmailViewModel(NavigationStore navigationStore, MockAuthenticationService authService, MainViewModel mainViewModel)
        {
            _navigationStore = navigationStore;
            _authService = authService;
            _mainViewModel = mainViewModel;
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
                _mainViewModel.UserEmail = Email;
                _navigationStore.CurrentViewModel = new LoginPinViewModel(_navigationStore, _authService, _mainViewModel);
            }
            else
            {
                ErrorMessage = "El correo ingresado no es válido.";
                HasError = true;
            }
        }
    }
}
