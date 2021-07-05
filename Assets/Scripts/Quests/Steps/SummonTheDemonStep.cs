using System;
using System.Collections.Generic;

namespace Quests.Steps {
    public class SummonTheDemonStep : QuestStep {
        private readonly Func<int, int, string> _descriptionGetter;

        public SummonTheDemonStep(Func<int, int, string> descriptionGetter) : base(string.Empty) {
            _descriptionGetter = descriptionGetter;
        }

        protected override void SubscribeListeners() {
            // (QuestManager.Instance.winConditionTracker as UpgradePortalWinConditionTracker).SubscribeToPortalUpgraded(this);
        }
        protected override void UnSubscribeListeners() {
            // (QuestManager.Instance.winConditionTracker as UpgradePortalWinConditionTracker).SubscribeToPortalUpgraded(this);
        }
        public override void Activate() {
            base.Activate();
            CheckForCompletion();
        }

        #region Listeners
        public void OnPortalLevelUpgraded(int p_newPortalLevel) {
            // (QuestManager.Instance.winConditionTracker as UpgradePortalWinConditionTracker).currentLevel = p_newPortalLevel;
            // Messenger.Broadcast(UISignals.UPDATE_QUEST_STEP_ITEM, this as QuestStep);
            // CheckForCompletion();
        }
        private void CheckForCompletion() {
            // if ((QuestManager.Instance.winConditionTracker as UpgradePortalWinConditionTracker).currentLevel >= (QuestManager.Instance.winConditionTracker as UpgradePortalWinConditionTracker).targetLevel) {
            //     Complete();
            //     Messenger.Broadcast(PlayerSignals.WIN_GAME, "The almighty demon has been summoned. Congratulations!");
            // }
        }
        #endregion

        #region Description
        // protected override string GetStepDescription() {
        //     if (_descriptionGetter != null) {
        //         return _descriptionGetter.Invoke((QuestManager.Instance.winConditionTracker as UpgradePortalWinConditionTracker).currentLevel, (QuestManager.Instance.winConditionTracker as UpgradePortalWinConditionTracker).TargetLevel);
        //     }
        //     return base.GetStepDescription();
        // }
        #endregion

    }
}