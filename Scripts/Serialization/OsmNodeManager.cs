using System.Xml;
using UnityEngine;

public class OsmNodeManager : BaseOsm
{

    public XmlNodeList nodes { get; private set; }
    public Reader map { get; private set; }

    public OsmNodeManager(XmlNodeList nodeList, Reader map)
    {
        nodes = nodeList;
        this.map = map;
        getAllInformation();
    }

    public void getAllInformation()
    {
        foreach (XmlNode node in nodes)
        {
            OsmNode n = new OsmNode(node, map);
            if(n.IsObject()) map.objects.Add(n);    // dus als de tags er op wijzen dat het een object is uit de lijst van gedefinieerde objecten, toevoegen
            map.nodes[n.GetId()] = n;   // lijst van alle nodes!
        }
    }
}
