using System;
namespace Quests.Steps {
    public class PlayerActionContextMenuShown : QuestStep {
        private readonly Func<IPlayerActionTarget, bool> _validityChecker;
        public PlayerActionContextMenuShown(Func<IPlayerActionTarget, bool> validityChecker, string stepDescription = "Right click on a character") : base(stepDescription) {
            _validityChecker = validityChecker;
        }
        protected override void SubscribeListeners() {
            Messenger.AddListener<IPlayerActionTarget>(UISignals.PLAYER_ACTION_CONTEXT_MENU_SHOWN, CheckForCompletion);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener<IPlayerActionTarget>(UISignals.PLAYER_ACTION_CONTEXT_MENU_SHOWN, CheckForCompletion);
        }

        #region Listeners
        private void CheckForCompletion(IPlayerActionTarget p_target) {
            if (_validityChecker.Invoke(p_target)) {
                Complete();
            }
        }
        #endregion
    }
}