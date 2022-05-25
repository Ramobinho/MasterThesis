using System;
using System.Globalization;
using System.Xml;
using UnityEngine;

// Bij bicycle parking, vergeet niet capacitu te checken voor de grootte
// TODO: crossing!! later nadenken hoe op te lossen
public enum ObjectType
{
    bollard, bench, waste_basket, tree, street_lamp, post_box, street_cabinet, bicycle_parking, vending_machine, fountain
}
public class OsmNode : BaseOsm
{
    // XML node bevat latitude, longitude en id
    public ulong ID { get; private set; }
    public float X { get; private set; }    
    public float Y { get; private set; }
    public float Z { get; private set; }

    public ObjectType type;

    public Reader map;

    bool isObject; 

    // node naar een vector3 conversion impliciet (dus als je zegt Vectro3 = node, dan gaat hij dit doen.
    public static implicit operator Vector3 (OsmNode node)
    {
        return new Vector3(node.X, node.Z, node.Y);
    }
public OsmNode(XmlNode node, Reader map)    // krijgt node binnen met attributes
    {
        this.map = map;
        ID = GetAttribute<ulong>("id", node.Attributes);   // gaat "id" lezen uit xml file bij de bijbehorende node.
        X = (float)MercatorProjection.lonToX(GetAttribute<float>("lon", node.Attributes));
        Y = (float)MercatorProjection.latToY(GetAttribute<float>("lat", node.Attributes));
        Z = map.terrainMaker.FindHeight(new Vector3(X, 0, Y));
        XmlNodeList tags = node.SelectNodes("tag");
        foreach (XmlNode tag in tags)
        {
            string key = GetAttribute<string>("k", tag.Attributes);
            if (key == "amenity" || key == "natural" || key == "highway" || key == "barrier" || key == "man_made")
            {
                string oType = GetAttribute<string>("v", tag.Attributes);
                if (Enum.IsDefined(typeof(ObjectType), oType))
                {
                    type = (ObjectType)Enum.Parse(typeof(ObjectType), oType);
                    isObject = true;
                }
                else isObject = false;
            }
        }
    }

    public Vector3 getPosition() {
        return new Vector3(X, ObjectInfo.getObjectInfo(type).heightOffset + Z, Y) - map.bounds.Centre;
    }

    public Quaternion getRotation()
    {
        if (type == ObjectType.tree) return Quaternion.Euler(0, UnityEngine.Random.Range(0.0f, 360.0f), 0f);
        return ObjectInfo.getObjectInfo(type).rotation;
    }

    public string getPrefabName()
    {
        return ObjectInfo.getObjectInfo(type).objectName;
    }

    public Vector3 getScale() {
        if (type == ObjectType.tree) {
            float thickness = UnityEngine.Random.Range(0.6f, 1.4f);
            float t = Mathf.InverseLerp(0.6f, 1.4f, thickness);
            float height = Mathf.Lerp(0.6f, 2f, t);
            return new Vector3(thickness, height, thickness);
        }
        return ObjectInfo.getObjectInfo(type).localScale;
    }

    public ulong GetId()
    {
        return ID;
    }

    public bool IsObject()
    {
        return isObject;
    }
}
