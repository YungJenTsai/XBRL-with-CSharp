using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace TT.XBRLProcessor
{
    public class SchemaType
    {
        private XmlSchemaType _xmlSchemaType;
        private bool _isNumeric;
        private bool _isShares;
        private bool _isMonetary;

        public SchemaType()
        {
            _isMonetary = false;
            _isNumeric = false;
            _isShares = false;
            _xmlSchemaType = null;
        }

        public SchemaType(XmlSchemaType xmlSchemaType)
        {
            _xmlSchemaType = xmlSchemaType;

            switch (_xmlSchemaType.TypeCode)
            {
                case XmlTypeCode.Base64Binary:
                case XmlTypeCode.Decimal:
                case XmlTypeCode.Double:
                case XmlTypeCode.Float:
                case XmlTypeCode.HexBinary:
                case XmlTypeCode.Int:
                case XmlTypeCode.Integer:
                case XmlTypeCode.Long:
                case XmlTypeCode.NonNegativeInteger:
                case XmlTypeCode.NonPositiveInteger:
                case XmlTypeCode.PositiveInteger:
                case XmlTypeCode.Short:
                case XmlTypeCode.UnsignedInt:
                case XmlTypeCode.UnsignedLong:
                case XmlTypeCode.UnsignedShort:
                case XmlTypeCode.Byte:
                case XmlTypeCode.UnsignedByte:
                    _isNumeric = true;
                    break;
                default:
                    _isNumeric = false;
                    break;

            }

            if (_xmlSchemaType.Name == "shares" || _xmlSchemaType.Name == "sharesItemType")
            {
                IsShares = true;
            }
            else
            {
                IsShares = false;
            }

            if (_xmlSchemaType.Name == "monetary" || _xmlSchemaType.Name == "monetaryItemType")
            {
                IsMonetary = true;
            }
            else
            {
                IsMonetary = false;
            }
        }

        public bool IsNumeric { get => _isNumeric; private set => _isNumeric = value; }
        // public XmlSchemaType XmlSchemaType { get => _xmlSchemaType; set => _xmlSchemaType =  value; }
        public bool IsShares { get => _isShares; set => _isShares = value; }
        public bool IsMonetary { get => _isMonetary; set => _isMonetary = value; }
        public XmlQualifiedName QName { get => _xmlSchemaType.QualifiedName; }
    }
}
