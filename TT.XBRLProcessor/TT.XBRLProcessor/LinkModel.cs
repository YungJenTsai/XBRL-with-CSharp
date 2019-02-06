using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

namespace TT.XBRLProcessor
{
    public class LinkModel
    {
        public static XmlQualifiedName DefinitionLinkQN = new XmlQualifiedName("{http://www.xbrl.org/2003/linkbase}definitionLink");
        public static XmlQualifiedName CalculationLinkQN = new XmlQualifiedName("{http://www.xbrl.org/2003/linkbase}calculationLink");
        public static XmlQualifiedName PresentationLinkQN = new XmlQualifiedName("{http://www.xbrl.org/2003/linkbase}presentationLink");
        public static XmlQualifiedName ReferenceLinkQN = new XmlQualifiedName("{http://www.xbrl.org/2003/linkbase}referenceLink");
        public static XmlQualifiedName LabelLinkQN = new XmlQualifiedName("{http://www.xbrl.org/2003/linkbase}labelLink");
        public static XmlQualifiedName FootnoteLinkQN = new XmlQualifiedName("{http://www.xbrl.org/2003/linkbase}footnoteLink");
        public enum LinkModelType { None, CalcualationLink, DefinitionLink, LabelLink, PresentationLink, ReferenceLink, FootnoteLink };

        private XmlQualifiedName _qName;
        private List<LinkElement> _arcLinkElement;
        public XBRLDoc XBRLDoc { get; set; }

        public List<LinkElement> ArcLinkElement
        {
            get { return _arcLinkElement; }
            set { _arcLinkElement = value; }
        }


        public XmlQualifiedName QName
        {
            get { return _qName; }
            set { _qName = value; }
        }

        public Dictionary<string, List<LinkElement>> LabelResources { get; set; }

        public LinkModelType ModelType
        {
            get; set;
        }


        public XElement BaseElement
        {
            get; set;
        }
        public LinkModel(XElement xElement)
        {
            BaseElement = xElement;

            QName = new XmlQualifiedName(BaseElement.Name.LocalName, BaseElement.Name.NamespaceName);

            LabelResources = new Dictionary<string, List<LinkElement>>();

            ArcLinkElement = new List<LinkElement>();

            switch (xElement.Name.LocalName)
            {
                case "calculationLink":
                    ModelType = LinkModelType.CalcualationLink;
                    break;
                case "definitionLink":
                    ModelType = LinkModelType.DefinitionLink;
                    break;
                case "labelLink":
                    ModelType = LinkModelType.LabelLink;
                    break;
                case "presentationLink":
                    ModelType = LinkModelType.PresentationLink;
                    break;
                case "footnoteLink":
                    ModelType = LinkModelType.FootnoteLink;
                    break;
                case "referenceLink":
                    ModelType = LinkModelType.ReferenceLink;
                    break;
                default:
                    ModelType = LinkModelType.None;
                    break;
            }
        }

        public bool IsLinkModel
        {
            get
            {
                return ModelType != LinkModelType.None;
            }
        }
        public string LinkRole
        {
            get
            {
                return BaseElement.Attribute(XBRLConstants.XLinkRoleNamespaceUri)?.Value;
            }
        }
        public XNode FirstChild
        {
            get
            {
                return BaseElement.FirstNode;
            }
        }
    }
}
