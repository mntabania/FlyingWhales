namespace Quests.Steps {
    public class ActivateInvadeStep : QuestStep {
        public ActivateInvadeStep(string stepDescription = "Click on Invade") 
            : base(stepDescription) { }
        protected override void SubscribeListeners() {
            Messenger.AddListener(Signals.INVADE_ACTIVATED, Complete);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener(Signals.INVADE_ACTIVATED, Complete);
        }
    }
}