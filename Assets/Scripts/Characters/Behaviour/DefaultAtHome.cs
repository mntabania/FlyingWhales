using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;

public class DefaultAtHome : CharacterBehaviourComponent {
    public DefaultAtHome() {
        priority = 10;
        //attributes = new[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.WITHIN_HOME_SETTLEMENT_ONLY };
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        //if (character.isNormalCharacter) {
        //    LocationStructure targetDemonicStructure = PlayerManager.Instance.player.playerSettlement.GetFirstStructureOfType(STRUCTURE_TYPE.THE_PORTAL);
        //    if (character.faction != null && !character.faction.partyQuestBoard.HasPartyQuestWithTarget(PARTY_QUEST_TYPE.Counterattack, targetDemonicStructure)) {
        //        character.faction.partyQuestBoard.CreateCounterattackPartyQuest(character, character.homeSettlement, targetDemonicStructure);
        //    }
        //}
        if ((character.homeStructure == null || character.homeStructure.hasBeenDestroyed) && !character.HasTerritory()) {
#if DEBUG_LOG
            log = $"{log}\n-No home structure";
            log = $"{log}\n-Will do action Stand";
#endif
            character.PlanFixedJob(JOB_TYPE.IDLE_STAND, INTERACTION_TYPE.STAND, character, out producedJob);
            //log += $"\n-25% chance to Set Home";
            //int roll = Random.Range(0, 100);
            //log += $"\nRoll: {roll.ToString()}";
            //if(roll < 25) {
            //    producedJob = null;
            //    character.interruptComponent.TriggerInterrupt(INTERRUPT.Set_Home, null);
            //} else {
            //    log += $"\n-Will do action Stand";
            //    character.PlanIdle(JOB_TYPE.IDLE_STAND, INTERACTION_TYPE.STAND, character, out producedJob);
            //}
            return true;
        } else if (character.isAtHomeStructure || character.IsInTerritory()) {
            if (character.previousCharacterDataComponent.IsPreviousJobOrActionReturnHome()) {
#if DEBUG_LOG
                log = $"{log}\n-{character.name} is in home structure and just returned home";
#endif
                if ((character.characterClass.IsCombatant() || character.characterClass.className == "Noble") && !character.partyComponent.hasParty && character.homeSettlement != null 
                    && !character.traitContainer.HasTrait("Enslaved") && !character.structureComponent.HasWorkPlaceStructure()) {
                    bool shouldCreateOrJoinParty = true;
                    if (character.HasAfflictedByPlayerWith(PLAYER_SKILL_TYPE.AGORAPHOBIA)) {
                        shouldCreateOrJoinParty = PlayerSkillManager.Instance.GetAfflictionData(PLAYER_SKILL_TYPE.AGORAPHOBIA).currentLevel >= 3;
                    }

                    Party unfullParty = character.homeSettlement.GetFirstUnfullParty();
                    if(unfullParty == null) {
                        if (GameUtilities.RollChance(20) && character.faction != null && shouldCreateOrJoinParty) { //10
                            character.interruptComponent.TriggerInterrupt(INTERRUPT.Create_Party, character);
                        }
                    } else {
                        if (GameUtilities.RollChance(45) && shouldCreateOrJoinParty) { //15
                            character.interruptComponent.TriggerInterrupt(INTERRUPT.Join_Party, unfullParty.members[0]);
                        }
                    }
                }

                TileObject deskOrTable = character.currentStructure.GetUnoccupiedBuiltTileObject(TILE_OBJECT_TYPE.DESK, TILE_OBJECT_TYPE.TABLE);
#if DEBUG_LOG
                log = $"{log}\n-Sit if there is still an unoccupied Table or Desk in the current location";
#endif
                if (deskOrTable != null) {
#if DEBUG_LOG
                    log = $"{log}\n  -{character.name} will do action Sit on {deskOrTable}";
#endif
                    character.PlanFixedJob(JOB_TYPE.IDLE_SIT, INTERACTION_TYPE.SIT, deskOrTable, out producedJob);
                } else {
#if DEBUG_LOG
                    log = $"{log}\n-Otherwise, stand idle";
                    log = $"{log}\n  -{character.name} will do action Stand";
#endif
                    character.PlanFixedJob(JOB_TYPE.IDLE_STAND, INTERACTION_TYPE.STAND, character, out producedJob);
                }
                return true;
            } else {
#if DEBUG_LOG
                log = $"{log}\n-{character.name} is in home structure and previous action is not returned home";
#endif
                TIME_IN_WORDS currentTimeOfDay = GameManager.Instance.GetCurrentTimeInWordsOfTick(character);
                string strCurrentTimeOfDay = currentTimeOfDay.ToString();
//#if DEBUG_LOG
//                log = $"{log}\n-If it is Morning";
//#endif
//                if (currentTimeOfDay == TIME_IN_WORDS.MORNING || currentTimeOfDay == TIME_IN_WORDS.LUNCH_TIME || currentTimeOfDay == TIME_IN_WORDS.AFTERNOON) {
//#if DEBUG_LOG
//                    log = $"{log}\n-If character is an Archer, Marauder, or Shaman";
//#endif
//                    if (character.characterClass.className == "Archer" || character.characterClass.className == "Marauder" || character.characterClass.className == "Shaman") {
//#if DEBUG_LOG
//                        log = $"{log}\n-15% chance to Create Exploration Party if there are no Exploration Party whose leader lives in the same settlement";
//#endif
//                        if (character.faction != null && !character.faction.partyQuestBoard.HasPartyQuest(PARTY_QUEST_TYPE.Exploration)) {
//                            int chance = WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Icalawa ? 5 : 15;
//                            int roll = Random.Range(0, 100);
//                            log = $"{log}\nRoll: {roll.ToString()}";
//                            if (roll < chance) {
//                                character.faction.partyQuestBoard.CreateExplorationPartyQuest(character, character.homeSettlement, character.currentRegion);
//                                //character.jobComponent.TriggerExploreJob(out producedJob);
//                                return true;
//                            }
//                        } else {
//#if DEBUG_LOG
//                            log = $"{log}\n-Already has an Exploration party whose leader lives in the same settlement";
//#endif
//                        }
//                    }
//#if DEBUG_LOG
//                    log = $"{log}\n-If character has a Close Friend who it considers Missing";
//#endif
//                    Character missingCharacter = character.relationshipContainer.GetRandomMissingCharacterWithOpinion(RelationshipManager.Close_Friend);
//                    if(missingCharacter != null) {
//#if DEBUG_LOG
//                        log = $"{log}\n-Missing close friend: {missingCharacter}";
//#endif
//                        if (character.faction != null && !character.faction.partyQuestBoard.HasPartyQuestWithTarget(PARTY_QUEST_TYPE.Rescue, missingCharacter)
//                            && !character.faction.partyQuestBoard.HasPartyQuestWithTarget(PARTY_QUEST_TYPE.Demon_Rescue, missingCharacter)) {
//                            int chance = Random.Range(0, 100);
//#if DEBUG_LOG
//                            log = $"{log}\nRoll: {chance.ToString()}";
//#endif
//                            if (chance < 20) {
//                                if (missingCharacter.IsConsideredInDangerBy(character)) {
//                                    character.faction.partyQuestBoard.CreateRescuePartyQuest(character, character.homeSettlement, missingCharacter);
//                                    return true;
//                                }
//                            }
//                        }
//                    } else {
//#if DEBUG_LOG
//                        log = $"{log}\n-No missing close friend";
//#endif
//                    }
//#if DEBUG_LOG
//                    log = $"{log}\n-If character has a Friend who it considers Missing";
//#endif
//                    missingCharacter = character.relationshipContainer.GetRandomMissingCharacterWithOpinion(RelationshipManager.Friend);
//                    if (missingCharacter != null) {
//                        log = $"{log}\n-Missing friend: {missingCharacter}";
//                        if (character.faction != null && !character.faction.partyQuestBoard.HasPartyQuestWithTarget(PARTY_QUEST_TYPE.Rescue, missingCharacter)
//                            && !character.faction.partyQuestBoard.HasPartyQuestWithTarget(PARTY_QUEST_TYPE.Demon_Rescue, missingCharacter)) {
//                            int chance = Random.Range(0, 100);
//                            log = $"{log}\nRoll: {chance.ToString()}";
//                            if (chance < 20) {
//                                if (missingCharacter.IsConsideredInDangerBy(character)) {
//                                    character.faction.partyQuestBoard.CreateRescuePartyQuest(character, character.homeSettlement, missingCharacter);
//                                    return true;
//                                }
//                            }
//                        }
//                    } else {
//#if DEBUG_LOG
//                        log = $"{log}\n-No missing friend";
//#endif
//                    }
//#if DEBUG_LOG
//                    log = $"{log}\n-If character has a Lover/Affair/Relative who it considers Missing";
//#endif
//                    missingCharacter = character.relationshipContainer.GetRandomMissingCharacterThatIsFamilyMemberOrLoverAffairOf(character);
//                    if (missingCharacter != null) {
//#if DEBUG_LOG
//                        log = log + ("\n-Missing Lover/Affair/Relative: " + missingCharacter);
//                        log = $"{log}\n-Character is combatant, 15% chance to Create Rescue Party if there are no Rescue Party whose leader lives in the same settlement";
//#endif
//                        if (character.faction != null && !character.faction.partyQuestBoard.HasPartyQuestWithTarget(PARTY_QUEST_TYPE.Rescue, missingCharacter)
//                            && !character.faction.partyQuestBoard.HasPartyQuestWithTarget(PARTY_QUEST_TYPE.Demon_Rescue, missingCharacter)) {
//                            int chance = Random.Range(0, 100);
//#if DEBUG_LOG
//                            log = $"{log}\nRoll: {chance}";
//#endif
//                            if (chance < 40) {
//                                if (missingCharacter.IsConsideredInDangerBy(character)) {
//                                    character.faction.partyQuestBoard.CreateRescuePartyQuest(character, character.homeSettlement, missingCharacter);
//                                    return true;
//                                }
//                            }
//                        }
//                    } else {
//#if DEBUG_LOG
//                        log = $"{log}\n-No missing Lover/Affair/Relative";
//#endif
//                    }

//                } else {
//#if DEBUG_LOG
//                    log = $"{log}\n  -Time of Day: {strCurrentTimeOfDay}";
//#endif
//                }
#if DEBUG_LOG
                log = $"{log}\n-If it is Early Night, 10% chance to Host Social Party at Inn";
#endif
                if (currentTimeOfDay == TIME_IN_WORDS.EARLY_NIGHT && character.trapStructure.IsTrapped() == false && character.trapStructure.IsTrappedInArea() == false) {
#if DEBUG_LOG
                    log = $"{log}\n  -Time of Day: {strCurrentTimeOfDay}";
#endif
                    int roll = Random.Range(0, 100);
#if DEBUG_LOG
                    log = $"{log}\n  -RNG roll: {roll.ToString()}";
#endif
                    if (ChanceData.RollChance(CHANCE_TYPE.Host_Social_Party)) {
                        if (character.traitContainer.HasTrait("Agoraphobic")) {
#if DEBUG_LOG
                            log = $"{log}\n  -Character is agoraphobic, not hosting social party";
#endif
                        } else {
                            //StartGOAP(INTERACTION_TYPE.DRINK, null, GOAP_CATEGORY.IDLE);
                            LocationStructure structure = character.homeSettlement.GetFirstStructureOfTypeWithNoActiveSocialParty(STRUCTURE_TYPE.TAVERN);
                            if (structure != null) {
#if DEBUG_LOG
                                log = $"{log}\n  -Early Night: {character.name} host a social party at Inn";
#endif
                                if (character.jobComponent.TriggerHostSocialPartyJob(out producedJob)) {
                                    return true;
                                }
                                //character.PlanIdle(JOB_TYPE.VISIT_FRIEND, INTERACTION_TYPE.VISIT, character, out producedJob, new object[] { structure });
                            }
#if DEBUG_LOG
                            log = $"{log}\n  -No Inn Structure in the npcSettlement";
#endif
                        }
                    }
                } else {
#if DEBUG_LOG
                    log = $"{log}\n  -Time of Day: {strCurrentTimeOfDay}";
#endif
                }
#if DEBUG_LOG
                log = $"{log}\n-Otherwise, if it is Lunch Time or Afternoon, 25% chance to nap if there is still an unoccupied Bed in the house";
#endif
                if (currentTimeOfDay == TIME_IN_WORDS.LUNCH_TIME || currentTimeOfDay == TIME_IN_WORDS.AFTERNOON) {
#if DEBUG_LOG
                    log = $"{log}\n  -Time of Day: {strCurrentTimeOfDay}";
#endif
                    int chance = Random.Range(0, 100);
#if DEBUG_LOG
                    log = $"{log}\n  -RNG roll: {chance.ToString()}";
#endif
                    if (chance < 25) {
                        TileObject bed = character.currentStructure.GetUnoccupiedTileObject(TILE_OBJECT_TYPE.BED);
                        if (bed != null) {
                            if (character.traitContainer.HasTrait("Vampire")) {
#if DEBUG_LOG
                                log = $"{log}\n  -Character is vampiric, cannot do nap action";
#endif
                            } else {
#if DEBUG_LOG
                                log = $"{log}\n  -Afternoon: {character.name} will do action Nap on {bed}";
#endif
                                character.PlanFixedJob(JOB_TYPE.IDLE_NAP, INTERACTION_TYPE.NAP, bed, out producedJob);
                                return true;
                            }
                        } else {
#if DEBUG_LOG
                            log = $"{log}\n  -No unoccupied bed in the current structure";
#endif
                        }
                    }
                } else {
#if DEBUG_LOG
                    log = $"{log}\n  -Time of Day: {strCurrentTimeOfDay}";
#endif
                }
#if DEBUG_LOG
                log = $"{log}\n-Otherwise, if it is Morning or Afternoon or Early Night, and the character has a positive relationship with someone currently Paralyzed or Catatonic, 30% chance to Check Out one at random";
#endif
                if (currentTimeOfDay == TIME_IN_WORDS.MORNING || currentTimeOfDay == TIME_IN_WORDS.AFTERNOON || currentTimeOfDay == TIME_IN_WORDS.EARLY_NIGHT) {
#if DEBUG_LOG
                    log = $"{log}\n  -Time of Day: {strCurrentTimeOfDay}";
#endif
                    int chance = Random.Range(0, 100);
#if DEBUG_LOG
                    log = $"{log}\n  -RNG roll: {chance.ToString()}";
#endif
                    if (chance < 30 && character.trapStructure.IsTrapped() == false && character.trapStructure.IsTrappedInArea() == false) {
                        Character chosenCharacter =
                            character.GetDisabledCharacterToCheckOutThatHasIsInHomeSettlementOfThisCharacter();
                        if (chosenCharacter != null) {
                            if(chosenCharacter.homeStructure != null) {
#if DEBUG_LOG
                                log = $"{log}\n  -Will visit house of Disabled Character {chosenCharacter.name}";
#endif
                                character.PlanFixedJob(JOB_TYPE.CHECK_PARALYZED_FRIEND, INTERACTION_TYPE.VISIT, character, out producedJob, 
                                    new OtherData[] { new LocationStructureOtherData(chosenCharacter.homeStructure), new CharacterOtherData(chosenCharacter),  });
                                return true;
                            } else {
#if DEBUG_LOG
                                log = $"{log}\n  -{chosenCharacter.name} has no house. Will check out character instead";
#endif
                                GoapEffect effect = new GoapEffect(GOAP_EFFECT_CONDITION.IN_VISION, string.Empty, false, GOAP_EFFECT_TARGET.TARGET);
                                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CHECK_PARALYZED_FRIEND, effect, chosenCharacter, character);
                                producedJob = job;
                            }
                            return true;
                        }
#if DEBUG_LOG
                        log = $"{log}\n  -No available character to check out ";
#endif
                    }
                } else {
#if DEBUG_LOG
                    log = $"{log}\n  -Time of Day: {strCurrentTimeOfDay}";
#endif
                }
#if DEBUG_LOG
                log = $"{log}\n-Otherwise, if it is Morning or Afternoon, 25% chance to add Obtain Personal Item Job if the character's Inventory is not yet full";
#endif
                if (currentTimeOfDay == TIME_IN_WORDS.MORNING || currentTimeOfDay == TIME_IN_WORDS.AFTERNOON) {
#if DEBUG_LOG
                    log = $"{log}\n  -Time of Day: {strCurrentTimeOfDay}";
#endif
                    int chance = Random.Range(0, 100);
#if DEBUG_LOG
                    log = $"{log}\n  -RNG roll: {chance.ToString()}";
#endif
                    if (chance < 25) {
                        if (character.jobComponent.TryCreateObtainPersonalItemJob(out producedJob)) {
#if DEBUG_LOG
                            log = $"{log}\n  -Created Obtain Personal Item Job";
#endif
                            return true;
                        } else {
#if DEBUG_LOG
                            log = $"{log}\n  -Could not create Obtain Personal Item Job. Either the inventory has reached full capacity or character has no items that he/she is interested";
#endif
                        }
                    }
                } else {
#if DEBUG_LOG
                    log = $"{log}\n  -Time of Day: {strCurrentTimeOfDay}";
#endif
                }
#if DEBUG_LOG
                log = $"{log}\n-Otherwise, if it is Morning or Lunch Time or Afternoon or Early Night, 25% chance to enter Stroll Outside Mode for 1 hour";
#endif
                if ((currentTimeOfDay == TIME_IN_WORDS.MORNING || currentTimeOfDay == TIME_IN_WORDS.LUNCH_TIME || 
                     currentTimeOfDay == TIME_IN_WORDS.AFTERNOON || currentTimeOfDay == TIME_IN_WORDS.EARLY_NIGHT) 
                    && character.trapStructure.IsTrapped() == false && character.trapStructure.IsTrappedInArea() == false) {
#if DEBUG_LOG
                    log = $"{log}\n  -Time of Day: {strCurrentTimeOfDay}";
#endif
                    if (GameUtilities.RollChance(25) && CanCreateCraftMissingBedJob(character, ref log)) {
#if DEBUG_LOG
                        log = $"{log}\n  -No Available bed will create craft missing bed job";
#endif
                        return character.jobComponent.CreateCraftMissingFurniture(TILE_OBJECT_TYPE.BED,
                            character.currentStructure, out producedJob);
                    } else if (GameUtilities.RollChance(25) && CanCreateCraftMissingTableJob(character)) {
#if DEBUG_LOG
                        log = $"{log}\n  -No Available table will create craft missing bed job";
#endif
                        return character.jobComponent.CreateCraftMissingFurniture(TILE_OBJECT_TYPE.TABLE,
                            character.currentStructure, out producedJob);
                    } else {
                        int chance = Random.Range(0, 100);
#if DEBUG_LOG
                        log = $"{log}\n  -RNG roll: {chance.ToString()}";
#endif
                        if (chance < 25) {
#if DEBUG_LOG
                            log = $"{log}\n  -Morning, Afternoon, or Early Night: {character.name} will enter Stroll Outside Mode";
#endif
                            character.jobComponent.PlanIdleStrollOutside(out producedJob); //character.currentStructure
                            return true;
                        }    
                    }
                } else {
#if DEBUG_LOG
                    log = $"{log}\n  -Time of Day: {strCurrentTimeOfDay}";
#endif
                }
#if DEBUG_LOG
                log = $"{log}\n-Otherwise, if it is Morning, Lunch Time or Afternoon, 25% chance to someone with a positive relationship in current location and then set it as the Base Structure for 2.5 hours";
#endif
                if (currentTimeOfDay == TIME_IN_WORDS.MORNING || currentTimeOfDay == TIME_IN_WORDS.LUNCH_TIME || currentTimeOfDay == TIME_IN_WORDS.AFTERNOON) {
#if DEBUG_LOG
                    log = $"{log}\n  -Time of Day: {strCurrentTimeOfDay}";
#endif
                    // int chance = Random.Range(0, 100);
                    // log = $"{log}\n  -RNG roll: {chance.ToString()}";
                    if (ChanceData.RollChance(CHANCE_TYPE.Visit_Friend, ref log) && character.trapStructure.IsTrapped() == false && character.trapStructure.IsTrappedInArea() == false) { //chance < 25
                        WeightedDictionary<Character> visitWeights = GetCharacterToVisitWeights(character);
                        if (visitWeights.GetTotalOfWeights() > 0) {
                            Character targetCharacter = visitWeights.PickRandomElementGivenWeights();
                            LocationStructure targetStructure = targetCharacter.homeStructure;
                            Assert.IsNotNull(targetStructure, $"Home structure of visit target {targetCharacter.name} is null!");
#if DEBUG_LOG
                            log = $"{log}\n  -Morning or Afternoon: {character.name} will go to dwelling of character with positive relationship, {targetCharacter.name} and set Base Structure for 2.5 hours";
#endif
                            character.PlanFixedJob(JOB_TYPE.VISIT_FRIEND, INTERACTION_TYPE.VISIT, targetCharacter, out producedJob, 
                                new OtherData[] { new LocationStructureOtherData(targetStructure), new CharacterOtherData(targetCharacter),  });
                            return true;
                        } else {
#if DEBUG_LOG
                            log = $"{log}\n  -No valid character to visit.";
#endif
                        }
                    }
                } else {
#if DEBUG_LOG
                    log = $"{log}\n  -Time of Day: {strCurrentTimeOfDay}";
#endif
                }
#if DEBUG_LOG
                log = $"{log}\n-Otherwise, if character has at least one item in his inventory, 15% chance (multiplied by number of items in inventory, but cap at 4) to add a Drop Item job";
#endif
                if (character.HasItem() && character.homeStructure != null) {
#if DEBUG_LOG
                log = $"{log}\n  -Has {character.items.Count.ToString()} items in inventory";
#endif
                    //int multiplier = character.items.Count > 4 ? 4 : character.items.Count;
                    int chance = /*multiplier **/ 15;
                    int roll = Random.Range(0, 100);
#if DEBUG_LOG
                    log = $"{log}\n  -Chance: {chance.ToString()}";
                    log = $"{log}\n  -Roll: {roll.ToString()}";
#endif
                    if (roll < chance) {
#if DEBUG_LOG
                        log = $"{log}\n  -Will create Drop Item job";
#endif
                        character.jobComponent.CreateDropItemJob(JOB_TYPE.DROP_ITEM, character.GetRandomItem(), character.homeStructure);
                        return true;
                    } 
                } else {
#if DEBUG_LOG
                    log = $"{log}\n  -Has no item in inventory";
#endif
                }
#if DEBUG_LOG
                log = $"{log}\n-Otherwise, sit if there is still an unoccupied Table or Desk";
#endif
                TileObject deskOrTable = character.currentStructure.GetUnoccupiedBuiltTileObject(TILE_OBJECT_TYPE.DESK, TILE_OBJECT_TYPE.TABLE);
                if (deskOrTable != null) {
#if DEBUG_LOG
                    log = $"{log}\n  -{character.name} will do action Sit on {deskOrTable}";
#endif
                    character.PlanFixedJob(JOB_TYPE.IDLE_SIT, INTERACTION_TYPE.SIT, deskOrTable, out producedJob);
                    return true;
                }
#if DEBUG_LOG
                log = $"{log}\n  -No unoccupied Table or Desk";

                log = $"{log}\n-Otherwise, stand idle";
                log = $"{log}\n  -{character.name} will do action Stand";
#endif
                character.PlanFixedJob(JOB_TYPE.IDLE_STAND, INTERACTION_TYPE.STAND, character, out producedJob);
                //PlanIdleStroll(currentStructure);
                return true;
            }
        }
        return false;
    }


    private bool CanCreateCraftMissingBedJob(Character character, ref string log) {
#if DEBUG_LOG
        log = $"{log}\n{character.name} is checking if it can create craft missing bed job.";
#endif
        if (character.currentStructure is Wilderness == false) {
            Character lover = character.relationshipContainer.GetFirstCharacterWithRelationship(RELATIONSHIP_TYPE.LOVER);
            if (lover != null) {
#if DEBUG_LOG
                log = $"{log}\n{character.name} has a lover {lover.name}.";
#endif
            }
            //get built un owned beds or beds owned by this characters lover.
            if (!character.currentStructure.AnyBuiltTileObjectsOfTypeUnownedOrOwnedBy(TILE_OBJECT_TYPE.BED, character, lover, out var objectLog)) { //bed => bed.mapObjectState == MAP_OBJECT_STATE.BUILT && (bed.characterOwner == null || bed.characterOwner == character || (lover != null && bed.characterOwner == lover)))
                //if there are none, check if there are any unbuilt beds (Means there is an active job at the structure)
                //if there are no unbuilt beds then the character can create a craft missing bed job.
                log = log + objectLog;
                return !character.currentStructure.AnyUnbuiltTileObjectsOfType(TILE_OBJECT_TYPE.BED);
            } else {
                //do not create craft bed since character found an unowned bed or a bed that is owned by his/her lover
                log = log + objectLog;
                return false;
            }
        }
        return false;
    }
    private bool CanCreateCraftMissingTableJob(Character character) {
        if (character.currentStructure is Wilderness == false) {
            //check if there are any built tables at structure
            if (!character.currentStructure.AnyBuiltTileObjectsOfType(TILE_OBJECT_TYPE.TABLE)) {
                //if none, then check if there are any unbuilt tables at structure, if there are none, then allow job creation
                return !character.currentStructure.AnyUnbuiltTileObjectsOfType(TILE_OBJECT_TYPE.TABLE);
            } else {
                //do not create craft table since character found a built table
                return false;
            }
        }
        return false;
    }
}
