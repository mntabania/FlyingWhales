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
    private int _population;
    private int _populationCapacity;
    private int foundationYear;
    private int foundationMonth;
    private int foundationDay;

	private KingdomTypeData _kingdomTypeData;
    private KINGDOM_SIZE _kingdomSize;
	private Kingdom _sourceKingdom;
	private Kingdom _mainThreat;
	private int _actionDay;

    //Resources
    private Dictionary<RESOURCE, int> _availableResources; //only includes resources that the kingdom has bought via tile purchasing
    internal BASE_RESOURCE_TYPE basicResource;

    //Trading
    private Dictionary<Kingdom, EMBARGO_REASON> _embargoList;

    private int _baseArmor;
    private int _baseWeapons;
    private int _baseStability;
    private int _stability;
    private List<City> _cities;
    private List<Region> _regions;
	private List<City> _nonRebellingCities;
	private List<Camp> camps;
	internal City capitalCity;
	internal Citizen king;
    internal Citizen nextInLine;
	internal List<Citizen> successionLine;

	internal List<Rebellions> rebellions;

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
    private Dictionary<Region, FOG_OF_WAR_STATE> _regionFogOfWarDict;

    //Crimes
    private CrimeData _crimeData;
	private CrimeDate _crimeDate;

	//Events of Kingdom
	private List<GameEvent> _activeEvents;
	private List<GameEvent> _doneEvents;

    //Expansion
    private int _expansionRate;

	//Balance of Power
//	private int _effectivePower;
//	private int _effectiveDefense;
	private bool _isMobilizing;
	private int _militaryAlliancePower;
	private int _mutualDefenseTreatyPower;
	[NonSerialized] private List<Kingdom> _militaryAlliances;
    [NonSerialized] private List<Kingdom> _mutualDefenseTreaties;
    [NonSerialized] private List<Kingdom> _adjacentKingdoms;
//	[NonSerialized] private List<Kingdom> _allianceKingdoms;
	private GameDate _currentDefenseTreatyRejectionDate;
	private GameDate _currentMilitaryAllianceRejectionDate;
	private List<Wars> _mobilizationQueue;

    //protected Dictionary<CHARACTER_VALUE, int> _dictCharacterValues;
    //protected Dictionary<CHARACTER_VALUE, int> _importantCharacterValues;

    protected const int INCREASE_CITY_HP_CHANCE = 5;
	protected const int INCREASE_CITY_HP_AMOUNT = 20;
    protected const int GOLD_GAINED_FROM_TRADE = 10;
    protected const int UNREST_DECREASE_PER_MONTH = -5;
    protected const int STABILITY_DECREASE_CONQUER = -5;
    protected const int STABILITY_DECREASE_EMBARGO = -5;

	private bool _isDead;
	private bool _hasBioWeapon;
	private bool _isLockedDown;
	private bool _isTechProducing;
	private bool _isMilitarize;
	private bool _isFortifying;

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

	//Kingdom Threat
	private int _warmongerValue;

	//Alliance
	private AlliancePool _alliancePool;

	//Warfare
	private Dictionary<int, WarfareInfo> _warfareInfo;

    private float _researchRateFromKing;
    private float _draftRateFromKing;
    private float _productionRateFromKing;

    private int stabilityDecreaseFromInvasionCounter;
    internal List<GameDate> datesStabilityDecreaseWillExpire = new List<GameDate>(); //TODO Remove this when testing is done

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
    internal KINGDOM_SIZE kingdomSize {
        get { return _kingdomSize; }
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
    internal int population {
        get { return _population; }
    }
    internal int populationCapacity {
        get { return _populationCapacity; }
    }
    public int cityCap {
        get { return Mathf.FloorToInt(prestige / GridMap.Instance.numOfRegions); }
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
    internal List<Region> regions {
        get { return _regions; }
    }
//	public List<Camp> camps{
//		get{ return this._camps; }
//	}
    public int stability {
        get { return this._stability; }
        //get { return -100; }
        //		set { this._stability = value;}
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
    public int expansionRate {
        get { return _expansionRate; }
    }
    //public Dictionary<CHARACTER_VALUE, int> dictCharacterValues {
    //    get { return this._dictCharacterValues; }
    //}
    //public Dictionary<CHARACTER_VALUE, int> importantCharacterValues {
    //    get { return this._importantCharacterValues; }
    //}
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
		get { return GetNonRebellingCities(); }
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
    internal Dictionary<Region, FOG_OF_WAR_STATE> regionFogOfWarDict {
        get { return _regionFogOfWarDict; }
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
	public bool isFortifying{
		get { return this._isFortifying;}
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
	public int baseWeapons{
		get { return _baseWeapons;}
	}
	public int baseArmor{
		get { return _baseArmor;}
	}
//	public int effectiveWeapons{
//		get { return this.effectiveAttack + (int)(this.effectiveDefense / 3) + (int)(GetPosAllianceWeapons() / 3);}
//	}
//	public int effectiveArmor{
//		get { return this.effectiveDefense + (int)(this.effectiveAttack / 3) + (int)(GetPosAllianceArmor() / 3);}
//	}
	public int militaryAlliancePower{
		get { return this._militaryAlliancePower;}
	}
	public int mutualDefenseTreatyPower{
		get { return this._mutualDefenseTreatyPower;}
	}
	public List<Kingdom> militaryAlliances{
		get { return this._militaryAlliances;}
	}
	public List<Kingdom> mutualDefenseTreaties{
		get { return this._mutualDefenseTreaties;}
	}
	public List<Kingdom> adjacentKingdoms{
        get { return _adjacentKingdoms; }
    }
//	public List<Kingdom> allianceKingdoms{
//		get { return this._allianceKingdoms;}
//	}
	public bool isMobilizing{
		get { return this._isMobilizing;}
	}
	public float techProductionPercentage{
		get { return this._techProductionPercentage;}
	}
	public int actionDay{
		get { return this._actionDay;}
	}
	public int warmongerValue{
		get { return this._warmongerValue;}
	}
	public AlliancePool alliancePool{
		get { return this._alliancePool;}
	}
	public Dictionary<int, WarfareInfo> warfareInfo{
		get { return this._warfareInfo;}
	}
    internal int scientists {
        get { return Mathf.Max(1, Mathf.FloorToInt(population * researchRate)); }
    }
    internal int soldiers {
        get { return Mathf.Max(1, Mathf.FloorToInt(population * draftRate)); }
    }
    internal int workers {
        get { return Mathf.Max(1, Mathf.FloorToInt(population * productionRate)); }
    }
    internal float draftRate {
        get {
            if (this._kingdomTypeData == null) {
                return 0f;
            }
            return this._kingdomTypeData.populationRates.draftRate + _draftRateFromKing;
        }
    }
    internal float researchRate {
        get {
            if (this._kingdomTypeData == null) {
                return 0f;
            }
            return this._kingdomTypeData.populationRates.researchRate + _researchRateFromKing;
        }
    }
    internal float productionRate {
        get {
            if (this._kingdomTypeData == null) {
                return 0f;
            }
            return this._kingdomTypeData.populationRates.productionRate + _productionRateFromKing;
        }
    }
	internal int effectiveAttack{
		get{ 
			int mySoldiers = this.soldiers;
			return (int)((2 * mySoldiers * this._baseWeapons) / (mySoldiers + this._baseWeapons));
		}
	}
	internal int effectiveDefense{
		get{ 
			int mySoldiers = this.soldiers;
			return (int)((2 * mySoldiers * this._baseArmor) / (mySoldiers + this._baseArmor));
		}
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
		this.name = RandomNameGenerator.Instance.GenerateKingdomName(this.race);
		this.king = null;
        this.nextInLine = null;
        this._kingdomSize = KINGDOM_SIZE.SMALL;
        this._mainThreat = null;
        this.successionLine = new List<Citizen>();
		this._cities = new List<City> ();
        this._regions = new List<Region>();
		this._nonRebellingCities = new List<City> ();
		this.camps = new List<Camp> ();
		this.kingdomHistory = new List<History>();
		this.kingdomColor = Utilities.GetColorForKingdom();
		this._availableResources = new Dictionary<RESOURCE, int> ();
		this.relationships = new Dictionary<Kingdom, KingdomRelationship>();
		this._isDead = false;
		this._isLockedDown = false;
		this._isMilitarize = false;
		this._isFortifying = false;
		this._hasUpheldHiddenHistoryBook = false;
        this._embargoList = new Dictionary<Kingdom, EMBARGO_REASON>();
        this._stability = 0;
		this._sourceKingdom = sourceKingdom;
		this.borderConflictLoyaltyExpiration = 0;
		this.rebellions = new List<Rebellions> ();
		this._discoveredKingdoms = new List<Kingdom>();
		this._techLevel = 0;
		this._techCounter = 0;
		this._hasBioWeapon = false;
		this._boonOfPowers = new List<BoonOfPower> ();
		this._activatedBoonOfPowers = new List<BoonOfPower> ();
		this.plague = null;
        this.age = 0;
        this.foundationYear = GameManager.Instance.year;
        this.foundationDay = GameManager.Instance.days;
        this.foundationMonth = GameManager.Instance.month;
        //this._dictCharacterValues = new Dictionary<CHARACTER_VALUE, int>();
        //this._importantCharacterValues = new Dictionary<CHARACTER_VALUE, int>();

        //Fog Of War
        this._fogOfWar = new FOG_OF_WAR_STATE[(int)GridMap.Instance.width, (int)GridMap.Instance.height];
		this._fogOfWarDict = new Dictionary<FOG_OF_WAR_STATE, HashSet<HexTile>>();
		this._fogOfWarDict.Add(FOG_OF_WAR_STATE.HIDDEN, new HashSet<HexTile>(GridMap.Instance.listHexes.Select(x => x.GetComponent<HexTile>())));
		this._fogOfWarDict.Add(FOG_OF_WAR_STATE.SEEN, new HashSet<HexTile>());
		this._fogOfWarDict.Add(FOG_OF_WAR_STATE.VISIBLE, new HashSet<HexTile>());
        this._regionFogOfWarDict = new Dictionary<Region, FOG_OF_WAR_STATE>();
        for (int i = 0; i < GridMap.Instance.allRegions.Count; i++) {
            _regionFogOfWarDict.Add(GridMap.Instance.allRegions[i], FOG_OF_WAR_STATE.HIDDEN);
        }

        this._activeEvents = new List<GameEvent> ();
		this._doneEvents = new List<GameEvent> ();
		this.orderedMaleRoyalties = new List<Citizen> ();
		this.orderedFemaleRoyalties = new List<Citizen> ();
		this.orderedBrotherRoyalties = new List<Citizen> ();
		this.orderedSisterRoyalties = new List<Citizen> ();
		this._militaryAlliances = new List<Kingdom> ();
		this._mutualDefenseTreaties = new List<Kingdom> ();
		this._adjacentKingdoms = new List<Kingdom> ();
		this._currentDefenseTreatyRejectionDate = new GameDate (0, 0, 0);
		this._currentMilitaryAllianceRejectionDate = new GameDate (0, 0, 0);
		this._mobilizationQueue = new List<Wars> ();
		this._actionDay = 0;
		this._alliancePool = null;
		this._warfareInfo = new Dictionary<int, WarfareInfo>();
        this.stabilityDecreaseFromInvasionCounter = 0;
        AdjustPrestige(GridMap.Instance.numOfRegions);
        //		AdjustPrestige(500);


        AdjustPopulation(50);
        AdjustStability(50);
        AdjustBaseWeapons(25);
        AdjustBaseArmors(25);
        SetGrowthState(true);
        //this.GenerateKingdomCharacterValues();
        this.SetLockDown(false);
		this.SetTechProduction(true);
		this.SetTechProductionPercentage(1f);
		this.SetProductionGrowthPercentage(1f);
		this.UpdateTechCapacity ();
		this.SetSecession (false);
		this.SetRiot (false);
		this.SetWarmongerValue (15);
//		this.NewRandomCrimeDate (true);
		// Determine what type of Kingdom this will be upon initialization.
		this._kingdomTypeData = null;
        SetKingdomType(StoryTellingManager.Instance.GetRandomKingdomTypeForKingdom());
		//this.UpdateKingdomTypeData();

        this.basicResource = Utilities.GetBasicResourceForRace(race);

		Messenger.AddListener<Kingdom>("OnNewKingdomCreated", CreateNewRelationshipWithKingdom);
		//Messenger.AddListener("OnDayEnd", KingdomTickActions);
        Messenger.AddListener<Kingdom>("OnKingdomDied", OtherKingdomDiedActions);

		SchedulingManager.Instance.AddEntry (GameManager.Instance.month, GameManager.Instance.days, (GameManager.Instance.year + 1), () => AttemptToAge());
        //SchedulingManager.Instance.AddEntry (GameManager.Instance.month, GameManager.daysInMonth[GameManager.Instance.month], GameManager.Instance.year, () => DecreaseUnrestEveryMonth());
        SchedulingManager.Instance.AddEntry(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, () => IncreaseExpansionRatePerMonth());
        SchedulingManager.Instance.AddEntry(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, () => IncreaseBOPAttributesPerMonth());
        //SchedulingManager.Instance.AddEntry (GameManager.Instance.month, GameManager.daysInMonth[GameManager.Instance.month], GameManager.Instance.year, () => MonthlyPrestigeActions());
        //SchedulingManager.Instance.AddEntry (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, () => AdaptToKingValues());
        SchedulingManager.Instance.AddEntry(GameManager.Instance.month, 1, GameManager.Instance.year, () => IncreasePopulationEveryMonth());
        SchedulingManager.Instance.AddEntry (1, 1, GameManager.Instance.year + 1, () => WarmongerDecreasePerYear ());
        //		ScheduleEvents ();
        ScheduleOddDayActions();
        ScheduleActionDay();


        this.kingdomHistory.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "This kingdom was born.", HISTORY_IDENTIFIER.NONE));
	}

    public void CreateInitialCities(List<HexTile> initialCityLocations) {
        if (initialCityLocations.Count > 0) {
            for (int i = 0; i < initialCityLocations.Count; i++) {
                HexTile initialCityLocation = initialCityLocations[i];
                City newCity = this.CreateNewCityOnTileForKingdom(initialCityLocation);
                initialCityLocation.region.SetOccupant(newCity);
            }
        }
    }

    public void SetKingdomType(KINGDOM_TYPE kingdomType) {
        KINGDOM_TYPE prevKingdomType = this.kingdomType;
        switch (kingdomType) {
            case KINGDOM_TYPE.DEFENSIVE_KINGDOM:
                this._kingdomTypeData = KingdomManager.Instance.kingdomTypeNoble;
                break;
            case KINGDOM_TYPE.OFFENSIVE_KINGDOM:
                this._kingdomTypeData = KingdomManager.Instance.kingdomTypeEvil;
                break;
            case KINGDOM_TYPE.SCIENTIFIC_KINGDOM:
                this._kingdomTypeData = KingdomManager.Instance.kingdomTypeMerchant;
                break;
            case KINGDOM_TYPE.BALANCED_KINGDOM:
                this._kingdomTypeData = KingdomManager.Instance.kingdomTypeChaotic;
                break;
        }

        if (this.kingdomTypeData.dailyCumulativeEventRate != null) {
            this._dailyCumulativeEventRate = this._kingdomTypeData.dailyCumulativeEventRate;
        }

        // If the Kingdom Type Data changed
        if (prevKingdomType != this.kingdomType) {
            //Update Character Values of King and Governors
            //this.UpdateCharacterValuesOfKingsAndGovernors();

            //Update Relationship Opinion
            UpdateAllRelationshipsLikenessFromOthers();

            //if(prevKingdomType != KINGDOM_TYPE.NONE) {
            //    Log updateKingdomTypeLog = new Log(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "General", "Kingdom", "change_kingdom_type");
            //    updateKingdomTypeLog.AddToFillers(this, this.name, LOG_IDENTIFIER.KINGDOM_1);
            //    updateKingdomTypeLog.AddToFillers(null, Utilities.NormalizeString(this.kingdomType.ToString()), LOG_IDENTIFIER.OTHER);
            //    UIManager.Instance.ShowNotification(updateKingdomTypeLog);
            //}
        }
    }

	//// Updates this kingdom's type and horoscope
	//public void UpdateKingdomTypeData() {
	//	// Update Kingdom Type whenever the kingdom expands to a new city
	//	KingdomTypeData prevKingdomTypeData = this._kingdomTypeData;
	//	this._kingdomTypeData = StoryTellingManager.Instance.InitializeKingdomType (this);
	//	if(this.kingdomTypeData.dailyCumulativeEventRate != null){
	//		this._dailyCumulativeEventRate = this._kingdomTypeData.dailyCumulativeEventRate;
	//	}
	//	// If the Kingdom Type Data changed
	//	if (this._kingdomTypeData != prevKingdomTypeData) {
 //           //Update Character Values of King and Governors
 //           //this.UpdateCharacterValuesOfKingsAndGovernors();

	//		//Update Relationship Opinion
	//		UpdateAllRelationshipsLikeness();
	//		UpdateAllRelationshipsLikenessFromOthers ();
 //           //if (UIManager.Instance.currentlyShowingKingdom != null &&UIManager.Instance.currentlyShowingKingdom.id == this.id) {
 //               Log updateKingdomTypeLog = new Log(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "General", "Kingdom", "change_kingdom_type");
 //               updateKingdomTypeLog.AddToFillers(this, this.name, LOG_IDENTIFIER.KINGDOM_1);
 //               updateKingdomTypeLog.AddToFillers(null, Utilities.NormalizeString(this.kingdomType.ToString()), LOG_IDENTIFIER.OTHER);
 //               UIManager.Instance.ShowNotification(updateKingdomTypeLog);
 //           //}
 //       }
 //   }

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
		if(this.alliancePool != null){
			LeaveAlliance (true);
		}
      	ResolveWars();
        Messenger.RemoveListener<Kingdom>("OnNewKingdomCreated", CreateNewRelationshipWithKingdom);
        //Messenger.RemoveListener("OnDayEnd", KingdomTickActions);
        Messenger.RemoveListener<Kingdom>("OnKingdomDied", OtherKingdomDiedActions);

        Messenger.Broadcast<Kingdom>("OnKingdomDied", this);

        this.DeleteRelationships();
        KingdomManager.Instance.RemoveKingdom(this);
        Log newLog = new Log(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "General", "Kingdom", "obliterated");
        string yearsLasted = string.Empty;
        if (age == 1) {
            yearsLasted = "a year";
        } else if (age <= 0) {
            yearsLasted = "less than a year";
        } else {
            yearsLasted = age.ToString() + " years";
        }
        newLog.AddToFillers(this, this.name, LOG_IDENTIFIER.KINGDOM_1);
        newLog.AddToFillers(null, yearsLasted, LOG_IDENTIFIER.OTHER);
        UIManager.Instance.ShowNotification(newLog);

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
			if (rel.isAtWar) {
				rel.ChangeWarStatus (false, null);
			}
		}
		List<WarfareInfo> warfareInfos = this._warfareInfo.Values.ToList ();
		for (int i = 0; i < warfareInfos.Count; i++) {
			warfareInfos [i].warfare.UnjoinWar (this);
		}
    }
    protected void OtherKingdomDiedActions(Kingdom kingdomThatDied) {
        if (kingdomThatDied.id != this.id) {
            RemoveRelationshipWithKingdom(kingdomThatDied);
            RemoveKingdomFromDiscoveredKingdoms(kingdomThatDied);
            RemoveKingdomFromEmbargoList(kingdomThatDied);
            RemoveAdjacentKingdom(kingdomThatDied);
            if (_mainThreat != null && _mainThreat == kingdomThatDied) {
                _mainThreat = null;
            }
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
        if (relationship.relationshipStatus == RELATIONSHIP_STATUS.SPITE) {
            value = 5;
        } else if (relationship.relationshipStatus == RELATIONSHIP_STATUS.HATE) {
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
//    internal void UpdateMutualRelationships() {
//		foreach (KingdomRelationship currRel in relationships.Values) {
////        for (int i = 0; i < relationships.Count; i++) {
////            KingdomRelationship currRel = relationships.Values.ElementAt(i);
//            Kingdom targetKingdom = currRel.targetKingdom;
//            KingdomRelationship targetKingdomRel = targetKingdom.GetRelationshipWithKingdom(this);

//            if (targetKingdomRel == null || currRel == null) {
//                return;
//            }

//            currRel.ResetMutualRelationshipModifier();
//            targetKingdomRel.ResetMutualRelationshipModifier();

//			List<Kingdom> sourceKingRelationships = GetKingdomsByRelationship (new
//           [] { RELATIONSHIP_STATUS.HATE, RELATIONSHIP_STATUS.SPITE,
//				RELATIONSHIP_STATUS.AFFECTIONATE, RELATIONSHIP_STATUS.LOVE
//			}, targetKingdom);

//			List<Kingdom> targetKingRelationships = targetKingdom.GetKingdomsByRelationship (new
//                [] { RELATIONSHIP_STATUS.HATE, RELATIONSHIP_STATUS.SPITE,
//				RELATIONSHIP_STATUS.AFFECTIONATE, RELATIONSHIP_STATUS.LOVE
//			}, this);

////            List<Kingdom> kingdomsInCommon = sourceKingRelationships.Intersect(targetKingRelationships).ToList();
//			foreach (var currKingdom in sourceKingRelationships.Intersect(targetKingRelationships)) {
////            for (int j = 0; j < kingdomsInCommon.Count; j++) {
////                Kingdom currKingdom = kingdomsInCommon[j];
//                KingdomRelationship relSourceKingdom = this.GetRelationshipWithKingdom(currKingdom);
//                KingdomRelationship relTargetKingdom = targetKingdom.GetRelationshipWithKingdom(currKingdom);

//                if (relSourceKingdom.relationshipStatus == RELATIONSHIP_STATUS.HATE) {
//                    if (relTargetKingdom.relationshipStatus == RELATIONSHIP_STATUS.HATE ||
//                        relTargetKingdom.relationshipStatus == RELATIONSHIP_STATUS.SPITE) {
//                        currRel.AddMutualRelationshipModifier(5);
//                        targetKingdomRel.AddMutualRelationshipModifier(5);
//                    }
//                } else if (relSourceKingdom.relationshipStatus == RELATIONSHIP_STATUS.SPITE) {
//                    if (relTargetKingdom.relationshipStatus == RELATIONSHIP_STATUS.HATE) {
//                        currRel.AddMutualRelationshipModifier(5);
//                    } else if (relTargetKingdom.relationshipStatus == RELATIONSHIP_STATUS.SPITE) {
//                        targetKingdomRel.AddMutualRelationshipModifier(10);
//                    }
//                } else if (relSourceKingdom.relationshipStatus == RELATIONSHIP_STATUS.AFFECTIONATE) {
//                    if (relTargetKingdom.relationshipStatus == RELATIONSHIP_STATUS.AFFECTIONATE ||
//                        relTargetKingdom.relationshipStatus == RELATIONSHIP_STATUS.LOVE) {
//                        currRel.AddMutualRelationshipModifier(5);
//                        targetKingdomRel.AddMutualRelationshipModifier(5);
//                    }
//                } else if (relSourceKingdom.relationshipStatus == RELATIONSHIP_STATUS.LOVE) {
//                    if (relTargetKingdom.relationshipStatus == RELATIONSHIP_STATUS.AFFECTIONATE) {
//                        currRel.AddMutualRelationshipModifier(5);
//                        targetKingdomRel.AddMutualRelationshipModifier(5);
//                    } else if (relTargetKingdom.relationshipStatus == RELATIONSHIP_STATUS.LOVE) {
//                        currRel.AddMutualRelationshipModifier(10);
//                        targetKingdomRel.AddMutualRelationshipModifier(10);
//                    }
//                }
//            }
//        }
//    }
    internal void ResetRelationshipModifiers() {
        for (int i = 0; i < relationships.Count; i++) {
            KingdomRelationship currRel = relationships.ElementAt(i).Value;
            //currRel.ResetMutualRelationshipModifier();
            currRel.ResetEventModifiers();
        }
    }
    internal void UpdateAllRelationshipsLikeness() {
        if (this.king != null) {
			foreach (KingdomRelationship relationship in relationships.Values) {
                //if (relationship.isDiscovered) {
                    relationship.UpdateLikeness(null);
                //}
			}
        }
    }
	internal void UpdateAllRelationshipsLikenessFromOthers() {
		if (this.king != null) {
			foreach (KingdomRelationship relationship in relationships.Values) {
				KingdomRelationship relationshipFromOther = relationship.targetKingdom.GetRelationshipWithKingdom(this);
				relationshipFromOther.UpdateLikeness (null);
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
	//protected void KingdomTickActions(){
 //       //if (_isGrowthEnabled) {
 //       //    this.AttemptToExpand();
 //       //}
	//	this.IncreaseTechCounterPerTick();
 //       //this.TriggerEvents();
 //   }
//    private void AdaptToKingValues() {
//		if(!this.isDead){
//			for (int i = 0; i < _dictCharacterValues.Count; i++) {
//				CHARACTER_VALUE currValue = _dictCharacterValues.ElementAt(i).Key;
//				if (king.importantCharacterValues.ContainsKey(currValue)) {
//					UpdateSpecificCharacterValue(currValue, 1);
//				} else {
//					UpdateSpecificCharacterValue(currValue, -1);
//				}
//			}
//			UpdateKingdomCharacterValues();
//			SchedulingManager.Instance.AddEntry (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year + 1, () => AdaptToKingValues());
//		}
////        if(GameManager.Instance.days == 1 && GameManager.Instance.month == 1) {
////            for (int i = 0; i < _dictCharacterValues.Count; i++) {
////                CHARACTER_VALUE currValue = _dictCharacterValues.ElementAt(i).Key;
////                if (king.importantCharacterValues.ContainsKey(currValue)) {
////                    UpdateSpecificCharacterValue(currValue, 1);
////                } else {
////                    UpdateSpecificCharacterValue(currValue, -1);
////                }
////            }
////            UpdateKingdomCharacterValues();
////        }
//    }
    private void AttemptToAge() {
		if(!this.isDead){
			age += 1;
			SchedulingManager.Instance.AddEntry (GameManager.Instance.month, GameManager.Instance.days, (GameManager.Instance.year + 1), () => AttemptToAge());
		}

//        if(GameManager.Instance.year > foundationYear && GameManager.Instance.month == foundationMonth && GameManager.Instance.days == foundationDay) {
//            age += 1;
//        }
    }
//    private void TriggerEvents() {
////        this.TriggerSlavesMerchant();
////        this.TriggerHypnotism();
//        //this.TriggerKingdomHoliday();
////        //this.TriggerDevelopWeapons();
////        this.TriggerKingsCouncil();
////		this.TriggerCrime ();
//    }
	private void ScheduleEvents(){
		SchedulingManager.Instance.AddEntry (GameManager.Instance.month, GameManager.daysInMonth[GameManager.Instance.month], GameManager.Instance.year, () => TriggerSlavesMerchant());
		//SchedulingManager.Instance.AddEntry (GameManager.Instance.month, GameManager.daysInMonth[GameManager.Instance.month], GameManager.Instance.year, () => TriggerHypnotism());
		//SchedulingManager.Instance.AddEntry (GameManager.Instance.month, GameManager.daysInMonth[GameManager.Instance.month], GameManager.Instance.year, () => TriggerKingsCouncil());

		int month = UnityEngine.Random.Range (1, 5);
		SchedulingManager.Instance.AddEntry (month, UnityEngine.Random.Range(1, GameManager.daysInMonth[month]), GameManager.Instance.year, () => TriggerCrime());
	}

    #region Expansion Functions
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

        //if (cities.Count >= cityCap) {
        //    //Kingdom has reached max city capacity
        //    return;
        //}
        if(_expansionRate < GridMap.Instance.numOfRegions) {
            return;
        }

        float upperBound = 300f + (150f * (float)this.cities.Count);
        float chance = UnityEngine.Random.Range(0, upperBound);
        if (this.cities.Count > 0) {
            EventCreator.Instance.CreateExpansionEvent(this);
        }
    }
    private void IncreaseExpansionRatePerMonth() {
        //Reschedule next month
        GameDate dueDate = new GameDate(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
        dueDate.AddMonths(1);
        SchedulingManager.Instance.AddEntry(dueDate.month, dueDate.day, dueDate.year, () => IncreaseExpansionRatePerMonth());

        if (CityGenerator.Instance.GetExpandableTileForKingdom(this) == null) {
            //set expansion rate to 0 and don't increase expansion rate until kingdom can expand
            ResetExpansionRate();
            return;
        }
        if (_expansionRate < GridMap.Instance.numOfRegions) {
            AdjustExpansionRate(GetMonthlyExpansionRateIncrease());
        }
    }
    internal int GetMonthlyExpansionRateIncrease() {
        int monthlyExpansionRate = king.GetExpansionRateContribution();
        for (int i = 0; i < cities.Count; i++) {
            monthlyExpansionRate += cities[i].governor.GetExpansionRateContribution();
        }
        return monthlyExpansionRate;
    }
    internal void ResetExpansionRate() {
        _expansionRate = 0;
        KingdomManager.Instance.UpdateKingdomList();
    }
    private void AdjustExpansionRate(int adjustment) {
        _expansionRate += adjustment;
        _expansionRate = Mathf.Clamp(_expansionRate, 0, GridMap.Instance.numOfRegions);
        KingdomManager.Instance.UpdateKingdomList();
    }
    #endregion

    #region Odd Day Actions
    private void ScheduleOddDayActions() {
        KingdomManager.Instance.IncrementOddActionDay();
        SchedulingManager.Instance.AddEntry(GameManager.Instance.month, KingdomManager.Instance.oddActionDay, GameManager.Instance.year, () => OddDayActions());
    }
    private void OddDayActions() {
        if (_isGrowthEnabled) {
            AttemptToExpand();
        }
        GameDate nextActionDay = new GameDate(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
        nextActionDay.AddMonths(1);
        SchedulingManager.Instance.AddEntry(nextActionDay.month, nextActionDay.day, nextActionDay.year, () => OddDayActions());
    }
    #endregion

    #region Prestige
    internal void AdjustPrestige(int adjustment) {
        _prestige += adjustment;
        //_prestige = Mathf.Min(_prestige, KingdomManager.Instance.maxPrestige);
        //KingdomManager.Instance.UpdateKingdomPrestigeList();
    }
    internal void SetPrestige(int adjustment) {
        _prestige = adjustment;
        //_prestige = Mathf.Min(_prestige, KingdomManager.Instance.maxPrestige);
        //KingdomManager.Instance.UpdateKingdomPrestigeList();
    }
  //  internal void MonthlyPrestigeActions() {
  //      //Add Prestige
		//int prestigeToBeAdded = GetMonthlyPrestigeGain();
		//if(this.cityCap > this.cities.Count){
		//	float reduction = GetMonthlyPrestigeReduction (this.cityCap - this.cities.Count);
		//	prestigeToBeAdded -= (int)(prestigeToBeAdded * reduction);
		//}
		//AdjustPrestige(prestigeToBeAdded);
  //      //Reschedule event
  //      //GameDate gameDate = new GameDate(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
  //      //gameDate.AddMonths(1);
  //      //gameDate.day = GameManager.daysInMonth[gameDate.month];
  //      //SchedulingManager.Instance.AddEntry(gameDate.month, gameDate.day, gameDate.year, () => MonthlyPrestigeActions());
  //  }
	//private float GetMonthlyPrestigeReduction(int cityCapExcess){
	//	if(cityCapExcess == 1){
	//		return 0.05f;
	//	}else if(cityCapExcess == 2){
	//		return 0.15f;
	//	}else if(cityCapExcess == 3){
	//		return 0.3f;
	//	}else if(cityCapExcess == 4){
	//		return 0.5f;
	//	}else if(cityCapExcess == 5){
	//		return 0.7f;
	//	}else if(cityCapExcess == 6){
	//		return 0.9f;
	//	}else if(cityCapExcess >= 7){
	//		return 1f;
	//	}
	//	return 0f;
	//}
    //internal int GetMonthlyPrestigeGain() {
    //    int monthlyPrestigeGain = 0;
    //    monthlyPrestigeGain += king.GetPrestigeContribution();
    //    for (int i = 0; i < cities.Count; i++) {
    //        monthlyPrestigeGain += cities[i].governor.GetPrestigeContribution();
    //    }
    //    return monthlyPrestigeGain;
    //}
    #endregion

    #region Trading
    internal void AddKingdomToEmbargoList(Kingdom kingdomToAdd, EMBARGO_REASON embargoReason = EMBARGO_REASON.NONE) {
        if (!this._embargoList.ContainsKey(kingdomToAdd)) {
            this._embargoList.Add(kingdomToAdd, embargoReason);
            //Remove all existing trade routes between kingdomToAdd and this Kingdom
            //this.RemoveAllTradeRoutesWithOtherKingdom(kingdomToAdd);
            //kingdomToAdd.RemoveAllTradeRoutesWithOtherKingdom(this);
            //kingdomToAdd.AdjustStability(STABILITY_DECREASE_EMBARGO);
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
        _regions.Add(city.region);
        //this.UpdateKingdomTypeData();
        UpdateKingdomSize();
        UpdatePopulationCapacity();
        if (this._cities.Count == 1 && this._cities[0] != null) {
            SetCapitalCity(this._cities[0]);
        }
        KingdomManager.Instance.UpdateKingdomList();
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
        _regions.Remove(city.region);
        this.CheckIfKingdomIsDead();
        if (!this.isDead) {
            UpdateKingdomSize();
            UpdatePopulationCapacity();
            RevalidateKingdomAdjacency(city);
            //this.UpdateKingdomTypeData();
            for (int i = 0; i < this._cities.Count; i++) {
				if (this._cities[i].rebellion == null) {
					SetCapitalCity(this._cities[i]);
					break;
				}
			}
            KingdomManager.Instance.UpdateKingdomList();
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
    private void UpdateKingdomSize() {
        if(cities.Count < KingdomManager.Instance.smallToMediumReq) {
            _kingdomSize = KINGDOM_SIZE.SMALL;
        } else if (cities.Count >= KingdomManager.Instance.smallToMediumReq && cities.Count < KingdomManager.Instance.mediumToLargeReq) {
            _kingdomSize = KINGDOM_SIZE.MEDIUM;
        } else if (cities.Count >= KingdomManager.Instance.mediumToLargeReq) {
            _kingdomSize = KINGDOM_SIZE.LARGE;
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
    internal bool IsElligibleForRebellion() {
        if(stability <= -50 && kingdomSize != KINGDOM_SIZE.SMALL) {
            for (int i = 0; i < cities.Count; i++) {
                City currCity = cities[i];
                if(currCity.importantCitizensInCity.Values.Where(x => x.role != ROLE.KING && x.loyaltyToKing <= -50).Any()) {
                    return true;
                }
            }
        }
        return false;
    }
    internal List<Citizen> GetCitizensForRebellion() {
        List<Citizen> citizensForRebellion = new List<Citizen>();
        for (int i = 0; i < cities.Count; i++) {
            City currCity = cities[i];
            citizensForRebellion.AddRange(currCity.importantCitizensInCity.Values.Where(x => x.role != ROLE.KING && x.loyaltyToKing <= -50));
        }
        return citizensForRebellion;
    }
    #endregion

    #region Succession
    internal void UpdateKingSuccession() {
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
			if (this.successionLine [i].id != this.king.id) {
				if ((this.successionLine [i].father != null && this.king.father != null) && this.successionLine [i].father.id == this.king.father.id) {
					if (this.successionLine [i].gender == GENDER.MALE) {
						orderedBrotherRoyalties.Add (this.successionLine [i]);
					}else{
						orderedSisterRoyalties.Add (this.successionLine [i]);
					}
				}
			}
		}

        this.successionLine.Clear();
        this.successionLine.AddRange (orderedMaleRoyalties.OrderBy (x => x.generation).ThenByDescending (x => x.age));
		this.successionLine.AddRange (orderedFemaleRoyalties.OrderBy (x => x.generation).ThenByDescending (x => x.age));

		this.successionLine.AddRange (orderedBrotherRoyalties.OrderByDescending (x => x.age));
		this.successionLine.AddRange (orderedSisterRoyalties.OrderByDescending (x => x.age));

        Citizen newNextInLine = successionLine.FirstOrDefault();
        if (newNextInLine != null && nextInLine != null && newNextInLine != nextInLine && nextInLine.role == ROLE.CROWN_PRINCE) {
            //next in line is no longer the next in line
            nextInLine.AssignRole(ROLE.UNTRAINED);
        }
        nextInLine = newNextInLine;
        if(newNextInLine != null) {
            newNextInLine.AssignRole(ROLE.CROWN_PRINCE);
        }
        
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
                    //Debug.LogError("City of previous king is dead! But still creating king in that dead city");
					for (int i = 0; i < this.cities.Count; i++) {
						if(this.cities[i].rebellion == null){
							newKing = this.cities [i].CreateNewKing ();
							break;
						}
					}
				}else{
					newKing = this.king.city.CreateNewKing();
				}
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
		if(newKing == null){
			return;
		}
        //SetKingdomType(newKing.preferredKingdomType);
        SetCapitalCity(newKing.city);
        newKing.city.hasKing = true;

        if (!newKing.isDirectDescendant) {
            Utilities.ChangeDescendantsRecursively(newKing, true);
            if (this.king != null) {
                Utilities.ChangeDescendantsRecursively(this.king, false);
            }
        }

        Citizen previousKing = this.king;
        bool isNewKingGovernor = newKing.isGovernor;

        this.king = newKing;
        newKing.AssignRole(ROLE.KING);
        ((King)newKing.assignedRole).SetOwnedKingdom(this);

        if (isNewKingGovernor) {
            newKing.city.AssignNewGovernor();
        }

        newKing.history.Add(new History(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, newKing.name + " became the new King/Queen of " + this.name + ".", HISTORY_IDENTIFIER.NONE));

        ResetRelationshipModifiers();
        //UpdateMutualRelationships();

        this.successionLine.Clear();
        ChangeSuccessionLineRescursively(newKing);
        this.successionLine.AddRange(newKing.GetSiblings());
        UpdateKingSuccession();

        this.UpdateProductionRatesFromKing();
        //this.UpdateAllGovernorsLoyalty();
        this.UpdateAllRelationshipsLikeness();
        this.UpdateAllCitizensOpinionOfKing();
    }
    internal void UpdateAllCitizensOpinionOfKing() {
        for (int i = 0; i < cities.Count; i++) {
            City currCity = cities[i];
            for (int j = 0; j < currCity.citizens.Count; j++) {
                Citizen currCitizen = currCity.citizens[j];
                if(currCitizen.id != king.id) {
                    currCitizen.UpdateKingOpinion();
                }
            }
        }
    }
    #endregion

    #region War
    internal void AdjustExhaustionToAllRelationship(int amount) {
        for (int i = 0; i < relationships.Count; i++) {
            relationships.ElementAt(i).Value.AdjustExhaustion(amount);
        }
    }
    internal void ConquerCity(City city, General attacker) {
        if (this.id != city.kingdom.id) {
            KingdomRelationship rel = this.GetRelationshipWithKingdom(city.kingdom);
            if (rel != null && rel.war != null) {
				rel.war.ChangeDoneStateWarPair (true);
            }

			city.ConquerCity(this);
//            HexTile hex = city.hexTile;
//            if (this.race != city.kingdom.race) {
//                city.KillCity();
//            } else {
//				city.ConquerCity(this);
//            }
            //this.AdjustStability(STABILITY_DECREASE_CONQUER);
        } else {
			if(city.rebellion == null){
				city.ChangeToRebelFort(attacker.citizen.city.rebellion);
			}
        }

    }
	internal void ConquerCity(City city){
		if (this.id != city.kingdom.id) {
			city.ConquerCity(this);
//			HexTile hex = city.hexTile;
//			if (this.race != city.kingdom.race) {
//				city.KillCity();
//			} else {
//				city.ConquerCity(this);
//			}
			//this.AdjustStability(STABILITY_DECREASE_CONQUER);
		}
	}
    #endregion

    #region Kingdom Tile Management
    internal void HighlightAllOwnedTilesInKingdom() {
        for (int i = 0; i < this.cities.Count; i++) {
            this.cities[i].HighlightAllOwnedTiles(69f / 255f);
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
//		for (int i = 0; i < relationships.Count; i++) {
//			if(relationships.ElementAt(i).Value.isAtWar){
//				total += 1;
//			}
//		}
		foreach (KingdomRelationship relationship in this.relationships.Values) {
			if(relationship.isAtWar){
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
//	internal void AddResourceToKingdom(RESOURCE resource){
//		RESOURCE_BENEFITS resourceBenefit = Utilities.resourceBenefits[resource].Keys.FirstOrDefault();
//
//        if (!this._availableResources.ContainsKey(resource)) {
//			this._availableResources.Add(resource, 0);
//            //this.RemoveObsoleteTradeRoutes(resource);
//            if(resourceBenefit == RESOURCE_BENEFITS.GROWTH_RATE) {
//                this.UpdateAllCitiesDailyGrowth();
//            } else if (resourceBenefit == RESOURCE_BENEFITS.TECH_LEVEL) {
//                this.UpdateTechLevel();
//            }
//        }
//		this._availableResources[resource] += 1;
//        if (resourceBenefit == RESOURCE_BENEFITS.EXPANSION_RATE) {
//            this.UpdateExpansionRate();
//        }
//    }
//    internal void UpdateExpansionRate() {
//        this.expansionChance = this.kingdomTypeData.expansionRate;
//
//        for (int i = 0; i < this.availableResources.Keys.Count; i++) {
//            RESOURCE currResource = this.availableResources.Keys.ElementAt(i);
//            if (Utilities.GetBaseResourceType(currResource) == this.basicResource) {
//                int multiplier = this.availableResources[currResource];
//				RESOURCE_BENEFITS resourceBenefit = Utilities.resourceBenefits[currResource].Keys.FirstOrDefault();
//                float expansionRateGained = Utilities.resourceBenefits[currResource][resourceBenefit];
//                if (resourceBenefit == RESOURCE_BENEFITS.EXPANSION_RATE) {
//                    this.expansionChance += expansionRateGained * multiplier;
//                }
//            }
//        }
//    }
//    internal void UpdateTechLevel() {
//        this._techLevel = 0;
////        List<RESOURCE> allAvailableResources = this._availableResources.Keys.ToList();
////        for (int i = 0; i < allAvailableResources.Count; i++) {
//		foreach (RESOURCE currResource in this._availableResources.Keys) {
////            RESOURCE currResource = allAvailableResources[i];
//			RESOURCE_BENEFITS resourceBenefit = Utilities.resourceBenefits[currResource].Keys.FirstOrDefault();
//            if (resourceBenefit == RESOURCE_BENEFITS.TECH_LEVEL) {
//				int upgrade = (int)Utilities.resourceBenefits[currResource][resourceBenefit];
//				UpgradeTechLevel(upgrade);
//            }
//        }
//    }
//    internal void UpdateAllCitiesDailyGrowth() {
//        //get all resources from tiles and trade routes, only include trade routes where this kingom is the target
//        List<RESOURCE> allAvailableResources = this._availableResources.Keys.ToList();
//        int dailyGrowthGained = this.ComputeDailyGrowthGainedFromResources(allAvailableResources);
//        for (int i = 0; i < this.cities.Count; i++) {
//            City currCity = this.cities[i];
//            currCity.UpdateDailyGrowthBasedOnSpecialResources(dailyGrowthGained);
//        }
//    }
//    private int ComputeDailyGrowthGainedFromResources(List<RESOURCE> allAvailableResources) {
//        int dailyGrowthGained = 0;
//        for (int i = 0; i < allAvailableResources.Count; i++) {
//            RESOURCE currentResource = allAvailableResources[i];
//			RESOURCE_BENEFITS resourceBenefit = Utilities.resourceBenefits[currentResource].Keys.FirstOrDefault();
//            if(resourceBenefit == RESOURCE_BENEFITS.GROWTH_RATE) {
//                dailyGrowthGained += (int)Utilities.resourceBenefits[currentResource][resourceBenefit];
//            }
//        }
//        return dailyGrowthGained;
//    }
//    /*
//     * Gets a list of resources that otherKingdom does not have access to (By self or by trade).
//     * Will compare to this kingdoms available resources (excl. resources from trade)
//     * */
    internal List<RESOURCE> GetResourcesOtherKingdomDoesNotHave(Kingdom otherKingdom) {
        List<RESOURCE> resourcesOtherKingdomDoesNotHave = new List<RESOURCE>();
//        List<RESOURCE> allAvailableResourcesOfOtherKingdom = otherKingdom.availableResources.Keys.ToList();
		bool hasContainedResource = false;
		foreach (RESOURCE currKey in this._availableResources.Keys) {
			hasContainedResource = false;
			foreach (RESOURCE otherCurrKey in otherKingdom.availableResources.Keys) {
				if(otherCurrKey == currKey){
					hasContainedResource = true;
					break;
				}
			}
			if(!hasContainedResource){
				resourcesOtherKingdomDoesNotHave.Add(currKey);
			}
		}
        return resourcesOtherKingdomDoesNotHave;
    }
//    internal void UpdateAvailableResources() {
//        this._availableResources.Clear();
//        for (int i = 0; i < this.cities.Count; i++) {
//            City currCity = this.cities[i];
//            for (int j = 0; j < currCity.structures.Count; j++) {
//                HexTile currHexTile = currCity.structures[j];
//                if (currHexTile.specialResource != RESOURCE.NONE) {
//                    this.AddResourceToKingdom(currHexTile.specialResource);
//                }
//            }
//        }
//    }
    /*
     * <summary>
     * Set growth state of kingdom, disabling growth will prevent expansion,
     * building of new settlements and pregnancy
     * */
    internal void SetGrowthState(bool state) {
        _isGrowthEnabled = state;
    }
    #endregion

    #region Tech
 //   private void IncreaseTechCounterPerTick(){
	//	if(!this._isTechProducing){
	//		return;
	//	}
	//	int amount = this.cities.Count + GetTechContributionFromCitizens();
	//	amount = (int)(amount * this._techProductionPercentage);
	//	this.AdjustTechCounter (amount);
	//}
    internal int GetMonthlyTechGain() {
        int monthlyTechGain = GetTechContributionFromCitizens();
        for (int i = 0; i < cities.Count; i++) {
            City currCity = cities[i];
            if (!currCity.isDead && currCity.rebellion == null) {
                monthlyTechGain += currCity.techPoints;
            }
        }
        //Tech Gains
        monthlyTechGain = ((2 * scientists * monthlyTechGain) / (scientists + monthlyTechGain));
        monthlyTechGain = Mathf.FloorToInt(monthlyTechGain * techProductionPercentage);
        return monthlyTechGain;
    }
    internal int GetTechContributionFromCitizens() {
        int techContributionsFromCitizens = 0;
        techContributionsFromCitizens += king.GetTechContribution();
        for (int i = 0; i < cities.Count; i++) {
            techContributionsFromCitizens += cities[i].governor.GetTechContribution();
        }
        return techContributionsFromCitizens;
    }
	private void UpdateTechCapacity(){
		this._techCapacity = 500 + ((400 + (100 * this._techLevel)) * this._techLevel);
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
		if(this._techLevel < 0){
			amount -= this._techLevel;
			this._techLevel = 0;
		}
		this._techCounter = 0;
		AdjustPowerPointsToAllCities(amount);
        AdjustDefensePointsToAllCities(amount);
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
	//private void UpdateCharacterValuesOfKingsAndGovernors(){
	//	if(this.king != null){
	//		this.king.UpdateCharacterValues ();
	//	}
	//	for(int i = 0; i < this.cities.Count; i++){
	//		if(this.cities[i].governor != null){
	//			this.cities [i].governor.UpdateCharacterValues ();
	//		}
	//	}
	//}
    //internal void GenerateKingdomCharacterValues() {
    //    this._dictCharacterValues.Clear();
    //    this._dictCharacterValues = System.Enum.GetValues(typeof(CHARACTER_VALUE)).Cast<CHARACTER_VALUE>().ToDictionary(x => x, x => UnityEngine.Random.Range(1, 101));
    //    UpdateKingdomCharacterValues();
    //}
    //internal void UpdateKingdomCharacterValues() {
    //    this._importantCharacterValues = this._dictCharacterValues.Where(x => x.Value >= 50).OrderByDescending(x => x.Value).Take(4).ToDictionary(x => x.Key, x => x.Value);
    //}
    //private void UpdateSpecificCharacterValue(CHARACTER_VALUE key, int value) {
    //    if (this._dictCharacterValues.ContainsKey(key)) {
    //        this._dictCharacterValues[key] += value;
    //        //			UpdateCharacterValueByKey(key, value);
    //    }
    //}
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

	internal bool HasWar(Kingdom exceptionKingdom = null){
//		for(int i = 0; i < relationships.Count; i++){
//			if(relationships.ElementAt(i).Value.isAtWar){
//				return true;
//			}
//		}
		if(exceptionKingdom == null){
//			foreach (KingdomRelationship relationship in relationships.Values) {
//				if (relationship.isAtWar) {
//					return true;
//				}
//			}
			if(this._warfareInfo.Count > 0){
				return true;
			}
		}else{
			foreach (KingdomRelationship relationship in relationships.Values) {
				if (relationship.isAtWar && exceptionKingdom.id != relationship.targetKingdom.id) {
					return true;
				}
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
//    private void TriggerHypnotism() {
//		if(!this.isDead){
//			if (this.king.importantCharacterValues.ContainsKey(CHARACTER_VALUE.INFLUENCE)) {
//				List<GameEvent> previousHypnotismEvents = GetEventsOfType (EVENT_TYPES.HYPNOTISM, false);
//				if (!previousHypnotismEvents.Where(x => x.startYear == GameManager.Instance.year).Any()) {
//					List<Kingdom> notFriends = new List<Kingdom>();
//					for (int i = 0; i < discoveredKingdoms.Count; i++) {
//						Kingdom currKingdom = discoveredKingdoms[i];
//						KingdomRelationship rel = currKingdom.GetRelationshipWithKingdom(this);
//						if (rel.relationshipStatus != RELATIONSHIP_STATUS.AFFECTIONATE && rel.relationshipStatus != RELATIONSHIP_STATUS.LOVE) {
//							notFriends.Add(currKingdom);
//						}
//					}
//					if (UnityEngine.Random.Range(0, 100) < 10 && notFriends.Count > 0) {
//						EventCreator.Instance.CreateHypnotismEvent(this, notFriends[UnityEngine.Random.Range(0, notFriends.Count)]);
//					}
//				}
//			}
//			GameDate gameDate = new GameDate (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
//			gameDate.AddMonths (1);
//			gameDate.day = GameManager.daysInMonth [gameDate.month];
//			SchedulingManager.Instance.AddEntry (gameDate.month, gameDate.day, gameDate.year, () => TriggerHypnotism());
//		}
////        if (this.king.importantCharacterValues.ContainsKey(CHARACTER_VALUE.INFLUENCE)) {
////            if (GameManager.Instance.days == GameManager.daysInMonth[GameManager.Instance.month]) {
////                List<GameEvent> previousHypnotismEvents = EventManager.Instance.GetEventsStartedByKingdom(this, new EVENT_TYPES[] { EVENT_TYPES.HYPNOTISM }, false);
////                if (previousHypnotismEvents.Where(x => x.startYear == GameManager.Instance.year).Count() <= 0) {
////                    List<Kingdom> notFriends = new List<Kingdom>();
////                    for (int i = 0; i < discoveredKingdoms.Count; i++) {
////                        Kingdom currKingdom = discoveredKingdoms[i];
////                        KingdomRelationship rel = currKingdom.king.GetRelationshipWithKingdom(this.king);
////                        if (rel.relationshipStatus != RELATIONSHIP_STATUS.AFFECTIONATE && rel.relationshipStatus != RELATIONSHIP_STATUS.LOVE) {
////                            notFriends.Add(currKingdom);
////                        }
////                    }
////                    if (UnityEngine.Random.Range(0, 100) < 10 && notFriends.Count > 0) {
////                        EventCreator.Instance.CreateHypnotismEvent(this, notFriends[UnityEngine.Random.Range(0, notFriends.Count)]);
////                    }
////                }
////            }
////        }
//    }
    #endregion

    #region Kingdom Holiday
//    private void TriggerKingdomHoliday() {
//        if (this.king.importantCharacterValues.ContainsKey(CHARACTER_VALUE.TRADITION)) {
//            if (Utilities.IsCurrentDayMultipleOf(15)) {
////                List<GameEvent> activeHolidays = EventManager.Instance.GetEventsStartedByKingdom(this, new EVENT_TYPES[] { EVENT_TYPES.KINGDOM_HOLIDAY });
////                List<GameEvent> activeWars = EventManager.Instance.GetAllEventsKingdomIsInvolvedIn(this, new EVENT_TYPES[] { EVENT_TYPES.KINGDOM_WAR });
//                if(!HasActiveEvent(EVENT_TYPES.KINGDOM_HOLIDAY) && !HasActiveEvent(EVENT_TYPES.KINGDOM_WAR)) { //There can only be 1 active holiday per kingdom at a time. && Kingdoms that are at war, cannot celebrate holidays.
//                    if (UnityEngine.Random.Range(0, 100) < 10) {
//                        if(UnityEngine.Random.Range(0, 100) < 50) {
//                            //Celebrate Holiday
//                            EventCreator.Instance.CreateKingdomHolidayEvent(this);
//                        } else {
//                            //If a king chooses not to celebrate the holiday, his governors that value TRADITION will decrease loyalty by 20.
//                            for (int i = 0; i < cities.Count; i++) {
//                                Governor currGovernor = (Governor)cities[i].governor.assignedRole;
//                                if (currGovernor.citizen.importantCharacterValues.ContainsKey(CHARACTER_VALUE.TRADITION)) {
//									currGovernor.AddEventModifier(-5, "Did not celebrate holiday", null);
//                                }
//                            }
//                            if (_importantCharacterValues.ContainsKey(CHARACTER_VALUE.TRADITION)) {
//                                AdjustStability(-10);
//                            }
//                        }
//                    }
//                }
//            }
//        }
//    }
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
//    protected void TriggerKingsCouncil() {
//		if(!this.isDead){
//			if(this.king.importantCharacterValues.ContainsKey(CHARACTER_VALUE.LIBERTY) || this.king.importantCharacterValues.ContainsKey(CHARACTER_VALUE.PEACE)) {
//				if (UnityEngine.Random.Range(0, 100) < 2) {
//					if (discoveredKingdoms.Count > 2 && !HasActiveEvent(EVENT_TYPES.KINGDOM_WAR) && !HasActiveEvent(EVENT_TYPES.KINGS_COUNCIL)) {
//						EventCreator.Instance.CreateKingsCouncilEvent(this);
//					}
//				}
//			}
//			GameDate gameDate = new GameDate (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
//			gameDate.AddMonths (1);
//			gameDate.day = GameManager.daysInMonth [gameDate.month];
//			SchedulingManager.Instance.AddEntry (gameDate.month, gameDate.day, gameDate.year, () => TriggerKingsCouncil());
//		}
////        if(this.king.importantCharacterValues.ContainsKey(CHARACTER_VALUE.LIBERTY) || this.king.importantCharacterValues.ContainsKey(CHARACTER_VALUE.PEACE)) {
////            if (GameManager.Instance.days == GameManager.daysInMonth[GameManager.Instance.month]) {
////                if (UnityEngine.Random.Range(0, 100) < 2) {
////                    if (discoveredKingdoms.Count > 2 && EventManager.Instance.GetEventsStartedByKingdom(this, new EVENT_TYPES[] { EVENT_TYPES.KINGDOM_WAR, EVENT_TYPES.KINGS_COUNCIL }).Count <= 0) {
////                        EventCreator.Instance.CreateKingsCouncilEvent(this);
////                    }
////                }
////            }
////        }
//    }
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
    internal void SetFogOfWarStateForRegion(Region region, FOG_OF_WAR_STATE fowState) {
        _regionFogOfWarDict[region] = fowState;
        for (int i = 0; i < region.tilesInRegion.Count; i++) {
            SetFogOfWarStateForTile(region.tilesInRegion[i], fowState);
        }
    }
    internal void SetFogOfWarStateForTile(HexTile tile, FOG_OF_WAR_STATE fowState) {
        FOG_OF_WAR_STATE previousStateOfTile = _fogOfWar[tile.xCoordinate, tile.yCoordinate];
        //Remove tile from previous list that it belonged to
        _fogOfWarDict[previousStateOfTile].Remove(tile);

        //Set new state of tile in fog of war dictionary
        _fogOfWar[tile.xCoordinate, tile.yCoordinate] = fowState;
        //Check if tile is already in the list
        if (!_fogOfWarDict[fowState].Contains(tile)) {
            //if not, add it to the new states list
            _fogOfWarDict[fowState].Add(tile);
        }

        //if this kingdom is currently the active kingdom, automatically update the tile visual
        if (UIManager.Instance.currentlyShowingKingdom != null && UIManager.Instance.currentlyShowingKingdom.id == this.id) {
            UpdateFogOfWarVisualForTile(tile, fowState);
        }

        //For checking if tile dictionary is accurate, remove this when checking is no longer necessary
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
//		for (int i = 0; i < relationships.Count; i++) {
//			if(relationships.ElementAt(i).Value.isAtWar){
//				numOfWars += 1;
//			}
//		}
		foreach (KingdomRelationship relationship in this.relationships.Values) {
			if(relationship.isAtWar){
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

    #region Event Management
    internal void AddActiveEvent(GameEvent gameEvent) {
        this.activeEvents.Add(gameEvent);
    }
    internal void RemoveActiveEvent(GameEvent gameEvent) {
        this.activeEvents.Remove(gameEvent);
        AddToDoneEvents(gameEvent);
    }
    internal void AddToDoneEvents(GameEvent gameEvent) {
        this.doneEvents.Add(gameEvent);
        if (this.doneEvents.Count > KingdomManager.Instance.maxKingdomEventHistory) {
            this.doneEvents.RemoveAt(0);
        }
    }
    internal bool HasActiveEvent(EVENT_TYPES eventType) {
        for (int i = 0; i < this.activeEvents.Count; i++) {
            if (this.activeEvents[i].eventType == eventType) {
                return true;
            }
        }
        return false;
    }
    internal int GetActiveEventsOfTypeCount(EVENT_TYPES eventType) {
        int count = 0;
        for (int i = 0; i < this.activeEvents.Count; i++) {
            if (this.activeEvents[i].eventType == eventType) {
                count += 1;
            }
        }
        return count;
    }
    internal List<GameEvent> GetEventsOfType(EVENT_TYPES eventType, bool isActiveOnly = true) {
        List<GameEvent> gameEvents = new List<GameEvent>();
        for (int i = 0; i < this.activeEvents.Count; i++) {
            if (this.activeEvents[i].eventType == eventType) {
                gameEvents.Add(this.activeEvents[i]);
            }
        }
        if (!isActiveOnly) {
            for (int i = 0; i < this.doneEvents.Count; i++) {
                if (this.doneEvents[i].eventType == eventType) {
                    gameEvents.Add(this.doneEvents[i]);
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
    #endregion

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

	#region Balance of Power
	internal void Militarize(bool state, bool isAttacking = false){
		this._isMilitarize = state;
		if(UIManager.Instance.currentlyShowingKingdom.id == this.id){
			UIManager.Instance.militarizingGO.SetActive (state);
		}
        if (state) {
            Kingdom kingdom2 = null;
            float highestInvasionValue = -1;
            foreach (KingdomRelationship kr in relationships.Values) {
                if (kr.isDiscovered) {
                    if(kr.targetKingdomInvasionValue > highestInvasionValue) {
                        kingdom2 = kr.targetKingdom;
                        highestInvasionValue = kr.targetKingdomInvasionValue;
                    }
					if(kr.isAtWar && isAttacking){
						kingdom2 = kr.targetKingdom;
						break;
					}
                }
            }
            if (kingdom2 != null) {
				string militarizeFileName = "militarize";
				if(isAttacking){
					militarizeFileName = "militarize_attack";
				}
                Log militarizeLog = new Log(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "General", "Kingdom", "militarize");
                militarizeLog.AddToFillers(this, this.name, LOG_IDENTIFIER.KINGDOM_1);
                militarizeLog.AddToFillers(kingdom2, kingdom2.name, LOG_IDENTIFIER.KINGDOM_2);
                UIManager.Instance.ShowNotification(militarizeLog, new HashSet<Kingdom>() { this }, false);
            }
        }
    }
	internal void Fortify(bool state, bool isUnderAttack = false){
		this._isFortifying = state;
		if(UIManager.Instance.currentlyShowingKingdom.id == this.id){
			UIManager.Instance.fortifyingGO.SetActive (state);
		}
		if (state) {
            Kingdom kingdom2 = null;
            float highestKingdomThreat = -1;
            foreach (KingdomRelationship kr in relationships.Values) {
                if (kr.isDiscovered) {
                    if (kr.targetKingdomThreatLevel > highestKingdomThreat) {
                        kingdom2 = kr.targetKingdom;
                        highestKingdomThreat = kr.targetKingdomThreatLevel;
                    }
					if(kr.isAtWar && isUnderAttack){
						kingdom2 = kr.targetKingdom;
						break;
					}
                }
            }
            if (kingdom2 != null) {
				string fortifyFileName = "fortify";
				if(isUnderAttack){
					fortifyFileName = "fortify_under_attack";
				}
				Log fortifyLog = new Log(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "General", "Kingdom", fortifyFileName);
                fortifyLog.AddToFillers(this, this.name, LOG_IDENTIFIER.KINGDOM_1);
                fortifyLog.AddToFillers(kingdom2, kingdom2.name, LOG_IDENTIFIER.KINGDOM_2);
                UIManager.Instance.ShowNotification(fortifyLog, new HashSet<Kingdom>() { this }, false);
            }
		}
	}
	private void ScheduleActionDay(){
		KingdomManager.Instance.IncrementCurrentActionDay (2);
		SchedulingManager.Instance.AddEntry (GameManager.Instance.month, KingdomManager.Instance.currentActionDay, GameManager.Instance.year, () => ActionDay ());
		this._actionDay = KingdomManager.Instance.currentActionDay;
	}
	private void ActionDay(){
		if(!this.isDead){
			UpdateThreatLevelsAndInvasionValues ();
			if (this.king.balanceType == PURPOSE.BALANCE) {
				SeeksBalance.Initialize(this);
			}else if (this.king.balanceType == PURPOSE.SUPERIORITY) {
				SeeksSuperiority.Initialize(this);
			}

			GameDate gameDate = new GameDate(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
			gameDate.AddMonths (1);
			SchedulingManager.Instance.AddEntry (gameDate.month, gameDate.day, gameDate.year, () => ActionDay ());

		}
	}
//	private void SeekBalance(){
//		bool mustSeekAlliance = false;
//        Debug.Log("========== " + name + " is seeking balance " + GameManager.Instance.month.ToString() + "/" + GameManager.Instance.days.ToString() + "/" + GameManager.Instance.year.ToString() + " ==========");
//		//break any alliances with anyone whose threat value is 100 or above and lose 50 Prestige
//		if(this.alliancePool != null){
//			for (int i = 0; i < this.alliancePool.kingdomsInvolved.Count; i++) {
//				Kingdom kingdom = this.alliancePool.kingdomsInvolved[i];
//				if(this.id != kingdom.id){
//					KingdomRelationship kr = GetRelationshipWithKingdom(kingdom);
//					if(kr.targetKingdomThreatLevel >= 100){
//						LeaveAlliance ();
//						AdjustPrestige(-GridMap.Instance.numOfRegions);
//                        Debug.Log(name + " broke alliance with " + kingdom.name +
//                            " because it's threat level is " + kr.targetKingdomThreatLevel.ToString() + "," + name + 
//                            " lost 50 prestige. Prestige is now " + prestige.ToString());
//						break;
//					}
//				}
//			}
//		}
//
//		//if there are kingdoms whose threat value is 50 or above that is not part of my alliance
//		foreach (KingdomRelationship relationship in this.relationships.Values) {
//			if(relationship.isDiscovered){
//				if(relationship.targetKingdomThreatLevel >= 50){
//					if(relationship.targetKingdom.alliancePool == null){
//						mustSeekAlliance = true;
//						break;
//					}else{
//						if(this.alliancePool == null){
//							mustSeekAlliance = true;
//							break;
//						}else{
//							if(this.alliancePool.id != relationship.targetKingdom.alliancePool.id){
//								mustSeekAlliance = true;
//								break;
//							}
//						}
//					}
//				}
//			}
//		}
//		if(mustSeekAlliance){
//			//if i am not part of any alliance, create or join an alliance if possible
//			if(this.alliancePool == null){
//				SeekAlliance ();
//			}
//
//			//if Stability is greater than -50, militarize, otherwise only 25% chance to militarize
//			if(this.stability > -50){
//				Militarize (true);
//                Debug.Log(name + " has " + stability.ToString() + " stability and starts militarizing");
//            } else{
//				int chance = UnityEngine.Random.Range (0, 100);
//				if(chance < 25){
//                    Militarize (true);
//                    Debug.Log(name + " has " + stability.ToString() + " stability and starts militarizing");
//                }
//			}
//		}
//
//		bool hasAllianceInWar = false;
//		if(this.alliancePool != null){
//			bool hasLeftAlliance = false;
//			Dictionary<Warfare, WAR_SIDE> warsToJoin = new Dictionary<Warfare, WAR_SIDE>();
//			foreach (KingdomRelationship relationship in this.relationships.Values) {
//				if(!relationship.isAtWar && !relationship.AreAllies() && relationship.isDiscovered){
//					for (int i = 0; i < this.alliancePool.kingdomsInvolved.Count; i++) {
//						Kingdom allyKingdom = this.alliancePool.kingdomsInvolved[i];
//						if(this.id != allyKingdom.id){
//							KingdomRelationship kr = allyKingdom.GetRelationshipWithKingdom(relationship.targetKingdom);
//							if(kr.isAtWar){
//								hasAllianceInWar = true;
//								if(!warsToJoin.ContainsKey(kr.warfare)){
//									KingdomRelationship krWithAlly = GetRelationshipWithKingdom (allyKingdom);
//									int totalChanceOfJoining = krWithAlly.totalLike * 2;
//									int chance = UnityEngine.Random.Range (0, 100);
//									if(chance < totalChanceOfJoining){
//										//Join War
//										warsToJoin.Add(kr.warfare, kr.warfare.kingdomSideWeariness[allyKingdom.id]);
//										Debug.Log(name + " will join in " + allyKingdom.name + "'s war");
//	                                } else{
//										//Don't join war, leave alliance, lose 100 prestige
//										LeaveAlliance();
//										int prestigeLost = (int)((float)GridMap.Instance.numOfRegions * 1.5f);
//										AdjustPrestige (-prestigeLost);
//										hasLeftAlliance = true;
//										Debug.Log(name + " does not join in " + allyKingdom.name + "'s war, leaves the alliance and loses " + prestigeLost.ToString() + " prestige. Prestige is now " + prestige.ToString());
//	                                    break;
//									}
//								}
//							}
//						}
//					}
//					if(hasLeftAlliance){
//						break;
//					}
//				}
//			}
//			if(!hasLeftAlliance && warsToJoin.Count > 0){
//				foreach (Warfare warfare in warsToJoin.Keys) {
//					warfare.JoinWar(warsToJoin[warfare], this);
//				}
//			}
//		}
//        //if prestige can still accommodate more cities but nowhere to expand and currently not at war and none of my allies are at war
//        if (this.alliancePool == null || !hasAllianceInWar){
//			if(this.cities.Count < this.cityCap){
//				if(!HasWar()){
//					HexTile hexTile = CityGenerator.Instance.GetExpandableTileForKingdom(this);
//					if(hexTile == null){
//						//Can no longer expand
//						Kingdom targetKingdom = null;
//						float highestInvasionValue = 0;
//						foreach (KingdomRelationship relationship in this.relationships.Values) {
//							if(relationship.isAdjacent && relationship.targetKingdomInvasionValue > 50){
//								if(!relationship.AreAllies()){
//									if(targetKingdom == null){
//										targetKingdom = relationship.targetKingdom;
//										highestInvasionValue = relationship.targetKingdomInvasionValue;
//									}else{
//										if(relationship.targetKingdomInvasionValue > highestInvasionValue){
//											targetKingdom = relationship.targetKingdom;
//											highestInvasionValue = relationship.targetKingdomInvasionValue;
//										}
//									}
//								}
//							}
//						}
//						if(targetKingdom != null){
//                            //if there is anyone whose Invasion Value is 50 or above, prepare for war against the one with the highest Invasion Value
//                            Warfare warfare = new Warfare (this, targetKingdom);
//                            Debug.Log(name + " prepares for war against " + targetKingdom.name);
//                        }
//					}
//				}
//			}
//		}
//
//        Debug.Log("========== END SEEKS BALANCE " + name + " ==========");
//
//	}
	private void SeeksBandwagon(){
		KingdomRelationship relationship = GetRelationshipWithKingdom (this._mainThreat);

        //Check if main threat has not rejected a military alliance in the past 3 months.
        bool canBeAlly = false;
        GameDate gameDate = _mainThreat._currentMilitaryAllianceRejectionDate;
        if (gameDate.year != 0) {
            gameDate.AddMonths(3);
            if (GameManager.Instance.year > gameDate.year) {
                canBeAlly = true;
            } else if (GameManager.Instance.year == gameDate.year) {
                if (GameManager.Instance.month > gameDate.month) {
                    canBeAlly = true;
                }
            }
        } else {
            canBeAlly = true;
        }

        //Check if this kingdom is not already in a military alliance with main threat and that there is no currently active
        //military alliance offer between them.
        if (!relationship.isMilitaryAlliance && canBeAlly && relationship.currentActiveMilitaryAllianceOffer == null){
            KingdomRelationship relationshipOfMainThreatWithThis = _mainThreat.GetRelationshipWithKingdom(this);
			if(relationshipOfMainThreatWithThis.totalLike >= 0){
                //Send Military Alliance Offer
                EventCreator.Instance.CreateMilitaryAllianceOffer(this, this._mainThreat);
			}else{
				//Send Tribute
				EventCreator.Instance.CreateTributeEvent(this, this._mainThreat);
				Militarize (true);
			}
		}
	}
	private void SeeksBuckpass(){
		KingdomRelationship relationship = GetRelationshipWithKingdom (this._mainThreat);
		if(!relationship.isMilitaryAlliance){
			if(relationship.totalLike >= 0){
				Militarize (true);
			}else{
				Militarize (true);
				int chance = UnityEngine.Random.Range (0, 3);
				if(chance == 0){
                    //Send Provoker
                    EventCreator.Instance.CreateProvocationEvent(this, _mainThreat);
                } else if(chance == 1){
					if(this._mainThreat.adjacentKingdoms.Count > 0){
						Kingdom targetKingdom = this._mainThreat.adjacentKingdoms [UnityEngine.Random.Range (0, this._mainThreat.adjacentKingdoms.Count)];
						//Send Instigator
						EventCreator.Instance.CreateInstigationEvent(this, this._mainThreat, targetKingdom);

					}
				}
			}
		}
	}
	private void SeekSuperiority(){
		Militarize (true);
		KingdomRelationship relationship = GetRelationshipWithKingdom (this._mainThreat);
		if (!relationship.isMilitaryAlliance) {
			Kingdom currentPossibleAlly = null;
			KingdomRelationship currentPossibleAllyRelationship = null;
			bool canBeAlly = false;
            List<Kingdom> possibleAllies = new List<Kingdom>(discoveredKingdoms);
            possibleAllies.Remove(_mainThreat);
            for (int i = 0; i < possibleAllies.Count; i++) {
				Kingdom targetKingdom = possibleAllies[i];
				KingdomRelationship kingdomRelationship = GetRelationshipWithKingdom (targetKingdom);
				KingdomRelationship mainThreatKingdomRelationship = this._mainThreat.GetRelationshipWithKingdom (targetKingdom);

				canBeAlly = false;

				if((kingdomRelationship.isAdjacent || mainThreatKingdomRelationship.isAdjacent) && kingdomRelationship.totalLike >= 0 && 
                    !kingdomRelationship.isMilitaryAlliance && kingdomRelationship.currentActiveMilitaryAllianceOffer == null){
					GameDate gameDate = targetKingdom._currentMilitaryAllianceRejectionDate;
					if(gameDate.year != 0){
						gameDate.AddMonths (3);
						if(GameManager.Instance.year > gameDate.year){
							canBeAlly = true;
						}else if(GameManager.Instance.year == gameDate.year){
							if(GameManager.Instance.month > gameDate.month){
								canBeAlly = true;
							}
						}
					}else{
						canBeAlly = true;
					}

					if(canBeAlly){
						if(currentPossibleAlly != null){
							if(kingdomRelationship.totalLike > currentPossibleAllyRelationship.totalLike){
								currentPossibleAlly = targetKingdom;
								currentPossibleAllyRelationship = kingdomRelationship;
							}
						}else{
							currentPossibleAlly = targetKingdom;
							currentPossibleAllyRelationship = kingdomRelationship;
						}
					}
				}
			}

			if(currentPossibleAlly != null){
                //Send Military Alliance Offer
                EventCreator.Instance.CreateMilitaryAllianceOffer(this, currentPossibleAlly);
            }
		}else{
//			relationship.ChangeMilitaryAlliance (false);
		}
	}
	internal void AdjustStability(int amountToAdjust) {
    	this._stability += amountToAdjust;
        this._stability = Mathf.Clamp(this._stability, -100, 100);

        //If a Kingdom has a -100 Stability, a Rebellion will automatically occur which will be started by the one with the most 
        //negative opinion towards the King. The Kingdom's Stability will then reset back to 50.
        if (_stability <= -100 && kingdomSize != KINGDOM_SIZE.SMALL) {
            StartAutomaticRebellion();
        }
    }
    internal void AdjustBaseWeapons(int amountToAdjust) {
		this._baseWeapons += amountToAdjust;
		if(this._baseWeapons < 0){
			this._baseWeapons = 0;
		}
        KingdomManager.Instance.UpdateKingdomList();
    }
    internal void SetBaseWeapons(int newBaseWeapons) {
        _baseWeapons = newBaseWeapons;
        KingdomManager.Instance.UpdateKingdomList();
    }
	internal void AdjustBaseArmors(int amountToAdjust) {
		this._baseArmor += amountToAdjust;
		if(this._baseArmor < 0){
			this._baseArmor = 0;
		}
        KingdomManager.Instance.UpdateKingdomList();
	}
    internal void SetBaseArmor(int newBaseArmor) {
        _baseArmor = newBaseArmor;
        KingdomManager.Instance.UpdateKingdomList();
    }
    internal void ChangeStability(int newAmount) {
		this._stability = newAmount;
        this._stability = Mathf.Clamp(this._stability, -100, 100);
        //If a Kingdom has a -100 Stability, a Rebellion will automatically occur which will be started by the one with the most 
        //negative opinion towards the King. The Kingdom's Stability will then reset back to 50.
        if (_stability <= -100 && kingdomSize != KINGDOM_SIZE.SMALL) {
            StartAutomaticRebellion();
        }
    }
	internal void AdjustMilitaryAlliancePower(int amount){
		this._militaryAlliancePower += amount;
        _militaryAlliancePower = Mathf.Max(_militaryAlliancePower, 0);
    }
	private void UpdateOtherMilitaryAlliancePower(int amount){
		for (int i = 0; i < this._militaryAlliances.Count; i++) {
			this._militaryAlliances [i].AdjustMilitaryAlliancePower(amount);
		}
	}
	internal void AdjustMutualDefenseTreatyPower(int amount){
		this._mutualDefenseTreatyPower += amount;
        _mutualDefenseTreatyPower = Mathf.Max(_mutualDefenseTreatyPower, 0);
    }
	private void UpdateOtherMutualDefenseTreatyPower(int amount){
		for (int i = 0; i < this._mutualDefenseTreaties.Count; i++) {
			this._mutualDefenseTreaties [i].AdjustMutualDefenseTreatyPower(amount);
		}
	}
	private int GetMilitaryAlliancePower(){
		int militaryAlliancePower = 0;
		for (int i = 0; i < this._militaryAlliances.Count; i++) {
			militaryAlliancePower += this._militaryAlliances [i].baseWeapons;
		}
		return militaryAlliancePower;
	}
	private int GetMutualDefenseTreatyPower(){
		int mutualDefenseTreatyPower = 0;
		for (int i = 0; i < this._mutualDefenseTreaties.Count; i++) {
			mutualDefenseTreatyPower += this._mutualDefenseTreaties [i].baseArmor;
		}
		return mutualDefenseTreatyPower;
	}
	internal void AddMilitaryAlliance(Kingdom kingdom){
		this._militaryAlliances.Add (kingdom);
		AdjustMilitaryAlliancePower (kingdom.baseWeapons);
	}
	internal void RemoveMilitaryAlliance(Kingdom kingdom){
		this._militaryAlliances.Remove(kingdom);
		AdjustMilitaryAlliancePower (-kingdom.baseWeapons);
	}
	internal void AddMutualDefenseTreaty(Kingdom kingdom){
		this._mutualDefenseTreaties.Add (kingdom);
		AdjustMutualDefenseTreatyPower (kingdom.baseArmor);
	}
	internal void RemoveMutualDefenseTreaty(Kingdom kingdom){
		this._mutualDefenseTreaties.Remove(kingdom);
		AdjustMutualDefenseTreatyPower (-kingdom.baseArmor);
	}

    //	internal void AddAllianceKingdom(Kingdom kingdom){
    //		if (!this._allianceKingdoms.Contains(kingdom)) {
    //			this._allianceKingdoms.Add(kingdom);
    //		}
    //
    //	}
    //	internal void RemoveAllianceKingdom(Kingdom kingdom){
    //		this._allianceKingdoms.Remove(kingdom);
    //	}
    internal bool IsMilitaryAlliance(Kingdom kingdom){
		for (int i = 0; i < this._militaryAlliances.Count; i++) {
			if (this._militaryAlliances[i].id == kingdom.id) {
				return true;
			}
		}
		return false;
	}
	internal bool IsMutualDefenseTreaty(Kingdom kingdom){
		for (int i = 0; i < this._mutualDefenseTreaties.Count; i++) {
			if (this._mutualDefenseTreaties[i].id == kingdom.id) {
				return true;
			}
		}
		return false;
	}
	internal void UpdateCurrentDefenseTreatyRejectionDate(int month, int day, int year){
		this._currentDefenseTreatyRejectionDate.month = month;
		this._currentDefenseTreatyRejectionDate.day = day;
		this._currentDefenseTreatyRejectionDate.year = year;
	}
	internal void UpdateCurrentMilitaryAllianceRejectionDate(int month, int day, int year){
		this._currentMilitaryAllianceRejectionDate.month = month;
		this._currentMilitaryAllianceRejectionDate.day = day;
		this._currentMilitaryAllianceRejectionDate.year = year;
	}

    private void IncreaseBOPAttributesPerMonth() {
        int totalWeaponsIncrease = 0;
        int totalArmorIncrease = 0;
        int totalTechIncrease = GetTechContributionFromCitizens();
        int totalStabilityIncrease = GetStabilityContributionFromCitizens();
        for (int i = 0; i < cities.Count; i++) {
            City currCity = cities[i];
            if (!currCity.isDead && currCity.rebellion == null) {
                int weaponsContribution = currCity.powerPoints;
                int armorContribution = currCity.defensePoints;
                int techContribution = currCity.techPoints;
                currCity.MonthlyResourceBenefits(ref weaponsContribution, ref armorContribution, ref totalStabilityIncrease);
                totalWeaponsIncrease += weaponsContribution;
                totalArmorIncrease += armorContribution;
                totalTechIncrease += techContribution;
            }
        }
        //Kings and Governors provide monthly Stability gains based on their Efficiency trait.  This is reduced by the Kingdom's Draft Rate.
        totalStabilityIncrease = Mathf.FloorToInt(totalStabilityIncrease * (1f - draftRate));

        if (isMilitarize) {
            //Militarizing multiplies Weapon production by 2.5 for the month in exchange for 0 Armor and Tech production.
            totalWeaponsIncrease = Mathf.FloorToInt(totalWeaponsIncrease * 2.5f);
            totalArmorIncrease = 0;
            totalTechIncrease = 0;
            Militarize(false);
        } else if (isFortifying) {
            //Fortifying multiplies Armor production by 2.5 for the month in exchange for 0 Weapon and Tech production.
            totalArmorIncrease = Mathf.FloorToInt(totalArmorIncrease * 2.5f);
            totalWeaponsIncrease = 0;
            totalTechIncrease = 0;
            Fortify(false);
        }
        //overpopulation reduces Stability by 1 point per 10% of Overpopulation each month
        int overpopulation = GetOverpopulationPercentage();
        totalStabilityIncrease -= overpopulation / 10;
        //When occupying an invaded city, monthly Stability is reduced by 2 for six months.
        totalStabilityIncrease -= (stabilityDecreaseFromInvasionCounter * 2);

        AdjustBaseWeapons(totalWeaponsIncrease);
        AdjustBaseArmors(totalArmorIncrease);
        AdjustStability(totalStabilityIncrease);

        //Tech Gains
        totalTechIncrease = ((2 * scientists * totalTechIncrease) / (scientists + totalTechIncrease));
        totalTechIncrease = Mathf.FloorToInt(totalTechIncrease * techProductionPercentage);
        AdjustTechCounter(totalTechIncrease);

        //Reschedule event
        GameDate dueDate = new GameDate(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
        dueDate.AddMonths(1);
        SchedulingManager.Instance.AddEntry(dueDate.month, dueDate.day, dueDate.year, () => IncreaseBOPAttributesPerMonth());

    }
    internal int GetMonthlyStabilityGain() {
        int totalStabilityIncrease = GetStabilityContributionFromCitizens();
        totalStabilityIncrease = Mathf.FloorToInt(totalStabilityIncrease * (1f - draftRate));
        for (int i = 0; i < cities.Count; i++) {
            City currCity = cities[i];
            if (!currCity.isDead && currCity.rebellion == null) {
                int weaponsContribution = 0;
                int armorContribution = 0;
                currCity.MonthlyResourceBenefits(ref weaponsContribution, ref armorContribution, ref totalStabilityIncrease);
            }
        }
        //overpopulation reduces Stability by 1 point per 10% of Overpopulation each month
        int overpopulation = GetOverpopulationPercentage();
        totalStabilityIncrease -= overpopulation / 10;
        //When occupying an invaded city, monthly Stability is reduced by 2 for six months.
        totalStabilityIncrease -= (stabilityDecreaseFromInvasionCounter * 2);
        return totalStabilityIncrease;
    }
    internal void AddStabilityDecreaseBecauseOfInvasion() {
        stabilityDecreaseFromInvasionCounter += 1;
        //Reschedule event
        GameDate dueDate = new GameDate(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
        dueDate.AddMonths(6);
        SchedulingManager.Instance.AddEntry(dueDate.month, dueDate.day, dueDate.year, () => ReduceStabilityDecreaseBecauseOfInvasion());
        datesStabilityDecreaseWillExpire.Add(dueDate);
    }
    private void ReduceStabilityDecreaseBecauseOfInvasion() {
        stabilityDecreaseFromInvasionCounter -= 1;
    }
    private int GetStabilityContributionFromCitizens() {
        int stabilityContributionsFromCitizens = 0;
        stabilityContributionsFromCitizens += king.GetStabilityContribution();
        for (int i = 0; i < cities.Count; i++) {
            stabilityContributionsFromCitizens += cities[i].governor.GetStabilityContribution();
        }
        return stabilityContributionsFromCitizens;
    }
    //    internal void AdjustBaseWeapons(int adjustment) {
    //        _baseWeapons += adjustment;
    //    }
    //    internal void AdjustBaseArmor(int adjustment) {
    //        _baseArmor += adjustment;
    //    }
    internal void UpdateProductionRatesFromKing() {
        _researchRateFromKing = 0f;
        _draftRateFromKing = 0f;
        _productionRateFromKing = 0f;

        switch (king.science) {
            case SCIENCE.ERUDITE:
                _researchRateFromKing = 0.10f;
                break;
            case SCIENCE.ACADEMIC:
                _researchRateFromKing = 0.05f;
                break;
            case SCIENCE.IGNORANT:
                _researchRateFromKing = -0.05f;
                break;
            default:
                break;
        }
        _productionRateFromKing -= _researchRateFromKing;

        switch (king.military) {
            case MILITARY.HOSTILE:
                _draftRateFromKing = 0.10f;
                break;
            case MILITARY.MILITANT:
                _draftRateFromKing = 0.05f;
                break;
            case MILITARY.PACIFIST:
                _draftRateFromKing = -0.05f;
                break;
            default:
                break;
        }
        _productionRateFromKing -= _draftRateFromKing;


    }
    #endregion

    #region Adjacency
    internal void AddAdjacentKingdom(Kingdom kingdom) {
        if (!_adjacentKingdoms.Contains(kingdom)) {
            this._adjacentKingdoms.Add(kingdom);
        }
    }
    internal void RemoveAdjacentKingdom(Kingdom kingdom) {
        this._adjacentKingdoms.Remove(kingdom);
    }
    internal void RevalidateKingdomAdjacency(City removedCity) {
        List<Kingdom> kingdomsToCheck = new List<Kingdom>();
        for (int i = 0; i < removedCity.region.adjacentRegions.Count; i++) {
            Region currRegion = removedCity.region.adjacentRegions[i];
            if (currRegion.occupant != null) {
                if (currRegion.occupant.kingdom != this && !kingdomsToCheck.Contains(currRegion.occupant.kingdom)) {
                    kingdomsToCheck.Add(currRegion.occupant.kingdom);
                }
            }
        }
        for (int i = 0; i < kingdomsToCheck.Count; i++) {
            Kingdom otherKingdom = kingdomsToCheck[i];
            KingdomRelationship kr = GetRelationshipWithKingdom(otherKingdom);
            if (kr.isAdjacent) {
                bool isValid = false;
                //Revalidate adjacency
                for (int j = 0; j < cities.Count; j++) {
                    Region regionOfCurrCity = cities[j].region;
                    foreach (Region otherRegion in regionOfCurrCity.adjacentRegions.Where(x => x.occupant != null && x.occupant.kingdom.id != this.id)) {
                        if (otherRegion.occupant.kingdom.id == otherKingdom.id) {
                            //otherKingdom is still adjacent to this kingdom, validity verified!
                            isValid = true;
                            break;
                        } 
                        //else if (kingdomsToCheck.Contains(otherRegion.occupant.kingdom)) {
                        //    //otherRegion.occupant.kingdom is still adjacent to this kingdom, validity verified!
                        //    isValid = true;
                        //    break;
                        //}
                    }
                    if (isValid) {
                        //otherKingdom has already been verified! Skip checking of other cities
                        break;
                    }
                }
                //Loop of all cities is done, check if validity has returned a true value,
                //if not, this kingdom and other kingdom are no longer adjacent, change appropriately
                if (!isValid) {
                    kr.ChangeAdjacency(false);
                }
            }
        }

    }
    #endregion

    #region Population
    internal int GetOverpopulationPercentage() {
        float overpopulationPercentage = ((float)_population / (float)_populationCapacity) * 100f;
        //overpopulationPercentage = overpopulationPercentage * 100 - 100;
        overpopulationPercentage -= 100;
        overpopulationPercentage = Mathf.Clamp(overpopulationPercentage, 0f, 100f);
        return Mathf.FloorToInt(overpopulationPercentage);
    }
    internal void UpdatePopulationCapacity() {
        _populationCapacity = GetPopulationCapacity();
    }
    internal int GetPopulationCapacity() {
        int populationCapacity = 0;
        for (int i = 0; i < cities.Count; i++) {
            populationCapacity += 500 + (50 * cities[i].cityLevel);
        }
        return populationCapacity;
    }
    internal int GetPopulationGrowth() {
        int populationGrowth = 0;
        for (int i = 0; i < cities.Count; i++) {
			populationGrowth += cities[i].region.populationGrowth + (cities[i].cityLevel * 2);
        }
		populationGrowth += this.techLevel * cities.Count;
        float overpopulationPercentage = GetOverpopulationPercentage();
        int populationGrowthReduction = Mathf.FloorToInt(populationGrowth * (overpopulationPercentage/100f));
		return Mathf.FloorToInt(populationGrowth * ((100f-overpopulationPercentage) * 0.01f));
    }
    private void IncreasePopulationEveryMonth() {
        AdjustPopulation(GetPopulationGrowth());
        GameDate dueDate = new GameDate(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
        dueDate.AddMonths(1);
        SchedulingManager.Instance.AddEntry(dueDate.month, dueDate.day, dueDate.year, () => IncreasePopulationEveryMonth());
    }
    internal void AdjustPopulation(int adjustment) {
        _population += adjustment;
        if(_population <= 0) {
            //if at any time population is reduced to 0, the Kingdom will cease to exist and all his cities will be destroyed
            while (cities.Count > 0) {
                cities[0].KillCity();
            }
        }
        KingdomManager.Instance.UpdateKingdomList();
    }
    internal void SetPopulation(int newPopulation) {
        _population = newPopulation;
        KingdomManager.Instance.UpdateKingdomList();
    }
    #endregion

    #region Automatic Rebellion
    private void StartAutomaticRebellion() {
        //If a Kingdom has a -100 Stability, a Rebellion will automatically occur which will be started by the one with the most 
        //negative opinion towards the King. The Kingdom's Stability will then reset back to 50.
        List<Citizen> possibleCitizensForRebellion = new List<Citizen>();
        for (int i = 0; i < cities.Count; i++) {
            possibleCitizensForRebellion.AddRange(cities[i].importantCitizensInCity.Values.Where(x => x.role != ROLE.KING));
        }
        if (possibleCitizensForRebellion.Count > 0) {
            possibleCitizensForRebellion.OrderBy(x => x.loyaltyToKing).First().StartRebellion();
        }
    }
    #endregion

    internal void AddToNonRebellingCities(City city){
		this._nonRebellingCities.Add (city);
	}
	internal void RemoveFromNonRebellingCities(City city){
		this._nonRebellingCities.Remove (city);
	}
	internal List<City> GetNonRebellingCities(){
		List<City> nonRebels = new List<City> ();
		for (int i = 0; i < this.cities.Count; i++) {
			if(this.cities[i].rebellion == null){
				nonRebels.Add (this.cities [i]);
			}
		}
		return nonRebels;
	}

	internal void AddToMobilizationQueue(Wars war){
		this._mobilizationQueue.Add (war);
	}
	internal void RemoveFromMobilizationQueue(Wars war){
		this._mobilizationQueue.Remove (war);
	}
	internal void MobilizingState(bool state){
		this._isMobilizing = state;
	}
	internal void CheckMobilizationQueue(){
		if (this._mobilizationQueue.Count > 0) {
			this._mobilizationQueue [0].InitializeMobilization ();
			this._mobilizationQueue.RemoveAt (0);
		}else{
			MobilizingState (false);
		}
	}

	private void AdjustPowerPointsToAllCities(int amount){
		for (int i = 0; i < this.cities.Count; i++) {
			this.cities[i].AdjustPowerPoints(amount);
		}
	}

    private void AdjustDefensePointsToAllCities(int amount) {
        for (int i = 0; i < this.cities.Count; i++) {
            this.cities[i].AdjustDefensePoints(amount);
        }
    }

	internal void AdjustWarmongerValue(int amount){
		this._warmongerValue += amount;
		this._warmongerValue = Mathf.Clamp(this._warmongerValue, 0, 100);
	}

	internal void SetWarmongerValue(int amount){
		this._warmongerValue = amount;
	}

	internal void WarmongerDecreasePerYear(){
		if(!this.isDead){
			if (!HasWar ()) {
				AdjustWarmongerValue (-5);
			}
			SchedulingManager.Instance.AddEntry (1, 1, GameManager.Instance.year + 1, () => WarmongerDecreasePerYear ());
		}
	}
	internal void UpdateThreatLevelsAndInvasionValues(){
		foreach (KingdomRelationship relationship in this.relationships.Values) {
			relationship.UpdateThreatLevelAndInvasionValue ();
		}
	}

	internal void SeekAlliance(){
		List<KingdomRelationship> kingdomRelationships = this.relationships.Values.OrderByDescending(x => x.totalLike).ToList ();
		Kingdom kingdomWithHighestThreat = GetKingdomWithHighestThreat();
		if(kingdomWithHighestThreat != null){
			for (int i = 0; i < kingdomRelationships.Count; i++) {
				KingdomRelationship kr = kingdomRelationships [i];
				if(kr.isDiscovered){
					if(kr.targetKingdom.id != kingdomWithHighestThreat.id){
						KingdomRelationship rk = kr.targetKingdom.GetRelationshipWithKingdom (kingdomWithHighestThreat);
						if(rk.isAdjacent){
							if(kr.targetKingdom.alliancePool == null){
								Debug.Log(name + " is looking to create an alliance with " + kr.targetKingdom.name);
								bool hasCreated = KingdomManager.Instance.AttemptToCreateAllianceBetweenTwoKingdoms(this, kr.targetKingdom);
								if(hasCreated){
									string log = name + " has created an alliance with ";
									for (int j = 0; j < _alliancePool.kingdomsInvolved.Count; j++) {
										if(_alliancePool.kingdomsInvolved[j].id != id) {
											log += _alliancePool.kingdomsInvolved[j].name;
											if(j + 1 < _alliancePool.kingdomsInvolved.Count) {
												log += ", ";
											}
										}
									}
									Debug.Log(log);
									break;
								}
							}else{
								Debug.Log(name + " is looking to join the alliance of " + kr.targetKingdom.name);
								bool hasJoined = kr.targetKingdom.alliancePool.AttemptToJoinAlliance(this, kr.targetKingdom);
								if(hasJoined){
									string log = name + " has joined an alliance with ";
									for (int j = 0; j < _alliancePool.kingdomsInvolved.Count; j++) {
										if (_alliancePool.kingdomsInvolved[j].id != id) {
											log += _alliancePool.kingdomsInvolved[j].name;
											if (j + 1 < _alliancePool.kingdomsInvolved.Count) {
												log += ", ";
											}
										}
									}
									break;
								}
							}
						}
					}
				}
			}
		}
		if(this._alliancePool == null){
			for (int i = 0; i < kingdomRelationships.Count; i++) {
				KingdomRelationship kr = kingdomRelationships [i];
				if(kr.isDiscovered){
					if(kr.targetKingdom.alliancePool == null){
						Debug.Log(name + " is looking to create an alliance with " + kr.targetKingdom.name);
						bool hasCreated = KingdomManager.Instance.AttemptToCreateAllianceBetweenTwoKingdoms(this, kr.targetKingdom);
						if(hasCreated){
							string log = name + " has created an alliance with ";
							for (int j = 0; j < _alliancePool.kingdomsInvolved.Count; j++) {
								if(_alliancePool.kingdomsInvolved[j].id != id) {
									log += _alliancePool.kingdomsInvolved[j].name;
									if(j + 1 < _alliancePool.kingdomsInvolved.Count) {
										log += ", ";
									}
								}
							}
							Debug.Log(log);
							break;
						}
					}else{
						Debug.Log(name + " is looking to join the alliance of " + kr.targetKingdom.name);
						bool hasJoined = kr.targetKingdom.alliancePool.AttemptToJoinAlliance(this, kr.targetKingdom);
						if(hasJoined){
							string log = name + " has joined an alliance with ";
							for (int j = 0; j < _alliancePool.kingdomsInvolved.Count; j++) {
								if (_alliancePool.kingdomsInvolved[j].id != id) {
									log += _alliancePool.kingdomsInvolved[j].name;
									if (j + 1 < _alliancePool.kingdomsInvolved.Count) {
										log += ", ";
									}
								}
							}
							break;
						}
					}
				}
			}
		}
        if(_alliancePool == null) {
            Debug.Log(name + " has failed to create/join an alliance");
        }
	}
	internal void SeekAllianceWith(Kingdom targetKingdom){
		if(targetKingdom.alliancePool == null){
			Debug.Log(name + " is looking to create an alliance with " + targetKingdom.name);
			bool hasCreated = KingdomManager.Instance.AttemptToCreateAllianceBetweenTwoKingdoms(this, targetKingdom);
			if(hasCreated){
				string log = name + " has created an alliance with ";
				for (int j = 0; j < _alliancePool.kingdomsInvolved.Count; j++) {
					if(_alliancePool.kingdomsInvolved[j].id != id) {
						log += _alliancePool.kingdomsInvolved[j].name;
						if(j + 1 < _alliancePool.kingdomsInvolved.Count) {
							log += ", ";
						}
					}
				}
				Debug.Log(log);
			}
		}else{
			Debug.Log(name + " is looking to join the alliance of " + targetKingdom.name);
			bool hasJoined = targetKingdom.alliancePool.AttemptToJoinAlliance(this, targetKingdom);
			if(hasJoined){
				string log = name + " has joined an alliance with ";
				for (int j = 0; j < _alliancePool.kingdomsInvolved.Count; j++) {
					if (_alliancePool.kingdomsInvolved[j].id != id) {
						log += _alliancePool.kingdomsInvolved[j].name;
						if (j + 1 < _alliancePool.kingdomsInvolved.Count) {
							log += ", ";
						}
					}
				}
			}
		}
	}
	internal void SetAlliancePool(AlliancePool alliancePool){
		this._alliancePool = alliancePool;
	}
	internal int GetUnadjacentPosAllianceWeapons(){
		int posAlliancePower = 0;
		if(this.alliancePool != null){
			for (int i = 0; i < this.alliancePool.kingdomsInvolved.Count; i++) {
				Kingdom kingdomInAlliance = this.alliancePool.kingdomsInvolved[i];
				if(this.id != kingdomInAlliance.id){
					KingdomRelationship relationship = kingdomInAlliance.GetRelationshipWithKingdom(this);
					if(relationship.totalLike >= 35){
						posAlliancePower += (int)((float)kingdomInAlliance.baseWeapons * 0.1f);
					}
				}
			}
        }
		return posAlliancePower;
	}
	internal int GetUnadjacentPosAllianceArmor(){
		int posAllianceDefense = 0;
		if(this.alliancePool != null){
			for (int i = 0; i < this.alliancePool.kingdomsInvolved.Count; i++) {
				Kingdom kingdomInAlliance = this.alliancePool.kingdomsInvolved[i];
				if(this.id != kingdomInAlliance.id){
					KingdomRelationship relationship = kingdomInAlliance.GetRelationshipWithKingdom(this);
					if(relationship.totalLike >= 35){
						posAllianceDefense += (int)((float)kingdomInAlliance.baseArmor * 0.1f);
					}
				}
			}
        }
//		foreach (KingdomRelationship relationship in this.relationships.Values) {
////			if(relationship.isAlly){
//				KingdomRelationship relationshipFrom = relationship.targetKingdom.GetRelationshipWithKingdom (this);
//				if(relationshipFrom.totalLike >= 35){
//					posAllianceDefense += relationship.targetKingdom.baseDefense;
//				}
////			}
//		}
		return posAllianceDefense;
	}
	internal void AddWarfareInfo(WarfareInfo info){
		if(!this._warfareInfo.ContainsKey(info.warfare.id)){
			this._warfareInfo.Add(info.warfare.id, info);
		}
	}
	internal void RemoveWarfareInfo(Warfare warfare){
		this._warfareInfo.Remove(warfare.id);
	}
	internal WarfareInfo GetWarfareInfo(int id){
		if(this._warfareInfo.ContainsKey(id)){
			return this._warfareInfo[id];
		}
		return new WarfareInfo(WAR_SIDE.NONE, null);
	}
	internal void LeaveAlliance(bool doNotShowLog = false){
		if(this.alliancePool != null){
            AlliancePool leftAlliance = this.alliancePool;
            //List<Kingdom> kingdomsInvolved = new List<Kingdom>();
            //for (int i = 0; i < leftAlliance.kingdomsInvolved.Count; i++) {
            //    Kingdom kingdomInvolved = leftAlliance.kingdomsInvolved[i];
            //    kingdomsInvolved.Add(kingdomInvolved);
            //    if (kingdomInvolved.warfareInfo.Count > 0) {
            //        foreach (WarfareInfo currWarFare in kingdomInvolved.warfareInfo.Values) {
            //            foreach (List<Kingdom> kingdomsToAdd in currWarFare.warfare.kingdomSideList.Values) {
            //                kingdomsInvolved.AddRange(kingdomsToAdd);
            //            }
            //        }
            //    }
            //}

            this.alliancePool.RemoveKingdomInAlliance(this);
			//When leaving an alliance, Stability is reduced by 15
			this.AdjustStability(-15);
			if(!doNotShowLog){
				Log newLog = new Log (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Alliance", "leave_alliance");
				newLog.AddToFillers (this, this.name, LOG_IDENTIFIER.KINGDOM_1);
                newLog.AddToFillers(null, leftAlliance.name, LOG_IDENTIFIER.ALLIANCE_NAME);
				UIManager.Instance.ShowNotification (newLog);
			}
		}
	}
	internal bool IsUnderAttack(){
		for (int i = 0; i < this.cities.Count; i++) {
			if(this.cities[i].isDefending){
				return true;
			}
		}
		return false;
	}
	internal bool IsAttacking(){
		for (int i = 0; i < this.cities.Count; i++) {
			if(this.cities[i].isAttacking){
				return true;
			}
		}
		return false;
	}
	internal string ProvideWeaponsArmorsAidToKingdom(Kingdom kingdomToBeProvided, float transferPercentage){
		string weaponsOrArmor = "Weapons";
		bool isAllyUnderAttack = kingdomToBeProvided.IsUnderAttack ();
		bool isAllyAttacking = kingdomToBeProvided.IsAttacking ();
		int transferAmount = 0;
		if(isAllyUnderAttack && isAllyAttacking){
			weaponsOrArmor = "Armors";
		}else{
			if(isAllyUnderAttack){
				weaponsOrArmor = "Armors";
			}
		}

		if(weaponsOrArmor == "Weapons"){
			transferAmount = (int)(this.baseWeapons * transferPercentage);
			this.AdjustBaseWeapons (-transferAmount);
			kingdomToBeProvided.AdjustBaseWeapons (transferAmount);
		}else{
			transferAmount = (int)(this.baseArmor * transferPercentage);
			this.AdjustBaseArmors (-transferAmount);
			kingdomToBeProvided.AdjustBaseArmors (transferAmount);
		}

		return transferAmount.ToString () + " " + weaponsOrArmor;
	}
	internal Kingdom GetKingdomWithHighestThreat(){
		float highestThreatLevel = 0f;
		Kingdom threat = null;
		foreach (KingdomRelationship kr in relationships.Values) {
			if(kr.targetKingdomThreatLevel > highestThreatLevel){
				highestThreatLevel = kr.targetKingdomThreatLevel;
				threat = kr.targetKingdom;
			}
		}
		return threat;
	}
	internal void ShowTransferWeaponsArmorsLog(Kingdom allyKingdom, string amount){
        List<Kingdom> kingdomsToShowNotif = new List<Kingdom>();
        foreach (WarfareInfo currWarFare in this.warfareInfo.Values) {
            foreach (List<Kingdom> kingdomsInvolved in currWarFare.warfare.kingdomSideList.Values) {
                kingdomsToShowNotif.AddRange(kingdomsInvolved);
            }
        }
        foreach (WarfareInfo currWarFare in allyKingdom.warfareInfo.Values) {
            foreach (List<Kingdom> kingdomsInvolved in currWarFare.warfare.kingdomSideList.Values) {
                kingdomsToShowNotif.AddRange(kingdomsInvolved);
            }
        }
        Log newLog = new Log (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "General", "Kingdom", "transfer_weapons_armors");
		newLog.AddToFillers (this, this.name, LOG_IDENTIFIER.KINGDOM_1);
		newLog.AddToFillers (allyKingdom, allyKingdom.name, LOG_IDENTIFIER.KINGDOM_2);
		newLog.AddToFillers (null, amount, LOG_IDENTIFIER.OTHER);
		UIManager.Instance.ShowNotification (newLog, new HashSet<Kingdom>(kingdomsToShowNotif));
	}
	internal void ShowJoinWarLog(Kingdom allyKingdom, Warfare warfare){
        List<Kingdom> kingdomsToShowNotif = new List<Kingdom>();
        kingdomsToShowNotif.AddRange(warfare.GetListFromSide(WAR_SIDE.A));
        kingdomsToShowNotif.AddRange(warfare.GetListFromSide(WAR_SIDE.B));
        foreach (WarfareInfo currWarFare in allyKingdom.warfareInfo.Values) {
            foreach (List<Kingdom> kingdomsInvolved in currWarFare.warfare.kingdomSideList.Values) {
                kingdomsToShowNotif.AddRange(kingdomsInvolved);
            }
        }
        Log newLog = new Log (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "General", "Kingdom", "join_war");
		newLog.AddToFillers (this, this.name, LOG_IDENTIFIER.KINGDOM_1);
		newLog.AddToFillers (null, warfare.name, LOG_IDENTIFIER.WAR_NAME);
		newLog.AddToFillers (allyKingdom, allyKingdom.name, LOG_IDENTIFIER.KINGDOM_2);
		UIManager.Instance.ShowNotification (newLog, new HashSet<Kingdom>(kingdomsToShowNotif));
	}
	internal void ShowRefuseAndLeaveAllianceLog(AlliancePool alliance, Warfare warfare){
        List<Kingdom> kingdomsToShowNotif = new List<Kingdom>();
        kingdomsToShowNotif.Add(this);
        kingdomsToShowNotif.AddRange(warfare.GetListFromSide(WAR_SIDE.A));
        kingdomsToShowNotif.AddRange(warfare.GetListFromSide(WAR_SIDE.B));
        kingdomsToShowNotif.AddRange(alliance.kingdomsInvolved);
        Log newLog = new Log (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "General", "Kingdom", "refuse_and_leave_alliance");
		newLog.AddToFillers (this, this.name, LOG_IDENTIFIER.KINGDOM_1);
		newLog.AddToFillers (null, warfare.name, LOG_IDENTIFIER.WAR_NAME);
		newLog.AddToFillers (null, alliance.name, LOG_IDENTIFIER.ALLIANCE_NAME);
		UIManager.Instance.ShowNotification (newLog, new HashSet<Kingdom>(kingdomsToShowNotif));
	}
	internal void ShowDoNothingLog(Warfare warfare){
        List<Kingdom> kingdomsToShowNotif = new List<Kingdom>();
        kingdomsToShowNotif.AddRange(warfare.GetListFromSide(WAR_SIDE.A));
        kingdomsToShowNotif.AddRange(warfare.GetListFromSide(WAR_SIDE.B));
        Log newLog = new Log (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "General", "Kingdom", "do_nothing_war");
		newLog.AddToFillers (this, this.name, LOG_IDENTIFIER.KINGDOM_1);
		newLog.AddToFillers (null, warfare.name, LOG_IDENTIFIER.WAR_NAME);
		UIManager.Instance.ShowNotification (newLog, new HashSet<Kingdom>(kingdomsToShowNotif));
	}
	internal void ShowBetrayalWarLog(Warfare warfare, Kingdom kingdom){
        List<Kingdom> kingdomsToShowNotif = new List<Kingdom>();
        kingdomsToShowNotif.Add(kingdom);
        kingdomsToShowNotif.AddRange(warfare.GetListFromSide(WAR_SIDE.A));
        kingdomsToShowNotif.AddRange(warfare.GetListFromSide(WAR_SIDE.B));
        foreach (WarfareInfo currWarFare in kingdom.warfareInfo.Values) {
            foreach (List<Kingdom> kingdomsInvolved in currWarFare.warfare.kingdomSideList.Values) {
                kingdomsToShowNotif.AddRange(kingdomsInvolved);
            }
        }
        Log newLog = new Log (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "General", "Kingdom", "betrayal_war");
		newLog.AddToFillers (this, this.name, LOG_IDENTIFIER.KINGDOM_1);
		newLog.AddToFillers (null, warfare.name, LOG_IDENTIFIER.WAR_NAME);
		newLog.AddToFillers (kingdom, kingdom.name, LOG_IDENTIFIER.KINGDOM_2);
		UIManager.Instance.ShowNotification (newLog, new HashSet<Kingdom>(kingdomsToShowNotif));
	}
	internal void ShowBetrayalProvideLog(AlliancePool alliance, string logAmount, Kingdom kingdom){
        List<Kingdom> kingdomsToShowNotif = new List<Kingdom>();
        kingdomsToShowNotif.Add(this);
        kingdomsToShowNotif.Add(kingdom);
        kingdomsToShowNotif.AddRange(alliance.kingdomsInvolved);

        foreach (WarfareInfo currWarFare in this.warfareInfo.Values) {
            foreach (List<Kingdom> kingdomsInvolved in currWarFare.warfare.kingdomSideList.Values) {
                kingdomsToShowNotif.AddRange(kingdomsInvolved);
            }
        }
        foreach (WarfareInfo currWarFare in kingdom.warfareInfo.Values) {
            foreach (List<Kingdom> kingdomsInvolved in currWarFare.warfare.kingdomSideList.Values) {
                kingdomsToShowNotif.AddRange(kingdomsInvolved);
            }
        }
        Log newLog = new Log (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "General", "Kingdom", "betrayal_provide");
		newLog.AddToFillers (this, this.name, LOG_IDENTIFIER.KINGDOM_1);
		newLog.AddToFillers (null, alliance.name, LOG_IDENTIFIER.ALLIANCE_NAME);
		newLog.AddToFillers (null, logAmount, LOG_IDENTIFIER.OTHER);
		newLog.AddToFillers (kingdom, kingdom.name, LOG_IDENTIFIER.KINGDOM_2);
		UIManager.Instance.ShowNotification (newLog, new HashSet<Kingdom>(kingdomsToShowNotif));
	}
}
