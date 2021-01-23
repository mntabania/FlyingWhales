using System;
using System.Collections.Generic;

namespace Quests.Steps {
    public class KillVillagersByPsychopathStep : QuestStep, IcalawaWinConditionTracker.IListenerKillingEvents {
        private readonly Func<int, string> _descriptionGetter;

        public KillVillagersByPsychopathStep(Func<int, string> descriptionGetter) : base(string.Empty) {
            _descriptionGetter = descriptionGetter;
        }

        protected override void SubscribeListeners() {
            QuestManager.Instance.GetWinConditionTracker<IcalawaWinConditionTracker>().SubscribeToKillingEvents(this);
        }
        protected override void UnSubscribeListeners() {
            QuestManager.Instance.GetWinConditionTracker<IcalawaWinConditionTracker>().UnsubscribeToKillingEvents(this);
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
                Messenger.Broadcast(PlayerSignals.WIN_GAME, $"You've helped the Evil Psychopath kill {IcalawaWinConditionTracker.TotalCharactersToKill.ToString()} Villagers. Congratulations!");
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