using System;
using System.Collections.Generic;

namespace Quests.Steps {
    public class WipeElvenKingdomAndSurviveHumanStep : QuestStep, AffatWinConditionTracker.Listener {
        private readonly Func<List<Character>, int, string> _descriptionGetter;

        public WipeElvenKingdomAndSurviveHumanStep(Func<List<Character>, int, string> descriptionGetter) : base(string.Empty) {
            _descriptionGetter = descriptionGetter;
        }

        protected override void SubscribeListeners() {
            (QuestManager.Instance.winConditionTracker as AffatWinConditionTracker).Subscribe(this);
        }
        protected override void UnSubscribeListeners() {
            (QuestManager.Instance.winConditionTracker as AffatWinConditionTracker).Unsubscribe(this);
        }

        private void CheckForCompletion(int p_elvenCount, int p_humanCount) {
            if (p_elvenCount <= 0) {
                Complete();
                Messenger.Broadcast(PlayerSignals.WIN_GAME);
            }
        }

        #region Listeners
        public void OnCharacterEliminated(Character p_character, int p_elvenCount, int p_humanCount) {
            objectsToCenter?.Remove(p_character);
            Messenger.Broadcast(UISignals.UPDATE_QUEST_STEP_ITEM, this as QuestStep);
            CheckForCompletion(p_elvenCount, p_humanCount);
        }
        public void OnCharacterAddedAsTarget(Character p_character, int p_elvenCount, int p_humanCount) {
            objectsToCenter?.Add(p_character);
            Messenger.Broadcast(UISignals.UPDATE_QUEST_STEP_ITEM, this as QuestStep);
            CheckForCompletion(p_elvenCount, p_humanCount);
        }

        #endregion

        #region Description
        protected override string GetStepDescription() {
            if (_descriptionGetter != null) {
                return _descriptionGetter.Invoke((QuestManager.Instance.winConditionTracker as AffatWinConditionTracker).elvensToEliminate, (QuestManager.Instance.winConditionTracker as AffatWinConditionTracker).elvensToEliminate.Count);
            }
            return base.GetStepDescription();
        }
        #endregion
    }
}