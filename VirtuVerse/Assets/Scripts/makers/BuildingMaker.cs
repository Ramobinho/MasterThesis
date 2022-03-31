using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class BuildingMaker : InfraStructureBehaviour
{

    public Material building;

    IEnumerator Start()
    {
        while (!map.IsReady)
        {
            yield return null;
        }

        var countVertex = 0;
        var countPoly = 0;

        // find all nodes that form boundaries (probably buildings)
        foreach (var way in map.ways.FindAll((w) => { return w.IsBuilding && w.NodeIds.Count > 1; }))
        {
            GameObject go = new GameObject(way.ID.ToString());
            Vector3 localOrigin = GetCentre(way);
            go.transform.position = localOrigin - map.bounds.Centre;

            MeshFilter mf = go.AddComponent<MeshFilter>();
            MeshRenderer mr = go.AddComponent<MeshRenderer>();

            mr.material = building;

            List<Vector3> vectors = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<int> indicies = new List<int>();


            //initial vertex & index
            OsmNode p1 = map.nodes[way.NodeIds[0]];

            Vector3 v1 = p1 - localOrigin;
            Vector3 v2 = v1 + new Vector3(0, way.Height, 0);

            vectors.Add(v1);
            vectors.Add(v2);

            normals.Add(-Vector3.forward);
            normals.Add(-Vector3.forward);

            int idx1, idx2;
            idx2 = vectors.Count - 1;
            idx1 = vectors.Count - 2;

            for (int i = 1; i < way.NodeIds.Count; i++)
            {
                OsmNode p2 = map.nodes[way.NodeIds[i]];

                Vector3 v3 = p1 - localOrigin;
                Vector3 v4 = v3 + new Vector3(0, way.Height, 0);

                vectors.Add(v3);
                vectors.Add(v4);

                normals.Add(-Vector3.forward);
                normals.Add(-Vector3.forward);

                int idx3, idx4;
                idx4 = vectors.Count - 1;
                idx3 = vectors.Count - 2;

                //triangles have to go clockwise, this allows them to only be visible when
                //looking at them from outside. skips generation of inside walls which would 
                //be useless overhead.

                //anti-clockwise also necessary, osm sequence isn't consequent and as such only
                //taking one sequence into account will differ from inside/outside walls depending
                //on the building

                //first
                indicies.Add(idx1);
                indicies.Add(idx2);
                indicies.Add(idx3);

                //second
                indicies.Add(idx2);
                indicies.Add(idx4);
                indicies.Add(idx3);

                //third
                indicies.Add(idx1);
                indicies.Add(idx3);
                indicies.Add(idx2);

                //fourth
                indicies.Add(idx2);
                indicies.Add(idx3);
                indicies.Add(idx4);

                p1 = p2;

                v1 = v3;
                v2 = v4;

                idx1 = idx3;
                idx2 = idx4;
            }

            mf.mesh.vertices = vectors.ToArray();
            mf.mesh.normals = normals.ToArray();
            mf.mesh.triangles = indicies.ToArray();

            countVertex += vectors.Count;
            countPoly += indicies.Count;

            yield return null;
        }

        /*Debug.Log(countVertex);
        Debug.Log(countPoly / 3);*/
    }
}