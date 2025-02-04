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
    public List<Node> nodes;

    public List<FloorInfo> constantFloors;

    public List<FloorInfo> notPremitted;

    public MapNodeBehaviour GetMapNode(int2 index)
    {

        if (TryGetConstantFloor(constantFloors, index.y, out MapNodeBehaviour mapSlot))
            return mapSlot;

        return RandomSlot(index.y);
    }


    public MapNodeBehaviour RandomSlot(int floorLevel)
    {

        Node[] availableNodes = GetAvailableMapNodes(floorLevel);
        float randomValue = UnityEngine.Random.Range(0, TotalOdds(availableNodes));

        for (int i = 0; i < availableNodes.Length; i++)
        {
            randomValue -= availableNodes[i].spawnOdds;

            if (randomValue <= 0)
                return availableNodes[i].node;
        }

        Debug.LogError("No mapslot found!");

        return new MapNodeBehaviour();
    }

    float TotalOdds(Node[] nodes) => nodes.Sum(t => t.spawnOdds);

    Node[] GetAvailableMapNodes(int floorLevel)
    {

        List<Node> temp = new List<Node>();

        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].unlockLevel <= floorLevel && MapNodeIsPremitted(nodes[i].node, notPremitted, floorLevel))
                temp.Add(nodes[i]);
        }

        if (temp.Count == 0)
            Debug.LogError("No available slots");

        return temp.ToArray();
    }

    bool TryGetConstantFloor(List<FloorInfo> floorInfo, int floorLevel, out MapNodeBehaviour mapSlot)
    {

        for (int i = 0; i < floorInfo.Count; i++)
        {
            if (floorInfo[i].floorLevel == floorLevel)
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
    bool MapNodeIsPremitted(MapNodeBehaviour mapSlot, List<FloorInfo> nonPremittedList, int floorLevel)
    {
        for (int i = 0; i < nonPremittedList.Count; i++)
        {
            if (nonPremittedList[i].mapNode == mapSlot && nonPremittedList[i].floorLevel == floorLevel)
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


[CustomEditor(typeof(SpawningContainer)), CanEditMultipleObjects]
public class SpawnContainerEditor : Editor
{
    SpawningContainer container;
    private void OnEnable()
    {
        container = target as SpawningContainer;
    }

    [SerializeField] bool showNodes;
    [SerializeField] bool showConstantFloors;
    [SerializeField] bool showNonPremitted;

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();

        //__________Nodes_______________
        showNodes = EditorGUILayout.Foldout(showNodes, "Nodes");

        if (showNodes)
        {
            for (global::System.Int32 i = 0; i < container.nodes.Count; i++)
            {
                EditorGUILayout.LabelField("----------------------------");

                if (container.nodes[i].node)
                    EditorGUILayout.LabelField(container.nodes[i].node.name);
                else
                    EditorGUILayout.LabelField("No node avaliable");

                container.nodes[i].spawnOdds = EditorGUILayout.FloatField("SpawnOdds", container.nodes[i].spawnOdds);
                container.nodes[i].unlockLevel = EditorGUILayout.IntField("Unlock level", container.nodes[i].unlockLevel);

                container.nodes[i].node = (MapNodeBehaviour)EditorGUILayout.ObjectField("Node", container.nodes[i].node, typeof(MapNodeBehaviour), true);

                if (GUILayout.Button("Remove"))
                {
                    container.nodes.RemoveAt(i);
                }
            }

            EditorGUILayout.LabelField("----------------------------");

            if (GUILayout.Button("Add Node"))
                container.nodes.Add(new Node());
            EditorGUILayout.LabelField("----------------------------");
        }

        BetterSpace(2);

        //______Constant floors_________

        showConstantFloors = EditorGUILayout.Foldout(showConstantFloors, "Constant Floors");

        if (showConstantFloors)
        {
            for (global::System.Int32 i = 0; i < container.constantFloors.Count; i++)
            {
                EditorGUILayout.LabelField("----------------------------");

                if (container.nodes[i].node)
                    EditorGUILayout.LabelField(container.constantFloors[i].mapNode.name);
                else
                    EditorGUILayout.LabelField("No node avaliable");

                container.constantFloors[i].floorLevel = EditorGUILayout.IntField("Floor level", container.constantFloors[i].floorLevel);

                container.constantFloors[i].mapNode = (MapNodeBehaviour)EditorGUILayout.ObjectField("Node", container.constantFloors[i].mapNode, typeof(MapNodeBehaviour), true);

                if (GUILayout.Button("Remove"))
                {
                    container.constantFloors.RemoveAt(i);
                }
            }

            EditorGUILayout.LabelField("----------------------------");

            if (GUILayout.Button("Add Node"))
                container.constantFloors.Add(new FloorInfo());
            EditorGUILayout.LabelField("----------------------------");
        }

        BetterSpace(2);

        //_______Not Premitted__________

        showNonPremitted = EditorGUILayout.Foldout(showNonPremitted, "Not Premitted Node");

        if (showNonPremitted)
        {
            for (global::System.Int32 i = 0; i < container.notPremitted.Count; i++)
            {
                EditorGUILayout.LabelField("----------------------------");

                if (container.nodes[i].node)
                    EditorGUILayout.LabelField(container.notPremitted[i].mapNode.name);
                else
                    EditorGUILayout.LabelField("No node avaliable");

                container.constantFloors[i].floorLevel = EditorGUILayout.IntField("Floor level", container.notPremitted[i].floorLevel);

                container.constantFloors[i].mapNode = (MapNodeBehaviour)EditorGUILayout.ObjectField("Node", container.notPremitted[i].mapNode, typeof(MapNodeBehaviour), true);

                if (GUILayout.Button("Remove"))
                {
                    container.constantFloors.RemoveAt(i);
                }
            }

            EditorGUILayout.LabelField("----------------------------");

            if (GUILayout.Button("Add Node"))
                container.constantFloors.Add(new FloorInfo());
            EditorGUILayout.LabelField("----------------------------");
        }
        //_______Record Things__________

        Undo.RecordObject(container, container.name);

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();

            serializedObject.Update();

            EditorUtility.SetDirty(container);
        }

    }


    void BetterSpace(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            EditorGUILayout.Space();
        }
    }

}


public enum NodeTypes
{
    Elite,
    Event,
    Market,
    Monster,
    RestSite,
    Treasure
}

