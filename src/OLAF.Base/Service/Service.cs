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

            Clients = clients?.ToList() ?? throw new ArgumentNullException(nameof(clients));
            if (Clients.Count == 0)
            {
                throw new ArgumentException("At least one client must be specified.", nameof(clients));
            }

            Status = ApiStatus.Initializing;
        }
        #endregion

        #region Abstract members
        public abstract ApiResult Init();
        protected abstract ApiResult ProcessClientQueue(TClientMessage message);
        #endregion

        #region Properties
        public Type QueueMessageType { get; } = typeof(TServiceMessage);

        public List<Type> Clients { get; }

        public bool ShutdownRequested => shutdownRequested;

        public bool ShutdownCompleted => shutdownCompleted;

        protected List<Thread> Threads { get; set; }

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
                Info("{0} service shutdown completed successfully.", Name);
                return ApiResult.Success;
            }
            else
            {
                Info("{0} threads in {1} service did not shutdown.", Threads.Count(t => t.IsAlive), Name);
                return ApiResult.Failure;
            }
        }

        protected virtual void ObserveClientQueue(Type client, CancellationToken token)
        {
            try
            {
                while (!shutdownRequested && !token.IsCancellationRequested)
                {
                    TClientMessage message =
                        (TClientMessage)Global.MessageQueue.Dequeue(client, cancellationToken);
                    ProcessClientQueue(message);
                }
                Info("Stopping client queue {0} observer in service {1}.", client.Name, type.Name);
                Status = ApiStatus.Ok;
                return;
            }
            catch (OperationCanceledException)
            {
                Info("Stopping client queue {0} observer in service {1}.", client.Name, type.Name);
                Status = ApiStatus.Ok;
                return;
            }
            catch (Exception ex)
            {
                Error(ex, "Error occurred in client queue {0} observer in service.", client.Name, type.Name);
            }
        }
        #endregion

        #region Fields
        protected bool shutdownRequested = false;
        protected bool shutdownCompleted = false;
        #endregion
    }
}
