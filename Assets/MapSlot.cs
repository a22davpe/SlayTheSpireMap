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


public class NodeInfo{

    public NodeInfo(int2 index, Vector2 position){

        this.position = position;
        this.index = index;

    }

    public List<int2> RoadsIn;

    public List<int2> RoadsOut;

    public int2 index;

    public Vector2 position;

}