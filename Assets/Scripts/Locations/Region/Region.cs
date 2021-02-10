﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Region_Features;
using Locations.Settlements;
using Logs;
using PathFind;
using SpriteGlow;
using UnityEngine;
using UtilityScripts;
using Random = UnityEngine.Random;

public class Region : ISavable, ILogFiller {
    public string persistentID { get; }
    public int id { get; }
    public string name { get; private set; }
    public string description => GetDescription();
    public Dictionary<GridNeighbourDirection, Region> neighboursWithDirection { get; private set; }
    public List<Region> neighbours { get; private set; }
    public List<HexTile> tiles { get; private set; }
    public List<HexTile> shuffledNonMountainWaterTiles { get; private set; }
    public HexTile coreTile { get; private set; }
    public Color regionColor { get; }
    public List<Faction> factionsHere { get; private set; }
    public List<Character> residents { get; private set; }
    public List<Character> charactersAtLocation { get; private set; }
    public HexTile[,] hexTileMap { get; private set; }
    public Area[,] areaMap { get; private set; }
    public Dictionary<STRUCTURE_TYPE, List<LocationStructure>> structures { get; private set; }
    public List<LocationStructure> allStructures { get; private set; }
    public RegionFeatureComponent regionFeatureComponent { get; }
    public List<BaseSettlement> settlementsInRegion { get; private set; }
    public RegionDivisionComponent regionDivisionComponent { get; }
    /// <summary>
    /// Number of tile objects in this region categorized by type.
    /// NOTE: This isn't saved/loaded since this is updated every time a new tile object is placed.
    /// So loading this from a file would result in duplicates, since we still go through the process of
    /// placing tile objects when loading them.
    /// NOTE: This just includes BUILT objects!
    /// </summary>
    public Dictionary<TILE_OBJECT_TYPE, int> objectsInRegionCount { get; private set; }

    private RegionInnerTileMap _regionInnerTileMap; //inner map of the region, this should only be used if this region does not have an npcSettlement. 
    private string _activeEventAfterEffectScheduleId;
    private List<Border> _borders;
    private int _canShowNotificationVotes;

    #region getter/setter
    public BaseLandmark mainLandmark => coreTile.landmarkOnTile;
    public InnerTileMap innerMap => _regionInnerTileMap;
    public OBJECT_TYPE objectType => OBJECT_TYPE.Region;
    public Type serializedData => typeof(SaveDataRegion);
    public bool canShowNotifications => _canShowNotificationVotes > 0;
    #endregion

    private Region() {
        charactersAtLocation = new List<Character>();
        factionsHere = new List<Faction>();
        residents = new List<Character>();
        regionFeatureComponent = new RegionFeatureComponent();
        settlementsInRegion = new List<BaseSettlement>();
        neighbours = new List<Region>();
        neighboursWithDirection = new Dictionary<GridNeighbourDirection, Region>();
        objectsInRegionCount = new Dictionary<TILE_OBJECT_TYPE, int>();
    }
    public Region(HexTile coreTile, string p_name = "") : this() {
        persistentID = System.Guid.NewGuid().ToString();
        id = UtilityScripts.Utilities.SetID(this);
        name = string.IsNullOrEmpty(p_name) ? RandomNameGenerator.GetRegionName() : p_name;
        this.coreTile = coreTile;
        tiles = new List<HexTile>();
        shuffledNonMountainWaterTiles = new List<HexTile>();
        AddTile(coreTile);
        regionColor = GenerateRandomRegionColor();
        regionDivisionComponent = new RegionDivisionComponent();
        Debug.Log($"Created region {this.name} with core tile {coreTile.ToString()}");
    }
    public Region(SaveDataRegion data) : this() {
        persistentID = data.persistentID;
        id = UtilityScripts.Utilities.SetID(this, data.id);
        name = data.name;
        coreTile = GridMap.Instance.normalHexTiles[data.coreTileID];
        tiles = new List<HexTile>();
        shuffledNonMountainWaterTiles = new List<HexTile>();
        regionColor = data.regionColor;
        objectsInRegionCount = new Dictionary<TILE_OBJECT_TYPE, int>();

        //Components
        regionDivisionComponent = data.regionDivisionComponent.Load();
    }

    #region Loading
    public void LoadReferences(SaveDataRegion saveDataRegion) {
        string summary = $"Loading {name} references:";
        summary = $"{summary}\nLoading Residents:";
        for (int i = 0; i < saveDataRegion.residentIDs.Length; i++) {
            string residentID = saveDataRegion.residentIDs[i];
            Character resident = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(residentID);
            if (resident != null) {
                residents.Add(resident);
                summary = $"{summary}\n- {resident.name}";    
            } else {
                Debug.LogWarning($"Trying to add resident at {name} with ID {residentID} but could not find character with that ID");
            }
            
        }
        summary = $"{summary}\nLoading characters at Location:";
        for (int i = 0; i < saveDataRegion.charactersAtLocationIDs.Length; i++) {
            string charactersAtLocationID = saveDataRegion.charactersAtLocationIDs[i];
            Character character = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(charactersAtLocationID);
            if (character != null) {
                charactersAtLocation.Add(character);
                summary = $"{summary}\n- {character.name}";    
            } else {
                Debug.LogWarning($"Trying to add character at location {name} with ID {charactersAtLocationID} but could not find character with that ID");
            }
        }

        foreach (KeyValuePair<GridNeighbourDirection, string> item in saveDataRegion.neighboursWithDirection) {
            neighboursWithDirection.Add(item.Key, DatabaseManager.Instance.regionDatabase.GetRegionByPersistentID(item.Value));
        }

        for (int i = 0; i < saveDataRegion.neighbours.Count; i++) {
            neighbours.Add(DatabaseManager.Instance.regionDatabase.GetRegionByPersistentID(saveDataRegion.neighbours[i]));
        }

        for (int i = 0; i < saveDataRegion.factionsHereIDs.Length; i++) {
            string factionHereID = saveDataRegion.factionsHereIDs[i];
            Faction faction = DatabaseManager.Instance.factionDatabase.GetFactionBasedOnPersistentID(factionHereID);
            factionsHere.Add(faction);
        }
        
        Debug.Log(summary);
    }
    #endregion

    #region Tiles
    public void AddTile(HexTile tile) {
        if (!tiles.Contains(tile)) {
            tiles.Add(tile);
            if(tile.elevationType != ELEVATION.MOUNTAIN && tile.elevationType != ELEVATION.WATER) {
                if(shuffledNonMountainWaterTiles.Count > 1) {
                    int index = UnityEngine.Random.Range(0, shuffledNonMountainWaterTiles.Count + 1);
                    if(index == shuffledNonMountainWaterTiles.Count) {
                        shuffledNonMountainWaterTiles.Add(tile);
                    } else {
                        shuffledNonMountainWaterTiles.Insert(index, tile);
                    }
                } else {
                    shuffledNonMountainWaterTiles.Add(tile);
                }
            }
            tile.SetRegion(this);
        }
    }
    private void RemoveTile(HexTile tile) {
        if (tiles.Remove(tile)) {
            shuffledNonMountainWaterTiles.Remove(tile);
            tile.SetRegion(null);
        }
    }
    #endregion

    #region Utilities
    public void SetName(string name) {
        this.name = name;
    }
    private Color GenerateRandomRegionColor() {
        if (id == 1) {
            return Color.cyan;
        } else if (id == 2) {
            return Color.yellow;
        } else if (id == 3) {
            return Color.green;
        } else if (id == 4) {
            return Color.red;
        } else if (id == 5) {
            return Color.magenta;
        }
        return Random.ColorHSV();
    }
    private string GetDescription() {
        // if (coreTile.isCorrupted) {
        //     if (mainLandmark.specificLandmarkType == LANDMARK_TYPE.NONE) {
        //         return "This region is empty. You may assign a minion to build a demonic landmark here.";
        //     }
        // }
        // return LandmarkManager.Instance.GetLandmarkData(mainLandmark.specificLandmarkType).description;
        return string.Empty;
    }
    public void FinalizeData() {
        DetermineHexTileMap();
    }
    public void GenerateOuterBorders() {
        _borders = GetOuterBorders();
    }
    private List<HexTile> GetOuterTiles() {
        List<HexTile> outerTiles = new List<HexTile>();
        for (int i = 0; i < tiles.Count; i++) {
            HexTile currTile = tiles[i];
            if (currTile.AllNeighbours.Count != 6 || currTile.HasNeighbourFromOtherRegion()) {
                outerTiles.Add(currTile);
            }
        }
        return outerTiles;
    }
    private List<Border> GetOuterBorders() {
        List<HexTile> outerTiles = GetOuterTiles();
        List<Border> borders = new List<Border>();
        HEXTILE_DIRECTION[] dirs = CollectionUtilities.GetEnumValues<HEXTILE_DIRECTION>();
        
        GameObject borderParent = new GameObject($"{this.name} Borders");
        borderParent.transform.SetParent(GridMap.Instance.transform);
        
        for (int i = 0; i < outerTiles.Count; i++) {
            HexTile currTile = outerTiles[i];
            for (int j = 0; j < dirs.Length; j++) {
                HEXTILE_DIRECTION dir = dirs[j];
                if (dir == HEXTILE_DIRECTION.NONE) { continue; }
                HexTile neighbour = currTile.GetNeighbour(dir);
                if (neighbour == null || neighbour.region != currTile.region) {
                    SpriteRenderer border = currTile.GetBorder(dir);
                    
                    GameObject borderGO = new GameObject("Region Border");
                    borderGO.transform.SetParent(borderParent.transform);
                    borderGO.transform.localScale = border.transform.localScale;
                    borderGO.transform.position = border.gameObject.transform.position;
                    
                    SpriteRenderer regionBorder = borderGO.AddComponent<SpriteRenderer>();
                    regionBorder.sprite = border.sprite;
                    regionBorder.sortingOrder = border.sortingOrder;
                    regionBorder.sortingLayerName = border.sortingLayerName;
                    regionBorder.color = this.regionColor;

                    SpriteGlowEffect glowEffect = borderGO.AddComponent<SpriteGlowEffect>();
                    glowEffect.GlowColor = regionColor;
                    glowEffect.GlowBrightness = 1.5f;
                    glowEffect.OutlineWidth = 2;

                    borders.Add(new Border(regionBorder, glowEffect));
                }
            }
        }
        return borders;
    }
    //public List<Region> AdjacentRegions() {
    //    List<Region> adjacent = null;
    //    for (int i = 0; i < tiles.Count; i++) {
    //        HexTile currTile = tiles[i];
    //        List<Region> regions;
    //        if (currTile.TryGetDifferentRegionNeighbours(out regions)) {
    //            for (int j = 0; j < regions.Count; j++) {
    //                Region currRegion = regions[j];
    //                if(adjacent == null) { adjacent = new List<Region>(); }
    //                if (!adjacent.Contains(currRegion)) {
    //                    adjacent.Add(currRegion);
    //                }
    //            }
    //        }
    //    }
    //    return adjacent;
    //}
    public void OnHoverOverAction() { }
    public void OnHoverOutAction() { }
    public void ShowBorders(Color color, bool showGlow = false) {
        for (int i = 0; i < _borders.Count; i++) {
            Border s = _borders[i];
            s.SetBorderState(true);
            s.SetColor(color);
            s.SetGlowState(showGlow);
        }
    }
    public void HideBorders(bool glowState = false) {
        for (int i = 0; i < _borders.Count; i++) {
            Border s = _borders[i];
            s.SetBorderState(false);
            s.SetGlowState(glowState);
        }
    }
    public void SetBorderGlowEffectState(bool state) {
        for (int i = 0; i < _borders.Count; i++) {
            Border s = _borders[i];
            s.SetGlowState(state);
        }
    }
    public void CenterCameraOnRegion() {
        coreTile.CenterCameraHere();
    }
    public bool HasTileWithFeature(string featureName) {
        for (int i = 0; i < tiles.Count; i++) {
            HexTile tile = tiles[i];
            if (tile.featureComponent.HasFeature(featureName)) {
                return true;
            }
        }
        return false;
    }
    public List<HexTile> GetTilesWithFeature(string featureName) {
        List<HexTile> tilesWithFeature = new List<HexTile>();
        for (int i = 0; i < tiles.Count; i++) {
            HexTile tile = tiles[i];
            if (tile.featureComponent.HasFeature(featureName)) {
                tilesWithFeature.Add(tile);
            }
        }
        return tilesWithFeature;
    }
    public List<HexTile> GetAreasOccupiedByVillagers() {
        List<HexTile> areas = null;
        for (int i = 0; i < residents.Count; i++) {
            Character regionResident = residents[i];
            if (regionResident.isNormalCharacter && regionResident.HasTerritory()) {
                if (areas == null) {
                    areas = new List<HexTile>();
                }
                if (areas.Contains(regionResident.territory) == false) {
                    areas.Add(regionResident.territory);
                }
            }
        }
        return areas;
    }
    public void PopulateNeighbours() {
        for (int i = 0; i < tiles.Count; i++) {
            HexTile currTile = tiles[i];
            if (currTile.TryGetDifferentRegionNeighbours(out var regions)) {
                for (int j = 0; j < regions.Count; j++) {
                    Region currRegion = regions[j];
                    if (this != currRegion) {
                        if (!neighbours.Contains(currRegion)) {
                            neighbours.Add(currRegion);
                        }    
                    }
                }
                // for (int j = 0; j < regions.Count; j++) {
                //     Region currRegion = regions[j];
                //     if(this != currRegion) {
                //         Vector3 direction = (currRegion.coreTile.transform.position - coreTile.transform.position).normalized;
                //         GridNeighbourDirection neighbourDir = GridNeighbourDirection.East;
                //         if (direction.y > 0) {
                //             neighbourDir = GridNeighbourDirection.North;
                //             if(direction.x > 0) {
                //                 neighbourDir = GridNeighbourDirection.North_East;
                //             } else if (direction.x < 0) {
                //                 neighbourDir = GridNeighbourDirection.North_West;
                //             }
                //         } else if (direction.y < 0) {
                //             neighbourDir = GridNeighbourDirection.South;
                //             if (direction.x > 0) {
                //                 neighbourDir = GridNeighbourDirection.South_East;
                //             } else if (direction.x < 0) {
                //                 neighbourDir = GridNeighbourDirection.South_West;
                //             }
                //         } else {
                //             if (direction.x < 0) {
                //                 neighbourDir = GridNeighbourDirection.West;
                //             } else if (direction.x > 0) {
                //                 neighbourDir = GridNeighbourDirection.East;
                //             }
                //         }
                //         //if (direction.x > 0 && direction.y > 0) {
                //         //    neighbourDir = GridNeighbourDirection.North_East;
                //         //} else if (direction.x > 0 && direction.y < 0) {
                //         //    neighbourDir = GridNeighbourDirection.South_East;
                //         //} else if (direction.x < 0 && direction.y > 0) {
                //         //    neighbourDir = GridNeighbourDirection.North_West;
                //         //} else if (direction.x < 0 && direction.y < 0) {
                //         //    neighbourDir = GridNeighbourDirection.South_West;
                //         //} else if (direction.x < 0) {
                //         //    neighbourDir = GridNeighbourDirection.West;
                //         //} else if (direction.x > 0) {
                //         //    neighbourDir = GridNeighbourDirection.East;
                //         //} else if (direction.y > 0) {
                //         //    neighbourDir = GridNeighbourDirection.North;
                //         //} else if (direction.y < 0) {
                //         //    neighbourDir = GridNeighbourDirection.South;
                //         //}
                //         if (!neighboursWithDirection.ContainsKey(neighbourDir)) {
                //             neighboursWithDirection.Add(neighbourDir, currRegion);
                //         }
                //         if (!neighbours.Contains(currRegion)) {
                //             neighbours.Add(currRegion);
                //         }
                //     }
                // }
            }
        }

        // //If region has no West neighbour, we must not have a North West/South West Neighbours because we cannot have diagonal only neighbours
        // //So if there is a North/South West neighbours they are probably just North/South, so we must change the key to North/South
        // //Same as East
        // if (!neighboursWithDirection.ContainsKey(GridNeighbourDirection.West)) {
        //     if (!neighboursWithDirection.ContainsKey(GridNeighbourDirection.North)) {
        //         if (neighboursWithDirection.ContainsKey(GridNeighbourDirection.North_West)) {
        //             neighboursWithDirection.Add(GridNeighbourDirection.North, neighboursWithDirection[GridNeighbourDirection.North_West]);
        //             neighboursWithDirection.Remove(GridNeighbourDirection.North_West);
        //         }
        //     }
        //     if (!neighboursWithDirection.ContainsKey(GridNeighbourDirection.South)) {
        //         if (neighboursWithDirection.ContainsKey(GridNeighbourDirection.South_West)) {
        //             neighboursWithDirection.Add(GridNeighbourDirection.South, neighboursWithDirection[GridNeighbourDirection.South_West]);
        //             neighboursWithDirection.Remove(GridNeighbourDirection.South_West);
        //         }
        //     }
        // }
        // if (!neighboursWithDirection.ContainsKey(GridNeighbourDirection.East)) {
        //     if (!neighboursWithDirection.ContainsKey(GridNeighbourDirection.North)) {
        //         if (neighboursWithDirection.ContainsKey(GridNeighbourDirection.North_East)) {
        //             neighboursWithDirection.Add(GridNeighbourDirection.North, neighboursWithDirection[GridNeighbourDirection.North_East]);
        //             neighboursWithDirection.Remove(GridNeighbourDirection.North_East);
        //         }
        //     }
        //     if (!neighboursWithDirection.ContainsKey(GridNeighbourDirection.South)) {
        //         if (neighboursWithDirection.ContainsKey(GridNeighbourDirection.South_East)) {
        //             neighboursWithDirection.Add(GridNeighbourDirection.South, neighboursWithDirection[GridNeighbourDirection.South_East]);
        //             neighboursWithDirection.Remove(GridNeighbourDirection.South_East);
        //         }
        //     }
        // }
        
        //compute north, south, east and west region neighbours
        int width = hexTileMap.GetUpperBound(0);
        int height = hexTileMap.GetUpperBound(1);
        int midX = (width) / 2;
        int midY = (height) / 2;
        HexTile leftMostCenter = hexTileMap[0, midY];
        HexTile rightMostCenter = hexTileMap[width, midY];
        HexTile bottomMostCenter = hexTileMap[midX, 0];
        HexTile topMostCenter = hexTileMap[midX, height];

        HexTile left = leftMostCenter.GetNeighbour(HEXTILE_DIRECTION.WEST);
        HexTile right = rightMostCenter.GetNeighbour(HEXTILE_DIRECTION.EAST);
        HexTile bottom = bottomMostCenter.GetNeighbour(HEXTILE_DIRECTION.SOUTH_WEST);
        HexTile top = topMostCenter.GetNeighbour(HEXTILE_DIRECTION.NORTH_EAST);
        
        string summary = $"Neighbours of region {name}:";
        if (left != null) {
            neighboursWithDirection.Add(GridNeighbourDirection.West, left.region);
            summary = $"{summary}\nWest - {left.xCoordinate.ToString()}, {left.yCoordinate.ToString()}";
        }
        if (right != null) {
            neighboursWithDirection.Add(GridNeighbourDirection.East, right.region);
            summary = $"{summary}\nEast - {right.xCoordinate.ToString()}, {right.yCoordinate.ToString()}";
        }
        if (bottom != null) {
            neighboursWithDirection.Add(GridNeighbourDirection.South, bottom.region);
            summary = $"{summary}\nSouth - {bottom.xCoordinate.ToString()}, {bottom.yCoordinate.ToString()}";
        }
        if (top != null) {
            neighboursWithDirection.Add(GridNeighbourDirection.North, top.region);
            summary = $"{summary}\nNorth - {top.xCoordinate.ToString()}, {top.yCoordinate.ToString()}";
        }
        Debug.Log(summary); //Test
    }
    public bool HasNeighbourInDirection(GridNeighbourDirection direction) {
        return GetNeighbourInDirection(direction) != null;
    }
    public Region GetNeighbourInDirection(GridNeighbourDirection direction) {
        if (neighboursWithDirection.ContainsKey(direction)) {
            return neighboursWithDirection[direction];
        }
        return null;
    }
    #endregion

    #region Characters
    public void LoadCharacterHere(Character character) {
        charactersAtLocation.Add(character);
        character.SetRegionLocation(this);
        Messenger.Broadcast(RegionSignals.CHARACTER_ENTERED_REGION, character, this);
    }
    public void AddCharacterToLocation(Character character, LocationGridTile tileOverride = null, bool isInitial = false) {
        character.SetRegionLocation(this);
        if (!charactersAtLocation.Contains(character)) {
            charactersAtLocation.Add(character);
            Messenger.Broadcast(RegionSignals.CHARACTER_ENTERED_REGION, character, this);
        }
    }
    public void RemoveCharacterFromLocation(Character character) {
        if (charactersAtLocation.Remove(character)) {
            character.currentStructure?.RemoveCharacterAtLocation(character);
            // for (int i = 0; i < features.Count; i++) {
            //     features[i].OnRemoveCharacterFromRegion(this, character);
            // }
            character.SetRegionLocation(null);
            Messenger.Broadcast(RegionSignals.CHARACTER_EXITED_REGION, character, this);
        }
    }
    //public void RemoveCharacterFromLocation(Party party) {
    //    RemoveCharacterFromLocation(party.owner);
    //}
    public bool IsResident(Character character) {
        return residents.Contains(character);
    }
    public bool AddResident(Character character) {
        if (!residents.Contains(character)) {
            residents.Add(character);
            character.SetHomeRegion(this);
        }
        return false;
    }
    public void RemoveResident(Character character) {
        if (residents.Remove(character)) {
            character.SetHomeRegion(null);
        }
    }
    public int GetCountOfAliveCharacterWithSameTerritory(Character character) {
        int count = 0;
        if (character.HasTerritory()) {
            for (int i = 0; i < residents.Count; i++) {
                Character resident = residents[i];
                if (resident != character && !resident.isDead && resident.HasTerritory()) {
                    if (resident.IsTerritory(character.territory)) {
                        count++;
                    }
                }
            }
        }
        return count;
    }
    public Character GetRandomCharacterWithPathAndFaction(Character source) {
        List<Character> validCharacters = ObjectPoolManager.Instance.CreateNewCharactersList();
        Character chosenCharacter = null;
        for (int i = 0; i < charactersAtLocation.Count; i++) {
            Character character = charactersAtLocation[i];
            if (source != character && source.movementComponent.HasPathTo(character.gridTileLocation) && !character.isDead && character.faction == source.faction) {
                validCharacters.Add(character);
            }
        }
        if(validCharacters != null) {
            chosenCharacter = UtilityScripts.CollectionUtilities.GetRandomElement(validCharacters);
        }
        ObjectPoolManager.Instance.ReturnCharactersListToPool(validCharacters);
        return chosenCharacter;
    }
    public Character GetRandomCharacterThatMeetCriteria(System.Func<Character, bool> validityChecker) {
        List<Character> validCharacters = ObjectPoolManager.Instance.CreateNewCharactersList();
        Character chosenCharacter = null;
        for (int i = 0; i < charactersAtLocation.Count; i++) {
            Character character = charactersAtLocation[i];
            if (validityChecker.Invoke(character)) {
                validCharacters.Add(character);
            }
        }
        if (validCharacters != null) {
            chosenCharacter = UtilityScripts.CollectionUtilities.GetRandomElement(validCharacters);
        }
        ObjectPoolManager.Instance.ReturnCharactersListToPool(validCharacters);
        return chosenCharacter;
    }
    #endregion

    #region Faction
    public void AddFactionHere(Faction faction) {
        if (!IsFactionHere(faction) && faction.isMajorFaction) {
            factionsHere.Add(faction);
            //Once a faction is added and there is no ruling faction yet, automatically let the added faction own the region
            // if(owner == null) {
            //     LandmarkManager.Instance.OwnRegion(faction, this);
            // }
        }
    }
    public void RemoveFactionHere(Faction faction) {
        if (factionsHere.Remove(faction)) {
            //If a faction is removed and it is the ruling faction, transfer ruling faction to the next faction on the list if there's any, if not make the region part of neutral faction
            // if(owner == faction) {
            //     LandmarkManager.Instance.UnownSettlement(this);
            //     if(factionsHere.Count > 0) {
            //         LandmarkManager.Instance.OwnRegion(factionsHere[0], this);
            //     } else {
            //         FactionManager.Instance.neutralFaction.AddToOwnedSettlements(this);
            //     }
            // }
        }
    }
    public bool IsFactionHere(Faction faction) {
        return factionsHere.Contains(faction);
    }
    #endregion
    
    #region Structures
    public void CreateStructureList() {
        structures = new Dictionary<STRUCTURE_TYPE, List<LocationStructure>>();
        allStructures = new List<LocationStructure>();
    }
    public void GenerateStructures() {
        CreateStructureList();
        LandmarkManager.Instance.CreateNewStructureAt(this, STRUCTURE_TYPE.WILDERNESS);
    }
    public void AddStructure(LocationStructure structure) {
        Debug.Assert(!structure.hasBeenDestroyed, $"Structure {structure} has been destroyed but is being added to {name}");
        if (!structures.ContainsKey(structure.structureType)) {
            structures.Add(structure.structureType, new List<LocationStructure>());
        }

        if (!structures[structure.structureType].Contains(structure)) {
            structures[structure.structureType].Add(structure);
            allStructures.Add(structure);
            // Debug.Log($"New structure {structure.name} was added to region {name}" );
        }
    }
    public void RemoveStructure(LocationStructure structure) {
        if (structures.ContainsKey(structure.structureType)) {
            if (structures[structure.structureType].Remove(structure)) {
                allStructures.Remove(structure);
                if (structures[structure.structureType].Count == 0) { //this is only for optimization
                    structures.Remove(structure.structureType);
                }
            }
        }
    }
    public LocationStructure GetRandomStructureOfType(STRUCTURE_TYPE type) {
        if (structures.ContainsKey(type)) {
            return structures[type][UtilityScripts.Utilities.Rng.Next(0, structures[type].Count)];
        }
        return null;
    }
    public LocationStructure GetFirstUnoccupiedStructureOfType(STRUCTURE_TYPE type) {
        if (structures.ContainsKey(type)) {
            List<LocationStructure> structuresOfType = structures[type];
            for (int i = 0; i < structuresOfType.Count; i++) {
                if (!structuresOfType[i].IsOccupied()) {
                    return structuresOfType[i];
                }
            }
        }
        return null;
    }
    public LocationStructure GetRandomStructure() {
        LocationStructure randomStructure = null;
        while (randomStructure == null) {
            KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> kvp = structures.ElementAt(UnityEngine.Random.Range(0, structures.Count));
            if (kvp.Key != STRUCTURE_TYPE.CAVE && kvp.Key != STRUCTURE_TYPE.OCEAN && kvp.Value.Count > 0) {
                randomStructure = kvp.Value[UnityEngine.Random.Range(0, kvp.Value.Count)];
            }
        }
        return randomStructure;
        //Dictionary<STRUCTURE_TYPE, List<LocationStructure>> _allStructures = new Dictionary<STRUCTURE_TYPE, List<LocationStructure>>(this.structures);
        //_allStructures.Remove(STRUCTURE_TYPE.CAVE);
        //_allStructures.Remove(STRUCTURE_TYPE.OCEAN);
        //int dictIndex = UnityEngine.Random.Range(0, _allStructures.Count);
        //int count = 0;
        //foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> kvp in _allStructures) {
        //    if (count == dictIndex) {
        //        return kvp.Value[UnityEngine.Random.Range(0, kvp.Value.Count)];
        //    }
        //    count++;
        //}
        //return null;
    }
    public LocationStructure GetRandomStructureThatMeetCriteria(System.Func<LocationStructure, bool> checker) {
        List<LocationStructure> structureChoices = ObjectPoolManager.Instance.CreateNewStructuresList();
        LocationStructure chosenStructure = null;
        for (int i = 0; i < allStructures.Count; i++) {
            LocationStructure currStructure = allStructures[i];
            if (checker.Invoke(currStructure)) {
                structureChoices.Add(currStructure);
            }
        }
        if (structureChoices != null && structureChoices.Count > 0) {
            chosenStructure = structureChoices[UnityEngine.Random.Range(0, structureChoices.Count)];
        }
        ObjectPoolManager.Instance.ReturnStructuresListToPool(structureChoices);
        return chosenStructure;
    }
    public LocationStructure GetRandomStructureOfTypeThatMeetCriteria(System.Func<LocationStructure, bool> checker, params STRUCTURE_TYPE[] type) {
        List<LocationStructure> structureChoices = ObjectPoolManager.Instance.CreateNewStructuresList();
        LocationStructure chosenStructure = null;
        for (int i = 0; i < type.Length; i++) {
            if (structures.ContainsKey(type[i])) {
                List<LocationStructure> structuresOfType = structures[type[i]];
                for (int j = 0; j < structuresOfType.Count; j++) {
                    LocationStructure possibleStructure = structuresOfType[j];
                    if (checker.Invoke(possibleStructure)) {
                        structureChoices.Add(possibleStructure);
                    }
                }
            }
        }
        if (structureChoices != null && structureChoices.Count > 0) {
            chosenStructure = structureChoices[UnityEngine.Random.Range(0, structureChoices.Count)];
        }
        ObjectPoolManager.Instance.ReturnStructuresListToPool(structureChoices);
        return chosenStructure;
    }
    public LocationStructure GetRandomSpecialStructureExcept(List<LocationStructure> exceptions) {
        List<LocationStructure> specialStructures = ObjectPoolManager.Instance.CreateNewStructuresList();
        LocationStructure chosenStructure = null;
        for (int i = 0; i < allStructures.Count; i++) {
            LocationStructure currStructure = allStructures[i];
            if (currStructure.settlementLocation != null && currStructure.settlementLocation.locationType == LOCATION_TYPE.DUNGEON && currStructure.passableTiles.Count > 0) {
                if(exceptions.Contains(currStructure)) { continue; }
                specialStructures.Add(currStructure);
            }
        }
        if (specialStructures != null && specialStructures.Count > 0) {
            chosenStructure = specialStructures[UnityEngine.Random.Range(0, specialStructures.Count)];
        }
        ObjectPoolManager.Instance.ReturnStructuresListToPool(specialStructures);
        return chosenStructure;
    }
    public LocationStructure GetStructureOfTypeWithoutSettlement(STRUCTURE_TYPE type) {
        if (structures.ContainsKey(type)) {
            List<LocationStructure> chosenStructures = structures[type];
            for (int i = 0; i < chosenStructures.Count; i++) {
                if(chosenStructures[i].settlementLocation == null) {
                    return chosenStructures[i];
                }
            }
        }
        return null;
    }
    public LocationStructure GetStructureByID(STRUCTURE_TYPE type, int id) {
        if (structures.ContainsKey(type)) {
            List<LocationStructure> locStructures = structures[type];
            for (int i = 0; i < locStructures.Count; i++) {
                if(locStructures[i].id == id) {
                    return locStructures[i];
                }
            }
        }
        return null;
    }
    public List<LocationStructure> GetStructuresAtLocation() {
        List<LocationStructure> structuresAtLocation = new List<LocationStructure>();
        foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> kvp in this.structures) {
            for (int i = 0; i < kvp.Value.Count; i++) {
                LocationStructure currStructure = kvp.Value[i];
                structuresAtLocation.Add(currStructure);
            }
        }
        return structuresAtLocation;
    }
    public List<T> GetStructuresAtLocation<T>(STRUCTURE_TYPE type) where T : LocationStructure{
        List<T> structuresAtLocation = new List<T>();
        foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> kvp in this.structures) {
            for (int i = 0; i < kvp.Value.Count; i++) {
                LocationStructure currStructure = kvp.Value[i];
                if (currStructure.structureType == type) {
                    structuresAtLocation.Add(currStructure as T);
                }
            }
        }
        return structuresAtLocation;
    }
    public IPointOfInterest GetFirstTileObjectOnTheFloorOwnedBy(Character character, System.Func<IPointOfInterest, bool> validityChecker = null) {
        foreach (List<LocationStructure> structureList in structures.Values) {
            for (int i = 0; i < structureList.Count; i++) {
                LocationStructure currStructure = structureList[i];
                if (currStructure.occupiedArea != null && currStructure.occupiedArea.settlementOnTile == character.homeSettlement) {
                    for (int j = 0; j < currStructure.pointsOfInterest.Count; j++) {
                        IPointOfInterest poi = currStructure.pointsOfInterest.ElementAt(j);
                        if(poi.gridTileLocation != null && poi.IsOwnedBy(character)) {
                            if (validityChecker != null) {
                                if (validityChecker.Invoke(poi)) {
                                    return poi;
                                }
                            } else {
                                return poi;    
                            }
                        }
                    }
                }
            }
        }
        return null;
    }
    public bool HasStructure(STRUCTURE_TYPE type) {
        return structures.ContainsKey(type);
    }
    #endregion

    #region Inner Map
    public void SetRegionInnerMap(RegionInnerTileMap regionInnerTileMap) {
        _regionInnerTileMap = regionInnerTileMap;
    }
    //public bool AddSpecialTokenToLocation(SpecialToken token, LocationStructure structure = null, LocationGridTile gridLocation = null) {
    //    token.SetOwner(this.owner);
    //    if (innerMap != null) { //if the npcSettlement map of this npcSettlement has already been created.
    //        if (structure != null) {
    //            structure.AddItem(token, gridLocation);
    //        } else {
    //            //get structure for token
    //            LocationStructure chosen = InnerMapManager.Instance.GetRandomStructureToPlaceItem(this, token);
    //            chosen.AddItem(token);
    //        }
    //    }
    //    return true;
    //}
    //public void RemoveSpecialTokenFromLocation(SpecialToken token) {
    //    LocationStructure takenFrom = token.structureLocation;
    //    if (takenFrom != null) {
    //        takenFrom.RemoveItem(token);
    //    }
    //}
    public bool IsRequiredByLocation(TileObject item) {
        return false;
    }
    public void AllowNotifications() {
        _canShowNotificationVotes++;
    }
    public void BlockNotifications() {
        _canShowNotificationVotes--;
    }
    public List<TileObject> GetTileObjectsOfType(TILE_OBJECT_TYPE type) {
        List<TileObject> objs = new List<TileObject>();
        foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> keyValuePair in structures) {
            for (int i = 0; i < keyValuePair.Value.Count; i++) {
                List<TileObject> tileObjects = keyValuePair.Value[i].GetTileObjectsOfType(type);
                if(tileObjects != null) {
                    objs.AddRange(tileObjects);
                }
            }
        }
        return objs;
    }
    public bool HasTileObjectOfType(TILE_OBJECT_TYPE type) {
        foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> keyValuePair in structures) {
            for (int i = 0; i < keyValuePair.Value.Count; i++) {
                LocationStructure structure = keyValuePair.Value[i];
                if (structure.HasTileObjectOfType(type)) {
                    return true;
                }
            }
        }
        return false;
    }
    public List<T> GetTileObjectsOfType<T>() where T : TileObject{
        List<T> objs = new List<T>();
        foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> keyValuePair in structures) {
            for (int i = 0; i < keyValuePair.Value.Count; i++) {
                objs.AddRange(keyValuePair.Value[i].GetTileObjectsOfType<T>());
            }
        }
        return objs;
    }
    #endregion

    #region Hex Tile Map
    private void DetermineHexTileMap() {
        if(tiles.Count <= 0) {
            return;
        }
        int maxX = tiles.Max(t => t.data.xCoordinate);
        int minX = tiles.Min(t => t.data.xCoordinate);
        int maxY = tiles.Max(t => t.data.yCoordinate);
        int minY = tiles.Min(t => t.data.yCoordinate);

        int width = maxX - minX + 1;
        int height = maxY - minY + 1;
        
        hexTileMap = new HexTile[width, height];
        for (int x = minX; x <= maxX; x++) {
            for (int y = minY; y <= maxY; y++) {
                int mapXIndex = x - minX;
                int mapYIndex = y - minY;

                HexTile tile = GridMap.Instance.map[x, y];
                // if (tiles.Contains(tile)) {
                    hexTileMap[mapXIndex, mapYIndex] = tile;
                // } else {
                //     hexTileMap[mapXIndex, mapYIndex] = null;
                // }
            }
        }
    }
    public HexTile GetLeftMostTile() {
        int leftMostXCoordinate = GetLeftMostCoordinate();
        //loop through even rows first, if there are left most tiles that are
        //on an even row, then consider them as the left most tile.
        for (int x = 0; x <= hexTileMap.GetUpperBound(0); x++) {
            for (int y = 0; y <= hexTileMap.GetUpperBound(1); y++) {
                HexTile tile = hexTileMap[x, y];
                if (tile.region == this
                    && UtilityScripts.Utilities.IsEven(tile.yCoordinate)
                    && tile.xCoordinate == leftMostXCoordinate) {
                    return tile;
                }
            }    
        }
        //if no left most tile is in an even row, then just return the first tile that is on
        //the left most column
        for (int x = 0; x <= hexTileMap.GetUpperBound(0); x++) {
            for (int y = 0; y <= hexTileMap.GetUpperBound(1); y++) {
                HexTile tile = hexTileMap[x, y];
                if (tile.region == this && tile.xCoordinate == leftMostXCoordinate) {
                    return tile;
                }
            }    
        }

        return null; //NOTE: this should never happen
    }
    public HexTile GetRightMostTile() {
        int rightMostXCoordinate = GetRightMostCoordinate();
        //loop through odd rows first, if there are right most tiles that are
        //on an odd row, then consider them as the right most tile.
        for (int x = 0; x <= hexTileMap.GetUpperBound(0); x++) {
            for (int y = 0; y <= hexTileMap.GetUpperBound(1); y++) {
                HexTile tile = hexTileMap[x, y];
                if (tile.region == this 
                    && UtilityScripts.Utilities.IsEven(tile.yCoordinate) == false 
                    && tile.xCoordinate == rightMostXCoordinate) {
                    return tile;
                }
            }
        }
        //if no right most tile is in an odd row, then just return the first tile that is on
        //the right most column
        for (int x = 0; x <= hexTileMap.GetUpperBound(0); x++) {
            for (int y = 0; y <= hexTileMap.GetUpperBound(1); y++) {
                HexTile tile = hexTileMap[x, y];
                if (tile.region == this && tile.xCoordinate == rightMostXCoordinate) {
                    return tile;
                }
            }    
        }

        return null; //NOTE: this should never happen
    }
    private int GetLeftMostCoordinate() {
        return tiles.Min(t => t.data.xCoordinate);
    }
    private int GetRightMostCoordinate() {
        return tiles.Max(t => t.data.xCoordinate);
    }
    public List<int> GetLeftMostRows() {
        List<int> rows = new List<int>();
        HexTile leftMostTile = GetLeftMostTile();
        for (int x = 0; x <= hexTileMap.GetUpperBound(0); x++) {
            for (int y = 0; y <= hexTileMap.GetUpperBound(1); y++) {
                HexTile tile = hexTileMap[x, y];
                if (tile.region == this
                    && tile.xCoordinate == leftMostTile.xCoordinate
                    && UtilityScripts.Utilities.IsEven(leftMostTile.yCoordinate) == UtilityScripts.Utilities.IsEven(tile.yCoordinate) //only include tiles that are on the same row type as the left most tile (odd/even)
                    && rows.Contains(y) == false) {
                    rows.Add(y);
                }
            }
        }
        return rows;
    }
    public List<int> GetRightMostRows() {
        List<int> rows = new List<int>();
        HexTile rightMostTile = GetRightMostTile();
        for (int x = 0; x <= hexTileMap.GetUpperBound(0); x++) {
            for (int y = 0; y <= hexTileMap.GetUpperBound(1); y++) {
                HexTile tile = hexTileMap[x, y];
                if (tile.region == this
                    && tile.xCoordinate == rightMostTile.xCoordinate
                    && UtilityScripts.Utilities.IsEven(rightMostTile.yCoordinate) == UtilityScripts.Utilities.IsEven(tile.yCoordinate) //only include tiles that are on the same row type as the right most tile (odd/even)
                    && rows.Contains(y) == false) {
                    rows.Add(y);
                }
            }
        }
        return rows;
    }
    public bool AreLeftAndRightMostTilesInSameRowType() {
        List<int> leftMostRows = GetLeftMostRows();
        List<int> rightMostRows = GetRightMostRows();
        for (int i = 0; i < leftMostRows.Count; i++) {
            int currLeftRow = leftMostRows[i];
            if (rightMostRows.Contains(currLeftRow)) {
                //left most rows and right most rows have at least 1 row in common
                return true;
            } else {
                bool isLeftRowEven = UtilityScripts.Utilities.IsEven(currLeftRow);
                for (int j = 0; j < rightMostRows.Count; j++) {
                    int currRightRow = rightMostRows[j];
                    bool isRightRowEven = UtilityScripts.Utilities.IsEven(currRightRow);
                    if (isLeftRowEven == isRightRowEven) {
                        return true;
                    }
                }  
            }
        }
        return false;
    }
    public int GetDifferentRegionTilesInRow(int row) {
        int count = 0;
        for (int x = 0; x <= hexTileMap.GetUpperBound(0); x++) {
            for (int y = 0; y <= hexTileMap.GetUpperBound(1); y++) {
                HexTile tile = hexTileMap[x, y];
                if (y == row && tile.region != this) {
                    count++;
                }
            }
        }
        return count;
    }
    public HexTile GetRandomHexThatMeetCriteria(System.Func<HexTile, bool> validityChecker) {
        List<HexTile> hexes = ObjectPoolManager.Instance.CreateNewHexTilesList();
        HexTile chosenHex = null;
        for (int i = 0; i < tiles.Count; i++) {
            HexTile currHex = tiles[i];
            if (validityChecker.Invoke(currHex)) {
                hexes.Add(currHex);
            }
        }
        if (hexes != null && hexes.Count > 0) {
            chosenHex = hexes[UnityEngine.Random.Range(0, hexes.Count)];
        }
        ObjectPoolManager.Instance.ReturnHexTilesListToPool(hexes);
        return chosenHex;
    }
    #endregion

    #region Location Grid Tiles
    public LocationGridTile GetRandomOutsideSettlementLocationGridTileWithPathTo(Character character) {
        LocationGridTile chosenTile = null;
        //while(chosenTile == null) {
            for (int i = 0; i < shuffledNonMountainWaterTiles.Count; i++) {
                if (shuffledNonMountainWaterTiles[i].settlementOnTile == null) {
                    HexTile hex = shuffledNonMountainWaterTiles[i];
                    LocationGridTile potentialTile = hex.locationGridTiles[UnityEngine.Random.Range(0, hex.locationGridTiles.Length)];
                    if(character.movementComponent.HasPathToEvenIfDiffRegion(potentialTile)) {
                        chosenTile = potentialTile;
                        break;
                    }
                }
            }
        //}
        return chosenTile;
    }
    #endregion

    #region Settlements
    public void UpdateSettlementsInRegion() {
        settlementsInRegion.Clear();
        for (int i = 0; i < tiles.Count; i++) {
            HexTile tile = tiles[i];
            if (tile.settlementOnTile != null) {
                if (!settlementsInRegion.Contains(tile.settlementOnTile)) {
                    settlementsInRegion.Add(tile.settlementOnTile);    
                }
            }
        }
    }
    public List<BaseSettlement> GetSettlementsInRegion(System.Func<BaseSettlement, bool> validityChecker) {
        List<BaseSettlement> settlements = null;
        for (int i = 0; i < settlementsInRegion.Count; i++) {
            BaseSettlement settlement = settlementsInRegion[i];
            if (validityChecker.Invoke(settlement)) {
                if (settlements == null) {
                    settlements = new List<BaseSettlement>();
                }
                if (settlements.Contains(settlement) == false) {
                    settlements.Add(settlement);    
                }
            }
        }
        return settlements;
    }
    public BaseSettlement GetFirstSettlementInRegion(System.Func<BaseSettlement, bool> validityChecker) {
        for (int i = 0; i < settlementsInRegion.Count; i++) {
            BaseSettlement settlement = settlementsInRegion[i];
            if (validityChecker.Invoke(settlement)) {
                return settlement;
            }
        }
        return null;
    }
    public bool IsRegionVillageCapacityReached() {
        int count = 0;
        for (int i = 0; i < settlementsInRegion.Count; i++) {
            BaseSettlement settlement = settlementsInRegion[i];
            if (settlement.locationType == LOCATION_TYPE.VILLAGE) {
                count++;
            }
        }
        return count >= LandmarkManager.REGION_VILLAGE_CAPACITY;
    }
    #endregion

    #region Tile Objects
    public void AddTileObjectInRegion(TileObject tileObject) {
        if (!objectsInRegionCount.ContainsKey(tileObject.tileObjectType)) {
            objectsInRegionCount.Add(tileObject.tileObjectType, 0);
        }
        objectsInRegionCount[tileObject.tileObjectType] += 1;
        // if (tileObject.tileObjectType == TILE_OBJECT_TYPE.WOOD_PILE) {
        //     int count = objectsInRegionCount[tileObject.tileObjectType];
        //     Debug.Log($"Added {tileObject.nameWithID} to objects in region. Count is {count.ToString()}");    
        // }
    }
    public void RemoveTileObjectInRegion(TileObject tileObject) {
        if (objectsInRegionCount.ContainsKey(tileObject.tileObjectType)) {
            objectsInRegionCount[tileObject.tileObjectType] -= 1;
            // if (tileObject.tileObjectType == TILE_OBJECT_TYPE.WOOD_PILE) {
            //     Debug.Log($"Removed {tileObject.nameWithID} to objects in region. Count is {objectsInRegionCount[tileObject.tileObjectType].ToString()}");
            // }
            if (objectsInRegionCount[tileObject.tileObjectType] <= 0) {
                objectsInRegionCount.Remove(tileObject.tileObjectType);
            }
        }
    }
    public int GetTileObjectInRegionCount(TILE_OBJECT_TYPE tileObjectType) {
        if (objectsInRegionCount.ContainsKey(tileObjectType)) {
            return objectsInRegionCount[tileObjectType];
        }
        return 0;
    }
    #endregion

    public void CleanUp() {
        tiles?.Clear();
        tiles = null;
        shuffledNonMountainWaterTiles?.Clear();
        shuffledNonMountainWaterTiles = null;
        coreTile = null;
        factionsHere?.Clear();
        factionsHere = null;
        residents?.Clear();
        residents = null;
        charactersAtLocation?.Clear();
        charactersAtLocation = null;
        hexTileMap = null;
        structures?.Clear();
        structures = null;
        settlementsInRegion?.Clear();
        settlementsInRegion = null;
    }
}

public class Border {
    private SpriteRenderer borderSprite { get; }
    private SpriteGlowEffect glowEffect { get; }

    public Border(SpriteRenderer _borderSprite, SpriteGlowEffect _glowEffect) {
        borderSprite = _borderSprite;
        glowEffect = _glowEffect;
        SetGlowState(false);
    }
    
    public void SetBorderState(bool state) {
        borderSprite.gameObject.SetActive(state);
    }
    public void SetGlowState(bool state) {
        glowEffect.enabled = state;
    }

    public void SetColor(Color color) {
        borderSprite.color = color;
        glowEffect.GlowColor = color;
    }
}