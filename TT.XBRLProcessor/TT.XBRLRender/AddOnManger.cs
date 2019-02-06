using System;
using System.Collections.Generic;
using TT.XBRLProcessor;
using log4net;
using System.Xml.Schema;
using System.Xml.Linq;
using TT.Utilities;

namespace TT.XBRLRender
{
    public class AddOnManger
    {
        private XBRLModel xBRLModel;
        private ILog _logger;
        private XDocument _xDoc;

        public ILog Logger
        {
            get { return _logger; }
            set { _logger = value; }
        }

        public XBRLModel XBRLModel
        {
            get { return xBRLModel; }
            set { xBRLModel = value; }
        }

        public AddOnManger(XBRLModel xBRLModel)
        {
            XBRLModel = xBRLModel;
            Logger = Utilities.LogManager.GetLogger(typeof(AddOnManger));
            _xDoc = XDocument.Load(@"TaxonomyAddonManager.xml");
        }

        internal List<Uri> GetAddOnUriList()
        {
            // Get list of AddOn URLs

            HashSet<string> nameSpaceUriSet = new HashSet<string>();
            List<Uri> addOnUri = new List<Uri>();

            foreach(Fact fact in XBRLModel.FactList)
            {
                if (!nameSpaceUriSet.Contains(fact.Item.Name.NamespaceName))
                {
                    nameSpaceUriSet.Add(fact.Item.Name.NamespaceName);
                }
            }

            XmlSchemaSet schemaSet = XBRLModel.Schemas;

            foreach(XmlSchema schema in schemaSet.Schemas())
            {
                if (nameSpaceUriSet.Contains(schema.TargetNamespace))
                {
                    Uri uri = new Uri(schema.SourceUri);
                    string filename = System.IO.Path.GetFileName(uri.LocalPath);

                    if (filename.EndsWith(".xsd"))
                    {
                        foreach (XElement element in _xDoc.Descendants("TaxonomyAddon"))
                        {
                            XElement taxonomyElement = element.Element("Taxonomy");

                            if(taxonomyElement.Value == filename)
                            {
                                Logger.Info(taxonomyElement.Value);
                                string addOnFile;
                                Uri newUri;

                                if(element.Element("DefinitionFiles") != null)
                                {
                                    addOnFile = element.Element("DefinitionFiles").Element("string").Value;

                                    newUri = new Uri(uri, addOnFile);

                                    addOnUri.Add(newUri);
                                    Logger.Info(newUri.AbsoluteUri);

                                }

                                if (element.Element("ReferenceFiles") != null)
                                {
                                    addOnFile = element.Element("ReferenceFiles").Element("string").Value;

                                    newUri = new Uri(uri, addOnFile);

                                    addOnUri.Add(newUri);
                                    Logger.Info(newUri.AbsoluteUri);

                                }
                            }
                        }

                    }

                }
            }

            return addOnUri;
        }

        internal void LoadAddOnTaxonomy()
        {
            List<Uri> addOnUriList = GetAddOnUriList();

            foreach (Uri uri in addOnUriList)
            {
                xBRLModel.Load(uri);
            }
        }
    }
}
