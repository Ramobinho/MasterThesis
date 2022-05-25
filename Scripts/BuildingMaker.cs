using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

struct WindowInformation {
    public float width;
    public float height;
    public float depth;
    public float MinHorizontalGap;
    public float MaxHorizontalGap;
    public float horizontalCenterOfGravity;
    public float verticalCenterOfGravity;

    public WindowInformation(float w, float h, float d, float g, float gm, float hz, float vz) {
        width = w;
        height = h;
        depth = d;
        MinHorizontalGap = g; 
        MaxHorizontalGap = gm;
        horizontalCenterOfGravity = hz;
        verticalCenterOfGravity = vz; 
    }
}

struct DoorInformation
{
    public float width;
    public float height;
    public float minDepth;
    public float maxDepth;
    public float horizontalCenterOfGravity;
    public float verticalCenterOfGravity;

    public DoorInformation(float w, float h, float mid, float mad, float hz, float vz)
    {
        width = w;
        height = h;
        minDepth = mid;
        maxDepth = mad;
        horizontalCenterOfGravity = hz;
        verticalCenterOfGravity = vz;
    }
}
public class BuildingMaker : InfrastructureBehaviour
{
    public GameObject wideWindow;
    public GameObject normalBigWindow;
    public GameObject normalWindow0;
    public GameObject normalWindow1;
    public GameObject normalWindow2;
    public GameObject door0;
    public GameObject door1;
    public GameObject door2;
    public GameObject door3;


    Dictionary<GameObject, DoorInformation> doorInformationDictionary = new Dictionary<GameObject, DoorInformation>();
    Dictionary<GameObject, WindowInformation> windowInformationDictionary = new Dictionary<GameObject, WindowInformation>();
    GameObject buildingGo;
    GameObject wallsGo;
    GameObject roofGo;
    GameObject windowsGo;
    float foundationDepth;
    float maxHeight;
    bool buildingKeyPressed = false;

    IEnumerator Start() 
    {
        Debug.Log(map.IsBuildingKeyPressed());
        while (!map.IsReady || !buildingKeyPressed)
        {
            yield return null;
        }
       
        Debug.Log("building started");
        windowInformationDictionary.Add(wideWindow, new WindowInformation(3.5f, 1.8f, -0.35f, 2f, 2.5f, 1.75f, 0.9f));
        windowInformationDictionary.Add(normalBigWindow, new WindowInformation(3f, 2.25f, -0.35f, 2f, 2.5f, 1.5f, 1.12f));
        windowInformationDictionary.Add(normalWindow0, new WindowInformation(1.3f, 2.3f, 0, 1, 2, -0.65f, -1.15f));
        windowInformationDictionary.Add(normalWindow1, new WindowInformation(1.89f, 1.75f, -0.2f, 1, 2, 0.95f, 0.875f));
        windowInformationDictionary.Add(normalWindow2, new WindowInformation(0.6f, 0.6f, 0, 0.8f, 1f, -0.3f, 0.3f));

        doorInformationDictionary.Add(door0, new DoorInformation(2f, 3f, 0, 0, 1f, 0));
        doorInformationDictionary.Add(door1, new DoorInformation(1.5f, 2.5f, -0.5f, -0.25f, 0.75f, 0));
        doorInformationDictionary.Add(door2, new DoorInformation(1.1f, 1.8f, -0.5f, -0.25f, 0.55f, 0));
        doorInformationDictionary.Add(door3, new DoorInformation(1.5f, 2.5f, 0, 0, -0.75f, 2.5f));

        Debug.Log("amount of buildings theoretically" + map.buildings.Count);
        foreach (KMLBuilding b in map.buildings)
        {

            name = b.getName();
            buildingGo = new GameObject(name);
            buildingGo.transform.position = b.GetCentre() - map.bounds.Centre;
            wallsGo = new GameObject("wallsGo");
            wallsGo.transform.position = b.GetCentre() - map.bounds.Centre;
            roofGo = new GameObject("roofGo");
            roofGo.transform.position = b.GetCentre() - map.bounds.Centre;
            windowsGo = new GameObject("windowsGo");
            windowsGo.transform.position = b.GetCentre() - map.bounds.Centre;

            wallsGo.transform.parent = buildingGo.transform;
            roofGo.transform.parent = buildingGo.transform;
            windowsGo.transform.parent = buildingGo.transform;

            List<kmlNode> outer = b.getOuterBoundaries();
            outer = RemoveBadVertices(ref outer);
            List<kmlNode> inner = b.getInnerBoundaries();

            maxHeight = 0;
            float minHeight = 1000;
            for (int i = 0; i < outer.Count; i++)
            {
                if (outer[i].Z > maxHeight) maxHeight = outer[i].Z;
                if (outer[i].Z < minHeight) minHeight = outer[i].Z;
            }

            foundationDepth = maxHeight - minHeight + 2;   // safety margin of 2 meters
            foundationDepth = 0;
            if (inner.Count > 2)
            {
                inner = RemoveBadVertices(ref inner);
                createWalls(ref wallsGo, b, inner, "inner_walls", b.getWallMaterial(), -foundationDepth, outer[0].Height + foundationDepth, 0);
                createWalls(ref wallsGo, b, outer, "outer_walls", b.getWallMaterial(), -foundationDepth, outer[0].Height + foundationDepth, 0);
                createFullComplexRoof(ref roofGo, b, inner, outer, UnityEngine.Random.Range(0.1f, 0.6f), 1);
                PlaceWindows(windowsGo, inner, inner[0].Height);
            }
            else
            {
                createWalls(ref wallsGo, b, outer, "walls", b.getWallMaterial(), -foundationDepth, outer[0].Height + foundationDepth, 0);
                //findBiggestRectangleInside(outer);
                createFullRoof(ref roofGo, b, outer, UnityEngine.Random.Range(0.1f, 0.6f), 1);
            }
            PlaceWindows(windowsGo, outer, outer[0].Height);
            buildingGo.transform.position += new Vector3(0, maxHeight, 0);
        }
        Debug.Log("done making buildings");
    }

    public void SetBuildingKeyPressed() {
        buildingKeyPressed = true;
    }

     private List<kmlNode> RemoveBadVertices(ref List<kmlNode> nodes)
    {
        // eerste 2 chekcen met laatse node
        List<kmlNode> newList = new List<kmlNode>();
        kmlNode p1 = nodes[nodes.Count-2];
        kmlNode p2 = nodes[nodes.Count-1];
        kmlNode p3 = nodes[1];
        Vector3 t0 = new Vector3(p1.X, 0, p1.Y);
        Vector3 t1 = new Vector3(p2.X, 0, p2.Y);
        Vector3 t2 = new Vector3(p3.X, 0, p3.Y);
        Vector3 v1 = t1 - t0;
        Vector3 v2 = t2 - t1;
        if (Vector3.Angle(v1, v2) > 3)
        {
            newList.Add(p2);
        }

        for (int i = 2; i < nodes.Count; i++)
        {
            p1 = nodes[i - 2];
            p2 = nodes[i-1];
            p3 = nodes[i];
            t0 = new Vector3(p1.X, 0, p1.Y);
            t1 = new Vector3(p2.X, 0, p2.Y);
            t2 = new Vector3(p3.X, 0, p3.Y);
            v1 = t1 - t0;
            v2 = t2 - t1;
            if (Vector3.Angle(v1, v2) > 3) {
                newList.Add(p2);
            }
        }
        newList.Add(newList[0]);
        return newList;
    }
    private void PlaceWindows(GameObject g, List<kmlNode> nodes, float wallHeight)
    {
        int windowIndex = UnityEngine.Random.Range(0, windowInformationDictionary.Count);
        float wWidth = windowInformationDictionary.ElementAt(windowIndex).Value.width;
        float wHeight = windowInformationDictionary.ElementAt(windowIndex).Value.height;
        float wDepth = windowInformationDictionary.ElementAt(windowIndex).Value.depth;
        float wHGapMin = windowInformationDictionary.ElementAt(windowIndex).Value.MinHorizontalGap;
        float wHGapMax = windowInformationDictionary.ElementAt(windowIndex).Value.MaxHorizontalGap;
        float hCG = windowInformationDictionary.ElementAt(windowIndex).Value.horizontalCenterOfGravity;
        float vCG = windowInformationDictionary.ElementAt(windowIndex).Value.verticalCenterOfGravity;
        float levelheight = UnityEngine.Random.Range(3.5f, 4f);
        

        for (int i = 1; i < nodes.Count; i++)
        {
            int doorIndex = UnityEngine.Random.Range(0, doorInformationDictionary.Count);
            float dWidth = doorInformationDictionary.ElementAt(doorIndex).Value.width;
            float dHeight = doorInformationDictionary.ElementAt(doorIndex).Value.height;
            float midDepth = doorInformationDictionary.ElementAt(doorIndex).Value.minDepth;
            float madDepth = doorInformationDictionary.ElementAt(doorIndex).Value.maxDepth;
            float dhCG = doorInformationDictionary.ElementAt(doorIndex).Value.horizontalCenterOfGravity;
            float dvCG = doorInformationDictionary.ElementAt(doorIndex).Value.verticalCenterOfGravity;

            kmlNode p1 = nodes[i - 1];
            kmlNode p2 = nodes[i];
            // to calculate normal
            Vector3 t0 = new Vector3(p1.X, 0, p1.Y);
            Vector3 t1 = new Vector3(p2.X, 0, p2.Y);
            Vector3 t2 = new Vector3(p2.X, 0.1f, p2.Y);    
            Vector3 pNorm = (Vector3.Cross(t1 - t0, t2 - t0)).normalized;

            float wallWidth = Vector3.Distance(t0, t1);
            int amountHorizontally = (int)Math.Floor((wallWidth / (wWidth + UnityEngine.Random.Range(wHGapMin, wHGapMax))));
            int amountVertically = (int)Math.Floor(wallHeight/ (levelheight));

            if (wallWidth < 3) continue;
            Vector3 v1 = new Vector3(p1.X, 0, p1.Y) - map.bounds.Centre;
            Vector3 v2 = new Vector3(p2.X, 0, p2.Y) - map.bounds.Centre;
            Vector3 widthVector = Vector3.Lerp(v1, v2, 0.5f + dWidth/(2*wallWidth));
            // TODO: hoogte van terrain samplen en daar deur plaatse.
            // widthVector + new Vector3(0, dvCG, 0) + pNorm * (0.05f- UnityEngine.Random.Range(midDepth, madDepth))
            float h = map.terrainMaker.FindHeight(new Vector3(Vector3.Lerp(v1, v2, 0.5f).x, 0, Vector3.Lerp(v1, v2, 0.5f).z));
           /* h += h;
            h -= maxHeight;*/
            Vector3 doorPos = new Vector3(widthVector.x, h + dvCG , widthVector.z);
            GameObject doorGo = Instantiate(doorInformationDictionary.ElementAt(doorIndex).Key,doorPos + pNorm * (0.05f - UnityEngine.Random.Range(midDepth, madDepth)), Quaternion.Euler(0, -Vector3.SignedAngle(v1 - v2, transform.right, Vector3.up) + 270, 0));
            doorGo.transform.parent = g.transform;
            for (int j = 1; j < amountVertically + 1; j++)
            {
                float height = ((float)(j) / (amountVertically + 1)) * wallHeight + vCG;
                for (int k = 1; k < amountHorizontally + 1 ; k++)
                {
                    widthVector = Vector3.Lerp(v1, v2, (float)(k) / (amountHorizontally + 1) + hCG / wallWidth);
                    if (UnityEngine.Random.Range(0, 10) > 1 && (Vector3.Distance(doorGo.transform.position, widthVector + new Vector3(0, height, 0) + pNorm * (0.1f - wDepth)) > 5))
                    {
                        GameObject go = Instantiate(windowInformationDictionary.ElementAt(windowIndex).Key, widthVector + new Vector3(0, height, 0) + pNorm * (0.1f - wDepth), Quaternion.Euler(0, -Vector3.SignedAngle(v1 - v2, transform.right, Vector3.up) + 270, 0));
                        go.transform.parent = g.transform;
                    }
                }
            }
        } 
    }


    private void createFullRoof(ref GameObject parent, KMLBuilding b, List<kmlNode> nodes, float roofHeight, float roofOverhang)
    {
        createRoof(ref parent,b, nodes, "roof", true,0, roofOverhang);
        createRoof(ref parent, b, nodes, "roof", false, roofHeight, roofOverhang);
        createWalls(ref parent, b, nodes, "roofwalls", b.getRoofMaterial(),nodes[0].Height, roofHeight, roofOverhang);
    }

    private void createFullComplexRoof(ref GameObject parent,KMLBuilding b, List<kmlNode> inner, List<kmlNode> outer, float roofHeight, float roofOverhang)
    {
        createComplexRoof(ref parent, b, inner, outer, true, roofHeight, roofOverhang);
        createComplexRoof(ref parent, b, inner, outer, false, roofHeight, roofOverhang);
        createWalls(ref parent, b, outer, "roofwalls", b.getRoofMaterial(), outer[0].Height, roofHeight, roofOverhang);
    }

    private void createWalls(ref GameObject parent, KMLBuilding b, List<kmlNode> nodes, string name, Material material, float heightOffset, float height, float scaleOffset) {


        GameObject go = new GameObject(name);
        Vector3 localOrigin = b.GetCentre();
        go.transform.position = localOrigin - map.bounds.Centre;

        MeshFilter mf = go.AddComponent<MeshFilter>();
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.material = material;

        // Create the collections for the object's vertices, indices, UVs etc.
        List<Vector3> vectors = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> indices = new List<int>();

        for (int i = 1; i < nodes.Count; i++)
        {

            kmlNode p1 = nodes[i - 1];
            kmlNode p2 = nodes[i];

            Vector3 v1 = new Vector3(p1.X, heightOffset, p1.Y) - b.GetCentre();
            Vector3 v2 = new Vector3(p2.X, heightOffset, p2.Y) - b.GetCentre();
            Vector3 v3 = v1 + new Vector3(0, height, 0);
            Vector3 v4 = v2 + new Vector3(0, height, 0);

            vectors.Add(v1);
            vectors.Add(v2);
            vectors.Add(v3);
            vectors.Add(v4);

            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(1, 0));
            uvs.Add(new Vector2(0, 1));
            uvs.Add(new Vector2(1, 1));

            normals.Add(-Vector3.forward);
            normals.Add(-Vector3.forward);
            normals.Add(-Vector3.forward);
            normals.Add(-Vector3.forward);

            int idx1, idx2, idx3, idx4;
            idx4 = vectors.Count - 1;
            idx3 = vectors.Count - 2;
            idx2 = vectors.Count - 3;
            idx1 = vectors.Count - 4;

            // first triangle v1, v3, v2
            indices.Add(idx1);
            indices.Add(idx3);
            indices.Add(idx2);

            // second         v3, v4, v2
            indices.Add(idx3);
            indices.Add(idx4);
            indices.Add(idx2);

            // third          v2, v3, v1
            indices.Add(idx2);
            indices.Add(idx3);
            indices.Add(idx1);

            // fourth         v2, v4, v3
            indices.Add(idx2);
            indices.Add(idx4);
            indices.Add(idx3);

        }

        // Apply the data to the mesh
        mf.mesh.vertices = vectors.ToArray();
        mf.mesh.normals = normals.ToArray();
        mf.mesh.triangles = indices.ToArray();
        mf.mesh.uv = uvs.ToArray();

        go.transform.parent = parent.transform;
        Bounds bounds = mf.mesh.bounds;
        go.transform.localScale += new Vector3(scaleOffset / bounds.size.x, 0, scaleOffset / bounds.size.z);
    }

    private void createRoof(ref GameObject parent, KMLBuilding b, List<kmlNode> nodes, string name, bool reverse, float heightOffset, float scaleOffset) {

        // Create an instance of the object and place it in the centre of its points
        GameObject go = new GameObject(name);
        Vector3 localOrigin = b.GetCentre();
        go.transform.position = localOrigin - map.bounds.Centre;

        MeshFilter mf = go.AddComponent<MeshFilter>();
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.material = b.getRoofMaterial();

        Vector2[] vertices2D = new Vector2[nodes.Count-1];

        for (int i = 0; i < vertices2D.Length; i++) {
            vertices2D[i] = new Vector2(nodes[i].X, nodes[i].Y) - new Vector2(b.GetCentre().x, b.GetCentre().z);
        }

        Triangulator tr = new Triangulator(vertices2D, reverse);
        int[] indices = tr.Triangulate();

        // Create the Vector3 vertices
        Vector3[] vertices = new Vector3[vertices2D.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new Vector3(vertices2D[i].x, nodes[i].Height + heightOffset, vertices2D[i].y);
        }

        mf.mesh.vertices = vertices;
        mf.mesh.triangles = indices;
        mf.mesh.RecalculateNormals();
        mf.mesh.RecalculateBounds();

        Bounds bounds = mf.mesh.bounds;
        Vector2[] uvs = new Vector2[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            uvs[i] = new Vector2(vertices[i].x / bounds.size.x, vertices[i].z / bounds.size.z);
        }
        mf.mesh.uv = uvs;

        mr.material.mainTextureScale = new Vector2(bounds.size.x, bounds.size.z);

        go.transform.parent = parent.transform;
        go.transform.localScale += new Vector3(scaleOffset / bounds.size.x, 0, scaleOffset / bounds.size.z);


    }




    // splitting into 2 parts. 2 daken tegen elkaar eigenlijk
    private void createComplexRoof(ref GameObject parent, KMLBuilding b, List<kmlNode> inner, List<kmlNode> outer, bool reverse, float offset, float roofOverhang)
    {
        int iFirst = 0;
        int iSecond = 0;
        float dist = 0;
        // iSecond is index van punt op inner boundary zo ver mogelijk van punt op index iFirst
        for (int i = 1; i <= inner.Count - 1; i++) {
            float tDist = Vector2.Distance(new Vector2(inner[0].X, inner[0].Y), new Vector2(inner[i].X, inner[i].Y));
            if (tDist > dist)
            {
                dist = tDist;
                iSecond = i;
            }
        }
 
        int oFirst = 0;
        int oSecond  = 0;

        float distClosestFirst = 1000;
        float distClosestSecond = 1000;
        float tempDist;
        // find indices of nearest poiints on outer boundary for two inner points
        for (int i = 0; i <= outer.Count - 1; i++) {
            tempDist = Vector2.Distance(new Vector2(inner[iFirst].X, inner[iFirst].Y), new Vector2(outer[i].X, outer[i].Y));
            if (tempDist < distClosestFirst)
            {
                distClosestFirst = tempDist;
                oFirst = i;
            }
            tempDist = Vector2.Distance(new Vector2(inner[iSecond].X, inner[iSecond].Y), new Vector2(outer[i].X, outer[i].Y));
            if (tempDist < distClosestSecond)
            {
                distClosestSecond = tempDist;
                oSecond = i;
            }
        }

        if (oFirst == oSecond) oSecond = oFirst + 1;

        // voeg alle punten voor oFirst nog eens toe
        for (int i = 0; i < oFirst; i++) {
            outer.Add(outer[i]);
        }
        // en verwijder die 
        for (int i = oFirst-1; i >= 0; i--) {
            outer.RemoveAt(i);
        }

        if (oSecond > oFirst) oSecond = oSecond - oFirst;
        else oSecond = outer.Count - oFirst + oSecond;
        oFirst = 0;

        List<kmlNode> leftHalfNodes = new List<kmlNode>();
        List<kmlNode> rightHalfNodes = new List<kmlNode>();
        // adding nodes from left shape
        for (int i = oFirst; i <= oSecond; i++)
        {
            rightHalfNodes.Add(outer[i]);
        }
        for (int i = iSecond; i <= inner.Count-1; i++)
        {
            rightHalfNodes.Add(inner[i]);
        }
        rightHalfNodes.Add(inner[iFirst]);
        rightHalfNodes.Add(outer[oFirst]);
        createRoof(ref parent, b, rightHalfNodes, "right",reverse,offset, roofOverhang);

        // adding nodes from right shape
        leftHalfNodes.Add(outer[0]);
        for (int i = outer.Count-1; i >= oSecond; i--)
        {
            leftHalfNodes.Add(outer[i]);
        }

        for (int i = iSecond; i >= iFirst; i--)
        {
            leftHalfNodes.Add(inner[i]);
        }
        leftHalfNodes.Add(outer[0]);
        createRoof(ref parent, b, leftHalfNodes, "left",reverse, offset, roofOverhang);
    }
}