using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class NewMinionAbilityUI : PopupMenuBase {

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

    public void ShowNewMinionAbilityUI<T>(T objectToAdd) {
        if (PlayerUI.Instance.IsMajorUIShowing()) {
            PlayerUI.Instance.AddPendingUI(() => ShowNewMinionAbilityUI(objectToAdd));
            return;
        }
        if (!GameManager.Instance.isPaused) {
            UIManager.Instance.Pause();
            UIManager.Instance.SetSpeedTogglesState(false);
        }
        UtilityScripts.Utilities.DestroyChildren(choicesParent);
        string identifier = string.Empty;
        if (objectToAdd is CombatAbility) {
            titleText.text = "New Combat Ability";
            identifier = "combat";
        }else if(objectToAdd is PlayerSpell) {
            titleText.text = "New Spell";
            identifier = "intervention";
        }
        UpdateObjectToAdd(objectToAdd);
        for (int i = 0; i < PlayerManager.Instance.player.minions.Count; i++) {
            Minion currMinion = PlayerManager.Instance.player.minions[i];
            GameObject choiceGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(choicePrefab.name, Vector3.zero, Quaternion.identity, choicesParent);
            MinionAbilityChoiceItem item = choiceGO.GetComponent<MinionAbilityChoiceItem>();
            item.toggle.group = choiceToggleGroup;
            item.SetMinion(currMinion, identifier);
        }
        addBtn.interactable = false;
        base.Open();
    }

    private void UpdateObjectToAdd(object obj) {
        objToAdd = obj;
        if (obj is PlayerSpell) {
            PlayerSpell action = obj as PlayerSpell;
            string text = action.name;
            text += $"\nLevel: {action.level}";
            text += $"\nDescription: {action.description}";
            otaText.text = text;
            otaImage.sprite = PlayerManager.Instance.GetJobActionSprite(action.name);
        } else if (obj is CombatAbility) {
            CombatAbility ability = obj as CombatAbility;
            string text = ability.name;
            text += $"\nLevel: {ability.lvl}";
            text += $"\nDescription: {ability.description}";
            otaText.text = text;
            otaImage.sprite = PlayerManager.Instance.GetCombatAbilitySprite(ability.name);
        }
    }


    public override void Close() {
        base.Close();
        if (!PlayerUI.Instance.TryShowPendingUI()) {
            UIManager.Instance.ResumeLastProgressionSpeed(); //if no other UI was shown, unpause game
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
        } 
        //else if (objToAdd is PlayerJobAction) {
        //    PlayerJobAction ability = objToAdd as PlayerJobAction;
        //    selectedMinion.GainNewInterventionAbility(ability, true);
        //}
    }
    public void OnClickCancel() {
        Close();
    }
}
