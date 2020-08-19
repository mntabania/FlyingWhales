using System.Collections;
using System.Collections.Generic;
namespace Traits {
    public class Pyrophobic : Trait {

        private Character owner;
        private List<BurningSource> seenBurningSources;

        public Pyrophobic() {
            name = "Pyrophobic";
            description = "Will almost always flee when it sees a Fire.";
            type = TRAIT_TYPE.FLAW;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            seenBurningSources = new List<BurningSource>();
            AddTraitOverrideFunctionIdentifier(TraitManager.See_Poi_Trait);
        }

        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character character) {
                owner = character;
                if (character.traitContainer.HasTrait("Burning")) {
                    Burning burning = character.traitContainer.GetNormalTrait<Burning>("Burning");
                    burning.CharacterBurningProcess(character);
                }
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
            Burning burning = targetPOI.traitContainer.GetNormalTrait<Burning>("Burning");
            if (burning != null) {
                AddKnownBurningSource(burning.sourceOfBurning, targetPOI);
            }
            return base.OnSeePOI(targetPOI, characterThatWillDoJob);
        }
        public bool AddKnownBurningSource(BurningSource burningSource, IPointOfInterest burningPOI) {
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
            string debugLog = $"{owner.name} saw a fire for the first time, reduce Happiness by 20. ";
            owner.needsComponent.AdjustHappiness(-20f);
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
                owner.traitContainer.AddTrait(owner, "Anxious");
                owner.interruptComponent.TriggerInterrupt(INTERRUPT.Cowering, owner, reason: "saw fire");
            }
            owner.logComponent.PrintLogIfActive(debugLog);
            
            Log log = new Log(GameManager.Instance.Today(), "Trait", "Pyrophobic", "on_see_first");
            log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddLogToInvolvedObjects();
        }

        #region Listeners
        private void OnBurningSourceInactive(BurningSource burningSource) {
            RemoveKnownBurningSource(burningSource);
        }
        #endregion
    }
}

