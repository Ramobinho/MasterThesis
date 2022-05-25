using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;

/*public enum BuildingType
{
    unclassified, apartments, dormitory, house, residential, church, garage, commercial, retail, office, shed
}*/

public class OsmBuilding : OsmWay
{
    public ulong ID { get; private set; }


    public BuildingType type;

    private string name;

    private string number;
    private string street;
    private int levels;

    public Material material { get; private set; }


    public OsmBuilding(XmlNode node, Reader map):base(node,map) {

        number = "0";
        street = "";
        material = Resources.Load<Material>("art/building");
        GetTags();
    }



    override public void GetTags()
    {
        XmlNodeList tags = xmldata.SelectNodes("tag");
        foreach (XmlNode tag in tags)
        {
            string key = GetAttribute<string>("k", tag.Attributes);
            if (key == "building:material")
            {
                string materialName = GetAttribute<string>("v", tag.Attributes);
                string path = "art/" + materialName;
                material = Resources.Load<Material>(path);
                if (material == null) {
                    material = Resources.Load<Material>("art/building");
                }
                break;
            }
            else if (key == "building" || key == "building:part")
            {
                string aType = GetAttribute<string>("v", tag.Attributes);
                if (Enum.IsDefined(typeof(BuildingType), aType)) type = (BuildingType)Enum.Parse(typeof(BuildingType), aType);
                else type = BuildingType.unclassified;
            }
            else if (key == "addr:housenumber") number = GetAttribute<string>("v", tag.Attributes);
            else if (key == "addr:street") street = GetAttribute<string>("v", tag.Attributes);
            //else if (key == "building:levels") levels = GetAttribute<int>("v", tag.Attributes);
        }
        name = street + " " + number;
    }

    public Vector3 GetCentre() {
        Vector3 total = Vector3.zero;
        foreach (ulong id in NodeIDs)
        {
            OsmNode n = map.nodes[id];
            total += new Vector3(n.X, 0, n.Y);
        }
        return total / NodeIDs.Count;
    }

    public Material GetMaterial() {
        return material;
    }

    public BuildingType GetClassification() {
        return type;
    }

    public string GetName()
    {
        return name;
    }
}

