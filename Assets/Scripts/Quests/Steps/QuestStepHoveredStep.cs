using System.Collections;
using Tutorial;
using UnityEngine;
namespace Quests.Steps {
    public class QuestStepHoveredStep : QuestStep {

        private readonly QuestStep _requiredStep; //if this is null, then this step just requires to hover over itself.
        private Coroutine _waitCoroutine;
        public QuestStepHoveredStep(QuestStep requiredStep = null, string stepDescription = "Mouse hover over this") : base(stepDescription) {
            _requiredStep = requiredStep;
        }
        
        protected override void SubscribeListeners() {
            Messenger.AddListener<QuestStep>(Signals.QUEST_STEP_HOVERED, OnQuestStepHovered);
            Messenger.AddListener<QuestStep>(Signals.QUEST_STEP_HOVERED_OUT, OnQuestStepHoveredOut);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener<QuestStep>(Signals.QUEST_STEP_HOVERED, OnQuestStepHovered);
            Messenger.RemoveListener<QuestStep>(Signals.QUEST_STEP_HOVERED_OUT, OnQuestStepHoveredOut);
        }

        private void OnQuestStepHovered(QuestStep step) {
            if (_waitCoroutine == null) {
                _waitCoroutine = TutorialManager.Instance.StartCoroutine(Wait());    
            }
            
        }
        private void OnQuestStepHoveredOut(QuestStep step) {
            if (_waitCoroutine != null) {
                TutorialManager.Instance.StopCoroutine(_waitCoroutine);
                _waitCoroutine = null;
            }
        }
        private IEnumerator Wait() {
            yield return new WaitForSeconds(1f);
            Complete();
        }
        
        // private void CheckForCompletion(QuestStep step) {
        //     if (_requiredStep == step || (_requiredStep == null && step == this)) {
        //         Complete();
        //     }
        //     
        // }
    }
}