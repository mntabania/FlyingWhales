using System;
using System.Collections.Generic;
using Factions.Faction_Types;

namespace Quests.Steps {
    public class DeclareWarQuestStep : QuestStep, ZenkoWinConditionTracker.Listener {

        private Dictionary<FactionType, List<FactionType>> m_declaredWarList = new Dictionary<FactionType, List<FactionType>>();


        private readonly Func<int, string> _descriptionGetter;

        public DeclareWarQuestStep(Func<int, string> descriptionGetter) : base(string.Empty) {
            _descriptionGetter = descriptionGetter;
        }

        protected override void SubscribeListeners() {
            (QuestManager.Instance.winConditionTracker as ZenkoWinConditionTracker).Subscribe(this);
        }
        protected override void UnSubscribeListeners() {
            (QuestManager.Instance.winConditionTracker as ZenkoWinConditionTracker).Unsubscribe(this);
        }

        private void CheckForCompletion(int p_remainingWarDeclaration) {
            if (p_remainingWarDeclaration <= 0) {
                Complete();
                Messenger.Broadcast(PlayerSignals.WIN_GAME);
            }
        }

        #region Listeners
        public void OnFactionRelationshipChanged(int p_remainingWarDeclaration) {
            CheckForCompletion(p_remainingWarDeclaration);
        }
        #endregion

        #region Description
        protected override string GetStepDescription() {
            if (_descriptionGetter != null) {
                return _descriptionGetter.Invoke((QuestManager.Instance.winConditionTracker as ZenkoWinConditionTracker).RemainingWarDeclaration);
            }
            return base.GetStepDescription();
        }
        #endregion
    }
}