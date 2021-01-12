using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
namespace Inner_Maps {
    public class LocationGridTileCollection {
        
        public int id { get; }
        public Dictionary<GridNeighbourDirection, LocationGridTileCollection> neighbours { get; private set; }
        public Vector2Int locationInGrid { get; }
        public LocationGridTile[] tilesInTerritory { get; private set; }
        public InnerMapHexTile partOfHextile { get; private set; }
        public LocationGridTileCollectionItem locationGridTileCollectionItem { get; private set; }
        public Region region { get; private set; }

        public bool isPartOfParentRegionMap => partOfHextile != null;
        
        public LocationGridTileCollection(Vector2Int _locationInGrid, LocationGridTileCollectionItem locationGridTileCollectionItem) {
            id = UtilityScripts.Utilities.SetID(this);
            locationInGrid = _locationInGrid;
            this.locationGridTileCollectionItem = locationGridTileCollectionItem;
        }

        #region Initialization
        public void Initialize(InnerTileMap map) {
            region = map.region;
            DetermineTilesInnTerritory(map);
        }
        public void SetAsPartOfHexTile(InnerMapHexTile tile) {
            partOfHextile = tile;
        }
        #endregion
        
        #region Tiles
        private void DetermineTilesInnTerritory(InnerTileMap tileMap) {
            tilesInTerritory = new LocationGridTile[InnerMapManager.BuildingSpotSize.x * InnerMapManager.BuildingSpotSize.y];
            int radius = Mathf.FloorToInt(InnerMapManager.BuildingSpotSize.x / 2f);
            Vector2 localPosition = locationGridTileCollectionItem.transform.localPosition;
            Vector2Int centeredLocation
                = new Vector2Int(Mathf.FloorToInt(localPosition.x), Mathf.FloorToInt(localPosition.y));  
            Vector2Int startingPos = new Vector2Int(centeredLocation.x - radius, centeredLocation.y - radius);
            Vector2Int endPos = new Vector2Int(centeredLocation.x + radius, centeredLocation.y + radius);
            int tileCount = 0;
            for (int x = startingPos.x; x <= endPos.x; x++) {
                for (int y = startingPos.y; y <= endPos.y; y++) {
                    LocationGridTile tile = tileMap.map[x, y];
                    tile.SetCollectionOwner(this);
                    tilesInTerritory[tileCount] = tile;
                    tileCount++;
                }
            }
        }
        #endregion

        #region Other Data
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
        public void FindNeighbours(InnerTileMap map) {
            if (neighbours != null) {
                throw new Exception($"Tile Collection {id.ToString()} is trying to find neighbours again!");
            }
            //Debug.Log("Finding neighbours for build spot " + id.ToString());
            neighbours = new Dictionary<GridNeighbourDirection, LocationGridTileCollection>();
            int mapUpperBoundX = map.locationGridTileCollections.GetUpperBound(0);
            int mapUpperBoundY = map.locationGridTileCollections.GetUpperBound(1);
            Point thisPoint = new Point(locationInGrid.x, locationInGrid.y);
            foreach (KeyValuePair<GridNeighbourDirection, Point> kvp in possibleExits) {
                GridNeighbourDirection direction = kvp.Key;
                Point exit = kvp.Value;
                Point result = exit.Sum(thisPoint);
                if (UtilityScripts.Utilities.IsInRange(result.X, 0, mapUpperBoundX + 1) &&
                    UtilityScripts.Utilities.IsInRange(result.Y, 0, mapUpperBoundY + 1)) {
                    neighbours.Add(direction, map.locationGridTileCollections[result.X, result.Y]);
                }
            }
        }
        #endregion

        #region Data Getting
        public HexTile GetConnectedHextileOrNearestHextile() {
            if (isPartOfParentRegionMap) {
                return partOfHextile.hexTileOwner;
            }
            return GetFirstTileInNeighbours();
        }
        private HexTile GetFirstTileInNeighbours() {
            foreach (LocationGridTileCollection collection in neighbours.Values) {
                if (collection.isPartOfParentRegionMap) {
                    return collection.partOfHextile.hexTileOwner;
                }
            }
            throw new Exception("Could not find tile collection neighbour that is connected to a hextile! This should never happen since unconnected tiles always have neighbouring hex tiles to its left or right");
        }
        private bool GetHextileInDirection(GridNeighbourDirection p_direction, out HexTile p_tile) {
            if (neighbours.ContainsKey(p_direction)) {
                LocationGridTileCollection gridTileCollection = neighbours[p_direction];
                if (gridTileCollection.isPartOfParentRegionMap) {
                    p_tile = gridTileCollection.partOfHextile.hexTileOwner;
                    return true;
                }
            }
            p_tile = null;
            return false;
        }
        public HexTile GetNearestPlainHexTileWithNoResident() {
            if (isPartOfParentRegionMap) {
                if(partOfHextile.hexTileOwner.elevationType != ELEVATION.WATER && partOfHextile.hexTileOwner.elevationType != ELEVATION.MOUNTAIN) {
                    if (!partOfHextile.hexTileOwner.HasAliveVillagerResident()) {
                        return partOfHextile.hexTileOwner;
                    }
                }
            }
            foreach (LocationGridTileCollection collection in neighbours.Values) {
                if (collection.partOfHextile != null && collection.region == region) {
                    if (collection.partOfHextile.hexTileOwner.elevationType != ELEVATION.WATER && collection.partOfHextile.hexTileOwner.elevationType != ELEVATION.MOUNTAIN) {
                        if (!collection.partOfHextile.hexTileOwner.HasAliveVillagerResident()) {
                            return collection.partOfHextile.hexTileOwner;
                        }
                    }
                }
            }
            foreach (LocationGridTileCollection collection in neighbours.Values) {
                if (collection.region == region) {
                    HexTile nearestHex = collection.GetNearestPlainHexTileWithNoResident();
                    if (nearestHex != null) {
                        return nearestHex;
                    }
                }
            }
            return null;
        }
        #endregion

        #region Pathfinding Updates
        public void UpdatePathfindingGraph() {
            locationGridTileCollectionItem.UpdatePathfindingGraph();
        }
        #endregion

        #region Clean Up
        public void CleanUp() {
            neighbours?.Clear();
            neighbours = null;
            tilesInTerritory = null;
            partOfHextile = null;
            Object.Destroy(locationGridTileCollectionItem);
            locationGridTileCollectionItem = null;
            region = null;
        }
        #endregion
        
    }
}