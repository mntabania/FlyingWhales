using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public class InnerPathfindingThread : Multithread {
    public List<LocationGridTile> path { get; private set; }
    public Character character { get; private set; }
    public LocationGridTile startingTile { get; private set; }
    public LocationGridTile destinationTile { get; private set; }
    public GRID_PATHFINDING_MODE pathfindingMode { get; private set; }
    public bool doNotMove { get; private set; }

    public InnerPathfindingThread(Character character, LocationGridTile startingTile, LocationGridTile destinationTile, GRID_PATHFINDING_MODE pathfindingMode) {
        this.character = character;
        this.startingTile = startingTile;
        this.destinationTile = destinationTile;
        this.pathfindingMode = pathfindingMode;
        this.path = null;
    }
    public void SetDoNotMove(bool state) {
        doNotMove = state;
    }
    #region Overrides
    public override void DoMultithread() {
        base.DoMultithread();
        try {
            FindPath();
        } catch (System.Exception e) {
            Debug.LogError(
                $"Problem with {character.name}'s {pathfindingMode} Pathfinding from {startingTile} to {destinationTile}!\n{e.Message}\n{e.StackTrace}");
        }
    }
    public override void FinishMultithread() {
        base.FinishMultithread();
        ReturnPath();
    }
    #endregion
    public void FindPath() {
        path = PathGenerator.Instance.GetPath(startingTile, destinationTile, pathfindingMode);
    }

    public void ReturnPath() {
        //this.character.marker.ReceivePathFromPathfindingThread(this);
    }
}
