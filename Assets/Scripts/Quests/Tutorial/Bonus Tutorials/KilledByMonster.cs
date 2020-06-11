using System.Collections.Generic;
using System.Linq;
using Quests;
using Quests.Steps;

namespace Tutorial {
    public class KilledByMonster : LogQuest {

        private Character _targetCharacter;
        
        public KilledByMonster() : base("Killed By a Monster", TutorialManager.Tutorial.Killed_By_Monster) { }

        #region Criteria
        protected override void ConstructCriteria() {
            _activationCriteria = new List<QuestCriteria>() {
                new CharacterDied(IsDeadCharacterValid).SetOnMeetAction(SetTargetCharacter)
            };
        }
        private bool IsDeadCharacterValid(Character character) {
            if (character.isNormalCharacter && character.deathLog.key.Equals("death_attacked") 
                && character.deathLog.HasFillerThatMeetsRequirement(o => o is Summon)) {
                return true;
            }
            return false;
        }
        protected override bool HasMetAllCriteria() {
            bool hasMetAllCriteria = base.HasMetAllCriteria();
            if (hasMetAllCriteria) {
                //no other active log quest
                return TutorialManager.Instance.HasActiveLogQuest() == false;
            }
            return false;
        }
        private void SetTargetCharacter(QuestCriteria criteria) {
            if (criteria is CharacterDied metCriteria) {
                _targetCharacter = metCriteria.character;
            }
        }
        #endregion

        #region Availability
        protected override void MakeAvailable() {
            base.MakeAvailable();
            TutorialManager.Instance.ActivateTutorialButDoNotShow(this);
            PlayerUI.Instance.ShowGeneralConfirmation("Killed by a Monster", 
                $"A {UtilityScripts.Utilities.VillagerIcon()}Villager has been {UtilityScripts.Utilities.ColorizeAction("killed by a monster")}! " +
                "A Tutorial Quest has been added to teach you how to figure out what happened.", 
                onClickOK: () => TutorialManager.Instance.ShowTutorial(this)
            );
        }
        #endregion

        #region Activation
        public override void Activate() {
            base.Activate();
            Messenger.AddListener<Log, IPointOfInterest>(Signals.LOG_REMOVED, OnLogRemoved);
        }
        public override void Deactivate() {
            base.Deactivate();
            Messenger.RemoveListener<Log, IPointOfInterest>(Signals.LOG_REMOVED, OnLogRemoved);
        }
        private void OnLogRemoved(Log log, IPointOfInterest poi) {
            if (poi == _targetCharacter && log.key.Equals("death_attacked")) {
                //check if target character still has any logs about being killed by a monster
                if (poi.logComponent.GetLogsInCategory("Generic").Count(x => x.HasFillerThatMeetsRequirement(o => o is Summon)) == 0) {
                    //consider this quest as failed if all logs of this character regarding being killed by a monster  has been deleted.
                    TutorialManager.Instance.FailTutorialQuest(this); 
                }
            }
        }
        #endregion
        
        #region Steps
        protected override void ConstructSteps() {
            steps = new List<QuestStepCollection>() {
                new QuestStepCollection(
                    new ClickOnCharacterStep($"Find the dead {UtilityScripts.Utilities.VillagerIcon()}Villager", IsCharacterValid)
                        .SetObjectsToCenter(_targetCharacter),
                    new ToggleTurnedOnStep("CharacterInfo_Logs", "Click on Log tab", () => UIManager.Instance.GetCurrentlySelectedPOI() == _targetCharacter),
                    new LogHistoryItemClicked("Find its killer", IsClickedLogObjectValid)
                        .SetHoverOverAction(OnHoverFindKiller)
                        .SetHoverOutAction(UIManager.Instance.HideSmallInfo)
                )
            };
        }
        #endregion

        #region Step Helpers
        private bool IsCharacterValid(Character character) {
            return character == _targetCharacter;
        }
        private bool IsClickedLogObjectValid(object obj, Log log, IPointOfInterest owner) {
            if (owner == _targetCharacter && obj is Summon && log.key.Equals("death_attacked")) {
                return true;
            }
            return false;
        }
        private void OnHoverFindKiller(QuestStepItem item) {
            UIManager.Instance.ShowSmallInfo(
                $"Check out the {UtilityScripts.Utilities.VillagerIcon()}Villager's Log to see which " +
                $"{UtilityScripts.Utilities.MonsterIcon()}Monster killed it and then {UtilityScripts.Utilities.ColorizeAction("click on its name")} to find it.",
                TutorialManager.Instance.killedByMonsterLog, "Browsing Logs", item.hoverPosition
            );
        }
        #endregion
    }
}