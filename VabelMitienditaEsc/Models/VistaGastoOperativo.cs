using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VabelMitienditaEsc.Models
{
    public class VistaGastoOperativo
    {
        public int idGastos { get; set; }
        public DateTime fecha { get; set; }
        public string descripcion { get; set; }
        public decimal monto { get; set; }
        public string nomUsuario { get; set; }
        public string nombreEmpresaProveedor { get; set; }
        public string formaPago { get; set; }
    }
}
