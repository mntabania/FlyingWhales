using System;
using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Quests;
using Quests.Steps;
using UnityEngine;
namespace Tutorial {
    public class FrameUp : BonusTutorial {

        private TileObject _droppedObject;
        private LocationStructure _droppedAtStructure;
        
        public FrameUp() : base("Frame Up", TutorialManager.Tutorial.Frame_Up) { }

        #region Criteria
        protected override void ConstructCriteria() {
            _activationCriteria = new List<QuestCriteria>() {
                new HasCompletedTutorialQuest(TutorialManager.Tutorial.Elemental_Interactions)
            };
        }
        #endregion

        #region Availability
        protected override void MakeAvailable() {
            base.MakeAvailable();
            TutorialManager.Instance.ActivateTutorial(this);
        }
        #endregion
        
        #region Steps
        protected override void ConstructSteps() {
            steps = new List<QuestStepCollection>() {
                new QuestStepCollection(
                    new SeizePOIStep("Take someone's item", 
                            poi => poi is TileObject tileObject && tileObject.characterOwner != null 
                            && tileObject.characterOwner.isDead == false && tileObject.characterOwner.traitContainer.HasTrait("Blessed") == false)
                        .SetHoverOverAction(OnHoverOwnership)
                        .SetHoverOutAction(UIManager.Instance.HideSmallInfo),
                    new DropPOIAtStructureStep(IsDroppedAtStructureValid, poi => poi is TileObject tileObject && tileObject.characterOwner != null,
                        "Drop at another one's house")
                        .SetHoverOverAction(OnHoverStructureOwnership)
                        .SetHoverOutAction(UIManager.Instance.HideSmallInfo)
                ),
                new QuestStepCollection(OnActivateSecondCollection, OnDeactivateSecondCollection,
                    new SeizePOIStep("Seize item owner", poi => _droppedObject.characterOwner == poi)
                        .SetObjectsToCenter(GetItemOwnerToCenter),
                    new DropPOIAtStructureStep(IsDroppedAtSameStructure, poi => _droppedObject.characterOwner == poi,
                            "Drop at the same house")
                        .SetObjectsToCenter(GetHouseToCenter)
                        .SetCompleteAction(OnCompleteDropAtSameHouse)
                ),
            };
        }
        private bool IsDroppedAtStructureValid(LocationStructure structure, IPointOfInterest poi) {
            if (structure is Dwelling dwelling && dwelling.residents.Count > 0 
                && poi is TileObject tileObject && tileObject.characterOwner != null 
                && dwelling.residents.Contains(tileObject.characterOwner) == false) {
                _droppedObject = tileObject;
                _droppedAtStructure = structure;
                return true;
            }
            return false;
        }
        private bool IsDroppedAtSameStructure(LocationStructure structure, IPointOfInterest poi) {
            if (structure == _droppedAtStructure && poi == _droppedObject.characterOwner) {
                return true;
            }
            return false;
        }
        private void OnActivateSecondCollection() {
            Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
            Messenger.AddListener<TileObject, Character, LocationGridTile>(Signals.TILE_OBJECT_REMOVED, OnTileObjectRemoved);
        }
        private void OnDeactivateSecondCollection() {
            Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
            Messenger.RemoveListener<TileObject, Character, LocationGridTile>(Signals.TILE_OBJECT_REMOVED, OnTileObjectRemoved);
        }
        private void OnTileObjectRemoved(TileObject tileObject, Character removedBy, LocationGridTile removedFrom) {
            if (tileObject == _droppedObject) {
                FailQuest(); //the tile object was removed from where it was, fail this quest
            }
        }
        private void OnCharacterDied(Character character) {
            if (character == _droppedObject.characterOwner) {
                FailQuest(); //the owner of the dropped object died, fail this quest.
            }
        }
        #endregion

        #region Step Helper
        private void OnHoverOwnership(QuestStepItem stepItem) {
            UIManager.Instance.ShowSmallInfo(
                $"Check an object's owner in its {UtilityScripts.Utilities.ColorizeAction("Info tab")}. " +
                $"Find an object whose owner is a {UtilityScripts.Utilities.VillagerIcon()}Villager." +
                $"Make sure that the owner is {UtilityScripts.Utilities.ColorizeAction("still alive")} " +
                $"and does not have {UtilityScripts.Utilities.ColorizeAction("Blessed")} trait because we will be seizing him later.",
                TutorialManager.Instance.tileObjectOwner, "Ownership",
                stepItem.hoverPosition
            );
        }
        private void OnHoverStructureOwnership(QuestStepItem stepItem) {
            UIManager.Instance.ShowSmallInfo(
                $"Check a structure's residents in its {UtilityScripts.Utilities.ColorizeAction("Residents tab")}. " +
                "Drop the item at someone else's house.", TutorialManager.Instance.structureInfoResidents, "Structure Owner",
                stepItem.hoverPosition
            );
        }
        private List<ISelectable> GetItemOwnerToCenter() {
            return new List<ISelectable>() { _droppedObject.characterOwner };
        }
        private List<ISelectable> GetHouseToCenter() {
            return new List<ISelectable>() { _droppedAtStructure };
        }
        private void OnCompleteDropAtSameHouse() {
            TutorialManager.Instance.StartCoroutine(DelayedFrameUpPopup());
        }
        private IEnumerator DelayedFrameUpPopup() {
            yield return new WaitForSecondsRealtime(1.5f);
            PlayerUI.Instance.ShowGeneralConfirmation("Frame Up", 
                $"{UtilityScripts.Utilities.VillagerIcon()}Villagers are prone to making suspicious assumptions " +
                $"especially when they are in Bad or Critical Mood!\n\n" +
                $"{UtilityScripts.Utilities.ColorizeAction(_droppedObject.characterOwner.name)} just saw " +
                $"{UtilityScripts.Utilities.GetPronounString(_droppedObject.characterOwner.gender, PRONOUN_TYPE.POSSESSIVE, false)} " +
                $"{_droppedObject.name} in {UtilityScripts.Utilities.ColorizeAction(_droppedAtStructure.GetNameRelativeTo(_droppedObject.characterOwner))} " +
                $"and has now assumed that it has been {UtilityScripts.Utilities.ColorizeAction("stolen")}!"
            );
        }
        #endregion

        #region Failure
        protected override void FailQuest() {
            base.FailQuest();
            //respawn this Quest.
            TutorialManager.Instance.InstantiateTutorial(TutorialManager.Tutorial.Frame_Up);
        }
        #endregion
    }
}