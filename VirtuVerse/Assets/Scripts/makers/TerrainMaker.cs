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

    private int originX;
    private int originZ;

    private double upleftLat = 50.912744;
    private double upleftLon = 4.538111;

    private double downleftLat = 50.7329611;
    private double downleftLon = 4.53746111111;

    private double uprightLat = 50.9112028;
    private double uprightLon = 4.9930972222225;

    private double downrightLat = 50.731422;
    private double downrightLon = 4.990711;

    private float[,] heights;

    private int quads;

    public int res = 64000;
    private float scaleX;
    private float scaleZ;

    IEnumerator Start()
    {
        Debug.Log("Start mesh generation loop");
        GetHeightData();

        yield return null;
    }

    private void GetHeightData()
    {
        SetBounds();

        Debug.Log("Get heights");

        heights = new float[zSize + 1, xSize + 1];
        float maxHeight = 0;
        float minHeight = 10000;

        using (var file = System.IO.File.OpenRead(HeightMap))
        using (var reader = new System.IO.BinaryReader(file))
        {
            for (int z = 0; z < 20000; z++)
            {
                for (int x = 0; x < 32000; x++)
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

        Debug.Log("Get heights done");

        TerrainData test = new TerrainData();
        test.heightmapResolution = xSize + 1;
        test.baseMapResolution = 1024;
        test.SetDetailResolution(1024, 16);
        test.size = new Vector3(xSize, maxHeight - minHeight, zSize);
        GameObject terrain = new GameObject("terrain");

        TerrainCollider tercol = terrain.AddComponent<TerrainCollider>();
        Terrain huh = terrain.AddComponent<Terrain>();

        tercol.terrainData = test;
        huh.terrainData = test;
        huh.materialTemplate = terrainMaterial;
        test.SetHeights(0, 0, heights);

        test.size = new Vector3(xSize * scaleX, maxHeight - minHeight, zSize * scaleZ);

        huh.transform.position = new Vector3(huh.transform.position.x,
                                             huh.transform.position.y,
                                             huh.transform.position.z - (zSize * scaleZ));

        huh.transform.position = new Vector3(huh.transform.position.x - (int)((MercatorProjection.lonToX(map.bounds.MaxLon) - MercatorProjection.lonToX(map.bounds.MinLon)) / 2),
                                             huh.transform.position.y,
                                             huh.transform.position.z + (int)((MercatorProjection.latToY(map.bounds.MaxLat) - MercatorProjection.latToY(map.bounds.MinLat)) / 2));

        /*splitTerrain(huh);

        Destroy(test);*/
    }

    private void SetBounds()
    {
        Debug.Log("Set Bounds");

        /*double upleftX = MercatorProjection.lonToX(upleftLon);
        double upleftZ = MercatorProjection.latToY(upleftLat);
        double downleftX = MercatorProjection.lonToX(downleftLon);
        double downleftZ = MercatorProjection.latToY(downleftLat);
        double uprightX = MercatorProjection.lonToX(uprightLon);
        double uprightZ = MercatorProjection.latToY(uprightLat);
        double downrightX = MercatorProjection.lonToX(downrightLon);
        double downrightZ = MercatorProjection.latToY(downrightLat);

        double minX = (upleftX + downleftX) / 2;
        double maxX = (uprightX + downrightX) / 2;
        double rangeX = maxX - minX;

        double minZ = (downrightZ + downleftZ) / 2;
        double maxZ = (uprightZ + upleftZ) / 2;
        double rangeZ = maxZ - minZ;*/
        double upleftX = 505321.82;
        double upleftZ = 6605777.91;
        double downleftX = 505249.05;
        double downleftZ = 6574096.12;
        double uprightX = 555972.12;
        double uprightZ = 6605505.13;
        double downrightX = 555706.12;
        double downrightZ = 6573825.40;
        double rangeX = uprightX - upleftX;

        double rangeZ = upleftZ - downleftZ;
        double minX = upleftX;
        double maxZ = upleftZ;

        scaleX = (float)rangeX / 32000;
        scaleZ = (float)rangeZ / 20000;

        originX = (int)((MercatorProjection.lonToX(map.bounds.MinLon) - minX) / scaleX);
        originZ = (int)((maxZ - MercatorProjection.latToY(map.bounds.MaxLat)) / scaleZ);

        Debug.Log(originX + " " + originZ);

        originX = 0;
        originZ = 0;

        Debug.Log(originX + " " + originZ);

        xSize = (int)((MercatorProjection.lonToX(map.bounds.MaxLon) - MercatorProjection.lonToX(map.bounds.MinLon)) / scaleX);
        zSize = (int)((MercatorProjection.latToY(map.bounds.MaxLat) - MercatorProjection.latToY(map.bounds.MinLat)) / scaleZ);

        xSize = 3200;
        zSize = 2000;

        Debug.Log(xSize + " " + zSize);

        closestPower();

        Debug.Log("Set Bounds done");

    }

    private void closestPower()
    {
        Debug.Log("Closest power");

        int size = 1;
        int thres = xSize > zSize ? xSize : zSize;
        while(size < thres)
        {
            size *= 2;
        }

        xSize = zSize = size;

        Debug.Log("Closest power done");
    }

    private void splitTerrain(Terrain huh)
    {
        Debug.Log("Splitting terrain");

        TerrainData td = new TerrainData();
        GameObject tgo = Terrain.CreateTerrainGameObject(td);

        Terrain genTer = tgo.GetComponent(typeof(Terrain)) as Terrain;
        genTer.terrainData = td;

        genTer.terrainData.splatPrototypes = huh.terrainData.splatPrototypes;
        genTer.terrainData.detailPrototypes = huh.terrainData.detailPrototypes;
        genTer.terrainData.treePrototypes = huh.terrainData.treePrototypes;

        genTer.basemapDistance = huh.basemapDistance;
        genTer.castShadows = huh.castShadows;
        genTer.detailObjectDensity = huh.detailObjectDensity;
        genTer.detailObjectDistance = huh.detailObjectDistance;
        genTer.heightmapMaximumLOD = huh.heightmapMaximumLOD;
        genTer.heightmapPixelError = huh.heightmapPixelError;
        genTer.treeBillboardDistance = huh.treeBillboardDistance;
        genTer.treeCrossFadeLength = huh.treeCrossFadeLength;
        genTer.treeDistance = huh.treeDistance;
        genTer.treeMaximumFullLODCount = huh.treeMaximumFullLODCount;

        Vector3 parentPosition = huh.GetPosition();

        float spaceShiftX = huh.terrainData.size.z / 2;
        float spaceShiftZ = huh.terrainData.size.x / 2;

        float xWShift = (1 % 2) * spaceShiftX;
        float zWShift = (1 / 2) * spaceShiftZ;

        tgo.transform.position = new Vector3(tgo.transform.position.x + zWShift,
                                             tgo.transform.position.y,
                                             tgo.transform.position.z + xWShift);

        tgo.transform.position = new Vector3(tgo.transform.position.x + parentPosition.x,
                                             tgo.transform.position.y + parentPosition.y,
                                             tgo.transform.position.z + parentPosition.z);

        td.heightmapResolution = huh.terrainData.heightmapResolution / 2;

        td.size = new Vector3(huh.terrainData.size.x / 2,
                              huh.terrainData.size.y,
                              huh.terrainData.size.z / 2);

        float[,] parentHeight = huh.terrainData.GetHeights(0, 0, huh.terrainData.heightmapResolution, huh.terrainData.heightmapResolution);

        float[,] peaaceHeight = new float[huh.terrainData.heightmapResolution / 2 + 1,
                                          huh.terrainData.heightmapResolution / 2 + 1];

        int heightShift = huh.terrainData.heightmapResolution / 2;

        int startX = 0;
        int startY = 0;

        int endX = huh.terrainData.heightmapResolution / 2 + 1;
        int endY = huh.terrainData.heightmapResolution / 2 + 1;

        for (int x = startX; x < endX; x++)
        {
            for (int y = startY; y < endY; y++)
            {
                float ph = parentHeight[x + heightShift, y];

                peaaceHeight[x, y] = ph;
            }
        }

        genTer.terrainData.SetHeights(0, 0, peaaceHeight);

        Debug.Log("Splitting terrain done");
    }
}

