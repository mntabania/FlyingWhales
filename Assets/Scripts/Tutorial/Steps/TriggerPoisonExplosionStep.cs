namespace Tutorial {
    public class TriggerPoisonExplosionStep : TutorialQuestStep {
        public TriggerPoisonExplosionStep(string stepDescription = "Trigger Poison Explosion") : base(stepDescription) { }
        protected override void SubscribeListeners() {
            Messenger.AddListener<IPointOfInterest>(Signals.POISON_EXPLOSION_TRIGGERED, CheckForCompletion);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener<IPointOfInterest>(Signals.POISON_EXPLOSION_TRIGGERED, CheckForCompletion);
        }

        #region Listeners
        private void CheckForCompletion(IPointOfInterest poi) {
            Complete();
        }
        #endregion
    }
}