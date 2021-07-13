using System.Collections;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using Locations.Settlements;
using UnityEngine.Profiling;
using UtilityScripts;
using UnityEngine.Localization.Settings;

public class SkillData : IPlayerSkill {
    public const int MAX_SPELL_LEVEL = 3;
    //public virtual INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.NONE;
    public SPELL_TARGET[] targetTypes { get; protected set; }
    //public int radius { get; protected set; }
    public int charges { get; private set; }
    public int threatPerHour { get; private set; }
    public bool isInUse { get; private set; } //This means that this skill is unlocked by the player permanently
    public bool isTemporarilyInUse { get; private set; } //This means that this skill is not unlocked in 
    public int currentCooldownTick { get; private set; }
    public int baseMaxCharges { get; private set; }
    public int baseManaCost { get; private set; }
    public float basePierce { get; private set; }
    public int baseCooldown { get; private set; }
    public int baseThreat { get; private set; }
    public int bonusCharges { get; private set; }
    /// <summary>
    /// Current level of this spell. This starts at 0.
    /// 0 = Level 1, 1 = Level 2, etc.
    /// </summary>
    public int currentLevel { get; set; }
    public bool isUnlockedBaseOnRequirements { get; set; }

    public virtual string localizedName => LocalizationSettings.StringDatabase.GetLocalizedString("SkillNameAndDescription_Table", name);
    public virtual string localizedDescription => $"{LocalizationSettings.StringDatabase.GetLocalizedString("SkillNameAndDescription_Table", name + "_Description")}";

    public int unlockCost { get; set; }
    public SkillEventDispatcher skillEventDispatcher { get; }

    #region getters
    public virtual PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.NONE;
    public virtual string name { get { return string.Empty; } }
    public virtual string description { get { return string.Empty; } }
    public virtual PLAYER_SKILL_CATEGORY category { get { return PLAYER_SKILL_CATEGORY.NONE; } }
    public int maxCharges => SpellUtilities.GetModifiedSpellCost(baseMaxCharges, 1);
    public int manaCost => SpellUtilities.GetModifiedSpellCost(baseManaCost, 1);
    public int cooldown => SpellUtilities.GetModifiedSpellCost(baseCooldown, 1);
    public int threat => 0;// SpellUtilities.GetModifiedSpellCost(baseThreat, 1f); comment out for now so no threat will be passed

    /// <summary>
    /// The level that should be displayed on UI. This getter is only for convenience.
    /// </summary>
    public int levelForDisplay => currentLevel + 1;
    public bool isMaxLevel => currentLevel >= MAX_SPELL_LEVEL;
    public bool hasCharges => baseMaxCharges != -1;
    public bool hasCooldown => baseCooldown != -1;
    public bool hasManaCost => baseManaCost != -1;
    public bool hasBonusCharges => bonusCharges > 0;
    public int totalCharges => charges + bonusCharges;
    public virtual bool isInCooldown => hasCooldown && currentCooldownTick < cooldown;
    public string displayOfCurrentChargesWithBonusChargesNotCombined => GetDisplayOfCurrentChargesWithBonusChargesNotCombined();
    public string displayOfCurrentChargesWithBonusChargesCombined => GetDisplayOfCurrentChargesWithBonusChargesCombined();
    public string displayOfCurrentChargesWithBonusChargesCombinedIconFirst => GetDisplayOfCurrentChargesWithBonusChargesCombinedIconFirst();
    #endregion

    public void LevelUp() {
        PlayerSkillData playerSkillData = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(type);
        currentLevel = Mathf.Clamp(++currentLevel, 0, MAX_SPELL_LEVEL);
        SetManaCost(WorldSettings.Instance.worldSettingsData.IsScenarioMap() ? playerSkillData.GetManaCostForScenarios() : playerSkillData.GetManaCostBaseOnLevel(currentLevel)); // playerSkillData.GetManaCostBaseOnLevel(currentLevel)
        SetMaxCharges(playerSkillData.GetMaxChargesBaseOnLevel(currentLevel));
        SetPierce(PlayerSkillManager.Instance.GetAdditionalPiercePerLevelBaseOnLevel(type));
        SetCooldown(playerSkillData.GetCoolDownBaseOnLevel(currentLevel));
        SetCharges(maxCharges);
        FinishCooldown();
        if (category == PLAYER_SKILL_CATEGORY.AFFLICTION) {
            Messenger.Broadcast($"{localizedName}LevelUp", this);
        }
        Messenger.Broadcast(PlayerSkillSignals.PLAYER_SKILL_LEVEL_UP, this);
    }

    protected SkillData() {
        ResetData();
    }

    #region Virtuals
    public virtual void OnSetAsCurrentActiveSpell(){}
    public virtual void OnNoLongerCurrentActiveSpell(){}
    public virtual void ActivateAbility(IPointOfInterest targetPOI) {
        OnExecutePlayerSkill();
    }
    public virtual void ActivateAbility(LocationGridTile targetTile) {
        OnExecutePlayerSkill();
    }
    public virtual void ActivateAbility(LocationGridTile targetTile, ref Character spawnedCharacter) {
        OnExecutePlayerSkill();
    }
    public virtual void ActivateAbility(Area targetArea) {
        //if(targetHex.settlementOnTile != null) {
        //    if(targetHex.settlementOnTile.HasResidentInsideSettlement()){
        //        PlayerManager.Instance.player.threatComponent.AdjustThreat(20);
        //    }
        //}
        OnExecutePlayerSkill();
    }
    public virtual void ActivateAbility(LocationStructure targetStructure) {
        OnExecutePlayerSkill();
    }
    public virtual void ActivateAbility(StructureRoom room) {
        OnExecutePlayerSkill();
    }
    public virtual void ActivateAbility(BaseSettlement targetSettlement) {
        OnExecutePlayerSkill();
    }
    public virtual string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter) { return null; }
    public virtual string GetReasonsWhyCannotPerformAbilityTowards(TileObject targetTileObject) { return null; }
    public virtual string GetReasonsWhyCannotPerformAbilityTowards(BaseSettlement p_targetSettlement) { return string.Empty; }
    public virtual string GetReasonsWhyCannotPerformAbilityTowards(LocationStructure p_targetStructure) { return string.Empty; }
    public virtual bool CanPerformAbilityTowards(Character targetCharacter) {
        //(targetCharacter.race != RACE.HUMANS && targetCharacter.race != RACE.ELVES)
        if(targetCharacter.traitContainer.IsBlessed()) {
            return false;
        }
        return CanPerformAbility();
    }
    public virtual bool CanPerformAbilityTowards(TileObject tileObject) { return CanPerformAbility(); }
    public virtual bool CanPerformAbilityTowards(LocationGridTile targetTile, out string o_cannotPerformReason) {
        o_cannotPerformReason = string.Empty;
        return CanPerformAbility();
    }
    public virtual bool CanPerformAbilityTowards(Area targetArea) { return CanPerformAbility(); }
    public virtual bool CanPerformAbilityTowards(LocationStructure targetStructure) { return CanPerformAbility(); }
    public virtual bool CanPerformAbilityTowards(StructureRoom room) { return CanPerformAbility(); }
    public virtual bool CanPerformAbilityTowards(BaseSettlement targetSettlement) { return CanPerformAbility(); }
    /// <summary>
    /// Highlight the affected area of this spell given a tile.
    /// </summary>
    /// <param name="tile">The tile to take into consideration.</param>
    public virtual void ShowValidHighlight(LocationGridTile tile) { }
    public virtual void UnhighlightAffectedTiles() {
        TileHighlighter.Instance.HideHighlight();
    }
    /// <summary>
    /// Show an invalid highlight.
    /// </summary>
    /// <param name="tile"></param>
    /// <param name="invalidText"></param>
    /// <returns>True or false (Whether or not this spell showed an invalid highlight)</returns>
    public virtual bool ShowInvalidHighlight(LocationGridTile tile, ref string invalidText) { return false; }
    #endregion

    #region General
    public void ResetData() {
        charges = -1;
        baseManaCost = -1;
        baseCooldown = -1;
        baseMaxCharges = -1;
        baseThreat = 0;
        threatPerHour = 0;
        currentCooldownTick = cooldown;
        currentLevel = 0;
        ResetIsInUse();
        isTemporarilyInUse = false;
    }
    public bool CanPerformAbilityTowards(IPointOfInterest poi) {
        if(poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            return CanPerformAbilityTowards(poi as Character);
        } else if (poi.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
            return CanPerformAbilityTowards(poi as TileObject);
        }
        return CanPerformAbility();
    }
    public bool CanPerformAbility() {
        bool canBePerformed = (((!hasCharges || charges > 0) && isInUse) || hasBonusCharges) && (!hasManaCost || PlayerManager.Instance.player.mana >= manaCost); // && (!hasCooldown || currentCooldownTick >= cooldown);
        if (!canBePerformed) {
            if (type == PLAYER_SKILL_TYPE.SCHEME) {
                //This is the scheme parent, even if there are no charges, it can be performed so that the player can see the second column
                //This is assuming that the scheme parent does not have its personal cooldown, it should not have any cooldown itself because its cooldown is based on the specific schemes
                return true;
            }
        } else {
            if(category == PLAYER_SKILL_CATEGORY.SCHEME) {
                SkillData parentSchemeData = PlayerSkillManager.Instance.GetPlayerActionData(PLAYER_SKILL_TYPE.SCHEME);
                if (parentSchemeData.charges <= 0) {
                    //This is the specific scheme
                    //If the scheme parent does not have any charges, it should not be clickable
                    //This part will only also be called if the scheme parent does not have any charges
                    return false;
                }
            }
        }
        return canBePerformed;
    }
    /// <summary>
    /// Function that determines whether this action can target the given character or not.
    /// Regardless of cooldown state.
    /// </summary>
    /// <param name="poi">The target poi</param>
    /// <returns>true or false</returns>
    public bool CanTarget(IPointOfInterest poi, ref string hoverText) {
        if (poi.traitContainer.IsBlessed()) {
            hoverText = "Blessed characters cannot be targeted.";
            return false;
        }
        //Quick fix only, remove this later
        if (poi is Character) {
            Character targetCharacter = poi as Character;
            if (!targetCharacter.race.IsSapient()) {
                return false;
            }
        }
        hoverText = string.Empty;
        return CanPerformAbilityTowards(poi);
    }
    public bool CanTarget(LocationGridTile tile) {
        return CanPerformAbilityTowards(tile, out _);
    }
    public bool CanTarget(Area p_area) {
        return CanPerformAbilityTowards(p_area);
    }
    public bool CanTarget(BaseSettlement p_targetSettlement) {
        return CanPerformAbilityTowards(p_targetSettlement);
    }
    protected void IncreaseThreatForEveryCharacterThatSeesPOI(IPointOfInterest poi, int amount) {
        Messenger.Broadcast(CharacterSignals.INCREASE_THREAT_THAT_SEES_POI, poi, amount);
    }
    //protected void IncreaseThreatThatSeesTile(LocationGridTile targetTile, int amount) {
    //    Messenger.Broadcast(Signals.INCREASE_THREAT_THAT_SEES_TILE, targetTile, amount);
    //}
    public void OnLoadSpell() {
        Messenger.Broadcast(PlayerSkillSignals.CHARGES_ADJUSTED, this);
        if (hasCooldown) {
            if((hasCharges && charges < maxCharges) || currentCooldownTick < cooldown) {
                Messenger.Broadcast(PlayerSkillSignals.SPELL_COOLDOWN_STARTED, this);
                Messenger.AddListener(Signals.TICK_STARTED, PerTickCooldown);
            }
        }
    }
    public void OnExecutePlayerSkill() {
        if (!PlayerSkillManager.Instance.unlimitedCast) {
            //If there are bonus charges, decrease that first
            if (hasBonusCharges) {
                AdjustBonusCharges(-1);
            } else if (hasCharges) {
                if(charges > 0 && WorldSettings.Instance.worldSettingsData.playerSkillSettings.chargeAmount != SKILL_CHARGE_AMOUNT.Unlimited) {
                    AdjustCharges(-1);
                }
            } else {
                //Special Case for Schemes
                //Once the cooldown of specific scheme has no charges but has a cooldown, it will still go into cooldown, and at the end of the cooldown the charge will be added to the parent scheme
                if (category == PLAYER_SKILL_CATEGORY.SCHEME) {
                    StartCooldown();
                }
            }
            if (hasManaCost) {
                // if (!WorldSettings.Instance.worldSettingsData.omnipotentMode) {
                    PlayerManager.Instance.player.AdjustMana(-manaCost);
                // }
            }
        }
        // PlayerManager.Instance.player.threatComponent.AdjustThreatPerHour(threatPerHour);
        PlayerManager.Instance.player.threatComponent.AdjustThreat(threat);
        //PlayerManager.Instance.player.threatComponent.AdjustThreat(20);

        if (category == PLAYER_SKILL_CATEGORY.PLAYER_ACTION) {
            Messenger.Broadcast(PlayerSkillSignals.ON_EXECUTE_PLAYER_ACTION, this as PlayerAction);
        } else if (category == PLAYER_SKILL_CATEGORY.AFFLICTION) {
            Messenger.Broadcast(PlayerSkillSignals.ON_EXECUTE_AFFLICTION, this);
        } else {
            Messenger.Broadcast(PlayerSkillSignals.ON_EXECUTE_PLAYER_SKILL, this);
        }
    }
    public void StartCooldown() {
        if (hasCooldown && currentCooldownTick == cooldown) {
            SetCurrentCooldownTick(0);
            Messenger.Broadcast(PlayerSkillSignals.SPELL_COOLDOWN_STARTED, this);
            if(cooldown > 0) {
                Messenger.AddListener(Signals.TICK_STARTED, PerTickCooldown);
            } else {
                PerTickCooldown();
            }
        }
    }
    protected virtual void PerTickCooldown() {
#if DEBUG_PROFILER
        Profiler.BeginSample($"{localizedName} Per Tick Cooldown");
#endif
        currentCooldownTick++;
        // Assert.IsFalse(currentCooldownTick > cooldown, $"Cooldown tick became higher than cooldown in {name}. Cooldown is {cooldown.ToString()}. Cooldown Tick is {currentCooldownTick.ToString()}");
        if(currentCooldownTick >= cooldown) {
            //SetCharges(maxCharges);
            currentCooldownTick = cooldown;
            FinishCooldown();

            if (hasCharges && charges < maxCharges) {
                AdjustCharges(1);
            } else {
                //Special Case for Schemes
                //Once the cooldown of specific scheme is done, the charges will be added to the parent scheme, not the specific scheme
                if(category == PLAYER_SKILL_CATEGORY.SCHEME) {
                    SkillData schemeSkill = PlayerSkillManager.Instance.GetPlayerActionData(PLAYER_SKILL_TYPE.SCHEME);
                    if (schemeSkill.hasCharges && schemeSkill.charges < schemeSkill.maxCharges) {
                        schemeSkill.AdjustCharges(1);
                    }
                }
            }
            Messenger.Broadcast(PlayerSkillSignals.FORCE_RELOAD_PLAYER_ACTIONS);
            //Cooldown is started already in AdjustCharges
            //if (hasCharges && charges < maxCharges) {
            //    StartCooldown();
            //}
        }
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif
    }
    public virtual void FinishCooldown() {
        if (Messenger.eventTable.ContainsKey(Signals.TICK_STARTED)) {
            Messenger.RemoveListener(Signals.TICK_STARTED, PerTickCooldown);
        }
        Messenger.Broadcast(PlayerSkillSignals.SPELL_COOLDOWN_FINISHED, this);
    }
    public string GetManaCostChargesCooldownStr() {
        string str = "Mana Cost: " + manaCost;
        str += "\nCharges: " + charges;
        str += "\nCooldown: " + cooldown;
        return str;
    }
    private string GetDisplayOfCurrentChargesWithBonusChargesNotCombined() {
        return SpellUtilities.GetDisplayOfCurrentChargesWithBonusChargesNotCombined(charges, maxCharges, bonusCharges, hasCharges && isInUse);
    }
    private string GetDisplayOfCurrentChargesWithBonusChargesCombined() {
        string str = string.Empty;
        if (hasCharges || hasBonusCharges) {
            str += $"{(isInUse ? totalCharges : bonusCharges)}{(hasBonusCharges ? UtilityScripts.Utilities.BonusChargesIcon() : UtilityScripts.Utilities.ChargesIcon())}";
        }
        return str;
    }
    private string GetDisplayOfCurrentChargesWithBonusChargesCombinedIconFirst() {
        string str = string.Empty;
        if (hasCharges || hasBonusCharges) {
            str += $"{(hasBonusCharges ? UtilityScripts.Utilities.BonusChargesIcon() : UtilityScripts.Utilities.ChargesIcon())}{(isInUse ? totalCharges : bonusCharges)}";
        }
        return str;
    }
#endregion

#region Attributes
    public void SetIsUnlockBaseOnRequirements(bool p_isUnlocked) {
        isUnlockedBaseOnRequirements = p_isUnlocked;
    }
    public void SetMaxCharges(int amount) {
        baseMaxCharges = amount;
    }
    public void AdjustMaxCharges(int amount) {
        baseMaxCharges += amount;
    }
    public void SetCharges(int amount) {
        charges = amount;
    }
    public void SetBonusCharges(int amount) {
        bonusCharges = amount;
    }
    public void AdjustCharges(int amount) {
        charges += amount;
        Messenger.Broadcast(PlayerSkillSignals.CHARGES_ADJUSTED, this);

        if(charges < maxCharges) {
            StartCooldown();
        }
    }
    public void AdjustBonusCharges(int amount) {
        bonusCharges += amount;
        bonusCharges = Mathf.Max(bonusCharges, 0);
        Messenger.Broadcast(PlayerSkillSignals.BONUS_CHARGES_ADJUSTED, this);
    }

    public void SetPierce(float amount) {
        basePierce = amount;
    }

    public void SetUnlockCost(int amount) {
        unlockCost = amount;
    }

    public void SetManaCost(int amount) {
        baseManaCost = amount;
    }
    public void SetCooldown(int amount) {
        baseCooldown = amount;
        currentCooldownTick = cooldown;
    }
    public void SetCurrentCooldownTick(int amount) {
        currentCooldownTick = amount;
    }
    public void SetThreat(int amount) {
        baseThreat = amount;
    }
    public void SetThreatPerHour(int amount) {
        threatPerHour = amount;
    }
    public void SetIsInUse(bool state) {
        if (isInUse != state) {
            isInUse = state;
            PlayerSkillData playerSkillData = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(type);
            if (isInUse) {
                SetIsTemporarilyInUse(false);
                if (playerSkillData != null) {
                    PlayerManager.Instance.player.playerSkillComponent.AddTierCount(playerSkillData);
                }
            } else {
                if (playerSkillData != null) {
                    PlayerManager.Instance.player.playerSkillComponent.RemoveTierCount(playerSkillData);
                }
            }
        }
        // Debug.Log($"Set spell {name} in use to {isInUse.ToString()}");
    }
    public void ResetIsInUse() {
        isInUse = false;
    }
    public void SetIsTemporarilyInUse(bool p_state) {
        isTemporarilyInUse = p_state;
    }
    public void SetCurrentLevel(int amount) {
        currentLevel = amount;
    }
#endregion
}