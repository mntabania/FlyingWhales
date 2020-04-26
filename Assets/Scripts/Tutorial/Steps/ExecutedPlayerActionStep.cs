using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
namespace Tutorial {
    public class ExecutedPlayerActionStep : TutorialQuestStep {

        private readonly SPELL_TYPE actionType;
        
        public ExecutedPlayerActionStep(SPELL_TYPE actionType, string stepDescription = "Click on a button")
            : base(stepDescription) {
            this.actionType = actionType;
        }
        protected override void SubscribeListeners() {
            Messenger.AddListener<PlayerAction>(Signals.ON_EXECUTE_PLAYER_ACTION, CheckForCompletion);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener<PlayerAction>(Signals.ON_EXECUTE_PLAYER_ACTION, CheckForCompletion);
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