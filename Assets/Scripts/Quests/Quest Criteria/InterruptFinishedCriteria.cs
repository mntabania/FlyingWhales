using System;
namespace Quests {
    public class InterruptFinishedCriteria : QuestCriteria {
        public Character character { get; private set; }

        private INTERRUPT _neededInterrupt;
        
        public InterruptFinishedCriteria(INTERRUPT interrupt) {
            _neededInterrupt = interrupt;
        }
        
        public override void Enable() {
            Messenger.AddListener<INTERRUPT, Character>(CharacterSignals.INTERRUPT_FINISHED, OnInterruptFinished);
        }
        public override void Disable() {
            Messenger.RemoveListener<INTERRUPT, Character>(CharacterSignals.INTERRUPT_FINISHED, OnInterruptFinished);
        }
        
        private void OnInterruptFinished(INTERRUPT interrupt, Character actor) {
            if (interrupt == _neededInterrupt) {
                character = actor;
                SetCriteriaAsMet();
            }
        }
    }
}