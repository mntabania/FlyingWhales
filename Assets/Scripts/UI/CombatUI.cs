﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CombatUI : MonoBehaviour {
    public CombatSlotItem[] leftSlots;
    public CombatSlotItem[] rightSlots;

    public GameObject combatGO;
    public GameObject selectTargetIndicatorGO;
    public GameObject combatLogItemPrefab;

    public ScrollRect combatLogsScrollView;

    public Toggle playToggle;
    public Toggle pauseToggle;


    // Use this for initialization
    void Start () {
        Initialize();
	}
	
	private void Initialize() {
        for (int i = 0; i < leftSlots.Length; i++) {
            leftSlots[i].Initialize();
            leftSlots[i].SetGridNumber(i);
        }
        for (int i = 0; i < rightSlots.Length; i++) {
            rightSlots[i].Initialize();
            rightSlots[i].SetGridNumber(i);
        }
        //Messenger.AddListener<string>(Signals.ADD_TO_COMBAT_LOGS, AddCombatLogs);
        //Messenger.AddListener<Character, SIDES>(Signals.HIGHLIGHT_ATTACKER, HighlightAttacker);
        //Messenger.AddListener<Character, SIDES>(Signals.UNHIGHLIGHT_ATTACKER, UnhighlightAttacker);
        //Messenger.AddListener(Signals.UPDATE_COMBAT_GRIDS, UpdateCombatSlotItems);
    }
    public void OpenCombatUI(bool triggerCombatFight) {
        GameManager.Instance.SetPausedState(true);
        combatGO.SetActive(true);
        if (triggerCombatFight) {
            playToggle.isOn = true;
            ResetCombatLogs();
            FillLeftSlots();
            FillRightSlots();
            CombatManager.Instance.newCombat.Fight();
        }
    }
    public void CloseCombatUI() {
        GameManager.Instance.SetPausedState(false);
        combatGO.SetActive(false);
    }
    public void FillLeftSlots() {
        for (int i = 0; i < CombatManager.Instance.newCombat.leftSide.slots.Length; i++) {
            leftSlots[i].SetCharacter(CombatManager.Instance.newCombat.leftSide.slots[i].character);
        }
    }
    public void FillRightSlots() {
        for (int i = 0; i < CombatManager.Instance.newCombat.rightSide.slots.Length; i++) {
            rightSlots[i].SetCharacter(CombatManager.Instance.newCombat.rightSide.slots[i].character);
        }
    }

    public void AddCombatLogs(string text, SIDES side) {
        if (!combatGO.activeSelf) {
            return;
        }
        GameObject go = Instantiate(combatLogItemPrefab, combatLogsScrollView.content);
        go.GetComponent<CombatLogItem>().SetLog(text, side);
        //resultsText.text += "\n" + text;
    }
    private void ResetCombatLogs() {
        combatLogsScrollView.content.DestroyChildren();
    }
    public void HighlightAttacker(Character character, SIDES side) {
        if (!combatGO.activeSelf) {
            return;
        }
        if(side == SIDES.A) {
            for (int i = 0; i < leftSlots.Length; i++) {
                if(leftSlots[i].character != null && leftSlots[i].character.id == character.id) {
                    leftSlots[i].SetHighlight(true);
                }
            }
        } else {
            for (int i = 0; i < rightSlots.Length; i++) {
                if (rightSlots[i].character != null && rightSlots[i].character.id == character.id) {
                    rightSlots[i].SetHighlight(true);
                }
            }
        }
    }
    public void UnhighlightAttacker(Character character, SIDES side) {
        if (!combatGO.activeSelf) {
            return;
        }
        if (side == SIDES.A) {
            for (int i = 0; i < leftSlots.Length; i++) {
                if (leftSlots[i].character != null && leftSlots[i].character.id == character.id) {
                    leftSlots[i].SetHighlight(false);
                }
            }
        } else {
            for (int i = 0; i < rightSlots.Length; i++) {
                if (rightSlots[i].character != null && rightSlots[i].character.id == character.id) {
                    rightSlots[i].SetHighlight(false);
                }
            }
        }
    }
    public bool CanSlotBeTarget(CombatSlotItem combatSlotItem) {
        return CombatManager.Instance.newCombat.isSelectingTarget && 
            CombatManager.Instance.newCombat.currentAttacker.side != combatSlotItem.side && combatSlotItem.character != null;
    }
    public void ShowTargetCharacters(CombatSlotItem combatSlotItem) {
        List<int> targetIndexes = CombatManager.Instance.newCombat.GetTargetIndexesForCurrentAttackByIndex(combatSlotItem.gridNumber);
        if (targetIndexes.Contains(combatSlotItem.gridNumber)) {
            if(combatSlotItem.side == SIDES.A) {
                for (int i = 0; i < targetIndexes.Count; i++) {
                    leftSlots[targetIndexes[i]].SetTargetable(true);
                }
            } else {
                for (int i = 0; i < targetIndexes.Count; i++) {
                    rightSlots[targetIndexes[i]].SetTargetable(true);
                }
            }
        }
    }
    public void SelectTargetCharacters(CombatSlotItem combatSlotItem) {
        List<CombatCharacter> targetCharacters = new List<CombatCharacter>();
        if (combatSlotItem.side == SIDES.A) {
            for (int i = 0; i < leftSlots.Length; i++) {
                if (leftSlots[i].character != null && leftSlots[i].isTargetable) {
                    targetCharacters.Add(leftSlots[i].character.currentCombatCharacter);
                }
            }
        } else {
            for (int i = 0; i < rightSlots.Length; i++) {
                if (rightSlots[i].character != null && rightSlots[i].isTargetable) {
                    targetCharacters.Add(rightSlots[i].character.currentCombatCharacter);
                }
            }
        }
        CombatManager.Instance.newCombat.OnSelectTargets(targetCharacters);
    }
    public void HideTargetCharacters(CombatSlotItem combatSlotItem) {
        if (combatSlotItem.side == SIDES.A) {
            for (int i = 0; i < leftSlots.Length; i++) {
                leftSlots[i].SetTargetable(false);
            }
        } else {
            for (int i = 0; i < rightSlots.Length; i++) {
                rightSlots[i].SetTargetable(false);
            }
        }
    }
    public void UpdateCombatSlotItems() {
        if (!combatGO.activeSelf) {
            return;
        }
        FillLeftSlots();
        FillRightSlots();
    }
    public void OnClickPassSelectionOfTargets() {
        CombatManager.Instance.newCombat.OnSelectTargets(null);
    }
    public void OnTogglePlay(bool state) {
        if (state) {
            CombatManager.Instance.newCombat.SetPausedState(false);
        }
    }
    public void OnTogglePause(bool state) {
        if (state) {
            CombatManager.Instance.newCombat.SetPausedState(state);
        }
    }
}
