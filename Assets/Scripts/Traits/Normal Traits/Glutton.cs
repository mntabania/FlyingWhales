using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Glutton : Trait {

        private int additionalFullnessDecreaseRate;

        public Glutton() {
            name = "Glutton";
            description = "Eats a lot!";
            type = TRAIT_TYPE.FLAW;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            canBeTriggered = true;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character character) {
                additionalFullnessDecreaseRate = Mathf.CeilToInt(EditableValuesManager.Instance.baseFullnessDecreaseRate * 0.5f);
                character.needsComponent.SetFullnessForcedTick(0);
                character.needsComponent.AdjustFullnessDecreaseRate(additionalFullnessDecreaseRate);
                character.behaviourComponent.AddBehaviourComponent(typeof(GluttonBehaviour));
            }
        }
        public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
            base.LoadTraitOnLoadTraitContainer(addTo);
            if (addTo is Character) {
                additionalFullnessDecreaseRate = Mathf.CeilToInt(EditableValuesManager.Instance.baseFullnessDecreaseRate * 0.5f);
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is Character) {
                Character character = removedFrom as Character;
                character.needsComponent.SetFullnessForcedTick();
                character.needsComponent.AdjustFullnessDecreaseRate(-additionalFullnessDecreaseRate);
                character.behaviourComponent.RemoveBehaviourComponent(typeof(GluttonBehaviour));
            }
        }
        public override string TriggerFlaw(Character character) {
            if (!character.jobQueue.HasJob(JOB_TYPE.TRIGGER_FLAW)) {
                //Will perform Fullness Recovery.
                if (!character.traitContainer.HasTrait("Burning")) {
                    character.needsComponent.TriggerFlawFullnessRecovery(character);
                } else {
                    return "burning";
                }
            } else {
                return "has_trigger_flaw";
            }
            return base.TriggerFlaw(character);
        }
        #endregion
    }
}
