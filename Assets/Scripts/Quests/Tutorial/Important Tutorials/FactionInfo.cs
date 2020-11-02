using System.Collections.Generic;
using Quests;
using Quests.Steps;
namespace Tutorial {
    public class FactionInfo : BonusTutorial {
        public FactionInfo() : base("Faction Info", TutorialManager.Tutorial.Faction_Info) { }
        protected override void ConstructCriteria() {
            _activationCriteria = new List<QuestCriteria>() {
                new HasFinishedImportantTutorials(), 
                new IsAtTime(new []{ 
                    GameManager.Instance.GetTicksBasedOnHour(5), 
                    GameManager.Instance.GetTicksBasedOnHour(15),
                    GameManager.Instance.GetTicksBasedOnHour(21)
                }),
            };
        }
        protected override void ConstructSteps() {
            steps = new List<QuestStepCollection>() {
                new QuestStepCollection(
                    new ClickOnCharacterStep($"Click on a {UtilityScripts.Utilities.VillagerIcon()}Villager", validityChecker: character => character.isNormalCharacter),
                    new ToggleTurnedOnStep("CharacterInfo_Info", "Open its Info tab")
                        .SetOnTopmostActions(OnTopMostInfo, OnNoLongerTopMostInfo),
                    new EventLabelLinkClicked("FactionLbl", "Click on its Faction")
                        .SetHoverOverAction(OnHoverOverClickFaction)
                        .SetHoverOutAction(UIManager.Instance.HideSmallInfo)
                        .SetCompleteAction(OnCompleteClickFaction)
                ),
                // new QuestStepCollection(
                //     new ToggleTurnedOnStep("Faction Overview", "Open its Overview tab")
                //         .SetCompleteAction(OnClickOverview)
                //         .SetOnTopmostActions(OnTopMostFactionInfo, OnNoLongerTopMostFactionInfo),
                //     new ToggleTurnedOnStep("Faction Owned Locations", "Open its Locations tab")
                //         .SetCompleteAction(OnClickLocations)
                //         .SetOnTopmostActions(OnTopMostLocations, OnNoLongerTopMostLocations),
                //     new ToggleTurnedOnStep("Faction Relations", "Open its Relations tab")
                //         .SetCompleteAction(OnClickRelations)
                //         .SetOnTopmostActions(OnTopMostRelations, OnNoLongerTopMostRelations),
                //     new ToggleTurnedOnStep("Faction Crimes", "Open its Crimes tab")
                //         .SetCompleteAction(OnClickCrimes)
                //         .SetOnTopmostActions(OnTopMostCrimes, OnNoLongerTopMostCrimes),
                //     new ToggleTurnedOnStep("Faction Logs", "Open its Logs tab")
                //         .SetCompleteAction(OnClickLogs)
                //         .SetOnTopmostActions(OnTopMostLogs, OnNoLongerTopMostLogs)
                // )
            };
        }
        protected override void MakeAvailable() {
            base.MakeAvailable();
            TutorialManager.Instance.ActivateTutorial(this);
        }

        #region Step Helpers
        private void OnCompleteClickFaction() {
            PlayerUI.Instance.ShowGeneralConfirmation("Factions",
                "A Faction is a group of characters that belong together. It typically has a single Faction Leader, several sets of ideologies, Villager members and claimed territories. " +
                "You can browse other details regarding the Faction including what they consider as Criminal Acts as well as their claimed territories.");
        }
        private void OnClickOverview() {
            PlayerUI.Instance.ShowGeneralConfirmation("Overview Tab",
                $"The Overview tab provides you with basic information about the Faction such as its " +
                $"{UtilityScripts.Utilities.ColorizeAction("Name, Banner, Faction Leader, Ideologies and Relations")}.");
        }
        private void OnClickLocations() {
            PlayerUI.Instance.ShowGeneralConfirmation("Locations Tab",
                "The Locations tab shows a list of all territories belonging to the Faction.");
        }
        private void OnClickRelations() {
            PlayerUI.Instance.ShowGeneralConfirmation("Relations Tab",
                "The Relations tab shows the Faction's relationship with other Factions.");
        }
        private void OnClickCrimes() {
            PlayerUI.Instance.ShowGeneralConfirmation("Crimes Tab",
                "The Crimes tab shows a list of all the things that are considered a Crime in the Faction as well as their Severity.");
        }
        private void OnClickLogs() {
            PlayerUI.Instance.ShowGeneralConfirmation("Logs Tab",
                $"The Logs tab provides you with a timestamped list of Faction-related events.");
        }
        private void OnHoverOverClickFaction(QuestStepItem stepItem) {
            UIManager.Instance.ShowSmallInfo("You can see which faction a character belongs to in its info tab.", 
                TutorialManager.Instance.factionInfo, "Character Faction", stepItem.hoverPosition);
        }
        #endregion
        
        #region Info Tab
        private void OnTopMostInfo() {
            Messenger.Broadcast(Signals.SHOW_SELECTABLE_GLOW, "CharacterInfo_Info");
        }
        private void OnNoLongerTopMostInfo() {
            Messenger.Broadcast(Signals.HIDE_SELECTABLE_GLOW, "CharacterInfo_Info");
        }
        #endregion
        
        #region Faction Info Tab
        private void OnTopMostFactionInfo() {
            Messenger.Broadcast(Signals.SHOW_SELECTABLE_GLOW, "Faction Overview");
        }
        private void OnNoLongerTopMostFactionInfo() {
            Messenger.Broadcast(Signals.HIDE_SELECTABLE_GLOW, "Faction Overview");
        }
        #endregion
        
        #region Mood Tab
        private void OnTopMostCharacters() {
            Messenger.Broadcast(Signals.SHOW_SELECTABLE_GLOW, "Faction Characters");
        }
        private void OnNoLongerTopMostCharacters() {
            Messenger.Broadcast(Signals.HIDE_SELECTABLE_GLOW, "Faction Characters");
        }
        #endregion
        
        #region Locations Tab
        private void OnTopMostLocations() {
            Messenger.Broadcast(Signals.SHOW_SELECTABLE_GLOW, "Faction Owned Locations");
        }
        private void OnNoLongerTopMostLocations() {
            Messenger.Broadcast(Signals.HIDE_SELECTABLE_GLOW, "Faction Owned Locations");
        }
        #endregion
        
        #region Relations Tab
        private void OnTopMostRelations() {
            Messenger.Broadcast(Signals.SHOW_SELECTABLE_GLOW, "Faction Relations");
        }
        private void OnNoLongerTopMostRelations() {
            Messenger.Broadcast(Signals.HIDE_SELECTABLE_GLOW, "Faction Relations");
        }
        #endregion
        
        #region Crimes Tab
        private void OnTopMostCrimes() {
            Messenger.Broadcast(Signals.SHOW_SELECTABLE_GLOW, "Faction Crimes");
        }
        private void OnNoLongerTopMostCrimes() {
            Messenger.Broadcast(Signals.HIDE_SELECTABLE_GLOW, "Faction Crimes");
        }
        #endregion
        
        #region Logs Tab
        private void OnTopMostLogs() {
            Messenger.Broadcast(Signals.SHOW_SELECTABLE_GLOW, "Faction Logs");
        }
        private void OnNoLongerTopMostLogs() {
            Messenger.Broadcast(Signals.HIDE_SELECTABLE_GLOW, "Faction Logs");
        }
        #endregion
    }
}