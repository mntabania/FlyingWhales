using System;
using System.Collections.Generic;

namespace Quests.Steps {
    public class EliminateNumberOfVillagersUsingPlagueStep : QuestStep, OonaWinConditionTracker.Listener {
        private readonly Func<List<Character>, int, string> _descriptionGetter;

        public EliminateNumberOfVillagersUsingPlagueStep(Func<List<Character>, int, string> descriptionGetter) : base(string.Empty) {
            _descriptionGetter = descriptionGetter;
        }

        protected override void SubscribeListeners() {
            (QuestManager.Instance.winConditionTracker as AneemWinConditionTracker).Subscribe(this);
        }
        protected override void UnSubscribeListeners() {
            (QuestManager.Instance.winConditionTracker as AneemWinConditionTracker).Unsubscribe(this);
        }

        #region Listeners
        public void OnCharacterEliminated(Character p_character, int p_villagerCount) {
            objectsToCenter?.Remove(p_character);
            Messenger.Broadcast(UISignals.UPDATE_QUEST_STEP_ITEM, this as QuestStep);
            CheckForCompletion(p_villagerCount);
        }
        public void OnCharacterAddedAsTarget(Character p_character) {
            objectsToCenter?.Add(p_character);
            Messenger.Broadcast(UISignals.UPDATE_QUEST_STEP_ITEM, this as QuestStep);
        }
        private void CheckForCompletion(int p_villagerCount) {
            if (p_villagerCount <= 0) {
                Complete();
                Messenger.Broadcast(PlayerSignals.WIN_GAME);
            }
        }
        #endregion

        #region Description
        protected override string GetStepDescription() {
            if (_descriptionGetter != null) {
                return _descriptionGetter.Invoke((QuestManager.Instance.winConditionTracker as AneemWinConditionTracker).villagersToEliminate, (QuestManager.Instance.winConditionTracker as AneemWinConditionTracker).totalCharactersToEliminate);
            }
            return base.GetStepDescription();
        }
        #endregion
    }
}