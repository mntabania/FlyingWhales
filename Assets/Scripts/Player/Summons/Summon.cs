using System;
using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Traits;
using UnityEngine;
using Interrupts;

public class Summon : Character {

	public SUMMON_TYPE summonType { get; }
    private bool showNotificationOnDeath { get; set; }
    /// <summary>
    /// Is this monster unimportant? (i.e. skeleton carriers from Defiler/Prison)
    /// </summary>
    public bool isVolatileMonster { get; private set; }
    public virtual Faction defaultFaction => FactionManager.Instance.neutralFaction;

    #region getters
    public virtual SUMMON_TYPE adultSummonType => SUMMON_TYPE.None;
    public virtual COMBAT_MODE defaultCombatMode => COMBAT_MODE.Aggressive;
    public virtual bool defaultDigMode => false;
    public virtual string bredBehaviour => characterClass.traitNameOnTamedByPlayer;
    public override Type serializedData => typeof(SaveDataSummon);
    public bool isUsingDefaultName => name == raceClassName;
    #endregion

    protected Summon(SUMMON_TYPE summonType, string className, RACE race, GENDER gender) : base(className, race, gender) {
        this.summonType = summonType;
        showNotificationOnDeath = true;
        isVolatileMonster = false;
    }
    protected Summon(SaveDataSummon data) : base(data) {
        summonType = data.summonType;
        showNotificationOnDeath = true;
        isVolatileMonster = data.isVolatileMonster;
    }

    #region Overrides
    public override void Initialize() {
        ConstructDefaultActions();
        OnUpdateRace();
        OnUpdateCharacterClass();

        moodComponent.OnCharacterBecomeMinionOrSummon();
        moodComponent.SetMoodValue(50);
        
        needsComponent.Initialize();
        
        advertisedActions.Clear(); //This is so that any advertisements from OnUpdateRace will be negated. TODO: Make updating advertisements better.
        //TODO: Put this in a system
        AddAdvertisedAction(INTERACTION_TYPE.ABSORB_LIFE);
        AddAdvertisedAction(INTERACTION_TYPE.ABSORB_POWER);
        ConstructInitialGoapAdvertisementActions();
        needsComponent.SetFullnessForcedTick(0);
        needsComponent.SetTirednessForcedTick(0);
        needsComponent.SetHappinessForcedTick(0);
        behaviourComponent.ChangeDefaultBehaviourSet(CharacterManager.Default_Monster_Behaviour);
        movementComponent.AvoidAllFactions();
    }
    public override void OnActionPerformed(ActualGoapNode node) { } //overridden OnActionStateSet so that summons cannot witness other events.
    public override void Death(string cause = "normal", ActualGoapNode deathFromAction = null, Character responsibleCharacter = null, Log _deathLog = default, LogFillerStruct[] deathLogFillers = null,
        Interrupt interrupt = null) {
        if (!_isDead) {
            Region deathLocation = currentRegion;
            LocationStructure deathStructure = currentStructure;
            LocationGridTile deathTile = gridTileLocation;

            if (isLycanthrope) {
                //Added this so that human and lycan form can share the same death log and prevent duplicates
                Character humanForm = lycanData.originalForm;
                lycanData.LycanDies(this, cause, deathFromAction, responsibleCharacter, _deathLog, deathLogFillers);
                _deathLog = humanForm.deathLog;
                _deathLog.AddInvolvedObjectManual(persistentID);
            }
            
            SetIsDead(true);
            if (isLimboCharacter && isInLimbo) {
                //If a limbo character dies while in limbo, that character should not process death, instead he/she will be removed from the list
                CharacterManager.Instance.RemoveLimboCharacter(this);
                return;
            }
            //Remove disguise first before processing death
            reactionComponent.SetDisguisedCharacter(null);

            UnsubscribeSignals();
            if (stateComponent.currentState != null) {
                stateComponent.ExitCurrentState();
            }
            Messenger.Broadcast(CharacterSignals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, this as IPointOfInterest, "target is already dead");
            Messenger.Broadcast(CharacterSignals.FORCE_CANCEL_ALL_ACTIONS_TARGETING_POI, this as IPointOfInterest, "target is already dead");
            jobQueue.CancelAllJobs();
            DropAllItems(deathTile);
            UnownOrTransferOwnershipOfAllItems();

            reactionComponent.SetIsHidden(false);
            //clear traits that need to be removed
            traitsNeededToBeRemoved.Clear();

            UncarryPOI();
            Character carrier = isBeingCarriedBy;
            carrier?.UncarryPOI(this);

            if (homeRegion != null) {
                Region home = homeRegion;
                LocationStructure homeStructure = this.homeStructure;
                homeRegion.RemoveResident(this);
                MigrateHomeStructureTo(null, addToRegionResidents: false);
                SetHomeRegion(home); //keep this data with character to prevent errors
                //SetHomeStructure(homeStructure); //keep this data with character to prevent errors
            }
            if (partyComponent.hasParty) {
                partyComponent.currentParty.RemoveMember(this);
            }

            traitContainer.RemoveAllTraitsAndStatusesByName(this, "Criminal"); //remove all criminal type traits

            List<Trait> traitOverrideFunctions = traitContainer.GetTraitOverrideFunctions(TraitManager.Death_Trait);
            if (traitOverrideFunctions != null) {
                for (int i = 0; i < traitOverrideFunctions.Count; i++) {
                    Trait trait = traitOverrideFunctions[i];
                    if (trait.OnDeath(this)) {
                        i--;
                    }
                }
            }

            if (interruptComponent.isInterrupted && interruptComponent.currentInterrupt.interrupt != interrupt) {
                interruptComponent.ForceEndNonSimultaneousInterrupt();
            }
            traitContainer.AddTrait(this, "Dead", responsibleCharacter, gainedFromDoing: deathFromAction);
            if (cause == "attacked" && responsibleCharacter != null && responsibleCharacter.isInWerewolfForm) {
                traitContainer.AddTrait(this, "Mangled", responsibleCharacter, gainedFromDoing: deathFromAction);
            }
            Messenger.Broadcast(CharacterSignals.CHARACTER_DEATH, this as Character);

            marker?.OnDeath(deathTile);
            behaviourComponent.OnDeath();
            jobQueue.CancelAllJobs();

            Log localDeathLog;
            if (!_deathLog.hasValue) {
                localDeathLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "Generic", $"death_{cause}", null, LOG_TAG.Life_Changes);
                localDeathLog.AddToFillers(this, name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                if (responsibleCharacter != null) {
                    localDeathLog.AddToFillers(responsibleCharacter, responsibleCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                }
                if (deathLogFillers != null) {
                    for (int i = 0; i < deathLogFillers.Length; i++) {
                        localDeathLog.AddToFillers(deathLogFillers[i]);
                    }
                }
                //will only add death log to history if no death log is provided. NOTE: This assumes that if a death log is provided, it has already been added to this characters history.
                localDeathLog.AddLogToDatabase();
                if (showNotificationOnDeath) {
                    PlayerManager.Instance.player.ShowNotificationFrom(this, localDeathLog);    
                }
            } else {
                localDeathLog = _deathLog;
            }
            SetDeathLog(localDeathLog);
            AfterDeath(deathTile);
        }
    }
    protected override void OnTickStarted() {
        ProcessTraitsOnTickStarted();
        StartTickGoapPlanGeneration();
    }
    public override void OnUnseizePOI(LocationGridTile tileLocation) {
        base.OnUnseizePOI(tileLocation);
        //Reference: https://trello.com/c/Flr78mJy/3037-summon-types-should-no-longer-relocate-home-when-unseized-if-they-belong-to-a-major-faction
        if (faction != null && !faction.isMajorFaction) {
            //If you drop a monster at a structure that is not yet full and not occupied by villagers, they will set their home to that place.
            if(tileLocation.structure != null) {
                if(ShouldRelocateHomeToStructureOnUnseize(tileLocation.structure)) {
                    ClearTerritoryAndMigrateHomeStructureTo(tileLocation.structure);
                }
            }    
        }
    }
    private bool ShouldRelocateHomeToStructureOnUnseize(LocationStructure p_structure) {
        if (p_structure is Wilderness || p_structure is DemonicStructure) {
            return false;
        }
        //Reference: https://trello.com/c/NoWry5Tk/3041-monster-sets-home-to-settlement-structure-with-no-residents-if-unseized-there-even-though-settlement-is-still-owned-by-a-village
        if (p_structure.settlementLocation?.owner != null) {
            return false;
        }
        if (p_structure.HasReachedMaxResidentCapacity()) {
            return false;
        }
        if (p_structure.HasResidentThatMeetCriteria(r => r.isNormalCharacter)) {
            return false;
        }
        return true;
    }
    public override void LoadReferences(SaveDataCharacter data) {
        base.LoadReferences(data);
        Messenger.RemoveListener(Signals.HOUR_STARTED, () => needsComponent.DecreaseNeeds()); //do not make summons decrease needs
        movementComponent.UpdateSpeed();
    }
    #endregion

    #region Virtuals
    /// <summary>
    /// What should a summon do when it is placed.
    /// </summary>
    /// <param name="tile">The tile the summon was placed on.</param>
    public virtual void OnPlaceSummon(LocationGridTile tile) {
        SubscribeToSignals();
        Messenger.RemoveListener(Signals.HOUR_STARTED, () => needsComponent.DecreaseNeeds()); //do not make summons decrease needs
        movementComponent.UpdateSpeed();
        behaviourComponent.OnSummon(tile);
    }
    protected virtual void AfterDeath(LocationGridTile deathTileLocation) {
        if (marker == null && destroyMarkerOnDeath) {
            GameManager.Instance.CreateParticleEffectAt(deathTileLocation, PARTICLE_EFFECT.Minion_Dissipate);
        }
        behaviourComponent.SetIsHarassing(false, null);
        behaviourComponent.SetIsInvading(false, null);
        behaviourComponent.SetIsDefending(false, null);
    }
    public virtual void OnSummonAsPlayerMonster() {
        combatComponent.SetCombatMode(COMBAT_MODE.Aggressive);
    }
    #endregion

    #region Player Action Target
    public override void ConstructDefaultActions() {
        if (actions == null) {
            actions = new List<SPELL_TYPE>();
        } else {
            actions.Clear();
        }
        AddPlayerAction(SPELL_TYPE.SEIZE_MONSTER);
        AddPlayerAction(SPELL_TYPE.BREED_MONSTER);
        AddPlayerAction(SPELL_TYPE.AGITATE);
        AddPlayerAction(SPELL_TYPE.SNATCH);
        AddPlayerAction(SPELL_TYPE.SACRIFICE);
    }
    #endregion

    #region Selecatble
    public override bool IsCurrentlySelected() {
        Character characterToSelect = this;
        if (isLycanthrope) {
            characterToSelect = lycanData.activeForm;
        }
        return UIManager.Instance.monsterInfoUI.isShowing &&
               UIManager.Instance.monsterInfoUI.activeMonster == characterToSelect;
    }
    #endregion

    #region Utilities
    public void SetShowNotificationOnDeath(bool showNotificationOnDeath) {
        this.showNotificationOnDeath = showNotificationOnDeath;
    }
    public void SetIsVolatile(bool isVolatile) {
        isVolatileMonster = isVolatile;
    }
    #endregion
}

public class SummonSlot {
    public int level;
    public Summon summon;
    public bool isLocked {
        get { return false; }
    }

    public SummonSlot() {
        level = 1;
        summon = null;
    }

    public void SetSummon(Summon summon) {
        this.summon = summon;
        //if (this.summon != null) {
        //    this.summon.StartingLevel();
        //}
    }

    //public void LevelUp() {
    //    level++;
    //    level = Mathf.Clamp(level, 1, PlayerDB.MAX_LEVEL_SUMMON);
    //    if (this.summon != null) {
    //        this.summon.SetLevel(level);
    //    }
    //    Messenger.Broadcast(Signals.PLAYER_GAINED_SUMMON_LEVEL, this);
    //}
    //public void SetLevel(int amount) {
    //    level = amount;
    //    level = Mathf.Clamp(level, 1, PlayerDB.MAX_LEVEL_SUMMON);
    //    if (this.summon != null) {
    //        this.summon.SetLevel(level);
    //    }
    //    Messenger.Broadcast(Signals.PLAYER_GAINED_SUMMON_LEVEL, this);
    //}
}

[System.Serializable]
public class SaveDataSummon : SaveDataCharacter {
    public SUMMON_TYPE summonType;
    public bool isVolatileMonster;

    public override void Save(Character data) {
        base.Save(data);
        if(data is Summon summon) {
            summonType = summon.summonType;
            isVolatileMonster = summon.isVolatileMonster;
        }
    }
    public override Character Load() {
        Summon summon = CharacterManager.Instance.CreateNewSummon(this);
        return summon;
    }
}