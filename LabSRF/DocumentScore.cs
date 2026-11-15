using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabSRF
{
    public class DocumentScore
    {
        public string Name { get; set; }
        public double Score { get; set; }
        public DocumentScore(string name, double score)
        {
            this.Name = name;
            this.Score = score;
        }
    }
   
}
