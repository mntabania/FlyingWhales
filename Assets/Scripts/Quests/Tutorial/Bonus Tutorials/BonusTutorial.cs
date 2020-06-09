namespace Tutorial {
    public abstract class BonusTutorial : TutorialQuest {
        protected BonusTutorial(string _questName, TutorialManager.Tutorial _tutorialType) : base(_questName, _tutorialType) { }
        protected override void ConstructSteps() {
            steps = null;
        }
    }
}