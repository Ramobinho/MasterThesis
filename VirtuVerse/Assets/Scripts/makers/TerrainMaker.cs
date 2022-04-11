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

    private GameObject terrain;
    private Terrain huh;

    private int xSize;
    private int zSize;

    private int newSize;

    private int originX;
    private int originZ;

    private float[,] heights;
    private float[,] interHeights;

    private float minHeight = 10000;
    private float maxHeight = 0;

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

        Debug.Log(huh.GetPosition() + " " + (huh.GetPosition() + new Vector3(xSize * scaleX,0,zSize * scaleZ)) + " " + (xSize * scaleX) + " " + (zSize * scaleZ) + " " + (newSize + 1));

        map.HeightsReady();

        yield return null;
    }

    private void GetHeightData()
    {

        Debug.Log("Get heights");

        heights = new float[zSize+1, xSize+1];

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
                heights[j, i] = heights[j, i] / maxHeight;

            }
        }
        Debug.Log("max/min: " + maxHeight + " " + minHeight);
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

        Debug.Log("origin terrain: " + originX + " " + originZ);

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

        TerrainData test = new TerrainData();
        test.heightmapResolution = newSize + 1;
        test.baseMapResolution = 1024;
        test.SetDetailResolution(1024, 32);
        test.size = new Vector3(newSize, maxHeight - minHeight, newSize);
        terrain = new GameObject("Terrain");

        TerrainCollider tercol = terrain.AddComponent<TerrainCollider>();
        huh = terrain.AddComponent<Terrain>();

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

        interHeights = new float[newSize + 1, newSize + 1];

        float stepX = xSize + 1;
        float stepZ = zSize + 1;

        stepX /= (newSize + 1);
        stepZ /= (newSize + 1);

        for (int i = 0; i <= newSize; i++)
        {
            for (int j = 0; j <= newSize; j++)
            {
                //we willen de hoekpunten bewaren maar alles ertussen eigenlijk interpoleren
                //we gaan voor elke index in onze interHeights interpoleren tussen de vier overeenkomstige hoogtepunten 
                //uit de oorspronkelijke heights array.

                if (i == newSize && j == newSize)
                {
                    interHeights[j, i] = heights[(int) (j * stepZ), (int) (i * stepX)];
                }
                else if (j == newSize)
                {
                    float a = heights[(int)(j * stepZ), (int)(i * stepX)];
                    float b = heights[(int)(j * stepZ), (int)((i + 1) * stepX)];
                    interHeights[j, i] = Mathf.Lerp(a, b, (i * stepX) - a);
                }
                else if (i == newSize)
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
        //is zien of via terraindata de heights gelezen kunnen worden.        

        Vector3 v = p - map.bounds.Centre;

        if (v.x < huh.GetPosition().x || v.z < huh.GetPosition().z || v.x > huh.GetPosition().x + (xSize * scaleX) || v.z >= huh.GetPosition().z + (zSize * scaleZ))
        {
            return v;
        }

        Vector3 nv = new Vector3(v.x, huh.SampleHeight(v), v.z);
        return nv;
    }
}

