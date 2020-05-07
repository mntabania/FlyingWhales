using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Plagued : Status {

        public IPointOfInterest owner { get; private set; } //poi that has the poison

        private float pukeChance = 5f;
        private float septicChance = 2.5f;

        public Plagued() {
            name = "Plagued";
            description = "This character has a terrible disease.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = 0;
            advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.CURE_CHARACTER, };
            mutuallyExclusive = new string[] { "Robust" };
            moodEffect = -4;
            AddTraitOverrideFunctionIdentifier(TraitManager.Execute_After_Effect_Trait);
            // moodEffect = -30;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character character) {
                character.needsComponent.AdjustComfortDecreaseRate(10);
            } 
            //else if (addedTo is TileObject) {
            //    ticksDuration = GameManager.Instance.GetTicksBasedOnHour(12);
            //}
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            if (removedFrom is Character character) {
                character.needsComponent.AdjustComfortDecreaseRate(10);
            }
            base.OnRemoveTrait(removedFrom, removedBy);
        }
        // protected override void OnChangeLevel() {
        //     if (level == 1) {
        //         pukeChance = 5f;
        //         septicChance = 0.5f;
        //     } else if (level == 2) {
        //         pukeChance = 7f;
        //         septicChance = 1f;
        //     } else {
        //         pukeChance = 9f;
        //         septicChance = 1.5f;
        //     }
        // }
        public override bool PerTickOwnerMovement() {
            //NOTE: This is a wrong probability computation for floats - FIND A SOLUTION
            //float pukeRoll = Random.Range(0f, 100f);
            //float septicRoll = Random.Range(0f, 100f);
            int pukeRoll = Random.Range(0, 100);
            int septicRoll = Random.Range(0, 100);
            bool hasCreatedJob = false;
            if (pukeRoll < pukeChance) {
                //do puke action
                if (owner is Character character) {
                    if(character.characterClass.className == "Zombie"/* || (owner.currentActionNode != null && owner.currentActionNode.action.goapType == INTERACTION_TYPE.PUKE)*/) {
                        return hasCreatedJob;
                    }
                    return character.interruptComponent.TriggerInterrupt(INTERRUPT.Puke, owner);
                }
            } else if (septicRoll < septicChance) {
                if (owner is Character character) {
                    if (character.characterClass.className == "Zombie"/* || (owner.currentActionNode != null && owner.currentActionNode.action.goapType == INTERACTION_TYPE.PUKE)*/) {
                        return hasCreatedJob;
                    }
                    return character.interruptComponent.TriggerInterrupt(INTERRUPT.Septic_Shock, owner);
                }
            }
            return hasCreatedJob;
        }
        public override void ExecuteActionAfterEffects(INTERACTION_TYPE action, ActualGoapNode goapNode, ref bool isRemoved) {
            base.ExecuteActionAfterEffects(action, goapNode, ref isRemoved);
            if (goapNode.action.actionCategory == ACTION_CATEGORY.DIRECT || goapNode.action.actionCategory == ACTION_CATEGORY.CONSUME) {
                IPointOfInterest target;
                IPointOfInterest infector;
                int chance;
                if (TryGetTargetAndInfectorAndChance(goapNode, out target, out infector, out chance)) { //this is necessary so that this function can determine which of the characters is infecting the other
                    int roll = Random.Range(0, 100);
                    if (roll < chance) {
                        //target will be infected with plague
                        if (target.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
                            (target as Character).interruptComponent.TriggerInterrupt(INTERRUPT.Plagued, target);
                        } else if (target.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
                            target.traitContainer.AddTrait(target, "Plagued", overrideDuration: GameManager.Instance.GetTicksBasedOnHour(12));
                        }
                        //else {
                        //    target.traitContainer.AddTrait(target, "Plagued");
                        //}
                    }
                }
            }
        }
        #endregion

        private bool TryGetTargetAndInfectorAndChance(ActualGoapNode goapNode, out IPointOfInterest target, out IPointOfInterest infector, out int chance) {
            chance = 0;
            if (goapNode.actor == owner) {
                target = goapNode.poiTarget;
                infector = goapNode.actor;
            } else {
                target = goapNode.actor;
                infector = goapNode.poiTarget;
            }
            if(owner.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
                //if (goapNode.poiTarget == owner) {
                //    if (goapNode.goapType == INTERACTION_TYPE.CARRY || goapNode.goapType == INTERACTION_TYPE.CARRY_CORPSE || goapNode.goapType == INTERACTION_TYPE.INVITE) {
                //        chance = 50;
                //        return true;
                //    }
                //}
                if (goapNode.action.actionCategory == ACTION_CATEGORY.DIRECT || goapNode.action.actionCategory == ACTION_CATEGORY.CONSUME) {
                    chance = 35;
                    return true;
                }
            } else if (owner.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
                if (goapNode.action.actionCategory == ACTION_CATEGORY.DIRECT) {
                    chance = 35;
                } else if (goapNode.action.actionCategory == ACTION_CATEGORY.CONSUME) {
                    chance = 100;
                }
            }
            return true;
        }

        public void ChatInfection(Character target) {
            if(UnityEngine.Random.Range(0, 100) < 35) {
                target.interruptComponent.TriggerInterrupt(INTERRUPT.Plagued, target);
            }
        }
        public int GetCarryInfectChance() {
            return 50;
        }

        //private int GetInfectChanceForAction(INTERACTION_TYPE type) {
        //    switch (type) {
        //        case INTERACTION_TYPE.CHAT_CHARACTER:
        //            return GetChatInfectChance();
        //        case INTERACTION_TYPE.MAKE_LOVE:
        //            return GetMakeLoveInfectChance();
        //        case INTERACTION_TYPE.CARRY:
        //            return GetCarryInfectChance();
        //        default:
        //            return 0;
        //    }
        //}
    }

}
