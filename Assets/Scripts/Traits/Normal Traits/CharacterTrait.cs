using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Characters.Components;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using Locations.Settlements;
using Traits;
using UnityEngine.Assertions;
using UtilityScripts;

namespace Traits {
    //This trait is present in all characters
    //A dummy trait in order for some jobs to be created
    public class CharacterTrait : Trait, CharacterEventDispatcher.ITraitListener {
        //IMPORTANT NOTE: When the owner of this trait changed its alter ego, this trait will not be present in the alter ego anymore
        //Meaning that he/she cannot do the things specified in here anymore unless he/she switch to the ego which this trait is present
        public List<TileObject> alreadyInspectedTileObjects { get; private set; }
        public List<Character> charactersAlreadySawForHope { get; private set; }
        public HashSet<Character> charactersThatHaveReactedToThis { get; private set; }
        public List<TileObject> alreadyReactedToFoodPiles { get; private set; }
        private Dictionary<Character, List<string>> _traitsFromOtherCharacterThatThisIsAwareOf; 
        public Character owner { get; private set; }
        /// <summary>
        /// Has this character been abducted by the wild monster faction?
        /// NOTE: This is used in <see cref="ReactionComponent.ReactTo(IPointOfInterest,ref string)"/> to determine
        /// whether or not a wild monster will attack (On vision) the character that owns this.
        /// Example: Character that has been abducted by a Giant Spider
        /// </summary>
        public bool hasBeenAbductedByWildMonster { get; private set; }
        /// <summary>
        /// Has this character been abducted by the player faction?
        /// NOTE: This is used in <see cref="ReactionComponent.ReactTo(IPointOfInterest,ref string)"/> to determine
        /// whether or not a member of the player faction will attack (On vision) the character that owns this.
        /// Example: Character that has been abducted by an Abductor Monster
        /// </summary>
        public bool hasBeenAbductedByPlayerMonster { get; private set; }

        #region getters
        public override Type serializedData => typeof(SaveDataCharacterTrait);
        public Dictionary<Character, List<string>> traitsFromOtherCharacterThatThisIsAwareOf => _traitsFromOtherCharacterThatThisIsAwareOf;
        #endregion

        public CharacterTrait() {
            name = "Character Trait";
            type = TRAIT_TYPE.NEUTRAL;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            isHidden = true;
            hasBeenAbductedByWildMonster = false;
            hasBeenAbductedByPlayerMonster = false;
            alreadyInspectedTileObjects = new List<TileObject>();
            charactersAlreadySawForHope = new List<Character>();
            charactersThatHaveReactedToThis = new HashSet<Character>();
            alreadyReactedToFoodPiles = new List<TileObject>();
            _traitsFromOtherCharacterThatThisIsAwareOf = new Dictionary<Character, List<string>>();
            AddTraitOverrideFunctionIdentifier(TraitManager.Start_Perform_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.See_Poi_Trait);
        }

        #region Loading
        public override void LoadSecondWaveInstancedTrait(SaveDataTrait p_saveDataTrait) {
            base.LoadSecondWaveInstancedTrait(p_saveDataTrait);
            SaveDataCharacterTrait saveDataCharacterTrait = p_saveDataTrait as SaveDataCharacterTrait;
            Assert.IsNotNull(saveDataCharacterTrait);
            alreadyInspectedTileObjects = SaveUtilities.ConvertIDListToTileObjects(saveDataCharacterTrait.alreadyInspectedTileObjects);
            charactersAlreadySawForHope.AddRange(SaveUtilities.ConvertIDListToCharacters(saveDataCharacterTrait.charactersAlreadySawForHope));
            charactersThatHaveReactedToThis = new HashSet<Character>(SaveUtilities.ConvertIDListToCharacters(saveDataCharacterTrait.charactersThatHaveReactedToThis));
            if (saveDataCharacterTrait.alreadyReactedFoodPiles != null) {
                alreadyReactedToFoodPiles = SaveUtilities.ConvertIDListToTileObjects(saveDataCharacterTrait.alreadyReactedFoodPiles);    
            }
            hasBeenAbductedByPlayerMonster = saveDataCharacterTrait.hasBeenAbductedByPlayerMonster;
            hasBeenAbductedByWildMonster = saveDataCharacterTrait.hasBeenAbductedByWildMonster;
            if (saveDataCharacterTrait.traitsFromOtherCharacterThatThisIsAwareOf != null) {
                foreach (var kvp in saveDataCharacterTrait.traitsFromOtherCharacterThatThisIsAwareOf) {
                    Character character = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(kvp.Key);
                    _traitsFromOtherCharacterThatThisIsAwareOf.Add(character, kvp.Value);
                    character.eventDispatcher.SubscribeToCharacterLostTrait(this);
                }    
            }
        }
        #endregion
        
        public void AddAlreadyInspectedObject(TileObject to) {
            if (!alreadyInspectedTileObjects.Contains(to)) {
                alreadyInspectedTileObjects.Add(to);
            }
        }
        public bool HasAlreadyInspectedObject(TileObject to) {
            return alreadyInspectedTileObjects.Contains(to);
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
            if (targetPOI is TileObject item && characterThatWillDoJob.limiterComponent.canMove) {
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
                        CRIME_SEVERITY severity = CRIME_SEVERITY.None;
                        if (characterThatWillDoJob.faction != null) {
                            severity = characterThatWillDoJob.faction.factionType.GetCrimeSeverity(CRIME_TYPE.Demon_Worship);
                        }
                        if (severity != CRIME_SEVERITY.None && severity != CRIME_SEVERITY.Unapplicable) {
                            characterThatWillDoJob.jobComponent.TriggerDestroy(item);
                        }
                    }
                } else if (ShouldInspectItem(characterThatWillDoJob, item)) {
                    if (!characterThatWillDoJob.jobQueue.HasJob(JOB_TYPE.INSPECT, item) &&
                        !characterThatWillDoJob.jobComponent.HasHigherPriorityJobThan(JOB_TYPE.INSPECT)) {
                        characterThatWillDoJob.jobComponent.TriggerInspect(item);
                    }
                } else if (item.traitContainer.HasTrait("Edible") && characterThatWillDoJob.needsComponent.isStarving /*&& characterThatWillDoJob.limiterComponent.canDoFullnessRecovery*/ && 
                           !characterThatWillDoJob.traitContainer.HasTrait("Vampire") && !characterThatWillDoJob.traitContainer.HasTrait("Paralyzed")) {
                    characterThatWillDoJob.jobComponent.CreateFullnessRecoveryOnSight(item);
                } else if (!characterThatWillDoJob.IsInventoryAtFullCapacity() && (item.traitContainer.HasTrait("Treasure")) // characterThatWillDoJob.IsItemInteresting(item.name) || 
                    && !characterThatWillDoJob.jobComponent.HasHigherPriorityJobThan(JOB_TYPE.TAKE_ITEM) && characterThatWillDoJob.traitContainer.HasTrait("Suspicious") == false) {
                    //NOTE: Added checker if character can move, so that Paralyzed characters will not try to pick up items
                    if (item.CanBePickedUpNormallyUponVisionBy(characterThatWillDoJob)
                        && !characterThatWillDoJob.jobQueue.HasJob(JOB_TYPE.TAKE_ITEM)) {
                        int chance = 100;
                        if (characterThatWillDoJob.HasItem(item.name) || characterThatWillDoJob.HasOwnedItemInHomeStructure(item.name)) {
                            chance = 10;
                            int itemCount = characterThatWillDoJob.GetItemCount(item.name) + characterThatWillDoJob.GetNumOfOwnedItemsInHomeStructure(item.name);
                            if (itemCount >= 2) {
                                chance = 0;
                            }
                        }
                        if (UnityEngine.Random.Range(0, 100) < chance) {
                            if (item.characterOwner != null &&  !item.IsOwnedBy(characterThatWillDoJob)) {
                                characterThatWillDoJob.jobComponent.CreateStealItemJob(JOB_TYPE.TAKE_ITEM, item);
                            } else {
                                characterThatWillDoJob.jobComponent.CreateTakeItemJob(JOB_TYPE.TAKE_ITEM, item);    
                            }
                            return true;
                        }
                        // if (item.tileObjectType == TILE_OBJECT_TYPE.HERB_PLANT) {
                        //     HerbPlantProcessing(characterThatWillDoJob, item);
                        // }
                    }
                }
                // else if (item.tileObjectType == TILE_OBJECT_TYPE.HERB_PLANT) {
                //     HerbPlantProcessing(characterThatWillDoJob, item);
                // } 
                //else if (characterThatWillDoJob.partyComponent.hasParty) {
                //    if (characterThatWillDoJob.partyComponent.currentParty.isActive && characterThatWillDoJob.partyComponent.currentParty.currentQuest is HeirloomHuntPartyQuest quest) {
                //        if (quest.targetHeirloom == item) {
                //            quest.SetFoundHeirloom(true);
                //            characterThatWillDoJob.jobComponent.CreateDropItemJob(JOB_TYPE.DROP_ITEM_PARTY, quest.targetHeirloom, quest.targetHeirloom.structureSpot, true);
                //        }
                //    }
                //}
                else if (item.tileObjectType.IsDemonicStructureTileObject() && item.gridTileLocation?.structure is DemonicStructure demonicStructure) {
                    bool wasReportJobCreated = false;
                    if (WorldSettings.Instance.worldSettingsData.IsRetaliationAllowed() &&
                        !PlayerManager.Instance.player.retaliationComponent.isRetaliating &&
                        !PlayerManager.Instance.player.HasAlreadyReportedADemonicStructure(characterThatWillDoJob) &&
                        characterThatWillDoJob.limiterComponent.canWitness && !characterThatWillDoJob.behaviourComponent.isAttackingDemonicStructure && 
                        characterThatWillDoJob.homeSettlement != null && characterThatWillDoJob.necromancerTrait == null && characterThatWillDoJob.race.IsSapient() && 
                        characterThatWillDoJob.hasMarker && characterThatWillDoJob.carryComponent.IsNotBeingCarried() && !characterThatWillDoJob.isAlliedWithPlayer && 
                        (!characterThatWillDoJob.partyComponent.hasParty || !characterThatWillDoJob.partyComponent.currentParty.isActive || 
                         (characterThatWillDoJob.partyComponent.currentParty.currentQuest.partyQuestType != PARTY_QUEST_TYPE.Counterattack && 
                          !(characterThatWillDoJob.partyComponent.currentParty.currentQuest is IRescuePartyQuest))) && 
                        (Tutorial.TutorialManager.Instance.hasCompletedImportantTutorials || WorldSettings.Instance.worldSettingsData.worldType != WorldSettingsData.World_Type.Tutorial)) {
                        if (characterThatWillDoJob.faction != null && characterThatWillDoJob.faction.isMajorNonPlayer) {
                            //!characterThatWillDoJob.faction.partyQuestBoard.HasPartyQuest(PARTY_QUEST_TYPE.Counterattack) - Removed checking because characters no longer create counter attack quests
                            //&& !characterThatWillDoJob.faction.HasActiveReportDemonicStructureJob(demonicStructure) - Removed checking because we now allow characters to create multiple report jobs
                            wasReportJobCreated = characterThatWillDoJob.jobComponent.CreateReportDemonicStructure(demonicStructure);
                            if (wasReportJobCreated) {
                                Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "General", "Player", "structure_discovered", null, LOG_TAG.Player, LOG_TAG.Major);
                                log.AddToFillers(characterThatWillDoJob, characterThatWillDoJob.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                                log.AddToFillers(demonicStructure, demonicStructure.name, LOG_IDENTIFIER.LANDMARK_1);
                                log.AddLogToDatabase();
                                PlayerManager.Instance.player.ShowNotificationFromPlayer(log, true);
                            }
                        }
                    }
                    
                    if (!wasReportJobCreated && characterThatWillDoJob.limiterComponent.canWitness && !characterThatWillDoJob.behaviourComponent.isAttackingDemonicStructure &&
                        !PlayerManager.Instance.player.HasAlreadyReportedADemonicStructure(characterThatWillDoJob) &&
                        (!characterThatWillDoJob.partyComponent.hasParty || !characterThatWillDoJob.partyComponent.currentParty.isActive || 
                         (characterThatWillDoJob.partyComponent.currentParty.currentQuest.partyQuestType != PARTY_QUEST_TYPE.Counterattack && 
                          !(characterThatWillDoJob.partyComponent.currentParty.currentQuest is IRescuePartyQuest) && 
                          characterThatWillDoJob.partyComponent.currentParty.currentQuest.partyQuestType != PARTY_QUEST_TYPE.Heirloom_Hunt)) && 
                        !characterThatWillDoJob.isAlliedWithPlayer && 
                        characterThatWillDoJob.necromancerTrait == null && 
                        !characterThatWillDoJob.jobQueue.HasJob(JOB_TYPE.REPORT_CORRUPTED_STRUCTURE)) {
                        if (!characterThatWillDoJob.movementComponent.hasMovedOnCorruption) {
                            if (characterThatWillDoJob.isNormalCharacter) {
                                //Instead of fleeing when characterThatWillDoJob steps on a corrupted tile, trigger Shocked interrupt only
                                //The reason for this is to eliminate the bug wherein the characterThatWillDoJob will flee from the corrupted tile, then after fleeing, he will again move across it, thus triggering flee again, which results in unending loop of fleeing and moving
                                //So to eliminate this behaviour we will not let the characterThatWillDoJob flee, but will trigger Shocked interrupt only and then go on with his job/action
                                //https://trello.com/c/yiW344Sb/2499-villagers-fleeing-from-demonic-area-can-get-stuck-repeating-it
                                characterThatWillDoJob.interruptComponent.TriggerInterrupt(INTERRUPT.Shocked, characterThatWillDoJob, reason: $"saw {demonicStructure.name}");
                            }    
                        }
                    }
                }
                //if (characterThatWillDoJob.partyComponent.hasParty && characterThatWillDoJob.partyComponent.currentParty.isActive
                //    && owner.partyComponent.currentParty.partyState == PARTY_STATE.Working) {
                //    if (characterThatWillDoJob.partyComponent.currentParty.currentQuest is RaidPartyQuest raidParty && item is ResourcePile resourcePile 
                //        && resourcePile.gridTileLocation != null && resourcePile.gridTileLocation.IsPartOfSettlement(raidParty.targetSettlement)) {
                //        if (UnityEngine.Random.Range(0, 100) < 35) {
                //            if (owner.jobComponent.TriggerStealRaidJob(resourcePile)) {
                //                raidParty.SetIsSuccessful(true);
                //            }
                //        }
                //    }
                //}
            }
            if(targetPOI is Character targetCharacter) {
                if (characterThatWillDoJob.limiterComponent.canMove && characterThatWillDoJob.limiterComponent.canPerform) {
                    if (owner.partyComponent.hasParty && owner.partyComponent.currentParty.isActive) {
                        if (owner.partyComponent.currentParty.currentQuest is IRescuePartyQuest rescueParty) {
                            if (rescueParty.targetCharacter == targetCharacter) {
                                if (!targetCharacter.isDead) {
                                    if (targetCharacter.traitContainer.HasTrait("Restrained", "Unconscious", "Frozen", "Ensnared", "Enslaved")) {
                                        if (owner.jobComponent.TriggerReleaseJob(targetCharacter)) {
                                            rescueParty.SetIsReleasing(true);
                                        }
                                    } else {
                                        //rescueParty.SetIsSuccessful(true);
                                        rescueParty.SetIsReleasing(false);
                                        rescueParty.EndQuest("Target is safe");

                                        //if target is paralyzed carry back home
                                        if (targetCharacter.traitContainer.HasTrait("Paralyzed")) {
                                            if (!targetCharacter.IsPOICurrentlyTargetedByAPerformingAction(JOB_TYPE.MOVE_CHARACTER)) {
                                                //Do not set this as a party job
                                                owner.jobComponent.TryTriggerMoveCharacter(targetCharacter, false);
                                            }
                                        }
                                    }
                                } else {
                                    rescueParty.SetIsReleasing(false);
                                    rescueParty.EndQuest("Target is already dead");
                                }
                            }
                        }
                    }
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
                        } 
                        //else {
                        //    //If a restrained Villager from same Faction or other allied Faction was seen: Release
                        //    if (targetCharacter.traitContainer.HasTrait("Restrained")) {
                        //        if (owner.partyComponent.hasParty && owner.partyComponent.currentParty.isActive) {
                        //            if (owner.faction != null && owner.partyComponent.currentParty.partyState == PARTY_STATE.Working) {
                        //                if (owner.partyComponent.currentParty.currentQuest is ExplorationPartyQuest || owner.partyComponent.currentParty.currentQuest is ExterminationPartyQuest) {
                        //                    if (owner.faction == targetCharacter.faction || owner.faction.IsFriendlyWith(targetCharacter.faction)) {
                        //                        owner.jobComponent.TriggerReleaseJob(targetCharacter);
                        //                    }
                        //                }
                        //            }
                        //        }
                        //    }
                        //    if (targetCharacter.traitContainer.HasTrait("Restrained", "Unconscious", "Frozen", "Ensnared", "Enslaved")) {
                        //        if (owner.partyComponent.hasParty && owner.partyComponent.currentParty.isActive) {
                        //            if (owner.faction != null && owner.faction != targetCharacter.faction && owner.partyComponent.currentParty.partyState == PARTY_STATE.Working) {
                        //                if (owner.partyComponent.currentParty.currentQuest is ExplorationPartyQuest || owner.partyComponent.currentParty.currentQuest is ExterminationPartyQuest) {
                        //                    if (owner.faction.factionType.HasIdeology(FACTION_IDEOLOGY.Warmonger)) {
                        //                        if (GameUtilities.RollChance(ChanceData.GetChance(CHANCE_TYPE.Explore_Kidnap_Chance))) {
                        //                            owner.jobComponent.TriggerKidnapJob(targetCharacter);
                        //                        }
                        //                    } else if (owner.faction.factionType.HasIdeology(FACTION_IDEOLOGY.Peaceful) && !owner.faction.IsHostileWith(targetCharacter.faction)) {
                        //                        owner.jobComponent.TriggerReleaseJob(targetCharacter);
                        //                    }
                        //                }
                        //            }
                        //        }
                        //    }
                        //    if ((!targetCharacter.limiterComponent.canPerform || !targetCharacter.limiterComponent.canMove) && !owner.combatComponent.isInCombat && owner.partyComponent.hasParty && owner.partyComponent.currentParty.isActive && owner.partyComponent.currentParty.partyState == PARTY_STATE.Working) {
                        //        if (owner.partyComponent.currentParty.currentQuest is RaidPartyQuest raidParty
                        //            && targetCharacter.homeSettlement == raidParty.targetSettlement
                        //            && (targetCharacter.faction == null || owner.faction == null || owner.faction.IsHostileWith(targetCharacter.faction))) {
                        //            if (GameUtilities.RollChance(ChanceData.GetChance(CHANCE_TYPE.Raid_Kidnap_Chance))) {
                        //                if (!owner.jobQueue.HasJob(JOB_TYPE.STEAL_RAID)) {
                        //                    if (owner.jobComponent.TriggerKidnapRaidJob(targetCharacter)) {
                        //                        raidParty.SetIsSuccessful(true);
                        //                    }
                        //                }
                        //            }
                        //        }
                        //    }
                        //}
                    } else {
                        //if (owner.needsComponent.isStarving) {
                        //    owner.jobComponent.CreateButcherJob(targetCharacter);
                        //} else 
                        if (owner.isNormalCharacter
                            && targetCharacter.isNormalCharacter
                            && targetCharacter.gridTileLocation != null
                            && (!targetCharacter.gridTileLocation.IsPartOfSettlement() || (targetCharacter.gridTileLocation.IsPartOfSettlement(out BaseSettlement settlement) && settlement.locationType != LOCATION_TYPE.VILLAGE))
                            && owner.relationshipContainer.GetOpinionLabel(targetCharacter) != RelationshipManager.Rival) {
                            //If a villager is dead and is seen outside the village, bury it
                            if (owner.partyComponent.isMemberThatJoinedQuest) {
                                owner.jobComponent.TriggerPersonalBuryInActivePartyJob(targetCharacter);
                            } else {
                                if (owner.traitContainer.HasTrait("Necromancer")) {
                                    if (owner.faction.factionType.type != FACTION_TYPE.Undead) {
                                        owner.jobComponent.TriggerPersonalOutsideVillageBuryJob(targetCharacter);
                                    }
                                } else {
                                    owner.jobComponent.TriggerPersonalOutsideVillageBuryJob(targetCharacter);
                                }
                            }
                        }
                    }
                }
                if (!targetCharacter.isDead && owner.isNormalCharacter && targetCharacter.isNormalCharacter && owner.faction != targetCharacter.faction) {
                    if (owner.faction != null && targetCharacter.currentStructure != null && targetCharacter.currentStructure.isInterior && targetCharacter.currentStructure.settlementLocation != null
                        && targetCharacter.currentStructure.settlementLocation.owner == owner.faction) {
                        bool willReact = true;
                        switch (targetCharacter.currentStructure.structureType) {
                            case STRUCTURE_TYPE.TAVERN:
                            case STRUCTURE_TYPE.FARM:
                            case STRUCTURE_TYPE.HOSPICE:
                            case STRUCTURE_TYPE.CEMETERY:
                            case STRUCTURE_TYPE.CITY_CENTER:
                                willReact = false;
                                break;
                        }
                        if (willReact) {
                            if (owner.hasMarker) {
                                if (!targetCharacter.traitContainer.HasTrait("Restrained", "Unconscious")) {
                                    //If character considers the target a prisoner, do not assume trespassing
                                    //This might happen because if there is still no prison, the designated prison of the settlement is the city center
                                    //When a prisoner is seen in there the other characters might assume that he is trespassing when in fact he is not because he is imprisoned
                                    //So if the character that saw him considers him a prisoner, he must never assume that the character is imprisoned
                                    bool willCreateAssumption = true;
                                    if (targetCharacter.traitContainer.HasTrait("Prisoner")) {
                                        Prisoner prisoner = targetCharacter.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner");
                                        if (prisoner.IsConsideredPrisonerOf(owner)) {
                                            willCreateAssumption = false;
                                        }
                                    }
                                    if (willCreateAssumption && targetCharacter.isVagrant) {
                                        //removed vagrant trespassing because it causes an issue whenever a character leaves its current faction while it is still inside its previous settlement.
                                        willCreateAssumption = false;
                                    }
                                    if (willCreateAssumption && targetCharacter.traitContainer.HasTrait("Cultist") && owner.traitContainer.HasTrait("Cultist")) {
                                        //Do not assume that character is trespassing if both characters are cultists.
                                        //This is a fix for this issue: https://trello.com/c/SBNxYdlY/3085-live-03363-cultist-reporting-other-cultists
                                        willCreateAssumption = false;
                                    }
                                    if (willCreateAssumption && owner.relationshipContainer.IsFriendsWith(targetCharacter)) {
                                        //Fix for: https://trello.com/c/Ab3P6jFo/4627-enslaved-friend-accused-of-trespassing
                                        willCreateAssumption = false;
                                    }
                                    if (willCreateAssumption) {
                                        owner.assumptionComponent.CreateAndReactToNewAssumption(targetCharacter, owner, INTERACTION_TYPE.TRESPASSING, REACTION_STATUS.WITNESSED);
                                    }
                                }
                            }
                        }
                    }
                }
            }
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
        private void HerbPlantProcessing(Character actor, TileObject herbPlant) {
            NPCSettlement homeSettlement = actor.homeSettlement;
            if(homeSettlement != null) {
                LocationStructure cityCenter = homeSettlement.GetFirstStructureOfType(STRUCTURE_TYPE.CITY_CENTER);
                if(cityCenter != null && herbPlant.gridTileLocation != null && herbPlant.gridTileLocation.structure.structureType != STRUCTURE_TYPE.CITY_CENTER && herbPlant.gridTileLocation.structure.structureType != STRUCTURE_TYPE.HOSPICE) {
                    int numOfHerbPlantsInCityCenter = cityCenter.GetNumberOfTileObjects(TILE_OBJECT_TYPE.HERB_PLANT);
                    int numberOfHaulJobs = homeSettlement.GetNumberOfJobsThatTargetsTileObjectOfType(TILE_OBJECT_TYPE.HERB_PLANT);
                    if((numOfHerbPlantsInCityCenter + numberOfHaulJobs) < 4) {
                        homeSettlement.settlementJobTriggerComponent.TryCreateHaulJobForItems(herbPlant, cityCenter);
                    }
                }
            }
        }
        public override bool OnStartPerformGoapAction(ActualGoapNode node, ref bool willStillContinueAction) {
            //if(node.action.goapType == INTERACTION_TYPE.INVITE) {
            //    bool triggered = node.actor.interruptComponent.TriggerInterrupt(INTERRUPT.Invite_To_Make_Love, node.poiTarget);
            //    willStillContinueAction = node.actor.interruptComponent.triggeredSimultaneousInterrupt.identifier == "Accept";
            //    //node.actor.interruptComponent.triggeredSimultaneousInterrupt.SetIdentifier(string.Empty);
            //    return triggered;
            //}
            if (node.poiTarget.traitContainer.HasTrait("Booby Trapped")) {
                BoobyTrapped targetBoobyTrap = node.poiTarget.traitContainer.GetTraitOrStatus<BoobyTrapped>("Booby Trapped");
                bool triggered = targetBoobyTrap.OnPerformGoapAction(node, ref willStillContinueAction);
                if (triggered && !node.hasBeenReset && node.actor.jobQueue.jobsInQueue.Count > 0) {
                    node.actor.jobQueue.jobsInQueue[0].CancelJob();
                }
                return triggered;
            }
            return false;
        }
        #endregion

        public void SetHasBeenAbductedByPlayerMonster(bool state) {
            hasBeenAbductedByPlayerMonster = state;
        }
        public void SetHasBeenAbductedByWildMonster(bool state) {
            hasBeenAbductedByWildMonster = state;
        }

        #region Trait Awareness
        public void BecomeAwareOfTrait(Character p_character, Trait p_trait) {
            if (!traitsFromOtherCharacterThatThisIsAwareOf.ContainsKey(p_character)) {
                traitsFromOtherCharacterThatThisIsAwareOf.Add(p_character, new List<string>());
                p_character.eventDispatcher.SubscribeToCharacterLostTrait(this);
            }
            traitsFromOtherCharacterThatThisIsAwareOf[p_character].Add(p_trait.name);
#if DEBUG_LOG
            Debug.Log($"{GameManager.Instance.TodayLogString()}{owner.name} has become aware of {p_character.name}'s trait {p_trait.name}");
#endif
        }
        public bool IsAwareOfTrait(Character p_character, Trait p_trait) {
            if (traitsFromOtherCharacterThatThisIsAwareOf.ContainsKey(p_character)) {
                return traitsFromOtherCharacterThatThisIsAwareOf[p_character].Contains(p_trait.name);
            }
            return false;
        }
#endregion

#region CharacterEventDispatcher.ITraitListener Implementation
        public void OnCharacterGainedTrait(Character p_character, Trait p_gainedTrait) { }
        public void OnCharacterLostTrait(Character p_character, Trait p_lostTrait, Character p_removedBy) {
            if (traitsFromOtherCharacterThatThisIsAwareOf.ContainsKey(p_character)) {
                if (traitsFromOtherCharacterThatThisIsAwareOf[p_character].Remove(p_lostTrait.name)) {
#if DEBUG_LOG
                    Debug.Log($"{GameManager.Instance.TodayLogString()}{owner.name} has lost awareness of {p_character.name}'s trait {p_lostTrait.name} because that trait was removed!");
#endif
                    if (traitsFromOtherCharacterThatThisIsAwareOf[p_character].Count == 0) {
                        traitsFromOtherCharacterThatThisIsAwareOf.Remove(p_character);
                        p_character.eventDispatcher.UnsubscribeToCharacterLostTrait(this);    
                    }
                }
            }
        }
#endregion

#region Food Piles
        public void AddFoodPileAsReactedTo(FoodPile p_foodPile) {
            alreadyReactedToFoodPiles.Add(p_foodPile);
        }
        public void RemoveFoodPileAsReactedTo(FoodPile p_foodPile) {
            alreadyReactedToFoodPiles.Remove(p_foodPile);
        }
        public bool HasAlreadyReactedToFoodPile(FoodPile p_foodPile) {
            return alreadyReactedToFoodPiles.Contains(p_foodPile);
        }
#endregion
    }
}

#region Save Data
public class SaveDataCharacterTrait : SaveDataTrait {

    public List<string> alreadyInspectedTileObjects;
    public List<string> charactersAlreadySawForHope;
    public List<string> charactersThatHaveReactedToThis;
    public List<string> alreadyReactedFoodPiles;
    public bool hasBeenAbductedByPlayerMonster;
    public bool hasBeenAbductedByWildMonster;
    public Dictionary<string, List<string>> traitsFromOtherCharacterThatThisIsAwareOf;
    
    public override void Save(Trait trait) {
        base.Save(trait);
        CharacterTrait characterTrait = trait as CharacterTrait;
        Assert.IsNotNull(characterTrait);
        alreadyInspectedTileObjects = SaveUtilities.ConvertSavableListToIDs(characterTrait.alreadyInspectedTileObjects);
        charactersAlreadySawForHope = SaveUtilities.ConvertSavableListToIDs(characterTrait.charactersAlreadySawForHope);
        charactersThatHaveReactedToThis = SaveUtilities.ConvertSavableListToIDs(characterTrait.charactersThatHaveReactedToThis.ToList());
        alreadyReactedFoodPiles = SaveUtilities.ConvertSavableListToIDs(characterTrait.alreadyReactedToFoodPiles);
        hasBeenAbductedByPlayerMonster = characterTrait.hasBeenAbductedByPlayerMonster;
        hasBeenAbductedByWildMonster = characterTrait.hasBeenAbductedByWildMonster;
        traitsFromOtherCharacterThatThisIsAwareOf = new Dictionary<string, List<string>>();
        foreach (var kvp in characterTrait.traitsFromOtherCharacterThatThisIsAwareOf) {
            string characterID = kvp.Key.persistentID;
            List<string> traits = kvp.Value;
            traitsFromOtherCharacterThatThisIsAwareOf.Add(characterID, traits);
        }
    }
}
#endregion

