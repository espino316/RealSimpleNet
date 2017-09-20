using Microsoft.VisualStudio.TestTools.UnitTesting;
using RealSimpleNet.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RealSimpleNet.Helpers.Tests
{
    [TestClass()]
    public class StringsTests
    {
        [TestMethod()]
        public void LeftTest()
        {
            Assert.AreEqual("Ho", Strings.Left("Hola", 2));
        } // end void LeftTest
        
        [TestMethod()]
        public void RightTest()
        {
            Assert.AreEqual("la", Strings.Right("Hola", 2));
        }

        [TestMethod()]
        public void MidTest()
        {
            Assert.AreEqual("ol", Strings.Mid("Hola", 1, 2));
        }

        [TestMethod()]
        public void ReverseTest()
        {
            Assert.AreEqual("aloH", Strings.Reverse("Hola"));
        }

        [TestMethod()]
        public void AddSpacesTest()
        {
            string expected = "   Hola   ";
            string actual = Strings.AddSpaces("Hola", 3);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void N2Test()
        {
            Assert.AreEqual("5,432.00", Strings.N2(5432));
        }

        [TestMethod()]
        public void HttpTest()
        {
            Http http = new Http();
            http.AddHeader("eso", "es");
            http.AddParameter("foo", "bar");
            string response = http.Request<string>("POST", "http://prosyss.com");
            Assert.AreEqual("", response);
        }
    } // end class StringsTests
} // end namespace RealSimpleNet.Helpers.Tests