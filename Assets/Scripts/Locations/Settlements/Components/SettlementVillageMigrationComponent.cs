using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UtilityScripts;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;

public class SettlementVillageMigrationComponent : NPCSettlementComponent {
    public int villageMigrationMeter { get; private set; }
    public int perHourIncrement { get; private set; }
    public int longTermModifier { get; private set; }

    private const int MAX_MIGRATION_METER = 1000;
    private readonly GenericTextBookmarkable _migrationBookmark;
    private bool _isAlreadyBookmarked;

    public SettlementVillageMigrationComponent() {
        //villageMigrationMeter = 990;
        RandomizePerHourIncrement();
        _migrationBookmark = new GenericTextBookmarkable(GetMigrationMeterBookmarkName, () => BOOKMARK_TYPE.Text, 
            () => UIManager.Instance.ShowStructureInfo(owner.cityCenter), null, null, null);
    }
    public SettlementVillageMigrationComponent(SaveDataSettlementVillageMigrationComponent data) {
        villageMigrationMeter = data.villageMigrationMeter;
        perHourIncrement = data.perHourIncrement;
        longTermModifier = data.longTermModifier;
        _migrationBookmark = new GenericTextBookmarkable(GetMigrationMeterBookmarkName, () => BOOKMARK_TYPE.Text, 
            () => UIManager.Instance.ShowStructureInfo(owner.cityCenter), null, null, null);
    }

    #region Loading
    public void LoadReferences(SaveDataNPCSettlement saveDataNpcSettlement) {
        CheckIfBookmarkShouldBeAdded(GetNormalizedMigrationMeterValue());
    }
    #endregion

    #region Bookmark
    private void CheckIfBookmarkShouldBeAdded(float p_migrationMeterNormalized) {
        if (p_migrationMeterNormalized >= 0.9f && !_isAlreadyBookmarked) {
            _isAlreadyBookmarked = true;
            PlayerManager.Instance.player.bookmarkComponent.AddBookmark(_migrationBookmark, BOOKMARK_CATEGORY.Major_Events);
        }
    }
    private void CheckIfBookmarkShouldBeRemoved(float p_migrationMeterNormalized) {
        if (p_migrationMeterNormalized < 0.9f && _isAlreadyBookmarked) {
            _isAlreadyBookmarked = false;
            PlayerManager.Instance.player.bookmarkComponent.RemoveBookmark(_migrationBookmark, BOOKMARK_CATEGORY.Major_Events);
        }
    }
    #endregion
    
    #region Listeners
    public void OnHourStarted() {
        if (!IsMigrationEventAllowed()) {
            //If village does not allow villager migration, reset meter back to zero
            if(villageMigrationMeter > 0) {
                SetVillageMigrationMeter(0);
            }
            return;
        }
        IncreaseVillageMigrationMeter(GetPerHourMigrationRate());
    }
    public void OnSettlementTypeChanged() {
        RandomizePerHourIncrement();
    }
    public void OnStructureBuilt(LocationStructure structure) {
        if (!IsMigrationEventAllowed()) {
            return;
        }
        if (structure is Dwelling) {
            IncreaseVillageMigrationMeter(GameUtilities.RandomBetweenTwoNumbers(20, 30));
        } else {
            IncreaseVillageMigrationMeter(GameUtilities.RandomBetweenTwoNumbers(30, 40));
        }
    }
    public void OnFinishedQuest(PartyQuest quest) {
        if(quest.madeInLocation == owner) {
            if (quest.isSuccessful) {
                if (!IsMigrationEventAllowed()) {
                    return;
                }
                IncreaseVillageMigrationMeter(GameUtilities.RandomBetweenTwoNumbers(20, 30));
            }
        }
    }
    #endregion

    #region Migration Meter
    public void ForceRandomizePerHourIncrement() {
        RandomizePerHourIncrement();
    }
    private void RandomizePerHourIncrement() {
        // perHourIncrement = GameUtilities.RandomBetweenTwoNumbers(2, 5);
        List<Faction> humanAndElvenFactions =  DatabaseManager.Instance.factionDatabase.GetFactionsWithFactionType(FACTION_TYPE.Human_Empire, FACTION_TYPE.Elven_Kingdom);
        if (humanAndElvenFactions != null) {
            int humanAndElevenVillagesCount = 0;
            for (int i = 0; i < humanAndElvenFactions.Count; i++) {
                Faction faction = humanAndElvenFactions[i];
                for (int j = 0; j < faction.ownedSettlements.Count; j++) {
                    BaseSettlement settlement = faction.ownedSettlements[j];
                    if (settlement.locationType == LOCATION_TYPE.VILLAGE) {
                        humanAndElevenVillagesCount++;
                    }
                }
            }
            if (humanAndElevenVillagesCount <= 1) {
                perHourIncrement = GameUtilities.RandomBetweenTwoNumbers(4, 9);
            } else if (humanAndElevenVillagesCount >= 2 && humanAndElevenVillagesCount <= 3) {
                perHourIncrement = GameUtilities.RandomBetweenTwoNumbers(2, 7);
            } else if (humanAndElevenVillagesCount >= 4 && humanAndElevenVillagesCount <= 5) {
                perHourIncrement = GameUtilities.RandomBetweenTwoNumbers(1, 5);
            } else {
                perHourIncrement = GameUtilities.RandomBetweenTwoNumbers(1, 4);
            }
        } else {
            perHourIncrement = GameUtilities.RandomBetweenTwoNumbers(4, 9);
        }
    }
    private int GetPerHourMigrationRate() {
        if (IsMigrationEventAllowed()) {
            int migrationMeterModification = GetAdditionalMigrationMeterRatePerHour();
            int perHour = perHourIncrement + migrationMeterModification + longTermModifier;
            if(perHour < 1) {
                perHour = 1;
            }
            return perHour;    
        }
        return 0;
    }
    public void AdjustLongTermModifier(int amount) {
        longTermModifier += amount;
    }
    public void ResetLongTermModifier() {
        longTermModifier = 0;
    }
    private int ApplyVillageMigrationModifier(int p_amount) {
        if (WorldSettings.Instance.worldSettingsData.villageSettings.migrationSpeed == MIGRATION_SPEED.Slow) {
            //if migration speed is slow then half gained amount
            p_amount = Mathf.CeilToInt(p_amount / 2f);
        }
        return p_amount;
    }
    public void IncreaseVillageMigrationMeter(int amount) {
        amount = ApplyVillageMigrationModifier(amount);
        villageMigrationMeter += amount;
        villageMigrationMeter = Mathf.Clamp(villageMigrationMeter, 0, MAX_MIGRATION_METER);
        CheckIfBookmarkShouldBeAdded(GetNormalizedMigrationMeterValue());
        if (_isAlreadyBookmarked) {
            _migrationBookmark.bookmarkEventDispatcher.ExecuteBookmarkChangedNameOrElementsEvent(_migrationBookmark);
        }
        CheckIfMigrationMeterIsFull();
    }
    public void ReduceVillageMigrationMeter(int amount) {
        villageMigrationMeter -= amount;
        villageMigrationMeter = Mathf.Clamp(villageMigrationMeter, 0, MAX_MIGRATION_METER);
        if (_isAlreadyBookmarked) {
            _migrationBookmark.bookmarkEventDispatcher.ExecuteBookmarkChangedNameOrElementsEvent(_migrationBookmark);
        }
        CheckIfBookmarkShouldBeRemoved(GetNormalizedMigrationMeterValue());
    }
    public void SetVillageMigrationMeter(int amount) {
        villageMigrationMeter = amount;
        villageMigrationMeter = Mathf.Clamp(villageMigrationMeter, 0, MAX_MIGRATION_METER);
        
        //check both cases since set can be any value
        float normalizedMigrationMeter = GetNormalizedMigrationMeterValue();
        CheckIfBookmarkShouldBeRemoved(normalizedMigrationMeter);
        CheckIfBookmarkShouldBeAdded(normalizedMigrationMeter);
        if (_isAlreadyBookmarked) {
            _migrationBookmark.bookmarkEventDispatcher.ExecuteBookmarkChangedNameOrElementsEvent(_migrationBookmark);
        }
        
        CheckIfMigrationMeterIsFull();
    }
    private void CheckIfMigrationMeterIsFull() {
        if (villageMigrationMeter == MAX_MIGRATION_METER) {
            OnFullVillageMigrationMeter();
        }
    }
    private void OnFullVillageMigrationMeter() {
        SetVillageMigrationMeter(0);
        VillageMigrationEvent();
    }
    public float GetNormalizedMigrationMeterValue() {
        return villageMigrationMeter / (float) MAX_MIGRATION_METER;
    }
    public string GetMigrationMeterValueInText() {
        return $"{villageMigrationMeter.ToString()}/{MAX_MIGRATION_METER.ToString()}";
    }
    public string GetHoverTextOfMigrationMeter() {
        string text = $"Current Value: {GetMigrationMeterValueInText()}";
        text += $"\nIncrease Rate Per Hour: {ApplyVillageMigrationModifier(GetPerHourMigrationRate()).ToString()}";
        if (!IsMigrationEventAllowed()) {
            if (WorldSettings.Instance.worldSettingsData.villageSettings.migrationSpeed == MIGRATION_SPEED.None) {
                text += $"\n{UtilityScripts.Utilities.ColorizeInvalidText("Player has turned off Villager Migration in World Settings.")}";
            } else if (WorldSettings.Instance.worldSettingsData.villageSettings.disabledFactionMigrations.Count > 0) {
                text += $"\n{UtilityScripts.Utilities.ColorizeInvalidText($"Player has turned off Villager Migration for {WorldSettings.Instance.worldSettingsData.villageSettings.disabledFactionMigrations.ComafyList()} in World Settings.")}";
            } else {
                text += $"\n{UtilityScripts.Utilities.ColorizeInvalidText("Only Human and Elven Villages can trigger migration!")}";        
            }
        }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        text += $"\nBase Per Hour: {perHourIncrement.ToString()}";
        text += $"\nLong Term Modifier: {longTermModifier.ToString()}";
        text += $"\nFaction Type Modification: {GetAdditionalMigrationMeterRatePerHour().ToString()}";
#endif
        return text;
    }
    private int GetAdditionalMigrationMeterRatePerHour() {
        return owner.owner?.factionType.GetAdditionalMigrationMeterGain(owner) ?? 0;
    }
    #endregion

    #region Migration
    public bool IsMigrationEventAllowed() {
        return WorldSettings.Instance.worldSettingsData.villageSettings.migrationSpeed != MIGRATION_SPEED.None && 
               owner.locationType == LOCATION_TYPE.VILLAGE &&
               owner.owner != null && owner.residents.Count > 0 && owner.owner.isMajorNonPlayer &&
               WorldSettings.Instance.worldSettingsData.villageSettings.IsMigrationAllowedForFaction(owner.owner.factionType.type) &&  
               (owner.owner.factionType.type == FACTION_TYPE.Human_Empire || owner.owner.factionType.type == FACTION_TYPE.Elven_Kingdom);
    }
    public void InduceMigrationEvent() {
        if (owner.owner != null) {
            VillageMigrationEvent();
        } else {
            VillageMigrationEventOnEmptySettlement();
        }
    }
    private void VillageMigrationEvent() {
        string debugLog = string.Empty;
#if DEBUG_LOG
        debugLog = $"{GameManager.Instance.TodayLogString()}Village Migration Event for {owner.name} is triggered";
#endif
        if (IsMigrationEventAllowed()) {
            List<PreCharacterData> unspawnedCharacters = RuinarchListPool<PreCharacterData>.Claim();
            DatabaseManager.Instance.familyTreeDatabase.ForcePopulateAllUnspawnedCharactersThatFitFaction(unspawnedCharacters, owner.owner.race, owner.owner);
            if (unspawnedCharacters.Count > 0) {
                Migrate(unspawnedCharacters, owner.owner, ref debugLog);
            } else {
#if DEBUG_LOG
                debugLog += $"\nNo unspawned character to spawn for {owner.owner.race.ToString()}/{owner.owner.name}";
#endif
            }
            RuinarchListPool<PreCharacterData>.Release(unspawnedCharacters);
        }
#if DEBUG_LOG
        Debug.Log(debugLog);
#endif
    }
    private void VillageMigrationEventOnEmptySettlement() {
        string debugLog = string.Empty;
#if DEBUG_LOG
        debugLog = $"{GameManager.Instance.TodayLogString()}Village Migration Event for Empty Settlement is triggered";
#endif
        List<PreCharacterData> unspawnedCharacters = RuinarchListPool<PreCharacterData>.Claim();
        RACE migrationRace = RACE.HUMANS;
        if (GameUtilities.RollChance(50)) {
            migrationRace = RACE.ELVES;
        }
        DatabaseManager.Instance.familyTreeDatabase.ForcePopulateAllUnspawnedCharactersThatFitRace(unspawnedCharacters, migrationRace);
        if (unspawnedCharacters.Count > 0) {
            Faction newFaction = FactionManager.Instance.CreateNewFaction(FactionManager.Instance.GetFactionTypeForRace(migrationRace), race: migrationRace);
            newFaction.factionType.SetAsDefault();
            LandmarkManager.Instance.OwnSettlement(newFaction, owner);
            Migrate(unspawnedCharacters, newFaction, ref debugLog);
            Messenger.Broadcast(FactionSignals.FORCE_FACTION_UI_RELOAD);
        } else {
#if DEBUG_LOG
            debugLog += $"\nNo unspawned character to spawn for {migrationRace}/Vagrants";
#endif
        }
        RuinarchListPool<PreCharacterData>.Release(unspawnedCharacters);
#if DEBUG_LOG
        Debug.Log(debugLog);
#endif
    }
    private void Migrate(List<PreCharacterData> unspawnedCharacters, Faction faction, ref string debugLog) {
        AdjustLongTermModifier(-1);
        int randomAmount = UnityEngine.Random.Range(3, 6);
        // List<LocationGridTile> edgeTileChoices = null;
        // for (int i = 0; i < owner.region.innerMap.allEdgeTiles.Count; i++) {
        //     LocationGridTile tile = owner.region.innerMap.allEdgeTiles[i];
        //     //Area connectedAreaOrNearestArea = tile.area;
        //     if (!tile.corruptionComponent.isCorrupted && tile.IsPassable()) { //&& !connectedAreaOrNearestArea.isCorrupted
        //         if (edgeTileChoices == null) { edgeTileChoices = new List<LocationGridTile>(); }
        //         edgeTileChoices.Add(tile);
        //     }
        // }
        // if (edgeTileChoices == null) {
        //     edgeTileChoices = owner.region.innerMap.allEdgeTiles;
        // }
        //
        // LocationGridTile edgeTile = CollectionUtilities.GetRandomElement(edgeTileChoices);
        
        List<LocationGridTile> edgeTileChoices = RuinarchListPool<LocationGridTile>.Claim();
        for (int i = 0; i < owner.occupiedVillageSpot.migrationSpawningArea.gridTileComponent.borderTiles.Count; i++) {
            LocationGridTile tile = owner.occupiedVillageSpot.migrationSpawningArea.gridTileComponent.borderTiles[i];
            if (tile.IsAtEdgeOfMap() && !tile.corruptionComponent.isCorrupted && tile.IsPassable() && !tile.structure.isInterior) { //&& !connectedAreaOrNearestArea.isCorrupted
                edgeTileChoices.Add(tile);
            }
        }
        if (edgeTileChoices.Count <= 0) {
            if (owner.occupiedVillageSpot.migrationSpawningArea.gridTileComponent.passableTiles.Count > 0) {
                edgeTileChoices.AddRange(owner.occupiedVillageSpot.migrationSpawningArea.gridTileComponent.passableTiles);    
            } else {
                edgeTileChoices.AddRange(owner.occupiedVillageSpot.migrationSpawningArea.gridTileComponent.gridTiles);
            }
        }

        LocationGridTile edgeTile = CollectionUtilities.GetRandomElement(edgeTileChoices);
        RuinarchListPool<LocationGridTile>.Release(edgeTileChoices);
        
#if DEBUG_LOG
        debugLog += $"\nWill spawn {randomAmount} characters at {edgeTile}";
#endif
        for (int i = 0; i < randomAmount; i++) {
            if (unspawnedCharacters.Count <= 0) { break; }
            PreCharacterData characterToSpawn = CollectionUtilities.GetRandomElement(unspawnedCharacters);
            characterToSpawn.hasBeenSpawned = true;
            unspawnedCharacters.Remove(characterToSpawn);

            string classToCreate = "Farmer";
            // if (owner.classComponent.GetCurrentResidentClassAmount("Peasant") > 0) {
            //     //village already has at least 1 peasant
            //     classToCreate = GameUtilities.RollChance(90) ? CollectionUtilities.GetRandomElement(owner.owner.factionType.combatantClasses) : "Noble";
            // } else {
            //     if (i == 0) {
            //         //one of the migrants should always be a peasant
            //         classToCreate = "Peasant";
            //     } else {
            //         classToCreate = GameUtilities.RollChance(90) ? CollectionUtilities.GetRandomElement(owner.owner.factionType.combatantClasses) : "Noble";
            //     }
            // }

            Character newCharacter = CharacterManager.Instance.CreateNewCharacter(characterToSpawn, classToCreate, faction, owner);
            newCharacter.classComponent.RandomizeCurrentClassBasedOnAbleClasses();
            RelationshipManager.Instance.ApplyPreGeneratedRelationships(WorldConfigManager.Instance.mapGenerationData, characterToSpawn, newCharacter);
            newCharacter.CreateRandomInitialTraits();
            if (WorldSettings.Instance.worldSettingsData.villageSettings.blessedMigrants) {
                newCharacter.traitContainer.AddTrait(newCharacter, "Blessed");
            }
            newCharacter.CreateMarker();
            newCharacter.InitialCharacterPlacement(edgeTile);
            // newCharacter.MigrateHomeStructureTo(null, affectSettlement: false);
            // //set previous home structure to null since migrants homes are set automatically on character creation,
            // //and is only set to null above, causing the previous dwelling data to be set, which we don't want since the migrants cannot
            // //buy that specific home.
            // newCharacter.previousCharacterDataComponent.SetPreviousHomeStructure(null); 
#if DEBUG_LOG
            debugLog += $"\nSpawned new character {newCharacter.name} at {edgeTile}";
#endif
            //removed set home, since new migrants should go through the process of buying a home
            // newCharacter.interruptComponent.TriggerInterrupt(INTERRUPT.Set_Home, null);
            newCharacter.jobComponent.PlanReturnToVillageCenter(JOB_TYPE.RETURN_HOME_URGENT); //this will make the villager go to its home settlement
            Messenger.Broadcast(WorldEventSignals.NEW_VILLAGER_ARRIVED, newCharacter);

            Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "WorldEvents", "VillagerMigration", "new_villager", providedTags: LOG_TAG.Major);
            log.AddToFillers(newCharacter, newCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(newCharacter.homeRegion, newCharacter.homeRegion.name, LOG_IDENTIFIER.LANDMARK_1);
            log.AddLogToDatabase();
            PlayerManager.Instance.player.ShowNotificationFromPlayer(log, true);
        }
    }
    private string GetMigrationMeterBookmarkName() {
        return $"<b>{owner.name}</b> Incoming Migrants: {Mathf.FloorToInt(GetNormalizedMigrationMeterValue() * 100f).ToString()}%";
    }
#endregion
}

[System.Serializable]
public class SaveDataSettlementVillageMigrationComponent : SaveData<SettlementVillageMigrationComponent> {
    public int villageMigrationMeter;
    public int perHourIncrement;
    public int longTermModifier;

#region Overrides
    public override void Save(SettlementVillageMigrationComponent data) {
        villageMigrationMeter = data.villageMigrationMeter;
        perHourIncrement = data.perHourIncrement;
        longTermModifier = data.longTermModifier;
    }

    public override SettlementVillageMigrationComponent Load() {
        SettlementVillageMigrationComponent component = new SettlementVillageMigrationComponent(this);
        return component;
    }
#endregion
}
