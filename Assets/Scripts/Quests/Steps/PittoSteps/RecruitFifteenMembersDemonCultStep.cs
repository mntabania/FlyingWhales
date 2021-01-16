using System;
using System.Collections.Generic;

namespace Quests.Steps {
    public class RecruitFifteenMembersDemonCultStep : QuestStep, IcalawaWinConditionTracker.IListenerChangeTraits {
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
        private void CheckForCompletion(Character p_character) {
            if ((QuestManager.Instance.winConditionTracker as PittoWinConditionTracker).culstists.Count >= 15){
                Complete();
                Messenger.Broadcast(PlayerSignals.WIN_GAME);
            }
            
        }
        #endregion

        #region Description
        protected override string GetStepDescription() {
            if (_descriptionGetter != null) {
                return _descriptionGetter.Invoke((QuestManager.Instance.winConditionTracker as PittoWinConditionTracker).culstists.Count);
            }
            return base.GetStepDescription();
        }
        #endregion
    }
}