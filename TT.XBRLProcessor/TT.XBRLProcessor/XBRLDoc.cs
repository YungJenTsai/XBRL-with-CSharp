using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Linq;
using System.Diagnostics;

namespace TT.XBRLProcessor
{
    //XBRL instance document
    public class XBRLDoc : DocObject
    {
        internal XDocument _xDoc;

        private XmlSchema _xmlSchema;
        private XBRLModel _xBRLModel;
        List<Href> _hrefDocList;
        private Dictionary<string, Concept> _idConceptDict;

        public Dictionary<string, Concept> IDConceptDict
        {
            get => _idConceptDict;
            set => _idConceptDict = value;
        }


        public List<Href> HrefDocList { get => _hrefDocList; set => _hrefDocList = value; }

        public XBRLModel XBRLModel
        {
            get { return _xBRLModel; }
            set { _xBRLModel = value; }
        }


        public XmlSchema XmlSchema
        {
            get { return _xmlSchema; }
            set { _xmlSchema = value; }
        }


        public XBRLDoc(string uri, XBRLModel xbrlModel) : base(uri)
        {
            XBRLModel = xbrlModel;
            XmlSchema = null;
            HrefDocList = new List<Href>();
            IDConceptDict = new Dictionary<string, Concept>();
        }

        public XBRLDoc(Uri uri, XBRLModel xbrlModel) : base (uri)
        {
            XBRLModel = xbrlModel;
            XmlSchema = null;
            HrefDocList = new List<Href>();
            IDConceptDict = new Dictionary<string, Concept>();
        }

        public XBRLDoc(XmlSchema xmlSchema, XBRLModel xbrlModel) : base(xmlSchema.SourceUri)
        {
            XmlSchema = xmlSchema;
            XBRLModel = xbrlModel;
            HrefDocList = new List<Href>();
            DocType = DocumentType.Schema;
            IDConceptDict = new Dictionary<string, Concept>();
        }
        // NormalizedURI
        public void Load(/*XBRLModel xBrlModel, string uri, XBRLDoc parentDoc, string nameSpace, bool reloadCache */)
        {
            // Create a resolver with default credentials.
            /*
            UrlCachingResolver resolver = new UrlCachingResolver(false);
            resolver.Credentials = System.Net.CredentialCache.DefaultCredentials;

            // Set the reader settings object to use the resolver.
            var settings = new XmlReaderSettings();
            settings.XmlResolver = resolver;

            // Create the XmlReader object.
            XmlReader reader = XmlReader.Create(DocURI.AbsoluteUri, settings);
            */

            _xDoc = XDocument.Load(DocURI.AbsoluteUri, LoadOptions.SetLineInfo);

            if (_xDoc != null)
            {
                if (_xDoc.Root.Name.Namespace == XBRLConstants.XbrlNamespaceUri)
                {
                    switch (_xDoc.Root.Name.LocalName)
                    {
                        case "xbrl":
                            DocType = DocumentType.Instance;
                            break;
                        default:
                            DocType = DocumentType.UnknownXml;
                            break;
                    }
                }
                else if (_xDoc.Root.Name.Namespace == XBRLConstants.XbrlLinkbaseNamespaceUri)
                {
                    switch (_xDoc.Root.Name.LocalName)
                    {
                        case "xbrl":
                            DocType = DocumentType.Instance;
                            break;
                        case "linkbase":
                            DocType = DocumentType.Linkbase;
                            break;
                        default:
                            DocType = DocumentType.UnknownXml;
                            break;
                    }
                }
                else if (_xDoc.Root.Name.Namespace == XBRLConstants.XmlSchemaInstanceNamespaceUri ||
                    _xDoc.Root.Name.Namespace == XBRLConstants.XmlSchemaNamespaceUri)
                {
                    switch (_xDoc.Root.Name.LocalName)
                    {
                        case "schema":
                            DocType = DocumentType.Schema;
                            break;
                        default:
                            DocType = DocumentType.UnknownXml;
                            break;
                    }
                }
            }
        }

        public string GetPrefixOfNamespace(string nameSpace)
        {
            return _xDoc.Root.GetPrefixOfNamespace(nameSpace);
        }

        public XNamespace GetNamespaceOfPrefix(string prefix)
        {
            return _xDoc.Root.GetNamespaceOfPrefix(prefix);
        }

        public XDocument XDoc { get { return _xDoc; } private set { } }

        internal void DiscoverSchemaElements()
        {
            foreach (XmlSchemaObject schemaObj in XmlSchema.Items)
            {
                if (schemaObj is XmlSchemaAnnotation)
                {
                    foreach (XmlSchemaObject appInfo in (schemaObj as XmlSchemaAnnotation).Items)
                    {
                        if (appInfo is XmlSchemaAppInfo)
                        {
                            CollectAppInfo((appInfo as XmlSchemaAppInfo).Markup);
                        }
                    }

                }
                else if (schemaObj is XmlSchemaElement)
                {
                    XmlSchemaElement xmlSchemaElement = schemaObj as XmlSchemaElement;

                    if (!XBRLModel.ConceptByQNameDict.ContainsKey(xmlSchemaElement.QualifiedName))
                    {
                        Concept concept = new Concept(XBRLModel, xmlSchemaElement);
                        if(concept.ID != null)
                            IDConceptDict.Add(concept.ID, concept);
                        XBRLModel.ConceptByQNameDict.Add(xmlSchemaElement.QualifiedName, concept);
                    }
                    else
                    {
                        Debug.Assert(false);
                    }
                    // else
                    // Debug.Assert(_nameConceptDict[xmlSchemaElement.Name] == xmlSchemaElement);
                }
            }

        }

        private void CollectAppInfo(XmlNode[] xmlNodes)
        {
            foreach (XmlNode xmlNode in xmlNodes)
            {
                if (XBRLConstants.XbrlLinkbaseNamespaceUri == xmlNode.NamespaceURI)
                {
                    switch (xmlNode.LocalName)
                    {
                        case "roleType":
                            CollectRoleTypes(xmlNode);
                            break;
                        case "arcroleType":
                            CollectArcroleTypes(xmlNode);
                            break;
                        case "linkbaseRef":
                            LinkbaseRefDiscover(xmlNode);
                            break;
                        case "linkbase":
                            //LinkbaseDiscover(xmlNode);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void LinkbaseRefDiscover(XmlNode xmlNode)
        {

            if (xmlNode.LocalName == "linkbaseRef")
            {
                foreach (XmlAttribute attribute in xmlNode.Attributes)
                {
                    if (XBRLConstants.XLinkNamespaceUri == attribute.NamespaceURI && attribute.LocalName == "href")
                    {
                        XBRLDoc linkbaseDoc = DiscoverHref(xmlNode, attribute.Value);

                        if (!XBRLModel.AddDocument(linkbaseDoc))
                            return;

                        linkbaseDoc.LinkbaseDiscover();
                    }
                }
            }
        }

        internal void LinkbaseDiscover()
        {
            string[] roleNamespaceURI = new string[2] { XBRLConstants.XbrlLinkbaseRoleRefNamespaceUri, XBRLConstants.XbrlLinkbaseArcroleRefNamespaceUri };

            XElement node = (XElement) XDoc.Root.FirstNode;

            while (node != null)
            {
                if (node.Name == XBRLConstants.XbrlLinkbaseRoleRefNamespaceUri ||
                    node.Name == XBRLConstants.XbrlLinkbaseArcroleRefNamespaceUri)
                {
                    if (node.Name.LocalName == "roleRef" || node.Name.LocalName == "arcroleRef")
                    {
                        LinkElement linkElement = new LinkElement(node);
                        LinkbaseHrefDiscover(linkElement);
                    }

                    node = (XElement)node.NextNode;

                    continue;
                }

                if (node.Attribute(XBRLConstants.XLinkTypeNamespaceUri)?.Value == "extended")
                {
                    LinkModel linkModel = new LinkModel(node);

                    linkModel.XBRLDoc = this;

                    if (linkModel.IsLinkModel)
                    {
                        // LocateSchemaOfElementNamespace();
                        XmlQualifiedName linkQName = linkModel.QName;
                        string linkRole = linkModel.LinkRole;
                        bool isStandardExtLink = linkModel.IsLinkModel;

                        HashSet<string> arcRolesFound = new HashSet<string>();
                        bool dimensionArcFound = false;
                        bool formulaFound = false;
                        bool tableRenderingArcFound = false;

                        // IsStandardResourceOrExtLinkElement(); // #651 ModelDocument.Py
                        XElement childNode = (XElement)linkModel.FirstChild;
                        int linkElementSequence = 0;

                        while (childNode != null)
                        {
                            LinkElement linkElement = new LinkElement(childNode);

                            linkElement.LinkModel = linkModel;

                            LinkElement modelResource = null;

                            linkElementSequence++;

                            linkElement.ElementSequence = linkElementSequence;

                            string xLinkType = linkElement.XLinkType;

                            if (xLinkType == "locator")
                            {
                                Href href = LinkbaseHrefDiscover(linkElement);

                                if (href == null)
                                {
                                    Errors("locater href is wrong");
                                }
                                else
                                {
                                    linkElement.ModelHRef = href;
                                    modelResource = linkElement;
                                }
                            }
                            else if (xLinkType == "arc")
                            {
                                XmlQualifiedName arcQName = linkElement.QName;
                                string arcRole = linkElement.ArcRole;

                                linkModel.ArcLinkElement.Add(linkElement);

                                if (!arcRolesFound.Contains(arcRole))
                                {
                                    if (linkRole == string.Empty)
                                    {
                                        linkRole = XBRLConstants.XbrlDefautLinkRoleNamespaceUri;
                                    }



                                    List<BaseSetKey> baseSetKeys = new List<BaseSetKey>();
                                    baseSetKeys.Add(new BaseSetKey(arcRole, linkRole, linkQName, arcQName));
                                    baseSetKeys.Add(new BaseSetKey(arcRole, linkRole, null, null));
                                    baseSetKeys.Add(new BaseSetKey(arcRole, string.Empty, null, null));

                                    if (arcRole.StartsWith("http://xbrl.org/int/dim/arcrole/") && !dimensionArcFound)
                                    {
                                        baseSetKeys.Add(new BaseSetKey("XBRL-dimensions", string.Empty, null, null));
                                        baseSetKeys.Add(new BaseSetKey("XBRL-dimensions", linkRole, null, null));
                                        dimensionArcFound = true;
                                    }

                                    if (LinkElement.IsFormulaArcRole(arcRole) && !formulaFound)
                                    {
                                        baseSetKeys.Add(new BaseSetKey("XBRL-formula", string.Empty, null, null));
                                        baseSetKeys.Add(new BaseSetKey("XBRL-formula", linkRole, null, null));
                                        formulaFound = true;
                                    }

                                    foreach (BaseSetKey baseSetKey in baseSetKeys)
                                    {
                                        if (XBRLModel.BaseSets.ContainsKey(baseSetKey))
                                        {
                                            List<LinkModel> value = XBRLModel.BaseSets[baseSetKey];
                                            value.Add(linkModel);
                                        }
                                        else
                                        {
                                            List<LinkModel> linkModelList = new List<LinkModel>();
                                            linkModelList.Add(linkModel);
                                            XBRLModel.BaseSets.Add(baseSetKey, linkModelList);
                                        }
                                    }
                                    arcRolesFound.Add(arcRole);
                                }
                            }
                            else if (xLinkType == "resource")
                            {
                                modelResource = linkElement;
                            }

                            if (modelResource != null)
                            {
                                string linkLabel = linkElement.LinkLabel;
                                if (linkModel.LabelResources.ContainsKey(linkLabel))
                                {
                                    List<LinkElement> value = linkModel.LabelResources[linkLabel];
                                    value.Add(modelResource);
                                }
                                else
                                {
                                    List<LinkElement> linkElementList = new List<LinkElement>();
                                    linkElementList.Add(modelResource);
                                    linkModel.LabelResources.Add(linkLabel, linkElementList);
                                }
                            }

                            childNode = (XElement)childNode.NextNode;

                        }

                    }

                }
                node = (XElement)node.NextNode;
            }
        }

        private Href LinkbaseHrefDiscover(LinkElement linkElement)
        {
            string id = string.Empty;
            string url = string.Empty;
            bool isHref = false;
            XElement xElement = linkElement.BaseElement;

            foreach (XAttribute attribute in xElement.Attributes())
            {
                if (XBRLConstants.XLinkNamespaceUri == attribute.Name.Namespace && attribute.Name.LocalName == "href")
                {
                    string hrefStr = attribute.Value;

                    int splitIndex = hrefStr.IndexOf('#');
                    switch (splitIndex)
                    {
                        case -1:
                            id = hrefStr;
                            break;
                        case 0:
                            id = hrefStr.Substring(1);
                            break;
                        default:
                            string[] values = hrefStr.Split('#');
                            url = values[0];
                            id = values[1];
                            break;
                    }

                    isHref = true;

                    break;
                }
            }

            XBRLDoc newDoc;

            if(url != string.Empty)
            {
                Uri uri = XBRLModel.NormalizedPath(DocURI, url);

                newDoc = XBRLModel.FindDOCWithURI(uri);

                if(newDoc == null)
                {
                    newDoc = XBRLModel.AddHrefDoc(uri);
                }
            }
            else
            {
                newDoc = this;
            }
            //Debug.Assert(isHref);

            Href href = null;

            if (isHref)
            {
                href = new Href(newDoc, linkElement, id);
                HrefDocList.Add(href);
            }
            else
                Errors("LinkbaseHrefDiscover href handle is null");
            return href;
        }




        private XBRLDoc DiscoverHref(XmlNode xmlNode, string href)
        {

            XBRLDoc linkbaseDoc = new XBRLDoc(XBRLModel.NormalizedPath(DocURI, href).AbsoluteUri, XBRLModel);

            //Debug.Assert(linkbaseDoc.DocType == DocumentType.Linkbase);
 
            linkbaseDoc.Load();

            //            _hrefDocList.Add((href, linkbaseDoc, xmlNode));
            return linkbaseDoc;
        }

        public void Errors(string errorMessage)
        {
            Debug.Assert(false, errorMessage);
        }

        private void CollectRoleTypes(XmlNode xmlNode)
        {
            RoleType roleType = new RoleType(xmlNode);
            if (XBRLModel.RoleTypeDict.ContainsKey(roleType.RoleURI))
            {
                List<RoleType> value = XBRLModel.RoleTypeDict[roleType.RoleURI];
                value.Add(roleType);
            }
            else
            {
                List<RoleType> roleTypeList = new List<RoleType>();
                roleTypeList.Add(roleType);
                XBRLModel.RoleTypeDict.Add(roleType.RoleURI, roleTypeList);
            }
        }

        private void CollectArcroleTypes(XmlNode xmlNode)
        {
            ArcroleType arcroleType = new ArcroleType(xmlNode);
            if (XBRLModel.ArcroleTypeDict.ContainsKey(arcroleType.ArcroleURI))
            {
                List<ArcroleType> value = XBRLModel.ArcroleTypeDict[arcroleType.ArcroleURI];
                value.Add(arcroleType);
            }
            else
            {
                List<ArcroleType> arcroleTypeList = new List<ArcroleType>();
                arcroleTypeList.Add(arcroleType);
                XBRLModel.ArcroleTypeDict.Add(arcroleType.ArcroleURI, arcroleTypeList);
            }
        }

    }

}
