
using System.Xml;
using UnityEngine;

public class OsmBounds : BaseOsm
{
    public float MinLat { get; private set; }
    public float MaxLat { get; private set; }
    public float MinLon { get; private set; }
    public float MaxLon { get; private set; }

    public float MinY { get; private set; }
    public float MaxY { get; private set; }
    public float MinX { get; private set; }
    public float MaxX{ get; private set; }

    public Vector3 Centre { get; private set; }

    public OsmBounds(XmlNode node)
    {
        MinLat = GetAttribute<float>("minlat", node.Attributes);
        MaxLat = GetAttribute<float>("maxlat", node.Attributes);
        MinLon = GetAttribute<float>("minlon", node.Attributes);
        MaxLon = GetAttribute<float>("maxlon", node.Attributes);
        // berekenne van center
        float x = (float)((MercatorProjection.lonToX(MaxLon) + MercatorProjection.lonToX(MinLon)) / 2);
        float y = (float)((MercatorProjection.latToY(MaxLat) + MercatorProjection.latToY(MinLat)) / 2);
        Centre = new Vector3(x, 0, y);  // zodat alles pivot rond centerpunt (2e argument is height! dus eigenlijk z in 3D).
        MinY = (float)MercatorProjection.latToY(MinLat);
        MaxY = (float)MercatorProjection.latToY(MaxLat);
        MinX = (float)MercatorProjection.lonToX(MinLon);
        MaxX = (float)MercatorProjection.lonToX(MaxLon);
    }
}

