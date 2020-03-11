using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpUI : PopupMenuBase {

    [Header("Minion")]
    [SerializeField] private CharacterPortrait minionPortrait;
    [SerializeField] private TextMeshProUGUI minionText;

    [Header("Choices")]
    [SerializeField] private RectTransform choicesParent;
    [SerializeField] private GameObject choicePrefab;
    [SerializeField] private ToggleGroup choiceToggleGroup;

    [Header("Other")]
    [SerializeField] private Button levelUpBtn;
    [SerializeField] private TextMeshProUGUI titleText;

    private System.Action<object, object> onClickOk; //object1 is object to replace and object2 is objectToAdd
    private System.Action<object> onClickCancel; //object is the rejected object

    private object selectedObj;
    private Minion minionToLevelUp;


    //string identifierToLevelUp = this identifies what to level up for the particular minion, whether it's Combat Ability, Intervention Ability, Summon, or Artifact
    //if it is a Summon or Artifact, since it is not attached to a minion, load all player summons or artifacts
    public void ShowLevelUpUI(Minion minionToLevelUp, string identifierToLevelUp) {
        if (PlayerUI.Instance.IsMajorUIShowing()) {
            PlayerUI.Instance.AddPendingUI(() => ShowLevelUpUI(minionToLevelUp, identifierToLevelUp));
            return;
        }
        if (!GameManager.Instance.isPaused) {
            UIManager.Instance.Pause();
            UIManager.Instance.SetSpeedTogglesState(false);
        }
        UtilityScripts.Utilities.DestroyChildren(choicesParent);
        UpdateMinionToLevelUp(minionToLevelUp, identifierToLevelUp);

        List<object> choices = new List<object>();
        if(identifierToLevelUp.ToLower() == "combat ability") {
            choices.Add(minionToLevelUp.combatAbility);
        }
         //else if (identifierToLevelUp.ToLower() == "intervention ability") {
         //    for (int i = 0; i < minionToLevelUp.interventionAbilities.Length; i++) {
         //        if(minionToLevelUp.interventionAbilities[i] != null) {
         //            choices.Add(minionToLevelUp.interventionAbilities[i]);
         //        }
         //    }
         //} 
         else if (identifierToLevelUp.ToLower() == "summon_slot") {
            for (int i = 0; i < PlayerManager.Instance.player.summons.Count; i++) {
                choices.Add(PlayerManager.Instance.player.summons[i]);
            }
        } else if (identifierToLevelUp.ToLower() == "artifact_slot") {
            for (int i = 0; i < PlayerManager.Instance.player.artifacts.Count; i++) {
                choices.Add(PlayerManager.Instance.player.artifacts[i]);
            }
        }
        for (int i = 0; i < choices.Count; i++) {
            object currItem = choices[i];
            GameObject choiceGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(choicePrefab.name, Vector3.zero, Quaternion.identity, choicesParent);
            LevelUpChoiceItem item = choiceGO.GetComponent<LevelUpChoiceItem>();
            item.toggle.group = choiceToggleGroup;
            item.SetObject(currItem, OnSelectChoice);
        }
        levelUpBtn.interactable = false;
        base.Open();
    }

    private void UpdateMinionToLevelUp(Minion minion, string identifierToLevelUp) {
        minionPortrait.gameObject.SetActive(false);
        minionText.gameObject.SetActive(false);
        titleText.gameObject.SetActive(false);
        if(minion != null) {
            minionToLevelUp = minion;
            minionPortrait.GeneratePortrait(minionToLevelUp.character);
            string text = minionToLevelUp.character.name;
            text += $"\nLvl. {minionToLevelUp.character.level} {minionToLevelUp.character.raceClassName}";
            minionText.text = text;
            minionPortrait.gameObject.SetActive(true);
            minionText.gameObject.SetActive(true);
        } else {
            if (identifierToLevelUp.ToLower() == "combat ability") {
                titleText.text = "Gain a level for a Combat Ability!";
            } else if (identifierToLevelUp.ToLower() == "intervention ability") {
                titleText.text = "Gain a level for a Spell!";
            } else if (identifierToLevelUp.ToLower() == "summon_slot") {
                titleText.text = "Unlock or Level Up a Summon Slot!";
            } else if (identifierToLevelUp.ToLower() == "artifact_slot") {
                titleText.text = "Unlock or Level Up an Artifact Slot!";
            }
            titleText.gameObject.SetActive(true);
        }
    }


    public override void Close() {
        base.Close();
        if (!PlayerUI.Instance.TryShowPendingUI()) {
            UIManager.Instance.ResumeLastProgressionSpeed(); //if no other UI was shown, unpause game
        }
        
    }

    private void OnSelectChoice(object obj) {
        levelUpBtn.interactable = true;
        selectedObj = obj;
    }

    public void OnClickLevelUp() {
        if(selectedObj != null) {
            if (selectedObj is CombatAbility) {
                (selectedObj as CombatAbility).LevelUp();
            } else if (selectedObj is PlayerSpell) {
                (selectedObj as PlayerSpell).LevelUp();
            } else if (selectedObj is SummonSlot) {
                SummonSlot summonSlot = selectedObj as SummonSlot;
                //if (summonSlot.isLocked) {
                //    PlayerManager.Instance.player.IncreaseSummonSlot();
                //} else {
                    summonSlot.LevelUp();
                //}
            } else if (selectedObj is ArtifactSlot) {
                ArtifactSlot artifactSlot = selectedObj as ArtifactSlot;
                //if (artifactSlot.isLocked) {
                //    PlayerManager.Instance.player.IncreaseArtifactSlot();
                //} else {
                    artifactSlot.LevelUp();
                //}
            }
        }
        Close();
    }
    public void OnClickCancel() {
        Close();
    }
}
