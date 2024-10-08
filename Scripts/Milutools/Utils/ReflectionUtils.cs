using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Milutools.Utils
{
    public static class ReflectionUtils
    {
        private static Dictionary<Type, List<MethodInfo>> methodDict = new();
        
        public static List<MethodInfo> GetMethods<T>() where T : Attribute
        {
            var assembly = Assembly.Load("Assembly-CSharp");
            var types = assembly.GetTypes();
            if (!methodDict.ContainsKey(typeof(T)))
            {
                var methods = 
                    types.SelectMany(y => y.GetMethods())
                         .Where(x => x.GetCustomAttributes<T>().Any())
                         .ToList();
                methodDict.Add(typeof(T), methods);
            }

            return methodDict[typeof(T)];
        }
        
        public static MethodInfo GetFirstStaticMethod<T>() where T : Attribute
        {
            return GetMethods<T>().FirstOrDefault(x => x.IsStatic);
        }
    }
}
