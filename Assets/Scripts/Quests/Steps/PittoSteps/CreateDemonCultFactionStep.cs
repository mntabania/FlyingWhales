using System;
using System.Collections.Generic;

namespace Quests.Steps {
    public class CreateDemonFactionStep : QuestStep {
        private readonly Func<string> _descriptionGetter;

        public CreateDemonFactionStep(Func<string> descriptionGetter) : base(string.Empty) {
            _descriptionGetter = descriptionGetter;
        }

        protected override void SubscribeListeners() { }
        protected override void UnSubscribeListeners() { }
        protected override bool CheckIfStepIsAlreadyCompleted() {
            if (DatabaseManager.Instance.factionDatabase.GetFactionsWithFactionType(FACTION_TYPE.Demon_Cult)?.Count > 0) {
                return true;
            }
            return false;
        }

        #region Listeners
        public void OnFactionCreated(Faction p_faction) {
            Messenger.Broadcast(UISignals.UPDATE_QUEST_STEP_ITEM, this as QuestStep);
            CheckForCompletion();
        }
        private void CheckForCompletion() {
            Complete();
        }
        #endregion

        #region Description
        protected override string GetStepDescription() {
            if (_descriptionGetter != null) {
                return _descriptionGetter.Invoke();
            }
            return base.GetStepDescription();
        }
        #endregion
    }
}