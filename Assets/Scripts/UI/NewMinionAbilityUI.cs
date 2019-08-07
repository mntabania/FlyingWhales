﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class NewMinionAbilityUI : MonoBehaviour {

    [Header("Object To Add")]
    [SerializeField] private Image otaImage;
    [SerializeField] private TextMeshProUGUI otaText;

    [Header("Choices")]
    [SerializeField] private RectTransform choicesParent;
    [SerializeField] private GameObject choicePrefab;
    [SerializeField] private ToggleGroup choiceToggleGroup;

    [Header("Other")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Button addBtn;

    public Minion selectedMinion { get; private set; }
    private object objToAdd;

    private List<System.Action> pendingReplaceActions = new List<System.Action>();

    public void ShowNewMinionAbilityUI<T>(T objectToAdd) {
        if (this.gameObject.activeInHierarchy) {
            pendingReplaceActions.Add(() => ShowNewMinionAbilityUI(objectToAdd));
            return;
        }
        Utilities.DestroyChildren(choicesParent);
        string identifier = string.Empty;
        if (objectToAdd is CombatAbility) {
            titleText.text = "New Combat Ability";
            identifier = "combat";
        }else if(objectToAdd is PlayerJobAction) {
            titleText.text = "New Intervention Ability";
            identifier = "intervention";
        }
        UpdateObjectToAdd(objectToAdd);
        for (int i = 0; i < PlayerManager.Instance.player.minions.Length; i++) {
            if(PlayerManager.Instance.player.minions[i] != null) {
                Minion currMinion = PlayerManager.Instance.player.minions[i];
                GameObject choiceGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(choicePrefab.name, Vector3.zero, Quaternion.identity, choicesParent);
                MinionAbilityChoiceItem item = choiceGO.GetComponent<MinionAbilityChoiceItem>();
                item.toggle.group = choiceToggleGroup;
                item.SetMinion(currMinion, identifier);
            }
        }
        addBtn.interactable = false;
        this.gameObject.SetActive(true);
    }

    private void UpdateObjectToAdd(object obj) {
        objToAdd = obj;
        if (obj is PlayerJobAction) {
            PlayerJobAction action = obj as PlayerJobAction;
            string text = action.name;
            text += "\nLevel: " + action.level;
            text += "\nDescription: " + action.description;
            otaText.text = text;
            otaImage.sprite = PlayerManager.Instance.GetJobActionSprite(action.name);
        } else if (obj is CombatAbility) {
            CombatAbility ability = obj as CombatAbility;
            string text = ability.name;
            text += "\nLevel: " + ability.lvl;
            text += "\nDescription: " + ability.description;
            otaText.text = text;
            otaImage.sprite = PlayerManager.Instance.GetCombatAbilitySprite(ability.name);
        }
    }


    private void Close() {
        this.gameObject.SetActive(false);
        if (pendingReplaceActions.Count > 0) {
            System.Action pending = pendingReplaceActions[0];
            pendingReplaceActions.RemoveAt(0);
            pending.Invoke();
        }
    }

    public void OnSelectChoice(Minion minion) {
        addBtn.interactable = true;
        selectedMinion = minion;
    }

    public void OnClickAdd() {
        Close();
        if (objToAdd is CombatAbility) {
            CombatAbility ability = objToAdd as CombatAbility;
            selectedMinion.SetCombatAbility(ability, true);
        } else if (objToAdd is PlayerJobAction) {
            PlayerJobAction ability = objToAdd as PlayerJobAction;
            selectedMinion.AddInterventionAbility(ability, true);
        }
    }
    public void OnClickCancel() {
        Close();
    }
}
