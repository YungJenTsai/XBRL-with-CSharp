using TT.XBRLProcessor;
using log4net;
using TT.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace TT.XBRLRender
{
    public class XBRLRender
    {
        private XBRLModel _xBRLModel;
        private ILog _logger;
        private NetworkModel _presentationNetwork;
        private bool _isStatementNamespace;
        private bool _isDeiNamespace;
        List<Concept> _toDimensionConcept;
        List<Concept> _fromDimensionConcept;
        private Dictionary<string, Cube> _cubeDict;
        private Dictionary<XmlQualifiedName, Element> _elementDict;
        private List<Fact> _usedOrBrokenFacts = new List<Fact>();

        public NetworkModel PresentationNetwork
        {
            get { return _presentationNetwork; }
            set { _presentationNetwork = value; }
        }

        public ILog Logger
        {
            get { return _logger; }
            set { _logger = value; }
        }

        public XBRLModel XBRLModel
        {
            get { return _xBRLModel; }
            set { _xBRLModel = value; }
        }

        public bool IsDeiNamespace { get => _isDeiNamespace; set => _isDeiNamespace = value; }
        public bool IsStatementNamespace { get => _isStatementNamespace; set => _isStatementNamespace = value; }
        public List<Concept> ToDimensionConcept { get => _toDimensionConcept; set => _toDimensionConcept = value; }
        public List<Concept> FromDimensionConcept { get => _fromDimensionConcept; set => _fromDimensionConcept = value; }
        public Dictionary<string, Cube> CubeDict { get => _cubeDict; set => _cubeDict = value; }

        public XBRLRender(string url)
        {
            XBRLModel = new XBRLModel();
            XBRLModel.Load(url);

            Logger = Utilities.LogManager.GetLogger(typeof(XBRLRender));
        }

        public XBRLRender()
        {
            Logger = Utilities.LogManager.GetLogger(typeof(XBRLRender));
        }

        public void Initialize()
        {
            Logger = Utilities.LogManager.GetLogger(typeof(XBRLRender));

            IsStatementNamespace = false;

            foreach(string nameSpace in XBRLModel.NamespaceDocsDict.Keys)
            {
                if (nameSpace.Contains("/us-gaap/20"))
                {
                    IsStatementNamespace = true;
                    break;
                }
            }

            IsDeiNamespace = false;

            foreach (string nameSpace in XBRLModel.NamespaceDocsDict.Keys)
            {
                if (nameSpace.Contains("/dei/20"))
                {
                    IsDeiNamespace = true;
                    break;
                }
            }
        }

        private void BuildGraph(HashSet<Fact> duplicateFacts)
        {
            BaseSetKey baseSetKey;

            foreach (string linkRoleURI in PresentationNetwork.LinkRoleURIs)
            {
                Cube cube = new Cube(linkRoleURI, PresentationNetwork, this);
                cube.Segment = new Segment(cube, this);
                CubeDict.Add(linkRoleURI, cube);
            }

            ToDimensionConcept = new List<Concept>();
            foreach (Concept concept in PresentationNetwork.ToConceptRelationDict.Keys)
            {
                if (concept.IsDimensionItem)
                {
                    ToDimensionConcept.Add(concept);
                }
            }

            FromDimensionConcept = new List<Concept>();
            foreach (Concept concept in PresentationNetwork.FromConceptRelationDict.Keys)
            {
                if (concept.IsDimensionItem)
                {
                    FromDimensionConcept.Add(concept);
                }
            }

            baseSetKey = new BaseSetKey(XBRLConstants.Arcrole_DimensionDefault, string.Empty, null, null);
            NetworkModel dimensionNetwork = new NetworkModel(XBRLModel, baseSetKey);
            dimensionNetwork.Create();
            List<Concept> fromConceptUnionToConcept = FromDimensionConcept.Union(ToDimensionConcept).ToList<Concept>();

            _elementDict = new Dictionary<XmlQualifiedName, Element>();

            foreach (Concept concept in fromConceptUnionToConcept)
            {
                HashSet<Concept> defaultDimensionToConcept = null;
                if (dimensionNetwork.FromConceptRelationDict.ContainsKey(concept))
                {
                    defaultDimensionToConcept = new HashSet<Concept>();
                    foreach (Relation dimensionRelation in dimensionNetwork.FromConceptRelationDict[concept])
                    {
                        defaultDimensionToConcept.Add(dimensionRelation.ToConcept);
                    }
                }

                HashSet<Concept> presentationDimensionToConcept = null;

                if (PresentationNetwork.FromConceptRelationDict.ContainsKey(concept))
                {
                    presentationDimensionToConcept = new HashSet<Concept>();
                    HashSet<Relation> presentationDimensionRelations = new HashSet<Relation>();

                    foreach (Relation relation in PresentationNetwork.FromConceptRelationDict[concept])
                    {
                        string linkRoleURI = relation.ArcLinkElement.LinkModel.LinkRole;

                        Stack<Relation> stack = new Stack<Relation>();

                        Relation current = relation;
                        while (current != null)
                        {
                            if (current.ArcLinkElement.LinkModel.LinkRole == linkRoleURI && !presentationDimensionRelations.Contains(current))
                            {
                                presentationDimensionRelations.Add(current);
                                if (PresentationNetwork.FromConceptRelationDict.ContainsKey(current.ToConcept))
                                {

                                    foreach (Relation toRelation in PresentationNetwork.FromConceptRelationDict[current.ToConcept])
                                    {
                                        stack.Push(toRelation);
                                    }
                                }
                            }

                            if (stack.Count > 0)
                                current = stack.Pop();
                            else
                                current = null;
                        }

                        foreach (Relation dimensionToRelation in presentationDimensionRelations)
                        {
                            if (defaultDimensionToConcept.Contains(dimensionToRelation.ToConcept))
                                presentationDimensionToConcept.Add(dimensionToRelation.ToConcept);
                        }

                        if (defaultDimensionToConcept == null || defaultDimensionToConcept.Count == 0 || !defaultDimensionToConcept.SetEquals(presentationDimensionToConcept))
                        {
                            Cube cube = CubeDict[linkRoleURI];
                            cube.FilteredOutAxisSet.Add(concept.QName);
                        }
                        else
                        {
                            ;
                        }

                    }
                }
            }

            foreach (Cube cube in CubeDict.Values)
            {
                if (cube.FilteredOutAxisSet.Count > 0)
                {
                    Logger.Warn(cube.Definition + " the childreb of axis " + cube.FilteredOutAxisSet + " do not include thir defualts");
                }
            }

            foreach (KeyValuePair<XmlQualifiedName, List<Fact>> qnFact in XBRLModel.FactByQnameDict)
            {
                List<Fact> factList = qnFact.Value;
                XmlQualifiedName xmlQualifiedName = qnFact.Key;

                if (factList.Count > 1)
                {
                    factList = (factList.OrderBy(s => s.ContextRef).ThenBy(s => s.UnitRef).ThenBy(s => s.LineNumber)).ToList();
                    while (factList.Count > 0)
                    {
                        Fact firstFact = factList[0];
                        Fact firstEnUSLangFact;

                        List<int> discardedLineNumbers = new List<int>();
                        int counter = 0;

                        factList.RemoveAt(0);

                        if (firstFact.Lang == "en-US")
                        {
                            firstEnUSLangFact = firstFact;
                        }
                        else
                        {
                            firstEnUSLangFact = null;
                        }

                        // finds facts with same qname, contextRef and unitRef as firstFact

                        while (factList.Count > 0 &&
                            factList[0].QualifiedName == firstFact.QualifiedName &&
                            factList[0].ContextRef == firstFact.ContextRef &&
                            factList[0].UnitRef == firstFact.UnitRef
                            )
                        {
                            counter++;
                            Fact fact = factList[0];
                            factList.RemoveAt(0);
                            if (firstEnUSLangFact == null && fact.Lang == "en-US")
                            {
                                firstEnUSLangFact = fact;
                            }
                            else
                            {
                                duplicateFacts.Add(fact);
                                discardedLineNumbers.Add(fact.LineNumber);
                            }
                        }

                        if (counter > 0)
                        {
                            int linenumberofFactKeeping;
                            if (firstEnUSLangFact != null)
                            {
                                linenumberofFactKeeping = firstFact.LineNumber;
                            }
                            else
                            {
                                linenumberofFactKeeping = firstEnUSLangFact.LineNumber;

                                if (firstEnUSLangFact != firstFact)
                                {
                                    duplicateFacts.Add(firstFact);
                                    discardedLineNumbers.Add(firstFact.LineNumber);
                                }
                            }
                        }
                    }
                }

                foreach (Fact fact in qnFact.Value)
                {
                    bool elementBroken = false;

                    if (fact.Concept == null)
                    {
                        Logger.Warn("The element declaration for " + qnFact.Key.Namespace + ":" + qnFact.Key.Name + ", or one of its facts is broken. They will be ignored \n");
                        elementBroken = true;
                        Debug.Assert(false, "Fact with bad Element declaration");
                    }
                    else if (fact.Concept.SchemaType == null)
                    {
                        Logger.Warn("The type declaration of Element " + qnFact.Key.Namespace + ":" + qnFact.Key.Name + " is eiter broken or missing. The element will be ignored \n");
                        elementBroken = true;
                        Debug.Assert(false, "Fact with bad Element type");
                    }

                    if (fact.ContextRef == null || elementBroken)
                        continue;

                    _elementDict.Add(qnFact.Key, new Element(fact.Concept));
                    break;
                }
            }

            foreach (Concept concept in XBRLModel.ConceptByQNameDict.Values)
            {
                if (PresentationNetwork.ToConceptRelationDict.ContainsKey(concept))
                {
                    foreach (Relation relation in PresentationNetwork.ToConceptRelationDict[concept])
                    {
                        Cube cube = CubeDict[relation.LinkRole];
                        cube.Segment.FindRoot(concept, null, null, null, new HashSet<Relation>());

                        if (_elementDict.ContainsKey(concept.QName))
                        {
                            _elementDict[concept.QName].LinkCube(cube);
                        }

                    }
                }
            }
        }

        private bool ProcessEmbeddedCommand(Fact fact)
        {
            Debug.Assert(false);
            return false;
        }

        private void FactRealization(HashSet<Fact> duplicateFacts)
        {
            foreach(Fact fact in XBRLModel.FactList)
            {
                if (fact.IsTuple)
                {
                    Debug.Assert(false);
                    Logger.Warn(string.Format("A Fact with Qname {0} is a Tuple and Tuples are forbidden by the EDGAR Filer Manual. The Fact will be ignored.", fact.QualifiedName));
                    _usedOrBrokenFacts.Add(fact);
                }

                if(fact.ContextRef == null)
                {
                    Debug.Assert(false);
                    Logger.Warn(string.Format("Either the Context of a Fact with Qname {0}, or the reference to the Context in the Fact is broken.", fact.QualifiedName));
                    _usedOrBrokenFacts.Add(fact);
                }

                if(fact.Context.Scenario != null)
                {
                    Debug.Assert(false);
                    Logger.Warn(string.Format("The Context {0} has a scenario attribute. Such attributes are forbidden by the EDGAR Filer Manual. This filing will not validate, but this should not interfere with rendering", fact.ContextRef));
                }

                if (!_elementDict.ContainsKey(fact.QualifiedName))
                {
                    _usedOrBrokenFacts.Add(fact);
                    continue;
                }

                if (duplicateFacts.Contains(fact))
                    continue;

                if (!fact.IsNumeric)
                {
                    bool isEmbeddedCommand = false;

                    if (fact.Concept.IsTextBlock)
                    {
                        isEmbeddedCommand = ProcessEmbeddedCommand(fact);
                    }

                    string pattern = "[a-zA-Z-]+:[a-zA-Z]+";
                    Regex regEx = new Regex(pattern);
                    Match match = regEx.Match(fact.Value);

                    if (!isEmbeddedCommand && match.Success)
                    {
                        ;
                    }
                }
            }
        }

        public void HTMLRender(string url)
        {
            XBRLModel = new XBRLModel();
            XBRLModel.Load(url);
            HashSet<Fact> duplicateFacts = new HashSet<Fact>();

            //Skip Validate and do it later
            Initialize();

            // Process 
            AddOnManger addOnManger = new AddOnManger(XBRLModel);

            addOnManger.LoadAddOnTaxonomy();

            BaseSetKey baseSetKey = new BaseSetKey(XBRLConstants.Arcrole_ParentChild,string.Empty, null, null);
            PresentationNetwork = new NetworkModel(XBRLModel, baseSetKey);
            PresentationNetwork.Create();

            CubeDict = new Dictionary<string, Cube>();

            BuildGraph(duplicateFacts);

            FactRealization(duplicateFacts);
        }
    }
}
