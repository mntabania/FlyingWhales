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
using UnityEngine.Profiling;
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

        private static readonly GridNeighbourDirection[] gridNeighbourDirections = CollectionUtilities.GetEnumValues<GridNeighbourDirection>();
        private Dictionary<GridNeighbourDirection, Point> possibleExits = new Dictionary<GridNeighbourDirection, Point>() {
            {GridNeighbourDirection.North, new Point(0,1) },
            {GridNeighbourDirection.South, new Point(0,-1) },
            {GridNeighbourDirection.West, new Point(-1,0) },
            {GridNeighbourDirection.East, new Point(1,0) },
            {GridNeighbourDirection.North_West, new Point(-1,1) },
            {GridNeighbourDirection.North_East, new Point(1,1) },
            {GridNeighbourDirection.South_West, new Point(-1,-1) },
            {GridNeighbourDirection.South_East, new Point(1,-1) },
        };
        private PointFloat[] nodePoints = new PointFloat[] {
            new PointFloat(0.25f, 0.25f), //north east
            new PointFloat(-0.25f, 0.25f), //north west
            new PointFloat(0.25f, -0.25f), //south east
            new PointFloat(-0.25f, -0.25f), //south west
        };
        private Dictionary<GridNeighbourDirection, GridNodeBase> gridNodes = new Dictionary<GridNeighbourDirection, GridNodeBase>() {
            {GridNeighbourDirection.North_East, null },
            {GridNeighbourDirection.North_West, null },
            {GridNeighbourDirection.South_East, null },
            {GridNeighbourDirection.South_West, null },
        };

        public string persistentID { get; }
        public InnerTileMap parentMap { get; private set; }
        public Tilemap parentTileMap { get; private set; }
        public Area area { get; }
        public Vector3Int localPlace { get; }
        public Vector3 worldLocation { get; private set; }
        public Vector3 centeredWorldLocation { get; private set; }
        public Vector3 localLocation { get; }
        public Vector3 centeredLocalLocation { get; }
        public Tile_Type tileType { get; private set; }
        public Tile_State tileState { get; private set; }
        public Ground_Type groundType { get; private set; }
        public BIOMES mainBiomeType { get; private set; }
        public ELEVATION elevationType { get; private set; }
        public LocationStructure structure { get; private set; }
        public List<LocationGridTile> neighbourList { get; private set; }
        public List<Character> charactersHere { get; private set; }
        // public LocationGridTileCollection collectionOwner { get; private set; }
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
        public Biome_Tile_Type specificBiomeTileType { get; private set; }
        public string groundTileMapAssetName { get; private set; }
        public string wallTileMapAssetName { get; private set; }

        private Dictionary<GridNeighbourDirection, LocationGridTile> _neighbours;
        private Dictionary<GridNeighbourDirection, LocationGridTile> _fourNeighbours;
        private List<LocationGridTile> _fourNeighboursList;


        //Components
        public GridTileCorruptionComponent corruptionComponent { get; private set; }
        public GridTileMouseEventsComponent mouseEventsComponent { get; private set; }
        public GridTileTileObjectComponent tileObjectComponent { get; private set; }

        #region getters
        public OBJECT_TYPE objectType => OBJECT_TYPE.Gridtile;
        public System.Type serializedData => typeof(SaveDataLocationGridTile); 
        public bool isOccupied => tileState == Tile_State.Occupied;
        #endregion

        #region Pathfinding
        //public List<LocationGridTile> ValidTiles { get { return FourNeighbours().Where(o => o.tileType == Tile_Type.Empty).ToList(); } }
        //public List<LocationGridTile> CaveInterconnectionTiles { get { return FourNeighbours().Where(o => o.structure == structure).ToList(); } } //&& !o.HasDifferentStructureNeighbour()
        //public List<LocationGridTile> UnoccupiedNeighbours { get { return neighbourList.Where(o => !o.isOccupied && o.structure == structure).ToList(); } }
        //public List<LocationGridTile> unoccupiedOfCharactersNeighbours { get { return neighbourList.Where(o => o.charactersHere.Count <= 0 && o.structure == structure).ToList(); } }
        private Dictionary<GridNeighbourDirection, LocationGridTile> FourNeighboursDictionary() { return _fourNeighbours; }
        //public List<LocationGridTile> UnoccupiedNeighboursWithinArea {
        //    get {
        //        return neighbourList.Where(o =>
        //                !o.isOccupied && o.charactersHere.Count <= 0 && o.structure == structure &&
        //                o.area == area)
        //            .ToList();
        //    }
        //}
        #endregion
        
        public LocationGridTile(int x, int y, Tilemap tilemap, InnerTileMap p_parentMap, Area p_area) {
            persistentID = System.Guid.NewGuid().ToString();
            parentMap = p_parentMap;
            parentTileMap = tilemap;
            area = p_area;
            localPlace = new Vector3Int(x, y, 0);
            worldLocation = tilemap.CellToWorld(localPlace);
            localLocation = tilemap.CellToLocal(localPlace);
            centeredLocalLocation = new Vector3(localLocation.x + 0.5f, localLocation.y + 0.5f, localLocation.z);
            centeredWorldLocation = new Vector3(worldLocation.x + 0.5f, worldLocation.y + 0.5f, worldLocation.z);
            tileType = Tile_Type.Empty;
            tileState = Tile_State.Empty;
            charactersHere = new List<Character>();
            _fourNeighbours = new Dictionary<GridNeighbourDirection, LocationGridTile>();
            _neighbours = new Dictionary<GridNeighbourDirection, LocationGridTile>();
            neighbourList = new List<LocationGridTile>();
            isDefault = true;
            connectorsOnTile = 0;
            mainBiomeType = BIOMES.NONE;
            elevationType = ELEVATION.PLAIN;
            //Components
            corruptionComponent = new GridTileCorruptionComponent(); corruptionComponent.SetOwner(this);
            mouseEventsComponent = new GridTileMouseEventsComponent(); mouseEventsComponent.SetOwner(this);
            tileObjectComponent = new GridTileTileObjectComponent(); tileObjectComponent.SetOwner(this);
            DatabaseManager.Instance.locationGridTileDatabase.RegisterTile(this);
        }
        public LocationGridTile(SaveDataLocationGridTile data, Tilemap tilemap, InnerTileMap p_parentMap, Area p_area) {
            persistentID = data.persistentID;
            parentMap = p_parentMap;
            parentTileMap = tilemap;
            area = p_area;
            groundTileMapAssetName = data.groundTileMapAssetName;
            wallTileMapAssetName = data.wallTileMapAssetName;
            localPlace = new Vector3Int((int)data.localPlace.x, (int)data.localPlace.y, 0);
            worldLocation = data.worldLocation;
            localLocation = data.localLocation;
            centeredLocalLocation = data.centeredLocalLocation;
            centeredWorldLocation = data.centeredWorldLocation;
            tileType = data.tileType;
            tileState = data.tileState;
            charactersHere = new List<Character>();
            _fourNeighbours = new Dictionary<GridNeighbourDirection, LocationGridTile>();
            _neighbours = new Dictionary<GridNeighbourDirection, LocationGridTile>();
            neighbourList = new List<LocationGridTile>();
            isDefault = data.isDefault;
            connectorsOnTile = data.connectorsCount;
            corruptionComponent = data.corruptionComponent.Load(); corruptionComponent.SetOwner(this);
            mouseEventsComponent = data.mouseEventsComponent.Load(); mouseEventsComponent.SetOwner(this);
            tileObjectComponent = data.tileObjectComponent.Load(); tileObjectComponent.SetOwner(this);
            elevationType = data.elevation;
            DatabaseManager.Instance.locationGridTileDatabase.RegisterTile(this);
        }

        #region Loading
        public void LoadSecondWave(SaveDataLocationGridTile saveDataLocationGridTile) {
            //if (saveDataLocationGridTile.tileObjectComponent.hasLandmine) {
            //    SetHasLandmine(true);
            //}
            //if (saveDataLocationGridTile.tileObjectComponent.hasFreezingTrap) {
            //    SetHasFreezingTrap(true, saveDataLocationGridTile.tileObjectComponent.freezingTrapExclusions?.ToArray());
            //}
            //if (saveDataLocationGridTile.tileObjectComponent.hasSnareTrap) {
            //    SetHasSnareTrap(true);
            //}
            for (int i = 0; i < saveDataLocationGridTile.meteorCount; i++) {
                AddMeteor();
            }
            corruptionComponent.LoadSecondWave();
            mouseEventsComponent.LoadSecondWave();
            tileObjectComponent.LoadSecondWave();
        }
        #endregion

        #region Other Data
        public void SetTileType(Tile_Type tileType) {
            this.tileType = tileType;
        }
        public void SetGroundType(Ground_Type newGroundType, bool isInitial = false) {
            Ground_Type previousType = this.groundType;
            this.groundType = newGroundType;
            if (tileObjectComponent.genericTileObject != null) {
                switch (newGroundType) {
                    case Ground_Type.Grass:
                    case Ground_Type.Wood:
                    case Ground_Type.Sand:
                    case Ground_Type.Desert_Grass:
                    case Ground_Type.Structure_Stone:
                        tileObjectComponent.genericTileObject.traitContainer.AddTrait(tileObjectComponent.genericTileObject, "Flammable");
                        break;
                    case Ground_Type.Snow:
                        // genericTileObject.traitContainer.AddTrait(genericTileObject, "Frozen", bypassElementalChance: true, overrideDuration: 0);
                        tileObjectComponent.genericTileObject.traitContainer.RemoveTrait(tileObjectComponent.genericTileObject, "Flammable");
                        break;
                    default:
                        tileObjectComponent.genericTileObject.traitContainer.RemoveTrait(tileObjectComponent.genericTileObject, "Flammable");
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
            if (area != null) {
                area.gridTileComponent.EvaluatePassabilityOfTile(this);
            }
        }
        private void RevertBackToSnow() {
            SetGroundTilemapVisual(InnerMapManager.Instance.assetManager.snowTile, true);
        }
        public void UpdateWorldLocation() {
            worldLocation = parentTileMap.CellToWorld(localPlace);
            centeredWorldLocation = new Vector3(worldLocation.x + 0.5f, worldLocation.y + 0.5f, worldLocation.z);
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
                    if (mainBiomeType == BIOMES.SNOW || mainBiomeType == BIOMES.TUNDRA) {
                        if (assetName.Contains("dirtsnow")) {
                            return Ground_Type.Snow_Dirt;
                        } else if (assetName.Contains("snow")) {
                            return Ground_Type.Snow;
                        } else {
                            //override tile to use tundra soil
                            //parentMap.groundTilemap.SetTile(localPlace, InnerMapManager.Instance.assetManager.tundraTile);
                            SetGroundTilemapTileAsset(InnerMapManager.Instance.assetManager.tundraTile);
                            return Ground_Type.Tundra;
                        }
                    } else if (mainBiomeType == BIOMES.DESERT) {
                        if (structure != null && (structure.structureType == STRUCTURE_TYPE.CAVE || structure.structureType == STRUCTURE_TYPE.MONSTER_LAIR)) {
                            //override tile to use stone
                            //parentMap.groundTilemap.SetTile(localPlace, InnerMapManager.Instance.assetManager.stoneTile);
                            SetGroundTilemapTileAsset(InnerMapManager.Instance.assetManager.stoneTile);
                            return Ground_Type.Stone;
                        } else {
                            //override tile to use sand
                            //parentMap.groundTilemap.SetTile(localPlace, InnerMapManager.Instance.assetManager.desertSandTile);
                            SetGroundTilemapTileAsset(InnerMapManager.Instance.assetManager.desertSandTile);
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
                    //parentMap.groundTilemap.SetTile(localPlace, InnerMapManager.Instance.assetManager.tundraTile);
                    SetGroundTilemapTileAsset(InnerMapManager.Instance.assetManager.tundraTile);
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
            SetGroundTilemapTileAsset(tileBase);
            // parentMap.groundTilemap.RefreshTile(localPlace);
            if (tileObjectComponent.genericTileObject.mapObjectVisual != null && tileObjectComponent.genericTileObject.mapObjectVisual.usedSprite != null) {
                //if this tile's map object is shown and is showing a visual, update it's sprite to use the updated sprite.
                tileObjectComponent.genericTileObject.mapObjectVisual.SetVisual(parentMap.groundTilemap.GetSprite(localPlace));
            }
            UpdateGroundTypeBasedOnAsset();
            if (updateEdges) {
                CreateSeamlessEdgesForSelfAndNeighbours();
            }
        }
        public void SetStructureTilemapVisual(TileBase tileBase) {
            SetWallTilemapTileAsset(tileBase);
            UpdateGroundTypeBasedOnAsset();
        }
        public void SetGroundTilemapTileAsset(TileBase tileBase) {
            parentMap.groundTilemap.SetTile(localPlace, tileBase);
            UpdateGroundTileMapAssetName();
        }
        public void SetWallTilemapTileAsset(TileBase tileBase) {
            parentMap.structureTilemap.SetTile(localPlace, tileBase);
            UpdateWallTileMapAssetName();
        }
        public void UpdateGroundTileMapAssetNameForBatchedTileSetting() {
            UpdateGroundTileMapAssetName();
        }
        public void UpdateWallTileMapAssetNameForBatchedTileSetting() {
            UpdateWallTileMapAssetName();
        }
        private void UpdateGroundTileMapAssetName() {
            groundTileMapAssetName = parentMap.groundTilemap.GetTile(localPlace)?.name ?? string.Empty;
        }
        private void UpdateWallTileMapAssetName() {
            wallTileMapAssetName = parentMap.structureTilemap.GetTile(localPlace)?.name ?? string.Empty;
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
             TileBase groundTile = InnerTileMap.GetGroundAssetPerlin(floorSample, mainBiomeType);
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
                // case Ground_Type.Corrupted:
                case Ground_Type.Bone:
                    //if from structure, revert to original ground asset
                    nextGroundAsset = InnerTileMap.GetGroundAssetPerlin(floorSample, mainBiomeType);
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
        public void SetStructure(LocationStructure p_structure) {
            Assert.IsNotNull(p_structure);
            LocationStructure previousStructure = structure;
            structure?.RemoveTile(this);
            
            structure = p_structure;
            structure.AddTile(this);

            if(tileObjectComponent.objHere != null) {
                //Whenever a grid tile changes its structure (might be because a new structure is built on top of it), the object inside must update its awareness to that new structure
                LocationAwarenessUtility.RemoveFromAwarenessList(tileObjectComponent.objHere);
                LocationAwarenessUtility.AddToAwarenessList(tileObjectComponent.objHere, this);
            }
            if (previousStructure != structure) {
                //if tile changed structures then transfer characters here to that structure.
                //This is to prevent inconsistent data, so that we do not need to wait for the character to call CharacterMarker.UpdatePosition for its structure location to be updated.
                for (int i = 0; i < charactersHere.Count; i++) {
                    Character character = charactersHere[i];
                    structure.AddCharacterAtLocation(character);
                }
            }
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
#if DEBUG_PROFILER
            Profiler.BeginSample($"{character.name} Add Character To Tile");
#endif
            // if (!charactersHere.Contains(character)) {
                charactersHere.Add(character);

            // }
            if (tileObjectComponent.genericTileObject != null) {
                List<Trait> traitOverrideFunctions = tileObjectComponent.genericTileObject.traitContainer.GetTraitOverrideFunctions(TraitManager.Enter_Grid_Tile_Trait);
                if (traitOverrideFunctions != null) {
                    for (int i = 0; i < traitOverrideFunctions.Count; i++) {
                        Trait trait = traitOverrideFunctions[i];
                        trait.OnEnterGridTile(character, tileObjectComponent.genericTileObject);
                    }
                }
            }
            if (tileObjectComponent.hasLandmine) {
                GameManager.Instance.StartCoroutine(tileObjectComponent.TriggerLandmine(character));
            }

            if (!character.movementComponent.cameFromWurmHole) {
                if (tileObjectComponent.objHere != null && tileObjectComponent.objHere is WurmHole wurmHole) {
                    bool shouldGoThroughWurmHole = true;
                    if (character.carryComponent.isBeingCarriedBy != null) {
                        if (character.carryComponent.isBeingCarriedBy.movementComponent.isFlying) {
                            //characters being carried by flying characters should not go through wurm hole
                            shouldGoThroughWurmHole = false;    
                        }
                    } else {
                        if (character.movementComponent.isFlying) {
                            //flying characters that are not carried should not go through wurm hole
                            shouldGoThroughWurmHole = false;
                        }
                    }
                    
                    if (shouldGoThroughWurmHole && wurmHole.wurmHoleConnection.gridTileLocation != null) {
                        wurmHole.TravelThroughWurmHole(character);
                        return;
                    }
                }
            } else {
                character.movementComponent.SetCameFromWurmHole(false);
            }

            if (tileObjectComponent.objHere != null && tileObjectComponent.objHere is Rug rug) {
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
            if (tileObjectComponent.hasFreezingTrap && (tileObjectComponent.freezingTrapExclusions == null || !tileObjectComponent.freezingTrapExclusions.Contains(character.race))) {
                tileObjectComponent.TriggerFreezingTrap(character);
            }
            if (tileObjectComponent.hasSnareTrap) {
                tileObjectComponent.TriggerSnareTrap(character);
            }

            Character scorpionCharacter = GetCharacterWithRace(RACE.SCORPION);
            Scorpion scorpion = scorpionCharacter as Scorpion;
            if (scorpion != null && !scorpion.isDead && scorpion.limiterComponent.canPerform && scorpion.limiterComponent.canMove && scorpion != character && scorpion.heldCharacter == null && scorpion.isHidden) {
                if (character.canBeTargetedByLandActions) {
                    if (!scorpion.hasPulledForTheDay) {
                        scorpion.SetHasPulledForTheDay(true);
                        scorpion.SetHeldCharacter(character);
                        character.interruptComponent.TriggerInterrupt(INTERRUPT.Pulled_Down, scorpion);
                    }
                }
            }

            if (corruptionComponent.isCorrupted) {
                if(!character.isDead && character.limiterComponent.canMove && character.limiterComponent.canPerform) {
                    if (!character.movementComponent.hasMovedOnCorruption) {
                        //Corrupted hexes should also be avoided
                        //https://trello.com/c/6WJtivlY/1274-fleeing-should-not-go-to-corrupted-structures
                        //Note: Instead of always fleeing from all corrupted hex tiles all the time, we must only let the characters flee from it if it walks on a corrupted tile
                        //The reason for this is so that if the corrupted hex is too far away the character will not try to run from it, and thus, the flee path will not be messed up
                        //Right now, the flee path sometimes gets messed up when the character tries to run from another character, sometimes they go to the same direction, it is because right now, we always take into account the corrupted hex tile even if they are too far
                        if (character.marker && character.marker.hasFleePath && character.isNormalCharacter) {
                            if (character.gridTileLocation != null) {
                                //TileObject genericTileObject = character.gridTileLocation.hexTileOwner.GetCenterLocationGridTile().tileObjectComponent.genericTileObject;
                                character.movementComponent.SetHasMovedOnCorruption(true);
                                //character.marker.AddPOIAsInVisionRange(genericTileObject);
                                //character.combatComponent.Flight(genericTileObject, "saw something frightening", forcedFlight: true);
                                //genericTileObject.traitContainer.AddTrait(genericTileObject, "Danger Remnant");
                                character.marker.AddAvoidPositions(character.gridTileLocation.area.gridTileComponent.centerGridTile.worldLocation);
                                // return;
                            }
                        }
                    }


                    //Reporting does not trigger until Tutorial is over
                    //https://trello.com/c/OmmyR6go/1239-reporting-does-not-trigger-until-tutorial-is-over
                    // LocationStructure mostImportantStructureOnTile = area.structureComponent.GetMostImportantStructureOnTile();
                    // if(mostImportantStructureOnTile is DemonicStructure demonicStructure) {
                    //     if (character.limiterComponent.canWitness && !character.behaviourComponent.isAttackingDemonicStructure 
                    //         && character.homeSettlement != null && character.necromancerTrait == null && character.race.IsSapient()
                    //         && character.hasMarker && character.carryComponent.IsNotBeingCarried() && character.isAlliedWithPlayer == false
                    //         && (!character.partyComponent.hasParty || !character.partyComponent.currentParty.isActive || (character.partyComponent.currentParty.currentQuest.partyQuestType != PARTY_QUEST_TYPE.Counterattack && character.partyComponent.currentParty.currentQuest.partyQuestType != PARTY_QUEST_TYPE.Rescue)) 
                    //         //&& !InnerMapManager.Instance.HasWorldKnownDemonicStructure(mostImportantStructureOnTile)
                    //         && (Tutorial.TutorialManager.Instance.hasCompletedImportantTutorials || WorldSettings.Instance.worldSettingsData.worldType != WorldSettingsData.World_Type.Tutorial)) {
                    //         if (character.faction != null && character.faction.isMajorNonPlayer && !character.faction.partyQuestBoard.HasPartyQuest(PARTY_QUEST_TYPE.Counterattack) && !character.faction.HasActiveReportDemonicStructureJob(mostImportantStructureOnTile)) {
                    //             character.jobComponent.CreateReportDemonicStructure(mostImportantStructureOnTile);
                    //             return;
                    //         }
                    //     }
                    //     //If cannot report flee instead
                    //     //do not make characters that are allied with the player or attacking a demonic structure flee from corruption.
                    //     if (character.limiterComponent.canWitness && !character.behaviourComponent.isAttackingDemonicStructure 
                    //           && (!character.partyComponent.hasParty || !character.partyComponent.currentParty.isActive || (character.partyComponent.currentParty.currentQuest.partyQuestType != PARTY_QUEST_TYPE.Counterattack && character.partyComponent.currentParty.currentQuest.partyQuestType != PARTY_QUEST_TYPE.Rescue && character.partyComponent.currentParty.currentQuest.partyQuestType != PARTY_QUEST_TYPE.Heirloom_Hunt)) 
                    //           && character.isAlliedWithPlayer == false 
                    //           && character.necromancerTrait == null
                    //           && !character.jobQueue.HasJob(JOB_TYPE.REPORT_CORRUPTED_STRUCTURE)) {
                    //         if (!character.movementComponent.hasMovedOnCorruption) {
                    //             character.movementComponent.SetHasMovedOnCorruption(true);
                    //             if (character.isNormalCharacter) {
                    //                 //Instead of fleeing when character steps on a corrupted tile, trigger Shocked interrupt only
                    //                 //The reason for this is to eliminate the bug wherein the character will flee from the corrupted tile, then after fleeing, he will again move across it, thus triggering flee again, which results in unending loop of fleeing and moving
                    //                 //So to eliminate this behaviour we will not let the character flee, but will trigger Shocked interrupt only and then go on with his job/action
                    //                 //https://trello.com/c/yiW344Sb/2499-villagers-fleeing-from-demonic-area-can-get-stuck-repeating-it
                    //                 character.interruptComponent.TriggerInterrupt(INTERRUPT.Shocked, character);
                    //                 //genericTileObject.traitContainer.AddTrait(genericTileObject, "Danger Remnant");
                    //
                    //                 //if (character.characterClass.IsCombatant()) {
                    //                 //    character.behaviourComponent.SetIsAttackingDemonicStructure(true, demonicStructure);
                    //                 //} else {
                    //                 //    genericTileObject.traitContainer.AddTrait(genericTileObject, "Danger Remnant");
                    //                 //}
                    //             }
                    //         }
                    //     }
                    // }
                    
                    // if (character.limiterComponent.canWitness && !character.behaviourComponent.isAttackingDemonicStructure && 
                    //     (!character.partyComponent.hasParty || !character.partyComponent.currentParty.isActive || 
                    //      (character.partyComponent.currentParty.currentQuest.partyQuestType != PARTY_QUEST_TYPE.Counterattack && 
                    //       !(character.partyComponent.currentParty.currentQuest is IRescuePartyQuest) && 
                    //       character.partyComponent.currentParty.currentQuest.partyQuestType != PARTY_QUEST_TYPE.Heirloom_Hunt)) && 
                    //     !character.isAlliedWithPlayer && 
                    //     character.necromancerTrait == null && 
                    //     !character.jobQueue.HasJob(JOB_TYPE.REPORT_CORRUPTED_STRUCTURE)) {
                    //     if (!character.movementComponent.hasMovedOnCorruption) {
                    //         character.movementComponent.SetHasMovedOnCorruption(true);
                    //         if (character.isNormalCharacter) {
                    //             //Instead of fleeing when character steps on a corrupted tile, trigger Shocked interrupt only
                    //             //The reason for this is to eliminate the bug wherein the character will flee from the corrupted tile, then after fleeing, he will again move across it, thus triggering flee again, which results in unending loop of fleeing and moving
                    //             //So to eliminate this behaviour we will not let the character flee, but will trigger Shocked interrupt only and then go on with his job/action
                    //             //https://trello.com/c/yiW344Sb/2499-villagers-fleeing-from-demonic-area-can-get-stuck-repeating-it
                    //             character.interruptComponent.TriggerInterrupt(INTERRUPT.Shocked, character);
                    //         }
                    //     }
                    // }
                }
            } else {
                character.movementComponent.SetHasMovedOnCorruption(false);
            }
#if DEBUG_PROFILER
            Profiler.EndSample();
#endif
        }

        public Character GetCharacterWithRace(RACE p_lookUprace) {
            for (int i = 0; i < charactersHere.Count; i++) {
                Character character = charactersHere[i];
                if (character.race == p_lookUprace) {
                    return character;
                }
            }
            return null;
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
                Area hex = area;
                return hex == character.territory;
            }
            return false;
        }
#endregion

#region Utilities
        public LocationGridTile GetNeighbourAtDirection(GridNeighbourDirection dir) {
            if (_neighbours.ContainsKey(dir)) {
                return _neighbours[dir];
            }
            return null;
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
            for (int i = 0; i < gridNeighbourDirections.Length; i++) {
                if (!_neighbours.ContainsKey(gridNeighbourDirections[i])) {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Does this tile have a neighbour that is part of a different structure, or is part of the outside map?
        /// </summary>
        public bool HasDifferentDwellingOrOutsideNeighbour() {
            for (int i = 0; i < neighbourList.Count; i++) {
                LocationGridTile t = neighbourList[i];
                if (t.structure != structure) {
                    return true;
                }
            }
            return false;
        }
        public override string ToString() {
            return localPlace.ToString();
        }
        public float GetDistanceTo(LocationGridTile tile) {
            //if(structure.region != tile.structure.region) {
            //    //Computing distance of tiles from different region should be different
            //    //It should origin tile distance to origin tile region's edge tile + target tile region's edge tile to target tile
            //    Region targetRegion = tile.structure.region;
            //    LocationGridTile targetGate = GetTargetTileToGoToRegion(targetRegion);
            //    LocationGridTile exitTile = GetExitTileToGoToRegion(targetGate);

            //    float originDistanceToExitTile = Vector2.Distance(localLocation, exitTile.localLocation);
            //    float targetGateDistanceToTargetTile = Vector2.Distance(targetGate.localLocation, tile.localLocation);

            //    return originDistanceToExitTile + targetGateDistanceToTargetTile;
            //} else {
                return Vector2.Distance(localLocation, tile.localLocation);
            //}
        }
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
        public bool HasUnoccupiedNeighbour(bool sameStructureOnly = false) {
            for (int i = 0; i < neighbourList.Count; i++) {
                LocationGridTile tile = neighbourList[i];
                if (!tile.isOccupied && (!sameStructureOnly || tile.structure == structure)) {
                    return true;
                }
            }
            return false;
        }
        public void PopulateUnoccupiedNeighbours(List<LocationGridTile> neighbours, bool sameStructureOnly = false) {
            for (int i = 0; i < neighbourList.Count; i++) {
                LocationGridTile tile = neighbourList[i];
                if (!tile.isOccupied && (!sameStructureOnly || tile.structure == structure)) {
                    neighbours.Add(tile);
                }
            }
        }
        public void PopulateUnoccupiedNeighboursWithNoCharactersInSameAreaAndStructure(List<LocationGridTile> neighbours) {
            for (int i = 0; i < neighbourList.Count; i++) {
                LocationGridTile tile = neighbourList[i];
                if (!tile.isOccupied && tile.charactersHere.Count <= 0 && tile.structure == structure && tile.area == area) {
                    neighbours.Add(tile);
                }
            }
        }
        public void PopulateNeighboursWithNoCharacters(List<LocationGridTile> neighbours, bool sameStructureOnly = false) {
            for (int i = 0; i < neighbourList.Count; i++) {
                LocationGridTile tile = neighbourList[i];
                if (tile.charactersHere.Count <= 0 && (!sameStructureOnly || tile.structure == structure)) {
                    neighbours.Add(tile);
                }
            }
        }
        public void PopulateUnoccupiedNeighboursThatIsSameStructureAs(List<LocationGridTile> neighbours, LocationStructure structure) {
            for (int i = 0; i < neighbourList.Count; i++) {
                LocationGridTile tile = neighbourList[i];
                if (!tile.isOccupied && tile.structure == structure) {
                    neighbours.Add(tile);
                }
            }
        }
        public bool HasNeighbourOfElevation(ELEVATION elevation, bool useFourNeighbours = false) {
            Dictionary<GridNeighbourDirection, LocationGridTile> n = _neighbours;
            if (useFourNeighbours) {
                n = FourNeighboursDictionary();
            }
            for (int i = 0; i < n.Values.Count; i++) {
                if (_neighbours.Values.ElementAt(i).area.elevationType == elevation) {
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
            for (int i = 0; i < neighbourList.Count; i++) {
                LocationGridTile t = neighbourList[i];
                if (t.tileObjectComponent.objHere != null && t.tileObjectComponent.objHere.GetType() == type) {
                    return true;
                }
            }
            return false;
        }
        public bool HasNeighbouringWalledStructure() {
            for (int i = 0; i < neighbourList.Count; i++) {
                LocationGridTile t = neighbourList[i];
                if (t.structure != null && t.structure.structureType.IsOpenSpace() == false) {
                    return true;
                }
            }
            return false;
        }
        public List<LocationGridTile> FourNeighbours() {
            TryPopulateFourNeighboursList();
            return _fourNeighboursList;
        }
        public void PopulateFourNeighboursThatHasTileObjectOfType(List<LocationGridTile> neighbours, TILE_OBJECT_TYPE p_type) {
            List<LocationGridTile> fourNeighbours = FourNeighbours();
            if (fourNeighbours != null) {
                for (int i = 0; i < fourNeighbours.Count; i++) {
                    LocationGridTile t = fourNeighbours[i];
                    if (t.tileObjectComponent.objHere is TileObject to && to.tileObjectType == p_type) {
                        neighbours.Add(t);
                    }
                }
            }
        }
        public void PopulateFourNeighboursThatHasTileObjectOfType(List<LocationGridTile> neighbours, TILE_OBJECT_TYPE p_type, MapGenerationData p_mapGenerationData) {
            List<LocationGridTile> fourNeighbours = FourNeighbours();
            if (fourNeighbours != null) {
                for (int i = 0; i < fourNeighbours.Count; i++) {
                    LocationGridTile t = fourNeighbours[i];
                    if ((t.tileObjectComponent.objHere is TileObject to && to.tileObjectType == p_type) || (p_mapGenerationData.GetGeneratedObjectOnTile(t) == p_type)) {
                        neighbours.Add(t);
                    }
                }
            }
        }
        public void PopulateFourNeighboursValidTiles(List<LocationGridTile> neighbours) {
            List<LocationGridTile> fourNeighbours = FourNeighbours();
            if (fourNeighbours != null) {
                for (int i = 0; i < fourNeighbours.Count; i++) {
                    LocationGridTile t = fourNeighbours[i];
                    if (t.tileType == Tile_Type.Empty) {
                        neighbours.Add(t);
                    }
                }
            }
        }
        public void PopulateFourNeighboursInSameStructure(List<LocationGridTile> neighbours) {
            List<LocationGridTile> fourNeighbours = FourNeighbours();
            if (fourNeighbours != null) {
                for (int i = 0; i < fourNeighbours.Count; i++) {
                    LocationGridTile t = fourNeighbours[i];
                    if (t.structure == structure) {
                        neighbours.Add(t);
                    }
                }
            }
        }
        public int GetCountOfFourNeighboursInStructureType(STRUCTURE_TYPE p_type) {
            int count = 0;
            List<LocationGridTile> fourNeighbours = FourNeighbours();
            if (fourNeighbours != null) {
                for (int i = 0; i < fourNeighbours.Count; i++) {
                    LocationGridTile t = fourNeighbours[i];
                    if (t.structure.structureType == p_type) {
                        count++;
                    }
                }
            }
            return count;
        }
        private void TryPopulateFourNeighboursList() {
            if (_fourNeighboursList == null) {
                if (_fourNeighboursList == null) { _fourNeighboursList = new List<LocationGridTile>(); }
                for (int i = 0; i < _fourNeighbours.Values.Count; i++) {
                    _fourNeighboursList.Add(_fourNeighbours.Values.ElementAt(i));
                }
            }
        }
        public int GetCountOfNeighboursInStructureType(STRUCTURE_TYPE p_type) {
            int count = 0;
            if (neighbourList != null) {
                for (int i = 0; i < neighbourList.Count; i++) {
                    LocationGridTile t = neighbourList[i];
                    if (t.structure.structureType == p_type) {
                        count++;
                    }
                }
            }
            return count;
        }
        public int GetCountOfNeighboursThatHasTileObjectOfType(TILE_OBJECT_TYPE p_type) {
            int count = 0;
            if (neighbourList != null) {
                for (int i = 0; i < neighbourList.Count; i++) {
                    LocationGridTile t = neighbourList[i];
                    if (t.tileObjectComponent.objHere is TileObject to && to.tileObjectType == p_type) {
                        count++;
                    }
                }
            }
            return count;
        }
        public int GetCountOfNeighboursThatHasTileObjectOfType(TILE_OBJECT_TYPE p_type, MapGenerationData p_data) {
            int count = 0;
            if (neighbourList != null) {
                for (int i = 0; i < neighbourList.Count; i++) {
                    LocationGridTile t = neighbourList[i];
                    if ((t.tileObjectComponent.objHere is TileObject to && to.tileObjectType == p_type) || (p_data.GetGeneratedObjectOnTile(t) == p_type)) {
                        count++;
                    }
                }
            }
            return count;
        }
        //public LocationGridTile GetNearestUnoccupiedTileFromThis() {
        //    List<LocationGridTile> unoccupiedNeighbours = UnoccupiedNeighbours;
        //    if (unoccupiedNeighbours.Count == 0) {
        //        if (structure != null) {
        //            LocationGridTile nearestTile = null;
        //            float nearestDist = 99999f;
        //            for (int i = 0; i < structure.unoccupiedTiles.Count; i++) {
        //                LocationGridTile currTile = structure.unoccupiedTiles.ElementAt(i);
        //                if (currTile != this && currTile.groundType != Ground_Type.Water) {
        //                    float dist = Vector2.Distance(currTile.localLocation, localLocation);
        //                    if (dist < nearestDist) {
        //                        nearestTile = currTile;
        //                        nearestDist = dist;
        //                    }
        //                }
        //            }
        //            return nearestTile;
        //        }
        //    } else {
        //        return unoccupiedNeighbours[Random.Range(0, unoccupiedNeighbours.Count)];
        //    }
        //    return null;
        //}
        public LocationGridTile GetNeareastTileFromThisThatIsPassableOrHasNoWallsAndIsNotInOcean(List<LocationGridTile> checkedTiles) {
            if (!checkedTiles.Contains(this)) {
                checkedTiles.Add(this);
                if ((IsPassable() || !tileObjectComponent.HasWalls()) && structure.structureType != STRUCTURE_TYPE.OCEAN) {
                    return this;
                }
            }
            for (int i = 0; i < neighbourList.Count; i++) {
                LocationGridTile tile = neighbourList[i];
                if (!checkedTiles.Contains(tile)) {
                    checkedTiles.Add(tile);
                    if ((tile.IsPassable() || !tile.tileObjectComponent.HasWalls()) && tile.structure.structureType != STRUCTURE_TYPE.OCEAN) {
                        return tile;
                    }
                }
            }
            for (int i = 0; i < neighbourList.Count; i++) {
                LocationGridTile tile = neighbourList[i];
                LocationGridTile chosenTile = tile.GetNeareastTileFromThisThatIsPassableOrHasNoWallsAndIsNotInOcean(checkedTiles);
                if (chosenTile != null) {
                    return chosenTile;
                }
            }
            return null;
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
            LocationGridTile chosenTile = null;
            List<LocationGridTile> unoccupiedNeighbours = RuinarchListPool<LocationGridTile>.Claim();
            PopulateUnoccupiedNeighbours(unoccupiedNeighbours, true);
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
                    chosenTile = nearestTile;
                }
            } else {
                chosenTile = unoccupiedNeighbours[GameUtilities.RandomBetweenTwoNumbers(0, unoccupiedNeighbours.Count - 1)];
            }
            RuinarchListPool<LocationGridTile>.Release(unoccupiedNeighbours);
            return chosenTile;
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
            LocationGridTile chosenTile = null;
            List<LocationGridTile> unoccupiedNeighbours = RuinarchListPool<LocationGridTile>.Claim();
            PopulateUnoccupiedNeighbours(unoccupiedNeighbours, true);
            if (unoccupiedNeighbours.Count > 0) {
                chosenTile = unoccupiedNeighbours[GameUtilities.RandomBetweenTwoNumbers(0, unoccupiedNeighbours.Count - 1)];
            }
            RuinarchListPool<LocationGridTile>.Release(unoccupiedNeighbours);
            return chosenTile;
        }
        public LocationGridTile GetFirstNoObjectNeighbor(bool thisStructureOnly = false) {
            for (int i = 0; i < neighbourList.Count; i++) {
                LocationGridTile neighbor = neighbourList[i];
                if (!thisStructureOnly || neighbor.structure == structure) {
                    if (neighbor.tileObjectComponent.objHere == null && neighbor.IsPassable()) {
                        return neighbor;
                    }
                }
            }
            return null;
        }
        public LocationGridTile GetFirstNearestTileFromThisWithNoObject(bool thisStructureOnly = false, LocationGridTile exception = null) {
            List<LocationGridTile> checkedTiles = RuinarchListPool<LocationGridTile>.Claim();
            LocationGridTile chosenTile = GetFirstNearestTileFromThisWithNoObjectBase(thisStructureOnly, checkedTiles, exception);
            RuinarchListPool<LocationGridTile>.Release(checkedTiles);
            return chosenTile;
        }
        private LocationGridTile GetFirstNearestTileFromThisWithNoObjectBase(bool thisStructureOnly, List<LocationGridTile> checkedTiles, LocationGridTile exception) {
            if (!checkedTiles.Contains(this)) {
                checkedTiles.Add(this);

                if (tileObjectComponent.objHere == null && IsPassable() && this != exception) {
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

        public LocationGridTile GetRandomNeighborWithoutCharacters() {
            LocationGridTile chosenTile = null;
            List<LocationGridTile> choices = RuinarchListPool<LocationGridTile>.Claim();
            PopulateNeighboursWithNoCharacters(choices, true);
            if (choices.Count > 0) {
                chosenTile = choices[GameUtilities.RandomBetweenTwoNumbers(0, choices.Count - 1)];
            }
            return chosenTile;
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
        public LocationGridTile GetFirstNeighborThatIsPassableAndNoObjectAndSameAreaAs(Area p_area) {
            for (int i = 0; i < neighbourList.Count; i++) {
                LocationGridTile n = neighbourList[i];
                if (n.tileObjectComponent.objHere == null && n.IsPassable() && n.area == p_area) {
                    return n;
                }
            }
            return null;
        }
        public LocationGridTile GetFirstNeighborThatIsPassableAndNoObject() {
            for (int i = 0; i < neighbourList.Count; i++) {
                LocationGridTile n = neighbourList[i];
                if (n.tileObjectComponent.objHere == null && n.IsPassable()) {
                    return n;
                }
            }
            return null;
        }
        public LocationGridTile GetFirstNeighborThatIsPassable() {
            for (int i = 0; i < neighbourList.Count; i++) {
                LocationGridTile n = neighbourList[i];
                if (n.IsPassable()) {
                    return n;
                }
            }
            return null;
        }
        public LocationGridTile GetFirstNeighborThatIsPassableAndSameStructureAs(LocationStructure p_structure) {
            for (int i = 0; i < neighbourList.Count; i++) {
                LocationGridTile n = neighbourList[i];
                if (n.structure == p_structure && n.IsPassable()) {
                    return n;
                }
            }
            return null;
        }
        public bool IsAtEdgeOfWalkableMap() {
            if ((localPlace.y == 0 && localPlace.x >= 0 && localPlace.x <= parentMap.width - 1)
                || (localPlace.y == parentMap.height - 1 && localPlace.x >= 0 && localPlace.x <= parentMap.width - 1)
                || (localPlace.x == 0 && localPlace.y >= 0 && localPlace.y <= parentMap.height - 1) 
                || (localPlace.x == parentMap.width - 1 && localPlace.y >= 0 && localPlace.y <= parentMap.height - 1)) {
                return true;
            }
            return false;
        }
        private bool HasCardinalNeighbourOfDifferentGroundType() {
            List<LocationGridTile> cardinalNeighbours = FourNeighbours();
            for (int i = 0; i < cardinalNeighbours.Count; i++) {
                LocationGridTile t = cardinalNeighbours[i];
                if (t.groundType != groundType) {
                    return true;
                }
            }
            return false;
        }
        public void PopulateTraitablesOnTile(List<ITraitable> traitables) {
            traitables.Add(tileObjectComponent.genericTileObject);
            for (int i = 0; i < tileObjectComponent.walls.Count; i++) {
                ThinWall structureWallObject = tileObjectComponent.walls[i];
                traitables.Add(structureWallObject);
            }
            if (tileObjectComponent.objHere != null) {
                if (tileObjectComponent.objHere.mapObjectState == MAP_OBJECT_STATE.BUILT) {//|| (objHere is SpecialToken && (objHere as SpecialToken).mapObjectState == MAP_OBJECT_STATE.BUILT)
                    traitables.Add(tileObjectComponent.objHere);
                }
            }
            for (int i = 0; i < charactersHere.Count; i++) {
                Character character = charactersHere[i];
                traitables.Add(character);
            }
        }
        public void PopulateTraitablesOnTileThatCanHaveElementalTrait(List<ITraitable> traitables, string traitName, bool bypassElementalChance) {
            if (tileObjectComponent.genericTileObject.traitContainer.GetElementalTraitChanceToBeAdded(traitName, tileObjectComponent.genericTileObject, bypassElementalChance) > 0
                && tileObjectComponent.genericTileObject.CanBeAffectedByElementalStatus(traitName)) {
                traitables.Add(tileObjectComponent.genericTileObject);    
            }
            for (int i = 0; i < tileObjectComponent.walls.Count; i++) {
                ThinWall structureWallObject = tileObjectComponent.walls[i];
                if (structureWallObject.traitContainer.GetElementalTraitChanceToBeAdded(traitName, structureWallObject, bypassElementalChance) > 0) {
                    traitables.Add(structureWallObject);    
                }
            }
            if (tileObjectComponent.objHere != null) {
                if (tileObjectComponent.objHere.mapObjectState == MAP_OBJECT_STATE.BUILT && 
                    tileObjectComponent.objHere.traitContainer.GetElementalTraitChanceToBeAdded(traitName, tileObjectComponent.objHere, bypassElementalChance) > 0
                    && tileObjectComponent.objHere.CanBeAffectedByElementalStatus(traitName)) {//|| (objHere is SpecialToken && (objHere as SpecialToken).mapObjectState == MAP_OBJECT_STATE.BUILT)
                    traitables.Add(tileObjectComponent.objHere);
                }
            }
            for (int i = 0; i < charactersHere.Count; i++) {
                Character character = charactersHere[i];
                if (character.traitContainer.GetElementalTraitChanceToBeAdded(traitName, character, bypassElementalChance) > 0) {
                    traitables.Add(character);    
                }
            }
        }
        public void PerformActionOnTraitables(TraitableCallback callback) {
            callback.Invoke(tileObjectComponent.genericTileObject);
            for (int i = 0; i < tileObjectComponent.walls.Count; i++) {
                ThinWall structureWallObject = tileObjectComponent.walls[i];
                callback.Invoke(structureWallObject);
            }
            for (int i = 0; i < charactersHere.Count; i++) {
                Character character = charactersHere[i];
                if (tileObjectComponent.objHere is Tombstone tombstone && tombstone.character == character) {
                    //NOTE: Skip characters in tombstone when damaging character's here. //TODO: This is a quick fix
                    continue;
                }
                callback.Invoke(character);
            }
            if (tileObjectComponent.objHere != null && tileObjectComponent.objHere.mapObjectState == MAP_OBJECT_STATE.BUILT) {
                callback.Invoke(tileObjectComponent.objHere);
                //Sleeping characters in bed should also receive damage
                //https://trello.com/c/kFZAHo11/1203-sleeping-characters-in-bed-should-also-receive-damage
                if (tileObjectComponent.objHere is BaseBed bed) {
                    if (bed.users != null && bed.users.Length > 0) {
                        for (int i = 0; i < bed.users.Length; i++) {
                            Character user = bed.users[i];
                            //Should only apply if user is not part of charactersHere list so that no duplicate calls shall take place
                            if (user != null && !charactersHere.Contains(user)) {
                                callback.Invoke(user);
                            }
                        }
                    }
                }
            }
            Messenger.Broadcast(GridTileSignals.ACTION_PERFORMED_ON_TILE_TRAITABLES, this, callback);
        }
        public void PopulatePOIsOnTile(List<IPointOfInterest> pois) {
            pois.Add(tileObjectComponent.genericTileObject);
            if (tileObjectComponent.objHere != null) {
                if (tileObjectComponent.objHere.mapObjectState == MAP_OBJECT_STATE.BUILT) {
                    pois.Add(tileObjectComponent.objHere);
                }
            }
            for (int i = 0; i < charactersHere.Count; i++) {
                Character character = charactersHere[i];
                pois.Add(character);
            }
        }
        public void AddTraitToAllPOIsOnTile(string traitName) {
            tileObjectComponent.genericTileObject.traitContainer.AddTrait(tileObjectComponent.genericTileObject, traitName);
            if (tileObjectComponent.objHere != null) {
                if (tileObjectComponent.objHere.mapObjectState == MAP_OBJECT_STATE.BUILT) {
                    tileObjectComponent.objHere.traitContainer.AddTrait(tileObjectComponent.objHere, traitName);
                }
            }
            for (int i = 0; i < charactersHere.Count; i++) {
                Character character = charactersHere[i];
                character.traitContainer.AddTrait(character, traitName);
            }
        }
        public int GetDifferentElevationNeighboursCount() {
            int count = 0;
            for (int j = 0; j < neighbourList.Count; j++) {
                LocationGridTile neighbour = neighbourList[j];
                if (neighbour.elevationType != this.elevationType) {
                    count++;
                }
            }
            return count;
        }
        public bool IsPartOfSettlement(out BaseSettlement settlement) {
            if (area.settlementOnArea != null) {
                settlement = area.settlementOnArea;
                return true;
            }
            settlement = null;
            return false;
        }
        public bool IsPartOfSettlement(BaseSettlement settlement) {
            return area.settlementOnArea == settlement;
        }
        public bool IsPartOfSettlement() {
            return area.settlementOnArea != null;
        }
        public bool IsPartOfHumanElvenSettlement() {
            return area.settlementOnArea != null && area.settlementOnArea.locationType == LOCATION_TYPE.VILLAGE;
        }
        public bool IsPartOfActiveHumanElvenSettlement() {
            return IsPartOfHumanElvenSettlement() && area.settlementOnArea.residents.Count > 0;
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
            Area hexTile = area;
            for (int i = 0; i < hexTile.neighbourComponent.neighbours.Count; i++) {
                Area neighbour = hexTile.neighbourComponent.neighbours[i];
                if (neighbour.settlementOnArea == settlement) {
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
        public void PopulateTilesInRadius(List<LocationGridTile> tiles, int radius, int radiusLimit = 0, bool includeCenterTile = false, bool includeTilesInDifferentStructure = false, bool includeImpassable = true, bool includeTilesWithObject = true) {
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
                        if (!includeTilesWithObject && result.tileObjectComponent.objHere != null) { continue; }
                        if (!includeTilesInDifferentStructure
                            && (result.structure != structure && (!result.structure.structureType.IsOpenSpace() || !structure.structureType.IsOpenSpace()))) { continue; }
                        if (!includeImpassable && !result.IsPassable()) { continue; }
                        tiles.Add(result);
                    }
                }
            }
        }
        public bool IsPassable() {
            // if (structure is Cave && tileType == Tile_Type.Wall) {
            //     //had to add this checking because in map generation cave walls are not generated immediately,
            //     //which can cause Monsters to be generated on top of block walls after map generation is finished
            //     return false; 
            // }
            //Remove HasWalls checking because it is a wrong implementation
            //Reverted back to original checkers
            //Although there parts of the tile that is impassable because of thin walls, we do not consider the whole tile as impassable if there are thin walls because thin walls does not occupy the whole tile
            //We only consider it impassable if there is a block wall
            return (tileObjectComponent.objHere == null || !tileObjectComponent.objHere.IsUnpassable()) /*&& !tileObjectComponent.HasWalls()*/ && groundType != Ground_Type.Water;
        }
        //private LocationGridTile GetTargetTileToGoToRegion(Region region) {
        //    //if (currentRegion != null) {
        //    //    RegionInnerTileMap regionInnerTileMap = currentRegion.innerMap as RegionInnerTileMap;
        //    //    if (regionInnerTileMap != null) {
        //    //        return regionInnerTileMap.GetTileToGoToRegion(region);
        //    //    }
        //    //} else if (gridTileLocation != null) {
        //    //    RegionInnerTileMap regionInnerTileMap = gridTileLocation.parentMap.region.innerMap as RegionInnerTileMap;
        //    //    if (regionInnerTileMap != null) {
        //    //        return regionInnerTileMap.GetTileToGoToRegion(region);
        //    //    }
        //    //}
        //    RegionInnerTileMap regionInnerTileMap = structure.region.innerMap as RegionInnerTileMap;
        //    if (regionInnerTileMap != null) {
        //        return regionInnerTileMap.GetTileToGoToRegion(region);
        //    }
        //    return null;
        //}
        //public LocationGridTile GetExitTileToGoToRegion(Region region) {
        //    //gate -the tile where the character will appear in the target region
        //    LocationGridTile gate = GetTargetTileToGoToRegion(region);
        //    return GetExitTileToGoToRegion(gate);
        //}
        //public LocationGridTile GetExitTileToGoToRegion(LocationGridTile gateInTargetRegion) {
        //    //direction - the direction where the character must go in order to go to the other region, it is also the basis in which we get the tile where the character will exit in this region
        //    DIRECTION direction = gateInTargetRegion.GetDirection();
        //    LocationGridTile exitTile = GetNearestEdgeTileFromThis(direction);
        //    return exitTile;
        //}
#endregion

        public void InstantPlaceDemonicStructure(StructureSetting p_structureSetting) {
            List<GameObject> choices = InnerMapManager.Instance.GetStructurePrefabsForStructure(p_structureSetting.structureType, p_structureSetting.resource);
            GameObject chosenStructurePrefab = CollectionUtilities.GetRandomElement(choices);
            tileObjectComponent.genericTileObject.InstantPlaceStructure(chosenStructurePrefab.name, PlayerManager.Instance.player.playerSettlement);
        }
        public void PlaceSelfBuildingDemonicStructure(StructureSetting p_structureSetting, int p_buildingTimeInTicks) {
            if (p_buildingTimeInTicks > 0) {
                List<GameObject> choices = InnerMapManager.Instance.GetStructurePrefabsForStructure(p_structureSetting.structureType, p_structureSetting.resource);
                GameObject chosenStructurePrefab = CollectionUtilities.GetRandomElement(choices);
                tileObjectComponent.genericTileObject.PlaceSelfBuildingStructure(chosenStructurePrefab.name, PlayerManager.Instance.player.playerSettlement, p_buildingTimeInTicks);
            } else {
                InstantPlaceDemonicStructure(p_structureSetting);
            }
        }

#region Hextile
        public Area GetNearestHexTileWithinRegion() {
            if (area.elevationType != ELEVATION.WATER && area.elevationType != ELEVATION.MOUNTAIN) {
                return area;
            }
            Area nearestArea = null;
            float nearestDist = 0f;
            for (int i = 0; i < area.region.areas.Count; i++) {
                Area otherArea = area.region.areas[i];
                if (otherArea.elevationType != ELEVATION.WATER && otherArea.elevationType != ELEVATION.MOUNTAIN) {
                    float dist = GetDistanceTo(otherArea.gridTileComponent.centerGridTile);
                    if (nearestArea == null || dist < nearestDist) {
                        nearestArea = otherArea;
                        nearestDist = dist;
                    }
                }
            }
            return nearestArea;
        }
        public Area GetNearestAreaWithinRegionThatCharacterHasPathTo(Character p_character) {
            if (p_character.movementComponent.HasPathTo(area)) {
                return area;
            }
            Area nearestArea = null;
            float nearestDist = 0f;
            for (int i = 0; i < area.region.areas.Count; i++) {
                Area a = area.region.areas[i];
                if (p_character.movementComponent.HasPathTo(a)) {
                    float dist = GetDistanceTo(a.gridTileComponent.centerGridTile);
                    if (nearestArea == null || dist < nearestDist) {
                        nearestArea = a;
                        nearestDist = dist;
                    }
                }
            }
            return nearestArea;
        }
        public Area GetNearestAreaWithinRegionThatIsNotMountainAndWaterAndHasNoSettlement() {
            if (area.elevationType != ELEVATION.MOUNTAIN && area.elevationType != ELEVATION.WATER && area.settlementOnArea == null) {
                return area;
            }
            Area nearestArea = null;
            float nearestDist = 0f;
            for (int i = 0; i < area.region.areas.Count; i++) {
                Area a = area.region.areas[i];
                if (a.elevationType != ELEVATION.MOUNTAIN && a.elevationType != ELEVATION.WATER && a.settlementOnArea == null) {
                    float dist = GetDistanceTo(a.gridTileComponent.centerGridTile);
                    if (nearestArea == null || dist < nearestDist) {
                        nearestArea = a;
                        nearestDist = dist;
                    }
                }
            }
            return nearestArea;
        }
        public Area GetNearestHexTileForNecromancerSpawnLair(Character p_necromancer) {
            if (area.elevationComponent.IsFully(ELEVATION.PLAIN) && !area.structureComponent.HasStructureInArea() && !area.IsNextToOrPartOfVillage() && p_necromancer.movementComponent.HasPathTo(area)) {
                return area;
            }
            Area nearestArea = null;
            float nearestDist = 0f;
            for (int i = 0; i < area.region.areas.Count; i++) {
                Area a = area.region.areas[i];
                if (a.elevationComponent.IsFully(ELEVATION.PLAIN) && !a.structureComponent.HasStructureInArea() && !a.IsNextToOrPartOfVillage() && p_necromancer.movementComponent.HasPathTo(a)) {
                    float dist = GetDistanceTo(a.gridTileComponent.centerGridTile);
                    if (nearestArea == null || dist < nearestDist) {
                        nearestArea = a;
                        nearestDist = dist;
                    }
                }
            }
            return nearestArea;
        }
#endregion

#region Blueprints
        public void SetHasBlueprint(bool hasBlueprint) {
            this.hasBlueprint = hasBlueprint;
            if (hasBlueprint) {
                area.AddBlueprint();
            } else {
                area.RemoveBlueprint();
            }
        }
#endregion

#region Connectors
        public void AddConnector(StructureConnector p_connector) {
            connectorsOnTile++;
// #if DEBUG_LOG
//             Debug.Log($"Added connector on {this}. Connectors on tile are {connectorsOnTile.ToString()}");
// #endif
            area.structureComponent.AddStructureConnector(p_connector);
        }
        public void RemoveConnector(StructureConnector p_connector) {
            connectorsOnTile--;
            area.structureComponent.RemoveStructureConnector(p_connector);
        }
#endregion

#region Plagued Rats
        public void AddPlaguedRats(bool p_randomizedPosition = false, bool isFromSpell = false) {
            Summon summon = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Rat, PlayerManager.Instance.player.playerFaction, homeRegion: parentMap.region);
            summon.OnSummonAsPlayerMonster();
            CharacterManager.Instance.PlaceSummonInitially(summon, this);
            if (isFromSpell) {
                summon.combatComponent.SetCombatMode(COMBAT_MODE.Defend);
            }
			if (p_randomizedPosition) {
                Vector3 pos = summon.mapObjectVisual.transform.position;
                pos.x += Random.Range(-1f, 1f);
                pos.y += Random.Range(-1f, 1f);
                summon.mapObjectVisual.transform.position = pos;
            }

            BaseSettlement settlement = null;
            if (this.structure.structureType != STRUCTURE_TYPE.WILDERNESS && this.structure.structureType != STRUCTURE_TYPE.OCEAN && IsPartOfSettlement(out settlement) && settlement.locationType != LOCATION_TYPE.VILLAGE) {
                summon.MigrateHomeStructureTo(this.structure);
            } else {
                summon.SetTerritory(this.area, false);
            }
            summon.jobQueue.CancelAllJobs();
            Messenger.Broadcast(PlayerSignals.PLAYER_PLACED_SUMMON, summon);
        }
#endregion

#region Spawn Necronomicon
        public void AddNecronomicon() {
            Artifact artifact = InnerMapManager.Instance.CreateNewArtifact(ARTIFACT_TYPE.Necronomicon);
            this.structure.AddPOI(artifact, this);
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

#region Biomes
        public void SetIndividualBiomeType(BIOMES p_biome) {
            BiomeDivision previousBiomeDivision =  parentMap.region.biomeDivisionComponent.GetBiomeDivision(mainBiomeType);
            previousBiomeDivision?.RemoveTile(this);
            mainBiomeType = p_biome;
            BiomeDivision biomeDivision =  parentMap.region.biomeDivisionComponent.GetBiomeDivision(p_biome);
            biomeDivision?.AddTile(this);
            if (previousBiomeDivision != null) {
                area.biomeComponent.OnTileInAreaChangedBiome(this, previousBiomeDivision.biome);    
            }
            
        }
        public void SetSpecificBiomeType(Biome_Tile_Type p_type) {
            specificBiomeTileType = p_type;
        }
#endregion

#region Elevation
        public void SetElevation(ELEVATION p_elevation) {
            ELEVATION previousElevation = elevationType;
            elevationType = p_elevation;
            area.elevationComponent.OnTileInAreaChangedElevation(this, previousElevation);
        }
#endregion

#region Node Points
        public GridNodeBase GetGridNodeByWorldPosition(Vector3 p_worldPos) {
            for (int i = 0; i < nodePoints.Length; i++) {
                Vector3 pos = GetNodePointWorldLocation(nodePoints[i]);
                if (pos.Equals(p_worldPos)) {
                    return GetGridNodeByNodePointIndex(i);
                }
            }
            return null;
        }
        private Vector3 GetNodePointWorldLocation(PointFloat p_point) {
            float posX = centeredWorldLocation.x + p_point.X;
            float posY = centeredWorldLocation.y + p_point.Y;
            Vector3 pos = new Vector3(posX, posY, centeredWorldLocation.z);
            return pos;
        }
        private Vector3 GetNodePointWorldLocation(GridNeighbourDirection p_direction) {
            int pointIndex = GetNodePointIndexByDirection(p_direction);
            float posX = centeredWorldLocation.x + nodePoints[pointIndex].X;
            float posY = centeredWorldLocation.y + nodePoints[pointIndex].Y;
            Vector3 pos = new Vector3(posX, posY, centeredWorldLocation.z);
            return pos;
        }
        private int GetNodePointIndexByDirection(GridNeighbourDirection p_direction) {
            switch (p_direction) {
                case GridNeighbourDirection.North_East: return 0;
                case GridNeighbourDirection.North_West: return 1;
                case GridNeighbourDirection.South_East: return 2;
                case GridNeighbourDirection.South_West: return 3;
                default: return -1;
            }
        }
        private GridNeighbourDirection GetDirectionByNodePointIndex(int p_index) {
            switch (p_index) {
                case 0: return GridNeighbourDirection.North_East;
                case 1: return GridNeighbourDirection.North_West;
                case 2: return GridNeighbourDirection.South_East;
                case 3: return GridNeighbourDirection.South_West;
                default: return GridNeighbourDirection.North;
            }
        }
        public GridNodeBase GetGridNodeByDirection(GridNeighbourDirection p_direction) {
            if(gridNodes[p_direction] == null) {
                Vector3 pos = GetNodePointWorldLocation(p_direction);
                gridNodes[p_direction] = AstarPath.active.GetNearest(pos).node as GridNodeBase;
            }
            return gridNodes[p_direction];
        }
        public GridNodeBase GetGridNodeByNodePointIndex(int p_index) {
            GridNeighbourDirection direction = GetDirectionByNodePointIndex(p_index);
            if (gridNodes[direction] == null) {
                Vector3 pos = GetNodePointWorldLocation(direction);
                gridNodes[direction] = AstarPath.active.GetNearest(pos).node as GridNodeBase;
            }
            return gridNodes[direction];
        }
        public Vector3 GetPositionWithinTileThatIsOnAWalkableNode() {
            //Used WalkableErosion because for some reason the Walkable field always returns true
            if (AstarPath.active.GetNearest(centeredWorldLocation).node is GridNodeBase grid && grid.WalkableErosion) {
                return centeredWorldLocation;
            } else {
                for (int i = 0; i < nodePoints.Length; i++) {
                    Vector3 pos = GetNodePointWorldLocation(nodePoints[i]);
                    GridNodeBase gridNode = GetGridNodeByNodePointIndex(i);
                    if (gridNode.WalkableErosion) {
                        return pos;
                    }
                }
            }
            return centeredWorldLocation;
        }
        public bool HasUnwalkableNodes() {
            for (int i = 0; i < nodePoints.Length; i++) {
                Vector3 pos = GetNodePointWorldLocation(nodePoints[i]);
                GridNodeBase gridNode = GetGridNodeByNodePointIndex(i);
                if (!gridNode.WalkableErosion) {
                    return true;
                }
            }
            return false;
        }
        public Vector3 GetUnoccupiedWalkablePositionInTileWithDistanceLimitOf(float p_distanceLimit, Vector3 p_otherPos) {
            for (int i = 0; i < nodePoints.Length; i++) {
                Vector3 pos = GetNodePointWorldLocation(nodePoints[i]);
                float dist = Vector2.Distance(pos, p_otherPos);
                if (dist <= p_distanceLimit) {
                    GridNodeBase gridNode = GetGridNodeByNodePointIndex(i);
                    if (gridNode.WalkableErosion && !IsGridNodeOccupiedByActiveCharacter(gridNode)) {
                        return pos;
                    }
                }
            }
            return Vector3.positiveInfinity;
        }
        public bool IsGridNodeOccupiedByActiveCharacter(GridNodeBase p_gridNode) {
            for (int i = 0; i < charactersHere.Count; i++) {
                Character character = charactersHere[i];
                if (!character.isDead && character.limiterComponent.canPerform && character.limiterComponent.canMove) {
                    if (AstarPath.active.GetNearest(character.worldPosition).node is GridNodeBase grid && grid == p_gridNode) {
                        return true;
                    }
                }
            }
            return false;
        }
        public bool IsGridNodeOccupiedByNonRepositioningActiveCharacterOtherThan(Character p_character) {
            GridNodeBase currentGridNode = AstarPath.active.GetNearest(p_character.worldPosition).node as GridNodeBase;
            for (int i = 0; i < charactersHere.Count; i++) {
                Character otherCharacter = charactersHere[i];
                if (p_character != otherCharacter && !otherCharacter.isDead && otherCharacter.limiterComponent.canPerform && otherCharacter.limiterComponent.canMove) {
                    if (otherCharacter.combatComponent.isInCombat) {
                        if (otherCharacter.stateComponent.currentState is CombatState combatState) {
                            if (combatState.isRepositioning) {
                                if(combatState.repositioningTo != currentGridNode) {
                                    //Do not include in checking if another character is already leaving the grid node, i.e., that character is repositioning to another grid node
                                    continue;
                                }
                            }
                        }
                    }
                    if (AstarPath.active.GetNearest(otherCharacter.worldPosition).node is GridNodeBase grid && grid == currentGridNode) {
                        return true;
                    }
                }
            }
            return false;
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
            _fourNeighboursList?.Clear();
            _fourNeighboursList = null;
            neighbourList?.Clear();
            neighbourList = null;
            charactersHere?.Clear();
            // charactersHere = null;
            tileObjectComponent.CleanUp();
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