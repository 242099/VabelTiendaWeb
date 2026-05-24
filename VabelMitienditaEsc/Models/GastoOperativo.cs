using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VabelMitienditaEsc.Models
{
    public class GastoOperativo
    {
        public int idGastos { get; set; }
        public DateTime fecha { get; set; }
        public string descripcion { get; set; }
        public decimal monto { get; set; }
        public decimal tasaIVA { get; set; }
        public string observaciones { get; set; }
        public int idUsuario { get; set; }
        public int idProveedor { get; set; }
        public int idCuenta { get; set; }
        public int idFormaPago { get; set; }
        public int idTienda { get; set; }
        override public string ToString()
        {
            return $"GastoOperativo [idGastos={idGastos}, fecha={fecha}, descripcion={descripcion}, monto={monto}, tasaIVA={tasaIVA}, observaciones={observaciones}, idUsuario={idUsuario}, idProveedor={idProveedor}, idCuenta={idCuenta}, idFormaPago={idFormaPago}, idTienda={idTienda}]";
        }
    }
}
