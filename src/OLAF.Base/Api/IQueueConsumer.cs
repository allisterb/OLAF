using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLAF
{
    public interface IQueueConsumer<TMessage> where TMessage : Message
    {
        ApiResult Init();
        ApiResult Start();
        ApiResult Shutdown();
        bool ShutdownRequested { get; }
        bool ShutdownCompleted { get; }
        ApiResult ProcessDetectorQueue(TMessage message);
    }
}
