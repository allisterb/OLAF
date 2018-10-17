using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLAF
{
    public abstract class Detector<T> : OLAFApi<T> where T : Detector<T>
    {
    }
}
