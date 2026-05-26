using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using VabelMitienditaEsc.Models;

namespace VabelMitienditaEsc.Services
{
    public class InventarioService
    {
        private readonly string _connectionString;

        // INYECCIÓN DE LA CADENA DE CONEXIÓN
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
    }
}