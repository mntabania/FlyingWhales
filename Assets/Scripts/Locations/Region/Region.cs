using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using PathFind;
using SpriteGlow;
using UnityEngine;
using UtilityScripts;
using Random = UnityEngine.Random;

public class Region {
    public int id { get; }
    public string name { get; private set; }
    public string description => GetDescription();
    public List<HexTile> tiles { get; }
    public List<HexTile> shuffledNonMountainWaterTiles { get; }
    public HexTile coreTile { get; private set; }
    public LOCATION_TYPE locationType => LOCATION_TYPE.EMPTY;
    public Color regionColor { get; }
    public Minion assignedMinion { get; private set; }
    public List<Faction> factionsHere { get; }
    public List<Character> residents { get; }
    public DemonicLandmarkBuildingData demonicBuildingData { get; private set; }
    public DemonicLandmarkInvasionData demonicInvasionData { get; private set; }
    public GameObject eventIconGo { get; private set; }
    public List<Character> charactersAtLocation { get; }
    public RegionTileObject regionTileObject { get; private set; }
    public HexTile[,] hexTileMap { get; private set; }
    public LocationStructure mainStorage { get; private set; }
    public bool canShowNotifications { get; private set; }
    public Dictionary<POINT_OF_INTEREST_TYPE, List<IPointOfInterest>> awareness { get; }
    public List<IPointOfInterest> pendingAddAwareness { get; }
    public List<IPointOfInterest> pendingRemoveAwareness { get; }

    private RegionInnerTileMap _regionInnerTileMap; //inner map of the region, this should only be used if this region does not have an npcSettlement. 
    private string _activeEventAfterEffectScheduleId;
    private List<Border> _borders;
    public Dictionary<STRUCTURE_TYPE, List<LocationStructure>> structures { get; private set; }
    public List<LocationStructure> allStructures { get; private set; }

    #region getter/setter
    public BaseLandmark mainLandmark => coreTile.landmarkOnTile;
    public InnerTileMap innerMap => _regionInnerTileMap;
    #endregion

    private Region() {
        charactersAtLocation = new List<Character>();
        factionsHere = new List<Faction>();
        residents = new List<Character>();
        awareness = new Dictionary<POINT_OF_INTEREST_TYPE, List<IPointOfInterest>>();
        pendingAddAwareness = new List<IPointOfInterest>();
        pendingRemoveAwareness = new List<IPointOfInterest>();
    }
    public Region(HexTile coreTile) : this() {
        id = UtilityScripts.Utilities.SetID(this);
        name = RandomNameGenerator.GetRegionName();
        this.coreTile = coreTile;
        tiles = new List<HexTile>();
        shuffledNonMountainWaterTiles = new List<HexTile>();
        AddTile(coreTile);
        regionColor = GenerateRandomRegionColor();
        Debug.Log($"Created region {this.name} with core tile {coreTile.ToString()}");
    }
    public Region(SaveDataRegion data) : this() {
        id = UtilityScripts.Utilities.SetID(this, data.id);
        name = data.name;
        coreTile = GridMap.Instance.normalHexTiles[data.coreTileID];
        tiles = new List<HexTile>();
        shuffledNonMountainWaterTiles = new List<HexTile>();
        regionColor = data.regionColor;
    }

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
    public void RedetermineCore() {
        int maxX = tiles.Max(t => t.data.xCoordinate);
        int minX = tiles.Min(t => t.data.xCoordinate);
        int maxY = tiles.Max(t => t.data.yCoordinate);
        int minY = tiles.Min(t => t.data.yCoordinate);

        int x = (minX + maxX) / 2;
        int y = (minY + maxY) / 2;

        HexTile newCoreTile = GridMap.Instance.map[x, y];
        if (newCoreTile.IsAtEdgeOfMap() == false) {
            coreTile = newCoreTile;
        }
        
        //clear all tiles again after redetermining core
        List<HexTile> allTiles = new List<HexTile>(tiles);
        for (int i = 0; i < allTiles.Count; i++) {
            HexTile currTile = allTiles[i];
            if (currTile != coreTile) {
                RemoveTile(currTile);
            }
        }
        
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
    public List<Region> AdjacentRegions() {
        List<Region> adjacent = null;
        for (int i = 0; i < tiles.Count; i++) {
            HexTile currTile = tiles[i];
            List<Region> regions;
            if (currTile.TryGetDifferentRegionNeighbours(out regions)) {
                for (int j = 0; j < regions.Count; j++) {
                    Region currRegion = regions[j];
                    if(adjacent == null) { adjacent = new List<Region>(); }
                    if (!adjacent.Contains(currRegion)) {
                        adjacent.Add(currRegion);
                    }
                }
            }
        }
        return adjacent;
    }
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
    public bool HasActiveSettlement() {
        for (int i = 0; i < tiles.Count; i++) {
            HexTile tile = tiles[i];
            if (tile.settlementOnTile != null && tile.settlementOnTile is NPCSettlement npcSettlement && 
                npcSettlement.owner != null && (npcSettlement.owner.race == RACE.ELVES || npcSettlement.owner.race == RACE.HUMANS)) {
                return true;
            }
        }
        return false;
    }
    public List<BaseSettlement> GetSettlementsInRegion(System.Func<BaseSettlement, bool> validityChecker) {
        List<BaseSettlement> settlements = null;
        for (int i = 0; i < tiles.Count; i++) {
            HexTile tile = tiles[i];
            if (tile.settlementOnTile != null && validityChecker.Invoke(tile.settlementOnTile)) {
                if (settlements == null) {
                    settlements = new List<BaseSettlement>();
                }
                if (settlements.Contains(tile.settlementOnTile) == false) {
                    settlements.Add(tile.settlementOnTile);    
                }
            }
        }
        return settlements;
    }
    public List<HexTile> GetAreasOccupiedByVillagers() {
        List<HexTile> areas = null;
        for (int i = 0; i < residents.Count; i++) {
            Character regionResident = residents[i];
            if (regionResident.territorries != null && regionResident.territorries.Count > 0) {
                for (int j = 0; j < regionResident.territorries.Count; j++) {
                    HexTile territory = regionResident.territorries[j];
                    if (areas == null) {
                        areas = new List<HexTile>();
                    }
                    if (areas.Contains(territory) == false) {
                        areas.Add(territory);
                    }
                }
            }
        }
        return areas;
    }
    #endregion

    #region Invasion
    public bool CanBeInvaded() {
        // if (npcSettlement != null) {
        //     return npcSettlement.CanInvadeSettlement();
        // }
        //return HasCorruptedConnection() &&!coreTile.isCorrupted && !demonicInvasionData.beingInvaded;
        return  !coreTile.isCorrupted; //HasCorruptedConnection() && TODO:
    }
    public void StartInvasion(Minion assignedMinion) {
        //PlayerManager.Instance.player.SetInvadingRegion(this);
        assignedMinion.SetAssignedRegion(this);
        SetAssignedMinion(assignedMinion);

        demonicInvasionData = new DemonicLandmarkInvasionData() {
            beingInvaded = true,
            currentDuration = 0,
        };

        //ticksInInvasion = 0;
        Messenger.AddListener(Signals.TICK_STARTED, PerInvasionTick);
        // TimerHubUI.Instance.AddItem("Invasion of " + (mainLandmark.tileLocation.settlementOfTile != null ? mainLandmark.tileLocation.settlementOfTile.name : name), mainLandmark.invasionTicks, () => UIManager.Instance.ShowRegionInfo(this));
    }
    public void LoadInvasion(SaveDataRegion data) {
        //PlayerManager.Instance.player.SetInvadingRegion(this);
        //assignedMinion.SetAssignedRegion(this);
        //SetAssignedMinion(assignedMinion);

        demonicInvasionData = data.demonicInvasionData;
        if (demonicInvasionData.beingInvaded) {
            Messenger.AddListener(Signals.TICK_STARTED, PerInvasionTick);
            // TimerHubUI.Instance.AddItem("Invasion of " + (mainLandmark.tileLocation.settlementOfTile != null ? mainLandmark.tileLocation.settlementOfTile.name : name), mainLandmark.invasionTicks - demonicInvasionData.currentDuration, () => UIManager.Instance.ShowRegionInfo(this));
        }
    }
    private void PerInvasionTick() {
        DemonicLandmarkInvasionData tempData = demonicInvasionData;
        tempData.currentDuration++;
        demonicInvasionData = tempData;
        if (demonicInvasionData.currentDuration > mainLandmark.invasionTicks) {
            //invaded.
            Invade();
            // UIManager.Instance.ShowImportantNotification(GameManager.Instance.Today(),
            //     $"You have successfully invaded {this.name}", () => UIManager.Instance.ShowRegionInfo(this));
            Messenger.RemoveListener(Signals.TICK_STARTED, PerInvasionTick);
        }
    }
    private void Invade() {
        //corrupt region
        InvadeActions();
        //TODO:
        // LandmarkManager.Instance.OwnRegion(PlayerManager.Instance.player.playerFaction, this);
        //PlayerManager.Instance.AddTileToPlayerArea(coreTile);
        //PlayerManager.Instance.player.SetInvadingRegion(null);
        demonicInvasionData = new DemonicLandmarkInvasionData();
        assignedMinion.SetAssignedRegion(null);
        SetAssignedMinion(null);

        //This is done so that when a region is invaded by the player, the showing Info UI will update appropriately
        if (UIManager.Instance.regionInfoUI.isShowing && UIManager.Instance.regionInfoUI.activeRegion == this) {
            UIManager.Instance.ShowRegionInfo(this);
        }
    }
    public void SetAssignedMinion(Minion minion) {
        Minion previouslyAssignedMinion = assignedMinion;
        assignedMinion = minion;
        if (assignedMinion != null) {
            AddCharacterToLocation(assignedMinion.character);
            mainLandmark.OnMinionAssigned(assignedMinion); //a new minion was assigned 
        } else if (previouslyAssignedMinion != null) {
            RemoveCharacterFromLocation(previouslyAssignedMinion.character);
            mainLandmark.OnMinionUnassigned(previouslyAssignedMinion); //a minion was unassigned
        }
    }
    #endregion

    #region Player Build Structure
    public void StartBuildingStructure(LANDMARK_TYPE landmarkType, Minion minion) {
        SetAssignedMinion(minion);
        minion.SetAssignedRegion(this);
        LandmarkData landmarkData = LandmarkManager.Instance.GetLandmarkData(landmarkType);
        demonicBuildingData = new DemonicLandmarkBuildingData() {
            landmarkType = landmarkType,
            landmarkName = landmarkData.landmarkTypeString,
            buildDuration = landmarkData.buildDuration + Mathf.RoundToInt(landmarkData.buildDuration * PlayerManager.Instance.player.constructionRatePercentageModifier),
            currentDuration = 0,
        };
        coreTile.UpdateBuildSprites();
        TimerHubUI.Instance.AddItem($"Building {demonicBuildingData.landmarkName} at {name}", demonicBuildingData.buildDuration, () => UIManager.Instance.ShowRegionInfo(this));
        Messenger.AddListener(Signals.TICK_STARTED, PerTickBuilding);
    }
    public void LoadBuildingStructure(SaveDataRegion data) {
        demonicBuildingData = data.demonicBuildingData;
        if (demonicBuildingData.landmarkType != LANDMARK_TYPE.NONE) {
            TimerHubUI.Instance.AddItem($"Building {demonicBuildingData.landmarkName} at {name}", demonicBuildingData.buildDuration - demonicBuildingData.currentDuration, () => UIManager.Instance.ShowRegionInfo(this));
            Messenger.AddListener(Signals.TICK_STARTED, PerTickBuilding);
        }
    }
    private void PerTickBuilding() {
        DemonicLandmarkBuildingData tempData = demonicBuildingData;
        tempData.currentDuration++;
        demonicBuildingData = tempData;
        if (demonicBuildingData.currentDuration >= demonicBuildingData.buildDuration) {
            FinishBuildingStructure();
        }
    }
    private void FinishBuildingStructure() {
        //NOTE: We do not call SetAssignedMinion to null and SetAssignedRegion to null here because it is already called in CreateNewLandmarkOnTile inside DestroyLandmarkOnTile
        Messenger.RemoveListener(Signals.TICK_STARTED, PerTickBuilding);
        //mainLandmark.ChangeLandmarkType(demonicBuildingData.landmarkType);
        //int previousID = mainLandmark.id;
        BaseLandmark newLandmark = LandmarkManager.Instance.CreateNewLandmarkOnTile(coreTile, demonicBuildingData.landmarkType);
        //newLandmark.OverrideID(previousID);

        // UIManager.Instance.ShowImportantNotification(GameManager.Instance.Today(),
        //     $"Finished building {UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(newLandmark.specificLandmarkType.ToString())} at {this.name}", () => UIManager.Instance.ShowRegionInfo(this));
        demonicBuildingData = new DemonicLandmarkBuildingData();
        //assignedMinion.SetAssignedRegion(null);
        //SetAssignedMinion(null);

        newLandmark.OnFinishedBuilding();
        coreTile.UpdateBuildSprites();
        Messenger.Broadcast(Signals.REGION_INFO_UI_UPDATE_APPROPRIATE_CONTENT, this);
    }
    private void StopBuildingStructure() {
        if (demonicBuildingData.landmarkType != LANDMARK_TYPE.NONE) {
            Messenger.RemoveListener(Signals.TICK_STARTED, PerTickBuilding);
            TimerHubUI.Instance.RemoveItem($"Building {demonicBuildingData.landmarkName} at {name}");
            Messenger.Broadcast(Signals.REGION_INFO_UI_UPDATE_APPROPRIATE_CONTENT, this);
            demonicBuildingData = new DemonicLandmarkBuildingData();
            coreTile.UpdateBuildSprites();
        }
    }
    #endregion

    #region Corruption/Invasion
    public void InvadeActions() {
        mainLandmark?.ChangeLandmarkType(LANDMARK_TYPE.NONE);
        // ActivateRegionFeatures();
        // RemoveFeaturesAfterInvade();
        // ExecuteEventAfterInvasion();
        // ExecuteOtherAfterInvasionActions();
    }
    #endregion

    #region Events
    public void SetEventIcon(GameObject go) {
        eventIconGo = go;
    }
    public void OnCleansedRegion() {
        StopBuildingStructure();
    }
    #endregion

    #region Characters
    public void LoadCharacterHere(Character character) {
        charactersAtLocation.Add(character);
        character.SetRegionLocation(this);
        Messenger.Broadcast(Signals.CHARACTER_ENTERED_REGION, character, this);
    }
    public void AddCharacterToLocation(Character character, LocationGridTile tileOverride = null, bool isInitial = false) {
        if (!charactersAtLocation.Contains(character)) {
            charactersAtLocation.Add(character);
            character.SetRegionLocation(this);
            Messenger.Broadcast(Signals.CHARACTER_ENTERED_REGION, character, this);
        }
    }
    public void RemoveCharacterFromLocation(Character character) {
        if (charactersAtLocation.Remove(character)) {
            character.currentStructure?.RemoveCharacterAtLocation(character);
            // for (int i = 0; i < features.Count; i++) {
            //     features[i].OnRemoveCharacterFromRegion(this, character);
            // }
            character.SetRegionLocation(null);
            Messenger.Broadcast(Signals.CHARACTER_EXITED_REGION, character, this);
        }
    }
    public void RemoveCharacterFromLocation(Party party) {
        RemoveCharacterFromLocation(party.owner);
    }
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
    #endregion

    #region Faction
    public void AddFactionHere(Faction faction) {
        if (!IsFactionHere(faction) && faction.isMajorFaction) {
            factionsHere.Add(faction);
            //Once a faction is added and there is no ruling faction yet, automatically let the added faction own the region
            //TODO:
            // if(owner == null) {
            //     LandmarkManager.Instance.OwnRegion(faction, this);
            // }
        }
    }
    public void RemoveFactionHere(Faction faction) {
        if (factionsHere.Remove(faction)) {
            //If a faction is removed and it is the ruling faction, transfer ruling faction to the next faction on the list if there's any, if not make the region part of neutral faction
            //TODO:
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

    #region Awareness
    public void AddPendingAwareness(IPointOfInterest poi) {
        pendingAddAwareness.Add(poi);
    }
    public void RemovePendingAwareness(IPointOfInterest poi) {
        pendingRemoveAwareness.Add(poi);
    }
    public void UpdateAwareness() {
        for (int i = 0; i < pendingAddAwareness.Count; i++) {
            AddAwareness(pendingAddAwareness[i]);
        }
        for (int i = 0; i < pendingRemoveAwareness.Count; i++) {
            RemoveAwareness(pendingRemoveAwareness[i]);
        }
        pendingAddAwareness.Clear();
        pendingRemoveAwareness.Clear();
    }
    private bool AddAwareness(IPointOfInterest pointOfInterest) {
        if (!HasAwareness(pointOfInterest)) {
            if (!awareness.ContainsKey(pointOfInterest.poiType)) {
                awareness.Add(pointOfInterest.poiType, new List<IPointOfInterest>());
            }
            awareness[pointOfInterest.poiType].Add(pointOfInterest);
            //if (pointOfInterest is TreeObject) {
            //    List<IPointOfInterest> treeAwareness = GetTileObjectAwarenessOfType(TILE_OBJECT_TYPE.TREE_OBJECT);
            //    if (treeAwareness.Count >= Character.TREE_AWARENESS_LIMIT) {
            //        RemoveAwareness(treeAwareness[0]);
            //    }
            //}
            return true;
        }
        return false;
    }
    public void RemoveAwareness(IPointOfInterest pointOfInterest) {
        if (awareness.ContainsKey(pointOfInterest.poiType)) {
            List<IPointOfInterest> awarenesses = awareness[pointOfInterest.poiType];
            for (int i = 0; i < awarenesses.Count; i++) {
                IPointOfInterest iawareness = awarenesses[i];
                if (iawareness == pointOfInterest) {
                    awarenesses.RemoveAt(i);
                    break;
                }
            }
        }
    }
    public void RemoveAwareness(POINT_OF_INTEREST_TYPE poiType) {
        if (awareness.ContainsKey(poiType)) {
            awareness.Remove(poiType);
        }
    }
    public bool HasAwareness(IPointOfInterest poi) {
        if (awareness.ContainsKey(poi.poiType)) {
            List<IPointOfInterest> awarenesses = awareness[poi.poiType];
            for (int i = 0; i < awarenesses.Count; i++) {
                IPointOfInterest currPOI = awarenesses[i];
                if (currPOI == poi) {
                    return true;
                }
            }
            return false;
        }
        return false;
    }
    #endregion
    
    #region Structures
    public void GenerateStructures() {
        structures = new Dictionary<STRUCTURE_TYPE, List<LocationStructure>>();
        allStructures = new List<LocationStructure>();
        LandmarkManager.Instance.CreateNewStructureAt(this, STRUCTURE_TYPE.WILDERNESS);
    }
    public void AddStructure(LocationStructure structure) {
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
    public LocationStructure GetRandomUnoccupiedStructureWithTag(params STRUCTURE_TAG[] tag) {
        List<LocationStructure> structuresWithTag = null;
        for (int i = 0; i < allStructures.Count; i++) {
            LocationStructure currStructure = allStructures[i];
            if (!currStructure.IsOccupied()) {
                if (currStructure.HasStructureTag(tag)) {
                    if(structuresWithTag == null) { structuresWithTag = new List<LocationStructure>(); }
                    structuresWithTag.Add(currStructure);
                }
            }
        }
        if(structuresWithTag != null && structuresWithTag.Count > 0) {
            return structuresWithTag[UnityEngine.Random.Range(0, structuresWithTag.Count)];
        }
        return null;
    }
    public LocationStructure GetRandomUnoccupiedStructureWithTags() {
        List<LocationStructure> structuresWithTag = null;
        for (int i = 0; i < allStructures.Count; i++) {
            LocationStructure currStructure = allStructures[i];
            if (!currStructure.IsOccupied()) {
                if (currStructure.HasStructureTags()) {
                    if (structuresWithTag == null) { structuresWithTag = new List<LocationStructure>(); }
                    structuresWithTag.Add(currStructure);
                }
            }
        }
        if (structuresWithTag != null && structuresWithTag.Count > 0) {
            return structuresWithTag[UnityEngine.Random.Range(0, structuresWithTag.Count)];
        }
        return null;
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
                if (currStructure.occupiedHexTile != null && currStructure.occupiedHexTile.hexTileOwner.settlementOnTile == character.homeSettlement) {
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
    public void OnLocationStructureObjectPlaced(LocationStructure structure) {
        if (structure.structureType == STRUCTURE_TYPE.WAREHOUSE) {
            //if a warehouse was placed, and this npcSettlement does not yet have a main storage structure, or is using the city center as their main storage structure, then use the new warehouse instead.
            if (mainStorage == null || mainStorage.structureType == STRUCTURE_TYPE.CITY_CENTER) {
                SetMainStorage(structure);
            }
        } else if (structure.structureType == STRUCTURE_TYPE.CITY_CENTER) {
            if (mainStorage == null) {
                SetMainStorage(structure);
            }
        }
    }
    private void SetMainStorage(LocationStructure structure) {
        bool shouldCheckResourcePiles = mainStorage != null && structure != null && mainStorage != structure;
        mainStorage = structure;
        if (shouldCheckResourcePiles) {
            Messenger.Broadcast(Signals.REGION_CHANGE_STORAGE, this);
        }
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
        canShowNotifications = true;
    }
    public void BlockNotifications() {
        canShowNotifications = false;
    }
    public bool IsSameCoreLocationAs(Region location) {
        return location.coreTile == this.coreTile;
    }
    public void SetRegionTileObject(RegionTileObject _regionTileObject) {
        regionTileObject = _regionTileObject;
    }
    public List<TileObject> GetTileObjectsOfType(TILE_OBJECT_TYPE type) {
        List<TileObject> objs = new List<TileObject>();
        foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> keyValuePair in structures) {
            for (int i = 0; i < keyValuePair.Value.Count; i++) {
                objs.AddRange(keyValuePair.Value[i].GetTileObjectsOfType(type));
            }
        }
        return objs;
    }
    public int GetTileObjectsOfTypeCount(TILE_OBJECT_TYPE type) {
        int count = 0;
        foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> keyValuePair in structures) {
            for (int i = 0; i < keyValuePair.Value.Count; i++) {
                count += keyValuePair.Value[i].GetTileObjectsOfTypeCount(type);
            }
        }
        return count;
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
    public HexTile GetRandomNoStructureUncorruptedNotPartOrNextToVillagePlainHex() {
        List<HexTile> hexes = null;
        for (int i = 0; i < tiles.Count; i++) {
            HexTile currHex = tiles[i];
            if(currHex.elevationType != ELEVATION.WATER && currHex.elevationType != ELEVATION.MOUNTAIN && currHex.landmarkOnTile == null && !currHex.IsNextToOrPartOfVillage() && !currHex.isCorrupted) {
                if(hexes == null) { hexes = new List<HexTile>(); }
                hexes.Add(currHex);
            }
        }
        if(hexes != null && hexes.Count > 0) {
            return hexes[UnityEngine.Random.Range(0, hexes.Count)];
        }
        return null;
    }
    public HexTile GetRandomNoStructureUncorruptedPlainHex() {
        List<HexTile> hexes = null;
        for (int i = 0; i < tiles.Count; i++) {
            HexTile currHex = tiles[i];
            if (currHex.elevationType != ELEVATION.WATER && currHex.elevationType != ELEVATION.MOUNTAIN && currHex.landmarkOnTile == null && !currHex.isCorrupted) {
                if (hexes == null) { hexes = new List<HexTile>(); }
                hexes.Add(currHex);
            }
        }
        if (hexes != null && hexes.Count > 0) {
            return hexes[UnityEngine.Random.Range(0, hexes.Count)];
        }
        return null;
    }
    public HexTile GetRandomPlainHex() {
        List<HexTile> hexes = null;
        for (int i = 0; i < tiles.Count; i++) {
            HexTile currHex = tiles[i];
            if (currHex.elevationType != ELEVATION.WATER && currHex.elevationType != ELEVATION.MOUNTAIN) {
                if (hexes == null) { hexes = new List<HexTile>(); }
                hexes.Add(currHex);
            }
        }
        if (hexes != null && hexes.Count > 0) {
            return hexes[UnityEngine.Random.Range(0, hexes.Count)];
        }
        return null;
    }
    public HexTile GetRandomUncorruptedPlainHex() {
        List<HexTile> hexes = null;
        for (int i = 0; i < tiles.Count; i++) {
            HexTile currHex = tiles[i];
            if (currHex.elevationType != ELEVATION.WATER && currHex.elevationType != ELEVATION.MOUNTAIN && !currHex.isCorrupted) {
                if (hexes == null) { hexes = new List<HexTile>(); }
                hexes.Add(currHex);
            }
        }
        if (hexes != null && hexes.Count > 0) {
            return hexes[UnityEngine.Random.Range(0, hexes.Count)];
        }
        return null;
    }
    #endregion

    #region Location Grid Tiles
    public LocationGridTile GetRandomOutsideSettlementLocationGridTileWithPathTo(Character character) {
        LocationGridTile chosenTile = null;
        //while(chosenTile == null) {
            for (int i = 0; i < shuffledNonMountainWaterTiles.Count; i++) {
                if (shuffledNonMountainWaterTiles[i].settlementOnTile == null) {
                    HexTile hex = shuffledNonMountainWaterTiles[i];
                    LocationGridTile potentialTile = hex.locationGridTiles[UnityEngine.Random.Range(0, hex.locationGridTiles.Count)];
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