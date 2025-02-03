using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
public class MapGeneration : MonoBehaviour
{
    #region Serlized variables

    public bool GenerateNewMap;

    [Header("Road properties")]
    public int maxMapLength;
    public int minMapLength;
    public int maxWidth = 5;

    public int pathAmount = 6;

    [Space()]
    public bool paintRoads;

    public LineRenderer lineRendererPrefab;

    [Header("Noder properties")]

    public SpawningContainer spawningContainer;

    #endregion // Serlized variables

    #region Nonserlized variables

    Vector2[,] m_Map;

    List<RoadSegment> roadSegments;
    List<int2> nodes;

    [HideInInspector] public Dictionary<int2, NodeInfo> nodeDictionary;

    #endregion // Nonserlized variables


    private void Update() {
        if(GenerateNewMap)
        {
            GenerateNewMap = false;

            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }

            Generate();
        }
    }

    private void OnEnable() {
        Generate();
    }

    /// <summary>
    /// Generates a new map,
    /// deleting the old one
    /// </summary>
    public void Generate()
    {
        //https://steamcommunity.com/sharedfiles/filedetails/?id=2830078257 

        ResetValues(out int mapLength);

        //Places down everyRoad
        for (int i = 0; i < pathAmount; i++)
        {
            MakeRoad(mapLength, i);
        }

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
        m_Map = new Vector2[maxWidth, mapLength];

        for (int x = 0; x < m_Map.GetLength(0); x++)
        {
            for (int y = 0; y < m_Map.GetLength(1); y++)
            {
                m_Map[x, y] = Vector2.down;
            }

        }

        roadSegments = new List<RoadSegment>();
        nodes = new List<int2>();

        nodeDictionary = new Dictionary<int2, NodeInfo>();
    }

    #region Road Generation
    void MakeRoad(int mapLength, int roadIndex){

        //Each point on the road
        int2[] points = new int2[mapLength];


        //Sets a random startPoint
        int2 startIndex = new int2(UnityEngine.Random.Range(0, maxWidth), 0);
        points[0] = startIndex;

        //if(m_Map[startIndex.x,startIndex.y] == Vector2.down)
        //    m_Map[startIndex.x,startIndex.y] = new Vector2(startIndex.x + UnityEngine.Random.Range(-0.3f,0.3f),startIndex.y + UnityEngine.Random.Range(-0.3f,0.3f));

        nodeDictionary.TryAdd(points[0], new NodeInfo(startIndex, ShuffelPosition(startIndex)));

        nodes.Add(points[0]);
        
        
        
        for (int i = 1; i < mapLength; i++)
        {
            repitions = 0;
            int2 newIndex = GetNewRoadSegmentPosition(points[i-1]);
            
            points[i] = newIndex;

            nodeDictionary.TryAdd(newIndex, new NodeInfo(newIndex, ShuffelPosition(newIndex)));
            
            roadSegments.Add(new RoadSegment(points[i-1], points[i]));

            nodes.Add(points[i]);
        }
/*
        foreach (var segment in roadSegments)
        {
            int2 int2 = segment.EndPoint;
            
            if(m_Map[int2.x,int2.y] == Vector2.down)
            {
                m_Map[int2.x,int2.y] = new Vector2(int2.x + UnityEngine.Random.Range(-0.3f,0.3f),int2.y + UnityEngine.Random.Range(-0.3f,0.3f));
            }

        }*/

        if(paintRoads)
            PaintRoad( roadIndex, points);
        
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

    Vector2 ShuffelPosition(int2 index) => new Vector2(index.x + UnityEngine.Random.Range(-0.3f,0.3f), index.y + UnityEngine.Random.Range(-0.3f,0.3f));

    // counter to stop overflows
    int repitions;

    int2 GetNewRoadSegmentPosition(int2 currrentPosition){

        //If no new ways found, just go fowards
        if(repitions > 6)
            return currrentPosition + new int2(0,1);
        repitions ++;

        int2 newPosition = new int2(Mathf.Clamp( currrentPosition.x + UnityEngine.Random.Range(-1,2), 0,maxWidth-1), currrentPosition.y+1);

       if(roadSegments.Contains(new RoadSegment(currrentPosition, newPosition)))
            return GetNewRoadSegmentPosition(currrentPosition);

        switch ( newPosition.x - currrentPosition.x)
        {
            case -1:
                if(roadSegments.Contains(new RoadSegment(new int2(currrentPosition.x-1, currrentPosition.y), new int2(currrentPosition.x, currrentPosition.y+1))))
                    return GetNewRoadSegmentPosition(currrentPosition);
                
                return newPosition;


            case 1:
                if(roadSegments.Contains(new RoadSegment(new int2(currrentPosition.x+1, currrentPosition.y), new int2(currrentPosition.x, currrentPosition.y+1))))
                    return GetNewRoadSegmentPosition(currrentPosition);
                
                return newPosition;

            case 0:
                return newPosition;
            
            default:
                Debug.LogError("Faulty new position value", this);
                return newPosition;
        }

    }

    struct RoadSegment{

        public RoadSegment(int2 start, int2 end){
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

    void PlaceNodes(){

        foreach (KeyValuePair<int2,NodeInfo> node
         in nodeDictionary)
        {

            NodeInfo nodeInfo = node.Value;

            MapNode nodeType = spawningContainer.GetMapNode(nodeInfo.index);

            if(nodeType) Instantiate(nodeType, nodeInfo.position, Quaternion.identity, transform);
            else Debug.LogError($"no node found for index: {nodeInfo.index}");

        }
    }

    #endregion // Node Placement
}


