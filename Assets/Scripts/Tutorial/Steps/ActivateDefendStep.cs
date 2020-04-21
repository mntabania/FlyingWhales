namespace Tutorial {
    public class ActivateDefendStep : TutorialQuestStep {
        public ActivateDefendStep(string stepDescription = "Click on Defend", string tooltip = "") 
            : base(stepDescription, tooltip) { }
        protected override void SubscribeListeners() {
            Messenger.AddListener(Signals.DEFEND_ACTIVATED, Complete);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener(Signals.DEFEND_ACTIVATED, Complete);
        }
    }
}