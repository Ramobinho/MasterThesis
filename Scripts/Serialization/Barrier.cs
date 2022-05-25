using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;
public enum BarrierType { 
    wall, fence, kerb, hedge, retaining_wall
}

public enum FenceType
{
    standard, concrete, glass,  railing
}

public class Barrier : OsmWay
{
    public BarrierType barrierType;
    public FenceType fencetype;
    public Surface surfaceType;
    private static int counter;
    private int id;
    private float height;
    private float width;

    public Barrier(XmlNode node, Reader map): base(node,map) {


        id = System.Threading.Interlocked.Increment(ref counter);
        barrierType = BarrierType.wall;
        fencetype = FenceType.standard;
        width = 0;
        height = 0;
        GetTags();
    }

    override public void GetTags()
    {
        XmlNodeList tags = xmldata.SelectNodes("tag");
        foreach (XmlNode tag in tags)
        {
            string key = GetAttribute<string>("k", tag.Attributes);
            if (key == "barrier")
            {
                string type = GetAttribute<string>("v", tag.Attributes);
                if (Enum.IsDefined(typeof(BarrierType), type)) barrierType = (BarrierType)Enum.Parse(typeof(BarrierType), type);
            }
            else if (key == "fence_type")
            {
                string type = GetAttribute<string>("v", tag.Attributes);
                if (Enum.IsDefined(typeof(FenceType), type)) fencetype = (FenceType)Enum.Parse(typeof(FenceType), type);
            }
            else if (key == "material")
            {
                string type = GetAttribute<string>("v", tag.Attributes);
                if (Enum.IsDefined(typeof(Surface), type)) surfaceType = (Surface)Enum.Parse(typeof(Surface), type);
            }
            else if (key == "height")
            {
                height = GetAttribute<float>("v", tag.Attributes);
            }
            else if (key == "width")
            {
                width = GetAttribute<float>("v", tag.Attributes);
            }
        }
    }


    public Material GetMaterial()
    {
        if(fencetype != FenceType.standard) return Resources.Load<Material>("art/material/" + Enum.GetName(typeof(FenceType), fencetype));
        else if (surfaceType != Surface.unclassified) return Resources.Load<Material>("art/material/" + Enum.GetName(typeof(Surface), surfaceType));
        else return BarrierInfo.GetbarrierInfo(barrierType).material;
    }

    public string GetClassification()
    {
            return Enum.GetName(typeof(BarrierType), barrierType);
    }

    public int GetId() {
        return id;
    }

    public float GetHeight()
    {
        if (height != 0) return height;
        else return BarrierInfo.GetbarrierInfo(barrierType).height;
    }

    public float GetWidth()
    {
        if (width != 0) return width;
        else return BarrierInfo.GetbarrierInfo(barrierType).width;
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
}
