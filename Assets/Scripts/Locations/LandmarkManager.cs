using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using JetBrains.Annotations;
using Locations.Region_Features;
using Locations.Settlements;
using Locations.Settlements.Settlement_Types;
using Locations.Tile_Features;
using UnityEngine.Assertions;
using UtilityScripts;
using Random = UnityEngine.Random;

public partial class LandmarkManager : BaseMonoBehaviour {

    public static LandmarkManager Instance = null;
    public const int SUMMON_MINION_DURATION = 96;
    public const int REGION_VILLAGE_CAPACITY = 3;
    
    [SerializeField] private List<LandmarkData> landmarkData;
    public List<BaseLandmark> allLandmarks;
    [SerializeField] private GameObject landmarkGO;
    private Dictionary<LANDMARK_TYPE, LandmarkData> landmarkDataDict;
    
    public STRUCTURE_TYPE[] humanSurvivalStructures { get; private set; }
    public STRUCTURE_TYPE[] humanUtilityStructures { get; private set; }
    public STRUCTURE_TYPE[] humanCombatStructures { get; private set; }
    public STRUCTURE_TYPE[] elfSurvivalStructures { get; private set; }
    public STRUCTURE_TYPE[] elfUtilityStructures { get; private set; }
    public STRUCTURE_TYPE[] elfCombatStructures { get; private set; }

    #region getters
    public List<BaseSettlement> allSettlements => DatabaseManager.Instance.settlementDatabase.allSettlements;
    public List<NPCSettlement> allNonPlayerSettlements => DatabaseManager.Instance.settlementDatabase.allNonPlayerSettlements;
    #endregion
    
    public void Initialize() {
        allLandmarks = new List<BaseLandmark>();
        ConstructLandmarkData();
        LoadLandmarkTypeDictionary();
        ConstructRaceStructureRequirements();
    }

    #region Monobehaviours
    private void Awake() {
        Instance = this;
    }
    protected override void OnDestroy() {
        base.OnDestroy();
        Instance = null;
    }
    #endregion

    #region Landmarks
    private void ConstructLandmarkData() {
        for (int i = 0; i < landmarkData.Count; i++) {
            LandmarkData data = landmarkData[i];
            data.ConstructData();
        }
    }
    private void LoadLandmarkTypeDictionary() {
        landmarkDataDict = new Dictionary<LANDMARK_TYPE, LandmarkData>();
        for (int i = 0; i < landmarkData.Count; i++) {
            LandmarkData data = landmarkData[i];
            landmarkDataDict.Add(data.landmarkType, data);
        }
    }
    public BaseLandmark CreateNewLandmarkOnTile(HexTile location, LANDMARK_TYPE landmarkType) {
        if (location.landmarkOnTile != null) {
            //Destroy landmark on tile
            DestroyLandmarkOnTile(location);
        }
        BaseLandmark newLandmark = location.CreateLandmarkOfType(landmarkType);
        newLandmark.tileLocation.AdjustUncorruptibleLandmarkNeighbors(1);
        // location.UpdateBuildSprites();
        allLandmarks.Add(newLandmark);
        Messenger.Broadcast(LandmarkSignals.LANDMARK_CREATED, newLandmark);
        return newLandmark;
    }
    public void DestroyLandmarkOnTile(HexTile tile) {
        BaseLandmark landmarkOnTile = tile.landmarkOnTile;
        if (landmarkOnTile == null) {
            return;
        }
        landmarkOnTile.DestroyLandmark();
        // tile.UpdateBuildSprites();
        tile.RemoveLandmarkVisuals();
        tile.RemoveLandmarkOnTile();
        allLandmarks.Remove(landmarkOnTile);
        Messenger.Broadcast(LandmarkSignals.LANDMARK_DESTROYED, landmarkOnTile, tile);
    }
    public BaseLandmark LoadLandmarkOnTile(HexTile location, BaseLandmark landmark) {
        BaseLandmark newLandmark = location.LoadLandmark(landmark);
        return newLandmark;
    }
    public GameObject GetLandmarkGO() {
        return this.landmarkGO;
    }
    public BaseLandmark CreateNewLandmarkInstance(HexTile location, LANDMARK_TYPE type) {
        return new BaseLandmark(location, type);
    }
    public BaseLandmark CreateNewLandmarkInstance(HexTile location, SaveDataLandmark data) {
        if (data.landmarkType.IsPlayerLandmark()) {
            var typeName = $"{UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(data.landmarkType.ToString())}, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
            System.Type systemType = System.Type.GetType(typeName);
            if (systemType != null) {
                return System.Activator.CreateInstance(systemType, location, data) as BaseLandmark;
            }
            return null;
        } else {
            return new BaseLandmark(location, data);
        }
    }
    #endregion

    #region Landmark Generation
    public LandmarkData GetLandmarkData(LANDMARK_TYPE landmarkType) {
        if (landmarkDataDict.ContainsKey(landmarkType)) {
            return landmarkDataDict[landmarkType];
        }
        throw new System.Exception($"There is no landmark data for {landmarkType}");
    }
    public LandmarkData GetLandmarkData(string landmarkName) {
        for (int i = 0; i < landmarkData.Count; i++) {
            LandmarkData currData = landmarkData[i];
            if (currData.landmarkTypeString == landmarkName) {
                return currData;
            }
        }
        throw new System.Exception($"There is no landmark data for {landmarkName}");
    }
    public Island GetIslandOfRegion(Region region, List<Island> islands) {
        for (int i = 0; i < islands.Count; i++) {
            Island currIsland = islands[i];
            if (currIsland.regionsInIsland.Contains(region)) {
                return currIsland;
            }
        }
        return null;
    }
    private void MergeIslands(Island island1, Island island2, ref List<Island> islands) {
        island1.regionsInIsland.AddRange(island2.regionsInIsland);
        islands.Remove(island2);
    }
    #endregion

    #region Utilities
    public BaseLandmark GetLandmarkByID(int id) {
        List<BaseLandmark> landmarks = GetAllLandmarks();
        for (int i = 0; i < landmarks.Count; i++) {
            BaseLandmark currLandmark = landmarks[i];
            if (currLandmark.id == id) {
                return currLandmark;
            }
        }
        return null;
    }
    public BaseLandmark GetLandmarkByName(string name) {
        List<BaseLandmark> landmarks = GetAllLandmarks();
        for (int i = 0; i < landmarks.Count; i++) {
            BaseLandmark currLandmark = landmarks[i];
            if (currLandmark.landmarkName.Equals(name, System.StringComparison.CurrentCultureIgnoreCase)) {
                return currLandmark;
            }
        }
        //for (int i = 0; i < GridMap.Instance.allRegions.Count; i++) {
        //    Region currRegion = GridMap.Instance.allRegions[i];
        //    if (currRegion.mainLandmark.landmarkName.Equals(name, System.StringComparison.CurrentCultureIgnoreCase)) {
        //        return currRegion.mainLandmark;
        //    }
        //    for (int j = 0; j < currRegion.landmarks.Count; j++) {
        //        BaseLandmark currLandmark = currRegion.landmarks[j];
        //        if (currLandmark.landmarkName.Equals(name, System.StringComparison.CurrentCultureIgnoreCase)) {
        //            return currLandmark;
        //        }
        //    }
        //}
        return null;
    }
    public BaseLandmark GetLandmarkOfType(LANDMARK_TYPE landmarkType) {
        List<BaseLandmark> _allLandmarks = GetAllLandmarks();
        for (int i = 0; i < _allLandmarks.Count; i++) {
            BaseLandmark currLandmark = _allLandmarks[i];
            if (currLandmark.specificLandmarkType == landmarkType) {
                return currLandmark;
            }
        }
        return null;
    }
    public List<BaseLandmark> GetLandmarksOfType(LANDMARK_TYPE landmarkType) {
        List<BaseLandmark> landmarks = new List<BaseLandmark>();
        List<BaseLandmark> _allLandmarks = GetAllLandmarks();
        for (int i = 0; i < _allLandmarks.Count; i++) {
            BaseLandmark currLandmark = _allLandmarks[i];
            if (currLandmark.specificLandmarkType == landmarkType) {
                landmarks.Add(currLandmark);
            }
        }
        return landmarks;
    }
    public List<BaseLandmark> GetAllLandmarks() {
        List<BaseLandmark> landmarks = new List<BaseLandmark>();
        List<HexTile> choices = GridMap.Instance.normalHexTiles;
        for (int i = 0; i < choices.Count; i++) {
            HexTile currTile = choices[i];
            if (currTile.landmarkOnTile != null) {
                landmarks.Add(currTile.landmarkOnTile);
            }
        }
        return landmarks;
    }
    public List<LandmarkStructureSprite> GetLandmarkTileSprites(HexTile tile, LANDMARK_TYPE landmarkType, RACE race = RACE.NONE) {
        LandmarkData data = GetLandmarkData(landmarkType);
        if (data.biomeTileSprites.Count > 0) { //if the landmark type has a biome type tile sprite set, use that instead
            if (data.biomeTileSprites.ContainsKey(tile.biomeType)) {
                return data.biomeTileSprites[tile.biomeType]; //prioritize biome type sprites
            }
        }
        if (race == RACE.HUMANS) {
            return data.humansLandmarkTileSprites;
        } else if (race == RACE.ELVES) {
            return data.elvenLandmarkTileSprites;
        } else {
            if (data.neutralTileSprites.Count > 0) {
                return data.neutralTileSprites;
            } else {
                return null;
            }
        }
        
    }
    public List<Character> GetAllDeadCharactersInLocation(Region location) {
        List<Character> characters = new List<Character>();
        for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
            Character character = CharacterManager.Instance.allCharacters[i];
            if(character.isDead && character.currentRegion == location && !(character is Summon)) {
                if(character.marker || character.grave != null) { //Only resurrect characters who are in the tombstone or still has a marker in the npcSettlement
                    characters.Add(character);
                }
            }
        }
        return characters;
    }
    #endregion

    #region Settlements
    public NPCSettlement CreateNewSettlement(Region region, LOCATION_TYPE locationType, params HexTile[] tiles) {
        NPCSettlement newNpcSettlement = new NPCSettlement(region, locationType);
        if (tiles != null) {
            newNpcSettlement.AddTileToSettlement(tiles);    
        }
        Messenger.Broadcast(SettlementSignals.SETTLEMENT_CREATED, newNpcSettlement);
        DatabaseManager.Instance.settlementDatabase.RegisterSettlement(newNpcSettlement);
        newNpcSettlement.Initialize();
        return newNpcSettlement;
    }
    public NPCSettlement LoadNPCSettlement(SaveDataNPCSettlement saveDataNpcSettlement) {
        List<HexTile> tiles = GameUtilities.GetHexTilesGivenCoordinates(saveDataNpcSettlement.tileCoordinates, GridMap.Instance.map);
        NPCSettlement newNpcSettlement = new NPCSettlement(saveDataNpcSettlement);
        for (int i = 0; i < tiles.Count; i++) {
            HexTile tile = tiles[i];
            newNpcSettlement.AddTileToSettlement(tile);
        }
        Messenger.Broadcast(SettlementSignals.SETTLEMENT_CREATED, newNpcSettlement);
        DatabaseManager.Instance.settlementDatabase.RegisterSettlement(newNpcSettlement);
        return newNpcSettlement;
    }
    public PlayerSettlement CreateNewPlayerSettlement(params HexTile[] tiles) {
        PlayerSettlement newPlayerSettlement = new PlayerSettlement();
        newPlayerSettlement.AddTileToSettlement(tiles);
        Messenger.Broadcast(SettlementSignals.SETTLEMENT_CREATED, newPlayerSettlement);
        DatabaseManager.Instance.settlementDatabase.RegisterSettlement(newPlayerSettlement);
        return newPlayerSettlement;
    }
    public PlayerSettlement LoadPlayerSettlement(SaveDataPlayerSettlement saveDataPlayerSettlement) {
        PlayerSettlement newPlayerSettlement = new PlayerSettlement(saveDataPlayerSettlement);

        List<HexTile> tiles = GameUtilities.GetHexTilesGivenCoordinates(saveDataPlayerSettlement.tileCoordinates, GridMap.Instance.map);
        for (int i = 0; i < tiles.Count; i++) {
            HexTile tile = tiles[i];
            newPlayerSettlement.AddTileToSettlement(tile);
        }

        Messenger.Broadcast(SettlementSignals.SETTLEMENT_CREATED, newPlayerSettlement);
        DatabaseManager.Instance.settlementDatabase.RegisterSettlement(newPlayerSettlement);
        return newPlayerSettlement;
    }
    public NPCSettlement GetRandomVillageSettlement() {
        List<NPCSettlement> villages = null;
        for (int i = 0; i < allNonPlayerSettlements.Count; i++) {
            NPCSettlement settlement = allNonPlayerSettlements[i];
            if(settlement.locationType == LOCATION_TYPE.VILLAGE) {
                if(villages == null) { villages = new List<NPCSettlement>(); }
                villages.Add(settlement);
            }
        }
        if(villages != null && villages.Count > 0) {
            return villages[UnityEngine.Random.Range(0, villages.Count)];
        }
        return null;
    }
    public NPCSettlement GetRandomActiveVillageSettlement() {
        List<NPCSettlement> villages = null;
        for (int i = 0; i < allNonPlayerSettlements.Count; i++) {
            NPCSettlement settlement = allNonPlayerSettlements[i];
            if(settlement.locationType == LOCATION_TYPE.VILLAGE && settlement.owner != null && settlement.residents.Count > 0) {
                if(villages == null) { villages = new List<NPCSettlement>(); }
                villages.Add(settlement);
            }
        }
        if(villages != null && villages.Count > 0) {
            return villages[UnityEngine.Random.Range(0, villages.Count)];
        }
        return null;
    }
    public NPCSettlement GetFirstVillageSettlementInRegionWithAliveResident(Region region, Faction faction) {
        for (int i = 0; i < allNonPlayerSettlements.Count; i++) {
            NPCSettlement settlement = allNonPlayerSettlements[i];
            if (settlement.region == region && (settlement.locationType == LOCATION_TYPE.VILLAGE)
                && settlement.HasAliveResident()
                && (settlement.owner == null || faction == null || !faction.IsFriendlyWith(settlement.owner))
                && (settlement.owner != faction || (settlement.owner == null && faction == null))) {
                return settlement;
            }
        }
        return null;
    }
    public BaseSettlement GetSettlementByPersistentID(string id) {
        return DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentID(id);
    }
    public void OwnSettlement(Faction newOwner, BaseSettlement settlement) {
        if (settlement.owner != null) {
            UnownSettlement(settlement);
        }
        newOwner.AddToOwnedSettlements(settlement);
        settlement.SetOwner(newOwner);
    }
    public void UnownSettlement(BaseSettlement settlement) {
        settlement.owner?.RemoveFromOwnedSettlements(settlement);
        settlement.SetOwner(null);
    }
    public Vector2 GetNameplatePosition(HexTile tile) {
        Vector2 defaultPos = tile.transform.position;
        defaultPos.y -= 1.25f;
        return defaultPos;
    }
    public SETTLEMENT_TYPE GetSettlementTypeForCharacter(Character character) {
        // if (character.characterClass.className == "Cult Leader") {
        //     return SETTLEMENT_TYPE.Cult_Town;
        // }
        if (character.faction != null && character.faction.factionType.type == FACTION_TYPE.Demon_Cult) {
            return SETTLEMENT_TYPE.Cult_Town;
        }
        return GetSettlementTypeForRace(character.race);
    }
    public SETTLEMENT_TYPE GetSettlementTypeForRace(RACE race) {
        switch (race) {
            case RACE.HUMANS:
                return SETTLEMENT_TYPE.Human_Village;
            case RACE.ELVES:
                return SETTLEMENT_TYPE.Elven_Hamlet;
            default:
                return SETTLEMENT_TYPE.Human_Village;
        }
    }
    
    #endregion

    #region Location Structures
    public LocationStructure CreateNewStructureAt(Region location, STRUCTURE_TYPE structureType, BaseSettlement settlement = null) {
        string noSpacesTypeName = UtilityScripts.Utilities.RemoveAllWhiteSpace(UtilityScripts.Utilities
            .NormalizeStringUpperCaseFirstLettersNoSpace(structureType.ToString()));
        string typeName = $"Inner_Maps.Location_Structures.{ noSpacesTypeName }, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        Type type = Type.GetType(typeName);
        if (type != null) {
            var structure = Activator.CreateInstance(type, location) as LocationStructure;
            location.AddStructure(structure);
            settlement?.AddStructure(structure);
            Assert.IsNotNull(structure, $"Created structure of {structureType.ToString()} is null!");
            structure.Initialize();
            DatabaseManager.Instance.structureDatabase.RegisterStructure(structure);
            return structure;
        }
        else {
            throw new Exception($"No structure class for type {structureType.ToString()}, {noSpacesTypeName}");
        }
    }
    public LocationStructure LoadNewStructureAt(Region location, STRUCTURE_TYPE structureType, SaveDataLocationStructure saveDataLocationStructure) {
        string noSpacesTypeName = UtilityScripts.Utilities.RemoveAllWhiteSpace(UtilityScripts.Utilities
            .NormalizeStringUpperCaseFirstLettersNoSpace(structureType.ToString()));
        string typeName = $"Inner_Maps.Location_Structures.{ noSpacesTypeName }, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        Type type = Type.GetType(typeName);
        if (type != null) {
            var structure = Activator.CreateInstance(type, location, saveDataLocationStructure) as LocationStructure;
            Assert.IsNotNull(structure, $"Structure at {location.name} is null {structureType}");
            if (!structure.hasBeenDestroyed) {
                //Do not initialize structures that have been destroyed.
                structure.Initialize();    
            }
            DatabaseManager.Instance.structureDatabase.RegisterStructure(structure);
            return structure;
        }
        else {
            throw new Exception($"No structure class for type {structureType.ToString()}, {noSpacesTypeName}");
        }
    }
    private void ConstructRaceStructureRequirements() {
        humanSurvivalStructures = new STRUCTURE_TYPE[] { STRUCTURE_TYPE.WAREHOUSE, STRUCTURE_TYPE.CEMETERY, STRUCTURE_TYPE.PRISON, STRUCTURE_TYPE.SMITHY, STRUCTURE_TYPE.BARRACKS, STRUCTURE_TYPE.HOSPICE };
        humanUtilityStructures = new STRUCTURE_TYPE[] { STRUCTURE_TYPE.GRANARY, STRUCTURE_TYPE.MINER_CAMP, STRUCTURE_TYPE.TAVERN };
        humanCombatStructures = new STRUCTURE_TYPE[] { STRUCTURE_TYPE.RAIDER_CAMP, STRUCTURE_TYPE.ASSASSIN_GUILD, STRUCTURE_TYPE.HUNTER_LODGE, STRUCTURE_TYPE.MAGE_QUARTERS };
        elfSurvivalStructures = new STRUCTURE_TYPE[] { STRUCTURE_TYPE.HUNTER_LODGE, STRUCTURE_TYPE.HOSPICE, STRUCTURE_TYPE.MAGE_QUARTERS };
        elfUtilityStructures = new STRUCTURE_TYPE[] { STRUCTURE_TYPE.TAVERN, STRUCTURE_TYPE.WAREHOUSE, STRUCTURE_TYPE.CEMETERY, STRUCTURE_TYPE.PRISON, STRUCTURE_TYPE.GRANARY, STRUCTURE_TYPE.MINER_CAMP };
        elfCombatStructures = new STRUCTURE_TYPE[] { STRUCTURE_TYPE.SMITHY, STRUCTURE_TYPE.BARRACKS, STRUCTURE_TYPE.RAIDER_CAMP, STRUCTURE_TYPE.ASSASSIN_GUILD };
    }
    public STRUCTURE_TYPE[] GetRaceStructureRequirements(RACE race, string category) {
        if (race == RACE.ELVES) {
            if (category == "Survival") { return elfSurvivalStructures; }
            else if (category == "Utility") { return elfUtilityStructures; }
            else if (category == "Combat") { return elfCombatStructures; }
        }
        if (category == "Survival") {
            return humanSurvivalStructures;
        } else if (category == "Utility") {
            return humanUtilityStructures;
        } else {
            return humanCombatStructures;
        }
    }
    /// <summary>
    /// Place structures for settlement. This requires that the settlement has enough unoccupied hex tiles.
    /// NOTE: This function also creates the LocationStructure instances.
    /// </summary>
    /// <param name="settlement">The settlement to create structures for.</param>
    /// <param name="innerTileMap">The Inner map that the settlement is part of.</param>
    /// <param name="structureResource">The resource the structures should be made of.</param>
    /// <param name="structureTypes">The structure types to create.</param>
    public IEnumerator PlaceBuiltStructuresForSettlement(BaseSettlement settlement, InnerTileMap innerTileMap, RESOURCE structureResource, [NotNull]params STRUCTURE_TYPE[] structureTypes) {
        for (int i = 0; i < structureTypes.Length; i++) {
            STRUCTURE_TYPE structureType = structureTypes[i];
            HexTile chosenTile = settlement.GetFirstUnoccupiedHexTile();
            Assert.IsNotNull(chosenTile, $"There are no more unoccupied tiles to place structure {structureType.ToString()} for settlement {settlement.name}");
            PlaceBuiltStructureForSettlement(settlement, innerTileMap, chosenTile, structureType, structureResource);
            yield return null;
        }
    }
    /// <summary>
    /// Place a built structure for a settlement at a given tile.
    /// NOTE: This function also creates the LocationStructure instances.
    /// </summary>
    /// <param name="settlement">The settlement to create structures for.</param>
    /// <param name="innerTileMap">The Inner map that the settlement is part of.</param>
    /// <param name="tileLocation">The hextile to place the structure object at</param>
    /// <param name="structureType">The structure type to create</param>
    /// <param name="structureResource">The resource the structure should be made of.</param>
    public void PlaceBuiltStructureForSettlement(BaseSettlement settlement, InnerTileMap innerTileMap, HexTile tileLocation, STRUCTURE_TYPE structureType, RESOURCE structureResource) {
        List<GameObject> choices = InnerMapManager.Instance.GetStructurePrefabsForStructure(structureType, structureResource);
        GameObject chosenStructurePrefab = CollectionUtilities.GetRandomElement(choices);
        innerTileMap.PlaceBuiltStructureTemplateAt(chosenStructurePrefab, tileLocation, settlement);
    }
    /// <summary>
    /// Place a built structure for a settlement at a given tile.
    /// NOTE: This function also creates the LocationStructure instances.
    /// </summary>
    /// <param name="settlement">The settlement to create structures for.</param>
    /// <param name="innerTileMap">The Inner map that the settlement is part of.</param>
    /// <param name="tileLocation">The hextile to place the structure object at</param>
    /// <param name="structureSetting">The settings that the structure should use.</param>
    private void PlaceBuiltStructureForSettlement(BaseSettlement settlement, InnerTileMap innerTileMap, HexTile tileLocation, StructureSetting structureSetting) {
        List<GameObject> choices = InnerMapManager.Instance.GetStructurePrefabsForStructure(structureSetting);
        GameObject chosenStructurePrefab = CollectionUtilities.GetRandomElement(choices);
        innerTileMap.PlaceBuiltStructureTemplateAt(chosenStructurePrefab, tileLocation, settlement);
    }
    public IEnumerator PlaceIndividualBuiltStructureForSettlementCoroutine(BaseSettlement settlement, InnerTileMap innerTileMap, StructureSetting structureSetting) {
        HexTile chosenTile = settlement.GetFirstUnoccupiedHexTile();
        Assert.IsNotNull(chosenTile, $"There are no more unoccupied tiles to place structure {structureSetting.ToString()} for settlement {settlement.name}");
        PlaceIndividualBuiltStructureForSettlement(settlement, innerTileMap, chosenTile, structureSetting);
        yield return null;
    }
    public List<LocationStructure> PlaceIndividualBuiltStructureForSettlement(BaseSettlement settlement, InnerTileMap innerTileMap, HexTile tileLocation, StructureSetting structureSetting) {
        List<GameObject> choices = InnerMapManager.Instance.GetIndividualStructurePrefabsForStructure(structureSetting);
        GameObject chosenStructurePrefab = CollectionUtilities.GetRandomElement(choices);
        return innerTileMap.PlaceBuiltStructureTemplateAt(chosenStructurePrefab, tileLocation, settlement);
    }
    public LocationStructure PlaceIndividualBuiltStructureForSettlement(BaseSettlement settlement, InnerTileMap innerTileMap, LocationGridTile tileLocation, StructureSetting structureSetting) {
        List<GameObject> choices = InnerMapManager.Instance.GetIndividualStructurePrefabsForStructure(structureSetting);
        GameObject chosenStructurePrefab = CollectionUtilities.GetRandomElement(choices);
        return innerTileMap.PlaceBuiltStructureTemplateAt(chosenStructurePrefab, tileLocation, settlement);
    }
    public List<LocationStructure> PlaceIndividualBuiltStructureForSettlement(BaseSettlement settlement, InnerTileMap innerTileMap, HexTile tileLocation, string prefabName) {
        GameObject chosenStructurePrefab = ObjectPoolManager.Instance.GetOriginalObjectFromPool(prefabName);
        return innerTileMap.PlaceBuiltStructureTemplateAt(chosenStructurePrefab, tileLocation, settlement);
    }
    public LocationStructure PlaceIndividualBuiltStructureForSettlement(BaseSettlement settlement, InnerTileMap innerTileMap, LocationGridTile tileLocation, string prefabName) {
        GameObject chosenStructurePrefab = ObjectPoolManager.Instance.GetOriginalObjectFromPool(prefabName);
        return innerTileMap.PlaceBuiltStructureTemplateAt(chosenStructurePrefab, tileLocation, settlement);
    }
    public LocationStructure PlaceIndividualBuiltStructureForSettlement(BaseSettlement settlement, InnerTileMap innerTileMap, GameObject chosenPrefab, LocationGridTile centerTile) {
        return innerTileMap.PlaceBuiltStructureTemplateAt(chosenPrefab, centerTile, settlement);
    }
    public bool CanPlaceStructureBlueprint(NPCSettlement npcSettlement, StructureSetting structureToPlace, out LocationGridTile targetTile, out string structurePrefabName, out int connectorToUse) {
        List<StructureConnector> availableStructureConnectors = npcSettlement.GetAvailableStructureConnectors();
        availableStructureConnectors = CollectionUtilities.Shuffle(availableStructureConnectors);
        List<GameObject> prefabChoices = InnerMapManager.Instance.GetIndividualStructurePrefabsForStructure(structureToPlace);
        prefabChoices = CollectionUtilities.Shuffle(prefabChoices);
        for (int j = 0; j < prefabChoices.Count; j++) {
            GameObject prefabGO = prefabChoices[j];
            LocationStructureObject prefabObject = prefabGO.GetComponent<LocationStructureObject>();
            StructureConnector validConnector = prefabObject.GetFirstValidConnector(availableStructureConnectors, npcSettlement.region.innerMap, out var connectorIndex, out LocationGridTile tileToPlaceStructure);
            if (validConnector != null) {
                targetTile = tileToPlaceStructure;
                structurePrefabName = prefabGO.name;
                connectorToUse = connectorIndex;
                return true;
            }
        }
        targetTile = null;
        structurePrefabName = string.Empty;
        connectorToUse = -1;
        return false;
    }
    public bool HasEnoughSpaceForStructure(string structurePrefabName, LocationGridTile tileLocation) {
        GameObject ogObject = ObjectPoolManager.Instance.GetOriginalObjectFromPool(structurePrefabName);
        LocationStructureObject locationStructureObject = ogObject.GetComponent<LocationStructureObject>();
        return locationStructureObject.HasEnoughSpaceIfPlacedOn(tileLocation);
    }
    #endregion

    #region Tile Features
    public T CreateTileFeature<T>([NotNull] string featureName) where T : TileFeature {
        string typeName = $"Locations.Tile_Features.{featureName}, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        System.Type type = System.Type.GetType(typeName);
        Assert.IsNotNull(type, $"type for {featureName} is null!");
        return System.Activator.CreateInstance(type) as T;
    }
    #endregion

    #region Region Features
    public T CreateRegionFeature<T>([NotNull] string featureName) where T : RegionFeature {
        string typeName = $"Locations.Region_Features.{featureName}, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        System.Type type = System.Type.GetType(typeName);
        Assert.IsNotNull(type, $"type for {featureName} is null!");
        return System.Activator.CreateInstance(type) as T;
    }
    #endregion

    #region Settlement Type
    public SettlementType CreateSettlementType(SETTLEMENT_TYPE settlementType) {
        string enumStr = settlementType.ToString();
        var typeName = $"Locations.Settlements.Settlement_Types.{UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(enumStr)}, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        Type type = Type.GetType(typeName);
        if (type != null) {
            SettlementType data = Activator.CreateInstance(type) as SettlementType;
            return data;
        } else {
            throw new Exception($"{typeName} has no data!");
        }
    }
    public SettlementType CreateSettlementType(SaveDataSettlementType saveData) {
        string enumStr = saveData.settlementType.ToString();
        var typeName = $"Locations.Settlements.Settlement_Types.{UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(enumStr)}, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        Type type = Type.GetType(typeName);
        if (type != null) {
            SettlementType data = Activator.CreateInstance(type, saveData) as SettlementType;
            return data;
        } else {
            throw new Exception($"{typeName} has no data!");
        }
    }
    #endregion
}

public class Island {

    public List<Region> regionsInIsland;

    public Island(Region region) {
        regionsInIsland = new List<Region>();
        regionsInIsland.Add(region);
    }

    public bool TryGetLandmarkThatCanConnectToOtherIsland(Island otherIsland, List<Island> allIslands, out Region regionToConnectTo, out Region regionThatWillConnect) {
        for (int i = 0; i < regionsInIsland.Count; i++) {
            Region currRegion = regionsInIsland[i];
            List<Region> adjacent = currRegion.neighbours.Where(x => LandmarkManager.Instance.GetIslandOfRegion(x, allIslands) != this).ToList(); //get all adjacent regions, that does not belong to this island.
            if (adjacent != null && adjacent.Count > 0) {
                regionToConnectTo = adjacent[Random.Range(0, adjacent.Count)];
                regionThatWillConnect = currRegion;
                return true;

            }
        }
        regionToConnectTo = null;
        regionThatWillConnect = null;
        return false;
    }
}