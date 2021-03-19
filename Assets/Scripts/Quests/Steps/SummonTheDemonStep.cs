using System;
using System.Collections.Generic;

namespace Quests.Steps {
    public class SummonTheDemonStep : QuestStep, OonaWinConditionTracker.Listener {
        private readonly Func<int, int, string> _descriptionGetter;

        public SummonTheDemonStep(Func<int, int, string> descriptionGetter) : base(string.Empty) {
            _descriptionGetter = descriptionGetter;
        }

        protected override void SubscribeListeners() {
            (QuestManager.Instance.winConditionTracker as OonaWinConditionTracker).Subscribe(this);
        }
        protected override void UnSubscribeListeners() {
            (QuestManager.Instance.winConditionTracker as OonaWinConditionTracker).Unsubscribe(this);
        }
        public override void Activate() {
            base.Activate();
            CheckForCompletion();
        }

        #region Listeners
        public void OnSummonMeterUpdated(int p_currentSummonCount, int p_targetSummonCount) {
            (QuestManager.Instance.winConditionTracker as OonaWinConditionTracker).currentSummonPoints = p_currentSummonCount;
            (QuestManager.Instance.winConditionTracker as OonaWinConditionTracker).targetSummonPoints = p_targetSummonCount;
            Messenger.Broadcast(UISignals.UPDATE_QUEST_STEP_ITEM, this as QuestStep);
            CheckForCompletion();
        }
        private void CheckForCompletion() {
            if ((QuestManager.Instance.winConditionTracker as OonaWinConditionTracker).currentSummonPoints >= (QuestManager.Instance.winConditionTracker as OonaWinConditionTracker).targetSummonPoints) {
                Complete();
                Messenger.Broadcast(PlayerSignals.WIN_GAME, "The almighty demon has been summoned. Congratulations!");
            }
        }
        #endregion

        #region Description
        protected override string GetStepDescription() {
            if (_descriptionGetter != null) {
                return _descriptionGetter.Invoke((QuestManager.Instance.winConditionTracker as OonaWinConditionTracker).currentSummonPoints, (QuestManager.Instance.winConditionTracker as OonaWinConditionTracker).targetSummonPoints);
            }
            return base.GetStepDescription();
        }
        #endregion


    }
}