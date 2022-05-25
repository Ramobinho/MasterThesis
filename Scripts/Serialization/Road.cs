using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;


 public enum Surface {
    unclassified, asphalt, unpaved, paved, ground, fine_gravel, woodchips, concrete, paving_stones, gravel, dirt, grass, sand, wood, stepping_stones, compacted, sett, glass, brick, acrylic, cobblestone
}

public enum RoadType
{
    unclassified, pier, residential, service, track, tertiary_link,footway, path, tertiary, crossing, secondary, primary, cycleway, platform, living_street, pedestrian, road
}
public class Road : OsmWay
{

    private RoadType type;
    private Surface surface;
    private static int counter;
    private int id;
    private int lanes;
    private float width;
    private bool lit;

    public Road(XmlNode node, Reader map):base(node,map)
    {


        id = System.Threading.Interlocked.Increment(ref counter);
        type = RoadType.unclassified;
        surface = Surface.unclassified;
        lanes = 0;
        width = 0f;
        GetTags();
    }



   override public void GetTags()
    {
        XmlNodeList tags = xmldata.SelectNodes("tag");
        foreach (XmlNode tag in tags)
        {
            string key = GetAttribute<string>("k", tag.Attributes);
            if (key == "highway")
            {
                string highwayType = GetAttribute<string>("v", tag.Attributes);
                if (Enum.IsDefined(typeof(RoadType), highwayType)) type = (RoadType)Enum.Parse(typeof(RoadType), highwayType);
                else
                {
                    Debug.Log("ROAD WAS MADE, BUT type IS NOT DEFINED: name: " + highwayType);
                    type = RoadType.unclassified;
                }
            }
            else if (key == "width")
            {
                width = GetAttribute<float>("v", tag.Attributes);
            }
            else if (key == "lanes")
            {
                lanes = GetAttribute<int>("v", tag.Attributes);
            }
            else if (key == "surface")
            {
                string surfaceType = GetAttribute<string> ("v", tag.Attributes);
                if (Enum.IsDefined(typeof(Surface), surfaceType)) surface = (Surface)Enum.Parse(typeof(Surface), surfaceType);
                else surface = Surface.unclassified;
            }
            else if (key == "lit")
            {
                string litValue = GetAttribute<string>("v", tag.Attributes);
                if (litValue != "no") { lit = true; }
                AddLanterns();
            }

            else if (key == "man_made")
            { 
                string highwayType = GetAttribute<string>("v", tag.Attributes);
                if (Enum.IsDefined(typeof(RoadType), highwayType)) type = (RoadType)Enum.Parse(typeof(RoadType), highwayType);
                else
                {
                    type = RoadType.unclassified;
                }
            }
        }
    }


    public string GetTexture()
    {
        if (surface != Surface.unclassified) return Enum.GetName(typeof(Surface), surface);
        else return RoadInfo.getRoadInfo(type).texture;
    }

    public float GetWidth()
    {
        if (width != 0) return width;
        else return RoadInfo.getRoadInfo(type).width;
    }

    public int GetLanes()
    {
        if (lanes != 0) return lanes;
        else return RoadInfo.getRoadInfo(type).lanes;
    }

    public string GetClassification()
    {
        return Enum.GetName(typeof(RoadType), type);
    }

    public int GetId()
    {
        return id;
    }

    public bool IsLit()
    {
        if (!lit) return RoadInfo.getRoadInfo(type).isLit;
        else return true;
    }
    public Vector3 GetCentre()
    {
        Vector3 total = Vector3.zero;

        foreach (ulong n in NodeIDs)
        {
            total += new Vector3(map.nodes[n].X, 0, map.nodes[n].Y);
        }
        return total / NodeIDs.Count;
    }

    public List<ulong> GetnodeIDs() {
        return NodeIDs;
    }

    public int GetHierarchy()
    {
        return RoadInfo.getRoadInfo(type).hierarchy;
    }

    private void AddLanterns()
    {
        /*int distance = 0;
        int spacing = 20; // 20 meter spacing between lanterns
        Debug.Log(NodeIDs.Count());
        for (int i = 1; i < NodeIDs.Count; i++)
        {
            OsmNode p1 = map.nodes[NodeIDs[i - 1]];
            OsmNode p2 = map.nodes[NodeIDs[i]];

            Vector3 s1 = p1 - GetCentre();
            Vector3 s2 = p2 - GetCentre();
            Vector3 diff = (s2 - s1).normalized;
            var cross = Vector3.Cross(diff, Vector3.up) * getWidth() * getLanes();    // *1.1 dus iets breder

            if ((int)Vector3.Distance(s1, s2) > 20) {
                Vector3 lantern1 = s1 + cross;
                Vector3 lantern2 = s2 - cross;

                map.lanterns.Add(new Lantern(s1.x + lantern1.x, s1.y + lantern1.y, s1.z + lantern1.z));
                map.lanterns.Add(new Lantern(s2.x + lantern2.x, s2.y + lantern2.y, s2.z + lantern2.z));
            }
*/

            /*Vector3 diff = (s2 - s1).normalized;
            var cross = Vector3.Cross(diff, Vector3.up) * getWidth() * getLanes()* 1.1f;    // *1.1 dus iets breder

            int pointDist = (int)Vector3.Distance(s1, s2);
            int j = 0;
            for (j = -distance; j < pointDist; j += spacing)
            {
                Vector3 offset = s1 + diff*j*spacing + cross;
                map.lanterns.Add(new Lantern(s1.x + offset.x, s1.y+ offset.y, s1.z + offset.z));
            }
            distance = pointDist - j; // distance from previous*/
        //}
    }
}


