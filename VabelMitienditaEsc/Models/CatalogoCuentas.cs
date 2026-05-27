using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VabelMitienditaEsc.Models
{
    public class CatalogoCuentas
    {
        public int idCuenta { get; set; }
        public string codigoCuenta { get; set; }
        public string nombreCuenta { get; set; }
        public int idTipoCuenta { get; set; }
        public bool activa { get; set; }
        override public string ToString()
        {
            return $"CatalogoCuentas [idCuenta={idCuenta}, codigoCuenta={codigoCuenta}, nombreCuenta={nombreCuenta}, idTipoCuenta={idTipoCuenta}, activa={activa}]";
        }
    }
}
