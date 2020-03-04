using System;
using System.Collections.Generic;
using System.Linq;
using BayatGames.SaveGameFree.Types;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using PathFind;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UtilityScripts;
using Random = UnityEngine.Random;
namespace Inner_Maps {
    public class LocationGridTile : IHasNeighbours<LocationGridTile> {

        public enum Tile_Type { Empty, Wall, Structure_Entrance }
        public enum Tile_State { Empty, Occupied }
        public enum Ground_Type { Soil, Grass, Stone, Snow, Tundra, Cobble, Wood, Snow_Dirt, Water, Cave, Corrupted, 
            Desert_Grass, Sand, Desert_Stone, Bone, Demon_Stone, Flesh
        }
        public bool hasDetail { get; set; }
        public InnerTileMap parentMap { get; private set; }
        public Tilemap parentTileMap { get; private set; }
        public Vector3Int localPlace { get; private set; }
        public Vector3 worldLocation { get; private set; }
        public Vector3 centeredWorldLocation { get; private set; }
        public Vector3 localLocation { get; private set; }
        public Vector3 centeredLocalLocation { get; private set; }
        public Tile_Type tileType { get; private set; }
        public Tile_State tileState { get; private set; }
        public Ground_Type groundType { get; private set; }
        public LocationStructure structure { get; private set; }
        private Dictionary<GridNeighbourDirection, LocationGridTile> neighbours { get; set; }
        private Dictionary<GridNeighbourDirection, LocationGridTile> fourNeighbours { get; set; }
        public List<LocationGridTile> neighbourList { get; private set; }
        public IPointOfInterest objHere { get; private set; }
        public List<Character> charactersHere { get; private set; }
        public bool isOccupied => tileState == Tile_State.Occupied;
        public bool isLocked { get; private set; } //if a tile is locked, any asset on it should not be replaced.
        public TILE_OBJECT_TYPE reservedObjectType { get; private set; } //the only type of tile object that can be placed here
        public FurnitureSpot furnitureSpot { get; private set; }
        public bool hasFurnitureSpot { get; private set; }
        public List<Trait> normalTraits => genericTileObject.traitContainer.allTraits;
        public bool hasBlueprint { get; private set; }

        private Color defaultTileColor;

        public List<LocationGridTile> ValidTiles { get { return FourNeighbours().Where(o => o.tileType == Tile_Type.Empty).ToList(); } }
        public List<LocationGridTile> UnoccupiedNeighbours { get { return neighbours.Values.Where(o => !o.isOccupied && o.structure == structure).ToList(); } }
        public List<LocationGridTile> UnoccupiedNeighboursWithinHex { get { return neighbours.Values.Where(o => !o.isOccupied && o.charactersHere.Count <= 0 && o.structure == structure && o.buildSpotOwner.hexTileOwner == buildSpotOwner.hexTileOwner).ToList(); } }

        public GenericTileObject genericTileObject { get; private set; }
        public List<StructureWallObject> walls { get; private set; }
        public bool isCorrupted => groundType == Ground_Type.Corrupted;
        
        public LocationGridTile(int x, int y, Tilemap tilemap, InnerTileMap parentMap) {
            this.parentMap = parentMap;
            parentTileMap = tilemap;
            localPlace = new Vector3Int(x, y, 0);
            worldLocation = tilemap.CellToWorld(localPlace);
            localLocation = tilemap.CellToLocal(localPlace);
            centeredLocalLocation = new Vector3(localLocation.x + 0.5f, localLocation.y + 0.5f, localLocation.z);
            centeredWorldLocation = new Vector3(worldLocation.x + 0.5f, worldLocation.y + 0.5f, worldLocation.z);
            tileType = Tile_Type.Empty;
            tileState = Tile_State.Empty;
            charactersHere = new List<Character>();
            walls = new List<StructureWallObject>();
            SetLockedState(false);
            SetReservedType(TILE_OBJECT_TYPE.NONE);
            defaultTileColor = Color.white;
        }
        public LocationGridTile(SaveDataLocationGridTile data, Tilemap tilemap, InnerTileMap parentMap) {
            this.parentMap = parentMap;
            parentTileMap = tilemap;
            localPlace = new Vector3Int((int)data.localPlace.x, (int)data.localPlace.y, 0);
            worldLocation = data.worldLocation;
            localLocation = data.localLocation;
            centeredLocalLocation = data.centeredLocalLocation;
            centeredWorldLocation = data.centeredWorldLocation;
            tileType = data.tileType;
            tileState = data.tileState;
            SetLockedState(data.isLocked);
            SetReservedType(data.reservedObjectType);
            charactersHere = new List<Character>();
            walls = new List<StructureWallObject>();
            defaultTileColor = Color.white;
        }

        public void CreateGenericTileObject() {
            genericTileObject = new GenericTileObject();
        }
        public void UpdateWorldLocation() {
            worldLocation = parentTileMap.CellToWorld(localPlace);
            centeredWorldLocation = new Vector3(worldLocation.x + 0.5f, worldLocation.y + 0.5f, worldLocation.z);
        }
        public List<LocationGridTile> FourNeighbours() {
            List<LocationGridTile> fn = new List<LocationGridTile>();
            foreach (KeyValuePair<GridNeighbourDirection, LocationGridTile> keyValuePair in fourNeighbours) {
                fn.Add(keyValuePair.Value);
            }
            return fn;
        }
        private Dictionary<GridNeighbourDirection, LocationGridTile> FourNeighboursDictionary() { return fourNeighbours; }
        public void FindNeighbours(LocationGridTile[,] map) {
            fourNeighbours = new Dictionary<GridNeighbourDirection, LocationGridTile>();
            neighbours = new Dictionary<GridNeighbourDirection, LocationGridTile>();
            neighbourList = new List<LocationGridTile>();
            int mapUpperBoundX = map.GetUpperBound(0);
            int mapUpperBoundY = map.GetUpperBound(1);
            Point thisPoint = new Point(localPlace.x, localPlace.y);
            foreach (KeyValuePair<GridNeighbourDirection, Point> kvp in possibleExits) {
                GridNeighbourDirection currDir = kvp.Key;
                Point exit = kvp.Value;
                Point result = exit.Sum(thisPoint);
                if (UtilityScripts.Utilities.IsInRange(result.X, 0, mapUpperBoundX + 1) &&
                    UtilityScripts.Utilities.IsInRange(result.Y, 0, mapUpperBoundY + 1)) {
                    LocationGridTile tile = map[result.X, result.Y];
                    neighbours.Add(currDir, tile);
                    neighbourList.Add(tile);
                    if (currDir.IsCardinalDirection()) {
                        fourNeighbours.Add(currDir, tile);
                    }
                }

            }
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
        public void SetTileType(Tile_Type tileType) {
            this.tileType = tileType;
        }
        private void SetGroundType(Ground_Type groundType) {
            this.groundType = groundType;
            if (genericTileObject != null) {
                switch (groundType) {
                    case Ground_Type.Grass:
                    case Ground_Type.Wood:
                    case Ground_Type.Sand:
                    case Ground_Type.Desert_Grass:
                    case Ground_Type.Soil:
                        genericTileObject.traitContainer.AddTrait(genericTileObject, "Flammable");
                        break;
                    default:
                        genericTileObject.traitContainer.RemoveTrait(genericTileObject, "Flammable");
                        break;
                }
            }
        }
        public void UpdateGroundTypeBasedOnAsset() {
            Sprite groundAsset = parentMap.groundTilemap.GetSprite(localPlace);
            Sprite structureAsset = parentMap.structureTilemap.GetSprite(localPlace);
            if (ReferenceEquals(structureAsset, null) == false) {
                string assetName = structureAsset.name.ToLower();
                if (assetName.Contains("dungeon") || assetName.Contains("cave") || assetName.Contains("laid")) {
                    SetGroundType(Ground_Type.Cave);
                } else if (assetName.Contains("water") || assetName.Contains("pond") || assetName.Contains("shore")) {
                    SetGroundType(Ground_Type.Water);
                } 
            } else if (ReferenceEquals(groundAsset, null) == false) {
                string assetName = groundAsset.name.ToLower();
                if (assetName.Contains("desert")) {
                    if (assetName.Contains("grass")) {
                        SetGroundType(Ground_Type.Desert_Grass);
                    } else if (assetName.Contains("sand")) {
                        SetGroundType(Ground_Type.Sand);
                    } else if (assetName.Contains("rocks")) {
                        SetGroundType(Ground_Type.Desert_Stone);
                    }
                } else if (assetName.Contains("corruption") || assetName.Contains("corrupted")) {
                    SetGroundType(Ground_Type.Corrupted);
                } else if (assetName.Contains("bone")) {
                    SetGroundType(Ground_Type.Bone);
                } else if (assetName.Contains("structure floor") || assetName.Contains("wood")) {
                    SetGroundType(Ground_Type.Wood);
                } else if (assetName.Contains("cobble")) {
                    SetGroundType(Ground_Type.Cobble);
                } else if (assetName.Contains("water") || assetName.Contains("pond")) {
                    SetGroundType(Ground_Type.Water);
                } else if (assetName.Contains("dirt") || assetName.Contains("soil") || assetName.Contains("outside") || assetName.Contains("snow")) {
                    if (parentMap.location.coreTile.biomeType == BIOMES.SNOW || parentMap.location.coreTile.biomeType == BIOMES.TUNDRA) {
                        if (assetName.Contains("dirtsnow")) {
                            SetGroundType(Ground_Type.Snow_Dirt);
                        } else if (assetName.Contains("snow")) {
                            SetGroundType(Ground_Type.Snow);
                        } else {
                            SetGroundType(Ground_Type.Tundra);
                            //override tile to use tundra soil
                            parentMap.groundTilemap.SetTile(localPlace, InnerMapManager.Instance.assetManager.tundraTile);    
                        }
                    } else if (parentMap.location.coreTile.biomeType == BIOMES.DESERT) {
                        if (structure != null && (structure.structureType == STRUCTURE_TYPE.CAVE || structure.structureType == STRUCTURE_TYPE.MONSTER_LAIR)) {
                            SetGroundType(Ground_Type.Stone);
                            //override tile to use stone
                            parentMap.groundTilemap.SetTile(localPlace, InnerMapManager.Instance.assetManager.stoneTile);    
                        } else {
                            SetGroundType(Ground_Type.Sand);
                            //override tile to use sand
                            parentMap.groundTilemap.SetTile(localPlace, InnerMapManager.Instance.assetManager.desertSandTile);
                        }
                        
                    } else {
                        SetGroundType(Ground_Type.Soil);
                    }
                } else if (assetName.Contains("stone") || assetName.Contains("road")) {
                    if (assetName.Contains("demon")) {
                        SetGroundType(Ground_Type.Demon_Stone);   
                    } else {
                        SetGroundType(Ground_Type.Stone);    
                    }
                } else if (assetName.Contains("grass")) {
                    SetGroundType(Ground_Type.Grass);
                } else if (assetName.Contains("tundra")) {
                    SetGroundType(Ground_Type.Tundra);
                    //override tile to use tundra soil
                    parentMap.groundTilemap.SetTile(localPlace, InnerMapManager.Instance.assetManager.tundraTile);
                } else if (assetName.Contains("flesh")) {
                    SetGroundType(Ground_Type.Flesh);
                }
            }
        }
        public bool TryGetNeighbourDirection(LocationGridTile tile, out GridNeighbourDirection dir) {
            foreach (KeyValuePair<GridNeighbourDirection, LocationGridTile> keyValuePair in neighbours) {
                if (keyValuePair.Value == tile) {
                    dir = keyValuePair.Key;
                    return true;
                }
            }
            dir = GridNeighbourDirection.East;
            return false;
        }

        #region Visuals
        public TileBase previousGroundVisual { get; private set; }
        public void SetGroundTilemapVisual(TileBase tileBase) {
            SetPreviousGroundVisual(parentMap.groundTilemap.GetTile(localPlace));
            parentMap.groundTilemap.SetTile(localPlace, tileBase);
            UpdateGroundTypeBasedOnAsset();
        }
        public void SetStructureTilemapVisual(TileBase tileBase) {
            parentMap.structureTilemap.SetTile(localPlace, tileBase);
            UpdateGroundTypeBasedOnAsset();
        }
        public void SetPreviousGroundVisual(TileBase tileBase) {
            previousGroundVisual = tileBase;
        }
        public void RevertToPreviousGroundVisual() {
            if (ReferenceEquals(previousGroundVisual, null) == false) {
                SetGroundTilemapVisual(previousGroundVisual);
            }
        }
        public void CreateSeamlessEdgesForSelfAndNeighbours() {
            CreateSeamlessEdgesForTile(parentMap);
            for (int i = 0; i < neighbourList.Count; i++) {
                LocationGridTile neighbour = neighbourList[i];
                neighbour.CreateSeamlessEdgesForTile(parentMap);
            }
        }
        public void CreateSeamlessEdgesForTile(InnerTileMap map) {
            // string summary = $"Creating seamless edges for tile {ToString()}";
            Dictionary<GridNeighbourDirection, LocationGridTile> neighbours;
            if (HasCardinalNeighbourOfDifferentGroundType(out neighbours)) {
                // summary += $"\nHas Neighbour of different ground type. Checking neighbours {neighbours.Count.ToString()}";
                Dictionary<GridNeighbourDirection, LocationGridTile> fourNeighbours = FourNeighboursDictionary();
                foreach (KeyValuePair<GridNeighbourDirection, LocationGridTile> keyValuePair in fourNeighbours) {
                    LocationGridTile currNeighbour = keyValuePair.Value;
                    bool createEdge = false;
                    // summary += $"\n\tChecking {currNeighbour.ToString()}. Ground type is {groundType.ToString()}. Neighbour Ground Type is {currNeighbour.groundType.ToString()}";
                    if (this.groundType != Ground_Type.Cave && currNeighbour.groundType == Ground_Type.Cave) {
                        createEdge = true;
                    } else if (currNeighbour.tileType == Tile_Type.Wall || currNeighbour.tileType == Tile_Type.Structure_Entrance) {
                        createEdge = false;
                    } else if (groundType != Ground_Type.Water && currNeighbour.groundType == Ground_Type.Water) {
                        createEdge = true;
                    } else if (groundType == Ground_Type.Corrupted && currNeighbour.groundType != Ground_Type.Bone) {
                        createEdge = true;
                    } else if (groundType == Ground_Type.Demon_Stone && currNeighbour.groundType != Ground_Type.Corrupted) {
                        createEdge = true;
                    } else if (groundType == Ground_Type.Bone) {
                        createEdge = true;
                    } else if (currNeighbour.groundType == Ground_Type.Bone) {
                        createEdge = false;
                    } else if (groundType != Ground_Type.Corrupted && currNeighbour.groundType == Ground_Type.Corrupted) {
                        createEdge = false;
                    } else if (groundType == Ground_Type.Snow && currNeighbour.groundType != Ground_Type.Snow) {
                        createEdge = true;
                    } else if (groundType == Ground_Type.Cobble && currNeighbour.groundType != Ground_Type.Snow) {
                        createEdge = true;
                    } else if ((groundType == Ground_Type.Tundra || groundType == Ground_Type.Snow_Dirt) &&
                               (currNeighbour.groundType == Ground_Type.Stone || currNeighbour.groundType == Ground_Type.Soil)) {
                        createEdge = true;
                    } else if (groundType == Ground_Type.Grass && currNeighbour.groundType == Ground_Type.Soil) {
                        createEdge = true;
                    } else if (groundType == Ground_Type.Soil && currNeighbour.groundType == Ground_Type.Stone) {
                        createEdge = true;
                    } else if (groundType == Ground_Type.Stone && currNeighbour.groundType == Ground_Type.Grass) {
                        createEdge = true;
                    } else if (groundType == Ground_Type.Desert_Grass &&
                               (currNeighbour.groundType == Ground_Type.Desert_Stone || currNeighbour.groundType == Ground_Type.Sand)) {
                        createEdge = true;
                    } else if (groundType == Ground_Type.Sand && currNeighbour.groundType == Ground_Type.Desert_Stone) {
                        createEdge = true;
                    } else if (groundType == Ground_Type.Sand && currNeighbour.groundType == Ground_Type.Stone) {
                        createEdge = true;
                    }
                    // summary += $"\n\tWill create edge? {createEdge.ToString()}. At {keyValuePair.Key.ToString()}";
                    Tilemap mapToUse;
                    switch (keyValuePair.Key) {
                        case GridNeighbourDirection.North:
                            mapToUse = map.northEdgeTilemap;
                            break;
                        case GridNeighbourDirection.South:
                            mapToUse = map.southEdgeTilemap;
                            break;
                        case GridNeighbourDirection.West:
                            mapToUse = map.westEdgeTilemap;
                            break;
                        case GridNeighbourDirection.East:
                            mapToUse = map.eastEdgeTilemap;
                            break;
                        default:
                            mapToUse = null;
                            break;
                    }
                    Assert.IsNotNull(mapToUse, $"{nameof(mapToUse)} != null");
                    if (createEdge) {
                        Assert.IsTrue(InnerMapManager.Instance.assetManager.edgeAssets.ContainsKey(groundType), 
                            $"No edge asset for {groundType.ToString()} for neighbour {currNeighbour.groundType.ToString()} ");
                        Assert.IsTrue(InnerMapManager.Instance.assetManager.edgeAssets[groundType].Count > (int)keyValuePair.Key, 
                            $"No edge asset for {groundType.ToString()} for neighbour {currNeighbour.groundType.ToString()} for direction {keyValuePair.Key.ToString()} ");
                        mapToUse.SetTile(localPlace, InnerMapManager.Instance.assetManager.edgeAssets[groundType][(int)keyValuePair.Key]);    
                    } else {
                        mapToUse.SetTile(localPlace, null);
                    }
                }
                // Debug.Log(summary);    
            }
        }
        #endregion

        #region Structures
        public void SetStructure(LocationStructure structure) {
            this.structure?.RemoveTile(this);
            this.structure = structure;
            this.structure.AddTile(this);
            genericTileObject.ManualInitialize(this);
        }
        public void SetTileState(Tile_State state) {
            if (structure != null) {
                if (tileState == Tile_State.Empty && state == Tile_State.Occupied) {
                    structure.RemoveUnoccupiedTile(this);
                } else if (tileState == Tile_State.Occupied && state == Tile_State.Empty && reservedObjectType == TILE_OBJECT_TYPE.NONE) {
                    structure.AddUnoccupiedTile(this);
                }
            }
            tileState = state;
        }
        #endregion

        #region Characters
        public void AddCharacterHere(Character character) {
            // if (!charactersHere.Contains(character)) {
                charactersHere.Add(character);
            // }
            if(genericTileObject != null) {
                for (int i = 0; i < genericTileObject.traitContainer.onEnterGridTileTraits.Count; i++) {
                    genericTileObject.traitContainer.onEnterGridTileTraits[i].OnEnterGridTile(character, genericTileObject);
                }
            }
        }
        public void RemoveCharacterHere(Character character) {
            charactersHere.Remove(character);
        }
        #endregion

        #region Points of Interest
        public void SetObjectHere(IPointOfInterest poi) {
            objHere = poi;
            poi.SetGridTileLocation(this);
            poi.OnPlacePOI();
            SetTileState(Tile_State.Occupied);
            Messenger.Broadcast(Signals.OBJECT_PLACED_ON_TILE, this, poi);
        }
        public IPointOfInterest RemoveObjectHere(Character removedBy) {
            if (objHere != null) {
                IPointOfInterest removedObj = objHere;
                if (objHere is TileObject && removedBy != null) {
                    //if the object in this tile is a tile object and it was removed by a character, use tile object specific function
                    (objHere as TileObject).RemoveTileObject(removedBy);
                } else {
                    objHere.SetGridTileLocation(null);
                    objHere.OnDestroyPOI();
                }
                objHere = null;
                SetTileState(Tile_State.Empty);
                Messenger.Broadcast(Signals.STOP_CURRENT_ACTION_TARGETING_POI, removedObj);
                return removedObj;
            }
            return null;
        }
        public IPointOfInterest RemoveObjectHereWithoutDestroying() {
            if (objHere != null) {
                IPointOfInterest removedObj = objHere;
                LocationGridTile gridTile = objHere.gridTileLocation;
                objHere.SetGridTileLocation(null);
                objHere = null;
                SetTileState(Tile_State.Empty);
                if (removedObj.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
                    (removedObj as TileObject).OnRemoveTileObject(null, gridTile, false, false);
                }
                removedObj.SetPOIState(POI_STATE.INACTIVE);
                return removedObj;
            }
            return null;
        }
        public IPointOfInterest RemoveObjectHereDestroyVisualOnly(Character remover = null) {
            if (objHere != null) {
                IPointOfInterest removedObj = objHere;
                LocationGridTile gridTile = objHere.gridTileLocation;
                objHere.SetGridTileLocation(null);
                objHere = null;
                SetTileState(Tile_State.Empty);
                if (removedObj.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
                    TileObject removedTileObj = removedObj as TileObject;
                    removedTileObj.OnRemoveTileObject(null, gridTile, false, false);
                    removedTileObj.DestroyMapVisualGameObject();
                }
                removedObj.SetPOIState(POI_STATE.INACTIVE);
                Messenger.Broadcast(Signals.STOP_CURRENT_ACTION_TARGETING_POI_EXCEPT_ACTOR, removedObj, remover);
                return removedObj;
            }
            return null;
        }
        #endregion

        #region Utilities
        public bool IsAtEdgeOfMap() {
            GridNeighbourDirection[] dirs = CollectionUtilities.GetEnumValues<GridNeighbourDirection>();
            for (int i = 0; i < dirs.Length; i++) {
                if (!neighbours.ContainsKey(dirs[i])) {
                    return true;
                }
            }
            return false;
        }
        public bool HasNeighborAtEdgeOfMap() {
            foreach (KeyValuePair<GridNeighbourDirection, LocationGridTile> kvp in neighbours) {
                if (kvp.Value.IsAtEdgeOfMap()) {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Does this tile have a neighbour that is part of a different structure, or is part of the outside map?
        /// </summary>
        public bool HasDifferentDwellingOrOutsideNeighbour() {
            foreach (KeyValuePair<GridNeighbourDirection, LocationGridTile> kvp in neighbours) {
                if (kvp.Value.structure != structure) {
                    return true;
                }
            }
            return false;
        }
        public override string ToString() {
            return localPlace.ToString();
        }
        public float GetDistanceTo(LocationGridTile tile) {
            return Vector2.Distance(localLocation, tile.localLocation);
        }
        public bool HasOccupiedNeighbour() {
            for (int i = 0; i < neighbours.Values.Count; i++) {
                if (neighbours.Values.ElementAt(i).isOccupied) {
                    return true;
                }
            }
            return false;
        }
        public bool HasNeighbourOfElevation(ELEVATION elevation, bool useFourNeighbours = false) {
            Dictionary<GridNeighbourDirection, LocationGridTile> n = neighbours;
            if (useFourNeighbours) {
                n = FourNeighboursDictionary();
            }
            for (int i = 0; i < n.Values.Count; i++) {
                if (neighbours.Values.ElementAt(i).buildSpotOwner.hexTileOwner &&
                    neighbours.Values.ElementAt(i).buildSpotOwner.hexTileOwner.elevationType == elevation) {
                    return true;
                }
            }
            return false;
        }
        public bool HasNeighbourOfType(Tile_Type type, bool useFourNeighbours = false) {
            Dictionary<GridNeighbourDirection, LocationGridTile> n = neighbours;
            if (useFourNeighbours) {
                n = FourNeighboursDictionary();
            }
            for (int i = 0; i < n.Values.Count; i++) {
                if (neighbours.Values.ElementAt(i).tileType == type) {
                    return true;
                }
            }
            return false;
        }
        public int GetCountNeighboursOfType(Tile_Type type, bool useFourNeighbours = false) {
            int count = 0;
            Dictionary<GridNeighbourDirection, LocationGridTile> n = neighbours;
            if (useFourNeighbours) {
                n = FourNeighboursDictionary();
            }
            for (int i = 0; i < n.Values.Count; i++) {
                if (neighbours.Values.ElementAt(i).tileType == type) {
                    count++;
                }
            }
            return count;
        }
        public bool HasNeighbourOfType(Ground_Type type, bool useFourNeighbours = false) {
            Dictionary<GridNeighbourDirection, LocationGridTile> n = neighbours;
            if (useFourNeighbours) {
                n = FourNeighboursDictionary();
            }
            for (int i = 0; i < n.Values.Count; i++) {
                if (neighbours.Values.ElementAt(i).groundType == type) {
                    return true;
                }
            }
            return false;
        }
        public bool HasNeighbourNotInList(List<LocationGridTile> list, bool useFourNeighbours = false) {
            Dictionary<GridNeighbourDirection, LocationGridTile> n = neighbours;
            if (useFourNeighbours) {
                n = FourNeighboursDictionary();
            }
            for (int i = 0; i < n.Values.Count; i++) {
                if (list.Contains(neighbours.Values.ElementAt(i)) == false) {
                    return true;
                }
            }
            return false;
        }
        public bool IsNeighbour(LocationGridTile tile) {
            foreach (KeyValuePair<GridNeighbourDirection, LocationGridTile> keyValuePair in neighbours) {
                if (keyValuePair.Value == tile) {
                    return true;
                }
            }
            return false;
        }
        public bool IsAdjacentTo(Type type) {
            foreach (KeyValuePair<GridNeighbourDirection, LocationGridTile> keyValuePair in neighbours) {
                if ((keyValuePair.Value.objHere != null && keyValuePair.Value.objHere.GetType() == type)) {
                    return true;
                }
            }
            return false;
        }
        public bool HasNeighbouringWalledStructure() {
            foreach (KeyValuePair<GridNeighbourDirection, LocationGridTile> keyValuePair in neighbours) {
                if (keyValuePair.Value.structure != null && keyValuePair.Value.structure.structureType.IsOpenSpace() == false) {
                    return true;
                }
            }
            return false;
        }
        public LocationGridTile GetNearestUnoccupiedTileFromThis() {
            List<LocationGridTile> unoccupiedNeighbours = UnoccupiedNeighbours;
            if (unoccupiedNeighbours.Count == 0) {
                if (structure != null) {
                    LocationGridTile nearestTile = null;
                    float nearestDist = 99999f;
                    for (int i = 0; i < structure.unoccupiedTiles.Count; i++) {
                        LocationGridTile currTile = structure.unoccupiedTiles.ElementAt(i);
                        if (currTile != this && currTile.groundType != Ground_Type.Water) {
                            float dist = Vector2.Distance(currTile.localLocation, localLocation);
                            if (dist < nearestDist) {
                                nearestTile = currTile;
                                nearestDist = dist;
                            }
                        }
                    }
                    return nearestTile;
                }
            } else {
                return unoccupiedNeighbours[Random.Range(0, unoccupiedNeighbours.Count)];
            }
            return null;
        }
        public LocationGridTile GetNearestEdgeTileFromThis() {
            if (IsAtEdgeOfWalkableMap() && structure != null) {
                return this;
            }

            LocationGridTile nearestEdgeTile = null;
            List<LocationGridTile> neighbours = neighbourList;
            for (int i = 0; i < neighbours.Count; i++) {
                if (neighbours[i].IsAtEdgeOfWalkableMap() && neighbours[i].structure != null) {
                    nearestEdgeTile = neighbours[i];
                    break;
                }
            }
            if (nearestEdgeTile == null) {
                float nearestDist = -999f;
                for (int i = 0; i < parentMap.allEdgeTiles.Count; i++) {
                    LocationGridTile currTile = parentMap.allEdgeTiles[i];
                    float dist = Vector2.Distance(currTile.localLocation, localLocation);
                    if (nearestDist == -999f || dist < nearestDist) {
                        if (currTile.structure != null) {
                            nearestEdgeTile = currTile;
                            nearestDist = dist;
                        }
                    }
                }
            }
            return nearestEdgeTile;
        }
        public LocationGridTile GetRandomUnoccupiedNeighbor() {
            List<LocationGridTile> unoccupiedNeighbours = UnoccupiedNeighbours;
            if (unoccupiedNeighbours.Count > 0) {
                return unoccupiedNeighbours[Random.Range(0, unoccupiedNeighbours.Count)];
            }
            return null;
        }
        public void SetLockedState(bool state) {
            isLocked = state;
        }
        public bool IsAtEdgeOfWalkableMap() {
            if ((localPlace.y == InnerTileMap.SouthEdge && localPlace.x >= InnerTileMap.WestEdge && localPlace.x <= parentMap.width - InnerTileMap.EastEdge - 1)
                || (localPlace.y == parentMap.height - InnerTileMap.NorthEdge - 1 && localPlace.x >= InnerTileMap.WestEdge && localPlace.x <= parentMap.width - InnerTileMap.EastEdge - 1)
                || (localPlace.x == InnerTileMap.WestEdge && localPlace.y >= InnerTileMap.SouthEdge && localPlace.y <= parentMap.height - InnerTileMap.NorthEdge - 1) 
                || (localPlace.x == parentMap.width - InnerTileMap.EastEdge - 1 && localPlace.y >= InnerTileMap.SouthEdge && localPlace.y <= parentMap.height - InnerTileMap.NorthEdge - 1)) {
                return true;
            }
            return false;
        }
        public void HighlightTile() {
            parentMap.groundTilemap.SetColor(localPlace, Color.blue);
        }
        public void HighlightTile(Color color) {
            parentMap.groundTilemap.SetColor(localPlace, color);
        }
        public void UnhighlightTile() {
            parentMap.groundTilemap.SetColor(localPlace, defaultTileColor);
        }
        private bool HasCardinalNeighbourOfDifferentGroundType(out Dictionary<GridNeighbourDirection, LocationGridTile> differentTiles) {
            bool hasDiff = false;
            differentTiles = new Dictionary<GridNeighbourDirection, LocationGridTile>();
            Dictionary<GridNeighbourDirection, LocationGridTile> cardinalNeighbours = FourNeighboursDictionary();
            foreach (KeyValuePair<GridNeighbourDirection, LocationGridTile> keyValuePair in cardinalNeighbours) {
                if (keyValuePair.Value.groundType != groundType) {
                    hasDiff = true;
                    differentTiles.Add(keyValuePair.Key, keyValuePair.Value);
                }
            }
            return hasDiff;
        }
        public void SetDefaultTileColor(Color color) {
            defaultTileColor = color;
        }
        public List<ITraitable> GetTraitablesOnTileWithTrait(string requiredTrait) {
            List<ITraitable> traitables = new List<ITraitable>();
            if (genericTileObject.traitContainer.HasTrait(requiredTrait)) {
                traitables.Add(genericTileObject);
            }
            for (int i = 0; i < walls.Count; i++) {
                StructureWallObject structureWallObject = walls[i];
                if (structureWallObject.traitContainer.HasTrait(requiredTrait)) {
                    traitables.Add(structureWallObject);
                }
            }
            if (objHere != null && objHere.traitContainer.HasTrait(requiredTrait)) {
                if ((objHere is TileObject && (objHere as TileObject).mapObjectState == MAP_OBJECT_STATE.BUILT)) { //|| (objHere is SpecialToken && (objHere as SpecialToken).mapObjectState == MAP_OBJECT_STATE.BUILT)
                    traitables.Add(objHere);
                }
            }
        
            for (int i = 0; i < charactersHere.Count; i++) {
                Character character = charactersHere[i];
                if (character.traitContainer.HasTrait(requiredTrait)) {
                    traitables.Add(character);
                }
            }
            return traitables;
        }
        public List<ITraitable> GetTraitablesOnTile() {
            List<ITraitable> traitables = new List<ITraitable>();
            traitables.Add(genericTileObject);
            for (int i = 0; i < walls.Count; i++) {
                StructureWallObject structureWallObject = walls[i];
                traitables.Add(structureWallObject);
            }
            if (objHere != null) {
                if ((objHere is TileObject && (objHere as TileObject).mapObjectState == MAP_OBJECT_STATE.BUILT)) {//|| (objHere is SpecialToken && (objHere as SpecialToken).mapObjectState == MAP_OBJECT_STATE.BUILT)
                    traitables.Add(objHere);
                }
            }
            for (int i = 0; i < charactersHere.Count; i++) {
                Character character = charactersHere[i];
                traitables.Add(character);
            }
            return traitables;
        }
        public void PerformActionOnTraitables(TraitableCallback callback) {
            callback.Invoke(genericTileObject);
            for (int i = 0; i < walls.Count; i++) {
                StructureWallObject structureWallObject = walls[i];
                callback.Invoke(structureWallObject);
            }
            if (objHere is TileObject tileObject && tileObject.mapObjectState == MAP_OBJECT_STATE.BUILT) {
                callback.Invoke(objHere);
            }
            for (int i = 0; i < charactersHere.Count; i++) {
                Character character = charactersHere[i];
                callback.Invoke(character);
            }
        }
        public List<IPointOfInterest> GetPOIsOnTile() {
            List<IPointOfInterest> pois = new List<IPointOfInterest>();
            pois.Add(genericTileObject);
            if (objHere != null) {
                if ((objHere is TileObject && (objHere as TileObject).mapObjectState == MAP_OBJECT_STATE.BUILT)) {
                    pois.Add(objHere);
                }
            }
            for (int i = 0; i < charactersHere.Count; i++) {
                Character character = charactersHere[i];
                pois.Add(character);
            }
            return pois;
        }
        public int GetNeighbourOfTypeCount(Ground_Type type, bool useFourNeighbours = false) {
            int count = 0;
            Dictionary<GridNeighbourDirection, LocationGridTile> n = neighbours;
            if (useFourNeighbours) {
                n = FourNeighboursDictionary();
            }
            for (int i = 0; i < n.Values.Count; i++) {
                if (neighbours.Values.ElementAt(i).groundType == type) {
                    count++;
                }
            }
            return count;
        }
        public bool IsPartOfSettlement(out Settlement settlement) {
            if (buildSpotOwner.isPartOfParentRegionMap && buildSpotOwner.hexTileOwner.settlementOnTile != null) {
                settlement = buildSpotOwner.hexTileOwner.settlementOnTile;
                return true;
            }
            settlement = null;
            return false;
        }
        public bool IsPartOfSettlement(Settlement settlement) {
            return buildSpotOwner.isPartOfParentRegionMap && buildSpotOwner.hexTileOwner.settlementOnTile == settlement;
        }
        public bool IsPartOfSettlement() {
            return buildSpotOwner.isPartOfParentRegionMap && buildSpotOwner.hexTileOwner.settlementOnTile != null;
        }
        public bool IsNextToSettlement(out Settlement settlement) {
            for (int i = 0; i < neighbourList.Count; i++) {
                LocationGridTile tile = neighbourList[i];
                if (tile.IsPartOfSettlement(out settlement)) {
                    return true;
                }
            }
            settlement = null;
            return false;
        }
        public bool IsNextToSettlement(Settlement settlement) {
            for (int i = 0; i < neighbourList.Count; i++) {
                LocationGridTile tile = neighbourList[i];
                if (tile.IsPartOfSettlement(settlement)) {
                    return true;
                }
            }
            return false;
        }
        public bool IsNextToOrPartOfSettlement(out Settlement settlement) {
            return IsPartOfSettlement( out settlement) || IsNextToSettlement(out settlement);
        }
        public bool IsNextToOrPartOfSettlement(Settlement settlement) {
            return IsPartOfSettlement(settlement) || IsNextToSettlement(settlement);
        }
        public List<LocationGridTile> GetTilesInRadius(int radius, int radiusLimit = 0, bool includeCenterTile = false, bool includeTilesInDifferentStructure = false) {
            List<LocationGridTile> tiles = new List<LocationGridTile>();
            int mapSizeX = parentMap.map.GetUpperBound(0);
            int mapSizeY = parentMap.map.GetUpperBound(1);
            int x = localPlace.x;
            int y = localPlace.y;
            if (includeCenterTile) {
                tiles.Add(this);
            }
            int xLimitLower = x - radiusLimit;
            int xLimitUpper = x + radiusLimit;
            int yLimitLower = y - radiusLimit;
            int yLimitUpper = y + radiusLimit;


            for (int dx = x - radius; dx <= x + radius; dx++) {
                for (int dy = y - radius; dy <= y + radius; dy++) {
                    if (dx >= 0 && dx <= mapSizeX && dy >= 0 && dy <= mapSizeY) {
                        if (dx == x && dy == y) {
                            continue;
                        }
                        if (radiusLimit > 0 && dx > xLimitLower && dx < xLimitUpper && dy > yLimitLower && dy < yLimitUpper) {
                            continue;
                        }
                        LocationGridTile result = parentMap.map[dx, dy];
                        if (result.structure == null) { continue; } //do not include tiles with no structures
                        if (!includeTilesInDifferentStructure 
                            && (result.structure != structure && (!result.structure.structureType.IsOpenSpace() || !structure.structureType.IsOpenSpace()))) { continue; }
                        tiles.Add(result);
                    }
                }
            }
            return tiles;
        }
        #endregion

        #region Mouse Actions
        //        public void OnClickTileActions(PointerEventData.InputButton inputButton) {
        //            if (InnerMapManager.Instance.IsMouseOnMarker()) {
        //                return;
        //            }
        //            if (objHere == null) {
        //#if UNITY_EDITOR
        //                if (inputButton == PointerEventData.InputButton.Right) {
        //                    UIManager.Instance.poiTestingUI.ShowUI(this);
        //                } else {
        //                    Messenger.Broadcast(Signals.HIDE_MENUS);
        //                }
        //#else
        //             Messenger.Broadcast(Signals.HIDE_MENUS);
        //#endif
        //            } else if (objHere is TileObject || objHere is SpecialToken) {
        //#if UNITY_EDITOR
        //                if (inputButton == PointerEventData.InputButton.Right) {
        //                    if (objHere is TileObject) {
        //                        UIManager.Instance.poiTestingUI.ShowUI(objHere);
        //                    }
        //                }
        //                //else {
        //                //    if (objHere is TileObject) {
        //                //        UIManager.Instance.ShowTileObjectInfo(objHere as TileObject);
        //                //    }
        //                //}
        //#else
        //              //if (inputButton == PointerEventData.InputButton.Left) {
        //              //   if (objHere is TileObject) {
        //              //       UIManager.Instance.ShowTileObjectInfo(objHere as TileObject);
        //              //   }
        //              //}
        //#endif
        //            }
        //        }
        #endregion

        #region Tile Objects
        public void SetReservedType(TILE_OBJECT_TYPE reservedType) {
            if (structure != null) {
                if (reservedObjectType != TILE_OBJECT_TYPE.NONE && reservedType == TILE_OBJECT_TYPE.NONE && tileState == Tile_State.Empty) {
                    structure.AddUnoccupiedTile(this);
                } else if (reservedObjectType == TILE_OBJECT_TYPE.NONE && reservedType != TILE_OBJECT_TYPE.NONE) {
                    structure.RemoveUnoccupiedTile(this);
                }
            }
            reservedObjectType = reservedType;
        }
        #endregion

        #region Furniture Spots
        public void SetFurnitureSpot(FurnitureSpot spot) {
            furnitureSpot = spot;
            hasFurnitureSpot = true;
        }
        public FURNITURE_TYPE GetFurnitureThatCanProvide(FACILITY_TYPE facility) {
            List<FURNITURE_TYPE> choices = new List<FURNITURE_TYPE>();
            if (furnitureSpot.allowedFurnitureTypes != null) {
                for (int i = 0; i < furnitureSpot.allowedFurnitureTypes.Length; i++) {
                    FURNITURE_TYPE currType = furnitureSpot.allowedFurnitureTypes[i];
                    if (currType.ConvertFurnitureToTileObject().CanProvideFacility(facility)) {
                        choices.Add(currType);
                    }
                }
                if (choices.Count > 0) {
                    return choices[Random.Range(0, choices.Count)];
                }
            }
            throw new Exception(
                $"Furniture spot at {ToString()} cannot provide facility {facility}! Should not reach this point if that is the case!");
        }
        #endregion

        #region Building
        public BuildingSpot buildSpotOwner { get; private set; }
        public void SetHasBlueprint(bool hasBlueprint) {
            this.hasBlueprint = hasBlueprint;
        }
        public void SetBuildSpotOwner(BuildingSpot buildSpot) {
            buildSpotOwner = buildSpot;
        }
        #endregion

        #region Walls
        public void AddWallObject(StructureWallObject structureWallObject) {
            walls.Add(structureWallObject);
        }
        public void RemoveWallObject(StructureWallObject structureWallObject) {
            walls.Remove(structureWallObject);
        }
        public void ClearWallObjects() {
            walls.Clear();
        }
        #endregion

        #region Corruption
        public void CorruptTile() {
            SetGroundTilemapVisual(InnerMapManager.Instance.assetManager.corruptedTile);
            CreateSeamlessEdgesForSelfAndNeighbours();
            if (hasDetail) {
                parentMap.detailsTilemap.SetTile(localPlace, null);
            }
            if (objHere != null) {
                if (objHere is TreeObject tree) {
                    (tree.mapObjectVisual as TileObjectGameObject).UpdateTileObjectVisual(tree);
                } else if (objHere is BlockWall blockWall) {
                    blockWall.SetWallType(WALL_TYPE.Demon_Stone);
                    blockWall.UpdateVisual(this);
                } else {
                    structure.RemovePOI(objHere);
                }
            }
        }
        #endregion
    }

    [Serializable]
    public struct TwoTileDirections {
        public GridNeighbourDirection from;
        public GridNeighbourDirection to;

        public TwoTileDirections(GridNeighbourDirection from, GridNeighbourDirection to) {
            this.from = from;
            this.to = to;
        }
    }


    [Serializable]
    public class SaveDataLocationGridTile {
        public Vector3Save localPlace; //this is the id
        public Vector3Save worldLocation;
        public Vector3Save centeredWorldLocation;
        public Vector3Save localLocation;
        public Vector3Save centeredLocalLocation;
        public LocationGridTile.Tile_Type tileType;
        public LocationGridTile.Tile_State tileState;
        public LocationGridTile.Ground_Type groundType;
        //public LocationStructure structure { get; private set; }
        //public Dictionary<TileNeighbourDirection, LocationGridTile> neighbours { get; private set; }
        //public List<Vector3Save> neighbours;
        //public List<TileNeighbourDirection> neighbourDirections;
        public List<SaveDataTrait> traits;
        //public List<int> charactersHere;
        public int objHereID;
        public POINT_OF_INTEREST_TYPE objHereType;
        public TILE_OBJECT_TYPE objHereTileObjectType;


        public TILE_OBJECT_TYPE reservedObjectType;
        public FurnitureSpot furnitureSpot;
        public bool hasFurnitureSpot;
        public bool hasDetail;
        public bool isInside;
        public bool isLocked;

        public int structureID;
        public STRUCTURE_TYPE structureType;

        private LocationGridTile loadedGridTile;

        //tilemap assets
        public string groundTileMapAssetName;
        public string roadTileMapAssetName;
        public string wallTileMapAssetName;
        public string detailTileMapAssetName;
        public string structureTileMapAssetName;
        public string objectTileMapAssetName;

        public Matrix4x4 groundTileMapMatrix;
        public Matrix4x4 roadTileMapMatrix;
        public Matrix4x4 wallTileMapMatrix;
        public Matrix4x4 detailTileMapMatrix;
        public Matrix4x4 structureTileMapMatrix;
        public Matrix4x4 objectTileMapMatrix;

        public void Save(LocationGridTile gridTile) {
            localPlace = new Vector3Save(gridTile.localPlace);
            worldLocation = gridTile.worldLocation;
            centeredWorldLocation = gridTile.centeredWorldLocation;
            localLocation = gridTile.localLocation;
            centeredLocalLocation = gridTile.centeredLocalLocation;
            tileType = gridTile.tileType;
            tileState = gridTile.tileState;
            groundType = gridTile.groundType;
            reservedObjectType = gridTile.reservedObjectType;
            furnitureSpot = gridTile.furnitureSpot;
            hasFurnitureSpot = gridTile.hasFurnitureSpot;
            hasDetail = gridTile.hasDetail;
            isLocked = gridTile.isLocked;

            if(gridTile.structure != null) {
                structureID = gridTile.structure.id;
                structureType = gridTile.structure.structureType;
            } else {
                structureID = -1;
            }

            //neighbourDirections = new List<TileNeighbourDirection>();
            //neighbours = new List<Vector3Save>();
            //foreach (KeyValuePair<TileNeighbourDirection, LocationGridTile> kvp in gridTile.neighbours) {
            //    neighbourDirections.Add(kvp.Key);
            //    neighbours.Add(new Vector3Save(kvp.Value.localPlace));
            //}

            traits = new List<SaveDataTrait>();
            for (int i = 0; i < gridTile.normalTraits.Count; i++) {
                SaveDataTrait saveDataTrait = SaveManager.ConvertTraitToSaveDataTrait(gridTile.normalTraits[i]);
                if (saveDataTrait != null) {
                    saveDataTrait.Save(gridTile.normalTraits[i]);
                    traits.Add(saveDataTrait);
                }
            }

            if(gridTile.objHere != null) {
                objHereID = gridTile.objHere.id;
                objHereType = gridTile.objHere.poiType;
                if(gridTile.objHere is TileObject) {
                    objHereTileObjectType = (gridTile.objHere as TileObject).tileObjectType;
                }
            } else {
                objHereID = -1;
            }

            //tilemap assets
            groundTileMapAssetName = gridTile.parentMap.groundTilemap.GetTile(gridTile.localPlace)?.name ?? string.Empty;
            detailTileMapAssetName = gridTile.parentMap.detailsTilemap.GetTile(gridTile.localPlace)?.name ?? string.Empty;
            structureTileMapAssetName = gridTile.parentMap.structureTilemap.GetTile(gridTile.localPlace)?.name ?? string.Empty;

            groundTileMapMatrix = gridTile.parentMap.groundTilemap.GetTransformMatrix(gridTile.localPlace);
            detailTileMapMatrix = gridTile.parentMap.detailsTilemap.GetTransformMatrix(gridTile.localPlace);
            structureTileMapMatrix = gridTile.parentMap.structureTilemap.GetTransformMatrix(gridTile.localPlace);
        }

        public LocationGridTile Load(Tilemap tilemap, InnerTileMap parentAreaMap, Dictionary<string, TileBase> tileAssetDB) {
            LocationGridTile tile = new LocationGridTile(this, tilemap, parentAreaMap);

            if(structureID != -1) {
                LocationStructure structure = (parentAreaMap.location as Settlement).GetStructureByID(structureType, structureID);
                tile.SetStructure(structure);
            }

            //tile.SetGroundType(groundType);
            if (hasFurnitureSpot) {
                tile.SetFurnitureSpot(furnitureSpot);
            }
            loadedGridTile = tile;

            //load tile assets
            // tile.SetGroundTilemapVisual(InnerMapManager.Instance.TryGetTileAsset(groundTileMapAssetName, tileAssetDB));
            // tile.parentMap.detailsTilemap.SetTile(tile.localPlace, InnerMapManager.Instance.TryGetTileAsset(detailTileMapAssetName, tileAssetDB));
            // tile.parentMap.structureTilemap.SetTile(tile.localPlace, InnerMapManager.Instance.TryGetTileAsset(structureTileMapAssetName, tileAssetDB));

            tile.parentMap.groundTilemap.SetTransformMatrix(tile.localPlace, groundTileMapMatrix);
            tile.parentMap.detailsTilemap.SetTransformMatrix(tile.localPlace, detailTileMapMatrix);
            tile.parentMap.structureTilemap.SetTransformMatrix(tile.localPlace, structureTileMapMatrix);

            return tile;
        }

        public void LoadTraits() {
            for (int i = 0; i < traits.Count; i++) {
                Character responsibleCharacter = null;
                Trait trait = traits[i].Load(ref responsibleCharacter);
                loadedGridTile.genericTileObject.traitContainer.AddTrait(loadedGridTile.genericTileObject, trait, responsibleCharacter);
            }
        }

        //This is loaded last so release loadedGridTile here
        public void LoadObjectHere() {
            if(objHereID != -1) {
                if(objHereType == POINT_OF_INTEREST_TYPE.CHARACTER) {
                    loadedGridTile.structure.AddPOI(CharacterManager.Instance.GetCharacterByID(objHereID), loadedGridTile);
                }

                //NOTE: Do not load item in grid tile because it is already loaded in LoadAreaItems
                //else if (objHereType == POINT_OF_INTEREST_TYPE.ITEM) {
                //    loadedGridTile.structure.AddPOI(TokenManager.Instance.GetSpecialTokenByID(objHereID), loadedGridTile);
                //}
                else if (objHereType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
                    TileObject obj = InnerMapManager.Instance.GetTileObject(objHereTileObjectType, objHereID);
                    if (obj == null) {
                        throw new Exception(
                            $"Could not find object of type {objHereTileObjectType} with id {objHereID} at {loadedGridTile.structure}");
                    }
                    loadedGridTile.structure.AddPOI(obj, loadedGridTile);
                }
            }
            //loadedGridTile = null;
        }
    }
}