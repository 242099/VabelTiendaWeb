using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Threading.Tasks;
using VabelMitienditaEsc.Core;
using VabelMitienditaEsc.Models;
using VabelMitienditaEsc.Services;

namespace VabelMitienditaEsc.ViewModels
{
    public partial class LobbyViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;
        private readonly TiendaService _tiendaService;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(NombreCompleto))]
        private Usuario? _usuarioActual;

        [ObservableProperty]
        private Tienda? _tiendaActual;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _mensajeError = string.Empty;

        public string NombreCompleto => UsuarioActual != null
            ? $"{UsuarioActual.Nombre} {UsuarioActual.APaterno} {UsuarioActual.AMaterno}".Trim()
            : "Usuario Desconocido";

        public LobbyViewModel(MainViewModel mainViewModel, TiendaService tiendaService)
        {
            _mainViewModel = mainViewModel;
            _tiendaService = tiendaService;

            // Asignar el usuario que inició sesión desde el contexto principal
            UsuarioActual = _mainViewModel.CurrentUser;

            _ = CargarDatosLobbyAsync();
        }

        private async Task CargarDatosLobbyAsync()
        {
            if (UsuarioActual == null)
            {
                MensajeError = "No se pudo recuperar la sesión del usuario actual.";
                return;
            }

            if (UsuarioActual.IdTienda <= 0)
            {
                MensajeError = "El usuario no tiene una tienda asignada válida.";
                return;
            }

            IsBusy = true;
            MensajeError = string.Empty;

            try
            {
                TiendaActual = await _tiendaService.GetTiendaByIdAsync(UsuarioActual.IdTienda);

                if (TiendaActual == null)
                {
                    MensajeError = "No se encontró la información de la tienda en el servidor.";
                }
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al conectar con la base de datos: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}