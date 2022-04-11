using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;

class MapReader : MonoBehaviour
{
    public Dictionary<ulong, OsmNode> nodes;
    public List<OsmWay> ways;
    public OsmBounds bounds;
    public TerrainData terrainData1;
    public float[,] heights;
    public MeshMaker meshMaker;
    public TerrainMaker terrainMaker;

    public string resourceFile;

    public bool IsReady { get; private set; }

    void Start()
    {
        nodes = new Dictionary<ulong, OsmNode>();
        ways = new List<OsmWay>();   
        meshMaker = GetComponent<MeshMaker>();
        terrainMaker = GetComponent<TerrainMaker>();

        var txtAsset = Resources.Load<TextAsset>(resourceFile);

        XmlDocument doc = new XmlDocument();
        doc.LoadXml(txtAsset.text);

        SetBounds(doc.SelectSingleNode("/osm/bounds"));
    }

    public void HeightsReady()
    {
        var txtAsset = Resources.Load<TextAsset>(resourceFile);

        XmlDocument doc = new XmlDocument();
        doc.LoadXml(txtAsset.text);

        GetNodes(doc.SelectNodes("/osm/node"));
        GetWays(doc.SelectNodes("/osm/way"));

        IsReady = true;
    }

    void Update()
    {
        if (IsReady)
        {
            MeshMaker mesh = GetComponent<MeshMaker>();
            foreach (OsmWay w in ways)
            {
                if (w.Visable)
                {
                    Color c = Color.cyan;               //cyan building
                    if (!w.IsBoundary) c = Color.red;   //red road

                    for (int i = 1; i < w.NodeIds.Count; i++)
                    {
                        OsmNode p1 = nodes[w.NodeIds[i - 1]];
                        OsmNode p2 = nodes[w.NodeIds[i]];

                        Debug.DrawLine(p1 - bounds.Centre, p2 - bounds.Centre, c);
                    }
                }
            }
        }

    }

    void GetWays(XmlNodeList xmlNodeList)
    {
        foreach (XmlNode node in xmlNodeList)
        {
            OsmWay way = new OsmWay(node);
            ways.Add(way);
        }
    }

    void GetNodes(XmlNodeList xmlNodeList)
    {
        foreach (XmlNode n in xmlNodeList)
        {
            OsmNode node = new OsmNode(n);
            node.Z = terrainMaker.FindHeight(node).y;
            nodes[node.ID] = node;
        }
    }

    void SetBounds(XmlNode xmlNode)
    {
        bounds = new OsmBounds(xmlNode);
    }
}
