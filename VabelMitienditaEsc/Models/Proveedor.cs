using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VabelMitienditaEsc.Models
{
    public class Proveedor
    {
        public int idProveedor { get; set; }
        public string nombreEmpresa { get; set; }
        public string nombre { get; set; }
        public string aPaterno { get; set; }
        public string aMaterno { get; set; }
        public string telefono { get; set; }
        public string? email { get; set; }
        public string rfc { get; set; }
        public string? calle { get; set; }
        public string? numero { get; set; }
        public string? ciudad { get; set; }
        public DateOnly fechaRegistro { get; set; }
        public override string ToString()
        {
            return $"Proveedor [idProveedor={idProveedor}, nombreEmpresa={nombreEmpresa}, nombre={nombre} {aPaterno} {aMaterno}, telefono={telefono}, email={email}, rfc={rfc}, calle={calle}, numero={numero}, ciudad={ciudad}, fechaRegistro={fechaRegistro}]";
        }
    }
}
