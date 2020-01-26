using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;

namespace Json.Serialization
{
    static class JsonReflection
    {
        /// <summary>
        /// For a type that inherits generic IEnumerable, get the type of each item in that IEnumerable
        /// </summary>
        public static Type GetEnumerableType(Type type)
        {
            foreach (Type intType in type.GetInterfaces())
            {
                if (intType.IsGenericType && intType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    return intType.GetGenericArguments()[0];
            }
            return null;
        }

        /// <summary>
        /// Get a function that converts a string to an object of the specified type
        /// </summary>
        public static Func<string, object> GetParser(Type type)
        {
            if (type == typeof(string))
                return s => s;
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                return GetParser(type.GetGenericArguments()[0]);

            MethodInfo parse = type.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(string) }, null);

            if (parse == null)
                return null;
            else
                return s => parse.Invoke(null, new object[] { s });
        }

        /// <summary>
        /// Get a function that converts an object of the specified type to a string
        /// </summary>
        /// <param name="type"></param>
        public static Func<object, string> GetToString(Type type)
        {
            if (type == typeof(string))
                return obj => obj as string;

            MethodInfo toString = type.GetMethod("ToString", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { }, null);

            if (toString == null)
                return null;
            else
                return obj => toString.Invoke(obj, new object[] { }) as string;
        }

        /// <summary>
        /// For a specified object type, get a function that either invokes the parameterless constructor of the object, or else returns an uninitialized object.
        /// </summary>
        /// <param name="type">Object type for which to obtain a default maker</param>
        public static Func<object> GetDefaultMaker(Type type)
        {
            ConstructorInfo ci = type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { }, null);
            if (ci == null)
                return () => FormatterServices.GetUninitializedObject(type);
            else
                return () => ci.Invoke(new object[] { });
        }

        /// <summary>
        /// Perform a type-at-runtime verison of the explicit (type)data cast
        /// </summary>
        /// <remarks>
        /// https://stackoverflow.com/a/27584212/651139
        /// </remarks>
        public static object Cast(object data, Type type)
        {
            var DataParam = Expression.Parameter(typeof(object), "data");
            var Body = Expression.Block(Expression.Convert(Expression.Convert(DataParam, data.GetType()), type));

            var Run = Expression.Lambda(Body, DataParam).Compile();
            var ret = Run.DynamicInvoke(data);
            return ret;
        }

        public class DictionaryInfo
        {
            public Type KeyType;
            public Type ValueType;
        }

        public static DictionaryInfo IsDictionary(Type type)
        {
            while (type != null && type != typeof(object))
            {
                var cur = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
                if (cur == typeof(Dictionary<,>))
                    return new DictionaryInfo() { KeyType = type.GetGenericArguments()[0], ValueType = type.GetGenericArguments()[1] };
                type = type.BaseType;
            }
            return null;
        }
    }
}
