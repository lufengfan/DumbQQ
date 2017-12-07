using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;

namespace DumbQQ.Utils
{
    internal static class ReflectionHelper
    {
        public static CookieCollection GetAllCookies(this CookieContainer container)
        {
            var allCookies = new CookieCollection();
            var domainTableField = container.GetType().GetRuntimeFields().FirstOrDefault(x => x.Name == "m_domainTable");
            // ReSharper disable once PossibleNullReferenceException
            var domains = (IDictionary) domainTableField.GetValue(container);

            foreach (var val in domains.Values)
            {
                var type = val.GetType().GetRuntimeFields().First(x => x.Name == "m_list");
                var values = (IDictionary) type.GetValue(val);
                foreach (CookieCollection cookies in values.Values)
                    allCookies.Add(cookies);
            }
            return allCookies;
        }

        public static IEnumerable<FieldInfo> GetRuntimeFields(this Type type)
        {
            if (type == null) throw new ArgumentNullException("type");

            Type tRuntimeType = Assembly.GetAssembly(typeof(Type)).GetTypes().FirstOrDefault(t => t.Name == "RuntimeType");
            if (tRuntimeType != null)
            {
                if (tRuntimeType.IsAssignableFrom(type))
                    return type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                else
                    throw new ArgumentException($"{type.FullName} 不是运行时类型。", nameof(type));
            }
            else throw new NotSupportedException("不支持获取运行时字段。");
        }
    }
}