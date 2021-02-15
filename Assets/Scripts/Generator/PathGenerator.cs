using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using JetBrains.Annotations;

public class PathGenerator : MonoBehaviour {

	public static PathGenerator Instance = null;

    void Awake(){
		Instance = this;
	}
    public List<LocationGridTile> GetPath(LocationGridTile startingTile, LocationGridTile destinationTile, GRID_PATHFINDING_MODE pathMode = GRID_PATHFINDING_MODE.NORMAL, bool includeFirstTile = false) {
        List<LocationGridTile> path = null;
        
        //normal pathfinding logic
        Func<LocationGridTile, LocationGridTile, double> distance = (node1, node2) => 1;
        Func<LocationGridTile, double> estimate = t => Math.Sqrt(Math.Pow(t.localPlace.x - destinationTile.localPlace.x, 2) + Math.Pow(t.localPlace.y - destinationTile.localPlace.y, 2));
        var p = PathFind.PathFind.FindPath(startingTile, destinationTile, distance, estimate, pathMode);
        if (p != null) {
            path = p.ToList();
        }
            
        if (path != null) {
            path.Reverse();
            if (!includeFirstTile) {
                path.RemoveAt(0);
            }
            return path;
        }
        return null;
    }

    #region Tile Getters
    private List<LocationGridTile> SameStructureTiles(LocationGridTile tile, params object[] args) {
        LocationStructure structure = args[0] as LocationStructure;
        LocationGridTile startingTile = args[1] as LocationGridTile;
        LocationGridTile destinationTile = args[2] as LocationGridTile;

        List<LocationGridTile> tiles = new List<LocationGridTile>();
        List<LocationGridTile> neighbours = tile.FourNeighbours();
        for (int i = 0; i < neighbours.Count; i++) {
            LocationGridTile currTile = neighbours[i];
            //if (currTile == startingTile || currTile == destinationTile || (currTile.tileAccess == LocationGridTile.Tile_Access.Passable && currTile.structure == structure)) {
            //    tiles.Add(currTile);
            //}
        }
        return tiles;
    }
    private List<LocationGridTile> AllowedStructureTiles(LocationGridTile tile, params object[] args) {
        List<STRUCTURE_TYPE> allowedTypes = args[0] as List<STRUCTURE_TYPE>;
        LocationGridTile startingTile = args[1] as LocationGridTile;
        LocationGridTile destinationTile = args[2] as LocationGridTile;

        List<LocationGridTile> tiles = new List<LocationGridTile>();
        List<LocationGridTile> neighbours = tile.FourNeighbours();
        for (int i = 0; i < neighbours.Count; i++) {
            LocationGridTile currTile = neighbours[i];
            //if (currTile == startingTile || currTile == destinationTile || (currTile.tileAccess == LocationGridTile.Tile_Access.Passable && currTile.structure != null && allowedTypes.Contains(currTile.structure.structureType))) {
            //    tiles.Add(currTile);
            //}
        }
        return tiles;
    }
    #endregion
}
