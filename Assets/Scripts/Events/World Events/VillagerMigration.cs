using System;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;
namespace Events.World_Events {
    public class VillagerMigration : WorldEvent {

        public VillagerMigration() { }
        public VillagerMigration(SaveDataVillagerMigration data) { }
        
        public override void InitializeEvent() {
            Messenger.AddListener(Signals.HOUR_STARTED, OnHourStarted);
        }
        
        #region Listeners
        private void OnHourStarted() {
            var hoursBasedOnTicks = GameManager.Instance.GetHoursBasedOnTicks(GameManager.Instance.Today().tick);
            if (hoursBasedOnTicks == 9 || hoursBasedOnTicks == 13) {
                string debugLog = $"{GameManager.Instance.TodayLogString()}Checking for villager migration:";
                NPCSettlement randomSettlement = LandmarkManager.Instance.GetRandomActiveVillageSettlement();
                if(randomSettlement != null) {
                    int unoccupiedDwellings = GetUnoccupiedDwellingCount(randomSettlement);
                    debugLog = $"{debugLog}\n{randomSettlement.name} was chosen. It has {unoccupiedDwellings.ToString()} unoccupied dwellings.";
                    int baseChance = 5;
                    if (GameManager.Instance.Today().day >= 15) {
                        baseChance = 3;
                    }
                    int chance = baseChance * unoccupiedDwellings;
                    //cap chance to 20% before day 15 and 10% after day 15
                    if (GameManager.Instance.Today().day < 15) {
                        chance = Mathf.Min(chance, 20);
                    } else {
                        chance = Mathf.Min(chance, 10);
                    }
                    if (GameUtilities.RollChance(chance, ref debugLog)) { //7
                        int availableCapacity = unoccupiedDwellings; //to get available capacity, get all unoccupied dwellings multiplied by the maximum number of residents per dwelling (2)
                        int randomAmount = UnityEngine.Random.Range(1, 4);
                        randomAmount = Mathf.Min(randomAmount, availableCapacity);
                        List<PreCharacterData> unspawnedCharacters = DatabaseManager.Instance.familyTreeDatabase.ForceGetAllUnspawnedCharactersThatFitFaction(randomSettlement.owner.race, randomSettlement.owner);
                        LocationGridTile edgeTile = CollectionUtilities.GetRandomElement(randomSettlement.region.innerMap.allEdgeTiles);
                        debugLog = $"{debugLog}\nWill spawn {randomAmount.ToString()} characters at {edgeTile}";
                        for (int i = 0; i < randomAmount; i++) {
                            if (unspawnedCharacters.Count <= 0) { break; }
                            PreCharacterData characterToSpawn = CollectionUtilities.GetRandomElement(unspawnedCharacters);
                            characterToSpawn.hasBeenSpawned = true;
                            unspawnedCharacters.Remove(characterToSpawn);

                            string classToCreate;
                            if (i == 0) {
                                //always ensure that first villager is a civilian type 
                                //https://trello.com/c/I53VfSsC/2688-one-of-the-migrants-should-always-be-a-non-combatant-the-rest-should-be-combatants
                                classToCreate = CollectionUtilities.GetRandomElement(randomSettlement.owner.factionType.civilianClasses);
                            } else {
                                classToCreate = CollectionUtilities.GetRandomElement(randomSettlement.owner.factionType.combatantClasses);
                            }
                            // if (GameUtilities.RollChance(50)) {
                            //     classToCreate = CollectionUtilities.GetRandomElement(randomSettlement.owner.factionType.combatantClasses);
                            // } else {
                            //     classToCreate = CollectionUtilities.GetRandomElement(randomSettlement.owner.factionType.civilianClasses);
                            // }
                            Character newCharacter = CharacterManager.Instance.CreateNewCharacter(characterToSpawn, classToCreate, randomSettlement.owner, randomSettlement);
                            RelationshipManager.Instance.ApplyPreGeneratedRelationships(WorldConfigManager.Instance.mapGenerationData, characterToSpawn, newCharacter);
                            newCharacter.CreateRandomInitialTraits();
                            newCharacter.CreateMarker();
                            newCharacter.InitialCharacterPlacement(edgeTile);
                            newCharacter.MigrateHomeStructureTo(null, affectSettlement: false);
                            Debug.Log($"Spawned new villager {newCharacter.name} at {edgeTile}");
                            newCharacter.interruptComponent.TriggerInterrupt(INTERRUPT.Set_Home, null);
                            newCharacter.jobComponent.PlanReturnHomeUrgent();
                            Messenger.Broadcast(Signals.NEW_VILLAGER_ARRIVED, newCharacter);

                            debugLog = $"{debugLog}\nNew character {newCharacter.name} was spawned.";

                            Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "WorldEvents", "VillagerMigration", "new_villager", providedTags: LOG_TAG.Major);
                            log.AddToFillers(newCharacter, newCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                            log.AddToFillers(newCharacter.homeRegion, newCharacter.homeRegion.name, LOG_IDENTIFIER.LANDMARK_1);
                            log.AddLogToDatabase();
                            PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
                        }
                    }
                }
                Debug.Log(debugLog);
            }
        }
        #endregion

        private int GetUnoccupiedDwellingCount(NPCSettlement npcSettlement) {
            int count = 0;
            List<LocationStructure> dwellings = npcSettlement.GetStructuresOfType(STRUCTURE_TYPE.DWELLING);
            if(dwellings != null && dwellings.Count > 0) {
                for (int i = 0; i < dwellings.Count; i++) {
                    LocationStructure dwelling = dwellings[i];
                    if (dwelling.residents.Count <= 0) {
                        count++;
                    }
                }
            }
            return count;
        }
        public override SaveDataWorldEvent Save() {
            SaveDataVillagerMigration save = new SaveDataVillagerMigration();
            save.Save(this);
            return save;
        }
    }

    public class SaveDataVillagerMigration : SaveDataWorldEvent {
        public override WorldEvent Load() {
            return new VillagerMigration(this);
        }
    }
}