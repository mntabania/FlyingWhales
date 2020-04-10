using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class SaveDataPlayer {
    public int exp;
    public List<PlayerSkillTreeNodeData> learnedSkills;
    public List<PlayerSkillTreeNodeData> unlockedSkills;

    public void InitializeInitialData() {
        exp = 10000;
        learnedSkills = new List<PlayerSkillTreeNodeData>();
        unlockedSkills = new List<PlayerSkillTreeNodeData>();
        for (int i = 0; i < SaveManager.Instance.allSkillTrees.Length; i++) {
            PlayerSkillTree currSkillTree = SaveManager.Instance.allSkillTrees[i];
            for (int j = 0; j < currSkillTree.initialLearnedSkills.Length; j++) {
                SPELL_TYPE node = currSkillTree.initialLearnedSkills[j];
                LearnSkill(node, currSkillTree.nodes[node]);
            }
        }

        //learnedSkills = new List<PlayerSkillTreeNodeData>() {
        //    //Ravager
        //    new PlayerSkillTreeNodeData() { skill = SPELL_TYPE.INVADE, charges = -1, cooldown = -1, manaCost = -1 },
        //    new PlayerSkillTreeNodeData() { skill = SPELL_TYPE.METEOR, charges = -1, cooldown = -1, manaCost = -1 },
        //    new PlayerSkillTreeNodeData() { skill = SPELL_TYPE.LIGHTNING, charges = -1, cooldown = -1, manaCost = -1 },
        //    new PlayerSkillTreeNodeData() { skill = SPELL_TYPE.THE_KENNEL, charges = -1, cooldown = -1, manaCost = -1 },

        //    //Puppet Master
        //    new PlayerSkillTreeNodeData() { skill = SPELL_TYPE.KLEPTOMANIA, charges = -1, cooldown = -1, manaCost = -1 },
        //    new PlayerSkillTreeNodeData() { skill = SPELL_TYPE.PYROPHOBIA, charges = -1, cooldown = -1, manaCost = -1 },
        //    new PlayerSkillTreeNodeData() { skill = SPELL_TYPE.AGITATE, charges = -1, cooldown = -1, manaCost = -1 },
        //    new PlayerSkillTreeNodeData() { skill = SPELL_TYPE.SEIZE_OBJECT, charges = -1, cooldown = -1, manaCost = -1 },

        //    //Lich
        //    new PlayerSkillTreeNodeData() { skill = SPELL_TYPE.SEIZE_MONSTER, charges = -1, cooldown = -1, manaCost = -1 },
        //    new PlayerSkillTreeNodeData() { skill = SPELL_TYPE.POISON, charges = -1, cooldown = -1, manaCost = -1 },
        //    new PlayerSkillTreeNodeData() { skill = SPELL_TYPE.SKELETON_MARAUDER, charges = -1, cooldown = -1, manaCost = -1 },
        //    new PlayerSkillTreeNodeData() { skill = SPELL_TYPE.DEMON_ENVY, charges = -1, cooldown = -1, manaCost = -1 },
        //};

        //for (int i = 0; i < allSkillTrees.Length; i++) {
        //    PlayerSkillTree currSkillTree = allSkillTrees[i];
        //    for (int j = 0; j < currSkillTree.tree.Length; j++) {
        //        PlayerSkillTreeNodeID node = currSkillTree.tree[j];
        //        PlayerSkillTreeNodeData learnedSkill = new PlayerSkillTreeNodeData() { skill = node.skillType, charges = -1, cooldown = -1, manaCost = -1 };
        //        learnedSkills.Add(learnedSkill);
        //        if(node.unlockedSkills != null && node.unlockedSkills.Length > 0) {
        //            for (int k = 0; k < node.unlockedSkills.Length; k++) {
        //                PlayerSkillTreeNodeID unlockedNode = node.unlockedSkills[k];
        //                PlayerSkillTreeNodeData unlockedSkill = new PlayerSkillTreeNodeData() { skill = unlockedNode.skillType, charges = -1, cooldown = -1, manaCost = -1 };
        //                unlockedSkills.Add(unlockedSkill);
        //            }
        //        }
        //    }
        //}
    }
    public void SetExp(int amount) {
        exp = amount;
    }
    public void AdjustExp(int amount) {
        exp += amount;
        if(exp < 0) {
            exp = 0;
        }
    }

    public void LearnSkill(SPELL_TYPE skillType, int cost) {
        PlayerSkillTreeNodeData learnedSkill = new PlayerSkillTreeNodeData() { skill = skillType, charges = -1, cooldown = -1, manaCost = -1 };
        learnedSkills.Add(learnedSkill);
        AdjustExp(-cost);
        for (int i = 0; i < SaveManager.Instance.allSkillTrees.Length; i++) {
            PlayerSkillTree skillTree = SaveManager.Instance.allSkillTrees[i];
            if (skillTree.nodes.ContainsKey(skillType)) {
                PlayerSkillTreeNode node = skillTree.nodes[skillType];
                if (node.unlockedSkills != null && node.unlockedSkills.Length > 0) {
                    for (int k = 0; k < node.unlockedSkills.Length; k++) {
                        PlayerSkillTreeNodeData unlockedSkill = new PlayerSkillTreeNodeData() { skill = node.unlockedSkills[k], charges = -1, cooldown = -1, manaCost = -1 };
                        unlockedSkills.Add(unlockedSkill);
                    }
                }
            }
        }
    }
    public void LearnSkill(SPELL_TYPE skillType, PlayerSkillTreeNode node) {
        PlayerSkillTreeNodeData learnedSkill = new PlayerSkillTreeNodeData() { skill = skillType, charges = -1, cooldown = -1, manaCost = -1 };
        learnedSkills.Add(learnedSkill);
        if (node.unlockedSkills != null && node.unlockedSkills.Length > 0) {
            for (int k = 0; k < node.unlockedSkills.Length; k++) {
                PlayerSkillTreeNodeData unlockedSkill = new PlayerSkillTreeNodeData() { skill = node.unlockedSkills[k], charges = -1, cooldown = -1, manaCost = -1 };
                unlockedSkills.Add(unlockedSkill);
            }
        }
    }
    public bool IsSkillLearned(SPELL_TYPE skillType) {
        if (learnedSkills != null) {
            for (int i = 0; i < learnedSkills.Count; i++) {
                PlayerSkillTreeNodeData nodeData = learnedSkills[i];
                if (nodeData.skill == skillType) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool IsSkillUnlocked(SPELL_TYPE skillType) {
        if (unlockedSkills != null) {
            for (int i = 0; i < unlockedSkills.Count; i++) {
                PlayerSkillTreeNodeData nodeData = unlockedSkills[i];
                if (nodeData.skill == skillType) {
                    return true;
                }
            }
        }
        return false;
    }
}
