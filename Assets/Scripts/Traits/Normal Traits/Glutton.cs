using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Glutton : Trait {

        private float additionalFullnessDecreaseRate;
        private Character m_owner;

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
                m_owner = character;
                CheckIfShouldListenToLevelUpEvent(character);
                additionalFullnessDecreaseRate = GetHungerDecreaseRate(character);
                character.needsComponent.SetFullnessForcedTick(0);
                character.needsComponent.AdjustFullnessDecreaseRate(additionalFullnessDecreaseRate);
                character.behaviourComponent.AddBehaviourComponent(typeof(GluttonBehaviour));
            }
        }
        public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
            base.LoadTraitOnLoadTraitContainer(addTo);
            if (addTo is Character character) {
                m_owner = character;
                additionalFullnessDecreaseRate = GetHungerDecreaseRate(character);
            }
        }
        protected override void OnAfflictionLeveledUp(SkillData p_skillData, PlayerSkillData p_playerSkillData) {
            base.OnAfflictionLeveledUp(p_skillData, p_playerSkillData);
            m_owner.needsComponent.AdjustFullnessDecreaseRate(-additionalFullnessDecreaseRate);
            additionalFullnessDecreaseRate = GetHungerDecreaseRate(m_owner);
            m_owner.needsComponent.AdjustFullnessDecreaseRate(additionalFullnessDecreaseRate);
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is Character character) {
                UnsubscribeToLevelUpEvent(character);
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

        private float GetHungerDecreaseRate(Character p_character) {
            var hungerRate = p_character.WasAfflictedByPlayer(this) ? 
                PlayerSkillManager.Instance.GetAfflictionHungerRatePerLevel(PLAYER_SKILL_TYPE.GLUTTONY) : 
                PlayerSkillManager.Instance.GetAfflictionHungerRatePerLevel(PLAYER_SKILL_TYPE.GLUTTONY, 0);
            hungerRate /= 100f;
            return EditableValuesManager.Instance.baseFullnessDecreaseRate * hungerRate;
        }
    }
}
