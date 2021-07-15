using System;
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
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif


public partial class LandmarkManager : BaseMonoBehaviour {

    public static LandmarkManager Instance = null;
    public const int REGION_VILLAGE_CAPACITY = 3;
    
    [SerializeField] private StructureDataDictionary structureData;
    
    
    //public STRUCTURE_TYPE[] humanSurvivalStructures { get; private set; }
    //public STRUCTURE_TYPE[] humanUtilityStructures { get; private set; }
    //public STRUCTURE_TYPE[] humanCombatStructures { get; private set; }
    //public STRUCTURE_TYPE[] elfSurvivalStructures { get; private set; }
    //public STRUCTURE_TYPE[] elfUtilityStructures { get; private set; }
    //public STRUCTURE_TYPE[] elfCombatStructures { get; private set; }

    #region getters
    public List<BaseSettlement> allSettlements => DatabaseManager.Instance.settlementDatabase.allSettlements;
    public List<NPCSettlement> allNonPlayerSettlements => DatabaseManager.Instance.settlementDatabase.allNonPlayerSettlements;
    #endregion
    
    public void Initialize() {
        //ConstructRaceStructureRequirements();
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
    public List<LocationStructure> GetStructuresOfType(STRUCTURE_TYPE p_structureType) {
        Dictionary<STRUCTURE_TYPE, List<LocationStructure>> structureDict = GridMap.Instance.mainRegion.structures;
        if (structureDict.ContainsKey(p_structureType)) {
            return structureDict[p_structureType];
        }
        return null;
    }
    public int GetStructuresOfTypeCount(STRUCTURE_TYPE p_structureType) {
        int count = 0;
        List<LocationStructure> structures = GetStructuresOfType(p_structureType);
        if (structures != null) {
            count = structures.Count;
        }
        return count;
    }
    public void PopulateAllSpecialStructures(List<LocationStructure> specialStructures) {
        foreach (var regionStructure in GridMap.Instance.mainRegion.structures) {
            if (regionStructure.Key.IsSpecialStructure()) {
                specialStructures.AddRange(regionStructure.Value);
            }
        }
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
        //List<Area> tiles = GameUtilities.GetHexTilesGivenCoordinates(saveDataNpcSettlement.tileCoordinates, GridMap.Instance.map);
        NPCSettlement newNpcSettlement = new NPCSettlement(saveDataNpcSettlement);
        //for (int i = 0; i < tiles.Count; i++) {
        //    Area tile = tiles[i];
        //    newNpcSettlement.AddAreaToSettlement(tile);
        //}
        //Messenger.Broadcast(SettlementSignals.SETTLEMENT_CREATED, newNpcSettlement);
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

        //Moved this to LoadReferences because the first load is for initial data only since we multithread initial data
        //List<Area> areas = RuinarchListPool<Area>.Claim();
        //GameUtilities.PopulateAreasGivenCoordinates(areas, saveDataPlayerSettlement.tileCoordinates, GridMap.Instance.map);
        //for (int i = 0; i < areas.Count; i++) {
        //    Area a = areas[i];
        //    newPlayerSettlement.AddAreaToSettlement(a);
        //}
        //RuinarchListPool<Area>.Release(areas);
        //Messenger.Broadcast(SettlementSignals.SETTLEMENT_CREATED, newPlayerSettlement);
        DatabaseManager.Instance.settlementDatabase.RegisterSettlement(newPlayerSettlement);
        return newPlayerSettlement;
    }
    public NPCSettlement GetRandomActiveSapientSettlement() {
        List<BaseSettlement> villages = RuinarchListPool<BaseSettlement>.Claim();
        for (int i = 0; i < allNonPlayerSettlements.Count; i++) {
            NPCSettlement settlement = allNonPlayerSettlements[i];
            if(settlement.locationType == LOCATION_TYPE.VILLAGE && settlement.owner != null && settlement.residents.Count > 0 && settlement.owner.race.IsSapient()) {
                villages.Add(settlement);
            }
        }
        NPCSettlement chosen = null;
        if(villages.Count > 0) {
            chosen = villages[UnityEngine.Random.Range(0, villages.Count)] as NPCSettlement;
        }
        RuinarchListPool<BaseSettlement>.Release(villages);
        return chosen;
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
    //private void ConstructRaceStructureRequirements() {
    //    humanSurvivalStructures = new STRUCTURE_TYPE[] { STRUCTURE_TYPE.WAREHOUSE, STRUCTURE_TYPE.CEMETERY, STRUCTURE_TYPE.PRISON, STRUCTURE_TYPE.BARRACKS, STRUCTURE_TYPE.HOSPICE };
    //    humanUtilityStructures = new STRUCTURE_TYPE[] { STRUCTURE_TYPE.TAVERN };
    //    humanCombatStructures = new STRUCTURE_TYPE[] { STRUCTURE_TYPE.HUNTER_LODGE, STRUCTURE_TYPE.MAGE_QUARTERS };
    //    elfSurvivalStructures = new STRUCTURE_TYPE[] { STRUCTURE_TYPE.HUNTER_LODGE, STRUCTURE_TYPE.HOSPICE, STRUCTURE_TYPE.MAGE_QUARTERS };
    //    elfUtilityStructures = new STRUCTURE_TYPE[] { STRUCTURE_TYPE.TAVERN, STRUCTURE_TYPE.WAREHOUSE, STRUCTURE_TYPE.CEMETERY, STRUCTURE_TYPE.PRISON };
    //    elfCombatStructures = new STRUCTURE_TYPE[] { STRUCTURE_TYPE.BARRACKS };
    //}
    //public STRUCTURE_TYPE[] GetRaceStructureRequirements(RACE race, string category) {
    //    if (race == RACE.ELVES) {
    //        if (category == "Survival") { return elfSurvivalStructures; }
    //        else if (category == "Utility") { return elfUtilityStructures; }
    //        else if (category == "Combat") { return elfCombatStructures; }
    //    }
    //    if (category == "Survival") {
    //        return humanSurvivalStructures;
    //    } else if (category == "Utility") {
    //        return humanUtilityStructures;
    //    } else {
    //        return humanCombatStructures;
    //    }
    //}
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
            Area chosenTile = settlement.areas.ElementAtOrDefault(0);
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
        List<GameObject> choices = InnerMapManager.Instance.GetStructurePrefabsForStructure(structureSetting);
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
        bool canPlace = false;
        List<StructureConnector> availableStructureConnectors = RuinarchListPool<StructureConnector>.Claim();
        npcSettlement.PopulateStructureConnectorsForStructureType(availableStructureConnectors, structureToPlace.structureType);
        string connectorLog;
        if (structureToPlace.structureType == STRUCTURE_TYPE.MINE) {
            //order spots based on distance with settlement city center
            availableStructureConnectors = availableStructureConnectors.OrderBy(c => Vector2.Distance(c.transform.position, 
                npcSettlement.cityCenter.tiles.ElementAt(0).centeredWorldLocation)).ToList();
// #if DEBUG_LOG
//             Debug.Log($"Evaluating structure connectors for {npcSettlement.name} to place {structureToPlace.ToString()}. Available connectors are:\n {availableStructureConnectors.ComafyList()}");
// #endif
            CanPlaceStructureBlueprintMine(npcSettlement, structureToPlace, availableStructureConnectors, out targetTile, out structurePrefabName, out connectorToUse, out connectorTile, out canPlace, out connectorLog);
// #if DEBUG_LOG
//             Debug.Log($"Found Connector at {connectorTile}. Connector Log for {npcSettlement.name} to place {structureToPlace.ToString()}:\n {connectorLog}");
// #endif
        } else {
            //did not shuffle connectors for mine since we want the village to place the mine as close as possible.
            //Related card: https://trello.com/c/lFTbmJ4d/4932-optimize-mine-placement
            CollectionUtilities.Shuffle(availableStructureConnectors);
// #if DEBUG_LOG
//             Debug.Log($"Evaluating structure connectors for {npcSettlement.name} to place {structureToPlace.ToString()}. Available connectors are:\n {availableStructureConnectors.ComafyList()}");
// #endif
            CanPlaceStructureBlueprintDefault(npcSettlement, structureToPlace, availableStructureConnectors, out targetTile, out structurePrefabName, out connectorToUse, out connectorTile, out canPlace, out connectorLog);
// #if DEBUG_LOG
//             Debug.Log($"Found Connector at {connectorTile}. Connector Log for {npcSettlement.name} to place {structureToPlace.ToString()}:\n {connectorLog}");
// #endif
        }
        // List<GameObject> prefabChoices = InnerMapManager.Instance.GetStructurePrefabsForStructure(structureToPlace);
        // CollectionUtilities.Shuffle(prefabChoices);
        // bool canPlace = false;
        // targetTile = null;
        // structurePrefabName = string.Empty;
        // connectorToUse = -1;
        // connectorTile = null;
        // for (int j = 0; j < prefabChoices.Count; j++) {
        //     GameObject prefabGO = prefabChoices[j];
        //     LocationStructureObject prefabObject = prefabGO.GetComponent<LocationStructureObject>();
        //     StructureConnector validConnector = prefabObject.GetFirstValidConnector(availableStructureConnectors, npcSettlement.region.innerMap, npcSettlement, 
        //         out var connectorIndex, out LocationGridTile tileToPlaceStructure, out connectorTile, structureToPlace, out _);
        //     if (validConnector != null) {
        //         targetTile = tileToPlaceStructure;
        //         structurePrefabName = prefabGO.name;
        //         connectorToUse = connectorIndex;
        //         canPlace = true;
        //         break;
        //     }
        // }
        RuinarchListPool<StructureConnector>.Release(availableStructureConnectors);
        return canPlace;
    }
    public StructureConnector CanPlaceStructureBlueprintDefault(NPCSettlement npcSettlement, StructureSetting structureToPlace, List<StructureConnector> availableStructureConnectors,
        out LocationGridTile targetTile, out string structurePrefabName, out int connectorToUse, out LocationGridTile connectorTile, out bool canPlace, out string functionLog) {
        List<GameObject> prefabChoices = InnerMapManager.Instance.GetStructurePrefabsForStructure(structureToPlace);
        CollectionUtilities.Shuffle(prefabChoices);
        canPlace = false;
        targetTile = null;
        structurePrefabName = string.Empty;
        connectorToUse = -1;
        connectorTile = null;
        functionLog = string.Empty;
        for (int j = 0; j < prefabChoices.Count; j++) {
            GameObject prefabGO = prefabChoices[j];
            LocationStructureObject prefabObject = prefabGO.GetComponent<LocationStructureObject>();
            StructureConnector validConnector = prefabObject.GetFirstValidConnector(availableStructureConnectors, npcSettlement.region.innerMap, npcSettlement, 
                out var connectorIndex, out LocationGridTile tileToPlaceStructure, out connectorTile, structureToPlace, out var connectorLog);
            if (validConnector != null) {
                targetTile = tileToPlaceStructure;
                structurePrefabName = prefabGO.name;
                connectorToUse = connectorIndex;
                canPlace = true;
#if DEBUG_LOG
                functionLog = $"{functionLog}\n{connectorLog}";
#endif
                return validConnector;
            }
#if DEBUG_LOG
            functionLog = $"{functionLog}\n{connectorLog}";
#endif
        }
        return null;
    }
    public StructureConnector CanPlaceStructureBlueprintMine(NPCSettlement npcSettlement, StructureSetting structureToPlace, List<StructureConnector> availableStructureConnectors,
        out LocationGridTile targetTile, out string structurePrefabName, out int connectorToUse, out LocationGridTile connectorTile, out bool canPlace, out string functionLog) {
        List<GameObject> prefabChoices = InnerMapManager.Instance.GetStructurePrefabsForStructure(structureToPlace);
        CollectionUtilities.Shuffle(prefabChoices);
        canPlace = false;
        targetTile = null;
        structurePrefabName = string.Empty;
        connectorToUse = -1;
        connectorTile = null;
        functionLog = string.Empty;
        for (int i = 0; i < availableStructureConnectors.Count; i++) {
            StructureConnector connector = availableStructureConnectors[i];
            for (int j = 0; j < prefabChoices.Count; j++) {
                GameObject prefabGO = prefabChoices[j];
                LocationStructureObject prefabObject = prefabGO.GetComponent<LocationStructureObject>();
                if (prefabObject.IsConnectorValid(connector, npcSettlement.region.innerMap, npcSettlement, 
                    out var connectorIndex, out LocationGridTile tileToPlaceStructure, out connectorTile, structureToPlace, out var connectorLog)) {
                    targetTile = tileToPlaceStructure;
                    structurePrefabName = prefabGO.name;
                    connectorToUse = connectorIndex;
                    canPlace = true;
#if DEBUG_LOG
                    functionLog = $"{functionLog}\n{connectorLog}";
#endif
                    return connector;
                }
#if DEBUG_LOG
                functionLog = $"{functionLog}\n{connectorLog}";
#endif
            }   
        }
        return null;
    }
    public bool HasEnoughSpaceForStructure(string structurePrefabName, LocationGridTile tileLocation) {
        GameObject ogObject = ObjectPoolManager.Instance.GetOriginalObjectFromPool(structurePrefabName);
        LocationStructureObject locationStructureObject = ogObject.GetComponent<LocationStructureObject>();
        return locationStructureObject.HasEnoughSpaceIfPlacedOn(tileLocation);
    }
    public bool HasAffectedCorruptedTilesForStructure(string structurePrefabName, LocationGridTile tileLocation) {
        GameObject ogObject = ObjectPoolManager.Instance.GetOriginalObjectFromPool(structurePrefabName);
        LocationStructureObject locationStructureObject = ogObject.GetComponent<LocationStructureObject>();
        return locationStructureObject.HasAffectedCorruptedTilesIfPlacedOn(tileLocation);
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
        List<STRUCTURE_TYPE> structureTypes = CollectionUtilities.GetEnumValues<STRUCTURE_TYPE>().ToList();
        for (int i = 0; i < scriptableObjects.Length; i++) {
            string asset = scriptableObjects[i];
            StructureData loadedSprite = (StructureData)UnityEditor.AssetDatabase.LoadAssetAtPath(asset, typeof(StructureData));
            string strStructureName = loadedSprite.name;
            strStructureName = strStructureName.Replace("Data", "").TrimEnd().ToUpper();
            strStructureName = UtilityScripts.Utilities.NotNormalizedConversionStringToEnum(strStructureName);
            STRUCTURE_TYPE structureType = (STRUCTURE_TYPE)System.Enum.Parse(typeof(STRUCTURE_TYPE), strStructureName);
            structureTypes.Remove(structureType);
            structureData.Add(structureType, loadedSprite);
        }
        EditorUtility.SetDirty(this);
        if (structureTypes.Count > 0) {
            Debug.LogWarning($"No Structure Data for: {structureTypes.ComafyList()}");
        }
    }
#endif
    #endregion
}