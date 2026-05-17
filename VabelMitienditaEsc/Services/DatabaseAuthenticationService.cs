using System;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace VabelMitienditaEsc.Services
{
    public class DatabaseAuthenticationService
    {
        private readonly string _connectionString;

        public DatabaseAuthenticationService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<bool> ValidateEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string query = "SELECT COUNT(1) FROM usuario WHERE email = @email AND activo = 1";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@email", email);
                    int count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    return count > 0;
                }
            }
        }

        public async Task<(bool IsValid, string NombreUsuario)> ValidatePinAsync(string email, string pinText)
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string query = "SELECT nombre, contrasena FROM usuario WHERE email = @email AND activo = 1 LIMIT 1";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@email", email);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            string dbHash = reader.GetString(reader.GetOrdinal("contrasena"));
                            string nombre = reader.GetString(reader.GetOrdinal("nombre"));

                            // Comparación utilizando la librería BCrypt.Net-Next
                            // Verify automáticamente extrae el "Salt" del hash guardado y realiza la validación
                            if (BCrypt.Net.BCrypt.Verify(pinText, dbHash))
                            {
                                return (true, nombre);
                            }
                        }
                    }
                }
            }
            return (false, string.Empty);
        }
    }
}