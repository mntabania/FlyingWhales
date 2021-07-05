using System;
using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using PathFinding;
using UnityEngine;
using UnityEngine.Profiling;

public class PathfindingManager : BaseMonoBehaviour {

    public static PathfindingManager Instance = null;
    private const float nodeSize = 0.5f; //0.4

    [SerializeField] private AstarPath aStarPath;

    private GridGraph mainGraph;
    private List<CharacterAIPath> _allAgents;
    
    #region getters/setters
    public List<CharacterAIPath> allAgents => _allAgents;
    #endregion

    private void Awake() {
        Instance = this;
        _allAgents = new List<CharacterAIPath>();
    }
    void Start() {
        Messenger.AddListener<bool>(UISignals.PAUSED, OnGamePaused);
    }
    public void RescanGrid(GridGraph graph) {
        AstarPath.active.Scan(graph);
    }
    public void AddAgent(CharacterAIPath agent) {
        _allAgents.Add(agent);
    }
    public void RemoveAgent(CharacterAIPath agent) {
        _allAgents.Remove(agent);
    }
    public void UpdatePathfindingGraphPartialCoroutine(Bounds bounds) {
        StartCoroutine(UpdatePathfindingGraphPartial(bounds));
    }
    private IEnumerator UpdatePathfindingGraphPartial(Bounds bounds) {
        yield return null;
        AstarPath.active.UpdateGraphs(bounds);
    }
    public void UpdatePathfindingGraphPartialCoroutine(GraphUpdateObject guo) {
        StartCoroutine(UpdatePathfindingGraphPartial(guo));
    }
    private IEnumerator UpdatePathfindingGraphPartial(GraphUpdateObject guo) {
        yield return null;
        AstarPath.active.UpdateGraphs(guo);
    }
    public bool HasPath(LocationGridTile fromTile, LocationGridTile toTile) {
        if (fromTile == null || toTile == null) { return false; }
        if (fromTile == toTile) { return true; }
        return PathUtilities.IsPathPossible(AstarPath.active.GetNearest(fromTile.centeredWorldLocation, fromTile.parentMap.onlyUnwalkableGraph).node,
            AstarPath.active.GetNearest(toTile.centeredWorldLocation, toTile.parentMap.onlyUnwalkableGraph).node);
    }
    public bool HasPathEvenDiffRegion(LocationGridTile fromTile, LocationGridTile toTile) {
        if (fromTile == null || toTile == null) { return false; }
        if (fromTile == toTile) { return true; }
        if (fromTile.structure == null) {
            Debug.LogError($"Structure of {fromTile.ToString()} is null");
            return false;
        }
        if (toTile.structure == null) {
            Debug.LogError($"Structure of {toTile.ToString()} is null");
            return false;
        }
        if(fromTile.structure.region == toTile.structure.region) {
            return PathUtilities.IsPathPossible(AstarPath.active.GetNearest(fromTile.centeredWorldLocation, fromTile.parentMap.onlyUnwalkableGraph).node,
                    AstarPath.active.GetNearest(toTile.centeredWorldLocation, toTile.parentMap.onlyUnwalkableGraph).node);
        } else {
            LocationGridTile nearestEdgeFrom = fromTile.GetNearestEdgeTileFromThis();
            if(PathUtilities.IsPathPossible(AstarPath.active.GetNearest(fromTile.centeredWorldLocation, fromTile.parentMap.onlyUnwalkableGraph).node,
                    AstarPath.active.GetNearest(nearestEdgeFrom.centeredWorldLocation, nearestEdgeFrom.parentMap.onlyUnwalkableGraph).node)) {
                LocationGridTile nearestEdgeTo = toTile.GetNearestEdgeTileFromThis();
                return PathUtilities.IsPathPossible(AstarPath.active.GetNearest(toTile.centeredWorldLocation, toTile.parentMap.onlyUnwalkableGraph).node,
                    AstarPath.active.GetNearest(nearestEdgeTo.centeredWorldLocation, nearestEdgeTo.parentMap.onlyUnwalkableGraph).node);
            }
        }
        return false;
    }
    public bool HasPathEvenDiffRegion(LocationGridTile fromTile, LocationGridTile toTile, NNConstraint constraint) {
        if (fromTile == null || toTile == null) { return false; }
        if (fromTile == toTile) { return true; }
        GraphNode fromNode = AstarPath.active.GetNearest(fromTile.centeredWorldLocation, constraint).node;
        GraphNode toNode = AstarPath.active.GetNearest(toTile.centeredWorldLocation, constraint).node;
        if (fromNode == null || toNode == null) { return false; }
        
        if(fromTile.structure.region == toTile.structure.region) {
            return PathUtilities.IsPathPossible(fromNode, toNode);
        } else {
            LocationGridTile nearestEdgeFrom = fromTile.GetNearestEdgeTileFromThis();
            GraphNode nearestEdgeFromNode = AstarPath.active.GetNearest(nearestEdgeFrom.centeredWorldLocation, constraint).node;
            if (nearestEdgeFromNode == null) { return false; }
            if(PathUtilities.IsPathPossible(fromNode, nearestEdgeFromNode)) {
                LocationGridTile nearestEdgeTo = toTile.GetNearestEdgeTileFromThis();
                GraphNode nearestEdgeToNode = AstarPath.active.GetNearest(nearestEdgeTo.centeredWorldLocation, constraint).node;
                if (nearestEdgeToNode == null) { return false; }
                return PathUtilities.IsPathPossible(toNode, nearestEdgeToNode);
            }
        }
        return false;
    }

    #region Map Creation
    public void CreatePathfindingGraphForLocation(InnerTileMap newMap) {
        var pathfindingGraph = CreatePathfindingGraph(newMap, typeof(RuinarchGridGraph), $"{newMap.region.name} Main Map");
        newMap.pathfindingGraph = pathfindingGraph;

        var unwalkableGraph = CreatePathfindingGraph(newMap, typeof(GridGraph), $"{newMap.region.name} UnWalkable Map");
        newMap.unwalkableGraph = unwalkableGraph;
    }
    private GridGraph CreatePathfindingGraph(InnerTileMap newMap, System.Type graphType, string name) {
        GridGraph gg = aStarPath.data.AddGraph(graphType) as GridGraph;
        gg.name = name;
        gg.cutCorners = false;
        gg.rotation = new Vector3(-90f, 0f, 0f);
        gg.nodeSize = nodeSize;

        int mapWidth = newMap.width;
        int mapHeight = newMap.height;

        gg.SetDimensions(Mathf.FloorToInt(mapWidth / gg.nodeSize), Mathf.FloorToInt(mapHeight / gg.nodeSize), nodeSize);
        Vector3 pos = InnerMapManager.Instance.transform.position;
        pos.x += (newMap.width / 2f);
        pos.y += (newMap.height / 2f) + newMap.transform.localPosition.y;

        gg.center = pos;
        gg.collision.use2D = true;
        gg.collision.type = ColliderType.Sphere;
        gg.collision.diameter = 1f; //0.8f - Switched to 1f so that both sides of thin walls are unwalkable
        gg.collision.mask = LayerMask.GetMask("Unpassable");
        return gg;
    }
    #endregion

    private void OnGamePaused(bool state) {
        if (state) {
            for (int i = 0; i < _allAgents.Count; i++) {
                CharacterAIPath currentAI = _allAgents[i];
                currentAI.marker.PauseAnimation();
            }
        } else {
            for (int i = 0; i < _allAgents.Count; i++) {
                CharacterAIPath currentAI = _allAgents[i];
                currentAI.marker.UnpauseAnimation();
            }
        }
    }
    
    #region Monobehaviours
    private void Update() {
        for (int i = 0; i < _allAgents.Count; i++) {
            CharacterAIPath currentAI = _allAgents[i];
#if DEBUG_PROFILER
            Profiler.BeginSample($"{currentAI.marker.character.name} - Pathfinding Update");
#endif
            currentAI.marker.ManualUpdate();
#if DEBUG_PROFILER
            Profiler.EndSample();
#endif
        }
    }
    protected override void OnDestroy() {
        _allAgents.Clear();
        _allAgents = null;
        aStarPath = null;
        base.OnDestroy();
        Instance = null;
    }
#endregion

#region Graph Updates
    public void ApplyGraphUpdateSceneCoroutine(GraphUpdateScene gus) {
        StartCoroutine(UpdateGraph(gus));
    }
    private IEnumerator UpdateGraph(GraphUpdateScene gus) {
        yield return null;
        gus.Apply();
    }
#endregion
}
