using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;

public class PlayerSkillComponent {
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
    
    //Blackmail
    public List<Character> blackmailedCharacters { get; private set; }
    
    //Skill Unlocking
    public PLAYER_SKILL_TYPE currentSpellBeingUnlocked { get; private set; }
    public int currentSpellUnlockCost { get; private set; }
    public RuinarchTimer timerUnlockSpell { get; private set; }
    public RuinarchCooldown cooldownReroll { get; private set; }
    public List<PLAYER_SKILL_TYPE> currentSpellChoices { get; private set; }
        
    public PLAYER_SKILL_TYPE currentDemonBeingSummoned { get; private set; }
    public int currentDemonUnlockCost { get; private set; }
    public RuinarchTimer timerSummonDemon { get; private set; }
        
    public PLAYER_SKILL_TYPE currentStructureBeingUnlocked { get; private set; }
    public int currentStructureUnlockCost { get; private set; }
    public RuinarchTimer timerUnlockStructure { get; private set; }

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
        blackmailedCharacters = new List<Character>();
        //summons = new List<Summon>();
        //canTriggerFlaw = true;
        //canRemoveTraits = true;
        currentSpellBeingUnlocked = PLAYER_SKILL_TYPE.NONE;
        timerUnlockSpell = new RuinarchTimer("Spell Unlock", BOOKMARK_CATEGORY.Portal);
        cooldownReroll = new RuinarchCooldown("Reroll");
        currentSpellChoices = new List<PLAYER_SKILL_TYPE>();
        currentDemonBeingSummoned = PLAYER_SKILL_TYPE.NONE;
        timerSummonDemon = new RuinarchTimer("Summon Demon", BOOKMARK_CATEGORY.Portal);
        currentStructureBeingUnlocked = PLAYER_SKILL_TYPE.NONE;
        timerUnlockStructure = new RuinarchTimer("Obtain Blueprint", BOOKMARK_CATEGORY.Portal);
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
        timerUnlockSpell.Start(GameManager.Instance.Today(), GameManager.Instance.Today().AddDays(1), OnCompleteSpellUnlockTimer);
        timerUnlockSpell.SetOnSelectAction(() => UIManager.Instance.ShowStructureInfo(PlayerManager.Instance.player.playerSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.THE_PORTAL)));
        PlayerManager.Instance.player.bookmarkComponent.AddBookmark(timerUnlockSpell);
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
        Messenger.Broadcast(SpellSignals.PLAYER_GAINED_SPELL, currentSpellBeingUnlocked);
        Messenger.Broadcast(PlayerSignals.PLAYER_FINISHED_SKILL_UNLOCK, currentSpellBeingUnlocked, currentSpellUnlockCost);
        currentSpellBeingUnlocked = PLAYER_SKILL_TYPE.NONE;
        currentSpellUnlockCost = 0;
    }
    public void OnRerollUsed() {
        ResetPlayerSpellChoices();
        cooldownReroll.Start(GameManager.Instance.Today(), GameManager.Instance.Today().AddTicks(GameManager.ticksPerHour));
    }
    public void PlayerChoseMinionToUnlock(PLAYER_SKILL_TYPE p_skillType, int p_unlockCost) {
        currentDemonBeingSummoned = p_skillType;
        currentDemonUnlockCost = p_unlockCost;
        SkillData skillData = PlayerSkillManager.Instance.GetPlayerSkillData(p_skillType);
        timerSummonDemon.SetTimerName($"{LocalizationManager.Instance.GetLocalizedValue("UI", "PortalUI", "summon_demon_active")} {skillData.name}");
        timerSummonDemon.Start(GameManager.Instance.Today(), GameManager.Instance.Today().AddDays(1), OnCompleteMinionUnlock);
        timerSummonDemon.SetOnSelectAction(() => UIManager.Instance.ShowStructureInfo(PlayerManager.Instance.player.playerSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.THE_PORTAL)));
        PlayerManager.Instance.player.bookmarkComponent.AddBookmark(timerSummonDemon);
        Messenger.Broadcast(PlayerSignals.PLAYER_CHOSE_DEMON_TO_UNLOCK, p_skillType, p_unlockCost);
    }
    public void CancelCurrentMinionUnlock() {
        //Refund player mana
        PlayerManager.Instance.player.bookmarkComponent.RemoveBookmark(timerSummonDemon);
        PlayerManager.Instance.player.AdjustMana(currentDemonUnlockCost);
        currentDemonBeingSummoned = PLAYER_SKILL_TYPE.NONE;
        currentDemonUnlockCost = 0;
        timerSummonDemon.Stop();
        Messenger.Broadcast(PlayerSignals.PLAYER_DEMON_UNLOCK_CANCELLED);
    }
    private void OnCompleteMinionUnlock() {
        PlayerManager.Instance.player.bookmarkComponent.RemoveBookmark(timerSummonDemon);
        PlayerManager.Instance.player.playerSkillComponent.SetPlayerSkillData(currentDemonBeingSummoned);
        Messenger.Broadcast(PlayerSignals.PLAYER_FINISHED_DEMON_UNLOCK, currentDemonBeingSummoned, currentDemonUnlockCost);
        currentDemonBeingSummoned = PLAYER_SKILL_TYPE.NONE;
        currentDemonUnlockCost = 0;
    }
    public void PlayerChoseStructureToUnlock(PLAYER_SKILL_TYPE p_skillType, int p_unlockCost) {
        currentStructureBeingUnlocked = p_skillType;
        currentStructureUnlockCost = p_unlockCost;
        SkillData skillData = PlayerSkillManager.Instance.GetPlayerSkillData(p_skillType);
        timerUnlockStructure.SetTimerName($"{LocalizationManager.Instance.GetLocalizedValue("UI", "PortalUI", "obtain_blueprint_active")} {skillData.name}");
        timerUnlockStructure.Start(GameManager.Instance.Today(), GameManager.Instance.Today().AddDays(1), OnCompleteStructureUnlock);
        timerUnlockStructure.SetOnSelectAction(() => UIManager.Instance.ShowStructureInfo(PlayerManager.Instance.player.playerSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.THE_PORTAL)));
        PlayerManager.Instance.player.bookmarkComponent.AddBookmark(timerUnlockStructure);
        Messenger.Broadcast(PlayerSignals.PLAYER_CHOSE_STRUCTURE_TO_UNLOCK, p_skillType, p_unlockCost);
    }
    public void CancelCurrentStructureUnlock() {
        //Refund player mana
        PlayerManager.Instance.player.bookmarkComponent.RemoveBookmark(timerUnlockStructure);
        PlayerManager.Instance.player.AdjustMana(currentStructureUnlockCost);
        currentStructureBeingUnlocked = PLAYER_SKILL_TYPE.NONE;
        currentStructureUnlockCost = 0;
        timerUnlockStructure.Stop();
        Messenger.Broadcast(PlayerSignals.PLAYER_STRUCTURE_UNLOCK_CANCELLED);
    }
    private void OnCompleteStructureUnlock() {
        PlayerManager.Instance.player.bookmarkComponent.RemoveBookmark(timerUnlockStructure);
        PlayerManager.Instance.player.playerSkillComponent.SetPlayerSkillData(currentStructureBeingUnlocked);
        Messenger.Broadcast(SpellSignals.PLAYER_GAINED_DEMONIC_STRUCTURE, currentStructureBeingUnlocked);
        Messenger.Broadcast(UISignals.UPDATE_BUILD_LIST);
        Messenger.Broadcast(PlayerSignals.PLAYER_FINISHED_STRUCTURE_UNLOCK, currentStructureBeingUnlocked, currentStructureUnlockCost);
        currentStructureBeingUnlocked = PLAYER_SKILL_TYPE.NONE;
        currentStructureUnlockCost = 0;
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
    public void LoadPlayerSkillTreeOrLoadout(SaveDataPlayer save) {
        if (PlayerSkillManager.Instance.unlockAllSkills) {
            PopulateDevModeSkills();
            PlayerSkillLoadout loadout = PlayerSkillManager.Instance.GetSelectedLoadout();
            PopulatePassiveSkills(loadout.passiveSkills);
            // PopulatePassiveSkills(PlayerSkillManager.Instance.allPassiveSkillTypes);
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
                shouldAddSpell = PlayerSkillManager.Instance.GetPlayerSkillData(data.skill) != null 
                && data.skill != PLAYER_SKILL_TYPE.OSTRACIZER && data.skill != PLAYER_SKILL_TYPE.SKELETON;
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
    private void PopulateAllSkills(List<PLAYER_SKILL_TYPE> skillTypes) {
        if (skillTypes != null) {
            for (int i = 0; i < skillTypes.Count; i++) {
                PLAYER_SKILL_TYPE spellType = skillTypes[i];
                SetPlayerSkillData(spellType);
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

    #region Blackmail
    /// <summary>
    /// Add a character to the list of characters that the player has
    /// blackmail material on.
    /// </summary>
    /// <param name="p_character">The character to add.</param>
    public void AddCharacterToBlackmailList(Character p_character) {
        blackmailedCharacters.Add(p_character);
    }
    /// <summary>
    /// Remove a character to the list of characters that the player has
    /// blackmail material on.
    /// </summary>
    /// <param name="p_character">The character to remove.</param>
    public void RemoveCharacterFromBlackmailList(Character p_character) {
        blackmailedCharacters.Remove(p_character);
    }
    /// <summary>
    /// Has the player already stored blackmail for a given character.
    /// </summary>
    /// <param name="p_character">The character in question.</param>
    /// <returns>True or false.</returns>
    public bool AlreadyHasBlackmail(Character p_character) {
        return blackmailedCharacters.Contains(p_character) || PlayerManager.Instance.player.HasHostageIntel(p_character);
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
        blackmailedCharacters = SaveUtilities.ConvertIDListToCharacters(data.blackmailedCharacters);
        currentSpellBeingUnlocked = data.currentSpellBeingUnlocked;
        currentSpellUnlockCost = data.currentSpellUnlockCost;
        timerUnlockSpell = data.timerUnlockSpell;
        if (currentSpellBeingUnlocked != PLAYER_SKILL_TYPE.NONE) {
            timerUnlockSpell.LoadStart(OnCompleteSpellUnlockTimer);
        }
        cooldownReroll = data.cooldownReroll;
        if (!cooldownReroll.IsFinished()) {
            cooldownReroll.LoadStart();
        }
        currentDemonBeingSummoned = data.currentDemonBeingSummoned;
        currentDemonUnlockCost = data.currentDemonUnlockCost;
        timerSummonDemon = data.timerSummonDemon;
        if (currentDemonBeingSummoned != PLAYER_SKILL_TYPE.NONE) {
            timerSummonDemon.LoadStart(OnCompleteMinionUnlock);
        }
        currentStructureBeingUnlocked = data.currentStructureBeingUnlocked;
        currentStructureUnlockCost = data.currentStructureUnlockCost;
        timerUnlockStructure = data.timerUnlockStructure;
        if (currentSpellBeingUnlocked != PLAYER_SKILL_TYPE.NONE) {
            timerUnlockStructure.LoadStart(OnCompleteStructureUnlock);
        }
        currentSpellChoices = data.currentSpellChoices;
    }
    #endregion
}
[System.Serializable]
public class SaveDataPlayerSkillComponent : SaveData<PlayerSkillComponent> {
    public List<SaveDataPlayerSkill> skills;
    public List<string> blackmailedCharacters;
    //public bool canTriggerFlaw;
    //public bool canRemoveTraits;
    //Skill Unlocking
    public PLAYER_SKILL_TYPE currentSpellBeingUnlocked;
    public int currentSpellUnlockCost;
    public RuinarchTimer timerUnlockSpell;
    public RuinarchCooldown cooldownReroll;
    public List<PLAYER_SKILL_TYPE> currentSpellChoices;
    public PLAYER_SKILL_TYPE currentDemonBeingSummoned;
    public int currentDemonUnlockCost;
    public RuinarchTimer timerSummonDemon;
    public PLAYER_SKILL_TYPE currentStructureBeingUnlocked;
    public int currentStructureUnlockCost;
    public RuinarchTimer timerUnlockStructure;

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

        blackmailedCharacters = SaveUtilities.ConvertSavableListToIDs(component.blackmailedCharacters);
        
        currentSpellBeingUnlocked = component.currentSpellBeingUnlocked;
        currentSpellUnlockCost = component.currentSpellUnlockCost;
        timerUnlockSpell = component.timerUnlockSpell;
        cooldownReroll = component.cooldownReroll;
        currentSpellChoices = component.currentSpellChoices;
        currentDemonBeingSummoned = component.currentDemonBeingSummoned;
        currentDemonUnlockCost = component.currentDemonUnlockCost;
        timerSummonDemon = component.timerSummonDemon;
        currentStructureBeingUnlocked = component.currentStructureBeingUnlocked;
        currentStructureUnlockCost = component.currentStructureUnlockCost;
        timerUnlockStructure = component.timerUnlockStructure;
    }
    public override PlayerSkillComponent Load() {
        PlayerSkillComponent component = new PlayerSkillComponent();
        component.LoadSkills(skills);
        return component;
    }
}