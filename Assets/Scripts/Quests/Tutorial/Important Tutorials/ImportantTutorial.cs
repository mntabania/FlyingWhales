namespace Tutorial {
    public abstract class ImportantTutorial : TutorialQuest {
        
        protected ImportantTutorial(string _questName, TutorialManager.Tutorial _tutorialType) : base(_questName, _tutorialType) { }
        
        #region Availability
        protected override void MakeAvailable() {
            base.MakeAvailable();
            TutorialManager.Instance.AddTutorialToWaitList(this);
        }
        protected override void MakeUnavailable() {
            base.MakeUnavailable();
            TutorialManager.Instance.RemoveTutorialFromWaitList(this);
        }
        #endregion
    }
}