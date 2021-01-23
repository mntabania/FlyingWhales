using System;
using System.Collections.Generic;
namespace Quests.Steps {
    public class ProtectHumansStep : QuestStep, AffattWinConditionTracker.Listener {
        private readonly Func<List<Character>, int, string> _descriptionGetter;

        public ProtectHumansStep(Func<List<Character>, int, string> descriptionGetter) : base(string.Empty) {
            _descriptionGetter = descriptionGetter;
        }

        protected override void SubscribeListeners() {
            QuestManager.Instance.GetWinConditionTracker<AffattWinConditionTracker>().Subscribe(this);
        }
        protected override void UnSubscribeListeners() {
            QuestManager.Instance.GetWinConditionTracker<AffattWinConditionTracker>().Unsubscribe(this);
        }
        private void CheckForCompletion() {
            if (QuestManager.Instance.GetWinConditionTracker<AffattWinConditionTracker>().humans.Count < AffattWinConditionTracker.MinimumHumans) {
                FailStep();
            }
        }

        #region Listeners
        public void OnCharacterEliminated(Character p_character, int p_elvenCount, int p_humanCount) {
            objectsToCenter?.Remove(p_character);
            Messenger.Broadcast(UISignals.UPDATE_QUEST_STEP_ITEM, this as QuestStep);
            CheckForCompletion();
        }
        public void OnCharacterAddedAsTarget(Character p_character, int p_elvenCount, int p_humanCount) {
            objectsToCenter?.Add(p_character);
            Messenger.Broadcast(UISignals.UPDATE_QUEST_STEP_ITEM, this as QuestStep);
            CheckForCompletion();
        }

        #endregion

        #region Description
        protected override string GetStepDescription() {
            if (_descriptionGetter != null) {
                return _descriptionGetter.Invoke(QuestManager.Instance.GetWinConditionTracker<AffattWinConditionTracker>().humans, 
                    QuestManager.Instance.GetWinConditionTracker<AffattWinConditionTracker>().totalHumansToProtect);
            }
            return base.GetStepDescription();
        }
        #endregion
    }
}