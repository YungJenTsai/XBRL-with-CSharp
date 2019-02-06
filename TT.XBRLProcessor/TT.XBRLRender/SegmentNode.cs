using System;
using System.Collections.Generic;
using System.Text;
using TT.XBRLProcessor;

namespace TT.XBRLRender
{
    public class SegmentNode
    {
        Concept _concept;
        Relation _relation;
        bool _mayBeUnit;
        List<SegmentNode> _children;

        public SegmentNode(Concept concept, Relation reltion, bool mayBeUnit)
        {
            Concept = concept;
            Relation = reltion;
            MayBeUnit = mayBeUnit;
            Children = new List<SegmentNode>();
        }

        public void AddChild(SegmentNode child)
        {
            Children.Add(child);
        }

        public Concept Concept { get => _concept; set => _concept = value; }
        public Relation Relation { get => _relation; set => _relation = value; }
        public bool MayBeUnit { get => _mayBeUnit; set => _mayBeUnit = value; }
        public List<SegmentNode> Children { get => _children; set => _children = value; }
    }
}
