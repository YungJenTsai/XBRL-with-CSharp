using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace TT.XBRLProcessor
{
    //
    //XBRL 2.1 implementation 
    //https://www.xbrl.org/Specification/XBRL-RECOMMENDATION-2003-12-31+Corrected-Errata-2008-07-02.htm#_4.7
    //
    enum DurationPeriod { None = 0, Instant, StartAndEnd,Forever};

    public class Context
    {
        private string _id;
        private string _identifier;

        private string _identifierScheme;
        private bool _isInstant;
        private bool _isForever;
        private System.DateTime _startDate;
        private System.DateTime _endDate;
        private System.DateTime _instantDate;
        private XElement _segment;
        private XElement _scenario;
        private XBRLModel _xbrlModel;
        private XElement _item;


        public Context(XBRLModel model, XElement item)
        {
            _item = item;
            _xbrlModel = model;

            _id = item.Attribute("id").Value;
            _isInstant = false;
            _isForever = false;
            _startDate = default(System.DateTime);
            _endDate = default(System.DateTime);
            _instantDate = default(System.DateTime);
            _identifier = string.Empty;
            _identifierScheme = string.Empty;
            _segment = null;
            _scenario = null;

            XElement el = (XElement) item.FirstNode;

            while(el != null)
            {
                switch (el.Name.LocalName)
                {
                    case "period":
                        ReadPeriod(el);
                        break;
                    case "entity":
                        ReadEntity(el);
                        break;
                    case "scenario":
                        ReadScenario(el);
                        break;
                    default:
                        break;
                }

                el = (XElement) el.NextNode;
            }
        }

        private void ReadPeriod(XElement periodItem)
        {
            XElement el = (XElement)periodItem.FirstNode;

            //Traverse all the children (but not decendents)
            while(el != null)
            {
                var parsedDate = default(System.DateTime);

                switch (el.Name.LocalName)
                {
                    case "instant":
                        _isInstant = true;
                        System.DateTime.TryParse(el.Value, out parsedDate);
                        _instantDate = parsedDate;
                        break;
                    case "forever":
                        IsForever = true;
                        break;
                    case "startDate":
                        System.DateTime.TryParse(el.Value, out parsedDate);
                        _startDate = parsedDate;
                        break;
                    case "endDate":
                        System.DateTime.TryParse(el.Value, out parsedDate);
                        _endDate = parsedDate;
                        break;
                    default:
                        break;
                }

                el = (XElement) el.NextNode;
            }
        }

        private void ReadEntity(XElement entityItem)
        {
            XElement el = (XElement)entityItem.FirstNode;

            //Traverse all the children (but not decendents)
            while (el != null)
            {
                switch (el.Name.LocalName)
                {
                    case "identifier":
                        _identifier = el.Value;
                        if (el.Attribute("scheme") != null)
                            _identifierScheme = el.Attribute("scheme").Value;
                        break;
                    case "segment":
                        _segment = el;
                        break;
                    default:
                        break;
                }

                el = (XElement) el.NextNode;
            }
        }

        private void ReadScenario(XElement scenarioItem)
        {
                _scenario = scenarioItem;
        }

#region filed properties
        public XElement Item
        {
            get { return _item; }
            set { _item = value; }
        }

        public XBRLModel Model
        {
            get { return _xbrlModel; }
            set { _xbrlModel = value; }
        }


        public string ID {
            get { return _id; }
            set { _id = value; }
        }

        public string IdentifierScheme {
            get { return _identifierScheme; }
            set { _identifierScheme = value; }
        }

        public string Identifier
        {
            get { return _identifier; }
            set { _identifier = value; }
        }

        public bool IsInstant {
            get { return _isInstant; }
            set { _isInstant = value; }
        }

        public bool IsForever
        {
            get { return _isForever; }
            set { _isForever = value; }
        }

        public System.DateTime StartDate
        {
            get { return _startDate; }
            set { _startDate = value; }
        }

        public System.DateTime EndDate
        {
            get { return _endDate; }
            set { _endDate = value; }
        }

        public System.DateTime InstantDate
        {
            get { return _instantDate; }
            set { _instantDate = value; }
        }

        public XElement Segment
        {
            get { return _segment; }
            set { _segment = value; }
        }

        public XElement Scenario
        {
            get { return _scenario; }
            set { _scenario = value; }
        }
#endregion

    }
}
