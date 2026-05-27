using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using MySql.Data.MySqlClient;
using VabelMitienditaEsc.Models;

namespace VabelMitienditaEsc.Services
{
    public class VentasService
    {
        private readonly string _connectionString;

        // INYECCIÓN DE LA CADENA DE CONEXIÓN
        public VentasService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<int> RegistrarVentaAsync(decimal total, decimal impuestos, IEnumerable<Producto> productosCarrito, int idTienda, int idUsuario, int idFormaPago)
        {
            int idVentaGenerada = 0;

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (MySqlTransaction transaction = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        // 1. Insertar la Cabecera de la Venta (Alineado a tu DB vabel_db normalizada)
                        string queryVenta = @"
                            INSERT INTO ventas (porcentaje_iva, observaciones, id_usuario, id_estatus, id_forma_pago, id_tienda) 
                            VALUES (@impuestos, 'Venta de mostrador', @idUsuario, 1, @idFormaPago, @idTienda);
                            SELECT LAST_INSERT_ID();";

                        using (MySqlCommand cmdVenta = new MySqlCommand(queryVenta, conn, transaction))
                        {
                            cmdVenta.Parameters.AddWithValue("@impuestos", impuestos);
                            cmdVenta.Parameters.AddWithValue("@idUsuario", idUsuario);
                            cmdVenta.Parameters.AddWithValue("@idFormaPago", idFormaPago);
                            cmdVenta.Parameters.AddWithValue("@idTienda", idTienda);
                            idVentaGenerada = Convert.ToInt32(await cmdVenta.ExecuteScalarAsync());
                        }

                        // 2. Insertar Detalle y Descontar Stock
                        foreach (var item in productosCarrito)
                        {
                            string queryDetalle = @"
                                INSERT INTO detalle_ventas (id_venta, id_producto, cantidad, precio_unitario) 
                                VALUES (@idVenta, @idProducto, @cantidad, @precioUnitario)";

                            using (MySqlCommand cmdDetalle = new MySqlCommand(queryDetalle, conn, transaction))
                            {
                                cmdDetalle.Parameters.AddWithValue("@idVenta", idVentaGenerada);
                                cmdDetalle.Parameters.AddWithValue("@idProducto", item.ProductoId);
                                cmdDetalle.Parameters.AddWithValue("@precioUnitario", item.PrecioVenta);
                                cmdDetalle.Parameters.AddWithValue("@cantidad", item.CantidadCarrito);
                                await cmdDetalle.ExecuteNonQueryAsync();
                            }

                            string queryUpdateStock = @"
                                UPDATE inventario_producto 
                                SET stock_actual = stock_actual - @cantidad 
                                WHERE id_producto = @idProducto AND id_tienda = @idTienda";

                            using (MySqlCommand cmdUpdate = new MySqlCommand(queryUpdateStock, conn, transaction))
                            {
                                cmdUpdate.Parameters.AddWithValue("@cantidad", item.CantidadCarrito);
                                cmdUpdate.Parameters.AddWithValue("@idProducto", item.ProductoId);
                                cmdUpdate.Parameters.AddWithValue("@idTienda", idTienda);
                                await cmdUpdate.ExecuteNonQueryAsync();
                            }
                        }

                        await transaction.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        MessageBox.Show("Error al registrar la venta en la base de datos: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        throw;
                    }
                }
            }
            return idVentaGenerada;
        }
    }
}