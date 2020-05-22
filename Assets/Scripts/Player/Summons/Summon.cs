using System;
using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Traits;
using UnityEngine;
using Interrupts;

public class Summon : Character, IWorldObject {

	public SUMMON_TYPE summonType { get; private set; }
    public bool hasBeenUsed { get; private set; } //has this summon been used in the current map. TODO: Set this to false at end of invasion of map.
    
    #region getters/setters
    public virtual string worldObjectName {
        get { return $"{name} ({UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(summonType.ToString())})"; }
    }
    public WORLD_OBJECT_TYPE worldObjectType {
        get { return WORLD_OBJECT_TYPE.SUMMON; }
    }
    #endregion

    // public Summon(SUMMON_TYPE summonType, CharacterRole role, RACE race, GENDER gender) : base(role, race, gender) {
    //     this.summonType = summonType;
    //     territorries = new List<HexTile>();
    // }
    protected Summon(SUMMON_TYPE summonType, string className, RACE race, GENDER gender) : base(className, race, gender) {
        this.summonType = summonType;
        //territorries = new List<HexTile>();
    }
    protected Summon(SaveDataCharacter data) : base(data) {
        this.summonType = data.summonType;
        //territorries = new List<HexTile>();
    }

    #region Overrides
    public override void Initialize() {
        ConstructDefaultActions();
        OnUpdateRace();
        OnUpdateCharacterClass();

        moodComponent.OnCharacterBecomeMinionOrSummon();
        moodComponent.SetMoodValue(50);

        CreateOwnParty();
        
        needsComponent.Initialize();
        
        advertisedActions.Clear(); //This is so that any advertisements from OnUpdateRace will be negated. TODO: Make updating advertisements better.
        ConstructInitialGoapAdvertisementActions();
        needsComponent.SetFullnessForcedTick(0);
        needsComponent.SetTirednessForcedTick(0);
        behaviourComponent.ChangeDefaultBehaviourSet(CharacterManager.Default_Monster_Behaviour);
        //SubscribeToSignals(); //NOTE: Only made characters subscribe to signals when their npcSettlement is the one that is currently active. TODO: Also make sure to unsubscribe a character when the player has completed their map.
    }
    public override void OnActionPerformed(ActualGoapNode node) { } //overridden OnActionStateSet so that summons cannot witness other events.
    protected override void OnSuccessInvadeArea(NPCSettlement npcSettlement) {
        base.OnSuccessInvadeArea(npcSettlement);
        //clean up
        Reset();
        //PlayerManager.Instance.player.playerNpcSettlement.AddCharacterToLocation(this);
        //ResetToFullHP();
        Death();
    }
    public override void Death(string cause = "normal", ActualGoapNode deathFromAction = null, Character responsibleCharacter = null, Log _deathLog = null, LogFiller[] deathLogFillers = null, Interrupt interrupt = null) {
        if (!_isDead) {
            Region deathLocation = currentRegion;
            LocationStructure deathStructure = currentStructure;
            LocationGridTile deathTile = gridTileLocation;

            if (lycanData != null) {
                lycanData.LycanDies(this, cause, deathFromAction, responsibleCharacter, _deathLog, deathLogFillers);
            }
            
            SetIsDead(true);
            if (isLimboCharacter && isInLimbo) {
                //If a limbo character dies while in limbo, that character should not process death, instead he/she will be removed from the list
                CharacterManager.Instance.RemoveLimboCharacter(this);
                return;
            }
            UnsubscribeSignals();
            behaviourComponent.SetIsHarassing(false, null);
            behaviourComponent.SetIsInvading(false, null);
            behaviourComponent.SetIsDefending(false, null);

            //if (currentParty.specificLocation == null) {
            //    throw new Exception("Specific location of " + this.name + " is null! Please use command /l_character_location_history [Character Name/ID] in console menu to log character's location history. (Use '~' to show console menu)");
            //}
            if (stateComponent.currentState != null) {
                stateComponent.ExitCurrentState();
            }
            //else if (stateComponent.stateToDo != null) {
            //    stateComponent.SetStateToDo(null);
            //}
            //if (deathFromAction != null) { //if this character died from an action, do not cancel the action that he/she died from. so that the action will just end as normal.
            //    CancelAllJobsTargettingThisCharacterExcept(deathFromAction, "target is already dead", false);
            //} else {
            //    CancelAllJobsTargettingThisCharacter("target is already dead", false);
            //}
            //ForceCancelAllJobsTargettingCharacter(false, "target is already dead");
            Messenger.Broadcast(Signals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, this as IPointOfInterest, "target is already dead");
            CancelAllJobs();
            //Messenger.Broadcast(Signals.CANCEL_CURRENT_ACTION, this as Character, "target is already dead");
            //if (currentActionNode != null) {
            //    currentActionNode.StopActionNode(false);
            //}
            if (currentSettlement != null && isHoldingItem) {
                DropAllItems(deathTile);
            }
            //if (ownParty.specificLocation != null && isHoldingItem) {
            //    DropAllTokens(ownParty.specificLocation, currentStructure, deathTile, true);
            //}

            //clear traits that need to be removed
            traitsNeededToBeRemoved.Clear();

            Character carrier = isBeingCarriedBy;
            if (carrier != null) {
                carrier.UncarryPOI(this);
            }
            ownParty.PartyDeath();
            currentRegion?.RemoveCharacterFromLocation(this);
            SetRegionLocation(deathLocation); //set the specific location of this party, to the location it died at
            SetCurrentStructureLocation(deathStructure, false);

            // if (_role != null) {
            //     _role.OnDeath(this);
            // }

            if (homeRegion != null) {
                Region home = homeRegion;
                LocationStructure homeStructure = this.homeStructure;
                homeRegion.RemoveResident(this);
                SetHomeRegion(home); //keep this data with character to prevent errors
                SetHomeStructure(homeStructure); //keep this data with character to prevent errors
            }
            //if (homeNpcSettlement != null) {
            //    NPCSettlement home = homeNpcSettlement;
            //    Dwelling homeStructure = this.homeStructure;
            //    homeNpcSettlement.RemoveResident(this);
            //    SetHome(home); //keep this data with character to prevent errors
            //    SetHomeStructure(homeStructure); //keep this data with character to prevent errors
            //}

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
            //for (int i = 0; i < traitContainer.allTraitsAndStatuses.Count; i++) {
            //    if (traitContainer.allTraitsAndStatuses[i].OnDeath(this)) {
            //        i--;
            //    }
            //}

            marker?.OnDeath(deathTile);

            if (interruptComponent.isInterrupted && interruptComponent.currentInterrupt != interrupt) {
                interruptComponent.ForceEndNonSimultaneousInterrupt();
            }

            Dead dead = new Dead();
            dead.AddCharacterResponsibleForTrait(responsibleCharacter);
            traitContainer.AddTrait(this, dead, gainedFromDoing: deathFromAction);
            Messenger.Broadcast(Signals.CHARACTER_DEATH, this as Character);

            CancelAllJobs();

            //Debug.Log(GameManager.Instance.TodayLogString() + this.name + " died of " + cause);
            Log deathLog;
            if (_deathLog == null) {
                deathLog = new Log(GameManager.Instance.Today(), "Character", "Generic", $"death_{cause}");
                deathLog.AddToFillers(this, name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                if (responsibleCharacter != null) {
                    deathLog.AddToFillers(responsibleCharacter, responsibleCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                }
                if (deathLogFillers != null) {
                    for (int i = 0; i < deathLogFillers.Length; i++) {
                        deathLog.AddToFillers(deathLogFillers[i]);
                    }
                }
                //will only add death log to history if no death log is provided. NOTE: This assumes that if a death log is provided, it has already been added to this characters history.
                logComponent.AddHistory(deathLog);
                PlayerManager.Instance.player.ShowNotificationFrom(this, deathLog);
            } else {
                deathLog = _deathLog;
            }

            AfterDeath(deathTile);
        }
    }
    protected override void OnTickStarted() {
        //What happens every start of tick

        //Out of combat hp recovery
        if (!isDead && !isInCombat) {
            HPRecovery(0.0025f);
        }
        ProcessTraitsOnTickStarted();
        StartTickGoapPlanGeneration();
        // if (!ownParty.icon.isTravelling && !isInCombat) {
        //     GoToWorkArea();
        // }

        //StartTickGoapPlanGeneration();

        //if (isDead || minion != null) {
        //    return;
        //}

        ////Out of combat hp recovery
        //if (stateComponent.currentState == null || stateComponent.currentState.characterState != CHARACTER_STATE.COMBAT) {
        //    HPRecovery(0.0025f);
        //}

        ////This is to ensure that this character will not be idle forever
        ////If at the start of the tick, the character is not currently doing any action, and is not waiting for any new plans, it means that the character will no longer perform any actions
        ////so start doing actions again
        //SetHasAlreadyAskedForPlan(false);
        //if (CanPlanGoap()) {
        //    PerStartTickActionPlanning();
        //}
    }
    //protected override void PerStartTickActionPlanning() {
    //    //base.IdlePlans();
    //    GoToWorkArea();
    //}
    #endregion

    #region Virtuals
    /// <summary>
    /// What should a summon do when it is placed.
    /// </summary>
    /// <param name="tile">The tile the summon was placed on.</param>
    public virtual void OnPlaceSummon(LocationGridTile tile) {
        hasBeenUsed = true;
        SubscribeToSignals();
        Messenger.RemoveListener(Signals.HOUR_STARTED, () => needsComponent.DecreaseNeeds()); //do not make summons decrease needs
        //Messenger.RemoveListener(Signals.TICK_STARTED, PerTickGoapPlanGeneration); //do not make summons plan goap actions by default
        //if (GameManager.Instance.isPaused) {
        //    DecreaseCanMove(); //TODO: Handle this somehwere better?
        //    marker.PauseAnimation();
        //}
        movementComponent.UpdateSpeed();
    }
    protected virtual void AfterDeath(LocationGridTile deathTileLocation) { }
    #endregion

    public void Reset() {
        hasBeenUsed = false;
        SetIsDead(false);
        if (ownParty == null) {
            CreateOwnParty();
            ownParty.CreateIcon();
        }
        traitContainer.RemoveAllNonPersistentTraitAndStatuses(this);
        //ClearAllAwareness();
        CancelAllJobs();
        ResetToFullHP();
    }

    #region World Object
    public void Obtain() {
        //invading a region with a summon will recruit that summon for the player
        //UIManager.Instance.ShowImportantNotification(GameManager.Instance.Today(), "Gained new Summon: " + this.summonType.SummonName(), () => PlayerManager.Instance.player.GainSummon(this, true));
    }
    #endregion

    #region Player Action Target
    public override void ConstructDefaultActions() {
        if (actions == null) {
            actions = new List<SPELL_TYPE>();
        } else {
            actions.Clear();
        }
        //PlayerAction seizeAction = new PlayerAction(PlayerDB.Seize_Character_Action,
        //() => !PlayerManager.Instance.player.seizeComponent.hasSeizedPOI && !this.traitContainer.HasTrait("Leader", "Blessed"),
        //null,
        //() => PlayerManager.Instance.player.seizeComponent.SeizePOI(this));

        AddPlayerAction(SPELL_TYPE.SEIZE_MONSTER);
        AddPlayerAction(SPELL_TYPE.BREED_MONSTER);
    }
    #endregion

    #region Selecatble
    public override bool IsCurrentlySelected() {
        return UIManager.Instance.monsterInfoUI.isShowing &&
               UIManager.Instance.monsterInfoUI.activeMonster == this;
    }
    #endregion
}

public class SummonSlot {
    public int level;
    public Summon summon;
    public bool isLocked {
        get { return false; }
        //get { return PlayerManager.Instance.player.GetIndexForSummonSlot(this) >= PlayerManager.Instance.player.maxSummonSlots; }
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
