using UnityEngine;

[RequireComponent(typeof(MapReader))]
abstract class InfraStructureBehaviour : MonoBehaviour
{
    protected MapReader map;

    void Awake()
    {
        map = GetComponent<MapReader>();    
    }

    protected Vector3 GetCentre(OsmWay way)
    {
        Vector3 total = Vector3.zero;

        foreach (var id in way.NodeIds)
        {
            total += map.nodes[id];
        }

        return total / way.NodeIds.Count;
    }
}