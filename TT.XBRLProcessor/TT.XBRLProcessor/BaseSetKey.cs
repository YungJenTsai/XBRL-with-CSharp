using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace TT.XBRLProcessor
{
    public class BaseSetKey
    {
        public string ArcRole { get; set; }
        public string LinkRole { get; set; }
        public XmlQualifiedName LinkQName { get; set; }
        public XmlQualifiedName ArcQName { get; set; }

        private int _hashCode;

        public BaseSetKey(
            string arcRole,
            string linkRole,
            XmlQualifiedName linkQName,
            XmlQualifiedName arcQName)
        {
            ArcRole = arcRole;
            LinkRole = linkRole;
            LinkQName = linkQName;
            ArcQName = arcQName;
            _hashCode = 0;
            GetHashCode();
        }


        public override int GetHashCode()
        {
            int h1 = 0;
            int h2 = 0;
            int h3 = 0;
            int h4 = 0;

            if (_hashCode == 0)
            {

                if (this.ArcRole != string.Empty)
                    h1 = this.ArcRole.GetHashCode();

                if (this.LinkRole != string.Empty)
                    h2 = this.LinkRole.GetHashCode();

                if (this.ArcQName != null)
                    h3 = this.ArcQName.GetHashCode();

                if (this.LinkQName != null)
                    h4 = this.LinkQName.GetHashCode();

                _hashCode = h1 ^ h2 ^ h3 ^ h4;
            }
            return _hashCode;
        }

        public override bool Equals(object obj)
        {
            if (obj is BaseSetKey)
            {
                BaseSetKey baseSetKey = (BaseSetKey)obj;

                if (this.ArcRole != baseSetKey.ArcRole)
                    return false;
                if (this.LinkRole != baseSetKey.LinkRole)
                    return false;

                if (this.LinkQName != null)
                {
                    if (baseSetKey.LinkQName != null)
                    {
                        if ((this.LinkQName.Namespace != baseSetKey.LinkQName.Namespace) || (this.ArcQName.Name != baseSetKey.ArcQName.Name))
                            return false;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    if (baseSetKey.LinkQName != null)
                        return false;
                }

                if (this.ArcQName != null)
                {
                    if (baseSetKey.ArcQName != null)
                    {
                        if ((this.ArcQName.Namespace != baseSetKey.ArcQName.Namespace) || (this.ArcQName.Name != baseSetKey.ArcQName.Name))
                            return false;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    if (baseSetKey.ArcQName != null)
                        return false;
                }

                return true;
            }

            return false;
        }

    }
}
