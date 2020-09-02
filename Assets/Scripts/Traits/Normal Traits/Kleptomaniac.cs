using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;

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
            AddTraitOverrideFunctionIdentifier(TraitManager.Execute_Expected_Effect_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.Death_Trait);
        }

        #region Overrides
        public override void OnAddTrait(ITraitable sourceCharacter) {
            base.OnAddTrait(sourceCharacter);
            traitOwner = sourceCharacter as Character;
        }
        public override string TriggerFlaw(Character character) {
            //The character will begin Hunt for Blood.
            if (!character.jobQueue.HasJob(JOB_TYPE.TRIGGER_FLAW)) {
                bool triggerBrokenhearted = false;
                Heartbroken heartbroken = character.traitContainer.GetNormalTrait<Heartbroken>("Heartbroken");
                if (heartbroken != null) {
                    triggerBrokenhearted = UnityEngine.Random.Range(0, 100) < (25 * character.traitContainer.stacks[heartbroken.name]);
                }
                if (!triggerBrokenhearted) {
                    if (character.jobQueue.HasJob(JOB_TYPE.HAPPINESS_RECOVERY)) {
                        character.jobQueue.CancelAllJobs(JOB_TYPE.HAPPINESS_RECOVERY);
                    }

                    List<TileObject> choices = new List<TileObject>();
                    for (int i = 0; i < character.currentRegion.charactersAtLocation.Count; i++) {
                        Character otherCharacter = character.currentRegion.charactersAtLocation[i];
                        for (int j = 0; j < otherCharacter.items.Count; j++) {
                            TileObject currItem = otherCharacter.items[j];
                            if (CanBeStolen(currItem)) {
                                choices.Add(currItem);    
                            }
                        }
                    }
                    
                    //NOTE: Might be heavy on performance, optimize this!
                    foreach (KeyValuePair<STRUCTURE_TYPE,List<LocationStructure>> pair in character.currentRegion.structures) {
                        for (int i = 0; i < pair.Value.Count; i++) {
                            LocationStructure structure = pair.Value[i];
                            for (int j = 0; j < structure.pointsOfInterest.Count; j++) {
                                IPointOfInterest poi = structure.pointsOfInterest.ElementAt(j);
                                if (poi.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
                                    TileObject item = poi as TileObject;
                                    if (CanBeStolen(item)) {
                                        choices.Add(item);
                                    }
                                }
                            }
                        }
                    }
                    if (choices.Count > 0) {
                        IPointOfInterest target = CollectionUtilities.GetRandomElement(choices);
                        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.TRIGGER_FLAW, INTERACTION_TYPE.STEAL, target, character);
                        character.jobQueue.AddJobInQueue(job);
                    } else {
                        return "no_target";
                    }
                } else {
                    heartbroken.TriggerBrokenhearted();
                }
            }
            return base.TriggerFlaw(character);
        }
        public override void ExecuteCostModification(INTERACTION_TYPE action, Character actor, IPointOfInterest poiTarget, OtherData[] otherData, ref int cost) {
            if (action == INTERACTION_TYPE.STEAL) {
                cost = 0;//Utilities.rng.Next(5, 10);//5,46
            } else if (action == INTERACTION_TYPE.PICK_UP) {
                cost = 10000;//Utilities.rng.Next(5, 10);//5,46
            }
        }
        public override void ExecuteExpectedEffectModification(INTERACTION_TYPE action, Character actor, IPointOfInterest poiTarget, OtherData[] otherData, ref List<GoapEffect> effects) {
            if (action == INTERACTION_TYPE.STEAL) {
                effects.Add(new GoapEffect(GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR));
            }
        }
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

