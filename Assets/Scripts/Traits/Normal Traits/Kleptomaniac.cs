using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;
using UnityEngine.Assertions;
using Locations.Settlements;

namespace Traits {
    public class Kleptomaniac : Trait {
        private Character traitOwner;

        public Kleptomaniac() {
            name = "Kleptomaniac";
            description = "Cannot stop itself from stealing things.";
            type = TRAIT_TYPE.FLAW;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = 0;
            canBeTriggered = true;
            //AddTraitOverrideFunctionIdentifier(TraitManager.Execute_Expected_Effect_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.Death_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.See_Poi_Trait);
        }

        #region Overrides
        public override void OnAddTrait(ITraitable sourceCharacter) {
            base.OnAddTrait(sourceCharacter);
            traitOwner = sourceCharacter as Character;
            traitOwner.behaviourComponent.AddBehaviourComponent(typeof(KleptomaniacBehaviour));
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            traitOwner.behaviourComponent.RemoveBehaviourComponent(typeof(KleptomaniacBehaviour));
        }
        public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
            base.LoadTraitOnLoadTraitContainer(addTo);
            traitOwner = addTo as Character;
        }
        public override bool OnSeePOI(IPointOfInterest targetPOI, Character characterThatWillDoJob) {
            if (targetPOI is Character targetCharacter) {
                PLAYER_SKILL_TYPE playerSkillType = GetAfflictionSkillType();
                if (playerSkillType != PLAYER_SKILL_TYPE.NONE && characterThatWillDoJob.HasAfflictedByPlayerWith(playerSkillType)) {
                    //affliction was applied by player
                    PlayerSkillData playerSkillData = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(playerSkillType);
                    SkillData skillData = PlayerSkillManager.Instance.GetSkillData(playerSkillType);
                    if (playerSkillData.afflictionUpgradeData.HasAddedBehaviourForLevel(AFFLICTION_SPECIFIC_BEHAVIOUR.Do_Pick_Pocket, skillData.currentLevel)) {
                        var chanceMet = ChanceData.RollChance(skillData.currentLevel == 1 ? CHANCE_TYPE.Kleptomania_Pickpocket_Level_1 : CHANCE_TYPE.Kleptomania_Pickpocket_Level_2);
                        if (chanceMet && !characterThatWillDoJob.IsHostileWith(targetCharacter) && 
                            !characterThatWillDoJob.relationshipContainer.IsFriendsWith(targetCharacter)) {
                            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.KLEPTOMANIAC_STEAL, INTERACTION_TYPE.PICKPOCKET, targetCharacter, characterThatWillDoJob);
                            characterThatWillDoJob.jobQueue.AddJobInQueue(job);
                        }
                    }
                }    
            }
            return base.OnSeePOI(targetPOI, characterThatWillDoJob);
        }
        public override string TriggerFlaw(Character character) {
            //The character will begin Hunt for Blood.
            if (!character.jobQueue.HasJob(JOB_TYPE.TRIGGER_FLAW)) {
                bool triggerBrokenhearted = false;
                Heartbroken heartbroken = character.traitContainer.GetTraitOrStatus<Heartbroken>("Heartbroken");
                if (heartbroken != null) {
                    triggerBrokenhearted = UnityEngine.Random.Range(0, 100) < (25 * character.traitContainer.stacks[heartbroken.name]);
                }
                if (!triggerBrokenhearted) {
                    if (character.jobQueue.HasJob(JOB_TYPE.HAPPINESS_RECOVERY)) {
                        character.jobQueue.CancelAllJobs(JOB_TYPE.HAPPINESS_RECOVERY);
                    }

                    //List<TileObject> choices = new List<TileObject>();
                    //for (int i = 0; i < character.currentRegion.charactersAtLocation.Count; i++) {
                    //    Character otherCharacter = character.currentRegion.charactersAtLocation[i];
                    //    for (int j = 0; j < otherCharacter.items.Count; j++) {
                    //        TileObject currItem = otherCharacter.items[j];
                    //        if (CanBeStolen(currItem)) {
                    //            choices.Add(currItem);
                    //        }
                    //    }
                    //}

                    //NOTE: Might be heavy on performance, optimize this!
                    //foreach (KeyValuePair<STRUCTURE_TYPE,List<LocationStructure>> pair in character.currentRegion.structures) {
                    //    for (int i = 0; i < pair.Value.Count; i++) {
                    //        LocationStructure structure = pair.Value[i];
                    //        for (int j = 0; j < structure.pointsOfInterest.Count; j++) {
                    //            IPointOfInterest poi = structure.pointsOfInterest.ElementAt(j);
                    //            if (poi.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
                    //                TileObject item = poi as TileObject;
                    //                if (CanBeStolen(item)) {
                    //                    choices.Add(item);
                    //                }
                    //            }
                    //        }
                    //    }
                    //}
                    List<Character> choices = RuinarchListPool<Character>.Claim();
                    BaseSettlement currentSettlement = character.currentSettlement;
                    if (currentSettlement != null) {
                        //Pickpocket only from characters inside Settlement, if not inside a settlement, cannot target anyone
                        for (int i = 0; i < currentSettlement.areas.Count; i++) {
                            Area area = currentSettlement.areas[i];
                            for (int j = 0; j < area.locationCharacterTracker.charactersAtLocation.Count; j++) {
                                Character otherCharacter = area.locationCharacterTracker.charactersAtLocation[j];
                                if (character != otherCharacter && otherCharacter.hasMarker && !otherCharacter.isBeingSeized && (otherCharacter.HasItem() || otherCharacter.moneyComponent.HasCoins())) {
                                    choices.Add(otherCharacter);
                                }
                            }
                        }
                        //for (int i = 0; i < character.currentSettlement.SettlementResources.characters.Count; i++) {
                        //    Character otherCharacter = character.currentSettlement.SettlementResources.characters[i];
                        //    if (character != otherCharacter && otherCharacter.hasMarker && !otherCharacter.isBeingSeized && (otherCharacter.HasItem() || otherCharacter.moneyComponent.HasCoins())) {
                        //        choices.Add(otherCharacter);
                        //    }
                        //}    
                    }
                    if (choices.Count > 0) {
                        Character target = CollectionUtilities.GetRandomElement(choices);
                        LocationGridTile targetTile = target.gridTileLocation;
                        //if(target.isBeingCarriedBy != null) {
                        //    targetTile = target.isBeingCarriedBy.gridTileLocation;
                        //}
                        if (!character.movementComponent.HasPathToEvenIfDiffRegion(targetTile)) {
                            return "no_path_to_target";
                        } else {
                            bool stealCoins = false;
                            bool stealItem = false;
                            bool hasItem = target.HasItem();
                            bool hasCoins = target.moneyComponent.HasCoins();
                            if (hasItem && hasCoins) {
                                if (GameUtilities.RandomBetweenTwoNumbers(0, 1) == 0) {
                                    stealCoins = true;
                                } else {
                                    stealItem = true;
                                }
                            } else if (hasItem) {
                                stealItem = true;
                            } else if (hasCoins) {
                                stealCoins = true;
                            }
                            if (stealItem) {
                                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.TRIGGER_FLAW, INTERACTION_TYPE.PICKPOCKET, target, character);
                                //GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.TRIGGER_FLAW, INTERACTION_TYPE.STEAL, target, character);
                                Assert.IsNotNull(character.currentSettlement);
                                job.AddPriorityLocation(INTERACTION_TYPE.PICKPOCKET, character.currentSettlement);
                                character.jobQueue.AddJobInQueue(job);
                            } else if (stealCoins) {
                                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.TRIGGER_FLAW, INTERACTION_TYPE.STEAL_COINS, target, character);
                                character.jobQueue.AddJobInQueue(job);
                            }

                        }
                    } else {
                        return "no_target";
                    }
                    RuinarchListPool<Character>.Release(choices);
                } else {
                    heartbroken.TriggerBrokenhearted();
                }
            }
            return base.TriggerFlaw(character);
        }
        public override void ExecuteCostModification(INTERACTION_TYPE action, Character actor, IPointOfInterest poiTarget, OtherData[] otherData, ref int cost) {
            if (action == INTERACTION_TYPE.STEAL || action == INTERACTION_TYPE.PICKPOCKET || action == INTERACTION_TYPE.STEAL_ANYTHING || action == INTERACTION_TYPE.STEAL_COINS) {
                cost = 0;//Utilities.rng.Next(5, 10);//5,46
            } else if (action == INTERACTION_TYPE.PICK_UP) {
                cost = 10000;//Utilities.rng.Next(5, 10);//5,46
            }
        }
        //public override void ExecuteExpectedEffectModification(INTERACTION_TYPE action, Character actor, IPointOfInterest poiTarget, OtherData[] otherData, ref List<GoapEffect> effects) {
        //    if (action == INTERACTION_TYPE.STEAL) {
        //        effects.Add(new GoapEffect(GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR));
        //    }
        //}
        #endregion

        private bool CanBeStolen(TileObject item) {
            if (item is StructureTileObject || item is GenericTileObject) {
                return false;
            }
            if (item.isBeingCarriedBy != null) {
                if (item.isBeingCarriedBy == traitOwner || item.isBeingCarriedBy.relationshipContainer.IsFriendsWith(traitOwner)) {
                    return false;
                }
                return true;
            } else {
                return item.characterOwner != null && !item.IsOwnedBy(traitOwner) && !traitOwner.relationshipContainer.IsFriendsWith(item.characterOwner);
            }
        }
    }
}

