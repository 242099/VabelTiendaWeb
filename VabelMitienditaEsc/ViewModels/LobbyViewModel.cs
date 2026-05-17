using CommunityToolkit.Mvvm.Input;
using VabelMitienditaEsc.Core;

namespace VabelMitienditaEsc.ViewModels
{
    public partial class LobbyViewModel : ViewModelBase
    {
        private readonly NavigationStore _navigationStore;
        private readonly MainViewModel _mainViewModel;

        // Propiedad expuesta a la vista que formatea el saludo dinámico
        public string WelcomeMessage => $"¡Bienvenido, {_mainViewModel.UserName}!";
        public LobbyViewModel(NavigationStore navigationStore, MainViewModel mainViewModel)
        {
            _navigationStore = navigationStore;
            _mainViewModel = mainViewModel;
        }
    }
}
