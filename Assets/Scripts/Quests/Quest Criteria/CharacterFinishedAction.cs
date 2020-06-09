using System;
namespace Quests {
    public class CharacterFinishedAction : QuestCriteria {
        private readonly INTERACTION_TYPE _interactionType;
        public ActualGoapNode finishedAction { get; private set; }
        
        public CharacterFinishedAction(INTERACTION_TYPE interactionType) {
            _interactionType = interactionType;
        }
        
        public override void Enable() {
            Messenger.AddListener<ActualGoapNode>(Signals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
        }
        public override void Disable() {
            Messenger.RemoveListener<ActualGoapNode>(Signals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
        }
        
        private void OnCharacterFinishedAction(ActualGoapNode actualGoapNode) {
            if (actualGoapNode.action.goapType == _interactionType 
                && actualGoapNode.actionStatus == ACTION_STATUS.SUCCESS) {
                finishedAction = actualGoapNode;
                SetCriteriaAsMet();
            }
        }
    }
}