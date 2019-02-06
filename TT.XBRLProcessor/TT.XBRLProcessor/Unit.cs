using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace TT.XBRLProcessor
{
    public class Unit
    {
        private XElement _xElement;

        public Unit(XElement xElement)
        {
            _xElement = xElement;
        }

        public string ID
        {
            get
            {
                return _xElement.Attribute("id").Value;
            }
        }

    }
}
