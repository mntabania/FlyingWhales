using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using ECS;

public class HextileInfoUI : UIMenu {

    [Space(10)]
    [Header("Content")]
    [SerializeField] private TweenPosition tweenPos;
    [SerializeField] private TextMeshProUGUI hexTileInfoLbl;
    [SerializeField] private ScrollRect infoScrollView;

    internal HexTile currentlyShowingHexTile {
        get { return _data as HexTile; }
    }

    internal override void Initialize() {
        base.Initialize();
        Messenger.AddListener("UpdateUI", UpdateHexTileInfo);
        //tweenPos.AddOnFinished(() => UpdateHexTileInfo());
    }

    public override void OpenMenu() {
        base.OpenMenu();
        UpdateHexTileInfo();
    }

    public override void SetData(object data) {
        if (currentlyShowingHexTile != null) {
            currentlyShowingHexTile.clickHighlightGO.SetActive(false);
        }
        base.SetData(data); //replace this existing data
        currentlyShowingHexTile.clickHighlightGO.SetActive(true);
        if (isShowing) {
            UpdateHexTileInfo();
        }
    }

    public void UpdateHexTileInfo() {
        if(currentlyShowingHexTile == null) {
            return;
        }
        string text = string.Empty;
        text += "<b>Name:</b> " + currentlyShowingHexTile.tileName;
		text += "\n<b>Biome:</b> " + currentlyShowingHexTile.biomeType.ToString();
		text += "\n<b>Elevation:</b> " + currentlyShowingHexTile.elevationType.ToString ();

        text += "\n<b>Landmark Name:</b> ";
		if(currentlyShowingHexTile.landmarkOnTile != null){
			text += currentlyShowingHexTile.landmarkOnTile.landmarkName;
		}else{
			text += "NONE";
		}

		text += "\n<b>Base Landmark Type:</b> ";
		if(currentlyShowingHexTile.landmarkOnTile != null){
			text += currentlyShowingHexTile.landmarkOnTile.urlName;
		}else{
			text += "NONE";
		}

		//text += "\n<b>Road Type:</b> " + currentlyShowingHexTile.roadType.ToString ();;

		text += "\n<b>Characters: </b> ";
		if (currentlyShowingHexTile.charactersAtLocation.Count > 0) {
			for (int i = 0; i < currentlyShowingHexTile.charactersAtLocation.Count; i++) {
                Character currChar = currentlyShowingHexTile.charactersAtLocation[i];
                text += "\n" + currChar.urlName + " - " + (currChar.characterClass != null ? currChar.characterClass.className : "NONE") + "/" + (currChar.role != null ? currChar.role.roleType.ToString() : "NONE");
                if (currChar.actionData.currentAction != null) {
                    //if (currChar.currentTask.taskType == TASK_TYPE.QUEST) {
                    //	OldQuest.Quest currQuest = (OldQuest.Quest)currChar.currentTask;
                    //	text += " (" + currQuest.urlName + ")";
                    //} else {
                    text += " (" + currChar.actionData.currentAction.actionData.actionName.ToString() + ")";
                    //}
                }
    //            if (currentlyShowingHexTile.charactersAtLocation[i] is ECS.Character) {
				//	Character currChar = currentlyShowingHexTile.charactersAtLocation [i];
				//	text += "\n" + currChar.urlName + " - " + (currChar.characterClass != null ? currChar.characterClass.className : "NONE") + "/" + (currChar.role != null ? currChar.role.roleType.ToString () : "NONE");
				//	if (currChar.actionData.currentAction != null) {
				//		//if (currChar.currentTask.taskType == TASK_TYPE.QUEST) {
				//		//	OldQuest.Quest currQuest = (OldQuest.Quest)currChar.currentTask;
				//		//	text += " (" + currQuest.urlName + ")";
				//		//} else {
				//			text += " (" + currChar.actionData.currentAction.actionData.actionName.ToString () + ")";
				//		//}
				//	}
				//} else if (currentlyShowingHexTile.charactersAtLocation[i] is Party) {
				//	Party currParty = (Party)currentlyShowingHexTile.charactersAtLocation [i];
				//	text += "\n" + currParty.urlName + " - " + (currParty.currentAction != null ? currParty.currentAction.ToString () : "NONE");
				//}
			}
		} else {
			text += "NONE";
		}
        hexTileInfoLbl.text = text;
    }

//	public void OnClickCloseBtn(){
////		UIManager.Instance.playerActionsUI.HidePlayerActionsUI ();
//		HideMenu ();
//	}
}
