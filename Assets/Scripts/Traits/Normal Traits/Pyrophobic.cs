using System;
using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;

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
        public override void LoadSecondWaveInstancedTrait(SaveDataTrait p_saveDataTrait) {
            base.LoadSecondWaveInstancedTrait(p_saveDataTrait);
            SaveDataPyrophobic saveDataPyrophobic = p_saveDataTrait as SaveDataPyrophobic;
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
                Messenger.AddListener<BurningSource>(InnerMapSignals.BURNING_SOURCE_INACTIVE, OnBurningSourceInactive);
            }
        }
        public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
            base.LoadTraitOnLoadTraitContainer(addTo);
            if (addTo is Character character) {
                owner = character;
                Messenger.AddListener<BurningSource>(InnerMapSignals.BURNING_SOURCE_INACTIVE, OnBurningSourceInactive);
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is Character) {
                Messenger.RemoveListener<BurningSource>(InnerMapSignals.BURNING_SOURCE_INACTIVE, OnBurningSourceInactive);
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
            bool hasSeenFireFirstTime = false;
            if (!seenBurningSources.Contains(burningSource)) {
                seenBurningSources.Add(burningSource);
                hasSeenFireFirstTime = true;
            }
            bool shouldAddAnxiousTrait = true;
            bool shouldTryPassOut = false;
            bool shouldTryHeartAttack = false;
            if (owner.HasAfflictedByPlayerWith(name)) {
                SkillData pyrophobia = PlayerSkillManager.Instance.GetAfflictionData(PLAYER_SKILL_TYPE.PYROPHOBIA);
                shouldAddAnxiousTrait = pyrophobia.currentLevel >= 1;
                shouldTryPassOut = pyrophobia.currentLevel >= 2;
                shouldTryHeartAttack = pyrophobia.currentLevel >= 3;
            }
            if (shouldTryHeartAttack && GameUtilities.RollChance(10)) {
                if(owner.interruptComponent.TriggerInterrupt(INTERRUPT.Heart_Attack, owner, identifier: "saw fire")) {
                    return hasSeenFireFirstTime;
                }
            }
            if (shouldAddAnxiousTrait) {
                owner.traitContainer.AddTrait(owner, "Anxious");
            }
            if (shouldTryPassOut && GameUtilities.RollChance(20)) {
                if (owner.interruptComponent.TriggerInterrupt(INTERRUPT.Pass_Out, owner, identifier: "saw fire")) {
                    return hasSeenFireFirstTime;
                }
            }
            if (hasSeenFireFirstTime) {
                TriggerReactionToFireOnFirstTimeSeeing(burningPOI);
            } else {
                //When a character sees a fire source for the second time: Trigger Flight Response.
                owner.combatComponent.Flight(burningPOI);
            }
            return hasSeenFireFirstTime;
        }
        private void RemoveKnownBurningSource(BurningSource burningSource) {
            seenBurningSources.Remove(burningSource);
        }
        private void TriggerReactionToFireOnFirstTimeSeeing(IPointOfInterest burningPOI) {
#if DEBUG_LOG
            string debugLog = $"{owner.name} saw a fire for the first time, reduce Happiness by 20 and become anxious. ";
#endif
            owner.needsComponent.AdjustHappiness(-20f);

            if (GameUtilities.RollChance(10)) {
#if DEBUG_LOG
                debugLog += $"{owner.name} became catatonic";
#endif
                owner.traitContainer.AddTrait(owner, "Catatonic");
            } else if (GameUtilities.RollChance(15)) {
#if DEBUG_LOG
                debugLog += $"{owner.name} became berserked";
#endif
                owner.traitContainer.AddTrait(owner, "Berserked");
            } else if (GameUtilities.RollChance(15)) {
#if DEBUG_LOG
                debugLog += $"{owner.name} Had a seizure";
#endif
                owner.interruptComponent.TriggerInterrupt(INTERRUPT.Seizure, owner);
            } else if (GameUtilities.RollChance(10) && (owner.characterClass.className == "Druid" || owner.characterClass.className == "Shaman" || owner.characterClass.className == "Mage")) {

#if DEBUG_LOG
                debugLog += $"{owner.name} Had a loss of control";
#endif
                owner.interruptComponent.TriggerInterrupt(INTERRUPT.Loss_Of_Control, owner);
            } else {
#if DEBUG_LOG
                debugLog += $"{owner.name} became anxious and is cowering.";
#endif
                owner.interruptComponent.TriggerInterrupt(INTERRUPT.Cowering, owner, reason: "saw fire");
            }
#if DEBUG_LOG
            owner.logComponent.PrintLogIfActive(debugLog);
#endif

            Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Trait", "Pyrophobic", "on_see_first", null, LOG_TAG.Combat);
            log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddLogToDatabase(true);

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