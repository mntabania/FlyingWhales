using System;
using System.Collections.Generic;

namespace Quests.Steps {
    public class RecruitFifteenMembersDemonCultStep : QuestStep, PittoWinConditionTracker.IListenerChangeTraits {
        private readonly Func<int, string> _descriptionGetter;

        public RecruitFifteenMembersDemonCultStep(Func<int, string> descriptionGetter) : base(string.Empty) {
            _descriptionGetter = descriptionGetter;
        }

        protected override void SubscribeListeners() {
            (QuestManager.Instance.winConditionTracker as PittoWinConditionTracker).SubscribeToChangeTraitEvents(this);
        }
        protected override void UnSubscribeListeners() {
            (QuestManager.Instance.winConditionTracker as PittoWinConditionTracker).UnsubscribeToChangeTraitEvents(this);
        }

        #region Listeners
        public void OnCharacterChangeTrait(Character p_character) {
            Messenger.Broadcast(UISignals.UPDATE_QUEST_STEP_ITEM, this as QuestStep);
            CheckForCompletion(p_character);
        }

        public void OnCharacterDied(Character p_character) {
            Messenger.Broadcast(UISignals.UPDATE_QUEST_STEP_ITEM, this as QuestStep);
            CheckForCompletion(p_character);
        }

        private void CheckForCompletion(Character p_character) {
            if ((QuestManager.Instance.winConditionTracker as PittoWinConditionTracker).cultists.Count >= 12){
                Complete();
                Messenger.Broadcast(PlayerSignals.WIN_GAME, "You've successfully setup a sizable Demon Cult. Congratulations!");
            }
        }
        #endregion

        #region Description
        protected override string GetStepDescription() {
            if (_descriptionGetter != null) {
                return _descriptionGetter.Invoke((QuestManager.Instance.winConditionTracker as PittoWinConditionTracker).cultists.Count);
            }
            return base.GetStepDescription();
        }
        #endregion
    }
}