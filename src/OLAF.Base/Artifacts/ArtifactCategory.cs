using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLAF
{
    public class ArtifactCategory
    {
        public ArtifactCategory(string name, string detail, double score)
        {
            Name = name;
            Detail = detail;
            Score = score;
        }

        public string Name { get; }

        public string Detail { get; }

        public double Score { get; }
    }
}
