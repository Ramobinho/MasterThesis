using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;


public enum LandCover
{
    unclassified, greenery, grass, flowers, sand, gravel, scrub,soil, woodchips, wood, fine_gravel
} 


public enum AreaType
{
    forest, unclassified, trees, greenery, pier, wood, marina, woodchips, bare_rock, industrial, shrubbery, garden, park, pitch, outdoor_seating, playground, fitness_station, beach_resort, dog_park, golf_course, nature_reserve, swimming_pool, swimming_area, track, parking, bicycle_parking, parking_space, car_sharing, grave_yard, brownfield,railway,recreation_ground,urban_green,flowerbed,farmland,farmyard,grass,greenfield,meadow,cemetery,village_green,vineyard,orchard,landfill,plant_nursery,allotments, basin, water, scrub, sand,beach,land,square, grassland, fountain
}
public class Area : OsmWay
 {

    private static int counter;
    private int id;
    public AreaType type;
    private Surface surface;
    private LandCover landcover;
    public int SurfaceArea;

    public Area(XmlNode node, Reader map) : base(node, map)
    {

        id = System.Threading.Interlocked.Increment(ref counter);
        type = AreaType.unclassified;
        landcover = LandCover.unclassified;
        GetTags();
        GetSurfaceArea();
    }


    public override void GetTags()
    {
        XmlNodeList tags = xmldata.SelectNodes("tag");
        foreach (XmlNode tag in tags)
        {
            string key = GetAttribute<string>("k", tag.Attributes);
            if (key == "leisure" || key == "amenity" || key == "landuse" || key == "natural" || key == "place")
            {
                string aType = GetAttribute<string>("v", tag.Attributes);
                if (Enum.IsDefined(typeof(AreaType), aType)) type = (AreaType)Enum.Parse(typeof(AreaType), aType);
                else
                {
                    Debug.Log("AREA WAS MADE, BUT NAME IS NOT DEFINED: name: " + aType);
                    type = AreaType.unclassified;
                }
            }
            if (key == "landcover")
            {
                string landCover = GetAttribute<string>("v", tag.Attributes);
                if (Enum.IsDefined(typeof(LandCover), landCover)) landcover = (LandCover)Enum.Parse(typeof(LandCover), landCover);
                else
                {
                    if (landCover == "scrub;trees" || landCover == "trees") type = AreaType.trees;
                    else landcover = LandCover.unclassified;
                }
            }
            else if (key == "surface")
            {
                string surfaceType = GetAttribute<string>("v", tag.Attributes);
                if (Enum.IsDefined(typeof(Surface), surfaceType)) surface = (Surface)Enum.Parse(typeof(Surface), surfaceType);
                else
                {
                    Debug.Log("AREA WAS MADE, BUT NAME IS NOT DEFINED: name: " + surfaceType);
                    surface = Surface.unclassified;
                }
            }
        }
    }

    public virtual string GetTexture() {
        if (surface != Surface.unclassified) return Enum.GetName(typeof(Surface), surface);
        else if (landcover != LandCover.unclassified) return Enum.GetName(typeof(LandCover), landcover);
        else return AreaInfo.getAreaInfo(type).texture;
    }


    virtual public string GetClassification()
    {
        return Enum.GetName(typeof(AreaType), type);
    }

    public int GetId()
    {
        return id;
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


    public float GetSurfaceArea()
    {

        float minx = 1000000;
        float maxx = 0;

        float miny = 1000000;
        float maxy = 0;

        foreach (ulong i in NodeIDs)
        {
            float x = map.nodes[i].X;
            float y = map.nodes[i].Y;
            if (x < minx) minx = x;
            else if (x > maxx) maxx = x;

            if (y < miny) miny = x;
            else if (y > maxy) maxy = y;
        }

        minx -= map.bounds.Centre.x;
        maxx -= map.bounds.Centre.x;
        miny -= map.bounds.Centre.y;
        maxy -= map.bounds.Centre.y;

        return (maxx - minx) * (maxy - miny);
    }
}

