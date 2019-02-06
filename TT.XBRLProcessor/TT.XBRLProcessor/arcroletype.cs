using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace TT.XBRLProcessor
{
    public class ArcroleType
    {
        private XmlNode _xmlNode;
        private string _arcroleURI;
        private string _id;
        private string _definition;
        private List<string> _usedOn;
        private string _cyclesAllowed;

        public string CyclesAllowed
        {
            get { return _cyclesAllowed; }
            set { _cyclesAllowed = value; }
        }


        public XmlNode Item
        {
            get { return _xmlNode; }
            set { _xmlNode = value; }
        }

        public List<string> UsedOn
        {
            get { return _usedOn; }
            set { _usedOn = value; }
        }


        public string Definition
        {
            get { return _definition; }
            set { _definition = value; }
        }


        public string ArcroleURI
        {
            get { return _arcroleURI; }
            set { _arcroleURI = value; }
        }

        public string ID
        {
            get { return _id; }
            set { _id = value; }
        }
        public ArcroleType(XmlNode xmlNode)
        {
            _xmlNode = xmlNode;

            _id = _xmlNode.Attributes["id"]?.Value;
            _arcroleURI = xmlNode.Attributes["arcroleURI"]?.Value;
            _cyclesAllowed = xmlNode.Attributes["cyclesAllowed"]?.Value;
            _usedOn = new List<string>();

            foreach (XmlNode currentChild in xmlNode.ChildNodes)
            {
                if (currentChild.LocalName.Equals("definition") == true)
                {
                    _definition = currentChild.InnerText;
                }
                if (currentChild.LocalName.Equals("usedOn") == true)
                {
                    _usedOn.Add(currentChild.InnerText);
                }
            }
        }
    }
}
