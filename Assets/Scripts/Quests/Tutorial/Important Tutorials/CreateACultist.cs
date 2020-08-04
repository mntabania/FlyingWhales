using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Quests;
using Quests.Steps;

namespace Tutorial {
    public class CreateACultist : BonusTutorial {

        public CreateACultist() : base("Create A Cultist", TutorialManager.Tutorial.Create_A_Cultist) { }

        #region Criteria
        protected override void ConstructCriteria() {
            _activationCriteria = new List<QuestCriteria>() {
                new StructureBuiltCriteria(STRUCTURE_TYPE.DEFILER)
            };
        }
        #endregion
      
        protected override void ConstructSteps() {
            steps = new List<QuestStepCollection>() {
                // new QuestStepCollection(
                //     new ToggleTurnedOnStep("Build Tab", "Open Build Menu")
                //         .SetOnTopmostActions(OnTopMostBuildTab, OnNoLongerTopMostBuildTab),
                //     new ToggleTurnedOnStep("Defiler", "Choose the Defiler")
                //         .SetOnTopmostActions(OnTopMostChooseDefiler, OnNoLongerTopMostChooseDefiler),
                //     new StructureBuiltStep(STRUCTURE_TYPE.DEFILER, "Place on an unoccupied Area.")
                // ),
                new QuestStepCollection(
                    new ClickOnRoomStep("Click on the Chamber", room => room is DefilerRoom)
                        .SetHoverOverAction(OnHoverChamber)
                        .SetHoverOutAction(UIManager.Instance.HideSmallInfo)
                ),
                new QuestStepCollection(
                    new ExecutedPlayerActionStep(SPELL_TYPE.SEIZE_CHARACTER, $"Seize a Villager.")
                        .SetHoverOverAction(OnHoverSeizeCharacter)
                        .SetHoverOutAction(UIManager.Instance.HideSmallInfo)
                        .SetOnTopmostActions(OnTopMostSeizeVillager, OnNoLongerTopMostSeizeVillager),
                    new DropCharacterAtStructureRoomStep<DefilerRoom>("Drop at the Chamber"),
                    new ClickOnRoomStep("Click on the Chamber", IsClickedRoomValid),
                    new ExecutedPlayerActionStep(SPELL_TYPE.BRAINWASH, "Click on Brainwash button")
                        .SetHoverOverAction(OnHoverBrainwash)
                        .SetHoverOutAction(UIManager.Instance.HideSmallInfo)
                        .SetOnTopmostActions(OnTopMostBrainwash, OnNoLongerTopMostBrainwash)
                        .SetCompleteAction(OnCompleteBrainwash)
                )
            };
        }
        protected override void MakeAvailable() {
            base.MakeAvailable();
            PlayerUI.Instance.ShowGeneralConfirmation("Defiler", "You've just built a new Demonic Structure: The Defiler! " +
                                                                 "This Structure allows the player to brainwash Villagers into Cultists." +
                                                                 "A Tutorial Quest has been created to teach you how to use it.", 
                onClickOK: () => TutorialManager.Instance.ActivateTutorial(this));
        }
        public override void Activate() {
            base.Activate();
            Messenger.Broadcast(Signals.UPDATE_BUILD_LIST);
        }

        #region Step Helpers
        private bool IsClickedRoomValid(StructureRoom room) {
            return room is DefilerRoom defilerRoom && defilerRoom.HasValidBrainwashTarget();
        }
        private void OnHoverSeizeCharacter(QuestStepItem stepItem) {
            UIManager.Instance.ShowSmallInfo($"You can {UtilityScripts.Utilities.ColorizeAction("seize")} a villager and then {UtilityScripts.Utilities.ColorizeAction("drop")} it at any empty tile.", 
                TutorialManager.Instance.seizeImage, "Seize", stepItem.hoverPosition);
        }
        private void OnHoverChamber(QuestStepItem stepItem) {
            UIManager.Instance.ShowSmallInfo(
                $"The Defiler has a central chamber where you can {UtilityScripts.Utilities.ColorizeAction("Brainwash")} " +
                $"a {UtilityScripts.Utilities.ColorizeAction("Villager")} and turn it into a {UtilityScripts.Utilities.ColorizeAction("Cultist")}. " +
                $"Low Mood and certain negative traits increase the success rate.", 
                TutorialManager.Instance.defilerChamberVideo, "Defiler Chamber", stepItem.hoverPosition);
        }
        private void OnHoverBrainwash(QuestStepItem stepItem) {
            UIManager.Instance.ShowSmallInfo("The Brainwash button can be found beside the current Room nameplate.", "Brainwash", stepItem.hoverPosition);
        }
        #endregion

        #region Step Completion Actions
        private void OnCompleteBrainwash() {
            PlayerUI.Instance.ShowGeneralConfirmation("Cultist",
                $"{UtilityScripts.Utilities.ColorizeAction("Cultists")} are secretly aligned with your cause. They won't be under your control for the most " +
                "part but they will perform special actions to assist you. You can also instruct them to place traps or " +
                $"poison other Villagers. You can also transform one into an {UtilityScripts.Utilities.ColorizeAction("Abomination")}.");
        }
        #endregion

        #region Choose Torture Chamber
        private void OnTopMostChooseDefiler() {
            Messenger.Broadcast(Signals.SHOW_SELECTABLE_GLOW, "Defiler");
        }
        private void OnNoLongerTopMostChooseDefiler() {
            Messenger.Broadcast(Signals.HIDE_SELECTABLE_GLOW, "Defiler");
        }
        #endregion

        #region Build Tab
        private void OnTopMostBuildTab() {
            Messenger.Broadcast(Signals.SHOW_SELECTABLE_GLOW, "Build Tab");
        }
        private void OnNoLongerTopMostBuildTab() {
            Messenger.Broadcast(Signals.HIDE_SELECTABLE_GLOW, "Build Tab");
        }
        #endregion
        
        #region Seize Villager
        private void OnTopMostSeizeVillager() {
            Messenger.Broadcast(Signals.SHOW_SELECTABLE_GLOW, "Seize Villager");
        }
        private void OnNoLongerTopMostSeizeVillager() {
            Messenger.Broadcast(Signals.HIDE_SELECTABLE_GLOW, "Seize Villager");
        }
        #endregion

        #region Torture Button
        private void OnTopMostBrainwash() {
            Messenger.Broadcast(Signals.SHOW_SELECTABLE_GLOW, "Brainwash");
        }
        private void OnNoLongerTopMostBrainwash() {
            Messenger.Broadcast(Signals.HIDE_SELECTABLE_GLOW, "Brainwash");
        }
        #endregion
    }
}