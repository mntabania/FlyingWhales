using System;
using System.Collections.Generic;

namespace Quests.Steps {
    public class RecruitFifteenMembersDemonCultStep : QuestStep {
        private readonly Func<int, string> _descriptionGetter;

        public RecruitFifteenMembersDemonCultStep(Func<int, string> descriptionGetter) : base(string.Empty) {
            _descriptionGetter = descriptionGetter;
        }

        protected override void SubscribeListeners() { }
        protected override void UnSubscribeListeners() { }

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
            if ((QuestManager.Instance.winConditionTracker as RecruitCultistsWinConditionTracker).cultists.Count >= 12){
                Complete();
                Messenger.Broadcast(PlayerSignals.WIN_GAME, "Your Cultists performed the dark ritual, tainting the divine energy for your own consumption!");
            }
        }
        #endregion

        #region Description
        protected override string GetStepDescription() {
            if (_descriptionGetter != null) {
                return _descriptionGetter.Invoke((QuestManager.Instance.winConditionTracker as RecruitCultistsWinConditionTracker).cultists.Count);
            }
            return base.GetStepDescription();
        }
        #endregion
    }
}