using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TheEyeUI : MonoBehaviour {

    //private TheEye theEye;

    #region General
    private Minion minionToInterfere; 
    public void OnClickInterfere(BaseLandmark landmark) {
        //theEye = landmark as TheEye;
        minionToInterfere = null;
        UIManager.Instance.dualObjectPicker.ShowDualObjectPicker<Character>(PlayerManager.Instance.player.minions.Select(x => x.character).ToList(), "Choose Minion",
            CanChooseMinion, OnHoverEnterMinion, OnHoverExitMinion, OnPickFirstObject, ConfirmInterfere, "Interfere");
    }
    private void OnPickFirstObject(object obj) {
        // List<Region> activeEventRegions = new List<Region>();
        // for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
        //     Region currRegion = GridMap.Instance.allRegions[i];
        //     if (currRegion.activeEvent != null) {
        //         activeEventRegions.Add(currRegion);
        //     }
        // }
        // minionToInterfere = (obj as Character).minion;
        // UIManager.Instance.dualObjectPicker.PopulateColumn(activeEventRegions, CanInterfere, null, null, UIManager.Instance.dualObjectPicker.column2ScrollView, UIManager.Instance.dualObjectPicker.column2ToggleGroup, "Choose Event", "WorldEvent");
        List<Character> targets = new List<Character>();
        for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
            Region currRegion = GridMap.Instance.allRegions[i];
            if (currRegion.locationType != LOCATION_TYPE.SETTLEMENT) {
                for (int j = 0; j < currRegion.charactersAtLocation.Count; j++) {
                    Character character = currRegion.charactersAtLocation[j];
                    if (character.currentActionNode?.poiTarget is TileObject) {
                        TileObject target = character.currentActionNode.poiTarget as TileObject;
                        if (target.tileObjectType == TILE_OBJECT_TYPE.REGION_TILE_OBJECT) {
                            targets.Add(character); // character is currently targeting a region, add to choices
                        }
                    }
                }    
            }
        }
        minionToInterfere = (obj as Character).minion;
        UIManager.Instance.dualObjectPicker.PopulateColumn(targets, CanInterfere, null, null, UIManager.Instance.dualObjectPicker.column2ScrollView, UIManager.Instance.dualObjectPicker.column2ToggleGroup, "Choose Event", "WorldEvent");
    }
    private bool CanInterfere(Character character) {
        return true;
    }
    private void OnHoverEnterMinion(Character character) {
        if (!CanChooseMinion(character)) {
            string message = string.Empty;
            if (character.minion.isAssigned) {
                message = $"{character.name} is already doing something else.";
            } else if (!character.minion.deadlySin.CanDoDeadlySinAction(DEADLY_SIN_ACTION.SABOTEUR) && !character.minion.deadlySin.CanDoDeadlySinAction(DEADLY_SIN_ACTION.FIGHTER)) {
                message = $"{character.name} does not have the required trait: Saboteur or Fighter";
            }
            UIManager.Instance.ShowSmallInfo(message);
        }
    }
    private void OnHoverExitMinion(Character character) {
        UIManager.Instance.HideSmallInfo();
    }
    private void ConfirmInterfere(object minionObj, object regionObj) {
        Character character = minionObj as Character;
        Character targetCharacter = regionObj as Character;

        (UIManager.Instance.regionInfoUI.activeRegion.mainLandmark as TheEye).StartInterference(targetCharacter, character);
    }
    #endregion

    private bool CanChooseMinion(Character character) {
        return !character.minion.isAssigned && (character.minion.deadlySin.CanDoDeadlySinAction(DEADLY_SIN_ACTION.SABOTEUR) || character.minion.deadlySin.CanDoDeadlySinAction(DEADLY_SIN_ACTION.FIGHTER));
    }

}