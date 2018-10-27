using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLAF
{
    public enum ExitCode
    {
        Success = 0,
        UnhandledException = 1,
        InvalidOptions = 2,
        FileOrDirectoryNotFound = 3,
        InitError = 4,
        StartError = 5,
        ShutdownError = 6
    }
}
