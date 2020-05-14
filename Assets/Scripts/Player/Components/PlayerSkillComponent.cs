using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

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
        if (PlayerSkillManager.Instance.unlockAllSkills || WorldConfigManager.Instance.isDemoWorld) {
            PopulateDevModeSkills();
        } else {
            PopulateAllSkills();
        }
    }
    public void LoadSummons(SaveDataPlayer save) {
        if(save.kennelSummons != null && save.kennelSummons.Count > 0) {
            for (int i = 0; i < save.kennelSummons.Count; i++) {
                SaveDataSummon summonData = save.kennelSummons[i];
                Summon savedSummon = CharacterManager.Instance.CreateNewSummon(summonData, player.playerFaction);
                AddSummon(savedSummon);
            }
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
    public bool HasAnyAvailableAffliction() {
        for (int i = 0; i < afflictions.Count; i++) {
            SpellData spellData = PlayerSkillManager.Instance.GetPlayerSkillData(afflictions[i]);
            if (spellData.hasCharges == false) {
                return true;
            } else {
                //spell uses charges. Check if has any
                if (spellData.charges > 0) {
                    return true;
                }
            }
        }
        return false;
    }
    #endregion

    #region Skills
    private void PopulateDevModeSkills() {
        for (int i = 0; i < PlayerSkillManager.Instance.allSkillTrees.Length; i++) {
            PlayerSkillTree skillTree = PlayerSkillManager.Instance.allSkillTrees[i];
            foreach (KeyValuePair<SPELL_TYPE, PlayerSkillTreeNode> item in skillTree.nodes) {
                bool shouldAddSpell = true;
                if (WorldConfigManager.Instance.isDemoWorld) {
                    //if demo world, spell should be added if it is not a minion type. If it is, check if that spell is in
                    //the available set of spells for the demo. Other spells are added because in the demo, their buttons should still
                    //be seen, but instead, should not be clickable.
                    shouldAddSpell = (PlayerSkillManager.Instance.IsMinion(item.Key) == false ||
                                     WorldConfigManager.Instance.availableSpellsInDemoBuild.Contains(item.Key)) 
                                     && item.Key != SPELL_TYPE.KNOCKOUT && item.Key != SPELL_TYPE.HARASS;
                }
                if (shouldAddSpell) {
                    SetPlayerSkillData(item.Key, item.Value);
                }
            }
        }
        SpellData afflict = PlayerSkillManager.Instance.GetPlayerSkillData(SPELL_TYPE.AFFLICT);
        CategorizePlayerSkill(afflict);
        SpellData buildDemonicStructure = PlayerSkillManager.Instance.GetPlayerSkillData(SPELL_TYPE.BUILD_DEMONIC_STRUCTURE);
        CategorizePlayerSkill(buildDemonicStructure);
        SpellData torture = PlayerSkillManager.Instance.GetPlayerSkillData(SPELL_TYPE.TORTURE);
        CategorizePlayerSkill(torture);
        
        //For Demo
        if (WorldConfigManager.Instance.isDemoWorld) {
            SpellData rain = PlayerSkillManager.Instance.GetPlayerSkillData(SPELL_TYPE.RAIN);
            CategorizePlayerSkill(rain);    
        }
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
        CategorizePlayerSkill(spellData);
        spellData.SetMaxCharges(node.charges);
        spellData.SetCharges(node.charges);
        spellData.SetCooldown(node.cooldown);
        spellData.SetManaCost(node.manaCost);
        spellData.SetThreat(node.threat);
        spellData.SetThreatPerHour(node.threatPerHour);

    }
    private void SetPlayerSkillData(SPELL_TYPE skill, PlayerSkillTreeNode node) {
        SpellData spellData = PlayerSkillManager.Instance.GetPlayerSkillData(skill);
        CategorizePlayerSkill(spellData);
        spellData.SetMaxCharges(node.charges);
        spellData.SetCharges(node.charges);
        spellData.SetCooldown(node.cooldown);
        spellData.SetManaCost(node.manaCost);
        spellData.SetThreat(node.threat);
        spellData.SetThreatPerHour(node.threatPerHour);
    }
    private void CategorizePlayerSkill(SpellData spellData) {
        Assert.IsNotNull(spellData, "Given spell data in CategorizePlayerSkill is null!");
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
    #endregion

    #region Summons
    public void AddSummon(Summon summon) {
        summons.Add(summon);
    }
    public void RemoveSummon(Summon summon, bool removeOnSaveFile = false) {
        if (summons.Remove(summon)) {
            if (removeOnSaveFile) {
                SaveManager.Instance.currentSaveDataPlayer.RemoveKennelSummon(summon);
            }
            Messenger.Broadcast(Signals.SUMMON_REMOVED, summon);
        }
    }
    public string GetSummonDescription(SUMMON_TYPE currentlySelectedSummon) {
        switch (currentlySelectedSummon) {
            case SUMMON_TYPE.Wolf:
                return "Summon a wolf to run amok.";
            case SUMMON_TYPE.Skeleton:
                return "Summon a skeleton that will abduct a random character.";
            case SUMMON_TYPE.Golem:
                return "Summon a stone golem that can sustain alot of hits.";
            case SUMMON_TYPE.Succubus:
                return "Summon a succubus that will seduce a male character and eliminate him.";
            case SUMMON_TYPE.Incubus:
                return "Summon a succubus that will seduce a female character and eliminate her.";
            default:
                return
                    $"Summon a {UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(currentlySelectedSummon.ToString())}";
        }
    }
    #endregion
}