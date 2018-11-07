using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLAF
{
    public abstract class Pipeline : OLAFApi<Pipeline, Message>
    {
        #region Constructors
        protected Pipeline(Profile profile)
        {
            Profile = profile ?? throw new ArgumentNullException(nameof(profile));
            Services = new SortedList<int, IService>();
            Status = ApiStatus.Initializing;
        }
        #endregion

        #region Properties
        public Profile Profile { get; }

        public List<IMonitor> Monitors => Profile.Monitors;

        public Type[] MonitorClients => Monitors?.Select(m => m.Type).ToArray();

        public SortedList<int, IService> Services { get; }

        public static Dictionary<string, HashSet<string>> Dictionaries { get; private set; }

        public static Dictionary<string, string> DictionaryFiles { get; } = new Dictionary<string, string>()
        {
            {"words_en", "words_en.txt.gz" }
        };

       
        #endregion

        #region Methods
        public virtual ApiResult Init()
        {
            if (Status != ApiStatus.Initializing) return ApiResult.Failure;

            if (SetupDictionaries() != ApiResult.Success)
            {
                return SetErrorStatusAndReturnFailure("Could not initialize dictionaries. Not initializing pipeline.");
            }

            if (!Monitors.All(m => m.Status == ApiStatus.Initialized))
            {
                foreach(IMonitor m in Monitors.Where(m => m.Status != ApiStatus.Ok))
                {
                    Error("Monitor {0} has errors.", m.GetType().Name, type.Name);
                }
                return SetErrorStatusAndReturnFailure($"Not initializing pipeline {Name}.");
            }

            for(int i = 0; i < Services.Count; i++)
            {
                if (Services[i].Init() != ApiResult.Success)
                {
                    Error("Service {0} did not initialize.", Services[i].GetType().Name);
                    SetErrorStatusAndReturnFailure($"Not initializing pipeline {Name}.");
                }

            }
            return SetInitializedStatusAndReturnSucces();
        }

        public virtual ApiResult Start()
        {
            ThrowIfNotInitialized();
            if (!Monitors.All(m => m.Status == ApiStatus.Ok))
            {
                foreach (IMonitor m in Monitors.Where(m => m.Status != ApiStatus.Ok))
                {
                    Error("Monitor {0} did not start.", m.GetType().Name);
                }
                return SetErrorStatusAndReturnFailure($"Not starting pipeline {Name}.");
            }

            for (int i = 0; i < Services.Count; i++)
            {
                if (Services[i].Start() != ApiResult.Success)
                {
                    Error("Service {0} did not start.", Services[i].GetType().Name);
                    return SetErrorStatusAndReturnFailure($"Not starting pipeline {Name}.");
                }
            }
            return SetOkStatusAndReturnSucces();
        }

        public virtual ApiResult Shutdown()
        {
            ThrowIfNotOk();
            for (int i = 0; i < Services.Count; i++)
            {
                if (Services[i].Shutdown() != ApiResult.Success)
                {
                    Error("Service {0} did not shutdown successfully.", Services[i].GetType().Name);
                }
            }

            if (Services.All(s => s.Value.Status == ApiStatus.Ok))
            {
                Info("{0} pipeline shutdown completed successfully.", Name);
                return SetOkStatusAndReturnSucces();
            }
            else
            {
                Error("Error(s) occurred during pipeline {0} shutdown.", type.Name);
                return SetErrorStatusAndReturnFailure();
            }
        }

        protected int AddService(IService service)
        {
            int i = Services.Count;
            if (i == 0)
            {
                service.AddClients(MonitorClients);
                service.Pipeline = this;
                Services.Add(0, service);
            }
            else
            {
                service.AddClient(Services.Last().Value.Type);
                service.Pipeline = this;
                Services.Add(i, service);
            }
            return i;
        }

        protected void AddService<T>() where T : IService => 
            AddService((IService)Activator.CreateInstance(typeof(T), Profile));

        protected void SetPipelineInitializingStatus()
        {
            if (Services.All(s => s.Value.Status == ApiStatus.Initializing))
            {
                this.Status = ApiStatus.Initializing;
            }
            else
            {
                var u = Services.Where(s => s.Value.Status != ApiStatus.Initializing);
                Error("The following services could not be constructed: {0}", u.Select(s => s.Value.Name));
                this.Status = ApiStatus.Error;
            }
        }

        public static ApiResult SetupDictionaries()
        {
            if (Dictionaries != null && Dictionaries.Count == DictionaryFiles.Count * 2)
            {
                Debug("Dictionaries already setup. Not running dictionary setup again.");
                return ApiResult.Success;
            }
            else if (Dictionaries == null)
            {
                Dictionaries = new Dictionary<string, HashSet<string>>(DictionaryFiles.Count * 2);
            }

            int setup = 0;

            using (var op = Begin("Setting up dictionaries"))
            {   
                foreach (var df in DictionaryFiles)
                {
                    var dfpath = GetDataDirectoryPathTo("dictionaries", df.Value);
                    if (Dictionaries.ContainsKey(df.Key))
                    {
                        Debug("Not updating existing dictionary {0}.", df.Key);
                        continue;
                    }
                    else if (!File.Exists(dfpath))
                    {
                        Error("The dictionary file {0} for dictionary {1} could not be found.", dfpath, df.Key);
                        continue;
                    }
                    else
                    {
                        try
                        {
                            using (FileStream fs = File.OpenRead(dfpath))
                            using (Stream s = df.Value.EndsWith(".gz") || df.Value.EndsWith(".GZ") ? 
                                new GZipStream(fs, CompressionMode.Decompress) : (Stream)fs)
                            {
           
                                var data = ReadAllLines(() => s, Encoding.UTF8).ToArray();
                                Dictionaries.Add(df.Key, new HashSet<string>(data));
                                Debug("Read {0} entries from file.", data.Length, dfpath);
                                Info("Dictionary {0} has {1} entries from file: {2}.", df.Key, Dictionaries[df.Key].Count, dfpath);
                                Dictionaries.Add(df.Key + "_3grams", new HashSet<string>(data.Where(w => w.Length <= 3)));
                                Info("Dictionary {0} has {1} entries from file: {2}.", df.Key + "_3grams",  
                                    Dictionaries[df.Key  + "_3grams"].Count, dfpath);
                                setup++;
                            }
                        }
                        catch (Exception e)
                        {
                            Error(e, "An error occurred reading dictionary file {0}.", dfpath);
                            continue;
                        }
                    }
                }
                op.Complete();
            }
            return setup > 0 ? ApiResult.Success : ApiResult.Failure;
        }

        //From https://stackoverflow.com/a/13312954 by Jon Skeet
        protected static IEnumerable<string> ReadAllLines(Func<Stream> streamProvider,
                                      Encoding encoding)
        {
            using (var stream = streamProvider())
            using (var reader = new StreamReader(stream, encoding))
            {
                while (!reader.EndOfStream)
                {
                    yield return reader.ReadLine();
                }
            }
        }
        #endregion

    }
}
