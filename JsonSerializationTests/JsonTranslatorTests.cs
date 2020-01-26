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
            string json = translator.MakeJson<Classes.BasicClass>(bc1).ToString();
            Classes.BasicClass bc2 = translator.MakeObject<Classes.BasicClass>(JsonObject.Parse(json));
            Assert.IsTrue(Classes.BasicClass.AreEqual(bc1, bc2));
        }

        [TestMethod()]
        public void MakeObjectTest_Complex()
        {
            Classes.ComplexClass cc1 = Classes.ComplexClass.MakeExample1();
            var translator = new JsonTranslator();
            string json = translator.MakeJson<Classes.ComplexClass>(cc1).ToMultilineString();
            Classes.ComplexClass cc2 = translator.MakeObject<Classes.ComplexClass>(JsonObject.Parse(json));
            string notEqualBecause = Classes.ComplexClass.NotEqualBecause(cc1, cc2);
            Assert.IsNull(notEqualBecause, notEqualBecause);
        }
    }
}