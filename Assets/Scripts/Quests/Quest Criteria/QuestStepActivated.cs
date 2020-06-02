using Quests.Steps;
using Tutorial;
namespace Quests {
    public class QuestStepActivated<T> : QuestCriteria where T : QuestStep {

        public override void Enable() {
            Messenger.AddListener<QuestStep>(Signals.QUEST_STEP_ACTIVATED, OnQuestStepActivated);
        }
        public override void Disable() {
            Messenger.RemoveListener<QuestStep>(Signals.QUEST_STEP_ACTIVATED, OnQuestStepActivated);
        }
        
        private void OnQuestStepActivated(QuestStep questStep) {
            if (questStep is T) {
                SetCriteriaAsMet();
            }
        }
    }
}