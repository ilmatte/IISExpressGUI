using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISExpressGui.Domain
{
    public class WebSite
    {
        public WebSite()
        {
        }

        public long Id { get; set; }
        public string Name { get; set; }
        public string PhysicalPath { get; set; }
        public string Url { get; set; }
        public string Port { get; set; }
        public bool IsRunning { get; set; }        
    }
}
