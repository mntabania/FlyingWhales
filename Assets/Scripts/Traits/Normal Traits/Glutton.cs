using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Glutton : Trait {

        private float additionalFullnessDecreaseRate;
        private Character m_owner;

        public Glutton() {
            name = "Glutton";
            description = "Eats a lot! If afflicted by the player, will produce a Chaos Orb each time it eats.";
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
                if (character.HasAfflictedByPlayerWith(this)) {
                    character.traitComponent.SubscribeToGluttonLevelUpSignal();
                    SubscribeToAfflictionSignals();
                }
                additionalFullnessDecreaseRate = GetHungerDecreaseRate(character);
                // character.needsComponent.SetFullnessForcedTick(0);
                character.needsComponent.AdjustFullnessDecreaseRate(additionalFullnessDecreaseRate);
                character.behaviourComponent.AddBehaviourComponent(typeof(GluttonBehaviour));
            }
        }
        public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
            base.LoadTraitOnLoadTraitContainer(addTo);
            if (addTo is Character character) {
                m_owner = character;
                if (character.HasAfflictedByPlayerWith(this)) {
                    character.traitComponent.SubscribeToGluttonLevelUpSignal();
                    SubscribeToAfflictionSignals();
                }
                additionalFullnessDecreaseRate = GetHungerDecreaseRate(character);
            }
        }
        public void OnGluttonLeveledUp() {
            m_owner.needsComponent.AdjustFullnessDecreaseRate(-additionalFullnessDecreaseRate);
            additionalFullnessDecreaseRate = GetHungerDecreaseRate(m_owner);
            m_owner.needsComponent.AdjustFullnessDecreaseRate(additionalFullnessDecreaseRate);
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is Character character) {
                UnsubscribeToAfflictionSignals();
                character.traitComponent.UnsubscribeToGluttonLevelUpSignal();
                // character.needsComponent.SetFullnessForcedTick();
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
            var hungerRate = p_character.HasAfflictedByPlayerWith(this) ? 
                PlayerSkillManager.Instance.GetAfflictionHungerRatePerLevel(PLAYER_SKILL_TYPE.GLUTTONY) : 
                PlayerSkillManager.Instance.GetAfflictionHungerRatePerLevel(PLAYER_SKILL_TYPE.GLUTTONY, 0);
            hungerRate /= 100f;
            return EditableValuesManager.Instance.baseFullnessDecreaseRate * hungerRate;
        }

        #region Chaos Orbs
        private void SubscribeToAfflictionSignals() {
            Messenger.AddListener<ActualGoapNode>(JobSignals.STARTED_PERFORMING_ACTION, OnActionPerformed);
        }
        private void UnsubscribeToAfflictionSignals() {
            if (Messenger.eventTable.ContainsKey(JobSignals.STARTED_PERFORMING_ACTION)) {
                Messenger.RemoveListener<ActualGoapNode>(JobSignals.STARTED_PERFORMING_ACTION, OnActionPerformed);
            }
        }
        private void OnActionPerformed(ActualGoapNode p_action) {
            if (p_action.action.actionCategory == ACTION_CATEGORY.CONSUME && p_action.actor == m_owner) {
                DispenseChaosOrbsForAffliction(m_owner, 1);
            }
        }
        #endregion
    }
}
