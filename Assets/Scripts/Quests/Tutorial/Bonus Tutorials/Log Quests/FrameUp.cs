﻿using System;
using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Quests;
using Quests.Steps;
using UnityEngine;
namespace Tutorial {
    public class FrameUp : LogQuest {

        private TileObject _droppedObject;
        private LocationStructure _droppedAtStructure;
        private Character _droppedObjectOwner; 
        
        public FrameUp() : base("Frame Up", TutorialManager.Tutorial.Frame_Up) { }

        #region Criteria
        protected override void ConstructCriteria() {
            _activationCriteria = new List<QuestCriteria>() {
                new HasFinishedImportantTutorials(),
                new IsAtTime(
                    new [] {
                        GameManager.Instance.GetTicksBasedOnHour(7),
                        GameManager.Instance.GetTicksBasedOnHour(13),
                        GameManager.Instance.GetTicksBasedOnHour(19)
                    }
                )
            };
        }
        protected override bool HasMetAllCriteria() {
            bool hasMetCriteria = base.HasMetAllCriteria();
            if (hasMetCriteria) {
                return PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.SEIZE_OBJECT).isInUse &&
                       PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.SEIZE_CHARACTER).isInUse;
            }
            return false;
        }
        #endregion

        #region Availability
        protected override void MakeAvailable() {
            base.MakeAvailable();
            TutorialManager.Instance.ShowTutorial(this);
        }
        #endregion

        #region Steps
        protected override void ConstructSteps() {
            steps = new List<QuestStepCollection>() {
                new QuestStepCollection(
                    new SeizePOIStep("Take someone's item", 
                            poi => poi is TileObject tileObject && tileObject.characterOwner != null 
                            && tileObject.characterOwner.isDead == false && tileObject.characterOwner.traitContainer.IsBlessed() == false)
                        .SetHoverOverAction(OnHoverOwnership)
                        .SetHoverOutAction(UIManager.Instance.HideSmallInfo),
                    new DropPOIAtStructureStep(IsDroppedAtStructureValid, poi => poi is TileObject tileObject && tileObject.characterOwner != null
                            && tileObject.characterOwner.isDead == false && tileObject.characterOwner.traitContainer.IsBlessed() == false,
                        "Drop at another one's house")
                        .SetHoverOverAction(OnHoverStructureOwnership)
                        .SetHoverOutAction(UIManager.Instance.HideSmallInfo)
                ),
                new QuestStepCollection(OnActivateSecondCollection, OnDeactivateSecondCollection,
                    new SeizePOIStep("Seize item owner", poi => _droppedObjectOwner == poi)
                        .SetObjectsToCenter(GetItemOwnerToCenter),
                    new DropPOIAtStructureStep(IsDroppedAtSameStructure, poi => _droppedObjectOwner == poi,
                            "Drop at the same house")
                        .SetObjectsToCenter(GetHouseToCenter),
                    new CharacterAssumedStep(poi => poi == _droppedObject && _droppedObject.gridTileLocation.structure == _droppedAtStructure, 
                            character => character == _droppedObjectOwner, 
                            "Wait for the character's reaction")
                        .SetCompleteAction(OnCompleteDropAtSameHouse)
                ),
                new QuestStepCollection(OnActivateThirdCollection, OnDeactivateThirdCollection,
                    new ClickOnCharacterStep("Click item owner", character => character == _droppedObjectOwner)
                        .SetObjectsToCenter(GetItemOwnerToCenter),
                    new ToggleTurnedOnStep("CharacterInfo_Logs", "Click on Log tab", () => UIManager.Instance.GetCurrentlySelectedPOI() == _droppedObjectOwner),
                    new LogHistoryItemClicked("Click assumed thief's name", IsClickedLogObjectValid)
                        .SetHoverOverAction(OnHoverAssumedThief)
                        .SetHoverOutAction(UIManager.Instance.HideSmallInfo)
                ),
            };
        }
        private bool IsDroppedAtStructureValid(LocationStructure structure, IPointOfInterest poi) {
            if (structure is Dwelling dwelling && dwelling.residents.Count > 0 
                && poi is TileObject tileObject && tileObject.characterOwner != null 
                && dwelling.residents.Contains(tileObject.characterOwner) == false) {
                _droppedObject = tileObject;
                _droppedAtStructure = structure;
                _droppedObjectOwner = tileObject.characterOwner;
                return true;
            }
            return false;
        }
        private bool IsDroppedAtSameStructure(LocationStructure structure, IPointOfInterest poi) {
            if (structure == _droppedAtStructure && poi == _droppedObjectOwner) {
                return true;
            }
            return false;
        }
        private void OnActivateSecondCollection() {
            Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
            Messenger.AddListener<TileObject, Character, LocationGridTile>(GridTileSignals.TILE_OBJECT_REMOVED, OnTileObjectRemoved);
        }
        private void OnDeactivateSecondCollection() {
            Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
            Messenger.RemoveListener<TileObject, Character, LocationGridTile>(GridTileSignals.TILE_OBJECT_REMOVED, OnTileObjectRemoved);
        }
        private void OnActivateThirdCollection() {
            Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
            Messenger.AddListener<TileObject, Character, LocationGridTile>(GridTileSignals.TILE_OBJECT_REMOVED, OnTileObjectRemoved);
        }
        private void OnDeactivateThirdCollection() {
            Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
            Messenger.RemoveListener<TileObject, Character, LocationGridTile>(GridTileSignals.TILE_OBJECT_REMOVED, OnTileObjectRemoved);
        }
        private void OnTileObjectRemoved(TileObject tileObject, Character removedBy, LocationGridTile removedFrom) {
            if (tileObject == _droppedObject && removedBy == null) {
                FailQuest(); //the tile object was removed from where it was, fail this quest
            }
        }
        private void OnCharacterDied(Character character) {
            if (_droppedObjectOwner == character) {
                FailQuest(); //the owner of the dropped object died, fail this quest.
            }
        }
        #endregion

        #region Step Helper
        private void OnHoverOwnership(QuestStepItem stepItem) {
            UIManager.Instance.ShowSmallInfo(
                $"Check an object's owner in its {UtilityScripts.Utilities.ColorizeAction("Info tab")}. " +
                $"Find an object whose owner is a Villager." +
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
            return new List<ISelectable>() { _droppedObjectOwner };
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
                $"Villagers are prone to making suspicious assumptions " +
                $"especially when they are in Bad or Critical Mood!\n\n" +
                $"{UtilityScripts.Utilities.ColorizeAction(_droppedObjectOwner.name)} just saw " +
                $"{UtilityScripts.Utilities.GetPronounString(_droppedObjectOwner.gender, PRONOUN_TYPE.POSSESSIVE, false)} " +
                $"{_droppedObject.name} in {UtilityScripts.Utilities.ColorizeAction(_droppedAtStructure.GetNameRelativeTo(_droppedObjectOwner))} " +
                $"and has now assumed that it has been {UtilityScripts.Utilities.ColorizeAction("stolen")}!"
            );
        }
        private bool IsClickedLogObjectValid(object obj, string log, IPointOfInterest owner) {
            if (owner == _droppedObjectOwner && obj is Character clickedCharacter 
                && clickedCharacter != _droppedObjectOwner
                && log.Contains("assumed")) {
                return true;
            }
            return false;
        }
        private void OnHoverAssumedThief(QuestStepItem stepItem) {
            Character owner = _droppedObjectOwner;
            UIManager.Instance.ShowSmallInfo(
                $"An {UtilityScripts.Utilities.ColorizeAction("assumption log")} should have been registered in " +
                $"{owner.name}'s Log tab. {UtilityScripts.Utilities.ColorizeAction("Click on the name")} of the " +
                $"Villager that he thought stole {UtilityScripts.Utilities.GetPronounString(owner.gender, PRONOUN_TYPE.POSSESSIVE, false)} {_droppedObject.name}.", 
                TutorialManager.Instance.assumedThief, "Assumption Logs",
                stepItem.hoverPosition
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