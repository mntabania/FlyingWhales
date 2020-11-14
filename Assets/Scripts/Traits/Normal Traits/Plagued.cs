using System.Collections.Generic;
using Plague.Fatality;
using Plague.Symptom;
using Plague.Transmission;
using UnityEngine.Assertions;
using UnityEngine;
using Traits;

namespace Traits {
    public class Plagued : Status {

        public interface IPlaguedListener {
            void PerTickMovement(Character p_character);
            void CharacterGainedTrait(Character p_character, Trait p_gainedTrait);
            void CharacterStartedPerformingAction(Character p_character);
            void CharacterDonePerformingAction(Character p_character, ActualGoapNode p_actionPerformed);
            void HourStarted(Character p_character, int numberOfHours);
        }

        private System.Action<Character> _perTickMovement;
        private System.Action<Character, Trait> _characterGainedTrait;
        private System.Action<Character> _characterStartedPerformingAction;
        private System.Action<Character, int> _hourStarted;
        private System.Action<Character, ActualGoapNode> _characterDonePerformingAction;

        public IPointOfInterest owner { get; private set; } //poi that has the poison

        private int _numberOfHoursPassed;
        private GameObject _infectedEffectGO;

        #region getters
        public int numberOfHoursPassed => _numberOfHoursPassed;
        #endregion

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
            AddTraitOverrideFunctionIdentifier(TraitManager.Hour_Started_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.Start_Perform_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.Execute_After_Effect_Trait);
        }

        #region Loading
        public override void LoadFirstWaveInstancedTrait(SaveDataTrait saveDataTrait) {
            base.LoadFirstWaveInstancedTrait(saveDataTrait);
            SaveDataPlagued saveDataPlagued = saveDataTrait as SaveDataPlagued;
            Assert.IsNotNull(saveDataPlagued);
            _numberOfHoursPassed = saveDataPlagued.numberOfHoursPassed;
        }
        public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
            base.LoadTraitOnLoadTraitContainer(addTo);
            if (addTo is IPointOfInterest poi) {
                owner = poi;
                _infectedEffectGO = GameManager.Instance.CreateParticleEffectAt(owner, PARTICLE_EFFECT.Infected, false);
                if (poi is Character) {
                    Messenger.AddListener<ITraitable, Trait>(TraitSignals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
                }
                Messenger.AddListener<Fatality>(PlayerSignals.ADDED_PLAGUE_DISEASE_FATALITY, OnPlagueDiseaseFatalityAdded);
                Messenger.AddListener<PlagueSymptom>(PlayerSignals.ADDED_PLAGUE_DISEASE_SYMPTOM, OnPlagueDiseaseSymptomAdded);
                for (int i = 0; i < PlagueDisease.Instance.activeFatalities.Count; i++) {
                    Fatality fatality = PlagueDisease.Instance.activeFatalities[i];
                    AddFatality(fatality);
                }
                for (int i = 0; i < PlagueDisease.Instance.activeSymptoms.Count; i++) {
                    PlagueSymptom symptom = PlagueDisease.Instance.activeSymptoms[i];
                    AddSymptom(symptom);
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
                }
                Messenger.AddListener<Fatality>(PlayerSignals.ADDED_PLAGUE_DISEASE_FATALITY, OnPlagueDiseaseFatalityAdded);
                Messenger.AddListener<PlagueSymptom>(PlayerSignals.ADDED_PLAGUE_DISEASE_SYMPTOM, OnPlagueDiseaseSymptomAdded);
                for (int i = 0; i < PlagueDisease.Instance.activeFatalities.Count; i++) {
                    Fatality fatality = PlagueDisease.Instance.activeFatalities[i];
                    AddFatality(fatality);
                }
                for (int i = 0; i < PlagueDisease.Instance.activeSymptoms.Count; i++) {
                    PlagueSymptom symptom = PlagueDisease.Instance.activeSymptoms[i];
                    AddSymptom(symptom);
                }
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
                }
                Messenger.RemoveListener<Fatality>(PlayerSignals.ADDED_PLAGUE_DISEASE_FATALITY, OnPlagueDiseaseFatalityAdded);
                Messenger.RemoveListener<PlagueSymptom>(PlayerSignals.ADDED_PLAGUE_DISEASE_SYMPTOM, OnPlagueDiseaseSymptomAdded);
                for (int i = 0; i < PlagueDisease.Instance.activeFatalities.Count; i++) {
                    Fatality fatality = PlagueDisease.Instance.activeFatalities[i];
                    RemoveFatality(fatality);
                }
                for (int i = 0; i < PlagueDisease.Instance.activeSymptoms.Count; i++) {
                    PlagueSymptom symptom = PlagueDisease.Instance.activeSymptoms[i];
                    RemoveSymptom(symptom);
                }
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
        public override void OnHourStarted(ITraitable traitable) {
            base.OnHourStarted(traitable);
            _numberOfHoursPassed++;
            if (traitable is Character character) {
                _hourStarted?.Invoke(character, _numberOfHoursPassed);
            }
        }
        public override bool OnStartPerformGoapAction(ActualGoapNode node, ref bool willStillContinueAction) {
            if (node.actor == owner && owner is Character character) {
                _characterStartedPerformingAction?.Invoke(character);

                //If character can no longer do happiness recovery and the action that is starting is a happiness recovery type job, character should no longer continue doing the job
                if(node.associatedJobType.IsHappinessRecoveryTypeJob() && !character.limiterComponent.canDoHappinessRecovery) {
                    willStillContinueAction = false;
                } else if (node.associatedJobType.IsTirednessRecoveryTypeJob() && !character.limiterComponent.canDoTirednessRecovery) {
                    willStillContinueAction = false;
                }
                return true;
            }
            return base.OnStartPerformGoapAction(node, ref willStillContinueAction);
        }
        public override void ExecuteActionAfterEffects(INTERACTION_TYPE action, ActualGoapNode goapNode, ref bool isRemoved) {
            if (goapNode.actor == owner && owner is Character character) {
                _characterDonePerformingAction?.Invoke(character, goapNode);
            }
            base.ExecuteActionAfterEffects(action, goapNode, ref isRemoved);
        }
        #endregion

        #region Fatalities
        public void AddFatality(Fatality p_fatality) {
            SubscribeToAllPlagueListenerEvents(p_fatality);
        }
        public void RemoveFatality(Fatality p_fatality) {
            UnsubscribeToAllPlagueListenerEvents(p_fatality);
        }
        #endregion

        #region Symptoms
        public void AddSymptom(PlagueSymptom p_symptom) {
            SubscribeToAllPlagueListenerEvents(p_symptom);
        }
        public void RemoveSymptom(PlagueSymptom p_symptom) {
            UnsubscribeToAllPlagueListenerEvents(p_symptom);
        }
        #endregion

        #region Events
        private void SubscribeToAllPlagueListenerEvents(IPlaguedListener p_plaguedListener) {
            SubscribeToPerTickMovement(p_plaguedListener);
            SubscribeToCharacterGainedTrait(p_plaguedListener);
            SubscribeToCharacterStartedPerformingAction(p_plaguedListener);
            SubscribeToHourStarted(p_plaguedListener);
            SubscribeToCharacterDonePerformingAction(p_plaguedListener);
        }
        private void UnsubscribeToAllPlagueListenerEvents(IPlaguedListener p_plaguedListener) {
            UnsubscribeToPerTickMovement(p_plaguedListener);
            UnsubscribeToCharacterGainedTrait(p_plaguedListener);
            UnsubscribeToCharacterStartedPerformingAction(p_plaguedListener);
            UnsubscribeToCharacterDonePerformingAction(p_plaguedListener);
        }
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
        private void SubscribeToHourStarted(IPlaguedListener p_plaguedListener) {
            _hourStarted += p_plaguedListener.HourStarted;
        }
        private void UnsubscribeToHourStarted(IPlaguedListener p_plaguedListener) {
            _hourStarted -= p_plaguedListener.HourStarted;
        }
        private void SubscribeToCharacterDonePerformingAction(IPlaguedListener p_plaguedListener) {
            _characterDonePerformingAction += p_plaguedListener.CharacterDonePerformingAction;
        }
        private void UnsubscribeToCharacterDonePerformingAction(IPlaguedListener p_plaguedListener) {
            _characterDonePerformingAction -= p_plaguedListener.CharacterDonePerformingAction;
        }
        #endregion

        #region Listeners
        private void OnPlagueDiseaseFatalityAdded(Fatality p_fatality) {
            AddFatality(p_fatality);
        }
        private void OnPlagueDiseaseSymptomAdded(PlagueSymptom p_symptom) {
            AddSymptom(p_symptom);
        }
        private void OnTraitableGainedTrait(ITraitable p_traitable, Trait p_trait) {
            //TODO: Might be a better way to trigger that the character that owns this has gained a trait, rather than listening to a signal and filtering results
            if (p_traitable == owner && owner is Character character) {
                _characterGainedTrait?.Invoke(character, p_trait);
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

#region Save Data
public class SaveDataPlagued : SaveDataTrait {
    public int numberOfHoursPassed;
    public override void Save(Trait trait) {
        base.Save(trait);
        Plagued plagued = trait as Plagued;
        Assert.IsNotNull(plagued);
        numberOfHoursPassed = plagued.numberOfHoursPassed;
    }
}
#endregion