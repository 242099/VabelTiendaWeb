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
    public class ProveedorService
    {
        private readonly string _connectionString;

        public ProveedorService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<bool> InsertarProveedor(Proveedor prov)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    string query = @"INSERT INTO proveedor 
                               (nombre_empresa, nombre, apaterno, amaterno, telefono, email, rfc, calle, numero, ciudad, fecha_registro)
                               VALUES (@nombre_empresa, @nombre, @apaterno, @amaterno, @telefono, @email, @rfc, @calle, @numero, @ciudad, @fecha_registro)";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@nombre_empresa", prov.nombreEmpresa);
                        cmd.Parameters.AddWithValue("@nombre", prov.nombre);
                        cmd.Parameters.AddWithValue("@apaterno", prov.aPaterno);
                        cmd.Parameters.AddWithValue("@amaterno", prov.aMaterno);
                        cmd.Parameters.AddWithValue("@telefono", prov.telefono);
                        cmd.Parameters.AddWithValue("@email", (object)prov.email ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@rfc", prov.rfc);
                        cmd.Parameters.AddWithValue("@calle", (object)prov.calle ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@numero", (object)prov.numero ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@ciudad", (object)prov.ciudad ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@fecha_registro", prov.fechaRegistro.ToDateTime(TimeOnly.MinValue));
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

        public async Task<bool> ActualizarProveedor(Proveedor prov)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    string query = @"UPDATE proveedor 
                               SET nombre_empresa = @nombre_empresa,
                                   nombre = @nombre,
                                   apaterno = @apaterno, 
                                   amaterno = @amaterno, 
                                   telefono = @telefono, 
                                   email = @email, 
                                   rfc = @rfc, 
                                   calle = @calle, 
                                   numero = @numero, 
                                   ciudad = @ciudad,
                                   fecha_registro = @fecha_registro
                               WHERE id_proveedor = @id_proveedor";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@nombre_empresa", prov.nombreEmpresa);
                        cmd.Parameters.AddWithValue("@nombre", prov.nombre);
                        cmd.Parameters.AddWithValue("@apaterno", prov.aPaterno);
                        cmd.Parameters.AddWithValue("@amaterno", prov.aMaterno);
                        cmd.Parameters.AddWithValue("@telefono", prov.telefono);
                        cmd.Parameters.AddWithValue("@email", (object)prov.email ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@rfc", prov.rfc);
                        cmd.Parameters.AddWithValue("@calle", (object)prov.calle ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@numero", (object)prov.numero ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@ciudad", (object)prov.ciudad ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@fecha_registro", prov.fechaRegistro.ToDateTime(TimeOnly.MinValue));
                        cmd.Parameters.AddWithValue("@id_proveedor", prov.idProveedor);

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

        public async Task<bool> BorrarProveedor(int id)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    string query = @"DELETE FROM proveedor WHERE id_proveedor = @id";

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

        public async Task<List<ProveedorVistaLista>> ListarProveedores()
        {
            List<ProveedorVistaLista> listaProveedores = new List<ProveedorVistaLista>();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    string query = @"SELECT id_proveedor, nombre_empresa, 
                               CONCAT_WS(' ', nombre, apaterno, amaterno) AS nombre_completo, 
                               telefono, email
                               FROM proveedor 
                               ORDER BY nombre_empresa DESC";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                listaProveedores.Add(new ProveedorVistaLista
                                {
                                    idProveedor = reader.GetInt32("id_proveedor"),
                                    nombreEmpresa = reader.GetString("nombre_empresa"),
                                    nombreCompleto = reader.GetString("nombre_completo"),
                                    telefono = reader.GetString("telefono"),
                                    email = reader.IsDBNull(reader.GetOrdinal("email")) ? "Sin email registrado" : reader.GetString("email")
                                });
                            }
                        }
                    }
                }

                return listaProveedores;
            }
            catch (MySqlException e)
            {
                MessageBox.Show(e.Message);
                return null;
            }
        }

        public async Task<bool> BuscarPorEmpresa(string empresa)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    string query = @"SELECT * FROM proveedor WHERE nombre_empresa = @empresa";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@empresa", empresa);
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
