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
        public List<string> TargetClasses { get; set; }
        public string Folder { get; set; }

        public Dictionary<int,double> NormalizedVector { get; set; }

        public DocumentInfo()
        {

        }
        public DocumentInfo (string name,Dictionary<int,int> wf,List<string> tc,string f,Dictionary<int,double> nv)
        {
            Name = name;
            WordFrequencies = wf;
            TargetClasses = tc;
            Folder = f;
            NormalizedVector = nv;
        }
    }
}
