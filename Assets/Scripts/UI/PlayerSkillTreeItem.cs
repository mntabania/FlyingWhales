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
    private System.Action<SPELL_TYPE> onClick;

    public void SetData(SPELL_TYPE skillType, PlayerSkillTreeNode data, System.Action<SPELL_TYPE> onClick) {
        this.skillType = skillType;
        this.onClick = onClick;
        SetButtonImg(data.buttonSprite);
        SetSkillState();
    }

    private void SetButtonImg(Sprite sprite) {
        buttonImg.sprite = sprite;
    }
    private void SetSkillState() {
        bool isLearned = SaveManager.Instance.currentSaveDataPlayer.IsSkillLearned(skillType);
        if (isLearned) {
            OnLearnSkill();
        }
    }
    public void OnLearnSkill() {
        SpriteState newSpriteState = new SpriteState();
        newSpriteState.highlightedSprite = learnedSpriteHover;
        newSpriteState.pressedSprite = learnedSpriteHover;
        newSpriteState.selectedSprite = learnedSpriteHover;
        newSpriteState.disabledSprite = learnedSpriteDefault;
        button.spriteState = newSpriteState;
        buttonBorderImg.sprite = learnedSpriteDefault;
    }
    public void OnClickButton() {
        onClick(skillType);
    }
}