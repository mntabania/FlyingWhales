using Tutorial;
namespace Quests {
    public class HasFinishedImportantTutorials : QuestCriteria {

        public override void Enable() {
            Messenger.AddListener(Signals.FINISHED_IMPORTANT_TUTORIALS, SetCriteriaAsMet);
        }
        public override void Disable() {
            Messenger.RemoveListener(Signals.FINISHED_IMPORTANT_TUTORIALS, SetCriteriaAsMet);
        }
    }
}