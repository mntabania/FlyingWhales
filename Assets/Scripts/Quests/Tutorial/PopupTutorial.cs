namespace Tutorial {
    public abstract class PopupTutorial : TutorialQuest {
        protected PopupTutorial(string _questName, TutorialManager.Tutorial _tutorialType) : base(_questName, _tutorialType) { }
        protected override void ConstructSteps() {
            steps = null;
        }
        protected override void MakeAvailable() {
            isAvailable = true;
            Activate();
        }
        public override void Activate() {
            StopCheckingCriteria();
        }
    }
}