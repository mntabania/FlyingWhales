using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using Locations.Settlements;
using UnityEngine.Profiling;
using UtilityScripts;

public class SkillData : IPlayerSkill {
    public virtual PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.NONE;
    public virtual string name { get { return string.Empty; } }
    public virtual string description { get { return string.Empty; } }
    public virtual PLAYER_SKILL_CATEGORY category { get { return PLAYER_SKILL_CATEGORY.NONE; } }
    //public virtual INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.NONE;
    public SPELL_TARGET[] targetTypes { get; protected set; }
    //public int radius { get; protected set; }
    public int maxCharges => SpellUtilities.GetModifiedSpellCost(baseMaxCharges, 1f);
    public int charges { get; private set; }
    public int manaCost => SpellUtilities.GetModifiedSpellCost(baseManaCost, 1f);
    public int cooldown => SpellUtilities.GetModifiedSpellCost(baseCooldown, 1f);
    public int threat => SpellUtilities.GetModifiedSpellCost(baseThreat, 1f);
    public int threatPerHour { get; private set; }
    public bool isInUse { get; private set; }
    public int currentCooldownTick { get; private set; }
    public bool hasCharges => baseMaxCharges != -1;
    public bool hasCooldown => baseCooldown != -1;
    public bool hasManaCost => baseManaCost != -1;
    public virtual bool isInCooldown => hasCooldown && currentCooldownTick < cooldown;

    public int baseMaxCharges { get; private set; }
    public int baseManaCost { get; private set; }
    public float basePierce { get; private set; }
    public int baseCooldown { get; private set; }
    public int baseThreat { get; private set; }
    public int currentLevel { get; set; }

    public int unlockCost { get; set; }

    public void LevelUp() {
        PlayerSkillData playerSkillData = PlayerSkillManager.Instance.GetPlayerSkillData<PlayerSkillData>(type);
        currentLevel = Mathf.Clamp(++currentLevel, 0, 3);
        SetManaCost(playerSkillData.GetManaCostBaseOnLevel(currentLevel));
        SetMaxCharges(playerSkillData.GetMaxChargesBaseOnLevel(currentLevel));
        SetPierce(PlayerSkillManager.Instance.GetAdditionalPiercePerLevelBaseOnLevel(type));
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
        isInUse = false;
        currentLevel = 0;
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
        bool canBePerformed = (!hasCharges || charges > 0) && (!hasManaCost || PlayerManager.Instance.player.mana >= manaCost); // && (!hasCooldown || currentCooldownTick >= cooldown);
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
        Messenger.Broadcast(PlayerSignals.CHARGES_ADJUSTED, this);
        if (hasCooldown) {
            if((hasCharges && charges < maxCharges) || currentCooldownTick < cooldown) {
                Messenger.Broadcast(SpellSignals.SPELL_COOLDOWN_STARTED, this);
                Messenger.AddListener(Signals.TICK_STARTED, PerTickCooldown);
            }
        }
    }
    public void OnExecutePlayerSkill() {
        
        if (!PlayerSkillManager.Instance.unlimitedCast) {
            if (hasCharges) {
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
            Messenger.Broadcast(SpellSignals.ON_EXECUTE_PLAYER_ACTION, this as PlayerAction);
        } else if (category == PLAYER_SKILL_CATEGORY.AFFLICTION) {
            Messenger.Broadcast(SpellSignals.ON_EXECUTE_AFFLICTION, this);
        } else {
            Messenger.Broadcast(SpellSignals.ON_EXECUTE_PLAYER_SKILL, this);
        }
    }
    private void StartCooldown() {
        if (hasCooldown && currentCooldownTick == cooldown) {
            SetCurrentCooldownTick(0);
            Messenger.Broadcast(SpellSignals.SPELL_COOLDOWN_STARTED, this);
            Messenger.AddListener(Signals.TICK_STARTED, PerTickCooldown);
        }
    }
    private void PerTickCooldown() {
        Profiler.BeginSample($"{name} Per Tick Cooldown");
        currentCooldownTick++;
        // Assert.IsFalse(currentCooldownTick > cooldown, $"Cooldown tick became higher than cooldown in {name}. Cooldown is {cooldown.ToString()}. Cooldown Tick is {currentCooldownTick.ToString()}");
        if(currentCooldownTick >= cooldown) {
            //SetCharges(maxCharges);
            if(hasCharges && charges < maxCharges) {
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
            Messenger.RemoveListener(Signals.TICK_STARTED, PerTickCooldown);
            Messenger.Broadcast(SpellSignals.SPELL_COOLDOWN_FINISHED, this);
            Messenger.Broadcast(SpellSignals.FORCE_RELOAD_PLAYER_ACTIONS);

            if(hasCharges && charges < maxCharges) {
                StartCooldown();
            }
        }
        Profiler.EndSample();
    }
    public string GetManaCostChargesCooldownStr() {
        string str = "Mana Cost: " + manaCost;
        str += "\nCharges: " + charges;
        str += "\nCooldown: " + cooldown;
        return str;
    }
    #endregion

    #region Attributes
    public void SetMaxCharges(int amount) {
        baseMaxCharges = amount;
    }
    public void SetCharges(int amount) {
        charges = amount;
    }
    public void AdjustCharges(int amount) {
        charges += amount;
        Messenger.Broadcast(PlayerSignals.CHARGES_ADJUSTED, this);

        if(charges < maxCharges) {
            StartCooldown();
        }
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
        isInUse = state;
        // Debug.Log($"Set spell {name} in use to {isInUse.ToString()}");
    }

    public void SetCurrentLevel(int amount) {
        currentLevel = amount;
    }
    #endregion
}