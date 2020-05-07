using Quests.Steps;
namespace Quests {
    public abstract class ReactionQuest : Quest {

        protected ReactionQuest(string _questName) : base(_questName) { }
        
        public override void Activate() {
            ConstructSteps();
            base.Activate();
        }
        protected override void CompleteQuest() {
            QuestManager.Instance.CompleteQuest(this);
        }
        protected void FailQuest() {
            //fail uncompleted steps.
            for (int i = 0; i < activeStepCollection.steps.Count; i++) {
                QuestStep step = activeStepCollection.steps[i];
                if (step.isCompleted == false) {
                    step.FailStep();
                }
            }
            QuestManager.Instance.CompleteQuest(this);
        }
    }
}