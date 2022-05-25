using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RelationMaker : InfrastructureBehaviour
{

    bool RelationKeyPressed = false;
    IEnumerator Start()
    {
        while (!map.IsReady || !RelationKeyPressed)
        {
            yield return null;
        }

        foreach (OsmRelation r in map.relations)
        {
            if (!r.AreAllNodesPresent()) continue;
            List<ulong> inner = r.GetInnerBoundaries();
            List<ulong> outer = r.GetOuterBoundaries();
            if (inner.Count < 2)
            {
                CreatePlane(r, outer);
            }
            else
            {
                createComplexPlane(r, inner, outer);
            }
        }
        Debug.Log("done making relations");
    }

    private void CreatePlane(OsmRelation r, List<ulong> nodes)
    {

        TerrainPainter terrainPainter = map.terrainMaker.to.GetComponent<TerrainPainter>();
        terrainPainter.PaintRelation(nodes, map, r.GetTexture());
    }


    private void createComplexPlane(OsmRelation r, List<ulong> inner, List<ulong> outer)
    {
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
        CreatePlane(r, rightHalfNodes);
        TerrainPainter terrainPainter = map.terrainMaker.to.GetComponent<TerrainPainter>();
        terrainPainter.PaintRelation(rightHalfNodes, map, r.GetTexture());

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
        CreatePlane(r, leftHalfNodes);
        terrainPainter.PaintRelation(leftHalfNodes, map, r.GetTexture());
    }

    public void SetRelationKeyPressed()
    {
        RelationKeyPressed = true;
    }
}