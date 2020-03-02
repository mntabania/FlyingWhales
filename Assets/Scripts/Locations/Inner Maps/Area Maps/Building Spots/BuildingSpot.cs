using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;
using Random = UnityEngine.Random;

public class BuildingSpot {

    //data
	public int id { get; }
    public bool isOccupied { get; private set; }
    public Vector3Int location { get; }
    private Vector3 centeredLocation { get; }
    public LocationGridTile[] tilesInTerritory { get; private set; }
    public Vector2Int locationInBuildSpotGrid { get; private set; }
    public Dictionary<GridNeighbourDirection, BuildingSpot> neighbours { get; private set; }
    public HexTile hexTileOwner { get; private set; } //if this is null then it means that this build spot belongs to a hex tile that is not in this build spots region
    public BuildingSpotItem spotItem { get; private set; }

    //Building
    public LocationStructureObject blueprint { get; private set; }
    public STRUCTURE_TYPE blueprintType { get; private set; }

    #region getters
    public bool hasBlueprint => blueprint != null;
    /// <summary>
    /// Is this build spot to be considered as part of it's region map ("part" meaning if this spots tiles are valid)
    /// </summary>
    public bool isPartOfParentRegionMap => hexTileOwner;
    public bool canBeBuiltOnByNPC => isPartOfParentRegionMap && hexTileOwner.isCurrentlyBeingCorrupted == false 
                                    && HasCorruptedTile() == false;
    public bool canBeBuiltOnByPlayer => isPartOfParentRegionMap && hexTileOwner.isCurrentlyBeingCorrupted == false;
    #endregion

    public BuildingSpot(BuildingSpotData data) {
        id = data.id;
        location = data.location;
        locationInBuildSpotGrid = data.buildingSpotGridPos;
        centeredLocation = new Vector3(location.x + 0.5f, location.y + 0.5f);
    }
    public BuildingSpot(Vector3Int tileLocation, Vector2Int locationInBuildSpotGrid) {
        id = UtilityScripts.Utilities.SetID(this);
        location = tileLocation;
        this.locationInBuildSpotGrid = locationInBuildSpotGrid;
        centeredLocation = new Vector3(location.x + 0.5f, location.y + 0.5f);
    }

    public void Initialize(InnerTileMap tileMap) {
        //get the tiles in this spots territory.
        spotItem = spotItem;
        DetermineTilesInnTerritory(tileMap);
    }
    public void SetBuildSpotItem(BuildingSpotItem spotItem) {
        this.spotItem = spotItem;
    }
    private void DetermineTilesInnTerritory(InnerTileMap tileMap) {
        tilesInTerritory = new LocationGridTile[InnerMapManager.BuildingSpotSize.x * InnerMapManager.BuildingSpotSize.y];
        int radius = Mathf.FloorToInt(InnerMapManager.BuildingSpotSize.x / 2f);
        Vector2Int startingPos = new Vector2Int(location.x - radius, location.y - radius);
        Vector2Int endPos = new Vector2Int(location.x + radius, location.y + radius);
        int tileCount = 0;
        for (int x = startingPos.x; x <= endPos.x; x++) {
            for (int y = startingPos.y; y <= endPos.y; y++) {
                LocationGridTile tile = tileMap.map[x, y];
                tilesInTerritory[tileCount] = tile;
                tile.SetBuildSpotOwner(this);
                tileCount++;
            }
        }
    }
    public void FindNeighbours(InnerTileMap map) {
        if (neighbours != null) {
            throw new Exception($"Build spot {id.ToString()} is trying to find neighbours again!");
        }
        //Debug.Log("Finding neighbours for build spot " + id.ToString());
        neighbours = new Dictionary<GridNeighbourDirection, BuildingSpot>();
        int mapUpperBoundX = map.buildingSpots.GetUpperBound(0);
        int mapUpperBoundY = map.buildingSpots.GetUpperBound(1);
        Point thisPoint = new Point(locationInBuildSpotGrid.x, locationInBuildSpotGrid.y);
        foreach (KeyValuePair<GridNeighbourDirection, Point> kvp in possibleExits) {
            GridNeighbourDirection direction = kvp.Key;
            Point exit = kvp.Value;
            Point result = exit.Sum(thisPoint);
            if (UtilityScripts.Utilities.IsInRange(result.X, 0, mapUpperBoundX + 1) &&
                UtilityScripts.Utilities.IsInRange(result.Y, 0, mapUpperBoundY + 1)) {
                neighbours.Add(direction, map.buildingSpots[result.X, result.Y]);
            }
        }
    }
    public HexTile GetNearestHexTile() {
        if(hexTileOwner != null) { return hexTileOwner; }
        //foreach (BuildingSpot buildSpot in neighbours.Values) {
        //    HexTile nearestHex = buildSpot.GetNearestHexTile();
        //    if (nearestHex != null) {
        //        return nearestHex;
        //    }
        //}
        foreach (BuildingSpot buildSpot in neighbours.Values) {
            if(buildSpot.hexTileOwner != null) {
                return buildSpot.hexTileOwner;
            }
        }
        foreach (BuildingSpot buildSpot in neighbours.Values) {
            HexTile nearestHex = buildSpot.GetNearestHexTile();
            if (nearestHex != null) {
                return nearestHex;
            }
        }
        return null;
    }

    #region Data Setting
    public void SetIsOccupied(bool isOccupied) {
        this.isOccupied = isOccupied;
    }
    private List<BuildingSpot> GetNeighbourList() {
        List<BuildingSpot> adjacent = new List<BuildingSpot>();
        if (neighbours == null) {
            throw new Exception($"Building spot { id } has a null neighbours dictionary!");
        }
        foreach (KeyValuePair<GridNeighbourDirection, BuildingSpot> kvp in neighbours) {
            adjacent.Add(kvp.Value);
        }
        return adjacent;
    }
    private Dictionary<GridNeighbourDirection, Point> possibleExits =>
        new Dictionary<GridNeighbourDirection, Point>() {
            {GridNeighbourDirection.North, new Point(0,1) },
            {GridNeighbourDirection.South, new Point(0,-1) },
            {GridNeighbourDirection.West, new Point(-1,0) },
            {GridNeighbourDirection.East, new Point(1,0) },
            {GridNeighbourDirection.North_West, new Point(-1,1) },
            {GridNeighbourDirection.North_East, new Point(1,1) },
            {GridNeighbourDirection.South_West, new Point(-1,-1) },
            {GridNeighbourDirection.South_East, new Point(1,-1) },
        };
    public void SetHexTileOwner(HexTile tile) {
        hexTileOwner = tile;
    }
    #endregion

    #region Checkers
    public void UpdateAdjacentSpotsOccupancy(InnerTileMap map) {
        List<BuildingSpot> adjacent = GetNeighbourList();
        for (int i = 0; i < adjacent.Count; i++) {
            BuildingSpot currSpot = adjacent[i];
            currSpot.CheckIfOccupied();
        }
    }
    private void CheckIfOccupied() {
        bool occupied = false;
        for (int i = 0; i < tilesInTerritory.Length; i++) {
            LocationGridTile currTile = tilesInTerritory[i];
            if (currTile.hasBlueprint || (currTile.structure != null && currTile.structure.structureType != STRUCTURE_TYPE.WILDERNESS && currTile.structure.structureType != STRUCTURE_TYPE.WORK_AREA)) {
                //the spot is now occupied. set that
                occupied = true;
                break;
            }
        }
        if (occupied) {
            SetIsOccupied(true);
            // SetAllAdjacentSpotsAsOpen(map); //set all adjacent unoccupied spots as open
        } else {
            SetIsOccupied(false);
        }
    }
    public bool CanFitStructureOnSpot(LocationStructureObject obj, InnerTileMap map, string builderIdentifier) {
        return map.CanBuildSpotFit(obj, this, builderIdentifier);
    }
    /// <summary>
    /// Is this build spot open for the given settlement.
    /// </summary>
    /// <param name="settlement">The provided settlement</param>
    /// <returns>True or false</returns>
    public bool IsOpenFor(Settlement settlement) {
        if (canBeBuiltOnByNPC == false) {
            return false;
        }
        if (isOccupied) {
            return false;
        }
        if (hasBlueprint) {
            return false;
        }
        //to check if this spot is open for the given settlement
        //    -First check if the hex tile that this spot belongs to is part of the settlement, if it is, then consider this spot as open.
        //    -If the first check fails, then check if this spots hex tile owner is next to any tiles that are part of the given 
        //    settlement, if it is, then consider this spot as open.
        bool open = settlement.tiles.Contains(hexTileOwner);
        if (open == false) {
            for (int i = 0; i < hexTileOwner.AllNeighbours.Count; i++) {
                HexTile neighbour = hexTileOwner.AllNeighbours[i];
                if (settlement.tiles.Contains(neighbour)) {
                    open = true;
                    break;
                }
            }    
        }
        return open;
    }
    private bool HasCorruptedTile() {
        for (int i = 0; i < tilesInTerritory.Length; i++) {
            LocationGridTile tile = tilesInTerritory[i];
            if (tile.isCorrupted) {
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Building
    public void SetBlueprint(LocationStructureObject blueprint, STRUCTURE_TYPE blueprintType) {
        this.blueprint = blueprint;
        this.blueprintType = blueprintType;
    }
    public void ClearBlueprints() {
        blueprint = null;
    }
    public Vector3 GetPositionToPlaceStructure(LocationStructureObject structureObj) {
        if (structureObj.IsHorizontallyBig() && structureObj.IsVerticallyBig()) {
            Vector3 pos = location;
            pos.x += Mathf.Ceil(structureObj.center.x / 2f) + 0.5f;
            pos.y += Mathf.Ceil(structureObj.center.y / 2f) + 0.5f;
            if (structureObj.name.Contains("Portal")) {
                Debug.Log($"Placing portal at {pos.ToString()}. Location of build spot is {location.ToString()}");
            }
            return pos;
        }
        
        int borderSize = InnerMapManager.BuildingSpotBorderSize;

        int maxSizeX = InnerMapManager.BuildingSpotSize.x - (borderSize * 2);
        int maxSizeY = InnerMapManager.BuildingSpotSize.y - (borderSize * 2);
        
        //if the structure objects width or height exceeds the max size, then it's position cannot be randomized in that axis.
        //if the structure can occupy more than 1 spot
        //adjust the maxSize depending on the number of slots it can occupy 
        //i.e (if structure occupies 2 spots horizontally then its max size X) = (Building_Spot_Size.x * 2) - (Building_Spot_Border_Size * 2)
        if (structureObj.IsHorizontallyBig()) {
            maxSizeX = (InnerMapManager.BuildingSpotSize.x * 2) - (borderSize * 2);
        } else if (structureObj.IsVerticallyBig()) {
            maxSizeY = (InnerMapManager.BuildingSpotSize.y * 2) - (borderSize * 2);
        }

        int xPos = 0;
        int yPos = 0;

        if (structureObj.size.x < maxSizeX) {
            //if structure size is less than the max size of the given axis
            //then the structure can be randomly placed by the difference between the max size and the size of the given structure
            //i.e. if max size is 15 and the structure size is 10, then that structure can be randomly placed between 0 - 5 units in the given axis.
            int difference = maxSizeX - structureObj.size.x;
            xPos = Random.Range(0, difference);
        }

        if (structureObj.size.y < maxSizeY) {
            int difference = maxSizeY - structureObj.size.y;
            yPos = Random.Range(0, difference);
        }

        Vector3 randomPos = new Vector3(xPos, yPos, 0);
        randomPos += centeredLocation;

        return randomPos;
    }
    public bool CanBeBuiltOnBy(string builderIdentifier) {
        if (builderIdentifier == "NPC") {
            return canBeBuiltOnByNPC;
        } else if (builderIdentifier == "Player") {
            return canBeBuiltOnByPlayer;
        }
        return false;
    }
    #endregion

}
