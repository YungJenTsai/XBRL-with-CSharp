using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TT.XBRLProcessor;
using TT.Utilities;
using log4net;
using System.Collections.Generic;
using System.Linq;

namespace TT.XBRLProcessor.Test
{
    [TestClass]
    public class AAPLFileTest
    {
        static XBRLModel xBRLModel;


        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            xBRLModel = new XBRLModel();
            xBRLModel.Load(@"E:\sec\aapl-20170930.xml");
        }

        [TestMethod]
        public void ConceptTest()
        {
            Assert.AreEqual(xBRLModel.ConceptByQNameDict.Count, 20895); 
        }

        [TestMethod]
        public void FactTest1()
        {
            Assert.AreEqual(xBRLModel.FactByQnameDict.Count, 368);
        }

        [TestMethod]
        public void FactTest2()
        {
            Assert.AreEqual(xBRLModel.FactList.Count, 1511);
        }

        [TestMethod]
        public void ContextTest()
        {
            Assert.AreEqual(xBRLModel.ContextDict.Count, 426);
        }

        [TestMethod]
        public void UnitTest()
        {
            Assert.AreEqual(xBRLModel.UnitDict.Count, 10);
        }

        [TestMethod]
        public void ArcroleTypeTest()
        {
            Assert.AreEqual(xBRLModel.ArcroleTypeDict.Count, 7);
        }

        [TestMethod]
        public void RoleTypeTest()
        {
            Assert.AreEqual(xBRLModel.RoleTypeDict.Count, 656);
        }

        [TestMethod]
        public void BasesetTest()
        {
            Assert.AreEqual(xBRLModel.BaseSets.Count, 461);
        }

        [TestMethod]
        public void NamspcaceDocTest()
        {
            Assert.AreEqual(xBRLModel.NamespaceDocsDict.Count, 23);
        }

        [TestMethod]
        public void DocTest()
        {
            Assert.AreEqual(xBRLModel.DocumentDict.Count, 28);
        }

        [TestMethod]
        public void CalLinkbaseTest()
        {
            List<XBRLDoc> list = xBRLModel.DocumentDict.Values.ToList();

            Assert.AreEqual(list[24].HrefDocList.Count, 247);
        }

        [TestMethod]
        public void DefLinkbaseTest()
        {
            List<XBRLDoc> list = xBRLModel.DocumentDict.Values.ToList();

            Assert.AreEqual(list[25].HrefDocList.Count, 552);
        }

        [TestMethod]
        public void LabLinkbaseTest()
        {
            List<XBRLDoc> list = xBRLModel.DocumentDict.Values.ToList();

            Assert.AreEqual(list[26].HrefDocList.Count, 697);
        }

        [TestMethod]
        public void PreLinkbaseTest()
        {
            List<XBRLDoc> list = xBRLModel.DocumentDict.Values.ToList();

            Assert.AreEqual(list[27].HrefDocList.Count, 916);
        }

        [TestMethod]
        public void LinkModelBaseSetTest()
        {
            BaseSetKey baseSetKey = new BaseSetKey(XBRLConstants.Arcrole_ParentChild, string.Empty, null, null);

            List<LinkModel> linkModels = xBRLModel.GetLinkModels(baseSetKey);
            Assert.AreEqual(linkModels.Count, 74);
        }
    }
}
