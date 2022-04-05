using UnityEngine;

[RequireComponent(typeof(MapReader))]
abstract class InfraStructureBehaviour : MonoBehaviour
{
    protected MapReader map;
    protected MeshMaker meshMaker;

    void Awake()
    {
        map = GetComponent<MapReader>();
        meshMaker = GetComponent<MeshMaker>();
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