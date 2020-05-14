using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Quests;
using Quests.Steps;

namespace Tutorial {
    public class TortureChambers : TutorialQuest {

        public TortureChambers() : base("Torture Chambers", TutorialManager.Tutorial.Torture_Chambers) { }

        #region Criteria
        protected override void ConstructCriteria() {
            _activationCriteria = new List<QuestCriteria>() {
                new HasCompletedTutorialQuest(TutorialManager.Tutorial.Basic_Controls)
            };
            Messenger.AddListener<LocationStructure>(Signals.STRUCTURE_OBJECT_PLACED, OnAlreadyBuiltStructure);
        }
        protected override bool HasMetAllCriteria() {
            bool hasMetAllCriteria = base.HasMetAllCriteria();
            if (hasMetAllCriteria) {
                return PlayerSkillManager.Instance.GetDemonicStructureSkillData(SPELL_TYPE.TORTURE_CHAMBER).charges > 0;
            }
            return false;
        }
        #endregion
      
        protected override void ConstructSteps() {
            steps = new List<QuestStepCollection>() {
                new QuestStepCollection(
                    new ClickOnEmptyAreaStep(), 
                    new ObjectPickerShownStep("Click on Build Structure button", "Demonic Structure")
                        .SetHoverOverAction(OnHoverBuildStructure)
                        .SetHoverOutAction(UIManager.Instance.HideSmallInfo), 
                    new StructureBuiltStep(STRUCTURE_TYPE.TORTURE_CHAMBERS, "Choose the Torture Chamber")
                        .SetCompleteAction(OnStructureBuilt)),
                new QuestStepCollection(
                    new DropCharacterAtStructureRoomStep<TortureRoom>("Seize a Villager.")
                        .SetHoverOverAction(OnHoverSeizeCharacter)
                        .SetHoverOutAction(UIManager.Instance.HideSmallInfo),
                    new ClickOnRoomStep("Click on that chamber", IsClickedRoomValid),
                    new ExecutedPlayerActionStep(SPELL_TYPE.TORTURE, "Click on Torture button")
                        .SetHoverOverAction(OnHoverBeginTorture)
                        .SetHoverOutAction(UIManager.Instance.HideSmallInfo)
                )
            };
        }
        public override void Activate() {
            base.Activate();
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
            return room is TortureRoom tortureRoom && tortureRoom.HasValidTortureTarget();
        }
        private void OnHoverBuildStructure(QuestStepItem stepItem) {
            UIManager.Instance.ShowSmallInfo("The Build Structure button can be found beside the current Area nameplate.", 
                TutorialManager.Instance.buildStructureButton, "Building Structures", stepItem.hoverPosition);
        }
        private void OnHoverSeizeCharacter(QuestStepItem stepItem) {
            UIManager.Instance.ShowSmallInfo("The Torture Chambers have 3 separate rooms. Each one can be occupied by one Villager. Drop a seized Villager at one of the chambers to proceed.", 
                TutorialManager.Instance.chambersVideo, "Chambers", stepItem.hoverPosition);
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
                "For example, the Torture Chambers allow you to torture Villagers to afflict them with negative Traits and Statuses.",
                TutorialManager.Instance.demonicStructureVideoClip);
        }
        #endregion
    }
}