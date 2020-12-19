﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class SettlementVillageMigrationComponent : NPCSettlementComponent {
    public int villageMigrationMeter { get; private set; }
    public int perHourIncrement { get; private set; }

    private const int MAX_MIGRATION_METER = 1000;

    public SettlementVillageMigrationComponent() {
        //villageMigrationMeter = 990;
        RandomizePerHourIncrement();
    }
    public SettlementVillageMigrationComponent(SaveDataSettlementVillageMigrationComponent data) {
        villageMigrationMeter = data.villageMigrationMeter;
        perHourIncrement = data.perHourIncrement;
    }

    #region Listeners
    public void OnHourStarted(NPCSettlement p_settlement) {
        if (!IsMigrationEventAllowed()) {
            //If village does not allow villager migration, reset meter back to zero
            if(villageMigrationMeter > 0) {
                SetVillageMigrationMeter(0);
            }
            return;
        }
        int migrationMeterModification = p_settlement.owner?.factionType.GetAdditionalMigrationMeterGain(p_settlement) ?? 0;
        AdjustVillageMigarationMeter(perHourIncrement + migrationMeterModification);
    }
    public void OnSettlementTypeChanged() {
        RandomizePerHourIncrement();
    }
    public void OnStructureBuilt(LocationStructure structure) {
        if (!IsMigrationEventAllowed()) {
            return;
        }
        if (structure is Dwelling) {
            AdjustVillageMigarationMeter(GameUtilities.RandomBetweenTwoNumbers(40, 60));
        } else {
            AdjustVillageMigarationMeter(GameUtilities.RandomBetweenTwoNumbers(60, 80));
        }
    }
    public void OnFinishedQuest(PartyQuest quest) {
        if(quest.madeInLocation == owner) {
            if (quest.isSuccessful) {
                if (!IsMigrationEventAllowed()) {
                    return;
                }
                AdjustVillageMigarationMeter(GameUtilities.RandomBetweenTwoNumbers(80, 120));
            }
        }
    }
    #endregion

    #region Migration Meter
    private void RandomizePerHourIncrement() {
        perHourIncrement = GameUtilities.RandomBetweenTwoNumbers(3, 9);
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
    #endregion

    #region Migration
    public bool IsMigrationEventAllowed() {
        return owner.owner != null && owner.residents.Count > 0 && owner.owner.isMajorNonPlayer && (owner.owner.factionType.type == FACTION_TYPE.Human_Empire || owner.owner.factionType.type == FACTION_TYPE.Elven_Kingdom);
    }
    private void VillageMigrationEvent() {
        string debugLog = $"{GameManager.Instance.TodayLogString()}Village Migration Event for {owner.name} is triggered";

        if (IsMigrationEventAllowed()) {
            int randomAmount = UnityEngine.Random.Range(2, 6);
            List<PreCharacterData> unspawnedCharacters = DatabaseManager.Instance.familyTreeDatabase.ForceGetAllUnspawnedCharactersThatFitFaction(owner.owner.race, owner.owner);
            if (unspawnedCharacters.Count > 0) {
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

    #region Overrides
    public override void Save(SettlementVillageMigrationComponent data) {
        villageMigrationMeter = data.villageMigrationMeter;
        perHourIncrement = data.perHourIncrement;
    }

    public override SettlementVillageMigrationComponent Load() {
        SettlementVillageMigrationComponent component = new SettlementVillageMigrationComponent(this);
        return component;
    }
    #endregion
}
