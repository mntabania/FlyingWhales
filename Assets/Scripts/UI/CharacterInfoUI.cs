﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CharacterInfoUI : UIMenu {

    private const int MAX_HISTORY_LOGS = 20;

    [Space(10)]
    [Header("Content")]
    [SerializeField] private TweenPosition tweenPos;
    [SerializeField] private UILabel generalInfoLbl;
	[SerializeField] private UILabel statInfoLbl;
	[SerializeField] private UILabel traitInfoLbl;
	[SerializeField] private UILabel followersLbl;
	[SerializeField] private UILabel equipmentInfoLbl;
	[SerializeField] private UILabel inventoryInfoLbl;
	[SerializeField] private UILabel relationshipsLbl;
	[SerializeField] private UILabel historyLbl;

	[SerializeField] private UIScrollView followersScrollView;
    [SerializeField] private UIScrollView equipmentScrollView;
	[SerializeField] private UIScrollView inventoryScrollView;
    [SerializeField] private UIScrollView relationshipsScrollView;

    [Space(10)]
    [Header("Logs")]
    [SerializeField]
    private GameObject logHistoryPrefab;
    [SerializeField] private UITable logHistoryTable;
    [SerializeField] private UIScrollView historyScrollView;
    [SerializeField] private Color evenLogColor;
    [SerializeField] private Color oddLogColor;

    private LogHistoryItem[] logHistoryItems;

	private ECS.Character _activeCharacter;

    internal ECS.Character currentlyShowingCharacter {
        get { return _data as ECS.Character; }
    }

	internal ECS.Character activeCharacter{
		get { return _activeCharacter; }
	}

    internal override void Initialize() {
        base.Initialize();
        Messenger.AddListener("UpdateUI", UpdateCharacterInfo);
        logHistoryItems = new LogHistoryItem[MAX_HISTORY_LOGS];
        //populate history logs table
        for (int i = 0; i < MAX_HISTORY_LOGS; i++) {
            GameObject newLogItem = ObjectPoolManager.Instance.InstantiateObjectFromPool(logHistoryPrefab.name, Vector3.zero, Quaternion.identity, logHistoryTable.transform);
            newLogItem.name = "-1";
            logHistoryItems[i] = newLogItem.GetComponent<LogHistoryItem>();
            newLogItem.transform.localScale = Vector3.one;
            newLogItem.SetActive(true);
        }
    }

	public override void ShowMenu (){
		base.ShowMenu ();
        _activeCharacter = (ECS.Character)_data;
        historyScrollView.ResetPosition();
    }
    public override void OpenMenu() {
        base.OpenMenu();
        historyScrollView.ResetPosition();
        UpdateCharacterInfo();
    }

    public override void SetData(object data) {
        if (_data != null) {
            ECS.Character previousCharacter = _data as ECS.Character;
            if (previousCharacter.avatar != null) {
                previousCharacter.avatar.SetHighlightState(false);
            }
        }
        base.SetData(data);
        if (currentlyShowingCharacter.avatar != null) {
            currentlyShowingCharacter.avatar.SetHighlightState(true);
        }
        historyScrollView.ResetPosition();
        if (isShowing) {
            UpdateCharacterInfo();
        }
    }

	public void UpdateCharacterInfo(){
		if(currentlyShowingCharacter == null) {
			return;
		}
		UpdateGeneralInfo();
		UpdateStatInfo ();
		UpdateTraitInfo	();
		UpdateFollowersInfo ();
		UpdateEquipmentInfo ();
		UpdateInventoryInfo ();
		UpdateRelationshipInfo ();
		UpdateHistoryInfo ();
	}
    public void UpdateGeneralInfo() {
        string text = string.Empty;
        text += currentlyShowingCharacter.id;
        text += "\n" + currentlyShowingCharacter.name;
		text += "\n" + Utilities.GetNormalizedSingularRace (currentlyShowingCharacter.raceSetting.race) + " " + Utilities.NormalizeString (currentlyShowingCharacter.gender.ToString ());
		if(currentlyShowingCharacter.characterClass != null && currentlyShowingCharacter.characterClass.className != "Classless"){
			text += " " + currentlyShowingCharacter.characterClass.className;
		}
		if(currentlyShowingCharacter.role != null){
			text += " (" + currentlyShowingCharacter.role.roleType.ToString() + ")";
		}

		text += "\nFaction: " + (currentlyShowingCharacter.faction != null ? currentlyShowingCharacter.faction.urlName : "NONE");
        text += ",    Village: ";
        if (currentlyShowingCharacter.home != null) {
            text += currentlyShowingCharacter.home.urlName;
        } else {
            text += "NONE";
        }
        text += "\nCurrent Action: ";
        if (currentlyShowingCharacter.currentTask != null) {
            text += currentlyShowingCharacter.currentTask.taskType.ToString() + " ";
            for (int i = 0; i < currentlyShowingCharacter.currentTask.alignments.Count; i++) {
                ACTION_ALIGNMENT currAlignment = currentlyShowingCharacter.currentTask.alignments[i];
                text += currAlignment.ToString();
                if (i + 1 < currentlyShowingCharacter.currentTask.alignments.Count) {
                    text += ", ";
                }
            }
        } else {
            text += "NONE";
        }
		text += "\nCurrent State: ";
		if (currentlyShowingCharacter.currentTask != null) {
			if(currentlyShowingCharacter.currentTask.currentState != null){
				text += currentlyShowingCharacter.currentTask.currentState.stateName;
			}
		} else {
			text += "NONE";
		}
        text += "\nCurrent Quest: ";
        if (currentlyShowingCharacter.currentQuest != null) {
            text += currentlyShowingCharacter.currentQuest.questName.ToString() + "(" + currentlyShowingCharacter.currentQuestPhase.phaseName + ")";
        } else {
            text += "NONE";
        }
        text += "\nGold: " +  currentlyShowingCharacter.gold.ToString();
        text += ",    Prestige: " + currentlyShowingCharacter.prestige.ToString();
		text += "\nParty: " + (currentlyShowingCharacter.party != null ? currentlyShowingCharacter.party.urlName : "NONE");
		text += "\nCivilians: " + "[url=civilians]" + currentlyShowingCharacter.civilians.ToString () + "[/url]";
//        foreach (KeyValuePair<RACE, int> kvp in currentlyShowingCharacter.civiliansByRace) {
//            if (kvp.Value > 0) {
//                text += "\n" + kvp.Key.ToString() + " - " + kvp.Value.ToString();
//            }
//        }
        //		text += "\n[b]Skills:[/b] ";
        //		if(currentlyShowingCharacter.skills.Count > 0){
        //			for (int i = 0; i < currentlyShowingCharacter.skills.Count; i++) {
        //				ECS.Skill skill = currentlyShowingCharacter.skills [i];
        //				text += "\n  - " + skill.skillName;
        //			}
        //		}else{
        //			text += "NONE";
        //		}

        generalInfoLbl.text = text;
//        infoScrollView.ResetPosition();

    }

	private void UpdateStatInfo(){
		string text = string.Empty;
		text += "[b]STATS[/b]";
		text += "\nHP: " + currentlyShowingCharacter.currentHP.ToString() + "/" + currentlyShowingCharacter.maxHP.ToString();
		text += "\nStr: " + currentlyShowingCharacter.strength.ToString();
		text += "\nInt: " + currentlyShowingCharacter.intelligence.ToString();
		text += "\nAgi: " + currentlyShowingCharacter.agility.ToString();
		statInfoLbl.text = text;
	}
	private void UpdateTraitInfo(){
		string text = string.Empty;
		text += "[b]TRAITS AND TAGS[/b]";
		if(currentlyShowingCharacter.traits.Count > 0){
			text += "\n";
			for (int i = 0; i < currentlyShowingCharacter.traits.Count; i++) {
				Trait trait = currentlyShowingCharacter.traits [i];
				if(i > 0){
					text += ", ";
				}
				text += trait.traitName;
			}
			if(currentlyShowingCharacter.traits.Count > 0){
				text += ", ";
			}
			for (int i = 0; i < currentlyShowingCharacter.tags.Count; i++) {
				CharacterTag tag = currentlyShowingCharacter.tags [i];
				if(i > 0){
					text += ", ";
				}
				text += tag.tagName;
			}
		}else{
			text += "\nNONE";
		}
		traitInfoLbl.text = text;
	}
	private void UpdateFollowersInfo(){
		string text = string.Empty;
		if(currentlyShowingCharacter.party != null && currentlyShowingCharacter.party.partyLeader.id == currentlyShowingCharacter.id){
			for (int i = 0; i < currentlyShowingCharacter.party.followers.Count; i++) {
				ECS.Character follower = currentlyShowingCharacter.party.followers [i];
				if(i > 0){
					text += "\n";
				}
				text += follower.urlName;
			}
		}else{
			text += "NONE";
		}
		followersLbl.text = text;
		followersScrollView.UpdatePosition ();
	}

	private void UpdateEquipmentInfo(){
		string text = string.Empty;
		if(currentlyShowingCharacter.equippedItems.Count > 0){
			for (int i = 0; i < currentlyShowingCharacter.equippedItems.Count; i++) {
				ECS.Item item = currentlyShowingCharacter.equippedItems [i];
				if(i > 0){
					text += "\n";
				}
				text += item.itemName;
				if(item is ECS.Weapon){
					ECS.Weapon weapon = (ECS.Weapon)item;
					if(weapon.bodyPartsAttached.Count > 0){
						text += " (";
						for (int j = 0; j < weapon.bodyPartsAttached.Count; j++) {
							if(j > 0){
								text += ", ";
							}
							text += weapon.bodyPartsAttached [j].name;
						}
						text += ")";
					}
				}else if(item is ECS.Armor){
					ECS.Armor armor = (ECS.Armor)item;
					text += " (" + armor.bodyPartAttached.name + ")";
				}
			}
		}else{
			text += "NONE";
		}
		equipmentInfoLbl.text = text;
		equipmentScrollView.UpdatePosition ();
	}

	private void UpdateInventoryInfo(){
		string text = string.Empty;
		if(currentlyShowingCharacter.inventory.Count > 0) {
			for (int i = 0; i < currentlyShowingCharacter.inventory.Count; i++) {
				ECS.Item item = currentlyShowingCharacter.inventory [i];
				if(i > 0){
					text += "\n";
				}
				text += item.itemName;
			}
		}else{
			text += "NONE";
		}
		inventoryInfoLbl.text = text;
		inventoryScrollView.UpdatePosition ();
	}

	private void UpdateRelationshipInfo(){
		string text = string.Empty;
		if (currentlyShowingCharacter.relationships.Count > 0) {
			bool isFirst = true;
			foreach (KeyValuePair<ECS.Character, Relationship> kvp in currentlyShowingCharacter.relationships) {
				if(!isFirst){
					text += "\n";
				}else{
					isFirst = false;
				}
				text += kvp.Key.role.roleType.ToString() + " " + kvp.Key.urlName + ": " + kvp.Value.totalValue.ToString();
			}
		} else {
			text += "NONE";
		}

		relationshipsLbl.text = text;
		relationshipsScrollView.UpdatePosition();
	}
	private void UpdateHistoryInfo(){
        for (int i = 0; i < logHistoryItems.Length; i++) {
            LogHistoryItem currItem = logHistoryItems[i];
            Log currLog = currentlyShowingCharacter.history.ElementAtOrDefault(i);
            if (currLog != null) {
                currItem.SetLog(currLog);
                currItem.gameObject.SetActive(true);

                if (Utilities.IsEven(i)) {
                    currItem.SetLogColor(evenLogColor);
                } else {
                    currItem.SetLogColor(oddLogColor);
                }
            } else {
                currItem.gameObject.SetActive(false);
            }
        }
        if (this.gameObject.activeInHierarchy) {
            StartCoroutine(UIManager.Instance.RepositionTable(logHistoryTable));
        }
        //string text = string.Empty;
        //if (currentlyShowingCharacter.history.Count > 0) {
        //	for (int i = 0; i < currentlyShowingCharacter.history.Count; i++) {
        //		if(i > 0){
        //			text += "\n";
        //		}
        //		text += currentlyShowingCharacter.history[i];
        //	}
        //} else {
        //	text += "NONE";
        //}

        //historyLbl.text = text;
        //historyScrollView.UpdatePosition();
    }
	public void CenterCameraOnCharacter() {
        GameObject centerOn = null;
        if (currentlyShowingCharacter.avatar != null) {
			centerOn = currentlyShowingCharacter.avatar.specificLocation.tileLocation.gameObject;
        } else {
            centerOn = currentlyShowingCharacter.currLocation.gameObject;
        }
        CameraMove.Instance.CenterCameraOn(centerOn);
    }

    public bool IsCharacterInfoShowing(ECS.Character character) {
        return (isShowing && currentlyShowingCharacter == character);
    }

	#region Overrides
	public override void HideMenu (){
        if (currentlyShowingCharacter.avatar != null) {
            currentlyShowingCharacter.avatar.SetHighlightState(false);
        }
		_activeCharacter = null;
		base.HideMenu ();
	}
	#endregion
//	public void OnClickCloseBtn(){
////		UIManager.Instance.playerActionsUI.HidePlayerActionsUI ();
//		HideMenu ();
//	}
}
