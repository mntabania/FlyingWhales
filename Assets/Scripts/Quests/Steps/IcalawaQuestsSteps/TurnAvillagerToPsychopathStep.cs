using System;
using System.Collections.Generic;

namespace Quests.Steps {
    public class TurnAvillagerToPsychopathStep : QuestStep, IcalawaWinConditionTracker.IListenerChangeTraits {
        private readonly Func<string, string> _descriptionGetter;

        public TurnAvillagerToPsychopathStep(Func<string, string> descriptionGetter) : base(string.Empty) {
            _descriptionGetter = descriptionGetter;
        }

        protected override void SubscribeListeners() {
            (QuestManager.Instance.winConditionTracker as IcalawaWinConditionTracker).SubscribeToChangeTraitEvents(this);
        }
        protected override void UnSubscribeListeners() {
            (QuestManager.Instance.winConditionTracker as IcalawaWinConditionTracker).UnsubscribeToChangeTraitEvents(this);
        }

        #region Listeners
        public void OnCharacterChangeTrait(Character p_character) {
            Messenger.Broadcast(UISignals.UPDATE_QUEST_STEP_ITEM, this as QuestStep);
            CheckForCompletion(p_character);
        }
        private void CheckForCompletion(Character p_character) {
            Complete();
        }
        #endregion

        #region Description
        protected override string GetStepDescription() {
            if (_descriptionGetter != null) {
                return _descriptionGetter.Invoke((QuestManager.Instance.winConditionTracker as IcalawaWinConditionTracker).psychoPath.firstNameWithColor);
            }
            return base.GetStepDescription();
        }
        #endregion
    }
}