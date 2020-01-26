using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;

namespace Json.Serialization
{
    /// <summary>
    /// Defines special JSON serialization techniques for specific classes.
    /// </summary>
    public interface ITranslatorExtensions
    {
        /// <summary>
        /// Return a function that translates a .NET business object of the specified type into a JsonObject.
        /// </summary>
        JsonTranslator.JsonMaker MakeJsonMaker(Type objectType);

        /// <summary>
        /// Return a function that translates a JsonObject into a .NET business object of the specified type.
        /// </summary>
        JsonTranslator.ObjectMaker MakeObjectMaker(Type objectType);
    }

    /// <summary>
    /// Contains special JSON [de]serialization routines for commonly-used types.
    /// Could also act as a base class for a more sophisticated set of extensions.
    /// </summary>
    public class DefaultTranslatorExtensions : ITranslatorExtensions
    {
        #region Infrastructure

        private Dictionary<Type, JsonTranslator.JsonMaker> _JsonMakers = new Dictionary<Type, JsonTranslator.JsonMaker>();
        private Dictionary<Type, JsonTranslator.ObjectMaker> _ObjectMakers = new Dictionary<Type, JsonTranslator.ObjectMaker>();

        public DefaultTranslatorExtensions()
        {
            foreach (MethodInfo mi in this.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Static))
            {
                SerializedTypeAttribute st = mi.GetCustomAttribute(typeof(SerializedTypeAttribute)) as SerializedTypeAttribute;
                if (st != null)
                {
                    ParameterInfo[] args = mi.GetParameters();
                    if (args.Length != 1)
                        continue;
                    if (args[0].ParameterType == typeof(object) && mi.ReturnType == typeof(JsonObject))
                        _JsonMakers[st.Type] = (JsonTranslator.JsonMaker)Delegate.CreateDelegate(typeof(JsonTranslator.JsonMaker), mi);
                    if (args[0].ParameterType == typeof(JsonObject) && mi.ReturnType == typeof(object))
                        _ObjectMakers[st.Type] = (JsonTranslator.ObjectMaker)Delegate.CreateDelegate(typeof(JsonTranslator.ObjectMaker), mi);
                }
            }
        }

        // Implements ITranslatorExtensions.MakeJsonMaker
        public JsonTranslator.JsonMaker MakeJsonMaker(Type objectType)
        {
            return _JsonMakers.ContainsKey(objectType) ? _JsonMakers[objectType] : null;
        }

        // Implements ITranslatorExtensions.MakeObjectMaker
        public JsonTranslator.ObjectMaker MakeObjectMaker(Type objectType)
        {
            return _ObjectMakers.ContainsKey(objectType) ? _ObjectMakers[objectType] : null;
        }

        class SerializedTypeAttribute : Attribute
        {
            public Type Type;

            public SerializedTypeAttribute(Type type)
            {
                this.Type = type;
            }
        }

        private static DefaultTranslatorExtensions _Singleton = null;
        public static DefaultTranslatorExtensions Singleton
        {
            get
            {
                if (_Singleton == null)
                    _Singleton = new DefaultTranslatorExtensions();
                return _Singleton;
            }
        }

        #endregion

        #region Custom serialization routines

        #region Rectangle

        [SerializedType(typeof(Rectangle))]
        static JsonObject MakeJson_Rectangle(Object obj)
        {
            Rectangle r = (Rectangle)obj;
            return new JsonObject(r.Left + "," + r.Top + "," + r.Width + "," + r.Height);
        }

        [SerializedType(typeof(Rectangle))]
        static object MakeObject_Rectangle(JsonObject json)
        {
            if (json.ObjectType != JsonObject.Type.String)
                throw new FormatException("Expected JSON String type for .NET Rectangle but found instead " + json.ObjectType);
            int[] v = json.String.Split(',').Select(s => int.Parse(s)).ToArray();
            return new Rectangle(v[0], v[1], v[2], v[3]);
        }

        #endregion

        #region DateTime

        [SerializedType(typeof(DateTime))]
        static JsonObject MakeJson_DateTime(Object obj)
        {
            DateTime t = ((DateTime)obj).ToUniversalTime();
            string baseString = t.ToString("yyyy-MM-ddTHH:mm:ss");

            const long TICKS_PER_SECOND = 10000000;
            long fractionalSeconds = t.Ticks % TICKS_PER_SECOND;
            string fractionalString;
            if (fractionalSeconds > 0)
            {
                int leadingZeros = 6 - (int)Math.Floor(Math.Log10(fractionalSeconds));
                while (fractionalSeconds % 10 == 0)
                {
                    fractionalSeconds /= 10;
                }
                fractionalString = "." + new string('0', leadingZeros) + fractionalSeconds;
            }
            else
            {
                fractionalString = "";
            }

            return new JsonObject(baseString + fractionalString + "Z");
        }

        [SerializedType(typeof(DateTime))]
        static object MakeObject_DateTime(JsonObject json)
        {
            if (json.ObjectType == JsonObject.Type.String)
                return DateTime.Parse(json.String, null, System.Globalization.DateTimeStyles.RoundtripKind).ToLocalTime(); //"2010-08-20T15:00:00Z"
            else
                throw new FormatException("Invalid JSON: Expected parseable JSON date String; instead found JSON " + json.ObjectType);
        }

        #endregion

        #region IPEndPoint

        [SerializedType(typeof(IPEndPoint))]
        static JsonObject MakeJson_IPEndPoint(Object obj)
        {
            if (obj == null)
                return JsonObject.Null;
            else
                return new JsonObject(((IPEndPoint)obj).ToString());
        }

        [SerializedType(typeof(IPEndPoint))]
        static object MakeObject_IPEndPoint(JsonObject json)
        {
            if (json.ObjectType == JsonObject.Type.Null)
                return null;
            else if (json.ObjectType == JsonObject.Type.String)
            {
                int i = json.String.LastIndexOf(':');
                if (i < 0) throw new FormatException("Invalid JSON: Expected IPEndPoint in form of <IPAddress>:<Port>");
                return new IPEndPoint(IPAddress.Parse(json.String.Substring(0, i)), int.Parse(json.String.Substring(i + 1)));
            }
            else
                throw new FormatException("Invalid JSON: Expected parseable JSON IPEndPoint string; instead found JSON " + json.ObjectType);
        }

        #endregion

        #region DirectoryInfo

        [SerializedType(typeof(DirectoryInfo))]
        static JsonObject MakeJson_DirectoryInfo(Object obj)
        {
            if (obj == null)
                return JsonObject.Null;
            else
                return new JsonObject(((DirectoryInfo)obj).FullName);
        }

        [SerializedType(typeof(DirectoryInfo))]
        static object MakeObject_DirectoryInfo(JsonObject json)
        {
            if (json.ObjectType == JsonObject.Type.Null)
                return null;
            else if (json.ObjectType == JsonObject.Type.String)
            {
                return new DirectoryInfo(json.String);
            }
            else
                throw new FormatException("Invalid JSON: Expected DirectoryInfo full path as JSON String; instead found JSON " + json.ObjectType);
        }

        #endregion

        #region FileInfo

        [SerializedType(typeof(FileInfo))]
        static JsonObject MakeJson_FileInfo(Object obj)
        {
            if (obj == null)
                return JsonObject.Null;
            else
                return new JsonObject(((FileInfo)obj).FullName);
        }

        [SerializedType(typeof(FileInfo))]
        static object MakeObject_FileInfo(JsonObject json)
        {
            if (json.ObjectType == JsonObject.Type.Null)
                return null;
            else if (json.ObjectType == JsonObject.Type.String)
            {
                return new FileInfo(json.String);
            }
            else
                throw new FormatException("Invalid JSON: Expected FileInfo full path as JSON String; instead found JSON " + json.ObjectType);
        }

        #endregion

        #region double[,]

        [SerializedType(typeof(double[,]))]
        static JsonObject MakeJson_Double2d(Object obj)
        {
            if (obj == null)
                return JsonObject.Null;

            double[,] matrix = (double[,])obj;

            var dict = new Dictionary<string, JsonObject>();
            dict["N"] = new JsonObject(matrix.GetLength(0));
            dict["M"] = new JsonObject(matrix.GetLength(1));
            double[] v = new double[matrix.Length];
            Buffer.BlockCopy(matrix, 0, v, 0, v.Length * sizeof(double));
            dict["V"] = new JsonObject(v.Select(m => new JsonObject(m)));
            return new JsonObject(dict);
        }

        [SerializedType(typeof(double[,]))]
        static object MakeObject_Double2d(JsonObject json)
        {
            if (json.ObjectType == JsonObject.Type.Null)
                return null;
            else if (json.ObjectType == JsonObject.Type.Dictionary)
            {
                Dictionary<string, JsonObject> jmat = json.Dictionary;
                int n = (int)jmat["N"].Number;
                int m = (int)jmat["M"].Number;
                double[] v = jmat["V"].Array.Select(j => j.Number).ToArray();
                double[,] mat = new double[n, m];
                Buffer.BlockCopy(v, 0, mat, 0, n * m * sizeof(double));
                return mat;
            }
            else
                throw new FormatException("Invalid JSON: Expected double[,] as JSON Dictionary; instead found JSON " + json.ObjectType);
        }

        #endregion

        #region Uri

        [SerializedType(typeof(Uri))]
        static JsonObject MakeJson_Uri(Object obj)
        {
            if (obj == null)
                return JsonObject.Null;
            else
                return new JsonObject(((Uri)obj).AbsoluteUri);
        }

        [SerializedType(typeof(Uri))]
        static object MakeObject_Uri(JsonObject json)
        {
            if (json.ObjectType == JsonObject.Type.Null)
                return null;
            else if (json.ObjectType == JsonObject.Type.String)
            {
                return new Uri(json.String);
            }
            else
                throw new FormatException("Invalid JSON: Expected Uri string; instead found JSON " + json.ObjectType);
        }

        #endregion

        #endregion
    }
}
