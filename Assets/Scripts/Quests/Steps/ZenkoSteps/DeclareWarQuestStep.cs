using System;
using System.Collections.Generic;
using Factions.Faction_Types;

namespace Quests.Steps {
    public class DeclareWarQuestStep : QuestStep, ZenkoWinConditionTracker.IListener {

        private readonly Func<int, string> _descriptionGetter;

        public DeclareWarQuestStep(Func<int, string> descriptionGetter) : base(string.Empty) {
            _descriptionGetter = descriptionGetter;
        }

        protected override void SubscribeListeners() {
            QuestManager.Instance.GetWinConditionTracker<ZenkoWinConditionTracker>().Subscribe(this);
        }
        protected override void UnSubscribeListeners() {
            QuestManager.Instance.GetWinConditionTracker<ZenkoWinConditionTracker>().Unsubscribe(this);
        }

        private void CheckForCompletion(int p_activeWars) {
            if (p_activeWars >= ZenkoWinConditionTracker.ActiveWarRequirement) {
                Complete();
                Messenger.Broadcast(PlayerSignals.WIN_GAME, $"You've triggered {ZenkoWinConditionTracker.ActiveWarRequirement.ToString()} concurrent wars. Congratulations!");
            }
        }

        #region Listeners
        public void OnFactionRelationshipChanged(int p_activeWars) {
            CheckForCompletion(p_activeWars);
            Messenger.Broadcast(UISignals.UPDATE_QUEST_STEP_ITEM, this as QuestStep);
        }
        public void OnFactionDisbanded(Faction p_faction) {
            Messenger.Broadcast(UISignals.UPDATE_QUEST_STEP_ITEM, this as QuestStep);
        }
        #endregion

        #region Description
        protected override string GetStepDescription() {
            if (_descriptionGetter != null) {
                return _descriptionGetter.Invoke(QuestManager.Instance.GetWinConditionTracker<ZenkoWinConditionTracker>().activeWars);
            }
            return base.GetStepDescription();
        }
        #endregion
    }
}