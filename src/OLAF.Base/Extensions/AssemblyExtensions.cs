using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using System.Reflection;
using System.Text;

namespace OLAF
{
    public static class AssemblyExtensions
    {
        private static DirectoryInfo AssemblyDirectory = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;

        private static Version AssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;

        private static ILogger L = Global.Logger;

        public static List<Assembly> LoadAllFrom(this Assembly assembly, string includedFilePattern, params string[] excludedFileNames)
        {
            string[] assemblyFiles = null;
            try
            {
                assemblyFiles = Directory.GetFiles(AssemblyDirectory.FullName, includedFilePattern).ToArray();
            }
            catch (Exception e)
            {
                L.Error(e, "Exception thrown searching directory {0} for file pattern {1}.",
                    Assembly.GetExecutingAssembly().Location, includedFilePattern);
            }
            if (assemblyFiles == null)
            {
                L.Debug("No assembly files match pattern: {0}", includedFilePattern);
                return null;
            }
            else
            {
                return LoadAllFrom(assembly, assemblyFiles, Global.ExcludedAssemblyNames);
            }
        }

        public static List<Assembly> LoadAllFrom(this Assembly assembly, string[] includedFileNames, params string[] excludedFileNames)
        {
            if (includedFileNames == null)
            {
                throw new ArgumentNullException(nameof(includedFileNames));
            }
            List<Assembly> assemblies = new List<Assembly>();
            bool hasExlusions = excludedFileNames != null;
            for (int i = 0; i < includedFileNames.Length; i++)
            {
                if (hasExlusions && excludedFileNames.Any(f => includedFileNames[i].EndsWith(f)))
                {
                    continue;
                }
                else if (!File.Exists(includedFileNames[i]))
                {
                    continue;
                }
                else
                {
                    try
                    {
                        assemblies.Add(Assembly.LoadFrom(includedFileNames[i]));
                    }
                    catch (Exception e)
                    {
                        L.Error(e, "Exception thrown loading assembly from file {0}.", includedFileNames[i]);
                    }

                }
            }
            if (assemblies.Count == 0)
            {
                L.Debug("No assemblies loaded.");
                return null;
            }
            else
            {

                return assemblies;
            }
        }
    }
}
