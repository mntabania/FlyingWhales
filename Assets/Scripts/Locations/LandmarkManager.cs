using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using JetBrains.Annotations;
using Locations.Settlements;
using UnityEngine.Tilemaps;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine.Assertions;
using UtilityScripts;
using Debug = System.Diagnostics.Debug;
using Random = UnityEngine.Random;
using ThePortal = Inner_Maps.Location_Structures.ThePortal;

public partial class LandmarkManager : MonoBehaviour {

    public static LandmarkManager Instance = null;
    public static readonly int Max_Connections = 3;
    public const int DELAY_DIVINE_INTERVENTION_DURATION = 144;
    public const int SUMMON_MINION_DURATION = 96;
    public const int MAX_RESOURCE_PILE = 500;
    
    [SerializeField] private List<LandmarkData> landmarkData;
    public List<AreaData> areaData;

    public List<BaseLandmark> allLandmarks;
    public List<BaseSettlement> allSettlements;
    public List<NPCSettlement> allNonPlayerSettlements;

    [SerializeField] private GameObject landmarkGO;

    private Dictionary<LANDMARK_TYPE, LandmarkData> landmarkDataDict;

    public AreaTypeSpriteDictionary locationPortraits;
    public List<LocationEvent> locationEventsData { get; private set; }

    public STRUCTURE_TYPE[] humanSurvivalStructures { get; private set; }
    public STRUCTURE_TYPE[] humanUtilityStructures { get; private set; }
    public STRUCTURE_TYPE[] humanCombatStructures { get; private set; }
    public STRUCTURE_TYPE[] elfSurvivalStructures { get; private set; }
    public STRUCTURE_TYPE[] elfUtilityStructures { get; private set; }
    public STRUCTURE_TYPE[] elfCombatStructures { get; private set; }

    //The Anvil
    public Dictionary<string, AnvilResearchData> anvilResearchData;

    public void Initialize() {
        allSettlements = new List<BaseSettlement>();
        allNonPlayerSettlements = new List<NPCSettlement>();
        ConstructLandmarkData();
        LoadLandmarkTypeDictionary();
        ConstructLocationEventsData();
        ConstructRaceStructureRequirements();
    }

    #region Monobehaviours
    private void Awake() {
        Instance = this;
    }
    #endregion

    #region Landmarks
    private void ConstructLocationEventsData() {
        locationEventsData = new List<LocationEvent>() {
            new NewResidentEvent(),
        };
    }
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
        location.UpdateBuildSprites();
        Messenger.Broadcast(Signals.LANDMARK_CREATED, newLandmark);
        return newLandmark;
    }
    public void DestroyLandmarkOnTile(HexTile tile) {
        BaseLandmark landmarkOnTile = tile.landmarkOnTile;
        if (landmarkOnTile == null) {
            return;
        }
        landmarkOnTile.DestroyLandmark();
        tile.UpdateBuildSprites();
        tile.RemoveLandmarkVisuals();
        tile.RemoveLandmarkOnTile();
        Messenger.Broadcast(Signals.LANDMARK_DESTROYED, landmarkOnTile, tile);
        // if (landmarkOnTile.specificLandmarkType.IsPlayerLandmark() && tile.region.locationType.IsSettlementType() == false) {
        //     Messenger.Broadcast(Signals.FORCE_CANCEL_ALL_JOB_TYPES_TARGETING_POI, 
        //         tile.region.regionTileObject as IPointOfInterest, 
        //         "target has been destroyed", JOB_TYPE.ATTACK_DEMONIC_REGION);    
        // }
    }
    public BaseLandmark LoadLandmarkOnTile(HexTile location, BaseLandmark landmark) {
        BaseLandmark newLandmark = location.LoadLandmark(landmark);
        return newLandmark;
    }
    public GameObject GetLandmarkGO() {
        return this.landmarkGO;
    }
    public bool AreAllNonPlayerAreasCorrupted() {
        List<NPCSettlement> areas = allNonPlayerSettlements;
        for (int i = 0; i < areas.Count; i++) {
            NPCSettlement npcSettlement = areas[i];
            for (int j = 0; j < npcSettlement.tiles.Count; j++) {
                HexTile currTile = npcSettlement.tiles[j];
                if (!currTile.isCorrupted) {
                    return false;
                }    
            }
           
        }
        return true;
    }
    public BaseLandmark CreateNewLandmarkInstance(HexTile location, LANDMARK_TYPE type) {
        // if (type.IsPlayerLandmark()) {
        //     var typeName = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(type.ToString());
        //     System.Type systemType = System.Type.GetType(typeName);
        //     if (systemType != null) {
        //         return System.Activator.CreateInstance(systemType, location, type) as BaseLandmark;
        //     }
        //     return null;
        // } else {
            return new BaseLandmark(location, type);
        // }
    }
    public BaseLandmark CreateNewLandmarkInstance(HexTile location, SaveDataLandmark data) {
        if (data.landmarkType.IsPlayerLandmark()) {
            var typeName = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(data.landmarkType.ToString());
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
        List<BaseLandmark> allLandmarks = GetAllLandmarks();
        for (int i = 0; i < allLandmarks.Count; i++) {
            BaseLandmark currLandmark = allLandmarks[i];
            if (currLandmark.id == id) {
                return currLandmark;
            }
        }
        return null;
    }
    public BaseLandmark GetLandmarkByName(string name) {
        List<BaseLandmark> allLandmarks = GetAllLandmarks();
        for (int i = 0; i < allLandmarks.Count; i++) {
            BaseLandmark currLandmark = allLandmarks[i];
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
    public List<Character> GetAllDeadCharactersInLocation(ILocation location) {
        List<Character> characters = new List<Character>();
        for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
            Character character = CharacterManager.Instance.allCharacters[i];
            if(character.isDead && character.currentRegion.IsSameCoreLocationAs(location) && !(character is Summon)) {
                if(character.marker || character.grave != null) { //Only resurrect characters who are in the tombstone or still has a marker in the npcSettlement
                    characters.Add(character);
                }
            }
        }
        return characters;
    }
    #endregion

    #region Areas
    public AreaData GetAreaData(LOCATION_TYPE locationType) {
        for (int i = 0; i < areaData.Count; i++) {
            AreaData currData = areaData[i];
            if (currData.locationType == locationType) {
                return currData;
            }
        }
        throw new System.Exception($"No npcSettlement data for type {locationType}");
    }
    public NPCSettlement CreateNewSettlement(Region region, LOCATION_TYPE locationType, int citizenCount, params HexTile[] tiles) {
        NPCSettlement newNpcSettlement = new NPCSettlement(region, locationType, citizenCount);
        newNpcSettlement.AddTileToSettlement(tiles);
        Messenger.Broadcast(Signals.AREA_CREATED, newNpcSettlement);
        allSettlements.Add(newNpcSettlement);
        if(locationType != LOCATION_TYPE.DEMONIC_INTRUSION) {
            allNonPlayerSettlements.Add(newNpcSettlement);
        }
        return newNpcSettlement;
    }
    public PlayerSettlement CreateNewPlayerSettlement(params HexTile[] tiles) {
        PlayerSettlement newPlayerSettlement = new PlayerSettlement();
        newPlayerSettlement.AddTileToSettlement(tiles);
        Messenger.Broadcast(Signals.AREA_CREATED, newPlayerSettlement);
        allSettlements.Add(newPlayerSettlement);
        return newPlayerSettlement;
    }
    public void RemoveArea(NPCSettlement npcSettlement) {
        allSettlements.Remove(npcSettlement);
    }
    public NPCSettlement CreateNewArea(SaveDataArea saveDataArea) {
        NPCSettlement newNpcSettlement = new NPCSettlement(saveDataArea);

        if (locationPortraits.ContainsKey(newNpcSettlement.locationType)) {
        }
        Messenger.Broadcast(Signals.AREA_CREATED, newNpcSettlement);
        allSettlements.Add(newNpcSettlement);
        if (saveDataArea.locationType != LOCATION_TYPE.DEMONIC_INTRUSION) {
            allNonPlayerSettlements.Add(newNpcSettlement);
        }
        return newNpcSettlement;
    }

    public BaseSettlement GetAreaByID(int id) {
        for (int i = 0; i < allSettlements.Count; i++) {
            BaseSettlement settlement = allSettlements[i];
            if (settlement.id == id) {
                return settlement;
            }
        }
        return null;
    }
    public BaseSettlement GetAreaByName(string name) {
        for (int i = 0; i < allSettlements.Count; i++) {
            BaseSettlement settlement = allSettlements[i];
            if (settlement.name.Equals(name)) {
                return settlement;
            }
        }
        return null;
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
    #endregion

    #region Location Structures
    public LocationStructure CreateNewStructureAt(ILocation location, STRUCTURE_TYPE type, BaseSettlement settlement = null) {
        LocationStructure createdStructure = null;
        switch (type) {
            case STRUCTURE_TYPE.DWELLING:
                createdStructure = new Dwelling(location);
                break;
            case STRUCTURE_TYPE.CITY_CENTER:
                createdStructure = new CityCenter(location);
                break;
            case STRUCTURE_TYPE.THE_PORTAL:
                createdStructure = new Inner_Maps.Location_Structures.ThePortal(location);
                break;
            case STRUCTURE_TYPE.THE_SPIRE:
                createdStructure = new Inner_Maps.Location_Structures.TheSpire(location);
                break;
            case STRUCTURE_TYPE.TORTURE_CHAMBER:
                createdStructure = new TortureChamber(location);
                break;
            case STRUCTURE_TYPE.THE_EYE:
                createdStructure = new Inner_Maps.Location_Structures.TheEye(location);
                break;
            case STRUCTURE_TYPE.GOADER:
                createdStructure = new Inner_Maps.Location_Structures.Goader(location);
                break;
            case STRUCTURE_TYPE.DEMONIC_PRISON:
                createdStructure = new DemonicPrison(location);
                break;
            case STRUCTURE_TYPE.THE_KENNEL:
                createdStructure = new Inner_Maps.Location_Structures.TheKennel(location);
                break;
            case STRUCTURE_TYPE.THE_CRYPT:
                createdStructure = new Inner_Maps.Location_Structures.TheCrypt(location);
                break;
            default:
                createdStructure = new LocationStructure(type, location);
                break;
        }
        location.AddStructure(createdStructure);
        settlement?.AddStructure(createdStructure);
        createdStructure.Initialize();
        return createdStructure;
    }
    public LocationStructure LoadStructureAt(ILocation location, SaveDataLocationStructure data) {
        LocationStructure createdStructure = data.Load(location);
        if (createdStructure != null) {
            location.AddStructure(createdStructure);
        }
        return createdStructure;
    }
    private void ConstructRaceStructureRequirements() {
        //humanSurvivalStructures = new STRUCTURE_TYPE[] { STRUCTURE_TYPE.WAREHOUSE };
        //humanUtilityStructures = new STRUCTURE_TYPE[] { STRUCTURE_TYPE.WAREHOUSE };
        //humanCombatStructures = new STRUCTURE_TYPE[] { STRUCTURE_TYPE.WAREHOUSE };
        //elfSurvivalStructures = new STRUCTURE_TYPE[] { STRUCTURE_TYPE.WAREHOUSE };
        //elfUtilityStructures = new STRUCTURE_TYPE[] { STRUCTURE_TYPE.WAREHOUSE };
        //elfCombatStructures = new STRUCTURE_TYPE[] { STRUCTURE_TYPE.WAREHOUSE };

        humanSurvivalStructures = new STRUCTURE_TYPE[] { STRUCTURE_TYPE.WAREHOUSE, STRUCTURE_TYPE.CEMETERY, STRUCTURE_TYPE.PRISON, STRUCTURE_TYPE.SMITHY, STRUCTURE_TYPE.BARRACKS, STRUCTURE_TYPE.APOTHECARY };
        humanUtilityStructures = new STRUCTURE_TYPE[] { STRUCTURE_TYPE.GRANARY, STRUCTURE_TYPE.MINER_CAMP, STRUCTURE_TYPE.INN };
        humanCombatStructures = new STRUCTURE_TYPE[] { STRUCTURE_TYPE.RAIDER_CAMP, STRUCTURE_TYPE.ASSASSIN_GUILD, STRUCTURE_TYPE.HUNTER_LODGE, STRUCTURE_TYPE.MAGE_QUARTERS };
        elfSurvivalStructures = new STRUCTURE_TYPE[] { STRUCTURE_TYPE.HUNTER_LODGE, STRUCTURE_TYPE.APOTHECARY, STRUCTURE_TYPE.MAGE_QUARTERS };
        elfUtilityStructures = new STRUCTURE_TYPE[] { STRUCTURE_TYPE.INN, STRUCTURE_TYPE.WAREHOUSE, STRUCTURE_TYPE.CEMETERY, STRUCTURE_TYPE.PRISON, STRUCTURE_TYPE.GRANARY, STRUCTURE_TYPE.MINER_CAMP };
        elfCombatStructures = new STRUCTURE_TYPE[] { STRUCTURE_TYPE.SMITHY, STRUCTURE_TYPE.BARRACKS, STRUCTURE_TYPE.RAIDER_CAMP, STRUCTURE_TYPE.ASSASSIN_GUILD };
    }
    public STRUCTURE_TYPE[] GetRaceStructureRequirements(RACE race, string category) {
        if(race == RACE.HUMANS) {
            if (category == "Survival") { return humanSurvivalStructures; }
            else if (category == "Utility") { return humanUtilityStructures; }
            else if (category == "Combat") { return humanCombatStructures; }
        } else if (race == RACE.ELVES) {
            if (category == "Survival") { return elfSurvivalStructures; }
            else if (category == "Utility") { return elfUtilityStructures; }
            else if (category == "Combat") { return elfCombatStructures; }
        }
        return null;
    }
    /// <summary>
    /// Place structures for settlement. This requires that the settlement has enough unoccupied hex tiles.
    /// NOTE: This function also creates the LocationStructure instances.
    /// </summary>
    /// <param name="settlement">The settlement to create structures for.</param>
    /// <param name="innerTileMap">The Inner map that the settlement is part of.</param>
    /// <param name="structureTypes">The structure types to create.</param>
    public IEnumerator PlaceBuiltStructuresForSettlement(BaseSettlement settlement, InnerTileMap innerTileMap, [NotNull]params STRUCTURE_TYPE[] structureTypes) {
        for (int i = 0; i < structureTypes.Length; i++) {
            STRUCTURE_TYPE structureType = structureTypes[i];
            HexTile chosenTile = settlement.GetRandomUnoccupiedHexTile();
            Assert.IsNotNull(chosenTile, $"There are no more unoccupied tiles to place structure {structureType.ToString()} for settlement {settlement.name}");
            PlaceBuiltStructureForSettlement(settlement, innerTileMap, chosenTile, structureType);
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
    public void PlaceBuiltStructureForSettlement(BaseSettlement settlement, InnerTileMap innerTileMap, HexTile tileLocation, STRUCTURE_TYPE structureType) {
        List<GameObject> choices =
            InnerMapManager.Instance.GetStructurePrefabsForStructure(structureType);
        GameObject chosenStructurePrefab = CollectionUtilities.GetRandomElement(choices);
        innerTileMap.PlaceBuiltStructureTemplateAt(chosenStructurePrefab, tileLocation, settlement);
    }
    #endregion

    #region Regions
    public TileFeature CreateTileFeature([NotNull] string featureName) {
        try {
            Debug.Assert(featureName != null, $"{nameof(featureName)} != null");
            return System.Activator.CreateInstance(System.Type.GetType(featureName)) as TileFeature;
        } catch {
            throw new System.Exception($"Cannot create region feature with name {featureName}");
        }
        
    }
    public Region GetRandomRegionWithFeature(string feature) {
        List<Region> choices = new List<Region>();
        for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
            Region region = GridMap.Instance.allRegions[i];
            if (region.HasTileWithFeature(feature)) {
                choices.Add(region);
            }
        }
        if (choices.Count > 0) {
            return CollectionUtilities.GetRandomElement(choices);
        }
        return null;
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
            List<Region> adjacent = currRegion.AdjacentRegions().Where(x => LandmarkManager.Instance.GetIslandOfRegion(x, allIslands) != this).ToList(); //get all adjacent regions, that does not belong to this island.
            if (adjacent.Count > 0) {
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