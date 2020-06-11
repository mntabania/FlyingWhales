using System.Collections.Generic;
using Quests;
using Tutorial;
namespace Tutorial {
    public class PauseReminder  : BonusTutorial {

        private int _timesPaused;
        public PauseReminder() : base("Pause Reminder", TutorialManager.Tutorial.Pause_Reminder) {
            _timesPaused = 0;
        }
        protected override void ConstructCriteria() {
            _activationCriteria = new List<QuestCriteria>(
                new QuestCriteria[] {
                    new IsAtDay(1),
                    new IsAtTime(new[] {GameManager.Instance.GetTicksBasedOnHour(23)}), 
                }    
            );
            Messenger.AddListener(Signals.PAUSED_BY_PLAYER, OnPausedByPlayer);
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
        protected override void MakeAvailable() {
            base.MakeAvailable();
            TutorialManager.Instance.ActivateTutorialButDoNotShow(this);
        }
        public override void Activate() {
            Messenger.RemoveListener(Signals.PAUSED_BY_PLAYER, OnPausedByPlayer);
            StopCheckingCriteria();
            PlayerUI.Instance.ShowGeneralConfirmation("Pause", 
                $"Don't forget that you can {UtilityScripts.Utilities.ColorizeAction("Pause")} the game at anytime! " +
                "A smart Ruinarch will liberally use his time-bending powers to ruminate over what's taking place in the world." +
                $"\n\nUse the {UtilityScripts.Utilities.ColorizeAction("spacebar")} to quickly toggle between pause and unpause."
            );
            CompleteQuest();
        }
        public override void Deactivate() {
            base.Deactivate();
            Messenger.RemoveListener(Signals.PAUSED_BY_PLAYER, OnPausedByPlayer);
        }
    }
}