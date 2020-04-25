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
    public Button unlockButton;
    public Sprite learnedButtonSprite;
    public Sprite notLearnedButtonSprite;
    public GameObject confirmationGO;

    private SpellData skillData;
    private PlayerSkillTreeNode skillTreeNode;

    public void ShowPlayerSkillDetails(SpellData skillData, PlayerSkillTreeNode skillTreeNode) {
        this.skillData = skillData;
        this.skillTreeNode = skillTreeNode;
        UpdateData();
        gameObject.SetActive(true);
    }
    public void HidePlayerSkillDetails() {
        gameObject.SetActive(false);
    }

    private void UpdateData() {
        titleText.text = skillData.name;
        descriptionText.text = skillData.description;
        expText.text = skillTreeNode.expCost + " XP";

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
