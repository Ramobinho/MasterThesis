using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrierMaker : InfrastructureBehaviour
{
    [HideInInspector]
    public float barrierWidth;
    [HideInInspector]
    public float barrierHeight;
    [HideInInspector]
    bool barrierKeyPressed = false;

    IEnumerator Start()
    {

        while (!map.IsReady || !barrierKeyPressed)
        {
            yield return null;
        }

        Debug.Log("barriers started");
        // Iterate through the roads and build each one
        foreach (var b in map.barriers)
        {
            barrierWidth = b.GetWidth();
            barrierHeight = b.GetHeight();

            string name = b.GetClassification() + b.GetId();
            // Create an instance of the object and place it in the centre of its points
            GameObject go = new GameObject(name);
            Vector3 localOrigin = b.GetCentre();
            go.transform.position = localOrigin - map.bounds.Centre;

            MeshFilter mf = go.AddComponent<MeshFilter>();
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.material = b.GetMaterial();
            // Create the collections for the object's vertices, indices, UVs etc.
            List<Vector3> vectors = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> indices = new List<int>();

            float distance = 0;
            float totalHeight = 2 * barrierHeight + barrierWidth;
            // calculate total distance before
            for (int i = 1; i < b.NodeIDs.Count; i++)
            {
                OsmNode p1 = map.nodes[b.NodeIDs[i - 1]];
                OsmNode p2 = map.nodes[b.NodeIDs[i]];
                Vector3 s1 = p1 - localOrigin;
                Vector3 s2 = p2 - localOrigin;
                distance += Vector3.Distance(s1, s2);
            }

            float partialDistance = 0;
            for (int i = 1; i < b.NodeIDs.Count; i++)
            {
                
                OsmNode p1 = map.nodes[b.NodeIDs[i - 1]];
                OsmNode p2 = map.nodes[b.NodeIDs[i]];

                Vector3 s1 = p1 - localOrigin;
                Vector3 s2 = p2 - localOrigin;


                Vector3 diff = (s2 - s1).normalized;
                var cross = Vector3.Cross(diff, Vector3.up) * barrierWidth;

                // Create points that represent the width of the road
                Vector3 v1 = s1 + cross;
                Vector3 v2 = s1 - cross;
                Vector3 v3 = s2 + cross;
                Vector3 v4 = s2 - cross;

                Vector3 h1 = v1 + new Vector3(0, barrierHeight, 0);
                Vector3 h2 = v2 + new Vector3(0, barrierHeight, 0);
                Vector3 h3 = v3 + new Vector3(0, barrierHeight, 0);
                Vector3 h4 = v4 + new Vector3(0, barrierHeight, 0);


                vectors.Add(v1);
                vectors.Add(v2);
                vectors.Add(v3);
                vectors.Add(v4);
                vectors.Add(h1);
                vectors.Add(h2);
                vectors.Add(h3);
                vectors.Add(h4);

                float BeforeDistance = partialDistance;
                partialDistance += Vector3.Distance(s1, s2);
                uvs.Add(new Vector2(BeforeDistance / distance, 0));
                uvs.Add(new Vector2(BeforeDistance / distance, totalHeight));
                uvs.Add(new Vector2(partialDistance / distance, 0));
                uvs.Add(new Vector2(partialDistance / distance, totalHeight));
                uvs.Add(new Vector2(BeforeDistance / distance, barrierHeight));
                uvs.Add(new Vector2(BeforeDistance / distance, barrierHeight+barrierWidth));
                uvs.Add(new Vector2(partialDistance / distance, barrierHeight));
                uvs.Add(new Vector2(partialDistance / distance, barrierHeight + barrierWidth));


                normals.Add(Vector3.up);
                normals.Add(Vector3.up);
                normals.Add(Vector3.up);
                normals.Add(Vector3.up);
                normals.Add(Vector3.up);
                normals.Add(Vector3.up);
                normals.Add(Vector3.up);
                normals.Add(Vector3.up);

                int idv1, idv2, idv3, idv4, idh1, idh2, idh3, idh4;

                idh4 = vectors.Count - 1;
                idh3 = vectors.Count - 2;
                idh2 = vectors.Count - 3;
                idh1 = vectors.Count - 4;
                idv4 = vectors.Count - 5;
                idv3 = vectors.Count - 6;
                idv2 = vectors.Count - 7;
                idv1 = vectors.Count - 8;



                // upperplane counterclockwise triangle1
                indices.Add(idh1);
                indices.Add(idh3);
                indices.Add(idh2);

                // upperplane  counterclockwise triangle2
                indices.Add(idh3);
                indices.Add(idh4);
                indices.Add(idh2);

                // side1 triangle 1
                indices.Add(idv1);
                indices.Add(idv3);
                indices.Add(idh3);
                // side1 triangle 1
                indices.Add(idh3);
                indices.Add(idv3);
                indices.Add(idv1);

                // side1 triangle 2
                indices.Add(idh3);
                indices.Add(idh1);
                indices.Add(idv1);
                // side1 triangle 2
                indices.Add(idv1);
                indices.Add(idh1);
                indices.Add(idh3);

                // side2 triangle 1
                indices.Add(idv2);
                indices.Add(idv4);
                indices.Add(idh4);
                // side2 triangle 1
                indices.Add(idh4);
                indices.Add(idv4);
                indices.Add(idv2);

                // side2 triangle 2
                indices.Add(idh4);
                indices.Add(idv2);
                indices.Add(idh2);
                // side2 triangle 2
                indices.Add(idv2);
                indices.Add(idh2);
                indices.Add(idh4);
            }

            // Apply the data to the mesh
            mf.mesh.vertices = vectors.ToArray();
            mf.mesh.normals = normals.ToArray();
            mf.mesh.triangles = indices.ToArray();
            mf.mesh.uv = uvs.ToArray();
            mr.material.mainTextureScale = new Vector2(distance/2, (totalHeight) /5.2f);
           
        }
        Debug.Log("done making barriers");
    }

    public void SetBarrierKeyPressed() {
        barrierKeyPressed = true;
        Debug.Log("set barrier key pressed is true");
    }
}
