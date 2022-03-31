
using System.Collections.Generic;
using System.Xml;

class OsmWay : BaseOsm
{
    public ulong ID { get; private set; }

    public bool Visable { get; private set; }

    public List<ulong> NodeIds { get; private set; }

    public bool IsBoundary { get; private set; }

    public bool IsBuilding { get; private set; }

    public bool IsRoad { get; private set; }

    public float Height { get; private set; }

    public OsmWay(XmlNode node)
    {
        NodeIds = new List<ulong>();
        Height = 3.0f;

        ID = GetAttribute<ulong>("id", node.Attributes);
        Visable = GetAttribute<bool>("visible", node.Attributes);

        XmlNodeList nds = node.SelectNodes("nd");
        foreach(XmlNode n in nds)
        {
            ulong refNo = GetAttribute<ulong>("ref", n.Attributes);
            NodeIds.Add(refNo);
        }

        if(NodeIds.Count > 1)
        {
            IsBoundary = NodeIds[0] == NodeIds[NodeIds.Count - 1];
        }

        XmlNodeList tags = node.SelectNodes("tag");
        foreach(XmlNode t in tags)
        {
            string key = GetAttribute<string>("k", t.Attributes);
            if(key == "building:levels")
            {
                Height = 3.0f * GetAttribute<float>("v", t.Attributes);

            }
            else if(key == "height")
            {
                Height = GetAttribute<float>("v", t.Attributes);
            }
            else if(key == "building")
            {
                IsBuilding = true;
            }
            else if(key == "highway")
            {
                IsRoad = true;               
            }
        }
    }
}