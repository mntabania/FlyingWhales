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
        private void SetTargetCharacter(QuestCriteria criteria) {
            if (criteria is CharacterDied metCriteria) {
                _targetCharacter = metCriteria.character;
            }
        }
        #endregion

        #region Availability
        protected override void MakeAvailable() {
            base.MakeAvailable();
            PlayerUI.Instance.ShowGeneralConfirmation("Killed by a Monster", 
                $"A Villager has been {UtilityScripts.Utilities.ColorizeAction("killed by a monster")}! " +
                "A Tutorial Quest has been added to teach you how to figure out what happened.", 
                onClickOK: () => TutorialManager.Instance.ShowTutorial(this)
            );
        }
        #endregion

        #region Activation
        public override void Activate() {
            base.Activate();
            Messenger.AddListener<Log>(UISignals.LOG_REMOVED_FROM_DATABASE, OnLogRemoved);
            Messenger.AddListener<Character>(CharacterSignals.CHARACTER_MARKER_DESTROYED, OnCharacterMarkerDestroyed);
        }
        public override void Deactivate() {
            base.Deactivate();
            Messenger.RemoveListener<Log>(UISignals.LOG_REMOVED_FROM_DATABASE, OnLogRemoved);
            Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_MARKER_DESTROYED, OnCharacterMarkerDestroyed);
            _targetCharacter = null;
        }
        
        #endregion
        
        #region Steps
        protected override void ConstructSteps() {
            steps = new List<QuestStepCollection>() {
                new QuestStepCollection(
                    new ClickOnCharacterStep($"Find the dead Villager", IsCharacterValid)
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
        private bool IsClickedLogObjectValid(object obj, string log, IPointOfInterest owner) {
            if (owner == _targetCharacter && obj is Summon && log.Contains("killed")) {
                return true;
            }
            return false;
        }
        private void OnHoverFindKiller(QuestStepItem item) {
            UIManager.Instance.ShowSmallInfo(
                $"Check out the Villager's Log to see which " +
                $"Monster killed it and then {UtilityScripts.Utilities.ColorizeAction("click on its name")} to find it.",
                TutorialManager.Instance.killedByMonsterLog, "Browsing Logs", item.hoverPosition
            );
        }
        #endregion

        #region Failure
        private void OnCharacterMarkerDestroyed(Character character) {
            if (character == _targetCharacter) {
                FailQuest();
            }
        }
        private void OnLogRemoved(Log log) {
            if (log.IsInvolved(_targetCharacter) && log.key.Equals("death_attacked")) {
                //check if target character still has any logs about being killed by a monster
                TutorialManager.Instance.FailTutorialQuest(this); 
            }
        }
        protected override void FailQuest() {
            base.FailQuest();
            //respawn this Quest.
            TutorialManager.Instance.InstantiateTutorial(TutorialManager.Tutorial.Killed_By_Monster);
        }
        #endregion
    }
}