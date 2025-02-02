using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class MapNode : MonoBehaviour
{
    public GameObject visual;

    public Vector2 pos;

    public List<Vector2> outRoads;

    public bool HasCrossingRoad(int roadXDirection){
        return outRoads.Contains(new Vector2(-roadXDirection,1) + pos);
    }
}
