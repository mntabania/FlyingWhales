﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class MinionCommandsUI : MonoBehaviour {
    public RectTransform rt;
    public IPointOfInterest targetPOI { get; private set; }

    public Button[] characterCommands;
    public Button[] monsterCommands;
    public Button[] artifactCommands;

    #region Utilities
    public void ShowUI(IPointOfInterest poi) {
        targetPOI = poi;
        if (ShowButtons(poi)) {
            UIManager.Instance.HideSmallInfo();
            UIManager.Instance.PositionTooltip(gameObject, rt, rt);
            gameObject.SetActive(true);
            Messenger.AddListener<KeyCode>(ControlsSignals.KEY_DOWN, OnKeyPressed);
        } else {
            targetPOI = null;
        }
    }
    public void HideUI() {
        gameObject.SetActive(false);
        this.targetPOI = null;
        Messenger.RemoveListener<KeyCode>(ControlsSignals.KEY_DOWN, OnKeyPressed);
    }
    private void OnKeyPressed(KeyCode keyCode) {
        if (keyCode == KeyCode.Mouse0 && UIManager.Instance.IsMouseOnUI() == false) {
            HideUI();
        } else if (keyCode == KeyCode.Escape) {
            HideUI();
        }
    }
    private bool ShowButtons(IPointOfInterest targetPOI) {
        if (targetPOI.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            if (targetPOI is Summon) {
                ShowCommands(false, true, false);
                return true;
            } else {
                Character target = targetPOI as Character;
                if (target.minion == null) {
                    ShowCommands(true, false, false);
                    return true;
                }
            }
        } else if (targetPOI.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
            if (targetPOI is Artifact) {
                ShowCommands(false, false, true);
                return true;
            }
        }
        return false;
    }
    private void ShowCommands(bool showCharacterCommands, bool showMonsterCommands, bool showArtifactCommands) {
        for (int i = 0; i < characterCommands.Length; i++) {
            characterCommands[i].gameObject.SetActive(showCharacterCommands);
            if (showCharacterCommands) {
                SetButtonInteractability(characterCommands[i]);
            }
        }
        for (int i = 0; i < monsterCommands.Length; i++) {
            monsterCommands[i].gameObject.SetActive(showMonsterCommands);
            if (showMonsterCommands) {
                SetButtonInteractability(monsterCommands[i]);
            }
        }
        for (int i = 0; i < artifactCommands.Length; i++) {
            artifactCommands[i].gameObject.SetActive(showArtifactCommands);
            if (showArtifactCommands) {
                SetButtonInteractability(artifactCommands[i]);
            }
        }
    }
    private void SetButtonInteractability(Button button) {
        Func<bool> isInteractable = null;
        if(button.gameObject.name == "Kill") {
            isInteractable = CanKill;
        } else if (button.gameObject.name == "Knockout") {
            isInteractable = CanKnockout;
        } else if (button.gameObject.name == "Abduct") {
            isInteractable = CanAbduct;
        } 
        //else if (button.gameObject.name == "Learn Monster") {
        //    isInteractable = CanLearnMonster;
        //} 
        //else if (button.gameObject.name == "Take Artifact") {
        //    isInteractable = CanTakeArtifact;
        //}
        button.interactable = isInteractable == null || isInteractable();
    }
    private bool CanIssueCommand() {
        bool canIssueCommand = true;
        Character actor = UIManager.Instance.characterInfoUI.activeCharacter;
        if(!actor.movementComponent.HasPathToEvenIfDiffRegion(targetPOI.gridTileLocation)) {
            canIssueCommand = false;
            PlayerUI.Instance.ShowGeneralConfirmation("Instruction Error", "Cannot instruct minion. It has no path towards the target.");
        }
        return canIssueCommand;
    }
    #endregion

    #region Character
    public void Knockout() {
        if (CanIssueCommand()) {
            if (targetPOI is Character) {
            Character actor = UIManager.Instance.characterInfoUI.activeCharacter;
                actor.jobComponent.CreateKnockoutJob(targetPOI as Character);
            } else {
                Debug.LogError($"{targetPOI.name} is not a character!");
            }
        }
        HideUI();
    }
    private bool CanKnockout() {
        return !targetPOI.traitContainer.HasTrait("Unconscious");
    }
    public void Kill() {
        if (CanIssueCommand()) {
            if (targetPOI is Character) {
            Character actor = UIManager.Instance.characterInfoUI.activeCharacter;
                actor.jobComponent.CreateDemonKillJob(targetPOI as Character);
            } else {
                Debug.LogError($"{targetPOI.name} is not a character!");
            }
        }
        HideUI();
    }
    private bool CanKill() {
        return !(targetPOI as Character).isDead;
    }
    public void Abduct() {
        if (CanIssueCommand()) {
            if (targetPOI is Character) {
                Character actor = UIManager.Instance.characterInfoUI.activeCharacter;
                actor.jobComponent.CreateAbductJob(targetPOI as Character);
            } else {
                Debug.LogError($"{targetPOI.name} is not a character!");
            }
        }
        HideUI();
    }
    private bool CanAbduct() {
        return !(targetPOI.traitContainer.HasTrait("Restrained") && targetPOI.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner == PlayerManager.Instance.player.portalTile);
    }
    #endregion

    //#region Monsters
    //public void LearnMonster() {
    //    if (CanIssueCommand()) {
    //        if (targetPOI is Character) {
    //            Character actor = UIManager.Instance.characterInfoUI.activeCharacter;
    //            actor.jobComponent.CreateLearnMonsterJob(targetPOI as Character);
    //        } else {
    //            Debug.LogError($"{targetPOI.name} is not a character!");
    //        }
    //    }
    //    HideUI();
    //}
    //private bool CanLearnMonster() {
    //    Character target = targetPOI as Character;
    //    return !PlayerManager.Instance.player.archetype.HasMonster(target.race, target.characterClass.className);
    //}
    //#endregion

    //#region Artifacts
    //public void TakeArtifact() {
    //    if (CanIssueCommand()) {
    //        if (targetPOI is Artifact) {
    //        Character actor = UIManager.Instance.characterInfoUI.activeCharacter;
    //            actor.jobComponent.CreateTakeArtifactJob(targetPOI as Artifact, PlayerManager.Instance.player.portalTile.locationGridTiles[0].structure);
    //        } else {
    //            Debug.LogError($"{targetPOI.name} is not an artifact!");
    //        }
    //    }
    //    HideUI();
    //}
    //private bool CanTakeArtifact() {
    //    return targetPOI.gridTileLocation != null;
    //}
    //#endregion
}
