using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine.Assertions;
using UtilityScripts;

public class SocializingBehaviour : CharacterBehaviourComponent {

    public SocializingBehaviour() {
        priority = 1000;
        attributes = new BEHAVIOUR_COMPONENT_ATTRIBUTE[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.STOPS_BEHAVIOUR_LOOP };
    }
    
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        LocationStructure targetStructure = character.behaviourComponent.targetSocializeStructure;
#if DEBUG_LOG
        log = $"{log}\n{character.name} Socializing Behaviour at {targetStructure.name}.";
#endif
        if (character.behaviourComponent.socializingEndTime.hasValue && character.behaviourComponent.socializingEndTime.IsBefore(GameManager.Instance.Today())) {
#if DEBUG_LOG
            log = $"{log}\n\t- Socializing End time has been reached.";
#endif
            character.behaviourComponent.ClearOutSocializingBehaviour();
            producedJob = null;
            return false;
        } else if (targetStructure.hasBeenDestroyed) {
#if DEBUG_LOG
            log = $"{log}\n\t- Target structure has been destroyed.";
#endif
            character.behaviourComponent.ClearOutSocializingBehaviour();
            producedJob = null;
            return false;
        } else if (character.currentStructure == targetStructure) {
#if DEBUG_LOG
            log = $"{log}\n\t- {character.name} is at target structure.";
#endif
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
                if (GameUtilities.RollChance(10, ref log)) {
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
            
#if DEBUG_LOG
            log = $"{log}\n\t- Will try to create roam around structure job.";
#endif
            if (character.jobComponent.TriggerRoamAroundStructure(JOB_TYPE.SOCIALIZE, out producedJob)) {
#if DEBUG_LOG
                log = $"{log}\n\t- Roam around structure job successfully created.";
#endif
                return true;
            }
        } else {
#if DEBUG_LOG
            log = $"{log}\n\t- {character.name} is not at target structure, create job to go to target structure.";
#endif
            var targetTile = targetStructure.passableTiles.Count > 0 ? CollectionUtilities.GetRandomElement(targetStructure.passableTiles) : CollectionUtilities.GetRandomElement(targetStructure.tiles);
            if (character.movementComponent.HasPathToEvenIfDiffRegion(targetTile)) {
                return character.jobComponent.CreateGoToJob(JOB_TYPE.VISIT_STRUCTURE, targetTile, out producedJob);    
            }
            
#if DEBUG_LOG
            log = $"{log}\n\t- Removed socializing behaviour since character cannot reach {targetStructure.name}.";
#endif
            character.behaviourComponent.ClearOutSocializingBehaviour();
            producedJob = null;
            return false;
        }
        //returned true because we expect that if a character is socializing, this behaviour should always return a job.
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


