using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

class RoadMaker : InfraStructureBehaviour
{
    public Material roadMaterial;

    public GameObject[] roads { get; private set; }

    public GameObject[] finalGo { get; private set; }

    IEnumerator Start()
    {
        while (!map.IsReady)
        {
            yield return null;
        }

        Debug.Log("Road generation started");

        roads = new GameObject[meshMaker.quads * meshMaker.quads];

        MeshFilter[] cmfs = new MeshFilter[meshMaker.quads * meshMaker.quads];
        MeshRenderer[] cmrs = new MeshRenderer[meshMaker.quads * meshMaker.quads];
        Mesh[] finalMesh = new Mesh[roads.Length];

        for (int i = 0; i < roads.Length; i++)
        {
            roads[i] = new GameObject("roads" + i);
            cmfs[i] = roads[i].AddComponent<MeshFilter>();
            cmrs[i] = roads[i].AddComponent<MeshRenderer>();

            cmrs[i].material = roadMaterial;

            finalMesh[i] = new Mesh();
        }

        CombineInstance[,] combiner = new CombineInstance[map.ways.FindAll((w) => { return w.IsRoad; }).Count, roads.Length];

        int counter = 0;

        foreach(var way in map.ways.FindAll((w) => { return w.IsRoad; }))
        {
            GameObject go = new GameObject(way.ID.ToString());
            Vector3 localOrigin = GetCentre(way);
            go.transform.position = localOrigin - map.bounds.Centre;

            MeshFilter mf = go.AddComponent<MeshFilter>();
            MeshRenderer mr = go.AddComponent<MeshRenderer>();

            mr.material = roadMaterial;

            List<Vector3> vectors = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> indicies = new List<int>();

            for (int i = 1; i < way.NodeIds.Count; i++)
            {
                OsmNode p1 = map.nodes[way.NodeIds[i - 1]];
                OsmNode p2 = map.nodes[way.NodeIds[i]];

                Vector3 s1 = p1 - localOrigin;
                Vector3 s2 = p2 - localOrigin;

                Vector3 diff = (s2 - s1).normalized;
                var cross = Vector3.Cross(diff, Vector3.up) * 2.0f;

                Vector3 v1 = s1 + cross;
                Vector3 v2 = s1 - cross;
                Vector3 v3 = s2 + cross;
                Vector3 v4 = s2 - cross;

                vectors.Add(v1);
                vectors.Add(v2);
                vectors.Add(v3);
                vectors.Add(v4);

                uvs.Add(new Vector2(0, 0));
                uvs.Add(new Vector2(1, 0));
                uvs.Add(new Vector2(0, 1));
                uvs.Add(new Vector2(1, 1));

                normals.Add(Vector3.up);
                normals.Add(Vector3.up);
                normals.Add(Vector3.up);
                normals.Add(Vector3.up);

                int idx1, idx2, idx3, idx4;
                idx4 = vectors.Count - 1;
                idx3 = vectors.Count - 2;
                idx2 = vectors.Count - 3;
                idx1 = vectors.Count - 4;

                //triangles have to go clockwise & counter-clockwise
                //this means the walls are backed together and facing opposite
                //directions, this makes that you can't see through them
                // first triangle
                indicies.Add(idx1);
                indicies.Add(idx3);
                indicies.Add(idx2);

                //second
                indicies.Add(idx3);
                indicies.Add(idx4);
                indicies.Add(idx2);
            }

            mf.mesh.vertices = vectors.ToArray();
            mf.mesh.normals = normals.ToArray();
            mf.mesh.triangles = indicies.ToArray();
            mf.mesh.uv = uvs.ToArray();

            //berekenen to welke quad dit stuk straat behoort

            double rangeX = MercatorProjection.lonToX(map.bounds.MaxLon) - MercatorProjection.lonToX(map.bounds.MinLon);
            double rangeZ = MercatorProjection.latToY(map.bounds.MaxLat) - MercatorProjection.latToY(map.bounds.MinLat);

            double intervalX = rangeX / meshMaker.quads;
            double intervalZ = rangeZ / meshMaker.quads;

            int pos = 0;
            for (int i = 0; i < way.NodeIds.Count; i++)
            {
                Vector3 test = map.nodes[way.NodeIds[i]];
                if (test.x > MercatorProjection.lonToX(map.bounds.MinLon) && test.x < MercatorProjection.lonToX(map.bounds.MaxLon) && test.z > MercatorProjection.latToY(map.bounds.MinLat) && test.z < MercatorProjection.latToY(map.bounds.MaxLat))
                {
                    pos = i;
                    i = way.NodeIds.Count;
                }
            }

            Vector3 p0 = map.nodes[way.NodeIds[pos]];

            int quad = (int)(p0.x - MercatorProjection.lonToX(map.bounds.MinLon)) / (int)intervalX;
            quad += ((int)(p0.z - MercatorProjection.latToY(map.bounds.MinLat)) / (int)intervalZ) * meshMaker.quads;

            /*Debug.Log(MercatorProjection.lonToX(map.bounds.MinLon) + " " + MercatorProjection.lonToX(map.bounds.MaxLon) + " " + MercatorProjection.latToY(map.bounds.MinLat) + " " + MercatorProjection.latToY(map.bounds.MaxLat));
            Debug.Log(p0);
            Debug.Log(quad);*/

            while (quad >= meshMaker.quads * meshMaker.quads) { quad -= meshMaker.quads; }
            while (quad < 0) { quad += 5; }

            combiner[counter, quad].subMeshIndex = 0;
            combiner[counter, quad].mesh = mf.sharedMesh;
            combiner[counter, quad].transform = mf.transform.localToWorldMatrix;

            counter++;

            Destroy(go);

            yield return null;

            }

        for (int i = 0; i < roads.Length; i++)
        {
            finalMesh[i].CombineMeshes(Enumerable.Range(0, combiner.GetLength(0)).Select(x => combiner[x, i]).ToArray());
            cmfs[i].sharedMesh = finalMesh[i];
        }


        Debug.Log("Road generation done");

        Debug.Log("start big combine");
        MeshCombiner();
        Debug.Log("combining done");
    }

    public void MeshCombiner()
    {
        finalGo = new GameObject[meshMaker.quads * meshMaker.quads];

        MeshFilter[] cmfs = new MeshFilter[meshMaker.quads * meshMaker.quads];
        MeshRenderer[] cmrs = new MeshRenderer[meshMaker.quads * meshMaker.quads];

        for (int i = 0; i < finalGo.Length; i++)
        {
            finalGo[i] = new GameObject("Final" + i);
            cmfs[i] = finalGo[i].AddComponent<MeshFilter>();
            cmrs[i] = finalGo[i].AddComponent<MeshRenderer>();
        }

        for (int i = 0; i < finalGo.Length; i++)
        {
            List<CombineInstance> combinations = new List<CombineInstance>();

            CombineInstance c1 = new CombineInstance();
            c1.mesh = meshMaker.mfs[i].sharedMesh;
            c1.subMeshIndex = 0;
            c1.transform = meshMaker.mfs[i].transform.localToWorldMatrix;
            combinations.Add(c1);

            CombineInstance c2 = new CombineInstance();
            c2.mesh = roads[i].GetComponent<MeshFilter>().sharedMesh;
            c2.subMeshIndex = 0;
            c2.transform = roads[i].GetComponent<MeshFilter>().transform.localToWorldMatrix;
            combinations.Add(c2);

            Mesh finalMesh = new Mesh();
            finalMesh.CombineMeshes(combinations.ToArray(), false);
            cmfs[i].sharedMesh = finalMesh;

            Destroy(roads[i]);
            Destroy(meshMaker.gos[i]);
        }
    }
}