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
        protected override bool HasMetAllCriteria() {
            bool hasMetCriteria = base.HasMetAllCriteria();
            if (hasMetCriteria) {
                return PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.SEIZE_CHARACTER).isInUse;
            }
            return false;
        }
        #endregion
      
        protected override void ConstructSteps() {
            steps = new List<QuestStepCollection>() {
                new QuestStepCollection(
                    new ClickOnRoomStep("Click on the Chamber", room => room is PrisonCell)
                        .SetHoverOverAction(OnHoverChamber)
                        .SetHoverOutAction(UIManager.Instance.HideSmallInfo),
                    new PlayerActionContextMenuShown(target => target is Character character && character.isNormalCharacter, $"Right click on a Villager"),
                    new ExecutedPlayerActionStep(PLAYER_SKILL_TYPE.SEIZE_CHARACTER, $"Seize Brainwash Target")
                        .SetHoverOverAction(OnHoverSeizeCharacter)
                        .SetHoverOutAction(UIManager.Instance.HideSmallInfo)
                        .SetOnTopmostActions(OnTopMostSeizeVillager, OnNoLongerTopMostSeizeVillager),
                    new DropCharacterAtStructureRoomStep<PrisonCell>("Drop at the Chamber"),
                    new PlayerActionContextMenuShown(IsClickedTargetValid, $"Right click on Character or Chamber"),
                    new ExecutedPlayerActionStep(PLAYER_SKILL_TYPE.BRAINWASH, "Click on Brainwash option")
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
                                                                 "This Structure allows the player to brainwash Villagers into Cultists. " +
                                                                 "A Tutorial Quest has been created to teach you how to use it.", 
                onClickOK: () => TutorialManager.Instance.ActivateTutorial(this));
        }
        public override void Activate() {
            base.Activate();
            Messenger.Broadcast(UISignals.UPDATE_BUILD_LIST);
        }

        #region Step Helpers
        private bool IsClickedTargetValid(IPlayerActionTarget p_target) {
            if (p_target is Character character) {
                return character.gridTileLocation != null && character.gridTileLocation.structure.IsTilePartOfARoom(character.gridTileLocation, out var room) && room is PrisonCell;    
            } else if (p_target is PrisonCell defilerRoom) {
                return defilerRoom.HasValidBrainwashTarget();    
            }
            return false;
        }
        private void OnHoverSeizeCharacter(QuestStepItem stepItem) {
            UIManager.Instance.ShowSmallInfo(
                $"Look for a Villager with either Low or Critical Mood. Villagers who are Evil, " +
                $"Treacherous or have recently been Betrayed are also much easier to Brainwash. " +
                $"Faction Leaders and Settlement Rulers are especially difficult to successfully brainwash.", 
                stepItem.hoverPosition, "Good Cultist Candidates"
            );
        }
        private void OnHoverChamber(QuestStepItem stepItem) {
            UIManager.Instance.ShowSmallInfo(
                $"The Defiler has a central chamber where you can {UtilityScripts.Utilities.ColorizeAction("Brainwash")} " +
                $"a {UtilityScripts.Utilities.ColorizeAction("Villager")} and turn it into a {UtilityScripts.Utilities.ColorizeAction("Cultist")}. " +
                $"Low Mood and certain negative traits increase the success rate.", 
                TutorialManager.Instance.defilerChamberVideo, "Defiler Chamber", stepItem.hoverPosition);
        }
        private void OnHoverBrainwash(QuestStepItem stepItem) {
            UIManager.Instance.ShowSmallInfo("The Brainwash button can be found beside the current Room nameplate.", 
                stepItem.hoverPosition, "Brainwash");
        }
        #endregion

        #region Step Completion Actions
        private void OnCompleteBrainwash() {
            PlayerUI.Instance.ShowGeneralConfirmation("Cultist",
                $"{UtilityScripts.Utilities.ColorizeAction("Cultists")} are secretly aligned with your cause. They won't be under your control for the most " +
                "part but they will perform special actions to assist you. Depending on your Archetype, you may also instruct them to preach, spread rumor, place traps or snatch other Villagers. " +
                "A Cultist may eventually even become a Cult Leader!");
        }
        #endregion

        #region Choose Torture Chamber
        private void OnTopMostChooseDefiler() {
            Messenger.Broadcast(UISignals.SHOW_SELECTABLE_GLOW, "Defiler");
        }
        private void OnNoLongerTopMostChooseDefiler() {
            Messenger.Broadcast(UISignals.HIDE_SELECTABLE_GLOW, "Defiler");
        }
        #endregion

        #region Build Tab
        private void OnTopMostBuildTab() {
            Messenger.Broadcast(UISignals.SHOW_SELECTABLE_GLOW, "Build Tab");
        }
        private void OnNoLongerTopMostBuildTab() {
            Messenger.Broadcast(UISignals.HIDE_SELECTABLE_GLOW, "Build Tab");
        }
        #endregion
        
        #region Seize Villager
        private void OnTopMostSeizeVillager() {
            Messenger.Broadcast(UISignals.SHOW_SELECTABLE_GLOW, "Seize Villager");
        }
        private void OnNoLongerTopMostSeizeVillager() {
            Messenger.Broadcast(UISignals.HIDE_SELECTABLE_GLOW, "Seize Villager");
        }
        #endregion

        #region Torture Button
        private void OnTopMostBrainwash() {
            Messenger.Broadcast(UISignals.SHOW_SELECTABLE_GLOW, "Brainwash");
        }
        private void OnNoLongerTopMostBrainwash() {
            Messenger.Broadcast(UISignals.HIDE_SELECTABLE_GLOW, "Brainwash");
        }
        #endregion
    }
}