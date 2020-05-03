using System.Collections.Generic;
using UnityEngine;
namespace Tutorial {
    public class ShareAnIntel : TutorialQuest {
        public ShareAnIntel() : base("Share an Intel", TutorialManager.Tutorial.Share_An_Intel) { }

        public override int priority => 30;

        #region Criteria
        protected override void ConstructCriteria() {
            _activationCriteria = new List<TutorialQuestCriteria>() {
                new PlayerHasNotCastedForSeconds(15f),
                new PlayerHasNotCompletedTutorialInSeconds(15f),
                new HasCompletedTutorialQuest(TutorialManager.Tutorial.Build_A_Kennel)
            };
            Messenger.AddListener(Signals.ON_OPEN_SHARE_INTEL, CompleteTutorial);
        }
        protected override bool HasMetAllCriteria() {
            bool hasMetAllCriteria = base.HasMetAllCriteria();
            if (hasMetAllCriteria) {
                return PlayerSkillManager.Instance.GetPlayerSkillData(SPELL_TYPE.THE_EYE).charges > 0;
            }
            return false;
        }
        #endregion

        #region Overrides
        public override void Activate() {
            Messenger.RemoveListener(Signals.ON_OPEN_SHARE_INTEL, CompleteTutorial);
            base.Activate();
        }
        public override void Deactivate() {
            base.Deactivate();
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