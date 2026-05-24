using System;

namespace VabelMitienditaEsc.Models
{
    public class TransaccionHistorial
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string Tipo { get; set; } // "Venta", "Compra" o "Gasto Operativo"
        public string Concepto { get; set; } // "descripcion" en la base de datos
        public decimal Monto { get; set; }
        public string Observaciones { get; set; }
        public string MetodoPago { get; set; }
        public bool EsVenta { get; set; } // true = Verde (Ingreso), false = Rojo (Egreso)
        public string Icono { get; set; } // Carácter de Material Icons o Glyph
    }
}