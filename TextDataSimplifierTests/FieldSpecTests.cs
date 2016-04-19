using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GruntWurk.Tests {
    [TestClass()]
    public class FieldSpecTests {
        [TestMethod()]
        public void FieldSpecTest() {
            FieldSpec fspec = new FieldSpec("Amount, Line 1, Col 10, Width 4");
            Assert.AreEqual("Amount", fspec.FieldName);
            Assert.AreEqual(1, fspec.LineNo);
            Assert.AreEqual(10, fspec.ColNo);
            Assert.AreEqual(4,fspec.Width);

            // Any order (except field name must come first)
            fspec = new FieldSpec("Amount, Width 4, Col 10, Line 1");
            Assert.AreEqual("Amount", fspec.FieldName);
            Assert.AreEqual(1, fspec.LineNo);
            Assert.AreEqual(10, fspec.ColNo);
            Assert.AreEqual(4, fspec.Width);

            // Assume line 1 if not given
            fspec = new FieldSpec("Amount, Col 10, Width 4");
            Assert.AreEqual("Amount", fspec.FieldName);
            Assert.AreEqual(1, fspec.LineNo);
            Assert.AreEqual(10, fspec.ColNo);
            Assert.AreEqual(4, fspec.Width);

            // Assume 0 if Width or Column not given (which will later be considered an error)
            fspec = new FieldSpec("Amount");
            Assert.AreEqual("Amount", fspec.FieldName);
            Assert.AreEqual(1, fspec.LineNo);
            Assert.AreEqual(0, fspec.ColNo);
            Assert.AreEqual(0, fspec.Width);

            fspec = new FieldSpec("Three Part Name, Width 4, Col 10, Line 1");
            Assert.AreEqual("Three Part Name", fspec.FieldName);
            Assert.AreEqual(1, fspec.LineNo);
            Assert.AreEqual(10, fspec.ColNo);
            Assert.AreEqual(4, fspec.Width);
        }
    }
}