using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Json.Serialization.Tests
{
    class Classes
    {
        public class BasicClass
        {
            public string StringField;
            private int IntField;
            private double DoubleField;

            public BasicClass(string stringValue, int intValue, double doubleValue)
            {
                this.StringField = stringValue;
                this.IntField = intValue;
                this.DoubleField = doubleValue;
            }

            public static bool AreEqual(BasicClass obj1, BasicClass obj2)
            {
                if (obj1 == null)
                {
                    return obj2 == null;
                }
                return obj1.StringField == obj2.StringField &&
                    obj1.IntField == obj2.IntField &&
                    obj1.DoubleField == obj2.DoubleField;
            }
        }

        public enum AnEnum
        {
            Value1,
            SecondValue,
            AThirdValue,
        }

        public class ComplexClass
        {
            public bool? NullableBool;
            private AnEnum Enum;
            public double? NullableDouble;
            private int? NullableInt;
            public float Float;
            protected float? NullableFloat;
            public bool Bool;
            public Dictionary<string, string> SimpleDict;
            private Dictionary<int?, DateTime> NullableIntKeyedDateTimeDict;
            public double[] DoubleArray;
            public List<bool> BoolList;
            public Dictionary<string, List<(int, ComplexClass)>> Children;
            public Tuple<string, int, bool> PlainTuple;
            public IPEndPoint IPEndPoint;
            public DirectoryInfo DirectoryInfo;
            public FileInfo FileInfo;
            public double[,] DoubleArray2D;
            public double DoubleProperty { get; private set; }

            [JsonIgnore]
            public double IgnoredDouble;

            public ComplexClass()
            {
                NullableBool = true;
                Enum = AnEnum.SecondValue;
                NullableDouble = 123.456;
                NullableInt = 2468;
                Float = 654.321f;
                NullableFloat = 3.1415f;
                Bool = false;
                SimpleDict = new Dictionary<string, string>() { { "foo", "bar" }, { "bar", "baz" } };
                NullableIntKeyedDateTimeDict = new Dictionary<int?, DateTime>() { { 1, new DateTime(2010, 1, 2, 3, 4, 5) }, { 2, new DateTime(1955, 11, 5) } };
                DoubleArray = new double[] { 1.2, 3.4, 5.6 };
                BoolList = new List<bool>() { false, false, false, true, true, true, false, false, false };
                Children = null;
                PlainTuple = new Tuple<string, int, bool>("tuple", 22, true);
                IPEndPoint = new IPEndPoint(IPAddress.Loopback, 1234);
                DirectoryInfo = new DirectoryInfo(@"C:\Windows");
                FileInfo = new FileInfo(@"C:\Windows\notepad.exe");
                DoubleArray2D = new double[3, 3] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } };
                DoubleProperty = 555.222;
            }

            public ComplexClass(
                bool? nullableBool,
                AnEnum anEnum,
                double? nullableDouble,
                int? nullableInt,
                float aFloat,
                float? nullableFloat,
                bool aBool,
                Dictionary<string, string> simpleDict,
                Dictionary<int?, DateTime> nullableIntKeyedDateTimeDict,
                double[] doubleArray,
                List<bool> boolList,
                Dictionary<string, List<(int, ComplexClass)>> children,
                Tuple<string, int, bool> plainTuple,
                IPEndPoint ipEndPoint,
                DirectoryInfo directoryInfo,
                FileInfo fileInfo,
                double[,] doubleArray2D,
                double doubleProperty)
            {
                NullableBool = nullableBool;
                Enum = anEnum;
                NullableDouble = nullableDouble;
                Float = aFloat;
                NullableFloat = nullableFloat;
                Bool = aBool;
                SimpleDict = simpleDict;
                NullableIntKeyedDateTimeDict = nullableIntKeyedDateTimeDict;
                DoubleArray = doubleArray;
                BoolList = boolList;
                Children = children;
                PlainTuple = plainTuple;
                IPEndPoint = ipEndPoint;
                DirectoryInfo = directoryInfo;
                FileInfo = fileInfo;
                DoubleArray2D = doubleArray2D;
                DoubleProperty = doubleProperty;
            }

            public static ComplexClass MakeExample1()
            {
                return new ComplexClass(
                    null,
                    AnEnum.AThirdValue,
                    42.42,
                    1357,
                    123.321f,
                    null,
                    true,
                    new Dictionary<string, string>() { { "hello", "world" }, { "Hello", "World" } },
                    new Dictionary<int?, DateTime>() { { 9000, new DateTime(2015, 12, 21, 7, 8, 9) }, { 1000, new DateTime(2020, 3, 4, 5, 9, 8, 7) } },
                    new double[] { 100.001, 200.002, 300.003, 400 },
                    new List<bool>() { true, false, true, false, true },
                    new Dictionary<string, List<(int, ComplexClass)>>()
                    {
                        { "FOO", new List<(int, ComplexClass)>() { (1, new ComplexClass()), (10, null) } },
                        { "BAR", new List<(int, ComplexClass)>() { (333, null), (222, new ComplexClass()) } }
                    },
                    new Tuple<string, int, bool>("example1", 777, false),
                    new IPEndPoint(IPAddress.IPv6Loopback, 4321),
                    new DirectoryInfo(@"C:\Users"),
                    new FileInfo(@"C:\Windows\explorer.exe"),
                    new double[3, 3] { { 9, 6, 3 }, { 8, 5, 62}, { 7, 4, 1 } },
                    333.444);
            }

            public static string NotEqualBecause(ComplexClass obj1, ComplexClass obj2)
            {
                if (obj1 == null)
                {
                    if (obj2 == null)
                    {
                        return null;
                    } else
                    {
                        return "ComplexClass1 was null, but ComplexClass2 was not null";
                    }
                }
                if (obj2 == null)
                {
                    return "ComplexClass1 was not null, but ComplexClass2 was null";
                }
                if (obj1.NullableBool != obj2.NullableBool) return "NullableBool value mismatch";
                if (obj1.Enum != obj2.Enum) return "Enum value mismatch";
                if (obj1.NullableDouble != obj2.NullableDouble) return "NullableDouble value mismatch";
                if (obj1.NullableInt != obj2.NullableInt) return "NullableInt value mismatch";
                if (obj1.Float != obj2.Float) return "Float value mismatch";
                if (obj1.NullableFloat != obj2.NullableFloat) return "NullableFloat value mismatch";
                if (obj1.Bool != obj2.Bool) return "Bool value mismatch";

                if (obj1.SimpleDict.Count != obj2.SimpleDict.Count) return "SimpleDict entry count mismatch";
                foreach (string key in obj1.SimpleDict.Keys)
                {
                    if (!obj2.SimpleDict.ContainsKey(key)) return "SimpleDict key set mismatch with key " + key;
                    if (obj1.SimpleDict[key] != obj2.SimpleDict[key]) return "SimpleDict value mismatch for key " + key;
                }

                if (obj1.NullableIntKeyedDateTimeDict.Count != obj2.NullableIntKeyedDateTimeDict.Count) return "NullableIntKeyedDateTimeDict entry count mismatch";
                foreach (int? key in obj1.NullableIntKeyedDateTimeDict.Keys)
                {
                    if (!obj2.NullableIntKeyedDateTimeDict.ContainsKey(key)) return "NullableIntKeyedDateTimeDict key set mismatch";
                    if (obj1.NullableIntKeyedDateTimeDict[key] != obj2.NullableIntKeyedDateTimeDict[key]) return "NullableIntKeyedDateTimeDict value mismatch for key " + key;
                }

                if ((obj1.DoubleArray == null) != (obj1.DoubleArray == null)) return "DoubleArray null status mismatch";
                if (obj1.DoubleArray.Length != obj2.DoubleArray.Length) return "DoubleArray length mismatch";
                for (int i = 0; i < obj1.DoubleArray.Length; i++)
                {
                    if (obj1.DoubleArray[i] != obj2.DoubleArray[i]) return "DoubleArray value mismatch at index " + i;
                }

                if ((obj1.BoolList == null) != (obj1.BoolList == null)) return "BoolList null status mismatch";
                if (obj1.BoolList.Count != obj2.BoolList.Count) return "BoolList length mismatch";
                for (int i = 0; i < obj1.BoolList.Count; i++)
                {
                    if (obj1.BoolList[i] != obj2.BoolList[i]) return "BoolList value mismatch at index " + i;
                }

                if ((obj1.Children == null) != (obj1.Children == null)) return "Children null status mismatch";
                if (obj1.Children != null)
                {
                    if (obj1.Children.Count != obj2.Children.Count) return "Children entry count mismatch";
                    foreach (string key in obj1.Children.Keys)
                    {
                        if (!obj2.Children.ContainsKey(key)) return "Children key set mismatch with key " + key;
                        List<(int, ComplexClass)> l1 = obj1.Children[key];
                        List<(int, ComplexClass)> l2 = obj2.Children[key];
                        if ((l1 == null) != (l1 == null)) return "Children null status mismatch at key " + key;
                        if (l1.Count != l2.Count) return "Children length mismatch at key " + key;
                        for (int i = 0; i < l1.Count; i++)
                        {
                            (int, ComplexClass) t1 = l1[i];
                            (int, ComplexClass) t2 = l2[i];
                            if (t1.Item1 != t2.Item1) return "Children Item1 value mismatch at key " + key + ", index " + i;
                            string notEqualBecause = ComplexClass.NotEqualBecause(t1.Item2, t2.Item2);
                            if (notEqualBecause != null) return "Children Item2 value mismatch at key " + key + ", index " + i + ": " + notEqualBecause;
                        }
                    }
                }

                if (obj1.PlainTuple.Item1 != obj2.PlainTuple.Item1) return "PlainTuple.Item1 value mismatch";
                if (obj1.PlainTuple.Item2 != obj2.PlainTuple.Item2) return "PlainTuple.Item2 value mismatch";
                if (obj1.PlainTuple.Item3 != obj2.PlainTuple.Item3) return "PlainTuple.Item3 value mismatch";

                if ((obj1.IPEndPoint == null) != (obj2.IPEndPoint == null)) return "IPEndPoint null status mismatch";
                if (obj1.IPEndPoint != null)
                {
                    if (obj1.IPEndPoint.ToString() != obj2.IPEndPoint.ToString()) return "IPEndPoint value mismatch";
                }

                if ((obj1.DirectoryInfo == null) != (obj2.DirectoryInfo == null)) return "DirectoryInfo null status mismatch";
                if (obj1.DirectoryInfo.FullName != obj2.DirectoryInfo.FullName) return "DirectoryInfo value mismatch";

                if ((obj1.FileInfo == null) != (obj2.FileInfo == null)) return "FileInfo null status mismatch";
                if (obj1.FileInfo.FullName != obj2.FileInfo.FullName) return "FileInfo value mismatch";

                if ((obj1.DoubleArray2D == null) != (obj2.DoubleArray2D == null)) return "DoubleArray2D null status mismatch";
                if (obj1.DoubleArray2D != null)
                {
                    if (obj1.DoubleArray2D.GetLength(0) != obj2.DoubleArray2D.GetLength(0)) return "DoubleArray2D length mismatch in first dimension";
                    if (obj1.DoubleArray2D.GetLength(1) != obj2.DoubleArray2D.GetLength(1)) return "DoubleArray2D length mismatch in second dimension";
                    for (int i = 0; i < obj1.DoubleArray2D.GetLength(0); i++)
                    {
                        for (int j = 0; j < obj2.DoubleArray2D.GetLength(1); j++)
                        {
                            if (obj1.DoubleArray2D[i, j] != obj2.DoubleArray2D[i, j]) return "DoubleArray2D value mismatch at element " + i + ", " + j;
                        }
                    }
                }

                if (obj1.DoubleProperty != obj2.DoubleProperty) return "DoubleProperty value mismatch";

                return null;
            }
        }
    }
}
