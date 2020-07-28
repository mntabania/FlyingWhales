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
                )
            };
        }
        
        #region Step Helpers
        private void OnCompleteExecuteSpell() {
            UIManager.Instance.generalConfirmationWithVisual.ShowGeneralConfirmation("Factions",
                "A Faction is a group of characters that belong together. It typically has a single Faction Leader, several sets of ideologies a", 
                TutorialManager.Instance.spellsVideoClip);
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
        
    }
}