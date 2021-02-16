﻿using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using JetBrains.Annotations;
using Locations.Region_Features;
using Locations.Settlements;
using Locations.Settlements.Settlement_Types;
using Locations.Area_Features;
using UnityEngine.Assertions;
using UtilityScripts;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif


public partial class LandmarkManager : BaseMonoBehaviour {

    public static LandmarkManager Instance = null;
    public const int REGION_VILLAGE_CAPACITY = 3;
    
    [SerializeField] private StructureDataDictionary structureData;
    
    
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

    #region Utilities
    public LocationStructure GetSpecialStructureOfType(STRUCTURE_TYPE p_structureType) {
        List<LocationStructure> _allLandmarks = GetAllSpecialStructures();
        for (int i = 0; i < _allLandmarks.Count; i++) {
            LocationStructure structure = _allLandmarks[i];
            if (structure.structureType == p_structureType) {
                return structure;
            }
        }
        return null;
    }
    public List<LocationStructure> GetSpecialStructuresOfType(STRUCTURE_TYPE p_structureType) {
        List<LocationStructure> structures = new List<LocationStructure>();
        List<LocationStructure> _allLandmarks = GetAllSpecialStructures();
        for (int i = 0; i < _allLandmarks.Count; i++) {
            LocationStructure currLandmark = _allLandmarks[i];
            if (currLandmark.structureType == p_structureType) {
                structures.Add(currLandmark);
            }
        }
        return structures;
    }
    public List<LocationStructure> GetAllSpecialStructures() {
        List<LocationStructure> specialStructures = RuinarchListPool<LocationStructure>.Claim();
        foreach (var regionStructure in GridMap.Instance.mainRegion.structures) {
            if (regionStructure.Key.IsSpecialStructure()) {
                specialStructures.AddRange(regionStructure.Value);
            }
        }
        return specialStructures;
    }
    #endregion

    #region Settlements
    public NPCSettlement CreateNewSettlement(Region region, LOCATION_TYPE locationType, params Area[] tiles) {
        NPCSettlement newNpcSettlement = new NPCSettlement(region, locationType);
        if (tiles != null) {
            newNpcSettlement.AddAreaToSettlement(tiles);    
        }
        Messenger.Broadcast(SettlementSignals.SETTLEMENT_CREATED, newNpcSettlement);
        DatabaseManager.Instance.settlementDatabase.RegisterSettlement(newNpcSettlement);
        newNpcSettlement.Initialize();
        return newNpcSettlement;
    }
    public NPCSettlement LoadNPCSettlement(SaveDataNPCSettlement saveDataNpcSettlement) {
        List<Area> tiles = GameUtilities.GetHexTilesGivenCoordinates(saveDataNpcSettlement.tileCoordinates, GridMap.Instance.map);
        NPCSettlement newNpcSettlement = new NPCSettlement(saveDataNpcSettlement);
        for (int i = 0; i < tiles.Count; i++) {
            Area tile = tiles[i];
            newNpcSettlement.AddAreaToSettlement(tile);
        }
        Messenger.Broadcast(SettlementSignals.SETTLEMENT_CREATED, newNpcSettlement);
        DatabaseManager.Instance.settlementDatabase.RegisterSettlement(newNpcSettlement);
        return newNpcSettlement;
    }
    public PlayerSettlement CreateNewPlayerSettlement(params Area[] tiles) {
        PlayerSettlement newPlayerSettlement = new PlayerSettlement();
        newPlayerSettlement.AddAreaToSettlement(tiles);
        Messenger.Broadcast(SettlementSignals.SETTLEMENT_CREATED, newPlayerSettlement);
        DatabaseManager.Instance.settlementDatabase.RegisterSettlement(newPlayerSettlement);
        return newPlayerSettlement;
    }
    public PlayerSettlement LoadPlayerSettlement(SaveDataPlayerSettlement saveDataPlayerSettlement) {
        PlayerSettlement newPlayerSettlement = new PlayerSettlement(saveDataPlayerSettlement);

        List<Area> tiles = GameUtilities.GetHexTilesGivenCoordinates(saveDataPlayerSettlement.tileCoordinates, GridMap.Instance.map);
        for (int i = 0; i < tiles.Count; i++) {
            Area tile = tiles[i];
            newPlayerSettlement.AddAreaToSettlement(tile);
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
    public NPCSettlement GetRandomActiveSapientSettlement() {
        List<NPCSettlement> villages = null;
        for (int i = 0; i < allNonPlayerSettlements.Count; i++) {
            NPCSettlement settlement = allNonPlayerSettlements[i];
            if(settlement.locationType == LOCATION_TYPE.VILLAGE && settlement.owner != null && settlement.residents.Count > 0 && settlement.owner.race.IsSapient()) {
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
            if (settlement.region == region && settlement.locationType == LOCATION_TYPE.VILLAGE
                && settlement.HasAliveResident()
                && settlement.owner != faction
                && (settlement.owner == null || faction == null || faction.IsHostileWith(settlement.owner))) {
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
    public SETTLEMENT_TYPE GetSettlementTypeForFaction(Faction faction) {
        switch (faction.factionType.type) {
            case FACTION_TYPE.Elven_Kingdom:
                return SETTLEMENT_TYPE.Elven_Hamlet;
            case FACTION_TYPE.Human_Empire:
                return SETTLEMENT_TYPE.Human_Village;
            case FACTION_TYPE.Vampire_Clan:
            case FACTION_TYPE.Lycan_Clan:
                return GetSettlementTypeForRace(faction.race);
            case FACTION_TYPE.Demon_Cult:
                return SETTLEMENT_TYPE.Cult_Town;
            default:
                return GetSettlementTypeForRace(faction.race);
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
    public IEnumerator PlaceBuiltLandmark(BaseSettlement settlement, InnerTileMap innerTileMap, RESOURCE structureResource, [NotNull]params STRUCTURE_TYPE[] structureTypes) {
        for (int i = 0; i < structureTypes.Length; i++) {
            STRUCTURE_TYPE structureType = structureTypes[i];
            Area chosenTile = settlement.areas[0];
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
    public void PlaceBuiltStructureForSettlement(BaseSettlement settlement, InnerTileMap innerTileMap, Area tileLocation, STRUCTURE_TYPE structureType, RESOURCE structureResource) {
        List<GameObject> choices = InnerMapManager.Instance.GetStructurePrefabsForStructure(structureType, structureResource);
        GameObject chosenStructurePrefab = CollectionUtilities.GetRandomElement(choices);
        innerTileMap.PlaceBuiltStructureTemplateAt(chosenStructurePrefab, tileLocation, settlement);
    }
    public IEnumerator PlaceFirstStructureForSettlement(BaseSettlement settlement, InnerTileMap innerTileMap, StructureSetting structureSetting) {
        Area chosenTile = settlement.areas[0];
        Assert.IsNotNull(chosenTile, $"There are no more unoccupied tiles to place structure {structureSetting.ToString()} for settlement {settlement.name}");
        PlaceIndividualBuiltStructureForSettlement(settlement, innerTileMap, chosenTile, structureSetting);
        yield return null;
    }
    private List<LocationStructure> PlaceIndividualBuiltStructureForSettlement(BaseSettlement settlement, InnerTileMap innerTileMap, Area tileLocation, StructureSetting structureSetting) {
        List<GameObject> choices = InnerMapManager.Instance.GetIndividualStructurePrefabsForStructure(structureSetting);
        GameObject chosenStructurePrefab = CollectionUtilities.GetRandomElement(choices);
        return innerTileMap.PlaceBuiltStructureTemplateAt(chosenStructurePrefab, tileLocation, settlement);
    }
    public LocationStructure PlaceIndividualBuiltStructureForSettlement(BaseSettlement settlement, InnerTileMap innerTileMap, LocationGridTile tileLocation, string prefabName) {
        GameObject chosenStructurePrefab = ObjectPoolManager.Instance.GetOriginalObjectFromPool(prefabName);
        return innerTileMap.PlaceBuiltStructureTemplateAt(chosenStructurePrefab, tileLocation, settlement);
    }
    public LocationStructure PlaceIndividualBuiltStructureForSettlement(BaseSettlement settlement, InnerTileMap innerTileMap, GameObject chosenPrefab, LocationGridTile centerTile) {
        return innerTileMap.PlaceBuiltStructureTemplateAt(chosenPrefab, centerTile, settlement);
    }
    public bool CanPlaceStructureBlueprint(NPCSettlement npcSettlement, StructureSetting structureToPlace, out LocationGridTile targetTile, out string structurePrefabName, 
        out int connectorToUse, out LocationGridTile connectorTile) {
        List<StructureConnector> availableStructureConnectors = npcSettlement.GetStructureConnectorsForStructureType(structureToPlace.structureType);
        availableStructureConnectors = CollectionUtilities.Shuffle(availableStructureConnectors);
        List<GameObject> prefabChoices = InnerMapManager.Instance.GetIndividualStructurePrefabsForStructure(structureToPlace);
        prefabChoices = CollectionUtilities.Shuffle(prefabChoices);
        for (int j = 0; j < prefabChoices.Count; j++) {
            GameObject prefabGO = prefabChoices[j];
            LocationStructureObject prefabObject = prefabGO.GetComponent<LocationStructureObject>();
            StructureConnector validConnector = prefabObject.GetFirstValidConnector(availableStructureConnectors, npcSettlement.region.innerMap, out var connectorIndex, out LocationGridTile tileToPlaceStructure, out connectorTile, structureToPlace);
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
        connectorTile = null;
        return false;
    }
    public bool HasEnoughSpaceForStructure(string structurePrefabName, LocationGridTile tileLocation) {
        GameObject ogObject = ObjectPoolManager.Instance.GetOriginalObjectFromPool(structurePrefabName);
        LocationStructureObject locationStructureObject = ogObject.GetComponent<LocationStructureObject>();
        return locationStructureObject.HasEnoughSpaceIfPlacedOn(tileLocation);
    }
    #endregion

    #region Area Features
    public T CreateAreaFeature<T>([NotNull] string featureName) where T : AreaFeature {
        string typeName = $"Locations.Area_Features.{featureName}, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
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

    #region Structure Data
    public StructureData GetStructureData(STRUCTURE_TYPE p_structureType) {
        if (structureData.ContainsKey(p_structureType)) {
            return structureData[p_structureType];
        }
        throw new Exception($"No structure data for {p_structureType.ToString()}");
    }
#if UNITY_EDITOR
    public void LoadStructureData() {
        structureData = new StructureDataDictionary();
        string assetPath = "Assets/Scriptable Object Assets/Structure Data/";
        string[] scriptableObjects = Directory.GetFiles(assetPath,"*.asset");
        for (int i = 0; i < scriptableObjects.Length; i++) {
            string asset = scriptableObjects[i];
            StructureData loadedSprite = (StructureData)UnityEditor.AssetDatabase.LoadAssetAtPath(asset, typeof(StructureData));
            string strStructureName = loadedSprite.name;
            strStructureName = strStructureName.Replace("Data", "").TrimEnd().ToUpper();
            strStructureName = UtilityScripts.Utilities.NotNormalizedConversionStringToEnum(strStructureName);
            STRUCTURE_TYPE structureType = (STRUCTURE_TYPE)System.Enum.Parse(typeof(STRUCTURE_TYPE), strStructureName);
            structureData.Add(structureType, loadedSprite);
        }
        EditorUtility.SetDirty(this);
    }
#endif
    #endregion
}