using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLAF
{
    public class SimpleConsoleLogger : ILogger
    {
        public void Info(string messageTemplate, params object[] propertyValues)
            => Console.WriteLine(messageTemplate, propertyValues);

        public void Debug(string messageTemplate, params object[] propertyValues)
            => Console.WriteLine(messageTemplate, propertyValues);

        public void Error(string messageTemplate, params object[] propertyValues)
            => Console.WriteLine(messageTemplate, propertyValues);

        public void Error(Exception e, string messageTemplate, params object[] propertyValues)
            => Console.WriteLine(messageTemplate + Environment.NewLine + e.Message + Environment.NewLine + 
                e.StackTrace, propertyValues);

        public void Verbose(string messageTemplate, params object[] propertyValues)
            => Console.WriteLine(messageTemplate, propertyValues);

        public void Warn(string messageTemplate, params object[] propertyValues)
            => Console.WriteLine(messageTemplate, propertyValues);

        public void Close() {}
    }
}
