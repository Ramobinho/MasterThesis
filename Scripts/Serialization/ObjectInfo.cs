using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public struct ObjectParameters
{

    public Quaternion rotation;
    public Vector3 localScale;
    public string objectName;
    public float heightOffset;


    public ObjectParameters(Quaternion r, Vector3 scale, string oName, float heightOffset)
    {
        rotation = r;
        localScale = scale;
        objectName = oName;
        this.heightOffset = heightOffset;
    }
}

public static class ObjectInfo
{
    public static Dictionary<ObjectType, ObjectParameters> objectInfo = new Dictionary<ObjectType, ObjectParameters> {
            { ObjectType.bollard , new ObjectParameters( Quaternion.Euler(0, 0, 0f),new Vector3(1, 1, 1),"bollard", 0) },
            { ObjectType.bench , new ObjectParameters( Quaternion.Euler(-90f, 180f, 0f),new Vector3(2, 2, 2), "bench", 0) },
            { ObjectType.waste_basket , new ObjectParameters( Quaternion.Euler(-90f, 0f, 0f),new Vector3(0.05f, 0.05f, 0.05f), "waste_basket", 0) },
            { ObjectType.tree, new ObjectParameters( Quaternion.Euler(0, 0, 0f), new Vector3(1, 1, 1),"tree", 0) },
            { ObjectType.street_lamp , new ObjectParameters(Quaternion.Euler(0, 0, 0f), new Vector3(1, 1, 1),"street_lamp", 0) },
            { ObjectType.post_box , new ObjectParameters( Quaternion.Euler(0, 0, 0f), new Vector3(1, 1, 1),"post_box", 0) },
            { ObjectType.bicycle_parking , new ObjectParameters( Quaternion.Euler(-90, 0, 0f),new Vector3(1, 1, 1),"bicycle_parking", 0) },
            { ObjectType.vending_machine , new ObjectParameters( Quaternion.Euler(0, 0, 0f), new Vector3(1.3f, 1.3f, 1.3f),"vending_machine", 0) },
            { ObjectType.fountain , new ObjectParameters( Quaternion.Euler(0, 0, 0f), new Vector3(1, 1, 1),"fountain", 0) },
            { ObjectType.street_cabinet , new ObjectParameters( Quaternion.Euler(-90, 0, 0f), new Vector3(1, 1, 1),"street_cabinet", 0.44f) }
    };

    public static ObjectParameters getObjectInfo(ObjectType type)
    {
        return objectInfo[type];
    }
}
