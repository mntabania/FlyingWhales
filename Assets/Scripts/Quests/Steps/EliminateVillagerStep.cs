using System;
using System.Collections.Generic;

namespace Quests.Steps {
    public class EliminateVillagerStep : QuestStep, OonaWinConditionTracker.Listener {
        private readonly Func<List<Character>, int, string> _descriptionGetter;

        public EliminateVillagerStep(Func<List<Character>, int, string> descriptionGetter) : base(string.Empty) {
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
        public void OnCharacterEliminated(Character p_character, int count) {
            objectsToCenter?.Remove(p_character);
            Messenger.Broadcast(UISignals.UPDATE_QUEST_STEP_ITEM, this as QuestStep);
            CheckForCompletion();
        }
        public void OnCharacterAddedAsTarget(Character p_character) {
            objectsToCenter?.Add(p_character);
            Messenger.Broadcast(UISignals.UPDATE_QUEST_STEP_ITEM, this as QuestStep);
        }
        private void CheckForCompletion() {
            if ((QuestManager.Instance.winConditionTracker as OonaWinConditionTracker).totalCharactersToEliminate <= 0) {
                Complete();
                Messenger.Broadcast(PlayerSignals.WIN_GAME);
            }
        }
        #endregion

        #region Description
        protected override string GetStepDescription() {
            if (_descriptionGetter != null) {
                return _descriptionGetter.Invoke((QuestManager.Instance.winConditionTracker as OonaWinConditionTracker).villagersToEliminate, (QuestManager.Instance.winConditionTracker as OonaWinConditionTracker).totalCharactersToEliminate);
            }
            return base.GetStepDescription();
        }
        #endregion

        
    }
}