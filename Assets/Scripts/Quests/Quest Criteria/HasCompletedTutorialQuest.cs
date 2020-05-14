using Tutorial;
namespace Quests {
    public class HasCompletedTutorialQuest : QuestCriteria {
        private readonly TutorialManager.Tutorial _tutorialToComplete;
        
        public HasCompletedTutorialQuest(TutorialManager.Tutorial tutorialToComplete) {
            _tutorialToComplete = tutorialToComplete;
        }
        
        public override void Enable() {
            if (TutorialManager.Instance.HasTutorialBeenCompleted(_tutorialToComplete)) {
                SetCriteriaAsMet();
            } else {
                Messenger.AddListener<TutorialQuest>(Signals.TUTORIAL_QUEST_COMPLETED, OnTutorialQuestCompleted);    
            }
        }
        public override void Disable() {
            Messenger.RemoveListener<TutorialQuest>(Signals.TUTORIAL_QUEST_COMPLETED, OnTutorialQuestCompleted);
        }
        
        private void OnTutorialQuestCompleted(TutorialQuest completedTutorial) {
            if (completedTutorial.tutorialType == _tutorialToComplete) {
                SetCriteriaAsMet();
            }
        }
    }
}