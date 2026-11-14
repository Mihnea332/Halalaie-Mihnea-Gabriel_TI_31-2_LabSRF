using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabSRF
{
    public class Attribute
    {
        public int OriginalIndex { get; set; }
        public string Name { get; set; }
        public double InfoGain { get; set; }

        public Attribute(int index,string name,double infoGain)
        {
            this.OriginalIndex = index;
            this.Name = name;
            this.InfoGain = infoGain;
        }
    }
}
