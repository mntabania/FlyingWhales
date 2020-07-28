using Quests.Steps;
namespace Quests {
    public abstract class ReactionQuest : SteppedQuest {

        protected ReactionQuest(string _questName) : base(_questName) { }
        
        public override void Activate() {
            ConstructSteps();
            base.Activate();
        }
        protected override void CompleteQuest() {
            QuestManager.Instance.CompleteQuest(this);
        }
        protected override void FailQuest() {
            base.FailQuest();
            QuestManager.Instance.CompleteQuest(this);
        }
    }
}