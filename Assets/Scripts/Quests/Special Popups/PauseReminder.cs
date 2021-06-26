using System.Collections.Generic;
namespace Quests.Special_Popups {
    public class PauseReminder  : SpecialPopup {

        private int _timesPaused;
        public PauseReminder() : base("Pause Reminder", QuestManager.Special_Popup.Pause_Reminder) {
            _timesPaused = 0;
        }
        protected override void ConstructCriteria() {
            _activationCriteria = new List<QuestCriteria>(
                new QuestCriteria[] {
                    new IsAtDay(1),
                    new IsAtTime(new[] {GameManager.Instance.GetTicksBasedOnHour(23)}), 
                }    
            );
            Messenger.AddListener(UISignals.PAUSED_BY_PLAYER, OnPausedByPlayer);
        }
        private void OnPausedByPlayer() {
            _timesPaused++;
            if (_timesPaused >= 2) {
                CompleteQuest();
            }
        }
        protected override bool HasMetAllCriteria() {
            bool hasMetAllCriteria = base.HasMetAllCriteria();
            if (hasMetAllCriteria) {
                if (_timesPaused < 2) {
                    return true;
                } else {
                    Deactivate(); //deactivate this quest.
                    return false;
                }
            }
            return false;
        }
        public override void Activate() {
            Messenger.RemoveListener(UISignals.PAUSED_BY_PLAYER, OnPausedByPlayer);
            PlayerUI.Instance.ShowGeneralConfirmation("Pause", 
                $"Don't forget that you can {UtilityScripts.Utilities.ColorizeAction("Pause")} the game at anytime! " +
                "A smart Ruinarch will liberally use his time-bending powers to ruminate over what's taking place in the world." +
                $"\n\nUse the {UtilityScripts.Utilities.ColorizeAction("Space bar")} to quickly toggle between pause and unpause."
            );
            CompleteQuest();
        }
        public override void Deactivate() {
            base.Deactivate();
            StopCheckingCriteria();
            Messenger.RemoveListener(UISignals.PAUSED_BY_PLAYER, OnPausedByPlayer);
        }
    }
}