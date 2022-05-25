using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public struct RoadParameters
{

    public float width;
    public int lanes;
    public string texture;
    public bool isLit;
    public int hierarchy;


    public RoadParameters(float width, int lanes,  string texture, bool isLit, int hierarchy)
    {
        this.width = width;
        this.lanes = lanes;
        this.isLit = isLit;
        this.texture = texture;
        this.hierarchy = hierarchy;
    }
}
public static class RoadInfo
{
    public static Dictionary<RoadType, RoadParameters> roadInfo = new Dictionary<RoadType, RoadParameters> {
        { RoadType.unclassified , new RoadParameters( 3, 1,"road",false,0) },
        { RoadType.footway , new RoadParameters( 1.8f, 1, "footway",false,1) },
        { RoadType.cycleway , new RoadParameters( 1.5f, 1,"cycleway",true,2) },
        { RoadType.primary , new RoadParameters( 3.5f, 2,"road",true,0) },
        { RoadType.tertiary_link , new RoadParameters( 2.5f, 1,"road",false,0) },
        { RoadType.residential , new RoadParameters( 2.8f, 2,"road",false,0) },
        { RoadType.service , new RoadParameters( 2, 1,"parking",true,0) },
        { RoadType.track , new RoadParameters( 2, 1,"track",false,1) },
        { RoadType.path , new RoadParameters( 1.4f, 1, "footway",false,1)},
        { RoadType.tertiary , new RoadParameters( 2.5f, 2,"road",false,2)},
        { RoadType.crossing , new RoadParameters( 2.0f, 1,"crossing",false,2)},
        { RoadType.pier , new RoadParameters(2.0f, 1,"wood",false,2)},
        { RoadType.secondary , new RoadParameters( 3.0f, 2,"road",true,1) },
        { RoadType.platform , new RoadParameters( 2.0f, 1,"platform",false,2) },
        { RoadType.living_street , new RoadParameters( 2.5f, 1, "road",false,0) },
        { RoadType.pedestrian , new RoadParameters( 1.8f, 1,"footway",true,1) },
        { RoadType.road , new RoadParameters( 2.8f, 2,"road",false,0) }
    };

    public static RoadParameters getRoadInfo(RoadType type)
    {
        return roadInfo[type];
    }
}
