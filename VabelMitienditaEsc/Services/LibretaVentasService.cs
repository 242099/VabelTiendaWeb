using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using VabelMitienditaEsc.Models;

namespace VabelMitienditaEsc.Services
{
    public class LibretaVentasService
    {
        private readonly string _connectionString;

        public LibretaVentasService(string connectionString)
        {
            _connectionString = connectionString;
        }

        // 1. OBTENER TOTALES DE GANANCIAS Y GASTOS (CALCULADOS DESDE EL DETALLE)
        public async Task<(decimal ganancias, decimal gastos)> GetTotalesFinancierosAsync(DateTime? inicio, DateTime? fin)
        {
            decimal ganancias = 0;
            decimal gastos = 0;

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                // 1.1 Ganancias = Suma de (cantidad * precio_unitario) en Ventas
                string queryVentas = @"SELECT COALESCE(SUM(dv.cantidad * dv.precio_unitario), 0) 
                                       FROM ventas v
                                       INNER JOIN detalle_ventas dv ON v.id_venta = dv.id_venta
                                       WHERE 1=1";
                if (inicio.HasValue) queryVentas += " AND v.fecha >= @inicio";
                if (fin.HasValue) queryVentas += " AND v.fecha <= @fin";

                using (MySqlCommand cmd = new MySqlCommand(queryVentas, conn))
                {
                    if (inicio.HasValue) cmd.Parameters.AddWithValue("@inicio", inicio.Value);
                    if (fin.HasValue) cmd.Parameters.AddWithValue("@fin", fin.Value);
                    ganancias = Convert.ToDecimal(await cmd.ExecuteScalarAsync());
                }

                // 1.2 Gastos = Compras de Mercancía + Gastos Operativos
                string queryCompras = @"SELECT COALESCE(SUM(dc.cantidad * dc.precio_unitario), 0) 
                                        FROM compras c
                                        INNER JOIN detalle_compras dc ON c.id_compra = dc.id_compra
                                        WHERE 1=1";
                if (inicio.HasValue) queryCompras += " AND c.fecha >= @inicio";
                if (fin.HasValue) queryCompras += " AND c.fecha <= @fin";

                decimal totalCompras = 0;
                using (MySqlCommand cmd = new MySqlCommand(queryCompras, conn))
                {
                    if (inicio.HasValue) cmd.Parameters.AddWithValue("@inicio", inicio.Value);
                    if (fin.HasValue) cmd.Parameters.AddWithValue("@fin", fin.Value);
                    totalCompras = Convert.ToDecimal(await cmd.ExecuteScalarAsync());
                }

                string queryGastosOp = @"SELECT COALESCE(SUM(monto), 0) 
                                         FROM gastos_operativos 
                                         WHERE 1=1";
                if (inicio.HasValue) queryGastosOp += " AND fecha >= @inicio";
                if (fin.HasValue) queryGastosOp += " AND fecha <= @fin";

                decimal totalGastosOp = 0;
                using (MySqlCommand cmd = new MySqlCommand(queryGastosOp, conn))
                {
                    if (inicio.HasValue) cmd.Parameters.AddWithValue("@inicio", inicio.Value);
                    if (fin.HasValue) cmd.Parameters.AddWithValue("@fin", fin.Value);
                    totalGastosOp = Convert.ToDecimal(await cmd.ExecuteScalarAsync());
                }

                gastos = totalCompras + totalGastosOp;
            }

            return (ganancias, gastos);
        }

        // 2. OBTENER EL TOP 3 DE PRODUCTOS MÁS VENDIDOS
        public async Task<List<TopProducto>> GetTopProductosAsync()
        {
            List<TopProducto> lista = new();
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string query = @"SELECT p.nombre AS NombreProducto, COALESCE(SUM(dv.cantidad), 0) AS CantidadVendida 
                                 FROM detalle_ventas dv
                                 INNER JOIN productos p ON dv.id_producto = p.id_producto
                                 GROUP BY p.id_producto, p.nombre
                                 ORDER BY CantidadVendida DESC
                                 LIMIT 3";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    int posicion = 1;
                    while (await reader.ReadAsync())
                    {
                        lista.Add(new TopProducto
                        {
                            Posicion = posicion++,
                            NombreProducto = reader.GetString("NombreProducto"),
                            CantidadVendida = reader.GetInt32("CantidadVendida")
                        });
                    }
                }
            }
            return lista;
        }

        // 3. OBTENER HISTORIAL DE VENTAS (Calculando el total con subconsulta)
        public async Task<List<TransaccionHistorial>> GetHistorialVentasAsync(DateTime? inicio, DateTime? fin)
        {
            List<TransaccionHistorial> lista = new();
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                // Mapeamos 'observaciones' como Concepto, y calculamos el total al vuelo
                string query = @"SELECT v.id_venta, v.fecha, v.observaciones, fp.nombre,
                                        (SELECT COALESCE(SUM(cantidad * precio_unitario), 0) FROM detalle_ventas WHERE id_venta = v.id_venta) AS total_calculado
                                 FROM ventas v
                                 INNER JOIN formas_pago fp ON v.id_forma_pago = fp.id_forma_pago
                                 WHERE 1=1";

                if (inicio.HasValue) query += " AND v.fecha >= @inicio";
                if (fin.HasValue) query += " AND v.fecha <= @fin";
                query += " ORDER BY v.fecha DESC";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    if (inicio.HasValue) cmd.Parameters.AddWithValue("@inicio", inicio.Value);
                    if (fin.HasValue) cmd.Parameters.AddWithValue("@fin", fin.Value);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            lista.Add(new TransaccionHistorial
                            {
                                Id = reader.GetInt32("id_venta"),
                                Fecha = reader.GetDateTime("fecha"),
                                Tipo = "Venta",
                                Concepto = reader.IsDBNull(reader.GetOrdinal("observaciones")) ? "Venta General" : reader.GetString("observaciones"),
                                Monto = reader.GetDecimal("total_calculado"),
                                MetodoPago = reader.GetString("nombre"),
                                EsVenta = true,
                                Icono = "\ue8a1"
                            });
                        }
                    }
                }
            }
            return lista;
        }

        // 4. OBTENER HISTORIAL DE COMPRAS Y GASTOS (Unión de ambas tablas)
        public async Task<List<TransaccionHistorial>> GetHistorialGastosAsync(DateTime? inicio, DateTime? fin)
        {
            List<TransaccionHistorial> lista = new();
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                // Unimos con UNION ALL las compras (cálculo de detalle) y los gastos operativos (monto directo)
                string query = @"
                    SELECT id, fecha, observaciones, nombre, total_calculado, tipo FROM (
                        SELECT c.id_compra AS id, c.fecha, c.observaciones, fp.nombre,
                               (SELECT COALESCE(SUM(cantidad * precio_unitario), 0) FROM detalle_compras WHERE id_compra = c.id_compra) AS total_calculado,
                               'Compra Stock' AS tipo
                        FROM compras c
                        INNER JOIN formas_pago fp ON c.id_forma_pago = fp.id_forma_pago
                        
                        UNION ALL
                        
                        SELECT g.id_gasto AS id, g.fecha, g.descripcion AS observaciones, fp.nombre,
                               g.monto AS total_calculado,
                               'Gasto Operativo' AS tipo
                        FROM gastos_operativos g
                        INNER JOIN formas_pago fp ON g.id_forma_pago = fp.id_forma_pago
                    ) AS HistorialUnido
                    WHERE 1=1";

                if (inicio.HasValue) query += " AND fecha >= @inicio";
                if (fin.HasValue) query += " AND fecha <= @fin";
                query += " ORDER BY fecha DESC";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    if (inicio.HasValue) cmd.Parameters.AddWithValue("@inicio", inicio.Value);
                    if (fin.HasValue) cmd.Parameters.AddWithValue("@fin", fin.Value);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            lista.Add(new TransaccionHistorial
                            {
                                Id = reader.GetInt32("id"),
                                Fecha = reader.GetDateTime("fecha"),
                                Tipo = reader.GetString("tipo"),
                                Concepto = reader.IsDBNull(reader.GetOrdinal("observaciones")) ? "Sin descripción" : reader.GetString("observaciones"),
                                Monto = reader.GetDecimal("total_calculado"),
                                MetodoPago = reader.GetString("nombre"),
                                EsVenta = false,
                                Icono = "\uea14"
                            });
                        }
                    }
                }
            }
            return lista;
        }

        // 5. OBTENER CATÁLOGO DE MÉTODOS DE PAGO
        public async Task<List<TransaccionHistorial>> GetMetodosPagoAsync()
        {
            List<TransaccionHistorial> lista = new();
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string query = "SELECT id_forma_pago, nombre FROM formas_pago ORDER BY id_forma_pago ASC";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        lista.Add(new TransaccionHistorial
                        {
                            Id = reader.GetInt32("id_forma_pago"),
                            Concepto = reader.GetString("nombre"),
                            MetodoPago = "Activo",
                            Icono = "\ue8a1"
                        });
                    }
                }
            }
            return lista;
        }

        // 6. INSERTAR NUEVO MÉTODO DE PAGO
        public async Task InsertMetodoPagoAsync(string nombreForma)
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string query = "INSERT INTO formas_pago (nombre) VALUES (@nombre)";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@nombre", nombreForma);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        // Se anexa este método a la clase existente para recuperar las formas de pago de la base de datos
        // Este modelo exclusivamente devuelve una lista de objetos FormaPago, sin mapear a TransaccionHistorial, para usos específicos en la UI o lógica de negocio
        public async Task<List<FormaPago>> GetFormasPagoAsync()
        {
            List<FormaPago> lista = new List<FormaPago>();
            string query = "SELECT id_forma_pago, nombre FROM formas_pago ORDER BY id_forma_pago ASC";

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            lista.Add(new FormaPago
                            {
                                IdFormaPago = reader.GetInt32("id_forma_pago"),
                                Nombre = reader.GetString("nombre")
                            });
                        }
                    }
                }
            }
            return lista;
        }
    }
}