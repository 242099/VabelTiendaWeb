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
    public class CatalogoCuentasService
    {
        private readonly string _connectionString;

        public CatalogoCuentasService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<bool> Insertar(CatalogoCuentas CatCuentas)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    string query = @"INSERT INTO catalogo_cuentas (codigo_cuenta, nombre_cuenta, id_tipo_cuenta, activa) 
                        VALUES (@codigo_cuenta, @nombre, @idTipoCuenta, @activa)";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@codigo_cuenta", CatCuentas.codigoCuenta);
                        cmd.Parameters.AddWithValue("@nombre", CatCuentas.nombreCuenta);
                        cmd.Parameters.AddWithValue("@idTipoCuenta", CatCuentas.idTipoCuenta);
                        cmd.Parameters.AddWithValue("@activa", CatCuentas.activa);

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

        public async Task<bool> Actualizar(CatalogoCuentas CatCuentas)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    string query = @"UPDATE catalogo_cuentas 
                               SET codigo_cuenta = @codigo_cuenta, 
                                   nombre_cuenta = @nombre, 
                                   id_tipo_cuenta = @idTipoCuenta, 
                                   activa = @activa
                                WHERE id_cuenta = @id";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@codigo_cuenta", CatCuentas.codigoCuenta);
                        cmd.Parameters.AddWithValue("@nombre", CatCuentas.nombreCuenta);
                        cmd.Parameters.AddWithValue("@idTipoCuenta", CatCuentas.idTipoCuenta);
                        cmd.Parameters.AddWithValue("@activa", CatCuentas.activa);
                        cmd.Parameters.AddWithValue("@id", CatCuentas.idCuenta);

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
                    string query = @"DELETE FROM catalogo_cuentas WHERE id_cuenta = @id";

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

        public async Task<List<CatalogoCuentas>> Listar()
        {
            List<CatalogoCuentas> vista = new List<CatalogoCuentas>();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    string query = @"SELECT id_cuenta, codigo_cuenta, nombre_cuenta, id_tipo_cuenta, activa
                               FROM catalogo_cuentas 
                               WHERE activa = 1
                               ORDER BY nombre_cuenta ASC";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                vista.Add(new CatalogoCuentas
                                {
                                    idCuenta = reader.GetInt32("id_cuenta"),
                                    codigoCuenta = reader.GetString("codigo_cuenta"),
                                    nombreCuenta = reader.GetString("nombre_cuenta"),
                                    idTipoCuenta = reader.GetInt32("id_tipo_cuenta"),
                                    activa = reader.GetBoolean("activa")
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

        public async Task<bool> Buscar(string cod)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    string query = @"SELECT id_cuenta, codigo_cuenta, nombre_cuenta, tipos_cuenta.nombre_tipo, activa
                               FROM catalogo_cuentas 
                               JOIN tipos_cuenta ON catalogo_cuentas.id_tipo_cuenta = tipos_cuenta.id_tipo_cuenta
                               WHERE codigo_cuenta = @cod";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@cod", cod);
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
