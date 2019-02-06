using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Diagnostics;

namespace TT.XBRLProcessor
{
    public class Fact
    {
        private XBRLModel _xBRLModel;
        private XElement _xElement;
        private string _id = null;
        private string _contextRef = null;
        private string _unitRef = null;
        private XmlQualifiedName _qualifiedName;
        private bool _found;
        private Concept _concept;
        private int _lineNumber = 0;
        private string _lang = null;
        private Context _context;
        private Unit _unit;
        private string _value;


        public Concept Concept
        {
            get { return _concept; }
            set { _concept = value; }
        }

        public bool Found
        {
            get { return _found; }
            set { _found = value; }
        }

        public bool IsTuple
        {
            get => Concept.IsTuple;
        }

        public bool IsItem
        {
            get => Concept.IsItem;
        }

        public bool IsNumeric
        {
            get => Concept.IsNumeric;
        }

        public Fact(XBRLModel xBRLModel, XElement factNode)
        {
            _xBRLModel = xBRLModel;
            _xElement = factNode;
            _qualifiedName = new XmlQualifiedName(factNode.Name.LocalName, factNode.Name.Namespace.NamespaceName);

            if (xBRLModel.ConceptByQNameDict.ContainsKey(_qualifiedName))
            {
                Concept = xBRLModel.ConceptByQNameDict[_qualifiedName];
            }
            else
            {
                Concept = null;
                Debug.Assert(false, "Concept is null in Fact");

            }

            /*
            if ((IsXbrlNamespace(factNode.Name.Namespace.NamespaceName) == false)
                && (IsW3Namespace(factNode.Name.Namespace.NamespaceName) == false)
                && (!(factNode.NodeType == XmlNodeType.Comment)))
            {

                // This item could be a fact, or it could be a tuple. Examine the schemas
                // to find out what we're dealing with.
                XmlQualifiedName qn = new XmlQualifiedName(factNode.Name.LocalName, factNode.Name.Namespace.NamespaceName);
                var MatchingElement = xBRLModel.Schemas.GlobalElements[qn];
                if (MatchingElement != null)
                {

                    switch (MatchingElement.SubstitutionGroup)
                    {
                        case Element.ElementSubstitutionGroup.Item:
                            FactToReturn = new Item(ParentFragment, FactNode);
                            break;
                        case Element.ElementSubstitutionGroup.Tuple:
                            FactToReturn = new Tuple(ParentFragment, FactNode);
                            break;
                        default:
                            // This type is unknown, so leave it alone.
                            break;

                    }

                }
            }
            */
        }


        public bool IsComment => _xElement.NodeType == XmlNodeType.Comment;

        public XmlQualifiedName QualifiedName
        {
            get { return _qualifiedName; }
            set { _qualifiedName = value; }
        }

        public string ContextRef
        {
            get
            {
                if (_contextRef == null)
                    _contextRef = _xElement.Attribute("contextRef").Value;
                return _contextRef;
            }
        }


        public string ID
        {
            get
            {
                if (_id == null)
                    _id = _xElement.Attribute("id").Value;
                return _id;
            }
        }


        public XElement Item
        {
            get { return _xElement; }
            set { _xElement = value; }
        }

        public string UnitRef
        {
            get
            {
                if (_unitRef == null)
                    _unitRef = _xElement.Attribute("unitRef")?.Value.Trim();
                return _unitRef;
            }
        }

        public int LineNumber
        {
            get
            {
                if (_lineNumber == 0)
                {
                    IXmlLineInfo info = _xElement;
                    if (info.HasLineInfo())
                        _lineNumber = info.LineNumber;
                }

                return _lineNumber;
            }
        }

        public string Lang
        {
            get
            {
                if (_lang == null)
                {
                    XElement xElement = _xElement;

                    while (xElement != null)
                    {
                        _lang = xElement.Attribute("{http://www.w3.org/XML/1998/namespace}lang")?.Value;

                        if (_lang == null)
                        {
                            xElement = xElement.Parent;
                        }
                    }

                    if (_lang == null)
                    {
                        if (_concept != null && !_concept.IsNumeric)
                        {
                            _lang = "en-US";
                        }
                        else
                        {
                            _lang = string.Empty;
                        }
                    }
                }

                return _lang;
            }
        }

        public string Value
        {
            get
            {
                if (_value == null)
                {
                   _value = _xElement.Value;

                    if(_value == null || _value == string.Empty)
                    {
                        Debug.Assert(false);
                        if (Concept.DefaultValue != null)
                            _value = Concept.DefaultValue;
                        else if (Concept.FixedValue != null)
                            _value = Concept.FixedValue;
                        else
                            _value = string.Empty;
                    }
                }

                return _value;
            }
        }

        public Context Context
        {
            get
            {
                if(_context == null)
                    _context = _xBRLModel.ContextDict[_contextRef];

                return _context;
            }
        }

        public Unit Unit
        {
            get
            {
                if (_unit == null)
                {
                    _unit = _xBRLModel.UnitDict[_unitRef];
                }

                return _unit;
            }
        }
    }
}
