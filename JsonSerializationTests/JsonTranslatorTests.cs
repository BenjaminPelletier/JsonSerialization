using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Json.Serialization.Tests
{
    [TestClass()]
    public class JsonTranslatorTests
    {
        [TestMethod()]
        public void MakeObjectTest_Basic()
        {
            var bc1 = new Classes.BasicClass("foo", 123, 456.789);
            var translator = new JsonTranslator();
            string json = translator.MakeJson(bc1).ToString();
            Classes.BasicClass bc2 = translator.MakeObject<Classes.BasicClass>(JsonObject.Parse(json));
            Assert.IsTrue(Classes.BasicClass.AreEqual(bc1, bc2));
        }

        [TestMethod()]
        public void MakeObjectTest_Complex()
        {
            Classes.ComplexClass cc1 = Classes.ComplexClass.MakeExample1();
            cc1.IgnoredDouble = 123;
            var translator = new JsonTranslator();
            string json = translator.MakeJson<Classes.ComplexClass>(cc1).ToMultilineString();
            cc1.IgnoredDouble = 321;
            Classes.ComplexClass cc2 = translator.MakeObject<Classes.ComplexClass>(JsonObject.Parse(json));
            string notEqualBecause = Classes.ComplexClass.NotEqualBecause(cc1, cc2);
            Assert.IsNull(notEqualBecause, notEqualBecause);
        }

        [TestMethod()]
        public void MakeObjectTest_Unicode()
        {
            var bc1 = new Classes.BasicClass("Bulletproof [feat. XYLØ]", 222, 99.99);
            string json = JsonTranslator.Singleton.MakeJson(bc1).ToString();
            Assert.IsTrue(json.IndexOf(@"\u00d8") > 0);
            Classes.BasicClass bc2 = JsonTranslator.Singleton.MakeObject<Classes.BasicClass>(JsonObject.Parse(json));
            Assert.IsTrue(Classes.BasicClass.AreEqual(bc1, bc2));
        }
    }
}