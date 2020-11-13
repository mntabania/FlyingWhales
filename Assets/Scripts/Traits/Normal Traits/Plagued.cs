using System.Collections.Generic;
using Plague.Fatality;
using Plague.Transmission;
using UnityEngine.Assertions;
using UnityEngine;

namespace Traits {
    public class Plagued : Status {

        public interface IPlaguedListener {
            void PerTickMovement(Character p_character);
            void CharacterGainedTrait(Character p_character, Trait p_gainedTrait);
            void CharacterStartedPerformingAction(Character p_character);
        }
        
        private System.Action<Character> _perTickMovement;
        private System.Action<Character, Trait> _characterGainedTrait;
        private System.Action<Character> _characterStartedPerformingAction;
        
        public IPointOfInterest owner { get; private set; } //poi that has the poison

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
                if (poi is Character) {
                    Messenger.AddListener<ITraitable, Trait>(TraitSignals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
                    Messenger.AddListener<ActualGoapNode>(JobSignals.STARTED_PERFORMING_ACTION, OnStartedPerformingAction);    
                }
                Messenger.AddListener<Fatality>(PlayerSignals.ADDED_PLAGUED_DISEASE_FATALITY, OnPlagueDiseaseFatalityAdded);
                for (int i = 0; i < PlagueDisease.Instance.activeFatalities.Count; i++) {
                    Fatality fatality = PlagueDisease.Instance.activeFatalities[i];
                    SubscribeToPerTickMovement(fatality);
                    SubscribeToCharacterGainedTrait(fatality);
                    SubscribeToCharacterStartedPerformingAction(fatality);
                }
            }
        }
        #endregion
        
        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is IPointOfInterest poi) {
                owner = poi;
                _infectedEffectGO = GameManager.Instance.CreateParticleEffectAt(owner, PARTICLE_EFFECT.Infected, false);
                if (poi is Character) {
                    Messenger.AddListener<ITraitable, Trait>(TraitSignals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
                    Messenger.AddListener<ActualGoapNode>(JobSignals.STARTED_PERFORMING_ACTION, OnStartedPerformingAction);    
                }
                Messenger.AddListener<Fatality>(PlayerSignals.ADDED_PLAGUED_DISEASE_FATALITY, OnPlagueDiseaseFatalityAdded);
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (_infectedEffectGO) {
                ObjectPoolManager.Instance.DestroyObject(_infectedEffectGO);
                _infectedEffectGO = null;
            }
            if (removedFrom is IPointOfInterest) {
                if (removedFrom is Character) {
                    Messenger.RemoveListener<ITraitable, Trait>(TraitSignals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
                    Messenger.RemoveListener<ActualGoapNode>(JobSignals.STARTED_PERFORMING_ACTION, OnStartedPerformingAction);
                }
                Messenger.RemoveListener<Fatality>(PlayerSignals.ADDED_PLAGUED_DISEASE_FATALITY, OnPlagueDiseaseFatalityAdded);
            }
        }
        public override bool PerTickOwnerMovement() {
            if(owner.traitContainer.HasTrait("Plague Reservoir")) {
                return false;
            }
            if (owner is Character character) {
                _perTickMovement?.Invoke(character);
            }
            return false;
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

        #region Fatalities
        public void AddFatality(Fatality fatality) {
            SubscribeToPerTickMovement(fatality);
            SubscribeToCharacterGainedTrait(fatality);
            SubscribeToCharacterStartedPerformingAction(fatality);
        }
        public void RemoveFatality(Fatality fatality) {
            UnsubscribeToPerTickMovement(fatality);
            UnsubscribeToCharacterGainedTrait(fatality);
            UnsubscribeToCharacterStartedPerformingAction(fatality);
        }
        #endregion

        #region Events
        private void SubscribeToPerTickMovement(IPlaguedListener plaguedListener) {
            _perTickMovement += plaguedListener.PerTickMovement;
        }
        private void UnsubscribeToPerTickMovement(IPlaguedListener plaguedListener) {
            _perTickMovement -= plaguedListener.PerTickMovement;
        }
        private void SubscribeToCharacterGainedTrait(IPlaguedListener plaguedListener) {
            _characterGainedTrait += plaguedListener.CharacterGainedTrait;
        }
        private void UnsubscribeToCharacterGainedTrait(IPlaguedListener plaguedListener) {
            _characterGainedTrait -= plaguedListener.CharacterGainedTrait;
        }
        private void SubscribeToCharacterStartedPerformingAction(IPlaguedListener plaguedListener) {
            _characterStartedPerformingAction += plaguedListener.CharacterStartedPerformingAction;
        }
        private void UnsubscribeToCharacterStartedPerformingAction(IPlaguedListener plaguedListener) {
            _characterStartedPerformingAction -= plaguedListener.CharacterStartedPerformingAction;
        }
        #endregion

        #region Listeners
        private void OnPlagueDiseaseFatalityAdded(Fatality p_fatality) {
            AddFatality(p_fatality);
        }
        private void OnTraitableGainedTrait(ITraitable p_traitable, Trait p_trait) {
            //TODO: Might be a better way to trigger that the character that owns this has gained a trait, rather than listening to a signal and filtering results
            if (p_traitable == owner && owner is Character character) {
                _characterGainedTrait?.Invoke(character, p_trait);
            }
        }
        private void OnStartedPerformingAction(ActualGoapNode p_actualGoapNode) {
            //TODO: Might be a better way to trigger that the character that owns this has started performing an action, rather than listening to a signal and filtering results
            if (p_actualGoapNode.actor == owner && owner is Character character) {
                _characterStartedPerformingAction?.Invoke(character);
            }
        }
        #endregion
    }
}

