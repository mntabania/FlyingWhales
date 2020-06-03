using System.Collections;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Quests;
using Quests.Steps;
using UnityEngine;
namespace Tutorial {
    public class BasicControls : TutorialQuest {

        public BasicControls() : base("Basic Controls", TutorialManager.Tutorial.Basic_Controls) { }
        protected override void ConstructCriteria() {
            _activationCriteria = new List<QuestCriteria>() {
                new GameHasBeenRunningForSeconds(WorldConfigManager.Instance.isDemoWorld ? 1f : 8f)
            };
        }
        protected override void ConstructSteps() {
            QuestStep hoveredStep = new QuestStepHoveredStep()
                .SetHoverOverAction(OnHoverHoverThis)
                .SetHoverOutAction(UIManager.Instance.HideSmallInfo);
            QuestStep look = new LookAroundStep()
                .SetHoverOverAction(OnHoverLookAround)
                .SetHoverOutAction(UIManager.Instance.HideSmallInfo);
            QuestStep unpause = new UnpauseStep()
                .SetHoverOverAction(OnHoverUnpause)
                .SetHoverOutAction(UIManager.Instance.HideSmallInfo)
                .SetOnTopmostActions(OnTopMostUnpause, OnNopLongerTopMostUnpause);
            QuestStep objectClick = new ClickOnObjectStep();
            QuestStep characterClick = new ClickOnCharacterStep($"Click on a {UtilityScripts.Utilities.VillagerIcon()}Villager", character => character.isNormalCharacter);
            QuestStep structureClick = new ClickOnStructureStep();
            QuestStep hexTileClick = new ClickOnAreaStep().SetHoverOverAction(OnHoverClickArea)
                .SetHoverOutAction(UIManager.Instance.HideSmallInfo);
            if (WorldConfigManager.Instance.isDemoWorld == false) {
                hexTileClick.SetCompleteAction(OnCompleteClickArea);
            }

            steps = new List<QuestStepCollection>() {
                new QuestStepCollection(hoveredStep, look, unpause),
                new QuestStepCollection(objectClick, characterClick),
                new QuestStepCollection(structureClick, hexTileClick),
            };
        }

        #region Step Helpers
        private void OnHoverHoverThis(QuestStepItem stepItem) {
            UIManager.Instance.ShowSmallInfo(
                "Some steps in a checklist will have a different text color," +
                "\n- <#FFFB00>Text with this color provide more information when hovered</color>" +
                "\n- <#FFFFFF>Text with this color does not.</color>",
                stepItem.hoverPosition, "Checklist Help"
            );
        }
        private void OnHoverLookAround(QuestStepItem stepItem) {
            UIManager.Instance.ShowSmallInfo(
                 "You can move the camera around by doing any of the following: " +
                 "\n- Using the " +
                 "<sprite=\"Text_Sprites\" name=\"W\">" +
                 "<sprite=\"Text_Sprites\" name=\"A\">" +
                 "<sprite=\"Text_Sprites\" name=\"S\">" +
                 "<sprite=\"Text_Sprites\" name=\"D\"> keys " +
                 "\n- Using the Arrow Keys " +
                 "\n- Press and hold the middle mouse button and drag", 
                stepItem.hoverPosition,
                "Camera Movement"
            );
        }
        private void OnHoverClickArea(QuestStepItem stepItem) {
            UIManager.Instance.ShowSmallInfo("You can select an area by clicking empty spaces in the Interior Map.", 
                stepItem.hoverPosition,
                "Selecting Areas");
        }
        private void OnCompleteClickArea() {
            TutorialManager.Instance.StartCoroutine(DelayedAreaTooltip());
        }
        private IEnumerator DelayedAreaTooltip() {
            yield return new WaitForSeconds(1f);
            UIManager.Instance.generalConfirmationWithVisual.ShowGeneralConfirmation("Areas", 
                "Areas have representations in both the Interior Map and World Map. " +
                "An Area in the World Map is represented by a Hex Tile and an Area in the Interior Map is represented by a Square when selected.", 
                TutorialManager.Instance.areaVideoClip);
        }
        #endregion

        #region Unpause Step
        private void OnHoverUnpause(QuestStepItem stepItem) {
            UIManager.Instance.ShowSmallInfo("You can Pause/Unpause the game using the Space Bar. Alternatively, " +
                                             "you can also use the Time Control buttons on the right of your screen.", 
                TutorialManager.Instance.timeControlsVideoClip,
                "Controlling Time", stepItem.hoverPosition);
        }
        private void OnTopMostUnpause() {
            Messenger.Broadcast(Signals.SHOW_SELECTABLE_GLOW, "1x");
            Messenger.Broadcast(Signals.SHOW_SELECTABLE_GLOW, "2x");
            Messenger.Broadcast(Signals.SHOW_SELECTABLE_GLOW, "4x");
        }
        private void OnNopLongerTopMostUnpause() {
            Messenger.Broadcast(Signals.HIDE_SELECTABLE_GLOW, "1x");
            Messenger.Broadcast(Signals.HIDE_SELECTABLE_GLOW, "2x");
            Messenger.Broadcast(Signals.HIDE_SELECTABLE_GLOW, "4x");
        }
        #endregion
    }
}