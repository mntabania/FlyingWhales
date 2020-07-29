using System.Collections.Generic;
using Quests;
using Quests.Steps;
namespace Tutorial {
    public class FactionInfo : ImportantTutorial {
        public FactionInfo() : base("Faction Info", TutorialManager.Tutorial.Faction_Info) { }
        protected override void ConstructCriteria() {
            _activationCriteria = new List<QuestCriteria>() {
                new HasCompletedTutorialQuest(TutorialManager.Tutorial.Elemental_Interactions)
            };
        }
        protected override void ConstructSteps() {
            steps = new List<QuestStepCollection>() {
                new QuestStepCollection(
                    new ClickOnCharacterStep($"Click on a Villager", validityChecker: character => character.isNormalCharacter),
                    new ToggleTurnedOnStep("CharacterInfo_Info", "Open its Info tab")
                        .SetOnTopmostActions(OnTopMostInfo, OnNoLongerTopMostInfo),
                    new EventLabelLinkClicked("FactionLbl", "Click on its Faction")
                        .SetCompleteAction(OnCompleteExecuteSpell)
                ),
                new QuestStepCollection(
                    new ToggleTurnedOnStep("Faction Overview", "Open Faction Overview tab")
                        .SetCompleteAction(OnClickOverview)
                        .SetOnTopmostActions(OnTopMostFactionInfo, OnNoLongerTopMostFactionInfo),
                    new ToggleTurnedOnStep("Faction Characters", "Open Characters tab")
                        .SetCompleteAction(OnClickFactionCharacters)
                        .SetOnTopmostActions(OnTopMostCharacters, OnNoLongerTopMostCharacters),
                    new ToggleTurnedOnStep("Faction Owned Locations", "Open Locations tab")
                        .SetCompleteAction(OnClickLocations)
                        .SetOnTopmostActions(OnTopMostLocations, OnNoLongerTopMostLocations),
                    new ToggleTurnedOnStep("Faction Logs", "Open Logs tab")
                        .SetCompleteAction(OnClickLogs)
                        .SetOnTopmostActions(OnTopMostLogs, OnNoLongerTopMostLogs)
                )
            };
        }
        
        #region Step Helpers
        private void OnCompleteExecuteSpell() {
            UIManager.Instance.generalConfirmationWithVisual.ShowGeneralConfirmation("Factions",
                "A Faction is a group of characters that belong together. It typically has a single Faction Leader, several sets of ideologies a", 
                TutorialManager.Instance.spellsVideoClip);
        }
        private void OnClickOverview() {
            UIManager.Instance.generalConfirmationWithVisual.ShowGeneralConfirmation("Overview Tab",
                $"The Info tab provides you with basic information about the \nVillager such as its " +
                $"{UtilityScripts.Utilities.ColorizeAction("Combat Stats, Affiliations, temporary Statuses, permanent Traits and Items held")}.",
                TutorialManager.Instance.infoTab);
        }
        private void OnClickFactionCharacters() {
            UIManager.Instance.generalConfirmationWithVisual.ShowGeneralConfirmation("Faction Characters Tab",
                $"The Mood tab provides you with an overview of the Villager's current state of mind. " +
                $"A Villager's Mood is primarily affected by {UtilityScripts.Utilities.ColorizeAction("Statuses")}. " +
                $"The lower a Villager's Mood is, the less cooperative it is with others, and may even eventually run amok!" +
                $"\n\nA Villager also has {UtilityScripts.Utilities.ColorizeAction("several Needs")} that apply various " +
                $"Statuses depending on how high or low they are.",
                TutorialManager.Instance.moodTab);
        }
        private void OnClickLocations() {
            UIManager.Instance.generalConfirmationWithVisual.ShowGeneralConfirmation("Owned Locations Tab",
                $"The Relations tab shows a Villager's relationship with its neighbors. " +
                $"A Villager will {UtilityScripts.Utilities.ColorizeAction("not cooperate")} with its enemies, " +
                "so one subtle way of reducing a Village's power is by having its residents dislike each other.",
                TutorialManager.Instance.relationsTab);
        }
        private void OnClickLogs() {
            UIManager.Instance.generalConfirmationWithVisual.ShowGeneralConfirmation("Logs Tab",
                $"The Log tab provides you with a timestamped list of what a \nVillager has done.",
                TutorialManager.Instance.logsTab);
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
        
        #region Relations Tab
        private void OnTopMostLocations() {
            Messenger.Broadcast(Signals.SHOW_SELECTABLE_GLOW, "Faction Owned Locations");
        }
        private void OnNoLongerTopMostLocations() {
            Messenger.Broadcast(Signals.HIDE_SELECTABLE_GLOW, "Faction Owned Locations");
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