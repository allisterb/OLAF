using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLAF
{
    [Flags]
    public enum ApiResult
    {
        Unknown = -1,
        Success = 0,
        Failure = 1,
        NoOp = 2
    }
}
