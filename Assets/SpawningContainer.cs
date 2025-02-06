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

    GUIStyle titleStyle;
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

        titleStyle = new GUIStyle(GUI.skin.label);

        titleStyle.richText = true;

        //__________Nodes_______________
        showNodes = EditorGUILayout.Foldout(showNodes, "Nodes");

        if (showNodes)
        {
            for (global::System.Int32 i = 0; i < container.nodes.Count; i++)
            {
                EditorGUILayout.LabelField("----------------------------");

                Node node = container.nodes[i];

                if (container.nodes[i].node)
                    EditorGUILayout.LabelField($"<color=#{GetColorByNodeType(node.node.nodeType).ToHexString()}>{node.node.name}</color>", titleStyle);
                
                else
                    EditorGUILayout.LabelField("No node avaliable");

                container.nodes[i].spawnOdds = EditorGUILayout.FloatField("SpawnOdds", node.spawnOdds);
                container.nodes[i].unlockLevel = EditorGUILayout.IntField("Unlock level", node.unlockLevel);

                container.nodes[i].node = (MapNodeBehaviour)EditorGUILayout.ObjectField("Node", node.node, typeof(MapNodeBehaviour), true);

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
        DrawFloorInfoList("Constant Floors", ref showConstantFloors, container.constantFloors);

        BetterSpace(2);

        //_______Not Premitted__________
        DrawFloorInfoList("Non premitted nodes", ref showNonPremitted, container.notPremitted);

        //_______Record Things__________

        Undo.RecordObject(container, container.name);

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();

            serializedObject.Update();

            EditorUtility.SetDirty(container);
        }

    }

    private void DrawFloorInfoList(string name ,ref bool show, List<FloorInfo> information)
    {
        show = EditorGUILayout.Foldout(show, name);

        if (show)
        {
            for (global::System.Int32 i = 0; i < information.Count; i++)
            {
                EditorGUILayout.LabelField("----------------------------");

                if (information[i].mapNode)
                    EditorGUILayout.LabelField(information[i].mapNode.name);
                else
                    EditorGUILayout.LabelField("No node avaliable");

                information[i].floorLevel = EditorGUILayout.IntField("Floor level", information[i].floorLevel);

                information[i].mapNode = (MapNodeBehaviour)EditorGUILayout.ObjectField("Node", information[i].mapNode, typeof(MapNodeBehaviour), true);

                if (GUILayout.Button("Remove"))
                {
                    information.RemoveAt(i);
                }
            }
            EditorGUILayout.LabelField("----------------------------");

            if (GUILayout.Button("Add Node"))
                information.Add(new FloorInfo());
            EditorGUILayout.LabelField("----------------------------");
        }
    }

    Color GetColorByNodeType(NodeType type)
    {
        switch (type)
        {
            case NodeType.Elite:
                return Color.red;
            case NodeType.Event:
                return Color.white;
            case NodeType.Market:
                return Color.yellow;
            case NodeType.Monster:
                return Color.green;
            case NodeType.RestSite:
                return new Color(0.69f,0.09f,0.91f) ;
            case NodeType.Treasure:
                return Color.blue;
            default:
                Debug.LogError("No color set for this type", this);
                return Color.gray;
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


public enum NodeType
{
    Elite,
    Event,
    Market,
    Monster,
    RestSite,
    Treasure
}

