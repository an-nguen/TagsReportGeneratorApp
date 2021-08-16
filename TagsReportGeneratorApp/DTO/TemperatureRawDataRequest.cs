using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagsReportGeneratorApp.Models.Request
{
    public class TemperatureRawDataRequest
    {
        public string uuid { get; set; }
        public string fromDate { get; set; }
        public string toDate { get; set; }
    }
}
