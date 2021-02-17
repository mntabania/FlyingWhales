using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Inner_Maps;
using UtilityScripts;

public class AreaGridTileComponent : AreaComponent {
    public LocationGridTile centerGridTile { get; private set; }
    public List<LocationGridTile> gridTiles { get; private set; }
    public List<LocationGridTile> borderTiles { get; private set; }
    public bool isCorrupted => gridTiles.Any(t => t.corruptionComponent.isCorrupted);

    public AreaGridTileComponent() {
        gridTiles = new List<LocationGridTile>();
        borderTiles = new List<LocationGridTile>();
    }

    #region Data Setting
    public void SetCenterGridTile(LocationGridTile p_gridTile) {
        centerGridTile = p_gridTile;
    }
    public void AddGridTile(LocationGridTile p_gridTile) {
        gridTiles.Add(p_gridTile);
    }
    public void PopulateBorderTiles(Area p_area) {
        borderTiles.Clear();
        InnerTileMap tileMap = p_area.region.innerMap;
        //To populate border tiles we need to know the width and height of hextile in the inner map, which is currently InnerMapManager.BuildingSpotSize x 2
        int hexWidth = InnerMapManager.AreaLocationGridTileSize.x;
        int hexHeight = InnerMapManager.AreaLocationGridTileSize.y;

        //Our origin point will always be the first entry in the locationGridTiles list, assuming that the first entry is always the lower left corner of the hex tile
        int originX = gridTiles.Min(t => t.localPlace.x);
        int originY = gridTiles.Min(t => t.localPlace.y);

        //Let's get the actual width and height from the origin point
        int actualHeight = originY + (hexHeight - 1);
        int actualWidth = originX + (hexWidth - 1);

        //Now, to calculate border tiles, we will simply add the origin points and the hex width and height and loop through all the tiles corresponding those points
        //There are four sides to the borders since the hex tile in the inner map is a square, we will call it A - left side, B - up side, C - right side, and D - down side
        
        //To get A, we must increment from originY, while the originX is constant
        for (int i = originY; i < actualHeight; i++) {
            borderTiles.Add(tileMap.map[originX, i]);
        }
        //To get B, we must increment from originX, while actualHeight is constant
        for (int i = originX; i < actualWidth; i++) {
            borderTiles.Add(tileMap.map[i, actualHeight]);
        }
        //To get C, we must increment from originY, while actualWidth is constant
        for (int i = originY; i <= actualHeight; i++) {
            borderTiles.Add(tileMap.map[actualWidth, i]);
        }
        //To get D, we must increment from originX, while originY is constant
        for (int i = originX + 1; i < actualWidth; i++) {
            borderTiles.Add(tileMap.map[i, originY]);
        }

        //IMPORTANT NOTE BELOW! DO NOT DELETE COMMENT!
        //Let's check using an example, if the origin point is (0, 0) and the actual width = 7, and the actual height = 7
        //Then A = (0, 0) to (0, 6)
        //B = (0, 7) to (6, 7)
        //C = (7, 0) to (7, 7)
        //D = (1, 0) to (6, 0)

    }
    #endregion
    
    #region Utilities
    public LocationGridTile GetRandomTile() {
        return gridTiles[UnityEngine.Random.Range(0, gridTiles.Count)];
    }
    public LocationGridTile GetRandomTileThatMeetCriteria(Func<LocationGridTile, bool> checker) {
        List<LocationGridTile> tiles = null;
        for (int i = 0; i < gridTiles.Count; i++) {
            LocationGridTile tile = gridTiles[i];
            if (checker.Invoke(tile)) {
                if (tiles == null) { tiles = new List<LocationGridTile>(); }
                tiles.Add(tile);
            }
        }
        if (tiles != null && tiles.Count > 0) {
            return UtilityScripts.CollectionUtilities.GetRandomElement(tiles);
        }
        return null;
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
    public LocationGridTile GetRandomTileThatCharacterCanReach(Character p_character) {
        List<LocationGridTile> tiles = ObjectPoolManager.Instance.CreateNewGridTileList();
        tiles.AddRange(gridTiles);
        tiles.Shuffle();
        LocationGridTile chosenTile = null;
        for (int i = 0; i < tiles.Count; i++) {
            LocationGridTile tile = tiles[i];
            if (p_character.movementComponent.HasPathToEvenIfDiffRegion(tile)) {
                chosenTile = tile;
                break;
            }
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
            if (currTile.structure.isInterior || currTile.corruptionComponent.isCorrupted) {
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
            if (currTile.structure.isInterior || currTile.corruptionComponent.isCorrupted) {
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
            if (currTile.corruptionComponent.isCorrupted) {
                return true;
            }
        }
        return false;
    }
    public void StartCorruption(Area p_area) {
        InstantlyCorruptAllOwnedInnerMapTiles();
        OnCorruptSuccess(p_area);
    }
    public void RemoveCorruption(Area p_area) {
        PlayerManager.Instance.player.playerSettlement.RemoveAreaFromSettlement(p_area);
        for (int i = 0; i < gridTiles.Count; i++) {
            LocationGridTile tile = gridTiles[i];
            tile.corruptionComponent.UnCorruptTile();
        }
    }
    private void InstantlyCorruptAllOwnedInnerMapTiles() {
        for (int i = 0; i < gridTiles.Count; i++) {
            LocationGridTile tile = gridTiles[i];
            tile.corruptionComponent.CorruptTile();
        }
    }
    private void OnCorruptSuccess(Area p_area) {
        PlayerManager.Instance.player.playerSettlement.AddAreaToSettlement(p_area);
        //remove features
        p_area.featureComponent.RemoveAllFeatures(p_area);
    }
    public List<LocationGridTile> GetUnoccupiedTiles() {
        List<LocationGridTile> tiles = null;
        for (int i = 0; i < gridTiles.Count; i++) {
            LocationGridTile tile = gridTiles[i];
            if (tile.objHere == null) {
                if (tiles == null) { tiles = new List<LocationGridTile>(); }
                tiles.Add(tile);
            }
        }
        return tiles;
    }
    #endregion
}