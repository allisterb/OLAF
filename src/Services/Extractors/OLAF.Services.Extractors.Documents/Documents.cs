using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

using TikaOnDotNet.TextExtraction;
using TikaOnDotNet.TextExtraction.Stream;

namespace OLAF.Services.Extractors
{
    
    public class Documents : Service<FileArtifact, TextArtifact>
    {
        static Documents()
        {
            Environment.SetEnvironmentVariable("TIKA_CONFIG", "tika-config.xml", EnvironmentVariableTarget.Process);
            /* Work around Tikaondotnet #15: https://github.com/KevM/tikaondotnet/issues/15 */

            var t = typeof(com.sun.codemodel.@internal.ClassType);
            t = typeof(com.sun.org.apache.xalan.@internal.xsltc.trax.TransformerFactoryImpl);
            t = typeof(com.sun.org.glassfish.external.amx.AMX); 
        }
        public Documents(Profile profile, params Type[] clients) : base(profile, clients) {}

        protected TextExtractor Extractor { get; set; }

        public override ApiResult Init()
        {
            Extractor = new TextExtractor();
            return SetInitializedStatusAndReturnSucces();   
        }

        protected override ApiResult ProcessClientQueueMessage(FileArtifact artifact)
        {
            if (!artifact.HasData)
            {
                try
                {
                    artifact.FileOpenAttempts++;
                    using (var op = Begin("Extracting text from artifact {0}", artifact.Id))
                    {
                        var result = Extractor.Extract(artifact.Path);
                        if (!ReferenceEquals(result, null) & !string.IsNullOrEmpty(result.Text))
                        {
                            Debug("Extracted {0} document from file {1}.", result.ContentType, artifact.Path);
                            op.Complete();
                            TextArtifact text = new TextArtifact(artifact.Path, result.Text);
                            if (result.Metadata != null)
                            {
                                foreach (var m in result.Metadata)
                                {
                                    text.Metadata.Add(m.Key, m.Value);
                                }
                            }
                            EnqueueMessage(text);
                            Info("{0} added artifact id {1} of type {2} from artifact {3}.", Name, text.Id, text.GetType(),
                                artifact.Id);
                            return ApiResult.Success;
                        }
                        else
                        {
                            Error("Could not extract document from file {0}.", artifact.Path);
                        }
                    }
                }
                catch (ArgumentException ae)
                {
                    if (artifact.FileLocked && artifact.FileOpenAttempts > 5)
                    {
                        Error(ae, "{0} file locked for more than 5 attempts or other error during extraction. Aborting extract attempt.", artifact.Name);
                        return ApiResult.Failure;
                    }
                    else if (ae.Message.Contains("Parameter is not valid"))
                    {
                        artifact.FileLocked = true;
                    }

                }
                catch (Exception e)
                {
                    Error(e, "An error occurred attempting to read the file {0}.", artifact.Path);
                    return ApiResult.Failure;
                }
                
                if (artifact.FileLocked && artifact.FileOpenAttempts <= 5)
                {
                    Debug("{0} file may be locked...pausing a bit and trying extraction again...({1})", artifact.Name,
                        artifact.FileOpenAttempts);
                    Thread.Sleep(100);
                    return ProcessClientQueueMessage(artifact);
                }
                else if (artifact.FileLocked && artifact.FileOpenAttempts > 5)
                {
                    Error("{0} file locked for more than 5 attempts or other error during extraction. Aborting extract attempt.", artifact.Name);
                    return ApiResult.Failure;
                }
                else
                {
                    Error("Unknown error extracting document from {0}; aborting extract attempt.", artifact.Path);
                    return ApiResult.Failure;
                }
            }
            else
            {
                using (var op = Begin("Extracting document from artifact {0}", artifact.Id))
                {
                    var result = Extractor.Extract(artifact.Data);
                    if (!ReferenceEquals(result, null) & !string.IsNullOrEmpty(result.Text))
                    {
                        Debug("Extracted {0} document from artifact {1} data.", result.ContentType, artifact.Id);
                        op.Complete();
                        TextArtifact text = new TextArtifact(artifact.Path, result.Text);
                        if (result.Metadata != null)
                        {
                            foreach (var m in result.Metadata)
                            {
                                text.Metadata.Add(m.Key, m.Value);
                            }
                        }
                        EnqueueMessage(text);
                        Info("{0} added artifact id {1} of type {2} from artifact {3}.", Name, text.Id, text.GetType(),
                            artifact.Id);
                        return ApiResult.Success;
                    }
                    else
                    {
                        Error("Could not extract document from artifact {0} data.", artifact.Id);
                        return ApiResult.Failure;
                    }
                }
            }
        }
    }
}
