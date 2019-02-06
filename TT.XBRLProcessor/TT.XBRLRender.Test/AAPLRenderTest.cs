using System;
using TT.XBRLProcessor;
using TT.XBRLRender;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using log4net;

namespace TT.XBRLRender.Test
{

    [TestClass]
    public class AAPLRenderTest
    {
        private static XBRLRender xBRLRender;


        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            xBRLRender = new XBRLRender();
            xBRLRender.HTMLRender(@"e:\sec\aapl-20170930.xml");
        }

        [TestMethod]
        public void RenderConceptTest()
        {
            Assert.AreEqual(xBRLRender.XBRLModel.ConceptByQNameDict.Count, 20895);
        }

        [TestMethod]
        public void RenderFactTest1()
        {
            Assert.AreEqual(xBRLRender.XBRLModel.FactByQnameDict.Count, 368);
        }

        [TestMethod]
        public void RenderFactTest2()
        {
            Assert.AreEqual(xBRLRender.XBRLModel.FactList.Count, 1511);
        }

        [TestMethod]
        public void RenderContextTest()
        {
            Assert.AreEqual(xBRLRender.XBRLModel.ContextDict.Count, 426);
        }

        [TestMethod]
        public void RenderUnitTest()
        {
            Assert.AreEqual(xBRLRender.XBRLModel.UnitDict.Count, 10);
        }

        [TestMethod]
        public void RenderArcroleTypeTest()
        {
            Assert.AreEqual(xBRLRender.XBRLModel.ArcroleTypeDict.Count, 7);
        }

        [TestMethod]
        public void RenderRoleTypeTest()
        {
            Assert.AreEqual(xBRLRender.XBRLModel.RoleTypeDict.Count, 658);
        }

        [TestMethod]
        public void RenderBasesetTest()
        {
            Assert.AreEqual(xBRLRender.XBRLModel.BaseSets.Count, 464);
        }

        [TestMethod]
        public void RenderNamspcaceDocTest()
        {
            Assert.AreEqual(xBRLRender.XBRLModel.NamespaceDocsDict.Count, 24);
        }

        [TestMethod]
        public void RenderDocTest()
        {
            Assert.AreEqual(xBRLRender.XBRLModel.DocumentDict.Count, 33);
        }

        [TestMethod]
        public void RenderCalLinkbaseTest()
        {
            List<XBRLDoc> list = xBRLRender.XBRLModel.DocumentDict.Values.ToList();

            Assert.AreEqual(list[24].HrefDocList.Count, 247);
        }

        [TestMethod]
        public void RenderDefLinkbaseTest()
        {
            List<XBRLDoc> list = xBRLRender.XBRLModel.DocumentDict.Values.ToList();

            Assert.AreEqual(list[25].HrefDocList.Count, 552);
        }

        [TestMethod]
        public void RenderLabLinkbaseTest()
        {
            List<XBRLDoc> list = xBRLRender.XBRLModel.DocumentDict.Values.ToList();

            Assert.AreEqual(list[26].HrefDocList.Count, 697);
        }

        [TestMethod]
        public void RenderPreLinkbaseTest()
        {
            List<XBRLDoc> list = xBRLRender.XBRLModel.DocumentDict.Values.ToList();

            Assert.AreEqual(list[27].HrefDocList.Count, 916);
        }

        [TestMethod]
        public void RenderLinkModelBaseSetTest()
        {
            BaseSetKey baseSetKey = new BaseSetKey(XBRLConstants.Arcrole_ParentChild, string.Empty, null, null);

            List<LinkModel> linkModels = xBRLRender.XBRLModel.GetLinkModels(baseSetKey);
            Assert.AreEqual(linkModels.Count, 74);
        }
    }
}
