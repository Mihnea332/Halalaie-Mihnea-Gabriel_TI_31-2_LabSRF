using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabSRF
{
    public class DocumentInfo
    {
        public string Name { get; set; }
        public Dictionary<int, int> WordFrequencies { get; set; }
        public string TargetClass { get; set; }
        public string Folder { get; set; }

        
    }
}
