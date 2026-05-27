using MySql.Data.MySqlClient;
using System.Data;
using System.Text;
using System.Windows;
using VabelMitienditaEsc.Models;

namespace VabelMitienditaEsc.Services
{
    public class FormasPagoService
    {
        private readonly string _connectionString;

        public FormasPagoService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<bool> Insertar(FormaPago formaP)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    string query = @"INSERT INTO formas_pago (nombre) VALUES (@nombre)";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@nombre", formaP.Nombre);
                        int result = await cmd.ExecuteNonQueryAsync();
                        return result > 0;
                    }
                }
            }
            catch (MySqlException e)
            {
                MessageBox.Show(e.Message);
                return false;
            }
        }

        public async Task<bool> Actualizar(FormaPago formaP)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    string query = @"UPDATE formas_pago 
                               SET nombre = @nom
                               WHERE id_forma_pago = @id";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@nom", formaP.Nombre);
                        cmd.Parameters.AddWithValue("@id", formaP.IdFormaPago);

                        int result = await cmd.ExecuteNonQueryAsync();
                        return result > 0;
                    }
                }
            }
            catch (MySqlException e)
            {
                MessageBox.Show(e.Message);
                return false;
            }
        }

        public async Task<bool> Borrar(int id)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    string query = @"DELETE FROM formas_pago WHERE id_forma_pago = @id";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        int result = await cmd.ExecuteNonQueryAsync();
                        return result > 0;
                    }
                }
            }
            catch (MySqlException e)
            {
                MessageBox.Show(e.Message);
                return false;
            }
        }

        public async Task<List<FormaPago>> Listar()
        {
            List<FormaPago> vista = new List<FormaPago>();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    string query = @"SELECT * FROM formas_pago";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                vista.Add(new FormaPago
                                {
                                    IdFormaPago = reader.GetInt32("id_forma_pago"),
                                    Nombre = reader.GetString("nombre")
                                });
                            }
                        }
                    }
                }

                return vista;
            }
            catch (MySqlException e)
            {
                MessageBox.Show(e.Message);
                return null;
            }
        }
    }
}
