using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class OsmRelationManager : BaseOsm


{

    public XmlNodeList ways { get; private set; }
    public Reader map { get; private set; }

    public OsmRelationManager(XmlNodeList nodeList, Reader map)
    {
        ways = nodeList;
        this.map = map;
        getAllInformation();
    }


    // loopt over alle relations
    public void getAllInformation()
    {
        foreach (XmlNode node in ways)
        {
            bool isMulti = false;
            bool HasSurface = false;
            XmlNodeList tags = node.SelectNodes("tag");
            foreach (XmlNode t in tags)
            {
                string key = GetAttribute<string>("k", t.Attributes);
                if (key == "type")
                {
                    string val = GetAttribute<string>("v", t.Attributes);
                    if (val == "multipolygon") isMulti = true;
                }
                else if (key == "surface" || key == "parking")
                {
                    HasSurface = true;
                }
                else if (key == "place") {
                    string val = GetAttribute<string>("v", t.Attributes);
                    if (val == "square") HasSurface = true;
                }
                else if (key == "area")
                {
                    string val = GetAttribute<string>("v", t.Attributes); 
                    if (val == "yes") HasSurface = true;
                }

                if (isMulti && HasSurface) 
                {
                    // NOG VERANDEREN, NORMAAL ALLEEN 2E STUK
                    OsmRelation relation = new OsmRelation(node, map);
                    map.relations.Add(relation);
                    AddLayer(relation.GetTexture());
                    //getNodesFromRelation(t);
                    break;
                }
            }
        }
    }

    // is voor 1 relation, nodes er uit halen
    private void getNodesFromRelation(XmlNode node) {
        XmlNodeList nds = node.SelectNodes("member");
        bool AllNodesPresent = true;
        List<ulong> OuterNodeIDs = new List<ulong>();
        List<ulong> InnerNodeIDs= new List<ulong>();

        foreach (XmlNode n in nds)
        {
            string type = GetAttribute<string>("type", n.Attributes);
            if (type == "way")
            {
                ulong reference = GetAttribute<ulong>("ref", n.Attributes);
                string role = GetAttribute<string>("role", n.Attributes);
                if (!map.ways.ContainsKey(reference)) AllNodesPresent = false;
                else if (role == "outer") OuterNodeIDs.AddRange(map.ways[reference].GetNodeIDs());
                else if (role == "inner") InnerNodeIDs.AddRange(map.ways[reference].GetNodeIDs());
            }
            else if (type == "node")
            {
                ulong reference = GetAttribute<ulong>("ref", n.Attributes);
                string role = GetAttribute<string>("role", n.Attributes);
                if (!map.ways.ContainsKey(reference)) AllNodesPresent = false;
                else if (role == "outer") OuterNodeIDs.Add(map.nodes[reference].GetId());
                else if (role == "inner") InnerNodeIDs.Add(map.nodes[reference].GetId());
            }
        }
        // only if all nodes are present in the current map, create area from it
        if (AllNodesPresent) CreateAreaFromNodes(OuterNodeIDs, InnerNodeIDs, node);
    }

    private void CreateAreaFromNodes(List<ulong> outer, List<ulong> inner, XmlNode node) {
        if (inner.Count < 2)
        {
            //Area a = new Area(outer, map, node );
            //map.areas.Add(a);
           // AddLayer(a.GetTexture());
        }
        else
        {
            CreateAreaWithHole(outer, inner, node);
        }
    }

    private void CreateAreaWithHole(List<ulong> outer, List<ulong> inner, XmlNode node) {
        int iFirst = 0;
        int iSecond = 0;
        float dist = 0;
        // iSecond is index van punt op inner boundary zo ver mogelijk van punt op index iFirst
        for (int i = 1; i <= inner.Count - 1; i++)
        {
            float tDist = Vector2.Distance(new Vector2(map.nodes[inner[0]].X, map.nodes[inner[0]].Y), new Vector2(map.nodes[inner[i]].X, map.nodes[inner[i]].Y));
            if (tDist > dist)
            {
                dist = tDist;
                iSecond = i;
            }
        }

        int oFirst = 0;
        int oSecond = 0;

        float distClosestFirst = 1000;
        float distClosestSecond = 1000;
        float tempDist;
        // find indices of nearest poiints on outer boundary for two inner points
        for (int i = 0; i <= outer.Count - 1; i++)
        {
            tempDist = Vector2.Distance(new Vector2(map.nodes[inner[iFirst]].X, map.nodes[inner[iFirst]].Y), new Vector2(map.nodes[outer[i]].X, map.nodes[outer[i]].Y));
            if (tempDist < distClosestFirst)
            {
                distClosestFirst = tempDist;
                oFirst = i;
            }
            tempDist = Vector2.Distance(new Vector2(map.nodes[inner[iSecond]].X, map.nodes[inner[iSecond]].Y), new Vector2(map.nodes[outer[i]].X, map.nodes[outer[i]].Y));
            if (tempDist < distClosestSecond)
            {
                distClosestSecond = tempDist;
                oSecond = i;
            }
        }

        if (oFirst == oSecond) oSecond = oFirst + 1;

        // voeg alle punten voor oFirst nog eens toe
        for (int i = 0; i < oFirst; i++)
        {
            outer.Add(outer[i]);
        }
        // en verwijder die 
        for (int i = oFirst - 1; i >= 0; i--)
        {
            outer.RemoveAt(i);
        }

        if (oSecond > oFirst) oSecond = oSecond - oFirst;
        else oSecond = outer.Count - oFirst + oSecond;
        oFirst = 0;

        List<ulong> leftHalfNodes = new List<ulong>();
        List<ulong> rightHalfNodes = new List<ulong>();
        // adding nodes from left shape
        for (int i = oFirst; i <= oSecond; i++)
        {
            rightHalfNodes.Add(outer[i]);
        }
        for (int i = iSecond; i <= inner.Count - 1; i++)
        {
            rightHalfNodes.Add(inner[i]);
        }
        rightHalfNodes.Add(inner[iFirst]);
        rightHalfNodes.Add(outer[oFirst]);
        //Area a = new Area(rightHalfNodes, map, node);
       //map.areas.Add(a);
        //AddLayer(a.GetTexture());

        // adding nodes from right shape
        leftHalfNodes.Add(outer[0]);
        for (int i = outer.Count - 1; i >= oSecond; i--)
        {
            leftHalfNodes.Add(outer[i]);
        }

        for (int i = iSecond; i >= iFirst; i--)
        {
            leftHalfNodes.Add(inner[i]);
        }
        leftHalfNodes.Add(outer[0]);
        //Area b = new Area(leftHalfNodes, map, node);
        //map.areas.Add(b);
       // AddLayer(a.GetTexture());
    }

    private void AddLayer(string texture)
    {
        Terrain terrain = map.terrainMaker.to.GetComponent<Terrain>();

        TerrainLayer[] oldLayers = terrain.terrainData.terrainLayers;

        TerrainLayer t0 = new TerrainLayer();
        t0.diffuseTexture = Resources.Load<Texture2D>("art/texturesAndNormals/" + texture);
        t0.normalMapTexture = Resources.Load<Texture2D>("art/texturesAndNormals/" + texture + "-normal");
        t0.tileOffset = Vector2.zero;
        t0.tileSize = Vector2.one;
        t0.name = texture;

        for (int i = 0; i < oldLayers.Length; i++)
        {
            if (oldLayers[i].name == t0.name)
            {
                return;
            }
        }

        TerrainLayer[] newLayers = new TerrainLayer[oldLayers.Length + 1];
        System.Array.Copy(oldLayers, 0, newLayers, 0, oldLayers.Length);
        newLayers[oldLayers.Length] = t0;

        terrain.terrainData.terrainLayers = newLayers;
    }
}
