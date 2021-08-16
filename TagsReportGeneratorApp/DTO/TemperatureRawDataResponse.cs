using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagsReportGeneratorApp.Models.Response
{
    public class TemperatureRawDataResponse
    {
        public string __type { get; set; }
        public string time { get; set; }
        public double temp_degC { get; set; }
        public double cap { get; set; }
        public double lux { get; set; }
        public double battery_volts { get; set; }
    }
}
