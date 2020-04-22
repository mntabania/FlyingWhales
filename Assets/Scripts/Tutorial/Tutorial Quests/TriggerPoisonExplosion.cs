using System;
using System.Collections.Generic;
namespace Tutorial {
    public class TriggerPoisonExplosion : TutorialQuest {
        public TriggerPoisonExplosion() : base("Trigger a Poison Explosion", TutorialManager.Tutorial.Trigger_Poison_Explosion) { }
        public override void WaitForAvailability() {
            Messenger.AddListener<SpellData>(Signals.ON_EXECUTE_SPELL, OnSpellExecuted);
            Messenger.AddListener<PlayerAction>(Signals.ON_EXECUTE_PLAYER_ACTION, OnSpellExecuted);
        }
        protected override void StopWaitingForAvailability() {
            Messenger.RemoveListener<SpellData>(Signals.ON_EXECUTE_SPELL, OnSpellExecuted);
            Messenger.RemoveListener<PlayerAction>(Signals.ON_EXECUTE_PLAYER_ACTION, OnSpellExecuted);
        }
        protected override void ConstructSteps() {
            steps = new List<TutorialQuestStepCollection>() {
                new TutorialQuestStepCollection(
                    new TriggerPoisonExplosionStep("Deal fire damage")
                        .SetHoverOverAction(OnHoverTriggerPoisonExplosionStep)
                        .SetHoverOutAction(OnHoverOutTriggerPoisonExplosionStep)
                )
            };
        }

        #region Listeners
        private void OnSpellExecuted(SpellData spellData) {
            if (spellData.type == SPELL_TYPE.POISON || spellData.type == SPELL_TYPE.SPLASH_POISON) {
                MakeAvailable();
            }
        }
        #endregion

        #region Step Helpers
        private void OnHoverTriggerPoisonExplosionStep(TutorialQuestStepItem stepItem) {
            UIManager.Instance.ShowSmallInfo("All damage in Ruinarch can have an Elemental Type (Fire, Water, Wind, etc.)." +
                                             "You can trigger a poison explosion by dealing <color=\"red\">Fire Damage</color>" +
                                             " on a <color=\"green\">Poisoned</color> object.", 
                TutorialManager.Instance.fireDamageVideoClip, "Dealing Fire Damage", stepItem.hoverPosition);
        }
        private void OnHoverOutTriggerPoisonExplosionStep() {
            UIManager.Instance.HideSmallInfo();
        }
        #endregion
    }
}