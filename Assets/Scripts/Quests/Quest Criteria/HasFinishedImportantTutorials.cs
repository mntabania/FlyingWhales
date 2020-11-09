using Tutorial;
namespace Quests {
    public class HasFinishedImportantTutorials : QuestCriteria {

        public override void Enable() {
            if (TutorialManager.Instance.hasCompletedImportantTutorials) {
                SetCriteriaAsMet();
            } else {
                Messenger.AddListener(PlayerQuestSignals.FINISHED_IMPORTANT_TUTORIALS, SetCriteriaAsMet);    
            }
        }
        public override void Disable() {
            Messenger.RemoveListener(PlayerQuestSignals.FINISHED_IMPORTANT_TUTORIALS, SetCriteriaAsMet);
        }
    }
}