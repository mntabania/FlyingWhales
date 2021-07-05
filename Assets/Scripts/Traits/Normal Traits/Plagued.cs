using System;
using System.Collections.Generic;
using Plague.Fatality;
using Plague.Symptom;
using Plague.Transmission;
using Plague.Death_Effect;
using UnityEngine.Assertions;
using UnityEngine;
using Traits;
using System.Linq;

namespace Traits {
    public class Plagued : Status {

        public interface IPlaguedListener {
            void PerTickWhileStationaryOrUnoccupied(Character p_character);
            void CharacterGainedTrait(Character p_character, Trait p_gainedTrait);
            void CharacterStartedPerformingAction(Character p_character, ActualGoapNode p_action);
            void CharacterDonePerformingAction(Character p_character, INTERACTION_TYPE p_actionPerformed);
            void HourStarted(Character p_character, int numberOfHours);
        }

        public interface IPlagueDeathListener {
            void OnDeath(Character p_character); 
        }

        private Action<Character> _perTickWhileStationaryOrUnoccupied;
        private Action<Character, Trait> _characterGainedTrait;
        private Action<Character, ActualGoapNode> _characterStartedPerformingAction;
        private Action<Character, int> _hourStarted;
        private Action<Character, INTERACTION_TYPE> _characterDonePerformingAction;
        private Action<Character> _characterDeath;

        public IPointOfInterest owner { get; private set; } //poi that has the poison

        private int _numberOfHoursPassed;
        private GameObject _infectedEffectGO;

        #region getters
        public override bool isPersistent => true;
        public int numberOfHoursPassed => _numberOfHoursPassed;
        public override Type serializedData => typeof(SaveDataPlagued);
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
            AddTraitOverrideFunctionIdentifier(TraitManager.Per_Tick_While_Stationary_Unoccupied);
            AddTraitOverrideFunctionIdentifier(TraitManager.Initiate_Map_Visual_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.Destroy_Map_Visual_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.Hour_Started_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.Start_Perform_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.Execute_After_Effect_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.Death_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.After_Death);
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
                if (_infectedEffectGO) {
                    ObjectPoolManager.Instance.DestroyObject(_infectedEffectGO);
                    _infectedEffectGO = null;
                }
                _infectedEffectGO = GameManager.Instance.CreateParticleEffectAt(owner, PARTICLE_EFFECT.Infected, false);
                if (poi is Character) {
                    Messenger.AddListener<ITraitable, Trait>(TraitSignals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
                }
                Messenger.AddListener<Fatality>(PlayerSignals.ADDED_PLAGUE_DISEASE_FATALITY, OnPlagueDiseaseFatalityAdded);
                Messenger.AddListener<PlagueSymptom>(PlayerSignals.ADDED_PLAGUE_DISEASE_SYMPTOM, OnPlagueDiseaseSymptomAdded);
                Messenger.AddListener<PlagueDeathEffect>(PlayerSignals.SET_PLAGUE_DEATH_EFFECT, OnSetPlagueDeathEffect);
                Messenger.AddListener<PlagueDeathEffect>(PlayerSignals.UNSET_PLAGUE_DEATH_EFFECT, OnUnsetPlagueDeathEffect);

                for (int i = 0; i < PlagueDisease.Instance.activeFatalities.Count; i++) {
                    Fatality fatality = PlagueDisease.Instance.activeFatalities[i];
                    AddFatality(fatality);
                }
                for (int i = 0; i < PlagueDisease.Instance.activeSymptoms.Count; i++) {
                    PlagueSymptom symptom = PlagueDisease.Instance.activeSymptoms[i];
                    AddSymptom(symptom);
                }
                AddDeathEffect(PlagueDisease.Instance.activeDeathEffect);
            }
        }
        #endregion
        
        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is IPointOfInterest addedToPOI) {
                owner = addedToPOI;
                if (_infectedEffectGO) {
                    ObjectPoolManager.Instance.DestroyObject(_infectedEffectGO);
                    _infectedEffectGO = null;
                }
                _infectedEffectGO = GameManager.Instance.CreateParticleEffectAt(owner, PARTICLE_EFFECT.Infected, false);
                if (addedToPOI is Character character) {
                    Messenger.AddListener<ITraitable, Trait>(TraitSignals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
                    if(!character.traitContainer.HasTrait("Plague Reservoir") && PlayerManager.Instance.player.plagueComponent.CanGainPlaguePoints()) {
                        if (character.isNormalCharacter) {
                            PlayerManager.Instance.player?.plagueComponent.GainPlaguePointFromCharacter(5, character);
                        } else {
                            if (character is Summon summon) {
                                if (summon.summonType != SUMMON_TYPE.Rat) {
                                    PlayerManager.Instance.player?.plagueComponent.GainPlaguePointFromCharacter(1, character);
                                }
                            } else {
                                PlayerManager.Instance.player?.plagueComponent.GainPlaguePointFromCharacter(1, character);
                            }
                        }
                    }
                }
                Messenger.AddListener<Fatality>(PlayerSignals.ADDED_PLAGUE_DISEASE_FATALITY, OnPlagueDiseaseFatalityAdded);
                Messenger.AddListener<PlagueSymptom>(PlayerSignals.ADDED_PLAGUE_DISEASE_SYMPTOM, OnPlagueDiseaseSymptomAdded);
                Messenger.AddListener<PlagueDeathEffect>(PlayerSignals.SET_PLAGUE_DEATH_EFFECT, OnSetPlagueDeathEffect);
                Messenger.AddListener<PlagueDeathEffect>(PlayerSignals.UNSET_PLAGUE_DEATH_EFFECT, OnUnsetPlagueDeathEffect);

                for (int i = 0; i < PlagueDisease.Instance.activeFatalities.Count; i++) {
                    Fatality fatality = PlagueDisease.Instance.activeFatalities[i];
                    AddFatality(fatality);
                }
                for (int i = 0; i < PlagueDisease.Instance.activeSymptoms.Count; i++) {
                    PlagueSymptom symptom = PlagueDisease.Instance.activeSymptoms[i];
                    AddSymptom(symptom);
                }
                PlagueDisease.Instance.UpdateActiveCasesOnPOIGainedPlagued(addedToPOI);
                AddDeathEffect(PlagueDisease.Instance.activeDeathEffect);
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (_infectedEffectGO) {
                ObjectPoolManager.Instance.DestroyObject(_infectedEffectGO);
                _infectedEffectGO = null;
            }
            if (removedFrom is IPointOfInterest removedFromPOI) {
                if (removedFrom is Character) {
                    Messenger.RemoveListener<ITraitable, Trait>(TraitSignals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
                }
                Messenger.RemoveListener<Fatality>(PlayerSignals.ADDED_PLAGUE_DISEASE_FATALITY, OnPlagueDiseaseFatalityAdded);
                Messenger.RemoveListener<PlagueSymptom>(PlayerSignals.ADDED_PLAGUE_DISEASE_SYMPTOM, OnPlagueDiseaseSymptomAdded);
                Messenger.RemoveListener<PlagueDeathEffect>(PlayerSignals.SET_PLAGUE_DEATH_EFFECT, OnSetPlagueDeathEffect);
                Messenger.RemoveListener<PlagueDeathEffect>(PlayerSignals.UNSET_PLAGUE_DEATH_EFFECT, OnUnsetPlagueDeathEffect);

                for (int i = 0; i < PlagueDisease.Instance.activeFatalities.Count; i++) {
                    Fatality fatality = PlagueDisease.Instance.activeFatalities[i];
                    RemoveFatality(fatality);
                }
                for (int i = 0; i < PlagueDisease.Instance.activeSymptoms.Count; i++) {
                    PlagueSymptom symptom = PlagueDisease.Instance.activeSymptoms[i];
                    RemoveSymptom(symptom);
                }
                if (removedFrom is Character character) {
                    if (!character.isDead) {
                        PlagueDisease.Instance.UpdateActiveCasesOnPOILostPlagued(removedFromPOI);
                        if (!character.characterClass.IsZombie()) {
                            PlagueDisease.Instance.UpdateRecoveriesOnPOILostPlagued(removedFromPOI);
                        }    
                    }
                }
                RemoveDeathEffect(PlagueDisease.Instance.activeDeathEffect);
            }
        }
        public override bool PerTickWhileStationaryOrUnoccupied(Character p_character) {
            if (owner is Character character) {
                _perTickWhileStationaryOrUnoccupied?.Invoke(character);
            }
            return false;
        }
        public override void ExecuteActionPreEffects(INTERACTION_TYPE action, ActualGoapNode p_actionNode) {
            base.ExecuteActionPreEffects(action, p_actionNode);
            IPointOfInterest otherObject = GetOtherObjectInAction(p_actionNode);
            if (otherObject is StructureTileObject || otherObject is GenericTileObject) {
                return;
            }
            switch (p_actionNode.action.actionCategory) {
                case ACTION_CATEGORY.CONSUME:
                    if (!otherObject.traitContainer.HasTrait("Plagued")) {
                        ConsumptionTransmission.Instance.Transmit(owner, otherObject, PlagueDisease.Instance.GetTransmissionLevel(PLAGUE_TRANSMISSION.Consumption));    
                    }
                    break;
                case ACTION_CATEGORY.DIRECT:
                    if (!otherObject.traitContainer.HasTrait("Plagued")) {
                        PhysicalContactTransmission.Instance.Transmit(owner, otherObject, PlagueDisease.Instance.GetTransmissionLevel(PLAGUE_TRANSMISSION.Physical_Contact));    
                    }
                    break;
                case ACTION_CATEGORY.VERBAL:
                    if(p_actionNode.actor == owner) {
                        //Only transmit verbally if the plagued character is the actor of the action
                        AirborneTransmission.Instance.Transmit(owner, null, PlagueDisease.Instance.GetTransmissionLevel(PLAGUE_TRANSMISSION.Airborne));
                    }
                    break;
                    
            }
        }
        public override void OnInitiateMapObjectVisual(ITraitable traitable) {
            if (traitable is IPointOfInterest poi) {
                if (_infectedEffectGO) {
                    ObjectPoolManager.Instance.DestroyObject(_infectedEffectGO);
                    _infectedEffectGO = null;
                }
                _infectedEffectGO = GameManager.Instance.CreateParticleEffectAt(poi, PARTICLE_EFFECT.Infected, false);
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
            //We still have a checker here for plague reservoir and zombie even though we already have a checker for it in fatality/symptom/death effect
            //The reason is the code after Invoking the _characterStartedPerformingAction
            //Since we do are not entirely sure that the reason for canDoHappinessRecovery being false is because of the _characterStartedPerformingAction since we do not return anything from it
            //There might be other reasons why the canDoHappinessRecovery is false, so the safest option is not to invoke it at all
            if (owner.traitContainer.HasTrait("Plague Reservoir")) {
                return false;
            }
            if (node.actor == owner && owner is Character character) {
                if (character.characterClass.IsZombie()) {
                    //Do not do start perform effect if character is a zombie
                    return false;
                }
                if(_characterStartedPerformingAction != null) {
                    _characterStartedPerformingAction.Invoke(character, node);
                    if (character.interruptComponent.isInterrupted && character.interruptComponent.currentInterrupt.interrupt.type == INTERRUPT.Total_Organ_Failure) {
                        willStillContinueAction = false;
                    }
                    //If character can no longer do happiness recovery and the action that is starting is a happiness recovery type job, character should no longer continue doing the job
                    if (node.associatedJobType.IsHappinessRecoveryTypeJob() && !character.limiterComponent.canDoHappinessRecovery) {
                        if (node.actor.jobQueue.jobsInQueue.Count > 0) {
                            node.actor.jobQueue.jobsInQueue[0].CancelJob();
                        }
                        willStillContinueAction = false;
                        if (owner.traitContainer.HasTrait("Depressed")) {
                            Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Trait", name, "depressed", null, LOG_TAG.Life_Changes);
                            log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                            log.AddLogToDatabase(true);
                        }
                    } else if (node.associatedJobType.IsTirednessRecoveryTypeJob() && !character.limiterComponent.canDoTirednessRecovery) {
                        if (node.actor.jobQueue.jobsInQueue.Count > 0) {
                            node.actor.jobQueue.jobsInQueue[0].CancelJob();
                        }
                        willStillContinueAction = false;
                        if (owner.traitContainer.HasTrait("Insomnia")) {
                            Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Trait", name, "insomnia", null, LOG_TAG.Life_Changes);
                            log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                            log.AddLogToDatabase(true);
                        }
                    }
                    return true;
                }
            }
            return base.OnStartPerformGoapAction(node, ref willStillContinueAction);
        }
        public override void ExecuteActionAfterEffects(INTERACTION_TYPE action, Character actor, IPointOfInterest target, ACTION_CATEGORY category, ref bool isRemoved) {
            if (actor == owner && owner is Character character) {
                _characterDonePerformingAction?.Invoke(character, action);
            }
            base.ExecuteActionAfterEffects(action, actor, target, category, ref isRemoved);
        }
        public override void AfterDeath(Character character) {
            _characterDeath?.Invoke(character);
        }
        public override bool OnDeath(Character character) {
            if (!character.characterClass.IsZombie()) {
                if (PlayerManager.Instance.player.plagueComponent.CanGainPlaguePoints()) {
                    PlayerManager.Instance.player.plagueComponent.GainPlaguePointFromCharacter(2, character);    
                }
                PlagueDisease.Instance.UpdateActiveCasesOnCharacterDied(character);
            }
            return base.OnDeath(character);
        }
        protected override string GetDescriptionInUI() {
            string tooltip = base.GetDescriptionInUI();
            tooltip = $"{tooltip}\n{PlagueDisease.Instance.GetPlagueEffectsSummary()}";
            return tooltip;
        }
        public override void OnCopyStatus(Status statusToCopy, ITraitable from, ITraitable to) {
            base.OnCopyStatus(statusToCopy, from, to);
            if (statusToCopy is Plagued status) {
                _numberOfHoursPassed = status.numberOfHoursPassed;
            }
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

        #region Death Effect
        public void AddDeathEffect(PlagueDeathEffect p_deathEffect) {
            if(p_deathEffect != null) {
                SubscribeToDeathEffect(p_deathEffect);
            }
        }
        public void RemoveDeathEffect(PlagueDeathEffect p_deathEffect) {
            if (p_deathEffect != null) {
                UnsubscribeToDeathEffect(p_deathEffect);
            }
        }
        #endregion

        #region Events
        private void SubscribeToAllPlagueListenerEvents(IPlaguedListener p_plaguedListener) {
            SubscribeToPerTickWhileStationaryOrUnoccupied(p_plaguedListener);
            SubscribeToCharacterGainedTrait(p_plaguedListener);
            SubscribeToCharacterStartedPerformingAction(p_plaguedListener);
            SubscribeToHourStarted(p_plaguedListener);
            SubscribeToCharacterDonePerformingAction(p_plaguedListener);
        }
        private void UnsubscribeToAllPlagueListenerEvents(IPlaguedListener p_plaguedListener) {
            UnsubscribeToPerTickWhileStationaryOrUnoccupied(p_plaguedListener);
            UnsubscribeToCharacterGainedTrait(p_plaguedListener);
            UnsubscribeToCharacterStartedPerformingAction(p_plaguedListener);
            UnsubscribeToCharacterDonePerformingAction(p_plaguedListener);
        }
        private void SubscribeToPerTickWhileStationaryOrUnoccupied(IPlaguedListener plaguedListener) {
            _perTickWhileStationaryOrUnoccupied += plaguedListener.PerTickWhileStationaryOrUnoccupied;
        }
        private void UnsubscribeToPerTickWhileStationaryOrUnoccupied(IPlaguedListener plaguedListener) {
            _perTickWhileStationaryOrUnoccupied -= plaguedListener.PerTickWhileStationaryOrUnoccupied;
        }
        private void SubscribeToCharacterGainedTrait(IPlaguedListener plaguedListener) {
            _characterGainedTrait += plaguedListener.CharacterGainedTrait;
        }
        private void UnsubscribeToCharacterGainedTrait(IPlaguedListener plaguedListener) {
            _characterGainedTrait -= plaguedListener.CharacterGainedTrait;
        }
        private void SubscribeToCharacterStartedPerformingAction(IPlaguedListener plaguedListener) {
            _characterStartedPerformingAction += (pCharacter, pAction) => plaguedListener.CharacterStartedPerformingAction(pCharacter, pAction);
        }
        private void UnsubscribeToCharacterStartedPerformingAction(IPlaguedListener plaguedListener) {
            _characterStartedPerformingAction -= (pCharacter, pAction) => plaguedListener.CharacterStartedPerformingAction(pCharacter, pAction);
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
        private void SubscribeToDeathEffect(IPlagueDeathListener p_plaguedDeathListener) {
            _characterDeath += p_plaguedDeathListener.OnDeath;
        }
        private void UnsubscribeToDeathEffect(IPlagueDeathListener p_plaguedDeathListener) {
            _characterDeath -= p_plaguedDeathListener.OnDeath;
        }
        #endregion

        #region Listeners
        private void OnPlagueDiseaseFatalityAdded(Fatality p_fatality) {
            AddFatality(p_fatality);
        }
        private void OnPlagueDiseaseSymptomAdded(PlagueSymptom p_symptom) {
            AddSymptom(p_symptom);
        }
        private void OnSetPlagueDeathEffect(PlagueDeathEffect p_deathEffect) {
            AddDeathEffect(p_deathEffect);
        }
        private void OnUnsetPlagueDeathEffect(PlagueDeathEffect p_deathEffect) {
            RemoveDeathEffect(p_deathEffect);
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