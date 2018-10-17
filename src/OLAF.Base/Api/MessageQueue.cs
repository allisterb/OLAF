using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OLAF
{
    public class MessageQueue : OLAFApi<MessageQueue>
    {
        #region Constructors
        public MessageQueue(CancellationToken ct)
        {
            CancellationToken = ct;
        }
        #endregion

        #region Properties
        public ConcurrentDictionary<OLAFHook, BlockingCollection<Message>> Queue { get; } =
            new ConcurrentDictionary<OLAFHook, BlockingCollection<Message>>();

        protected CancellationToken CancellationToken { get; }
        #endregion

        #region Methods
        public void AddHook(OLAFHook hook)
        {
            Queue.AddOrUpdate(hook, new BlockingCollection<Message>(new ConcurrentQueue<Message>()), 
            (h, u) =>
            {
                return null;
            });
        }

        public void AddAction(OLAFHook hook, Message action)
        {
            Queue[hook].Add(action);
        }
        #endregion
    }
}
