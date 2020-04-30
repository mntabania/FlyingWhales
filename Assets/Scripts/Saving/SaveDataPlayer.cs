using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Tutorial;

[System.Serializable]
public class SaveDataPlayer {
    public int exp;
    public List<PlayerSkillTreeNodeData> learnedSkills;
    public List<PlayerSkillTreeNodeData> unlockedSkills;
    public List<SaveDataSummon> kennelSummons;
    public List<SaveDataTileObject> cryptTileObjects;
    public List<TutorialManager.Tutorial> completedTutorials;
    
    public void InitializeInitialData() {
        exp = 10000;
        learnedSkills = new List<PlayerSkillTreeNodeData>();
        unlockedSkills = new List<PlayerSkillTreeNodeData>();
        for (int i = 0; i < PlayerSkillManager.Instance.allSkillTrees.Length; i++) {
            PlayerSkillTree currSkillTree = PlayerSkillManager.Instance.allSkillTrees[i];
            for (int j = 0; j < currSkillTree.initialLearnedSkills.Length; j++) {
                SPELL_TYPE node = currSkillTree.initialLearnedSkills[j];
                LearnSkill(node, currSkillTree.nodes[node]);
            }
        }
        PlayerSkillTreeNodeData afflict = new PlayerSkillTreeNodeData() { skill = SPELL_TYPE.AFFLICT, charges = -1, cooldown = -1, manaCost = -1 };
        learnedSkills.Add(afflict);
        PlayerSkillTreeNodeData buildDemonicStructure = new PlayerSkillTreeNodeData() { skill = SPELL_TYPE.BUILD_DEMONIC_STRUCTURE, charges = -1, cooldown = -1, manaCost = -1 };
        learnedSkills.Add(buildDemonicStructure);

        InitializeTutorialData();
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
    public void LearnSkill(SPELL_TYPE skillType, PlayerSkillTreeNode node) {
        AdjustExp(-node.expCost);
        PlayerSkillTreeNodeData learnedSkill = new PlayerSkillTreeNodeData() { skill = skillType, charges = node.charges, cooldown = node.cooldown, manaCost = node.manaCost };
        learnedSkills.Add(learnedSkill);

        PlayerSkillTreeNode learnedNode = PlayerSkillManager.Instance.GetPlayerSkillTreeNode(skillType);
        if (learnedNode.unlockedSkills != null && learnedNode.unlockedSkills.Length > 0) {
            for (int k = 0; k < learnedNode.unlockedSkills.Length; k++) {
                SPELL_TYPE unlockedSkillType = learnedNode.unlockedSkills[k];
                PlayerSkillTreeNode unlockedNode = PlayerSkillManager.Instance.GetPlayerSkillTreeNode(unlockedSkillType); //skillTree.nodes[unlockedSkillType];
                PlayerSkillTreeNodeData unlockedSkill = new PlayerSkillTreeNodeData() { skill = unlockedSkillType, charges = unlockedNode.charges, cooldown = unlockedNode.cooldown, manaCost = unlockedNode.manaCost };
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

    #region Tile Objects
    public void SaveTileObjects(List<TileObject> tileObjects) {
        if (cryptTileObjects == null) {
            cryptTileObjects = new List<SaveDataTileObject>();
        }
        for (int i = 0; i < tileObjects.Count; i++) {
            cryptTileObjects.Add(new SaveDataTileObject(tileObjects[i]));
        }
    }
    #endregion

    #region Tutorials
    public void InitializeTutorialData() {
        completedTutorials = new List<TutorialManager.Tutorial>();
    }
    public void AddTutorialAsCompleted(TutorialManager.Tutorial tutorial) {
        completedTutorials.Add(tutorial);
    }
    public void ResetTutorialProgress() {
        completedTutorials.Clear();
    }
    #endregion
}
