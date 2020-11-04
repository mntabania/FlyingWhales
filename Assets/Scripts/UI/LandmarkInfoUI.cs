using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;

public class LandmarkInfoUI : UIMenu {

    private const int MAX_HISTORY_LOGS = 20;

    public bool isWaitingForAttackTarget;

    [Space(10)]
    [Header("Content")]
    [SerializeField] private TweenPosition tweenPos;
    [SerializeField] private TextMeshProUGUI landmarkInfoLbl;
    [SerializeField] private ScrollRect infoScrollView;
	

    [Space(10)]
    [Header("Logs")]
    [SerializeField] private GameObject logHistoryPrefab;
    [SerializeField] private ScrollRect historyScrollView;
    [SerializeField] private Color evenLogColor;
    [SerializeField] private Color oddLogColor;

    [Space(10)]
    [Header("Settlement")]
    [SerializeField] private GameObject attackButtonGO;
    [SerializeField] private Toggle attackBtnToggle;

    private LogHistoryItem[] logHistoryItems;

    internal BaseLandmark currentlyShowingLandmark {
        get { return _data as BaseLandmark; }
    }

    internal override void Initialize() {
        base.Initialize();
        isWaitingForAttackTarget = false;
        Messenger.AddListener("UpdateUI", UpdateLandmarkInfo);
        //tweenPos.AddOnFinished(() => UpdateLandmarkInfo());
        Messenger.AddListener<object>(Signals.HISTORY_ADDED, UpdateHistory);

        logHistoryItems = new LogHistoryItem[MAX_HISTORY_LOGS];
        //populate history logs table
        for (int i = 0; i < MAX_HISTORY_LOGS; i++) {
            GameObject newLogItem = ObjectPoolManager.Instance.InstantiateObjectFromPool(logHistoryPrefab.name, Vector3.zero, Quaternion.identity, historyScrollView.content);
            //newLogItem.name = "-1";
            logHistoryItems[i] = newLogItem.GetComponent<LogHistoryItem>();
            newLogItem.transform.localScale = Vector3.one;
            newLogItem.SetActive(true);
        }
        for (int i = 0; i < logHistoryItems.Length; i++) {
            logHistoryItems[i].gameObject.SetActive(false);
        }
    }

    public override void OpenMenu() {
        base.OpenMenu();
        //RepositionHistoryScrollView();
        //StartCoroutine(UIManager.Instance.RepositionScrollView(infoScrollView));
        //UpdateLandmarkInfo();
        //UpdateAllHistoryInfo();
    }

    public override void ShowMenu() {
        base.ShowMenu();
        //RepositionHistoryScrollView();
        //StartCoroutine(UIManager.Instance.RepositionScrollView(infoScrollView));
        UpdateLandmarkInfo();
        UpdateAllHistoryInfo();
        ShowAttackButton();
    }
    public override void HideMenu() {
        base.HideMenu();
        SetAttackButtonState(false);
        //HidePlayerActions();
    }
    public override void SetData(object data) {
        base.SetData(data);
        UIManager.Instance.hexTileInfoUI.SetData((data as BaseLandmark).tileLocation);
        //ShowPlayerActions();
        if (isShowing) {
            UpdateLandmarkInfo();
        }
    }

    public void UpdateLandmarkInfo() {
        if(currentlyShowingLandmark == null) {
            return;
        }
        string text = string.Empty;
		if (currentlyShowingLandmark.landmarkName != string.Empty) {
			text += "<b>Name:</b> " + currentlyShowingLandmark.landmarkName + "\n";
		}
		text += "<b>Location:</b> " + currentlyShowingLandmark.tileLocation.urlName;
        text += "\n<b>Landmark Type:</b> " + currentlyShowingLandmark.landmarkObj.objectName + " (" + currentlyShowingLandmark.landmarkObj.currentState.stateName + ")";
        text += "\n<b>HP:</b> " + currentlyShowingLandmark.landmarkObj.currentHP.ToString() + "/" + currentlyShowingLandmark.landmarkObj.maxHP.ToString();
        text += "\n<b>Durability:</b> " + currentlyShowingLandmark.currDurability.ToString() + "/" + currentlyShowingLandmark.totalDurability.ToString();
        text += "\n<b>Can Be Occupied:</b> " + currentlyShowingLandmark.canBeOccupied.ToString();
		text += "\n<b>Is Occupied:</b> " + currentlyShowingLandmark.isOccupied.ToString();

        if (currentlyShowingLandmark.owner != null) {
            text += "\n<b>Owner:</b> " + currentlyShowingLandmark.owner.urlName + "/" + currentlyShowingLandmark.owner.race.ToString();
            //text += "\n<b>Regional Population: </b> " + currentlyShowingLandmark.totalPopulation.ToString();
            text += "\n<b>Settlement Population: </b> " + "<link=civilians>" + currentlyShowingLandmark.landmarkObj.GetTotalCivilians().ToString() + "</link>";
			//text += "\n<b>Population Growth: </b> " + (currentlyShowingLandmark.totalPopulation * currentlyShowingLandmark.tileLocation.region.populationGrowth).ToString();
        }

        text += "\n<b>Connections: </b> ";
        if (currentlyShowingLandmark.connections.Count > 0) {
            for (int i = 0; i < currentlyShowingLandmark.connections.Count; i++) {
                BaseLandmark connection = currentlyShowingLandmark.connections[i];
                text += "\n" + connection.urlName;
            }
        } else {
            text += "NONE";
        }

        text += "\n<b>Characters At Landmark: </b> ";
        if (currentlyShowingLandmark.charactersAtLocation.Count > 0) {
			for (int i = 0; i < currentlyShowingLandmark.charactersAtLocation.Count; i++) {
                object currObject = currentlyShowingLandmark.charactersAtLocation[i];
                if (currObject is ECS.Character) {
					ECS.Character currChar = (ECS.Character)currObject;
					text += "\n" + currChar.urlName + " - " + (currChar.characterClass != null ? currChar.characterClass.className : "NONE") + "/" + (currChar.role != null ? currChar.role.roleType.ToString () : "NONE");
					if (currChar.actionData.currentAction != null) {
						//if (currChar.currentTask.taskType == TASK_TYPE.QUEST) {
						//	OldQuest.Quest currQuest = (OldQuest.Quest)currChar.currentTask;
						//	text += " (" + currQuest.urlName + ")";
						//} else {
							text += " (" + currChar.actionData.currentAction.actionData.actionName + ")";
						//}
                        //for (int j = 0; j < currChar.currentAction.alignments.Count; j++) {
                        //    ACTION_ALIGNMENT currAlignment = currChar.currentAction.alignments[j];
                        //    text += currAlignment.ToString();
                        //    if (j + 1 < currChar.currentAction.alignments.Count) {
                        //        text += ", ";
                        //    }
                        //}
                    }
				} else if (currObject is Party) {
					Party currParty = (Party)currObject;
					text += "\n" + currParty.urlNameWithRole + " - " + (currParty.currentAction != null ? currParty.currentAction.ToString () : "NONE");
				}
			}
		} else {
			text += "NONE";
		}
        text += "\n<b>Characters At Tile: </b> ";
		if (currentlyShowingLandmark.tileLocation.charactersAtLocation.Count > 0) {
			for (int i = 0; i < currentlyShowingLandmark.tileLocation.charactersAtLocation.Count; i++) {
				object currObject = currentlyShowingLandmark.tileLocation.charactersAtLocation[i];
                if (currObject is ECS.Character) {
                    ECS.Character currChar = (ECS.Character)currObject;
                    text += "\n" + currChar.urlName + " - " + (currChar.characterClass != null ? currChar.characterClass.className : "NONE") + "/" + (currChar.role != null ? currChar.role.roleType.ToString() : "NONE");
                    if (currChar.actionData.currentAction != null) {
                        //if (currChar.currentTask.taskType == TASK_TYPE.QUEST) {
                        //    OldQuest.Quest currQuest = (OldQuest.Quest)currChar.currentTask;
                        //    text += " (" + currQuest.urlName + ")";
                        //} else {
                            text += " (" + currChar.actionData.currentAction.actionData.actionName + ")";
                        //}
                    }
                } else if (currObject is Party) {
                    Party currParty = (Party)currObject;
                    text += "\n" + currParty.urlName + " - " + (currParty.currentAction != null ? currParty.currentAction.ToString() : "NONE");
                }
            }
        } else {
            text += "NONE";
        }

		text += "\n<b>Prisoners:</b> ";
		if(currentlyShowingLandmark.prisoners.Count > 0){
			for (int i = 0; i < currentlyShowingLandmark.prisoners.Count; i++) {
				ECS.Character prisoner = currentlyShowingLandmark.prisoners [i];
				text += "\n" + prisoner.prisonerName;
			}
		} else {
			text += "NONE";
		}

		text += "\n<b>Traces:</b> ";
		if(currentlyShowingLandmark.characterTraces.Count > 0){
			foreach (ECS.Character character in currentlyShowingLandmark.characterTraces.Keys) {
				text += "\n" + character.urlName + ", " + currentlyShowingLandmark.characterTraces[character].ToStringDate();
			}
		} else {
			text += "NONE";
		}

        //if (currentlyShowingLandmark is Settlement) {
        //    Settlement currSettlement = currentlyShowingLandmark as Settlement;
        //    text += "\n<b>Materials: </b> ";
        //    if (currSettlement.availableMaterials.Count > 0) {
        //        for (int i = 0; i < currSettlement.availableMaterials.Count; i++) {
        //            MATERIAL currMat = currSettlement.availableMaterials[i];
        //            text += "\n" + currMat.ToString();
        //        }
        //    } else {
        //        text += "NONE";
        //    }
        //}

        //text += "\n<b>Objects: </b> ";
        //if (currentlyShowingLandmark.objects.Count > 0) {
        //    for (int i = 0; i < currentlyShowingLandmark.objects.Count; i++) {
        //        IObject currObj = currentlyShowingLandmark.objects[i];
        //        if (!currObj.isInvisible) {
        //            text += "\n" + currObj.objectName + " (" + currObj.currentState.stateName + ")";
        //            if(currObj.objectType == OBJECT_TYPE.STRUCTURE) {
        //                StructureObj structure = currObj as StructureObj;
        //                text += " (" + structure.currentHP + "/" + structure.maxHP + ")";
        //            }
        //        }
        //    }
        //} else {
        //    text += "NONE";
        //}

        text += "\n<b>Technologies: </b> ";
        List<TECHNOLOGY> availableTech = currentlyShowingLandmark.technologies.Where(x => x.Value == true).Select(x => x.Key).ToList();
        if (availableTech.Count > 0) {
            for (int i = 0; i < availableTech.Count; i++) {
                TECHNOLOGY currTech = availableTech[i];
                text += "\n" + currTech.ToString();
                //if (i + 1 != availableTech.Count) {
                //    text += ", ";
                //}
            }
        } else {
            text += "NONE";
        }

		text += "\n<b>Items: </b> ";
		if (currentlyShowingLandmark.itemsInLandmark.Count > 0) {
			for (int i = 0; i < currentlyShowingLandmark.itemsInLandmark.Count; i++) {
				ECS.Item item = currentlyShowingLandmark.itemsInLandmark[i];
				text += "\n" + item.itemName + " (" + ((item.owner == null ? "NONE" : item.owner.name)) + ")";
			}
		} else {
			text += "NONE";
		}
       
        landmarkInfoLbl.text = text;

		//UpdateHistoryInfo ();
    }

    #region Log History
    private void UpdateHistory(object obj) {
        if (obj is BaseLandmark && currentlyShowingLandmark != null && (obj as BaseLandmark).id == currentlyShowingLandmark.id) {
            UpdateAllHistoryInfo();
        }
    }
    private void UpdateAllHistoryInfo() {
        List<Log> landmarkHistory = new List<Log>(currentlyShowingLandmark.history.OrderBy(x => x.id));
        for (int i = 0; i < logHistoryItems.Length; i++) {
            LogHistoryItem currItem = logHistoryItems[i];
            Log currLog = landmarkHistory.ElementAtOrDefault(i);
            if (currLog != null) {
                currItem.gameObject.SetActive(true);
                currItem.SetLog(currLog);
                if (Utilities.IsEven(i)) {
                    currItem.SetLogColor(evenLogColor);
                } else {
                    currItem.SetLogColor(oddLogColor);
                }
            } else {
                currItem.gameObject.SetActive(false);
            }
        }
        //if (this.gameObject.activeInHierarchy) {
        //    StartCoroutine(UIManager.Instance.RepositionTable(logHistoryTable));
        //    StartCoroutine(UIManager.Instance.RepositionScrollView(historyScrollView));
        //}
    }
    //private void RepositionHistoryScrollView() {
    //    for (int i = 0; i < logHistoryItems.Length; i++) {
    //        LogHistoryItem currItem = logHistoryItems[i];
    //        currItem.gameObject.SetActive(true);
    //    }
    //    StartCoroutine(UIManager.Instance.RepositionScrollView(historyScrollView));
    //    for (int i = 0; i < logHistoryItems.Length; i++) {
    //        LogHistoryItem currItem = logHistoryItems[i];
    //        currItem.gameObject.SetActive(false);
    //    }
    //}
    private bool IsLogAlreadyShown(Log log) {
        for (int i = 0; i < logHistoryItems.Length; i++) {
            LogHistoryItem currItem = logHistoryItems[i];
            if (currItem.log != null) {
                if (currItem.log.id == log.id) {
                    return true;
                }
            }
        }
        return false;
    }
    #endregion

    public void OnClickCloseBtn(){
//		UIManager.Instance.playerActionsUI.HidePlayerActionsUI ();
		HideMenu ();
	}

	//public void OnClickExpandBtn(){
	//	currentlyShowingLandmark.owner.internalQuestManager.CreateExpandQuest(currentlyShowingLandmark);
	//	expandBtnGO.SetActive (false);
	//}
	//public void OnClickExploreRegionBtn(){
	//	currentlyShowingLandmark.tileLocation.region.centerOfMass
 //           .landmarkOnTile.owner.internalQuestManager.CreateExploreTileQuest(currentlyShowingLandmark);
 //       exploreBtnGO.SetActive(false);
 //   }

//    private void ShowPlayerActions(){
//		expandBtnGO.SetActive (CanExpand());
////		exploreBtnGO.SetActive (CanExploreTile ());
//		exploreBtnGO.SetActive (false);
//        //buildStructureBtnGO.SetActive(CanBuildStructure());
//    }
	//private void HidePlayerActions(){
	//	expandBtnGO.SetActive (false);
	//	exploreBtnGO.SetActive (false);
	//}
	//private bool CanExpand(){
	//	if(isShowing && currentlyShowingLandmark != null && currentlyShowingLandmark is Settlement){
	//		Settlement settlement = (Settlement)currentlyShowingLandmark;
	//		if(settlement.owner != null && settlement.owner.factionType == FACTION_TYPE.MAJOR){
 //               Construction constructionData = ProductionManager.Instance.GetConstructionDataForCity();
 //               if (settlement.HasAdjacentUnoccupiedTile() && !settlement.owner.internalQuestManager.AlreadyHasQuestOfType(QUEST_TYPE.EXPAND, settlement)) {
 //                   return true;
 //               }
 //               //            if (settlement.CanAffordConstruction(constructionData) && settlement.HasAdjacentUnoccupiedTile() && !settlement.owner.internalQuestManager.AlreadyHasQuestOfType(QUEST_TYPE.EXPAND, settlement)){
 //               //	return true;
 //               //}
 //           }
	//	}
	//	return false;
	//}

	//private bool CanExploreTile(){
	//	if(isShowing && currentlyShowingLandmark != null && !currentlyShowingLandmark.isExplored
	//		&& currentlyShowingLandmark.owner == null && currentlyShowingLandmark.location.region.centerOfMass.isOccupied 
 //           && !currentlyShowingLandmark.location.region.centerOfMass
 //           .landmarkOnTile.owner.internalQuestManager.AlreadyHasQuestOfType(QUEST_TYPE.EXPLORE_TILE, currentlyShowingLandmark)) {
	//		return true;
	//	}
	//	return false;
	//}

    //private bool CanBuildStructure() {
    //    if (isShowing && currentlyShowingLandmark != null && currentlyShowingLandmark.location.region.owner != null 
    //        && currentlyShowingLandmark.owner == null && !currentlyShowingLandmark.location.HasStructure()
    //        && !currentlyShowingLandmark.location.region.owner.internalQuestManager.AlreadyHasQuestOfType(QUEST_TYPE.BUILD_STRUCTURE, currentlyShowingLandmark)
    //        && currentlyShowingLandmark is ResourceLandmark) {
    //        Settlement settlement = currentlyShowingLandmark.location.region.centerOfMass.landmarkOnTile as Settlement;
    //        ResourceLandmark resourceLandmark = currentlyShowingLandmark as ResourceLandmark;
    //        Construction constructionData = ProductionManager.Instance.GetConstruction(resourceLandmark.materialData.structure.name);
    //        if (settlement.CanAffordConstruction(constructionData) && settlement.HasTechnology(Utilities.GetNeededTechnologyForMaterial(resourceLandmark.materialOnLandmark))) {
    //            return true;
    //        }
    //    }
    //    return false;
    //}

    public void CenterOnLandmark() {
        currentlyShowingLandmark.CenterOnLandmark();
    }

    #region Attack Landmark
    private void ShowAttackButton() {
        BaseLandmark landmark = currentlyShowingLandmark;
        if (!landmark.isAttackingAnotherLandmark) {
            if ((landmark.landmarkObj.specificObjectType == SPECIFIC_OBJECT_TYPE.HUMAN_SETTLEMENT || landmark.landmarkObj.specificObjectType == SPECIFIC_OBJECT_TYPE.ELVEN_SETTLEMENT)
                && landmark.landmarkObj.currentState.stateName == "Ready") {

                attackButtonGO.SetActive(true);
            } else {
                attackButtonGO.SetActive(false);
            }
        } else {
            attackButtonGO.SetActive(false);
        }
        SetAttackButtonState(false);
    }
    public void ToggleAttack() {
        isWaitingForAttackTarget = !isWaitingForAttackTarget;
        attackBtnToggle.isOn = !attackBtnToggle.isOn;
    }
    public void SetAttackButtonState(bool state) {
        isWaitingForAttackTarget = state;
        attackBtnToggle.isOn = state;
    }
    public void SetActiveAttackButtonGO(bool state) {
        attackButtonGO.SetActive(state);
    }
    #endregion
}
