using Microsoft.VisualStudio.TestTools.UnitTesting;
using GruntWurk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GruntWurk.Tests {
    [TestClass()]
    public class PageProfileTests {
        [TestMethod()]
        public void AdjustedDataLinesEndTest() {
            PageProfile prof = new PageProfile();

            prof.SetDataLines("3..9");
            Assert.AreEqual(9,prof.AdjustedDataLinesEnd(99));

            prof.SetDataLines("3..N");
            Assert.AreEqual(99, prof.AdjustedDataLinesEnd(99));

            prof.SetDataLines("3..N-9");
            Assert.AreEqual(90, prof.AdjustedDataLinesEnd(99));
        }
    }
}