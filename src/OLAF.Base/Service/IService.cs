using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace OLAF
{
    public interface IService : IQueueProducer
    {
        Type Type { get; }

        string Name { get; }

        Pipeline Pipeline { get; set; }

        ApiStatus Status { get; }

        List<Thread> Threads { get; }

        bool ShutdownRequested { get; }

        bool ShutdownCompleted { get; }

        ApiResult Init();

        ApiResult Start();

        ApiResult Shutdown();

        void AddClient(Type c);

        void AddClients(IEnumerable<Type> clients);

        bool IsLastInPipeline { get; }
       
    }
}
