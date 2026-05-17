using System;

namespace VabelMitienditaEsc.Models
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public string Nombre { get; set; }
        public string APaterno { get; set; }
        public string AMaterno { get; set; }
        public string Email { get; set; }
        public string RFC { get; set; }
        public string CURP { get; set; }
        // Nota: Por seguridad, en memoria rara vez guardamos la contraseña, 
        // así que omitimos esa propiedad en el Modelo mientras no la necesitemos.
    }
}