using System;
using System.Collections.Generic;

namespace Quests.Steps {
    public class EliminateNumberOfVillagersUsingPlagueStep : QuestStep {
        private readonly Func<List<Character>, int, string> _descriptionGetter;

        public EliminateNumberOfVillagersUsingPlagueStep(Func<List<Character>, int, string> descriptionGetter) : base(string.Empty) {
            _descriptionGetter = descriptionGetter;
        }

        protected override void SubscribeListeners() { }
        protected override void UnSubscribeListeners() { }

        #region Listeners
        private void CheckForCompletion(int p_villagerCount) {
            if (p_villagerCount <= 0) {
                Complete();
                Messenger.Broadcast(PlayerSignals.WIN_GAME, $"You managed to wipe out {PlagueDeathWinConditionTracker.Elimination_Requirement.ToString()} Villagers using Plague. Congratulations!");
            }
        }
        #endregion

        #region Description
        protected override string GetStepDescription() {
            if (_descriptionGetter != null) {
                return _descriptionGetter.Invoke(QuestManager.Instance.GetWinConditionTracker<PlagueDeathWinConditionTracker>().villagersToEliminate, 
                    QuestManager.Instance.GetWinConditionTracker<PlagueDeathWinConditionTracker>().totalCharactersToEliminate);
            }
            return base.GetStepDescription();
        }
        #endregion
    }
}