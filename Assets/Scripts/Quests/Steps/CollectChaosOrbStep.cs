using System;
namespace Quests.Steps {
    public class CollectChaosOrbStep : QuestStep {
        
        public CollectChaosOrbStep(string stepDescription = "Hover on a Mana Orb") : base(stepDescription) { }
        protected override void SubscribeListeners() {
            Messenger.AddListener(Signals.CHAOS_ORB_COLLECTED, Complete);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener(Signals.CHAOS_ORB_COLLECTED, Complete);
        }
    }
}