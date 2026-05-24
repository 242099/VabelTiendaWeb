using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Threading.Tasks;
using VabelMitienditaEsc.Models;

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

        // Cambia el método ValidatePinAsync a esto:
        public async Task<Usuario> ValidatePinAsync(string email, string pinText)
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                // Traemos todos los datos útiles del usuario
                string query = @"SELECT u.id_usuario, u.nombre, u.apellido_paterno, u.apellido_materno, u.email, u.contrasena, 
                                u.RFC, u.CURP, u.id_rol, u.id_tienda, r.nombre_rol 
                         FROM usuario u
                         INNER JOIN roles_usuario r ON u.id_rol = r.id_rol
                         WHERE u.email = @email AND u.activo = 1";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@email", email);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            string dbHash = reader.GetString("contrasena");

                            if (BCrypt.Net.BCrypt.Verify(pinText, dbHash))
                            {
                                // Si la contraseña es correcta, construimos y retornamos el Modelo
                                return new Usuario
                                {
                                    IdUsuario = reader.GetInt32("id_usuario"),
                                    Nombre = reader.GetString("nombre"),
                                    APaterno = reader.GetString("apellido_paterno"),
                                    AMaterno = reader.GetString("apellido_materno"),
                                    Email = reader.GetString("email"),
                                    RFC = reader.IsDBNull(reader.GetOrdinal("RFC")) ? string.Empty : reader.GetString("RFC"),
                                    CURP = reader.IsDBNull(reader.GetOrdinal("CURP")) ? string.Empty : reader.GetString("CURP"),
                                    IdRol = reader.GetInt32("id_rol"),
                                    IdTienda = reader.GetInt32("id_tienda"),
                                    NombreRol = reader.GetString("nombre_rol")
                                };
                            }
                        }
                    }
                }
            }
            return null; // Si falla la validación, devolvemos nulo
        }
    }
}