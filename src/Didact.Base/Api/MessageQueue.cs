using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Didact
{
    public class MessageQueue : DidactApi<MessageQueue>
    {
        #region Constructors
        public MessageQueue(CancellationToken ct)
        {
            CancellationToken = ct;
        }
        #endregion

        #region Properties
        public ConcurrentDictionary<DidactHook, BlockingCollection<Message>> Queue { get; } =
            new ConcurrentDictionary<DidactHook, BlockingCollection<Message>>();

        protected CancellationToken CancellationToken { get; }
        #endregion

        #region Methods
        public void AddHook(DidactHook hook)
        {
            Queue.AddOrUpdate(hook, new BlockingCollection<Message>(new ConcurrentQueue<Message>()), 
            (h, u) =>
            {
                return null;
            });
        }

        public void AddAction(DidactHook hook, Message action)
        {
            Queue[hook].Add(action);
        }
        #endregion
    }
}