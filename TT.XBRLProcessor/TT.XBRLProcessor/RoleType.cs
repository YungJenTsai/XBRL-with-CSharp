using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace TT.XBRLProcessor
{
    public class RoleType
    {
        private XmlNode _xmlNode;
        private string _roleURI;
        private string _id;
        private string _definition;
        private List<string> _usedOn;
        private string _label;
        private NetworkModel _labelNetwork;

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


        public string RoleURI
        {
            get { return _roleURI; }
            set { _roleURI = value; }
        }

        public string ID
        {
            get { return _id; }
            set { _id = value; }
        }

        public string Label { get => _label; set => _label = value; }

        public RoleType(XmlNode xmlNode)
        {
            _xmlNode = xmlNode;

            _id = _xmlNode.Attributes["id"]?.Value;
            _roleURI = xmlNode.Attributes["roleURI"]?.Value;
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

        public void GenLabel(XBRLModel xBRLModel,
                        string role = null, 
                        bool fallbackToQName = false, 
                        bool fallbackToXLinkLabel = false, 
                        string cultureName = null, 
                        bool strip = false)
        {
            if (role == null)
                role = XBRLConstants.Arcrole_GenStandardLabel;
            if (role == XBRLConstants.Arcrole_ConceptNameLabelRole)
                Label = _xmlNode.LocalName;

            BaseSetKey baseSetKey = new BaseSetKey(XBRLConstants.Arcrole_ElementLabel, _roleURI, null, null);
            _labelNetwork = new NetworkModel(xBRLModel, baseSetKey);
            _labelNetwork.Create();

            if(_labelNetwork.Relation != null)
            {
                ;
            }
            else
            {
                Label = null;
            }
        }
    }
}
