using CommunityToolkit.Mvvm.ComponentModel;
using MySql.Data.MySqlClient; // Requiere el paquete NuGet MySql.Data
using System.Collections.ObjectModel;
using System.Data;
using VabelMitienditaEsc.Core;
using VabelMitienditaEsc.Models;

namespace VabelMitienditaEsc.ViewModels
{
    public partial class LobbyViewModel : ViewModelBase
    {
        private readonly NavigationStore _navigationStore;
        private readonly MainViewModel _mainViewModel;
        private readonly string _connectionString = "Server=localhost;Database=vabel_db;Uid=root;Pwd=root;";

        [ObservableProperty] private string _fechaActual;
        [ObservableProperty] private decimal _totalIngresos;
        [ObservableProperty] private int _ventasTotales;
        [ObservableProperty] private decimal _ticketPromedio;
        [ObservableProperty] private decimal _totalEfectivo;
        [ObservableProperty] private decimal _totalTarjeta;
        [ObservableProperty] private decimal _totalTransferencia;

        public ObservableCollection<VentaReciente> UltimasVentas { get; set; }

        public LobbyViewModel(NavigationStore navigationStore, MainViewModel mainViewModel)
        {
            _navigationStore = navigationStore;
            _mainViewModel = mainViewModel;
            UltimasVentas = new ObservableCollection<VentaReciente>();
            FechaActual = $"📅 {DateTime.Now:dd MMM yyyy}";

            _ = CargarDatosBalanceAsync();
        }

        private async Task CargarDatosBalanceAsync()
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                // 1. Cargar Estadísticas del día actual
                string queryEstadisticas = @"
                    SELECT 
                        COUNT(DISTINCT v.id_venta) AS TotalVentas,
                        COALESCE(SUM(dv.cantidad * dv.precio_unitario), 0) AS TotalIngresos
                    FROM ventas v
                    LEFT JOIN detalle_ventas dv ON v.id_venta = dv.id_venta
                    WHERE DATE(v.fecha) = CURDATE() AND v.id_tienda = @idTienda;";

                using (var cmd = new MySqlCommand(queryEstadisticas, connection))
                {
                    cmd.Parameters.AddWithValue("@idTienda", _mainViewModel.CurrentUser?.IdTienda ?? 1);
                    using var reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        VentasTotales = reader.GetInt32("TotalVentas");
                        TotalIngresos = reader.GetDecimal("TotalIngresos");
                        TicketPromedio = VentasTotales > 0 ? TotalIngresos / VentasTotales : 0;
                    }
                }

                // 2. Cargar desglose por método de pago
                string queryMetodosPago = @"
                    SELECT 
                        fp.nombre, 
                        COALESCE(SUM(dv.cantidad * dv.precio_unitario), 0) AS TotalMetodo
                    FROM ventas v
                    JOIN formas_pago fp ON v.id_forma_pago = fp.id_forma_pago
                    JOIN detalle_ventas dv ON v.id_venta = dv.id_venta
                    WHERE DATE(v.fecha) = CURDATE() AND v.id_tienda = @idTienda
                    GROUP BY fp.id_forma_pago;";

                using (var cmd = new MySqlCommand(queryMetodosPago, connection))
                {
                    cmd.Parameters.AddWithValue("@idTienda", _mainViewModel.CurrentUser?.IdTienda ?? 1);
                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        string metodo = reader.GetString("nombre").ToLower();
                        decimal monto = reader.GetDecimal("TotalMetodo");

                        if (metodo.Contains("efectivo")) TotalEfectivo = monto;
                        else if (metodo.Contains("tarjeta")) TotalTarjeta = monto;
                        else if (metodo.Contains("transferencia")) TotalTransferencia = monto;
                    }
                }

                // 3. Cargar las últimas ventas en la lista
                string queryUltimasVentas = @"
                    SELECT 
                        v.id_venta, 
                        v.fecha, 
                        SUM(dv.cantidad) AS TotalArticulos,
                        SUM(dv.cantidad * dv.precio_unitario) AS TotalVenta
                    FROM ventas v
                    JOIN detalle_ventas dv ON v.id_venta = dv.id_venta
                    WHERE DATE(v.fecha) = CURDATE() AND v.id_tienda = @idTienda
                    GROUP BY v.id_venta, v.fecha
                    ORDER BY v.fecha DESC LIMIT 10;";

                UltimasVentas.Clear();
                using (var cmd = new MySqlCommand(queryUltimasVentas, connection))
                {
                    cmd.Parameters.AddWithValue("@idTienda", _mainViewModel.CurrentUser?.IdTienda ?? 1);
                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        UltimasVentas.Add(new VentaReciente
                        {
                            Hora = reader.GetDateTime("fecha").ToString("HH:mm"),
                            FolioTicket = $"Ticket #{reader.GetInt32("id_venta"):D3}",
                            ResumenArticulos = $"({reader.GetInt32("TotalArticulos")} arts.)",
                            TotalVenta = reader.GetDecimal("TotalVenta")
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                // Manejar error de conexión aquí
                Console.WriteLine(ex.Message);
            }
        }
    }
}