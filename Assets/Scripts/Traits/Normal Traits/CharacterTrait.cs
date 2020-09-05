﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;
using Locations.Settlements;
using Traits;
using UnityEngine.Assertions;
namespace Traits {
    //This trait is present in all characters
    //A dummy trait in order for some jobs to be created
    public class CharacterTrait : Trait {
        //IMPORTANT NOTE: When the owner of this trait changed its alter ego, this trait will not be present in the alter ego anymore
        //Meaning that he/she cannot do the things specified in here anymore unless he/she switch to the ego which this trait is present
        public List<TileObject> alreadyInspectedTileObjects { get; private set; }
        public List<Character> charactersAlreadySawForHope { get; private set; }
        public HashSet<Character> charactersThatHaveReactedToThis { get; private set; }
        public Character owner { get; private set; }

        #region getters
        public override Type serializedData => typeof(SaveDataCharacterTrait);
        #endregion

        public CharacterTrait() {
            name = "Character Trait";
            type = TRAIT_TYPE.NEUTRAL;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            isHidden = true;
            alreadyInspectedTileObjects = new List<TileObject>();
            charactersAlreadySawForHope = new List<Character>();
            charactersThatHaveReactedToThis = new HashSet<Character>();
            AddTraitOverrideFunctionIdentifier(TraitManager.Start_Perform_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.See_Poi_Trait);
        }

        #region Loading
        public override void LoadSecondWaveInstancedTrait(SaveDataTrait saveDataTrait) {
            base.LoadSecondWaveInstancedTrait(saveDataTrait);
            SaveDataCharacterTrait saveDataCharacterTrait = saveDataTrait as SaveDataCharacterTrait;
            Assert.IsNotNull(saveDataCharacterTrait);
            alreadyInspectedTileObjects = SaveUtilities.ConvertIDListToTileObjects(saveDataCharacterTrait.alreadyInspectedTileObjects);
            charactersAlreadySawForHope.AddRange(SaveUtilities.ConvertIDListToCharacters(saveDataCharacterTrait.charactersAlreadySawForHope));
            charactersThatHaveReactedToThis = new HashSet<Character>(SaveUtilities.ConvertIDListToCharacters(saveDataCharacterTrait.charactersThatHaveReactedToThis));
        }
        #endregion
        
        public void AddAlreadyInspectedObject(TileObject to) {
            if (!alreadyInspectedTileObjects.Contains(to)) {
                alreadyInspectedTileObjects.Add(to);
            }
        }
        public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
            base.LoadTraitOnLoadTraitContainer(addTo);
            owner = addTo as Character;
        }
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            owner = addedTo as Character;
        }

        #region Reactions
        public void AddCharacterThatHasReactedToThis(Character character) {
            if (!charactersThatHaveReactedToThis.Contains(character)) {
                charactersThatHaveReactedToThis.Add(character);
            }
        }
        public void RemoveCharacterThatHasReactedToThis(Character character) {
            charactersThatHaveReactedToThis.Remove(character);
        }
        public bool HasReactedToThis(Character character) {
            return charactersThatHaveReactedToThis.Contains(character);
        }
        #endregion
        
        #region Overrides
        public override bool OnSeePOI(IPointOfInterest targetPOI, Character characterThatWillDoJob) {
            if (targetPOI is TileObject item) {
                //if(item is Heirloom) {
                //    Debug.Log("sdfsdf");
                //}
                if (item is TreasureChest) {
                    if (characterThatWillDoJob.jobQueue.HasJob(JOB_TYPE.OPEN_CHEST, item) == false 
                        && characterThatWillDoJob.traitContainer.HasTrait("Suspicious") == false) {
                        //if character is non suspicious, create an open chest job.
                        characterThatWillDoJob.jobComponent.CreateOpenChestJob(item);
                    }
                } else if (item is CultistKit && characterThatWillDoJob.traitContainer.HasTrait("Cultist") == false) {
                    //When a non-cultist sees a Cultist Kit, they will destroy it.
                    //Reference: https://www.notion.so/ruinarch/685c3fcca68545e285120e8778beed30?v=bc3ddbfa0b414ad881cc5e6d688bbe60&p=f4cd97ba9b53420fa6dedf0bf0650cb5
                    if (characterThatWillDoJob.jobQueue.HasJob(JOB_TYPE.DESTROY, item) == false) {
                        //if character is non suspicious, create an open chest job.
                        characterThatWillDoJob.jobComponent.TriggerDestroy(item);
                    }
                } else if (ShouldInspectItem(characterThatWillDoJob, item)) {
                    if (!characterThatWillDoJob.jobQueue.HasJob(JOB_TYPE.INSPECT, item) && 
                        !characterThatWillDoJob.jobComponent.HasHigherPriorityJobThan(JOB_TYPE.INSPECT)) {
                        characterThatWillDoJob.jobComponent.TriggerInspect(item);
                    }
                } else if (item.traitContainer.HasTrait("Edible") && characterThatWillDoJob.needsComponent.isStarving && !characterThatWillDoJob.traitContainer.HasTrait("Vampiric") && !characterThatWillDoJob.traitContainer.HasTrait("Paralyzed")) {
                    characterThatWillDoJob.jobComponent.CreateEatJob(item);
                } else if (!characterThatWillDoJob.IsInventoryAtFullCapacity() && (characterThatWillDoJob.IsItemInteresting(item.name) || item.traitContainer.HasTrait("Treasure"))) {
                    if (!characterThatWillDoJob.jobComponent.HasHigherPriorityJobThan(JOB_TYPE.TAKE_ITEM) 
                        && characterThatWillDoJob.traitContainer.HasTrait("Suspicious") == false) {
                        if (item.CanBePickedUpNormallyUponVisionBy(characterThatWillDoJob)
                            && !characterThatWillDoJob.jobQueue.HasJob(JOB_TYPE.TAKE_ITEM)) {
                            int chance = 100;
                            if (characterThatWillDoJob.HasItem(item.name)) {
                                chance = 10;
                            }
                            if (UnityEngine.Random.Range(0, 100) < chance) {
                                characterThatWillDoJob.jobComponent.CreateTakeItemJob(item);
                                return true;
                            }
                        }
                    }
                } else if (characterThatWillDoJob.partyComponent.hasParty) {
                    if(characterThatWillDoJob.partyComponent.currentParty is HeirloomHuntParty heirloomParty) {
                        if(heirloomParty.targetHeirloom == item) {
                            heirloomParty.SetFoundHeirloom(true);
                            characterThatWillDoJob.jobComponent.CreateDropItemJob(JOB_TYPE.DROP_ITEM_PARTY, heirloomParty.targetHeirloom, heirloomParty.targetHeirloom.structureSpot, true);
                        }
                    }
                }
            }
            if(targetPOI is Character targetCharacter) {
                if (!targetCharacter.isDead) {
                    if (!targetCharacter.isNormalCharacter) {
                        string opinionLabel = characterThatWillDoJob.relationshipContainer.GetOpinionLabel(targetCharacter);
                        if (opinionLabel == RelationshipManager.Friend) {
                            if (!charactersAlreadySawForHope.Contains(targetCharacter)) {
                                charactersAlreadySawForHope.Add(targetCharacter);
                                characterThatWillDoJob.needsComponent.AdjustHope(-5f);
                            }
                        } else if (opinionLabel == RelationshipManager.Close_Friend) {
                            if (!charactersAlreadySawForHope.Contains(targetCharacter)) {
                                charactersAlreadySawForHope.Add(targetCharacter);
                                characterThatWillDoJob.needsComponent.AdjustHope(-10f);
                            }
                        }
                    } else {
                        if(targetCharacter.traitContainer.HasTrait("Restrained", "Unconscious", "Frozen", "Ensnared")) {
                            if(owner.partyComponent.hasParty) {
                                if(owner.partyComponent.currentParty is RescueParty rescueParty) {
                                    if (rescueParty.isWaitTimeOver && rescueParty.targetCharacter == targetCharacter) {
                                        if (owner.jobComponent.TriggerReleaseJob(targetCharacter)) {
                                            rescueParty.SetIsReleasing(true);
                                        }
                                    }
                                } else if (owner.faction != null && owner.faction != targetCharacter.faction) {
                                    if (owner.partyComponent.currentParty is ExplorationParty exploreParty) {
                                        if (exploreParty.isWaitTimeOver) {
                                            if (owner.faction.factionType.HasIdeology(FACTION_IDEOLOGY.Warmonger)){
                                                if(UnityEngine.Random.Range(0, 100) < 15) {
                                                    owner.jobComponent.TriggerKidnapJob(targetCharacter);
                                                }
                                            } else if (owner.faction.factionType.HasIdeology(FACTION_IDEOLOGY.Peaceful)) {
                                                owner.jobComponent.TriggerReleaseJob(targetCharacter);
                                            }

                                        }
                                    } else if (owner.partyComponent.currentParty is RaidParty raidParty) {
                                        if (raidParty.isWaitTimeOver) {
                                            if (UnityEngine.Random.Range(0, 100) < 15) {
                                                owner.jobComponent.TriggerKidnapJob(targetCharacter);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if(owner.isNormalCharacter && targetCharacter.isNormalCharacter && owner.faction != targetCharacter.faction) {
                            if(owner.faction != null && targetCharacter.currentStructure != null && targetCharacter.currentStructure.isInterior && targetCharacter.currentStructure.settlementLocation != null
                                && targetCharacter.currentStructure.settlementLocation.owner == owner.faction) {
                                bool willReact = true;
                                switch (targetCharacter.currentStructure.structureType) {
                                    case STRUCTURE_TYPE.TAVERN:
                                    case STRUCTURE_TYPE.FARM:
                                    case STRUCTURE_TYPE.APOTHECARY:
                                    case STRUCTURE_TYPE.CEMETERY:
                                    case STRUCTURE_TYPE.CITY_CENTER:
                                        willReact = false;
                                        break;
                                }
                                if (willReact) {
                                    if (owner.marker) {
                                        if (!targetCharacter.traitContainer.HasTrait("Restrained", "Unconscious")) {
                                            owner.assumptionComponent.CreateAndReactToNewAssumption(targetCharacter, owner, INTERACTION_TYPE.TRESPASSING, REACTION_STATUS.WITNESSED);
                                        }
                                    }
                                }
                            }
                        }
                    }
                } else {
                    //if (owner.needsComponent.isStarving) {
                    //    owner.jobComponent.CreateButcherJob(targetCharacter);
                    //} else 
                    if(owner.isNormalCharacter 
                        && targetCharacter.isNormalCharacter 
                        && targetCharacter.gridTileLocation != null 
                        && (!targetCharacter.gridTileLocation.IsPartOfSettlement() || (targetCharacter.gridTileLocation.IsPartOfSettlement(out BaseSettlement settlement) && settlement.locationType != LOCATION_TYPE.SETTLEMENT))
                        && owner.relationshipContainer.GetOpinionLabel(targetCharacter) != RelationshipManager.Rival) {
                        //If a villager is dead and is seen outside the village, bury it
                        owner.jobComponent.TriggerPersonalBuryJob(targetCharacter);
                    }
                }
            }
            //if (targetPOI is Character || targetPOI is Tombstone) {
            //    Character targetCharacter = null;
            //    if (targetPOI is Character character) {
            //        targetCharacter = character;
            //    } else {
            //        targetCharacter = (targetPOI as Tombstone).character;
            //    }
            //    if (targetCharacter.isDead) {
            //        Dead deadTrait = targetCharacter.traitContainer.GetNormalTrait<Dead>("Dead");
            //        if (deadTrait != null && deadTrait.responsibleCharacter != characterThatWillDoJob 
            //                              && !deadTrait.charactersThatSawThisDead.Contains(characterThatWillDoJob)) {
            //            deadTrait.AddCharacterThatSawThisDead(characterThatWillDoJob);
                    
            //            // Log sawDeadLog = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "saw_dead");
            //            // sawDeadLog.AddToFillers(characterThatWillDoJob, characterThatWillDoJob.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            //            // sawDeadLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            //            // characterThatWillDoJob.logComponent.AddHistory(sawDeadLog);
            //            // PlayerManager.Instance.player.ShowNotificationFrom(sawDeadLog, characterThatWillDoJob, false);
            //            //
            //            //
            //            // if (characterThatWillDoJob.relationshipContainer.HasRelationshipWith(targetCharacter, RELATIONSHIP_TYPE.LOVER)) {
            //            //     characterThatWillDoJob.traitContainer.AddTrait(characterThatWillDoJob, "Heartbroken");
            //            //     bool hasCreatedJob = RandomizeBetweenShockAndCryJob(characterThatWillDoJob);
            //            //     //characterThatWillDoJob.needsComponent.AdjustHappiness(-6000);
            //            //     return hasCreatedJob;
            //            // } else if (characterThatWillDoJob.relationshipContainer.HasRelationshipWith(targetCharacter, RELATIONSHIP_TYPE.RELATIVE)) {
            //            //     characterThatWillDoJob.traitContainer.AddTrait(characterThatWillDoJob, "Griefstricken");
            //            //     bool hasCreatedJob = RandomizeBetweenShockAndCryJob(characterThatWillDoJob);
            //            //     //characterThatWillDoJob.needsComponent.AdjustHappiness(-4000);
            //            //     return hasCreatedJob;
            //            // } else if (characterThatWillDoJob.RelationshipManager.IsFriendsWith(targetCharacter)) {
            //            //     characterThatWillDoJob.traitContainer.AddTrait(characterThatWillDoJob, "Griefstricken");
            //            //     bool hasCreatedJob = CreatePrioritizedShockJob(characterThatWillDoJob);
            //            //     //characterThatWillDoJob.needsComponent.AdjustHappiness(-2000);
            //            //     return hasCreatedJob;
            //            // }
            //        }
            //    } else { 
            //        //character is not dead
            //        // if (targetCharacter.canMove == false || targetCharacter.canWitness == false) {
            //        //     if (characterThatWillDoJob.jobComponent.TryTriggerFeed(targetCharacter) == false) {
            //        //         if (characterThatWillDoJob.jobComponent.TryTriggerMoveCharacterTirednessRecovery(targetCharacter) == false) {
            //        //             characterThatWillDoJob.jobComponent.TryTriggerMoveCharacterHappinessRecovery(targetCharacter);
            //        //         }    
            //        //     }
            //        // }
            //        if (targetCharacter.race == RACE.SKELETON || targetCharacter.characterClass.className == "Zombie") {
            //            string opinionLabel = characterThatWillDoJob.relationshipContainer.GetOpinionLabel(targetCharacter);
            //            if (opinionLabel == RelationshipManager.Friend) {
            //                if (!charactersAlreadySawForHope.Contains(targetCharacter)) {
            //                    charactersAlreadySawForHope.Add(targetCharacter);
            //                    characterThatWillDoJob.needsComponent.AdjustHope(-5f);
            //                }
            //            } else if (opinionLabel == RelationshipManager.Close_Friend) {
            //                if (!charactersAlreadySawForHope.Contains(targetCharacter)) {
            //                    charactersAlreadySawForHope.Add(targetCharacter);
            //                    characterThatWillDoJob.needsComponent.AdjustHope(-10f);
            //                }
            //            }
            //        }
            //    }
            //}
            return base.OnSeePOI(targetPOI, characterThatWillDoJob);
        }
        private bool ShouldInspectItem(Character characterThatWillDoJob, TileObject item) {
            if (item is Excalibur excalibur) {
                return excalibur.lockedState == Excalibur.Locked_State.Locked && characterThatWillDoJob.isNormalCharacter && !excalibur.HasInspectedThis(characterThatWillDoJob);    
            } else if (item is BerserkOrb) {
                return true;
            } else if (item is GorgonEye) {
                return true;
            } else if (item is HeartOfTheWind) {
                return true;
            }
            return false;
        }
        public override bool OnStartPerformGoapAction(ActualGoapNode node, ref bool willStillContinueAction) {
            if(node.action.goapType == INTERACTION_TYPE.MAKE_LOVE) {
                bool triggered = node.actor.interruptComponent.TriggerInterrupt(INTERRUPT.Invite_To_Make_Love, node.poiTarget);
                willStillContinueAction = node.actor.interruptComponent.triggeredSimultaneousInterrupt.identifier == "Accept";
                //node.actor.interruptComponent.triggeredSimultaneousInterrupt.SetIdentifier(string.Empty);
                return triggered;
            }
            if (node.poiTarget.traitContainer.HasTrait("Booby Trapped")) {
                BoobyTrapped targetBoobyTrap = node.poiTarget.traitContainer.GetNormalTrait<BoobyTrapped>("Booby Trapped");
                bool triggered = targetBoobyTrap.OnPerformGoapAction(node, ref willStillContinueAction);
                if (triggered && node.actor.jobQueue.jobsInQueue.Count > 0) {
                    node.actor.jobQueue.jobsInQueue[0].CancelJob();
                }
                return triggered;
            }
            return false;
        }
        #endregion
    }
}

#region Save Data
public class SaveDataCharacterTrait : SaveDataTrait {

    public List<string> alreadyInspectedTileObjects;
    public List<string> charactersAlreadySawForHope;
    public List<string> charactersThatHaveReactedToThis;
    
    public override void Save(Trait trait) {
        base.Save(trait);
        CharacterTrait characterTrait = trait as CharacterTrait;
        Assert.IsNotNull(characterTrait);
        alreadyInspectedTileObjects = SaveUtilities.ConvertSavableListToIDs(characterTrait.alreadyInspectedTileObjects);
        charactersAlreadySawForHope = SaveUtilities.ConvertSavableListToIDs(characterTrait.charactersAlreadySawForHope);
        charactersThatHaveReactedToThis = SaveUtilities.ConvertSavableListToIDs(characterTrait.charactersThatHaveReactedToThis.ToList());
    }
}
#endregion

