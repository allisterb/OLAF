using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace OLAF
{
    public interface IService
    {
        Type Type { get; }

        string Name { get; }

        ApiResult Init();

        ApiResult Start();

        ApiResult Shutdown();

        void AddClient(Type c);

        void AddClients(IEnumerable<Type> clients);

        ApiStatus Status { get; }

        List<Thread> Threads { get; }

        bool ShutdownRequested { get; }

        bool ShutdownCompleted { get; }
    }
}
