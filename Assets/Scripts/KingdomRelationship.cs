﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class KingdomRelationship {

    private Kingdom _sourceKingdom;
    private Kingdom _targetKingdom;
    private RELATIONSHIP_STATUS _relationshipStatus;
    private List<History> _relationshipHistory;
    private List<ExpirableModifier> _eventModifiers;
    private int _like;
    private int _eventLikenessModifier;
    public int forTestingLikeModifier;
    private string _relationshipSummary;
    private string _relationshipEventsSummary;
    private bool _isInitial;
    private bool _isSharingBorder;

    private bool _isAtWar;
	private bool _isAdjacent;
	private bool _isDiscovered;
	private bool _isRecentWar;
//	private bool _isPreparingForWar;
    private Wars _war;
    private InvasionPlan _invasionPlan;
    private RequestPeace _requestPeace;
    private KingdomWar _kingdomWarData;
    private GameDate _requestPeaceCooldown;
    private MilitaryAllianceOffer _currentActiveMilitaryAllianceOffer;
    private MutualDefenseTreaty _currentActiveDefenseTreatyOffer;

	private bool _isMilitaryAlliance;
	private bool _isMutualDefenseTreaty;

	private Dictionary<EVENT_TYPES, bool> _eventBuffs;

	private GameDate _currentExpirationDefenseTreaty;
	private GameDate _currentExpirationMilitaryAlliance;

	//Kingdom Threat
	private float _racePercentageModifier;
	private float _targetKingdomThreatLevel;
	private float _targetKingdomInvasionValue;

	internal int _effectivePower;
	internal int _effectiveDef;

	internal int _usedSourceEffectivePower;
	internal int _usedSourceEffectiveDef;
	internal int _usedTargetEffectivePower;
	internal int _usedTargetEffectiveDef;

	private Warfare _warfare;
	private Battle _battle;

    #region getters/setters
    public Kingdom sourceKingdom {
        get { return _sourceKingdom; }
    }
    public Kingdom targetKingdom {
        get { return _targetKingdom; }
    }
    public RELATIONSHIP_STATUS relationshipStatus {
        get { return _relationshipStatus; }
    }
    public List<ExpirableModifier> eventModifiers {
        get { return this._eventModifiers; }
    }
    public string relationshipSummary {
        get { return this._relationshipSummary + this._relationshipEventsSummary; }
    }
    public int totalLike {
        get { return _like + _eventLikenessModifier + forTestingLikeModifier; }
    }
    public int eventLikenessModifier {
        get { return _eventLikenessModifier; }
    }
    public bool isAtWar {
        get { return _isAtWar; }
    }
//	public bool isPreparingForWar {
//		get { return this._isPreparingForWar; }
//	}
    public Wars war {
        get { return _war; }
    }
    public bool isSharingBorder {
        get { return _isSharingBorder; }
    }
    public KingdomWar kingdomWarData {
        get { return _kingdomWarData; }
    }
    public InvasionPlan invasionPlan {
        get { return _invasionPlan; }
    }
    public RequestPeace requestPeace {
        get { return _requestPeace; }
    }
    public GameDate requestPeaceCooldown {
        get { return _requestPeaceCooldown; }
    }
    public MilitaryAllianceOffer currentActiveMilitaryAllianceOffer {
        get { return _currentActiveMilitaryAllianceOffer; }
        set {
            if(_currentActiveMilitaryAllianceOffer != null && _currentActiveMilitaryAllianceOffer.isActive) {
                throw new System.Exception (_sourceKingdom.name + " still has an active military alliance offer with " + _targetKingdom.name);
            } else {
                _currentActiveMilitaryAllianceOffer = value;
            }
        }
    }
    public MutualDefenseTreaty currentActiveDefenseTreatyOffer {
        get { return _currentActiveDefenseTreatyOffer; }
        set {
            if (_currentActiveDefenseTreatyOffer != null && _currentActiveDefenseTreatyOffer.isActive) {
                throw new System.Exception(_sourceKingdom.name + " still has an active defense treaty offer with " + _targetKingdom.name);
            } else {
                _currentActiveDefenseTreatyOffer = value;
            }
        }
    }
    public bool isMilitaryAlliance {
		get { return this._isMilitaryAlliance; }
	}
	public bool isMutualDefenseTreaty {
		get { return this._isMutualDefenseTreaty; }
	}
	public bool isAdjacent {
        get { return this._isAdjacent; }
	}
	public Dictionary<EVENT_TYPES, bool> eventBuffs {
		get { return this._eventBuffs; }
	}
	public float targetKingdomThreatLevel{
		get {
			return this._targetKingdomThreatLevel;
		}
	}
	public float targetKingdomInvasionValue{
		get {
			return this._targetKingdomInvasionValue;
		}
	}
	public bool isDiscovered {
		get { return this._isDiscovered; }
	}
	public Warfare warfare{
		get { return this._warfare; }
	}
	public Battle battle{
		get { return this._battle; }
	}
	public bool isRecentWar {
		get { return this._isRecentWar; }
	}
    #endregion

    /* <summary> Create a new relationship between 2 kingdoms </summary>
     * <param name="sourceKingdom">Kingdom that this relationship object belongs to.</param> 
     * <param name="targetKingdom">Kingdom that this relationship is aimed towards.</param>
     * */
    public KingdomRelationship(Kingdom _sourceKingdom, Kingdom _targetKingdom) {
        this._sourceKingdom = _sourceKingdom;
        this._targetKingdom = _targetKingdom;
        _relationshipHistory = new List<History>();
        _eventModifiers = new List<ExpirableModifier>();
        _like = 0;
        _eventLikenessModifier = 0;
        _relationshipSummary = string.Empty;
        _relationshipEventsSummary = string.Empty;
        _isInitial = false;
        _kingdomWarData = new KingdomWar(_targetKingdom);
        _requestPeaceCooldown = new GameDate(0,0,0);
		this._isMilitaryAlliance = false;
		this._isMutualDefenseTreaty = false;
		this._isAdjacent = false;
		this._isRecentWar = false;
//		this._isPreparingForWar = false;
		this._isAtWar = false;
		this._currentExpirationDefenseTreaty = new GameDate (0, 0, 0);
		this._currentExpirationMilitaryAlliance = new GameDate (0, 0, 0);
		this._warfare = null;
		this._battle = null;

		this._eventBuffs = new Dictionary<EVENT_TYPES, bool>(){
			{EVENT_TYPES.TRIBUTE, false},
			{EVENT_TYPES.INSTIGATION, false},
		};

		SetRaceThreatModifier ();
        //UpdateLikeness(null);
    }

    /*
     * <summary>
     * Force change the base like value of this relationship
     * NOTE: This will automatically update relationshipStatus
     * </summary>
     * */
    internal void SetLikeness(int likeness) {
        _like = likeness;
        UpdateKingRelationshipStatus();
    }

    /*
     * <summary>
     * Update relationshipStatus based on total likeness.
     * </summary>
     * */
    internal void UpdateKingRelationshipStatus() {
        RELATIONSHIP_STATUS previousStatus = _relationshipStatus;
        if (totalLike <= -81) {
            _relationshipStatus = RELATIONSHIP_STATUS.SPITE;
        } else if (totalLike >= -80 && totalLike <= -41) {
            _relationshipStatus = RELATIONSHIP_STATUS.HATE;
        } else if (totalLike >= -40 && totalLike <= -21) {
            _relationshipStatus = RELATIONSHIP_STATUS.DISLIKE;
        } else if (totalLike >= -20 && totalLike <= 20) {
            _relationshipStatus = RELATIONSHIP_STATUS.NEUTRAL;
        } else if (totalLike >= 21 && totalLike <= 40) {
            _relationshipStatus = RELATIONSHIP_STATUS.LIKE;
        } else if (totalLike >= 41 && totalLike <= 80) {
            _relationshipStatus = RELATIONSHIP_STATUS.AFFECTIONATE;
        } else {
            _relationshipStatus = RELATIONSHIP_STATUS.LOVE;
        }
        //if (previousStatus != _relationshipStatus) {
        //    //relationship status changed
        //    _sourceKingdom.UpdateMutualRelationships();
        //}
    }

    //internal void ResetMutualRelationshipModifier() {
    //    _likeFromMutualRelationships = 0;
    //}

    //internal void AddMutualRelationshipModifier(int amount) {
    //    this._likeFromMutualRelationships += amount;
    //}

    /*
     * <summary>
     * Update likeness based on set criteria (shared values, shared kingdom type, etc.)
     * </summary>
     * */
    internal void UpdateLikeness(GameEvent gameEventTrigger, ASSASSINATION_TRIGGER_REASONS assassinationReasons = ASSASSINATION_TRIGGER_REASONS.NONE, bool isDiscovery = false) {
        this._relationshipSummary = string.Empty;
        int baseLoyalty = 0;
        int adjustment = 0;


        //Kingdom Type
        if (_sourceKingdom.kingdomTypeData.dictRelationshipKingdomType.ContainsKey(_targetKingdom.kingdomType)) {
            adjustment = _sourceKingdom.kingdomTypeData.dictRelationshipKingdomType[_targetKingdom.kingdomType];
            baseLoyalty += adjustment;
            if (adjustment >= 0) {
                this._relationshipSummary += "+";
            }
            this._relationshipSummary += adjustment.ToString() + " Kingdom Type.\n";
        }

        //Recent War
		if (this._isRecentWar) {
            adjustment = -30;
            baseLoyalty += adjustment;
            this._relationshipSummary += adjustment.ToString() + " Recent War.\n";
        }

		//Race
		if (this._sourceKingdom.race == this._targetKingdom.race) {
			adjustment = 15;
			baseLoyalty += adjustment;
			this._relationshipSummary += "+" + adjustment.ToString() + " Same Race.\n";
		}else{
			adjustment = -15;
			baseLoyalty += adjustment;
			this._relationshipSummary += adjustment.ToString() + " Different Race.\n";
		}

		//Charisma Trait
		if(this._targetKingdom.king.charisma == CHARISMA.CHARISMATIC){
			adjustment = 15;
			baseLoyalty += adjustment;
			this._relationshipSummary += "+" + adjustment.ToString() + " Charmed.\n";
		}else if(this._targetKingdom.king.charisma == CHARISMA.REPULSIVE){
			adjustment = -15;
			baseLoyalty += adjustment;
			this._relationshipSummary += adjustment.ToString() + " Repulsed.\n";
		}

		//Military Trait
		if(this._sourceKingdom.king.military == MILITARY.PACIFIST){
			if(this._targetKingdom.king.military != MILITARY.HOSTILE){
				adjustment = 15;
				baseLoyalty += adjustment;
				this._relationshipSummary += "+" + adjustment.ToString() + " Pacifist.\n";
			}else{
				adjustment = -15;
				baseLoyalty += adjustment;
				this._relationshipSummary += adjustment.ToString() + " Disapproved Hostility.\n";
			}
		}else if(this._sourceKingdom.king.military == MILITARY.HOSTILE){
			adjustment = -15;
			baseLoyalty += adjustment;
			this._relationshipSummary += adjustment.ToString() + " Hostile.\n";
		}

		//Science Trait
		if(this._sourceKingdom.king.science == SCIENCE.ERUDITE && this._targetKingdom.king.science == SCIENCE.ERUDITE){
			adjustment = 15;
			baseLoyalty += adjustment;
			this._relationshipSummary += "+" + adjustment.ToString() + " Both Erudite.\n";
		}else if(this._sourceKingdom.king.science == SCIENCE.ERUDITE && this._targetKingdom.king.science == SCIENCE.IGNORANT){
			adjustment = -15;
			baseLoyalty += adjustment;
			this._relationshipSummary += adjustment.ToString() + " Dislikes Ignorant.\n";
		}

		//Intelligence Trait
		if(this._sourceKingdom.king.intelligence == INTELLIGENCE.SMART && this._targetKingdom.king.intelligence == INTELLIGENCE.SMART){
			adjustment = 15;
			baseLoyalty += adjustment;
			this._relationshipSummary += "+" + adjustment.ToString() + " Both Smart.\n";
		}else if(this._sourceKingdom.king.intelligence == INTELLIGENCE.SMART && this._targetKingdom.king.intelligence == INTELLIGENCE.DUMB){
			adjustment = -15;
			baseLoyalty += adjustment;
			this._relationshipSummary += adjustment.ToString() + " Dislikes Dumb.\n";
		}

		//Efficieny Trait
		if(this._sourceKingdom.king.efficiency == EFFICIENCY.EFFICIENT && this._targetKingdom.king.efficiency == EFFICIENCY.EFFICIENT){
			adjustment = 15;
			baseLoyalty += adjustment;
			this._relationshipSummary += "+" + adjustment.ToString() + " Both Efficient.\n";
		}else if(this._sourceKingdom.king.efficiency == EFFICIENCY.EFFICIENT && this._targetKingdom.king.efficiency == EFFICIENCY.INEPT){
			adjustment = -15;
			baseLoyalty += adjustment;
			this._relationshipSummary += adjustment.ToString() + " Dislikes Inept.\n";
		}

		if(this._targetKingdomThreatLevel >= 0f && this._targetKingdomThreatLevel < 1f){
			adjustment = 25;
		}else if (this._targetKingdomThreatLevel >= 1f && this._targetKingdomThreatLevel < 26f){
			adjustment = 0;
		}else if (this._targetKingdomThreatLevel >= 26f && this._targetKingdomThreatLevel < 51f){
			adjustment = -25;
		}else if (this._targetKingdomThreatLevel >= 51f && this._targetKingdomThreatLevel < 100f){
			adjustment = -50;
		}else{
			adjustment = -100;
		}
		baseLoyalty += adjustment;

		if(adjustment >= 0){
			this._relationshipSummary += "+";
		}
		this._relationshipSummary += adjustment.ToString() + " Kingdom Threat.\n";

        this._like = 0;
        this.AdjustLikeness(baseLoyalty, gameEventTrigger, assassinationReasons, isDiscovery);
        if (UIManager.Instance.currentlyShowingKingdom != null && UIManager.Instance.currentlyShowingKingdom == _sourceKingdom) {
            UIManager.Instance.UpdateRelationships();
        }
    }

    /*
     * <summary>
     * Adjust likeness by an amount, relative to the current base likeness.
     * </summary>
     * <param name="adjustment"> Amount of likeness to add/subtract (Specify with +/-) </param>
     * <param name="gameEventTrigger"> Game Event that triggered change in relationship </param>
     * <param name="assassinationReasons"> Assassination Reason (Default to NONE) </param>
     * <param name="isDiscovery"> Is from discovery? (Default is false) </param>
     * */
    internal void AdjustLikeness(int adjustment, GameEvent gameEventTrigger, ASSASSINATION_TRIGGER_REASONS assassinationReasons = ASSASSINATION_TRIGGER_REASONS.NONE, bool isDiscovery = false) {
        RELATIONSHIP_STATUS previousStatus = _relationshipStatus;
        this._like += adjustment;
        this._like = Mathf.Clamp(this.totalLike, -100, 100);
        this.UpdateKingRelationshipStatus();

        if (!this._isInitial) {
            if (this.totalLike < 0) { //Relationship deteriorated
                _sourceKingdom.OnRelationshipDeteriorated(this, gameEventTrigger, isDiscovery, assassinationReasons);
                this.CheckForEmbargo(previousStatus, gameEventTrigger);
            } else { //Relationship improved
                _sourceKingdom.OnRelationshipImproved(this);
                this.CheckForDisembargo(previousStatus);
            }
        } else {
            this._isInitial = false;
        }
    }

    internal void ChangeRelationshipStatus(RELATIONSHIP_STATUS newStatus, GameEvent gameEventTrigger = null) {
        if (newStatus == RELATIONSHIP_STATUS.LOVE) {
            this._like = 81;
        } else if (newStatus == RELATIONSHIP_STATUS.AFFECTIONATE) {
            this._like = 41;
        } else if (newStatus == RELATIONSHIP_STATUS.LIKE) {
            this._like = 21;
        } else if (newStatus == RELATIONSHIP_STATUS.NEUTRAL) {
            this._like = 0;
        } else if (newStatus == RELATIONSHIP_STATUS.DISLIKE) {
            this._like = -21;
        } else if (newStatus == RELATIONSHIP_STATUS.HATE) {
            this._like = -41;
        } else if (newStatus == RELATIONSHIP_STATUS.SPITE) {
            this._like = -81;
        }
        UpdateKingRelationshipStatus();
        if (this.relationshipStatus == RELATIONSHIP_STATUS.HATE || this.relationshipStatus == RELATIONSHIP_STATUS.SPITE) {
            Embargo(gameEventTrigger);
        }
    }

    /*
     * <summary>
     * Set if sourceKingdom and targetKingdom are sharing borders
     * </summary>
     * */
    internal void SetBorderSharing(bool isSharingBorder) {
        _isSharingBorder = isSharingBorder;
        UpdateLikeness(null);
    }

    #region Trading
    /*
     * <summary>
     * This function checks if the targetKingdom is to be embargoed,
     * </summary>
     * */
    protected void CheckForEmbargo(RELATIONSHIP_STATUS previousRelationshipStatus, GameEvent gameEventTrigger) {
        if (previousRelationshipStatus != _relationshipStatus) { // Check if the relationship between the 2 kings changed in status
            //Check if the source kings relationship with king has changed to enemy or rival, if so, put king's kingdom in source king's embargo list
            if (_relationshipStatus == RELATIONSHIP_STATUS.HATE || _relationshipStatus == RELATIONSHIP_STATUS.SPITE) {
                Embargo(gameEventTrigger);
            }
        }
    }

    /*
     * <summary>
     * Put targetKing's kingdom in sourceKing's kingdom embargo list.
     * TODO: Add embargo reason from gameEventReasonForEmbargo when adding
     * kingdom to embargo list.
     * </summary>
     * */
    protected void Embargo(GameEvent gameEventReasonForEmbargo) {
        _sourceKingdom.AddKingdomToEmbargoList(_targetKingdom);
        //Debug.LogError(this.sourceKing.city.kingdom.name + " put " + this.king.city.kingdom.name + " in it's embargo list, beacuase of " + gameEventReasonForEmbargo.eventType.ToString());
    }

    /*
     * <summary>
     * This function checks if the targetKingdom is to be disembargoed,
     * requirement/s for disembargo:
     * - Relationship should go from ENEMY to COLD.
     * </summary>
     * */
    protected void CheckForDisembargo(RELATIONSHIP_STATUS previousRelationshipStatus) {
        if (previousRelationshipStatus != _relationshipStatus) { // Check if the relationship between the 2 kings changed in status
            //if the relationship changed from enemy to cold, disembargo the targetKing
            if (previousRelationshipStatus == RELATIONSHIP_STATUS.HATE && _relationshipStatus == RELATIONSHIP_STATUS.DISLIKE) {
                Disembargo();
            }
        }
    }

    /*
     * <summary>
     * Remove targetKingdom from sourceKingdom's embargo list
     * </summary>
     * */
    protected void Disembargo() {
        _sourceKingdom.RemoveKingdomFromEmbargoList(_targetKingdom);
        //Debug.LogError(_sourceKingdom.name + " removed " + _targetKingdom.name + " from it's embargo list!");
    }
    #endregion

    #region Event Modifiers
    /*
     * <summary>
     * Add an event modifier (A relationship modifier that came from a specific event that involved the sourceKingdom and targetKingdom)
     * </summary>
     * */
	internal void AddEventModifier(int modification, string summary, GameEvent gameEventTrigger = null, bool hasExpiration = true, ASSASSINATION_TRIGGER_REASONS assassinationReasons = ASSASSINATION_TRIGGER_REASONS.NONE, bool isDiscovery = false) {
        RELATIONSHIP_STATUS previousStatus = _relationshipStatus;
		bool hasAddedModifier = false;
		GameDate dateTimeToUse = new GameDate(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
		if(gameEventTrigger != null){
			if (gameEventTrigger.eventType == EVENT_TYPES.KINGDOM_WAR) {
				dateTimeToUse.AddYears(1); //Relationship modifiers from war will take 1 year to expire
			} else {
				dateTimeToUse.AddMonths(3); //Relationship modifiers from all other events will take 3 months to expire
			}
		}else{
			dateTimeToUse.AddMonths(3);
		}

		if(gameEventTrigger != null){
			if(this.eventBuffs.ContainsKey(gameEventTrigger.eventType)){
				if(this.eventBuffs[gameEventTrigger.eventType]){
					for (int i = 0; i < this._eventModifiers.Count; i++) {
						ExpirableModifier expMod = this._eventModifiers [i];
						if(expMod.modifierGameEvent.eventType == gameEventTrigger.eventType){
							expMod.SetModifier (expMod.modifier + modification);

							if(hasExpiration){
								SchedulingManager.Instance.RemoveSpecificEntry(expMod.dueDate.month, expMod.dueDate.day, expMod.dueDate.year, () => RemoveEventModifier(expMod));
								expMod.SetDueDate (dateTimeToUse);
								SchedulingManager.Instance.AddEntry(expMod.dueDate.month, expMod.dueDate.day, expMod.dueDate.year, () => RemoveEventModifier(expMod));
							}

							hasAddedModifier = true;
							break;
						}
					}
				}else{
					this.eventBuffs[gameEventTrigger.eventType] = true;
				}
			}
		}

		if(!hasAddedModifier){
			ExpirableModifier expMod = new ExpirableModifier(gameEventTrigger, summary, dateTimeToUse, modification);
			this._eventModifiers.Add(expMod);
			if(hasExpiration){
				SchedulingManager.Instance.AddEntry(expMod.dueDate.month, expMod.dueDate.day, expMod.dueDate.year, () => RemoveEventModifier(expMod));
			}
		}

        this._eventLikenessModifier += modification;
        if (modification < 0) {
            //Deteriorate Relationship
            _sourceKingdom.OnRelationshipDeteriorated(this, gameEventTrigger, isDiscovery, assassinationReasons);
            CheckForEmbargo(previousStatus, gameEventTrigger);
            if (gameEventTrigger != null && gameEventTrigger.eventType != EVENT_TYPES.KINGDOM_WAR) {
                 _sourceKingdom.WarTrigger(this, gameEventTrigger, _sourceKingdom.kingdomTypeData, gameEventTrigger.warTrigger);
            }
        } else {
            //Improve Relationship
            _sourceKingdom.OnRelationshipImproved(this);
            CheckForDisembargo(previousStatus);
        }
        UpdateKingRelationshipStatus();
    }

    /*
     * <summary>
     * Remove a specific event modifier
     * </summary>
     * */
    private void RemoveEventModifier(ExpirableModifier expMod) {
		if(!this._sourceKingdom.isDead && !this._targetKingdom.isDead){
			if(expMod.modifier < 0) {
				//if the modifier is negative, return the previously subtracted value
				_eventLikenessModifier += Mathf.Abs(expMod.modifier);
			} else {
				//if the modifier is positive, subtract the amount that was previously added
				_eventLikenessModifier -= expMod.modifier;
			}
			_eventModifiers.Remove(expMod);
			if(expMod.modifierGameEvent != null){
				if(this._eventBuffs.ContainsKey(expMod.modifierGameEvent.eventType)){
					this._eventBuffs [expMod.modifierGameEvent.eventType] = false;
				}
			}
			UpdateKingRelationshipStatus();
		}
        
    }
	internal void RemoveEventModifierBySummary(string summary){
		for (int i = 0; i < this._eventModifiers.Count; i++) {
			ExpirableModifier expMod = this._eventModifiers[i];
			if(expMod.summary == summary){
				if(expMod.modifier < 0) {
					//if the modifier is negative, return the previously subtracted value
					_eventLikenessModifier += Mathf.Abs(expMod.modifier);
				} else {
					//if the modifier is positive, subtract the amount that was previously added
					_eventLikenessModifier -= expMod.modifier;
				}
				if(expMod.modifierGameEvent != null){
					if(this._eventBuffs.ContainsKey(expMod.modifierGameEvent.eventType)){
						this._eventBuffs [expMod.modifierGameEvent.eventType] = false;
					}
				}
				UpdateKingRelationshipStatus();
				_eventModifiers.RemoveAt(i);
				break;
			}
		}
	}
    /*
     * <summary>
     * Reset all Event Modifiers for this relationship
     * </summary>
     * */
    internal void ResetEventModifiers() {
        this._eventLikenessModifier = 0;
        this._eventModifiers.Clear();
        UpdateKingRelationshipStatus();
    }
    #endregion

    #region War Functions
    /*
     * <summary>
     * Create an invasion plan against targetKingdom
     * NOTE: sourceKingdom and targetKingdom must already have a war event between them for this to work
     * </summary>
     * */
    internal void CreateInvasionPlan(GameEvent gameEventTrigger) {
        if (_invasionPlan == null) {
//            _invasionPlan = new InvasionPlan(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year,
//            _sourceKingdom.king, _sourceKingdom, _targetKingdom, gameEventTrigger, _war);
        } else {
            throw new Exception (_sourceKingdom.name + " already has an invasion plan towards " + _targetKingdom.name);
        }
    }

    internal void AssignRequestPeaceEvent(RequestPeace rp) {
        _requestPeace = rp;
    }

    /*
     * <summary>
     * Declare peace between sourceKingdom and targetKingdom
     * </summary>
     * */
    internal void DeclarePeace() {
        if (this._invasionPlan != null && this._invasionPlan.isActive) {
            this._invasionPlan.CancelEvent();
            this._invasionPlan = null;
        }

        if (this._requestPeace != null && this._requestPeace.isActive) {
//            this._war.GameEventWarWinner(this._targetKingdom);
            this._requestPeace.CancelEvent();
            this._requestPeace = null;
        }
        this._war = null;
        this._isAtWar = false;
    }

    internal void TriggerRequestPeace() {
        this._kingdomWarData.citiesLost += 1;
        if (this._sourceKingdom.cities.Count > 1 && _requestPeaceCooldown.month == 0) {
            int peaceValue = 20;
            int chance = UnityEngine.Random.Range(0, 100);
            if (this._war != null && chance < (peaceValue * this._kingdomWarData.citiesLost)) {
                //Request Peace
//                this._war.RequestPeace(this._sourceKingdom);
            }
        }
    }

    /*
     * <summary>
     * Queue sourceKingdom for request peace cooldown.
     * A kingdom can only request peace once every 3 months
     * </summary>
     * */
    internal void SetMoveOnPeriodAfterRequestPeaceRejection() {
		if(!this._sourceKingdom.isDead && !this._targetKingdom.isDead){
	        GameDate dueDate = new GameDate(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
	        dueDate.AddMonths(3);
	        _requestPeaceCooldown = dueDate;
	        SchedulingManager.Instance.AddEntry(dueDate.month, dueDate.day, dueDate.year, () => ResetRequestPeaceCooldown());
		}
    }

    /*
     * <summary>
     * Reset Request Peace Cooldown
     * </summary>
     * */
    private void ResetRequestPeaceCooldown() {
        _requestPeaceCooldown = new GameDate(0,0,0);
    }

    internal void AssignWarEvent(Wars war) {
        _war = war;
    }

    internal void SetWarStatus(bool warStatus, Warfare warfare) {
		if(this._isAtWar != warStatus){
			this._isAtWar = warStatus;
			SetWarfare(warfare);
		}
    }
	internal void SetRecentWar(bool state) {
		if(this._isRecentWar != state){
			this._isRecentWar = state;
		}
	}
	internal void SetDiscovery(bool state) {
		if(this._isDiscovered != state){
			this._isDiscovered = state;
		}
	}
//	internal void SetPreparingWar(bool state) {
//		this._isPreparingForWar = state;
//	}
    internal void AdjustExhaustion(int amount) {
        if (_isAtWar) {
            _kingdomWarData.AdjustExhaustion(amount);
        }
    }
    #endregion

    #region Relationship History
    internal void AddRelationshipHistory(History relHistory) {
        _relationshipHistory.Add(relHistory);
    }
    #endregion

	internal void ChangeAdjacency(bool state){
		AdjustAdjacency (state);
		KingdomRelationship kr = this._targetKingdom.GetRelationshipWithKingdom (this._sourceKingdom);
		kr.AdjustAdjacency (state);
	}
	private void AdjustAdjacency(bool state){
		if(this._isAdjacent != state){
			this._isAdjacent = state;
//            UpdateTargetInvasionValue();
//            UpdateTargetKingdomThreatLevel();
            if (state){
				this._sourceKingdom.AddAdjacentKingdom (this._targetKingdom);
			}else{
				this._sourceKingdom.RemoveAdjacentKingdom (this._targetKingdom);
			}
		}
	}

	internal void ChangeWarStatus(bool state, Warfare warfare){
		SetWarStatus(state, warfare);
		if(state){
//			SetPreparingWar (false);
			this._sourceKingdom.AdjustWarmongerValue (25);
		}
		KingdomRelationship kr = this._targetKingdom.GetRelationshipWithKingdom (this._sourceKingdom);
		kr.SetWarStatus(state, warfare);
	}
	internal void ChangeBattle(Battle battle){
		SetBattle(battle);
		KingdomRelationship kr = this._targetKingdom.GetRelationshipWithKingdom (this._sourceKingdom);
		kr.SetBattle(battle);
	}
	internal void ChangeDiscovery(bool state){
		SetDiscovery(state);
		KingdomRelationship kr = this._targetKingdom.GetRelationshipWithKingdom (this._sourceKingdom);
		kr.SetDiscovery(state);
	}
	internal void ChangeRecentWar(bool state){
		if(!this._sourceKingdom.isDead && !this._targetKingdom.isDead){
			SetRecentWar (state);
			KingdomRelationship kr = this._targetKingdom.GetRelationshipWithKingdom (this._sourceKingdom);
			kr.SetRecentWar(state);
		}
	}

	private void SetRaceThreatModifier(){
		if(this._sourceKingdom.race != this._targetKingdom.race){
			this._racePercentageModifier = 1.15f;
		}else{
			this._racePercentageModifier = 1f;
		}
	}
	internal void UpdateThreatLevelAndInvasionValue(){
		KingdomRelationship rk = this._targetKingdom.GetRelationshipWithKingdom (this._sourceKingdom);
		UpdateTheoreticalAttackAndDefense ();
		rk.UpdateTheoreticalAttackAndDefense ();

		this._usedSourceEffectivePower = this._effectivePower;
		this._usedSourceEffectiveDef = this._effectiveDef;
		this._usedTargetEffectivePower = rk._effectivePower;
		this._usedTargetEffectiveDef = rk._effectiveDef;

		UpdateTargetKingdomThreatLevel ();
		UpdateTargetInvasionValue ();
        UpdateLikeness(null);
    }
	internal void UpdateTargetKingdomThreatLevel(){
		float threatLevel = 0f;
		if(this._usedTargetEffectivePower > this._usedSourceEffectiveDef){
			//+1 for every percentage point of effective power above my effective defense (max 100)
			threatLevel = (((float)this._usedTargetEffectivePower / (float)this._usedSourceEffectiveDef) * 100f) - 100f;
			threatLevel = Mathf.Clamp (threatLevel, 0f, 100f);

			//if different race: +15%
			threatLevel *= this._racePercentageModifier;

			if(AreAllies()){
				threatLevel -= (threatLevel * 0.5f);
			}else{
				//if currently at war with someone else: -50%
				if(this._targetKingdom.HasWar()){
					if(this._sourceKingdom.alliancePool != null){
						bool atWarWithMyAlliance = false;
						for (int i = 0; i < this._sourceKingdom.alliancePool.kingdomsInvolved.Count; i++) {
							Kingdom allyKingdom = this._sourceKingdom.alliancePool.kingdomsInvolved [i];
							if(this._sourceKingdom.id != allyKingdom.id){
								KingdomRelationship kr = allyKingdom.GetRelationshipWithKingdom (this._targetKingdom);
								if(kr.isAtWar){
									atWarWithMyAlliance = true;
									break;
								}
							}
						}
						if(!atWarWithMyAlliance){
							threatLevel -= (threatLevel * 0.5f);
						}
					}else{
						threatLevel -= (threatLevel * 0.5f);
					}
				}
			}

			//if not at war but militarizing
			if(!this._targetKingdom.HasWar() && this._targetKingdom.isMilitarize){
				threatLevel *= 1.25f;
			}

			//recent war with us
			if(this._isRecentWar){
				threatLevel -= (threatLevel * 0.5f);
			}

			//warmongering
			if(this._targetKingdom.warmongerValue == 0){
				threatLevel -= (threatLevel * 0.5f);
			}else if(this._targetKingdom.warmongerValue > 0 && this._targetKingdom.warmongerValue < 25){
				threatLevel -= (threatLevel * 0.25f);
			}else if(this._targetKingdom.warmongerValue >= 25 && this._targetKingdom.warmongerValue < 50){
				threatLevel *= 1.25f;
			}else if(this._targetKingdom.warmongerValue >= 50 && this._targetKingdom.warmongerValue < 75){
				threatLevel *= 1.5f;
			}else{
				threatLevel *= 2f;
			}

			//adjacency
			if(!this._isAdjacent){
				threatLevel -= (threatLevel * 0.5f);
			}

//			//cannot expand due to lack of stability
//			if(this._targetKingdom.stability <= 0){
//				threatLevel -= (threatLevel * 0.5f);
//			}

//			//still adjacent to unoccupied region / can still expand
//			HexTile hexTile = CityGenerator.Instance.GetExpandableTileForKingdom (this._targetKingdom);
//			if(hexTile != null){
//				threatLevel -= (threatLevel * 0.25f);
//			}
		}

		this._targetKingdomThreatLevel = threatLevel;
		if(this._targetKingdomThreatLevel < 0f){
			this._targetKingdomThreatLevel = 0f;
		}
		
	}

	internal void UpdateTargetInvasionValue(){
		float invasionValue = 0;
		if(this._usedSourceEffectivePower > this._usedTargetEffectiveDef){
			if (this.isAdjacent) {
				//+1 for every percentage point of my effective power above his effective defense (no max cap)
				invasionValue = (((float)this._usedSourceEffectivePower / (float)this._usedTargetEffectiveDef) * 100f) - 100f;
				if (invasionValue < 0) {
					invasionValue = 0;
				}

				//+-% for every point of Opinion towards target
				int likeFactor = (int)((float)(this.totalLike) / 2f);
				float likePercent = (float)likeFactor / 100f;
				invasionValue -= (likePercent * invasionValue);

				//if allies
				if(AreAllies()){
					invasionValue -= (invasionValue * 0.5f);
				}

				//if target is currently at war with someone else: +25%
				if(this._targetKingdom.HasWar(this._sourceKingdom)){
					invasionValue *= 1.25f;
				}

				//recent war with us
				if(this._isRecentWar){
					invasionValue -= (invasionValue * 0.5f);
				}

				if (this._targetKingdom.kingdomSize == KINGDOM_SIZE.SMALL) {
					if (this._sourceKingdom.kingdomSize == KINGDOM_SIZE.MEDIUM) {
						invasionValue -= (invasionValue * 0.25f);
					} else if (this._sourceKingdom.kingdomSize == KINGDOM_SIZE.LARGE) {
						invasionValue -= (invasionValue * 0.5f);
					}
				} else if (this._targetKingdom.kingdomSize == KINGDOM_SIZE.MEDIUM) {
					if (this._sourceKingdom.kingdomSize == KINGDOM_SIZE.LARGE) {
						invasionValue -= (invasionValue * 0.25f);
					}
				}
			}
		}

		this._targetKingdomInvasionValue = invasionValue;
		if(this._targetKingdomInvasionValue < 0){
			this._targetKingdomInvasionValue = 0;
		}
		if (UIManager.Instance.currentlyShowingKingdom != null && UIManager.Instance.currentlyShowingKingdom == _sourceKingdom) {
			UIManager.Instance.UpdateRelationships();
		}
	}
	internal void UpdateTheoreticalAttackAndDefense(){
		int theoreticalAttack = GetTheoreticalAttack ();
		int theoreticalDefense = GetTheoreticalDefense ();
		int posAllianceAttack = GetAdjacentPosAllianceWeapons ();
//		int posAllianceDefense = GetAdjacentPosAllianceArmors ();
//		int usedPosAllianceAttack = (int)((float)posAllianceAttack / 2f);

		this._effectivePower = theoreticalAttack + posAllianceAttack;
		this._effectiveDef = theoreticalDefense + posAllianceAttack;
	}

	private int GetTheoreticalAttack(){
		int soldiers = this._sourceKingdom.soldiers;
		int posAllianceAttack = GetUnadjacentPosAllianceWeapons ();
		return (2 * soldiers * (this._sourceKingdom.baseWeapons + posAllianceAttack)) / (soldiers + (this._sourceKingdom.baseWeapons + posAllianceAttack));
	}
	private int GetTheoreticalDefense(){
		int soldiers = this._sourceKingdom.soldiers;
		int posAllianceDefense = GetUnadjacentPosAllianceArmors ();
		return (int)(2 * soldiers * (this._sourceKingdom.baseArmor + posAllianceDefense)) / (soldiers + (this._sourceKingdom.baseArmor + posAllianceDefense));
	}
	private int GetUnadjacentPosAllianceWeapons(){
		int posAlliancePower = 0;
		if(this._sourceKingdom.alliancePool != null){
			for (int i = 0; i < this._sourceKingdom.alliancePool.kingdomsInvolved.Count; i++) {
				Kingdom kingdomInAlliance = this._sourceKingdom.alliancePool.kingdomsInvolved[i];
				if(this._sourceKingdom.id != kingdomInAlliance.id && this._targetKingdom.id != kingdomInAlliance.id){
					KingdomRelationship relationship = kingdomInAlliance.GetRelationshipWithKingdom(this._sourceKingdom);
					KingdomRelationship relationshipToEnemy = kingdomInAlliance.GetRelationshipWithKingdom(this._targetKingdom);
					if(relationship.totalLike >= 0 && !relationshipToEnemy.isAdjacent){
						float weapons = (float)kingdomInAlliance.baseWeapons * 0.1f;
						posAlliancePower += (int)(weapons * GetOpinionPercentage(relationship.totalLike));
					}
				}
			}
		}
		return posAlliancePower;
	}
	private int GetAdjacentPosAllianceWeapons(){
		int posAlliancePower = 0;
		if(this._sourceKingdom.alliancePool != null){
			for (int i = 0; i < this._sourceKingdom.alliancePool.kingdomsInvolved.Count; i++) {
				Kingdom kingdomInAlliance = this._sourceKingdom.alliancePool.kingdomsInvolved[i];
				if(this._sourceKingdom.id != kingdomInAlliance.id && this._targetKingdom.id != kingdomInAlliance.id){
					KingdomRelationship relationship = kingdomInAlliance.GetRelationshipWithKingdom(this._sourceKingdom);
					KingdomRelationship relationshipToEnemy = kingdomInAlliance.GetRelationshipWithKingdom(this._targetKingdom);
					if(relationship.totalLike >= 0 && relationshipToEnemy.isAdjacent){
						posAlliancePower += (int)((float)kingdomInAlliance.effectiveAttack * GetOpinionPercentage(relationship.totalLike));
					}
				}
			}
		}
		return posAlliancePower;
	}
	private int GetUnadjacentPosAllianceArmors(){
		int posAllianceDefense = 0;
		if(this._sourceKingdom.alliancePool != null){
			for (int i = 0; i < this._sourceKingdom.alliancePool.kingdomsInvolved.Count; i++) {
				Kingdom kingdomInAlliance = this._sourceKingdom.alliancePool.kingdomsInvolved[i];
				if(this._sourceKingdom.id != kingdomInAlliance.id && this._targetKingdom.id != kingdomInAlliance.id){
					KingdomRelationship relationship = kingdomInAlliance.GetRelationshipWithKingdom(this._sourceKingdom);
					KingdomRelationship relationshipToEnemy = kingdomInAlliance.GetRelationshipWithKingdom(this._targetKingdom);
					if(relationship.totalLike >= 0 && !relationshipToEnemy.isAdjacent){
						float armors = (float)kingdomInAlliance.baseArmor * 0.1f;
						posAllianceDefense += (int)(armors * GetOpinionPercentage(relationship.totalLike));
					}
				}
			}
		}
		return posAllianceDefense;
	}
	private int GetAdjacentPosAllianceArmors(){
		int posAllianceDefense = 0;
		if(this._sourceKingdom.alliancePool != null){
			for (int i = 0; i < this._sourceKingdom.alliancePool.kingdomsInvolved.Count; i++) {
				Kingdom kingdomInAlliance = this._sourceKingdom.alliancePool.kingdomsInvolved[i];
				if(this._sourceKingdom.id != kingdomInAlliance.id && this._targetKingdom.id != kingdomInAlliance.id){
					KingdomRelationship relationship = kingdomInAlliance.GetRelationshipWithKingdom(this._sourceKingdom);
					KingdomRelationship relationshipToEnemy = kingdomInAlliance.GetRelationshipWithKingdom(this._targetKingdom);
					if(relationship.totalLike >= 0 && relationshipToEnemy.isAdjacent){
						posAllianceDefense += (int)((float)kingdomInAlliance.effectiveDefense * GetOpinionPercentage(relationship.totalLike));
					}
				}
			}
		}
		return posAllianceDefense;
	}
	private float GetOpinionPercentage(int opinion){
		if(opinion >= 0 && opinion < 35){
			return 0.25f;
		}else if(opinion >= 35 && opinion < 50){
			return 0.5f;
		}else{
			return 1f;
		}
	}
	internal bool AreAllies(){
		if(this._sourceKingdom.alliancePool == null || this._targetKingdom.alliancePool == null){
			return false;
		}else{
			if(this._sourceKingdom.alliancePool.id != this._targetKingdom.alliancePool.id){
				return false;
			}
		}
		return true;
	}
	internal void SetWarfare(Warfare warfare){
		this._warfare = warfare;
	}
	internal void SetBattle(Battle battle){
		this._battle = battle;
	}
}
