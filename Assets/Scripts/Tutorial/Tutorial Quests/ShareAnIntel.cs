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
                    new ClickOnEmptyAreaStep(validityChecker: IsSelectedAreaValid)
                        .SetHoverOverAction(OnHoverEmptyArea)
                        .SetHoverOutAction(UIManager.Instance.HideSmallInfo),
                    new ObjectPickerShownStep("Click on Build Structure button", "Demonic Structure"), 
                    new StructureBuiltStep(STRUCTURE_TYPE.THE_EYE, "Build The Eye")
                ),
                new TutorialQuestStepCollection (new StoreIntelStep()
                    .SetHoverOverAction(OnHoverStoreIntel)
                    .SetHoverOutAction(UIManager.Instance.HideSmallInfo)),
                new TutorialQuestStepCollection(new ShowIntelMenuStep(),
                    new SelectIntelStep("Choose the stored intel"),
                    new ShareIntelStep("Share to a sapient character")
                        .SetHoverOverAction(OnHoverShareIntel)
                        .SetHoverOutAction(UIManager.Instance.HideSmallInfo)
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
            if (isAvailable && TutorialManager.Instance.IsInWaitList(this)) { 
                //if the tutorial is already available, but is inactive, make this tutorial unavailable again.
                MakeUnavailable();
            }
        }
        #endregion

        #region Step Helpers
        private bool IsSelectedAreaValid(HexTile tile) {
            return tile.region.HasActiveSettlement();
        }
        private void OnHoverEmptyArea(TutorialQuestStepItem item) {
            UIManager.Instance.ShowSmallInfo("In order to effectively use The Eye. It must be built at a region that " +
                                             "has an active settlement.", TutorialManager.Instance.villageVideoClip, 
                "The Eye", item.hoverPosition);
        }
        private void OnHoverStoreIntel(TutorialQuestStepItem item) {
            UIManager.Instance.ShowSmallInfo("Keep an eye at the bottom right of your screen, " +
                                             "because any time a character does something interesting, " +
                                             "The Eye will notify you there. Some notifications can be stored as Intel by clicking " +
                                             "on the eye icon next to it.\n\n NOTE: Only regions that have The Eye structure will notify you.", 
                TutorialManager.Instance.storeIntelVideoClip, "Storing Intel", item.hoverPosition);
        }
        private void OnHoverShareIntel(TutorialQuestStepItem item) {
            UIManager.Instance.ShowSmallInfo("After choosing which intel to share, your cursor will change. " +
                                             "Just hover your cursor over a character and click on them to share your selected intel. " +
                                             "\n\nNOTE: your cursor will change based on if your target is valid or not.", 
                TutorialManager.Instance.shareIntelVideoClip, "Sharing Intel", item.hoverPosition);
        }
        #endregion
    }
}