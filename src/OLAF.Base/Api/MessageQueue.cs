using System;
using System.Collections;
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
        public MessageQueue(Type[] types)
        {
            Index = new SortedList(types.Length);
            Queue = new BlockingCollection<Message>[(types.Length)];
            for (int i = 0; i < types.Length; i++)
            {
                Queue[i] = new BlockingCollection<Message>();
                Index.Add(i, types[i]);

            }
        }
        #endregion

        #region Properties
        public SortedList Index { get; }
        public BlockingCollection<Message>[] Queue { get; }
        #endregion

        #region Methods
        public void Enqueue<T>(Message message)
        {
            Queue[Index.IndexOfValue(typeof(T))].Add(message);
        }
        #endregion
    }
}
