using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;

// checken of barrier is ook 
/*public enum Surface
{
    unclassified, asphalt, unpaved, paved, ground, concrete, paving_stones, gravel, dirt, grass, sand, wood, stepping_stones, compacted, sett
}*/
public class OsmRelation : BaseOsm
{
    public List<ulong> OuterNodeIDs;
    public List<ulong> InnerNodeIDs;
    public Reader map { get; private set; }
    private XmlNode node;
    private Surface surface;
    bool AllNodesPresent;

    public OsmRelation(XmlNode node, Reader map) {
        OuterNodeIDs = new List<ulong>();
        InnerNodeIDs = new List<ulong>();
        this.map = map;
        this.node = node;
        surface = Surface.sett;
        AllNodesPresent = true;
        GetNodes();
        GetTags();

    }

    private void GetNodes() {
        XmlNodeList nds = node.SelectNodes("member");
        foreach (XmlNode n in nds)
        {
            string type = GetAttribute<string>("type", n.Attributes);
            if (type == "way")
            {
                ulong reference = GetAttribute<ulong>("ref", n.Attributes);
                string role = GetAttribute<string>("role", n.Attributes);
                if (!map.ways.ContainsKey(reference)) AllNodesPresent = false;
                else if (role == "outer") OuterNodeIDs.AddRange(map.ways[reference].GetNodeIDs());
                else if (role == "inner") InnerNodeIDs.AddRange(map.ways[reference].GetNodeIDs());
            }
            else if (type == "node")
            {
                ulong reference = GetAttribute<ulong>("ref", n.Attributes);
                string role = GetAttribute<string>("role", n.Attributes);
                if (!map.ways.ContainsKey(reference)) AllNodesPresent = false;
                else if (role == "outer") OuterNodeIDs.Add(map.nodes[reference].GetId());
                else if (role == "inner") InnerNodeIDs.Add(map.nodes[reference].GetId());
            }
        }
    }
    public void GetTags()
    {
        XmlNodeList tags = node.SelectNodes("tag");
        foreach (XmlNode tag in tags)
        {
            string key = GetAttribute<string>("k", tag.Attributes);
            if (key == "parking")
            {
                if(surface == Surface.sett) surface = Surface.concrete;
            }
            else if (key == "surface")
            {
                string surf = GetAttribute<string>("v", tag.Attributes);
                if (Enum.IsDefined(typeof(Surface), surf)) surface = (Surface)Enum.Parse(typeof(Surface), surf);
                else surface = Surface.sett;
            }
            else if (key == "barrier")
            {
                // TODO: eerst enum maken van barriermaterial
            }
        }
    }

    public Vector3 GetCentre()
    {
        Vector3 total = Vector3.zero;

        foreach (ulong id in OuterNodeIDs)
        {
            OsmNode n = map.nodes[id];
            total += new Vector3(n.X, 0, n.Y);
        }
        return total / OuterNodeIDs.Count;
    }

    public List<ulong> GetOuterBoundaries() {
        return OuterNodeIDs;
    }
    public List<ulong> GetInnerBoundaries()
    {
        return InnerNodeIDs;
    }
     public string GetTexture()
    {
        return Enum.GetName(typeof(Surface), surface); ;
    }

    public bool AreAllNodesPresent() {
        return AllNodesPresent;
    }




}
