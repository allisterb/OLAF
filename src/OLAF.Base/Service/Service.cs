using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace OLAF
{
    public abstract class Service<TClient, TClientMessage, TServiceMessage> : 
        OLAFApi<Service<TClient, TClientMessage, TServiceMessage>, TServiceMessage>,
        IService
        where TClient : OLAFApi<TClient, TClientMessage>
        where TClientMessage : Message
        where TServiceMessage : Message
    {
        #region Constructors
        public Service(Profile profile, params TClient[] clients) : base()
        {
            Profile = profile ?? throw new ArgumentNullException(nameof(profile));
            Clients = clients.ToList();
            Status = ApiStatus.Initializing;
        }
        #endregion

        #region Abstract methods
        public abstract ApiResult Init();
        protected abstract ApiResult ProcessClientQueue(TClientMessage message);
        #endregion

        #region Properties
        public string ApiAccountName { get; protected set; }

        public string ApiAccountKey { get; protected set; }

        public string ApiConnectionString { get; protected set; }

        public Uri EndpointUrl { get; protected set; }

        public Profile Profile { get; }

        public List<TClient> Clients { get; }

        public Thread QueueMonitorThread { get; protected set; }

        public bool ShutdownRequested => shutdownRequested;

        public bool ShutdownCompleted => shutdownCompleted;

        protected List<Thread> Threads { get; set; }
        #endregion

        #region Methods
        public virtual ApiResult Start()
        {
            ThrowIfNotInitialized();
            QueueMonitorThread = new Thread(() => MonitorQueue(Global.CancellationTokenSource.Token));
            Threads = new List<Thread>() { QueueMonitorThread };
            QueueMonitorThread.Start();
            return ApiResult.Success;
        }

        public virtual ApiResult Shutdown()
        {
            ThrowIfNotInitialized();
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
                Info("{0} shutdown complete.", this.GetType().Name);
                return ApiResult.Success;
            }
            else
            {
                Info("{0} threads in {1} did not shutdown.", Threads.Count(t => t.IsAlive),
                    this.GetType().Name);
                return ApiResult.Failure;
            }
        }

        protected virtual void MonitorQueue(CancellationToken token)
        {
            try
            {
                while (!shutdownRequested && !token.IsCancellationRequested)
                {
                    TClientMessage message =
                        (TClientMessage)Global.MessageQueue.Dequeue<TClient>(cancellationToken);
                    ProcessClientQueue(message);
                }
                Info("Stopping {0} queue monitor.", type.Name);
                Status = ApiStatus.Ok;
                return;
            }
            catch (OperationCanceledException)
            {
                Info("Stopping {0} queue monitor.", type.Name);
                Status = ApiStatus.Ok;
                return;
            }
            catch (Exception ex)
            {
                Error(ex, "Error occurred during {0} queue monitoring.", type.Name);
            }
        }
        #endregion

        #region Fields
        protected bool shutdownRequested = false;
        protected bool shutdownCompleted = false;
        #endregion
    }
}
