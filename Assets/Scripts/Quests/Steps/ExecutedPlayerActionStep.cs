namespace Quests.Steps {
    public class ExecutedPlayerActionStep : QuestStep {

        private readonly PLAYER_SKILL_TYPE actionType;
        
        public ExecutedPlayerActionStep(PLAYER_SKILL_TYPE actionType, string stepDescription = "Click on a button")
            : base(stepDescription) {
            this.actionType = actionType;
        }
        protected override void SubscribeListeners() {
            Messenger.AddListener<PlayerAction>(PlayerSkillSignals.ON_EXECUTE_PLAYER_ACTION, CheckForCompletion);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener<PlayerAction>(PlayerSkillSignals.ON_EXECUTE_PLAYER_ACTION, CheckForCompletion);
        }

        #region Listeners
        private void CheckForCompletion(PlayerAction playerAction) {
            if (playerAction.type == actionType) {
                Complete();
            }
        }
        #endregion
    }
}