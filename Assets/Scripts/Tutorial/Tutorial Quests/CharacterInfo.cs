using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Tutorial {
    public class CharacterInfo : TutorialQuest {
        
        private Coroutine availabilityTimer;
        public override int priority => 5;
        
        public CharacterInfo() : base("Character Info", TutorialManager.Tutorial.Character_Info) { }

        #region Overrides
        public override void WaitForAvailability() {
            Messenger.AddListener<SpellData>(Signals.ON_EXECUTE_SPELL, OnSpellExecutedWhileWaitingForAvailability);
            Messenger.AddListener<SpellData>(Signals.ON_EXECUTE_AFFLICTION, OnSpellExecutedWhileWaitingForAvailability);
            Messenger.AddListener<PlayerAction>(Signals.ON_EXECUTE_PLAYER_ACTION, OnSpellExecutedWhileWaitingForAvailability);
            availabilityTimer = TutorialManager.Instance.StartCoroutine(WaitForSeconds());
            
            Messenger.RemoveListener<SpellData>(Signals.ON_EXECUTE_SPELL, OnSpellExecutedWhileInWaitList);
            Messenger.RemoveListener<SpellData>(Signals.ON_EXECUTE_AFFLICTION, OnSpellExecutedWhileInWaitList);
            Messenger.RemoveListener<PlayerAction>(Signals.ON_EXECUTE_PLAYER_ACTION, OnSpellExecutedWhileInWaitList);
        }
        protected override void StopWaitingForAvailability() {
            Messenger.RemoveListener<SpellData>(Signals.ON_EXECUTE_SPELL, OnSpellExecutedWhileWaitingForAvailability);
            Messenger.RemoveListener<SpellData>(Signals.ON_EXECUTE_AFFLICTION, OnSpellExecutedWhileWaitingForAvailability);
            Messenger.RemoveListener<PlayerAction>(Signals.ON_EXECUTE_PLAYER_ACTION, OnSpellExecutedWhileWaitingForAvailability);
            if (availabilityTimer != null) {
                TutorialManager.Instance.StopCoroutine(availabilityTimer);
            }
            Messenger.AddListener<SpellData>(Signals.ON_EXECUTE_SPELL, OnSpellExecutedWhileInWaitList);
            Messenger.AddListener<SpellData>(Signals.ON_EXECUTE_AFFLICTION, OnSpellExecutedWhileInWaitList);
            Messenger.AddListener<PlayerAction>(Signals.ON_EXECUTE_PLAYER_ACTION, OnSpellExecutedWhileInWaitList);
        }
        public override void Activate() {
            base.Activate();
            //stop listening for spell execution while in wait list.
            Messenger.RemoveListener<SpellData>(Signals.ON_EXECUTE_SPELL, OnSpellExecutedWhileInWaitList);
            Messenger.RemoveListener<SpellData>(Signals.ON_EXECUTE_AFFLICTION, OnSpellExecutedWhileInWaitList);
            Messenger.RemoveListener<PlayerAction>(Signals.ON_EXECUTE_PLAYER_ACTION, OnSpellExecutedWhileInWaitList);
        }
        public override void Deactivate() {
            base.Deactivate();
            Messenger.RemoveListener<SpellData>(Signals.ON_EXECUTE_SPELL, OnSpellExecutedWhileInWaitList);
            Messenger.RemoveListener<SpellData>(Signals.ON_EXECUTE_AFFLICTION, OnSpellExecutedWhileInWaitList);
            Messenger.RemoveListener<PlayerAction>(Signals.ON_EXECUTE_PLAYER_ACTION, OnSpellExecutedWhileInWaitList);
        }
        protected override void ConstructSteps() {
            steps = new List<TutorialQuestStepCollection>() {
                new TutorialQuestStepCollection(
                    new ClickOnCharacterStep("Click on a sapient character", validityChecker: IsSelectedCharacterValid),
                    new ToggleTurnedOnStep("CharacterInfo_Info", "Open its Info tab"),
                    new ToggleTurnedOnStep("CharacterInfo_Mood", "Open its Mood tab"),
                    new ToggleTurnedOnStep("CharacterInfo_Relations", "Open its Relations tab"),
                    new ToggleTurnedOnStep("CharacterInfo_Logs", "Open its Logs tab")
                )
            };
        }
        #endregion

        #region Availability Functions
        private void OnSpellExecutedWhileWaitingForAvailability(SpellData spellData) {
            //reset wait time
            TutorialManager.Instance.StopCoroutine(availabilityTimer);
            availabilityTimer = TutorialManager.Instance.StartCoroutine(WaitForSeconds());
        }
        private IEnumerator WaitForSeconds() {
            yield return new WaitForSecondsRealtime(10);
            MakeAvailable();
        }
        #endregion
        
        #region Wait List Functions
        private void OnSpellExecutedWhileInWaitList(SpellData spellData) {
            MakeUnavailable();
        }
        #endregion

        #region Step Helpers
        private bool IsSelectedCharacterValid(Character character) {
            return character.IsNormalCharacter();
        }
        #endregion
    }
}