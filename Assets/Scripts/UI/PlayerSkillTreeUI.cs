using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillTreeUI : MonoBehaviour {
    public ParentPlayerSkillTreeUI parentSkillTreeUI;
    public PlayerSkillTreeNodeItemDictionary skillTreeItems;

    public void LoadSkillTree() {
        foreach (KeyValuePair<SPELL_TYPE, PlayerSkillTreeItem> item in skillTreeItems) {
            item.Value.SetData(item.Key, parentSkillTreeUI.skillTree.nodes[item.Key], OnClickSkillTreeButton);
        }
    }

    public void OnClickSkillTreeButton(SPELL_TYPE skillType, PlayerSkillTreeItem skillTreeItem) {
        parentSkillTreeUI.OnClickSkillTreeButton(skillType, skillTreeItem);
    }
}