using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public async Task<bool> Insertar(FormasPago formaP)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    string query = @"INSERT INTO formas_pago (nombre) VALUES (@nombre)";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@nombre", formaP.nombre);
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

        public async Task<bool> Actualizar(FormasPago formaP)
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
                        cmd.Parameters.AddWithValue("@nom", formaP.nombre);
                        cmd.Parameters.AddWithValue("@id", formaP.idFormaPago);

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

        public async Task<List<FormasPago>> Listar()
        {
            List<FormasPago> vista = new List<FormasPago>();

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
                                vista.Add(new FormasPago
                                {
                                    idFormaPago = reader.GetInt32("id_forma_pago"),
                                    nombre = reader.GetString("nombre")
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
