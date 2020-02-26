using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReplaceUI : PopupMenuBase {

    [Header("Object To Add")]
    [SerializeField] private Image otaImage;
    [SerializeField] private CharacterPortrait portrait;
    [SerializeField] private TextMeshProUGUI otaText;

    [Header("Choices")]
    [SerializeField] private RectTransform choicesParent;
    [SerializeField] private GameObject choicePrefab;
    [SerializeField] private ToggleGroup choiceToggleGroup;

    [Header("Other")]
    [SerializeField] private TextMeshProUGUI newObjectLbl;
    [SerializeField] private Button replaceBtn;

    private System.Action<object, object> onClickReplace; //object1 is object to replace and object2 is objectToAdd
    private System.Action<object> onClickCancel; //object is the rejected object

    private object selectedObj;
    private object objToAdd;


    /// <summary>
    /// Show Replace Object UI.
    /// </summary>
    /// <typeparam name="T">The type of objects concerned</typeparam>
    /// <param name="choices">The objects that can be replaced.</param>
    /// <param name="objectToAdd">The object to add.</param>
    /// <param name="onClickReplace">What should happen when an object is replaced.</param>
    /// <param name="onClickCancel">What should happen when the offer was rejected.</param>
    public void ShowReplaceUI<T>(List<T> choices, T objectToAdd, System.Action<object, object> onClickReplace, System.Action<object> onClickCancel) {
        if (PlayerUI.Instance.IsMajorUIShowing()) {
            PlayerUI.Instance.AddPendingUI(() => ShowReplaceUI(choices, objectToAdd, onClickReplace, onClickCancel));
            return;
        }
        if (!GameManager.Instance.isPaused) {
            UIManager.Instance.Pause();
            UIManager.Instance.SetSpeedTogglesState(false);
        }
        UtilityScripts.Utilities.DestroyChildren(choicesParent);
        if(objectToAdd is Minion) {
            newObjectLbl.text = "New Minion!";
        } else {
            newObjectLbl.text =
                $"New {UtilityScripts.Utilities.NormalizeNoSpaceString(objectToAdd.GetType().BaseType.ToString())}!";
        }
        UpdateObjectToAdd(objectToAdd);
        for (int i = 0; i < choices.Count; i++) {
            T currItem = choices[i];
            GameObject choiceGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(choicePrefab.name, Vector3.zero, Quaternion.identity, choicesParent);
            ReplaceChoiceItem item = choiceGO.GetComponent<ReplaceChoiceItem>();
            item.toggle.group = choiceToggleGroup;
            item.SetObject(currItem, OnSelectChoice);
        }
        replaceBtn.interactable = false;
        this.onClickReplace = onClickReplace;
        this.onClickCancel = onClickCancel;
        base.Open();
    }

    private void UpdateObjectToAdd(object obj) {
        objToAdd = obj;
        otaImage.gameObject.SetActive(false);
        portrait.gameObject.SetActive(false);
        if (obj is Summon) {
            Summon summon = obj as Summon;
            otaImage.sprite = CharacterManager.Instance.GetSummonSettings(summon.summonType).summonPortrait;
            string text = $"{summon.name} ({summon.summonType.SummonName()})";
            text += $"\nLevel: {summon.level}";
            text += $"\nDescription: {PlayerManager.Instance.player.GetSummonDescription(summon.summonType)}";
            otaText.text = text;
            otaImage.gameObject.SetActive(true);
        } 
        // else if (obj is Artifact) {
        //     Artifact artifact = obj as Artifact;
        //     string text = artifact.name;
        //     text += "\nLevel: " + artifact.level.ToString();
        //     text += "\nDescription: " + PlayerManager.Instance.player.GetArtifactDescription(artifact.type);
        //     otaText.text = text;
        //     otaImage.sprite = CharacterManager.Instance.GetArtifactSettings(artifact.type).artifactPortrait;
        //     otaImage.gameObject.SetActive(true);
        // } 
        else if (obj is PlayerSpell) {
            PlayerSpell action = obj as PlayerSpell;
            string text = action.name;
            text += $"\nDescription: {action.description}";
            otaText.text = text;
            otaImage.sprite = PlayerManager.Instance.GetJobActionSprite(action.name);
            otaImage.gameObject.SetActive(true);
        } else if (obj is CombatAbility) {
            CombatAbility ability = obj as CombatAbility;
            string text = ability.name;
            text += $"\nDescription: {ability.description}";
            otaText.text = text;
            otaImage.sprite = PlayerManager.Instance.GetCombatAbilitySprite(ability.name);
            otaImage.gameObject.SetActive(true);
        } else if (obj is Minion) {
            Minion minion = obj as Minion;
            string text = minion.character.name;
            text += $"\nLvl. {minion.character.level} {minion.character.raceClassName}";
            otaText.text = text;
            portrait.GeneratePortrait(minion.character);
            portrait.gameObject.SetActive(true);
        }
    }


    public override void Close() {
        base.Close();
        if (!PlayerUI.Instance.TryShowPendingUI()) {
            UIManager.Instance.ResumeLastProgressionSpeed(); //if no other UI was shown, unpause game
        }
    }

    private void OnSelectChoice(object obj) {
        replaceBtn.interactable = true;
        selectedObj = obj;
    }

    public void OnClickReplace() {
        onClickReplace.Invoke(selectedObj, objToAdd);
        Close();
    }
    public void OnClickCancel() {
        onClickCancel.Invoke(objToAdd);
        Close();
    }
}
