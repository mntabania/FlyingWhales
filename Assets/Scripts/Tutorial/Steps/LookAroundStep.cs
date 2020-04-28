namespace Tutorial {
    public class LookAroundStep : TutorialQuestStep {
        public LookAroundStep(string stepDescription = "Look Around") : base(stepDescription) { }
        
        protected override void SubscribeListeners() {
            Messenger.AddListener(Signals.CAMERA_MOVED_BY_PLAYER, Complete);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener(Signals.CAMERA_MOVED_BY_PLAYER, Complete);
        }
    }
}