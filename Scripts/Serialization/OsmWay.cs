using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;


public class OsmWay : BaseOsm
{
    public ulong ID { get; private set; }
    public List<ulong> NodeIDs { get; private set; }
    public XmlNode xmldata { get; private set; }
    public Reader map { get; private set; }

    public OsmWay(XmlNode node, Reader map)
    {
        ID = GetAttribute<ulong>("id", node.Attributes);
        xmldata = node;
        this.map = map;
        NodeIDs = new List<ulong>();
        GetPoints();
    }

    public void GetPoints()
    {
        XmlNodeList nds = xmldata.SelectNodes("nd");
        foreach (XmlNode n in nds)
        {
            ulong refNo = GetAttribute<ulong>("ref", n.Attributes);
            NodeIDs.Add(refNo);
        }
    }

    public virtual void GetTags() { 
    }

    public ulong GetId() {
        return ID;
    }

    public List<ulong> GetNodeIDs() {
        return NodeIDs;
    }
}

