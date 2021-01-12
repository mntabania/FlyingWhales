using System;
using System.Collections.Generic;

namespace Quests.Steps {
    public class EliminateVillagerStep : QuestStep, EliminateVillagerTracker.IListener {
        private readonly Func<List<Character>, int, string> _descriptionGetter;

        public EliminateVillagerStep(Func<List<Character>, int, string> descriptionGetter) : base(string.Empty) {
            _descriptionGetter = descriptionGetter;
        }

        protected override void SubscribeListeners() {
            QuestManager.Instance.eliminateVillagerTracker.Subscribe(this);
        }
        protected override void UnSubscribeListeners() {
            QuestManager.Instance.eliminateVillagerTracker.Unsubscribe(this);
        }

        #region Listeners
        public void OnCharacterEliminated(Character p_character) {
            objectsToCenter?.Remove(p_character);
            Messenger.Broadcast(UISignals.UPDATE_QUEST_STEP_ITEM, this as QuestStep);
            CheckForCompletion();
        }
        public void OnCharacterAddedAsTarget(Character p_character) {
            objectsToCenter?.Add(p_character);
            Messenger.Broadcast(UISignals.UPDATE_QUEST_STEP_ITEM, this as QuestStep);
        }
        private void CheckForCompletion() {
            if (QuestManager.Instance.eliminateVillagerTracker.totalCharactersToEliminate <= 0) {
                Complete();
                Messenger.Broadcast(PlayerSignals.WIN_GAME);
            }
        }
        #endregion

        #region Description
        protected override string GetStepDescription() {
            if (_descriptionGetter != null) {
                return _descriptionGetter.Invoke(QuestManager.Instance.eliminateVillagerTracker.villagersToEliminate, QuestManager.Instance.eliminateVillagerTracker.totalCharactersToEliminate);
            }
            return base.GetStepDescription();
        }
        #endregion

        
    }
}