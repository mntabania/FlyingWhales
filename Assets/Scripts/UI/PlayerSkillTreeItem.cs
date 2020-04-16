using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class PlayerSkillTreeItem : MonoBehaviour {
    public Sprite learnedSpriteDefault;
    public Sprite learnedSpriteHover;

    public Image buttonImg;
    public Image buttonBorderImg;
    public Button button;

    private SPELL_TYPE skillType;
    private System.Action<SPELL_TYPE, PlayerSkillTreeItem> onClick;

    public void SetData(SPELL_TYPE skillType, PlayerSkillTreeNode data, System.Action<SPELL_TYPE, PlayerSkillTreeItem> onClick) {
        this.skillType = skillType;
        this.onClick = onClick;
        SetButtonImg(data.buttonSprite);
        SetSkillState();
    }

    private void SetButtonImg(Sprite sprite) {
        buttonImg.sprite = sprite;
    }
    public void SetButtonBorderImg(Sprite sprite) {
        buttonBorderImg.sprite = sprite;
    }
    public void OnClickSkillTreeItem() {
        SetButtonBorderImg(button.spriteState.selectedSprite);
    }
    public void OnUnclickSkillTreeItem() {
        SetButtonBorderImg(button.spriteState.disabledSprite);
    }
    private void SetSkillState() {
        bool isLearned = SaveManager.Instance.currentSaveDataPlayer.IsSkillLearned(skillType);
        if (isLearned) {
            OnLearnSkill();
        } else if (SaveManager.Instance.currentSaveDataPlayer.IsSkillUnlocked(skillType)) {
            buttonImg.color = Color.white;
        } else {
            buttonImg.color = Color.gray;
        }
    }
    public void OnLearnSkill() {
        SpriteState newSpriteState = new SpriteState();
        newSpriteState.highlightedSprite = learnedSpriteHover;
        newSpriteState.pressedSprite = learnedSpriteHover;
        newSpriteState.selectedSprite = learnedSpriteHover;
        newSpriteState.disabledSprite = learnedSpriteDefault;
        button.spriteState = newSpriteState;
        SetButtonBorderImg(learnedSpriteDefault);
        buttonImg.color = Color.white;
    }
    public void OnUnlockSkill() {
        buttonImg.color = Color.white;
    }
    public void OnClickButton() {
        onClick(skillType, this);
    }
}