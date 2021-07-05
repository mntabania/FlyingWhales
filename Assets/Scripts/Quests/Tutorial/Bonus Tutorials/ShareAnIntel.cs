using System.Collections.Generic;
using JetBrains.Annotations;
using Quests;
using Quests.Steps;
using UnityEngine;
namespace Tutorial {
    [UsedImplicitly]
    public class ShareAnIntel : BonusTutorial {
        public ShareAnIntel() : base("Share an Intel", TutorialManager.Tutorial.Share_An_Intel) { }
        
        #region Criteria
        protected override void ConstructCriteria() {
            _activationCriteria = new List<QuestCriteria>() {
                new HasCompletedTutorialQuest(TutorialManager.Tutorial.Afflictions)
            };
        }
        protected override bool HasMetAllCriteria() {
            bool hasMetAllCriteria = base.HasMetAllCriteria();
            if (hasMetAllCriteria) {
                return PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.WATCHER).isInUse;
            }
            return false;
        }
        #endregion

        #region Overrides
        protected override void MakeAvailable() {
            if (!PlayerManager.Instance.player.playerSettlement.HasStructure(STRUCTURE_TYPE.WATCHER)) {
                 SkillData spellData = PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.WATCHER);
                 if (spellData.charges <= 0) {
                     //if player does not yet have an eye structure and does not have an eye charge, then give them one so they can build one for this tutorial
                     spellData.AdjustCharges(1);
                 }
            }
            base.MakeAvailable();
            TutorialManager.Instance.ActivateTutorial(this);
        }
        public override void Activate() {
            base.Activate();
            Messenger.Broadcast(UISignals.UPDATE_BUILD_LIST);

        }
        protected override void ConstructSteps() {
            steps = new List<QuestStepCollection>();
            if (!PlayerManager.Instance.player.playerSettlement.HasStructure(STRUCTURE_TYPE.WATCHER)) {
                //Only add build eye step if player doesn't already have an eye built
                QuestStepCollection buildCollection = new QuestStepCollection(
                    new ToggleTurnedOnStep("Build Tab", "Open Build Menu")
                        .SetOnTopmostActions(OnTopMostBuildTab, OnNoLongerTopMostBuildTab),
                    new ToggleTurnedOnStep("Eye", "Choose the Eye")
                        .SetOnTopmostActions(OnTopMostTheEye, OnNoLongerTopMostTheEye),
                    new StructureBuiltStep(STRUCTURE_TYPE.WATCHER, "Place on an unoccupied Area")
                );
                steps.Add(buildCollection);
            }
            QuestStepCollection storeIntelStep = new QuestStepCollection(new StoreIntelStep()
                .SetHoverOverAction(OnHoverStoreIntel)
                .SetHoverOutAction(UIManager.Instance.HideSmallInfo)
                .SetOnTopmostActions(OnTopMostStoreIntel, OnNoLongerTopMostStoreIntel)
            );
            steps.Add(storeIntelStep);
            QuestStepCollection shareIntelStep = new QuestStepCollection(
                new ShowIntelMenuStep()
                    .SetOnTopmostActions(OnTopMostIntelTab, OnNoLongerTopMostIntelTab),
                new SelectIntelStep("Choose the stored intel"),
                new ShareIntelStep("Share to a Villager")
                    .SetHoverOverAction(OnHoverShareIntel)
                    .SetHoverOutAction(UIManager.Instance.HideSmallInfo)
            );
            steps.Add(shareIntelStep);
        }
        #endregion
        
        #region Step Helpers
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
            Messenger.Broadcast(UISignals.SHOW_SELECTABLE_GLOW, "Build Structure");
        }
        private void OnNoLongerTopMostBuildStructure() {
            Messenger.Broadcast(UISignals.HIDE_SELECTABLE_GLOW, "Build Structure");
        }
        #endregion
        
        #region The Eye
        private void OnTopMostTheEye() {
            Messenger.Broadcast(UISignals.SHOW_SELECTABLE_GLOW, "Eye");
        }
        private void OnNoLongerTopMostTheEye() {
            Messenger.Broadcast(UISignals.HIDE_SELECTABLE_GLOW, "Eye");
        }
        #endregion
        
        #region Store Intel
        private void OnTopMostStoreIntel() {
            Messenger.Broadcast(UISignals.SHOW_SELECTABLE_GLOW, "Store Intel Button");
        }
        private void OnNoLongerTopMostStoreIntel() {
            Messenger.Broadcast(UISignals.HIDE_SELECTABLE_GLOW, "Store Intel Button");
        }
        #endregion
        
        #region Intel Tab
        private void OnTopMostIntelTab() {
            Messenger.Broadcast(UISignals.SHOW_SELECTABLE_GLOW, "Intel Tab");
        }
        private void OnNoLongerTopMostIntelTab() {
            Messenger.Broadcast(UISignals.HIDE_SELECTABLE_GLOW, "Intel Tab");
        }
        #endregion
        
        #region Build Tab
        private void OnTopMostBuildTab() {
            Messenger.Broadcast(UISignals.SHOW_SELECTABLE_GLOW, "Build Tab");
        }
        private void OnNoLongerTopMostBuildTab() {
            Messenger.Broadcast(UISignals.HIDE_SELECTABLE_GLOW, "Build Tab");
        }
        #endregion
    }
}