using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class Load
    {
        public Load()
        {

        }
        public int Id { get; set; }
        public DateTime TimeFrom { get; set; }

        public DateTime TimeTo { get; set; }

        public Int32 LoadMWh { get; set; }
    }
}
