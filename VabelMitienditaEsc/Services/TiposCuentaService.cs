using MySql.Data;
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
    public class TiposCuentaService
    {
        private readonly string _connectionString;

        public TiposCuentaService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<bool> Insertar(TiposCuenta tipoCuenta)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    string query = @"INSERT INTO tipos_cuenta (nombre_tipo) VALUES (@nombreCuenta)";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@nombreCuenta", tipoCuenta.nombreTipo);
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

        public async Task<bool> Actualizar(TiposCuenta tipoCuenta)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    string query = @"UPDATE tipos_cuenta 
                               SET nombre_tipo = @nom
                               WHERE id_tipo_cuenta = @id";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@nom", tipoCuenta.nombreTipo);
                        cmd.Parameters.AddWithValue("@id", tipoCuenta.idTipoCuenta);

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
                    string query = @"DELETE FROM tipos_cuenta WHERE id_tipo_cuenta = @id";

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

        public async Task<List<TiposCuenta>> Listar()
        {
            List<TiposCuenta> vista = new List<TiposCuenta>();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    string query = @"SELECT * FROM tipos_cuenta";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                vista.Add(new TiposCuenta
                                {
                                    idTipoCuenta = reader.GetInt32("id_tipo_cuenta"),
                                    nombreTipo = reader.GetString("nombre_tipo")
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

        public async Task<bool> Buscar(string nom)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    string query = @"SELECT * FROM tipos_cuenta WHERE nombre_tipo = @nom";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@nom", nom);
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
    }
}
