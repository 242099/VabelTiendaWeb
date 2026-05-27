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
    public class GastosOperativosService
    {
        private readonly string _connectionString;

        public GastosOperativosService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<bool> InsertarGasto(GastoOperativo gasto)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    string query = @"INSERT INTO gastos_operativos 
                               (fecha, descripcion, monto, tasa_iva, observaciones, id_usuario, id_proveedor, id_cuenta, id_forma_pago, id_tienda)
                               VALUES (@fecha, @descripcion, @monto, @tasa_iva, @observaciones, @id_usuario, @id_proveedor, @id_cuenta, @id_forma_pago, @id_tienda)";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@fecha", gasto.fecha);
                        cmd.Parameters.AddWithValue("@descripcion", gasto.descripcion);
                        cmd.Parameters.AddWithValue("@monto", gasto.monto);
                        cmd.Parameters.AddWithValue("@tasa_iva", gasto.tasaIVA);
                        cmd.Parameters.AddWithValue("@observaciones", (Object)gasto.observaciones ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@id_usuario", gasto.idUsuario);
                        cmd.Parameters.AddWithValue("@id_proveedor", (object)gasto.idProveedor ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@id_cuenta", gasto.idCuenta);
                        cmd.Parameters.AddWithValue("@id_forma_pago", gasto.idFormaPago);
                        cmd.Parameters.AddWithValue("@id_tienda", gasto.idTienda);

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

        public async Task<bool> ActualizarGasto(GastoOperativo gasto)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    string query = @"UPDATE gastos_operativos 
                               SET fecha = @fecha,
                                   descripcion = @descripcion,
                                   monto = @monto, 
                                   tasa_iva = @tasa_iva, 
                                   observaciones = @observaciones, 
                                   id_usuario = @id_usuario, 
                                   id_proveedor = @id_proveedor, 
                                   id_cuenta = @id_cuenta, 
                                   id_forma_pago = @id_forma_pago, 
                                   id_tienda = @id_tienda
                               WHERE id_gasto = @id_gasto";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@fecha", gasto.fecha);
                        cmd.Parameters.AddWithValue("@descripcion", gasto.descripcion);
                        cmd.Parameters.AddWithValue("@monto", gasto.monto);
                        cmd.Parameters.AddWithValue("@tasa_iva", gasto.tasaIVA);
                        cmd.Parameters.AddWithValue("@observaciones", (object)gasto.observaciones ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@id_usuario", gasto.idUsuario);
                        cmd.Parameters.AddWithValue("@id_proveedor", (object)gasto.idProveedor ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@id_cuenta", gasto.idCuenta);
                        cmd.Parameters.AddWithValue("@id_forma_pago", gasto.idFormaPago);
                        cmd.Parameters.AddWithValue("@id_tienda", gasto.idTienda);
                        cmd.Parameters.AddWithValue("@id_gasto", gasto.idGastos);

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

        public async Task<GastoOperativo> ObtenerGastoPorId(int idGasto)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    string query = @"SELECT * FROM gastos_operativos WHERE id_gasto = @idGasto";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@idGasto", idGasto);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return new GastoOperativo
                                {
                                    idGastos = reader.GetInt32("id_gasto"),
                                    fecha = reader.GetDateTime("fecha"),
                                    descripcion = reader.GetString("descripcion"),
                                    monto = reader.GetDecimal("monto"),
                                    tasaIVA = reader.GetDecimal("tasa_iva"),
                                    observaciones = reader.IsDBNull(reader.GetOrdinal("observaciones")) ? null : reader.GetString("observaciones"),
                                    idUsuario = reader.GetInt32("id_usuario"),
                                    idProveedor = reader.IsDBNull(reader.GetOrdinal("id_proveedor")) ? (int?)null : reader.GetInt32("id_proveedor"),
                                    idCuenta = reader.GetInt32("id_cuenta"),
                                    idFormaPago = reader.GetInt32("id_forma_pago"),
                                    idTienda = reader.GetInt32("id_tienda")
                                };
                            }
                        }
                    }
                }

                return null;
            }
            catch (MySqlException e)
            {
                MessageBox.Show(e.Message);
                return null;
            }
        }

        public async Task<bool> BorrarGasto(int idGasto)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    string query = @"DELETE FROM gastos_operativos WHERE id_gasto = @idGastos";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@idGastos", idGasto);
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

        public async Task<List<VistaGastoOperativo>> ListarGastos(int idTienda)
        {
            List<VistaGastoOperativo> vistaGastos = new List<VistaGastoOperativo>();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    string query = @"SELECT go.id_gasto, 
                                   go.fecha, 
                                   go.descripcion, 
                                   go.monto, 
                                   CONCAT_WS(' ', u.nombre, u.apellido_paterno, u.apellido_materno) AS nombre_usuario, 
                                   COALESCE(p.nombre_empresa, 'Sin proveedor') AS nombre_empresa, 
                                   fp.nombre
                            FROM gastos_operativos go
                            JOIN usuario u ON go.id_usuario = u.id_usuario  
                            LEFT JOIN proveedor p ON go.id_proveedor = p.id_proveedor 
                            JOIN formas_pago fp ON go.id_forma_pago = fp.id_forma_pago 
                            WHERE go.id_tienda = @idTienda
                            ORDER BY go.fecha DESC";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@idTienda", idTienda);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                vistaGastos.Add(new VistaGastoOperativo
                                {
                                    idGastos = reader.GetInt32("id_gasto"),
                                    fecha = reader.GetDateTime("fecha"),
                                    descripcion = reader.GetString("descripcion"),
                                    monto = reader.GetDecimal("monto"),
                                    nomUsuario = reader.GetString("nombre_usuario"),
                                    nombreEmpresaProveedor = reader.IsDBNull(reader.GetOrdinal("nombre_empresa")) ? "Sin proveedor" : reader.GetString("nombre_empresa"),
                                    formaPago = reader.GetString("nombre")
                                });
                            }
                        }
                    }
                }

                return vistaGastos;
            }
            catch (MySqlException e)
            {
                MessageBox.Show(e.Message);
                return null;
            }
        }

        public async Task<List<VistaGastoOperativo>> BuscarGastoPorFecha(DateTime fechaInicial, DateTime fechaFinal, int idTienda)
        {
            List<VistaGastoOperativo> vistaGastos = new List<VistaGastoOperativo>();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    string query = @"SELECT go.id_gasto, 
                                   go.fecha, 
                                   go.descripcion, 
                                   go.monto, 
                                   CONCAT_WS(' ', u.nombre, u.apellido_paterno, u.apellido_materno) AS nombre_usuario, 
                                   COALESCE(p.nombre_empresa, 'Sin proveedor') AS nombre_empresa, 
                                   fp.nombre
                            FROM gastos_operativos go
                            JOIN usuario u ON go.id_usuario = u.id_usuario  
                            LEFT JOIN proveedor p ON go.id_proveedor = p.id_proveedor 
                            JOIN formas_pago fp ON go.id_forma_pago = fp.id_forma_pago 
                            WHERE go.fecha BETWEEN @fechaInicial AND @fechaFinal 
                            AND go.id_tienda = @idTienda
                            ORDER BY go.fecha DESC";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@fechaInicial", fechaInicial);
                        cmd.Parameters.AddWithValue("@fechaFinal", fechaFinal);
                        cmd.Parameters.AddWithValue("@idTienda", idTienda);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                vistaGastos.Add(new VistaGastoOperativo
                                {
                                    idGastos = reader.GetInt32("id_gasto"),
                                    fecha = reader.GetDateTime("fecha"),
                                    descripcion = reader.GetString("descripcion"),
                                    monto = reader.GetDecimal("monto"),
                                    nomUsuario = reader.GetString("nombre_usuario"),
                                    nombreEmpresaProveedor = reader.IsDBNull(reader.GetOrdinal("nombre_empresa")) ? "Sin proveedor" : reader.GetString("nombre_empresa"),
                                    formaPago = reader.GetString("nombre")
                                });
                            }
                        }
                    }
                }

                return vistaGastos;
            }
            catch (MySqlException e)
            {
                MessageBox.Show(e.Message);
                return null;
            }
        }
    }
}
