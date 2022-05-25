
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO: if is lit, lantaarnpalen toevoegen tussen wegstukken.

public class RoadMaker : InfrastructureBehaviour
{
    public GameObject lantern;
    private bool RoadKeyPressed = false;
    private float lanternDistance = 20;
    IEnumerator Start()
    {
        while (!map.IsReady || !RoadKeyPressed)
        {
            yield return null;
        }


        Debug.Log("start painting roads");
        List<Road> temp = map.roads;
        temp.Sort(SortHierarchy);
        foreach (Road road in temp)
        {
            List<ulong> nodes = road.GetnodeIDs();
            Vector2 link1 = new Vector2(0, 0);
            Vector2 link2 = new Vector2(0, 0);

            Vector2[] points = new Vector2[4];
            Vector2[] triangles = new Vector2[3];
            TerrainPainter terrainPainter = map.terrainMaker.to.GetComponent<TerrainPainter>();
            for (int i = 1; i < nodes.Count; i++)
            {
                
                OsmNode p1 = map.nodes[nodes[i - 1]];
                OsmNode p2 = map.nodes[nodes[i]];

                Vector3 s1 = p1;
                Vector3 s2 = p2;
                
                Vector3 diff = (s2 - s1).normalized;
                var cross = Vector3.Cross(diff, Vector3.up) * road.GetWidth() * road.GetLanes();

                float dist = Vector3.Distance(s1, s2);

                //if (road.IsLit()) {
                    for (float j = lanternDistance; j < dist; j += lanternDistance)
                    {
                        if (!(s1.x > map.bounds.MaxX | s1.x < map.bounds.MinX | s1.z > map.bounds.MaxY | s1.z < map.bounds.MinY))
                        {
                            Instantiate(lantern, Vector3.Lerp(s1 + cross - map.bounds.Centre, s2 + cross - map.bounds.Centre, lanternDistance / dist), Quaternion.Euler(0, 0, 0f));
                            Instantiate(lantern, Vector3.Lerp(s1 - cross - map.bounds.Centre, s2 - cross - map.bounds.Centre, lanternDistance / dist), Quaternion.Euler(0, 0, 0f));
                    }
              
                    }
               // }

                // Create points that represent the width of the road
                Vector2 crossV = new Vector2(cross.x, cross.z);
                points[0] = new Vector2(s1.x, s1.z) + crossV;
                points[3] = new Vector2(s1.x, s1.z) - crossV;
                points[1] = new Vector2(s2.x, s2.z) + crossV;
                points[2] = new Vector2(s2.x, s2.z) - crossV;
                terrainPainter.PaintRoad(points, road);

                if (i > 1)
                {

                    // 1
                    triangles[0] = link1;
                    triangles[2] = new Vector2(s1.x, s1.z) + crossV;
                    triangles[1] = new Vector2(s1.x, s1.z) - crossV;
                    terrainPainter.PaintRoad(triangles, road);

                    triangles[0] = link2;
                    triangles[2] = new Vector2(s1.x, s1.z) + crossV;
                    triangles[1] = new Vector2(s1.x, s1.z) - crossV;
                    terrainPainter.PaintRoad(triangles, road);

                }
                link1 = points[2];
                link2 = points[1];
            }

        }
        Debug.Log("done with roads");
    }

    private static int SortHierarchy(Road road1, Road road2)
    {
        if (road1.GetHierarchy() < road2.GetHierarchy()) return -1;
        if (road1.GetHierarchy() > road2.GetHierarchy()) return 1;
        return 0;
    }

    public void SetRoadKeyPressed() {
        RoadKeyPressed = true;
    }
} 
