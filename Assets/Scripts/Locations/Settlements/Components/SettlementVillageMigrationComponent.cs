using System.Collections;
using System.Collections.Generic;
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

    public SettlementVillageMigrationComponent() {
        //villageMigrationMeter = 990;
        RandomizePerHourIncrement();
    }
    public SettlementVillageMigrationComponent(SaveDataSettlementVillageMigrationComponent data) {
        villageMigrationMeter = data.villageMigrationMeter;
        perHourIncrement = data.perHourIncrement;
        longTermModifier = data.longTermModifier;
    }

    #region Listeners
    public void OnHourStarted() {
        if (!IsMigrationEventAllowed()) {
            //If village does not allow villager migration, reset meter back to zero
            if(villageMigrationMeter > 0) {
                SetVillageMigrationMeter(0);
            }
            return;
        }
        AdjustVillageMigarationMeter(GetPerHourMigrationRate());
    }
    public void OnSettlementTypeChanged() {
        RandomizePerHourIncrement();
    }
    public void OnStructureBuilt(LocationStructure structure) {
        if (!IsMigrationEventAllowed()) {
            return;
        }
        if (structure is Dwelling) {
            AdjustVillageMigarationMeter(GameUtilities.RandomBetweenTwoNumbers(20, 30));
        } else {
            AdjustVillageMigarationMeter(GameUtilities.RandomBetweenTwoNumbers(30, 40));
        }
    }
    public void OnFinishedQuest(PartyQuest quest) {
        if(quest.madeInLocation == owner) {
            if (quest.isSuccessful) {
                if (!IsMigrationEventAllowed()) {
                    return;
                }
                AdjustVillageMigarationMeter(GameUtilities.RandomBetweenTwoNumbers(20, 30));
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
    public int GetPerHourMigrationRate() {
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
    public void AdjustVillageMigarationMeter(int amount) {
        villageMigrationMeter += amount;
        villageMigrationMeter = Mathf.Clamp(villageMigrationMeter, 0, MAX_MIGRATION_METER);
        CheckIfMigrationMeterIsFull();
    }
    public void SetVillageMigrationMeter(int amount) {
        villageMigrationMeter = amount;
        villageMigrationMeter = Mathf.Clamp(villageMigrationMeter, 0, MAX_MIGRATION_METER);
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
        return $"{villageMigrationMeter}/{MAX_MIGRATION_METER}";
    }
    public string GetHoverTextOfMigrationMeter() {
        string text = $"Current Value: {GetMigrationMeterValueInText()}";
        text += $"\nIncrease Rate Per Hour: {GetPerHourMigrationRate()}";
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        text += $"\nBase Per Hour: {perHourIncrement}";
        text += $"\nLong Term Modifier: {longTermModifier}";
        text += $"\nFaction Type Modification: {GetAdditionalMigrationMeterRatePerHour()}";
#endif
        return text;
    }
    private int GetAdditionalMigrationMeterRatePerHour() {
        return owner.owner?.factionType.GetAdditionalMigrationMeterGain(owner) ?? 0;
    }
    #endregion

    #region Migration
    public bool IsMigrationEventAllowed() {
        return owner.owner != null && owner.residents.Count > 0 && owner.owner.isMajorNonPlayer && (owner.owner.factionType.type == FACTION_TYPE.Human_Empire || owner.owner.factionType.type == FACTION_TYPE.Elven_Kingdom);
    }
    private void VillageMigrationEvent() {
        string debugLog = $"{GameManager.Instance.TodayLogString()}Village Migration Event for {owner.name} is triggered";

        if (IsMigrationEventAllowed()) {
            List<PreCharacterData> unspawnedCharacters = DatabaseManager.Instance.familyTreeDatabase.ForceGetAllUnspawnedCharactersThatFitFaction(owner.owner.race, owner.owner);
            if (unspawnedCharacters.Count > 0) {
                AdjustLongTermModifier(-1);
                int randomAmount = UnityEngine.Random.Range(2, 6);
                LocationGridTile edgeTile = CollectionUtilities.GetRandomElement(owner.region.innerMap.allEdgeTiles);
                debugLog += $"\nWill spawn {randomAmount.ToString()} characters at {edgeTile}";
                for (int i = 0; i < randomAmount; i++) {
                    if (unspawnedCharacters.Count <= 0) { break; }
                    PreCharacterData characterToSpawn = CollectionUtilities.GetRandomElement(unspawnedCharacters);
                    characterToSpawn.hasBeenSpawned = true;
                    unspawnedCharacters.Remove(characterToSpawn);

                    string classToCreate;
                    if (owner.settlementClassTracker.GetCurrentResidentClassAmount("Peasant") > 0) {
                        //village already has at least 1 peasant
                        classToCreate = GameUtilities.RollChance(90) ? CollectionUtilities.GetRandomElement(owner.owner.factionType.combatantClasses) : "Noble";
                    } else {
                        if (i == 0) {
                            //one of the migrants should always be a peasant
                            classToCreate = "Peasant";
                        } else {
                            classToCreate = GameUtilities.RollChance(90) ? CollectionUtilities.GetRandomElement(owner.owner.factionType.combatantClasses) : "Noble";
                        }    
                    }
                    
                    Character newCharacter = CharacterManager.Instance.CreateNewCharacter(characterToSpawn, classToCreate, owner.owner, owner);
                    RelationshipManager.Instance.ApplyPreGeneratedRelationships(WorldConfigManager.Instance.mapGenerationData, characterToSpawn, newCharacter);
                    newCharacter.CreateRandomInitialTraits();
                    newCharacter.CreateMarker();
                    newCharacter.InitialCharacterPlacement(edgeTile);
                    newCharacter.MigrateHomeStructureTo(null, affectSettlement: false);
                    debugLog += $"\nSpawned new character {newCharacter.name} at {edgeTile}";
                    newCharacter.interruptComponent.TriggerInterrupt(INTERRUPT.Set_Home, null);
                    newCharacter.jobComponent.PlanReturnHomeUrgent();
                    Messenger.Broadcast(WorldEventSignals.NEW_VILLAGER_ARRIVED, newCharacter);

                    Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "WorldEvents", "VillagerMigration", "new_villager", providedTags: LOG_TAG.Major);
                    log.AddToFillers(newCharacter, newCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    log.AddToFillers(newCharacter.homeRegion, newCharacter.homeRegion.name, LOG_IDENTIFIER.LANDMARK_1);
                    log.AddLogToDatabase();
                    PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
                }
            } else {
                debugLog += $"\nNo unspawned character to spawn for {owner.owner.race.ToString()}/{owner.owner.name}";
            }
        }
        Debug.Log(debugLog);
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
