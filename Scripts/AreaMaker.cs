using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaMaker : InfrastructureBehaviour
{

    public GameObject weed1;
    public GameObject weed2;
    public GameObject grass;
    public GameObject bush;
    public GameObject tree1;
    public GameObject tree2;
    public GameObject tree3;
    public GameObject rock1;
    public GameObject rock2;
    public GameObject rock3;
    private GameObject[] vegetables;
    private bool AreaKeyPressed = false;


    IEnumerator Start()
    {
        while (!map.IsReady || !AreaKeyPressed)
        {
            yield return null;
        }

        vegetables = new GameObject[10];
        vegetables[0] = weed1;
        vegetables[1] = weed2;
        vegetables[2] = grass;
        vegetables[3] = bush;
        vegetables[4] = tree1;
        vegetables[5] = tree2;
        vegetables[6] = tree3;
        vegetables[7] = rock1;
        vegetables[8] = rock2;
        vegetables[9] = rock3;


        List<Area> temp = map.areas;
        temp.Sort(SortHierarchy);
        foreach (Area area in temp)
        {
            TerrainPainter terrainPainter = map.terrainMaker.to.GetComponent<TerrainPainter>();
            List<VegetationStruct> bushes = terrainPainter.PaintArea(area);
            foreach (VegetationStruct v in bushes)
            {
                float thickness = UnityEngine.Random.Range(0.6f, 1.4f);
                float toMap = Mathf.InverseLerp(0.6f, 1.4f, thickness);
                float height = Mathf.Lerp(0.6f, 2f, toMap);
                GameObject go = Instantiate(vegetables[v.index], v.position - map.bounds.Centre, Quaternion.Euler(0, UnityEngine.Random.Range(0.0f, 360.0f), 0f));
                go.transform.localScale = new Vector3(thickness, height, thickness);
            }
        }

        Debug.Log("done painting areas");
    }

    private static int SortHierarchy(Area area1, Area area2)
    {
       //if (area1.GetClassification() == "water") return 1;
        //if (area2.GetClassification() == "water") return -1;
        if (area1.GetSurfaceArea() < area2.GetSurfaceArea()) return 1;
        if (area1.GetSurfaceArea() > area2.GetSurfaceArea()) return -1;
        return 0;
    }
    public void SetAreaKeyPressed()
    {
        AreaKeyPressed = true;
    }
} 