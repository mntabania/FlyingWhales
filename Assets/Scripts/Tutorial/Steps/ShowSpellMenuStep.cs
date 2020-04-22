namespace Tutorial {
    public class ShowSpellMenuStep : TutorialQuestStep {
        public ShowSpellMenuStep(string stepDescription = "Click on Spells tab", string tooltip = "") 
            : base(stepDescription) { }
        protected override void SubscribeListeners() {
            Messenger.AddListener(Signals.SPELLS_MENU_SHOWN, Complete);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener(Signals.SPELLS_MENU_SHOWN, Complete);
        }
    }
}