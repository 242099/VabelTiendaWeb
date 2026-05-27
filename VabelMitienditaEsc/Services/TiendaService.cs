using MySql.Data.MySqlClient;
using System.Data;
using System.Threading.Tasks;
using VabelMitienditaEsc.Models;

namespace VabelMitienditaEsc.Services
{
    public class TiendaService
    {
        private readonly string _connectionString;

        public TiendaService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<Tienda?> GetTiendaByIdAsync(int idTienda)
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string query = "SELECT id_tienda, nombre, direccion, telefono, activa, fecha_registro FROM tienda WHERE id_tienda = @idTienda";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@idTienda", idTienda);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Tienda
                            {
                                IdTienda = reader.GetInt32("id_tienda"),
                                Nombre = reader.GetString("nombre"),
                                Direccion = reader.GetString("direccion"),
                                Telefono = reader.IsDBNull(reader.GetOrdinal("telefono")) ? string.Empty : reader.GetString("telefono"),
                                Activa = reader.GetBoolean("activa"),
                                FechaRegistro = reader.GetDateTime("fecha_registro")
                            };
                        }
                    }
                }
            }
            return null;
        }
    }
}