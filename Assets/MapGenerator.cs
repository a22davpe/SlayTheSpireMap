using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.TerrainTools;
using UnityEngine;
public class MapGenerator : MonoBehaviour
{
    /* TODO

    Iteration over road segments is ineffectiv kan use node info instead

    */


    #region Inspector variables

    [Header("Road properties")]
    [SerializeField] int maxMapLength;
    [SerializeField] int minMapLength;
    [SerializeField] int maxWidth = 5;

    [SerializeField] int pathAmount = 6;

    [Space()]
    [SerializeField] bool paintRoads;

    [SerializeField] LineRenderer lineRendererPrefab;

    [Header("Noder properties")]

    [SerializeField] bool placeNodes = true;
    [SerializeField] float shuffleStrength = 0.3f;

    [SerializeField] float distanceBetweenNodes = 1;
    [SerializeField] SpawningContainer spawningContainer;

    #endregion // Inspector variables

    #region NonInspector variables

    List<RoadSegment> roadSegments;

    [HideInInspector] public Dictionary<int2, NodeInfo> nodeDictionary;

    #endregion // NonInspector variables

    private void OnEnable()
    {
        Generate();
    }

    /// <summary>
    /// Generates a new map,
    /// deleting the old one
    /// </summary>
    public void Generate()
    {
        //Inspo
        //https://steamcommunity.com/sharedfiles/filedetails/?id=2830078257

        ResetValues(out int mapLength);

        //Places down everyRoad
        for (int i = 0; i < pathAmount; i++)
        {
            MakeRoad(mapLength, i);
        }

        if(placeNodes)
            PlaceNodes();
    }

    /// <summary>
    /// Clears all map related lists and sets a new map length
    /// </summary>
    /// <param name="mapLength"></param>
    private void ResetValues(out int mapLength)
    {
        mapLength = UnityEngine.Random.Range(minMapLength, maxMapLength + 1);

        //Resets all values

        roadSegments = new List<RoadSegment>();

        nodeDictionary = new Dictionary<int2, NodeInfo>();
    }

    #region Road Generation
    void MakeRoad(int mapLength, int roadIndex)
    {

        //Each point on the road
        int2[] points = new int2[mapLength];


        //Sets a random startPoint
        int2 startIndex = new int2(UnityEngine.Random.Range(0, maxWidth), 0);
        points[0] = startIndex;

        Vector2 transformPos = transform.position;

        nodeDictionary.TryAdd(points[0], new NodeInfo(startIndex, ShuffelPosition(startIndex) * distanceBetweenNodes + transformPos));


        for (int i = 1; i < mapLength; i++)
        {
            repitions = 0;
            int2 newIndex = GetNewRoadSegmentPosition(points[i - 1]);

            points[i] = newIndex;

            nodeDictionary.TryAdd(newIndex, new NodeInfo(newIndex, ShuffelPosition(newIndex) * distanceBetweenNodes + transformPos));

            nodeDictionary[newIndex].RoadsIn.Add(points[i - 1]);

            nodeDictionary[points[i - 1]].RoadsOut.Add(newIndex);

            roadSegments.Add(new RoadSegment(points[i - 1], points[i]));
        }

        if (paintRoads)
            PaintRoad(roadIndex, points);

    }

    private void PaintRoad(int roadIndex, int2[] points)
    {
        LineRenderer roadRenderer = Instantiate(lineRendererPrefab, transform);

        roadRenderer.positionCount = points.Length;

        Color lineColor = Color.HSVToRGB(roadIndex * 0.137508f % 1, 0.5f, 0.75f);

        roadRenderer.endColor = lineColor;

        roadRenderer.startColor = lineColor;

        UnityEngine.Vector3[] roadPoints = new UnityEngine.Vector3[points.Length];

        for (int i = 0; i < points.Length; i++)
        {
            int2 index = points[i];
            roadPoints[i] = nodeDictionary[index].position;
        }

        roadRenderer.SetPositions(roadPoints);
    }

    Vector2 ShuffelPosition(int2 index) => new Vector2(index.x + UnityEngine.Random.Range(-shuffleStrength, shuffleStrength), index.y + UnityEngine.Random.Range(-shuffleStrength, shuffleStrength));

    // counter to stop overflows
    int repitions;

    int2 GetNewRoadSegmentPosition(int2 currrentPosition)
    {

        //If no new ways found, just go fowards
        if (repitions > 6)
            return currrentPosition + new int2(0, 1);

        repitions++;

        int2 newPosition = new int2(Mathf.Clamp(currrentPosition.x + UnityEngine.Random.Range(-1, 2), 0, maxWidth - 1), currrentPosition.y + 1);

        // Check if it is a unique road
        if (roadSegments.Contains(new RoadSegment(currrentPosition, newPosition)))
            return GetNewRoadSegmentPosition(currrentPosition);


        //Make sure that roads dont cross each other
        switch (newPosition.x - currrentPosition.x)
        {
            case -1:
                if (roadSegments.Contains(new RoadSegment(new int2(currrentPosition.x - 1, currrentPosition.y), new int2(currrentPosition.x, currrentPosition.y + 1))))
                    return GetNewRoadSegmentPosition(currrentPosition);

                return newPosition;

            case 1:
                if (roadSegments.Contains(new RoadSegment(new int2(currrentPosition.x + 1, currrentPosition.y), new int2(currrentPosition.x, currrentPosition.y + 1))))
                    return GetNewRoadSegmentPosition(currrentPosition);

                return newPosition;

            case 0:
                return newPosition;

            default:
                Debug.LogError("Faulty new position value", this);
                return newPosition;
        }

    }

    struct RoadSegment
    {

        public RoadSegment(int2 start, int2 end)
        {
            m_End = end;
            m_Start = start;
        }

        public readonly int2 StartPoint => m_Start;

        public readonly int2 EndPoint => m_End;

        int2 m_Start;

        int2 m_End;
    }

    #endregion //Road Generation

    #region Node Placement

    void PlaceNodes()
    {

        foreach (KeyValuePair<int2, NodeInfo> node
         in nodeDictionary)
        {

            NodeInfo nodeInfo = node.Value;

            MapNodeBehaviour nodeType = spawningContainer.GetMapNode(nodeInfo.index);

            if (nodeType)
            {
                MapNodeBehaviour mapNode = Instantiate(nodeType, nodeInfo.position, Quaternion.identity, transform);

                mapNode.nodeInfo = nodeInfo;

            }
            else Debug.LogError($"no node found for index: {nodeInfo.index}");

        }
    }
    #endregion // Node Placement


#region CustomEditor
    [CustomEditor(typeof(MapGenerator))]
    public class MapGeneratorEditor: Editor
    {
        MapGenerator mapGenerator;

        private void OnEnable()
        {
            mapGenerator = target as MapGenerator;
        }



        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Generator New Map"))
                mapGenerator.Generate();

            base.OnInspectorGUI();
        }
    }
}
#endregion //CustomEditor


