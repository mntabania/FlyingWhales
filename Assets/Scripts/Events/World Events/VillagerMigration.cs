using System;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;
namespace Events.World_Events {
    public class VillagerMigration : WorldEvent {
        
        public override void InitializeEvent() {
            Messenger.AddListener(Signals.HOUR_STARTED, OnHourStarted);
        }
        
        #region Listeners
        private void OnHourStarted() {
            if (GameManager.Instance.GetHoursBasedOnTicks(GameManager.Instance.Today().tick) == 9) {
                if (GameUtilities.RollChance(7)) { //7
                    NPCSettlement randomSettlement = LandmarkManager.Instance.GetRandomActiveVillageSettlement();
                    int unoccupiedDwellings = GetUnoccupiedDwellingCount(randomSettlement);
                    if (unoccupiedDwellings > 0) {
                        int availableCapacity = unoccupiedDwellings; //to get available capacity, get all unoccupied dwellings multiplied by the maximum number of residents per dwelling (2)
                        int randomAmount = UnityEngine.Random.Range(1, 4);
                        randomAmount = Mathf.Min(randomAmount, availableCapacity);
                        List<PreCharacterData> unspawnedCharacters = DatabaseManager.Instance.familyTreeDatabase.ForceGetAllUnspawnedCharacters(randomSettlement.owner.race);
                        LocationGridTile edgeTile = CollectionUtilities.GetRandomElement(randomSettlement.region.innerMap.allEdgeTiles);
                        for (int i = 0; i < randomAmount; i++) {
                            if (unspawnedCharacters.Count == 0) { break; }
                            PreCharacterData characterToSpawn = CollectionUtilities.GetRandomElement(unspawnedCharacters);
                            characterToSpawn.hasBeenSpawned = true;
                            unspawnedCharacters.Remove(characterToSpawn);
                            
                            Character newCharacter = CharacterManager.Instance.CreateNewCharacter(characterToSpawn, randomSettlement.classManager.GetCurrentClassToCreate(), randomSettlement.owner, randomSettlement);
                            RelationshipManager.Instance.ApplyPreGeneratedRelationships(WorldConfigManager.Instance.mapGenerationData, characterToSpawn, newCharacter);
                            newCharacter.CreateMarker();
                            newCharacter.InitialCharacterPlacement(edgeTile);
                            newCharacter.MigrateHomeStructureTo(null, affectSettlement: false);
                            Debug.Log($"Spawned new villager {newCharacter.name} at {edgeTile}");
                            newCharacter.interruptComponent.TriggerInterrupt(INTERRUPT.Set_Home, null);
                            newCharacter.jobComponent.PlanReturnHomeUrgent();
                            Messenger.Broadcast(Signals.NEW_VILLAGER_ARRIVED, newCharacter);
                            
                            Log log = new Log(GameManager.Instance.Today(), "WorldEvents", "VillagerMigration", "new_villager", providedTags: LOG_TAG.Life_Changes);
                            log.AddToFillers(newCharacter, newCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                            log.AddToFillers(newCharacter.homeRegion, newCharacter.homeRegion.name, LOG_IDENTIFIER.LANDMARK_1);
                            log.AddLogToDatabase();
                            PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
                        }
                    }
                }
            }
        }
        #endregion

        private int GetUnoccupiedDwellingCount(NPCSettlement npcSettlement) {
            int count = 0;
            if (npcSettlement.HasStructure(STRUCTURE_TYPE.DWELLING)) {
                List<LocationStructure> dwellings = npcSettlement.GetStructuresOfType(STRUCTURE_TYPE.DWELLING);
                for (int i = 0; i < dwellings.Count; i++) {
                    LocationStructure dwelling = dwellings[i];
                    if (dwelling.residents.Count == 0) {
                        count++;
                    }
                }
            }
            return count;
        }
    }
}