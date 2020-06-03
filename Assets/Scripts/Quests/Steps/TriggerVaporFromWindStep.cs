﻿namespace Quests.Steps {
    public class TriggerVaporFromWindStep : QuestStep {
        public TriggerVaporFromWindStep(string stepDescription = "Huff Wet Floor") : base(stepDescription) { }
        protected override void SubscribeListeners() {
            Messenger.AddListener(Signals.VAPOR_FROM_WIND_TRIGGERED, Complete);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener(Signals.VAPOR_FROM_WIND_TRIGGERED, Complete);
        }
    }
}