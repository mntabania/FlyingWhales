using System;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using Locations.Settlements;
using UtilityScripts;

public class SkillData : IPlayerSkill {
    public virtual PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.NONE;
    public virtual string name { get { return string.Empty; } }
    public virtual string description { get { return string.Empty; } }
    public virtual PLAYER_SKILL_CATEGORY category { get { return PLAYER_SKILL_CATEGORY.NONE; } }
    //public virtual INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.NONE;
    public SPELL_TARGET[] targetTypes { get; protected set; }
    //public int radius { get; protected set; }
    public int maxCharges => SpellUtilities.GetModifiedSpellCost(baseMaxCharges, WorldSettings.Instance.worldSettingsData.GetChargeCostsModification());
    public int charges { get; private set; }
    public int manaCost => SpellUtilities.GetModifiedSpellCost(baseManaCost, WorldSettings.Instance.worldSettingsData.GetCostsModification());
    public int cooldown => SpellUtilities.GetModifiedSpellCost(baseCooldown, WorldSettings.Instance.worldSettingsData.GetCooldownSpeedModification());
    public int threat => SpellUtilities.GetModifiedSpellCost(baseThreat, WorldSettings.Instance.worldSettingsData.GetThreatModification());
    public int threatPerHour { get; private set; }
    public bool isInUse { get; private set; }
    public int currentCooldownTick { get; private set; }
    public bool hasCharges => baseMaxCharges != -1;
    public bool hasCooldown => baseCooldown != -1;
    public bool hasManaCost => baseManaCost != -1;
    public virtual bool isInCooldown => hasCooldown && currentCooldownTick < cooldown;

    public int baseMaxCharges { get; private set; }
    public int baseManaCost { get; private set; }
    public int baseCooldown { get; private set; }
    public int baseThreat { get; private set; }
    
    protected SkillData() {
        ResetData();
    }

    #region Virtuals
    public virtual void ActivateAbility(IPointOfInterest targetPOI) {
        OnExecutePlayerSkill();
    }
    public virtual void ActivateAbility(LocationGridTile targetTile) {
        OnExecutePlayerSkill();
    }
    public virtual void ActivateAbility(LocationGridTile targetTile, ref Character spawnedCharacter) {
        OnExecutePlayerSkill();
    }
    public virtual void ActivateAbility(HexTile targetHex) {
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
    public virtual bool CanPerformAbilityTowards(LocationGridTile targetTile) { return CanPerformAbility(); }
    public virtual bool CanPerformAbilityTowards(HexTile targetHex) { return CanPerformAbility(); }
    public virtual bool CanPerformAbilityTowards(LocationStructure targetStructure) { return CanPerformAbility(); }
    public virtual bool CanPerformAbilityTowards(StructureRoom room) { return CanPerformAbility(); }
    public virtual bool CanPerformAbilityTowards(BaseSettlement targetSettlement) { return CanPerformAbility(); }
    /// <summary>
    /// Highlight the affected area of this spell given a tile.
    /// </summary>
    /// <param name="tile">The tile to take into consideration.</param>
    public virtual void HighlightAffectedTiles(LocationGridTile tile) { }
    public virtual void UnhighlightAffectedTiles() {
        TileHighlighter.Instance.HideHighlight();
    }
    /// <summary>
    /// Show an invalid highlight.
    /// </summary>
    /// <param name="tile"></param>
    /// <param name="invalidText"></param>
    /// <returns>True or false (Whether or not this spell showed an invalid highlight)</returns>
    public virtual bool InvalidHighlight(LocationGridTile tile, ref string invalidText) { return false; }
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
        isInUse = false;
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
        return (!hasCharges || charges > 0) && (!hasManaCost || PlayerManager.Instance.player.mana >= manaCost); // && (!hasCooldown || currentCooldownTick >= cooldown);
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
        return CanPerformAbilityTowards(tile);
    }
    public bool CanTarget(HexTile hex) {
        return CanPerformAbilityTowards(hex);
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
            if(hasCharges && charges > 0 && WorldSettings.Instance.worldSettingsData.chargeAmount != SKILL_CHARGE_AMOUNT.Unlimited) {
                AdjustCharges(-1);
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
        currentCooldownTick++;
        // Assert.IsFalse(currentCooldownTick > cooldown, $"Cooldown tick became higher than cooldown in {name}. Cooldown is {cooldown.ToString()}. Cooldown Tick is {currentCooldownTick.ToString()}");
        if(currentCooldownTick >= cooldown) {
            //SetCharges(maxCharges);
            if(hasCharges && charges < maxCharges) {
                AdjustCharges(1);
            }
            Messenger.RemoveListener(Signals.TICK_STARTED, PerTickCooldown);
            Messenger.Broadcast(SpellSignals.SPELL_COOLDOWN_FINISHED, this);
            Messenger.Broadcast(SpellSignals.FORCE_RELOAD_PLAYER_ACTIONS);

            if(hasCharges && charges < maxCharges) {
                StartCooldown();
            }
        }
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
    #endregion
}