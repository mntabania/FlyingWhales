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

    private SpellData spellData;
    private PlayerSkillData skillData;
    private PlayerSkillTreeNode skillTreeNode;
    //private bool _useSkillData;

    public void ShowPlayerSkillDetails(SpellData spellData, PlayerSkillData skillData, PlayerSkillTreeNode skillTreeNode) {
        this.spellData = spellData;
        this.skillData = skillData;
        this.skillTreeNode = skillTreeNode;
        UpdateData();
        gameObject.SetActive(true);
    }
    public void HidePlayerSkillDetails() {
        gameObject.SetActive(false);
    }

    private void UpdateData() {
        titleText.text = spellData.name;
        descriptionText.text = spellData.description;
        expText.text = skillData.expCost + " XP";

        int charges = skillData.charges;
        int manaCost = skillData.manaCost;
        int cooldown = skillData.cooldown;

        //int charges = skillData.charges;
        //int manaCost = skillData.manaCost;
        //int cooldown = skillData.cooldown;
        //if(!_useSkillData) {
        //    charges = skillTreeNode.charges;
        //    manaCost = skillTreeNode.manaCost;
        //    cooldown = skillTreeNode.cooldown;
        //}

        categoryText.text = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(spellData.category.ToString());

        chargesText.text = "N/A";
        if (charges != -1) {
            chargesText.text = charges + "/" + charges;
        }

        manaCostText.text = "N/A";
        if (manaCost != -1) {
            manaCostText.text = "" + manaCost;
        }

        string cdText = string.Empty;
        if (cooldown == -1) {
            cdText = "N/A";
        } else {
            cdText = GameManager.GetTimeAsWholeDuration(cooldown) + " " + GameManager.GetTimeIdentifierAsWholeDuration(cooldown);
        }
        cooldownText.text = cdText;

        SaveDataPlayer saveDataPlayer = SaveManager.Instance.currentSaveDataPlayer;
        if (saveDataPlayer.IsSkillLearned(spellData.type) || saveDataPlayer.exp < skillData.expCost) {
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
            unlockButton.gameObject.SetActive(saveDataPlayer.IsSkillUnlocked(spellData.type));
        }
    }

    public void OnClickUnlock() {
        confirmationTitleText.text = "Are you sure you want to unlock " + spellData.name + "?";
        confirmationGO.SetActive(true);
    }
    public void OnClickYes() {
        SaveManager.Instance.currentSaveDataPlayer.LearnSkill(spellData.type, skillTreeNode);
        parentPlayerSkillTreeUI.OnLearnSkill(spellData.type);
        confirmationGO.SetActive(false);
    }
    public void OnClickNo() {
        confirmationGO.SetActive(false);
    }
}
