using System.Collections;
using System.Collections.Generic;
namespace Traits {
    public class Pyrophobic : Trait {

        private Character owner;
        private List<BurningSource> seenBurningSources;

        public Pyrophobic() {
            name = "Pyrophobic";
            description = "Pyrophobics are afraid of fires.";
            type = TRAIT_TYPE.FLAW;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            seenBurningSources = new List<BurningSource>();
        }

        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character character) {
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
            }
            return false;
        }
        private void RemoveKnownBurningSource(BurningSource burningSource) {
            seenBurningSources.Remove(burningSource);
        }
        private void TriggerReactionToFireOnFirstTimeSeeing(IPointOfInterest burningPOI) {
            string debugLog = $"{owner.name} saw a fire for the first time, reduce Happiness by 20, add Anxious status";
            owner.needsComponent.AdjustHappiness(-20f);
            owner.traitContainer.AddTrait(owner, "Anxious");
            int chance = UnityEngine.Random.Range(0, 2);
            if(chance == 0) {
                debugLog += "\n-Character decided to flee";
                //Log log = new Log(GameManager.Instance.Today(), "Trait", name, "flee");
                //log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                //owner.logComponent.RegisterLog(log, onlyClickedCharacter: false);
                owner.combatComponent.Flight(burningPOI, "pyrophobic");
            } else {
                debugLog += "\n-Character decided to trigger Cowering interrupt";
                owner.interruptComponent.TriggerInterrupt(INTERRUPT.Cowering, owner);
            }
            owner.logComponent.PrintLogIfActive(debugLog);
        }

        #region Listeners
        private void OnBurningSourceInactive(BurningSource burningSource) {
            RemoveKnownBurningSource(burningSource);
        }
        #endregion
    }
}

