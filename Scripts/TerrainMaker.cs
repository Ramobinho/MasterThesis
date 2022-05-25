using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

internal class TerrainMaker : InfrastructureBehaviour
{
    public string HeightMap;
    public Material terrainMaterial;
    public OsmBounds bounds;

    public GameObject to;
    private Terrain terrain;

    public int xSize { get; private set; }
    public int zSize { get; private set; }

    private int newSize;

    private int originX;
    private int originZ;

    private float[,] heights;
    private float[,] interHeights;

    private float minHeight = 10000;
    private float maxHeight = 0;

    public int imageHeight;
    public int imageWidth;

    private double topleftX;
    private double topleftZ;
    private double bottomleftX;
    private double bottomleftZ;
    private double toprightX;
    private double toprightZ;
    private double bottomrightX;
    private double bottomrightZ;

    public int res = 64000;
    public float scaleX { get; private set; }

    public float scaleZ { get; private set; }

    public TerrainMaker(string heightMap, Material m, OsmBounds bounds)
    {
        HeightMap = heightMap;
        terrainMaterial = m;
        this.bounds = bounds;
    }

    public GameObject GetTerrainObject() {
        return to;
    }
    public void StartTerrainGeneration()
    {
        Debug.Log("Start terrain generation loop");
        SetBounds();
        GetHeightData();

        InterpolateHeights();

        GenerateTerrain(maxHeight, minHeight);

        Debug.Log("terrain generation complete");
    }

    private void GetHeightData()
    {

        Debug.Log("Get heights");

        heights = new float[zSize + 1, xSize + 1];

        using (var file = System.IO.File.OpenRead(HeightMap))
        using (var reader = new System.IO.BinaryReader(file))
        {
            for (int z = 0; z < imageHeight; z++)
            {
                for (int x = 0; x < imageWidth; x++)
                {
                    float v = reader.ReadSingle();
                    if (x >= originX && x <= originX + xSize && z >= originZ && z <= originZ + zSize)
                    {
                        v = v < 0 ? 0 : v;
                        heights[zSize - z + originZ, x - originX] = v;
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

        for (int i = 0; i <= xSize; i++)
        {
            for (int j = 0; j <= zSize; j++)
            {
                heights[j, i] -= minHeight;
                heights[j, i] /= (maxHeight - minHeight);
            }
        }

        Debug.Log("Get heights done");
    }

    private void SetBounds()
    {
        Debug.Log("Set Bounds");

        /*double upleftX = 505249.054;
        double upleftZ = 6572592.141;
        double downleftX = 505249.054;
        double downleftZ = 6540725.562;
        double uprightX = 555971.874;
        double uprightZ = 6572592.141;
        double downrightX = 555971.874;
        double downrightZ = 6540725.562;*/
        double rangeX = (toprightX - topleftX + bottomrightX - bottomleftX) / 2;

        double rangeZ = (topleftZ - bottomleftZ + toprightZ - bottomrightZ) / 2;
        double minX = topleftX;
        double maxZ = topleftZ;

        scaleX = (float)rangeX / imageWidth;
        scaleZ = (float)rangeZ / imageHeight;

        originX = (int)((MercatorProjection.lonToX(bounds.MinLon) - minX) / scaleX);
        originZ = (int)((maxZ - MercatorProjection.latToY(bounds.MaxLat)) / scaleZ);

        xSize = (int)((MercatorProjection.lonToX(bounds.MaxLon) - MercatorProjection.lonToX(bounds.MinLon)) / scaleX);
        zSize = (int)((MercatorProjection.latToY(bounds.MaxLat) - MercatorProjection.latToY(bounds.MinLat)) / scaleZ);

        closestPower();

        Debug.Log("Set Bounds done");

    }

    private void closestPower()
    {
        Debug.Log("Closest power");

        int size = 1;
        int thresh = xSize > zSize ? xSize : zSize;
        while (size < thresh)
        {
            size *= 2;
        }

        newSize = size;

        Debug.Log("Closest power done");
    }

    void GenerateTerrain(float maxHeight, float minHeight)
    {
        Debug.Log("Starting terrain generation");

        TerrainData td = new TerrainData();
        td.heightmapResolution = newSize + 1;
        td.baseMapResolution = newSize;
        td.SetDetailResolution(newSize, 32);
        td.size = new Vector3(newSize, maxHeight - minHeight, newSize);
        to = new GameObject("Terrain");

        terrain = to.AddComponent<Terrain>();
        TerrainCollider tercol = to.AddComponent<TerrainCollider>();

        tercol.terrainData = td;
        terrain.terrainData = td;
        terrain.materialTemplate = terrainMaterial;
        td.SetHeights(0, 0, interHeights);

        td.size = new Vector3(xSize * scaleX, maxHeight - minHeight, zSize * scaleZ);

        terrain.transform.position = new Vector3(terrain.transform.position.x,
                                             terrain.transform.position.y,
                                             terrain.transform.position.z - (zSize * scaleZ));

        terrain.transform.position = new Vector3(terrain.transform.position.x - (int)((MercatorProjection.lonToX(bounds.MaxLon) - MercatorProjection.lonToX(bounds.MinLon)) / 2),
                                             terrain.transform.position.y,
                                             terrain.transform.position.z + (int)((MercatorProjection.latToY(bounds.MaxLat) - MercatorProjection.latToY(bounds.MinLat)) / 2));

        Debug.Log("Terrain generation complete");
    }

    private void InterpolateHeights()
    {
        Debug.Log("Starting to interpolate heights");

        interHeights = new float[newSize + 1, newSize + 1];

        float stepX = xSize + 1;
        float stepZ = zSize + 1;

        stepX /= (newSize + 1);
        stepZ /= (newSize + 1);

        for (int i = 0; i <= newSize; i++)
        {
            for (int j = 0; j <= newSize; j++)
            {
                float[] x_pos = { i - 1, i, i + 1, i + 2 };
                float[] z_pos = { j - 1, j, j + 1, j + 2 };

                float[] input = new float[4];
                float[] output = new float[4];
                for (int u = 0; u < 4; u++)
                {
                    for (int v = 0; v < 4; v++)
                    {
                        int z = (int)(z_pos[u] * stepZ);
                        int x = (int)(x_pos[v] * stepX);
                        if (x < 0) x = 0;
                        else if (x > xSize) x = xSize;

                        if (z < 0) z = 0;
                        else if (z > zSize) z = zSize;

                        input[v] = heights[z, x];
                    }
                    output[u] = CubicInterpol(input, (i * stepX) - (int)(i * stepX));
                }
                float r = CubicInterpol(output, (j * stepZ) - (int)(j * stepZ));
                interHeights[j, i] = r;
            }
        }

        Debug.Log("Interpolating hieghts done");
    }

    //v0 is punt voor a p en v3 is punt na q, dus v1 is p en v2 is q
    //hieruit volgt dat frac dus de afstand van het te interpoleren punt tussen v1 en v2    float CubicInterpol(float[] input, float frac)
    float CubicInterpol(float[] input, float frac)
    {
        float p = 0.5f * (input[3] - input[2]) - 1.5f * (input[0] - input[1]);
        float q = 0.5f * (input[0] + input[2]) - input[1] - p;
        float r = 0.5f * (input[2] - input[0]);

        return (frac * ((frac * ((frac * p) + q)) + r)) + input[1];



    }

    //Dit is de functie die MapReader gebruikt om aan een OsmNode een hoogte toe te kennen.
    public float FindHeight(Vector3 p)
    {
        //is zien of via terraindata de heights gelezen kunnen worden.        

        Vector3 v = p - bounds.Centre;

        if (v.x < terrain.GetPosition().x || v.z < terrain.GetPosition().z || v.x > terrain.GetPosition().x + (xSize * scaleX) || v.z >= terrain.GetPosition().z + (zSize * scaleZ))
        {
            return 0;
        }

        return terrain.SampleHeight(v);
    }

    public void setParam(int imageHeight, int imageWidth, double topleftX, double topleftZ,
                                                          double bottomleftX, double bottomleftZ,
                                                          double toprightX, double toprightZ,
                                                          double bottomrightX, double bottomrightZ)
    {
        this.imageHeight = imageHeight;
        this.imageWidth = imageWidth;
        this.topleftX = topleftX;
        this.topleftZ = topleftZ;
        this.bottomleftX = bottomleftX;
        this.bottomleftZ = bottomleftZ;
        this.toprightX = toprightX;
        this.toprightZ = toprightZ;
        this.bottomrightX = bottomrightX;
        this.bottomrightZ = bottomrightZ;
    }
}