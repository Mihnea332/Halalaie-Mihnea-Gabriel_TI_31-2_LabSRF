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

        public Dictionary<int,double> NormalizedVector { get; set; }

        public DocumentInfo()
        {

        }
        public DocumentInfo (string name,Dictionary<int,int> wf,string tc,string f,Dictionary<int,double> nv)
        {
            Name = name;
            WordFrequencies = wf;
            TargetClass = tc;
            Folder = f;
            NormalizedVector = nv;
        }
    }
}
