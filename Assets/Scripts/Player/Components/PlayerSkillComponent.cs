﻿using System.Collections;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;

public class PlayerSkillComponent {

    public const int RerollCooldownInHours = 12;
    
    //public List<PlayerSkillTreeNodeData> nodesData { get; protected set; }
    public List<PLAYER_SKILL_TYPE> spells { get; protected set; }
    public List<PLAYER_SKILL_TYPE> afflictions { get; protected set; }
    public List<PLAYER_SKILL_TYPE> schemes { get; protected set; }
    public List<PLAYER_SKILL_TYPE> playerActions { get; protected set; }
    public List<PLAYER_SKILL_TYPE> demonicStructuresSkills { get; protected set; }
    public List<PLAYER_SKILL_TYPE> minionsSkills { get; protected set; }
    public List<PLAYER_SKILL_TYPE> summonsSkills { get; protected set; }
    public List<PASSIVE_SKILL> passiveSkills { get; protected set; }
    //public List<Summon> summons { get; protected set; }
    //public bool canTriggerFlaw { get; protected set; }
    //public bool canRemoveTraits { get; protected set; }

    public int tier1Count { get; protected set; }
    public int tier2Count { get; protected set; }
    public int tier3Count { get; protected set; }
    
    //Skill Unlocking
    public PLAYER_SKILL_TYPE currentSpellBeingUnlocked { get; private set; }
    public int currentSpellUnlockCost { get; private set; }
    public RuinarchTimer timerUnlockSpell { get; private set; }
    public RuinarchTimer cooldownReroll { get; private set; }
    public List<PLAYER_SKILL_TYPE> currentSpellChoices { get; private set; }
        
    public Cost[] currentPortalUpgradeCost { get; private set; }
    public RuinarchTimer timerUpgradePortal { get; private set; }

    public PlayerSkillComponent() {
        //nodesData = new List<PlayerSkillTreeNodeData>();
        spells = new List<PLAYER_SKILL_TYPE>();
        afflictions = new List<PLAYER_SKILL_TYPE>();
        schemes = new List<PLAYER_SKILL_TYPE>();
        playerActions = new List<PLAYER_SKILL_TYPE>();
        demonicStructuresSkills = new List<PLAYER_SKILL_TYPE>();
        minionsSkills = new List<PLAYER_SKILL_TYPE>();
        summonsSkills = new List<PLAYER_SKILL_TYPE>();
        passiveSkills = new List<PASSIVE_SKILL>();
        //summons = new List<Summon>();
        //canTriggerFlaw = true;
        //canRemoveTraits = true;
        currentSpellBeingUnlocked = PLAYER_SKILL_TYPE.NONE;
        timerUnlockSpell = new RuinarchTimer("Spell Unlock");
        cooldownReroll = new RuinarchTimer("Reroll");
        currentSpellChoices = new List<PLAYER_SKILL_TYPE>();
        timerUpgradePortal = new RuinarchTimer("Summon Demon");

        Messenger.AddListener<LocationStructure>(StructureSignals.STRUCTURE_OBJECT_PLACED, OnStructurePlaced);
        Messenger.AddListener<LocationStructure>(StructureSignals.STRUCTURE_DESTROYED, OnStructureDestroyed);
    }

    void OnStructurePlaced(LocationStructure p_structure) {
        if (p_structure.structureType == STRUCTURE_TYPE.BIOLAB) {
            UnlockPlagueSkills();
        }
    }

    void OnStructureDestroyed(LocationStructure p_structure) {
        if (p_structure.structureType == STRUCTURE_TYPE.BIOLAB) {
			if (!HasBiolab()) {
                LockPlagueSkills();
            }
        }
    }

    void UnlockPlagueSkills() {
        SkillData skilldata = PlayerSkillManager.Instance.GetPlayerSkillData(PLAYER_SKILL_TYPE.PLAGUED_RAT);
        skilldata.SetIsUnlockBaseOnRequirements(true);
        AddCharges(skilldata.type, 1);
        skilldata = PlayerSkillManager.Instance.GetAfflictionData(PLAYER_SKILL_TYPE.PLAGUE);
        skilldata.SetIsUnlockBaseOnRequirements(true);
        AddCharges(skilldata.type, 1);
        Messenger.Broadcast(SpellSignals.PLAYER_GAINED_SPELL, PLAYER_SKILL_TYPE.PLAGUED_RAT);
    }

    void LockPlagueSkills() {
        SkillData skilldata = PlayerSkillManager.Instance.GetPlayerSkillData(PLAYER_SKILL_TYPE.PLAGUE);
        skilldata.SetIsUnlockBaseOnRequirements(false);
        skilldata.ResetData();
        skilldata = PlayerSkillManager.Instance.GetPlayerSkillData(PLAYER_SKILL_TYPE.PLAGUED_RAT);
        skilldata.SetIsUnlockBaseOnRequirements(false);
        skilldata.ResetData();
        Messenger.Broadcast(SpellSignals.PLAYER_LOST_SPELL, PLAYER_SKILL_TYPE.PLAGUED_RAT);
    }

    #region Loading
    public void LoadSkills(List<SaveDataPlayerSkill> data) {
        if(data != null) {
            for (int i = 0; i < data.Count; i++) {
                SkillData spell = data[i].Load();
                CategorizePlayerSkill(spell);
            }
        }
    }
    #endregion

    #region Unlocking
    public void PlayerChoseSkillToUnlock(SkillData p_skillData, int p_unlockCost) {
        currentSpellBeingUnlocked = p_skillData.type;
        currentSpellUnlockCost = p_unlockCost;
        timerUnlockSpell.SetTimerName($"{LocalizationManager.Instance.GetLocalizedValue("UI", "PortalUI", "release_ability_active")} {p_skillData.name}");
        timerUnlockSpell.Start(GameManager.Instance.Today(), GameManager.Instance.Today().AddDays(1), OnCompleteSpellUnlockTimer); //.AddDays(1)
        timerUnlockSpell.SetOnSelectAction(() => UIManager.Instance.ShowStructureInfo(PlayerManager.Instance.player.playerSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.THE_PORTAL)));
        PlayerManager.Instance.player.bookmarkComponent.AddBookmark(timerUnlockSpell, BOOKMARK_CATEGORY.Portal);
        Messenger.Broadcast(PlayerSignals.PLAYER_CHOSE_SKILL_TO_UNLOCK, p_skillData, p_unlockCost);
    }
    public void CancelCurrentPlayerSkillUnlock() {
        //Refund player mana
        PlayerManager.Instance.player.AdjustMana(currentSpellUnlockCost);
        currentSpellBeingUnlocked = PLAYER_SKILL_TYPE.NONE;
        currentSpellUnlockCost = 0;
        timerUnlockSpell.Stop();
        PlayerManager.Instance.player.bookmarkComponent.RemoveBookmark(timerUnlockSpell);
        Messenger.Broadcast(PlayerSignals.PLAYER_SKILL_UNLOCK_CANCELLED);
    }
    private void OnCompleteSpellUnlockTimer() {
        PlayerManager.Instance.player.bookmarkComponent.RemoveBookmark(timerUnlockSpell);
        PlayerManager.Instance.player.playerSkillComponent.SetPlayerSkillData(currentSpellBeingUnlocked);
        ResetPlayerSpellChoices();
        SkillData skillData = PlayerSkillManager.Instance.GetPlayerSkillData(currentSpellBeingUnlocked);
        if (skillData.category == PLAYER_SKILL_CATEGORY.SPELL) {
            Messenger.Broadcast(SpellSignals.PLAYER_GAINED_SPELL, currentSpellBeingUnlocked);    
        }
        Messenger.Broadcast(PlayerSignals.PLAYER_FINISHED_SKILL_UNLOCK, currentSpellBeingUnlocked, currentSpellUnlockCost);
        currentSpellBeingUnlocked = PLAYER_SKILL_TYPE.NONE;
        currentSpellUnlockCost = 0;
    }
    public void OnRerollUsed() {
        ResetPlayerSpellChoices();
        cooldownReroll.Start(GameManager.Instance.Today(), GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(RerollCooldownInHours)), OnRerollCooldownFinished);
    }
    private void OnRerollCooldownFinished() {
        Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "UI", "PortalUI", "reroll_available", null, LOG_TAG.Major);
        PlayerManager.Instance.player.ShowNotificationFromPlayer(log, true);
    }
    public void PlayerStartedPortalUpgrade(Cost[] p_upgradeCost, PortalUpgradeTier p_upgradeTier) {
        currentPortalUpgradeCost = p_upgradeCost;
        ThePortal portal = PlayerManager.Instance.player.playerSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.THE_PORTAL) as ThePortal;
        timerUpgradePortal.SetTimerName($"{LocalizationManager.Instance.GetLocalizedValue("UI", "PortalUI", "upgrade_portal_active")} {(portal.level + 1).ToString()}:");
        timerUpgradePortal.Start(GameManager.Instance.Today(), GameManager.Instance.Today().AddTicks(p_upgradeTier.upgradeTime), OnCompletePortalUpgrade);
        timerUpgradePortal.SetOnSelectAction(() => UIManager.Instance.ShowStructureInfo(portal));
        PlayerManager.Instance.player.bookmarkComponent.AddBookmark(timerUpgradePortal, BOOKMARK_CATEGORY.Portal);
        Messenger.Broadcast(PlayerSignals.PLAYER_STARTED_PORTAL_UPGRADE);
    }
    public void CancelPortalUpgrade() {
        //Refund player mana
        PlayerManager.Instance.player.bookmarkComponent.RemoveBookmark(timerUpgradePortal);
        for (int i = 0; i < currentPortalUpgradeCost.Length; i++) {
            PlayerManager.Instance.player.AddCurrency(currentPortalUpgradeCost[i]);    
        }
        currentPortalUpgradeCost = null;
        timerUpgradePortal.Stop();
        Messenger.Broadcast(PlayerSignals.PORTAL_UPGRADE_CANCELLED);
    }
    private void OnCompletePortalUpgrade() {
        PlayerManager.Instance.player.bookmarkComponent.RemoveBookmark(timerUpgradePortal);
        ThePortal portal = PlayerManager.Instance.player.playerSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.THE_PORTAL) as ThePortal;
        portal.GainUpgradePowers(portal.nextTier);
        portal.IncreaseLevel();
        Messenger.Broadcast(PlayerSignals.PLAYER_FINISHED_PORTAL_UPGRADE, portal.level);
        currentPortalUpgradeCost = null;
    }
    private void ResetPlayerSpellChoices() {
        currentSpellChoices.Clear();
        Debug.Log("Reset player spell choices.");
    }
    public void AddCurrentPlayerSpellChoice(PLAYER_SKILL_TYPE p_skillType) {
        currentSpellChoices.Add(p_skillType);
    }
    #endregion

    #region Skill Tree
    private void AddPlayerSkill(SkillData spellData, int charges, int manaCost, int cooldown, int threat, int threatPerHour, float pierce) {
        PlayerSkillData playerSkillData = PlayerSkillManager.Instance.GetPlayerSkillData<PlayerSkillData>(spellData.type);
        if (playerSkillData != null) {
            spellData.SetCurrentLevel(playerSkillData.cheatedLevel);    
            spellData.SetMaxCharges(playerSkillData.GetMaxChargesBaseOnLevel(spellData.currentLevel));
            spellData.SetCharges(charges);
            spellData.SetCooldown(playerSkillData.GetCoolDownBaseOnLevel(spellData.currentLevel));
            spellData.SetPierce(PlayerSkillManager.Instance.GetAdditionalPiercePerLevelBaseOnLevel(spellData.type));
            spellData.SetUnlockCost(playerSkillData.unlockCost);
            spellData.SetManaCost(playerSkillData.GetManaCostBaseOnLevel(spellData.currentLevel));
            spellData.SetThreat(threat);
            spellData.SetThreatPerHour(threatPerHour);
        } else {
            spellData.SetCurrentLevel(1);
            spellData.SetMaxCharges(charges);
            spellData.SetCharges(charges);
            spellData.SetCooldown(cooldown);
            spellData.SetPierce(pierce);
            spellData.SetUnlockCost(0);
            spellData.SetManaCost(manaCost);
            spellData.SetThreat(threat);
            spellData.SetThreatPerHour(threatPerHour);
        }
        // Debug.LogError(spellData.name + " -- " + spellData.currentLevel + " -- " + playerSkillData.cheatedLevel);
        CategorizePlayerSkill(spellData);
    }
    public void AddCharges(PLAYER_SKILL_TYPE spellType, int amount) {
        SkillData spellData = PlayerSkillManager.Instance.GetPlayerSkillData(spellType);
        if (spellData.isInUse) {
            spellData.AdjustCharges(amount);
        } else {
            AddPlayerSkill(spellData, amount, -1, -1, 0, 0, 0);
            var playerSkillData = PlayerSkillManager.Instance.GetPlayerSkillData<PlayerSkillData>(spellData.type);
            if (playerSkillData != null) {
                UpdateTierCount(playerSkillData);    
            }
        }
    }
    public void AddMaxCharges(PLAYER_SKILL_TYPE spellType, int amount) {
        SkillData spellData = PlayerSkillManager.Instance.GetPlayerSkillData(spellType);
        if (spellData.isInUse) {
            spellData.AdjustMaxCharges(amount);
            spellData.AdjustCharges(amount);
        } else {
            AddPlayerSkill(spellData, amount, -1, -1, 0, 0, 0);
            var playerSkillData = PlayerSkillManager.Instance.GetPlayerSkillData<PlayerSkillData>(spellData.type);
            if (playerSkillData != null) {
                UpdateTierCount(playerSkillData);    
            }
        }
    }
    public void LoadPlayerSkillTreeOrLoadout(SaveDataPlayer save) {
        ScenarioData scenarioData = null;
        if (WorldSettings.Instance.worldSettingsData.IsScenarioMap()) {
            scenarioData = WorldSettings.Instance.GetScenarioDataByWorldType(WorldSettings.Instance.worldSettingsData.worldType);
        }
        if (PlayerSkillManager.Instance.unlockAllSkills) {
            PopulateDevModeSkills(scenarioData);
            PlayerSkillLoadout loadout = PlayerSkillManager.Instance.GetSelectedLoadout();
            PopulatePassiveSkills(loadout.passiveSkills);
            // PopulatePassiveSkills(PlayerSkillManager.Instance.allPassiveSkillTypes);
        } else {
            //PopulateAllSkills(save.learnedSkills);
            PlayerSkillLoadout loadout = PlayerSkillManager.Instance.GetSelectedLoadout();
            PopulateAllSkills(loadout.spells.fixedSkills, scenarioData);
            PopulateAllSkills(loadout.afflictions.fixedSkills, scenarioData);
            PopulateAllSkills(loadout.minions.fixedSkills, scenarioData);
            PopulateAllSkills(loadout.structures.fixedSkills, scenarioData);
            PopulateAllSkills(loadout.miscs.fixedSkills, scenarioData);
            PopulatePassiveSkills(loadout.passiveSkills);
            
            LoadoutSaveData loadoutSaveData = save.GetLoadout(PlayerSkillManager.Instance.selectedArchetype);
            if (loadoutSaveData != null) {
                PopulateAllSkills(loadoutSaveData.extraSpells, scenarioData);
                PopulateAllSkills(loadoutSaveData.extraAfflictions, scenarioData);
                PopulateAllSkills(loadoutSaveData.extraMinions, scenarioData);
                PopulateAllSkills(loadoutSaveData.extraStructures, scenarioData);
                PopulateAllSkills(loadoutSaveData.extraMiscs, scenarioData);    
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

    private void UpdateTierCount(PlayerSkillData playerSkillData) {
        switch (playerSkillData.tier) {
            case 1: tier1Count++;
            break;
            case 2: tier2Count++;
            break;
            case 3: tier3Count++;
            break;
        }
    }
    public int GetLevelOfSkill(SkillData p_targetSkill) {
        int currentLevel = 0;
        switch (p_targetSkill.category) {
            case PLAYER_SKILL_CATEGORY.PLAYER_ACTION:
            playerActions.ForEach((eachSkill) => {
                if (eachSkill == p_targetSkill.type) {
                    currentLevel = PlayerSkillManager.Instance.GetPlayerSkillData(eachSkill).currentLevel;
                }
            });
            break;
            case PLAYER_SKILL_CATEGORY.SPELL:
            spells.ForEach((eachSkill) => {
                if (eachSkill == p_targetSkill.type) {
                    currentLevel = PlayerSkillManager.Instance.GetPlayerSkillData(eachSkill).currentLevel;
                }
            });
            break;
            case PLAYER_SKILL_CATEGORY.AFFLICTION:
            afflictions.ForEach((eachSkill) => {
                if (eachSkill == p_targetSkill.type) {
                    currentLevel = PlayerSkillManager.Instance.GetPlayerSkillData(eachSkill).currentLevel;
                }
            });
            break;
        }
        
        return currentLevel;
    }

    #region Utilities
    public bool CheckIfSkillIsAvailable(PLAYER_SKILL_TYPE p_targetSkill) {
        if (spells.Contains(p_targetSkill)) {
            return true;
        }
        if (playerActions.Contains(p_targetSkill)) {
            return true;
        }
        if (afflictions.Contains(p_targetSkill)) {
            return true;
        }
        return false;
    }
    public bool CanDoPlayerAction(PLAYER_SKILL_TYPE type) {
        return PlayerSkillManager.Instance.GetPlayerSkillData(type).isInUse;
    }
    public bool CanBuildDemonicStructure(PLAYER_SKILL_TYPE type) {
        return PlayerSkillManager.Instance.GetDemonicStructureSkillData(type).isInUse;
    }
    public bool HasAnyAvailableAffliction() {
        for (int i = 0; i < afflictions.Count; i++) {
            SkillData spellData = PlayerSkillManager.Instance.GetPlayerSkillData(afflictions[i]);
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
    private void PopulateDevModeSkills(ScenarioData scenarioData = null) {
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
                shouldAddSpell = PlayerSkillManager.Instance.GetPlayerSkillData(data.skill) != null 
                && data.skill != PLAYER_SKILL_TYPE.OSTRACIZER && data.skill != PLAYER_SKILL_TYPE.SKELETON;
            // }
            if (shouldAddSpell) {
                SetPlayerSkillData(data);
                if (scenarioData != null) {
                    //set level provided by scenario data
                    int providedLevel = scenarioData.GetLevelForPower(data.skill);
                    SkillData skillData = PlayerSkillManager.Instance.GetPlayerSkillData(data.skill);
                    for (int j = 0; j < providedLevel; j++) {
                        skillData.LevelUp();
                    }
                }
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
    private void PopulateAllSkills(List<PLAYER_SKILL_TYPE> skillTypes, ScenarioData scenarioData = null) {
        if (skillTypes != null) {
            for (int i = 0; i < skillTypes.Count; i++) {
                PLAYER_SKILL_TYPE spellType = skillTypes[i];
                SetPlayerSkillData(spellType);
                if (scenarioData != null) {
                    //set level provided by scenario data
                    int providedLevel = scenarioData.GetLevelForPower(spellType);
                    SkillData skillData = PlayerSkillManager.Instance.GetPlayerSkillData(spellType);
                    for (int j = 0; j < providedLevel; j++) {
                        skillData.LevelUp();
                    }
                }
            }
        }
    }
    public void SetPlayerSkillData(PLAYER_SKILL_TYPE skillType, bool testScene = false) {
        PlayerSkillData skillData = PlayerSkillManager.Instance.GetPlayerSkillData<PlayerSkillData>(skillType);
        SkillData spellData = PlayerSkillManager.Instance.GetPlayerSkillData(skillType);
        PlayerSkillData playerSkillData = PlayerSkillManager.Instance.GetPlayerSkillData<PlayerSkillData>(spellData.type);
        if (spellData == null) {
            Debug.LogError(skillType.ToString() + " data is null!");
        }
        if (!testScene && WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Tutorial &&
            skillType == PLAYER_SKILL_TYPE.BEHOLDER) {
            //if map is tutorial and spell is THE_EYE, Set max charges to only 1
            spellData.SetMaxCharges(1);  
            spellData.SetCharges(1);
        } else {
            spellData.SetMaxCharges(playerSkillData.GetMaxChargesBaseOnLevel(spellData.currentLevel));
            spellData.SetCharges(spellData.maxCharges);
        }
        spellData.SetCooldown(playerSkillData.GetCoolDownBaseOnLevel(spellData.currentLevel));
        spellData.SetPierce(PlayerSkillManager.Instance.GetAdditionalPiercePerLevelBaseOnLevel(skillType));
        spellData.SetUnlockCost(playerSkillData.unlockCost);
        spellData.SetManaCost(playerSkillData.GetManaCostBaseOnLevel(spellData.currentLevel));
        spellData.SetThreat(skillData.threat);
        spellData.SetThreatPerHour(skillData.threatPerHour);
        CategorizePlayerSkill(spellData);
    }
    private void SetPlayerSkillData(PlayerSkillData skillData) {
        SkillData spellData = PlayerSkillManager.Instance.GetPlayerSkillData(skillData.skill);
        PlayerSkillData playerSkillData = PlayerSkillManager.Instance.GetPlayerSkillData<PlayerSkillData>(spellData.type);
        spellData.currentLevel = playerSkillData.cheatedLevel;
        if (spellData == null) {
            Debug.LogError(skillData.skill.ToString() + " data is null!");
        }
        spellData.SetMaxCharges(playerSkillData.GetMaxChargesBaseOnLevel(spellData.currentLevel));
        spellData.SetCharges(spellData.maxCharges);
        spellData.SetCooldown(playerSkillData.GetCoolDownBaseOnLevel(spellData.currentLevel));
        spellData.SetUnlockCost(playerSkillData.unlockCost);
        spellData.SetPierce(PlayerSkillManager.Instance.GetAdditionalPiercePerLevelBaseOnLevel(spellData.type));
        spellData.SetManaCost(playerSkillData.GetManaCostBaseOnLevel(spellData.currentLevel));
        spellData.SetThreat(skillData.threat);
        spellData.SetThreatPerHour(skillData.threatPerHour);
        CategorizePlayerSkill(spellData);
    }
    public void CategorizePlayerSkill(SkillData spellData) {
        Assert.IsNotNull(spellData, "Given spell data in CategorizePlayerSkill is null!");
        spellData.SetIsInUse(true);
        if (spellData.category == PLAYER_SKILL_CATEGORY.AFFLICTION) {
            afflictions.Add(spellData.type);
        } else if (spellData.category == PLAYER_SKILL_CATEGORY.SCHEME) {
            schemes.Add(spellData.type);
        } else if (spellData.category == PLAYER_SKILL_CATEGORY.DEMONIC_STRUCTURE) {
            demonicStructuresSkills.Add(spellData.type);
            Messenger.Broadcast(SpellSignals.PLAYER_GAINED_DEMONIC_STRUCTURE, spellData.type);
        } else if (spellData.category == PLAYER_SKILL_CATEGORY.MINION) {
            minionsSkills.Add(spellData.type);
            Messenger.Broadcast(SpellSignals.ADDED_PLAYER_MINION_SKILL, spellData.type);
        } else if (spellData.category == PLAYER_SKILL_CATEGORY.PLAYER_ACTION) {
            playerActions.Add(spellData.type);
        } else if (spellData.category == PLAYER_SKILL_CATEGORY.SPELL) {
            spells.Add(spellData.type);
        } else if (spellData.category == PLAYER_SKILL_CATEGORY.SUMMON) {
            summonsSkills.Add(spellData.type);
            Messenger.Broadcast(SpellSignals.ADDED_PLAYER_SUMMON_SKILL, spellData.type);
        }
    }
    #endregion

    bool HasBiolab() {
        for(int x = 0; x < PlayerManager.Instance.player.playerSettlement.allStructures.Count; ++x) {
            if(PlayerManager.Instance.player.playerSettlement.allStructures[x].structureType == STRUCTURE_TYPE.BIOLAB) {
                return true;
			}
		}
        return false;
	}

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
            AddPassiveSkills(passiveSkillType);
        }
    }
    public void AddPassiveSkills(PASSIVE_SKILL passiveSkills) {
        PassiveSkill passiveSkill = PlayerSkillManager.Instance.GetPassiveSkill(passiveSkills);
        passiveSkill.ActivateSkill();
        this.passiveSkills.Add(passiveSkills);
        Debug.Log($"{GameManager.Instance.TodayLogString()}Activated passive skill {passiveSkills.ToString()}.");
    }
    #endregion

    #region Blackmail
    /// <summary>
    /// Has the player already stored blackmail for a given character.
    /// </summary>
    /// <param name="p_character">The character in question.</param>
    /// <returns>True or false.</returns>
    public bool AlreadyHasBlackmail(Character p_character) {
        return PlayerManager.Instance.player.HasHostageIntel(p_character);
    }
    #endregion

    #region Loading
    public void OnLoadSaveData() {
        for (int i = 0; i < spells.Count; i++) {
            PLAYER_SKILL_TYPE skillType = spells[i];
            SkillData skill = PlayerSkillManager.Instance.GetSpellData(skillType);
            skill.OnLoadSpell();
        }
        for (int i = 0; i < demonicStructuresSkills.Count; i++) {
            PLAYER_SKILL_TYPE skillType = demonicStructuresSkills[i];
            SkillData skill = PlayerSkillManager.Instance.GetDemonicStructureSkillData(skillType);
            skill.OnLoadSpell();
        }
        for (int i = 0; i < minionsSkills.Count; i++) {
            PLAYER_SKILL_TYPE skillType = minionsSkills[i];
            SkillData skill = PlayerSkillManager.Instance.GetMinionPlayerSkillData(skillType);
            skill.OnLoadSpell();
        }
        for (int i = 0; i < summonsSkills.Count; i++) {
            PLAYER_SKILL_TYPE skillType = summonsSkills[i];
            SkillData skill = PlayerSkillManager.Instance.GetSummonPlayerSkillData(skillType);
            skill.OnLoadSpell();
        }
        for (int i = 0; i < playerActions.Count; i++) {
            PLAYER_SKILL_TYPE skillType = playerActions[i];
            SkillData skill = PlayerSkillManager.Instance.GetPlayerActionData(skillType);
            skill.OnLoadSpell();
        }
        for (int i = 0; i < afflictions.Count; i++) {
            PLAYER_SKILL_TYPE skillType = afflictions[i];
            SkillData skill = PlayerSkillManager.Instance.GetAfflictionData(skillType);
            skill.OnLoadSpell();
        }
        for (int i = 0; i < schemes.Count; i++) {
            PLAYER_SKILL_TYPE skillType = schemes[i];
            SkillData skill = PlayerSkillManager.Instance.GetSchemeData(skillType);
            skill.OnLoadSpell();
        }
        //did not save passive skills since I expect that passive skills will always be constant per loadout.
        PlayerSkillLoadout loadout = PlayerSkillManager.Instance.GetSelectedLoadout();
        PopulatePassiveSkills(loadout.passiveSkills);
    }
    public void LoadReferences(SaveDataPlayerSkillComponent data) {
        currentSpellBeingUnlocked = data.currentSpellBeingUnlocked;
        currentSpellUnlockCost = data.currentSpellUnlockCost;
        timerUnlockSpell = data.timerUnlockSpell;
        if (currentSpellBeingUnlocked != PLAYER_SKILL_TYPE.NONE) {
            timerUnlockSpell.LoadStart(OnCompleteSpellUnlockTimer);
            timerUnlockSpell.SetOnSelectAction(() => UIManager.Instance.ShowStructureInfo(PlayerManager.Instance.player.playerSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.THE_PORTAL)));
            PlayerManager.Instance.player.bookmarkComponent.AddBookmark(timerUnlockSpell, BOOKMARK_CATEGORY.Portal);
        }
        cooldownReroll = data.cooldownReroll;
        if (!cooldownReroll.IsFinished()) {
            cooldownReroll.LoadStart();
        }
        currentPortalUpgradeCost = data.currentPortalUpgradeCost;
        timerUpgradePortal = data.timerUpgradePortal;
        if (currentPortalUpgradeCost != null) {
            timerUpgradePortal.LoadStart(OnCompletePortalUpgrade);
            timerUpgradePortal.SetOnSelectAction(() => UIManager.Instance.ShowStructureInfo(PlayerManager.Instance.player.playerSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.THE_PORTAL)));
            PlayerManager.Instance.player.bookmarkComponent.AddBookmark(timerUpgradePortal, BOOKMARK_CATEGORY.Portal);
        }
        currentSpellChoices = data.currentSpellChoices;
    }
    #endregion
}
[System.Serializable]
public class SaveDataPlayerSkillComponent : SaveData<PlayerSkillComponent> {
    public List<SaveDataPlayerSkill> skills;
    //public bool canTriggerFlaw;
    //public bool canRemoveTraits;
    //Skill Unlocking
    public PLAYER_SKILL_TYPE currentSpellBeingUnlocked;
    public int currentSpellUnlockCost;
    public RuinarchTimer timerUnlockSpell;
    public RuinarchTimer cooldownReroll;
    public List<PLAYER_SKILL_TYPE> currentSpellChoices;
    public Cost[] currentPortalUpgradeCost;
    public RuinarchTimer timerUpgradePortal;

    public override void Save(PlayerSkillComponent component) {
        //canTriggerFlaw = player.playerSkillComponent.canTriggerFlaw;
        //canRemoveTraits = player.playerSkillComponent.canRemoveTraits;

        skills = new List<SaveDataPlayerSkill>();
        for (int i = 0; i < component.spells.Count; i++) {
            SkillData spell = PlayerSkillManager.Instance.GetSpellData(component.spells[i]);
            SaveDataPlayerSkill dataPlayerSkill = new SaveDataPlayerSkill();
            dataPlayerSkill.Save(spell);
            skills.Add(dataPlayerSkill);
        }
        for (int i = 0; i < component.afflictions.Count; i++) {
            SkillData spell = PlayerSkillManager.Instance.GetAfflictionData(component.afflictions[i]);
            SaveDataPlayerSkill dataPlayerSkill = new SaveDataPlayerSkill();
            dataPlayerSkill.Save(spell);
            skills.Add(dataPlayerSkill);
        }
        for (int i = 0; i < component.schemes.Count; i++) {
            SkillData spell = PlayerSkillManager.Instance.GetSchemeData(component.schemes[i]);
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

        
        currentSpellBeingUnlocked = component.currentSpellBeingUnlocked;
        currentSpellUnlockCost = component.currentSpellUnlockCost;
        timerUnlockSpell = component.timerUnlockSpell;
        cooldownReroll = component.cooldownReroll;
        currentSpellChoices = component.currentSpellChoices;
        currentPortalUpgradeCost = component.currentPortalUpgradeCost;
        timerUpgradePortal = component.timerUpgradePortal;
    }
    public override PlayerSkillComponent Load() {
        PlayerSkillComponent component = new PlayerSkillComponent();
        component.LoadSkills(skills);
        return component;
    }
}