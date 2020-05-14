using System;
namespace Quests {
    public class ReactionQuestActivated<T> : QuestCriteria where T : Quest {

        public override void Enable() {
            Messenger.AddListener<Quest>(Signals.REACTION_QUEST_ACTIVATED, OnReactionQuestActivated);
        }
        
        public override void Disable() {
            Messenger.RemoveListener<Quest>(Signals.REACTION_QUEST_ACTIVATED, OnReactionQuestActivated);
        }
        
        private void OnReactionQuestActivated(Quest quest) {
            if (quest is T) {
                SetCriteriaAsMet();
            }
        }
    }
}