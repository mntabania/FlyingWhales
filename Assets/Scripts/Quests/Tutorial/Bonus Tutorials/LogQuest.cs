using Quests;
namespace Tutorial {
    public abstract class LogQuest : BonusTutorial {
        protected LogQuest(string _questName, TutorialManager.Tutorial _tutorialType) : base(_questName, _tutorialType) { }

        #region Criteria
        protected override bool HasMetAllCriteria() {
            bool hasMetAllCriteria = base.HasMetAllCriteria();
            if (hasMetAllCriteria) {
                //Log Quests should only pop-up when there is only 1 other active quest maximum and there are no Counterattack or Divine Intervention active
                //Reference: https://trello.com/c/skuSxlzf/1241-log-quests-should-only-pop-up-when-there-is-only-1-other-active-quest-maximum-and-there-are-no-counterattack-or-divine-intervent
                int activeQuestsCount = TutorialManager.Instance.GetAllActiveTutorialsCount() +
                                        QuestManager.Instance.GetActiveQuestsCount();
                bool hasActiveCounterattack = QuestManager.Instance.IsQuestActive<Quests.Counterattack>();
                bool hasActiveDivineIntervention = QuestManager.Instance.IsQuestActive<Quests.DivineIntervention>();
                return activeQuestsCount <= 1 && hasActiveCounterattack == false && hasActiveDivineIntervention == false;
            }
            return false;
        }
        #endregion
    }
}