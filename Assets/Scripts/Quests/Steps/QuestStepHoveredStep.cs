using UnityEngine;
namespace Quests.Steps {
    public class QuestStepHoveredStep : QuestStep {

        private readonly QuestStep _requiredStep; //if this is null, then this step just requires to hover over itself.
        public QuestStepHoveredStep(QuestStep requiredStep = null, string stepDescription = "Mouse hover over this") : base(stepDescription) {
            _requiredStep = requiredStep;
        }
        
        protected override void SubscribeListeners() {
            Messenger.AddListener<QuestStep>(Signals.QUEST_STEP_HOVERED, CheckForCompletion);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener<QuestStep>(Signals.QUEST_STEP_HOVERED, CheckForCompletion);
        }


        private void CheckForCompletion(QuestStep step) {
            if (_requiredStep == step || (_requiredStep == null && step == this)) {
                Complete();
            }
            
        }
    }
}