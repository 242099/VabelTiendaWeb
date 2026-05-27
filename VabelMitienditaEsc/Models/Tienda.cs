using System;

namespace VabelMitienditaEsc.Models
{
    public class Tienda
    {
        public int IdTienda { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public bool Activa { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}