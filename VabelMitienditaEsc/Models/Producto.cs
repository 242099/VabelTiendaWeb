using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace VabelMitienditaEsc.Models
{
    // Heredamos de ObservableObject para poder inyectar reactividad a la interfaz gráfica
    public partial class Producto : ObservableObject
    {
        // ==========================================
        // CAMPOS DE BASE DE DATOS (Tabla: productos)
        // ==========================================
        public int ProductoId { get; set; }
        public string CodigoBarra { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Marca { get; set; }
        public decimal PrecioCompra { get; set; }
        public decimal PrecioVenta { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaRegistro { get; set; }

        // ==========================================
        // RELACIONES (Tabla: categoria)
        // ==========================================
        public int CategoriaId { get; set; }
        public string CategoriaNombre { get; set; }

        // ==========================================
        // DATOS DE INVENTARIO (Tabla: inventario_producto)
        // ==========================================
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
        public string Ubicacion { get; set; }

        // ==========================================
        // ESTADOS DE INTERFAZ GRÁFICA (UI - Carrito)
        // ==========================================

        // Al usar [ObservableProperty], el toolkit genera "public int CantidadCarrito { get; set; }"
        // [NotifyPropertyChangedFor] avisa a la UI que TotalNeto también cambió y debe repintarse.
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TotalNeto))]
        private int _cantidadCarrito;

        // Propiedad calculada: Se actualiza sola en la tabla cuando CantidadCarrito cambia
        public decimal TotalNeto => CantidadCarrito * PrecioVenta;

        // Indicador de alertas
        public bool RequiereResurtido => StockActual <= StockMinimo;
    }
}