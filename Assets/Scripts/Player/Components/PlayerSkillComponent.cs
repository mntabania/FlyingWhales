using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillComponent {
    public Player player { get; private set; }
    public List<PlayerSkillTreeNodeData> nodesData { get; protected set; }
    public List<SPELL_TYPE> spells { get; protected set; }
    public List<SPELL_TYPE> afflictions { get; protected set; }
    public List<SPELL_TYPE> playerActions { get; protected set; }
    public List<SPELL_TYPE> demonicStructuresSkills { get; protected set; }
    public List<SPELL_TYPE> minionsSkills { get; protected set; }
    public List<SPELL_TYPE> summonsSkills { get; protected set; }
    public List<Summon> summons { get; protected set; }
    public bool canTriggerFlaw { get; protected set; }
    public bool canRemoveTraits { get; protected set; }

    public PlayerSkillComponent(Player player) {
        this.player = player;
        nodesData = new List<PlayerSkillTreeNodeData>();
        spells = new List<SPELL_TYPE>();
        afflictions = new List<SPELL_TYPE>();
        playerActions = new List<SPELL_TYPE>();
        demonicStructuresSkills = new List<SPELL_TYPE>();
        minionsSkills = new List<SPELL_TYPE>();
        summonsSkills = new List<SPELL_TYPE>();
        summons = new List<Summon>();
        canTriggerFlaw = true;
        canRemoveTraits = true;
    }

    #region Skill Tree
    public void AddPlayerSkillTreeNodeData(PlayerSkillTreeNodeData node) {
        nodesData.Add(node);
        SetPlayerSkillData(node);
        //TODO: Save
        //SaveManager.Instance.SaveCurrentStateOfWorld();
    }
    public bool RemovePlayerSkillTreeNodeData(PlayerSkillTreeNodeData node) {
        return nodesData.Remove(node);
    }
    public bool RemovePlayerSkillTreeNodeData(SPELL_TYPE skillType) {
        for (int i = 0; i < nodesData.Count; i++) {
            if (nodesData[i].skill == skillType) {
                nodesData.RemoveAt(i);
                return true;
            }
        }
        return false;
    }
    public void LoadPlayerSkillTreeNodeData(SaveDataPlayer save) {
        nodesData = save.learnedSkills;
        if (SaveManager.Instance.unlockAll) {
            PopulateDevModeSkills();
        } else {
            PopulateAllSkills();
        }
    }
    #endregion

    #region Utilities
    public bool CanAfflict(SPELL_TYPE type) {
        return afflictions.Contains(type);
    }
    public bool CanDoPlayerAction(SPELL_TYPE type) {
        return playerActions.Contains(type);
    }
    public bool CanBuildDemonicStructure(SPELL_TYPE type) {
        return demonicStructuresSkills.Contains(type);
    }
    public bool CanCastSpell(SPELL_TYPE type) {
        return spells.Contains(type);
    }
    #endregion

    private void PopulateDevModeSkills() {
        for (int i = 0; i < SaveManager.Instance.allSkillTrees.Length; i++) {
            PlayerSkillTree skillTree = SaveManager.Instance.allSkillTrees[i];
            foreach (KeyValuePair<SPELL_TYPE, PlayerSkillTreeNode> item in skillTree.nodes) {
                SetPlayerSkillData(item.Key, item.Value);
            }
        }
        SpellData afflict = PlayerSkillManager.Instance.GetPlayerSkillData(SPELL_TYPE.AFFLICT);
        CatergorizePlayerSkill(afflict);
        SpellData buildDemonicStructure = PlayerSkillManager.Instance.GetPlayerSkillData(SPELL_TYPE.BUILD_DEMONIC_STRUCTURE);
        CatergorizePlayerSkill(buildDemonicStructure);
    }
    private void PopulateAllSkills() {
        if (nodesData != null) {
            for (int i = 0; i < nodesData.Count; i++) {
                SetPlayerSkillData(nodesData[i]);
            }
        }
    }
    private void SetPlayerSkillData(PlayerSkillTreeNodeData node) {
        SpellData spellData = PlayerSkillManager.Instance.GetPlayerSkillData(node.skill);
        CatergorizePlayerSkill(spellData);
        spellData.SetCharges(node.charges);
        spellData.SetCooldown(node.cooldown);
        spellData.SetManaCost(node.manaCost);
    }
    private void SetPlayerSkillData(SPELL_TYPE skill, PlayerSkillTreeNode node) {
        SpellData spellData = PlayerSkillManager.Instance.GetPlayerSkillData(skill);
        CatergorizePlayerSkill(spellData);
        spellData.SetCharges(node.charges);
        spellData.SetCooldown(node.cooldown);
        spellData.SetManaCost(node.manaCost);
    }
    private void CatergorizePlayerSkill(SpellData spellData) {
        if(spellData == null) {
            Debug.Log("sdfsdf");
        }
        if (spellData.category == SPELL_CATEGORY.AFFLICTION) {
            afflictions.Add(spellData.type);
        } else if (spellData.category == SPELL_CATEGORY.DEMONIC_STRUCTURE) {
            demonicStructuresSkills.Add(spellData.type);
        } else if (spellData.category == SPELL_CATEGORY.MINION) {
            minionsSkills.Add(spellData.type);
        } else if (spellData.category == SPELL_CATEGORY.PLAYER_ACTION) {
            playerActions.Add(spellData.type);
        } else if (spellData.category == SPELL_CATEGORY.SPELL) {
            spells.Add(spellData.type);
        } else if (spellData.category == SPELL_CATEGORY.SUMMON) {
            summonsSkills.Add(spellData.type);
        }
    }
}