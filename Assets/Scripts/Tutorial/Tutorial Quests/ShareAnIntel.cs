using System.Collections.Generic;
using UnityEngine;
namespace Tutorial {
    public class ShareAnIntel : TutorialQuest {
        public ShareAnIntel() : base("Share an Intel", TutorialManager.Tutorial.Share_An_Intel) { }

        private float _notCastedTime;
        private bool hasRecentlyCastASpell;
        public override int priority => 30;
        public override void WaitForAvailability() {
            Messenger.AddListener<SpellData>(Signals.ON_EXECUTE_SPELL, OnSpellExecutedWhileInactive);
            Messenger.AddListener<SpellData>(Signals.ON_EXECUTE_AFFLICTION, OnSpellExecutedWhileInactive);
            Messenger.AddListener<PlayerAction>(Signals.ON_EXECUTE_PLAYER_ACTION, OnSpellExecutedWhileInactive);
            Messenger.AddListener(Signals.ON_OPEN_SHARE_INTEL, CompleteTutorial);
        }
        public override void Activate() {
            Messenger.RemoveListener<SpellData>(Signals.ON_EXECUTE_SPELL, OnSpellExecutedWhileInactive);
            Messenger.RemoveListener<SpellData>(Signals.ON_EXECUTE_AFFLICTION, OnSpellExecutedWhileInactive);
            Messenger.RemoveListener<PlayerAction>(Signals.ON_EXECUTE_PLAYER_ACTION, OnSpellExecutedWhileInactive);
            Messenger.RemoveListener(Signals.ON_OPEN_SHARE_INTEL, CompleteTutorial);
            base.Activate();
        }
        public override void Deactivate() {
            base.Deactivate();
            Messenger.RemoveListener<SpellData>(Signals.ON_EXECUTE_SPELL, OnSpellExecutedWhileInactive);
            Messenger.RemoveListener<SpellData>(Signals.ON_EXECUTE_AFFLICTION, OnSpellExecutedWhileInactive);
            Messenger.RemoveListener<PlayerAction>(Signals.ON_EXECUTE_PLAYER_ACTION, OnSpellExecutedWhileInactive);
            Messenger.RemoveListener(Signals.ON_OPEN_SHARE_INTEL, CompleteTutorial);
        }
        protected override void ConstructSteps() {
            steps = new List<TutorialQuestStepCollection>() {
                new TutorialQuestStepCollection (
                    new ClickOnEmptyAreaStep(tooltip: "There must be a village in the region", validityChecker: IsSelectedAreaValid),
                    new ObjectPickerShownStep("Click on Build Structure button", "Demonic Structure"), 
                    new StructureBuiltStep(STRUCTURE_TYPE.THE_EYE, "Build The Eye")
                ),
                new TutorialQuestStepCollection (new StoreIntelStep()),
                new TutorialQuestStepCollection(new ShowIntelMenuStep(),
                    new SelectIntelStep("Choose the stored intel"),
                    new ShareIntelStep("Share to a sapient character")
                )
            };
        }
        public override void PerFrameActions() {
            if (isAvailable) { return; }
            _notCastedTime += Time.deltaTime;
            //player has recently cast a spell if notCastTime is less than given amount of time.
            hasRecentlyCastASpell = _notCastedTime < 10f;
            CheckIfCanBeMadeAvailable();
        }
        private void CheckIfCanBeMadeAvailable() {
            if (hasRecentlyCastASpell) {
                return;
            }
            if (TutorialManager.Instance.HasTutorialBeenCompleted(TutorialManager.Tutorial.Build_A_Kennel) == false) {
                return;
            }
            if (PlayerSkillManager.Instance.GetPlayerSkillData(SPELL_TYPE.THE_EYE).charges <= 0) {
                return;
            }
            MakeAvailable();
        }

        #region Inactive State Listeners
        private void OnSpellExecutedWhileInactive(SpellData spellData) {
            _notCastedTime = 0f; //reset cast timer.
            if (isAvailable) { 
                //if the tutorial is already available, but is inactive, make this tutorial unavailable again.
                MakeUnavailable();
            }
        }
        #endregion

        #region Step Helpers
        private bool IsSelectedAreaValid(HexTile tile) {
            return tile.region.HasActiveSettlement();
        }
        #endregion
    }
}