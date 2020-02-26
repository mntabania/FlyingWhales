using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Ruinarch;
using Traits;
using UnityEngine;

public class PlayerSpell {

    //public PlayerJobData parentData { get; protected set; }
    //public Minion minion { get; protected set; }
    public SPELL_TYPE spellType { get; protected set; }
    public SPELL_CATEGORY spellCategory { get; protected set; }
    public string name { get { return PlayerManager.Instance.allSpellsData[spellType].name; } }
    public string description { get { return PlayerManager.Instance.allSpellsData[spellType].description; } }
    public int tier { get; protected set; }
    public int abilityRadius { get; protected set; } //0 means single target
    public virtual string dynamicDescription { get { return description; } }
    public int cooldown { get; protected set; } //cooldown in ticks
    //public Character assignedCharacter { get; protected set; }
    public SPELL_TARGET[] targetTypes { get; protected set; } //what sort of object does this action target
    public bool isActive { get; protected set; }
    public int ticksInCooldown { get; private set; } //how many ticks has this action been in cooldown?
    public int level { get; protected set; }
    //public List<ABILITY_TAG> abilityTags { get; protected set; }
    //public bool hasSecondPhase { get; protected set; }
    //public bool isInSecondPhase { get; protected set; }

    #region getters/setters
    public string worldObjectName {
        get { return name; }
    }
    public bool isInCooldown {
        get { return ticksInCooldown != cooldown; } //check if the ticks this action has been in cooldown is the same as cooldown
    }
    #endregion



    //public void SetParentData(PlayerJobData data) {
    //    parentData = data;
    //}
    //public void SetMinion(Minion minion) {
    //    this.minion = minion;
    //}

    public PlayerSpell(SPELL_TYPE spellType) {
        this.spellType = spellType;
        //this.name = Utilities.NormalizeStringUpperCaseFirstLetters(this.abilityType.ToString());
        //abilityTags = new List<ABILITY_TAG>();
        this.level = 1;
        this.tier = PlayerManager.Instance.GetSpellTier(spellType);
        this.abilityRadius = 0;
        //hasSecondPhase = false;
        OnLevelUp();
    }

    public void LevelUp() {
        level++;
        level = Mathf.Clamp(level, 1, PlayerDB.MAX_LEVEL_INTERVENTION_ABILITY);
        OnLevelUp();
    }
    public void SetLevel(int amount) {
        level = amount;
        level = Mathf.Clamp(level, 1, PlayerDB.MAX_LEVEL_INTERVENTION_ABILITY);
        OnLevelUp();
    }

    #region Virtuals
    public virtual void ActivateAction() { //this is called when the actions button is pressed
        if (this.isActive) { //if this action is still active, deactivate it first
            DeactivateAction();
        }
        //this.assignedCharacter = assignedCharacter;
        isActive = true;
        //parentData.SetActiveAction(this);
        //ActivateCooldown();
        //Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        //Messenger.AddListener<JOB, Character>(Signals.CHARACTER_UNASSIGNED_FROM_JOB, OnCharacterUnassignedFromJob);
        PlayerManager.Instance.player.ConsumeAbility(this);
    }
    public virtual void ActivateAction(IPointOfInterest targetPOI) { //this is called when the actions button is pressed
        ActivateAction();
    }
    public virtual void ActivateAction(Settlement targetSettlement) { //this is called when the actions button is pressed
        ActivateAction();
    }
    public virtual void ActivateAction(LocationGridTile targetTile) { 
        ActivateAction();
    }
    public virtual void ActivateAction(List<IPointOfInterest> targetPOIs) {
        ActivateAction();
    }
    public virtual void DeactivateAction() { //this is typically called when the character is assigned to another action or the assigned character dies
        isActive = false;
        //parentData.SetActiveAction(null);
        //Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        //Messenger.RemoveListener<JOB, Character>(Signals.CHARACTER_UNASSIGNED_FROM_JOB, OnCharacterUnassignedFromJob);
    }
    protected virtual void OnCharacterDied(Character characterThatDied) {
        //if (assignedCharacter != null && characterThatDied.id == assignedCharacter.id) {
        //    DeactivateAction();
        //    //ResetCooldown(); //only reset cooldown if the assigned character dies
        //}
    }
    protected virtual void OnCharacterUnassignedFromJob(JOB job, Character character) {
        //if (character.id == assignedCharacter.id) {
        //    DeactivateAction();
        //}
    }
    /// <summary>
    /// Can this action currently be performed.
    /// </summary>
    /// <returns>True or False</returns>
    public virtual bool CanPerformAction() {
        if (isInCooldown) {
            return false;
        }
        return true;
    }
    protected virtual bool CanPerformActionTowards(Character targetCharacter) {
        if (targetCharacter.traitContainer.HasTrait("Blessed")) {
            return false;
        }
        //Quick fix only, remove this later
        if (targetCharacter.race != RACE.HUMANS && targetCharacter.race != RACE.ELVES) {
            return false;
        }
        return CanPerformAction();
    }
    /// <summary>
    /// Function that determines whether this action can target the given character or not.
    /// Regardless of cooldown state.
    /// </summary>
    /// <param name="poi">The target poi</param>
    /// <returns>true or false</returns>
    public virtual bool CanTarget(IPointOfInterest poi, ref string hoverText) {
        if (poi.traitContainer.HasTrait("Blessed")) {
            hoverText = "Blessed characters cannot be targetted.";
            return false;
        }
        //Quick fix only, remove this later
        if (poi is Character) {
            Character targetCharacter = poi as Character;
            if(targetCharacter.race != RACE.HUMANS && targetCharacter.race != RACE.ELVES) {
                return false;
            }
        }
        hoverText = string.Empty;
        return true;
    }
    protected virtual void OnLevelUp() { }
    public virtual void SecondPhase() { }
    /// <summary>
    /// If the ability has a range, override this to show that range. <see cref="InputManager.Update"/>
    /// </summary>
    public virtual void ShowRange(LocationGridTile tile) { }
    public virtual void HideRange(LocationGridTile tile) { }
    #endregion

    #region Cooldown
    protected void SetDefaultCooldownTime(int cooldown) {
        this.cooldown = cooldown;
        ticksInCooldown = cooldown;
    }
    private void ActivateCooldown() {
        ticksInCooldown = 0;
        //parentData.SetLockedState(true);
        //Messenger.AddListener(Signals.TICK_ENDED, CheckForCooldown); //IMPORTANT NOTE: Cooldown will start but will not actually finish because this line of code is removed. This is removed this so that the ability can only be used once. Upon every enter of the settlement map, all cooldowns of intervention abilities must be reset
        Messenger.Broadcast(Signals.JOB_ACTION_COOLDOWN_ACTIVATED, this);
    }
    private void CheckForCooldown() {
        if (ticksInCooldown == cooldown) {
            //cooldown has been reached!
            OnCooldownDone();
        } else {
            ticksInCooldown++;
        }
    }
    public void InstantCooldown() {
        ticksInCooldown = cooldown;
        OnCooldownDone();
    }
    private void OnCooldownDone() {
        //parentData.SetLockedState(false);
        //Messenger.RemoveListener(Signals.TICK_ENDED, CheckForCooldown);
        Messenger.Broadcast(Signals.JOB_ACTION_COOLDOWN_DONE, this);
    }
    private void ResetCooldown() {
        ticksInCooldown = cooldown;
        //parentData.SetLockedState(false);
    }
    #endregion
}

public class SpellData {
    public virtual SPELL_TYPE ability => SPELL_TYPE.NONE;
    public virtual string name { get { return string.Empty; } }
    public virtual string description { get { return string.Empty; } }
    public virtual SPELL_CATEGORY category { get { return SPELL_CATEGORY.NONE; } }
    public virtual INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.NONE;
    public virtual int abilityRadius => 0; //0 means single target
    public SPELL_TARGET[] targetTypes { get; protected set; }

    public int tier { get; protected set; }
    public int manaCost { get; protected set; }

    public SpellData() {
        tier = PlayerManager.Instance.GetSpellTier(ability);
        manaCost = PlayerManager.Instance.GetManaCostForSpell(tier);
    }

    #region Virtuals
    public virtual void ActivateAbility(IPointOfInterest targetPOI) { }
    public virtual void ActivateAbility(LocationGridTile targetTile) { }
    public virtual void ActivateAbility(HexTile targetHex) { }
    public virtual bool CanPerformAbilityTowards(Character targetCharacter) {
        if((targetCharacter.race != RACE.HUMANS && targetCharacter.race != RACE.ELVES) || targetCharacter.traitContainer.HasTrait("Blessed")) {
            return false;
        }
        return true;
    }
    public virtual bool CanPerformAbilityTowards(TileObject tileObject) { return true; }
    public virtual bool CanPerformAbilityTowards(LocationGridTile targetTile) { return true; }
    public virtual bool CanPerformAbilityTowards(HexTile targetHex) { return true; }

    // public virtual bool CanPerformAbilityTowards(SpecialToken item) { return true; }

    /// <summary>
    /// If the ability has a range, override this to show that range. <see cref="InputManager.Update"/>
    /// </summary>
    public virtual void ShowRange(LocationGridTile tile) {
        if(abilityRadius > 0) {
            List<LocationGridTile> tiles = tile.GetTilesInRadius(abilityRadius, includeCenterTile: true, includeTilesInDifferentStructure: true);
            InnerMapManager.Instance.HighlightTiles(tiles);
        }
    }
    public virtual void HideRange(LocationGridTile tile) {
        if (abilityRadius > 0) {
            List<LocationGridTile> tiles = tile.GetTilesInRadius(abilityRadius, includeCenterTile: true, includeTilesInDifferentStructure: true);
            InnerMapManager.Instance.UnhighlightTiles(tiles);
        }
    }
    #endregion

    #region General
    public bool CanPerformAbilityTowards(IPointOfInterest poi) {
        if(poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            return CanPerformAbilityTowards(poi as Character);
        } else if (poi.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
            return CanPerformAbilityTowards(poi as TileObject);
        }
        return true;
    }
    /// <summary>
    /// Function that determines whether this action can target the given character or not.
    /// Regardless of cooldown state.
    /// </summary>
    /// <param name="poi">The target poi</param>
    /// <returns>true or false</returns>
    public bool CanTarget(IPointOfInterest poi, ref string hoverText) {
        if (poi.traitContainer.HasTrait("Blessed")) {
            hoverText = "Blessed characters cannot be targetted.";
            return false;
        }
        //Quick fix only, remove this later
        if (poi is Character) {
            Character targetCharacter = poi as Character;
            if (targetCharacter.race != RACE.HUMANS && targetCharacter.race != RACE.ELVES) {
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
    #endregion
}

public class PlayerJobActionSlot {
    public int level;
    public PlayerSpell ability;

    public PlayerJobActionSlot() {
        level = 1;
        ability = null;
    }

    public void SetAbility(PlayerSpell ability) {
        this.ability = ability;
        if (this.ability != null) {
            this.ability.SetLevel(level);
        }
    }

    public void LevelUp() {
        level++;
        level = Mathf.Clamp(level, 1, PlayerDB.MAX_LEVEL_INTERVENTION_ABILITY);
        if (this.ability != null) {
            this.ability.SetLevel(level);
        }
        Messenger.Broadcast(Signals.PLAYER_GAINED_INTERVENE_LEVEL, this);
    }
    public void SetLevel(int amount) {
        level = amount;
        level = Mathf.Clamp(level, 1, PlayerDB.MAX_LEVEL_INTERVENTION_ABILITY);
        if (this.ability != null) {
            this.ability.SetLevel(level);
        }
        Messenger.Broadcast(Signals.PLAYER_GAINED_INTERVENE_LEVEL, this);
    }
}