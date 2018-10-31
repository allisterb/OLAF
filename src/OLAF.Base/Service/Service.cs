using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace OLAF
{
    public abstract class Service<TClientMessage, TServiceMessage> : 
        OLAFApi<Service<TClientMessage, TServiceMessage>, TServiceMessage>, IService
        where TClientMessage : Message
        where TServiceMessage : Message
    {
        #region Constructors
       
        public Service(Profile profile, params Type[] clients) : base()
        {
            Profile = profile ?? throw new ArgumentNullException(nameof(profile));
            Clients = clients.ToList();
            Status = ApiStatus.Initializing;
        }
        #endregion

        #region Abstract members
        public abstract ApiResult Init();
        protected abstract ApiResult ProcessClientQueueMessage(TClientMessage message);
        #endregion

        #region Properties
        public Type QueueMessageType { get; } = typeof(TServiceMessage);

        public List<Type> Clients { get; }

        public bool ShutdownRequested => shutdownRequested;

        public bool ShutdownCompleted => shutdownCompleted;

        public List<Thread> Threads { get; protected set; }

        public string ApiAccountName { get; protected set; }

        public string ApiAccountKey { get; protected set; }

        public string ApiConnectionString { get; protected set; }

        public string ApiEndpointUrl { get; protected set; }

        public Profile Profile { get; }
        #endregion

        #region Methods
        public virtual ApiResult Start()
        {
            ThrowIfNotInitialized();
            Threads = new List<Thread>(Clients.Count);
            foreach (Type type in Clients)
            {
                Thread observeThread = new Thread(() => ObserveClientQueue(type, Global.CancellationTokenSource.Token));
                observeThread.Start();
                Threads.Add(observeThread);
            }
            Status = ApiStatus.Ok;
            return ApiResult.Success;
        }

        public virtual ApiResult Shutdown()
        {
            ThrowIfNotOk();
            shutdownRequested = true;
            if (!cancellationToken.IsCancellationRequested)
            {
                Global.CancellationTokenSource.Cancel();
            }
            int waitCount = 0;
            while (Threads.Any(t => t.IsAlive) && waitCount < 30)
            {
                Thread.Sleep(100);
                waitCount++;
            }
            if (Threads.All(t => !t.IsAlive))
            {
                shutdownCompleted = true;
                Info("All threads stopped. {0} service shutdown completed successfully.", Name);
                return ApiResult.Success;
            }
            else
            {
                Info("{0} threads in {1} did not stop. Aborting {0} threads", Threads.Count(t => t.IsAlive),
                    this.GetType().Name);
                foreach (Thread thread in Threads.Where(t => t.IsAlive))
                {
                    thread.Abort();
                }
                if (Threads.All(t => !t.IsAlive))
                {
                    shutdownCompleted = true;
                    Info("All threads stopped. {0} service shutdown completed successfully.", Name);
                    return ApiResult.Success;
                }
                else
                {
                    return ApiResult.Failure;
                }
            }
        }

        public void AddClient(Type c)
        {
            if (!c.Implements<IService>() && !c.Implements<IMonitor>())
            {
                throw new InvalidOperationException($"{c.Name} is not a service or monitor.");
            }
            else if (Clients.Contains(c))
            {
                return;
            }
            else
            {
                Clients.Add(c);
            }
        }

        public void AddClients(IEnumerable<Type> clients)
        {
            foreach(Type t in clients)
            {
                AddClient(t);
            }
        }

        protected virtual void ObserveClientQueue(Type client, CancellationToken token)
        {
            try
            {
                while (!shutdownRequested && !token.IsCancellationRequested)
                {
                    Message message =
                        Global.MessageQueue.Dequeue(client, cancellationToken);
                    if (message is TClientMessage)
                    {
                        Debug("{0} consuming message {1}.", Name, message.Id);
                        ProcessClientQueueMessage(message as TClientMessage);
                        
                    }
                    else
                    {
                        Debug("{0} passing on message {1}.", Name, message.Id);
                        EnqueueMessage(message);
                    }
                    
                }
                Info("Stopping {0} client queue observer in service {1}.", client.Name, type.Name);
                Status = ApiStatus.Ok;
                return;
            }
            catch (OperationCanceledException)
            {
                Info("Stopping {0} client queue observer in service {1}.", client.Name, type.Name);
                Status = ApiStatus.Ok;
                return;
            }
            catch (Exception ex)
            {
                Error(ex, "Error occurred during {0} client queue observng in service {1}. Resuming", client.Name, Name);
                ObserveClientQueue(client, token);
            }
        }

        
        #endregion

        #region Fields
        protected bool shutdownRequested = false;
        protected bool shutdownCompleted = false;
        #endregion
    }
}
