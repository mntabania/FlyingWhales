using System;
using System.Collections.Generic;

namespace Quests.Steps {
    public class KillVillagersByPsychopathStep : QuestStep, IcalawaWinConditionTracker.Listener {
        private readonly Func<int, string> _descriptionGetter;

        public KillVillagersByPsychopathStep(Func<int, string> descriptionGetter) : base(string.Empty) {
            _descriptionGetter = descriptionGetter;
        }

        protected override void SubscribeListeners() {
            (QuestManager.Instance.winConditionTracker as IcalawaWinConditionTracker).Subscribe(this);
        }
        protected override void UnSubscribeListeners() {
            (QuestManager.Instance.winConditionTracker as IcalawaWinConditionTracker).Unsubscribe(this);
        }

        #region Listeners
        public void OnCharacterEliminated(Character p_character) {
            objectsToCenter?.Remove(p_character);
            Messenger.Broadcast(UISignals.UPDATE_QUEST_STEP_ITEM, this as QuestStep);
            CheckForCompletion(p_character);
        }
        public void OnCharacterAddedAsTarget(Character p_character) {
            objectsToCenter?.Add(p_character);
            Messenger.Broadcast(UISignals.UPDATE_QUEST_STEP_ITEM, this as QuestStep);
        }
        private void CheckForCompletion(Character p_character) {
            UnityEngine.Debug.LogError(p_character.name + " -- " + (QuestManager.Instance.winConditionTracker as IcalawaWinConditionTracker).psychoPath.name);
            if ((QuestManager.Instance.winConditionTracker as IcalawaWinConditionTracker).totalCharactersToEliminate <= 0) {
                Complete();
                Messenger.Broadcast(PlayerSignals.WIN_GAME);
            } else if (p_character == (QuestManager.Instance.winConditionTracker as IcalawaWinConditionTracker).psychoPath) {
                PlayerUI.Instance.LoseGameOver("Psychopath Died, Mission Failed");
            }
        }
        #endregion

        #region Description
        protected override string GetStepDescription() {
            if (_descriptionGetter != null) {
                return _descriptionGetter.Invoke((QuestManager.Instance.winConditionTracker as IcalawaWinConditionTracker).totalCharactersToEliminate);
            }
            return base.GetStepDescription();
        }
        #endregion
    }
}