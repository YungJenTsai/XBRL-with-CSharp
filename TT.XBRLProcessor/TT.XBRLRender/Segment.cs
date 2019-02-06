using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using TT.XBRLProcessor;

namespace TT.XBRLRender
{
    public class Segment
    {
        NetworkModel _childNetworkModel;
        Cube _cube;
        XBRLRender _xBRLRender;
        List<SegmentNode> _rootNodes = new List<SegmentNode>();
        Dictionary<Relation, SegmentNode> _relationToChildNodeDict = new Dictionary<Relation, SegmentNode>();

        public Segment(Cube cube, XBRLRender xBRLRender)
        {
            Cube = cube;
            XBRLRender = xBRLRender;
            BaseSetKey baseSetKey = new BaseSetKey(XBRLConstants.Arcrole_ParentChild, Cube.LinkRoleUri, null, null);
            ChildNetworkModel = new NetworkModel(XBRLRender.XBRLModel, baseSetKey);
            ChildNetworkModel.Create();
        }

        public NetworkModel ChildNetworkModel { get => _childNetworkModel; set => _childNetworkModel = value; }
        public Cube Cube { get => _cube; set => _cube = value; }
        public XBRLRender XBRLRender { get => _xBRLRender; set => _xBRLRender = value; }
        public Dictionary<Relation, SegmentNode> RelationToChildNodeDict { get => _relationToChildNodeDict; set => _relationToChildNodeDict = value; }
        public List<SegmentNode> RootNodes { get => _rootNodes; set => _rootNodes = value; }

        public bool FindRoot(Concept concept, Relation relation, Concept childConcept, SegmentNode segmentNode,HashSet<Relation> relations)
        {
            SegmentNode childNode;

            if (relation != null)
            {
                if (relations.Contains(relation))
                {
                    XBRLRender.Logger.Fatal("The relation groul " + this.Cube.ToString() + " contains a directed cycle, which is a violdation of XBRL 2.1 section 5.2.4.2");
                    Debug.Assert(false, "The relation groul " + this.Cube.ToString() + " contains a directed cycle, which is a violdation of XBRL 2.1 section 5.2.4.2");

                    // Errors happen and terminate the call
                    return false; 
                }

                relations.Add(relation);

                if (this.RelationToChildNodeDict.ContainsKey(relation))
                {
                    SegmentNode parentNode = this.RelationToChildNodeDict[relation];

                    if (segmentNode != null)
                    {
                        parentNode.AddChild(segmentNode);
                        this.RelationToChildNodeDict.Add(segmentNode.Relation, segmentNode);
                    }

                    return true;
                }
                else
                {
                    bool isUnitConcept = false;

                    if (XBRLRender.XBRLModel.UnitDict.ContainsKey(childConcept.Name))
                        isUnitConcept = true;
                    childNode = new SegmentNode(childConcept, relation, isUnitConcept);
                    if(segmentNode != null)
                    {
                        childNode.AddChild(segmentNode);
                        this.RelationToChildNodeDict.Add(segmentNode.Relation, segmentNode);
                    }
                    segmentNode = childNode;
                }
            }
            else
            {
                childNode = null;
            }

            List<Relation> childrenRelations;

            if(!ChildNetworkModel.ToConceptRelationDict.ContainsKey(concept))
            {

                SegmentNode rootNode = null;
                foreach (SegmentNode node in RootNodes)
                {
                    if (node.Concept == concept)
                    {
                        rootNode = node;
                        break;
                    }
                }

                if(rootNode == null)
                {
                    bool isUnitConcept = false;

                    if (XBRLRender.XBRLModel.UnitDict.ContainsKey(concept.Name))
                        isUnitConcept = true;
                    rootNode = new SegmentNode(concept, null, isUnitConcept);
                    RootNodes.Add(rootNode);

                }

                rootNode.AddChild(childNode);
                this.RelationToChildNodeDict.Add(relation, childNode);

                return true;
            }
            else
            {
                childrenRelations = ChildNetworkModel.ToConceptRelationDict[concept];
            }

            HashSet<Relation> copyRelations = new HashSet<Relation>(relations);

            foreach (Relation childRelation in childrenRelations)
            {
                bool retValue = FindRoot(childRelation.FromConcept, childRelation, concept, segmentNode, copyRelations);

                if (!retValue)
                    return retValue;
            }

            return true;
        }
    }
}
