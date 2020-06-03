using System.Collections.Generic;
using Quests;
using Quests.Steps;
using UnityEngine;
namespace Tutorial {
    public class ShareAnIntel : TutorialQuest {
        public ShareAnIntel() : base("Share an Intel", TutorialManager.Tutorial.Share_An_Intel) { }

        public override int priority => 30;

        #region Criteria
        protected override void ConstructCriteria() {
            _activationCriteria = new List<QuestCriteria>() {
                new HasCompletedTutorialQuest(TutorialManager.Tutorial.Afflictions)
            };
            Messenger.AddListener(Signals.ON_OPEN_SHARE_INTEL, CompleteQuest);
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
            Messenger.RemoveListener(Signals.ON_OPEN_SHARE_INTEL, CompleteQuest);
            base.Activate();
        }
        public override void Deactivate() {
            base.Deactivate();
            Messenger.RemoveListener(Signals.ON_OPEN_SHARE_INTEL, CompleteQuest);
        }
        protected override void ConstructSteps() {
            steps = new List<QuestStepCollection>() {
                new QuestStepCollection (
                    new ClickOnEmptyAreaStep(validityChecker: IsSelectedAreaValid)
                        .SetHoverOverAction(OnHoverEmptyArea)
                        .SetHoverOutAction(UIManager.Instance.HideSmallInfo),
                    new ObjectPickerShownStep("Click on Build Structure button", "Demonic Structure")
                        .SetOnTopmostActions(OnTopMostBuildStructure, OnNoLongerTopMostBuildStructure), 
                    new StructureBuiltStep(STRUCTURE_TYPE.THE_EYE, "Build The Eye")
                        .SetOnTopmostActions(OnTopMostTheEye, OnNoLongerTopMostTheEye)
                ),
                new QuestStepCollection (new StoreIntelStep()
                    .SetHoverOverAction(OnHoverStoreIntel)
                    .SetHoverOutAction(UIManager.Instance.HideSmallInfo)
                    .SetOnTopmostActions(OnTopMostStoreIntel, OnNoLongerTopMostStoreIntel)
                ),
                new QuestStepCollection(
                    new ShowIntelMenuStep()
                        .SetOnTopmostActions(OnTopMostIntelTab, OnNoLongerTopMostIntelTab),
                    new SelectIntelStep("Choose the stored intel"),
                    new ShareIntelStep($"Share to a {UtilityScripts.Utilities.VillagerIcon()}Villager")
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
        private void OnHoverEmptyArea(QuestStepItem item) {
            UIManager.Instance.ShowSmallInfo("In order to effectively use The Eye. It must be built at a region that " +
                                             "has an active settlement.", TutorialManager.Instance.villageVideoClip, 
                "The Eye", item.hoverPosition);
        }
        private void OnHoverStoreIntel(QuestStepItem item) {
            UIManager.Instance.ShowSmallInfo("Keep an eye at the bottom right of your screen, " +
                                             "because any time a character does something interesting, " +
                                             "The Eye will notify you there. Some notifications can be stored as Intel by clicking " +
                                             "on the eye icon next to it.\n\n NOTE: Only regions that have The Eye structure will notify you.", 
                TutorialManager.Instance.storeIntelVideoClip, "Storing Intel", item.hoverPosition);
        }
        private void OnHoverShareIntel(QuestStepItem item) {
            UIManager.Instance.ShowSmallInfo("After choosing which intel to share, your cursor will change. " +
                                             "Just hover your cursor over a character and click on them to share your selected intel. " +
                                             "\n\nNOTE: your cursor will change based on if your target is valid or not.", 
                TutorialManager.Instance.shareIntelVideoClip, "Sharing Intel", item.hoverPosition);
        }
        #endregion
        
        #region Build Structure Button
        private void OnTopMostBuildStructure() {
            Messenger.Broadcast(Signals.SHOW_SELECTABLE_GLOW, "Build Structure");
        }
        private void OnNoLongerTopMostBuildStructure() {
            Messenger.Broadcast(Signals.HIDE_SELECTABLE_GLOW, "Build Structure");
        }
        #endregion
        
        #region The Eye
        private void OnTopMostTheEye() {
            Messenger.Broadcast(Signals.SHOW_SELECTABLE_GLOW, "The Eye");
        }
        private void OnNoLongerTopMostTheEye() {
            Messenger.Broadcast(Signals.HIDE_SELECTABLE_GLOW, "The Eye");
        }
        #endregion
        
        #region Store Intel
        private void OnTopMostStoreIntel() {
            Messenger.Broadcast(Signals.SHOW_SELECTABLE_GLOW, "Store Intel Button");
        }
        private void OnNoLongerTopMostStoreIntel() {
            Messenger.Broadcast(Signals.HIDE_SELECTABLE_GLOW, "Store Intel Button");
        }
        #endregion
        
        #region Intel Tab
        private void OnTopMostIntelTab() {
            Messenger.Broadcast(Signals.SHOW_SELECTABLE_GLOW, "Intel Tab");
        }
        private void OnNoLongerTopMostIntelTab() {
            Messenger.Broadcast(Signals.HIDE_SELECTABLE_GLOW, "Intel Tab");
        }
        #endregion
    }
}