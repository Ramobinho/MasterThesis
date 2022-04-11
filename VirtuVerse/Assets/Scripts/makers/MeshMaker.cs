using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MapReader))]
class MeshMaker : InfraStructureBehaviour
{
    public string HeightMap;

    public Material terrainMaterial;

    public GameObject[] gos { get; private set; }
    public MeshFilter[] mfs { get; private set; }
    public MeshRenderer[] mrs { get; private set; }

    public MeshCollider[] mcs { get; private set; }

    Vector3[,] vertices;
    int[,] triangles;
    Vector2[,] uvs;
    Color[,] colors;

    public Gradient gradient;

    private int xSize;
    private int zSize;
    private int s_xSize;
    private int s_zSize;

    private int originX;
    private int originZ;

    private float[,] heights;

    public int quads { get; private set; }

    public int res = 64000;
    public int overhead = 200;
    private float scaleX;
    private float scaleZ;

    IEnumerator Start()
    {
        Debug.Log("Start mesh generation loop");
        GetHeightData();

        CreateShape();
        UpdateMesh();

        map.HeightsReady();

        yield return null;
    }

    private void GetHeightData()
    {
        SetBounds();

        Debug.Log("Get heights");
        
        heights = new float[xSize + 1, zSize + 1];

        //volgens mij gebruik ik dit momenteel niet echt, maar kan gebruikt worden om hoogtes te schalen
        float maxHeight = 0;
        float minHeight = 10000;

        //gaat eigenlijk heel de raw file doorlopen en uit de regio bepaald door origin(x)(z) de hoogte uitnemen.
        using (var file = System.IO.File.OpenRead(HeightMap))
        using (var reader = new System.IO.BinaryReader(file))
        {
            for (int z = 0; z < 20171; z++)
            {
                for (int x = 0; x < 32107; x++)
                {
                    float v = (float)reader.ReadUInt16();
                    if (x >= originX && x <= originX + xSize && z >= originZ && z <= originZ + zSize)
                    {
                        heights[x - originX, zSize - z + originZ] = v;
                        maxHeight = v > maxHeight ? v : maxHeight;
                        minHeight = v < minHeight ? v : minHeight;
                    }
                }
                if (z >= originZ + zSize)
                {
                    break;
                }
            }
        }
        Debug.Log("Get heights done");
    }

    //dit is eigenlijk de link tussen osm en raw, hier probeer ik dus de origin en nodige size/schaal te bepalen
    private void SetBounds()
    {
        Debug.Log("Set Bounds");

        double upleftX = 505249.054;
        double upleftZ = 6572592.141;
        double downleftX = 505249.054;
        double downleftZ = 6540725.562;
        double uprightX = 555971.874;
        double uprightZ = 6572592.141;
        double downrightX = 555971.874;
        double downrightZ = 6540725.562;
        double rangeX = (uprightX - upleftX + downrightX - downleftX) / 2;

        double rangeZ = (upleftZ - downleftZ + uprightZ - downrightZ) / 2;
        double minX = upleftX;
        double maxZ = upleftZ;

        scaleX = (float)rangeX / 32107;
        scaleZ = (float)rangeZ / 20171;

        originX = (int)((MercatorProjection.lonToX(map.bounds.MinLon) - minX) / scaleX);
        originZ = (int)((maxZ - MercatorProjection.latToY(map.bounds.MaxLat)) / scaleZ);

        xSize = (int)((MercatorProjection.lonToX(map.bounds.MaxLon) - MercatorProjection.lonToX(map.bounds.MinLon)) / scaleX);
        zSize = (int)((MercatorProjection.latToY(map.bounds.MaxLat) - MercatorProjection.latToY(map.bounds.MinLat)) / scaleZ);

        Vector2 topleft = new Vector2((float)upleftX, (float)upleftZ);
        Vector2 bottomleft = new Vector2((float)downleftX, (float)downleftZ);
        Vector2 topright = new Vector2((float)uprightX, (float)uprightZ);
        Vector2 bottomright = new Vector2((float)downrightX, (float)downrightZ);

    }
    
    //Hier ga ik de vertices en driehoeken vormen. beetje ingewikkeld hoe het juist werkt maar dus vertices heeft als eerste de verschillende punten en als tweede het quadrant waartoe dat punt behoort.
    //Vult vertices op van links onder naar rechtsboven, quad per quad. ook de driehoeken zijn per quadrant gesorteerd.
    void CreateShape()
    {

        CalculateGrid();

        Debug.Log("Fill shape");

        float maxHeight = 0;
        float minHeight = 10000;

        for (int counter = 0, zGrid = 0; zGrid < quads; zGrid++)
        {
            for(int xGrid = 0; xGrid < quads; xGrid++, counter++)
            {
                for(int i = 0, z = 0; z <= s_zSize; z++)
                {
                    for(int x = 0; x <= s_xSize; x++, i++)
                    {
                        float height = heights[x + xGrid * s_xSize, z + zGrid * s_zSize];
                        vertices[i, counter] = new Vector3(((x + xGrid * s_xSize) * scaleX) - (xSize / 2)*scaleX, height, ((z + zGrid * s_zSize) * scaleZ) - (zSize / 2)*scaleZ);
                        uvs[i, counter] = new Vector2((float)x / s_xSize, (float)z / s_zSize);
                        maxHeight = height > maxHeight ? height : maxHeight;
                        minHeight = height < minHeight ? height : minHeight;
                    }
                }
            }
        }

        SetTextures(minHeight, maxHeight);

        for (int counter = 0; counter < quads * quads; counter++)
        {
            for (int ti = 0, vi = 0, z = 0; z < s_zSize; z++, vi++)
            {
                for (int x = 0; x < s_xSize; x++, ti += 6, vi++)
                {
                    triangles[ti, counter] = vi;
                    triangles[ti + 3, counter] = triangles[ti + 2, counter] = vi + 1;
                    triangles[ti + 4, counter] = triangles[ti + 1, counter] = vi + s_xSize + 1;
                    triangles[ti + 5, counter] = vi + s_xSize + 2;
                }
            }
        }

        Debug.Log("Fill shape done");
    }

    //Dit mag je opzich negeren, voorlopige oplossing om het terrein wat kleur te geven
    private void SetTextures(float minHeight, float maxHeight)
    {
        for (int counter = 0, zGrid = 0; zGrid < quads; zGrid++)
        {
            for (int xGrid = 0; xGrid < quads; xGrid++, counter++)
            {
                for (int i = 0, z = 0; z <= s_zSize; z++)
                {
                    for (int x = 0; x <= s_xSize; x++, i++)
                    {
                        colors[i, counter] = gradient.Evaluate(Mathf.InverseLerp(minHeight, maxHeight, vertices[i, counter].y));
                    }
                }
            }
        }
    }
    
    //Hier ga ik de quadranten vormen en berekenen hoe ik het gebied best opsplits
    private void CalculateGrid()
    {
        Debug.Log("Calculate grid");

        int thresh = ((xSize + 1) * (zSize + 1)) / res;
        quads = 1;
        while (quads * quads < thresh)
        {
            quads++;
        }

        s_xSize = xSize / quads;
        s_zSize = zSize / quads;

        vertices = new Vector3[(s_xSize + 1) * (s_zSize + 1), quads * quads];
        uvs = new Vector2[(s_xSize + 1) * (s_zSize + 1), quads * quads];
        triangles = new int[s_xSize * s_zSize * 6, quads * quads];
        colors = new Color[(s_xSize + 1) * (s_zSize + 1), quads * quads];

        Debug.Log("Calculate grid done");
    }

    //Simpelweg mesh updaten, dit moet per quad gebeuren natuurlijk
    void UpdateMesh()
    {

        Debug.Log("Update Mesh");

        gos = new GameObject[quads * quads];
        mfs = new MeshFilter[quads * quads];
        mrs = new MeshRenderer[quads * quads];
        mcs = new MeshCollider[quads * quads];

        for (int i = 0; i < quads * quads; i++)
        {
            gos[i] = new GameObject("Terrain" + i);
            mfs[i] = gos[i].AddComponent<MeshFilter>();
            mrs[i] = gos[i].AddComponent<MeshRenderer>();
            mcs[i] = gos[i].AddComponent<MeshCollider>();

            mrs[i].material = terrainMaterial;

            mfs[i].mesh.vertices = Enumerable.Range(0, vertices.GetLength(0)).Select(x => vertices[x, i]).ToArray();
            mfs[i].mesh.triangles = Enumerable.Range(0, triangles.GetLength(0)).Select(x => triangles[x, i]).ToArray();
            mfs[i].mesh.uv = Enumerable.Range(0, uvs.GetLength(0)).Select(x => uvs[x, i]).ToArray();
            mfs[i].mesh.colors = Enumerable.Range(0, uvs.GetLength(0)).Select(x => colors[x, i]).ToArray();

            mfs[i].mesh.RecalculateNormals();

            mcs[i].sharedMesh = mfs[i].sharedMesh;
        }

        Debug.Log("Update mesh done");
    }

    //Dit is de functie die MapReader gebruikt om aan een OsmNode een hoogte toe te kennen.
    public Vector3 FindHeight(OsmNode p)
    {
        Vector3 v = p - map.bounds.Centre;

        if (v.x < vertices[0, 0].x || v.z < vertices[0, 0].z || v.x > vertices[(s_xSize + 1) * (s_zSize + 1) - 1, quads * quads - 1].x || v.z >= vertices[(s_xSize + 1) * (s_zSize + 1) - 1, quads * quads - 1].z)
        {
            return v;
        }

        int x_count = (int)(((v.x / scaleX) + xSize / 2) / s_xSize);
        int z_count = (int)(((v.z / scaleZ) + zSize / 2) / s_zSize);
        z_count *= quads;
        int counter = x_count + z_count;
        Vector3 max = vertices[(s_xSize + 1) * (s_zSize + 1) - 1, counter];
        Vector3 min = vertices[0, counter];
        x_count = (int)((v.x - min.x) / scaleX);
        z_count = (int)((v.z - min.z) / scaleZ);
        z_count *= (s_xSize + 1);
        int j = x_count + z_count;
        Vector3 a = vertices[j, counter];
        Vector3 b = vertices[j+1, counter];
        Vector3 c = vertices[j+s_xSize+1, counter];
        Vector3 d = vertices[j+s_xSize+2, counter];
        Vector3 abu = Vector3.Lerp(a, b, (v.x - a.x)/(b.x - a.x));
        Vector3 cdv = Vector3.Lerp(c, d, (v.x - c.x) / (d.x - c.x));
        return Vector3.Lerp(abu,cdv, (cdv.z - v.z)/(cdv.z - abu.z));
        
    }
}

