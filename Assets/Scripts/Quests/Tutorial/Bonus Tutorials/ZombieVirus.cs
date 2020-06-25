using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Quests;
using Quests.Steps;
using UnityEngine;
namespace Tutorial {
    public class ZombieVirus : LogQuest {

        private Character _targetCharacter;
        
        public ZombieVirus() : base("Zombie Virus", TutorialManager.Tutorial.Zombie_Virus) { }

        #region Criteria
        protected override void ConstructCriteria() {
            _activationCriteria = new List<QuestCriteria>() {
                new CharacterGainedTrait("Infected", trait => trait.responsibleCharacter != null || trait.gainedFromDoing != null)
                    .SetOnMeetAction(OnCharacterInfected),
                // new IsAtTime(new [] {
                //     GameManager.Instance.GetTicksBasedOnHour(7),
                //     GameManager.Instance.GetTicksBasedOnHour(13),
                //     GameManager.Instance.GetTicksBasedOnHour(19)
                // })
            };
            Messenger.AddListener(Signals.HOUR_STARTED, OnHourStarted);
        }
        private void OnHourStarted() {
            GameDate today = GameManager.Instance.Today();
            int hour = GameManager.Instance.GetHoursBasedOnTicks(today.tick);
            if (hour == 7 || hour == 13 || hour == 19) {
                TryMakeAvailable();
            }
        }
        private void OnCharacterInfected(QuestCriteria criteria) {
            if (criteria is CharacterGainedTrait characterGainedTrait) {
                _targetCharacter = characterGainedTrait.character;
            }
        }
        #endregion

        #region Availability
        protected override void MakeAvailable() {
            base.MakeAvailable();
            Messenger.RemoveListener(Signals.HOUR_STARTED, OnHourStarted);
            TutorialManager.Instance.ActivateTutorialButDoNotShow(this);
            PlayerUI.Instance.ShowGeneralConfirmation("Infection", 
                $"A character has just {UtilityScripts.Utilities.ColorizeAction("contracted the Zombie Virus")}! " +
                "A Tutorial Quest has been added to walk you through our Zombies feature.", 
                onClickOK: () => TutorialManager.Instance.ShowTutorial(this)
            );
        }
        #endregion

        #region Activation
        public override void Activate() {
            base.Activate();
            Messenger.RemoveListener(Signals.HOUR_STARTED, OnHourStarted);
            Messenger.AddListener<Log, IPointOfInterest>(Signals.LOG_REMOVED, OnLogRemoved);
        }
        public override void Deactivate() {
            base.Deactivate();
            Messenger.RemoveListener(Signals.HOUR_STARTED, OnHourStarted);
            Messenger.RemoveListener<Log, IPointOfInterest>(Signals.LOG_REMOVED, OnLogRemoved);
        }
        private void OnLogRemoved(Log log, IPointOfInterest poi) {
            if (poi == _targetCharacter && log.key.Equals("contracted_zombie") 
                && log.GetFillerForIdentifier(LOG_IDENTIFIER.ACTIVE_CHARACTER).obj == _targetCharacter) {
                //if log about contracting zombie virus was removed from the target character, consider this quest as failed.
                TutorialManager.Instance.FailTutorialQuest(this); 
                
            }
        }
        #endregion
        
        #region Steps
        protected override void ConstructSteps() {
            steps = new List<QuestStepCollection>() {
                new QuestStepCollection(
                    new ClickOnCharacterStep("Find a newly Infected character", IsCharacterValid).SetObjectsToCenter(_targetCharacter),
                    new ToggleTurnedOnStep("CharacterInfo_Logs", "Click on Log tab", () => UIManager.Instance.GetCurrentlySelectedPOI() == _targetCharacter),
                    new LogHistoryItemClicked("Click on Infected source", IsClickedLogObjectValid)
                        .SetHoverOverAction(OnHoverInfectionLog)
                        .SetHoverOutAction(UIManager.Instance.HideSmallInfo)
                        .SetCompleteAction(OnCompleteFindInfectedSource)
                )
            };
        }
        #endregion

        #region Step Helpers
        private bool IsCharacterValid(Character character) {
            return character == _targetCharacter;
        }
        private bool IsClickedLogObjectValid(object obj, Log log, IPointOfInterest owner) {
            if (owner == _targetCharacter && obj is Character clickedCharacter && clickedCharacter != _targetCharacter
                && log.file.Equals("NonIntel") && log.key.Equals("contracted_zombie")) {
                return true;
            }
            return false;
        }
        private void OnCompleteFindInfectedSource() {
            TutorialManager.Instance.StartCoroutine(DelayedFindInfectedSourcePopup());
        }
        private IEnumerator DelayedFindInfectedSourcePopup() {
            yield return new WaitForSecondsRealtime(1.5f);
            PlayerUI.Instance.ShowGeneralConfirmation("Zombie Virus",
                $"Infected Villagers and Monsters become zombies when they die! " +
                $"They {UtilityScripts.Utilities.ColorizeAction("arise at dusk")} and then become {UtilityScripts.Utilities.ColorizeAction("lifeless again at dawn")}. " +
                "They are hostile to everyone except other undead - that includes your minions. " +
                $"They can {UtilityScripts.Utilities.ColorizeAction("infect those they attack")}, so go ahead and start your own zombie apocalypse!"
            );
        }
        private void OnHoverInfectionLog(QuestStepItem item) {
            UIManager.Instance.ShowSmallInfo(
                $"An {UtilityScripts.Utilities.ColorizeAction("infection log")} should have been registered in the Villager's Log tab. " +
                $"{UtilityScripts.Utilities.ColorizeAction("Click on the name")} of the Zombie that spread it.",
                TutorialManager.Instance.infectedLog, "Infected Log", item.hoverPosition
            );
        }
        #endregion
    }
}