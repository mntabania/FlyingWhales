using System;
using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
namespace Traits {
    public class Pyrophobic : Trait {

        private Character owner;

        #region getters
        public List<BurningSource> seenBurningSources { get; }
        public override Type serializedData => typeof(SaveDataPyrophobic);
        #endregion
        
        public Pyrophobic() {
            name = "Pyrophobic";
            description = "Will almost always flee when it sees a Fire.";
            type = TRAIT_TYPE.FLAW;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            seenBurningSources = new List<BurningSource>();
            canBeTriggered = false;
            AddTraitOverrideFunctionIdentifier(TraitManager.See_Poi_Trait);
        }

        #region Loading
        public override void LoadSecondWaveInstancedTrait(SaveDataTrait saveDataTrait) {
            base.LoadSecondWaveInstancedTrait(saveDataTrait);
            SaveDataPyrophobic saveDataPyrophobic = saveDataTrait as SaveDataPyrophobic;
            Assert.IsNotNull(saveDataPyrophobic);
            for (int i = 0; i < saveDataPyrophobic.seenBurningSources.Count; i++) {
                string burningSourceID = saveDataPyrophobic.seenBurningSources[i];
                BurningSource burningSource = DatabaseManager.Instance.burningSourceDatabase.GetOrCreateBurningSourceWithID(burningSourceID);
                seenBurningSources.Add(burningSource);
            }
        }
        #endregion
        
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character character) {
                owner = character;
                if (character.traitContainer.HasTrait("Burning")) {
                    Burning burning = character.traitContainer.GetTraitOrStatus<Burning>("Burning");
                    burning.CharacterBurningProcess(character);
                }
                Messenger.AddListener<BurningSource>(Signals.BURNING_SOURCE_INACTIVE, OnBurningSourceInactive);
            }
        }
        public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
            base.LoadTraitOnLoadTraitContainer(addTo);
            if (addTo is Character character) {
                owner = character;
                Messenger.AddListener<BurningSource>(Signals.BURNING_SOURCE_INACTIVE, OnBurningSourceInactive);
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is Character) {
                Messenger.RemoveListener<BurningSource>(Signals.BURNING_SOURCE_INACTIVE, OnBurningSourceInactive);
            }
        }
        public override bool OnSeePOI(IPointOfInterest targetPOI, Character characterThatWillDoJob) {
            Burning burning = targetPOI.traitContainer.GetTraitOrStatus<Burning>("Burning");
            if (burning != null) {
                AddKnownBurningSource(burning.sourceOfBurning, targetPOI);
            }
            return base.OnSeePOI(targetPOI, characterThatWillDoJob);
        }
        public bool AddKnownBurningSource(BurningSource burningSource, IPointOfInterest burningPOI) {
            if (burningSource == null) {
                Debug.LogWarning($"{owner.name} saw the fire of {burningPOI?.nameWithID} but it has no burning source!");
                return false;
            }
            if (!seenBurningSources.Contains(burningSource)) {
                seenBurningSources.Add(burningSource);
                TriggerReactionToFireOnFirstTimeSeeing(burningPOI);
                return true;
            } else {
                //When a character sees a fire source for the second time: Trigger Flight Response.
                owner.combatComponent.Flight(burningPOI);
            }
            return false;
        }
        private void RemoveKnownBurningSource(BurningSource burningSource) {
            seenBurningSources.Remove(burningSource);
        }
        private void TriggerReactionToFireOnFirstTimeSeeing(IPointOfInterest burningPOI) {
            string debugLog = $"{owner.name} saw a fire for the first time, reduce Happiness by 20 and become anxious. ";
            owner.needsComponent.AdjustHappiness(-20f);
            owner.traitContainer.AddTrait(owner, "Anxious");
            int roll = UnityEngine.Random.Range(0, 100);
            if (roll < 10) {
                debugLog += $"{owner.name} became catatonic";
                owner.traitContainer.AddTrait(owner, "Catatonic");
            } else if (roll < 25) {
                debugLog += $"{owner.name} became berserked";
                owner.traitContainer.AddTrait(owner, "Berserked");
            } else if (roll < 40) {
                debugLog += $"{owner.name} Had a seizure";
                owner.interruptComponent.TriggerInterrupt(INTERRUPT.Seizure, owner);
            } else if (roll < 50 && (owner.characterClass.className == "Druid" || owner.characterClass.className == "Shaman" || owner.characterClass.className == "Mage")) {
                debugLog += $"{owner.name} Had a loss of control";
                owner.interruptComponent.TriggerInterrupt(INTERRUPT.Loss_Of_Control, owner);
            } else {
                debugLog += $"{owner.name} became anxious and is cowering.";
                owner.interruptComponent.TriggerInterrupt(INTERRUPT.Cowering, owner, reason: "saw fire");
            }
            owner.logComponent.PrintLogIfActive(debugLog);
            
            Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Trait", "Pyrophobic", "on_see_first", null, LOG_TAG.Combat);
            log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddLogToDatabase();
        }

        #region Listeners
        private void OnBurningSourceInactive(BurningSource burningSource) {
            RemoveKnownBurningSource(burningSource);
        }
        #endregion
    }
}

#region Save Data
public class SaveDataPyrophobic : SaveDataTrait {
    public List<string> seenBurningSources;
    public override void Save(Trait trait) {
        base.Save(trait);
        Pyrophobic pyrophobic = trait as Pyrophobic;
        Assert.IsNotNull(pyrophobic);
        seenBurningSources = new List<string>();
        for (int i = 0; i < pyrophobic.seenBurningSources.Count; i++) {
            BurningSource burningSource = pyrophobic.seenBurningSources[i];
            if (burningSource != null) {
                seenBurningSources.Add(burningSource.persistentID);    
            }
        }
    }
}
#endregion