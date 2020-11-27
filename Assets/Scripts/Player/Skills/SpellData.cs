using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;

public class SpellData : IPlayerSkill {
    public virtual SPELL_TYPE type => SPELL_TYPE.NONE;
    public virtual string name { get { return string.Empty; } }
    public virtual string description { get { return string.Empty; } }
    public virtual SPELL_CATEGORY category { get { return SPELL_CATEGORY.NONE; } }
    //public virtual INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.NONE;
    public SPELL_TARGET[] targetTypes { get; protected set; }
    //public int radius { get; protected set; }

    public int maxCharges { get; private set; }
    public int charges { get; private set; }
    public int manaCost { get; private set; }
    public int cooldown { get; private set; }
    public int threat { get; private set; }
    public int threatPerHour { get; private set; }
    public bool isInUse { get; private set; }

    public int currentCooldownTick { get; private set; }
    public bool hasCharges => maxCharges != -1;
    public bool hasCooldown => cooldown != -1;
    public bool hasManaCost => manaCost != -1;
    public virtual bool isInCooldown => hasCooldown && currentCooldownTick < cooldown;

    protected SpellData() {
        ResetData();
    }

    #region Virtuals
    public virtual void ActivateAbility(IPointOfInterest targetPOI) {
        OnExecuteSpellActionAffliction();
    }
    public virtual void ActivateAbility(LocationGridTile targetTile) {
        OnExecuteSpellActionAffliction();
    }
    public virtual void ActivateAbility(LocationGridTile targetTile, ref Character spawnedCharacter) {
        OnExecuteSpellActionAffliction();
    }
    public virtual void ActivateAbility(HexTile targetHex) {
        //if(targetHex.settlementOnTile != null) {
        //    if(targetHex.settlementOnTile.HasResidentInsideSettlement()){
        //        PlayerManager.Instance.player.threatComponent.AdjustThreat(20);
        //    }
        //}
        OnExecuteSpellActionAffliction();
    }
    public virtual void ActivateAbility(LocationStructure targetStructure) {
        OnExecuteSpellActionAffliction();
    }
    public virtual void ActivateAbility(StructureRoom room) {
        OnExecuteSpellActionAffliction();
    }
    public virtual string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter) { return null; }
    public virtual bool CanPerformAbilityTowards(Character targetCharacter) {
        //(targetCharacter.race != RACE.HUMANS && targetCharacter.race != RACE.ELVES)
        if(targetCharacter.traitContainer.HasTrait("Blessed")) {
            return false;
        }
        return CanPerformAbility();
    }
    public virtual bool CanPerformAbilityTowards(TileObject tileObject) { return CanPerformAbility(); }
    public virtual bool CanPerformAbilityTowards(LocationGridTile targetTile) { return CanPerformAbility(); }
    public virtual bool CanPerformAbilityTowards(HexTile targetHex) { return CanPerformAbility(); }
    public virtual bool CanPerformAbilityTowards(LocationStructure targetStructure) { return CanPerformAbility(); }
    public virtual bool CanPerformAbilityTowards(StructureRoom room) { return CanPerformAbility(); }
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
        manaCost = -1;
        cooldown = -1;
        maxCharges = -1;
        threat = 0;
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
        if (poi.traitContainer.HasTrait("Blessed")) {
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
    public void OnExecuteSpellActionAffliction() {
        if (PlayerSkillManager.Instance.unlimitedCast == false) {
            if(hasCharges && charges > 0) {
                AdjustCharges(-1);
            }
            if (hasManaCost) {
                if (!WorldSettings.Instance.worldSettingsData.omnipotentMode) {
                    PlayerManager.Instance.player.AdjustMana(-manaCost);
                }
            }
        }
        // PlayerManager.Instance.player.threatComponent.AdjustThreatPerHour(threatPerHour);
        PlayerManager.Instance.player.threatComponent.AdjustThreat(threat);
        //PlayerManager.Instance.player.threatComponent.AdjustThreat(20);

        if (category == SPELL_CATEGORY.PLAYER_ACTION) {
            Messenger.Broadcast(SpellSignals.ON_EXECUTE_PLAYER_ACTION, this as PlayerAction);
        } else if (category == SPELL_CATEGORY.AFFLICTION) {
            Messenger.Broadcast(SpellSignals.ON_EXECUTE_AFFLICTION, this);
        } else if (category == SPELL_CATEGORY.SPELL || category == SPELL_CATEGORY.MINION || category == SPELL_CATEGORY.SUMMON) {
            Messenger.Broadcast(SpellSignals.ON_EXECUTE_SPELL, this);
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
        maxCharges = amount;
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
        manaCost = amount;
    }
    public void SetCooldown(int amount) {
        cooldown = amount;
        currentCooldownTick = cooldown;
    }
    public void SetCurrentCooldownTick(int amount) {
        currentCooldownTick = amount;
    }
    public void SetThreat(int amount) {
        threat = amount;
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