using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wsgenerales_master_csharp.App_Code
{
    public class LicenseInfo
    {
        public string SerialNumber { get; set; }

        public int Numero { get; set; }

        public string Jerarquia { get; set; }

        public int Estado { get; set; }

        public DateTime Vencimiento { get; set; }

        public string Observaciones { get; set; }

        public DateTime VencimientoLicencia { get; set; }

        public DateTime VencimientoSoporte { get; set; }
    }
}