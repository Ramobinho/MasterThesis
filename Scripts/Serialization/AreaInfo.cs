using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public struct AreaParameters
{

    public string texture;


    public AreaParameters(string m)
    {
        texture = m;
    }
}
public static class AreaInfo
{
    public static Dictionary<AreaType, AreaParameters> areaInfo = new Dictionary<AreaType, AreaParameters> {
            { AreaType.unclassified , new AreaParameters("parking") },
            { AreaType.park , new AreaParameters("soil") },
            { AreaType.garden , new AreaParameters("grass") },
            { AreaType.pitch , new AreaParameters("pitch") },
            { AreaType.outdoor_seating , new AreaParameters("grass") },
            { AreaType.playground , new AreaParameters("pitch") },
            { AreaType.fitness_station , new AreaParameters("pitch") },
            { AreaType.beach_resort , new AreaParameters("sand") },
            { AreaType.wood , new AreaParameters("forest") },
            { AreaType.bare_rock , new AreaParameters("sand") },
            { AreaType.marina , new AreaParameters("marina") },
            { AreaType.greenery , new AreaParameters("greenery")},
            { AreaType.woodchips , new AreaParameters("forest") },
            { AreaType.dog_park , new AreaParameters("grass") },
            { AreaType.golf_course , new AreaParameters("grass")},
            { AreaType.nature_reserve , new AreaParameters("scrub") },
            { AreaType.swimming_pool , new AreaParameters("water") },
            { AreaType.swimming_area , new AreaParameters("water") },
            { AreaType.track , new AreaParameters("sand")},
            { AreaType.parking , new AreaParameters("parking") },
            { AreaType.bicycle_parking , new AreaParameters("parking") },
            { AreaType.parking_space , new AreaParameters("parking") },
            { AreaType.grave_yard , new AreaParameters( "soil") },
            { AreaType.brownfield , new AreaParameters("soil") },
            { AreaType.railway , new AreaParameters("concrete") },
            { AreaType.recreation_ground , new AreaParameters("grass") },
            { AreaType.urban_green , new AreaParameters("grass") },
            { AreaType.flowerbed , new AreaParameters("flowers") },
            { AreaType.farmland , new AreaParameters("grass") },
            { AreaType.farmyard , new AreaParameters("grass") },
            { AreaType.grass , new AreaParameters("grass") },
            { AreaType.greenfield , new AreaParameters("grass") },
            { AreaType.shrubbery , new AreaParameters("shrubbery") },
            { AreaType.industrial , new AreaParameters("compacted") },
            { AreaType.meadow , new AreaParameters("grass") },
            { AreaType.cemetery , new AreaParameters("soil") },
            { AreaType.village_green , new AreaParameters("grass") },
            { AreaType.vineyard , new AreaParameters("flowers") },
            { AreaType.orchard , new AreaParameters("flowers") },
            { AreaType.landfill , new AreaParameters("grass") },
            { AreaType.plant_nursery , new AreaParameters("grass") },
            { AreaType.allotments , new AreaParameters("soil") },
            { AreaType.basin , new AreaParameters("water") },
            { AreaType.water , new AreaParameters("water") },
            { AreaType.scrub , new AreaParameters("scrub") },
            { AreaType.sand , new AreaParameters("sand") },
            { AreaType.beach , new AreaParameters( "sand") },
            { AreaType.land , new AreaParameters("grass") },
            { AreaType.car_sharing , new AreaParameters("parking") },
            { AreaType.square , new AreaParameters("sett") },
            { AreaType.grassland , new AreaParameters("grass") },
            { AreaType.fountain , new AreaParameters("water") },
            { AreaType.trees , new AreaParameters("forest") },
            { AreaType.forest , new AreaParameters("park2") }

    };

    public static AreaParameters getAreaInfo(AreaType type)
    {
        if (!areaInfo.ContainsKey(type)) Debug.Log(type);
        return areaInfo[type];
    }
}
