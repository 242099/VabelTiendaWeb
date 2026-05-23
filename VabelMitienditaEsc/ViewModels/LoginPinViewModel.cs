using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;
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

            // Nos suscribimos a los datos del Arduino al entrar a esta vista
            if (_mainViewModel.DispositivoArduino != null)
            {
                _mainViewModel.DispositivoArduino.DataReceived += OnArduinoDataReceived;
            }
        }

        // Este método se dispara automáticamente cuando el Arduino envía el PIN al presionar '#'
        private async void OnArduinoDataReceived(string pinDesdeArduino)
        {
            // Como el evento del SerialPort ocurre en un hilo secundario, 
            // usamos el Dispatcher para volver al hilo principal de la Interfaz (UI)
            await Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                IsLoading = true;
                HasError = false;

                // Validamos el PIN que llegó desde el Arduino matricial
                var usuarioLogueado = await _authService.ValidatePinAsync(
                    _mainViewModel.CurrentUser?.Email ?? _mainViewModel.TempEmail,
                    pinDesdeArduino);

                IsLoading = false;

                if (usuarioLogueado != null)
                {
                    // Le decimos al Arduino que el PIN fue correcto para que muestre "Bienvenido"
                    _mainViewModel.DispositivoArduino?.SendData("ACCESO_OK");

                    // Desuscribimos el evento para no tener errores de memoria al cambiar de vista
                    _mainViewModel.DispositivoArduino.DataReceived -= OnArduinoDataReceived;

                    _mainViewModel.CurrentUser = usuarioLogueado;
                    _mainViewModel.TempEmail = string.Empty;
                    _navigationStore.CurrentViewModel = new LobbyViewModel(_navigationStore, _mainViewModel);
                }
                else
                {
                    // Le decimos al Arduino que el PIN fue incorrecto
                    _mainViewModel.DispositivoArduino?.SendData("ACCESO_DENEGADO");

                    ErrorMessage = "PIN incorrecto desde Teclado Físico. Intente de nuevo.";
                    HasError = true;
                }
            });
        }

        [RelayCommand]
        private async Task Login(object passwordBoxObject)
        {
            // Se mantiene igual por si el usuario decide usar el teclado normal de la PC en el PasswordBox
            if (passwordBoxObject is PasswordBox passwordBox)
            {
                IsLoading = true;
                HasError = false;

                string clearTextPin = SecureStringToString(passwordBox.SecurePassword);
                var usuarioLogueado = await _authService.ValidatePinAsync(_mainViewModel.CurrentUser?.Email ?? _mainViewModel.TempEmail, clearTextPin);

                IsLoading = false;
                if (usuarioLogueado != null)
                {
                    _mainViewModel.DispositivoArduino?.SendData("ACCESO_OK"); // Sincronizamos pantalla

                    if (_mainViewModel.DispositivoArduino != null)
                        _mainViewModel.DispositivoArduino.DataReceived -= OnArduinoDataReceived;

                    _mainViewModel.CurrentUser = usuarioLogueado;
                    _mainViewModel.TempEmail = string.Empty;
                    _navigationStore.CurrentViewModel = new LobbyViewModel(_navigationStore, _mainViewModel);
                }
                else
                {
                    _mainViewModel.DispositivoArduino?.SendData("ACCESO_DENEGADO");

                    ErrorMessage = "PIN incorrecto. Intente de nuevo.";
                    HasError = true;
                    passwordBox.Clear();
                }
            }
        }

        [RelayCommand]
        private void Back()
        {
            if (_mainViewModel.DispositivoArduino != null)
            {
                _mainViewModel.DispositivoArduino.DataReceived -= OnArduinoDataReceived;
            }
            _navigationStore.CurrentViewModel = new LoginEmailViewModel(_navigationStore, _authService, _mainViewModel);
        }

        private string SecureStringToString(SecureString secureValue)
        {
            if (secureValue == null || secureValue.Length == 0) return string.Empty;
            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = Marshal.SecureStringToBSTR(secureValue);
                return Marshal.PtrToStringBSTR(ptr);
            }
            finally
            {
                if (ptr != IntPtr.Zero) Marshal.ZeroFreeBSTR(ptr);
            }
        }
    }
}