using MySql.Data.MySqlClient;
using System.Data;
using VabelMitienditaEsc.Models;

namespace VabelMitienditaEsc.Services
{
    public class InventarioService
    {
        private readonly string _connectionString;

        public InventarioService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<List<Producto>> GetProductosAsync(int idTienda)
        {
            List<Producto> lista = new List<Producto>();
            string query = @"
                SELECT 
                    p.id_producto, p.codigo, p.nombre, p.descripcion, p.marca,
                    p.precio_compra, p.precio_venta, p.activo, p.fecha_registro,
                    p.id_categoria, c.nombre as categoria_nombre,
                    i.stock_actual, i.stock_minimo, i.ubicacion
                FROM productos p
                INNER JOIN categoria c ON p.id_categoria = c.id_categoria
                INNER JOIN inventario_producto i ON p.id_producto = i.id_producto
                WHERE p.activo = 1 AND i.id_tienda = @idTienda";

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@idTienda", idTienda);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            lista.Add(new Producto
                            {
                                ProductoId = reader.GetInt32("id_producto"),
                                CodigoBarra = reader.GetString("codigo"),
                                Nombre = reader.GetString("nombre"),
                                Descripcion = reader.GetString("descripcion"),
                                Marca = reader.GetString("marca"),
                                PrecioCompra = reader.GetDecimal("precio_compra"),
                                PrecioVenta = reader.GetDecimal("precio_venta"),
                                Activo = reader.GetBoolean("activo"),
                                FechaRegistro = reader.GetDateTime("fecha_registro"),
                                CategoriaId = reader.GetInt32("id_categoria"),
                                CategoriaNombre = reader.GetString("categoria_nombre"),
                                StockActual = reader.GetInt32("stock_actual"),
                                StockMinimo = reader.GetInt32("stock_minimo"),
                                Ubicacion = reader.GetString("ubicacion")
                            });
                        }
                    }
                }
            }
            return lista;
        }

        public async Task<Producto> InsertProducto(Producto producto, int idTienda)
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var tran = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        string insertProducto = @"
                            INSERT INTO productos (codigo, nombre, descripcion, marca, precio_compra, precio_venta, activo, fecha_registro, id_categoria)
                            VALUES (@codigo, @nombre, @descripcion, @marca, @precio_compra, @precio_venta, 1, NOW(), @id_categoria);
                            SELECT LAST_INSERT_ID();";

                        long newId = 0;
                        using (MySqlCommand cmd = new MySqlCommand(insertProducto, conn, (MySqlTransaction)tran))
                        {
                            cmd.Parameters.AddWithValue("@codigo", producto.CodigoBarra ?? string.Empty);
                            cmd.Parameters.AddWithValue("@nombre", producto.Nombre ?? string.Empty);
                            cmd.Parameters.AddWithValue("@descripcion", producto.Descripcion ?? string.Empty);
                            cmd.Parameters.AddWithValue("@marca", producto.Marca ?? string.Empty);
                            cmd.Parameters.AddWithValue("@precio_compra", producto.PrecioCompra);
                            cmd.Parameters.AddWithValue("@precio_venta", producto.PrecioVenta);
                            cmd.Parameters.AddWithValue("@id_categoria", producto.CategoriaId);

                            var result = await cmd.ExecuteScalarAsync();
                            newId = Convert.ToInt64(result);
                        }

                        string insertInventario = @"
                            INSERT INTO inventario_producto (id_tienda, id_producto, stock_actual, stock_minimo, ubicacion)
                            VALUES (@idTienda, @idProducto, @stock_actual, @stock_minimo, @ubicacion);";

                        using (MySqlCommand cmd2 = new MySqlCommand(insertInventario, conn, (MySqlTransaction)tran))
                        {
                            cmd2.Parameters.AddWithValue("@idTienda", idTienda);
                            cmd2.Parameters.AddWithValue("@idProducto", newId);
                            cmd2.Parameters.AddWithValue("@stock_actual", producto.StockActual);
                            cmd2.Parameters.AddWithValue("@stock_minimo", producto.StockMinimo);
                            cmd2.Parameters.AddWithValue("@ubicacion", producto.Ubicacion ?? string.Empty);

                            await cmd2.ExecuteNonQueryAsync();
                        }

                        await tran.CommitAsync();

                        producto.ProductoId = (int)newId;
                        producto.FechaRegistro = DateTime.Now;
                        producto.Activo = true;

                        return producto;
                    }
                    catch
                    {
                        await tran.RollbackAsync();
                        throw;
                    }
                }
            }
        }

        public async Task<bool> ActualizarProducto(Producto producto, int idTienda)
        {
            string queryProducto = @"
                UPDATE productos 
                SET codigo = @codigo,
                    nombre = @nombre,
                    descripcion = @descripcion,
                    marca = @marca,
                    precio_compra = @precioCompra,
                    precio_venta = @precioVenta,
                    id_categoria = @idCategoria
                WHERE id_producto = @idProducto;";

            string queryInventario = @"
                UPDATE inventario_producto 
                SET stock_actual = @stockActual,
                    stock_minimo = @stockMinimo,
                    ubicacion = @ubicacion
                WHERE id_producto = @idProducto AND id_tienda = @idTienda;";

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (MySqlTransaction trans = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        using (MySqlCommand cmdProd = new MySqlCommand(queryProducto, conn, trans))
                        {
                            cmdProd.Parameters.AddWithValue("@idProducto", producto.ProductoId);
                            cmdProd.Parameters.AddWithValue("@codigo", producto.CodigoBarra);
                            cmdProd.Parameters.AddWithValue("@nombre", producto.Nombre);
                            cmdProd.Parameters.AddWithValue("@descripcion", producto.Descripcion ?? (object)DBNull.Value);
                            cmdProd.Parameters.AddWithValue("@marca", producto.Marca ?? (object)DBNull.Value);
                            cmdProd.Parameters.AddWithValue("@precioCompra", producto.PrecioCompra);
                            cmdProd.Parameters.AddWithValue("@precioVenta", producto.PrecioVenta);
                            cmdProd.Parameters.AddWithValue("@idCategoria", producto.CategoriaId);

                            await cmdProd.ExecuteNonQueryAsync();
                        }

                        using (MySqlCommand cmdInv = new MySqlCommand(queryInventario, conn, trans))
                        {
                            cmdInv.Parameters.AddWithValue("@idProducto", producto.ProductoId);
                            cmdInv.Parameters.AddWithValue("@idTienda", idTienda);
                            cmdInv.Parameters.AddWithValue("@stockActual", producto.StockActual);
                            cmdInv.Parameters.AddWithValue("@stockMinimo", producto.StockMinimo);
                            cmdInv.Parameters.AddWithValue("@ubicacion", producto.Ubicacion ?? (object)DBNull.Value);

                            await cmdInv.ExecuteNonQueryAsync();
                        }

                        await trans.CommitAsync();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        await trans.RollbackAsync();
                        System.Diagnostics.Debug.WriteLine($"Error Actualizar: {ex.Message}");
                        throw;
                    }
                }
            }
        }

        public async Task EliminarProducto(int productoId, int idTienda)
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var tran = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        string updateProducto = "UPDATE productos SET activo = 0 WHERE id_producto = @idProducto";
                        using (MySqlCommand cmd = new MySqlCommand(updateProducto, conn, (MySqlTransaction)tran))
                        {
                            cmd.Parameters.AddWithValue("@idProducto", productoId);
                            await cmd.ExecuteNonQueryAsync();
                        }

                        string deleteInventario = "DELETE FROM inventario_producto WHERE id_producto = @idProducto AND id_tienda = @idTienda";
                        using (MySqlCommand cmd2 = new MySqlCommand(deleteInventario, conn, (MySqlTransaction)tran))
                        {
                            cmd2.Parameters.AddWithValue("@idProducto", productoId);
                            cmd2.Parameters.AddWithValue("@idTienda", idTienda);
                            await cmd2.ExecuteNonQueryAsync();
                        }

                        await tran.CommitAsync();
                    }
                    catch
                    {
                        await tran.RollbackAsync();
                        throw;
                    }
                }
            }
        }

        public async Task<List<Categoria>> GetCategoriasAsync()
        {
            var lista = new List<Categoria>();
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string query = "SELECT id_categoria, nombre, descripcion FROM categoria";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        lista.Add(new Categoria
                        {
                            IdCategoria = reader.GetInt32("id_categoria"),
                            Nombre = reader.GetString("nombre"),
                            Descripcion = reader.GetString("descripcion")
                        });
                    }
                }
            }
            return lista;
        }
    }
}