using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Tutorial;

[System.Serializable]
public class SaveDataPlayer {
    public int exp;
    //public List<PlayerSkillDataCopy> learnedSkills;
    public List<SPELL_TYPE> learnedSkills;
    public List<SPELL_TYPE> unlockedSkills;
    public List<SaveDataSummon> kennelSummons;
    public List<SaveDataTileObject> cryptTileObjects;
    public List<TutorialManager.Tutorial> completedTutorials;

    //Loadouts
    public LoadoutSaveData ravagerLoadoutSaveData;
    public LoadoutSaveData puppetmasterLoadoutSaveData;
    public LoadoutSaveData lichLoadoutSaveData;

    public void InitializeInitialData() {
        exp = 10000;
        //learnedSkills = new List<PlayerSkillDataCopy>();
        learnedSkills = new List<SPELL_TYPE>();
        unlockedSkills = new List<SPELL_TYPE>();
        for (int i = 0; i < PlayerSkillManager.Instance.allSkillTrees.Length; i++) {
            PlayerSkillTree currSkillTree = PlayerSkillManager.Instance.allSkillTrees[i];
            for (int j = 0; j < currSkillTree.initialLearnedSkills.Length; j++) {
                SPELL_TYPE node = currSkillTree.initialLearnedSkills[j];
                LearnSkill(node, currSkillTree.nodes[node]);
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
        PlayerSkillData skillData = PlayerSkillManager.Instance.GetPlayerSkillData<PlayerSkillData>(skillType);
        AdjustExp(-skillData.expCost);
        learnedSkills.Add(skillType);
        //PlayerSkillDataCopy learnedSkill = new PlayerSkillDataCopy() { skill = skillType, charges = node.charges, cooldown = node.cooldown, manaCost = node.manaCost, threat = node.threat, threatPerHour = node.threatPerHour };
        //learnedSkills.Add(learnedSkill);

        //PlayerSkillTreeNode learnedNode = PlayerSkillManager.Instance.GetPlayerSkillTreeNode(skillType);
        if (node.unlockedSkills != null && node.unlockedSkills.Length > 0) {
            for (int k = 0; k < node.unlockedSkills.Length; k++) {
                SPELL_TYPE unlockedSkillType = node.unlockedSkills[k];
                //PlayerSkillTreeNode unlockedNode = PlayerSkillManager.Instance.GetPlayerSkillTreeNode(unlockedSkillType); //skillTree.nodes[unlockedSkillType];
                //PlayerSkillTreeNodeData unlockedSkill = new PlayerSkillTreeNodeData() { skill = unlockedSkillType, charges = unlockedNode.charges, cooldown = unlockedNode.cooldown, manaCost = unlockedNode.manaCost, threat = unlockedNode.threat, threatPerHour = unlockedNode.threatPerHour };
                unlockedSkills.Add(unlockedSkillType);
            }
        }
    }
    public bool IsSkillLearned(SPELL_TYPE skillType) {
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
    public bool IsSkillUnlocked(SPELL_TYPE skillType) {
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
    public List<SPELL_TYPE> GetLoadoutExtraSpells(PLAYER_ARCHETYPE archetype) {
        if (archetype == PLAYER_ARCHETYPE.Ravager) {
            return ravagerLoadoutSaveData.extraSpells;
        } else if (archetype == PLAYER_ARCHETYPE.Puppet_Master) {
            return puppetmasterLoadoutSaveData.extraSpells;
        } else if (archetype == PLAYER_ARCHETYPE.Lich) {
            return lichLoadoutSaveData.extraSpells;
        }
        return null;
    }
    public List<SPELL_TYPE> GetLoadoutExtraAfflictions(PLAYER_ARCHETYPE archetype) {
        if (archetype == PLAYER_ARCHETYPE.Ravager) {
            return ravagerLoadoutSaveData.extraAfflictions;
        } else if (archetype == PLAYER_ARCHETYPE.Puppet_Master) {
            return puppetmasterLoadoutSaveData.extraAfflictions;
        } else if (archetype == PLAYER_ARCHETYPE.Lich) {
            return lichLoadoutSaveData.extraAfflictions;
        }
        return null;
    }
    public List<SPELL_TYPE> GetLoadoutExtraMinions(PLAYER_ARCHETYPE archetype) {
        if (archetype == PLAYER_ARCHETYPE.Ravager) {
            return ravagerLoadoutSaveData.extraMinions;
        } else if (archetype == PLAYER_ARCHETYPE.Puppet_Master) {
            return puppetmasterLoadoutSaveData.extraMinions;
        } else if (archetype == PLAYER_ARCHETYPE.Lich) {
            return lichLoadoutSaveData.extraMinions;
        }
        return null;
    }
    public List<SPELL_TYPE> GetLoadoutExtraStructures(PLAYER_ARCHETYPE archetype) {
        if (archetype == PLAYER_ARCHETYPE.Ravager) {
            return ravagerLoadoutSaveData.extraStructures;
        } else if (archetype == PLAYER_ARCHETYPE.Puppet_Master) {
            return puppetmasterLoadoutSaveData.extraStructures;
        } else if (archetype == PLAYER_ARCHETYPE.Lich) {
            return lichLoadoutSaveData.extraStructures;
        }
        return null;
    }
    public List<SPELL_TYPE> GetLoadoutExtraMiscs(PLAYER_ARCHETYPE archetype) {
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
    #endregion
}

[System.Serializable]
public class LoadoutSaveData {
    public List<SPELL_TYPE> extraSpells;
    public List<SPELL_TYPE> extraAfflictions;
    public List<SPELL_TYPE> extraMinions;
    public List<SPELL_TYPE> extraStructures;
    public List<SPELL_TYPE> extraMiscs;

    public LoadoutSaveData() {
        extraSpells = new List<SPELL_TYPE>();
        extraAfflictions = new List<SPELL_TYPE>();
        extraMinions = new List<SPELL_TYPE>();
        extraStructures = new List<SPELL_TYPE>();
        extraMiscs = new List<SPELL_TYPE>();
    }

    public void AddExtraSpell(SPELL_TYPE skillType) {
        extraSpells.Add(skillType);
    }
    public void AddExtraAffliction(SPELL_TYPE skillType) {
        extraAfflictions.Add(skillType);
    }
    public void AddExtraMinion(SPELL_TYPE skillType) {
        extraMinions.Add(skillType);
    }
    public void AddExtraStructure(SPELL_TYPE skillType) {
        extraStructures.Add(skillType);
    }
    public void AddExtraMisc(SPELL_TYPE skillType) {
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