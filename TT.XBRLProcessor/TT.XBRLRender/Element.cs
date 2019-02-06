using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using TT.XBRLProcessor;

namespace TT.XBRLRender
{
    public class Element
    {
        private Concept _concept;
        private Dictionary<string, Cube> _cubeDict;

        public Element(Concept concept)
        {
            Concept = concept;
            CubeDict = new Dictionary<string, Cube>();
        }

        public void LinkCube(Cube cube)
        {
            if (!CubeDict.ContainsKey(cube.LinkRoleUri))
                CubeDict.Add(cube.LinkRoleUri, cube);
            else
            {
                Debug.Assert(cube == CubeDict[cube.LinkRoleUri]);
            }
        }
        public Concept Concept { get => _concept; set => _concept = value; }
        public Dictionary<string, Cube> CubeDict { get => _cubeDict; set => _cubeDict = value; }
    }
}
