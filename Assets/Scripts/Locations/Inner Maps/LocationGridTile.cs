using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using PathFind;
using Pathfinding;
using Scriptable_Object_Scripts;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UtilityScripts;
using Random = UnityEngine.Random;
namespace Inner_Maps {
    public class LocationGridTile : IHasNeighbours<LocationGridTile>, ISavable {

        public enum Tile_Type { Empty, Wall }
        public enum Tile_State { Empty, Occupied }
        public enum Ground_Type { Soil, Grass, Stone, Snow, Tundra, Cobble, Wood, Snow_Dirt, Water, Cave, Corrupted, 
            Desert_Grass, Sand, Desert_Stone, Bone, Demon_Stone, Flesh, Structure_Stone,
            Ruined_Stone
        }
        
        public string persistentID { get; }
        public InnerTileMap parentMap { get; private set; }
        public Tilemap parentTileMap { get; private set; }
        public Vector3Int localPlace { get; }
        public Vector3 worldLocation { get; private set; }
        public Vector3 centeredWorldLocation { get; private set; }
        public Vector3 localLocation { get; }
        public Vector3 centeredLocalLocation { get; }
        public Tile_Type tileType { get; private set; }
        public Tile_State tileState { get; private set; }
        public Ground_Type groundType { get; private set; }
        public LocationStructure structure { get; private set; }
        public List<LocationGridTile> neighbourList { get; private set; }
        public IPointOfInterest objHere { get; private set; }
        public List<Character> charactersHere { get; private set; }
        public GenericTileObject genericTileObject { get; private set; }
        public List<StructureWallObject> walls { get; private set; }
        public LocationGridTileCollection collectionOwner { get; private set; }
        public bool hasLandmine { get; private set; }
        public bool hasFreezingTrap { get; private set; }
        public bool hasSnareTrap { get; private set; }
        /// <summary>
        /// Does this tile have a blueprint on it.
        /// NOTE: This is not saved since blueprint placement is handled by GenericTileObject loading
        /// <see cref="GenericTileObject.LoadBlueprintOnTile"/>
        /// </summary>
        public bool hasBlueprint { get; private set; }
        /// <summary>
        /// Number of structure connectors on the tile.
        /// </summary>
        public int connectorsOnTile { get; private set; }
        /// <summary>
        /// The generated perlin noise sample of this tile.
        /// </summary>
        public float floorSample { get; private set; }
        public List<RACE> freezingTrapExclusions { get; private set; }
        /// <summary>
        /// Is this tile using the default settings (i.e. No Traits other than Flammable, No Jobs targeting its Generic Tile Object, etc.)
        /// </summary>
        public bool isDefault { get; private set; }
        /// <summary>
        /// Initial Ground Type of this tile.
        /// NOTE: This is not saved because this is determined every start up depending on the saved seed.
        /// <see cref="InnerTileMap.GroundPerlin"/>
        /// </summary>
        public Ground_Type initialGroundType { get; private set; }
        
        private GameObject _landmineEffect;
        private GameObject _freezingTrapEffect;
        private GameObject _snareTrapEffect;
        private Dictionary<GridNeighbourDirection, LocationGridTile> _neighbours;
        private Dictionary<GridNeighbourDirection, LocationGridTile> _fourNeighbours;

        #region getters
        public OBJECT_TYPE objectType => OBJECT_TYPE.Gridtile;
        public System.Type serializedData => typeof(SaveDataLocationGridTile); 
        public bool isOccupied => tileState == Tile_State.Occupied;
        public List<Trait> traits => genericTileObject.traitContainer.traits;
        public List<Status> statuses => genericTileObject.traitContainer.statuses;
        public bool isCorrupted => groundType == Ground_Type.Corrupted;
        public BIOMES biomeType => collectionOwner.GetConnectedHextileOrNearestHextile().biomeType;
        #endregion
        
        #region Pathfinding
        public List<LocationGridTile> ValidTiles { get { return FourNeighbours().Where(o => o.tileType == Tile_Type.Empty).ToList(); } }
        public List<LocationGridTile> CaveInterconnectionTiles { get { return FourNeighbours().Where(o => o.structure == structure && !o.HasDifferentStructureNeighbour()).ToList() ; } } //
        public List<LocationGridTile> UnoccupiedNeighbours { get { return neighbourList.Where(o => !o.isOccupied && o.structure == structure).ToList(); } }
        public List<LocationGridTile> UnoccupiedNeighboursWithinHex {
            get {
                return neighbourList.Where(o =>
                        !o.isOccupied && o.charactersHere.Count <= 0 && o.structure == structure &&
                        o.collectionOwner.isPartOfParentRegionMap &&
                        o.collectionOwner.partOfHextile.hexTileOwner == collectionOwner.partOfHextile.hexTileOwner)
                    .ToList();
            }
        }
        #endregion
        
        public LocationGridTile(int x, int y, Tilemap tilemap, InnerTileMap parentMap) {
            persistentID = System.Guid.NewGuid().ToString();
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
            _fourNeighbours = new Dictionary<GridNeighbourDirection, LocationGridTile>();
            _neighbours = new Dictionary<GridNeighbourDirection, LocationGridTile>();
            neighbourList = new List<LocationGridTile>();
            isDefault = true;
            connectorsOnTile = 0;
            DatabaseManager.Instance.locationGridTileDatabase.RegisterTile(this);
        }
        public LocationGridTile(SaveDataLocationGridTile data, Tilemap tilemap, InnerTileMap parentMap) {
            persistentID = data.persistentID;
            this.parentMap = parentMap;
            parentTileMap = tilemap;
            localPlace = new Vector3Int((int)data.localPlace.x, (int)data.localPlace.y, 0);
            worldLocation = data.worldLocation;
            localLocation = data.localLocation;
            centeredLocalLocation = data.centeredLocalLocation;
            centeredWorldLocation = data.centeredWorldLocation;
            tileType = data.tileType;
            tileState = data.tileState;
            charactersHere = new List<Character>();
            walls = new List<StructureWallObject>();
            _fourNeighbours = new Dictionary<GridNeighbourDirection, LocationGridTile>();
            _neighbours = new Dictionary<GridNeighbourDirection, LocationGridTile>();
            neighbourList = new List<LocationGridTile>();
            isDefault = data.isDefault;
            connectorsOnTile = data.connectorsCount;
            DatabaseManager.Instance.locationGridTileDatabase.RegisterTile(this);
        }

        #region Loading
        public void LoadSecondWave(SaveDataLocationGridTile saveDataLocationGridTile) {
            if (saveDataLocationGridTile.hasLandmine) {
                SetHasLandmine(true);
            }
            if (saveDataLocationGridTile.hasFreezingTrap) {
                SetHasFreezingTrap(true, saveDataLocationGridTile.freezingTrapExclusions?.ToArray());
            }
            if (saveDataLocationGridTile.hasSnareTrap) {
                SetHasSnareTrap(true);
            }
            for (int i = 0; i < saveDataLocationGridTile.meteorCount; i++) {
                AddMeteor();
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
        public void SetTileType(Tile_Type tileType) {
            this.tileType = tileType;
        }
        public void CreateGenericTileObject() {
            genericTileObject = new GenericTileObject(this);
        }
        public void LoadGenericTileObject(GenericTileObject genericTileObject) {
            this.genericTileObject = genericTileObject;
        }
        public void SetCollectionOwner(LocationGridTileCollection _collectionOwner) {
            collectionOwner = _collectionOwner;
        }
        private void SetGroundType(Ground_Type newGroundType, bool isInitial = false) {
            Ground_Type previousType = this.groundType;
            this.groundType = newGroundType;
            if (genericTileObject != null) {
                switch (newGroundType) {
                    case Ground_Type.Grass:
                    case Ground_Type.Wood:
                    case Ground_Type.Sand:
                    case Ground_Type.Desert_Grass:
                    case Ground_Type.Structure_Stone:
                        genericTileObject.traitContainer.AddTrait(genericTileObject, "Flammable");
                        break;
                    case Ground_Type.Snow:
                        // genericTileObject.traitContainer.AddTrait(genericTileObject, "Frozen", bypassElementalChance: true, overrideDuration: 0);
                        genericTileObject.traitContainer.RemoveTrait(genericTileObject, "Flammable");
                        break;
                    default:
                        genericTileObject.traitContainer.RemoveTrait(genericTileObject, "Flammable");
                        break;
                }
            }
            //if snow ground was set to tundra, schedule snow to grow back after a few hours
            if (GameManager.Instance.gameHasStarted && previousType == Ground_Type.Snow && newGroundType == Ground_Type.Tundra) {
                GameDate dueDate = GameManager.Instance.Today();
                dueDate.AddTicks(GameManager.Instance.GetTicksBasedOnHour(Random.Range(1, 4)));
                SchedulingManager.Instance.AddEntry(dueDate, RevertBackToSnow, this);
            }
            if (!isInitial) {
                if (initialGroundType != newGroundType) {
                    SetIsDefault(false);
                }    
            }
        }
        private void RevertBackToSnow() {
            SetGroundTilemapVisual(InnerMapManager.Instance.assetManager.snowTile, true);
        }
        public void UpdateWorldLocation() {
            worldLocation = parentTileMap.CellToWorld(localPlace);
            centeredWorldLocation = new Vector3(worldLocation.x + 0.5f, worldLocation.y + 0.5f, worldLocation.z);
        }
        public List<LocationGridTile> FourNeighbours() {
            List<LocationGridTile> fn = new List<LocationGridTile>();
            foreach (KeyValuePair<GridNeighbourDirection, LocationGridTile> keyValuePair in _fourNeighbours) {
                fn.Add(keyValuePair.Value);
            }
            return fn;
        }
        private Dictionary<GridNeighbourDirection, LocationGridTile> FourNeighboursDictionary() { return _fourNeighbours; }
        public void FindNeighbours(LocationGridTile[,] map) {
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
                    _neighbours.Add(currDir, tile);
                    neighbourList.Add(tile);
                    if (currDir.IsCardinalDirection()) {
                        _fourNeighbours.Add(currDir, tile);
                    }
                }

            }
        }
        #endregion
        
        #region Visuals
        public void SetInitialGroundType(Ground_Type groundType) {
            initialGroundType = groundType;
        }
        public void SetFloorSample(float floorSample) {
            this.floorSample = floorSample;
        }
        private Ground_Type GetGroundTypeBasedOnCurrentAsset() {
            Sprite groundAsset = parentMap.groundTilemap.GetSprite(localPlace);
            Sprite structureAsset = parentMap.structureTilemap.GetSprite(localPlace);
            if (ReferenceEquals(structureAsset, null) == false) {
                string assetName = structureAsset.name.ToLower();
                if (assetName.Contains("dungeon") || assetName.Contains("cave") || assetName.Contains("laid")) {
                    return Ground_Type.Cave;
                } else if (assetName.Contains("water") || assetName.Contains("pond") || assetName.Contains("shore")) {
                    return Ground_Type.Water;
                } 
            }
            if (ReferenceEquals(groundAsset, null) == false) {
                string assetName = groundAsset.name.ToLower();
                if (assetName.Contains("desert")) {
                    if (assetName.Contains("grass")) {
                        return Ground_Type.Desert_Grass;
                    } else if (assetName.Contains("sand")) {
                        return Ground_Type.Sand;
                    } else if (assetName.Contains("rocks")) {
                        return Ground_Type.Desert_Stone;
                    }
                } else if (assetName.Contains("corruption") || assetName.Contains("corrupted")) {
                    return Ground_Type.Corrupted;
                } else if (assetName.Contains("bone")) {
                    return Ground_Type.Bone;
                } else if (assetName.Contains("structure floor") || assetName.Contains("wood")) {
                    return Ground_Type.Wood;
                } else if (assetName.Contains("cobble")) {
                    return Ground_Type.Cobble;
                } else if (assetName.Contains("water") || assetName.Contains("pond")) {
                    return Ground_Type.Water;
                } else if (assetName.Contains("dirt") || assetName.Contains("soil") || assetName.Contains("outside") || assetName.Contains("snow")) {
                    if (biomeType == BIOMES.SNOW || biomeType == BIOMES.TUNDRA) {
                        if (assetName.Contains("dirtsnow")) {
                            return Ground_Type.Snow_Dirt;
                        } else if (assetName.Contains("snow")) {
                            return Ground_Type.Snow;
                        } else {
                            //override tile to use tundra soil
                            parentMap.groundTilemap.SetTile(localPlace, InnerMapManager.Instance.assetManager.tundraTile);
                            return Ground_Type.Tundra;
                        }
                    } else if (biomeType == BIOMES.DESERT) {
                        if (structure != null && (structure.structureType == STRUCTURE_TYPE.CAVE || structure.structureType == STRUCTURE_TYPE.MONSTER_LAIR)) {
                            //override tile to use stone
                            parentMap.groundTilemap.SetTile(localPlace, InnerMapManager.Instance.assetManager.stoneTile);
                            return Ground_Type.Stone;
                        } else {
                            //override tile to use sand
                            parentMap.groundTilemap.SetTile(localPlace, InnerMapManager.Instance.assetManager.desertSandTile);
                            return Ground_Type.Sand;
                        }
                        
                    } else {
                        return Ground_Type.Soil;
                    }
                } else if (assetName.Contains("stone") || assetName.Contains("road")) {
                    if (assetName.Contains("demon")) {
                        return Ground_Type.Demon_Stone;   
                    } else if (assetName.Contains("floor")) {
                        return Ground_Type.Structure_Stone;
                    } else {
                        return Ground_Type.Stone;    
                    }
                } else if (assetName.Contains("ruins")) {
                    return Ground_Type.Ruined_Stone;
                } else if (assetName.Contains("grass")) {
                    return Ground_Type.Grass;
                } else if (assetName.Contains("tundra")) {
                    //override tile to use tundra soil
                    parentMap.groundTilemap.SetTile(localPlace, InnerMapManager.Instance.assetManager.tundraTile);
                    return Ground_Type.Tundra;
                } else if (assetName.Contains("flesh")) {
                    return Ground_Type.Flesh;
                }
            }
            throw new Exception($"There is no ground type for ground asset: {groundAsset} or structure asset: {structureAsset}");
        }
        public void UpdateGroundTypeBasedOnAsset() {
            Ground_Type determinedGroundType = GetGroundTypeBasedOnCurrentAsset();
            SetGroundType(determinedGroundType);
        }
        /// <summary>
        /// Update this tiles initial ground type based on its given asset.
        /// NOTE: This should only be used on initial Map Generation
        /// <see cref="InnerTileMap.GroundPerlin"/> 
        /// </summary>
        public void InitialUpdateGroundTypeBasedOnAsset() {
            Ground_Type determinedGroundType = GetGroundTypeBasedOnCurrentAsset();
            SetGroundType(determinedGroundType, true);
            SetInitialGroundType(determinedGroundType);
        }
        public void SetGroundTilemapVisual(TileBase tileBase, bool updateEdges = false) {
            parentMap.groundTilemap.SetTile(localPlace, tileBase);
            parentMap.groundTilemap.RefreshTile(localPlace);
            if (genericTileObject.mapObjectVisual != null && genericTileObject.mapObjectVisual.usedSprite != null) {
                //if this tile's map object is shown and is showing a visual, update it's sprite to use the updated sprite.
                genericTileObject.mapObjectVisual.SetVisual(parentMap.groundTilemap.GetSprite(localPlace));
            }

            UpdateGroundTypeBasedOnAsset();
            if (updateEdges) {
                CreateSeamlessEdgesForSelfAndNeighbours();
            }
        }
        public void SetStructureTilemapVisual(TileBase tileBase) {
            parentMap.structureTilemap.SetTile(localPlace, tileBase);
            UpdateGroundTypeBasedOnAsset();
        }
        public void CreateSeamlessEdgesForSelfAndNeighbours() {
            CreateSeamlessEdgesForTile(parentMap);
            for (int i = 0; i < neighbourList.Count; i++) {
                LocationGridTile neighbour = neighbourList[i];
                neighbour.CreateSeamlessEdgesForTile(parentMap);
            }
        }
        public void CreateSeamlessEdgesForTile(InnerTileMap map) {
            if (HasCardinalNeighbourOfDifferentGroundType()) {
                BIOMES thisBiome = GetBiomeOfGroundType(groundType);
                Dictionary<GridNeighbourDirection, LocationGridTile> fourNeighbours = FourNeighboursDictionary();
                foreach (KeyValuePair<GridNeighbourDirection, LocationGridTile> keyValuePair in fourNeighbours) {
                    LocationGridTile currNeighbour = keyValuePair.Value;
                    bool createEdge = false;
                    BIOMES otherBiome = GetBiomeOfGroundType(currNeighbour.groundType);
                    if (thisBiome != otherBiome && thisBiome != BIOMES.NONE && otherBiome != BIOMES.NONE) {
                        if (thisBiome == BIOMES.SNOW) {
                            createEdge = true;
                        } else if (thisBiome == BIOMES.GRASSLAND && otherBiome == BIOMES.DESERT) {
                            createEdge = true;
                        }
                    } else {
                        if (groundType != Ground_Type.Cave && groundType != Ground_Type.Structure_Stone && currNeighbour.groundType == Ground_Type.Cave) {
                            createEdge = true;
                        } else if (currNeighbour.tileType == Tile_Type.Wall) {
                            createEdge = false;
                        } else if (groundType != Ground_Type.Water && currNeighbour.groundType == Ground_Type.Water) {
                            createEdge = true;
                        } else if (groundType == Ground_Type.Corrupted && currNeighbour.groundType != Ground_Type.Bone && currNeighbour.groundType != Ground_Type.Corrupted) {
                            createEdge = true;
                        } else if (groundType == Ground_Type.Demon_Stone && currNeighbour.groundType != Ground_Type.Corrupted && currNeighbour.groundType != Ground_Type.Demon_Stone && currNeighbour.groundType != Ground_Type.Bone) {
                            createEdge = true;
                        } else if (groundType == Ground_Type.Bone && (currNeighbour.groundType == Ground_Type.Corrupted || currNeighbour.groundType == Ground_Type.Demon_Stone)) {
                            createEdge = true;
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
                        } else if ((groundType != Ground_Type.Ruined_Stone && groundType != Ground_Type.Structure_Stone) 
                                   && currNeighbour.groundType == Ground_Type.Ruined_Stone) {
                            createEdge = true;
                        }
                    }
                    
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
                        if (InnerMapManager.Instance.assetManager.edgeAssets.ContainsKey(groundType) && 
                            InnerMapManager.Instance.assetManager.edgeAssets[groundType].Count > (int)keyValuePair.Key) {
                            mapToUse.SetTile(localPlace, InnerMapManager.Instance.assetManager.edgeAssets[groundType][(int)keyValuePair.Key]);    
                        }
                    } else {
                        mapToUse.SetTile(localPlace, null);
                    }
                }
            }
            else {
                map.northEdgeTilemap.SetTile(localPlace, null);
                map.southEdgeTilemap.SetTile(localPlace, null);
                map.westEdgeTilemap.SetTile(localPlace, null);
                map.eastEdgeTilemap.SetTile(localPlace, null);
            }
        }
        private BIOMES GetBiomeOfGroundType(Ground_Type groundType) {
            switch (groundType) {
                case Ground_Type.Sand:
                case Ground_Type.Desert_Grass:
                case Ground_Type.Desert_Stone:
                    return BIOMES.DESERT;
                case Ground_Type.Snow:
                case Ground_Type.Snow_Dirt:
                case Ground_Type.Tundra:
                    return BIOMES.SNOW;
                case Ground_Type.Soil:
                case Ground_Type.Grass:
                    return BIOMES.GRASSLAND;
                default:
                    return BIOMES.NONE;
            }
        }
        /// <summary>
        /// Set this tile to the ground that it originally was, aka before anything was put on it.
        /// </summary>
        public void RevertTileToOriginalPerlin() {
             TileBase groundTile = InnerTileMap.GetGroundAssetPerlin(floorSample, biomeType);
             SetGroundTilemapVisual(groundTile);
        }
        public void DetermineNextGroundTypeAfterDestruction() {
            TileBase nextGroundAsset;
            switch (groundType) {
                case Ground_Type.Grass:
                case Ground_Type.Stone:
                case Ground_Type.Water:
                    //if grass or stone or water revert to soil 
                    nextGroundAsset = InnerMapManager.Instance.assetManager.soilTile;
                    break;
                case Ground_Type.Snow:
                case Ground_Type.Snow_Dirt:
                    //if snow revert to tundra
                    nextGroundAsset = InnerMapManager.Instance.assetManager.tundraTile;
                    break;
                case Ground_Type.Cobble:
                case Ground_Type.Wood:
                case Ground_Type.Structure_Stone:
                case Ground_Type.Ruined_Stone:
                case Ground_Type.Demon_Stone:
                case Ground_Type.Flesh:
                case Ground_Type.Cave:
                case Ground_Type.Corrupted:
                case Ground_Type.Bone:
                    //if from structure, revert to original ground asset
                    nextGroundAsset = InnerTileMap.GetGroundAssetPerlin(floorSample, biomeType);
                    break;
                case Ground_Type.Desert_Grass:
                case Ground_Type.Sand:
                    //if desert grass or sand, revert to desert stone
                    nextGroundAsset = InnerMapManager.Instance.assetManager.desertStoneGroundTile;
                    break;
                default:
                    nextGroundAsset = null;
                    break;
            }
            if (nextGroundAsset != null) {
                SetGroundTilemapVisual(nextGroundAsset);
                CreateSeamlessEdgesForSelfAndNeighbours();
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
                } else if (tileState == Tile_State.Occupied && state == Tile_State.Empty) { //&& reservedObjectType == TILE_OBJECT_TYPE.NONE
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
                List<Trait> traitOverrideFunctions = genericTileObject.traitContainer.GetTraitOverrideFunctions(TraitManager.Enter_Grid_Tile_Trait);
                if (traitOverrideFunctions != null) {
                    for (int i = 0; i < traitOverrideFunctions.Count; i++) {
                        Trait trait = traitOverrideFunctions[i];
                        trait.OnEnterGridTile(character, genericTileObject);
                    }
                }
            }
            if (hasLandmine) {
                GameManager.Instance.StartCoroutine(TriggerLandmine(character));
            }

            if (!character.movementComponent.cameFromWurmHole) {
                if (objHere != null && objHere is WurmHole wurmHole) {
                    if (wurmHole.wurmHoleConnection.gridTileLocation != null) {
                        wurmHole.TravelThroughWurmHole(character);
                        return;
                    }
                }
            } else {
                character.movementComponent.SetCameFromWurmHole(false);
            }

            if (objHere != null && objHere is Rug rug) {
                //Booby trapped Rug should explode on contact
                //https://trello.com/c/rveaE94E/1942-booby-trapped-rug-should-explode-on-contact
                if(rug.traitContainer.HasTrait("Booby Trapped")) {
                    if(character.currentActionNode != null && character.currentActionNode.target == rug && character.currentActionNode.goapType == INTERACTION_TYPE.REMOVE_TRAP) {
                        //Should not activate booby trap if character is removing it
                    } else {
                        BoobyTrapped boobyTrapped = rug.traitContainer.GetTraitOrStatus<BoobyTrapped>("Booby Trapped");
                        boobyTrapped.DamageTargetByTrap(character, rug);
                    }
                }
            }
            if (hasFreezingTrap && (freezingTrapExclusions == null || !freezingTrapExclusions.Contains(character.race))) {
                TriggerFreezingTrap(character);
            }
            if (hasSnareTrap) {
                TriggerSnareTrap(character);
            }
            if (isCorrupted) {
                if(!character.isDead && character.limiterComponent.canMove && character.limiterComponent.canPerform) {
                    if (!character.movementComponent.hasMovedOnCorruption) {
                        //Corrupted hexes should also be avoided
                        //https://trello.com/c/6WJtivlY/1274-fleeing-should-not-go-to-corrupted-structures
                        //Note: Instead of always fleeing from all corrupted hex tiles all the time, we must only let the characters flee from it if it walks on a corrupted tile
                        //The reason for this is so that if the corrupted hex is too far away the character will not try to run from it, and thus, the flee path will not be messed up
                        //Right now, the flee path sometimes gets messed up when the character tries to run from another character, sometimes they go to the same direction, it is because right now, we always take into account the corrupted hex tile even if they are too far
                        if (character.marker && character.marker.hasFleePath && character.isNormalCharacter) {
                            if (character.gridTileLocation != null && character.gridTileLocation.collectionOwner.isPartOfParentRegionMap) {
                                //TileObject genericTileObject = character.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.GetCenterLocationGridTile().genericTileObject;
                                character.movementComponent.SetHasMovedOnCorruption(true);
                                //character.marker.AddPOIAsInVisionRange(genericTileObject);
                                //character.combatComponent.Flight(genericTileObject, "saw something frightening", forcedFlight: true);
                                //genericTileObject.traitContainer.AddTrait(genericTileObject, "Danger Remnant");
                                character.marker.AddAvoidPositions(character.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.GetCenterLocationGridTile().worldLocation);
                                return;
                            }
                        }
                    }


                    //Reporting does not trigger until Tutorial is over
                    //https://trello.com/c/OmmyR6go/1239-reporting-does-not-trigger-until-tutorial-is-over

                    LocationStructure mostImportantStructureOnTile =
                        collectionOwner.partOfHextile.hexTileOwner.GetMostImportantStructureOnTile();
                    if(mostImportantStructureOnTile is DemonicStructure demonicStructure) {
                        if (!character.behaviourComponent.isAttackingDemonicStructure 
                            && character.homeSettlement != null && character.necromancerTrait == null && character.race.IsSapient()
                            && character.marker != null && character.carryComponent.IsNotBeingCarried() && character.isAlliedWithPlayer == false
                            && (!character.partyComponent.hasParty || !character.partyComponent.currentParty.isActive || (character.partyComponent.currentParty.currentQuest.partyQuestType != PARTY_QUEST_TYPE.Counterattack && character.partyComponent.currentParty.currentQuest.partyQuestType != PARTY_QUEST_TYPE.Rescue)) 
                            //&& !InnerMapManager.Instance.HasWorldKnownDemonicStructure(mostImportantStructureOnTile)
                            && (Tutorial.TutorialManager.Instance.hasCompletedImportantTutorials || WorldSettings.Instance.worldSettingsData.worldType != WorldSettingsData.World_Type.Tutorial)) {
                            if (character.faction != null && character.faction.isMajorNonPlayer && !character.faction.partyQuestBoard.HasPartyQuest(PARTY_QUEST_TYPE.Counterattack) && !character.faction.HasActiveReportDemonicStructureJob(mostImportantStructureOnTile)) {
                                character.jobComponent.CreateReportDemonicStructure(mostImportantStructureOnTile);
                                return;
                            }
                        }
                        //If cannot report flee instead
                        //do not make characters that are allied with the player or attacking a demonic structure flee from corruption.
                        if (!character.behaviourComponent.isAttackingDemonicStructure 
                            && (!character.partyComponent.hasParty || !character.partyComponent.currentParty.isActive || (character.partyComponent.currentParty.currentQuest.partyQuestType != PARTY_QUEST_TYPE.Counterattack && character.partyComponent.currentParty.currentQuest.partyQuestType != PARTY_QUEST_TYPE.Rescue && character.partyComponent.currentParty.currentQuest.partyQuestType != PARTY_QUEST_TYPE.Heirloom_Hunt)) 
                            && character.isAlliedWithPlayer == false 
                            && character.necromancerTrait == null
                            && !character.jobQueue.HasJob(JOB_TYPE.REPORT_CORRUPTED_STRUCTURE)) {
                            if (!character.movementComponent.hasMovedOnCorruption) {
                                character.movementComponent.SetHasMovedOnCorruption(true);
                                if (character.isNormalCharacter) {
                                    //Instead of fleeing when character steps on a corrupted tile, trigger Shocked interrupt only
                                    //The reason for this is to eliminate the bug wherein the character will flee from the corrupted tile, then after fleeing, he will again move across it, thus triggering flee again, which results in unending loop of fleeing and moving
                                    //So to eliminate this behaviour we will not let the character flee, but will trigger Shocked interrupt only and then go on with his job/action
                                    //https://trello.com/c/yiW344Sb/2499-villagers-fleeing-from-demonic-area-can-get-stuck-repeating-it
                                    character.interruptComponent.TriggerInterrupt(INTERRUPT.Shocked, character);
                                    //genericTileObject.traitContainer.AddTrait(genericTileObject, "Danger Remnant");

                                    //if (character.characterClass.IsCombatant()) {
                                    //    character.behaviourComponent.SetIsAttackingDemonicStructure(true, demonicStructure);
                                    //} else {
                                    //    genericTileObject.traitContainer.AddTrait(genericTileObject, "Danger Remnant");
                                    //}
                                }
                            }
                        }
                    }
                }
            } else {
                character.movementComponent.SetHasMovedOnCorruption(false);
            }
        }
        public void RemoveCharacterHere(Character character) {
            charactersHere.Remove(character);
        }
        public bool IsInHomeOf(Character character) {
            if (character.homeSettlement != null) {
                return IsPartOfSettlement(character.homeSettlement);
            } else if (character.homeStructure != null) {
                return structure == character.homeStructure;
            } else if (character.HasTerritory()) {
                if (collectionOwner.isPartOfParentRegionMap) {
                    HexTile hex = collectionOwner.partOfHextile.hexTileOwner;
                    return hex == character.territory;
                }
            }
            return false;
        }
        #endregion

        #region Points of Interest
        public void SetObjectHere(IPointOfInterest poi) {
            bool isPassablePreviously = IsPassable();
            if (poi is TileObject tileObject) {
                if (tileObject.OccupiesTile()) {
                    objHere = poi;
                }
            } else {
                objHere = poi;    
            }
            
            poi.SetGridTileLocation(this);
            poi.OnPlacePOI();
            SetTileState(Tile_State.Occupied);
            if (!IsPassable()) {
                structure.RemovePassableTile(this);
            } else if (IsPassable() && !isPassablePreviously) {
                structure.AddPassableTile(this);
            }
            Messenger.Broadcast(GridTileSignals.OBJECT_PLACED_ON_TILE, this, poi);
        }
        public void LoadObjectHere(IPointOfInterest poi) {
            bool isPassablePreviously = IsPassable();
            if (poi is TileObject tileObject) {
                if (tileObject.OccupiesTile()) {
                    objHere = poi;
                }
            } else {
                objHere = poi;    
            }
            
            poi.SetGridTileLocation(this);
            poi.OnLoadPlacePOI();
            SetTileState(Tile_State.Occupied);
            if (!IsPassable()) {
                structure.RemovePassableTile(this);
            } else if (IsPassable() && !isPassablePreviously) {
                structure.AddPassableTile(this);
            }
            Messenger.Broadcast(GridTileSignals.OBJECT_PLACED_ON_TILE, this, poi);
        }
        public IPointOfInterest RemoveObjectHere(Character removedBy) {
            if (objHere != null) {
                IPointOfInterest removedObj = objHere;
                objHere = null;
                if (removedObj is TileObject tileObject) {
                    //if the object in this tile is a tile object and it was removed by a character, use tile object specific function
                    tileObject.RemoveTileObject(removedBy);
                } else {
                    removedObj.SetGridTileLocation(null);
                    removedObj.OnDestroyPOI();
                }
                SetTileState(Tile_State.Empty);
                Messenger.Broadcast(CharacterSignals.STOP_CURRENT_ACTION_TARGETING_POI, removedObj);
                Messenger.Broadcast(SpellSignals.RELOAD_PLAYER_ACTIONS, removedObj as IPlayerActionTarget);
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
                if (removedObj is TileObject tileObject) {
                    tileObject.OnRemoveTileObject(null, gridTile, false, false);
                }
                removedObj.SetPOIState(POI_STATE.INACTIVE);
                Messenger.Broadcast(SpellSignals.RELOAD_PLAYER_ACTIONS, removedObj as IPlayerActionTarget);
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
                if (removedObj is TileObject removedTileObj) {
                    removedTileObj.OnRemoveTileObject(null, gridTile, false, false);
                    removedTileObj.DestroyMapVisualGameObject();
                }
                removedObj.SetPOIState(POI_STATE.INACTIVE);
                Messenger.Broadcast(CharacterSignals.STOP_CURRENT_ACTION_TARGETING_POI_EXCEPT_ACTOR, removedObj, remover);
                Messenger.Broadcast(SpellSignals.RELOAD_PLAYER_ACTIONS, removedObj as IPlayerActionTarget);
                return removedObj;
            }
            return null;
        }
        #endregion

        #region Utilities
        public LocationGridTile GetNeighbourAtDirection(GridNeighbourDirection dir) {
            if (_neighbours.ContainsKey(dir)) {
                return _neighbours[dir];
            }
            return null;
        }
        public List<LocationGridTile> GetCrossNeighbours() {
            List<LocationGridTile> crossNeighbours = new List<LocationGridTile>();
            if (_neighbours.ContainsKey(GridNeighbourDirection.North)) {
                crossNeighbours.Add(_neighbours[GridNeighbourDirection.North]);
            }
            if (_neighbours.ContainsKey(GridNeighbourDirection.South)) {
                crossNeighbours.Add(_neighbours[GridNeighbourDirection.South]);
            }
            if (_neighbours.ContainsKey(GridNeighbourDirection.East)) {
                crossNeighbours.Add(_neighbours[GridNeighbourDirection.East]);
            }
            if (_neighbours.ContainsKey(GridNeighbourDirection.West)) {
                crossNeighbours.Add(_neighbours[GridNeighbourDirection.West]);
            }
            return crossNeighbours;
        }
        public bool TryGetNeighbourDirection(LocationGridTile tile, out GridNeighbourDirection dir) {
            foreach (KeyValuePair<GridNeighbourDirection, LocationGridTile> keyValuePair in _neighbours) {
                if (keyValuePair.Value == tile) {
                    dir = keyValuePair.Key;
                    return true;
                }
            }
            dir = GridNeighbourDirection.East;
            return false;
        }
        public bool IsAtEdgeOfMap() {
            GridNeighbourDirection[] dirs = CollectionUtilities.GetEnumValues<GridNeighbourDirection>();
            for (int i = 0; i < dirs.Length; i++) {
                if (!_neighbours.ContainsKey(dirs[i])) {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Does this tile have a neighbour that is part of a different structure, or is part of the outside map?
        /// </summary>
        public bool HasDifferentDwellingOrOutsideNeighbour() {
            foreach (KeyValuePair<GridNeighbourDirection, LocationGridTile> kvp in _neighbours) {
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
            if(structure.region != tile.structure.region) {
                //Computing distance of tiles from different region should be different
                //It should origin tile distance to origin tile region's edge tile + target tile region's edge tile to target tile
                Region targetRegion = tile.structure.region;
                LocationGridTile targetGate = GetTargetTileToGoToRegion(targetRegion);
                LocationGridTile exitTile = GetExitTileToGoToRegion(targetGate);

                float originDistanceToExitTile = Vector2.Distance(localLocation, exitTile.localLocation);
                float targetGateDistanceToTargetTile = Vector2.Distance(targetGate.localLocation, tile.localLocation);

                return originDistanceToExitTile + targetGateDistanceToTargetTile;
            } else {
                return Vector2.Distance(localLocation, tile.localLocation);
            }
        }
        public bool HasOccupiedNeighbour() {
            for (int i = 0; i < neighbourList.Count; i++) {
                LocationGridTile tile = neighbourList[i];
                if (tile.isOccupied) {
                    return true;
                }
            }
            return false;
        }
        public bool HasUnoccupiedNeighbour(out List<LocationGridTile> unoccupiedTiles, bool sameStructure = false) {
            bool hasUnoccupied = false;
            unoccupiedTiles = new List<LocationGridTile>();
            for (int i = 0; i < neighbourList.Count; i++) {
                LocationGridTile tile = neighbourList[i];
                if (tile.isOccupied == false) {
                    if (sameStructure) {
                        //if same structure switch is on, check if the neighbour is at the same structure
                        //as this tile before adding to list
                        if (tile.structure != structure) {
                            continue; //skip neighbour
                        }
                    }
                    unoccupiedTiles.Add(tile);
                    hasUnoccupied = true;
                }
            }
            return hasUnoccupied;
        }
        public bool HasNeighbourOfElevation(ELEVATION elevation, bool useFourNeighbours = false) {
            Dictionary<GridNeighbourDirection, LocationGridTile> n = _neighbours;
            if (useFourNeighbours) {
                n = FourNeighboursDictionary();
            }
            for (int i = 0; i < n.Values.Count; i++) {
                if (_neighbours.Values.ElementAt(i).collectionOwner.partOfHextile.hexTileOwner &&
                    _neighbours.Values.ElementAt(i).collectionOwner.partOfHextile.hexTileOwner.elevationType == elevation) {
                    return true;
                }
            }
            return false;
        }
        public bool HasNeighbourOfType(Tile_Type type, bool useFourNeighbours = false) {
            Dictionary<GridNeighbourDirection, LocationGridTile> n = _neighbours;
            if (useFourNeighbours) {
                n = FourNeighboursDictionary();
            }
            for (int i = 0; i < n.Values.Count; i++) {
                if (_neighbours.Values.ElementAt(i).tileType == type) {
                    return true;
                }
            }
            return false;
        }
        public int GetCountNeighboursOfType(Tile_Type type, bool useFourNeighbours = false) {
            int count = 0;
            Dictionary<GridNeighbourDirection, LocationGridTile> n = _neighbours;
            if (useFourNeighbours) {
                n = FourNeighboursDictionary();
            }
            for (int i = 0; i < n.Values.Count; i++) {
                if (_neighbours.Values.ElementAt(i).tileType == type) {
                    count++;
                }
            }
            return count;
        }
        public bool HasNeighbourOfType(Ground_Type type, bool useFourNeighbours = false) {
            Dictionary<GridNeighbourDirection, LocationGridTile> n = _neighbours;
            if (useFourNeighbours) {
                n = FourNeighboursDictionary();
            }
            for (int i = 0; i < n.Values.Count; i++) {
                if (_neighbours.Values.ElementAt(i).groundType == type) {
                    return true;
                }
            }
            return false;
        }
        public bool HasNeighbourNotInList(List<LocationGridTile> list, bool useFourNeighbours = false) {
            Dictionary<GridNeighbourDirection, LocationGridTile> n = _neighbours;
            if (useFourNeighbours) {
                n = FourNeighboursDictionary();
            }
            for (int i = 0; i < n.Values.Count; i++) {
                if (list.Contains(_neighbours.Values.ElementAt(i)) == false) {
                    return true;
                }
            }
            return false;
        }
        public bool HasDifferentStructureNeighbour(bool useFourNeighbours = false) {
            Dictionary<GridNeighbourDirection, LocationGridTile> n = _neighbours;
            if (useFourNeighbours) {
                n = FourNeighboursDictionary();
            }
            for (int i = 0; i < n.Values.Count; i++) {
                LocationGridTile tile = n.Values.ElementAt(i);
                if (tile.structure != structure) {
                    return true;
                }
            }
            return false;
        }
        public bool IsNeighbour(LocationGridTile tile, bool sameStructureOnly = false) {
            if (sameStructureOnly) {
                for (int i = 0; i < neighbourList.Count; i++) {
                    LocationGridTile neighbour = neighbourList[i];
                    if (neighbour == tile) {
                        if ((structure.structureType.IsOpenSpace() && tile.structure.structureType.IsOpenSpace()) || structure == tile.structure) {
                            return true;
                        }
                    }
                }
            } else {
                for (int i = 0; i < neighbourList.Count; i++) {
                    LocationGridTile neighbour = neighbourList[i];
                    if (neighbour == tile) {
                        return true;
                    }
                }
            }

            return false;
            //foreach (KeyValuePair<GridNeighbourDirection, LocationGridTile> keyValuePair in _neighbours) {
            //    if (keyValuePair.Value == tile) {
            //        return true;
            //    }
            //}
            //return false;
        }
        public bool IsAdjacentTo(Type type) {
            foreach (KeyValuePair<GridNeighbourDirection, LocationGridTile> keyValuePair in _neighbours) {
                if ((keyValuePair.Value.objHere != null && keyValuePair.Value.objHere.GetType() == type)) {
                    return true;
                }
            }
            return false;
        }
        public bool HasNeighbouringWalledStructure() {
            foreach (KeyValuePair<GridNeighbourDirection, LocationGridTile> keyValuePair in _neighbours) {
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
        public LocationStructure GetNearestInteriorStructureFromThis() {
            LocationStructure nearestStructure = null;
            if (structure != null) {
                if (structure.region.allStructures.Count > 0) {
                    float nearestDist = 99999f;
                    for (int i = 0; i < structure.region.allStructures.Count; i++) {
                        LocationStructure currStructure = structure.region.allStructures[i];
                        if (currStructure != structure && currStructure.isInterior) {
                            LocationGridTile randomPassableTile = currStructure.GetRandomPassableTile();
                            if (randomPassableTile != null && PathfindingManager.Instance.HasPath(this, randomPassableTile)) {
                                float dist = Vector2.Distance(randomPassableTile.localLocation, localLocation);
                                if (nearestStructure == null || dist < nearestDist) {
                                    nearestStructure = currStructure;
                                    nearestDist = dist;
                                }
                            }
                        }
                    }
                }
            }
            return nearestStructure;
        }
        public LocationStructure GetNearestInteriorStructureFromThisExcept(List<LocationStructure> exclusions) {
            LocationStructure nearestStructure = null;
            if (structure != null) {
                if (structure.region.allStructures.Count > 0) {
                    float nearestDist = 99999f;
                    for (int i = 0; i < structure.region.allStructures.Count; i++) {
                        LocationStructure currStructure = structure.region.allStructures[i];
                        if (currStructure != structure && currStructure.isInterior) {
                            if (exclusions != null && exclusions.Contains(currStructure)) {
                                continue;
                            }
                            LocationGridTile randomPassableTile = currStructure.GetRandomPassableTile();
                            if (randomPassableTile != null && PathfindingManager.Instance.HasPath(this, randomPassableTile)) {
                                float dist = Vector2.Distance(randomPassableTile.localLocation, localLocation);
                                if (nearestStructure == null || dist < nearestDist) {
                                    nearestStructure = currStructure;
                                    nearestDist = dist;
                                }
                            }
                        }
                    }
                }
            }
            return nearestStructure;
        }
        public LocationStructure GetNearestVillageStructureFromThisWithResidents(Character relativeTo = null) {
            LocationStructure nearestStructure = null;
            if (structure != null) {
                if (structure.region.allStructures.Count > 0) {
                    float nearestDist = 99999f;
                    for (int i = 0; i < structure.region.allStructures.Count; i++) {
                        LocationStructure currStructure = structure.region.allStructures[i];
                        if (currStructure != structure && currStructure.settlementLocation != null && !(currStructure.settlementLocation is PlayerSettlement)
                            && currStructure.settlementLocation.owner != null && currStructure.settlementLocation.residents.Count > 0) {
                            if (currStructure.settlementLocation.owner.isMajorNonPlayer) {
                                LocationGridTile randomPassableTile = currStructure.GetRandomPassableTile();
                                if (randomPassableTile != null && ((relativeTo != null && relativeTo.movementComponent.HasPathTo(randomPassableTile)) || PathfindingManager.Instance.HasPath(this, randomPassableTile))) {
                                    float dist = Vector2.Distance(randomPassableTile.localLocation, localLocation);
                                    if (nearestStructure == null || dist < nearestDist) {
                                        nearestStructure = currStructure;
                                        nearestDist = dist;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return nearestStructure;
        }
        public LocationGridTile GetNearestUnoccupiedTileFromThisWithStructure(STRUCTURE_TYPE structureType) {
            List<LocationGridTile> unoccupiedNeighbours = UnoccupiedNeighbours;
            if (unoccupiedNeighbours.Count == 0) {
                if (structure != null) {
                    LocationGridTile nearestTile = null;
                    float nearestDist = 99999f;
                    for (int i = 0; i < structure.unoccupiedTiles.Count; i++) {
                        LocationGridTile currTile = structure.unoccupiedTiles.ElementAt(i);
                        if (currTile != this && currTile.groundType != Ground_Type.Water && currTile.structure != null && currTile.structure.structureType == structureType) {
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
            if (IsAtEdgeOfWalkableMap()) {
                return this;
            }

            LocationGridTile nearestEdgeTile = null;
            List<LocationGridTile> neighbours = neighbourList;
            for (int i = 0; i < neighbours.Count; i++) {
                if (neighbours[i].IsAtEdgeOfWalkableMap()) {
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
                        nearestEdgeTile = currTile;
                        nearestDist = dist;
                    }
                }
            }
            return nearestEdgeTile;
        }
        public LocationGridTile GetNearestEdgeTileFromThis(DIRECTION direction) {
            if (IsInEdgeOfDirection(direction)) {
                return this;
            }

            LocationGridTile nearestEdgeTile = null;
            List<LocationGridTile> neighbours = neighbourList;
            for (int i = 0; i < neighbours.Count; i++) {
                if (neighbours[i].IsInEdgeOfDirection(direction)) {
                    nearestEdgeTile = neighbours[i];
                    break;
                }
            }
            if (nearestEdgeTile == null) {
                float nearestDist = -999f;
                for (int i = 0; i < parentMap.allEdgeTiles.Count; i++) {
                    LocationGridTile currTile = parentMap.allEdgeTiles[i];
                    if (currTile.IsInEdgeOfDirection(direction)) {
                        float dist = Vector2.Distance(currTile.localLocation, localLocation);
                        if (nearestDist == -999f || dist < nearestDist) {
                            nearestEdgeTile = currTile;
                            nearestDist = dist;
                        }
                    }
                }
            }
            return nearestEdgeTile;
        }
        public bool IsInEdgeOfDirection(DIRECTION direction) {
            if(direction == DIRECTION.RIGHT) {
                return localPlace.x == (parentMap.width - 1);
            } else if (direction == DIRECTION.LEFT) {
                return localPlace.x == 0;
            } else if (direction == DIRECTION.UP) {
                return localPlace.y == (parentMap.height - 1);
            } else if (direction == DIRECTION.DOWN) {
                return localPlace.y == 0;
            }
            return false;
        }
        public DIRECTION GetDirection() {
            if (localPlace.x == (parentMap.width - 1)) {
                return DIRECTION.RIGHT;
            } else if (localPlace.x == 0) {
                return DIRECTION.LEFT;
            } else if (localPlace.y == (parentMap.height - 1)) {
                return DIRECTION.UP;
            } else if (localPlace.y == 0) {
                return DIRECTION.DOWN;
            }
            throw new Exception("Cannot get the direction of " + ToString() + " in " + structure.region + " because this is not an edge tile");
        }
        public LocationGridTile GetRandomUnoccupiedNeighbor() {
            List<LocationGridTile> unoccupiedNeighbours = UnoccupiedNeighbours;
            if (unoccupiedNeighbours.Count > 0) {
                return unoccupiedNeighbours[Random.Range(0, unoccupiedNeighbours.Count)];
            }
            return null;
        }
        public LocationGridTile GetFirstNoObjectNeighbor(bool thisStructureOnly = false) {
            for (int i = 0; i < neighbourList.Count; i++) {
                LocationGridTile neighbor = neighbourList[i];
                if (!thisStructureOnly || neighbor.structure == structure) {
                    if (neighbor.objHere == null) {
                        return neighbor;
                    }
                }
            }
            return null;
        }
        public LocationGridTile GetFirstNearestTileFromThisWithNoObject(bool thisStructureOnly = false, LocationGridTile exception = null) {
            return GetFirstNearestTileFromThisWithNoObjectBase(thisStructureOnly, new List<LocationGridTile>(), exception);
        }
        private LocationGridTile GetFirstNearestTileFromThisWithNoObjectBase(bool thisStructureOnly, List<LocationGridTile> checkedTiles, LocationGridTile exception) {
            if (!checkedTiles.Contains(this)) {
                checkedTiles.Add(this);

                if (objHere == null && this != exception) {
                    return this;
                }
                LocationGridTile chosenTile = GetFirstNoObjectNeighbor(thisStructureOnly);
                if (chosenTile != null) {
                    return chosenTile;
                } else {
                    for (int i = 0; i < neighbourList.Count; i++) {
                        LocationGridTile neighbor = neighbourList[i];
                        if (neighbor == exception) {
                            continue; //skip exception tile.
                        }
                        if (!thisStructureOnly || neighbor.structure == structure) {
                            chosenTile = neighbor.GetFirstNearestTileFromThisWithNoObjectBase(thisStructureOnly, checkedTiles, exception);
                            if (chosenTile != null) {
                                return chosenTile;
                            }
                        }
                    }
                }

                //LocationGridTile chosenTile = GetFirstNoObjectNeighbor(structureOnly);
                //if (chosenTile != null) {
                //    return chosenTile;
                //} else {
                //    for (int i = 0; i < neighbourList.Count; i++) {
                //        LocationGridTile neighbor = neighbourList[i];
                //        chosenTile = neighbor.GetFirstNoObjectNeighbor(structureOnly);
                //        if (chosenTile != null) {
                //            return chosenTile;
                //        }
                //    }
                //    for (int i = 0; i < neighbourList.Count; i++) {
                //        LocationGridTile neighbor = neighbourList[i];
                //        chosenTile = neighbor.GetFirstNearestTileFromThisWithNoObjectBase(structureOnly, checkedTiles);
                //        if (chosenTile != null) {
                //            return chosenTile;
                //        }
                //    }
                //}
            }
            return null;
        }
        public LocationGridTile GetRandomNeighbor() {
            return neighbourList[Random.Range(0, neighbourList.Count)];
        }
        public LocationGridTile GetFirstNeighbor(bool sameStructure = true) {
            for (int i = 0; i < neighbourList.Count; i++) {
                LocationGridTile neighbour = neighbourList[i];
                if(!sameStructure || structure == neighbour.structure) {
                    return neighbour;
                }
            }
            return null;
        }
        public LocationGridTile GetFirstNeighborThatMeetCriteria(Func<LocationGridTile, bool> criteria) {
            for (int i = 0; i < neighbourList.Count; i++) {
                LocationGridTile neighbour = neighbourList[i];
                if (criteria.Invoke(neighbour)) {
                    return neighbour;
                }
            }
            return null;
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
        private bool HasCardinalNeighbourOfDifferentGroundType() {
            Dictionary<GridNeighbourDirection, LocationGridTile> cardinalNeighbours = FourNeighboursDictionary();
            foreach (KeyValuePair<GridNeighbourDirection, LocationGridTile> keyValuePair in cardinalNeighbours) {
                if (keyValuePair.Value.groundType != groundType) {
                    return true;
                }
            }
            return false;
        }
        public List<ITraitable> GetTraitablesOnTile() {
            List<ITraitable> traitables = new List<ITraitable>();
            traitables.Add(genericTileObject);
            for (int i = 0; i < walls.Count; i++) {
                StructureWallObject structureWallObject = walls[i];
                traitables.Add(structureWallObject);
            }
            if (objHere != null) {
                if ((objHere is TileObject tileObject && tileObject.mapObjectState == MAP_OBJECT_STATE.BUILT)) {//|| (objHere is SpecialToken && (objHere as SpecialToken).mapObjectState == MAP_OBJECT_STATE.BUILT)
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
            for (int i = 0; i < charactersHere.Count; i++) {
                Character character = charactersHere[i];
                if (objHere is Tombstone tombstone && tombstone.character == character) {
                    //NOTE: Skip characters in tombstone when damaging character's here. //TODO: This is a quick fix
                    continue;
                }
                callback.Invoke(character);
            }
            if (objHere is TileObject tileObject && tileObject.mapObjectState == MAP_OBJECT_STATE.BUILT) {
                callback.Invoke(objHere);
                //Sleeping characters in bed should also receive damage
                //https://trello.com/c/kFZAHo11/1203-sleeping-characters-in-bed-should-also-receive-damage
                if (tileObject is BaseBed bed) {
                    if (bed.users != null && bed.users.Length > 0) {
                        for (int i = 0; i < bed.users.Length; i++) {
                            Character user = bed.users[i];
                            //Should only apply if user is not part of charactersHere list so that no duplicate calls shall take place
                            if (!charactersHere.Contains(user)) {
                                callback.Invoke(user);
                            }
                        }
                    }
                }
            }
            Messenger.Broadcast(GridTileSignals.ACTION_PERFORMED_ON_TILE_TRAITABLES, this, callback);
        }
        public List<IPointOfInterest> GetPOIsOnTile() {
            List<IPointOfInterest> pois = new List<IPointOfInterest>();
            pois.Add(genericTileObject);
            if (objHere != null) {
                if ((objHere is TileObject tileObject && tileObject.mapObjectState == MAP_OBJECT_STATE.BUILT)) {
                    pois.Add(tileObject);
                }
            }
            for (int i = 0; i < charactersHere.Count; i++) {
                Character character = charactersHere[i];
                pois.Add(character);
            }
            return pois;
        }
        public void AddTraitToAllPOIsOnTile(string traitName) {
            genericTileObject.traitContainer.AddTrait(genericTileObject, traitName);
            if (objHere != null) {
                if ((objHere is TileObject tileObject && tileObject.mapObjectState == MAP_OBJECT_STATE.BUILT)) {
                    tileObject.traitContainer.AddTrait(objHere, traitName);
                }
            }
            for (int i = 0; i < charactersHere.Count; i++) {
                Character character = charactersHere[i];
                character.traitContainer.AddTrait(character, traitName);
            }
        }
        public int GetNeighbourOfTypeCount(Ground_Type type, bool useFourNeighbours = false) {
            int count = 0;
            Dictionary<GridNeighbourDirection, LocationGridTile> n = _neighbours;
            if (useFourNeighbours) {
                n = FourNeighboursDictionary();
            }
            for (int i = 0; i < n.Values.Count; i++) {
                if (_neighbours.Values.ElementAt(i).groundType == type) {
                    count++;
                }
            }
            return count;
        }
        public bool IsPartOfSettlement(out BaseSettlement settlement) {
            if (collectionOwner.isPartOfParentRegionMap && collectionOwner.partOfHextile.hexTileOwner.settlementOnTile != null) {
                settlement = collectionOwner.partOfHextile.hexTileOwner.settlementOnTile;
                return true;
            }
            settlement = null;
            return false;
        }
        public bool IsPartOfSettlement(BaseSettlement settlement) {
            return collectionOwner.isPartOfParentRegionMap && collectionOwner.partOfHextile.hexTileOwner.settlementOnTile == settlement;
        }
        public bool IsPartOfSettlement() {
            return collectionOwner.isPartOfParentRegionMap && collectionOwner.partOfHextile.hexTileOwner.settlementOnTile != null;
        }
        public bool IsPartOfHumanElvenSettlement() {
            return collectionOwner.isPartOfParentRegionMap && collectionOwner.partOfHextile.hexTileOwner.settlementOnTile != null && collectionOwner.partOfHextile.hexTileOwner.settlementOnTile.locationType == LOCATION_TYPE.VILLAGE;
        }
        public bool IsPartOfActiveHumanElvenSettlement() {
            return IsPartOfHumanElvenSettlement() && collectionOwner.partOfHextile.hexTileOwner.settlementOnTile.residents.Count > 0;
        }
        public bool IsNextToSettlement(out BaseSettlement settlement) {
            for (int i = 0; i < neighbourList.Count; i++) {
                LocationGridTile tile = neighbourList[i];
                if (tile.IsPartOfSettlement(out settlement)) {
                    return true;
                }
            }
            settlement = null;
            return false;
        }
        public bool IsNextToSettlement() {
            for (int i = 0; i < neighbourList.Count; i++) {
                LocationGridTile tile = neighbourList[i];
                if (tile.IsPartOfSettlement()) {
                    return true;
                }
            }
            return false;
        }
        public bool IsNextToSettlement(BaseSettlement settlement) {
            for (int i = 0; i < neighbourList.Count; i++) {
                LocationGridTile tile = neighbourList[i];
                if (tile.IsPartOfSettlement(settlement)) {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Is this tile part of an area that is next to the given settlement.
        /// </summary>
        /// <param name="settlement">The settlement to check</param>
        /// <returns>True or false.</returns>
        public bool IsNextToSettlementArea(BaseSettlement settlement) {
            if (collectionOwner.isPartOfParentRegionMap == false) {
                return false;
            }
            HexTile hexTile = collectionOwner.partOfHextile.hexTileOwner;
            for (int i = 0; i < hexTile.AllNeighbours.Count; i++) {
                HexTile neighbour = hexTile.AllNeighbours[i];
                if (neighbour.settlementOnTile == settlement) {
                    return true;
                }
            }
            return false;
        }
        public bool IsNextToOrPartOfSettlement(out BaseSettlement settlement) {
            return IsPartOfSettlement(out settlement) || IsNextToSettlement(out settlement);
        }
        public bool IsNextToOrPartOfSettlement() {
            return IsPartOfSettlement() || IsNextToSettlement();
        }
        public bool IsNextToOrPartOfSettlement(BaseSettlement settlement) {
            return IsPartOfSettlement(settlement) || IsNextToSettlement(settlement);
        }
        public bool IsNextToSettlementAreaOrPartOfSettlement(BaseSettlement settlement) {
            return IsPartOfSettlement(settlement) || IsNextToSettlementArea(settlement);
        }
        public List<LocationGridTile> GetTilesInRadius(int radius, int radiusLimit = 0, bool includeCenterTile = false, bool includeTilesInDifferentStructure = false, bool includeImpassable = true) {
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
                        if(!includeImpassable && !result.IsPassable()) { continue; }
                        tiles.Add(result);
                    }
                }
            }
            return tiles;
        }
        public bool IsPassable() {
            return (objHere == null || !(objHere is BlockWall)) && groundType != Ground_Type.Water;
        }
        private LocationGridTile GetTargetTileToGoToRegion(Region region) {
            //if (currentRegion != null) {
            //    RegionInnerTileMap regionInnerTileMap = currentRegion.innerMap as RegionInnerTileMap;
            //    if (regionInnerTileMap != null) {
            //        return regionInnerTileMap.GetTileToGoToRegion(region);
            //    }
            //} else if (gridTileLocation != null) {
            //    RegionInnerTileMap regionInnerTileMap = gridTileLocation.parentMap.region.innerMap as RegionInnerTileMap;
            //    if (regionInnerTileMap != null) {
            //        return regionInnerTileMap.GetTileToGoToRegion(region);
            //    }
            //}
            RegionInnerTileMap regionInnerTileMap = structure.region.innerMap as RegionInnerTileMap;
            if (regionInnerTileMap != null) {
                return regionInnerTileMap.GetTileToGoToRegion(region);
            }
            return null;
        }
        public LocationGridTile GetExitTileToGoToRegion(Region region) {
            //gate -the tile where the character will appear in the target region
            LocationGridTile gate = GetTargetTileToGoToRegion(region);
            return GetExitTileToGoToRegion(gate);
        }
        public LocationGridTile GetExitTileToGoToRegion(LocationGridTile gateInTargetRegion) {
            //direction - the direction where the character must go in order to go to the other region, it is also the basis in which we get the tile where the character will exit in this region
            DIRECTION direction = gateInTargetRegion.GetDirection();
            LocationGridTile exitTile = GetNearestEdgeTileFromThis(direction);
            return exitTile;
        }
        #endregion

        #region Walls
        public void AddWallObject(StructureWallObject structureWallObject) {
            walls.Add(structureWallObject);
        }
        public void ClearWallObjects() {
            walls.Clear();
        }
        #endregion

        #region Corruption
        public void CorruptTile() {
            SetGroundTilemapVisual(InnerMapManager.Instance.assetManager.corruptedTile);
            CreateSeamlessEdgesForSelfAndNeighbours();
            if (objHere != null) {
                if (objHere is TreeObject tree) {
                    (tree.mapObjectVisual as TileObjectGameObject).UpdateTileObjectVisual(tree);
                } else if (objHere is BlockWall blockWall) {
                    blockWall.SetWallType(WALL_TYPE.Demon_Stone);
                    blockWall.UpdateVisual(this);
                } else {
                    if (objHere is TileObject tileObject) {
                        if (objHere is Tombstone tombstone) {
                            tombstone.SetRespawnCorpseOnDestroy(false);
                        }
                        if (!tileObject.tileObjectType.IsTileObjectImportant() && !tileObject.traitContainer.HasTrait("Indestructible")) {
                            structure.RemovePOI(objHere);    
                        }    
                    }
                    
                    // structure.RemovePOI(objHere);
                }
            }
        }
        public void UnCorruptTile() {
            RevertTileToOriginalPerlin();
            CreateSeamlessEdgesForSelfAndNeighbours();
            if (objHere != null) {
                if (objHere is TileObject tileObject) {
                    if (!tileObject.traitContainer.HasTrait("Indestructible")) {
                        structure.RemovePOI(objHere);    
                    }
                } else {
                    structure.RemovePOI(objHere);
                }
            }
        }
        #endregion

        #region Landmine
        public void SetHasLandmine(bool state) {
            if(hasLandmine != state) {
                SetIsDefault(false);
                hasLandmine = state;
                if (hasLandmine) {
                    _landmineEffect = GameManager.Instance.CreateParticleEffectAt(this, PARTICLE_EFFECT.Landmine, InnerMapManager.DetailsTilemapSortingOrder - 1);
                } else {
                    ObjectPoolManager.Instance.DestroyObject(_landmineEffect);
                    _landmineEffect = null;
                }
            }
        }
        private IEnumerator TriggerLandmine(Character triggeredBy) {
            GameManager.Instance.CreateParticleEffectAt(this, PARTICLE_EFFECT.Landmine_Explosion);
            genericTileObject.traitContainer.AddTrait(genericTileObject, "Danger Remnant");
            yield return new WaitForSeconds(0.5f);
            SetHasLandmine(false);
            List<LocationGridTile> tiles = GetTilesInRadius(3, includeCenterTile: true, includeTilesInDifferentStructure: true);
            BurningSource bs = null;
            for (int i = 0; i < tiles.Count; i++) {
                LocationGridTile tile = tiles[i];
                List<IPointOfInterest> pois = tile.GetPOIsOnTile();
                for (int j = 0; j < pois.Count; j++) {
                    IPointOfInterest poi = pois[j];
                    if (poi.gridTileLocation == null) {
                        continue; //skip
                    }
                    if (poi is TileObject obj) {
                        if (obj.tileObjectType != TILE_OBJECT_TYPE.GENERIC_TILE_OBJECT) {
                            obj.AdjustHP(-500, ELEMENTAL_TYPE.Normal, true,
                                elementalTraitProcessor: (target, trait) => TraitManager.Instance.ProcessBurningTrait(target, trait, ref bs), showHPBar: true);
                        } else {
                            CombatManager.Instance.ApplyElementalDamage(0, ELEMENTAL_TYPE.Normal, obj,
                                elementalTraitProcessor: (target, trait) => TraitManager.Instance.ProcessBurningTrait(target, trait, ref bs));
                        }
                    } else if (poi is Character character) {
                        character.AdjustHP(-500, ELEMENTAL_TYPE.Normal, true,
                            elementalTraitProcessor: (target, trait) => TraitManager.Instance.ProcessBurningTrait(target, trait, ref bs), showHPBar: true);
                    } else {
                        poi.AdjustHP(-500, ELEMENTAL_TYPE.Normal, true,
                            elementalTraitProcessor: (target, trait) => TraitManager.Instance.ProcessBurningTrait(target, trait, ref bs), showHPBar: true);
                    }
                }
            }
        }
        #endregion

        #region Freezing Trap
        public void SetHasFreezingTrap(bool state, params RACE[] freezingTrapExclusions) {
            if (hasFreezingTrap != state) {
                SetIsDefault(false);
                hasFreezingTrap = state;
                if (hasFreezingTrap) {
                    if (collectionOwner.isPartOfParentRegionMap) {
                        collectionOwner.partOfHextile.hexTileOwner.AddFreezingTrapInHexTile();
                    }
                    this.freezingTrapExclusions = new List<RACE>(freezingTrapExclusions);
                    _freezingTrapEffect = GameManager.Instance.CreateParticleEffectAt(this, PARTICLE_EFFECT.Freezing_Trap, InnerMapManager.DetailsTilemapSortingOrder - 1);
                } else {
                    if (collectionOwner.isPartOfParentRegionMap) {
                        collectionOwner.partOfHextile.hexTileOwner.RemoveFreezingTrapInHexTile();
                    }
                    ObjectPoolManager.Instance.DestroyObject(_freezingTrapEffect);
                    _freezingTrapEffect = null;
                    this.freezingTrapExclusions = null;
                }
            }
        }
        private void TriggerFreezingTrap(Character triggeredBy) {
            GameManager.Instance.CreateParticleEffectAt(triggeredBy, PARTICLE_EFFECT.Freezing_Trap_Explosion);
            AudioManager.Instance.TryCreateAudioObject(PlayerSkillManager.Instance.GetPlayerSkillData<FreezingTrapSkillData>(PLAYER_SKILL_TYPE.FREEZING_TRAP).trapExplosionSound, this, 1, false);
            SetHasFreezingTrap(false);
            for (int i = 0; i < 3; i++) {
                if (triggeredBy.traitContainer.HasTrait("Frozen")) {
                    break;
                } else {
                    triggeredBy.traitContainer.AddTrait(triggeredBy, "Freezing", bypassElementalChance: true);
                }
            }
        }
        #endregion

        #region Freezing Trap
        public void SetHasSnareTrap(bool state) {
            if (hasSnareTrap != state) {
                SetIsDefault(false);
                hasSnareTrap = state;
                if (hasSnareTrap) {
                    _snareTrapEffect = GameManager.Instance.CreateParticleEffectAt(this, PARTICLE_EFFECT.Snare_Trap, InnerMapManager.DetailsTilemapSortingOrder - 1);
                } else {
                    ObjectPoolManager.Instance.DestroyObject(_snareTrapEffect);
                    _snareTrapEffect = null;
                }
            }
        }
        private void TriggerSnareTrap(Character triggeredBy) {
            GameManager.Instance.CreateParticleEffectAt(triggeredBy, PARTICLE_EFFECT.Snare_Trap_Explosion);
            SetHasSnareTrap(false);
            triggeredBy.traitContainer.AddTrait(triggeredBy, "Ensnared");
        }
        #endregion

        #region Hextile
        public HexTile GetNearestHexTileWithinRegion() {
            if (collectionOwner.isPartOfParentRegionMap) {
                HexTile hex = collectionOwner.partOfHextile.hexTileOwner;
                if (hex.elevationType != ELEVATION.WATER && hex.elevationType != ELEVATION.MOUNTAIN) {
                    return hex;
                }
            }

            HexTile nearestHex = null;
            float nearestDist = 0f;
            for (int i = 0; i < collectionOwner.region.tiles.Count; i++) {
                HexTile hex = collectionOwner.region.tiles[i];
                if (hex.elevationType != ELEVATION.WATER && hex.elevationType != ELEVATION.MOUNTAIN) {
                    float dist = GetDistanceTo(hex.GetCenterLocationGridTile());
                    if (nearestHex == null || dist < nearestDist) {
                        nearestHex = hex;
                        nearestDist = dist;
                    }
                }
            }
            return nearestHex;
        }
        public HexTile GetNearestHexTileWithinRegionThatMeetCriteria(System.Func<HexTile, bool> validityChecker) {
            if (collectionOwner.isPartOfParentRegionMap) {
                HexTile hex = collectionOwner.partOfHextile.hexTileOwner;
                if (validityChecker.Invoke(hex)) {
                    return hex;
                }
            }

            HexTile nearestHex = null;
            float nearestDist = 0f;
            for (int i = 0; i < collectionOwner.region.tiles.Count; i++) {
                HexTile hex = collectionOwner.region.tiles[i];
                if (validityChecker.Invoke(hex)) {
                    float dist = GetDistanceTo(hex.GetCenterLocationGridTile());
                    if (nearestHex == null || dist < nearestDist) {
                        nearestHex = hex;
                        nearestDist = dist;
                    }
                }
            }
            return nearestHex;
        }
        #endregion

        #region Blueprints
        public void SetHasBlueprint(bool hasBlueprint) {
            this.hasBlueprint = hasBlueprint;
            if (collectionOwner.isPartOfParentRegionMap) {
                if (hasBlueprint) {
                    collectionOwner.partOfHextile.hexTileOwner.AddBlueprint();
                } else {
                    collectionOwner.partOfHextile.hexTileOwner.RemoveBlueprint();
                }
            }
        }
        #endregion

        #region Connectors
        public void AddConnector() {
            connectorsOnTile++;
        }
        public void RemoveConnector() {
            connectorsOnTile--;
        }
        #endregion
        
        #region Meteor
        public int meteorCount { get; private set; }
        public void AddMeteor() {
            SetIsDefault(false);
            meteorCount++;
            GameManager.Instance.CreateParticleEffectAt(this, PARTICLE_EFFECT.Meteor_Strike);
        }
        public void RemoveMeteor() {
            meteorCount--;
        }
        #endregion

        #region Saving
        public void SetIsDefault(bool state) {
            isDefault = state;
            // Debug.Log($"{GameManager.Instance.TodayLogString()}Is Default state of {this} set to: {isDefault.ToString()}");
        }
        #endregion

        #region Clean Up
        public void CleanUp() {
            parentMap = null;
            parentTileMap = null;
            structure = null;
            _neighbours?.Clear();
            _neighbours = null;
            _fourNeighbours?.Clear();
            _fourNeighbours = null;
            neighbourList?.Clear();
            neighbourList = null;
            objHere = null;
            charactersHere?.Clear();
            charactersHere = null;
            genericTileObject = null;
            walls?.Clear();
            walls = null;
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
}