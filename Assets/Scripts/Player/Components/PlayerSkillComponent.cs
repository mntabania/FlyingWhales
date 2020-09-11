using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerSkillComponent {
    public Player player { get; private set; }
    //public List<PlayerSkillTreeNodeData> nodesData { get; protected set; }
    public List<SPELL_TYPE> spells { get; protected set; }
    public List<SPELL_TYPE> afflictions { get; protected set; }
    public List<SPELL_TYPE> playerActions { get; protected set; }
    public List<SPELL_TYPE> demonicStructuresSkills { get; protected set; }
    public List<SPELL_TYPE> minionsSkills { get; protected set; }
    public List<SPELL_TYPE> summonsSkills { get; protected set; }
    public List<PASSIVE_SKILL> passiveSkills { get; protected set; }
    //public List<Summon> summons { get; protected set; }
    //public bool canTriggerFlaw { get; protected set; }
    //public bool canRemoveTraits { get; protected set; }

    public PlayerSkillComponent(Player player) {
        this.player = player;
        //nodesData = new List<PlayerSkillTreeNodeData>();
        spells = new List<SPELL_TYPE>();
        afflictions = new List<SPELL_TYPE>();
        playerActions = new List<SPELL_TYPE>();
        demonicStructuresSkills = new List<SPELL_TYPE>();
        minionsSkills = new List<SPELL_TYPE>();
        summonsSkills = new List<SPELL_TYPE>();
        passiveSkills = new List<PASSIVE_SKILL>();
        //summons = new List<Summon>();
        //canTriggerFlaw = true;
        //canRemoveTraits = true;
    }
    public PlayerSkillComponent() {
        spells = new List<SPELL_TYPE>();
        afflictions = new List<SPELL_TYPE>();
        playerActions = new List<SPELL_TYPE>();
        demonicStructuresSkills = new List<SPELL_TYPE>();
        minionsSkills = new List<SPELL_TYPE>();
        summonsSkills = new List<SPELL_TYPE>();
        passiveSkills = new List<PASSIVE_SKILL>();
    }
    public void SetPlayer(Player player) {
        this.player = player;
    }

    #region Loading
    public void LoadSkills(List<SaveDataPlayerSkill> data) {
        if(data != null) {
            for (int i = 0; i < data.Count; i++) {
                SpellData spell = data[i].Load();
                CategorizePlayerSkill(spell);
            }
        }
    }
    #endregion

    #region Skill Tree
    public void AddPlayerSkill(SpellData spellData, int charges, int manaCost, int cooldown, int threat, int threatPerHour) {
        spellData.SetMaxCharges(charges);
        spellData.SetCharges(charges);
        spellData.SetCooldown(cooldown);
        spellData.SetManaCost(manaCost);
        spellData.SetThreat(threat);
        spellData.SetThreatPerHour(threatPerHour);
        CategorizePlayerSkill(spellData);
    }
    public void AddCharges(SPELL_TYPE spellType, int amount) {
        SpellData spellData = PlayerSkillManager.Instance.GetPlayerSpellData(spellType);
        if (spellData.isInUse) {
            spellData.AdjustCharges(amount);
        } else {
            AddPlayerSkill(spellData, amount, -1, -1, 0, 0);
        }
    }
    public void LoadPlayerSkillTreeOrLoadout(SaveDataPlayer save) {
        if (PlayerSkillManager.Instance.unlockAllSkills) {
            PopulateDevModeSkills();
            PopulatePassiveSkills(PlayerSkillManager.Instance.allPassiveSkillTypes);
        } else {
            //PopulateAllSkills(save.learnedSkills);
            PlayerSkillLoadout loadout = PlayerSkillManager.Instance.GetSelectedLoadout();
            PopulateAllSkills(loadout.spells.fixedSkills);
            PopulateAllSkills(loadout.afflictions.fixedSkills);
            PopulateAllSkills(loadout.minions.fixedSkills);
            PopulateAllSkills(loadout.structures.fixedSkills);
            PopulateAllSkills(loadout.miscs.fixedSkills);
            PopulatePassiveSkills(loadout.passiveSkills);
            
            LoadoutSaveData loadoutSaveData = save.GetLoadout(PlayerSkillManager.Instance.selectedArchetype);
            if (loadoutSaveData != null) {
                PopulateAllSkills(loadoutSaveData.extraSpells);
                PopulateAllSkills(loadoutSaveData.extraAfflictions);
                PopulateAllSkills(loadoutSaveData.extraMinions);
                PopulateAllSkills(loadoutSaveData.extraStructures);
                PopulateAllSkills(loadoutSaveData.extraMiscs);    
            }

            PopulateAllSkills(PlayerSkillManager.Instance.constantSkills);
        }
    }
    //public void LoadSummons(SaveDataPlayer save) {
    //    if(save.kennelSummons != null && save.kennelSummons.Count > 0) {
    //        for (int i = 0; i < save.kennelSummons.Count; i++) {
    //            SaveDataSummon summonData = save.kennelSummons[i];
    //            Summon savedSummon = CharacterManager.Instance.CreateNewSummon(summonData, player.playerFaction);
    //            AddSummon(savedSummon);
    //        }
    //    }
    //}
    #endregion

    #region Utilities
    public bool CanAfflict(SPELL_TYPE type) {
        return PlayerSkillManager.Instance.GetAfflictionData(type).isInUse;
    }
    public bool CanDoPlayerAction(SPELL_TYPE type) {
        return PlayerSkillManager.Instance.GetPlayerActionData(type).isInUse;
    }
    public bool CanBuildDemonicStructure(SPELL_TYPE type) {
        return PlayerSkillManager.Instance.GetDemonicStructureSkillData(type).isInUse;
    }
    public bool CanCastSpell(SPELL_TYPE type) {
        return PlayerSkillManager.Instance.GetSpellData(type).isInUse;
    }
    public bool HasAnyAvailableAffliction() {
        for (int i = 0; i < afflictions.Count; i++) {
            SpellData spellData = PlayerSkillManager.Instance.GetPlayerSpellData(afflictions[i]);
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
        foreach (PlayerSkillData data in PlayerSkillManager.Instance.playerSkillDataDictionary.Values) {
            bool shouldAddSpell = true;
            // if (WorldConfigManager.Instance.isTutorialWorld) {
            //     //if demo world, spell should be added if it is not a minion type. If it is, check if that spell is in
            //     //the available set of spells for the demo. Other spells are added because in the demo, their buttons should still
            //     //be seen, but instead, should not be clickable.
            //     shouldAddSpell = (PlayerSkillManager.Instance.IsMinion(data.skill) == false ||
            //                      WorldConfigManager.Instance.availableSpellsInTutorial.Contains(data.skill))
            //                      && data.skill != SPELL_TYPE.HARASS && data.skill != SPELL_TYPE.SKELETON_MARAUDER
            //                      && PlayerSkillManager.Instance.GetPlayerSpellData(data.skill) != null;
            // } else {
                shouldAddSpell = PlayerSkillManager.Instance.GetPlayerSpellData(data.skill) != null 
                && data.skill != SPELL_TYPE.OSTRACIZER && data.skill != SPELL_TYPE.MEDDLER && data.skill != SPELL_TYPE.CRYPT && data.skill != SPELL_TYPE.SKELETON_MARAUDER;
            // }
            if (shouldAddSpell) {
                SetPlayerSkillData(data);
            }
        }
        //for (int i = 0; i < PlayerSkillManager.Instance.allSkillTrees.Length; i++) {
        //    PlayerSkillTree skillTree = PlayerSkillManager.Instance.allSkillTrees[i];
        //    foreach (KeyValuePair<SPELL_TYPE, PlayerSkillTreeNode> item in skillTree.nodes) {
        //        bool shouldAddSpell = true;
        //        if (WorldConfigManager.Instance.isDemoWorld) {
        //            //if demo world, spell should be added if it is not a minion type. If it is, check if that spell is in
        //            //the available set of spells for the demo. Other spells are added because in the demo, their buttons should still
        //            //be seen, but instead, should not be clickable.
        //            shouldAddSpell = (PlayerSkillManager.Instance.IsMinion(item.Key) == false ||
        //                             WorldConfigManager.Instance.availableSpellsInDemoBuild.Contains(item.Key)) 
        //                             && item.Key != SPELL_TYPE.KNOCKOUT && item.Key != SPELL_TYPE.HARASS && item.Key != SPELL_TYPE.SKELETON_MARAUDER;
        //        } else {
        //            shouldAddSpell = item.Key != SPELL_TYPE.RAIN;
        //        }
        //        if (shouldAddSpell) {
        //            SetPlayerSkillData(item.Key);
        //        }
        //    }
        //}
        //SpellData afflict = PlayerSkillManager.Instance.GetPlayerSkillData(SPELL_TYPE.AFFLICT);
        //CategorizePlayerSkill(afflict);
        ////SpellData buildDemonicStructure = PlayerSkillManager.Instance.GetPlayerSkillData(SPELL_TYPE.BUILD_DEMONIC_STRUCTURE);
        ////CategorizePlayerSkill(buildDemonicStructure);
        //SpellData torture = PlayerSkillManager.Instance.GetPlayerSkillData(SPELL_TYPE.TORTURE);
        //CategorizePlayerSkill(torture);
        //SpellData breedMonster = PlayerSkillManager.Instance.GetPlayerSkillData(SPELL_TYPE.BREED_MONSTER);
        //AddPlayerSkill(breedMonster, -1, 10, 24, 0, 0);

        //For Demo
        //if (WorldConfigManager.Instance.isDemoWorld) {
        //    SpellData rain = PlayerSkillManager.Instance.GetPlayerSpellData(SPELL_TYPE.RAIN);
        //    CategorizePlayerSkill(rain);
        //}
        //SpellData meddler = PlayerSkillManager.Instance.GetPlayerSpellData(SPELL_TYPE.MEDDLER);
        //CategorizePlayerSkill(meddler);
        //SpellData ostracizer = PlayerSkillManager.Instance.GetPlayerSpellData(SPELL_TYPE.OSTRACIZER);
        //CategorizePlayerSkill(ostracizer);    
    }
    private void PopulateAllSkills(List<SPELL_TYPE> skillTypes) {
        if (skillTypes != null) {
            for (int i = 0; i < skillTypes.Count; i++) {
                SPELL_TYPE spellType = skillTypes[i];
                SetPlayerSkillData(spellType);
            }
        }
    }
    private void SetPlayerSkillData(SPELL_TYPE skillType) {
        PlayerSkillData skillData = PlayerSkillManager.Instance.GetPlayerSkillData<PlayerSkillData>(skillType);
        SpellData spellData = PlayerSkillManager.Instance.GetPlayerSpellData(skillType);
        if (spellData == null) {
            Debug.LogError(skillType.ToString() + " data is null!");
        }
        if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Tutorial &&
            skillType == SPELL_TYPE.EYE) {
            //if map is tutorial and spell is THE_EYE, Set max charges to only 1
            spellData.SetMaxCharges(1);  
            spellData.SetCharges(1);
        } else {
            spellData.SetMaxCharges(skillData.charges);    
            spellData.SetCharges(skillData.charges);
        }
        spellData.SetCooldown(skillData.cooldown);
        spellData.SetManaCost(skillData.manaCost);
        spellData.SetThreat(skillData.threat);
        spellData.SetThreatPerHour(skillData.threatPerHour);
        CategorizePlayerSkill(spellData);
    }
    private void SetPlayerSkillData(PlayerSkillData skillData) {
        SpellData spellData = PlayerSkillManager.Instance.GetPlayerSpellData(skillData.skill);
        if(spellData == null) {
            Debug.LogError(skillData.skill.ToString() + " data is null!");
        }
        spellData.SetMaxCharges(skillData.charges);
        spellData.SetCharges(skillData.charges);
        spellData.SetCooldown(skillData.cooldown);
        spellData.SetManaCost(skillData.manaCost);
        spellData.SetThreat(skillData.threat);
        spellData.SetThreatPerHour(skillData.threatPerHour);
        CategorizePlayerSkill(spellData);
    }
    public void CategorizePlayerSkill(SpellData spellData) {
        Assert.IsNotNull(spellData, "Given spell data in CategorizePlayerSkill is null!");
        spellData.SetIsInUse(true);
        if (spellData.category == SPELL_CATEGORY.AFFLICTION) {
            afflictions.Add(spellData.type);
        } else if (spellData.category == SPELL_CATEGORY.DEMONIC_STRUCTURE) {
            demonicStructuresSkills.Add(spellData.type);
        } else if (spellData.category == SPELL_CATEGORY.MINION) {
            minionsSkills.Add(spellData.type);
            Messenger.Broadcast(Signals.ADDED_PLAYER_MINION_SKILL, spellData.type);
        } else if (spellData.category == SPELL_CATEGORY.PLAYER_ACTION) {
            playerActions.Add(spellData.type);
        } else if (spellData.category == SPELL_CATEGORY.SPELL) {
            spells.Add(spellData.type);
        } else if (spellData.category == SPELL_CATEGORY.SUMMON) {
            summonsSkills.Add(spellData.type);
            Messenger.Broadcast(Signals.ADDED_PLAYER_SUMMON_SKILL, spellData.type);
        }
    }
    #endregion

    #region Summons
    //public void AddSummon(Summon summon) {
    //    summons.Add(summon);
    //}
    //public void RemoveSummon(Summon summon, bool removeOnSaveFile = false) {
    //    if (summons.Remove(summon)) {
    //        if (removeOnSaveFile) {
    //            SaveManager.Instance.currentSaveDataPlayer.RemoveKennelSummon(summon);
    //        }
    //        Messenger.Broadcast(Signals.SUMMON_REMOVED, summon);
    //    }
    //}
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
                return $"Summon a {UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(currentlySelectedSummon.ToString())}";
        }
    }
    #endregion

    #region Passive Skills
    private void PopulatePassiveSkills(PASSIVE_SKILL[] passiveSkills) {
        for (int i = 0; i < passiveSkills.Length; i++) {
            PASSIVE_SKILL passiveSkillType = passiveSkills[i];
            PassiveSkill passiveSkill = PlayerSkillManager.Instance.GetPassiveSkill(passiveSkillType);
            passiveSkill.ActivateSkill();
            this.passiveSkills.Add(passiveSkillType);
            Debug.Log($"{GameManager.Instance.TodayLogString()}Activated passive skill {passiveSkillType.ToString()}.");
        }
    }
    #endregion

    #region Loading
    public void OnLoadSaveData() {
        for (int i = 0; i < spells.Count; i++) {
            SPELL_TYPE skillType = spells[i];
            SpellData skill = PlayerSkillManager.Instance.GetSpellData(skillType);
            skill.OnLoadSpell();
        }
        for (int i = 0; i < demonicStructuresSkills.Count; i++) {
            SPELL_TYPE skillType = demonicStructuresSkills[i];
            SpellData skill = PlayerSkillManager.Instance.GetDemonicStructureSkillData(skillType);
            skill.OnLoadSpell();
        }
        for (int i = 0; i < minionsSkills.Count; i++) {
            SPELL_TYPE skillType = minionsSkills[i];
            SpellData skill = PlayerSkillManager.Instance.GetMinionPlayerSkillData(skillType);
            skill.OnLoadSpell();
        }
        for (int i = 0; i < summonsSkills.Count; i++) {
            SPELL_TYPE skillType = summonsSkills[i];
            SpellData skill = PlayerSkillManager.Instance.GetSummonPlayerSkillData(skillType);
            skill.OnLoadSpell();
        }
        for (int i = 0; i < playerActions.Count; i++) {
            SPELL_TYPE skillType = playerActions[i];
            SpellData skill = PlayerSkillManager.Instance.GetPlayerActionData(skillType);
            skill.OnLoadSpell();
        }
        for (int i = 0; i < afflictions.Count; i++) {
            SPELL_TYPE skillType = afflictions[i];
            SpellData skill = PlayerSkillManager.Instance.GetAfflictionData(skillType);
            skill.OnLoadSpell();
        }
        //did not save passive skills since I expect that passive skills will always be constant per loadout.
        PlayerSkillLoadout loadout = PlayerSkillManager.Instance.GetSelectedLoadout();
        PopulatePassiveSkills(loadout.passiveSkills);
    }
    #endregion
}
[System.Serializable]
public class SaveDataPlayerSkillComponent : SaveData<PlayerSkillComponent> {
    public List<SaveDataPlayerSkill> skills;
    //public bool canTriggerFlaw;
    //public bool canRemoveTraits;

    public override void Save(PlayerSkillComponent component) {
        //canTriggerFlaw = player.playerSkillComponent.canTriggerFlaw;
        //canRemoveTraits = player.playerSkillComponent.canRemoveTraits;

        skills = new List<SaveDataPlayerSkill>();
        for (int i = 0; i < component.spells.Count; i++) {
            SpellData spell = PlayerSkillManager.Instance.GetSpellData(component.spells[i]);
            SaveDataPlayerSkill dataPlayerSkill = new SaveDataPlayerSkill();
            dataPlayerSkill.Save(spell);
            skills.Add(dataPlayerSkill);
        }
        for (int i = 0; i < component.afflictions.Count; i++) {
            SpellData spell = PlayerSkillManager.Instance.GetAfflictionData(component.afflictions[i]);
            SaveDataPlayerSkill dataPlayerSkill = new SaveDataPlayerSkill();
            dataPlayerSkill.Save(spell);
            skills.Add(dataPlayerSkill);
        }
        for (int i = 0; i < component.playerActions.Count; i++) {
            PlayerAction spell = PlayerSkillManager.Instance.GetPlayerActionData(component.playerActions[i]);
            SaveDataPlayerSkill dataPlayerSkill = new SaveDataPlayerSkill();
            dataPlayerSkill.Save(spell);
            skills.Add(dataPlayerSkill);
        }
        for (int i = 0; i < component.demonicStructuresSkills.Count; i++) {
            DemonicStructurePlayerSkill spell = PlayerSkillManager.Instance.GetDemonicStructureSkillData(component.demonicStructuresSkills[i]);
            SaveDataPlayerSkill dataPlayerSkill = new SaveDataPlayerSkill();
            dataPlayerSkill.Save(spell);
            skills.Add(dataPlayerSkill);
        }
        for (int i = 0; i < component.minionsSkills.Count; i++) {
            MinionPlayerSkill spell = PlayerSkillManager.Instance.GetMinionPlayerSkillData(component.minionsSkills[i]);
            SaveDataPlayerSkill dataPlayerSkill = new SaveDataPlayerSkill();
            dataPlayerSkill.Save(spell);
            skills.Add(dataPlayerSkill);
        }
        for (int i = 0; i < component.summonsSkills.Count; i++) {
            SummonPlayerSkill spell = PlayerSkillManager.Instance.GetSummonPlayerSkillData(component.summonsSkills[i]);
            SaveDataPlayerSkill dataPlayerSkill = new SaveDataPlayerSkill();
            dataPlayerSkill.Save(spell);
            skills.Add(dataPlayerSkill);
        }
    }
    public override PlayerSkillComponent Load() {
        PlayerSkillComponent component = new PlayerSkillComponent();
        component.LoadSkills(skills);
        return component;
    }
}