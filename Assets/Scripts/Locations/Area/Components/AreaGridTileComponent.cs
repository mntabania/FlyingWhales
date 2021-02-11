﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Inner_Maps;
using UtilityScripts;

public class AreaGridTileComponent : AreaComponent {
    public LocationGridTile centerGridTile { get; private set; }
    public List<LocationGridTile> gridTiles { get; private set; }
    public List<LocationGridTile> borderTiles { get; private set; }

    public AreaGridTileComponent() {
        gridTiles = new List<LocationGridTile>();
        borderTiles = new List<LocationGridTile>();
    }

    #region Utilities
    public void SetCenterGridTile(LocationGridTile p_gridTile) {
        centerGridTile = p_gridTile;
    }
    public LocationGridTile GetRandomTile() {
        return gridTiles[UnityEngine.Random.Range(0, gridTiles.Count)];
    }
    public LocationGridTile GetRandomPassableTile() {
        LocationGridTile chosenTile = null;
        List<LocationGridTile> passableTiles = ObjectPoolManager.Instance.CreateNewGridTileList();
        for (int i = 0; i < gridTiles.Count; i++) {
            LocationGridTile tile = gridTiles[i];
            if (tile.IsPassable()) {
                passableTiles.Add(tile);
            }
        }
        if (passableTiles != null && passableTiles.Count > 0) {
            chosenTile = CollectionUtilities.GetRandomElement(passableTiles);
        }
        ObjectPoolManager.Instance.ReturnGridTileListToPool(passableTiles);
        return chosenTile;
    }
    public LocationGridTile GetRandomUnoccupiedTile() {
        List<LocationGridTile> tiles = ObjectPoolManager.Instance.CreateNewGridTileList();
        for (int i = 0; i < gridTiles.Count; i++) {
            LocationGridTile tile = gridTiles[i];
            if (tile.objHere == null) {
                tiles.Add(tile);
            }
        }
        LocationGridTile chosenTile = null;
        if (tiles != null && tiles.Count > 0) {
            chosenTile = UtilityScripts.CollectionUtilities.GetRandomElement(tiles);
        }
        ObjectPoolManager.Instance.ReturnGridTileListToPool(tiles);
        return chosenTile;
    }
    public LocationGridTile GetRandomPassableInOpenSpaceTile() {
        List<LocationGridTile> tiles = ObjectPoolManager.Instance.CreateNewGridTileList();
        for (int i = 0; i < gridTiles.Count; i++) {
            LocationGridTile tile = gridTiles[i];
            if (tile.IsPassable() && tile.structure.structureType.IsOpenSpace()) {
                tiles.Add(tile);
            }
        }
        LocationGridTile chosenTile = null;
        if (tiles != null && tiles.Count > 0) {
            chosenTile = UtilityScripts.CollectionUtilities.GetRandomElement(tiles);
        }
        ObjectPoolManager.Instance.ReturnGridTileListToPool(tiles);
        return chosenTile;
    }
    public LocationGridTile GetRandomUnoccupiedNoFreezingTrapNotNextToSettlementTile() {
        List<LocationGridTile> tiles = ObjectPoolManager.Instance.CreateNewGridTileList();
        for (int i = 0; i < gridTiles.Count; i++) {
            LocationGridTile tile = gridTiles[i];
            if (tile.hasFreezingTrap == false && tile.isOccupied == false && tile.IsNextToSettlement() == false) {
                tiles.Add(tile);
            }
        }
        LocationGridTile chosenTile = null;
        if (tiles != null && tiles.Count > 0) {
            chosenTile = UtilityScripts.CollectionUtilities.GetRandomElement(tiles);
        }
        ObjectPoolManager.Instance.ReturnGridTileListToPool(tiles);
        return chosenTile;
    }
    public LocationGridTile GetRandomPassableUnoccupiedNonWaterTile() {
        List<LocationGridTile> tiles = ObjectPoolManager.Instance.CreateNewGridTileList();
        for (int i = 0; i < gridTiles.Count; i++) {
            LocationGridTile tile = gridTiles[i];
            if (tile.objHere == null && tile.groundType != LocationGridTile.Ground_Type.Water && tile.IsPassable()) {
                tiles.Add(tile);
            }
        }
        LocationGridTile chosenTile = null;
        if (tiles != null && tiles.Count > 0) {
            chosenTile = UtilityScripts.CollectionUtilities.GetRandomElement(tiles);
        }
        ObjectPoolManager.Instance.ReturnGridTileListToPool(tiles);
        return chosenTile;
    }
    public void PopulateUnoccupiedTiles(List<LocationGridTile> tiles) {
        for (int i = 0; i < gridTiles.Count; i++) {
            LocationGridTile tile = gridTiles[i];
            if (tile.objHere == null) {
                tiles.Add(tile);
            }
        }
    }
    public void ChangeGridTilesBiome() {
        for (int i = 0; i < gridTiles.Count; i++) {
            LocationGridTile currTile = gridTiles[i];
            Vector3Int position = currTile.localPlace;
            TileBase groundTile = InnerTileMap.GetGroundAssetPerlin(currTile.floorSample, owner.areaData.biomeType);
            if (currTile.structure.isInterior || currTile.isCorrupted) {
                //set the previous tile to the new biome, so that when the structure is destroyed
                //it will revert to the right asset
            } else {
                currTile.parentMap.groundTilemap.SetTile(position, groundTile);
                currTile.UpdateGroundTypeBasedOnAsset();
                if (currTile.objHere != null && currTile.objHere.mapObjectVisual && currTile.objHere is TileObject tileObject) {
                    tileObject.mapVisual.UpdateTileObjectVisual(tileObject);
                }
                currTile.CreateSeamlessEdgesForSelfAndNeighbours();
            }
        }
    }
    public IEnumerator ChangeGridTilesBiomeCoroutine(System.Action onFinishChangeAction) {
        // List<LocationGridTile> gridTiles = new List<LocationGridTile>(locationGridTiles);
        // gridTiles = UtilityScripts.CollectionUtilities.Shuffle(gridTiles);
        for (int i = 0; i < gridTiles.Count; i++) {
            LocationGridTile currTile = gridTiles[i];
            Vector3Int position = currTile.localPlace;
            TileBase groundTile = InnerTileMap.GetGroundAssetPerlin(currTile.floorSample, owner.areaData.biomeType);
            if (currTile.structure.isInterior || currTile.isCorrupted) {
                //do not change tiles of interior or corrupted structures.
                continue;
            }

            currTile.parentMap.groundTilemap.SetTile(position, groundTile);
            currTile.UpdateGroundTypeBasedOnAsset();
            if (currTile.objHere != null && currTile.objHere.mapObjectVisual && currTile.objHere is TileObject tileObject) {
                tileObject.mapVisual.UpdateTileObjectVisual(tileObject);
            }
            currTile.CreateSeamlessEdgesForSelfAndNeighbours();
            yield return null;
        }
        onFinishChangeAction.Invoke();
    }
    public bool HasCorruption() {
        for (int i = 0; i < gridTiles.Count; i++) {
            LocationGridTile currTile = gridTiles[i];
            if (currTile.isCorrupted) {
                return true;
            }
        }
        return false;
    }
    #endregion
}