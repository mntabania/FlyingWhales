using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParentPlayerSkillTreeUI : MonoBehaviour {
    public PlayerSkillTree skillTree;
    public PlayerSkillDetails skillDetails;
    public PlayerSkillTreeUI[] skillTreeUI;

    private PlayerSkillTreeItem currentClickedSkillTreeItem;

    public void LoadSkillTree() {
        for (int i = 0; i < skillTreeUI.Length; i++) {
            skillTreeUI[i].LoadSkillTree();
        }
    }
    public void OnClickSkillTreeButton(SPELL_TYPE skillType, PlayerSkillTreeItem skillTreeItem) {
        skillDetails.ShowPlayerSkillDetails(PlayerSkillManager.Instance.GetPlayerSpellData(skillType), PlayerSkillManager.Instance.GetPlayerSkillData<PlayerSkillData>(skillType), skillTree.nodes[skillType]);
        SetCurrentClickedSkillTreeItem(skillTreeItem);
    }
    public void OnLearnSkill(SPELL_TYPE skillType) {
        for (int i = 0; i < skillTreeUI.Length; i++) {
            PlayerSkillTreeUI currSkillTreeUI = skillTreeUI[i];
            if (currSkillTreeUI.skillTreeItems.ContainsKey(skillType)) {
                currSkillTreeUI.skillTreeItems[skillType].OnLearnSkill();
                OnClickSkillTreeButton(skillType, currSkillTreeUI.skillTreeItems[skillType]);
                MainMenuManager.Instance.OnUnlockPlayerSkill();

                SPELL_TYPE[] unlockedSkills = skillTree.nodes[skillType].unlockedSkills;
                if (unlockedSkills != null && unlockedSkills.Length > 0) {
                    for (int j = 0; j < unlockedSkills.Length; j++) {
                        currSkillTreeUI.skillTreeItems[unlockedSkills[j]].OnUnlockSkill();
                    }
                }
                break;
            }
        }
    }

    private void SetCurrentClickedSkillTreeItem(PlayerSkillTreeItem skillTreeItem) {
        if(currentClickedSkillTreeItem != null) {
            currentClickedSkillTreeItem.OnUnclickSkillTreeItem();
        }
        currentClickedSkillTreeItem = skillTreeItem;
        currentClickedSkillTreeItem.OnClickSkillTreeItem();
    }
}