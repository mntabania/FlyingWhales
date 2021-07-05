using System;
using System.Collections.Generic;
namespace Quests.Steps {
    public class ProtectHumansStep : QuestStep {
        private readonly Func<List<Character>, int, string> _descriptionGetter;

        public ProtectHumansStep(Func<List<Character>, int, string> descriptionGetter) : base(string.Empty) {
            _descriptionGetter = descriptionGetter;
        }

        protected override void SubscribeListeners() { }
        protected override void UnSubscribeListeners() { }
        private void CheckForCompletion() {
            if (QuestManager.Instance.GetWinConditionTracker<HumansSurviveAndElvesWipedOutWinConditionTracker>().humans.Count < HumansSurviveAndElvesWipedOutWinConditionTracker.MinimumHumans) {
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
                return _descriptionGetter.Invoke(QuestManager.Instance.GetWinConditionTracker<HumansSurviveAndElvesWipedOutWinConditionTracker>().humans, 
                    QuestManager.Instance.GetWinConditionTracker<HumansSurviveAndElvesWipedOutWinConditionTracker>().totalHumansToProtect);
            }
            return base.GetStepDescription();
        }
        #endregion
    }
}