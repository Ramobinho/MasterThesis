using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MapReader))]
class TerrainMaker : InfraStructureBehaviour
{
    public string HeightMap;

    public Material terrainMaterial;

    private int xSize;
    private int zSize;

    private int newX;
    private int newZ;

    private int originX;
    private int originZ;

    private float[,] heights;
    private float[,] interHeights;

    private float minHeight = 0;
    private float maxHeight = 10000;

    public int res = 64000;
    private float scaleX;
    private float scaleZ;

    IEnumerator Start()
    {
        Debug.Log("Start mesh generation loop");
        SetBounds();
        GetHeightData();

        InterpolateHeights();

        GenerateTerrain(maxHeight, minHeight);

        yield return null;
    }

    private void GetHeightData()
    {

        Debug.Log("Get heights");

        heights = new float[zSize, xSize];

        using (var file = System.IO.File.OpenRead(HeightMap))
        using (var reader = new System.IO.BinaryReader(file))
        {
            for (int z = 0; z < 20171; z++)
            {
                for (int x = 0; x < 32107; x++)
                {
                    float v = (float)reader.ReadUInt16();
                    if (x >= originX && x < originX + xSize && z > originZ && z <= originZ + zSize)
                    {
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

        for (int i = 0; i < xSize; i++)
        {
            for (int j = 0; j < zSize; j++)
            {
                heights[j, i] = heights[j, i] / maxHeight;

            }
        }

        Debug.Log("Get heights done");
    }

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

        Debug.Log("size terrain: " + xSize + " " + zSize);

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

        newX = newZ = size;

        Debug.Log("Closest power done");
    }

    void GenerateTerrain(float maxHeight, float minHeight)
    {
        Debug.Log("Starting terrain generation");

        TerrainData test = new TerrainData();
        test.heightmapResolution = newX + 1;
        test.baseMapResolution = 1024;
        test.SetDetailResolution(1024, 32);
        test.size = new Vector3(newX, maxHeight - minHeight, newZ);
        GameObject terrain = new GameObject("Terrain");

        TerrainCollider tercol = terrain.AddComponent<TerrainCollider>();
        Terrain huh = terrain.AddComponent<Terrain>();

        tercol.terrainData = test;
        huh.terrainData = test;
        huh.materialTemplate = terrainMaterial;
        test.SetHeights(0, 0, interHeights);

        test.size = new Vector3(xSize * scaleX, maxHeight - minHeight, zSize * scaleZ);

        huh.transform.position = new Vector3(huh.transform.position.x,
                                             huh.transform.position.y,
                                             huh.transform.position.z - (zSize * scaleZ));

        huh.transform.position = new Vector3(huh.transform.position.x - (int)((MercatorProjection.lonToX(map.bounds.MaxLon) - MercatorProjection.lonToX(map.bounds.MinLon)) / 2),
                                             huh.transform.position.y,
                                             huh.transform.position.z + (int)((MercatorProjection.latToY(map.bounds.MaxLat) - MercatorProjection.latToY(map.bounds.MinLat)) / 2));

        Debug.Log("Terrain generation complete");
    }

    private void InterpolateHeights()
    {
        Debug.Log("Starting to interpolate heights");

        interHeights = new float[newZ + 1, newX + 1];

        float stepX = xSize;
        float stepZ = zSize;

        stepX /= (newX + 1);
        stepZ /= (newZ + 1);

        for (int i = 0; i <= newX; i++)
        {
            for (int j = 0; j <= newZ; j++)
            {
                //we willen de hoekpunten bewaren maar alles ertussen eigenlijk interpoleren
                //we gaan voor elke index in onze interHeights interpoleren tussen de vier overeenkomstige hoogtepunten 
                //uit de oorspronkelijke heights array.

                if (i == newX && j == newZ)
                {
                    interHeights[j, i] = heights[(int) (j * stepZ), (int) (i * stepX)];
                }
                else if (j == newZ)
                {
                    float a = heights[(int)(j * stepZ), (int)(i * stepX)];
                    float b = heights[(int)(j * stepZ), (int)((i + 1) * stepX)];
                    interHeights[j, i] = Mathf.Lerp(a, b, (i * stepX) - a);
                }
                else if (i == newX)
                {
                    float a = heights[(int)(j * stepZ), (int)(i * stepX)];
                    float c = heights[(int)((j + 1) * stepZ), (int)(i * stepX)];

                    interHeights[j, i] = Mathf.Lerp(a, c, c - (j * stepZ));
                }
                else
                {

                    float a = heights[(int)(j * stepZ), (int)(i * stepX)];
                    float b = heights[(int)(j * stepZ), (int)((i + 1) * stepX)];
                    float c = heights[(int)((j + 1) * stepZ), (int)(i * stepX)];
                    float d = heights[(int)((j + 1) * stepZ), (int)((i + 1) * stepX)];

                    float abu = Mathf.Lerp(a, b, (i * stepX) - a);
                    float cdv = Mathf.Lerp(c, d, (i * stepX) - c);

                    interHeights[j, i] = Mathf.Lerp(abu, cdv, cdv - (j * stepZ));
                }
            }
        }

        Debug.Log("Interpolating hieghts done");
    }

    //Dit is de functie die MapReader gebruikt om aan een OsmNode een hoogte toe te kennen.
    public Vector3 FindHeight(OsmNode p)
    {
        /*Vector3 v = p - map.bounds.Centre;

        if (v.x < vertices[0, 0].x || v.z < vertices[0, 0].z || v.x > vertices[(s_xSize + 1) * (s_zSize + 1) - 1, quads * quads - 1].x || v.z >= vertices[(s_xSize + 1) * (s_zSize + 1) - 1, quads * quads - 1].z)
        {
            v.z = 0;
            return v;
        }
        else
        {
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
            Vector3 b = vertices[j + 1, counter];
            Vector3 c = vertices[j + s_xSize + 1, counter];
            Vector3 d = vertices[j + s_xSize + 2, counter];
            Vector3 abu = Vector3.Lerp(a, b, (v.x - a.x) / (b.x - a.x));
            Vector3 cdv = Vector3.Lerp(c, d, (v.x - c.x) / (d.x - c.x));
            return Vector3.Lerp(abu, cdv, (cdv.z - v.z) / (cdv.z - abu.z));
        }*/
        return p;
    }
}

