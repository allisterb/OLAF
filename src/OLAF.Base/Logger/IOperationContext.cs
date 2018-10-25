using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLAF
{
    public interface IOperationContext : IDisposable
    {
        void Complete();
        void Cancel();
    }
}
