using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Quests;
using Tutorial;

[System.Serializable]
public class SaveDataPlayer {
    public string gameVersion;
    public int exp;
    //public List<PlayerSkillDataCopy> learnedSkills;
    public List<PLAYER_SKILL_TYPE> learnedSkills;
    public List<PLAYER_SKILL_TYPE> unlockedSkills;
    //public List<SaveDataSummon> kennelSummons;
    public List<SaveDataTileObject> cryptTileObjects;
    public List<TutorialManager.Tutorial> completedBonusTutorials;
    public List<QuestManager.Special_Popup> completedSpecialPopups;
    public List<WorldSettingsData.World_Type> unlockedWorlds;

    //Loadouts
    public LoadoutSaveData ravagerLoadoutSaveData;
    public LoadoutSaveData puppetmasterLoadoutSaveData;
    public LoadoutSaveData lichLoadoutSaveData;

    public bool moreLoadoutOptions;

    public void InitializeInitialData() {
        gameVersion = Application.version;
        exp = 10000;
        //learnedSkills = new List<PlayerSkillDataCopy>();
        learnedSkills = new List<PLAYER_SKILL_TYPE>();
        unlockedSkills = new List<PLAYER_SKILL_TYPE>();
        for (int i = 0; i < PlayerSkillManager.Instance.allSkillTrees.Length; i++) {
            PlayerSkillTree currSkillTree = PlayerSkillManager.Instance.allSkillTrees[i];
            for (int j = 0; j < currSkillTree.initialLearnedSkills.Length; j++) {
                PLAYER_SKILL_TYPE node = currSkillTree.initialLearnedSkills[j];
                if (PlayerSkillManager.Instance.GetPlayerSkillData<PlayerSkillData>(node) != null) {
                    LearnSkill(node, currSkillTree.nodes[node]);    
                }
            }
        }

        ravagerLoadoutSaveData = new LoadoutSaveData();
        puppetmasterLoadoutSaveData = new LoadoutSaveData();
        lichLoadoutSaveData = new LoadoutSaveData();
        //PlayerSkillTreeNodeData afflict = new PlayerSkillTreeNodeData() { skill = SPELL_TYPE.AFFLICT, charges = -1, cooldown = -1, manaCost = -1 };
        //learnedSkills.Add(afflict);
        //PlayerSkillTreeNodeData buildDemonicStructure = new PlayerSkillTreeNodeData() { skill = SPELL_TYPE.BUILD_DEMONIC_STRUCTURE, charges = -1, cooldown = -1, manaCost = -1 };
        //learnedSkills.Add(buildDemonicStructure);
        //PlayerSkillTreeNodeData breedMonster = new PlayerSkillTreeNodeData() { skill = SPELL_TYPE.BREED_MONSTER, charges = -1, cooldown = 48, manaCost = 10 };
        //learnedSkills.Add(breedMonster);
        InitializeTutorialData();
        completedSpecialPopups = new List<QuestManager.Special_Popup>();
        InitializeUnlockedWorlds();
    }
    /// <summary>
    /// Is this save data essentially empty. aka. everything is at default settings.
    /// NOTE: This is used for checking if reset progress popup should be shown to player
    /// when the new game button has been clicked.
    /// </summary>
    /// <returns>True or false.</returns>
    public bool IsDefault() {
        if (unlockedWorlds.Count > 2) {
            return false;
        }
        if (completedBonusTutorials.Count > 0) {
            return false;
        }
        if (completedSpecialPopups.Count > 0) {
            return false;
        }
        return true;
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
    public void LearnSkill(PLAYER_SKILL_TYPE skillType, PlayerSkillTreeNode node) {
        PlayerSkillData skillData = PlayerSkillManager.Instance.GetPlayerSkillData<PlayerSkillData>(skillType);
        AdjustExp(-skillData.expCost);
        learnedSkills.Add(skillType);
        //PlayerSkillDataCopy learnedSkill = new PlayerSkillDataCopy() { skill = skillType, charges = node.charges, cooldown = node.cooldown, manaCost = node.manaCost, threat = node.threat, threatPerHour = node.threatPerHour };
        //learnedSkills.Add(learnedSkill);

        //PlayerSkillTreeNode learnedNode = PlayerSkillManager.Instance.GetPlayerSkillTreeNode(skillType);
        if (node.unlockedSkills != null && node.unlockedSkills.Length > 0) {
            for (int k = 0; k < node.unlockedSkills.Length; k++) {
                PLAYER_SKILL_TYPE unlockedSkillType = node.unlockedSkills[k];
                //PlayerSkillTreeNode unlockedNode = PlayerSkillManager.Instance.GetPlayerSkillTreeNode(unlockedSkillType); //skillTree.nodes[unlockedSkillType];
                //PlayerSkillTreeNodeData unlockedSkill = new PlayerSkillTreeNodeData() { skill = unlockedSkillType, charges = unlockedNode.charges, cooldown = unlockedNode.cooldown, manaCost = unlockedNode.manaCost, threat = unlockedNode.threat, threatPerHour = unlockedNode.threatPerHour };
                unlockedSkills.Add(unlockedSkillType);
            }
        }
    }
    public bool IsSkillLearned(PLAYER_SKILL_TYPE skillType) {
        if (learnedSkills != null) {
            for (int i = 0; i < learnedSkills.Count; i++) {
                //PlayerSkillDataCopy nodeData = learnedSkills[i];
                if (learnedSkills[i] == skillType) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool IsSkillUnlocked(PLAYER_SKILL_TYPE skillType) {
        if (unlockedSkills != null) {
            for (int i = 0; i < unlockedSkills.Count; i++) {
                //PlayerSkillTreeNodeData nodeData = unlockedSkills[i];
                if (unlockedSkills[i] == skillType) {
                    return true;
                }
            }
        }
        return false;
    }
    #endregion

    //#region Summons
    //public void SaveSummons(List<Summon> summons) {
    //    if(kennelSummons == null) {
    //        kennelSummons = new List<SaveDataSummon>();
    //    }
    //    for (int i = 0; i < summons.Count; i++) {
    //        kennelSummons.Add(new SaveDataSummon(summons[i]));
    //    }
    //}
    //public void RemoveKennelSummon(Summon summon) {
    //    for (int i = 0; i < kennelSummons.Count; i++) {
    //        SaveDataSummon summonData = kennelSummons[i];
    //        if(summonData.className == summon.characterClass.className
    //            && summonData.summonType == summon.summonType
    //            && summonData.firstName == summon.firstName
    //            && summonData.surName == summon.surName) {
    //            kennelSummons.RemoveAt(i);
    //            break;
    //        }
    //    }
    //}
    //#endregion

    //#region Tile Objects
    //public void SaveTileObjects(List<TileObject> tileObjects) {
    //    if (cryptTileObjects == null) {
    //        cryptTileObjects = new List<SaveDataTileObject>();
    //    }
    //    for (int i = 0; i < tileObjects.Count; i++) {
    //        SaveDataTileObject saveDataTileObject = new SaveDataTileObject();
    //        saveDataTileObject.Save(tileObjects[i]);
    //        cryptTileObjects.Add(saveDataTileObject);
    //    }
    //}
    //#endregion

    #region Tutorials
    public void InitializeTutorialData() {
        completedBonusTutorials = new List<TutorialManager.Tutorial>();
    }
    public void AddBonusTutorialAsCompleted(TutorialManager.Tutorial tutorial) {
        if (!completedBonusTutorials.Contains(tutorial)) {
            completedBonusTutorials.Add(tutorial);    
        }
    }
    public void ResetBonusTutorialProgress() {
        if (completedBonusTutorials == null) {
            InitializeTutorialData();
        } else {
            completedBonusTutorials.Clear();    
        }
        
    }
    #endregion

    #region Loadout
    public LoadoutSaveData GetLoadout(PLAYER_ARCHETYPE archetype) {
        if(archetype == PLAYER_ARCHETYPE.Ravager) {
            return ravagerLoadoutSaveData;
        } else if (archetype == PLAYER_ARCHETYPE.Puppet_Master) {
            return puppetmasterLoadoutSaveData;
        } else if (archetype == PLAYER_ARCHETYPE.Lich) {
            return lichLoadoutSaveData;
        }
        return null;
    }
    public void SaveLoadoutExtraSpells(PLAYER_ARCHETYPE archetype, List<SkillSlotItem> slotItems) {
        if (archetype == PLAYER_ARCHETYPE.Ravager) {
            for (int i = 0; i < slotItems.Count; i++) {
                SkillSlotItem slotItem = slotItems[i];
                if(slotItem.skillData != null) {
                    ravagerLoadoutSaveData.AddExtraSpell(slotItem.skillData.skill);
                }
            }
        } else if (archetype == PLAYER_ARCHETYPE.Puppet_Master) {
            for (int i = 0; i < slotItems.Count; i++) {
                SkillSlotItem slotItem = slotItems[i];
                if (slotItem.skillData != null) {
                    puppetmasterLoadoutSaveData.AddExtraSpell(slotItem.skillData.skill);
                }
            }
        } else if (archetype == PLAYER_ARCHETYPE.Lich) {
            for (int i = 0; i < slotItems.Count; i++) {
                SkillSlotItem slotItem = slotItems[i];
                if (slotItem.skillData != null) {
                    lichLoadoutSaveData.AddExtraSpell(slotItem.skillData.skill);
                }
            }
        }
    }
    public void SaveLoadoutExtraAfflictions(PLAYER_ARCHETYPE archetype, List<SkillSlotItem> slotItems) {
        if (archetype == PLAYER_ARCHETYPE.Ravager) {
            for (int i = 0; i < slotItems.Count; i++) {
                SkillSlotItem slotItem = slotItems[i];
                if (slotItem.skillData != null) {
                    ravagerLoadoutSaveData.AddExtraAffliction(slotItem.skillData.skill);
                }
            }
        } else if (archetype == PLAYER_ARCHETYPE.Puppet_Master) {
            for (int i = 0; i < slotItems.Count; i++) {
                SkillSlotItem slotItem = slotItems[i];
                if (slotItem.skillData != null) {
                    puppetmasterLoadoutSaveData.AddExtraAffliction(slotItem.skillData.skill);
                }
            }
        } else if (archetype == PLAYER_ARCHETYPE.Lich) {
            for (int i = 0; i < slotItems.Count; i++) {
                SkillSlotItem slotItem = slotItems[i];
                if (slotItem.skillData != null) {
                    lichLoadoutSaveData.AddExtraAffliction(slotItem.skillData.skill);
                }
            }
        }
    }
    public void SaveLoadoutExtraMinions(PLAYER_ARCHETYPE archetype, List<SkillSlotItem> slotItems) {
        if (archetype == PLAYER_ARCHETYPE.Ravager) {
            for (int i = 0; i < slotItems.Count; i++) {
                SkillSlotItem slotItem = slotItems[i];
                if (slotItem.skillData != null) {
                    ravagerLoadoutSaveData.AddExtraMinion(slotItem.skillData.skill);
                }
            }
        } else if (archetype == PLAYER_ARCHETYPE.Puppet_Master) {
            for (int i = 0; i < slotItems.Count; i++) {
                SkillSlotItem slotItem = slotItems[i];
                if (slotItem.skillData != null) {
                    puppetmasterLoadoutSaveData.AddExtraMinion(slotItem.skillData.skill);
                }
            }
        } else if (archetype == PLAYER_ARCHETYPE.Lich) {
            for (int i = 0; i < slotItems.Count; i++) {
                SkillSlotItem slotItem = slotItems[i];
                if (slotItem.skillData != null) {
                    lichLoadoutSaveData.AddExtraMinion(slotItem.skillData.skill);
                }
            }
        }
    }
    public void SaveLoadoutExtraStructures(PLAYER_ARCHETYPE archetype, List<SkillSlotItem> slotItems) {
        if (archetype == PLAYER_ARCHETYPE.Ravager) {
            for (int i = 0; i < slotItems.Count; i++) {
                SkillSlotItem slotItem = slotItems[i];
                if (slotItem.skillData != null) {
                    ravagerLoadoutSaveData.AddExtraStructure(slotItem.skillData.skill);
                }
            }
        } else if (archetype == PLAYER_ARCHETYPE.Puppet_Master) {
            for (int i = 0; i < slotItems.Count; i++) {
                SkillSlotItem slotItem = slotItems[i];
                if (slotItem.skillData != null) {
                    puppetmasterLoadoutSaveData.AddExtraStructure(slotItem.skillData.skill);
                }
            }
        } else if (archetype == PLAYER_ARCHETYPE.Lich) {
            for (int i = 0; i < slotItems.Count; i++) {
                SkillSlotItem slotItem = slotItems[i];
                if (slotItem.skillData != null) {
                    lichLoadoutSaveData.AddExtraStructure(slotItem.skillData.skill);
                }
            }
        }
    }
    public void SaveLoadoutExtraMiscs(PLAYER_ARCHETYPE archetype, List<SkillSlotItem> slotItems) {
        if (archetype == PLAYER_ARCHETYPE.Ravager) {
            for (int i = 0; i < slotItems.Count; i++) {
                SkillSlotItem slotItem = slotItems[i];
                if (slotItem.skillData != null) {
                    ravagerLoadoutSaveData.AddExtraMisc(slotItem.skillData.skill);
                }
            }
        } else if (archetype == PLAYER_ARCHETYPE.Puppet_Master) {
            for (int i = 0; i < slotItems.Count; i++) {
                SkillSlotItem slotItem = slotItems[i];
                if (slotItem.skillData != null) {
                    puppetmasterLoadoutSaveData.AddExtraMisc(slotItem.skillData.skill);
                }
            }
        } else if (archetype == PLAYER_ARCHETYPE.Lich) {
            for (int i = 0; i < slotItems.Count; i++) {
                SkillSlotItem slotItem = slotItems[i];
                if (slotItem.skillData != null) {
                    lichLoadoutSaveData.AddExtraMisc(slotItem.skillData.skill);
                }
            }
        }
    }
    public List<PLAYER_SKILL_TYPE> GetLoadoutExtraSpells(PLAYER_ARCHETYPE archetype) {
        if (archetype == PLAYER_ARCHETYPE.Ravager) {
            return ravagerLoadoutSaveData.extraSpells;
        } else if (archetype == PLAYER_ARCHETYPE.Puppet_Master) {
            return puppetmasterLoadoutSaveData.extraSpells;
        } else if (archetype == PLAYER_ARCHETYPE.Lich) {
            return lichLoadoutSaveData.extraSpells;
        }
        return null;
    }
    public List<PLAYER_SKILL_TYPE> GetLoadoutExtraAfflictions(PLAYER_ARCHETYPE archetype) {
        if (archetype == PLAYER_ARCHETYPE.Ravager) {
            return ravagerLoadoutSaveData.extraAfflictions;
        } else if (archetype == PLAYER_ARCHETYPE.Puppet_Master) {
            return puppetmasterLoadoutSaveData.extraAfflictions;
        } else if (archetype == PLAYER_ARCHETYPE.Lich) {
            return lichLoadoutSaveData.extraAfflictions;
        }
        return null;
    }
    public List<PLAYER_SKILL_TYPE> GetLoadoutExtraMinions(PLAYER_ARCHETYPE archetype) {
        if (archetype == PLAYER_ARCHETYPE.Ravager) {
            return ravagerLoadoutSaveData.extraMinions;
        } else if (archetype == PLAYER_ARCHETYPE.Puppet_Master) {
            return puppetmasterLoadoutSaveData.extraMinions;
        } else if (archetype == PLAYER_ARCHETYPE.Lich) {
            return lichLoadoutSaveData.extraMinions;
        }
        return null;
    }
    public List<PLAYER_SKILL_TYPE> GetLoadoutExtraStructures(PLAYER_ARCHETYPE archetype) {
        if (archetype == PLAYER_ARCHETYPE.Ravager) {
            return ravagerLoadoutSaveData.extraStructures;
        } else if (archetype == PLAYER_ARCHETYPE.Puppet_Master) {
            return puppetmasterLoadoutSaveData.extraStructures;
        } else if (archetype == PLAYER_ARCHETYPE.Lich) {
            return lichLoadoutSaveData.extraStructures;
        }
        return null;
    }
    public List<PLAYER_SKILL_TYPE> GetLoadoutExtraMiscs(PLAYER_ARCHETYPE archetype) {
        if (archetype == PLAYER_ARCHETYPE.Ravager) {
            return ravagerLoadoutSaveData.extraMiscs;
        } else if (archetype == PLAYER_ARCHETYPE.Puppet_Master) {
            return puppetmasterLoadoutSaveData.extraMiscs;
        } else if (archetype == PLAYER_ARCHETYPE.Lich) {
            return lichLoadoutSaveData.extraMiscs;
        }
        return null;
    }
    public void ClearLoadoutSaveData(PLAYER_ARCHETYPE archetype) {
        if (archetype == PLAYER_ARCHETYPE.Ravager) {
            ravagerLoadoutSaveData.ClearExtraSpells();
            ravagerLoadoutSaveData.ClearExtraAfflictions();
            ravagerLoadoutSaveData.ClearExtraMinions();
            ravagerLoadoutSaveData.ClearExtraStructures();
            ravagerLoadoutSaveData.ClearExtraMiscs();
        } else if (archetype == PLAYER_ARCHETYPE.Puppet_Master) {
            puppetmasterLoadoutSaveData.ClearExtraSpells();
            puppetmasterLoadoutSaveData.ClearExtraAfflictions();
            puppetmasterLoadoutSaveData.ClearExtraMinions();
            puppetmasterLoadoutSaveData.ClearExtraStructures();
            puppetmasterLoadoutSaveData.ClearExtraMiscs();
        } else if (archetype == PLAYER_ARCHETYPE.Lich) {
            lichLoadoutSaveData.ClearExtraSpells();
            lichLoadoutSaveData.ClearExtraAfflictions();
            lichLoadoutSaveData.ClearExtraMinions();
            lichLoadoutSaveData.ClearExtraStructures();
            lichLoadoutSaveData.ClearExtraMiscs();
        }
    }
    public void SetMoreLoadoutOptions(bool state) {
        moreLoadoutOptions = state;
    }
    #endregion

    #region Special Popups
    public void AddSpecialPopupAsCompleted(QuestManager.Special_Popup popup) {
        completedSpecialPopups.Add(popup);
    }
    public void ResetSpecialPopupsProgress() {
        if (completedSpecialPopups == null) {
            completedSpecialPopups = new List<QuestManager.Special_Popup>();
        } else {
            completedSpecialPopups.Clear();    
        }
    }
    #endregion

    #region Unlocked Worlds
    private void InitializeUnlockedWorlds() {
        unlockedWorlds = new List<WorldSettingsData.World_Type>() { WorldSettingsData.World_Type.Tutorial, WorldSettingsData.World_Type.Oona };
    }
    public void UnlockWorld(WorldSettingsData.World_Type worldType) {
        if (!unlockedWorlds.Contains(worldType)) {
            unlockedWorlds.Add(worldType);
        }
    }
    public bool IsWorldUnlocked(WorldSettingsData.World_Type worldType) {
        return unlockedWorlds.Contains(worldType);
    }
    public void OnWorldCompleted(WorldSettingsData.World_Type worldType) {
        switch (worldType) {
            case WorldSettingsData.World_Type.Oona:
                UnlockWorld(WorldSettingsData.World_Type.Icalawa);
                UnlockWorld(WorldSettingsData.World_Type.Pangat_Loo);
                break;
            case WorldSettingsData.World_Type.Icalawa:
            case WorldSettingsData.World_Type.Pangat_Loo:
                UnlockWorld(WorldSettingsData.World_Type.Affatt);
                UnlockWorld(WorldSettingsData.World_Type.Custom);
                break;
            case WorldSettingsData.World_Type.Affatt:
                UnlockWorld(WorldSettingsData.World_Type.Zenko);
                UnlockWorld(WorldSettingsData.World_Type.Aneem);
                UnlockWorld(WorldSettingsData.World_Type.Pitto);
                break;
            case WorldSettingsData.World_Type.Zenko:
                UnlockWorld(WorldSettingsData.World_Type.Aneem);
                UnlockWorld(WorldSettingsData.World_Type.Pitto);    
                break;
        }
        SaveManager.Instance.savePlayerManager.SavePlayerData();
    }
    #endregion

    #region Loading
    public void ProcessOnLoad() {
        gameVersion = Application.version;
        if (unlockedWorlds == null) {
            InitializeUnlockedWorlds();
        }
    }
    #endregion
}

[System.Serializable]
public class LoadoutSaveData {
    public List<PLAYER_SKILL_TYPE> extraSpells;
    public List<PLAYER_SKILL_TYPE> extraAfflictions;
    public List<PLAYER_SKILL_TYPE> extraMinions;
    public List<PLAYER_SKILL_TYPE> extraStructures;
    public List<PLAYER_SKILL_TYPE> extraMiscs;

    public LoadoutSaveData() {
        extraSpells = new List<PLAYER_SKILL_TYPE>();
        extraAfflictions = new List<PLAYER_SKILL_TYPE>();
        extraMinions = new List<PLAYER_SKILL_TYPE>();
        extraStructures = new List<PLAYER_SKILL_TYPE>();
        extraMiscs = new List<PLAYER_SKILL_TYPE>();
    }

    public void AddExtraSpell(PLAYER_SKILL_TYPE skillType) {
        extraSpells.Add(skillType);
    }
    public void AddExtraAffliction(PLAYER_SKILL_TYPE skillType) {
        extraAfflictions.Add(skillType);
    }
    public void AddExtraMinion(PLAYER_SKILL_TYPE skillType) {
        extraMinions.Add(skillType);
    }
    public void AddExtraStructure(PLAYER_SKILL_TYPE skillType) {
        extraStructures.Add(skillType);
    }
    public void AddExtraMisc(PLAYER_SKILL_TYPE skillType) {
        extraMiscs.Add(skillType);
    }

    public void ClearExtraSpells() {
        extraSpells.Clear();
    }
    public void ClearExtraAfflictions() {
        extraAfflictions.Clear();
    }
    public void ClearExtraMinions() {
        extraMinions.Clear();
    }
    public void ClearExtraStructures() {
        extraStructures.Clear();
    }
    public void ClearExtraMiscs() {
        extraMiscs.Clear();
    }
}