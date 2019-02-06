using System;
using System.Collections.Generic;
using System.Text;

namespace TT.XBRLProcessor
{
    public class Href
    {
        public XBRLDoc XBRLDoc { get; set; }
        public LinkElement LinkElement { get; set; }
        public string ID { get; set; }
        public Href(XBRLDoc xBRLDoc, LinkElement linkElement, string id)
        {
            XBRLDoc = xBRLDoc;
            LinkElement = linkElement;
            ID = id;
        }
    }
}
