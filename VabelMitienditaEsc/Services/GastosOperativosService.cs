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
                        cmd.Parameters.AddWithValue("@observaciones", gasto.observaciones);
                        cmd.Parameters.AddWithValue("@id_usuario", gasto.idUsuario);
                        cmd.Parameters.AddWithValue("@id_proveedor", gasto.idProveedor);
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
                               WHERE id_gastos = @idGastos";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@fecha", gasto.fecha);
                        cmd.Parameters.AddWithValue("@descripcion", gasto.descripcion);
                        cmd.Parameters.AddWithValue("@monto", gasto.monto);
                        cmd.Parameters.AddWithValue("@tasa_iva", gasto.tasaIVA);
                        cmd.Parameters.AddWithValue("@observaciones", gasto.observaciones);
                        cmd.Parameters.AddWithValue("@id_usuario", gasto.idUsuario);
                        cmd.Parameters.AddWithValue("@id_proveedor", gasto.idProveedor);
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

        public async Task<bool> BorrarGasto(int idGasto)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    string query = @"DELETE FROM gastos_operativos WHERE id_gastos = @idGastos";

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
                    string query = @"SELECT id_gastos, fecha, descripcion, monto, 
                               CONCAT_WS(' ', usuario.nombre, usuario.aPaterno, usuario.aMaterno) AS nombre_usuario, 
                               proveedor.nombre_empresa, formas_pago.nombre
                               FROM gastos_operativos 
                               JOIN usuario ON id_usuario = usuario.id_usuario  
                               JOIN proveedor ON id_proveedor = proveedor.id_proveedor 
                               JOIN formas_pago ON id_forma_pago = formas_pago.id_forma_pago 
                               WHERE id_tienda = @idTienda
                               ORDER BY fecha DESC";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@idTienda", idTienda);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                vistaGastos.Add(new VistaGastoOperativo
                                {
                                    idGastos = reader.GetInt32("id_gastos"),
                                    fecha = reader.GetDateTime("fecha"),
                                    descripcion = reader.GetString("descripcion"),
                                    monto = reader.GetDecimal("monto"),
                                    nomUsuario = reader.GetString("nombre_usuario"),
                                    nombreEmpresaProveedor = reader.GetString("nombre_empresa"),
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

        public async Task<bool> BuscarGastoPorFecha(DateTime fechaInicial, DateTime fechaFinal)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    string query = @"SELECT * FROM gastos_operativos WHERE fecha BETWEEN @fechaInicial AND @fechaFinal";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@fechaInicial", fechaInicial);
                        cmd.Parameters.AddWithValue("@fechaFinal", fechaFinal);
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
