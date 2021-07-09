using System;
using System.Collections;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;

public class PlayerSkillComponent {

    public const int RerollCooldownInHours = 6;
    public const int GetBonusChargeCooldownInHours = 6;

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
    public GenericTextBookmarkable spellUnlockedBookmark { get; private set; }
    public string lastSpellUnlockSummary { get; private set; }
    public bool isSpellUnlockedBookmarked { get; private set; }
    // public RuinarchTimer cooldownReroll { get; private set; }
    public List<PLAYER_SKILL_TYPE> currentSpellChoices { get; private set; }

    public Cost[] currentPortalUpgradeCost { get; private set; }
    public RuinarchTimer timerUpgradePortal { get; private set; }
    public GenericTextBookmarkable previousPortalUpgradedBookmark { get; private set; }
    public string lastPortalUpgradeSummary { get; private set; }
    public bool isPreviousPortalUpgradeBookmarked { get; private set; }

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
        spellUnlockedBookmark = new GenericTextBookmarkable(GetSpellUnlockedString, () => BOOKMARK_TYPE.Special, OnSelectSpellUnlockedBookmark, RemoveSpellUnlockedBookmark, null, null);
        // cooldownReroll = new RuinarchTimer("Reroll");
        currentSpellChoices = new List<PLAYER_SKILL_TYPE>();
        timerUpgradePortal = new RuinarchTimer("Summon Demon");
        previousPortalUpgradedBookmark = new GenericTextBookmarkable(GetPortalUpgradedSummary, () => BOOKMARK_TYPE.Special, OnSelectPortalUpgradedBookmark, RemovePortalUpgradedBookmark, null, null);
        
        timerUnlockSpell.SetOnHoverOverAction(OnHoverOverReleaseAbilitiesBookmark);
        timerUnlockSpell.SetOnHoverOutAction(OnHoverOutReleaseAbilitiesBookmark);
        
        timerUpgradePortal.SetOnHoverOverAction(OnHoverOverUpgradePortalBookmark);
        timerUpgradePortal.SetOnHoverOutAction(OnHoverOutUpgradePortalBookmark);

        Messenger.AddListener<LocationStructure>(StructureSignals.STRUCTURE_OBJECT_PLACED, OnStructurePlaced);
        Messenger.AddListener<LocationStructure>(StructureSignals.STRUCTURE_DESTROYED, OnStructureDestroyed);
        Messenger.AddListener<SkillData>(PlayerSkillSignals.BONUS_CHARGES_ADJUSTED, OnBonusChargesAdjusted);
    }

    #region Listeners
    void OnStructurePlaced(LocationStructure p_structure) {
        if (p_structure.structureType == STRUCTURE_TYPE.BIOLAB) {
            UnlockPlagueSkills();
        }
    }

    void OnStructureDestroyed(LocationStructure p_structure) {
        if (p_structure.structureType == STRUCTURE_TYPE.BIOLAB) {
            if (!PlayerManager.Instance.player.playerSettlement.HasStructure(STRUCTURE_TYPE.BIOLAB)) {
                LockPlagueSkills();
            }
        }
    }
    private void OnBonusChargesAdjusted(SkillData p_skillData) {
        if (p_skillData.hasBonusCharges) {
            AddAndCategorizeTemporaryPlayerSkill(p_skillData);
        } else {
            if (!p_skillData.isInUse) {
                RemovePlayerSkill(p_skillData);
            }
        }
    }
    #endregion

    void UnlockPlagueSkills() {
        SkillData skilldata = PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.PLAGUED_RAT);
        skilldata.SetIsUnlockBaseOnRequirements(true);
        //AddCharges(skilldata.type, 1);
        AddAndCategorizePlayerSkill(skilldata);
        skilldata = PlayerSkillManager.Instance.GetAfflictionData(PLAYER_SKILL_TYPE.PLAGUE);
        skilldata.SetIsUnlockBaseOnRequirements(true);
        //AddCharges(skilldata.type, 1);
        AddAndCategorizePlayerSkill(skilldata);
        //Messenger.Broadcast(PlayerSkillSignals.PLAYER_GAINED_SPELL, PLAYER_SKILL_TYPE.PLAGUED_RAT);
    }

    void LockPlagueSkills() {
        SkillData skilldata = PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.PLAGUE);
        skilldata.SetIsUnlockBaseOnRequirements(false);
        skilldata.ResetData();
        RemovePlayerSkill(skilldata);
        skilldata = PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.PLAGUED_RAT);
        skilldata.SetIsUnlockBaseOnRequirements(false);
        //skilldata.ResetData();
        RemovePlayerSkill(skilldata);
        //Messenger.Broadcast(PlayerSkillSignals.PLAYER_LOST_SPELL, PLAYER_SKILL_TYPE.PLAGUED_RAT);
    }

    #region Loading
    public void LoadSkills(List<SaveDataSkillData> data) {
        if (data != null) {
            for (int i = 0; i < data.Count; i++) {
                SkillData skillData = data[i].Load();
                AddAndCategorizePlayerSkillBase(skillData); //Use Base function because we do not need to SetPlayerSkillData since it is already loaded
            }
        }
    }
    #endregion

    #region Unlocking
    public void PlayerChoseSkillToAddBonusCharge(SkillData p_skillData, int p_unlockCost) {
        currentSpellBeingUnlocked = p_skillData.type;
        currentSpellUnlockCost = p_unlockCost;
        timerUnlockSpell.SetTimerName($"{LocalizationManager.Instance.GetLocalizedValue("UI", "PortalUI", "release_ability_active")} {p_skillData.localizedName}");
        timerUnlockSpell.Start(GameManager.Instance.Today(), GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(GetBonusChargeCooldownInHours)), OnCompleteSpellUnlockTimer); //.AddDays(1)
        timerUnlockSpell.SetOnSelectAction(() => UIManager.Instance.ShowStructureInfo(PlayerManager.Instance.player.playerSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.THE_PORTAL)));
        PlayerManager.Instance.player.bookmarkComponent.AddBookmark(timerUnlockSpell, BOOKMARK_CATEGORY.Portal);
        if (isSpellUnlockedBookmarked) {
            RemoveSpellUnlockedBookmark();
        }
        Messenger.Broadcast(PlayerSignals.PLAYER_CHOSE_SKILL_TO_UNLOCK, p_skillData, p_unlockCost);
    }
    public void CancelCurrentPlayerSkillUnlock() {
        //Refund player Chaotic Energy
        // PlayerManager.Instance.player.plagueComponent.AdjustPlaguePoints(currentSpellUnlockCost);
        //This is so that refunding will not affect spirit energy
        PlayerManager.Instance.player.plagueComponent.AdjustPlaguePointsWithoutAffectingSpiritEnergy(currentSpellUnlockCost);
        currentSpellBeingUnlocked = PLAYER_SKILL_TYPE.NONE;
        currentSpellUnlockCost = 0;
        timerUnlockSpell.Stop();
        PlayerManager.Instance.player.bookmarkComponent.RemoveBookmark(timerUnlockSpell);
        Messenger.Broadcast(PlayerSignals.PLAYER_SKILL_UNLOCK_CANCELLED);
    }
    private void OnCompleteSpellUnlockTimer() {
        SkillData skillData = PlayerSkillManager.Instance.GetSkillData(currentSpellBeingUnlocked);
        PlayerSkillData playerSkillData = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(currentSpellBeingUnlocked);
        skillData.AdjustBonusCharges(playerSkillData.bonusChargeWhenUnlocked);
        ResetPlayerSpellChoices();
        Messenger.Broadcast(PlayerSignals.PLAYER_FINISHED_SKILL_UNLOCK, currentSpellBeingUnlocked, currentSpellUnlockCost);
        ProduceLogForUnlockedSkills(skillData, playerSkillData);
        currentSpellBeingUnlocked = PLAYER_SKILL_TYPE.NONE;
        currentSpellUnlockCost = 0;
        
        string chargeText = playerSkillData.bonusChargeWhenUnlocked == 1 ? "charge" : "charges";
        lastSpellUnlockSummary = $"Gained {playerSkillData.bonusChargeWhenUnlocked.ToString()}{UtilityScripts.Utilities.BonusChargesIcon()} <b>{skillData.localizedName}</b>";
        AddSpellUnlockedBookmark();
        PlayerManager.Instance.player.bookmarkComponent.RemoveBookmark(timerUnlockSpell);
    }
    private void ProduceLogForUnlockedSkills(SkillData p_skillData, PlayerSkillData p_playerSkillData) {
        Log m_log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Skills", "Unlock Skill", "skill_unlocked", providedTags: LOG_TAG.Player);
        m_log.AddTag(LOG_TAG.Major);
        string chargeText = p_playerSkillData.bonusChargeWhenUnlocked == 1 ? "charge" : "charges";
        m_log.AddToFillers(null, $"{p_playerSkillData.bonusChargeWhenUnlocked.ToString()} {chargeText}", LOG_IDENTIFIER.STRING_1);
        m_log.AddToFillers(null, p_skillData.localizedName, LOG_IDENTIFIER.STRING_2);
        m_log.AddLogToDatabase();
        PlayerManager.Instance.player.ShowNotificationFromPlayer(m_log, true);
    }
    public void OnRerollUsed() {
        ResetPlayerSpellChoices();
        // cooldownReroll.Start(GameManager.Instance.Today(), GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(RerollCooldownInHours)), OnRerollCooldownFinished);
    }
    private void OnRerollCooldownFinished() {
        Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "UI", "PortalUI", "reroll_available", null, LOG_TAG.Major);
        log.AddLogToDatabase();
        PlayerManager.Instance.player.ShowNotificationFromPlayer(log, true);
    }
    public void PlayerStartedPortalUpgrade(Cost[] p_upgradeCost, PortalUpgradeTier p_upgradeTier) {
        currentPortalUpgradeCost = p_upgradeCost;
        ThePortal portal = PlayerManager.Instance.player.playerSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.THE_PORTAL) as ThePortal;
        timerUpgradePortal.SetTimerName($"{LocalizationManager.Instance.GetLocalizedValue("UI", "PortalUI", "upgrade_portal_active")} {(portal.level + 1).ToString()}");
        timerUpgradePortal.Start(GameManager.Instance.Today(), GameManager.Instance.Today().AddTicks(p_upgradeTier.upgradeTime), OnCompletePortalUpgrade);
        timerUpgradePortal.SetOnSelectAction(() => UIManager.Instance.ShowStructureInfo(portal));
        PlayerManager.Instance.player.bookmarkComponent.AddBookmark(timerUpgradePortal, BOOKMARK_CATEGORY.Portal);
        if (isPreviousPortalUpgradeBookmarked) { RemovePortalUpgradedBookmark(); }
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
        ThePortal portal = PlayerManager.Instance.player.playerSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.THE_PORTAL) as ThePortal;
        portal.GainUpgradePowers(portal.nextTier);
        portal.IncreaseLevel();
        ProduceLogForPortalUpgrade(portal.level);
        Messenger.Broadcast(PlayerSignals.PLAYER_FINISHED_PORTAL_UPGRADE, portal.level);
        Messenger.Broadcast(PlayerSkillSignals.FORCE_RELOAD_PLAYER_ACTIONS);
        currentPortalUpgradeCost = null;

        lastPortalUpgradeSummary = $"Portal upgraded to Level {portal.level.ToString()}!";
        AddPortalUpgradedBookmark();
        PlayerManager.Instance.player.bookmarkComponent.RemoveBookmark(timerUpgradePortal);
    }

    void ProduceLogForPortalUpgrade(int level) {
        Log m_log = GameManager.CreateNewLog(GameManager.Instance.Today(), "General", "Player", "player_portal_upgraded", providedTags: LOG_TAG.Player);
        m_log.AddTag(LOG_TAG.Major);
        m_log.AddToFillers(null, "Level " + level.ToString(), LOG_IDENTIFIER.STRING_1);
        m_log.AddLogToDatabase();
        PlayerManager.Instance.player.ShowNotificationFromPlayer(m_log, true);
    }

    private void ResetPlayerSpellChoices() {
        currentSpellChoices.Clear();
#if DEBUG_LOG
        Debug.Log("Reset player spell choices.");
#endif
    }
    public void AddCurrentPlayerSpellChoice(PLAYER_SKILL_TYPE p_skillType) {
        currentSpellChoices.Add(p_skillType);
    }
    private void OnHoverOverReleaseAbilitiesBookmark(UIHoverPosition position) {
        string text = $"Will finish on {timerUnlockSpell.GetTimerEndString()}.";
        UIManager.Instance.ShowSmallInfo(text, position);
    }
    private void OnHoverOutReleaseAbilitiesBookmark() {
        UIManager.Instance.HideSmallInfo();
    }
    private void OnHoverOverUpgradePortalBookmark(UIHoverPosition position) {
        string text = $"Will finish on {timerUpgradePortal.GetTimerEndString()}.";
        UIManager.Instance.ShowSmallInfo(text, position);
    }
    private void OnHoverOutUpgradePortalBookmark() {
        UIManager.Instance.HideSmallInfo();
    }
    #endregion

    #region Skill Tree
    //private void AddPlayerSkill(SkillData spellData, int charges, int manaCost, int cooldown, int threat, int threatPerHour, float pierce) {
    //    PlayerSkillData playerSkillData = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(spellData.type);
    //    if (playerSkillData != null) {
    //        spellData.SetCurrentLevel(playerSkillData.cheatedLevel);
    //        spellData.SetMaxCharges(playerSkillData.GetMaxChargesBaseOnLevel(spellData.currentLevel));
    //        spellData.SetCharges(charges);
    //        spellData.SetCooldown(playerSkillData.GetCoolDownBaseOnLevel(spellData.currentLevel));
    //        spellData.SetPierce(PlayerSkillManager.Instance.GetAdditionalPiercePerLevelBaseOnLevel(spellData.type));
    //        spellData.SetUnlockCost(playerSkillData.unlockCost);
    //        spellData.SetManaCost(playerSkillData.GetManaCostBaseOnLevel(spellData.currentLevel));
    //        spellData.SetThreat(threat);
    //        spellData.SetThreatPerHour(threatPerHour);
    //    } else {
    //        spellData.SetCurrentLevel(1);
    //        spellData.SetMaxCharges(charges);
    //        spellData.SetCharges(charges);
    //        spellData.SetCooldown(cooldown);
    //        spellData.SetPierce(pierce);
    //        spellData.SetUnlockCost(0);
    //        spellData.SetManaCost(manaCost);
    //        spellData.SetThreat(threat);
    //        spellData.SetThreatPerHour(threatPerHour);
    //    }
    //    // Debug.LogError(spellData.name + " -- " + spellData.currentLevel + " -- " + playerSkillData.cheatedLevel);
    //    AddAndCategorizePlayerSkill(spellData);
    //}
    //public void AddCharges(PLAYER_SKILL_TYPE spellType, int amount) {
    //    SkillData spellData = PlayerSkillManager.Instance.GetSkillData(spellType);
    //    if (spellData.isInUse) {
    //        spellData.AdjustCharges(amount);
    //    } else {
    //        AddPlayerSkill(spellData, amount, -1, -1, 0, 0, 0);
    //        var playerSkillData = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(spellData.type);
    //        if (playerSkillData != null) {
    //            UpdateTierCount(playerSkillData);    
    //        }
    //    }
    //}
    //public void AddMaxCharges(PLAYER_SKILL_TYPE spellType, int amount) {
    //    SkillData spellData = PlayerSkillManager.Instance.GetSkillData(spellType);
    //    if (spellData.isInUse) {
    //        spellData.AdjustMaxCharges(amount);
    //        spellData.AdjustCharges(amount);
    //    } else {
    //        AddPlayerSkill(spellData, amount, -1, -1, 0, 0, 0);
    //        var playerSkillData = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(spellData.type);
    //        if (playerSkillData != null) {
    //            AddTierCount(playerSkillData);
    //        }
    //    }
    //}
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

    public void AddTierCount(PlayerSkillData playerSkillData) {
        switch (playerSkillData.tier) {
            case 1: tier1Count++;
                break;
            case 2: tier2Count++;
                break;
            case 3: tier3Count++;
                break;
        }
    }
    public void RemoveTierCount(PlayerSkillData playerSkillData) {
        switch (playerSkillData.tier) {
            case 1:
                tier1Count--;
                break;
            case 2:
                tier2Count--;
                break;
            case 3:
                tier3Count--;
                break;
        }
    }
    public int GetLevelOfSkill(SkillData p_targetSkill) {
        int currentLevel = 0;
        switch (p_targetSkill.category) {
            case PLAYER_SKILL_CATEGORY.PLAYER_ACTION:
                playerActions.ForEach((eachSkill) => {
                    if (eachSkill == p_targetSkill.type) {
                        currentLevel = PlayerSkillManager.Instance.GetSkillData(eachSkill).currentLevel;
                    }
                });
                break;
            case PLAYER_SKILL_CATEGORY.SPELL:
                spells.ForEach((eachSkill) => {
                    if (eachSkill == p_targetSkill.type) {
                        currentLevel = PlayerSkillManager.Instance.GetSkillData(eachSkill).currentLevel;
                    }
                });
                break;
            case PLAYER_SKILL_CATEGORY.AFFLICTION:
                afflictions.ForEach((eachSkill) => {
                    if (eachSkill == p_targetSkill.type) {
                        currentLevel = PlayerSkillManager.Instance.GetSkillData(eachSkill).currentLevel;
                    }
                });
                break;
        }

        return currentLevel;
    }

    #region Utilities
    public bool CheckIfSkillIsAvailable(PLAYER_SKILL_TYPE p_targetSkill) {
        SkillData skillData = PlayerSkillManager.Instance.GetSkillData(p_targetSkill);
        return skillData.isInUse;
        //if (spells.Contains(p_targetSkill)) {
        //    return true;
        //}
        //if (playerActions.Contains(p_targetSkill)) {
        //    return true;
        //}
        //if (afflictions.Contains(p_targetSkill)) {
        //    return true;
        //}
        //return false;
    }
    public bool CanDoPlayerAction(PLAYER_SKILL_TYPE type) {
        PlayerAction data = PlayerSkillManager.Instance.GetPlayerActionData(type);
        return data.isInUse || data.isTemporarilyInUse;
    }
    public bool CanBuildDemonicStructure(PLAYER_SKILL_TYPE type) {
        DemonicStructurePlayerSkill data = PlayerSkillManager.Instance.GetDemonicStructureSkillData(type);
        return data.isInUse || data.isTemporarilyInUse;
    }
    public bool HasAnyAvailableAffliction() {
        for (int i = 0; i < afflictions.Count; i++) {
            SkillData spellData = PlayerSkillManager.Instance.GetSkillData(afflictions[i]);
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
    public bool HasAfflictions() {
        return afflictions.Count > 0;
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
            //filtered defiler because of this task:
            //https://trello.com/c/Y91OZehr/4614-defiler-naka-unlock-sa-omnipotent
            shouldAddSpell = PlayerSkillManager.Instance.GetSkillData(data.skill) != null
            && data.skill != PLAYER_SKILL_TYPE.OSTRACIZER && data.skill != PLAYER_SKILL_TYPE.SKELETON && data.skill != PLAYER_SKILL_TYPE.DEFILER;
            // }
            if (shouldAddSpell) {
                AddAndCategorizePlayerSkill(data.skill, false, true);
                if (scenarioData != null) {
                    //set level provided by scenario data
                    int providedLevel = scenarioData.GetLevelForPower(data.skill);
                    SkillData skillData = PlayerSkillManager.Instance.GetSkillData(data.skill);
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
                AddAndCategorizePlayerSkill(spellType);
                if (scenarioData != null) {
                    //set level provided by scenario data
                    int providedLevel = scenarioData.GetLevelForPower(spellType);
                    SkillData skillData = PlayerSkillManager.Instance.GetSkillData(spellType);
                    for (int j = 0; j < providedLevel; j++) {
                        skillData.LevelUp();
                    }
                }
            }
        }
    }
    public void AddAndCategorizePlayerSkill(PLAYER_SKILL_TYPE p_skillType, bool testScene = false, bool isDevMode = false) {
        SkillData skillData = PlayerSkillManager.Instance.GetSkillData(p_skillType);
        AddAndCategorizePlayerSkill(skillData, testScene, isDevMode);
    }
    public void AddAndCategorizePlayerSkill(SkillData p_skillData, bool testScene = false, bool isDevMode = false) {
        Assert.IsNotNull(p_skillData, "Given spell data in AddAndCategorizePlayerSkill is null!");
        if (p_skillData.isTemporarilyInUse) {
            p_skillData.SetIsInUse(true);
        } else {
            if (!p_skillData.isInUse) {
                p_skillData.SetIsInUse(true);
                SetPlayerSkillData(p_skillData, testScene, isDevMode);
                AddAndCategorizePlayerSkillBase(p_skillData);
            }
        }
    }
    public void AddAndCategorizeTemporaryPlayerSkill(SkillData p_skillData, bool testScene = false, bool isDevMode = false) {
        Assert.IsNotNull(p_skillData, "Given spell data in AddAndCategorizeTemporaryPlayerSkill is null!");
        if (!p_skillData.isTemporarilyInUse && !p_skillData.isInUse) {
            p_skillData.SetIsTemporarilyInUse(true);
            SetPlayerSkillData(p_skillData, testScene, isDevMode);
            AddAndCategorizePlayerSkillBase(p_skillData);
        }
    }
    public void RemovePlayerSkill(PLAYER_SKILL_TYPE p_skillType) {
        SkillData skillData = PlayerSkillManager.Instance.GetSkillData(p_skillType);
        RemovePlayerSkill(skillData);
    }
    public void RemovePlayerSkill(SkillData p_skillData) {
        Assert.IsNotNull(p_skillData, "Given spell data in RemovePlayerSkill is null!");
        if (p_skillData.hasBonusCharges) {
            p_skillData.SetIsInUse(false);
            p_skillData.SetIsTemporarilyInUse(true);
        } else {
            RemovePlayerSkillBase(p_skillData);
            p_skillData.ResetData();
        }
    }
    private void AddAndCategorizePlayerSkillBase(SkillData p_skillData) {
        if (p_skillData.category == PLAYER_SKILL_CATEGORY.AFFLICTION) {
            afflictions.Add(p_skillData.type);
        } else if (p_skillData.category == PLAYER_SKILL_CATEGORY.SCHEME) {
            schemes.Add(p_skillData.type);
        } else if (p_skillData.category == PLAYER_SKILL_CATEGORY.DEMONIC_STRUCTURE) {
            demonicStructuresSkills.Add(p_skillData.type);
            Messenger.Broadcast(PlayerSkillSignals.PLAYER_GAINED_DEMONIC_STRUCTURE, p_skillData.type);
        } else if (p_skillData.category == PLAYER_SKILL_CATEGORY.MINION) {
            minionsSkills.Add(p_skillData.type);
            Messenger.Broadcast(PlayerSkillSignals.ADDED_PLAYER_MINION_SKILL, p_skillData.type);
        } else if (p_skillData.category == PLAYER_SKILL_CATEGORY.PLAYER_ACTION) {
            playerActions.Add(p_skillData.type);
        } else if (p_skillData.category == PLAYER_SKILL_CATEGORY.SPELL) {
            spells.Add(p_skillData.type);
            Messenger.Broadcast(PlayerSkillSignals.PLAYER_GAINED_SPELL, p_skillData.type);
        } else if (p_skillData.category == PLAYER_SKILL_CATEGORY.SUMMON) {
            summonsSkills.Add(p_skillData.type);
            Messenger.Broadcast(PlayerSkillSignals.ADDED_PLAYER_SUMMON_SKILL, p_skillData.type);
        }
    }
    private void RemovePlayerSkillBase(SkillData p_skillData) {
        if (p_skillData.category == PLAYER_SKILL_CATEGORY.AFFLICTION) {
            afflictions.Remove(p_skillData.type);
        } else if (p_skillData.category == PLAYER_SKILL_CATEGORY.SCHEME) {
            schemes.Remove(p_skillData.type);
        } else if (p_skillData.category == PLAYER_SKILL_CATEGORY.DEMONIC_STRUCTURE) {
            demonicStructuresSkills.Remove(p_skillData.type);
            Messenger.Broadcast(PlayerSkillSignals.PLAYER_LOST_DEMONIC_STRUCTURE, p_skillData.type);
        } else if (p_skillData.category == PLAYER_SKILL_CATEGORY.MINION) {
            minionsSkills.Remove(p_skillData.type);
        } else if (p_skillData.category == PLAYER_SKILL_CATEGORY.PLAYER_ACTION) {
            playerActions.Remove(p_skillData.type);
        } else if (p_skillData.category == PLAYER_SKILL_CATEGORY.SPELL) {
            spells.Remove(p_skillData.type);
            Messenger.Broadcast(PlayerSkillSignals.PLAYER_LOST_SPELL, p_skillData.type);
        } else if (p_skillData.category == PLAYER_SKILL_CATEGORY.SUMMON) {
            summonsSkills.Remove(p_skillData.type);
        }
    }
    private void SetPlayerSkillData(SkillData p_skillData, bool testScene, bool isDevMode) {
        PlayerSkillData playerSkillData = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(p_skillData.type);
        if (playerSkillData == null) {
            Debug.LogError(playerSkillData.skill.ToString() + " data is null!");
        }
        SetPlayerSkillDataBase(p_skillData, playerSkillData, testScene, isDevMode);
    }
    private void SetPlayerSkillDataBase(SkillData p_skillData, PlayerSkillData p_playerSkillData, bool testScene, bool isDevMode) {
        if (p_skillData == null) {
            Debug.LogError(p_playerSkillData.skill.ToString() + " data is null!");
        }
        if (!isDevMode && !testScene && WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Tutorial &&
            p_skillData.type == PLAYER_SKILL_TYPE.WATCHER) {
            //if map is tutorial and spell is THE_EYE, Set max charges to only 1
            p_skillData.SetMaxCharges(1);
            p_skillData.SetCharges(1);
        } else {
            p_skillData.SetMaxCharges(p_playerSkillData.GetMaxChargesBaseOnLevel(p_skillData.currentLevel));
            p_skillData.SetCharges(p_skillData.maxCharges);
        }
        p_skillData.SetCooldown(p_playerSkillData.GetCoolDownBaseOnLevel(p_skillData.currentLevel));
        p_skillData.SetPierce(PlayerSkillManager.Instance.GetAdditionalPiercePerLevelBaseOnLevel(p_skillData.type));
        p_skillData.SetUnlockCost(p_playerSkillData.GetUnlockCost());
        p_skillData.SetManaCost(WorldSettings.Instance.worldSettingsData.IsScenarioMap() ? p_playerSkillData.GetManaCostForScenarios() : p_playerSkillData.GetManaCostBaseOnLevel(p_skillData.currentLevel));
        p_skillData.SetThreat(p_playerSkillData.threat);
        p_skillData.SetThreatPerHour(p_playerSkillData.threatPerHour);
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
            AddPassiveSkills(passiveSkillType);
        }
    }
    public void AddPassiveSkills(PASSIVE_SKILL passiveSkills) {
        PassiveSkill passiveSkill = PlayerSkillManager.Instance.GetPassiveSkill(passiveSkills);
        passiveSkill.ActivateSkill();
        this.passiveSkills.Add(passiveSkills);
#if DEBUG_LOG
        Debug.Log($"{GameManager.Instance.TodayLogString()}Activated passive skill {passiveSkills.ToString()}.");
#endif
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

#region Bookmarks
    private string GetSpellUnlockedString() {
        return lastSpellUnlockSummary;
    }
    private void OnSelectSpellUnlockedBookmark() {
        UIManager.Instance.ShowPurchaseSkillUI();
    }
    private void RemoveSpellUnlockedBookmark() {
        isSpellUnlockedBookmarked = false;
        PlayerManager.Instance.player.bookmarkComponent.RemoveBookmark(spellUnlockedBookmark);
    }
    private void AddSpellUnlockedBookmark() {
        isSpellUnlockedBookmarked = true;
        PlayerManager.Instance.player.bookmarkComponent.AddBookmark(spellUnlockedBookmark, BOOKMARK_CATEGORY.Portal);
    }
    private string GetPortalUpgradedSummary() {
        return lastPortalUpgradeSummary;
    }
    private void OnSelectPortalUpgradedBookmark() {
        UIManager.Instance.ShowUpgradePortalUI(PlayerManager.Instance.player.playerSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.THE_PORTAL) as ThePortal);
    }
    private void RemovePortalUpgradedBookmark() {
        isPreviousPortalUpgradeBookmarked = false;
        PlayerManager.Instance.player.bookmarkComponent.RemoveBookmark(previousPortalUpgradedBookmark);
    }
    private void AddPortalUpgradedBookmark() {
        isPreviousPortalUpgradeBookmarked = true;
        PlayerManager.Instance.player.bookmarkComponent.AddBookmark(previousPortalUpgradedBookmark, BOOKMARK_CATEGORY.Portal);
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
        LoadSkills(data.skills);
        currentSpellBeingUnlocked = data.currentSpellBeingUnlocked;
        currentSpellUnlockCost = data.currentSpellUnlockCost;
        timerUnlockSpell = data.timerUnlockSpell;
        timerUnlockSpell.SetOnHoverOverAction(OnHoverOverReleaseAbilitiesBookmark);
        timerUnlockSpell.SetOnHoverOutAction(OnHoverOutReleaseAbilitiesBookmark);
        if (currentSpellBeingUnlocked != PLAYER_SKILL_TYPE.NONE) {
            timerUnlockSpell.LoadStart(OnCompleteSpellUnlockTimer);
            timerUnlockSpell.SetOnSelectAction(() => UIManager.Instance.ShowStructureInfo(PlayerManager.Instance.player.playerSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.THE_PORTAL)));
            PlayerManager.Instance.player.bookmarkComponent.AddBookmark(timerUnlockSpell, BOOKMARK_CATEGORY.Portal);
        }
        lastSpellUnlockSummary = data.lastSpellUnlockSummary;
        isSpellUnlockedBookmarked = data.isSpellUnlockedBookmarked;
        if (isSpellUnlockedBookmarked) {
            PlayerManager.Instance.player.bookmarkComponent.AddBookmark(spellUnlockedBookmark, BOOKMARK_CATEGORY.Portal);
        }
        
        // cooldownReroll = data.cooldownReroll;
        // if (!cooldownReroll.IsFinished()) {
        //     cooldownReroll.LoadStart();
        // }
        currentPortalUpgradeCost = data.currentPortalUpgradeCost;
        timerUpgradePortal = data.timerUpgradePortal;
        timerUpgradePortal.SetOnHoverOverAction(OnHoverOverUpgradePortalBookmark);
        timerUpgradePortal.SetOnHoverOutAction(OnHoverOutUpgradePortalBookmark);
        if (currentPortalUpgradeCost != null) {
            timerUpgradePortal.LoadStart(OnCompletePortalUpgrade);
            timerUpgradePortal.SetOnSelectAction(() => UIManager.Instance.ShowStructureInfo(PlayerManager.Instance.player.playerSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.THE_PORTAL)));
            PlayerManager.Instance.player.bookmarkComponent.AddBookmark(timerUpgradePortal, BOOKMARK_CATEGORY.Portal);
        }
        lastPortalUpgradeSummary = data.lastPortalUpgradeSummary;
        isPreviousPortalUpgradeBookmarked = data.isPreviousPortalUpgradeBookmarked;
        if (isPreviousPortalUpgradeBookmarked) {
            PlayerManager.Instance.player.bookmarkComponent.AddBookmark(previousPortalUpgradedBookmark, BOOKMARK_CATEGORY.Portal);
        }
        
        currentSpellChoices = data.currentSpellChoices;
    }
#endregion
}
[System.Serializable]
public class SaveDataPlayerSkillComponent : SaveData<PlayerSkillComponent> {
    public List<SaveDataSkillData> skills;
    //public bool canTriggerFlaw;
    //public bool canRemoveTraits;
    //Skill Unlocking
    public PLAYER_SKILL_TYPE currentSpellBeingUnlocked;
    public int currentSpellUnlockCost;
    public RuinarchTimer timerUnlockSpell;
    public string lastSpellUnlockSummary;
    public bool isSpellUnlockedBookmarked;
    public RuinarchTimer cooldownReroll;
    public List<PLAYER_SKILL_TYPE> currentSpellChoices;
    public Cost[] currentPortalUpgradeCost;
    public RuinarchTimer timerUpgradePortal;
    public string lastPortalUpgradeSummary;
    public bool isPreviousPortalUpgradeBookmarked;

    public override void Save(PlayerSkillComponent component) {
        //canTriggerFlaw = player.playerSkillComponent.canTriggerFlaw;
        //canRemoveTraits = player.playerSkillComponent.canRemoveTraits;

        skills = new List<SaveDataSkillData>();
        for (int i = 0; i < component.spells.Count; i++) {
            SkillData spell = PlayerSkillManager.Instance.GetSpellData(component.spells[i]);
            SaveDataSkillData dataPlayerSkill = new SaveDataSkillData();
            dataPlayerSkill.Save(spell);
            skills.Add(dataPlayerSkill);
        }
        for (int i = 0; i < component.afflictions.Count; i++) {
            SkillData spell = PlayerSkillManager.Instance.GetAfflictionData(component.afflictions[i]);
            SaveDataSkillData dataPlayerSkill = new SaveDataSkillData();
            dataPlayerSkill.Save(spell);
            skills.Add(dataPlayerSkill);
        }
        for (int i = 0; i < component.schemes.Count; i++) {
            SkillData spell = PlayerSkillManager.Instance.GetSchemeData(component.schemes[i]);
            SaveDataSkillData dataPlayerSkill = new SaveDataSkillData();
            dataPlayerSkill.Save(spell);
            skills.Add(dataPlayerSkill);
        }
        for (int i = 0; i < component.playerActions.Count; i++) {
            PlayerAction spell = PlayerSkillManager.Instance.GetPlayerActionData(component.playerActions[i]);
            SaveDataSkillData dataPlayerSkill = new SaveDataSkillData();
            dataPlayerSkill.Save(spell);
            skills.Add(dataPlayerSkill);
        }
        for (int i = 0; i < component.demonicStructuresSkills.Count; i++) {
            DemonicStructurePlayerSkill spell = PlayerSkillManager.Instance.GetDemonicStructureSkillData(component.demonicStructuresSkills[i]);
            SaveDataSkillData dataPlayerSkill = new SaveDataSkillData();
            dataPlayerSkill.Save(spell);
            skills.Add(dataPlayerSkill);
        }
        for (int i = 0; i < component.minionsSkills.Count; i++) {
            MinionPlayerSkill spell = PlayerSkillManager.Instance.GetMinionPlayerSkillData(component.minionsSkills[i]);
            SaveDataSkillData dataPlayerSkill = new SaveDataSkillData();
            dataPlayerSkill.Save(spell);
            skills.Add(dataPlayerSkill);
        }
        for (int i = 0; i < component.summonsSkills.Count; i++) {
            SummonPlayerSkill spell = PlayerSkillManager.Instance.GetSummonPlayerSkillData(component.summonsSkills[i]);
            SaveDataSkillData dataPlayerSkill = new SaveDataSkillData();
            dataPlayerSkill.Save(spell);
            skills.Add(dataPlayerSkill);
        }

        
        currentSpellBeingUnlocked = component.currentSpellBeingUnlocked;
        currentSpellUnlockCost = component.currentSpellUnlockCost;
        lastSpellUnlockSummary = component.lastSpellUnlockSummary;
        isSpellUnlockedBookmarked = component.isSpellUnlockedBookmarked;
        
        timerUnlockSpell = component.timerUnlockSpell;
        // cooldownReroll = component.cooldownReroll;
        currentSpellChoices = new List<PLAYER_SKILL_TYPE>(component.currentSpellChoices);
        currentPortalUpgradeCost = component.currentPortalUpgradeCost;
        timerUpgradePortal = component.timerUpgradePortal;
        lastPortalUpgradeSummary = component.lastPortalUpgradeSummary;
        isPreviousPortalUpgradeBookmarked = component.isPreviousPortalUpgradeBookmarked;
    }
    public override PlayerSkillComponent Load() {
        PlayerSkillComponent component = new PlayerSkillComponent();
        //component.LoadSkills(skills);
        return component;
    }
}