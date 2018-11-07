using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace OLAF
{
    public interface IMonitor : IQueueProducer
    {
        string Name { get; }

        ApiResult Init();

        ApiResult Start();

        ApiResult Shutdown();

        ApiStatus Status { get; }

        List<Thread> Threads { get; }

        Type Type { get; }

        bool ShutdownRequested { get; }

        bool ShutdownCompleted { get; }
    }
}
