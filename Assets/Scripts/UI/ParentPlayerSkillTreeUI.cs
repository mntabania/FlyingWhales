using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParentPlayerSkillTreeUI : MonoBehaviour {
    public PlayerSkillTree skillTree;
    public PlayerSkillDetails skillDetails;
    public PlayerSkillTreeUI[] skillTreeUI;

    public void LoadSkillTree() {
        for (int i = 0; i < skillTreeUI.Length; i++) {
            skillTreeUI[i].LoadSkillTree();
        }
    }
    public void OnClickSkillTreeButton(SPELL_TYPE skillType) {
        skillDetails.ShowPlayerSkillDetails(PlayerSkillManager.Instance.GetPlayerSkillData(skillType), skillTree.nodes[skillType]);
    }
    public void OnLearnSkill(SPELL_TYPE skillType) {
        for (int i = 0; i < skillTreeUI.Length; i++) {
            PlayerSkillTreeUI currSkillTreeUI = skillTreeUI[i];
            if (currSkillTreeUI.skillTreeItems.ContainsKey(skillType)) {
                currSkillTreeUI.skillTreeItems[skillType].OnLearnSkill();
                OnClickSkillTreeButton(skillType);
                MainMenuManager.Instance.OnUnlockPlayerSkill();
                break;
            }
        }
    }
}