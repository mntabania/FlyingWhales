using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Quests;
using Quests.Steps;

namespace Tutorial {
    public class Prison : ImportantTutorial {

        public Prison() : base("Prison", TutorialManager.Tutorial.Prison) { }

        #region Criteria
        protected override void ConstructCriteria() {
            _activationCriteria = new List<QuestCriteria>() {
                new HasCompletedTutorialQuest(TutorialManager.Tutorial.Spawn_An_Invader)
            };
            Messenger.AddListener<LocationStructure>(Signals.STRUCTURE_OBJECT_PLACED, OnAlreadyBuiltStructure);
        }
        protected override bool HasMetAllCriteria() {
            bool hasMetAllCriteria = base.HasMetAllCriteria();
            if (hasMetAllCriteria) {
                return PlayerSkillManager.Instance.GetDemonicStructureSkillData(SPELL_TYPE.TORTURE_CHAMBERS).charges > 0;
            }
            return false;
        }
        #endregion
      
        protected override void ConstructSteps() {
            steps = new List<QuestStepCollection>() {
                new QuestStepCollection(
                    new ToggleTurnedOnStep("Build Tab", "Open Build Menu")
                        .SetOnTopmostActions(OnTopMostBuildTab, OnNoLongerTopMostBuildTab),
                    new ToggleTurnedOnStep("Prison", "Choose Prison")
                        .SetOnTopmostActions(OnTopMostChooseTorture, OnNoLongerTopMostChooseTorture),
                    new StructureBuiltStep(STRUCTURE_TYPE.TORTURE_CHAMBERS, "Place on an unoccupied Area.")
                        .SetCompleteAction(OnStructureBuilt)
                ),
                new QuestStepCollection(
                    new ClickOnRoomStep("Click on a Cell", room => room is PrisonCell)
                        .SetHoverOverAction(OnHoverChamber)
                        .SetHoverOutAction(UIManager.Instance.HideSmallInfo)
                ),
                new QuestStepCollection(
                    new ExecutedPlayerActionStep(SPELL_TYPE.SEIZE_CHARACTER, $"Seize a Villager.")
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
        public override void Activate() {
            base.Activate();
            Messenger.Broadcast(Signals.UPDATE_BUILD_LIST);
            //stop listening for structure building, since another listener will be used to listen for step completion
            Messenger.RemoveListener<LocationStructure>(Signals.STRUCTURE_OBJECT_PLACED, OnAlreadyBuiltStructure);
        }
        public override void Deactivate() {
            base.Deactivate();
            //remove listener, this is for when the tutorial is completed without it being activated 
            Messenger.RemoveListener<LocationStructure>(Signals.STRUCTURE_OBJECT_PLACED, OnAlreadyBuiltStructure);
        }

        #region Availability Listeners
        private void OnAlreadyBuiltStructure(LocationStructure structure) {
            if (structure is Inner_Maps.Location_Structures.TortureChambers) {
                CompleteQuest(); //player already built a structure
            }
        }
        #endregion

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