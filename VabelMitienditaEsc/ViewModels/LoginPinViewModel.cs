using System.Security;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Controls;
using VabelMitienditaEsc.Core;
using VabelMitienditaEsc.Services;

namespace VabelMitienditaEsc.ViewModels
{
    public partial class LoginPinViewModel : ViewModelBase
    {
        private readonly NavigationStore _navigationStore;
        private readonly DatabaseAuthenticationService _authService;
        private readonly MainViewModel _mainViewModel;

        [ObservableProperty]
        private string _errorMessage;

        [ObservableProperty]
        private bool _hasError;

        [ObservableProperty]
        private bool _isLoading;

        public LoginPinViewModel(NavigationStore navigationStore, DatabaseAuthenticationService authService, MainViewModel mainViewModel)
        {
            _navigationStore = navigationStore;
            _authService = authService;
            _mainViewModel = mainViewModel;
        }

        [RelayCommand]
        private async Task Login(object passwordBoxObject)
        {
            if (passwordBoxObject is PasswordBox passwordBox)
            {
                IsLoading = true;
                HasError = false;

                string clearTextPin = SecureStringToString(passwordBox.SecurePassword);
                // El servicio ahora retorna una tupla con la validación y el nombre
                var usuarioLogueado = await _authService.ValidatePinAsync(_mainViewModel.CurrentUser?.Email ?? _mainViewModel.TempEmail, clearTextPin);
                // Nota: necesitarás guardar el Email temporalmente en alguna parte durante el proceso de login.
                IsLoading = false;
                if (usuarioLogueado != null)
                {
                    _mainViewModel.CurrentUser = usuarioLogueado;
                    _mainViewModel.TempEmail = string.Empty; // Limpiamos el email temporal ya que ya no es necesario
                    _navigationStore.CurrentViewModel = new LobbyViewModel(_navigationStore, _mainViewModel);
                }
                else
                {
                    ErrorMessage = "PIN incorrecto. Intente de nuevo.";
                    HasError = true;
                    passwordBox.Clear();
                }
            }
        }

        [RelayCommand]
        private void Back()
        {
            _navigationStore.CurrentViewModel = new LoginEmailViewModel(_navigationStore, _authService, _mainViewModel);
        }

        private string SecureStringToString(SecureString secureValue)
        {
            if (secureValue == null || secureValue.Length == 0)
                return string.Empty;

            IntPtr ptr = IntPtr.Zero;
            try
            {
                // Se almacena directamente en IntPtr, preservando los 64 bits de la dirección de memoria.
                ptr = Marshal.SecureStringToBSTR(secureValue);
                return Marshal.PtrToStringBSTR(ptr);
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                {
                    Marshal.ZeroFreeBSTR(ptr);
                }
            }
        }
    }
}