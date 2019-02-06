using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Diagnostics;

namespace TT.XBRLProcessor
{
    public class Concept
    {
        private XmlSchemaElement _xmlSchemaElement;
        private XBRLModel _xBRLModel;
        private string _periodType;
        private SchemaType _schemaType;
        private bool? _isItem;
        private bool? _isTuple;
        private bool? _isDimensionItem;
        private bool? _isTextBlock;
        private bool? _isDomainItem;
        private Concept _substituteHeadConcept;

        public XmlQualifiedName SubstitutionGroup { get => XmlSchemaElement.SubstitutionGroup; }
        public XmlQualifiedName QName { get => XmlSchemaElement.QualifiedName; }

        public string Name { get => XmlSchemaElement.Name; }
        public string DefaultValue { get => XmlSchemaElement.DefaultValue; }
        public string FixedValue { get => XmlSchemaElement.FixedValue; }

        public XmlQualifiedName TypeQName { get => XmlSchemaElement.ElementSchemaType.QualifiedName; }

        public string ID
        {
            get => XmlSchemaElement.Id;
        }

        public Concept(XBRLModel xBRLModel, XmlSchemaElement xmlSchemaElement)
        {
            XmlSchemaElement = xmlSchemaElement;
            XBRLModel = xBRLModel;
            DiscoverUnhandleAttributes();

            if (XBRLModel.SchemaTypeByQNameDict.ContainsKey(XmlSchemaElement.ElementSchemaType.QualifiedName))
            {
                SchemaType = XBRLModel.SchemaTypeByQNameDict[XmlSchemaElement.ElementSchemaType.QualifiedName];
            }
            else
            {
                if (XmlSchemaElement.ElementSchemaType.QualifiedName.IsEmpty != true)
                {
                    SchemaType = new SchemaType(XmlSchemaElement.ElementSchemaType);
                    XBRLModel.SchemaTypeByQNameDict.Add(XmlSchemaElement.ElementSchemaType.QualifiedName, SchemaType);
                }
                else
                {
                    if (XmlSchemaElement.QualifiedName.Namespace == XBRLConstants.XbrlNamespaceUri ||
                        XmlSchemaElement.QualifiedName.Namespace == XBRLConstants.XbrlLinkbaseNamespaceUri ||
                        XmlSchemaElement.QualifiedName.Namespace == XBRLConstants.XbrlRefUri)
                        SchemaType = XBRLModel.DefaultSchemaType;
                    else
                        Debug.Assert(false, "Conecpt with bad schema type");
                }
            }
        }

        public bool IsItem
        {
            get
            {
                if (_isItem == null)
                {
                    _isItem = FindSubstituteHeadWithQName(XBRLConstants.QNameXbrliItem);
                }

                // US_GAAP not accept fact as tuple
                Debug.Assert(_isItem != null);

                return (bool)_isItem;
            }
        }

        private bool CheckTextBlockType()
        {
            if (SchemaType == XBRLModel.DefaultSchemaType || _xmlSchemaElement.ElementSchemaType.QualifiedName.IsEmpty)
            {
                /*
                if (QName.Namespace.ToString() == XBRLConstants.XbrlNamespaceUri || QName.Namespace.ToString() == XBRLConstants.XmlSchemaNamespaceUri)
                    return false;

                if (XBRLModel.ConceptByQNameDict.ContainsKey(QName))
                {
                    Concept concept = XBRLModel.ConceptByQNameDict[this.QName];
                    if (concept != this)
                    {
                        return concept.IsTextBlock;
                    }
                }
                */
                return false;
            }

            if (SchemaType.QName.Name == "textBlockItemType" && SchemaType.QName.Namespace.ToString().Contains("/us-types/"))
                return true;
            if (SchemaType.QName.Name == "escapedItemType" && SchemaType.QName.Namespace.ToString().StartsWith(XBRLConstants.Arcrole_DtrTypesStartsWith))
                return true;

            return false;
        }

        public bool IsTextBlock
        {
            get
            {

                if(_isTextBlock == null)
                {
                    _isTextBlock = CheckTextBlockType();
                }

                return (bool)_isTextBlock;
            }
        }

        private bool CheckDomainItemType()
        {
            if (SchemaType == XBRLModel.DefaultSchemaType || _xmlSchemaElement.ElementSchemaType.QualifiedName.IsEmpty)
            {
                /*
                if (QName.Namespace.ToString() == XBRLConstants.XbrlNamespaceUri || QName.Namespace.ToString() == XBRLConstants.XmlSchemaNamespaceUri)
                    return false;

                if (XBRLModel.ConceptByQNameDict.ContainsKey(QName))
                {
                    Concept concept = XBRLModel.ConceptByQNameDict[this.QName];
                    if (concept != this)
                    {
                        return concept.IsDomainItem;
                    }
                }
                */
                return false;
            }

            if (SchemaType.QName.Name == "domainItemType" && (SchemaType.QName.Namespace.ToString().Contains("/us-types/") || SchemaType.QName.Namespace.ToString().StartsWith(XBRLConstants.Arcrole_DtrTypesStartsWith)))
                return true;

            if (SchemaType != XBRLModel.DefaultSchemaType && (SchemaType.QName.Namespace.ToString() == XBRLConstants.XbrlNamespaceUri || SchemaType.QName.Namespace.ToString() == XBRLConstants.XmlSchemaNamespaceUri))
                return false;

            if (SchemaType != XBRLModel.DefaultSchemaType && XBRLModel.ConceptByQNameDict.ContainsKey(SchemaType.QName))
            {
                Concept concept = XBRLModel.ConceptByQNameDict[SchemaType.QName];
                if (concept != this)
                {
                    return concept.IsDomainItem;
                }
            }

            return false;
        }

        public bool IsDomainItem
        {
            get
            {

                if (_isDomainItem == null)
                {
                    _isDomainItem = CheckDomainItemType();
                }

                return (bool)_isDomainItem;
            }
        }

        public bool IsTuple
        {
            get
            {
                if (_isTuple == null)
                {
                   _isTuple = FindSubstituteHeadWithQName(XBRLConstants.QNameXbrliTupe); 
                }

                // US_GAAP not accept fact as tuple
                Debug.Assert(_isTuple == false);

                return (bool)_isTuple;
            }
        }
        public bool IsNumeric
        {
            get => SchemaType.IsNumeric;
        }

        public bool IsShares
        {
            get => SchemaType.IsShares;
        }

        public bool IsMonetary
        {
            get => SchemaType.IsMonetary;
        }

        public bool IsAbstract { get { return XmlSchemaElement.IsAbstract; } }

        public XmlSchemaElement XmlSchemaElement { get => _xmlSchemaElement; set => _xmlSchemaElement = value; }
        public XBRLModel XBRLModel { get => _xBRLModel; set => _xBRLModel = value; }
        public SchemaType SchemaType { get => _schemaType; set => _schemaType = value; }

        private bool FindSubstituteHeadWithQName(XmlQualifiedName xmlQualifiedName)
        {
            if (_substituteHeadConcept == null)
            {


                Concept current = this;

                while (current != null)
                {
                    if (current.XmlSchemaElement.SubstitutionGroup.IsEmpty == false && XBRLModel.ConceptByQNameDict.ContainsKey(current.XmlSchemaElement.SubstitutionGroup))
                        current = XBRLModel.ConceptByQNameDict[current.XmlSchemaElement.SubstitutionGroup];
                    else
                        break;
                }

                _substituteHeadConcept = current;
            }

            Debug.Assert(_substituteHeadConcept != null);

            if (_substituteHeadConcept.XmlSchemaElement.QualifiedName == xmlQualifiedName && this.XmlSchemaElement.Namespaces.ToString() != XBRLConstants.XbrlNamespaceUri)
            {
                return true;
            }

            return false;
        }

        private bool FindSubstituteWithQName(XmlQualifiedName xmlQualifiedName)
        {
            Concept current = this;

            while (current != null)
            {
                if (current.XmlSchemaElement.QualifiedName == xmlQualifiedName)
                {
                    return true;
                }

                if (current.XmlSchemaElement.SubstitutionGroup.IsEmpty == false)
                {
                    if (XBRLModel.ConceptByQNameDict.ContainsKey(current.XmlSchemaElement.SubstitutionGroup))
                        current = XBRLModel.ConceptByQNameDict[current.XmlSchemaElement.SubstitutionGroup];
                    else
                        return false;
                }
                else
                {
                    return false;
                }
            }

            return false;
        }
        public bool IsDimensionItem
        {
            get
            {
                if(_isDimensionItem == null)
                    _isDimensionItem = FindSubstituteWithQName(XBRLConstants.QNameXbrldtDimensionItem);

                return (bool) _isDimensionItem;
            }
        }

        private void DiscoverUnhandleAttributes()
        {
            if (XmlSchemaElement.UnhandledAttributes != null)
            {
                foreach (XmlAttribute xmlAttribute in XmlSchemaElement.UnhandledAttributes)
                {
                    if (xmlAttribute.NamespaceURI == XBRLConstants.XbrlNamespaceUri)
                    {

                        if (xmlAttribute.LocalName == "periodType")
                        {
                            _periodType = xmlAttribute.Value;
                        }

                        if (xmlAttribute.LocalName == "balance")
                        {
                            _periodType = xmlAttribute.Value;
                        }

                    }
                }
            }
        }


    }
}
