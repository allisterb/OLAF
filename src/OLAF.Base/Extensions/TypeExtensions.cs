using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OLAF
{
    public static class TypeExtensions
    {
        public static bool Implements<T>(this Type type) => type.GetInterface(typeof(T).Name) != null;
    }
}
