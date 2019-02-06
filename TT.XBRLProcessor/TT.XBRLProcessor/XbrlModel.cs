using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Diagnostics;
using log4net;
using System.Xml.Serialization;

namespace TT.XBRLProcessor
{
    //
    // https://www.xbrl.org/Specification/XBRL-RECOMMENDATION-2003-12-31+Corrected-Errata-2008-07-02.htm
    //
    public class XBRLModel
    {
        private Dictionary<Uri, XBRLDoc> _documentDict;

        XBRLDoc _xBRLInstance = null;
        XmlSchemaSet _schemaSet;
        private List<Context> _contexts;
        private Dictionary<string, Context> _contextDict;
        private Dictionary<XmlQualifiedName, List<Fact>> _factByQnameDict;
        private Dictionary<XmlQualifiedName, SchemaType> _schemaTypeByQNameDict;
        private List<Fact> _factList;
        private List<Fact> _undefinedFactList;
        private Uri _baseUri;

        Dictionary<string, XBRLDoc> _namespaceDocsDict;
        Dictionary<string, string> _prefixNamespaceDict;
        Dictionary<XmlQualifiedName, Concept> _conceptByQNameDict;
        Dictionary<string, List<RoleType>> _roleTypeDict;
        Dictionary<string, List<ArcroleType>> _arcroleTypeDict;
        Dictionary<BaseSetKey, List<LinkModel>> _baseSets;
        private Dictionary<string, Unit> _unitDict;
        private SchemaType _defaultSchemaType;

        /// </summary>
        //
        // propertities
        //
        public Dictionary<Uri, XBRLDoc> DocumentDict
        {
            get { return _documentDict; }
            set { _documentDict = value; }
        }

        // public XBRLDoc InstanceDoc { get { return _doc; } private set { } }

        public Dictionary<XmlQualifiedName, Concept> ConceptByQNameDict { get => _conceptByQNameDict; set => _conceptByQNameDict = value; }
        public List<Fact> FactList { get => _factList; set => _factList = value; }
        public List<Fact> UndefinedFactList { get => _undefinedFactList; set => _undefinedFactList = value; }

        private ILog _logger;

        public ILog Logger
        {
            get { return _logger; }
            set { _logger = value; }
        }

        public Dictionary<string, Unit> UnitDict
        {
            get { return _unitDict; }
            set { _unitDict = value; }
        }



        public Uri BaseUri
        {
            get { return _baseUri; }
            set { _baseUri = value; }
        }

        public XmlSchemaSet Schemas { get {return _schemaSet; } }
        public Dictionary<XmlQualifiedName, List<Fact>> FactByQnameDict
        {
            get { return _factByQnameDict; }
            set { _factByQnameDict = value; }
        }


        public Dictionary<string, Context> ContextDict
        {
            get { return _contextDict; }
            set { _contextDict = value; }
        }


        public XBRLModel()
        {
            _documentDict = new Dictionary<Uri, XBRLDoc>();
            NamespaceDocsDict = new Dictionary<string, XBRLDoc>();
            PrefixNamespaceDict = new Dictionary<string, string>();
            ConceptByQNameDict = new Dictionary<XmlQualifiedName, Concept>();
            RoleTypeDict = new Dictionary<string, List<RoleType>>();
            ArcroleTypeDict = new Dictionary<string, List<ArcroleType>>();
            BaseSets = new Dictionary<BaseSetKey, List<LinkModel>>();
            _unitDict = new Dictionary<string, Unit>();
            FactList = new List<Fact>();
            UndefinedFactList = new List<Fact>();
            FactByQnameDict = new Dictionary<XmlQualifiedName, List<Fact>>();
            Contexts = new List<Context>();
            ContextDict = new Dictionary<string, Context>();
            SchemaTypeByQNameDict = new Dictionary<XmlQualifiedName, SchemaType>();

            DefaultSchemaType = new SchemaType();

            // Initialize Logger
            Logger = Utilities.LogManager.GetLogger(typeof(XBRLModel));

        }

        public List<Context> Contexts
        {
            get { return _contexts; }
            set { _contexts = value; }
        }

        public Dictionary<string, XBRLDoc> NamespaceDocsDict { get => _namespaceDocsDict; private set => _namespaceDocsDict = value; }
        public Dictionary<string, List<RoleType>> RoleTypeDict { get => _roleTypeDict; private set => _roleTypeDict = value; }
        public Dictionary<string, List<ArcroleType>> ArcroleTypeDict { get => _arcroleTypeDict; private set => _arcroleTypeDict = value; }
        public Dictionary<BaseSetKey, List<LinkModel>> BaseSets { get => _baseSets; set => _baseSets = value; }
        public Dictionary<XmlQualifiedName, SchemaType> SchemaTypeByQNameDict { get => _schemaTypeByQNameDict; set => _schemaTypeByQNameDict = value; }
        public SchemaType DefaultSchemaType { get => _defaultSchemaType; set => _defaultSchemaType = value; }
        public XBRLDoc XBRLInstance { get => _xBRLInstance; set => _xBRLInstance = value; }
        public Dictionary<string, string> PrefixNamespaceDict { get => _prefixNamespaceDict; private set => _prefixNamespaceDict = value; }

        //
        // Load XBRL instance into the model
        //
        public void Load(string url)
        {
            XBRLDoc xBRLDoc = new XBRLDoc(url, this);

            if (!LoadDocument(xBRLDoc))
            {
                Logger.Warn("File has tried to reload: " + xBRLDoc.DocURI.AbsoluteUri);
            }
            else
            {
                Logger.Info("File has succeeded to load: " + xBRLDoc.DocURI.AbsoluteUri);
            }
        }

        public void Load(Uri uri)
        {
            XBRLDoc xBRLDoc = new XBRLDoc(uri, this);
            if(!LoadDocument(xBRLDoc))
            {
                Logger.Warn("File has tried to reload: " + xBRLDoc.DocURI.AbsoluteUri);
            }
            else
            {
                Logger.Info("File has succeeded to load: " + xBRLDoc.DocURI.AbsoluteUri);
            }
        }

        internal bool LoadDocument(XBRLDoc xBRLDoc)
        { 

            xBRLDoc.Load();

            if (!AddDocument(xBRLDoc))
                return false;

            if (xBRLDoc.DocType == DocumentType.Instance)
            {
                Debug.Assert(XBRLInstance == null);

                XBRLInstance = xBRLDoc;

                BaseUri = xBRLDoc.DocURI;

                ParseSchemaSet(xBRLDoc);

                // XmlQualifiedName qname = new XmlQualifiedName(XBRLConstants.XbrlLinkbaseRoleTypeNamespaceUri);
                // Find roleTypeURI
                // IDictionaryEnumerator roleTypeEles = _schemaSet.GlobalElements.GetEnumerator();
                // roleTypeEles.Current();


                InstanceDiscover(xBRLDoc);
                //            CollectContextList();

                //            CollectFacts();
            } 
            else if (xBRLDoc.DocType == DocumentType.Linkbase)
            {
                xBRLDoc.LinkbaseDiscover();
            }

            return true;

        }

        private void InstanceDiscover(XBRLDoc xBRLDoc)
        {
            XElement child = (XElement) xBRLDoc.XDoc.Root.FirstNode;

            while (child != null)
            {
                if(child.Name.Namespace == XBRLConstants.XbrlNamespaceUri)
                {
                    if(child.Name.LocalName == "context")
                    {
                        Context context = new Context(this, child);
                        Contexts.Add(context);
                        ContextDict.Add(context.ID, context);

                    }
                    else if (child.Name.LocalName == "unit")
                    {
                        Unit unit = new Unit(child);
                        UnitDict.Add(unit.ID, unit);

                    }
                }
                else if(child.Name.Namespace == XBRLConstants.XbrlLinkbaseNamespaceUri)
                {
                    child = (XElement) child.NextNode;
                    continue;
                }
                else if ((child.Name.Namespace == XBRLConstants.IXbrlNamespaceUri || child.Name.Namespace == XBRLConstants.IXbrl11NamespaceUri) && 
                         child.Name.LocalName == "relationship")
                {
                    child = (XElement)child.NextNode;
                    continue;
                }
                else
                {
                    Fact fact = new Fact(this, child);

                    if(fact.Concept != null)
                    {
                        if (FactByQnameDict.ContainsKey(fact.QualifiedName))
                        {
                            List<Fact> factList = FactByQnameDict[fact.QualifiedName];
                            factList.Add(fact);
                        }
                        else
                        {
                            List<Fact> factList = new List<Fact>();
                            factList.Add(fact);
                            FactByQnameDict.Add(fact.QualifiedName, factList);
                        }
                        FactList.Add(fact);
                    }
                    else
                    {
                        UndefinedFactList.Add(fact);
                    }

                }
                child = (XElement)child.NextNode;
            }
        }

        //private void CollectContextList()
        //{

        //    IEnumerable<XElement> contextElements= _doc.XDoc.Root.Elements(XBRLConstants.XbrlContextNamespaceUri);

        //    foreach (XElement item in contextElements)
        //    {
        //        Context context = new Context(this, item);
        //        Contexts.Add(context);
        //        ContextDict.Add(context.ID, context);
        //    }
        //}

        private void AddSchemaFromURL(string schemaURL)
        {
            if (schemaURL == null)
                return;

            string [] splitString = schemaURL.Split(new char [] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            for(int i = 0; i < splitString.Length; i = i + 2)
            {
                _schemaSet.Add(splitString[i], NormalizedPath(null, splitString[i + 1]).AbsoluteUri);
                //_schemaSet.Add(splitString[i], NormalizedPath(@"E:\gepsio\JeffFerguson.Gepsio.Test\XBRL-CONF-2014-12-10\Common\100-schema\", splitString[i + 1]));
            }
        }

        private void AddSchemaRefFromURL(string schemaRefURL)
        {
            if (schemaRefURL == null)
                return;

            _schemaSet.Add(null, NormalizedPath(null, schemaRefURL).AbsoluteUri);
                //_schemaSet.Add(splitString[i], NormalizedPath(@"E:\gepsio\JeffFerguson.Gepsio.Test\XBRL-CONF-2014-12-10\Common\100-schema\", splitString[i + 1]));
        }

        private void LoadAndComplieSchemas(XBRLDoc xBRLDoc)
        {
            //initialize SchemaSet

            _schemaSet = new XmlSchemaSet();
         //   _schemaSet.XmlResolver = new UrlCachingResolver(false);

            // Get the root elememnt

            XElement root = xBRLDoc.XDoc.Root;

            XElement schemaLocationRefElement = null;

            //XName xname = new XName();
            AddSchemaFromURL((string)root.Attribute(XBRLConstants.XmlSchemaInstanceSchemaLocationNamespaceUri));

            if((schemaLocationRefElement = xBRLDoc.XDoc.Root.Element(XBRLConstants.XbrlLinkbaseSchemaRefNamespaceUri)) != null)
            {
                AddSchemaRefFromURL((string)schemaLocationRefElement.Attribute(XBRLConstants.XLinkHRefNamespaceUri));
            }


            _schemaSet.Compile();

            ProcessSchemaSemanticData();

        }

        //
        // Collect Schema semantic data for XBRL application
        //



        private void ProcessSchemaSemanticData()
        {
            List<XBRLDoc> schemaDocList = new List<XBRLDoc>();

            //Register all schema document
            foreach (XmlSchema xmlSchema in _schemaSet.Schemas())
            {
                if (xmlSchema.SchemaTypes.Count > 0)
                {
                    foreach (XmlSchemaType schemaType in xmlSchema.SchemaTypes.Values)
                    {
                        if (!SchemaTypeByQNameDict.ContainsKey(schemaType.QualifiedName))
                            SchemaTypeByQNameDict.Add(schemaType.QualifiedName, new SchemaType(schemaType));
                    }
                }
                
                for(int i=0; i < xmlSchema.Namespaces.Count; i++)
                {
                    ;// xmlSchema.Namespaces[i].
                }
                XBRLDoc xBRLDoc = new XBRLDoc(xmlSchema, this);
                if (AddDocument(xBRLDoc))
                    schemaDocList.Add(xBRLDoc);
            }

            foreach (XBRLDoc xBRLDoc in schemaDocList)
            {
                xBRLDoc.DiscoverSchemaElements();
            }

        }

        /// <summary>
        //  Parse XBRL instance
        //
        private void ParseSchemaSet(XBRLDoc xBRLDoc)
        {
            // Load and compile schema to _schemaSet
            LoadAndComplieSchemas(xBRLDoc);

        }

        internal bool AddDocument(XBRLDoc xBRLDoc)
        {
            //                _prefixNamespaceDict.Add(_doc.GetPrefixOfNamespace(xmlSchema.TargetNamespace), xmlSchema.TargetNamespace);
            if (DocumentDict.ContainsKey(xBRLDoc.DocURI))
            {
                return false; 
            }

            DocumentDict.Add(xBRLDoc.DocURI, xBRLDoc);

            if (xBRLDoc.DocType == DocumentType.Schema)
            {
                NamespaceDocsDict.Add(xBRLDoc.XmlSchema.TargetNamespace, xBRLDoc);
                XmlQualifiedName[] prefixNameSpace = xBRLDoc.XmlSchema.Namespaces.ToArray();

                for(int i = 0; i < prefixNameSpace.Length; i++)
                {
                    if(prefixNameSpace[i].Name != string.Empty)
                    {
                        if (!PrefixNamespaceDict.ContainsKey(prefixNameSpace[i].Name))
                        {
                            PrefixNamespaceDict.Add(prefixNameSpace[i].Name, prefixNameSpace[i].Namespace);
                        }
                    }
                }
            }

            return true;
        }

        internal XBRLDoc AddHrefDoc(Uri uri)
        {
            _schemaSet.Add(null, uri.AbsoluteUri);

            _schemaSet.Compile();

            ProcessSchemaSemanticData();

            return FindDOCWithURI(uri);
        }
        internal XBRLDoc FindDOCWithURI(Uri uri)
        {
            if (DocumentDict.ContainsKey(uri))
                return DocumentDict[uri];
            else
                return null;
        }

        internal XBRLDoc FindDOCWithNamespace(string targetNamespace)
        {
            return NamespaceDocsDict[targetNamespace];
        }

        internal Uri NormalizedPath(Uri baseUri,string url)
        {

            Uri uri;

            if (baseUri == null)
                uri = new Uri(BaseUri, url);
            else
                uri = new Uri(baseUri, url);

            return uri;
        }

        internal List<LinkModel> GetLinkModels(BaseSetKey baseSetKey)
        {
            if (BaseSets.ContainsKey(baseSetKey))
                return BaseSets[baseSetKey];
            else
                return null;
        }

    }
}
