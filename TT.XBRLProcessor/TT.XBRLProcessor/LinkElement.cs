using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

namespace TT.XBRLProcessor
{
    public class LinkElement
    {
        static HashSet<string> _formulaArcRoleSet = new HashSet<string> {"http://xbrl.org/arcrole/2008/assertion-set",
                       "http://xbrl.org/arcrole/2008/variable-set",
                       "http://xbrl.org/arcrole/2008/variable-set-filter",
                       "http://xbrl.org/arcrole/2008/variable-filter",
                       "http://xbrl.org/arcrole/2008/boolean-filter",
                       "http://xbrl.org/arcrole/2008/variable-set-precondition",
                       "http://xbrl.org/arcrole/2008/consistency-assertion-formula",
                       "http://xbrl.org/arcrole/2010/function-implementation",
                       "http://xbrl.org/arcrole/2010/assertion-satisfied-message",
                       "http://xbrl.org/arcrole/2010/assertion-unsatisfied-message",
                       "http://xbrl.org/arcrole/PR/2015-11-18/assertion-unsatisfied-severity",
                       "http://xbrl.org/arcrole/2010/instance-variable",
                       "http://xbrl.org/arcrole/2010/formula-instance",
                       "http://xbrl.org/arcrole/2010/function-implementation",
                       "http://xbrl.org/arcrole/2010/variables-scope" };
        private XElement _baseElement;
        private int _elementSequence;
        private Href _modelHRef;
        private XmlQualifiedName _qName;

        public LinkModel LinkModel { get; set; }
        public XmlQualifiedName QName
        {
            get { return _qName; }
            set { _qName = value; }
        }


        public string ArcRole
        {
            get { return _baseElement.Attribute(XBRLConstants.XLinkArcRoleNamespaceUri)?.Value; }
            // set { _arcRole = value; }
        }

        public string LinkLabel
        {
            get { return _baseElement.Attribute(XBRLConstants.XLinkLabelNamespaceUri)?.Value; }
            // set { _arcRole = value; }
        }

        public string LinkToLabel
        {
            get { return _baseElement.Attribute(XBRLConstants.XLinkToLabelNamespaceUri)?.Value; }
            // set { _arcRole = value; }
        }

        public string LinkFromLabel
        {
            get { return _baseElement.Attribute(XBRLConstants.XLinkFromLabelNamespaceUri)?.Value; }
            // set { _arcRole = value; }
        }

        public Href ModelHRef
        {
            get { return _modelHRef; }
            set { _modelHRef = value; }
        }


        public string XLinkType
        {
            get { return _baseElement.Attribute(XBRLConstants.XLinkTypeNamespaceUri)?.Value; }
         //   set { _xLinkType = value; }
        }


        public int ElementSequence
        {
            get { return _elementSequence; }
            set { _elementSequence = value; }
        }

        public XElement BaseElement
        {
            get { return _baseElement; }
         //   set { _baseElement = value; }
        }

        public LinkElement(XElement xElement)
        {
            _baseElement = xElement;
            QName = new XmlQualifiedName(BaseElement.Name.LocalName, BaseElement.Name.NamespaceName);
        }

        public static bool IsFormulaArcRole(string arcRole)
        {
            return _formulaArcRoleSet.Contains(arcRole);
        }
    }
}
