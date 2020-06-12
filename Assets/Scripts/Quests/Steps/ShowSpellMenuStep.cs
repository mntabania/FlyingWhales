namespace Quests.Steps {
    public class ShowSpellMenuStep : QuestStep {
        public ShowSpellMenuStep(string stepDescription = "Click on Spells tab") 
            : base(stepDescription) { }
        protected override void SubscribeListeners() {
            Messenger.AddListener(Signals.SPELLS_MENU_SHOWN, Complete);
            if (PlayerUI.Instance.spellsContainerGO.activeSelf) {
                Complete();
            }
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener(Signals.SPELLS_MENU_SHOWN, Complete);
        }
    }
}