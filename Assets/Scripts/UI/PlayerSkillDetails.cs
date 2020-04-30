using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerSkillDetails : MonoBehaviour {
    public ParentPlayerSkillTreeUI parentPlayerSkillTreeUI;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI expText;
    public TextMeshProUGUI confirmationTitleText;
    public TextMeshProUGUI categoryText;
    public TextMeshProUGUI chargesText;
    public TextMeshProUGUI manaCostText;
    public TextMeshProUGUI cooldownText;

    public Button unlockButton;
    public Sprite learnedButtonSprite;
    public Sprite notLearnedButtonSprite;
    public GameObject confirmationGO;

    private SpellData skillData;
    private PlayerSkillTreeNode skillTreeNode;
    private bool _useSkillData;

    public void ShowPlayerSkillDetails(SpellData skillData, PlayerSkillTreeNode skillTreeNode, bool useSkillData) {
        this.skillData = skillData;
        this.skillTreeNode = skillTreeNode;
        _useSkillData = useSkillData;
        UpdateData();
        gameObject.SetActive(true);
    }
    public void HidePlayerSkillDetails() {
        gameObject.SetActive(false);
    }

    private void UpdateData() {
        titleText.text = skillData.name;
        descriptionText.text = skillData.description;
        expText.text = skillTreeNode?.expCost + " XP";

        int charges = skillData.charges;
        int manaCost = skillData.manaCost;
        int cooldown = skillData.cooldown;
        if(!_useSkillData) {
            charges = skillTreeNode.charges;
            manaCost = skillTreeNode.manaCost;
            cooldown = skillTreeNode.cooldown;
        }
        categoryText.text = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(skillData.category.ToString());
        chargesText.text = "" + (charges != -1 ? charges : 0);
        manaCostText.text = "" + (manaCost != -1 ? manaCost : 0);

        string cdText = string.Empty;
        if(cooldown == -1) {
            cdText = "0 mins";
        } else {
            cdText = GameManager.GetTimeAsWholeDuration(cooldown) + " " + GameManager.GetTimeIdentifierAsWholeDuration(cooldown);
        }
        cooldownText.text = cdText;

        if (_useSkillData) {
            unlockButton.gameObject.SetActive(false);
        } else {
            SaveDataPlayer saveDataPlayer = SaveManager.Instance.currentSaveDataPlayer;
            if (saveDataPlayer.IsSkillLearned(skillData.type) || saveDataPlayer.exp < skillTreeNode.expCost) {
                SpriteState newSpriteState = new SpriteState();
                newSpriteState.highlightedSprite = unlockButton.spriteState.highlightedSprite;
                newSpriteState.pressedSprite = unlockButton.spriteState.pressedSprite;
                newSpriteState.selectedSprite = unlockButton.spriteState.selectedSprite;
                newSpriteState.disabledSprite = learnedButtonSprite;
                unlockButton.spriteState = newSpriteState;
                unlockButton.interactable = false;
                unlockButton.gameObject.SetActive(true);
            } else {
                SpriteState newSpriteState = new SpriteState();
                newSpriteState.highlightedSprite = unlockButton.spriteState.highlightedSprite;
                newSpriteState.pressedSprite = unlockButton.spriteState.pressedSprite;
                newSpriteState.selectedSprite = unlockButton.spriteState.selectedSprite;
                newSpriteState.disabledSprite = notLearnedButtonSprite;
                unlockButton.spriteState = newSpriteState;
                unlockButton.interactable = true;
                unlockButton.gameObject.SetActive(saveDataPlayer.IsSkillUnlocked(skillData.type));
            }
        }
    }

    public void OnClickUnlock() {
        confirmationTitleText.text = "Are you sure you want to unlock " + skillData.name + "?";
        confirmationGO.SetActive(true);
    }
    public void OnClickYes() {
        SaveManager.Instance.currentSaveDataPlayer.LearnSkill(skillData.type, skillTreeNode);
        parentPlayerSkillTreeUI.OnLearnSkill(skillData.type);
        confirmationGO.SetActive(false);
    }
    public void OnClickNo() {
        confirmationGO.SetActive(false);
    }
}
