using System;
namespace Quests.Steps {
    public class ClickOnChaosOrbStep : QuestStep {
        
        public ClickOnChaosOrbStep(string stepDescription = "Click on a Chaos Orb") : base(stepDescription) { }
        protected override void SubscribeListeners() {
            Messenger.AddListener(Signals.CHAOS_ORB_CLICKED, Complete);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener(Signals.CHAOS_ORB_CLICKED, Complete);
        }
    }
}