using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ECS;
using TMPro;
using UnityEngine.UI;

public class CharacterInfoUI : UIMenu {

    private const int MAX_HISTORY_LOGS = 20;
    public bool isWaitingForAttackTarget;
    public bool isWaitingForJoinBattleTarget;

    [Space(10)]
    [Header("Content")]
    [SerializeField] private TweenPosition tweenPos;
    [SerializeField] private CharacterPortrait characterPortrait;
    [SerializeField] private TextMeshProUGUI basicInfoLbl;
    [SerializeField] private TextMeshProUGUI generalInfoLbl;
    [SerializeField] private TextMeshProUGUI statInfoLbl;
    [SerializeField] private TextMeshProUGUI traitInfoLbl;
    [SerializeField] private TextMeshProUGUI equipmentInfoLbl;
    [SerializeField] private TextMeshProUGUI inventoryInfoLbl;
    [SerializeField] private TextMeshProUGUI relationshipsLbl;
    [SerializeField] private ScrollRect equipmentScrollView;
    [SerializeField] private ScrollRect inventoryScrollView;
    [SerializeField] private ScrollRect relationshipsScrollView;

    [Space(10)]
    [Header("Logs")]
    [SerializeField] private GameObject logHistoryPrefab;
    [SerializeField] private ScrollRect historyScrollView;
    [SerializeField] private Color evenLogColor;
    [SerializeField] private Color oddLogColor;

    [Space(10)]
    [Header("Character")]
    [SerializeField] private GameObject attackButtonGO;
    [SerializeField] private Toggle attackBtnToggle;
    [SerializeField] private GameObject joinBattleButtonGO;
    [SerializeField] private Toggle joinBattleBtnToggle;

    private LogHistoryItem[] logHistoryItems;

    private ECS.Character _activeCharacter;

    internal ECS.Character currentlyShowingCharacter {
        get { return _data as ECS.Character; }
    }

    internal ECS.Character activeCharacter {
        get { return _activeCharacter; }
    }

    internal override void Initialize() {
        base.Initialize();
        isWaitingForAttackTarget = false;
        Messenger.AddListener("UpdateUI", UpdateCharacterInfo);
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

    #region Overrides
    public override void HideMenu() {
        //if (currentlyShowingCharacter.avatar != null) {
        //    currentlyShowingCharacter.avatar.SetHighlightState(false);
        //}
        currentlyShowingCharacter.icon.SetAvatarState(false);
        _activeCharacter = null;
        base.HideMenu();
    }
    public override void ShowMenu() {
        base.ShowMenu();
        _activeCharacter = (ECS.Character)_data;
        //RepositionHistoryScrollView();
        UpdateCharacterInfo();
        UpdateAllHistoryInfo();
        SetAttackButtonState(false);
    }
    public override void OpenMenu() {
        base.OpenMenu();
        //RepositionHistoryScrollView();
        //UpdateCharacterInfo();
        //UpdateAllHistoryInfo();
    }
    #endregion

    public override void SetData(object data) {
        if (_data != null) {
            ECS.Character previousCharacter = _data as ECS.Character;
            //if (previousCharacter.avatar != null) {
            //    previousCharacter.avatar.SetHighlightState(false);
            //}
            //previousCharacter.icon.SetAvatarState(false);
        }
        base.SetData(data);
        //if (currentlyShowingCharacter.avatar != null) {
        //    currentlyShowingCharacter.avatar.SetHighlightState(true);
        //}
        //currentlyShowingCharacter.icon.SetAvatarState(true);
        //historyScrollView.ResetPosition();
        if (isShowing) {
            UpdateCharacterInfo();
        }
    }

    public void UpdateCharacterInfo() {
        if (currentlyShowingCharacter == null) {
            return;
        }
        UpdatePortrait();
        UpdateBasicInfo();
        UpdateGeneralInfo();
        UpdateStatInfo();
        UpdateTraitInfo();
        //UpdateFollowersInfo ();
        UpdateEquipmentInfo();
        UpdateInventoryInfo();
        UpdateRelationshipInfo();
        //UpdateAllHistoryInfo ();
    }
    private void UpdatePortrait() {
        characterPortrait.GeneratePortrait(currentlyShowingCharacter);
    }
    private void UpdateBasicInfo() {
        string text = string.Empty;
        text += "<b>Name: </b>" + currentlyShowingCharacter.name;
        text += "\n<b>Race: </b>" + Utilities.GetNormalizedSingularRace(currentlyShowingCharacter.raceSetting.race) + " " + Utilities.NormalizeString(currentlyShowingCharacter.gender.ToString());
        if (currentlyShowingCharacter.characterClass != null && currentlyShowingCharacter.characterClass.className != "Classless") {
            text += "\n<b>Class: </b> " + currentlyShowingCharacter.characterClass.className;
        }
        if (currentlyShowingCharacter.role != null) {
            text += "\n<b>Role: </b>" + currentlyShowingCharacter.role.roleType.ToString();
        }

        text += "\n<b>Faction: </b>" + (currentlyShowingCharacter.faction != null ? currentlyShowingCharacter.faction.urlName : "NONE");
        text += "\n<b>Village: </b>";
        if (currentlyShowingCharacter.home != null) {
            text += currentlyShowingCharacter.home.urlName;
        } else {
            text += "NONE";
        }
        basicInfoLbl.text = text;
    }
    public void UpdateGeneralInfo() {
        string text = string.Empty;
        //text += currentlyShowingCharacter.id;
        text += "<b>Specific Location: </b>" + (currentlyShowingCharacter.specificLocation != null ? currentlyShowingCharacter.specificLocation.locationName : "NONE");
        text += "\n<b>Current Action: </b>";
        if (currentlyShowingCharacter.actionData.currentAction != null) {
            text += currentlyShowingCharacter.actionData.currentAction.actionData.actionName.ToString() + " ";
            //for (int i = 0; i < currentlyShowingCharacter.currentAction.alignments.Count; i++) {
            //    ACTION_ALIGNMENT currAlignment = currentlyShowingCharacter.currentAction.alignments[i];
            //    text += currAlignment.ToString();
            //    if (i + 1 < currentlyShowingCharacter.currentAction.alignments.Count) {
            //        text += ", ";
            //    }
            //}
        } else {
            text += "NONE";
        }
        //text += "\nCurrent State: ";
        //if (currentlyShowingCharacter.currentAction != null) {
        //	if(currentlyShowingCharacter.currentAction.currentState != null){
        //		text += currentlyShowingCharacter.currentAction.currentState.stateName;
        //	}
        //} else {
        //	text += "NONE";
        //}
        //text += "\nCurrent Quest: ";
        //if (currentlyShowingCharacter.currentQuest != null) {
        //    text += currentlyShowingCharacter.currentQuest.questName.ToString() + "(" + currentlyShowingCharacter.currentQuestPhase.phaseName + ")";
        //} else {
        //    text += "NONE";
        //}
        //text += "\nGold: " +  currentlyShowingCharacter.gold.ToString();
        //text += ",    Prestige: " + currentlyShowingCharacter.prestige.ToString();
        //text += "\nParty: " + (currentlyShowingCharacter.party != null ? currentlyShowingCharacter.party.urlName : "NONE");
        //text += "\nCivilians: " + "[url=civilians]" + currentlyShowingCharacter.civilians.ToString () + "[/url]";
        if (currentlyShowingCharacter.role != null) {
            text += "\n<b>Fullness: </b>" + currentlyShowingCharacter.role.fullness + ", <b>Energy: </b>" + currentlyShowingCharacter.role.energy + ", <b>Fun: </b>" + currentlyShowingCharacter.role.fun;
            text += "\n<b>Sanity: </b>" + currentlyShowingCharacter.role.sanity + ", <b>Prestige: </b>" + currentlyShowingCharacter.role.prestige + ", <b>Safety: </b>" + currentlyShowingCharacter.role.safety;
            text += "\n<b>Happiness: </b>" + currentlyShowingCharacter.role.happiness;
        }
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

    private void UpdateStatInfo() {
        string text = string.Empty;
        text += "<b>Lvl: </b>" + currentlyShowingCharacter.level.ToString() + "/" + CharacterManager.Instance.maxLevel.ToString();
        text += ", <b>Exp: </b>" + currentlyShowingCharacter.experience.ToString() + "/" + currentlyShowingCharacter.maxExperience.ToString();
        text += "\n<b>HP: </b>" + currentlyShowingCharacter.currentHP.ToString() + "/" + currentlyShowingCharacter.maxHP.ToString();
        text += ", <b>SP: </b>" + currentlyShowingCharacter.currentSP.ToString() + "/" + currentlyShowingCharacter.maxSP.ToString();
        text += "\n<b>Str: </b>" + currentlyShowingCharacter.strength.ToString();
        text += ", <b>Int: </b>" + currentlyShowingCharacter.intelligence.ToString();
        text += "\n<b>Agi: </b>" + currentlyShowingCharacter.agility.ToString();
        text += ", <b>Vit: </b>" + currentlyShowingCharacter.vitality.ToString();
        statInfoLbl.text = text;
    }
    private void UpdateTraitInfo() {
        string text = string.Empty;
        if (currentlyShowingCharacter.traits.Count > 0 || currentlyShowingCharacter.tags.Count > 0) {
            for (int i = 0; i < currentlyShowingCharacter.traits.Count; i++) {
                Trait trait = currentlyShowingCharacter.traits[i];
                if (i > 0) {
                    text += ", ";
                }
                text += trait.traitName;
            }
            if (currentlyShowingCharacter.traits.Count > 0) {
                text += ", ";
            }
            for (int i = 0; i < currentlyShowingCharacter.tags.Count; i++) {
                CharacterTag tag = currentlyShowingCharacter.tags[i];
                if (i > 0) {
                    text += ", ";
                }
                text += tag.tagName;
            }
        } else {
            text += "\nNONE";
        }
        traitInfoLbl.text = text;
    }
    //private void UpdateFollowersInfo(){
    //	string text = string.Empty;
    //	if(currentlyShowingCharacter.party != null && currentlyShowingCharacter.party.partyLeader.id == currentlyShowingCharacter.id){
    //		for (int i = 0; i < currentlyShowingCharacter.party.followers.Count; i++) {
    //			ECS.Character follower = currentlyShowingCharacter.party.followers [i];
    //			if(i > 0){
    //				text += "\n";
    //			}
    //			text += follower.urlName;
    //		}
    //	}else{
    //		text += "NONE";
    //	}
    //	followersLbl.text = text;
    //}

    private void UpdateEquipmentInfo() {
        string text = string.Empty;
        if (currentlyShowingCharacter.equippedItems.Count > 0) {
            for (int i = 0; i < currentlyShowingCharacter.equippedItems.Count; i++) {
                ECS.Item item = currentlyShowingCharacter.equippedItems[i];
                if (i > 0) {
                    text += "\n";
                }
                text += item.itemName;
                if (item is ECS.Weapon) {
                    ECS.Weapon weapon = (ECS.Weapon)item;
                    if (weapon.bodyPartsAttached.Count > 0) {
                        text += " (";
                        for (int j = 0; j < weapon.bodyPartsAttached.Count; j++) {
                            if (j > 0) {
                                text += ", ";
                            }
                            text += weapon.bodyPartsAttached[j].name;
                        }
                        text += ")";
                    }
                } else if (item is ECS.Armor) {
                    ECS.Armor armor = (ECS.Armor)item;
                    text += " (" + armor.bodyPartAttached.name + ")";
                }
            }
        } else {
            text += "NONE";
        }
        equipmentInfoLbl.text = text;
        //equipmentScrollView.UpdatePosition ();
    }

    private void UpdateInventoryInfo() {
        string text = string.Empty;
        foreach (RESOURCE resource in currentlyShowingCharacter.characterObject.resourceInventory.Keys) {
            text += resource.ToString() + ": " + currentlyShowingCharacter.characterObject.resourceInventory[resource];
            text += "\n";
        }
        //if(currentlyShowingCharacter.inventory.Count > 0) {
        //	for (int i = 0; i < currentlyShowingCharacter.inventory.Count; i++) {
        //		ECS.Item item = currentlyShowingCharacter.inventory [i];
        //		if(i > 0){
        //			text += "\n";
        //		}
        //		text += item.itemName;
        //	}
        //}else{
        //	text += "NONE";
        //}
        inventoryInfoLbl.text = text;
        //inventoryScrollView.UpdatePosition ();
    }

    private void UpdateRelationshipInfo() {
        string text = string.Empty;
        if (currentlyShowingCharacter.relationships.Count > 0) {
            bool isFirst = true;
            foreach (KeyValuePair<ECS.Character, Relationship> kvp in currentlyShowingCharacter.relationships) {
                if (!isFirst) {
                    text += "\n";
                } else {
                    isFirst = false;
                }
                text += kvp.Key.role.roleType.ToString() + " " + kvp.Key.urlName;
                if (kvp.Value.relationshipStatuses.Count > 0) {
                    text += "(";
                    for (int i = 0; i < kvp.Value.relationshipStatuses.Count; i++) {
                        if (i > 0) {
                            text += ",";
                        }
                        text += kvp.Value.relationshipStatuses[i].ToString();
                    }
                    text += ")";
                }
            }
        } else {
            text += "NONE";
        }

        relationshipsLbl.text = text;
        //relationshipsScrollView.UpdatePosition();
    }

    #region History
    private void UpdateHistory(object obj) {
        if (obj is ECS.Character && currentlyShowingCharacter != null && (obj as ECS.Character).id == currentlyShowingCharacter.id) {
            UpdateAllHistoryInfo();
        }
    }
    private void UpdateAllHistoryInfo() {
        List<Log> characterHistory = new List<Log>(currentlyShowingCharacter.history.OrderBy(x => x.id));
        for (int i = 0; i < logHistoryItems.Length; i++) {
            LogHistoryItem currItem = logHistoryItems[i];
            Log currLog = characterHistory.ElementAtOrDefault(i);
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

    //public void CenterCameraOnCharacter() {
    //    GameObject centerOn = null;
    //    if (currentlyShowingCharacter.avatar != null) {
    //        centerOn = currentlyShowingCharacter.avatar.specificLocation.tileLocation.gameObject;
    //    } else {
    //        centerOn = currentlyShowingCharacter.currLocation.gameObject;
    //    }
    //    CameraMove.Instance.CenterCameraOn(centerOn);
    //}

    public bool IsCharacterInfoShowing(ECS.Character character) {
        return (isShowing && currentlyShowingCharacter == character);
    }

    #region Attack Character
    private void ShowAttackButton() {
        SetActiveAttackButtonGO(true);
        SetAttackButtonState(false);
    }
    public void ToggleAttack() {
        isWaitingForAttackTarget = !isWaitingForAttackTarget;
        if (isWaitingForAttackTarget) {
            SetJoinBattleButtonState(false);
        }
    }
    public void SetAttackButtonState(bool state) {
        isWaitingForAttackTarget = state;
        attackBtnToggle.isOn = state;
        if (isWaitingForAttackTarget) {
            SetJoinBattleButtonState(false);
        }
    }
    public void SetActiveAttackButtonGO(bool state) {
        attackButtonGO.SetActive(state);
    }
    #endregion

    #region Join Battle Character
    private void ShowJoinBattleButton() {
        SetActiveJoinBattleButtonGO(true);
        SetJoinBattleButtonState(false);
    }
    public void ToggleJoinBattle() {
        isWaitingForJoinBattleTarget = !isWaitingForJoinBattleTarget;
        if (isWaitingForJoinBattleTarget) {
            SetAttackButtonState(false);
        }
    }
    public void SetJoinBattleButtonState(bool state) {
        isWaitingForJoinBattleTarget = state;
        joinBattleBtnToggle.isOn = state;
        if (isWaitingForJoinBattleTarget) {
            SetAttackButtonState(false);
        }
    }
    public void SetActiveJoinBattleButtonGO(bool state) {
        joinBattleButtonGO.SetActive(state);
    }
    #endregion

    //	public void OnClickCloseBtn(){
    ////		UIManager.Instance.playerActionsUI.HidePlayerActionsUI ();
    //		HideMenu ();
    //	}
}
