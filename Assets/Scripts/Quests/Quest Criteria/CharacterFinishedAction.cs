using System;
namespace Quests {
    public class CharacterFinishedAction : QuestCriteria {
        private readonly INTERACTION_TYPE _interactionType;
        public ActualGoapNode finishedAction { get; private set; }
        
        public CharacterFinishedAction(INTERACTION_TYPE interactionType) {
            _interactionType = interactionType;
        }
        
        public override void Enable() {
            Messenger.AddListener<Character, IPointOfInterest, INTERACTION_TYPE, ACTION_STATUS>(JobSignals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
        }
        public override void Disable() {
            Messenger.RemoveListener<Character, IPointOfInterest, INTERACTION_TYPE, ACTION_STATUS>(JobSignals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
        }
        
        private void OnCharacterFinishedAction(Character p_actor, IPointOfInterest p_target, INTERACTION_TYPE p_type, ACTION_STATUS p_status) {
            if (p_type == _interactionType && p_status == ACTION_STATUS.SUCCESS) {
                //finishedAction = actualGoapNode;
                SetCriteriaAsMet();
            }
        }
    }
}