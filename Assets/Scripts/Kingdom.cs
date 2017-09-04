﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

[System.Serializable]
public class Kingdom{
    [Space(10)]
    [Header("General Info")]
    public int id;
	public string name;
	public RACE race;
    public int age;
    private int _prestige;
    private int _disloyaltyFromPrestige; //Loyalty subtracted from governors due to too many cities and lack of prestige
    private int foundationYear;
    private int foundationMonth;
    private int foundationDay;

	private KingdomTypeData _kingdomTypeData;
	private Kingdom _sourceKingdom;
	private Kingdom _mainThreat;

    //Resources
    private Dictionary<RESOURCE, int> _availableResources; //only includes resources that the kingdom has bought via tile purchasing
    internal BASE_RESOURCE_TYPE basicResource;

    //Trading
    private Dictionary<Kingdom, EMBARGO_REASON> _embargoList;

    private int _unrest;
    private List<City> _cities;
	private List<Camp> camps;
	internal City capitalCity;
	internal Citizen king;
	internal List<Citizen> successionLine;

	internal List<Rebellion> rebellions;

	internal Dictionary<Kingdom, KingdomRelationship> relationships;

	internal Color kingdomColor;
	internal List<History> kingdomHistory;

	[NonSerialized] private List<Kingdom> _discoveredKingdoms;

	//Plague
	internal Plague plague;

	//Boon of Power
	private List<BoonOfPower> _boonOfPowers;
	private List<BoonOfPower> _activatedBoonOfPowers;

	//Daily Cumulative
	private EventRate[] _dailyCumulativeEventRate;
	
	//Tech
	private int _techLevel;
	private int _techCapacity;
	private int _techCounter;

	//The First and The Keystone
	internal FirstAndKeystoneOwnership firstAndKeystoneOwnership;
    private bool _isGrowthEnabled;

	//Serum of Alacrity
	private int _serumsOfAlacrity;

    //FogOfWar
    private FOG_OF_WAR_STATE[,] _fogOfWar;
	private Dictionary<FOG_OF_WAR_STATE, HashSet<HexTile>> _fogOfWarDict;

	//Crimes
	private CrimeData _crimeData;
	private CrimeDate _crimeDate;

	//Events of Kingdom
	private List<GameEvent> _activeEvents;
	private List<GameEvent> _doneEvents;

    //Expansion
    private float expansionChance = 1f;

    protected Dictionary<CHARACTER_VALUE, int> _dictCharacterValues;
    protected Dictionary<CHARACTER_VALUE, int> _importantCharacterValues;

    protected const int INCREASE_CITY_HP_CHANCE = 5;
	protected const int INCREASE_CITY_HP_AMOUNT = 20;
    protected const int GOLD_GAINED_FROM_TRADE = 10;
    protected const int UNREST_DECREASE_PER_MONTH = -5;
    protected const int UNREST_INCREASE_CONQUER = 5;
    protected const int UNREST_INCREASE_EMBARGO = 5;

	private bool _isDead;
	private bool _hasBioWeapon;
	private bool _isLockedDown;
	private bool _isTechProducing;
	private bool _isMilitarize;

	private int borderConflictLoyaltyExpiration;
	private float _techProductionPercentage;
	private float _productionGrowthPercentage;

	private bool _hasUpheldHiddenHistoryBook;

	private bool _hasSecession;
	private bool _hasRiot;

	private List<Citizen> orderedMaleRoyalties;
	private List<Citizen> orderedFemaleRoyalties;
	private List<Citizen> orderedBrotherRoyalties;
	private List<Citizen> orderedSisterRoyalties;

	#region getters/setters
	public KINGDOM_TYPE kingdomType {
		get { 
			if (this._kingdomTypeData == null) {
				return KINGDOM_TYPE.NONE;
			}
			return this._kingdomTypeData.kingdomType; 
		}
	}
	public KingdomTypeData kingdomTypeData {
		get { return this._kingdomTypeData; }
	}
	public Kingdom sourceKingdom {
		get { return this._sourceKingdom; }
	}
	public Kingdom mainThreat {
		get { return this._mainThreat; }
	}
    public int prestige {
        get { return _prestige; }
    }
    public int disloyaltyFromPrestige {
        get { return _disloyaltyFromPrestige; }
    }
    public int cityCap {
        get { return Mathf.FloorToInt(_prestige / 100); }
    }
	public Dictionary<RESOURCE, int> availableResources{
		get{ return this._availableResources; }
	}
    public Dictionary<Kingdom, EMBARGO_REASON> embargoList {
        get { return this._embargoList;  }
    }
	public bool isDead{
		get{ return this._isDead; }
	}
	public List<City> cities{
		get{ return this._cities; }
	}
//	public List<Camp> camps{
//		get{ return this._camps; }
//	}
    public int unrest {
        get { return this._unrest; }
		set { this._unrest = value;}
    }
    public int basicResourceCount {
        get { return this._availableResources.Where(x => Utilities.GetBaseResourceType(x.Key) == this.basicResource).Sum(x => x.Value); }
    }
    public List<Kingdom> discoveredKingdoms {
        get { return this._discoveredKingdoms; }
    }
	public int techLevel{
		get{return this._techLevel + (3 * this._activatedBoonOfPowers.Count);}
	}
	public int techCapacity{
		get{return this._techCapacity;}
	}
	public int techCounter{
		get{return this._techCounter;}
	}
    public float expansionRate {
        get { return this.expansionChance; }
    }
    public Dictionary<CHARACTER_VALUE, int> dictCharacterValues {
        get { return this._dictCharacterValues; }
    }
    public Dictionary<CHARACTER_VALUE, int> importantCharacterValues {
        get { return this._importantCharacterValues; }
    }
    public bool hasBioWeapon {
		get { return this._hasBioWeapon; }
	}
	public EventRate[] dailyCumulativeEventRate {
		get { return this._dailyCumulativeEventRate; }
	}
//    public List<City> plaguedCities {
//        get { return this.cities.Where(x => x.plague != null).ToList(); }
//    }
    public bool isGrowthEnabled {
        get { return _isGrowthEnabled; }
    }
	public List<City> nonRebellingCities {
		get { return this.cities.Where(x => x.rebellion == null).ToList(); }
	}
	public int serumsOfAlacrity {
		get { return this._serumsOfAlacrity; }
	}
    public FOG_OF_WAR_STATE[,] fogOfWar {
        get { return _fogOfWar; }
    }
    public Dictionary<FOG_OF_WAR_STATE, HashSet<HexTile>> fogOfWarDict {
        get { return _fogOfWarDict; }
    }
//	public CombatStats combatStats {
//		get { return this._combatStats; }
//	}
//	public int waves{
//		get { return this._combatStats.waves - GetNumberOfWars();}
//	}
    public bool isLockedDown{
		get { return this._isLockedDown;}
	}
	public bool isTechProducing{
		get { return this._isTechProducing;}
	}
	public bool isMilitarize{
		get { return this._isMilitarize;}
	}
	public float productionGrowthPercentage {
		get { return this._productionGrowthPercentage; }
	}
	public bool hasUpheldHiddenHistoryBook{
		get { return this._hasUpheldHiddenHistoryBook;}
	}
	public bool hasSecession{
		get { return this._hasSecession;}
	}
	public bool hasRiot{
		get { return this._hasRiot;}
	}
	public List<GameEvent> activeEvents{
		get { return this._activeEvents;}
	}
	public List<GameEvent> doneEvents{
		get { return this._doneEvents;}
	}
    #endregion

    // Kingdom constructor paramters
    //	race - the race of this kingdom
    //	cities - the cities that this kingdom will initially own
    //	sourceKingdom (optional) - the kingdom from which this new kingdom came from
    public Kingdom(RACE race, List<HexTile> cities, Kingdom sourceKingdom = null) {
		this.id = Utilities.SetID(this);
		this.race = race;
        this._prestige = 0;
        this._disloyaltyFromPrestige = 0;
		this.name = RandomNameGenerator.Instance.GenerateKingdomName(this.race);
		this.king = null;
		this._mainThreat = null;
        this.successionLine = new List<Citizen>();
		this._cities = new List<City> ();
		this.camps = new List<Camp> ();
		this.kingdomHistory = new List<History>();
		this.kingdomColor = Utilities.GetColorForKingdom();
		this._availableResources = new Dictionary<RESOURCE, int> ();
		this.relationships = new Dictionary<Kingdom, KingdomRelationship>();
		this._isDead = false;
		this._isLockedDown = false;
		this._isMilitarize = false;
		this._hasUpheldHiddenHistoryBook = false;
        this._embargoList = new Dictionary<Kingdom, EMBARGO_REASON>();
        this._unrest = 0;
		this._sourceKingdom = sourceKingdom;
		this.borderConflictLoyaltyExpiration = 0;
		this.rebellions = new List<Rebellion> ();
		this._discoveredKingdoms = new List<Kingdom>();
		this._techLevel = 1;
		this._techCounter = 0;
		this._hasBioWeapon = false;
		this._boonOfPowers = new List<BoonOfPower> ();
		this._activatedBoonOfPowers = new List<BoonOfPower> ();
		this.plague = null;
        this.age = 0;
        this.foundationYear = GameManager.Instance.year;
        this.foundationDay = GameManager.Instance.days;
        this.foundationMonth = GameManager.Instance.month;
        this._dictCharacterValues = new Dictionary<CHARACTER_VALUE, int>();
        this._importantCharacterValues = new Dictionary<CHARACTER_VALUE, int>();
        this._fogOfWar = new FOG_OF_WAR_STATE[(int)GridMap.Instance.width, (int)GridMap.Instance.height];
		this._fogOfWarDict = new Dictionary<FOG_OF_WAR_STATE, HashSet<HexTile>>();
		this._fogOfWarDict.Add(FOG_OF_WAR_STATE.HIDDEN, new HashSet<HexTile>(GridMap.Instance.listHexes.Select(x => x.GetComponent<HexTile>())));
		this._fogOfWarDict.Add(FOG_OF_WAR_STATE.SEEN, new HashSet<HexTile>());
		this._fogOfWarDict.Add(FOG_OF_WAR_STATE.VISIBLE, new HashSet<HexTile>());
		this._activeEvents = new List<GameEvent> ();
		this._doneEvents = new List<GameEvent> ();
		this.orderedMaleRoyalties = new List<Citizen> ();
		this.orderedFemaleRoyalties = new List<Citizen> ();
		this.orderedBrotherRoyalties = new List<Citizen> ();
		this.orderedSisterRoyalties = new List<Citizen> ();

        AdjustPrestige(200);
        SetGrowthState(true);
        this.GenerateKingdomCharacterValues();
        this.SetLockDown(false);
		this.SetTechProduction(true);
		this.SetTechProductionPercentage(1);
		this.SetProductionGrowthPercentage(1);
		this.UpdateTechCapacity ();
		this.SetSecession (false);
		this.SetRiot (false);
//		this.NewRandomCrimeDate (true);
		// Determine what type of Kingdom this will be upon initialization.
		this._kingdomTypeData = null;
		this.UpdateKingdomTypeData();

        this.basicResource = Utilities.GetBasicResourceForRace(race);

        if (cities.Count > 0) {
            for (int i = 0; i < cities.Count; i++) {
                this.CreateNewCityOnTileForKingdom(cities[i]);
            }
        }

		//this.CreateInitialRelationships();
		Messenger.AddListener<Kingdom>("OnNewKingdomCreated", CreateNewRelationshipWithKingdom);
		Messenger.AddListener("OnDayEnd", KingdomTickActions);
        Messenger.AddListener<Kingdom>("OnKingdomDied", OtherKingdomDiedActions);

		SchedulingManager.Instance.AddEntry (GameManager.Instance.month, GameManager.Instance.days, (GameManager.Instance.year + 1), () => AttemptToAge());
		SchedulingManager.Instance.AddEntry (GameManager.Instance.month, GameManager.daysInMonth[GameManager.Instance.month], GameManager.Instance.year, () => DecreaseUnrestEveryMonth());
        SchedulingManager.Instance.AddEntry (GameManager.Instance.month, GameManager.daysInMonth[GameManager.Instance.month], GameManager.Instance.year, () => MonthlyPrestigeActions());
        SchedulingManager.Instance.AddEntry (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, () => AdaptToKingValues());

//		ScheduleEvents ();

		this.kingdomHistory.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "This kingdom was born.", HISTORY_IDENTIFIER.NONE));
	}
	// Updates this kingdom's type and horoscope
	public void UpdateKingdomTypeData() {
		// Update Kingdom Type whenever the kingdom expands to a new city
		KingdomTypeData prevKingdomTypeData = this._kingdomTypeData;
		this._kingdomTypeData = StoryTellingManager.Instance.InitializeKingdomType (this);
		if(this.kingdomTypeData.dailyCumulativeEventRate != null){
			this._dailyCumulativeEventRate = this._kingdomTypeData.dailyCumulativeEventRate;
		}
		// If the Kingdom Type Data changed
		if (this._kingdomTypeData != prevKingdomTypeData) {			
            // Update expansion chance
            this.UpdateExpansionRate();

			//Update Character Values of King and Governors
			this.UpdateCharacterValuesOfKingsAndGovernors();

			//Update Relationship Opinion
			UpdateAllRelationshipsLikeness();
        }

//		UpdateCombatStats();
    }

    #region Kingdom Death
    // Function to call if you want to determine whether the Kingdom is still alive or dead
    // At the moment, a Kingdom is considered dead if it doesnt have any cities.
    public bool isAlive() {
        if (this.nonRebellingCities.Count > 0) {
            return true;
        }
        return false;
    }
    /*
     * <summary>
	 * Every time a city of this kingdom dies, check if
	 * this kingdom has no more cities, if so, the kingdom is
	 * considered dead. Remove all ties from other kingdoms.
     * </summary>
	 * */
    internal void CheckIfKingdomIsDead() {
        if (this.cities.Count <= 0) {
            //Kingdom is dead
            this.DestroyKingdom();
        }
    }
    /*
     * <summary>
	 * Kill this kingdom. This removes all ties with other kingdoms.
	 * Only call this when a kingdom has no more cities.
     * </summary>
	 * */
    internal void DestroyKingdom() {
        _isDead = true;
        CancelEventKingdomIsInvolvedIn(EVENT_TYPES.ALL);
        ResolveWars();
        Messenger.RemoveListener<Kingdom>("OnNewKingdomCreated", CreateNewRelationshipWithKingdom);
        Messenger.RemoveListener("OnDayEnd", KingdomTickActions);
        Messenger.RemoveListener<Kingdom>("OnKingdomDied", OtherKingdomDiedActions);

        Messenger.Broadcast<Kingdom>("OnKingdomDied", this);

        this.DeleteRelationships();
        KingdomManager.Instance.allKingdoms.Remove(this);

        UIManager.Instance.CheckIfShowingKingdomIsAlive(this);

        Debug.Log(this.id + " - Kingdom: " + this.name + " has died!");
        Debug.Log("Stack Trace: " + System.Environment.StackTrace);
    }
    private void CancelEventKingdomIsInvolvedIn(EVENT_TYPES eventType) {
		List<GameEvent> eventsToCancel = new List<GameEvent>(this.activeEvents);
		if (eventType == EVENT_TYPES.ALL) {
			for (int i = 0; i < eventsToCancel.Count; i++) {
				eventsToCancel [i].CancelEvent ();
			}
		}else{
			for (int i = 0; i < eventsToCancel.Count; i++) {
				if (eventsToCancel[i].eventType == eventType) {
					eventsToCancel[i].CancelEvent ();
				}
			}
		}
    }
    private void ResolveWars() {
//        List<War> warsToResolve = relationships.Values.Where(x => x.war != null).Select(x => x.war).ToList();
//        for (int i = 0; i < warsToResolve.Count; i++) {
//            warsToResolve[i].DoneEvent();
//        }
		foreach (KingdomRelationship rel in relationships.Values) {
			if (rel.war != null) {
				rel.war.DoneEvent ();
			}
		}
    }
    protected void OtherKingdomDiedActions(Kingdom kingdomThatDied) {
        if (kingdomThatDied.id != this.id) {
            RemoveRelationshipWithKingdom(kingdomThatDied);
            RemoveKingdomFromDiscoveredKingdoms(kingdomThatDied);
            RemoveKingdomFromEmbargoList(kingdomThatDied);
        }
    }
    #endregion

    #region Relationship Functions
    internal void CreateInitialRelationships() {
        for (int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++) {
            if (KingdomManager.Instance.allKingdoms[i].id != this.id) {
                Kingdom currOtherKingdom = KingdomManager.Instance.allKingdoms[i];
                //CreateNewRelationshipWithKingdom(currOtherKingdom);
                this.relationships.Add(currOtherKingdom, new KingdomRelationship(this, currOtherKingdom));
            }
        }
    }
    /*
     * <summary>
	 * Used to create a new KingdomRelationship with the
	 * newly created kingdom. Function is listening to onCreateNewKingdom Event.
     * </summary>
	 * */
    protected void CreateNewRelationshipWithKingdom(Kingdom createdKingdom) {
        if (createdKingdom.id == this.id) {
            return;
        }

        if (relationships.ContainsKey(createdKingdom)) {
            return;
        }

        //Debug.Log(this.name + " created new relationship with " + createdKingdom.name);

        KingdomRelationship newRel = new KingdomRelationship(this, createdKingdom);
        relationships.Add(createdKingdom, newRel);
        newRel.UpdateLikeness(null);
    }
    /* 
     * <summary>
     * Clear all relationships from self.
     * </summary>
     * */
    protected void DeleteRelationships() {
        this.relationships.Clear();
    }
    /*
     * <summary>
     * Remove a kingdom from this kingdom's list of relationships
     * </summary>
     * */
    protected void RemoveRelationshipWithKingdom(Kingdom kingdomThatDied) {
        relationships.Remove(kingdomThatDied);
    }
    /*
     * <summary>
     * Actions to perform when a relationship that this kingdom owns, deteriorates
     * </summary>
     * */
    internal void OnRelationshipDeteriorated(KingdomRelationship relationship, GameEvent gameEventTrigger, bool isDiscovery, ASSASSINATION_TRIGGER_REASONS assassinationReasons) {
//        if (assassinationReasons != ASSASSINATION_TRIGGER_REASONS.NONE) {
//            TriggerAssassination(relationship, gameEventTrigger, assassinationReasons);
//        }
    }
    /*
    * <summary>
    * Actions to perform when a relationship that this kingdom owns, improves
    * </summary>
    * */
    internal void OnRelationshipImproved(KingdomRelationship relationship) {
        //Improvement of Relationship
        int chance = UnityEngine.Random.Range(0, 100);
        int value = 0;
        if (relationship.relationshipStatus == RELATIONSHIP_STATUS.RIVAL) {
            value = 5;
        } else if (relationship.relationshipStatus == RELATIONSHIP_STATUS.ENEMY) {
            value = 15;
        } else {
            value = 25;
        }
        if (chance < value) {
            CancelInvasionPlan(relationship);
        }
    }
    /*
     * <summary>
     * Get all kingdoms that this kingdom has a specific relationshipStatus
     * </summary>
     * <param name="relationshipStatuses">Relationship Statuses to be checked</param>
     * <param name="discoveredOnly">Should only return discovered kingdoms?</param>
     * */
	internal List<Kingdom> GetKingdomsByRelationship(RELATIONSHIP_STATUS[] relationshipStatuses, Kingdom exception = null, bool discoveredOnly = true) {
        List<Kingdom> kingdomsWithRelationshipStatus = new List<Kingdom>();
		if(discoveredOnly){
			foreach (Kingdom currKingdom in relationships.Keys) {
				if(exception != null && exception.id == currKingdom.id){
					continue;
				}
				//        for (int i = 0; i < relationships.Count; i++) {
				//            Kingdom currKingdom = relationships.Keys.ElementAt(i);
				if (!discoveredKingdoms.Contains(currKingdom)) {
					continue;
				}

				RELATIONSHIP_STATUS currStatus = relationships[currKingdom].relationshipStatus;
				if (relationshipStatuses.Contains(currStatus)) {
					kingdomsWithRelationshipStatus.Add(currKingdom);
				}
			}
		}else{
			foreach (Kingdom currKingdom in relationships.Keys) {
				//        for (int i = 0; i < relationships.Count; i++) {
				//            Kingdom currKingdom = relationships.Keys.ElementAt(i);
				if(exception != null && exception.id == currKingdom.id){
					continue;
				}
				RELATIONSHIP_STATUS currStatus = relationships[currKingdom].relationshipStatus;
				if (relationshipStatuses.Contains(currStatus)) {
					kingdomsWithRelationshipStatus.Add(currKingdom);
				}
			}
		}
        return kingdomsWithRelationshipStatus;
    }
    internal KingdomRelationship GetRelationshipWithKingdom(Kingdom kingdom) {
        if (relationships.ContainsKey(kingdom)) {
            return relationships[kingdom];
        } else {
            throw new Exception(this.name + " does not have relationship with " + kingdom.name);
        }
        
    }
    internal void UpdateMutualRelationships() {
		foreach (KingdomRelationship currRel in relationships.Values) {
//        for (int i = 0; i < relationships.Count; i++) {
//            KingdomRelationship currRel = relationships.Values.ElementAt(i);
            Kingdom targetKingdom = currRel.targetKingdom;
            KingdomRelationship targetKingdomRel = targetKingdom.GetRelationshipWithKingdom(this);

            if (targetKingdomRel == null || currRel == null) {
                return;
            }

            currRel.ResetMutualRelationshipModifier();
            targetKingdomRel.ResetMutualRelationshipModifier();

			List<Kingdom> sourceKingRelationships = GetKingdomsByRelationship (new
           [] { RELATIONSHIP_STATUS.ENEMY, RELATIONSHIP_STATUS.RIVAL,
				RELATIONSHIP_STATUS.FRIEND, RELATIONSHIP_STATUS.ALLY
			}, targetKingdom);

			List<Kingdom> targetKingRelationships = targetKingdom.GetKingdomsByRelationship (new
                [] { RELATIONSHIP_STATUS.ENEMY, RELATIONSHIP_STATUS.RIVAL,
				RELATIONSHIP_STATUS.FRIEND, RELATIONSHIP_STATUS.ALLY
			}, this);

//            List<Kingdom> kingdomsInCommon = sourceKingRelationships.Intersect(targetKingRelationships).ToList();
			foreach (var currKingdom in sourceKingRelationships.Intersect(targetKingRelationships)) {
//            for (int j = 0; j < kingdomsInCommon.Count; j++) {
//                Kingdom currKingdom = kingdomsInCommon[j];
                KingdomRelationship relSourceKingdom = this.GetRelationshipWithKingdom(currKingdom);
                KingdomRelationship relTargetKingdom = targetKingdom.GetRelationshipWithKingdom(currKingdom);

                if (relSourceKingdom.relationshipStatus == RELATIONSHIP_STATUS.ENEMY) {
                    if (relTargetKingdom.relationshipStatus == RELATIONSHIP_STATUS.ENEMY ||
                        relTargetKingdom.relationshipStatus == RELATIONSHIP_STATUS.RIVAL) {
                        currRel.AddMutualRelationshipModifier(5);
                        targetKingdomRel.AddMutualRelationshipModifier(5);
                    }
                } else if (relSourceKingdom.relationshipStatus == RELATIONSHIP_STATUS.RIVAL) {
                    if (relTargetKingdom.relationshipStatus == RELATIONSHIP_STATUS.ENEMY) {
                        currRel.AddMutualRelationshipModifier(5);
                    } else if (relTargetKingdom.relationshipStatus == RELATIONSHIP_STATUS.RIVAL) {
                        targetKingdomRel.AddMutualRelationshipModifier(10);
                    }
                } else if (relSourceKingdom.relationshipStatus == RELATIONSHIP_STATUS.FRIEND) {
                    if (relTargetKingdom.relationshipStatus == RELATIONSHIP_STATUS.FRIEND ||
                        relTargetKingdom.relationshipStatus == RELATIONSHIP_STATUS.ALLY) {
                        currRel.AddMutualRelationshipModifier(5);
                        targetKingdomRel.AddMutualRelationshipModifier(5);
                    }
                } else if (relSourceKingdom.relationshipStatus == RELATIONSHIP_STATUS.ALLY) {
                    if (relTargetKingdom.relationshipStatus == RELATIONSHIP_STATUS.FRIEND) {
                        currRel.AddMutualRelationshipModifier(5);
                        targetKingdomRel.AddMutualRelationshipModifier(5);
                    } else if (relTargetKingdom.relationshipStatus == RELATIONSHIP_STATUS.ALLY) {
                        currRel.AddMutualRelationshipModifier(10);
                        targetKingdomRel.AddMutualRelationshipModifier(10);
                    }
                }
            }
        }
    }
    internal void ResetRelationshipModifiers() {
        for (int i = 0; i < relationships.Count; i++) {
            KingdomRelationship currRel = relationships.ElementAt(i).Value;
            currRel.ResetMutualRelationshipModifier();
            currRel.ResetEventModifiers();
        }
    }
    internal void UpdateAllRelationshipsLikeness() {
        if (this.king != null) {
            for (int i = 0; i < relationships.Count; i++) {
                KingdomRelationship rel = relationships.ElementAt(i).Value;
                rel.UpdateLikeness(null);
            }
        }
    }
    #endregion

    private void CancelInvasionPlan(KingdomRelationship relationship) {
        //CANCEL INVASION PLAN
        if(activeEvents.Select(x => x.eventType).Contains(EVENT_TYPES.INVASION_PLAN)) {
            GameEvent invasionPlanToCancel = activeEvents
                .Where(x => x.eventType == EVENT_TYPES.INVASION_PLAN && x.startedByKingdom.id == id && ((InvasionPlan)x).targetKingdom.id == relationship.targetKingdom.id)
                .FirstOrDefault();
            if(invasionPlanToCancel != null) {
                invasionPlanToCancel.CancelEvent();
            }
        }
    }
    internal void WarTrigger(KingdomRelationship relationship, GameEvent gameEventTrigger, KingdomTypeData kingdomData, WAR_TRIGGER warTrigger) {
        if (relationship == null || warTrigger == WAR_TRIGGER.NONE) {
            return;
        }
        if (!this.discoveredKingdoms.Contains(relationship.targetKingdom) ||
            !relationship.targetKingdom.discoveredKingdoms.Contains(this)) {
            //At least one of the kingdoms have not discovered each other yet
            return;
        }

        if (this.HasActiveEvent(EVENT_TYPES.INVASION_PLAN)) {
            return;
        }

        War warEvent = KingdomManager.Instance.GetWarBetweenKingdoms(this, relationship.targetKingdom);
        if (warEvent != null && warEvent.isAtWar) {
            return;
        }
        int chance = UnityEngine.Random.Range(0, 100);
        int value = 0;
//        MILITARY_STRENGTH milStrength = relationship.targetKingdom.GetMilitaryStrengthAgainst(this);

//        if (kingdomData.dictWarTriggers.ContainsKey(warTrigger)) {
//            value = kingdomData.dictWarTriggers[warTrigger];
//        }
//
//        if (kingdomData.dictWarRateModifierMilitary.ContainsKey(milStrength)) {
//            float modifier = (float)value * ((float)kingdomData.dictWarRateModifierMilitary[milStrength] / 100f);
//            value += Mathf.RoundToInt(modifier);
//        }
//        if (kingdomData.dictWarRateModifierRelationship.ContainsKey(relationship.relationshipStatus)) {
//            float modifier = (float)value * ((float)kingdomData.dictWarRateModifierRelationship[relationship.relationshipStatus] / 100f);
//            value += Mathf.RoundToInt(modifier);
//        }
//        if (kingdomData._warRateModifierPer15HexDistance != 0) {
//            int distance = PathGenerator.Instance.GetDistanceBetweenTwoTiles(this.capitalCity.hexTile, relationship.targetKingdom.capitalCity.hexTile);
//            int multiplier = (int)(distance / kingdomData.hexDistanceModifier);
//            int dividend = kingdomData._warRateModifierPer15HexDistance * multiplier;
//            float modifier = (float)value * ((float)dividend / 100f);
//            value += Mathf.RoundToInt(modifier);
//        }
//        if (kingdomData._warRateModifierPerActiveWar != 0) {
//            int dividend = kingdomData._warRateModifierPerActiveWar * this.GetWarCount();
//            float modifier = (float)value * ((float)dividend / 100f);
//            value += Mathf.RoundToInt(modifier);
//        }

        if (chance < value) {
//            if (warEvent == null) {
//                warEvent = new War(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, this.king,
//                    this, relationship.targetKingdom, warTrigger);
//            }
//            warEvent.CreateInvasionPlan(this, gameEventTrigger);
        }
    }

	/*
	 * This function is listening to the onWeekEnd Event. Put functions that you want to
	 * happen every tick here.
	 * */
	protected void KingdomTickActions(){
        if (_isGrowthEnabled) {
            this.AttemptToExpand();
        }
		this.IncreaseTechCounterPerTick();
        //this.TriggerEvents();
    }
    private void AdaptToKingValues() {
		if(!this.isDead){
			for (int i = 0; i < _dictCharacterValues.Count; i++) {
				CHARACTER_VALUE currValue = _dictCharacterValues.ElementAt(i).Key;
				if (king.importantCharacterValues.ContainsKey(currValue)) {
					UpdateSpecificCharacterValue(currValue, 1);
				} else {
					UpdateSpecificCharacterValue(currValue, -1);
				}
			}
			UpdateKingdomCharacterValues();
			SchedulingManager.Instance.AddEntry (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year + 1, () => AdaptToKingValues());
		}
//        if(GameManager.Instance.days == 1 && GameManager.Instance.month == 1) {
//            for (int i = 0; i < _dictCharacterValues.Count; i++) {
//                CHARACTER_VALUE currValue = _dictCharacterValues.ElementAt(i).Key;
//                if (king.importantCharacterValues.ContainsKey(currValue)) {
//                    UpdateSpecificCharacterValue(currValue, 1);
//                } else {
//                    UpdateSpecificCharacterValue(currValue, -1);
//                }
//            }
//            UpdateKingdomCharacterValues();
//        }
    }
    private void AttemptToAge() {
		if(!this.isDead){
			age += 1;
			SchedulingManager.Instance.AddEntry (GameManager.Instance.month, GameManager.Instance.days, (GameManager.Instance.year + 1), () => AttemptToAge());
		}

//        if(GameManager.Instance.year > foundationYear && GameManager.Instance.month == foundationMonth && GameManager.Instance.days == foundationDay) {
//            age += 1;
//        }
    }
    private void TriggerEvents() {
//        this.TriggerSlavesMerchant();
//        this.TriggerHypnotism();
        this.TriggerKingdomHoliday();
//        //this.TriggerDevelopWeapons();
//        this.TriggerKingsCouncil();
//		this.TriggerCrime ();
    }
	private void ScheduleEvents(){
		SchedulingManager.Instance.AddEntry (GameManager.Instance.month, GameManager.daysInMonth[GameManager.Instance.month], GameManager.Instance.year, () => TriggerSlavesMerchant());
		SchedulingManager.Instance.AddEntry (GameManager.Instance.month, GameManager.daysInMonth[GameManager.Instance.month], GameManager.Instance.year, () => TriggerHypnotism());
		SchedulingManager.Instance.AddEntry (GameManager.Instance.month, GameManager.daysInMonth[GameManager.Instance.month], GameManager.Instance.year, () => TriggerKingsCouncil());

		int month = UnityEngine.Random.Range (1, 5);
		SchedulingManager.Instance.AddEntry (month, UnityEngine.Random.Range(1, GameManager.daysInMonth[month]), GameManager.Instance.year, () => TriggerCrime());
	}
    /*
    * Deacrease the kingdom's unrest by UNREST_DECREASE_PER_MONTH amount every month.
    * */
    protected void DecreaseUnrestEveryMonth() {
		if(!this.isDead){
	        this.AdjustUnrest(UNREST_DECREASE_PER_MONTH);
	        GameDate gameDate = new GameDate(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
	        gameDate.AddMonths(1);
	        gameDate.day = GameManager.daysInMonth[gameDate.month];
	        SchedulingManager.Instance.AddEntry(gameDate.month, gameDate.day, gameDate.year, () => DecreaseUnrestEveryMonth());
		}
    }
    /*
	 * Kingdom will attempt to expand. 
	 * Chance for expansion can be edited by changing the value of expansionChance.
	 * NOTE: expansionChance increases on it's own.
	 * */
    protected void AttemptToExpand() {
        if (HasActiveEvent(EVENT_TYPES.EXPANSION)) {
            //Kingdom has a currently active expansion event
            return;
        }

        if(cities.Count >= cityCap) {
            //Kingdom has reached max city capacity
            return;
        }

        float upperBound = 300f + (150f * (float)this.cities.Count);
        float chance = UnityEngine.Random.Range(0, upperBound);
        if (chance < this.expansionChance) {
            if (this.cities.Count > 0) {
                EventCreator.Instance.CreateExpansionEvent(this);
            }

        }
    }

    #region Prestige
    internal void AdjustPrestige(int adjustment) {
        _prestige += adjustment;
        KingdomManager.Instance.UpdateKingdomPrestigeList();
    }
    internal void MonthlyPrestigeActions() {
        //Add Prestige
        AdjustPrestige(10 + (2 * cities.Count));

        //Check if city count exceeds cap
        if (cities.Count > cityCap) {
            //If the Kingdom exceeds this, each month, all Governor's Opinion will decrease by 5 for every city over the cap
            int numOfExcessCities = cities.Count - cityCap;
            int increaseInDisloyalty = 5 * numOfExcessCities;
            _disloyaltyFromPrestige += increaseInDisloyalty;
        } else {
            if(_disloyaltyFromPrestige > 0) {
                //This will slowly recover when Prestige gets back to normal.
                _disloyaltyFromPrestige -= 5;
                if(_disloyaltyFromPrestige < 0) {
                    _disloyaltyFromPrestige = 0;
                }
            }
        }


        //Reschedule event
        GameDate gameDate = new GameDate(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
        gameDate.AddMonths(1);
        gameDate.day = GameManager.daysInMonth[gameDate.month];
        SchedulingManager.Instance.AddEntry(gameDate.month, gameDate.day, gameDate.year, () => MonthlyPrestigeActions());
    }
    #endregion

    #region Trading
    internal void AddKingdomToEmbargoList(Kingdom kingdomToAdd, EMBARGO_REASON embargoReason = EMBARGO_REASON.NONE) {
        if (!this._embargoList.ContainsKey(kingdomToAdd)) {
            this._embargoList.Add(kingdomToAdd, embargoReason);
            //Remove all existing trade routes between kingdomToAdd and this Kingdom
            //this.RemoveAllTradeRoutesWithOtherKingdom(kingdomToAdd);
            //kingdomToAdd.RemoveAllTradeRoutesWithOtherKingdom(this);
            kingdomToAdd.AdjustUnrest(UNREST_INCREASE_EMBARGO);
        }
        
    }
    internal void RemoveKingdomFromEmbargoList(Kingdom kingdomToRemove) {
        this._embargoList.Remove(kingdomToRemove);
    }
    #endregion

    #region City Management
    /* 
     * <summary>
	 * Create a new city obj on the specified hextile.
	 * Then add it to this kingdoms cities.
     * </summary>
	 * */
    internal City CreateNewCityOnTileForKingdom(HexTile tile) {
        City createdCity = CityGenerator.Instance.CreateNewCity(tile, this);
        this.AddCityToKingdom(createdCity);
        return createdCity;
    }
    /* 
     * <summary>
     * Add a city to this kingdom.
     * Recompute kingdom type data, available resources and
     * daily growth of all cities. Assign city as capital city
     * if city is first city in kingdom.
     * </summary>
     * */
    internal void AddCityToKingdom(City city) {
        this._cities.Add(city);
        this.UpdateKingdomTypeData();
        this.UpdateAvailableResources();
        this.UpdateAllCitiesDailyGrowth();
        this.UpdateExpansionRate();
        if (this._cities.Count == 1 && this._cities[0] != null) {
            SetCapitalCity(this._cities[0]);
        }
    }
    /* 
     * <summary>
     * Remove city from this kingdom.
     * Check if kingdom is dead beacuse of city removal.
     * If not, recompute this kingdom's capital city, kingdom type data, 
     * available resources, and daily growth of remaining cities.
     * </summary>
     * */
    internal void RemoveCityFromKingdom(City city) {
        city.rebellion = null;
        this._cities.Remove(city);
        this.CheckIfKingdomIsDead();
        if (!this.isDead) {
            //this.RemoveInvalidTradeRoutes();
            this.UpdateKingdomTypeData();
            this.UpdateAvailableResources();
            this.UpdateAllCitiesDailyGrowth();
            this.UpdateExpansionRate();
            //if (this._cities[0] != null) {
            SetCapitalCity(this._cities[0]);
            //}
        }

    }
    internal void SetCapitalCity(City city) {
        this.capitalCity = city;
        HexTile habitableTile;
        if (this.basicResource == BASE_RESOURCE_TYPE.STONE) {
            for (int i = 0; i < CityGenerator.Instance.stoneHabitableTiles.Count; i++) {
                habitableTile = CityGenerator.Instance.stoneHabitableTiles[i];
                this.capitalCity.AddHabitableTileDistance(habitableTile, PathGenerator.Instance.GetDistanceBetweenTwoTiles(city.hexTile, habitableTile));
            }
        } else if (this.basicResource == BASE_RESOURCE_TYPE.WOOD) {
            for (int i = 0; i < CityGenerator.Instance.woodHabitableTiles.Count; i++) {
                habitableTile = CityGenerator.Instance.woodHabitableTiles[i];
                this.capitalCity.AddHabitableTileDistance(habitableTile, PathGenerator.Instance.GetDistanceBetweenTwoTiles(city.hexTile, habitableTile));
            }
        }
    }
    #endregion

    #region Citizen Management
    internal List<Citizen> GetAllCitizensOfType(ROLE role) {
        List<Citizen> citizensOfType = new List<Citizen>();
        for (int i = 0; i < this.cities.Count; i++) {
            citizensOfType.AddRange(this.cities[i].GetCitizensWithRole(role));
        }
        return citizensOfType;
    }
    #endregion

    #region Succession
    internal void UpdateKingSuccession() {
		this.successionLine.Clear();
		orderedMaleRoyalties.Clear ();
		orderedFemaleRoyalties.Clear ();
		orderedBrotherRoyalties.Clear ();
		orderedSisterRoyalties.Clear ();

		for (int i = 0; i < this.successionLine.Count; i++) {
			if (this.successionLine [i].isDirectDescendant) {
				if (this.successionLine [i].generation > this.king.generation) {
					if (this.successionLine [i].gender == GENDER.MALE) {
						orderedMaleRoyalties.Add (this.successionLine [i]);
					} else {
						orderedFemaleRoyalties.Add (this.successionLine [i]);
					}
				}
			}
		}
		for (int i = 0; i < this.successionLine.Count; i++) {
			if (!this.successionLine [i].isDirectDescendant && this.successionLine [i].id != this.king.id) {
				if ((this.successionLine [i].father != null && this.king.father != null) && this.successionLine [i].father.id == this.king.father.id) {
					if (this.successionLine [i].gender == GENDER.MALE) {
						orderedBrotherRoyalties.Add (this.successionLine [i]);
					}else{
						orderedSisterRoyalties.Add (this.successionLine [i]);
					}
				}
			}
		}

		this.successionLine.AddRange (orderedMaleRoyalties.OrderBy (x => x.generation).ThenByDescending (x => x.age));
		this.successionLine.AddRange (orderedFemaleRoyalties.OrderBy (x => x.generation).ThenByDescending (x => x.age));

		this.successionLine.AddRange (orderedBrotherRoyalties.OrderByDescending (x => x.age));
		this.successionLine.AddRange (orderedSisterRoyalties.OrderByDescending (x => x.age));
    }
    internal void ChangeSuccessionLineRescursively(Citizen royalty) {
        if (this.king.id != royalty.id) {
            if (!royalty.isDead) {
                this.successionLine.Add(royalty);
            }
        }

        for (int i = 0; i < royalty.children.Count; i++) {
            if (royalty.children[i] != null) {
                this.ChangeSuccessionLineRescursively(royalty.children[i]);
            }
        }
    }
    internal void RemoveFromSuccession(Citizen citizen) {
        if (citizen != null) {
            for (int i = 0; i < this.successionLine.Count; i++) {
                if (this.successionLine[i].id == citizen.id) {
                    this.successionLine.RemoveAt(i);
                    break;
                }
            }
        }
    }
    internal void AssignNewKing(Citizen newKing, City city = null) {
        if (this.king != null) {
            if (this.king.city != null) {
                this.king.city.hasKing = false;
            }
        }

        if (newKing == null) {
            if (city == null) {
                if (this.king.city.isDead) {
                    Debug.LogError("City of previous king is dead! But still creating king in that dead city");
                }
                newKing = this.king.city.CreateNewKing();
            } else {
                newKing = city.CreateNewKing();
            }
            if (newKing == null) {
                if (this.king != null) {
                    if (this.king.city != null) {
                        this.king.city.hasKing = true;
                    }
                }
                return;
            }
        }
        SetCapitalCity(newKing.city);
        newKing.city.hasKing = true;

        if (!newKing.isDirectDescendant) {
            Utilities.ChangeDescendantsRecursively(newKing, true);
            if (this.king != null) {
                Utilities.ChangeDescendantsRecursively(this.king, false);
            }
        }

        Citizen previousKing = this.king;
        bool isNewKingdomGovernor = newKing.isGovernor;

        newKing.AssignRole(ROLE.KING);

        if (isNewKingdomGovernor) {
            newKing.city.AssignNewGovernor();
        }

        newKing.history.Add(new History(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, newKing.name + " became the new King/Queen of " + this.name + ".", HISTORY_IDENTIFIER.NONE));

        ResetRelationshipModifiers();
        UpdateMutualRelationships();

        this.successionLine.Clear();
        ChangeSuccessionLineRescursively(newKing);
        this.successionLine.AddRange(newKing.GetSiblings());
        UpdateKingSuccession();

        this.UpdateAllGovernorsLoyalty();
        this.UpdateAllRelationshipsLikeness();
    }
    #endregion

    #region War
    internal void AddInternationalWar(Kingdom kingdom) {
        //		Debug.Log ("INTERNATIONAL WAR");
        //		for(int i = 0; i < kingdom.cities.Count; i++){
        //			if(!this.intlWarCities.Contains(kingdom.cities[i])){
        //				this.intlWarCities.Add(kingdom.cities[i]);
        //			}
        //		}
        //		this.TargetACityToAttack ();
        //		for(int i = 0; i < this.cities.Count; i++){
        //			if(!this.king.campaignManager.SearchForDefenseWarCities(this.cities[i], WAR_TYPE.INTERNATIONAL)){
        //				this.king.campaignManager.defenseWarCities.Add(new CityWar(this.cities[i], false, WAR_TYPE.INTERNATIONAL));
        //			}
        //			if(this.cities[i].governor.supportedCitizen == null){
        //				if(!this.king.campaignManager.SearchForDefenseWarCities(kingdom.cities[i])){
        //					this.king.campaignManager.defenseWarCities.Add(new CityWar(kingdom.cities[i], false, WAR_TYPE.INTERNATIONAL));
        //				}
        //			}else{
        //				if(!this.king.SearchForSuccessionWar(this.cities[i].governor.supportedCitizen)){
        //					if(!this.king.campaignManager.SearchForDefenseWarCities(kingdom.cities[i])){
        //						this.king.campaignManager.defenseWarCities.Add(new CityWar(kingdom.cities[i], false, WAR_TYPE.INTERNATIONAL));
        //					}
        //				}
        //			}
        //		}
        //		this.king.campaignManager.CreateCampaign ();
    }
    internal void RemoveInternationalWar(Kingdom kingdom) {
        //		this.intlWarCities.RemoveAll(x => x.kingdom.id == kingdom.id);
        //		for(int i = 0; i < this.king.campaignManager.activeCampaigns.Count; i++){
        //			if(this.king.campaignManager.activeCampaigns[i].warType == WAR_TYPE.INTERNATIONAL){
        //				if(this.king.campaignManager.activeCampaigns[i].targetCity.kingdom.id == kingdom.id){
        //					this.king.campaignManager.CampaignDone(this.king.campaignManager.activeCampaigns[i]);
        //				}
        //			}
        //		}
    }
    //	internal void PassOnInternationalWar(){
    //		this.holderIntlWarCities.Clear();
    //		this.holderIntlWarCities.AddRange(this.intlWarCities);
    //	}
    //	internal void RetrieveInternationWar(){
    //		this.intlWarCities.AddRange(this.holderIntlWarCities);
    //		this.holderIntlWarCities.Clear();
    //	}
    //
    //internal City SearchForCityById(int id){
    //	for(int i = 0; i < this.cities.Count; i++){
    //		if(this.cities[i].id == id){
    //			return this.cities[i];
    //		}
    //	}
    //	return null;
    //}


    //	internal void AddInternationalWarCity(City newCity){
    //		for(int i = 0; i < this.relationshipsWithOtherKingdoms.Count; i++){
    //			if(this.relationshipsWithOtherKingdoms[i].isAtWar){
    //				if(!this.relationshipsWithOtherKingdoms[i].targetKingdom.intlWarCities.Contains(newCity)){
    //					this.relationshipsWithOtherKingdoms [i].targetKingdom.intlWarCities.Add (newCity);
    //				}
    //			}
    //		}
    //		if(this.IsKingdomHasWar()){
    //			if(!this.king.campaignManager.SearchForDefenseWarCities(newCity, WAR_TYPE.INTERNATIONAL)){
    //				this.king.campaignManager.defenseWarCities.Add (new CityWar (newCity, false, WAR_TYPE.INTERNATIONAL));
    //			}
    //		}

    //	}
    //internal bool IsKingdomHasWar(){
    //	for(int i = 0; i < this.relationshipsWithOtherKingdoms.Count; i++){
    //		if(this.relationshipsWithOtherKingdoms[i].isAtWar){
    //			return true;
    //		}
    //	}
    //	return false;
    //}
    internal void AdjustExhaustionToAllRelationship(int amount) {
        for (int i = 0; i < relationships.Count; i++) {
            relationships.ElementAt(i).Value.AdjustExhaustion(amount);
        }
    }
    internal void ConquerCity(City city, General attacker) {
        if (this.id != city.kingdom.id) {
            KingdomRelationship rel = this.GetRelationshipWithKingdom(city.kingdom);
            if (rel != null && rel.war != null) {
                rel.war.warPair.isDone = true;
            }

            HexTile hex = city.hexTile;
            //city.KillCity();
            if (this.race != city.kingdom.race) {
                city.KillCity();
            } else {
                city.ConquerCity(this);
                city.kingdom.RemoveCityFromKingdom(city);
            }

            //yield return null;
            //City newCity = CreateNewCityOnTileForKingdom(hex);
            //newCity.hp = 100;
            //newCity.CreateInitialFamilies(false);
            //			this.AddInternationalWarCity (newCity);
            //KingdomManager.Instance.CheckWarTriggerMisc(city.kingdom, WAR_TRIGGER.TARGET_GAINED_A_CITY);
            //Adjust unrest because a city of this kingdom was conquered.
            this.AdjustUnrest(UNREST_INCREASE_CONQUER);
        } else {
            if (city is RebelFort) {
                city.rebellion.KillFort();
                //				HexTile hex = city.hexTile;
                //				city.KillCity();
            } else {
                if (city.rebellion != null) {
                    city.ChangeToCity();
                } else {
                    city.ChangeToRebelFort(attacker.citizen.city.rebellion);
                }
            }

        }

    }
    //internal bool CheckForSpecificWar(Kingdom kingdom){
    //	for(int i = 0; i < this.relationshipsWithOtherKingdoms.Count; i++){
    //		if(this.relationshipsWithOtherKingdoms[i].targetKingdom.id == kingdom.id){
    //			if(this.relationshipsWithOtherKingdoms[i].isAtWar){
    //				return true;
    //			}
    //		}
    //	}
    //	return false;
    //}
    //internal void AssimilateKingdom(Kingdom newKingdom){
    //	for(int i = 0; i < this.cities.Count; i++){
    //		newKingdom.AddCityToKingdom (this.cities [i]);
    //	}
    //	KingdomManager.Instance.MakeKingdomDead(this);
    //}
    #endregion

    #region Kingdom Tile Management
    internal void HighlightAllOwnedTilesInKingdom() {
        for (int i = 0; i < this.cities.Count; i++) {
            if (UIManager.Instance.currentlyShowingCity != null && UIManager.Instance.currentlyShowingCity.id == this.cities[i].id) {
                continue;
            }
            this.cities[i].HighlightAllOwnedTiles(127.5f / 255f);
        }
    }
    internal void UnHighlightAllOwnedTilesInKingdom() {
        for (int i = 0; i < this.cities.Count; i++) {
            if (UIManager.Instance.currentlyShowingCity != null && UIManager.Instance.currentlyShowingCity.id == this.cities[i].id) {
                continue;
            }
            this.cities[i].UnHighlightAllOwnedTiles();
        }
    }
    #endregion


    internal MILITARY_STRENGTH GetMilitaryStrengthAgainst(Kingdom kingdom){
		int sourceMilStrength = this.GetAllCityHp ();
		int targetMilStrength = kingdom.GetAllCityHp ();

		int fiftyPercent = (int)(targetMilStrength * 0.50f);
		int twentyPercent = (int)(targetMilStrength * 0.20f);
//		Debug.Log ("TARGET MILITARY STRENGTH: " + targetMilStrength);
//		Debug.Log ("SOURCE MILITARY STRENGTH: " + sourceMilStrength);
		if(sourceMilStrength == 0 && targetMilStrength == 0){
			Debug.Log (this.name + "'s military is COMPARABLE to " + kingdom.name);
			return MILITARY_STRENGTH.COMPARABLE;
		}else{
			if(sourceMilStrength > (targetMilStrength + fiftyPercent)){
				Debug.Log (this.name + "'s military is MUCH STRONGER than " + kingdom.name);
				return MILITARY_STRENGTH.MUCH_STRONGER;
			}else if(sourceMilStrength > (targetMilStrength + twentyPercent)){
				Debug.Log (this.name + "'s military is SLIGHTLY STRONGER than " + kingdom.name);
				return MILITARY_STRENGTH.SLIGHTLY_STRONGER;
			}else if(sourceMilStrength > (targetMilStrength - twentyPercent)){
				Debug.Log (this.name + "'s military is COMPARABLE to " + kingdom.name);
				return MILITARY_STRENGTH.COMPARABLE;
			}else if(sourceMilStrength > (targetMilStrength - fiftyPercent)){
				Debug.Log (this.name + "'s military is SLIGHTLY WEAKER than " + kingdom.name);
				return MILITARY_STRENGTH.SLIGHTLY_WEAKER;
			}else{
				Debug.Log (this.name + "'s military is MUCH WEAKER than " + kingdom.name);
				return MILITARY_STRENGTH.MUCH_WEAKER;
			}

		}
	}

	internal int GetAllCityHp(){
		int total = 0;
		for(int i = 0; i < this.cities.Count; i++){
			total += this.cities[i].hp;
		}
		return total;
	}
	internal int GetWarCount(){
		int total = 0;
		for (int i = 0; i < relationships.Count; i++) {
			if(relationships.ElementAt(i).Value.isAtWar){
				total += 1;
			}
		}
		return total;
	}
	internal City GetNearestCityFromKing(List<City> cities){
		City nearestCity = null;
		int nearestDistance = 0;
		for(int i = 0; i < cities.Count; i++){
			List<HexTile> path = PathGenerator.Instance.GetPath (cities [i].hexTile, this.king.city.hexTile, PATHFINDING_MODE.AVATAR);
			if(path != null){
				if(nearestCity == null){
					nearestCity = cities [i];
					nearestDistance = path.Count;
				}else{
					if(path.Count < nearestDistance){
						nearestCity = cities [i];
						nearestDistance = path.Count;
					}
				}
			}
		}
		return nearestCity;
	}

	#region Resource Management
	/*
	 * Add resource type to this kingdoms
	 * available resource (DO NOT ADD GOLD TO THIS!).
	 * */
	internal void AddResourceToKingdom(RESOURCE resource){
		RESOURCE_BENEFITS resourceBenefit = Utilities.resourceBenefits[resource].Keys.FirstOrDefault();

        if (!this._availableResources.ContainsKey(resource)) {
			this._availableResources.Add(resource, 0);
            //this.RemoveObsoleteTradeRoutes(resource);
            if(resourceBenefit == RESOURCE_BENEFITS.GROWTH_RATE) {
                this.UpdateAllCitiesDailyGrowth();
            } else if (resourceBenefit == RESOURCE_BENEFITS.TECH_LEVEL) {
                this.UpdateTechLevel();
            }
        }
		this._availableResources[resource] += 1;
        if (resourceBenefit == RESOURCE_BENEFITS.EXPANSION_RATE) {
            this.UpdateExpansionRate();
        }
    }
    internal void UpdateExpansionRate() {
        this.expansionChance = this.kingdomTypeData.expansionRate;

        for (int i = 0; i < this.availableResources.Keys.Count; i++) {
            RESOURCE currResource = this.availableResources.Keys.ElementAt(i);
            if (Utilities.GetBaseResourceType(currResource) == this.basicResource) {
                int multiplier = this.availableResources[currResource];
				RESOURCE_BENEFITS resourceBenefit = Utilities.resourceBenefits[currResource].Keys.FirstOrDefault();
                float expansionRateGained = Utilities.resourceBenefits[currResource][resourceBenefit];
                if (resourceBenefit == RESOURCE_BENEFITS.EXPANSION_RATE) {
                    this.expansionChance += expansionRateGained * multiplier;
                }
            }
        }
    }
    internal void UpdateTechLevel() {
        this._techLevel = 1;
//        List<RESOURCE> allAvailableResources = this._availableResources.Keys.ToList();
//        for (int i = 0; i < allAvailableResources.Count; i++) {
		foreach (RESOURCE currResource in this._availableResources.Keys) {
//            RESOURCE currResource = allAvailableResources[i];
			RESOURCE_BENEFITS resourceBenefit = Utilities.resourceBenefits[currResource].Keys.FirstOrDefault();
            if (resourceBenefit == RESOURCE_BENEFITS.TECH_LEVEL) {
                this._techLevel += (int)Utilities.resourceBenefits[currResource][resourceBenefit];
            }
        }
    }
    internal void UpdateAllCitiesDailyGrowth() {
        //get all resources from tiles and trade routes, only include trade routes where this kingom is the target
        List<RESOURCE> allAvailableResources = this._availableResources.Keys.ToList();
        int dailyGrowthGained = this.ComputeDailyGrowthGainedFromResources(allAvailableResources);
        for (int i = 0; i < this.cities.Count; i++) {
            City currCity = this.cities[i];
            currCity.UpdateDailyGrowthBasedOnSpecialResources(dailyGrowthGained);
        }
    }
    private int ComputeDailyGrowthGainedFromResources(List<RESOURCE> allAvailableResources) {
        int dailyGrowthGained = 0;
        for (int i = 0; i < allAvailableResources.Count; i++) {
            RESOURCE currentResource = allAvailableResources[i];
			RESOURCE_BENEFITS resourceBenefit = Utilities.resourceBenefits[currentResource].Keys.FirstOrDefault();
            if(resourceBenefit == RESOURCE_BENEFITS.GROWTH_RATE) {
                dailyGrowthGained += (int)Utilities.resourceBenefits[currentResource][resourceBenefit];
            }
        }
        return dailyGrowthGained;
    }
    /*
     * Gets a list of resources that otherKingdom does not have access to (By self or by trade).
     * Will compare to this kingdoms available resources (excl. resources from trade)
     * */
    internal List<RESOURCE> GetResourcesOtherKingdomDoesNotHave(Kingdom otherKingdom) {
        List<RESOURCE> resourcesOtherKingdomDoesNotHave = new List<RESOURCE>();
        List<RESOURCE> allAvailableResourcesOfOtherKingdom = otherKingdom.availableResources.Keys.ToList();
        for (int i = 0; i < this._availableResources.Keys.Count; i++) {
            RESOURCE currKey = this._availableResources.Keys.ElementAt(i);
            if (!allAvailableResourcesOfOtherKingdom.Contains(currKey)) {
                //otherKingdom does not have that resource
                resourcesOtherKingdomDoesNotHave.Add(currKey);
            }
        }
        return resourcesOtherKingdomDoesNotHave;
    }
    internal void UpdateAvailableResources() {
        this._availableResources.Clear();
        for (int i = 0; i < this.cities.Count; i++) {
            City currCity = this.cities[i];
            for (int j = 0; j < currCity.structures.Count; j++) {
                HexTile currHexTile = currCity.structures[j];
                if (currHexTile.specialResource != RESOURCE.NONE) {
                    this.AddResourceToKingdom(currHexTile.specialResource);
                }
            }
        }
    }
    /*
     * <summary>
     * Set growth state of kingdom, disabling growth will prevent expansion,
     * building of new settlements and pregnancy
     * */
    internal void SetGrowthState(bool state) {
        _isGrowthEnabled = state;
    }
    #endregion

    #region Unrest
    internal void AdjustUnrest(int amountToAdjust) {
        this._unrest += amountToAdjust;
        this._unrest = Mathf.Clamp(this._unrest, 0, 100);
    }
	internal void ChangeUnrest(int newAmount){
		this._unrest = newAmount;
		this._unrest = Mathf.Clamp(this._unrest, 0, 100);
	}
    #endregion

    #region Tech
    private void IncreaseTechCounterPerTick(){
		if(!this._isTechProducing){
			return;
		}
		int amount = 1 * this.cities.Count;
		int bonus = 0;
        for (int i = 0; i < this._availableResources.Count; i++) {
            RESOURCE currResource = this._availableResources.Keys.ElementAt(i);
			RESOURCE_BENEFITS resourceBenefit = Utilities.resourceBenefits[currResource].Keys.FirstOrDefault();
            if(resourceBenefit == RESOURCE_BENEFITS.TECH_LEVEL) {
                bonus += (int)Utilities.resourceBenefits[currResource][resourceBenefit];
            }
        }
		amount += bonus;
		amount = (int)(amount * this._techProductionPercentage);
		this.AdjustTechCounter (amount);
	}
	private void UpdateTechCapacity(){
		this._techCapacity = 2000 * this._techLevel;
	}
	internal void AdjustTechCounter(int amount){
		this._techCounter += amount;
		this._techCounter = Mathf.Clamp(this._techCounter, 0, this._techCapacity);
		if(this._techCounter == this._techCapacity){
			this.UpgradeTechLevel (1);
		}
	}
	internal void UpgradeTechLevel(int amount){
		this._techLevel += amount;
		if(this._techLevel < 1){
			this._techLevel = 1;
		}
		this._techCounter = 0;
		this.UpdateTechCapacity ();
	}
	#endregion
	
	#region Discovery
    /*
     * Check all the neighburs of the border tiles and owned tiles of all this kingdom's
     * cities, and check if any of them are owned by another kingdom, if so,
     * the two kingdoms have now discovered each other.
     * */
    internal void CheckForDiscoveredKingdoms() {
        for (int i = 0; i < this.cities.Count; i++) {
            City currCity = this.cities[i];
            List<HexTile> tilesToCheck = currCity.ownedTiles.Union(currCity.borderTiles).ToList();
            for (int j = 0; j < tilesToCheck.Count; j++) {
                //Get all neighbour tiles that are owned, but not by this kingdom, 
                //and that kingdom is not already added to this kingdom's discovered kingdoms.
                List<HexTile> neighbours = tilesToCheck[j].AllNeighbours;
                    //.Where(x => x.ownedByCity != null && x.ownedByCity.kingdom.id != this.id && !this._discoveredKingdoms.Contains(x.ownedByCity.kingdom))
                    //.ToList();
                for (int k = 0; k < neighbours.Count; k++) {
                    HexTile currNeighbour = neighbours[k];
                    if (currNeighbour.isOccupied && currNeighbour.ownedByCity != null
                        && currNeighbour.ownedByCity.kingdom.id != this.id) {
                        Kingdom otherKingdom = currNeighbour.ownedByCity.kingdom;
						KingdomManager.Instance.DiscoverKingdom (this, otherKingdom);
                    } else if (currNeighbour.isBorder) {
                        for (int l = 0; l < currNeighbour.isBorderOfCities.Count; l++) {
                            Kingdom otherKingdom = currNeighbour.isBorderOfCities[l].kingdom;
                            if (otherKingdom.id != this.id && !this.discoveredKingdoms.Contains(otherKingdom)) {
                                KingdomManager.Instance.DiscoverKingdom(this, otherKingdom);
                            }
                        }
                    }
                }
            }
        }
    }
    /*
     * Check all the neighbours of a HexTile and check if any of them are owned by another kingdom, if so,
     * the two kingdoms have now discovered each other.
     * */
    internal void CheckForDiscoveredKingdoms(City city) {
        //Get all neighbour tiles that are owned, but not by this kingdom, 
        //and that kingdom is not already added to this kingdom's discovered kingdoms.
        List<HexTile> tilesToCheck = city.ownedTiles.Union(city.borderTiles).ToList();
        for (int i = 0; i < tilesToCheck.Count; i++) {
            List<HexTile> neighbours = tilesToCheck[i].AllNeighbours;
            for (int j = 0; j < neighbours.Count; j++) {
                HexTile currNeighbour = neighbours[j];
                if (currNeighbour.isOccupied && currNeighbour.ownedByCity != null
                    && currNeighbour.ownedByCity.kingdom.id != this.id) {
                    Kingdom otherKingdom = currNeighbour.ownedByCity.kingdom;
					KingdomManager.Instance.DiscoverKingdom (this, otherKingdom);
                } else if (currNeighbour.isBorder) {
                    for (int k = 0; k < currNeighbour.isBorderOfCities.Count; k++) {
                        Kingdom otherKingdom = currNeighbour.isBorderOfCities[k].kingdom;
                        if (otherKingdom.id != this.id && !this.discoveredKingdoms.Contains(otherKingdom)) {
                            KingdomManager.Instance.DiscoverKingdom(this, otherKingdom);
                        }
                    }
                }
            }
        }
    }
    internal void DiscoverKingdom(Kingdom discoveredKingdom) {
        if(discoveredKingdom.id != this.id) {
            if (!this._discoveredKingdoms.Contains(discoveredKingdom)) {
                this._discoveredKingdoms.Add(discoveredKingdom);
                Debug.Log(this.name + " discovered " + discoveredKingdom.name + "!");
                if (discoveredKingdom.plague != null) {
                    discoveredKingdom.plague.ForceUpdateKingRelationships(discoveredKingdom.king);
                }
            }
        }
    }
    internal void RemoveKingdomFromDiscoveredKingdoms(Kingdom kingdomToRemove) {
        for (int i = 0; i < _discoveredKingdoms.Count; i++) {
            Kingdom currKingdom = _discoveredKingdoms[i];
            if(currKingdom.id == kingdomToRemove.id) {
                _discoveredKingdoms.RemoveAt(i);
                break;
            }
        }
        //this._discoveredKingdoms.Remove(kingdomToRemove);
    }
    #endregion

	#region Character Values
	private void UpdateCharacterValuesOfKingsAndGovernors(){
		if(this.king != null){
			this.king.UpdateCharacterValues ();
		}
		for(int i = 0; i < this.cities.Count; i++){
			if(this.cities[i].governor != null){
				this.cities [i].governor.UpdateCharacterValues ();
			}
		}
	}
    internal void GenerateKingdomCharacterValues() {
        this._dictCharacterValues.Clear();
        this._dictCharacterValues = System.Enum.GetValues(typeof(CHARACTER_VALUE)).Cast<CHARACTER_VALUE>().ToDictionary(x => x, x => UnityEngine.Random.Range(1, 101));
        UpdateKingdomCharacterValues();
    }
    internal void UpdateKingdomCharacterValues() {
        this._importantCharacterValues = this._dictCharacterValues.Where(x => x.Value >= 50).OrderByDescending(x => x.Value).Take(4).ToDictionary(x => x.Key, x => x.Value);
    }
    private void UpdateSpecificCharacterValue(CHARACTER_VALUE key, int value) {
        if (this._dictCharacterValues.ContainsKey(key)) {
            this._dictCharacterValues[key] += value;
            //			UpdateCharacterValueByKey(key, value);
        }
    }
    #endregion

    #region Bioweapon
    internal void SetBioWeapon(bool state){
		this._hasBioWeapon = state;
	}
	#endregion

	#region Boon Of Power
	internal void CollectBoonOfPower(BoonOfPower boonOfPower){
		Debug.Log (this.name + " HAS COLLECTED A BOON OF POWER!");
		this._boonOfPowers.Add (boonOfPower);
		boonOfPower.AddOwnership (this);
	}
	internal void DestroyBoonOfPower(BoonOfPower boonOfPower){
		this._activatedBoonOfPowers.Remove (boonOfPower);
	}
	internal void ActivateBoonOfPowers(){
		for (int i = 0; i < this._boonOfPowers.Count; i++) {
			this._boonOfPowers [i].Activate ();
			this._activatedBoonOfPowers.Add (this._boonOfPowers [i]);
		}
		this._boonOfPowers.Clear ();
	}
	#endregion

	#region First And Keystone
	internal void CollectKeystone(){
		Debug.Log (this.name + " HAS COLLECTED A KEYSTONE!");
		GameEvent gameEvent = WorldEventManager.Instance.SearchEventOfType(EVENT_TYPES.FIRST_AND_KEYSTONE);
		if(gameEvent != null){
			((FirstAndKeystone)gameEvent).ChangeKeystoneOwnership (this);
		}
	}
	internal void CollectFirst(){
		Debug.Log (this.name + " HAS COLLECTED THE FIRST!");
		GameEvent gameEvent = WorldEventManager.Instance.SearchEventOfType(EVENT_TYPES.FIRST_AND_KEYSTONE);
		if(gameEvent != null){
			((FirstAndKeystone)gameEvent).ChangeFirstOwnership (this);
		}
	}
	#endregion

	internal bool HasWar(){
		for(int i = 0; i < relationships.Count; i++){
			if(relationships.ElementAt(i).Value.isAtWar){
				return true;
			}
		}
		return false;
	}

	internal Citizen GetRandomGovernorFromKingdom(){
		City randomCity = this.cities [UnityEngine.Random.Range (0, this.cities.Count)];
		return randomCity.governor;
	}

	#region Slaves Merchant
	private void TriggerSlavesMerchant(){
		if(!this.isDead){
			int chance = UnityEngine.Random.Range(0,100);
			if(chance < 8){
				EventCreator.Instance.CreateSlavesMerchantEvent(this.king);
			}
			GameDate gameDate = new GameDate (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
			gameDate.AddMonths (2);
			gameDate.day = UnityEngine.Random.Range (1, GameManager.daysInMonth [gameDate.month]);
			SchedulingManager.Instance.AddEntry (gameDate.month, gameDate.day, gameDate.year, () => TriggerSlavesMerchant());
		}
//		if(GameManager.Instance.days == 20){
//			int chance = UnityEngine.Random.Range(0,100);
//			if(chance < 8){
//				EventCreator.Instance.CreateSlavesMerchantEvent(this.king);
//			}
//		}
	}
    #endregion

    #region Hypnotism
    private void TriggerHypnotism() {
		if(!this.isDead){
			if (this.king.importantCharacterValues.ContainsKey(CHARACTER_VALUE.INFLUENCE)) {
				List<GameEvent> previousHypnotismEvents = GetEventsOfType (EVENT_TYPES.HYPNOTISM, false);
				if (!previousHypnotismEvents.Where(x => x.startYear == GameManager.Instance.year).Any()) {
					List<Kingdom> notFriends = new List<Kingdom>();
					for (int i = 0; i < discoveredKingdoms.Count; i++) {
						Kingdom currKingdom = discoveredKingdoms[i];
						KingdomRelationship rel = currKingdom.GetRelationshipWithKingdom(this);
						if (rel.relationshipStatus != RELATIONSHIP_STATUS.FRIEND && rel.relationshipStatus != RELATIONSHIP_STATUS.ALLY) {
							notFriends.Add(currKingdom);
						}
					}
					if (UnityEngine.Random.Range(0, 100) < 10 && notFriends.Count > 0) {
						EventCreator.Instance.CreateHypnotismEvent(this, notFriends[UnityEngine.Random.Range(0, notFriends.Count)]);
					}
				}
			}
			GameDate gameDate = new GameDate (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
			gameDate.AddMonths (1);
			gameDate.day = GameManager.daysInMonth [gameDate.month];
			SchedulingManager.Instance.AddEntry (gameDate.month, gameDate.day, gameDate.year, () => TriggerHypnotism());
		}
//        if (this.king.importantCharacterValues.ContainsKey(CHARACTER_VALUE.INFLUENCE)) {
//            if (GameManager.Instance.days == GameManager.daysInMonth[GameManager.Instance.month]) {
//                List<GameEvent> previousHypnotismEvents = EventManager.Instance.GetEventsStartedByKingdom(this, new EVENT_TYPES[] { EVENT_TYPES.HYPNOTISM }, false);
//                if (previousHypnotismEvents.Where(x => x.startYear == GameManager.Instance.year).Count() <= 0) {
//                    List<Kingdom> notFriends = new List<Kingdom>();
//                    for (int i = 0; i < discoveredKingdoms.Count; i++) {
//                        Kingdom currKingdom = discoveredKingdoms[i];
//                        KingdomRelationship rel = currKingdom.king.GetRelationshipWithKingdom(this.king);
//                        if (rel.relationshipStatus != RELATIONSHIP_STATUS.FRIEND && rel.relationshipStatus != RELATIONSHIP_STATUS.ALLY) {
//                            notFriends.Add(currKingdom);
//                        }
//                    }
//                    if (UnityEngine.Random.Range(0, 100) < 10 && notFriends.Count > 0) {
//                        EventCreator.Instance.CreateHypnotismEvent(this, notFriends[UnityEngine.Random.Range(0, notFriends.Count)]);
//                    }
//                }
//            }
//        }
    }
    #endregion

    #region Kingdom Holiday
    private void TriggerKingdomHoliday() {
        if (this.king.importantCharacterValues.ContainsKey(CHARACTER_VALUE.TRADITION)) {
            if (Utilities.IsCurrentDayMultipleOf(15)) {
//                List<GameEvent> activeHolidays = EventManager.Instance.GetEventsStartedByKingdom(this, new EVENT_TYPES[] { EVENT_TYPES.KINGDOM_HOLIDAY });
//                List<GameEvent> activeWars = EventManager.Instance.GetAllEventsKingdomIsInvolvedIn(this, new EVENT_TYPES[] { EVENT_TYPES.KINGDOM_WAR });
                if(!HasActiveEvent(EVENT_TYPES.KINGDOM_HOLIDAY) && !HasActiveEvent(EVENT_TYPES.KINGDOM_WAR)) { //There can only be 1 active holiday per kingdom at a time. && Kingdoms that are at war, cannot celebrate holidays.
                    if (UnityEngine.Random.Range(0, 100) < 10) {
                        if(UnityEngine.Random.Range(0, 100) < 50) {
                            //Celebrate Holiday
                            EventCreator.Instance.CreateKingdomHolidayEvent(this);
                        } else {
                            //If a king chooses not to celebrate the holiday, his governors that value TRADITION will decrease loyalty by 20.
                            for (int i = 0; i < cities.Count; i++) {
                                Governor currGovernor = (Governor)cities[i].governor.assignedRole;
                                if (currGovernor.citizen.importantCharacterValues.ContainsKey(CHARACTER_VALUE.TRADITION)) {
									currGovernor.AddEventModifier(-5, "Did not celebrate holiday", null);
                                }
                            }
                            if (_importantCharacterValues.ContainsKey(CHARACTER_VALUE.TRADITION)) {
                                AdjustUnrest(10);
                            }
                        }
                    }
                }
            }
        }
    }
    #endregion

    #region Develop Weapons
    private int _weaponsCount;
    public int weaponsCount {
        get { return _weaponsCount; }
    }
    internal void AdjustWeaponsCount(int adjustment) {
        _weaponsCount += adjustment;
    }
    //protected void TriggerDevelopWeapons() {
    //    if (this.king.importantCharacterValues.ContainsKey(CHARACTER_VALUE.STRENGTH)) {
    //        if (Utilities.IsCurrentDayMultipleOf(5)) {
    //            if (UnityEngine.Random.Range(0, 100) < 10) {
    //                if (EventManager.Instance.GetEventsStartedByKingdom(this, new EVENT_TYPES[] { EVENT_TYPES.DEVELOP_WEAPONS }).Count <= 0) {
    //                    //EventCreator.Instance.CreateDevelopWeaponsEvent(this);
    //                }
    //            }
    //        }
    //    }
    //}
    #endregion

    #region Kings Council
    protected void TriggerKingsCouncil() {
		if(!this.isDead){
			if(this.king.importantCharacterValues.ContainsKey(CHARACTER_VALUE.LIBERTY) || this.king.importantCharacterValues.ContainsKey(CHARACTER_VALUE.PEACE)) {
				if (UnityEngine.Random.Range(0, 100) < 2) {
					if (discoveredKingdoms.Count > 2 && !HasActiveEvent(EVENT_TYPES.KINGDOM_WAR) && !HasActiveEvent(EVENT_TYPES.KINGS_COUNCIL)) {
						EventCreator.Instance.CreateKingsCouncilEvent(this);
					}
				}
			}
			GameDate gameDate = new GameDate (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
			gameDate.AddMonths (1);
			gameDate.day = GameManager.daysInMonth [gameDate.month];
			SchedulingManager.Instance.AddEntry (gameDate.month, gameDate.day, gameDate.year, () => TriggerKingsCouncil());
		}
//        if(this.king.importantCharacterValues.ContainsKey(CHARACTER_VALUE.LIBERTY) || this.king.importantCharacterValues.ContainsKey(CHARACTER_VALUE.PEACE)) {
//            if (GameManager.Instance.days == GameManager.daysInMonth[GameManager.Instance.month]) {
//                if (UnityEngine.Random.Range(0, 100) < 2) {
//                    if (discoveredKingdoms.Count > 2 && EventManager.Instance.GetEventsStartedByKingdom(this, new EVENT_TYPES[] { EVENT_TYPES.KINGDOM_WAR, EVENT_TYPES.KINGS_COUNCIL }).Count <= 0) {
//                        EventCreator.Instance.CreateKingsCouncilEvent(this);
//                    }
//                }
//            }
//        }
    }
    #endregion

	#region Serum of Alacrity
	internal void AdjustSerumOfAlacrity(int amount){
		this._serumsOfAlacrity += amount;
		if(this._serumsOfAlacrity < 0){
			this._serumsOfAlacrity = 0;
		}
	}
	#endregion

	#region Hidden History Book
	internal void SetUpheldHiddenHistoryBook(bool state){
		this._hasUpheldHiddenHistoryBook = state;
	}
	#endregion

    #region Fog Of War
    internal void SetFogOfWarStateForTile(HexTile tile, FOG_OF_WAR_STATE fowState, bool isForcedUpdate = false) {
        FOG_OF_WAR_STATE previousStateOfTile = _fogOfWar[tile.xCoordinate, tile.yCoordinate];
        _fogOfWarDict[previousStateOfTile].Remove(tile);

        _fogOfWar[tile.xCoordinate, tile.yCoordinate] = fowState;
        if (!_fogOfWarDict[fowState].Contains(tile)) {
            _fogOfWarDict[fowState].Add(tile);
        }

        if (UIManager.Instance.currentlyShowingKingdom != null && UIManager.Instance.currentlyShowingKingdom.id == this.id) {
            UpdateFogOfWarVisualForTile(tile, fowState);
        }

        int sum = _fogOfWarDict.Sum(x => x.Value.Count);
        if (sum != GridMap.Instance.listHexes.Count) {
            throw new Exception("Fog of war dictionary is no longer accurate!");
        }
    }
    internal void UpdateFogOfWarVisual() {
        for (int x = 0; x < fogOfWar.GetLength(0); x++) {
            for (int y = 0; y < fogOfWar.GetLength(1); y++) {
                FOG_OF_WAR_STATE fowStateToUse = fogOfWar[x, y];
                HexTile currHexTile = GridMap.Instance.map[x, y];
                UpdateFogOfWarVisualForTile(currHexTile, fowStateToUse);
            }
        }
    }

    private void UpdateFogOfWarVisualForTile(HexTile hexTile, FOG_OF_WAR_STATE fowState) {
        hexTile.SetFogOfWarState(fowState);
    }

	internal FOG_OF_WAR_STATE GetFogOfWarStateOfTile(HexTile hexTile){
		return this._fogOfWar [hexTile.xCoordinate, hexTile.yCoordinate];
	}
    #endregion

	#region Altar of Blessing
	internal void CollectAltarOfBlessing(BoonOfPower boonOfPower){
		Debug.Log (this.name + " HAS COLLECTED A BOON OF POWER!");
		this._boonOfPowers.Add (boonOfPower);
		boonOfPower.AddOwnership (this);
	}
	#endregion
	internal int GetNumberOfWars(){
		int numOfWars = 0;
		for (int i = 0; i < relationships.Count; i++) {
			if(relationships.ElementAt(i).Value.isAtWar){
				numOfWars += 1;
			}
		}
		if(numOfWars > 0){
			numOfWars -= 1;
		}
		return numOfWars;
	}

//	private void UpdateCombatStats(){
//		this._combatStats = this._kingdomTypeData.combatStats;
//		this._combatStats.waves = this._kingdomTypeData.combatStats.waves - (GetNumberOfWars() + this.rebellions.Count);
//	}

	internal void SetLockDown(bool state){
		this._isLockedDown = state;
	}

	internal void SetTechProduction(bool state){
		this._isTechProducing = state;
	}
	internal void SetTechProductionPercentage(float amount){
		this._techProductionPercentage = amount;
	}
	internal void SetProductionGrowthPercentage(float amount){
		this._productionGrowthPercentage = amount;
	}
	internal void SetSecession(bool state){
		this._hasSecession = state;
	}
	internal void SetRiot(bool state){
		this._hasRiot = state;
	}

	#region Crimes
	private void NewRandomCrimeDate(bool isFirst = false){
		int month = 0;
		int day = 0;
		if(isFirst){
			month = UnityEngine.Random.Range (1, 5);
			day = UnityEngine.Random.Range (1, GameManager.daysInMonth [month] + 1);
		}else{
			int lowerBoundMonth = this._crimeDate.month + 3;
			int upperBoundMonth = lowerBoundMonth + 1;

			month = UnityEngine.Random.Range (lowerBoundMonth, upperBoundMonth + 1);
			if(month > 12){
				month -= 12;
			}
			day = UnityEngine.Random.Range (1, GameManager.daysInMonth [month] + 1);
		}
		this._crimeDate.month = month;
		this._crimeDate.day = day;
	} 

	private void TriggerCrime(){
		if(!this.isDead){
			CreateCrime ();
			GameDate gameDate = new GameDate (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
			gameDate.AddMonths (3);
			int chance = UnityEngine.Random.Range (0, 2);
			if(chance == 0){
				gameDate.AddMonths (1);
			}
			gameDate.day = UnityEngine.Random.Range (1, GameManager.daysInMonth [gameDate.month]);

			SchedulingManager.Instance.AddEntry (gameDate.month, gameDate.day, gameDate.year, () => TriggerCrime());
		}
//		if(GameManager.Instance.month == this._crimeDate.month && GameManager.Instance.days == this._crimeDate.day){
//			NewRandomCrimeDate ();
//			CreateCrime ();
//		}
	}

	private void CreateCrime(){
		CrimeData crimeData = CrimeEvents.Instance.GetRandomCrime ();
		EventCreator.Instance.CreateCrimeEvent (this, crimeData);
	}
	#endregion

	internal void AddActiveEvent(GameEvent gameEvent){
		this.activeEvents.Add (gameEvent);
	}
	internal void RemoveActiveEvent(GameEvent gameEvent){
		this.activeEvents.Remove (gameEvent);
		AddToDoneEvents (gameEvent);
	}
	internal void AddToDoneEvents(GameEvent gameEvent){
		this.doneEvents.Add (gameEvent);
		if(this.doneEvents.Count > KingdomManager.Instance.maxKingdomEventHistory){
			this.doneEvents.RemoveAt (0);
		}
	}
	internal bool HasActiveEvent(EVENT_TYPES eventType){
		for (int i = 0; i < this.activeEvents.Count; i++) {
			if(this.activeEvents[i].eventType == eventType){
				return true;
			}
		}
		return false;
	}
	internal int GetActiveEventsOfTypeCount(EVENT_TYPES eventType){
		int count = 0;
		for (int i = 0; i < this.activeEvents.Count; i++) {
			if(this.activeEvents[i].eventType == eventType){
				count += 1;
			}
		}
		return count;
	}
	internal List<GameEvent> GetEventsOfType(EVENT_TYPES eventType, bool isActiveOnly = true){
		List<GameEvent> gameEvents = new List<GameEvent> ();
		for (int i = 0; i < this.activeEvents.Count; i++) {
			if(this.activeEvents[i].eventType == eventType){
				gameEvents.Add (this.activeEvents [i]);
			}
		}
		if(!isActiveOnly){
			for (int i = 0; i < this.doneEvents.Count; i++) {
				if(this.doneEvents[i].eventType == eventType){
					gameEvents.Add (this.doneEvents [i]);
				}
			}
		}
		return gameEvents;
	}
//	internal bool HasActiveEventWith(EVENT_TYPES eventType, Kingdom kingdom){
//		for (int i = 0; i < this.activeEvents.Count; i++) {
//			if(this.activeEvents[i].eventType == eventType){
//				return true;
//			}
//		}
//		return false;
//	}


	#region Governors Loyalty/Opinion
	internal void HasConflicted(GameEvent gameEvent){
		for(int i = 0; i < this.cities.Count; i++){
			if(this.cities[i].governor != null){
				((Governor)this.cities[i].governor.assignedRole).AddEventModifier (-10, "Recent border conflict", gameEvent);
			}
		}
	}

	internal void UpdateAllGovernorsLoyalty(){
		for(int i = 0; i < this.cities.Count; i++){
			if(this.cities[i].governor != null){
				((Governor)this.cities[i].governor.assignedRole).UpdateLoyalty();
			}
		}
	}
	#endregion

	internal void CheckSharedBorders(){
		bool isSharingBorderNow = false;
		for (int i = 0; i < relationships.Count; i++) {
            KingdomRelationship currRel = relationships.ElementAt(i).Value;
            isSharingBorderNow = KingdomManager.Instance.IsSharingBorders (this, currRel.targetKingdom);
			if (isSharingBorderNow != currRel.isSharingBorder) {
                currRel.SetBorderSharing (isSharingBorderNow);
				KingdomRelationship rel2 = currRel.targetKingdom.GetRelationshipWithKingdom(this);
				rel2.SetBorderSharing (isSharingBorderNow);
			}
		}
	}

	#region Balance of Power
	internal void Militarize(bool state){
		this._isMilitarize = state;
	}

	private void ScheduleActionDay(){
		KingdomManager.Instance.IncrementCurrentActionDay (2);
		SchedulingManager.Instance.AddEntry (GameManager.Instance.month, KingdomManager.Instance.currentActionDay, GameManager.Instance.year, () => ActionDay ());
	}
	private void ActionDay(){
		if(!this.isDead){
			this._mainThreat = GetMainThreat ();
			if(this._mainThreat != null){
				//has main threat
				if (this.kingdomTypeData.purpose == PURPOSE.BALANCE) {
					SeeksBalance ();
				}else if (this.kingdomTypeData.purpose == PURPOSE.BANDWAGON) {
					SeeksBandwagon ();
				}else if (this.kingdomTypeData.purpose == PURPOSE.BUCKPASS) {
					SeeksBuckpass ();
				}else if (this.kingdomTypeData.purpose == PURPOSE.SUPERIORITY) {
					SeeksSuperiority ();
				}
			}else{
				//no main threat
			}

			GameDate gameDate;
			gameDate.month = GameManager.Instance.month;
			gameDate.day = GameManager.Instance.days;
			gameDate.year = GameManager.Instance.year;

			gameDate.AddMonths (1);
			SchedulingManager.Instance.AddEntry (gameDate.month, gameDate.day, gameDate.year, () => ActionDay ());

		}
	}
	private Kingdom GetMainThreat(){
		Kingdom threat = null;
		return threat;
	}
	private void SeeksBalance(){
		
	}
	private void SeeksBandwagon(){

	}
	private void SeeksBuckpass(){

	}
	private void SeeksSuperiority(){

	}
	#endregion
}
