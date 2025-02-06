using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;

public class MapNodeBehaviour : MonoBehaviour, IPointerClickHandler
{

    public NodeInfo nodeInfo;

    public NodeType nodeType;

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"Nodes in: {nodeInfo.RoadsIn.Count}");
        Debug.Log($"Nodes out: {nodeInfo.RoadsOut.Count}");
    }
}


public class NodeInfo{

    public NodeInfo(int2 index, Vector2 position){

        this.position = position;
        this.index = index;
    }

    public List<int2> RoadsIn = new List<int2>();

    public List<int2> RoadsOut = new List<int2>();

    public int2 index;

    public Vector2 position;

}