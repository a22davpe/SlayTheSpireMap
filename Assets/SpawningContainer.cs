using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Mathematics;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEditor;

[CreateAssetMenu]
[Serializable]
public class SpawningContainer : ScriptableObject
{
    public Node[] nodes;

    public FloorInfo[] constantFloors;

    public FloorInfo[] notPremitted;

    public MapNodeBehaviour GetMapNode(int2 index){

        if(TryGetConstantFloor(constantFloors, index.y, out MapNodeBehaviour mapSlot))
            return mapSlot;

        return RandomSlot(index.y);
    }


    public MapNodeBehaviour RandomSlot(int floorLevel){

        Node[] availableNodes = GetAvailableMapNodes(floorLevel);
        float randomValue = UnityEngine.Random.Range(0, TotalOdds(availableNodes));

        for (int i = 0; i < availableNodes.Length; i++)
        {
            randomValue -= availableNodes[i].spawnOdds;

            if(randomValue <= 0)
                return availableNodes[i].node;
        }

        Debug.LogError("No mapslot found!");

        return new MapNodeBehaviour();
    }

    float TotalOdds(Node[] nodes) => nodes.Sum(t => t.spawnOdds);

    Node[] GetAvailableMapNodes(int floorLevel){

        List<Node> temp = new List<Node>();

        for (int i = 0; i < nodes.Length; i++)
        {
            if(nodes[i].unlockLevel <= floorLevel && MapNodeIsPremitted(nodes[i].node, notPremitted, floorLevel))
                temp.Add(nodes[i]);
        }

        if(temp.Count == 0)
            Debug.LogError("No available slots");

        return temp.ToArray();
    }

        bool TryGetConstantFloor(FloorInfo[] floorInfo,int floorLevel,out MapNodeBehaviour mapSlot){

        for (int i = 0; i < floorInfo.Length; i++)
        {
            if(floorInfo[i].floorLevel == floorLevel)
            {
                mapSlot = floorInfo[i].mapNode;
                return true;
            }
        }

        mapSlot = null;
        return false;

    }

    /// <summary>
    /// Checks if the node is premitted based on an non premitted node list given the floor level
    /// </summary>
    /// <param name="mapSlot"></param>
    /// <param name="nonPremittedList"></param>
    /// <param name="floorLevel"></param>
    /// <returns></returns>
    bool MapNodeIsPremitted( MapNodeBehaviour mapSlot,FloorInfo[] nonPremittedList, int floorLevel){
        for (int i = 0; i < nonPremittedList.Length; i++)
        {
            if(nonPremittedList[i].mapNode == mapSlot && nonPremittedList[i].floorLevel == floorLevel)
            return false;
        }

        return true;
    }
}

[System.Serializable]

public class FloorInfo
{
    public int floorLevel = 0;
    public MapNodeBehaviour mapNode;
}

[System.Serializable]
public class Node
{

    public float spawnOdds = 1;

    public int unlockLevel = 0;

    public MapNodeBehaviour node;

}


[CustomEditor (typeof(SpawningContainer)), CanEditMultipleObjects]
public class SpawnContainerEditor : Editor
{
    SpawningContainer container;
    private void OnEnable()
    {
        container = target as SpawningContainer;
    }

    [SerializeField] bool showNodes;

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();

        base.OnInspectorGUI();

        //__________Nodes_______________
        showNodes = EditorGUILayout.Foldout(showNodes, "Nodes");

        if (showNodes)
        {
            for (global::System.Int32 i = 0; i < container.nodes.Length; i++)
            {
                
            }
        }

        //______Constant floors_________


        //_______Not Premitted__________


        //_______Record Things__________

        Undo.RecordObject(container, container.name);

        if(EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();

            serializedObject.Update();

            EditorUtility.SetDirty(container);
        }
    }


}

