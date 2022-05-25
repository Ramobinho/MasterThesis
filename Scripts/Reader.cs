
using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;


public class Reader : MonoBehaviour
{
    public string HeightMap;
    public Material terrainMaterial;

    [HideInInspector]
    internal List<KMLBuilding> buildings;
    [HideInInspector]
    public List<Barrier> barriers;
    [HideInInspector]
    public List<Area> areas;
    [HideInInspector]
    public List<Road> roads;
    [HideInInspector]
    public List<TreeRow> treeRows;
    [HideInInspector]
    public List<OsmNode> objects;
    [HideInInspector]
    internal TerrainMaker terrainMaker;


    // OSM
    [HideInInspector]  
    public Dictionary<ulong, OsmNode> nodes;
    [HideInInspector]
    public Dictionary<ulong, OsmWay> ways;   // dictionarry van nodes die we accessen via hun ID (dat een ulong is)
    [HideInInspector]
    public OsmBounds bounds;
    [HideInInspector]
    public List<OsmRelation> relations;
    [Tooltip("resource file contains the OSM map data")]
    public string osmFile;
    [HideInInspector]
    public Dictionary<Vector3, OsmBuilding> OSMBuildings;

    // KML
    [HideInInspector]
    private UnityEngine.Object[] txts;
    [Tooltip("resource file contains the KML map data")]
    public string kmlMap;

    // GDAL
    public int imageHeight;
    public int imageWidth;

    public double topleftX;
    public double topleftZ;
    public double bottomleftX;
    public double bottomleftZ;
    public double toprightX;
    public double toprightZ;
    public double bottomrightX;
    public double bottomrightZ;

    [HideInInspector]
    public bool buildKeyPressed;


    public bool IsReady { get; private set; }
    void Start()
    {
        IsReady = false;
        buildKeyPressed = false;
        buildings = new List<KMLBuilding>();
        barriers = new List<Barrier>();
        areas = new List<Area>();
        roads = new List<Road>();
        treeRows = new List<TreeRow>();
        objects = new List<OsmNode>();
        readOsm();
        readKml();
        matchOsmAndKmlBuildings();
        Debug.Log("amunt of buildings from OSM: " + OSMBuildings.Count);
        Debug.Log("amount of buildings from KML: " + buildings.Count);
        TerrainPainter terrainPainter = terrainMaker.GetTerrainObject().AddComponent<TerrainPainter>();
        terrainPainter.paint();
        IsReady = true; // alles is ingelezen, we can build
    }

    private void readOsm()
    {
        var temp = Time.realtimeSinceStartup;
        nodes = new Dictionary<ulong, OsmNode>();
        ways = new Dictionary<ulong, OsmWay>();
        OSMBuildings = new Dictionary<Vector3, OsmBuilding>();
        relations = new List<OsmRelation> ();
        var txtAsset = Resources.Load<TextAsset>(osmFile);
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(txtAsset.text);
        SetBounds(doc.SelectSingleNode("/osm/bounds"));
        Debug.Log(HeightMap);
        terrainMaker = new TerrainMaker(HeightMap, terrainMaterial, bounds);
        terrainMaker.setParam(imageHeight, imageWidth, topleftX, topleftZ, bottomleftX, bottomleftZ, toprightX, toprightZ, bottomrightX, bottomrightZ);
        terrainMaker.StartTerrainGeneration();
        GetNodes(doc.SelectNodes("/osm/node"));
        GetWays(doc.SelectNodes("/osm/way"));
        GetRelations(doc.SelectNodes("/osm/relation"));
        Debug.Log("readOsm took: " + (Time.realtimeSinceStartup - temp).ToString());
    }

    private void readKml()
    {
        var temp = Time.realtimeSinceStartup;
        txts = Resources.LoadAll(kmlMap, typeof(TextAsset));    // info over welk text bestand halen uit osm data?
        double maxX = MercatorProjection.lonToX(bounds.MaxLon);
        double minX = MercatorProjection.lonToX(bounds.MinLon);
        double maxY = MercatorProjection.latToY(bounds.MaxLat);
        double minY = MercatorProjection.latToY(bounds.MinLat);

        for (int i = 0; i < txts.Length; i++)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(((TextAsset)txts[i]).text);
            XmlNamespaceManager nameSpaceManager = new XmlNamespaceManager(doc.NameTable);
            nameSpaceManager.AddNamespace("kml", "http://www.opengis.net/kml/2.2");

            // take the first node and take a lat and lon (doesnt matter which one, we' know all are in a square of 1km). 
            string coordinates = doc.SelectSingleNode("kml/kml:Document/kml:Placemark/kml:Polygon/kml:outerBoundaryIs/kml:LinearRing/kml:coordinates", nameSpaceManager).InnerText;
            string[] data = coordinates.Split(',', ' ');
            float Longitude = float.Parse(data[0], System.Globalization.CultureInfo.InvariantCulture);
            float Latitude = float.Parse(data[1], System.Globalization.CultureInfo.InvariantCulture);
            float xPoint = (float)MercatorProjection.lonToX(Longitude);
            float yPoint = (float)MercatorProjection.latToY(Latitude);
            // distance from point in file to closest point on outer rectangular bound of interesting region should be less than 1 km.
            // source https://stackoverflow.com/questions/5254838/calculating-distance-between-a-point-and-a-rectangular-box-nearest-point
            var dx = Math.Max(minX - xPoint, Math.Max(0, xPoint - maxX));
            var dy = Math.Max(minY - yPoint, Math.Max(0, yPoint - maxY));
            double distance = Math.Sqrt(dx * dx + dy * dy);
            if (distance <= 1400)   // if within 1 km range then there might be interesting information for us
            { 
                XmlNodeList xmlNodeList = doc.SelectNodes("kml/kml:Document/kml:Placemark", nameSpaceManager);
                foreach (XmlNode n in xmlNodeList)
                {
                    string inner = "";
                    string outer = "";
                    XmlNode outerBs = n.SelectSingleNode("kml:Polygon/kml:outerBoundaryIs/kml:LinearRing/kml:coordinates", nameSpaceManager);
                    if (outerBs != null) outer = outerBs.InnerText;
                    if (string.IsNullOrEmpty(outer)) continue;
                    string[] coords = outer.Split(',', ' ');
                    float longi = float.Parse(coords[0], System.Globalization.CultureInfo.InvariantCulture);
                    float lati = float.Parse(coords[1], System.Globalization.CultureInfo.InvariantCulture);
                    // if this specific building is in the bounds, add em to list of buildings
                    if (!(longi > bounds.MaxLon | longi < bounds.MinLon | lati > bounds.MaxLat | lati < bounds.MinLat)) {
                        XmlNode innerBs = n.SelectSingleNode("kml:Polygon/kml:innerBoundaryIs/kml:LinearRing/kml:coordinates", nameSpaceManager);
                        if (innerBs != null) inner = innerBs.InnerText;
                        string name = n.SelectSingleNode("kml:name", nameSpaceManager).InnerText;
                        buildings.Add(new KMLBuilding(name, outer, inner, this));
                    }
                }
            }
        }
        Debug.Log("readKml took: " + (Time.realtimeSinceStartup - temp).ToString());
    }


    void GetWays(XmlNodeList xmlNodeList)
    {
        new OsmWayManager(xmlNodeList, this);
    }

    public void GetNodes(XmlNodeList xmlNodeList)
    {
        new OsmNodeManager(xmlNodeList, this);   
    }

    void SetBounds(XmlNode xmlNode)
    {
        bounds = new OsmBounds(xmlNode);
    }

    void GetRelations(XmlNodeList xmlNodeList)
    {
        new OsmRelationManager(xmlNodeList, this);
    }

    public void matchOsmAndKmlBuildings() {
        var temp = Time.realtimeSinceStartup;
        if (OSMBuildings.Count == 0) return;
        foreach (KeyValuePair<Vector3, OsmBuilding> entry in OSMBuildings)
        {
            float minDistance = 1000000;
            KMLBuilding matchB = null;
            foreach (KMLBuilding b in buildings)
            {
                float dist = Vector3.Distance(b.GetCentre(), entry.Key);
                if (dist < minDistance) {
                    minDistance = dist;
                    matchB = b;
                }
            }
            matchB.setMatch(entry.Value);
        }
        Debug.Log("matchOsm took: " + (Time.realtimeSinceStartup - temp).ToString());
    }

    public void BuildingKeyPressed() {
        Debug.Log("buildKeyPressed is true");
        buildKeyPressed = true;
    }

    public bool IsBuildingKeyPressed()
    {
        return buildKeyPressed;
    }
}
