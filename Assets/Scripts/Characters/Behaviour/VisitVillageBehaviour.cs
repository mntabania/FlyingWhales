using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UnityEngine.Assertions;
using UtilityScripts;

public class VisitVillageBehaviour : CharacterBehaviourComponent {
    
    public VisitVillageBehaviour() {
        priority = 1000;
        attributes = new BEHAVIOUR_COMPONENT_ATTRIBUTE[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.STOPS_BEHAVIOUR_LOOP };
    }
    
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        NPCSettlement targetVillage = character.behaviourComponent.targetVisitVillage;
#if DEBUG_LOG
        log = $"{log}\n- {character.name} is visiting village {targetVillage.name}.";
#endif
        if (character.behaviourComponent.visitVillageEndTime.hasValue && character.behaviourComponent.visitVillageEndTime.IsBefore(GameManager.Instance.Today())) {
#if DEBUG_LOG
            log = $"{log}\n\t- Visit village is at end date. Removing behaviour";
#endif            
            character.behaviourComponent.ClearOutVisitVillageBehaviour();
            producedJob = null;
            return false;
        } else {
            if (character.currentSettlement == targetVillage) {
#if DEBUG_LOG
                log = $"{log}\n\t- Actor is at target village";
#endif
#if DEBUG_LOG
                log = $"{log}\n\t- Visit village intent is {character.behaviourComponent.visitVillageIntent}";
#endif            
                if (character.behaviourComponent.visitVillageIntent == VISIT_VILLAGE_INTENT.Socialize) {
                    LocationStructure targetStructure = character.behaviourComponent.targetVisitVillageStructure;
                    if (targetStructure == null) {
#if DEBUG_LOG
                        log = $"{log}\n\t- Visit village target structure has not yet been processed. Wait for processing to kick in...";
#endif            
                        producedJob = null;
                        return false;
                    }
#if DEBUG_LOG
                    log = $"{log}\n\t- Visit village target structure is {targetStructure.name}";
#endif            
                    if (targetStructure.hasBeenDestroyed) {
#if DEBUG_LOG
                        log = $"{log}\n\t- Target structure has been destroyed. Removing behaviour.";
#endif            
                        character.behaviourComponent.ClearOutVisitVillageBehaviour();
                        producedJob = null;
                        return false;        
                    } else {
                        if (targetStructure.structureType == STRUCTURE_TYPE.TAVERN) {
#if DEBUG_LOG
                            log = $"{log}\n\t- Target structure is tavern. Rolling chance to drink.";
#endif
                            if (GameUtilities.RollChance(15, ref log)) {
                                List<TileObject> tables = targetStructure.GetTileObjectsOfType(TILE_OBJECT_TYPE.TABLE);
#if DEBUG_LOG
                                log = $"{log}\n\t- Checking for available tables. Total tables are {tables?.Count}.";
#endif
                                if (tables != null) {
                                    for (int i = 0; i < tables.Count; i++) {
                                        TileObject tileObject = tables[i];
                                        Table table = tileObject as Table;
                                        Assert.IsNotNull(table);
                                        if (table.mapObjectState == MAP_OBJECT_STATE.BUILT && table.CanAccommodateCharacter(character)) {
#if DEBUG_LOG
                                            log = $"{log}\n\t- Found table {table.nameWithID}. Will try do create drink job.";
#endif
                                            if (character.jobComponent.TriggerDrinkJob(JOB_TYPE.SOCIALIZE, table, out producedJob)) {
#if DEBUG_LOG
                                                log = $"{log}\n\t- Drink Job successfully created";
#endif
                                                return true;
                                            }
                                        }
                                    }    
                                }
                            }
                        } else if (targetStructure.structureType == STRUCTURE_TYPE.CITY_CENTER) {
#if DEBUG_LOG
                            log = $"{log}\n\t- Target structure is City Center.";
#endif
                            if (GameUtilities.RollChance(15, ref log)) {
                                List<TileObject> waterWells = targetStructure.GetTileObjectsOfType(TILE_OBJECT_TYPE.WATER_WELL);
#if DEBUG_LOG
                                log = $"{log}\n\t- Checking for available wells. Total wells are {waterWells?.Count}.";
#endif
                                if (waterWells != null) {
                                    for (int i = 0; i < waterWells.Count; i++) {
                                        TileObject tileObject = waterWells[i];
                                        WaterWell waterWell = tileObject as WaterWell;
                                        Assert.IsNotNull(waterWell);
                                        if (waterWell.mapObjectState == MAP_OBJECT_STATE.BUILT) {
#if DEBUG_LOG
                                            log = $"{log}\n\t- Found well {waterWell.nameWithID}. Will do drink water job.";
#endif
                                            return character.jobComponent.TriggerDrinkWaterJob(waterWell, out producedJob);
                                        }
                                    }
                                }
                            }
                        }

#if DEBUG_LOG
                        log = $"{log}\n\t- Will try to chat. Rolling chance.";
#endif
                        if (GameUtilities.RollChance(20, ref log)) {
                            if (ChatBehaviour(character, ref log, out producedJob)) {
                                return true;
                            }
                        }

                        TIME_IN_WORDS currentTimeOfDay = GameManager.Instance.GetCurrentTimeInWordsOfTick();
                        
#if DEBUG_LOG
                        log = $"{log}\n\t- Rolling chance to change intent.";
#endif
                        if (ChanceData.RollChance(CHANCE_TYPE.Change_Intent, ref log)) {
                            if (character.traitContainer.HasTrait("Kleptomaniac") && ChanceData.RollChance(CHANCE_TYPE.Change_Intent_Kleptomania, ref log)) {
                                character.behaviourComponent.SetVisitVillageIntent(VISIT_VILLAGE_INTENT.Steal);
                                LocationStructure randomStructure = targetVillage.GetRandomStructure();
                                character.behaviourComponent.SetTargetVisitVillageStructure(randomStructure);
#if DEBUG_LOG
                                log = $"{log}\n\t- Character is kleptomaniac, set intent to steal from {randomStructure.name}.";
#endif
                            } else if (character.traitContainer.HasTrait("Vampire") && 
                                (currentTimeOfDay == TIME_IN_WORDS.EARLY_NIGHT || currentTimeOfDay == TIME_IN_WORDS.LATE_NIGHT || currentTimeOfDay == TIME_IN_WORDS.AFTER_MIDNIGHT) && 
                                ChanceData.RollChance(CHANCE_TYPE.Change_Intent_Vampire, ref log)) {
                                character.behaviourComponent.SetVisitVillageIntent(VISIT_VILLAGE_INTENT.Drink_Blood);
#if DEBUG_LOG
                                log = $"{log}\n\t- Character is Vampire, set intent to drink blood.";
#endif
                            } else if (character.traitContainer.HasTrait("Cultist") && ChanceData.RollChance(CHANCE_TYPE.Change_Intent_Cultist, ref log)) {
                                character.behaviourComponent.SetVisitVillageIntent(VISIT_VILLAGE_INTENT.Preach);
#if DEBUG_LOG
                                log = $"{log}\n\t- Character is Cultist, set intent to preach.";
#endif
                            }
                        } else {
                            if (character.currentStructure != targetStructure) {
#if DEBUG_LOG
                                log = $"{log}\n\t- Not yet at target structure. Will go there now";
#endif
                                var targetTile = targetStructure.passableTiles.Count > 0 ? 
                                    CollectionUtilities.GetRandomElement(targetStructure.passableTiles) : 
                                    CollectionUtilities.GetRandomElement(targetStructure.tiles);
                                return character.jobComponent.CreateGoToJob(JOB_TYPE.VISIT_DIFFERENT_VILLAGE, targetTile, out producedJob);  
                            }
#if DEBUG_LOG
                            log = $"{log}\n\t- Will roam around current structure.";
#endif
                            if (character.jobComponent.TriggerRoamAroundStructure(JOB_TYPE.VISIT_DIFFERENT_VILLAGE, out producedJob)) {
                                return true;
                            }
                        }
                    
                    }
                }

#if DEBUG_LOG
                log = $"{log}\n\t- Visit village intent was changed to {character.behaviourComponent.visitVillageIntent}.";
#endif
                if (character.behaviourComponent.visitVillageIntent == VISIT_VILLAGE_INTENT.Steal) {
                    if (character.currentStructure == character.behaviourComponent.targetVisitVillageStructure) {
#if DEBUG_LOG
                        log = $"{log}\n\t- Character is at target steal structure {character.behaviourComponent.targetVisitVillageStructure.name}. Will create steal job.";
#endif
                        character.behaviourComponent.SetVisitVillageIntent(VISIT_VILLAGE_INTENT.Socialize);
                        //steal random item
                        if (character.jobComponent.TriggerRobLocation(character.currentStructure, INTERACTION_TYPE.STEAL_ANYTHING, out producedJob)) {
                            return true;
                        }
                    } else {
#if DEBUG_LOG
                        log = $"{log}\n\t- Character is not yet at target steal structure. Will create job to go there";
#endif
                        var targetTile = character.behaviourComponent.targetVisitVillageStructure.passableTiles.Count > 0 ? 
                            CollectionUtilities.GetRandomElement(character.behaviourComponent.targetVisitVillageStructure.passableTiles) : 
                            CollectionUtilities.GetRandomElement(character.behaviourComponent.targetVisitVillageStructure.tiles);
                        return character.jobComponent.CreateGoToJob(JOB_TYPE.VISIT_STRUCTURE, targetTile, out producedJob);  
                    }
                } else if (character.behaviourComponent.visitVillageIntent == VISIT_VILLAGE_INTENT.Drink_Blood) {
                    character.behaviourComponent.SetVisitVillageIntent(VISIT_VILLAGE_INTENT.Socialize);
                    List<Character> drinkBloodTargets = RuinarchListPool<Character>.Claim();
                    targetVillage.PopulateResidentsCurrentlyInsideVillage(drinkBloodTargets);
                    if (drinkBloodTargets.Count > 0) {
                        Character chosenTarget = CollectionUtilities.GetRandomElement(drinkBloodTargets);
#if DEBUG_LOG
                        log = $"{log}\n\t- Will create drink blood job targeting {chosenTarget.name}.";
#endif
                        if (character.jobComponent.CreateDrinkBloodJob(JOB_TYPE.FULLNESS_RECOVERY_URGENT, chosenTarget,
                            out producedJob)) {
                            RuinarchListPool<Character>.Release(drinkBloodTargets);
                            return true;
                        }
                    }
                    RuinarchListPool<Character>.Release(drinkBloodTargets);
                } else if (character.behaviourComponent.visitVillageIntent == VISIT_VILLAGE_INTENT.Preach) {
                    character.behaviourComponent.SetVisitVillageIntent(VISIT_VILLAGE_INTENT.Socialize);
                    if (character.jobComponent.TryGetValidEvangelizeTargetInsideVillage(out var targetCharacter, targetVillage)) {
#if DEBUG_LOG
                        log = $"{log}\n\t- Will Preach to target {targetCharacter.name}.";
#endif
                        return character.jobComponent.TryCreateEvangelizeJob(targetCharacter, out producedJob);    
                    }
#if DEBUG_LOG
                    log = $"{log}\n\t- Could not find a resident at {targetVillage.name} to preach to.";
#endif
                }
            } else {
#if DEBUG_LOG
                log = $"{log}\n\t- Not yet at target village, will go there now.";
#endif
                //go to target village
                var targetTile = targetVillage.cityCenter.passableTiles.Count > 0 ? 
                    CollectionUtilities.GetRandomElement(targetVillage.cityCenter.passableTiles) : 
                    CollectionUtilities.GetRandomElement(targetVillage.cityCenter.tiles);
                if (character.movementComponent.HasPathToEvenIfDiffRegion(targetTile)) {
                    return character.jobComponent.CreateGoToJob(JOB_TYPE.VISIT_DIFFERENT_VILLAGE, targetTile, out producedJob);     
                }
            }
        }
#if DEBUG_LOG
        log = $"{log}\n\t- Will roam around current structure.";
#endif
        if (character.jobComponent.TriggerRoamAroundStructure(JOB_TYPE.VISIT_DIFFERENT_VILLAGE, out producedJob)) {
            return true;
        }
        
#if DEBUG_LOG
        log = $"{log}\n\t- Reached loop hole catcher.";
#endif
        //returned true because we expect that if a character is visiting, this behaviour should always return a job.
        //This is just to enforce that rule in case there are loop holes
        producedJob = null;
        return true;
    }
    
    private bool ChatBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        if (character.isNormalCharacter && character.hasMarker && character.marker.inVisionCharacters.Count > 0 && CharacterManager.Instance.HasCharacterNotConversedInMinutes(character, 9)) {
#if DEBUG_LOG
            log = $"{log}\n{character.name} has characters in vision and has not conversed in at least 9 minutes.";
#endif
            List<Character> validChoices = RuinarchListPool<Character>.Claim();
            character.marker.PopulateCharactersThatIsNotDeadVillagerAndNotConversedInMinutes(validChoices, 9);
            Character chosenTarget = null;
            if (validChoices.Count > 0) {
                chosenTarget = CollectionUtilities.GetRandomElement(validChoices);
            }
            RuinarchListPool<Character>.Release(validChoices);
            if (chosenTarget != null) {
#if DEBUG_LOG
                log = $"{log}\n{character.name} has characters in vision that have not conversed in at least 9 minutes. Chosen target is {chosenTarget.name}. Rolling chat chance";
#endif
                if (character.nonActionEventsComponent.CanChat(chosenTarget) && GameUtilities.RollChance(50, ref log)) {
                    character.interruptComponent.TriggerInterrupt(INTERRUPT.Chat, chosenTarget);
                    producedJob = null;
                    return true;
                } else {
#if DEBUG_LOG
                    log = $"{log}\nChat roll failed.";
#endif
                    if (character.moodComponent.moodState == MOOD_STATE.Normal && RelationshipManager.Instance.IsCompatibleBasedOnSexualityAndOpinion(character, chosenTarget) && character.limiterComponent.isSociable) {
#if DEBUG_LOG
                        log = $"{log}\nCharacter is in normal mood and is compatible with target";
#endif
                        if (character.nonActionEventsComponent.CanFlirt(character, chosenTarget)) {
#if DEBUG_LOG
                            log = $"{log}\nCharacter can flirt with target.";
#endif
                            int compatibility = RelationshipManager.Instance.GetCompatibilityBetween(character, chosenTarget);
                            int baseChance = 0;

                            if (!character.relationshipContainer.HasRelationship(RELATIONSHIP_TYPE.LOVER) || 
                                character.relationshipContainer.HasRelationshipWith(chosenTarget, RELATIONSHIP_TYPE.AFFAIR)) {
                                baseChance += 20;
                            }
#if DEBUG_LOG
                            log = $"{log}\n-Flirt has {baseChance}% (multiplied by Compatibility value) chance to trigger";
#endif
                            if (character.moodComponent.moodState == MOOD_STATE.Normal) {
#if DEBUG_LOG
                                log = $"{log}\n-Flirt has +10% chance to trigger because character is in a normal mood";
#endif
                                baseChance += 10;
                            }

                            float flirtChance;
                            if (compatibility != -1) {
                                //has compatibility value
                                flirtChance = baseChance * compatibility;
#if DEBUG_LOG
                                log = $"{log}\n-Chance: {flirtChance.ToString()}";
#endif
                            } else {
                                //has NO compatibility value
                                flirtChance = baseChance * 2;
#if DEBUG_LOG
                                log = $"{log}\n-Chance: {flirtChance.ToString()} (No Compatibility)";
#endif
                            }

                            if (character.relationshipContainer.HasRelationshipWith(chosenTarget, RELATIONSHIP_TYPE.LOVER)) {
                                flirtChance *= 0.2f;
#if DEBUG_LOG
                                log = $"{log}\n-{chosenTarget.name} is lover of {character.name} multiply chance by 0.2%. Flirt chance is : {flirtChance.ToString()}";
#endif
                            }

                            if (GameUtilities.RollChance(flirtChance, ref log)) {
                                character.interruptComponent.TriggerInterrupt(INTERRUPT.Flirt, chosenTarget);
                                producedJob = null;
                                return true;
                            } else {
#if DEBUG_LOG
                                log = $"{log}\n-Flirt did not trigger";
#endif
                            }
                        } else {
#if DEBUG_LOG
                            log = $"{log}\n-Flirt did not trigger";
#endif
                        }
                    }
                }
            }
        }

        producedJob = null;
        return false;
    }
}
