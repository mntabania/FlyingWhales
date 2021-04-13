using System;
using System.Collections.Generic;

namespace Quests.Steps {
    public class SummonTheDemonStep : QuestStep, OonaWinConditionTracker.ListenerPortalUpgrade {
        private readonly Func<int, int, string> _descriptionGetter;

        public SummonTheDemonStep(Func<int, int, string> descriptionGetter) : base(string.Empty) {
            _descriptionGetter = descriptionGetter;
        }

        protected override void SubscribeListeners() {
            (QuestManager.Instance.winConditionTracker as OonaWinConditionTracker).SubscribeToPortalUpgraded(this);
        }
        protected override void UnSubscribeListeners() {
            (QuestManager.Instance.winConditionTracker as OonaWinConditionTracker).SubscribeToPortalUpgraded(this);
        }
        public override void Activate() {
            base.Activate();
            CheckForCompletion();
        }

        #region Listeners

        public void OnPortalLevelUpgraded(int p_newPortalLevel) {
            (QuestManager.Instance.winConditionTracker as OonaWinConditionTracker).currentLevel = p_newPortalLevel;
            Messenger.Broadcast(UISignals.UPDATE_QUEST_STEP_ITEM, this as QuestStep);
            CheckForCompletion();
        }
        private void CheckForCompletion() {
            if ((QuestManager.Instance.winConditionTracker as OonaWinConditionTracker).currentLevel >= (QuestManager.Instance.winConditionTracker as OonaWinConditionTracker).targetLevel) {
                Complete();
                Messenger.Broadcast(PlayerSignals.WIN_GAME, "The almighty demon has been summoned. Congratulations!");
            }
        }
        #endregion

        #region Description
        protected override string GetStepDescription() {
            if (_descriptionGetter != null) {
                return _descriptionGetter.Invoke((QuestManager.Instance.winConditionTracker as OonaWinConditionTracker).currentLevel, (QuestManager.Instance.winConditionTracker as OonaWinConditionTracker).targetLevel);
            }
            return base.GetStepDescription();
        }
        #endregion


    }
}