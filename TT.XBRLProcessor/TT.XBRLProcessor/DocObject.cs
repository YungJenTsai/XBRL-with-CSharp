using System;
using System.Collections.Generic;
using System.Text;

namespace TT.XBRLProcessor
{
    public enum DocumentType {UnknownXml, Instance, Schema, Linkbase,
                                TestcaseIndex, Testcse, Registry, RSS, ArcInfoSet, FactDimsInfoset,
                                InlineXBRL};
    public class DocObject
    {
        string _baseURI;
        Uri _uri;
        private DocumentType _type;

        public DocumentType DocType
        {
            get { return _type; }
            set { _type = value; }
        }

        public DocObject(Uri uri)
        {
            _baseURI = "";
            _uri = uri;
        }

        public DocObject(string uri)
        {
            _baseURI = "";
            _uri = new Uri(uri);
        }

        public DocObject(string uri, string baseURI)
        {
            _baseURI = baseURI;
            _uri = new Uri(uri);
        }

        // NormalizedURI
        private string NormalizedURI(string uri)
        {
            return uri;
        }

        public Uri DocURI { get { return _uri; } private set { } }
        public string DocBaseURI { get { return _baseURI; } private set { } }


    }
}
