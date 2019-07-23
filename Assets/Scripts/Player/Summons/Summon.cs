﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Summon : Character {

	public SUMMON_TYPE summonType { get; private set; }
    public bool hasBeenUsed { get; private set; } //has this summon been used in the current map. TODO: Set this to false at end of invasion of map.

    public Summon(SUMMON_TYPE summonType, CharacterRole role, RACE race, GENDER gender) : base(role, race, gender) {
        this.summonType = summonType;
    }
    public Summon(SUMMON_TYPE summonType, CharacterRole role, string className, RACE race, GENDER gender) : base(role, className, race, gender) {
        this.summonType = summonType;
    }

    #region Overrides
    public override void Initialize() {
        OnUpdateRace();
        OnUpdateCharacterClass();
        UpdateIsCombatantState();

        SetMoodValue(90);

        CreateOwnParty();

        tiredness = TIREDNESS_DEFAULT;
        fullness = FULLNESS_DEFAULT;
        happiness = HAPPINESS_DEFAULT;


        hSkinColor = UnityEngine.Random.Range(-360f, 360f);
        hHairColor = UnityEngine.Random.Range(-360f, 360f);
        demonColor = UnityEngine.Random.Range(-144f, 144f);

        //supply
        //SetSupply(UnityEngine.Random.Range(10, 61)); //Randomize initial supply per character (Random amount between 10 to 60.)

        ConstructInitialGoapAdvertisementActions();
        //SubscribeToSignals(); //NOTE: Only made characters subscribe to signals when their area is the one that is currently active. TODO: Also make sure to unsubscribe a character when the player has completed their map.
        //GetRandomCharacterColor();
        //GameDate gameDate = GameManager.Instance.Today();
        //gameDate.AddTicks(1);
        //SchedulingManager.Instance.AddEntry(gameDate, () => PlanGoapActions());
    }
    protected override void OnActionStateSet(GoapAction action, GoapActionState state) { } //overriddn OnActionStateSet so that summons cannot witness other events.
    protected override void OnSuccessInvadeArea(Area area) {
        base.OnSuccessInvadeArea(area);
        //clean up
        Reset();
        PlayerManager.Instance.player.playerArea.AddCharacterToLocation(this);
        ResetToFullHP();
    }
    public override void Death(string cause = "normal", GoapAction deathFromAction = null, Character responsibleCharacter = null) {
        if (!_isDead) {
            Area deathLocation = ownParty.specificLocation;
            LocationStructure deathStructure = currentStructure;
            LocationGridTile deathTile = gridTileLocation;

            SetIsDead(true);
            UnsubscribeSignals();

            if (currentParty.specificLocation == null) {
                throw new Exception("Specific location of " + this.name + " is null! Please use command /l_character_location_history [Character Name/ID] in console menu to log character's location history. (Use '~' to show console menu)");
            }
            if (stateComponent.currentState != null) {
                stateComponent.currentState.OnExitThisState();
            } else if (stateComponent.stateToDo != null) {
                stateComponent.SetStateToDo(null);
            }
            CancelAllJobsTargettingThisCharacter("target is already dead", false);
            Messenger.Broadcast(Signals.CANCEL_CURRENT_ACTION, this as Character, "target is already dead");
            if (currentAction != null && !currentAction.cannotCancelAction) {
                currentAction.StopAction();
            }
            if (ownParty.specificLocation != null && isHoldingItem) {
                DropAllTokens(ownParty.specificLocation, currentStructure, deathTile, true);
            }

            //clear traits that need to be removed
            traitsNeededToBeRemoved.Clear();

            if (!IsInOwnParty()) {
                _currentParty.RemoveCharacter(this);
            }
            _ownParty.PartyDeath();

            if (_role != null) {
                _role.OnDeath(this);
            }

            RemoveAllTraitsByType(TRAIT_TYPE.CRIMINAL); //remove all criminal type traits

            for (int i = 0; i < normalTraits.Count; i++) {
                normalTraits[i].OnDeath(this);
            }

            marker.OnDeath(deathTile);
            _numOfWaitingForGoapThread = 0; //for raise dead
            Dead dead = new Dead();
            dead.SetCharacterResponsibleForTrait(responsibleCharacter);
            AddTrait(dead, gainedFromDoing: deathFromAction);
            Messenger.Broadcast(Signals.CHARACTER_DEATH, this as Character);

            CancelAllJobsAndPlans();

            Debug.Log(GameManager.Instance.TodayLogString() + this.name + " died of " + cause);
            Log log = new Log(GameManager.Instance.Today(), "Character", "Generic", "death_" + cause);
            log.AddToFillers(this, name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            AddHistory(log);
        }
    }
    #endregion

    #region Virtuals
    /// <summary>
    /// What should a summon do when it is placed.
    /// </summary>
    /// <param name="tile">The tile the summon was placed on.</param>
    public virtual void OnPlaceSummon(LocationGridTile tile) {
        hasBeenUsed = true;
        SubscribeToSignals();
        Messenger.RemoveListener(Signals.HOUR_STARTED, DecreaseNeeds); //do not make summons decrease needs
        Messenger.RemoveListener(Signals.TICK_STARTED, DailyGoapPlanGeneration); //do not make summons plan goap actions by default
        if (GameManager.Instance.isPaused) {
            marker.pathfindingAI.AdjustDoNotMove(1);
            marker.PauseAnimation();
        }
    }
    #endregion

    public void Reset() {
        hasBeenUsed = false;
        SetIsDead(false);
        if (_ownParty == null) {
            CreateOwnParty();
            ownParty.CreateIcon();
        }
        RemoveAllNonPersistentTraits();
        ClearAllAwareness();
        CancelAllJobsAndPlans();
    }

}
