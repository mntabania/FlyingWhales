using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
namespace Tutorial {
    public class HarassAVillage : TutorialQuest {
        
        private Coroutine availabilityTimer;

        private bool hasNotCasted;
        
        public override int priority => 5;
        public HarassAVillage() : base("Harass A Village", TutorialManager.Tutorial.Harass_A_Village) { }
        
        #region Overrides
        public override void WaitForAvailability() {
            //wait for not casting
            Messenger.AddListener<SpellData>(Signals.ON_EXECUTE_SPELL, OnSpellExecutedWhileInactive);
            Messenger.AddListener<SpellData>(Signals.ON_EXECUTE_AFFLICTION, OnSpellExecutedWhileInactive);
            Messenger.AddListener<PlayerAction>(Signals.ON_EXECUTE_PLAYER_ACTION, OnSpellExecutedWhileInactive);
            availabilityTimer = TutorialManager.Instance.StartCoroutine(WaitForSeconds());
        }
        public override void Activate() {
            base.Activate();
            Messenger.RemoveListener<SpellData>(Signals.ON_EXECUTE_SPELL, OnSpellExecutedWhileInactive);
            Messenger.RemoveListener<SpellData>(Signals.ON_EXECUTE_AFFLICTION, OnSpellExecutedWhileInactive);
            Messenger.RemoveListener<PlayerAction>(Signals.ON_EXECUTE_PLAYER_ACTION, OnSpellExecutedWhileInactive);
        }
        public override void Deactivate() {
            base.Deactivate();
            Messenger.RemoveListener<SpellData>(Signals.ON_EXECUTE_SPELL, OnSpellExecutedWhileInactive);
            Messenger.RemoveListener<SpellData>(Signals.ON_EXECUTE_AFFLICTION, OnSpellExecutedWhileInactive);
            Messenger.RemoveListener<PlayerAction>(Signals.ON_EXECUTE_PLAYER_ACTION, OnSpellExecutedWhileInactive);
        }
        protected override void ConstructSteps() {
            steps = new List<TutorialQuestStepCollection>() {
                new TutorialQuestStepCollection(new ClickOnStructureStep("Select a Demonic Structure", "Demonic")),
            };
        }
        public override void PerFrameActions() {
            if (isAvailable) { return; }
            if (isAvailable == false) {
                //if tutorial quest is not yet available.
                if (HasMetCriteria()) {
                    MakeAvailable();
                }
            } else if (TutorialManager.Instance.IsInWaitList(this)) {
                //if tutorial is in wait list, check if quest is still valid.
                if (HasMetCriteria() == false) {
                    MakeUnavailable();
                }
            }
        }
        #endregion
        

        #region Availability Functions
        private void OnSpellExecutedWhileInactive(SpellData spellData) {
            //reset wait time
            TutorialManager.Instance.StopCoroutine(availabilityTimer);
            availabilityTimer = TutorialManager.Instance.StartCoroutine(WaitForSeconds());
            hasNotCasted = false;
        }
        private IEnumerator WaitForSeconds() {
            yield return new WaitForSecondsRealtime(10);
            hasNotCasted = true;
        }
        private bool HasMetCriteria() {
            if (hasNotCasted == false) { return false; }
            if (TutorialManager.Instance.HasActiveTutorial()) { return false; }
            if (InnerMapManager.Instance.currentlyShowingLocation == null) { return false; }
            if (InnerMapManager.Instance.currentlyShowingLocation.HasActiveSettlement() == false) { return false; }
            if (PlayerManager.Instance.player.minions.Count <= 0) { return false; }
            return true;
        }
        #endregion
        

        #region Step Helpers
        private bool IsSelectedCharacterValid(Character character) {
            return character.IsNormalCharacter();
        }
        #endregion
    }
}