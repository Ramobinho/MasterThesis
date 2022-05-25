
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public struct kmlNode
{
    public float Latitude { get; set; }
    public float Longitude { get; set; }

    public float Height { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
}

public enum BuildingType
{
    unclassified, apartments, house, dormitory, residential, church, garage, commercial, retail, office, shed
}

public enum BuildingSurface
{
    Plaster01, Plaster02, Plaster03, t1, t2, t3, t4, t5, t6, t7, t8
}

public enum RoofSurface
{
    metal, Plaster01, Plaster02, Plaster03, Plaster04, Plaster05, roof1, roof2
}

public class KMLBuilding
{
    private OsmBuilding matchingOsmBuilding;
    private List<kmlNode> outer;
    private List<kmlNode> inner;
    private static int counter;
    private static Random random;
    private int id;
    private BuildingType type;
    private string name;
    private BuildingSurface wallSurface;
    private RoofSurface roofSurface;

    static KMLBuilding()
    {
        random = new Random();
    }
    public KMLBuilding(string name, string outerBounds, string innerBounds, Reader reader)
    {
        this.name = name;
        outer = new List<kmlNode>();
        inner = new List<kmlNode>();
        this.id = System.Threading.Interlocked.Increment(ref counter);
        type = BuildingType.unclassified ;
        readString(ref outer, outerBounds,reader);
        readString(ref inner, innerBounds,reader);
        Array wallValues = Enum.GetValues(typeof(BuildingSurface));
        wallSurface = (BuildingSurface)wallValues.GetValue(random.Next(wallValues.Length));
        Array roofValues = Enum.GetValues(typeof(RoofSurface));
        int randomR = random.Next(roofValues.Length);
        Debug.Log(randomR);
        roofSurface = (RoofSurface)roofValues.GetValue(randomR);
    }

    // reading strings with coordinates and and converting them to nodes of polygon
    private void readString(ref List<kmlNode> l, string s, Reader reader) {
        if (string.IsNullOrEmpty(s)) return;
        string [] data = s.Split(',',' ');
        for (int i = 0; i < data.Length; i += 3)
        {
            kmlNode node = new kmlNode();
            node.Longitude = float.Parse(data[i], System.Globalization.CultureInfo.InvariantCulture);
            node.Latitude = float.Parse(data[i+1], System.Globalization.CultureInfo.InvariantCulture);
            node.Height = float.Parse(data[i + 2], System.Globalization.CultureInfo.InvariantCulture);
            if (node.Height == 0) node.Height = 3;    // random tuinhuisje van 3 meter ofzo?
            node.X = (float)MercatorProjection.lonToX(node.Longitude);
            node.Y = (float)MercatorProjection.latToY(node.Latitude);
            node.Z = reader.terrainMaker.FindHeight(new Vector3(node.X, 0, node.Y));
            l.Add(node);
        }
    }

    public int GetId() {
        return id;
    }
    public Vector3 GetCentre()
    {
        Vector3 total = Vector3.zero;

        foreach (kmlNode n in outer)
        {
            total += new Vector3(n.X, 0, n.Y);
        }
        return total / outer.Count;  
    }

    public List<kmlNode> getOuterBoundaries() {
        return outer;
    }

    public List<kmlNode> getInnerBoundaries()
    {
        return inner;
    }

    public Material getWallMaterial() {
        return Resources.Load<Material>("art/buildingMaterial/" + Enum.GetName(typeof(BuildingSurface), wallSurface));

    }

    public Material getRoofMaterial()
    {
        return Resources.Load<Material>("art/roofMaterial/" + Enum.GetName(typeof(RoofSurface), roofSurface));

    }

    public void setMatch(OsmBuilding m) {
        matchingOsmBuilding = m;
        //buildingMaterial = m.GetMaterial();
        type = m.GetClassification();
        if (name == "__" && m.GetName() != " 0") name = m.GetName();
        else if (name == "__" && m.GetName() != " 0") name = "unknown building name" ;
        name = m.GetClassification() + name;
    }

    public string getType() {
        return Enum.GetName(typeof(BuildingType), type);
    }

    public string getName()
    {
        return name;
    }
}
