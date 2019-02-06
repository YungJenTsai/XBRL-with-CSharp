using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace TT.XBRLProcessor
{
    public class Relation
    {
        private LinkElement _arcLinkElement;
        private Concept _toConcept;
        private Concept _fromConcept;
        private double _weight;
        private double _order;
        private int _priority;
        private string _preferredLabel;
        private int _hashcode;

        public double Order { get => _order; set => _order = value; }
        public Concept ToConcept { get => _toConcept; set => _toConcept = value; }
        public Concept FromConcept { get => _fromConcept; set => _fromConcept = value; }
        public double Weight { get => _weight; set => _weight = value; }
        public int Priority { get => _priority; set => _priority = value; }
        public string PreferredLabel { get => _preferredLabel; set => _preferredLabel = value; }
        public LinkElement ArcLinkElement { get => _arcLinkElement; set => _arcLinkElement = value; }

        public string LinkRole { get => ArcLinkElement.LinkModel.LinkRole; }

        public Relation(LinkElement arcLinkElement, Concept fromConcept, Concept toConcept)
        {
            ArcLinkElement = arcLinkElement;
            ToConcept = toConcept;
            FromConcept = fromConcept;

            Order = -1.0;
            string value = ArcLinkElement.BaseElement.Attribute("order")?.Value;
            if(value != null)
            {
                Order = double.Parse(value);
            }

            Priority = -1;
            value = ArcLinkElement.BaseElement.Attribute("priority")?.Value;
            if (value != null)
            {
                Priority = int.Parse(value);
            }

            Weight = -1.0;
            value = ArcLinkElement.BaseElement.Attribute("weight")?.Value;
            if (value != null)
            {
                Weight = double.Parse(value);
            }

            PreferredLabel = ArcLinkElement.BaseElement.Attribute("preferredLabel")?.Value;

            _hashcode = 0;
            GetHashCode();
        }

        public override int GetHashCode()
        {
            if (_hashcode == 0)
            {
                int h1 = 0;
                int h2 = 0;
                int h3 = 0;
                int h4 = 0;
                int h5 = 0;
                int h6 = 0;
                int h7 = 0;
                int h8 = 0;

                h1 = this.ArcLinkElement.QName.GetHashCode();

                h2 = this.ArcLinkElement.LinkModel.LinkRole.GetHashCode();

                h3 = this.ArcLinkElement.LinkModel.QName.GetHashCode();

                h4 = this.ToConcept.GetHashCode();

                h5 = this.FromConcept.GetHashCode();

                if (PreferredLabel != null)
                    h6 = PreferredLabel.GetHashCode();

                if (Order != -1.0)
                    h7 = this.Order.GetHashCode();

                if (Weight != -1.0)
                    h8 = this.Weight.GetHashCode();


                _hashcode = h1 ^ h2 ^ h3 ^ h4 ^ h5 ^ h6 ^ h7 ^ h8;
            }

            return _hashcode;
        }

        public override bool Equals(object obj)
        {
            if (obj is Relation)
            {
                Relation relation = (Relation)obj;

                if (this.ArcLinkElement != relation.ArcLinkElement)
                    return false;

                if (this.ToConcept != relation.ToConcept)
                    return false;

                if (this.FromConcept != relation.FromConcept)
                    return false;

                return true;
            }

            return false;
        }


    }
}
