using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class SaveDataPlayer {
    public int exp;
    public List<PlayerSkillTreeNodeData> learnedSkills;
    public List<PlayerSkillTreeNodeData> unlockedSkills;
    public List<SaveDataSummon> kennelSummons;

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
        PlayerSkillTreeNodeData afflict = new PlayerSkillTreeNodeData() { skill = SPELL_TYPE.AFFLICT, charges = -1, cooldown = -1, manaCost = -1 };
        learnedSkills.Add(afflict);
        PlayerSkillTreeNodeData buildDemonicStructure = new PlayerSkillTreeNodeData() { skill = SPELL_TYPE.BUILD_DEMONIC_STRUCTURE, charges = -1, cooldown = -1, manaCost = -1 };
        learnedSkills.Add(buildDemonicStructure);
    }

    #region Exp
    public void SetExp(int amount) {
        exp = amount;
    }
    public void AdjustExp(int amount) {
        exp += amount;
        if(exp < 0) {
            exp = 0;
        }
    }
    #endregion

    #region Skills
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
    #endregion

    #region Summons
    public void SaveSummons(List<Summon> summons) {
        if(kennelSummons == null) {
            kennelSummons = new List<SaveDataSummon>();
        }
        for (int i = 0; i < summons.Count; i++) {
            kennelSummons.Add(new SaveDataSummon(summons[i]));
        }
    }
    public void RemoveKennelSummon(Summon summon) {
        for (int i = 0; i < kennelSummons.Count; i++) {
            SaveDataSummon summonData = kennelSummons[i];
            if(summonData.className == summon.characterClass.className
                && summonData.summonType == summon.summonType
                && summonData.firstName == summon.firstName
                && summonData.surName == summon.surName) {
                kennelSummons.RemoveAt(i);
                break;
            }
        }
    }
    #endregion
}
