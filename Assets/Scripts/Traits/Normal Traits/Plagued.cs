using System.Collections;
using System.Collections.Generic;
using Plague.Transmission;
using UnityEngine;

namespace Traits {
    public class Plagued : Status {

        public IPointOfInterest owner { get; private set; } //poi that has the poison

        private readonly int pukeChance = 4;
        private readonly int septicChance = 1;

        private GameObject _infectedEffectGO;

        public Plagued() {
            name = "Plagued";
            description = "Has a terrible and virulent disease.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = 0;
            advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.CURE_CHARACTER };
            mutuallyExclusive = new string[] { "Robust" };
            moodEffect = -4;
            AddTraitOverrideFunctionIdentifier(TraitManager.Execute_Pre_Effect_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.Per_Tick_Movement);
            AddTraitOverrideFunctionIdentifier(TraitManager.Initiate_Map_Visual_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.Destroy_Map_Visual_Trait);
        }

        #region Loading
        public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
            base.LoadTraitOnLoadTraitContainer(addTo);
            if (addTo is IPointOfInterest poi) {
                owner = poi;
                _infectedEffectGO = GameManager.Instance.CreateParticleEffectAt(owner, PARTICLE_EFFECT.Infected, false);
            }
        }
        #endregion
        
        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is IPointOfInterest poi) {
                owner = poi;
                _infectedEffectGO = GameManager.Instance.CreateParticleEffectAt(owner, PARTICLE_EFFECT.Infected, false);
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (_infectedEffectGO) {
                ObjectPoolManager.Instance.DestroyObject(_infectedEffectGO);
                _infectedEffectGO = null;
            }
        }
        public override bool PerTickOwnerMovement() {
            if(owner.traitContainer.HasTrait("Plague Reservoir")) {
                return false;
            }
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
                    return character.interruptComponent.TriggerInterrupt(INTERRUPT.Puke, owner, "Plague");
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
        public override void ExecuteActionPreEffects(INTERACTION_TYPE action, ActualGoapNode p_actionNode) {
            base.ExecuteActionPreEffects(action, p_actionNode);
            IPointOfInterest otherObject = GetOtherObjectInAction(p_actionNode);
            switch (p_actionNode.action.actionCategory) {
                case ACTION_CATEGORY.CONSUME:
                    if (!otherObject.traitContainer.HasTrait("Plagued")) {
                        ConsumptionTransmission.Instance.Transmit(owner, otherObject, 1);    
                    }
                    break;
                case ACTION_CATEGORY.DIRECT:
                    if (!otherObject.traitContainer.HasTrait("Plagued") && otherObject is Character) {
                        PhysicalContactTransmission.Instance.Transmit(owner, otherObject, 1);    
                    }
                    break;
                case ACTION_CATEGORY.VERBAL:
                    AirborneTransmission.Instance.Transmit(owner, null, 1);
                    break;
                    
            }
        }
        public override void OnInitiateMapObjectVisual(ITraitable traitable) {
            if (traitable is Character character) {
                if (_infectedEffectGO) {
                    ObjectPoolManager.Instance.DestroyObject(_infectedEffectGO);
                    _infectedEffectGO = null;
                }
                _infectedEffectGO = GameManager.Instance.CreateParticleEffectAt(character, PARTICLE_EFFECT.Infected, false);
            }
        }
        public override void OnDestroyMapObjectVisual(ITraitable traitable) {
            if (_infectedEffectGO) {
                ObjectPoolManager.Instance.DestroyObject(_infectedEffectGO);
                _infectedEffectGO = null;
            }
        }
        #endregion

        private IPointOfInterest GetOtherObjectInAction(ActualGoapNode p_actionNode) {
            if (p_actionNode.actor != this.owner) {
                return p_actionNode.actor;
            }
            return p_actionNode.target;
        }
    }

}
