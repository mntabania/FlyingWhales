﻿using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Quests;
using Quests.Steps;

namespace Tutorial {
    public class Prison : BonusTutorial {

        public Prison() : base("Prison", TutorialManager.Tutorial.Prison) { }

        #region Criteria
        protected override void ConstructCriteria() {
            _activationCriteria = new List<QuestCriteria>() {
                new StructureBuiltCriteria(STRUCTURE_TYPE.TORTURE_CHAMBERS)
            };
        }
        protected override bool HasMetAllCriteria() {
            bool hasMetCriteria = base.HasMetAllCriteria();
            if (hasMetCriteria) {
                return PlayerSkillManager.Instance.GetPlayerSpellData(SPELL_TYPE.SEIZE_CHARACTER).isInUse;
            }
            return false;
        }
        #endregion
      
        protected override void ConstructSteps() {
            steps = new List<QuestStepCollection>() {
                // new QuestStepCollection(
                //     new ToggleTurnedOnStep("Build Tab", "Open Build Menu")
                //         .SetOnTopmostActions(OnTopMostBuildTab, OnNoLongerTopMostBuildTab),
                //     new ToggleTurnedOnStep("Prison", "Choose Prison")
                //         .SetOnTopmostActions(OnTopMostChooseTorture, OnNoLongerTopMostChooseTorture),
                //     new StructureBuiltStep(STRUCTURE_TYPE.TORTURE_CHAMBERS, "Place on an unoccupied Area.")
                //         .SetCompleteAction(OnStructureBuilt)
                // ),
                new QuestStepCollection(
                    new ClickOnRoomStep("Click on a Cell", room => room is PrisonCell)
                        .SetHoverOverAction(OnHoverChamber)
                        .SetHoverOutAction(UIManager.Instance.HideSmallInfo)
                ),
                new QuestStepCollection(
                    new ExecutedPlayerActionStep(SPELL_TYPE.SEIZE_CHARACTER, $"Seize a {UtilityScripts.Utilities.VillagerIcon()}Villager")
                        .SetHoverOverAction(OnHoverSeizeCharacter)
                        .SetHoverOutAction(UIManager.Instance.HideSmallInfo)
                        .SetOnTopmostActions(OnTopMostSeizeVillager, OnNoLongerTopMostSeizeVillager),
                    new DropCharacterAtStructureRoomStep<PrisonCell>("Drop at a Cell"),
                new ClickOnRoomStep("Click on that Cell", IsClickedRoomValid),
                    new ExecutedPlayerActionStep(SPELL_TYPE.TORTURE, "Click on Begin Torture button")
                        .SetHoverOverAction(OnHoverBeginTorture)
                        .SetHoverOutAction(UIManager.Instance.HideSmallInfo)
                        .SetOnTopmostActions(OnTopMostTorture, OnNoLongerTopMostTorture)
                )
            };
        }
        protected override void MakeAvailable() {
            base.MakeAvailable();
            PlayerUI.Instance.ShowGeneralConfirmation("Prison", "You've just built a new Demonic Structure: The Prison! " +
                                                                "This Structure allows you to capture and torture Villagers." +
                                                                "A Tutorial Quest has been created to teach you how to use it.", 
                onClickOK: () => TutorialManager.Instance.ActivateTutorial(this));
        }
        public override void Activate() {
            base.Activate();
            Messenger.Broadcast(Signals.UPDATE_BUILD_LIST);
        }
        
        #region Step Helpers
        private bool IsClickedRoomValid(StructureRoom room) {
            return room is PrisonCell tortureRoom && tortureRoom.HasValidTortureTarget();
        }
        private void OnHoverClickEmptyTile(QuestStepItem stepItem) {
            UIManager.Instance.ShowSmallInfo("Suggestion: choose an empty area far away from the Village", 
                stepItem.hoverPosition, "Choosing where to Build");
        }
        private void OnHoverBuildStructure(QuestStepItem stepItem) {
            UIManager.Instance.ShowSmallInfo("The Build Structure button can be found beside the current Area nameplate.", 
                TutorialManager.Instance.buildStructureButton, "Building Structures", stepItem.hoverPosition);
        }
        private void OnHoverSeizeCharacter(QuestStepItem stepItem) {
            UIManager.Instance.ShowSmallInfo($"You can {UtilityScripts.Utilities.ColorizeAction("seize")} a villager and then {UtilityScripts.Utilities.ColorizeAction("drop")} it at any empty tile.", 
                TutorialManager.Instance.seizeImage, "Seize", stepItem.hoverPosition);
        }
        private void OnHoverChamber(QuestStepItem stepItem) {
            UIManager.Instance.ShowSmallInfo($"The Prison have {UtilityScripts.Utilities.ColorizeAction("3 separate cells")}. Each cell can be occupied by one Villager.", 
                TutorialManager.Instance.chambersVideo, "Prison Cells", stepItem.hoverPosition);
        }
        private void OnHoverBeginTorture(QuestStepItem stepItem) {
            UIManager.Instance.ShowSmallInfo("The Torture button can be found beside the current Room nameplate.", 
                TutorialManager.Instance.tortureButton, "Starting torture", stepItem.hoverPosition);
        }
        #endregion

        #region Step Completion Actions
        private void OnStructureBuilt() {
            UIManager.Instance.generalConfirmationWithVisual.ShowGeneralConfirmation("Demonic Structures",
                "These are unique demonic structures that you can build on unoccupied Areas. " +
                "Each structure type has a unique use that may aid you in your invasion. " +
                $"For example, the Torture Chambers allow you to torture \nVillagers to afflict them with negative Traits and Statuses.\n\n" +
                "You have limited Charges per demonic structure so protect them from attacks!",
                TutorialManager.Instance.demonicStructureVideoClip);
        }
        #endregion

        #region Choose Torture Chamber
        private void OnTopMostChooseTorture() {
            Messenger.Broadcast(Signals.SHOW_SELECTABLE_GLOW, "Prison");
        }
        private void OnNoLongerTopMostChooseTorture() {
            Messenger.Broadcast(Signals.HIDE_SELECTABLE_GLOW, "Prison");
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
        private void OnTopMostTorture() {
            Messenger.Broadcast(Signals.SHOW_SELECTABLE_GLOW, "Torture");
        }
        private void OnNoLongerTopMostTorture() {
            Messenger.Broadcast(Signals.HIDE_SELECTABLE_GLOW, "Torture");
        }
        #endregion
    }
}