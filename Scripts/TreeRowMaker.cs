using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeRowMaker : MonoBehaviour
{
    protected Reader map;
    public GameObject tree;
    int treeSpacing = 10;

    bool TreerowKeyPressed = false;

    void Awake()
    {
        map = GetComponent<Reader>();
    }
    IEnumerator Start()
    {
        while (!map.IsReady || !TreerowKeyPressed)
        {
            yield return null;
        }

        foreach (TreeRow t in map.treeRows)
        {
            for (int i = 1; i < t.GetNodeIDs().Count; i++) {
                OsmNode p1 = map.nodes[t.GetNodeIDs()[i - 1]];
                OsmNode p2 = map.nodes[t.GetNodeIDs()[i]];

                Vector3 s1 = p1;
                Vector3 s2 = p2;

                float pointDistance = Vector3.Distance(s1, s2);
                int amountOfTrees = (int)pointDistance / treeSpacing;

                for (int j = 1; j < amountOfTrees + 1; j++) {

                    float thickness = UnityEngine.Random.Range(0.6f, 1.4f);
                    float toMap = Mathf.InverseLerp(0.6f, 1.4f, thickness);
                    float height = Mathf.Lerp(0.6f, 2f, toMap);
                    Vector3 pos = Vector3.Lerp(s1, s2, (float)(j) / (amountOfTrees + 1)) -map.bounds.Centre;
                    GameObject go = Instantiate(tree, pos, Quaternion.Euler(0, UnityEngine.Random.Range(0.0f, 360.0f), 0f));
                    go.transform.localScale = new Vector3(thickness, height, thickness); 
                    go.name = "treeRowTree"; 
                }
            }
        }
        Debug.Log("done making treerows");
    }

    public void SetTreeRowKeyPressed()
    {
        TreerowKeyPressed = true;
    }


}
