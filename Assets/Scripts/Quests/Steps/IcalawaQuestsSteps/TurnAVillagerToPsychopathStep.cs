using System;
using System.Collections.Generic;

namespace Quests.Steps {
    public class TurnAVillagerToPsychopathStep : QuestStep, IcalawaWinConditionTracker.IListenerChangeTraits {
        private readonly Func<string, string> _descriptionGetter;

        public TurnAVillagerToPsychopathStep(Func<string, string> descriptionGetter) : base(string.Empty) {
            _descriptionGetter = descriptionGetter;
        }

        protected override void SubscribeListeners() {
            QuestManager.Instance.GetWinConditionTracker<IcalawaWinConditionTracker>().SubscribeToChangeTraitEvents(this);
        }
        protected override void UnSubscribeListeners() {
            QuestManager.Instance.GetWinConditionTracker<IcalawaWinConditionTracker>().UnsubscribeToChangeTraitEvents(this);
        }
        protected override bool CheckIfStepIsAlreadyCompleted() {
            var icalawaWinConditionTracker = QuestManager.Instance.GetWinConditionTracker<IcalawaWinConditionTracker>();
            return icalawaWinConditionTracker.psychoPath.traitContainer.HasTrait("Psychopath");
        }

        #region Listeners
        public void OnCharacterGainedPsychopathTrait(Character p_character) {
            CheckForCompletion(p_character);
            Messenger.Broadcast(UISignals.UPDATE_QUEST_STEP_ITEM, this as QuestStep);
        }
        private void CheckForCompletion(Character p_character) {
            if (CheckIfStepIsAlreadyCompleted()) {
                Complete();    
            }
        }
        #endregion

        #region Description
        protected override string GetStepDescription() {
            if (_descriptionGetter != null) {
                return _descriptionGetter.Invoke(QuestManager.Instance.GetWinConditionTracker<IcalawaWinConditionTracker>().psychoPath.visuals.GetCharacterNameWithIconAndColor());
            }
            return base.GetStepDescription();
        }
        #endregion
    }
}