using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml.Linq;
using System.Linq;

namespace TT.XBRLProcessor
{
    public class NetworkModel
    {
        public XBRLModel XBRLModel { get; set; }
        public BaseSetKey BaseSetKey { get; set; }
        public List<LinkModel> LinkModels { get; set; }

        private List<Relation> _relation;

        private HashSet<string> _linkRoleURIs;

        public HashSet<string> LinkRoleURIs
        {
            get
            {
                if(_linkRoleURIs == null)
                {
                    _linkRoleURIs = new HashSet<string>();
                    foreach(Relation relation in Relation)
                    {
                        _linkRoleURIs.Add(relation.ArcLinkElement.LinkModel.LinkRole);
                    }
                }
                return _linkRoleURIs;
            }
        }


        private Dictionary<Concept, List<Relation>> _toConceptRelationDict;
        private Dictionary<Concept, List<Relation>> _fromConceptRelationDict;

        public Dictionary<Concept, List<Relation>> FromConceptRelationDict
        {
            get { return _fromConceptRelationDict; }
            set { _fromConceptRelationDict = value; }
        }

        public Dictionary<Concept, List<Relation>> ToConceptRelationDict
        {
            get { return _toConceptRelationDict; }
            set { _toConceptRelationDict = value; }
        }


        public List<Relation> Relation { get => _relation; set => _relation = value; }


        public NetworkModel (XBRLModel xBRLModel, BaseSetKey baseSetKey)
        {
            XBRLModel = xBRLModel;
            BaseSetKey = baseSetKey;
            LinkModels = XBRLModel.GetLinkModels(BaseSetKey);

        }

        public void Create()
        {
            Dictionary<int, Relation> relationDict = new Dictionary<int, Relation>();

            if (LinkModels != null)
            {


                foreach (LinkModel linkModel in LinkModels)
                {
                    List<LinkElement> linkElements = linkModel.ArcLinkElement;
                    List<LinkElement> arcLinkElements = new List<LinkElement>();

                    foreach (LinkElement linkElement in linkElements)
                    {
                        string arcRole = linkElement.ArcRole;
                        if (arcRole != null)
                        {
                            if (arcRole == BaseSetKey.ArcRole &&
                                (BaseSetKey.ArcQName == null || BaseSetKey.ArcQName == linkElement.QName) &&
                                (BaseSetKey.LinkQName == null || BaseSetKey.LinkQName == linkModel.QName))
                            {
                                arcLinkElements.Add(linkElement);
                            }
                        }
                    }

                    foreach (LinkElement arcLinkElement in arcLinkElements)
                    {
                        List<LinkElement> fromResources = linkModel.LabelResources[arcLinkElement.LinkFromLabel];
                        List<LinkElement> toResources = linkModel.LabelResources[arcLinkElement.LinkToLabel];

                        Debug.Assert(fromResources != null && toResources != null);

                        foreach (LinkElement fromResource in fromResources)
                        {
                            foreach (LinkElement toResource in toResources)
                            {
                                Concept fromConcept = fromResource.ModelHRef.XBRLDoc.IDConceptDict[fromResource.ModelHRef.ID];
                                Concept toConcept = toResource.ModelHRef.XBRLDoc.IDConceptDict[toResource.ModelHRef.ID];

                                Relation relation = new Relation(arcLinkElement, fromConcept, toConcept);

                                if (!relationDict.ContainsKey(relation.GetHashCode()))
                                {
                                    relationDict[relation.GetHashCode()] = relation;
                                    Debug.Assert(relation.Equals(relationDict[relation.GetHashCode()]));
                                }
                                else
                                {
                                    //
                                    // Debug it when the case hit 
                                    //
                                    Debug.Assert(relation.Equals(relationDict[relation.GetHashCode()]));
                                    if (!relation.Equals(relationDict[relation.GetHashCode()]))
                                    {
                                        ;
                                    }
                                }
                            }
                        }
                    }

                }

                //List<Relation> relationList = RelationDict.Select(kvp => kvp.Value).ToList();
                Relation = relationDict.Values.ToList();
                Relation.Sort((x, y) => x.Order.CompareTo(y.Order));

                ToConceptRelationDict = new Dictionary<Concept, List<Relation>>();
                FromConceptRelationDict = new Dictionary<Concept, List<Relation>>();

                foreach (Relation relation in Relation)
                {
                    if (!ToConceptRelationDict.ContainsKey(relation.ToConcept))
                    {
                        List<Relation> relations = new List<Relation>();
                        relations.Add(relation);
                        ToConceptRelationDict[relation.ToConcept] = relations;
                    }
                    else
                    {
                        ToConceptRelationDict[relation.ToConcept].Add(relation);
                    }

                    if (!FromConceptRelationDict.ContainsKey(relation.FromConcept))
                    {
                        List<Relation> relations = new List<Relation>();
                        relations.Add(relation);
                        FromConceptRelationDict[relation.FromConcept] = relations;
                    }
                    else
                    {
                        FromConceptRelationDict[relation.FromConcept].Add(relation);
                    }
                }
            }
        }
    }
}
