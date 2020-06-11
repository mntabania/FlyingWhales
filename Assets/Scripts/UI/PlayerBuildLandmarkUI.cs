using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using Locations.Features;

public class PlayerBuildLandmarkUI : MonoBehaviour {

    public Region targetRegion { get; private set; }

    #region General
    public void OnClickBuild(Region region) {
        targetRegion = region;
        List<string> landmarkNames = new List<string>();
        //for (int i = 0; i < PlayerManager.Instance.allLandmarksThatCanBeBuilt.Length; i++) {
        //    landmarkNames.Add(UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(PlayerManager.Instance.allLandmarksThatCanBeBuilt[i].ToString()));
        //}
        UIManager.Instance.dualObjectPicker.ShowDualObjectPicker(PlayerManager.Instance.player.minions.Select(x => x.character).ToList(), landmarkNames,
            "Choose a minion", "Choose a structure",
            CanChooseMinion, CanChooseLandmark,
            OnHoverEnterMinion, OnHoverLandmarkChoice,
            OnHoverExitMinion, OnHoverExitLandmarkChoice,
            StartBuild, "Build", column2Identifier: "Landmark");
    }
    private bool CanChooseMinion(Character character) {
        return !character.minion.isAssigned && character.minion.deadlySin.CanDoDeadlySinAction(DEADLY_SIN_ACTION.BUILDER);
    }
    private void OnHoverEnterMinion(Character character) {
        if (!CanChooseMinion(character)) {
            string message = string.Empty;
            if (character.minion.isAssigned) {
                message = $"{character.name} is already doing something else.";
            } else if (!character.minion.deadlySin.CanDoDeadlySinAction(DEADLY_SIN_ACTION.BUILDER)) {
                message = $"{character.name} does not have the required trait: Builder";
            }
            UIManager.Instance.ShowSmallInfo(message);
        }
    }
    private void OnHoverExitMinion(Character character) {
        UIManager.Instance.HideSmallInfo();
    }
    private bool CanChooseLandmark(string landmarkName) {
        if (landmarkName == "The Pit") {
            return false;
        }
        if(landmarkName == "The Kennel" && !UIManager.Instance.regionInfoUI.activeRegion.HasTileWithFeature(TileFeatureDB.Summons_Feature)) {
            return false;
        }
        if (landmarkName == "The Crypt" && (!UIManager.Instance.regionInfoUI.activeRegion.HasTileWithFeature(TileFeatureDB.Artifact_Feature) || PlayerManager.Instance.player.playerFaction.HasOwnedRegionWithLandmarkType(LANDMARK_TYPE.CRYPT))) {
            return false;
        }
        return true;
    }
    private void OnHoverLandmarkChoice(string landmarkName) {
        LandmarkData data = LandmarkManager.Instance.GetLandmarkData(landmarkName);
        string info = data.description;
        if (info != string.Empty) {
            info += "\n";
        }
        info += $"Duration: {GameManager.Instance.GetCeilingHoursBasedOnTicks(data.buildDuration).ToString()} hours";
        UIManager.Instance.ShowSmallInfo(info);
    }
    private void OnHoverExitLandmarkChoice(string landmarkName) {
        UIManager.Instance.HideSmallInfo();
    }
    private void StartBuild(object minionObj, object landmarkObj) {
        string landmarkTypeName = landmarkObj as string;
        Debug.Log($"Chose to build {landmarkTypeName}");
        // LandmarkData landmarkData = LandmarkManager.Instance.GetLandmarkData(landmarkObj as string);
        // targetRegion.StartBuildingStructure(landmarkData.landmarkType, (minionObj as Character).minion);
        // UIManager.Instance.regionInfoUI.UpdateInfo();
        // Messenger.Broadcast<Region>(Signals.REGION_INFO_UI_UPDATE_APPROPRIATE_CONTENT, targetRegion);
    }
    #endregion
}
