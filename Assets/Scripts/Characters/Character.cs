using Characters.Components;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Interrupts;
using JetBrains.Annotations;
using Locations;
using Locations.Settlements;
using Object_Pools;
using Plague.Transmission;
using System;
using System.Collections.Generic;
using System.Linq;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.Profiling;
using UtilityScripts;
using Factions.Faction_Types;

public class Character : Relatable, ILeader, IPointOfInterest, IJobOwner, IPlayerActionTarget, IObjectManipulator, IPartyQuestTarget, IGatheringTarget, IStoredTarget {
    private int _id;
    private string _firstName;
    private string _surName;
    protected bool _isDead;
    private GENDER _gender;
    private RaceData _raceSetting;
    private Faction _faction;
    private Minion _minion;
    private LocationStructure _currentStructure; //what structure is this character currently in.
    private Region _currentRegion;
    public LocationGridTile deathTilePosition { protected set; get; }
    public string persistentID { get; private set; }
    //visuals
    public CharacterVisuals visuals { get; }
    public int currentHP { get; protected set; }
    public int doNotRecoverHP { get; protected set; }
    public SEXUALITY sexuality { get; private set; }
    public bool canCombat { get; private set; } //This should only be a getter but since we need to know when the value changes it now has a setter
    public string deathStr { get; private set; }
    public int numOfActionsBeingPerformedOnThis { get; private set; } //this is increased, when the action of another character stops this characters movement
    public Region homeRegion { get; protected set; }
    public NPCSettlement homeSettlement { get; protected set; }
    public LocationStructure homeStructure { get; protected set; }
    public CharacterMarker marker { get; private set; }
    public JobQueueItem currentJob { get; private set; }
    public GoapPlan currentPlan { get; private set; }
    public ActualGoapNode currentActionNode { get; private set; }
    public JobQueue jobQueue { get; private set; }
    public TileObject tileObjectLocation { get; private set; }
    public CharacterTrait defaultCharacterTrait { get; private set; }
    public List<INTERACTION_TYPE> advertisedActions { get; private set; }
    public List<TileObject> items { get; private set; }
    public List<TileObject> equipmentInventory { get; private set; }
    public List<TileObject> ownedItems { get; private set; }
    public List<JobQueueItem> allJobsTargetingThis { get; private set; }
    public List<Trait> traitsNeededToBeRemoved { get; private set; }
    public bool hasUnresolvedCrime { get; protected set; }
    public bool isConversing { get; protected set; }
    public bool isInLimbo { get; protected set; }
    public bool isLimboCharacter { get; protected set; }
    public bool hasSeenFire { get; protected set; }
    public bool hasSeenWet { get; protected set; }
    public bool hasSeenPoisoned { get; protected set; }
    public bool destroyMarkerOnDeath { get; protected set; }
    public bool isWanderer { get; private set; }
    public bool hasBeenRaisedFromDead { get; private set; }
    public bool hasSubscribedToSignals { get; private set; }
    public bool shouldDoActionOnFirstTickUponLoadGame { get; private set; } //This should not be saved. Upon loading the game, this is always set to true so that if the character has a saved current action, it should resume on first tick
    public bool isPreplaced { get; private set; }
    public bool isStoredAsTarget { get; private set; }
    public bool isDeadReference { get; private set; }

    public bool isWildMonster { get; protected set; }
    public Log deathLog { get; private set; }
    public List<string> interestedItemNames { get; private set; }
    public List<JobQueueItem> forcedCancelJobsOnTickEnded { get; private set; }
    public Area territory { get; private set; }
    public LycanthropeData lycanData { get; protected set; }
    public Necromancer necromancerTrait { get; protected set; }
    public POI_STATE state { get; private set; }
    public ILocationAwareness currentLocationAwareness { get; private set; }
    public Vector2Int gridTileLocalPosition { get; private set; }
    public Vector3 gridTileWorldPosition { get; private set; }
    public bool hasMarker { get; private set; }
    public List<PLAYER_SKILL_TYPE> afflictionsSkillsInflictedByPlayer { get; set; }
    public LocationStructure deployedAtStructure { get; private set; }
    //public bool isInPendingAwarenessList { get; private set; }
    //misc
    public Tombstone grave { get; private set; }
    public INTERACTION_TYPE causeOfDeath { set; get; }
    public PLAYER_SKILL_TYPE skillCauseOfDeath { set; get; }

    //Components / Managers
    public TrapStructure trapStructure { get; private set; }
    public GoapPlanner planner { get; private set; }
    public CharacterClassComponent classComponent { get; private set; }
    public CharacterNeedsComponent needsComponent { get; private set; }
    public CharacterStructureComponent structureComponent { get; private set; }
    public CharacterStateComponent stateComponent { get; private set; }
    public NonActionEventsComponent nonActionEventsComponent { get; private set; }
    public InterruptComponent interruptComponent { get; private set; }
    public BehaviourComponent behaviourComponent { get; private set; }
    public MoodComponent moodComponent { get; private set; }
    public CharacterJobTriggerComponent jobComponent { get; private set; }
    public ReactionComponent reactionComponent { get; private set; }
    public LogComponent logComponent { get; private set; }
    public CombatComponent combatComponent { get; protected set; }
    public RumorComponent rumorComponent { get; private set; }
    public AssumptionComponent assumptionComponent { get; private set; }
    public MovementComponent movementComponent { get; private set; }
    public StateAwarenessComponent stateAwarenessComponent { get; private set; }
    public CarryComponent carryComponent { get; private set; }
    public CharacterPartyComponent partyComponent { get; private set; }
    public GatheringComponent gatheringComponent { get; private set; }
    public CharacterTileObjectComponent tileObjectComponent { get; private set; }
    public CrimeComponent crimeComponent { get; private set; }
    public ReligionComponent religionComponent { get; private set; }
    public LimiterComponent limiterComponent { get; private set; }
    public PiercingAndResistancesComponent piercingAndResistancesComponent { get; private set; }
    public CharacterEventDispatcher eventDispatcher { get; private set; }
    public PreviousCharacterDataComponent previousCharacterDataComponent { get; private set; }
    public CharacterTraitComponent traitComponent { get; private set; }
    public BookmarkableEventDispatcher bookmarkEventDispatcher { get; private set; }
    //public BuffStatsBonus buffStatsBonus { get; private set; }
    public EquipmentComponent equipmentComponent { get; private set; }
    public CharacterMoneyComponent moneyComponent { get; private set; }
    public ResourceStorageComponent resourceStorageComponent { get; private set; }
    public DailyScheduleComponent dailyScheduleComponent { get; private set; }

    //IMPORTANT: This component is not applicable to all characters, only VILLAGERS! So, this can be null if the character is NOT A VILLAGER.
    public CharacterTalentComponent talentComponent { get; private set; }
    //IMPORTANT: This component is not applicable to all characters, only VILLAGERS! So, this can be null if the character is NOT A VILLAGER.
    public VillagerWantsComponent villagerWantsComponent { get; private set; }

    #region getters / setters
    public string bookmarkName => lycanData != null ? lycanData.activeForm.visuals.GetCharacterNameWithIconAndColor() : visuals.GetCharacterNameWithIconAndColor();
    public BOOKMARK_TYPE bookmarkType => BOOKMARK_TYPE.Text_With_Cancel;
    public OBJECT_TYPE objectType => OBJECT_TYPE.Character;
    public STORED_TARGET_TYPE storedTargetType => this is Summon ? STORED_TARGET_TYPE.Monster : STORED_TARGET_TYPE.Character;
    public bool isTargetted { set; get; }
    public string iconRichText => lycanData != null ? lycanData.activeForm.visuals.GetCharacterStringIcon() : visuals.GetCharacterStringIcon();
    public virtual Type serializedData => typeof(SaveDataCharacter);
    public virtual string name => _firstName;
    public virtual string raceClassName => GetDefaultRaceClassName();
    public override string relatableName => _firstName;
    public override int id => _id;
    public override GENDER gender => _gender;
    public string fullName => $"{_firstName} {_surName}";
    public string firstName => _firstName;
    public string firstNameWithColor => GetFirstNameWithColor();
    public string surName => _surName;
    public string nameWithID => name;
    public bool isDead => _isDead;
    public bool isFactionLeader => faction != null && faction.leader == this;
    public bool isHoldingItem => items.Count > 0;
    public bool isAtHomeRegion => currentRegion == homeRegion && !carryComponent.masterCharacter.movementComponent.isTravellingInWorld;
    public bool isAtHomeStructure => currentStructure == homeStructure && homeStructure != null;
    public bool isPartOfHomeFaction => homeRegion != null && faction != null && homeRegion.IsFactionHere(faction); //is this character part of the faction that owns his home npcSettlement
    public bool isFactionless => faction == null || FactionManager.Instance.neutralFaction == faction;
    public bool isSettlementRuler => homeSettlement != null && homeSettlement.ruler == this;
    public bool isHidden => reactionComponent.isHidden;
    public bool isBeingSeized => PlayerManager.Instance.player != null && PlayerManager.Instance.player.seizeComponent.seizedPOI == this;
    public bool isLycanthrope => lycanData != null;
    public bool isInWerewolfForm => isLycanthrope && lycanData.isInWerewolfForm;
    public bool isInVampireBatForm => IsInVampireBatForm();
    /// <summary>
    /// Is this character a normal character?
    /// Characters that are not monsters or minions.
    /// </summary>
    public bool isNormalCharacter => (this is Summon) == false && minion == null && !characterClass.IsZombie(); //&& faction?.factionType.type != FACTION_TYPE.Undead; Removed this because we now want to consider the necromancer as a villager again. As per marvin, will test this to check if anything major will change.
    public bool isNormalAndNotAlliedWithPlayer => isNormalCharacter && !isAlliedWithPlayer;
    public bool isNormalEvenLycanAndNotAlliedWithPlayer => (isNormalCharacter || isLycanthrope) && necromancerTrait == null && !faction.isPlayerFaction && !isAlliedWithPlayer;
    public bool isNotSummonAndDemon => (this is Summon) == false && minion == null;
    public bool isNotSummonAndDemonAndZombie => (this is Summon) == false && minion == null && characterClass.IsZombie();
    public bool isConsideredRatman => faction?.factionType.type == FACTION_TYPE.Ratmen && race == RACE.RATMAN;
    public bool isMonsterOrRatmanOrUndead {
        get {
            if (faction != null) {
                return (race == RACE.RATMAN || this is Summon || faction.factionType.type == FACTION_TYPE.Undead) && faction.factionType.type != FACTION_TYPE.Demons && !faction.isMajorFaction;
            }
            return false;
        }
    }
    public bool canBeTargetedByLandActions => !movementComponent.isFlying && !reactionComponent.isHidden && !traitContainer.HasTrait("Disabler", "DeMooder");

    public int maxHP => combatComponent.maxHP;

    public bool isInfoUnlocked { set; get; }
    public Vector3 worldPosition => marker.transform.position;
    public Vector2 selectableSize => visuals.selectableSize;
    public Vector3 attackRangePosition => worldPosition;
    public Transform worldObject {
        get {
            if (hasMarker) {
                return marker.transform;
            }
            return null;
        }
    }
    public POINT_OF_INTEREST_TYPE poiType => POINT_OF_INTEREST_TYPE.CHARACTER;
    public RACE race => _raceSetting.race;
    public JOB_OWNER ownerType => JOB_OWNER.CHARACTER;
    public CharacterClass characterClass => classComponent.characterClass;
    public RaceData raceSetting => _raceSetting;
    public Faction faction => _faction;
    public Faction factionOwner => _faction;
    public Minion minion => _minion;
    public BaseSettlement currentSettlement => gridTileLocation != null ? areaLocation.settlementOnArea : null;
    public ProjectileReceiver projectileReceiver {
        get {
            if (hasMarker && marker.visionTrigger != null) {
                return marker.visionTrigger.projectileReceiver;
            }
            return null;
        }
    }
    public Character isBeingCarriedBy => carryComponent.isBeingCarriedBy;
    public JobTriggerComponent jobTriggerComponent => jobComponent;
    public GameObject visualGO => marker.gameObject;
    public Character characterOwner => null;
    public BaseMapObjectVisual mapObjectVisual => marker;
    /// <summary>
    /// Is this character allied with the player? Whether secretly (not part of player faction)
    /// or openly (part of player faction).
    /// </summary>
    public bool isAlliedWithPlayer => IsAlliedWithPlayer();
    public bool isNotHostileWithPlayer => IsNotHostileWithPlayer();

    public Region currentRegion {
        get {
            Character carrier = carryComponent.isBeingCarriedBy;
            if (carrier != null) {
                return carrier.currentRegion;
            }
            return _currentRegion;
        }
    }
    public LocationGridTile gridTileLocation {
        get {
            if (!hasMarker) {
                return null;
            }
            Character carrier = carryComponent.isBeingCarriedBy;
            if (carrier != null) {
                return carrier.gridTileLocation;
            }
            return GetLocationGridTileByXY(gridTileLocalPosition.x, gridTileLocalPosition.y);
        }
    }
    public Area areaLocation => gridTileLocation?.area;
    public LocationStructure currentStructure {
        get {
            Character carrier = carryComponent.isBeingCarriedBy;
            if (carrier != null) {
                return carrier.currentStructure;
            }
            return _currentStructure;
        }
    }
    public bool isVagrant => faction != null && faction?.factionType.type == FACTION_TYPE.Vagrants;
    /// <summary>
    /// Is the character part of the neutral faction? or no faction?
    /// </summary>
    public bool isVagrantOrFactionless => faction == null || FactionManager.Instance.vagrantFaction == faction; //is the character part of the friendly neutral faction? or no faction?
    public Faction prevFaction => previousCharacterDataComponent.previousFaction;
    #endregion

    public Character(string className, RACE race, GENDER gender, SEXUALITY sexuality, int id = -1) : this() {
        skillCauseOfDeath = PLAYER_SKILL_TYPE.NONE;
        persistentID = UtilityScripts.Utilities.GetNewUniqueID();
        _id = id == -1 ? UtilityScripts.Utilities.SetID(this) : UtilityScripts.Utilities.SetID(this, id);
        _gender = gender;
        classComponent.AssignClass(className, true);
        AssignRace(race, true);
        SetSexuality(sexuality);
        visuals = new CharacterVisuals(this);
        visuals.Initialize();
        needsComponent.UpdateBaseStaminaDecreaseRate();
        combatComponent.UpdateBasicData(true);
        structureComponent = new CharacterStructureComponent(); structureComponent.SetOwner(this);
        afflictionsSkillsInflictedByPlayer = new List<PLAYER_SKILL_TYPE>();
        combatComponent.ApplyInitialBonusStrengthAndIntelligenceOnCreation();
    }
    public Character(string className, RACE race, GENDER gender) : this() {
        skillCauseOfDeath = PLAYER_SKILL_TYPE.NONE;
        persistentID = UtilityScripts.Utilities.GetNewUniqueID();
        _id = UtilityScripts.Utilities.SetID(this);
        _gender = gender;
        classComponent.AssignClass(className, true);
        AssignRace(race, true);
        GenerateSexuality();
        visuals = new CharacterVisuals(this);
        visuals.Initialize();
        needsComponent.UpdateBaseStaminaDecreaseRate();
        combatComponent.UpdateBasicData(true);
        structureComponent = new CharacterStructureComponent(); structureComponent.SetOwner(this);
        afflictionsSkillsInflictedByPlayer = new List<PLAYER_SKILL_TYPE>();
        combatComponent.ApplyInitialBonusStrengthAndIntelligenceOnCreation();
    }
    private Character() {
        SetIsDead(false);
        //_overrideThoughts = new List<string>();
        //previousClassName = string.Empty; //Moved to CharacterClassComponent

        //Traits
        CreateTraitContainer();
        skillCauseOfDeath = PLAYER_SKILL_TYPE.NONE;
        advertisedActions = new List<INTERACTION_TYPE>();
        items = new List<TileObject>();
        equipmentInventory = new List<TileObject>();
        ownedItems = new List<TileObject>();
        allJobsTargetingThis = new List<JobQueueItem>();
        traitsNeededToBeRemoved = new List<Trait>();
        forcedCancelJobsOnTickEnded = new List<JobQueueItem>();
        interestedItemNames = new List<string>();
        SetPOIState(POI_STATE.ACTIVE);

        jobQueue = new JobQueue(this);
        trapStructure = new TrapStructure();
        planner = new GoapPlanner(this);
        jobComponent = new CharacterJobTriggerComponent(); jobComponent.SetOwner(this);
        logComponent = new LogComponent(); //logComponent.SetOwner(this);

        //Components
        classComponent = new CharacterClassComponent(); classComponent.SetOwner(this);
        needsComponent = new CharacterNeedsComponent(); needsComponent.SetOwner(this);
        stateComponent = new CharacterStateComponent(); stateComponent.SetOwner(this);
        nonActionEventsComponent = new NonActionEventsComponent(); nonActionEventsComponent.SetOwner(this);
        interruptComponent = new InterruptComponent(); interruptComponent.SetOwner(this);
        behaviourComponent = new BehaviourComponent(); behaviourComponent.SetOwner(this);
        moodComponent = new MoodComponent(); moodComponent.SetOwner(this);
        reactionComponent = new ReactionComponent(); reactionComponent.SetOwner(this);
        combatComponent = new CombatComponent(); combatComponent.SetOwner(this);
        rumorComponent = new RumorComponent(); rumorComponent.SetOwner(this);
        assumptionComponent = new AssumptionComponent(); assumptionComponent.SetOwner(this);
        movementComponent = new MovementComponent(); movementComponent.SetOwner(this);
        stateAwarenessComponent = new StateAwarenessComponent(); stateAwarenessComponent.SetOwner(this);
        carryComponent = new CarryComponent(); carryComponent.SetOwner(this);
        partyComponent = new CharacterPartyComponent(); partyComponent.SetOwner(this);
        gatheringComponent = new GatheringComponent(); gatheringComponent.SetOwner(this);
        tileObjectComponent = new CharacterTileObjectComponent(); tileObjectComponent.SetOwner(this);
        crimeComponent = new CrimeComponent(); crimeComponent.SetOwner(this);
        religionComponent = new ReligionComponent(); religionComponent.SetOwner(this);
        limiterComponent = new LimiterComponent(); limiterComponent.SetOwner(this);
        piercingAndResistancesComponent = new PiercingAndResistancesComponent(); piercingAndResistancesComponent.SetOwner(this);
        previousCharacterDataComponent = new PreviousCharacterDataComponent(); previousCharacterDataComponent.SetOwner(this);
        traitComponent = new CharacterTraitComponent(); traitComponent.SetOwner(this);
        moneyComponent = new CharacterMoneyComponent(); moneyComponent.SetOwner(this);
        eventDispatcher = new CharacterEventDispatcher();
        bookmarkEventDispatcher = new BookmarkableEventDispatcher();
        //buffStatsBonus = new BuffStatsBonus();
        equipmentComponent = new EquipmentComponent();
        // needsComponent.ResetSleepTicks();
        resourceStorageComponent = new ResourceStorageComponent();
        dailyScheduleComponent = new DailyScheduleComponent(); dailyScheduleComponent.SetOwner(this);
    }
    public Character(SaveDataCharacter data) {
        skillCauseOfDeath = PLAYER_SKILL_TYPE.NONE;
        shouldDoActionOnFirstTickUponLoadGame = true;
        advertisedActions = new List<INTERACTION_TYPE>();
        items = new List<TileObject>();
        equipmentInventory = new List<TileObject>();
        ownedItems = new List<TileObject>();
        allJobsTargetingThis = new List<JobQueueItem>();
        traitsNeededToBeRemoved = new List<Trait>();
        forcedCancelJobsOnTickEnded = new List<JobQueueItem>();
        interestedItemNames = new List<string>();

        jobQueue = new JobQueue(this);
        planner = new GoapPlanner(this);
        visuals = new CharacterVisuals(this, data);
        //_characterClass = CharacterManager.Instance.CreateNewCharacterClass(data.className); //Moved to CharacterClassComponent
        _raceSetting = RaceManager.Instance.GetRaceData(data.race);
        CreateTraitContainer();

        persistentID = data.persistentID;
        _id = data.id;
        _firstName = data.firstName;
        _surName = data.surName;
        _isDead = data.isDead;
        _gender = data.gender;
        sexuality = data.sexuality;
        currentHP = data.currentHP;
        doNotRecoverHP = data.doNotRecoverHP;
        advertisedActions = new List<INTERACTION_TYPE>(data.advertisedActions);
        canCombat = data.canCombat;
        deathStr = data.deathStr;
        hasUnresolvedCrime = data.hasUnresolvedCrime;
        isInLimbo = data.isInLimbo;
        isLimboCharacter = data.isLimboCharacter;
        destroyMarkerOnDeath = data.destroyMarkerOnDeath;
        isWanderer = data.isWanderer;
        hasBeenRaisedFromDead = data.hasBeenRaisedFromDead;
        interestedItemNames = data.interestedItemNames;
        state = data.state;
        causeOfDeath = data.causeOfDeath;
        //previousClassName = data.classComponent.previousClassName; //Moved to CharacterClassComponent
        isPreplaced = data.isPreplaced;
        isStoredAsTarget = data.isStoredAsTarget;
        isDeadReference = data.isDeadReference;

        if (data.afflictionsSkillsInflictedByPlayer != null) {
            afflictionsSkillsInflictedByPlayer = data.afflictionsSkillsInflictedByPlayer;
        } else {
            afflictionsSkillsInflictedByPlayer = new List<PLAYER_SKILL_TYPE>();
        }
        trapStructure = data.trapStructure.Load();
        classComponent = data.classComponent.Load(); classComponent.SetOwner(this);
        needsComponent = data.needsComponent.Load(); needsComponent.SetOwner(this);
        structureComponent = data.structureComponent.Load(); structureComponent.SetOwner(this);
        stateComponent = data.stateComponent.Load(); stateComponent.SetOwner(this);
        nonActionEventsComponent = data.nonActionEventsComponent.Load(); nonActionEventsComponent.SetOwner(this);
        interruptComponent = data.interruptComponent.Load(); interruptComponent.SetOwner(this);
        behaviourComponent = data.behaviourComponent.Load(); behaviourComponent.SetOwner(this);
        moodComponent = data.moodComponent.Load(); moodComponent.SetOwner(this);
        jobComponent = data.jobComponent.Load(); jobComponent.SetOwner(this);
        reactionComponent = data.reactionComponent.Load(); reactionComponent.SetOwner(this);
        logComponent = data.logComponent.Load(); //logComponent.SetOwner(this);
        combatComponent = data.combatComponent.Load(); combatComponent.SetOwner(this);
        rumorComponent = data.rumorComponent.Load(); rumorComponent.SetOwner(this);
        assumptionComponent = data.assumptionComponent.Load(); assumptionComponent.SetOwner(this);
        movementComponent = data.movementComponent.Load(); movementComponent.SetOwner(this);
        stateAwarenessComponent = data.stateAwarenessComponent.Load(); stateAwarenessComponent.SetOwner(this);
        carryComponent = data.carryComponent.Load(); carryComponent.SetOwner(this);
        partyComponent = data.partyComponent.Load(); partyComponent.SetOwner(this);
        gatheringComponent = data.gatheringComponent.Load(); gatheringComponent.SetOwner(this);
        tileObjectComponent = data.tileObjectComponent.Load(); tileObjectComponent.SetOwner(this);
        crimeComponent = data.crimeComponent.Load(); crimeComponent.SetOwner(this);
        religionComponent = data.religionComponent.Load(); religionComponent.SetOwner(this);
        limiterComponent = data.limiterComponent.Load(); limiterComponent.SetOwner(this);
        piercingAndResistancesComponent = data.piercingAndResistancesComponent.Load(); piercingAndResistancesComponent.SetOwner(this);
        previousCharacterDataComponent = data.previousCharacterDataComponent.Load(); previousCharacterDataComponent.SetOwner(this);
        traitComponent = data.traitComponent.Load(); traitComponent.SetOwner(this);
        moneyComponent = data.moneyComponent.Load(); moneyComponent.SetOwner(this);
        dailyScheduleComponent = data.dailyScheduleComponent.Load(); dailyScheduleComponent.SetOwner(this);

        if (data.talentComponent != null) {
            talentComponent = data.talentComponent.Load(); talentComponent.SetOwner(this);
        }
        if (data.villagerWantsComponent != null) {
            villagerWantsComponent = data.villagerWantsComponent.Load(); villagerWantsComponent.SetOwner(this);
        }

        //buffStatsBonus = data.buffStatusBonus.Load();
        eventDispatcher = new CharacterEventDispatcher();
        bookmarkEventDispatcher = new BookmarkableEventDispatcher();
        resourceStorageComponent = data.resourceStorageComponent.Load();

        if (data.hasMinion) {
            _minion = data.minion.Load(this);
            visuals.CreateWholeImageMaterial(visuals.portraitSettings);
        }

        //if (data.hasMarker) {
        //    //Create marker in first wave loading, so that when loading references of character in second wave we can put carried characters/objects because the marker is already created
        //    //This must be same as the Tile Object, visual of tile object must also be created on first wave
        //    CreateMarker();
        //}
    }
    /// <summary>
    /// Initialize data for this character that is not safe to put in the constructor.
    /// Usually this is data that is dependent on the character being fully constructed.
    /// </summary>
    public virtual void Initialize() {
        ConstructDefaultActions();
        OnUpdateRace();
        classComponent.OnUpdateCharacterClass();
        moodComponent.SetMoodValue(50);
        if (needsComponent.HasNeeds()) {
            needsComponent.Initialize();
        }
        religionComponent.Initialize();
        moneyComponent.Initialize();
        if (race != RACE.DEMON) {
            //Demons and Summons/Monsters can't have talents
            talentComponent = new CharacterTalentComponent(); talentComponent.SetOwner(this);
            talentComponent.ConstructAllTalents();
            talentComponent.RandomizeInitialTalents(this);
        }
        if (race.IsSapient() || race == RACE.RATMAN) {
            villagerWantsComponent = new VillagerWantsComponent(); villagerWantsComponent.SetOwner(this);
            villagerWantsComponent.Initialize(this);

            if (race == RACE.RATMAN) {
                isInfoUnlocked = true;
            }
        }
    }
    public void InitialCharacterPlacement(LocationGridTile tile) {
        if (needsComponent.HasNeeds()) {
            needsComponent.InitialCharacterPlacement();
        }

        ConstructInitialGoapAdvertisementActions();
        marker.InitialPlaceMarkerAt(tile); //since normal characters are already placed in their areas.
        //AddInitialAwareness();
        SubscribeToSignals();
        SubscribeToPermanentSignals();
    }

    #region Signals
    /// <summary>
    /// Make this character subscribe to signals that we never want to remove.
    /// </summary>
    private void SubscribeToPermanentSignals() {
        //had to make name change signal permanent because it is possible for the player to change the name of
        //this characters killer, and we still want to update this character's death log if that happens. 
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CHANGED_NAME, OnCharacterChangedName);
    }
    public virtual void SubscribeToSignals() {
        if (minion != null) {
#if DEBUG_LOG
            logComponent.PrintLogErrorIfActive($"{name} is a minion and has subscribed to the signals!");
#endif
        }
        if (hasSubscribedToSignals) {
            return;
        }
        hasSubscribedToSignals = true; //This is done so there will be no duplication of listening to signals
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
        Messenger.AddListener(Signals.TICK_STARTED, OnTickStarted);
        Messenger.AddListener(CharacterSignals.CHARACTER_TICK_ENDED, OnTickEnded);
        Messenger.AddListener(Signals.HOUR_STARTED, OnHourStarted);
        Messenger.AddListener(Signals.DAY_STARTED, DailyGoapProcesses);
        Messenger.AddListener<Character>(CharacterSignals.STARTED_TRAVELLING_IN_WORLD, OnLeaveArea);
        Messenger.AddListener<Character>(CharacterSignals.FINISHED_TRAVELLING_IN_WORLD, OnArrivedAtArea);
        Messenger.AddListener<IPointOfInterest, string>(CharacterSignals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, ForceCancelAllJobsTargetingPOI);
        Messenger.AddListener<IPointOfInterest, string, JOB_TYPE>(CharacterSignals.FORCE_CANCEL_ALL_JOB_TYPES_TARGETING_POI, ForceCancelAllJobsOfTypeTargetingPOI);
        Messenger.AddListener<IPointOfInterest, string>(CharacterSignals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI_EXCEPT_SELF, ForceCancelAllJobsTargetingPOIExceptSelf);
        Messenger.AddListener<IPointOfInterest, string>(CharacterSignals.FORCE_CANCEL_ALL_ACTIONS_TARGETING_POI, ForceCancelAllActionsTargetingPOI);

        //NOTE: To improve performance, since the calls here are just calls directly to the statecomponent owner, instead of all characters listening to the signal, i just directly call the statecomponent owner on the specific code
        //Less signal subscription, less checking
        //Messenger.AddListener<Character, CharacterState>(CharacterSignals.CHARACTER_STARTED_STATE, OnCharacterStartedState);
        //Messenger.AddListener<Character, CharacterState>(CharacterSignals.CHARACTER_ENDED_STATE, OnCharacterEndedState);
        Messenger.AddListener<ActualGoapNode>(JobSignals.STARTED_PERFORMING_ACTION, OnActionPerformed);
        Messenger.AddListener<InterruptHolder>(InterruptSignals.INTERRUPT_STARTED, OnInterruptStarted);
        Messenger.AddListener<IPointOfInterest>(CharacterSignals.ON_SEIZE_POI, OnSeizePOI);
        Messenger.AddListener<IPointOfInterest>(CharacterSignals.BEFORE_SEIZING_POI, OnBeforeSeizingPOI);
        Messenger.AddListener<TileObject>(CharacterSignals.STOP_CURRENT_ACTION_TARGETING_POI, OnStopCurrentActionTargetingPOI);
        Messenger.AddListener<TileObject, Character>(CharacterSignals.STOP_CURRENT_ACTION_TARGETING_POI_EXCEPT_ACTOR, OnStopCurrentActionTargetingPOIExceptActor);
        Messenger.AddListener<LocationStructure>(StructureSignals.STRUCTURE_DESTROYED, OnStructureDestroyed);
        Messenger.AddListener<IPointOfInterest, int>(CharacterSignals.INCREASE_THREAT_THAT_SEES_POI, IncreaseThreatThatSeesPOI);
        Messenger.AddListener<Faction, Character>(FactionSignals.CREATE_FACTION_INTERRUPT, OnFactionCreated);
        Messenger.AddListener<ITraitable, Trait>(TraitSignals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
        Messenger.AddListener<Faction, Faction, FACTION_RELATIONSHIP_STATUS, FACTION_RELATIONSHIP_STATUS>(FactionSignals.CHANGE_FACTION_RELATIONSHIP, OnChangeFactionRelationship);
        Messenger.AddListener<Character, PowerCrystal>(CharacterSignals.ON_ELF_ABSORB_CRYSTAL, OnPowerCrystalAbsorbedByAnElf);
        needsComponent.SubscribeToSignals();
        jobComponent.SubscribeToListeners();
        stateAwarenessComponent.SubscribeSignals();
        combatComponent.SubscribeToSignals();
        visuals.SubscribeListeners();
        religionComponent.SubscribeListeners();
        movementComponent.SubscribeToSignals();
    }
    public virtual void UnsubscribeSignals() {
        if (!hasSubscribedToSignals) {
            return;
        }
        hasSubscribedToSignals = false; //This is done so there will be no duplication of listening to signals
        Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
        Messenger.RemoveListener(Signals.TICK_STARTED, OnTickStarted);
        Messenger.RemoveListener(CharacterSignals.CHARACTER_TICK_ENDED, OnTickEnded);
        Messenger.RemoveListener(Signals.HOUR_STARTED, OnHourStarted);
        Messenger.RemoveListener(Signals.DAY_STARTED, DailyGoapProcesses);
        Messenger.RemoveListener<Character>(CharacterSignals.STARTED_TRAVELLING_IN_WORLD, OnLeaveArea);
        Messenger.RemoveListener<Character>(CharacterSignals.FINISHED_TRAVELLING_IN_WORLD, OnArrivedAtArea);
        Messenger.RemoveListener<IPointOfInterest, string>(CharacterSignals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, ForceCancelAllJobsTargetingPOI);
        Messenger.RemoveListener<IPointOfInterest, string, JOB_TYPE>(CharacterSignals.FORCE_CANCEL_ALL_JOB_TYPES_TARGETING_POI, ForceCancelAllJobsOfTypeTargetingPOI);
        Messenger.RemoveListener<IPointOfInterest, string>(CharacterSignals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI_EXCEPT_SELF, ForceCancelAllJobsTargetingPOIExceptSelf);
        Messenger.RemoveListener<IPointOfInterest, string>(CharacterSignals.FORCE_CANCEL_ALL_ACTIONS_TARGETING_POI, ForceCancelAllActionsTargetingPOI);
        //Messenger.RemoveListener<Character, CharacterState>(CharacterSignals.CHARACTER_STARTED_STATE, OnCharacterStartedState);
        //Messenger.RemoveListener<Character, CharacterState>(CharacterSignals.CHARACTER_ENDED_STATE, OnCharacterEndedState);
        Messenger.RemoveListener<ActualGoapNode>(JobSignals.STARTED_PERFORMING_ACTION, OnActionPerformed);
        Messenger.RemoveListener<InterruptHolder>(InterruptSignals.INTERRUPT_STARTED, OnInterruptStarted);
        Messenger.RemoveListener<IPointOfInterest>(CharacterSignals.ON_SEIZE_POI, OnSeizePOI);
        Messenger.RemoveListener<IPointOfInterest>(CharacterSignals.BEFORE_SEIZING_POI, OnBeforeSeizingPOI);
        Messenger.RemoveListener<TileObject>(CharacterSignals.STOP_CURRENT_ACTION_TARGETING_POI, OnStopCurrentActionTargetingPOI);
        Messenger.RemoveListener<TileObject, Character>(CharacterSignals.STOP_CURRENT_ACTION_TARGETING_POI_EXCEPT_ACTOR, OnStopCurrentActionTargetingPOIExceptActor);
        Messenger.RemoveListener<LocationStructure>(StructureSignals.STRUCTURE_DESTROYED, OnStructureDestroyed);
        Messenger.RemoveListener<IPointOfInterest, int>(CharacterSignals.INCREASE_THREAT_THAT_SEES_POI, IncreaseThreatThatSeesPOI);
        Messenger.RemoveListener<Faction, Character>(FactionSignals.CREATE_FACTION_INTERRUPT, OnFactionCreated);
        Messenger.RemoveListener<ITraitable, Trait>(TraitSignals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);

        needsComponent.UnsubscribeToSignals();
        jobComponent.UnsubscribeListeners();
        stateAwarenessComponent.UnsubscribeSignals();
        combatComponent.UnsubscribeToSignals();
        visuals.UnsubscribeListeners();
        religionComponent.UnsubscribeListeners();
        movementComponent.UnsubscribeFromSignals();
    }
    #endregion

    #region Virtuals
    public virtual void OnSetIsHidden() { }
    #endregion

    #region Listeners
    private void OnStopCurrentActionTargetingPOI(TileObject poi) {
        if (currentActionNode != null && currentActionNode.poiTarget == poi) {
            StopCurrentActionNode();
        }
    }
    private void OnStopCurrentActionTargetingPOIExceptActor(TileObject poi, Character actor) {
        if (currentActionNode != null && currentActionNode.poiTarget == poi && this != actor) {
            StopCurrentActionNode();
        }
    }
    private void IncreaseThreatThatSeesPOI(IPointOfInterest poi, int amount) {
        if (faction != null && faction.isMajorNonPlayerOrVagrant && marker) {
            if (poi is Character character) {
                if (marker.IsPOIInVision(character)) {
                    PlayerManager.Instance.player.threatComponent.AdjustThreatAndApplyModification(amount);
                }
            } else if (poi is TileObject tileObject) {
                if (marker.IsPOIInVision(tileObject)) {
                    PlayerManager.Instance.player.threatComponent.AdjustThreatAndApplyModification(amount);
                }
            }
        }
    }
    private void ProcessBeforeDeath(string cause, Character responsibleCharacter) {
        if (cause == "attacked" && responsibleCharacter != null) {
            //Death by attacked
            if (responsibleCharacter.isNormalCharacter && responsibleCharacter.faction != null && responsibleCharacter.faction.isMajorNonPlayer && responsibleCharacter.faction != faction
                && faction != null && faction.isMajorNonPlayer && (faction.factionType.type == FACTION_TYPE.Human_Empire || faction.factionType.type == FACTION_TYPE.Elven_Kingdom)) {
                //Killed by a character from another villager faction
                if (IsInHomeSettlement() && !homeSettlement.HasAliveResident()) {
                    GameDate dueDate = GameManager.Instance.Today();
                    dueDate.AddTicks(GameManager.Instance.GetTicksBasedOnHour(2));
                    LocationGridTile deathTile = gridTileLocation;
                    SchedulingManager.Instance.AddEntry(dueDate, () => SpawnRevenant(responsibleCharacter, deathTile), null);
                }
            }
        }
    }
    private void SpawnRevenant(Character responsibleCharacter, LocationGridTile deathTile) {
        BaseSettlement homeSettlement = null;
        deathTile.IsPartOfSettlement(out homeSettlement);
        Region homeRegion = deathTile.structure.region;

        if (homeSettlement == null) {
            //Will not spawn revenant if death tile has no settlement
            return;
        }

        Summon summon = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Revenant, FactionManager.Instance.undeadFaction, homeLocation: homeSettlement, homeRegion: homeRegion);
        CharacterManager.Instance.PlaceSummonInitially(summon, deathTile);
        Revenant revenant = summon as Revenant;
        if (responsibleCharacter.partyComponent.hasParty) {
            for (int i = 0; i < responsibleCharacter.partyComponent.currentParty.members.Count; i++) {
                Character member = responsibleCharacter.partyComponent.currentParty.members[i];
                revenant.AddBetrayer(member);
            }
        } else {
            revenant.AddBetrayer(responsibleCharacter);
        }

        int numOfGhosts = UnityEngine.Random.Range(1, 4);
        for (int i = 0; i < numOfGhosts; i++) {
            Character betrayer = revenant.GetRandomBetrayer();
            Summon ghost = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Ghost, FactionManager.Instance.undeadFaction, homeLocation: homeSettlement, homeRegion: homeRegion, homeStructure: currentStructure);
            (ghost as Ghost).SetBetrayedBy(betrayer);
            Area randomArea = homeSettlement.GetRandomArea();
            if (randomArea != null) {
                CharacterManager.Instance.PlaceSummonInitially(ghost, randomArea.gridTileComponent.GetRandomTile());
            }
        }


        Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "spawn_revenant", null, LOG_TAG.Life_Changes);
        log.AddToFillers(revenant, revenant.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(null, homeSettlement.name, LOG_IDENTIFIER.LANDMARK_1);
        log.AddLogToDatabase();
        PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
        LogPool.Release(log);
    }
    private void OnTraitableGainedTrait(ITraitable p_traitable, Trait p_trait) {
        if (p_trait is Burning burning) {
            if (currentActionNode != null && currentActionNode.actionStatus == ACTION_STATUS.PERFORMING &&
                currentActionNode.poiTarget == p_traitable && currentActionNode.action.actionCategory == ACTION_CATEGORY.CONSUME) {
                //Stop eating object that has been set on fire. And gain burning too.
                //https://trello.com/c/xDiItiDG/2026-ignite-food-vegetable-while-being-eaten-didnt-ignite
                StopCurrentActionNode(reason: "Object is burning");
                traitContainer.AddTrait(this, "Burning", out var addedTrait, bypassElementalChance: true);
                (addedTrait as Burning)?.SetSourceOfBurning(burning.sourceOfBurning, this);
                Burning currentBurning = traitContainer.GetTraitOrStatus<Burning>("Burning");
                currentBurning?.SetIsPlayerSource(burning.isPlayerSource);
            }
        }
    }
    private void OnCharacterChangedName(Character p_character) {
        if (p_character != this) {
            UpdateCurrentLogsBasedOnUpdatedCharacter(p_character);
            moodComponent.UpdateMoodSummaryLogsOnCharacterChangedName(p_character);
        }
    }
    private void UpdateCurrentLogsBasedOnUpdatedCharacter(Character p_character) {
        if (interruptComponent.isInterrupted) {
            //had to force update because for some reason involved objects are empty during this point
            //TODO: Find out why!
            interruptComponent.thoughtBubbleLog?.TryUpdateLogAfterRename(p_character, true);
        }
        if (currentActionNode != null) {
            currentActionNode.thoughtBubbleLog?.TryUpdateLogAfterRename(p_character);
            currentActionNode.thoughtBubbleMovingLog?.TryUpdateLogAfterRename(p_character);
            currentActionNode.descriptionLog?.TryUpdateLogAfterRename(p_character);
        }
        if (deathLog != null) {
            deathLog.TryUpdateLogAfterRename(p_character);
        }
        stateComponent.currentState?.thoughtBubbleLog?.TryUpdateLogAfterRename(p_character, true);
    }
    #endregion

    #region Sexuality
    private void GenerateSexuality() {
        if (GameUtilities.IsRaceBeast(race)) {
            //For beasts:
            //100 % straight
            sexuality = SEXUALITY.STRAIGHT;
        } else {
            //For sapient creatures:
            //80 % straight
            //10 % bisexual
            //10 % gay
            int chance = UnityEngine.Random.Range(0, 100);
            if (chance < 80) {
                sexuality = SEXUALITY.STRAIGHT;
            } else if (chance >= 80 && chance < 90) {
                sexuality = SEXUALITY.BISEXUAL;
            } else {
                sexuality = SEXUALITY.GAY;
            }
        }
    }
    public void SetSexuality(SEXUALITY sexuality) {
        this.sexuality = sexuality;
    }
    #endregion

    #region Marker
    public void SetGridTilePosition(Vector2 p_anchoredPos) {
#if DEBUG_PROFILER
        Profiler.BeginSample($"{name} - SetGridTilePosition - Floor to Int");
#endif
        int xFloorToInt = Mathf.FloorToInt(p_anchoredPos.x);
        int yFloorToInt = Mathf.FloorToInt(p_anchoredPos.y);
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif

#if DEBUG_PROFILER
        Profiler.BeginSample($"{name} - SetGridTilePosition");
#endif
        gridTileLocalPosition = new Vector2Int(xFloorToInt, yFloorToInt);
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif
    }
    public void SetGridTileWorldPosition(Vector3 p_pos) {
        gridTileWorldPosition = p_pos;
    }
    public void CreateMarker() {
        GameObject portraitGO = ObjectPoolManager.Instance.InstantiateObjectFromPool("CharacterMarker", Vector3.zero, Quaternion.identity, InnerMapManager.Instance.transform);
        CharacterMarker _marker = portraitGO.GetComponent<CharacterMarker>();
        SetCharacterMarker(_marker);
        _marker.SetCharacter(this);

        //List<Trait> traitOverrideFunctions = traitContainer.GetTraitOverrideFunctions(TraitManager.Initiate_Map_Visual_Trait);
        //if (traitOverrideFunctions != null) {
        //    for (int i = 0; i < traitOverrideFunctions.Count; i++) {
        //        Trait trait = traitOverrideFunctions[i];
        //        trait.OnInitiateMapObjectVisual(this);
        //    }
        //}
    }
    public void DestroyMarker(LocationGridTile destroyedAt = null, bool removeFromMasterList = true) {
        if (destroyedAt == null) {
            LocationGridTile gridTile = gridTileLocation;
            if (gridTile != null) {
                gridTile.RemoveCharacterHere(this);
                gridTile.structure.RemoveCharacterAtLocation(this);
                gridTile.area.locationCharacterTracker.RemoveCharacterFromLocation(this, gridTile.area);
            }
        } else {
            destroyedAt.RemoveCharacterHere(this);
            destroyedAt.structure.RemoveCharacterAtLocation(this);
            destroyedAt.area.locationCharacterTracker.RemoveCharacterFromLocation(this, destroyedAt.area);
        }
        ObjectPoolManager.Instance.DestroyObject(marker);
        SetCharacterMarker(null);
#if DEBUG_LOG
        Debug.Log($"{name}'s marker has been destroyed!");
#endif
        Messenger.Broadcast(JobSignals.CHECK_APPLICABILITY_OF_ALL_JOBS_TARGETING, this as IPointOfInterest);
        if (PlayerManager.Instance.player.seizeComponent.seizedPOI == this) {
            throw new Exception($"{name} is seized by the player but its marker was destroyed! Refer to call stack to find out what destroyed it.");
        }
        if (removeFromMasterList) {
            //In order to lessen the all character list (because it will keep on expanding), once character's body is destroyed, remove it from the master list
            //CharacterManager.Instance.RemoveCharacter(this);
        }
    }
    public void DisableMarker() {
        if (marker.gameObject.activeSelf) {
#if DEBUG_LOG
            Debug.Log($"Disabled marker of {name}");
#endif
            marker.gameObject.SetActive(false);
            // marker.SetVisualState(false);
            gridTileLocation.RemoveCharacterHere(this);
        }
    }
    public void EnableMarker() {
        // marker.SetVisualState(true);
        Assert.IsNotNull(marker, $"Marker of {name} is trying to be enabled, but is null!");
        if (!marker.gameObject.activeSelf) {
#if DEBUG_LOG
            Debug.Log($"Enabled marker of {name}");
#endif
            marker.gameObject.SetActive(true);
        }
    }
    private void SetCharacterMarker(CharacterMarker p_marker) {
        if (p_marker == null && marker != null) {
            //will set character marker to null
            //remove character from its current structure character list and its current tile character list
            gridTileLocation?.RemoveCharacterHere(this);
            currentStructure?.RemoveCharacterAtLocation(this);
        }
        this.marker = p_marker;
        if (p_marker == null) {
            hasMarker = false;
            Messenger.Broadcast(CharacterSignals.CHARACTER_MARKER_DESTROYED, this);
        } else {
            hasMarker = true;
        }
    }
    public virtual void PerTickDuringMovement() {
        //NOTE: Moved All Per Tick Movement effects to TryProcessTraitsOnTickEndedWhileStationaryOrUnoccupied because of task
        //https://trello.com/c/DqNguu3v/2941-symptoms-triggered-while-moving-should-also-be-triggered-when-character-is-stationary-and-not-doing-anything-and-not-sleeping
    }
    #endregion

    #region Jobs
    public void SetCurrentJob(JobQueueItem job) {
        currentJob = job;
    }
    //public JobQueueItem GetCurrentJob() {
    //    if(currentActionNode != null && jobQueue.jobsInQueue.Count > 0) {
    //        return jobQueue.jobsInQueue[0];
    //    }
    //    return null;
    //}
    public void AddJobTargetingThis(JobQueueItem job) {
        allJobsTargetingThis.Add(job);
        //removed this because all characters can be seen by each other at all times.
        // marker.visionTrigger.VoteToMakeVisibleToCharacters();
    }
    public bool RemoveJobTargetingThis(JobQueueItem job) {
        if (allJobsTargetingThis.Remove(job)) {
            //removed this because all characters can be seen by each other at all times.
            // marker.visionTrigger.VoteToMakeInvisibleToCharacters();
            return true;
        }
        return false;
    }
    public void ForceCancelAllJobsTargetingThisCharacter(bool shouldDoAfterEffect) {
        for (int i = 0; i < allJobsTargetingThis.Count; i++) {
            JobQueueItem job = allJobsTargetingThis[i];
            if (job.ForceCancelJob()) {
                i--;
            }
        }
    }
    public void ForceCancelAllJobsTargetingThisCharacter(JOB_TYPE jobType) {
        for (int i = 0; i < allJobsTargetingThis.Count; i++) {
            JobQueueItem job = allJobsTargetingThis[i];
            if (job.jobType == jobType) {
                if (job.ForceCancelJob()) {
                    i--;
                }

            }
        }
    }
    public void ForceCancelAllJobsTargettingThisCharacterExcept(JOB_TYPE jobType, string conditionKey, Character otherCharacter) {
        for (int i = 0; i < allJobsTargetingThis.Count; i++) {
            if (allJobsTargetingThis[i] is GoapPlanJob) {
                GoapPlanJob job = allJobsTargetingThis[i] as GoapPlanJob;
                if (job.jobType == jobType && job.assignedCharacter != otherCharacter && job.HasGoalConditionKey(conditionKey)) {
                    if (job.ForceCancelJob()) {
                        i--;
                    }
                }
            }
        }
    }
    public void ForceCancelAllJobsTargetingPOI(IPointOfInterest target, string reason) {
        for (int i = 0; i < jobQueue.jobsInQueue.Count; i++) {
            JobQueueItem job = jobQueue.jobsInQueue[i];
            if (job is GoapPlanJob) {
                GoapPlanJob goapJob = job as GoapPlanJob;
                if (goapJob.targetPOI == target) {
                    if (reason == GoapPlanJob.Target_Already_Dead_Reason) {
                        //if reason for cancellation is because of death, check if job should be cancelled if target dies.
                        if (goapJob.shouldBeCancelledOnDeath == false) {
                            continue; //skip
                        }
                    }
                    if (goapJob.ForceCancelJob(reason)) {
                        i--;
                    }
                }
            }
        }
    }
    private void ForceCancelAllActionsTargetingPOI(IPointOfInterest target, string reason) {
        if (currentActionNode != null && currentActionNode.associatedJob != null && currentActionNode.poiTarget == target && currentActionNode.associatedJob.poiTarget == target) {
            //if target is main target of the job, stop the whole job
            currentActionNode.associatedJob.ForceCancelJob(reason);
        }
    }
    private void ForceCancelAllJobsOfTypeTargetingPOI(IPointOfInterest target, string reason, JOB_TYPE jobType) {
        for (int i = 0; i < jobQueue.jobsInQueue.Count; i++) {
            JobQueueItem job = jobQueue.jobsInQueue[i];
            if (job is GoapPlanJob goapJob) {
                if (goapJob.jobType == jobType && goapJob.targetPOI == target) {
                    if (reason == GoapPlanJob.Target_Already_Dead_Reason) {
                        //if reason for cancellation is because of death, check if job should be cancelled if target dies.
                        if (goapJob.shouldBeCancelledOnDeath == false) {
                            continue; //skip
                        }
                    }
                    if (goapJob.ForceCancelJob(reason)) {
                        i--;
                    }
                }
            }
        }
    }
    private void ForceCancelAllJobsTargetingPOIExceptSelf(IPointOfInterest target, string reason) {
        for (int i = 0; i < jobQueue.jobsInQueue.Count; i++) {
            JobQueueItem job = jobQueue.jobsInQueue[i];
            if (job is GoapPlanJob) {
                GoapPlanJob goapJob = job as GoapPlanJob;
                if (goapJob.targetPOI == target && this != target) {
                    if (target is Character character && character.carryComponent.IsCurrentlyPartOf(this)) {
                        //if target is character and is being carried by this do not cancel the job 
                        continue; //skip
                    }
                    if (goapJob.ForceCancelJob(reason)) {
                        i--;
                    }
                }
            }
        }
    }
    //private void ForceCancelJobTypesTargetingPOI(IPointOfInterest target, string reason, JOB_TYPE jobType) {
    //    for (int i = 0; i < jobQueue.jobsInQueue.Count; i++) {
    //        JobQueueItem job = jobQueue.jobsInQueue[i];
    //        if (job.jobType == jobType && job is GoapPlanJob) {
    //            GoapPlanJob goapJob = job as GoapPlanJob;
    //            if (goapJob.targetPOI == target) {
    //                if (goapJob.ForceCancelJob(false, reason)) {
    //                    i--;
    //                }
    //            }
    //        }
    //    }
    //}
    public bool HasJobTargetingThis(JOB_TYPE jobType) {
        for (int i = 0; i < allJobsTargetingThis.Count; i++) {
            JobQueueItem job = allJobsTargetingThis[i];
            if (job.jobType == jobType) {
                return true;
            }
        }
        return false;
    }
    public bool HasJobTargetingThis(JOB_TYPE jobType1, JOB_TYPE jobType2) {
        for (int i = 0; i < allJobsTargetingThis.Count; i++) {
            JobQueueItem job = allJobsTargetingThis[i];
            if (job.jobType == jobType1 || job.jobType == jobType2) {
                return true;
            }
        }
        return false;
    }
    public bool HasJobTargetingThis(JOB_TYPE jobType1, JOB_TYPE jobType2, JOB_TYPE jobType3) {
        for (int i = 0; i < allJobsTargetingThis.Count; i++) {
            JobQueueItem job = allJobsTargetingThis[i];
            if (job.jobType == jobType1 || job.jobType == jobType2 || job.jobType == jobType3) {
                return true;
            }
        }
        return false;
    }
    public int GetNumOfJobsTargettingThisCharacter(JOB_TYPE jobType) {
        int count = 0;
        for (int i = 0; i < allJobsTargetingThis.Count; i++) {
            JobQueueItem job = allJobsTargetingThis[i];
            if (job.jobType == jobType) {
                count++;
            }
        }
        return count;
    }
    public GoapPlanJob GetJobTargettingThisCharacter(JOB_TYPE jobType, string conditionKey) {
        for (int i = 0; i < allJobsTargetingThis.Count; i++) {
            if (allJobsTargetingThis[i] is GoapPlanJob) {
                GoapPlanJob job = allJobsTargetingThis[i] as GoapPlanJob;
                if (job.jobType == jobType && job.HasGoalConditionKey(conditionKey)) {
                    return job;
                }
            }
        }
        return null;
    }
    public GoapPlanJob GetJobTargettingThisCharacter(JOB_TYPE jobType) {
        for (int i = 0; i < allJobsTargetingThis.Count; i++) {
            if (allJobsTargetingThis[i] is GoapPlanJob) {
                GoapPlanJob job = allJobsTargetingThis[i] as GoapPlanJob;
                if (job.jobType == jobType) {
                    return job;
                }
            }
        }
        return null;
    }
    private void CheckApprehendRelatedJobsOnLeaveLocation() {
        ForceCancelAllJobsTargetingThisCharacter(JOB_TYPE.APPREHEND);
        jobQueue.CancelAllJobs(JOB_TYPE.APPREHEND);
        //All apprehend jobs that are being done by this character must be unassigned
        //for (int i = 0; i < allGoapPlans.Count; i++) {
        //    GoapPlan plan = allGoapPlans[i];
        //    if (plan.job != null && plan.job.jobType == JOB_TYPE.APPREHEND) {
        //        plan.job.UnassignJob();
        //        i--;
        //    }
        //}
    }
    public void CancelOrUnassignRemoveTraitRelatedJobs() {
        ForceCancelAllJobsTargetingThisCharacter(JOB_TYPE.REMOVE_STATUS);
        jobQueue.CancelAllJobs(JOB_TYPE.REMOVE_STATUS);
        //TODO:
        //All remove trait jobs that are being done by this character must be unassigned
        //for (int i = 0; i < allGoapPlans.Count; i++) {
        //    GoapPlan plan = allGoapPlans[i];
        //    if (plan.job != null && plan.job.jobType == JOB_TYPE.REMOVE_TRAIT) {
        //        plan.job.UnassignJob();
        //        i--;
        //    }
        //}
    }
    public void CancelRemoveStatusFeedAndRepairJobsTargetingThis() {
        for (int i = 0; i < allJobsTargetingThis.Count; i++) {
            JobQueueItem job = allJobsTargetingThis[i];
            if (job.jobType == JOB_TYPE.REMOVE_STATUS || job.jobType == JOB_TYPE.REPAIR || job.jobType == JOB_TYPE.FEED) {
                if (job.CancelJob()) {
                    i--;
                }
            }
        }
    }
    //private bool CreateJobsOnEnterVisionWithCharacter(Character targetCharacter) {
    //    string log = $"{name} saw {targetCharacter.name}, will try to create jobs on enter vision...";
    //    if (!CanCharacterReact(targetCharacter)) {
    //        log += "\nCharacter cannot react!";
    //        logComponent.PrintLogIfActive(log);
    //        return true;
    //    }
    //    bool hasCreatedJob = false;
    //    log += "\nChecking source character traits...";
    //    for (int i = 0; i < traitContainer.allTraitsAndStatuses.Count; i++) {
    //        log += $"\n- {traitContainer.allTraitsAndStatuses[i].name}";
    //        if (traitContainer.allTraitsAndStatuses[i].OnSeePOI(targetCharacter, this)) {
    //            log += ": created a job!";
    //            hasCreatedJob = true;
    //        } else {
    //            log += ": did not create a job!";
    //        }
    //    }

    //    log += "\nChecking target character traits...";
    //    for (int i = 0; i < targetCharacter.traitContainer.allTraitsAndStatuses.Count; i++) {
    //        log += $"\n- {targetCharacter.traitContainer.allTraitsAndStatuses[i].name}";
    //        if (targetCharacter.traitContainer.allTraitsAndStatuses[i].CreateJobsOnEnterVisionBasedOnTrait(targetCharacter, this)) {
    //            hasCreatedJob = true;
    //            log += ": created a job!";
    //        } else {
    //            log += ": did not create a job!";
    //        }
    //    }
    //    logComponent.PrintLogIfActive(log);
    //    return hasCreatedJob;
    //}
    //public bool CreateJobsOnEnterVisionWith(IPointOfInterest targetPOI) {
    //    if (targetPOI is Character) {
    //        return CreateJobsOnEnterVisionWithCharacter(targetPOI as Character);
    //    }
    //    string log = $"{name} saw {targetPOI.name}, will try to create jobs on enter vision...";
    //    if (!CanCharacterReact(targetPOI)) {
    //        log += "\nCharacter cannot react!";
    //        logComponent.PrintLogIfActive(log);
    //        return true;
    //    }
    //    bool hasCreatedJob = false;
    //    log += "\nChecking source character traits...";
    //    for (int i = 0; i < traitContainer.allTraitsAndStatuses.Count; i++) {
    //        log += $"\n- {traitContainer.allTraitsAndStatuses[i].name}";
    //        if (traitContainer.allTraitsAndStatuses[i].OnSeePOI(targetPOI, this)) {
    //            log += ": created a job!";
    //            hasCreatedJob = true;
    //        } else {
    //            log += ": did not create a job!";
    //        }
    //    }
    //    log += "\nChecking target poi traits...";
    //    for (int i = 0; i < targetPOI.traitContainer.allTraitsAndStatuses.Count; i++) {
    //        log += $"\n- {targetPOI.traitContainer.allTraitsAndStatuses[i].name}";
    //        if (targetPOI.traitContainer.allTraitsAndStatuses[i].CreateJobsOnEnterVisionBasedOnTrait(targetPOI, this)) {
    //            log += ": created a job!";
    //            hasCreatedJob = true;
    //        } else {
    //            log += ": did not create a job!";
    //        }
    //    }

    //    logComponent.PrintLogIfActive(log);
    //    return hasCreatedJob;
    //}
    public bool CreateJobsOnTargetGainTrait(IPointOfInterest targetPOI, Trait traitGained) {
#if DEBUG_LOG
        string log = $"{targetPOI.name} gained trait {traitGained.name}, will try to create jobs based on it...";
#endif
        if (!CanCharacterReact(targetPOI)) {
#if DEBUG_LOG
            log += "\nCharacter cannot react!";
            logComponent.PrintLogIfActive(log);
#endif
            return true;
        }
        bool hasCreatedJob = false;
#if DEBUG_LOG
        log += "\nChecking trait...";
#endif
        if (traitGained.CreateJobsOnEnterVisionBasedOnTrait(targetPOI, this)) {
#if DEBUG_LOG
            log += ": created a job!";
#endif
            hasCreatedJob = true;
        } else {
#if DEBUG_LOG
            log += ": did not create a job!";
#endif
        }
#if DEBUG_LOG
        logComponent.PrintLogIfActive(log);
#endif
        return hasCreatedJob;
    }
    //public void CancelAllJobs(string reason = "") {
    //    //AdjustIsWaitingForInteraction(1);
    //    //StopCurrentActionNode(reason: reason);
    //    for (int i = 0; i < jobQueue.jobsInQueue.Count; i++) {
    //        if (jobQueue.jobsInQueue[i].CancelJob(reason: reason)) {
    //            i--;
    //        }
    //    }
    //    //if (homeNpcSettlement != null) {
    //    //    homeNpcSettlement.jobQueue.UnassignAllJobsTakenBy(this);
    //    //}

    //    //StopCurrentAction(false, reason: reason);
    //    //for (int i = 0; i < allGoapPlans.Count; i++) {
    //    //    if (DropPlan(allGoapPlans[i])) {
    //    //        i--;
    //    //    }
    //    //}
    //    //AdjustIsWaitingForInteraction(-1);
    //}
    //public void CancelAllJobs(JOB_TYPE jobType) {
    //    for (int i = 0; i < jobQueue.jobsInQueue.Count; i++) {
    //        JobQueueItem job = jobQueue.jobsInQueue[i];
    //        if (job.jobType == jobType) {
    //            if (job.CancelJob()) {
    //                i--;
    //            }
    //        }
    //    }
    //}
    public void CancelAllJobsExceptForCurrent() {
        if (currentJob != null) {
            for (int i = 0; i < jobQueue.jobsInQueue.Count; i++) {
                JobQueueItem job = jobQueue.jobsInQueue[i];
                if (job != currentJob) {
                    if (job.CancelJob()) {
                        i--;
                    }
                }
            }
        }
    }
    public void CancelAllJobsExceptForCurrent(params JOB_TYPE[] jobType) {
        if (currentJob != null) {
            for (int i = 0; i < jobQueue.jobsInQueue.Count; i++) {
                JobQueueItem job = jobQueue.jobsInQueue[i];
                if (job != currentJob) {
                    for (int j = 0; j < jobType.Length; j++) {
                        if (job.jobType == jobType[j]) {
                            if (job.CancelJob()) {
                                i--;
                            }
                            break;
                        }
                    }
                }
            }
        }
    }
    public bool CanCurrentJobBeOverriddenByJob(JobQueueItem job) {
        return false;
        ////GENERAL RULE: Plans/States that have no jobs are always the lowest priority
        ////Current job cannot be overriden by null job
        //if (job == null) {
        //    return false;
        //}
        //if (GetNormalTrait<Trait>("Berserked") != null /*||(stateComponent.stateToDo != null && stateComponent.stateToDo.characterState == CHARACTER_STATE.BERSERKED)*/) {
        //    //Berserked state cannot be overriden
        //    return false;
        //}
        //if (stateComponent.currentState == null && this.marker && this.marker.hasFleePath) {
        //    return false; //if the character is only fleeing, but is not in combat state, do not allow overriding.
        //}
        //if (stateComponent.currentState != null) {
        //    if (stateComponent.currentState.characterState == CHARACTER_STATE.COMBAT) {
        //        //Only override flee or engage state if the job is Berserked State, Berserk overrides all
        //        if (job is CharacterStateJob) {
        //            CharacterStateJob stateJob = job as CharacterStateJob;
        //            if (stateJob.targetState == CHARACTER_STATE.BERSERKED) {
        //                return true;
        //            }
        //        }
        //        return false;
        //    } else {
        //        //If current state is not Flee or Engage, it is certainly one of the major states since the only minor states are Flee and Engage
        //        //If current state has no job, it is automatically overridable, otherwise, if the current state's job has a lower job priority (higher number) than the parameter job, it is overridable
        //        if (stateComponent.currentState.job != null && !stateComponent.currentState.job.cannotOverrideJob && job.priority < stateComponent.currentState.job.priority) {
        //            return true;
        //        } else if (stateComponent.currentState.job == null) {
        //            return true;
        //        }
        //        return false;
        //    }
        //} 
        ////else if (stateComponent.stateToDo != null) {
        ////    if (stateComponent.stateToDo.characterState == CHARACTER_STATE.COMBAT) {
        ////        //Only override flee or engage state if the job is Berserked State, Berserk overrides all
        ////        if (job is CharacterStateJob) {
        ////            CharacterStateJob stateJob = job as CharacterStateJob;
        ////            if (stateJob.targetState == CHARACTER_STATE.BERSERKED) {
        ////                return true;
        ////            }
        ////        }
        ////        return false;
        ////    } else {
        ////        //If current state is not Flee or Engage, it is certainly one of the major states since the only minor states are Flee and Engage
        ////        //If current state has no job, it is automatically overridable, otherwise, if the current state's job has a lower job priority (higher number) than the parameter job, it is overridable
        ////        if (stateComponent.stateToDo.job != null && !stateComponent.stateToDo.job.cannotOverrideJob && job.priority < stateComponent.stateToDo.job.priority) {
        ////            return true;
        ////        } else if (stateComponent.stateToDo.job == null) {
        ////            return true;
        ////        }
        ////        return false;
        ////    }
        ////}
        ////Cannot override when resting
        //if (traitContainer.GetNormalTrait<Trait>("Resting") != null) {
        //    return false;
        //}
        ////If there is no current state then check the current action
        ////Same process applies that if the current action's job has a lower job priority (higher number) than the parameter job, it is overridable
        //if(currentActionNode != null && currentActionNode.goapType == INTERACTION_TYPE.MAKE_LOVE && !currentActionNode.isDone) {
        //    //Cannot override make love
        //    return false;
        //}
        //if (currentActionNode != null && currentActionNode.parentPlan != null) {
        //    if(currentActionNode.parentPlan.job != null && !currentActionNode.parentPlan.job.cannotOverrideJob
        //               && job.priority < currentActionNode.parentPlan.job.priority) {
        //        return true;
        //    } else if (currentActionNode.parentPlan.job == null) {
        //        return true;
        //    }
        //    return false;
        //}

        ////If nothing applies, always overridable
        //return true;
    }
    /// <summary>
    /// Gets the current priority of the character's current action or state.
    /// If he/she has none, this will return a very high number.
    /// </summary>
    /// <returns></returns>
    public int GetCurrentPriorityValue() {
        if (stateComponent.currentState != null && stateComponent.currentState.job != null) {
            return stateComponent.currentState.job.priority;
        }
        //else if (stateComponent.stateToDo != null && stateComponent.stateToDo.job != null) {
        //    return stateComponent.stateToDo.job.priority;
        //}
        else {
            JobQueueItem job = currentJob;
            if (job != null) {
                return job.priority;
            } else {
                return 999999;
            }
        }

        //else if (currentActionNode != null && currentActionNode.parentPlan != null && currentActionNode.parentPlan.job != null) {
        //    return currentActionNode.parentPlan.job.priority;
        //} else {
        //    return 999999;
        //}
    }
    //public void CreateReplaceTileObjectJob(TileObject removedObj, LocationGridTile removedFrom) {
    //    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.REPLACE_TILE_OBJECT, INTERACTION_TYPE.REPLACE_TILE_OBJECT, new Dictionary<INTERACTION_TYPE, object[]>() {
    //                    { INTERACTION_TYPE.REPLACE_TILE_OBJECT, new object[]{ removedObj, removedFrom } },
    //    });
    //    job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeReplaceTileObjectJob);
    //    job.SetCancelOnFail(false);
    //    job.SetCancelJobOnDropPlan(false);
    //    jobQueue.AddJobInQueue(job);
    //}
    public void NoPathToDoJobOrAction(JobQueueItem job, ActualGoapNode action) {
        //if (partyComponent.hasParty && job.isThisAPartyJob) {
        //    //If a party has no path to do action and the job that has no path is a party job, leave party
        //    partyComponent.currentParty.RemoveMember(this);
        //}
        if (gridTileLocation != null && limiterComponent.canMove) {
            //If this character cannot do job or action because he has no path but the reason why he has no path is beacuse he has no grid location, do not trigger fall back jobs
            if (job.jobType == JOB_TYPE.RETURN_PORTAL || job.jobType == JOB_TYPE.RETURN_TERRITORY) {
                //interruptComponent.TriggerInterrupt(INTERRUPT.Set_Home, null);
                jobComponent.TriggerRoamAroundTile(JOB_TYPE.NO_PATH_IDLE);
            } else if (job.jobType == JOB_TYPE.ROAM_AROUND_TERRITORY
                || job.jobType == JOB_TYPE.ROAM_AROUND_CORRUPTION
                || job.jobType == JOB_TYPE.ROAM_AROUND_PORTAL) {
                jobComponent.TriggerRoamAroundTile(JOB_TYPE.NO_PATH_IDLE);
            } else if (action.goapType == INTERACTION_TYPE.RETURN_HOME) {
                //interruptComponent.TriggerInterrupt(INTERRUPT.Set_Home, null);
                jobComponent.TriggerRoamAroundTile(JOB_TYPE.NO_PATH_IDLE);
            } else {
                jobComponent.TriggerStand(JOB_TYPE.NO_PATH_IDLE);
            }
        }
    }
    #endregion

    #region Faction
    public virtual bool SetFaction(Faction newFaction) {
        if (_faction == newFaction) {
            //ignore change, because character is already part of that faction
            return false;
        }
        if (_faction != null) {
            previousCharacterDataComponent.SetPreviousFaction(_faction);
        }
        _faction = newFaction;
        OnChangeFaction(prevFaction, newFaction);
        if (_faction != null) {
            bookmarkEventDispatcher.ExecuteBookmarkChangedNameOrElementsEvent(this); //since characters can change name icons based on faction
            Messenger.Broadcast(FactionSignals.FACTION_SET, this);
            eventDispatcher.ExecuteJoinedFaction(this, _faction);
        }
        return true;
    }
    public bool ChangeFactionTo(Faction newFaction, bool bypassIdeologyChecking = false) {
        if (faction == newFaction) {
            return false; //if the new faction is the same, ignore change
        }
        faction?.LeaveFaction(this);
        return newFaction.JoinFaction(this, bypassIdeologyChecking: bypassIdeologyChecking);
    }
    protected virtual void OnChangeFaction(Faction prevFaction, Faction newFaction) {
        if (prevFaction != null) {
            if (prevFaction.factionType.type == FACTION_TYPE.Undead) {
                behaviourComponent.RemoveBehaviourComponent(typeof(UndeadBehaviour));
            }
            if (prevFaction.factionType.type == FACTION_TYPE.Demons) {
                if (partyComponent.hasParty) {
                    //remove character from party when they are removed from demon faction.
                    //Related Task: https://trello.com/c/1x2q7FQY/4633-demonic-party-not-cleared-out-when-last-living-member-is-recruited-by-a-major-faction
                    partyComponent.currentParty.RemoveMember(this);
                }
            }
            //else if (prevFaction.factionType.type == FACTION_TYPE.Demons) {
            //    //This is only temporary
            //    //Right now combat behaviours are only applicable on characters in the Demon faction
            //    combatComponent.SetCombatBehaviour(CHARACTER_COMBAT_BEHAVIOUR.None);
            //}
        }
        if (newFaction != null) {
            if (newFaction.factionType.type == FACTION_TYPE.Undead) {
                behaviourComponent.AddBehaviourComponent(typeof(UndeadBehaviour));
            }
            //else if (newFaction.factionType.type == FACTION_TYPE.Demons) {
            //    //This is only temporary
            //    //Right now combat behaviours are only applicable on characters in the Demon faction
            //    CharacterClassData classData = CharacterManager.Instance.GetOrCreateCharacterClassData(_characterClass.className);
            //    combatComponent.SetCombatBehaviour(classData.combatBehaviourType);
            //}
        }

        movementComponent.OnChangeFactionTo(newFaction);
        movementComponent.RedetermineFactionsToAvoid(this);

        //When a monster underling by player becomes a part of another faction, add new monster underling charge because this means that the active monster is no longer an underling
        //It's as if the monster underling died, so, add the charge again
        //UPDATE: Removed this because the replenishing of charge now comes with a cooldown
        //if(prevFaction != null && prevFaction.factionType.type == FACTION_TYPE.Demons) {
        //    if(this is Summon summon) {
        //        PlayerManager.Instance.player.underlingsComponent.AdjustMonsterUnderlingCharge(summon.summonType, 1);
        //    }
        //}
        // if (newFaction != null && newFaction.isMajorFaction) {
        //     //if character is now part of a faction, then set its movement to not avoid that faction
        //     movementComponent.DoNotAvoidFaction(newFaction);    
        // }
        // Debug.Log($"{name} changed faction from {prevFaction?.name ?? "Null"} to {newFaction?.name ?? "Null"}");
        // if (PlayerManager.Instance.player != null && this.faction == PlayerManager.Instance.player.playerFaction) {
        //     ClearPlayerActions();
        // }
    }
    private void OnChangeFactionRelationship(Faction p_faction1, Faction p_faction2, FACTION_RELATIONSHIP_STATUS p_newStatus, FACTION_RELATIONSHIP_STATUS p_oldStatus) {
        if (p_faction1 == _faction || p_faction2 == _faction) {
            movementComponent.RedetermineFactionsToAvoid(this);
        }
        // if(faction1 == faction) {
        //     if(newStatus == FACTION_RELATIONSHIP_STATUS.Hostile) {
        //         //If at war with another faction, decrease hope 
        //         needsComponent.AdjustHope(-5f);
        //     }else if(oldStatus == FACTION_RELATIONSHIP_STATUS.Hostile && newStatus != FACTION_RELATIONSHIP_STATUS.Hostile) {
        //         //If no longer at war with another faction, increase hope
        //         needsComponent.AdjustHope(-5f);
        //     }
        // }
    }
    public Faction JoinFactionProcessing() {
        Faction chosenFaction = null;
        List<Faction> viableFactions = ObjectPoolManager.Instance.CreateNewFactionList();
        if (traitContainer.HasTrait("Cultist")) {
            viableFactions.Clear();
            for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++) {
                Faction faction = FactionManager.Instance.allFactions[i];
                if (faction.factionType.type == FACTION_TYPE.Demon_Cult
                    && faction != prevFaction
                    && !faction.isDisbanded
                    && !faction.IsCharacterBannedFromJoining(this)
                    && faction.ideologyComponent.DoesCharacterFitCurrentIdeologies(this)) {
                    bool hasCultLeader = faction.HasMemberThatIsNotDeadCultLeader();
                    if (hasCultLeader) {
                        chosenFaction = faction;
                        break;
                    } else {
                        viableFactions.Add(faction);
                    }
                }
            }
            if (chosenFaction == null && viableFactions.Count > 0) {
                chosenFaction = CollectionUtilities.GetRandomElement(viableFactions);
            }
        }
        if (chosenFaction == null) {
            viableFactions.Clear();
            if (currentRegion != null) {
                for (int i = 0; i < currentRegion.factionsHere.Count; i++) {
                    Faction potentialFaction = currentRegion.factionsHere[i];
                    if (potentialFaction.isMajorNonPlayer && !potentialFaction.isDisbanded
                        && !potentialFaction.IsCharacterBannedFromJoining(this)
                        && potentialFaction.ideologyComponent.DoesCharacterFitCurrentIdeologies(this)
                        && potentialFaction != prevFaction) {
                        if (potentialFaction.HasOwnedSettlementInRegion(currentRegion)) {
                            bool hasRelativeOrLoverThatIsNotEnemyRival = faction.HasMemberThatIsNotDeadAndIsFamilyOrLoverAndNotEnemyRivalWith(this);
                            if (hasRelativeOrLoverThatIsNotEnemyRival) {
                                chosenFaction = potentialFaction;
                                break;
                            } else {
                                bool hasCloseFriend = faction.HasMemberThatIsNotDeadAndIsCloseFriendWith(this);
                                if (hasCloseFriend) {
                                    chosenFaction = potentialFaction;
                                    break;
                                } else {
                                    bool hasNoRival = !faction.HasMemberThatIsNotDeadAndIsRivalWith(this);
                                    if (hasNoRival) {
                                        chosenFaction = potentialFaction;
                                        break;
                                    } else {
                                        bool hasFriend = faction.HasMemberThatIsNotDeadAndIsFriendWith(this);
                                        if (hasFriend) {
                                            chosenFaction = potentialFaction;
                                            break;
                                        } else {
                                            if (!viableFactions.Contains(potentialFaction)) {
                                                viableFactions.Add(potentialFaction);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (chosenFaction == null && viableFactions.Count > 0) {
                    chosenFaction = CollectionUtilities.GetRandomElement(viableFactions);
                }
            }
        }
        ObjectPoolManager.Instance.ReturnFactionListToPool(viableFactions);
        if (chosenFaction != null) {
            interruptComponent.TriggerInterrupt(INTERRUPT.Join_Faction, chosenFaction.characters[0], "join_faction_normal");
            return chosenFaction;
        }
        return null;
    }
    private void OnFactionCreated(Faction newFaction, Character creator) {
        if (this != creator) {
            if (faction == null || faction.isMajorNonPlayerOrVagrant) {
                JoinFactionProcessingSpecific(newFaction, creator);
            }
        }
    }
    public void JoinFactionProcessingSpecific(Faction faction, Character factionLeader) {
#if DEBUG_LOG
        string debugLog = "Faction " + faction.name + " is created by " + factionLeader.name + ". " + name + " will try to join";
#endif
        if (isFactionLeader || isSettlementRuler) {
#if DEBUG_LOG
            debugLog += "\nCharacter is faction leader/settlement ruler, WILL NOT JOIN";
            logComponent.PrintLogIfActive(debugLog);
#endif
            return;
        }
        if (partyComponent.isMemberThatJoinedQuest) {
#if DEBUG_LOG
            debugLog += "\nCharacter is in an active party with quest, WILL NOT JOIN";
            logComponent.PrintLogIfActive(debugLog);
#endif
            return;
        }
        if (!faction.ideologyComponent.DoesCharacterFitCurrentIdeologies(this)) {
#if DEBUG_LOG
            debugLog += "\nCharacter is does not fit faction ideologies, WILL NOT JOIN";
            logComponent.PrintLogIfActive(debugLog);
#endif
            return;
        }
        Faction currentFaction = this.faction;
        if (currentFaction == faction) {
#if DEBUG_LOG
            debugLog += "\nCharacter's current faction is the newly created faction, WILL NOT JOIN";
            logComponent.PrintLogIfActive(debugLog);
#endif
            return;
        }
        Character lover = relationshipContainer.GetFirstCharacterWithRelationship(RELATIONSHIP_TYPE.LOVER);
        bool hasNoLoverOrLoverIsNotFriendOrCloseFriend = lover == null || !relationshipContainer.IsFriendsWith(lover);
        bool isFactionLeaderFriendOrCloseFriend = relationshipContainer.IsFriendsWith(factionLeader);

        int chance = 0;
        if (lover == factionLeader && isFactionLeaderFriendOrCloseFriend) {
#if DEBUG_LOG
            debugLog += "\nFaction Leader is Lover and Faction Leader is Friend/Close Friend, 90%";
#endif
            chance = 90;
        } else if (traitContainer.HasTrait("Cultist") && faction.factionType.type == FACTION_TYPE.Demon_Cult) {
#if DEBUG_LOG
            debugLog += "\nCharacter is a Cultist and new faction is a Demon Cult";
#endif
            chance = 50;
        } else if (relationshipContainer.HasRelationshipWith(factionLeader, RELATIONSHIP_TYPE.AFFAIR) && isFactionLeaderFriendOrCloseFriend && hasNoLoverOrLoverIsNotFriendOrCloseFriend) {
#if DEBUG_LOG
            debugLog += "\nFaction Leader is Affair and Faction Leader is Friend/Close Friend and Character has no lover that is friend/close friend, 60%";
#endif
            chance = 60;
        } else if (relationshipContainer.IsFamilyMember(factionLeader) && isFactionLeaderFriendOrCloseFriend) {
#if DEBUG_LOG
            debugLog += "\nFaction Leader is Family Member and Faction Leader is Friend/Close Friend, 30%";
#endif
            chance = 30;
        } else if (isFactionLeaderFriendOrCloseFriend) {
#if DEBUG_LOG
            debugLog += "\nFaction Leader is Friend/Close Friend, 10%";
#endif
            chance = 10;
        } else if (isVagrantOrFactionless && race.IsSapient()) {
#if DEBUG_LOG
            debugLog += "\nCharacter is vagrant and sapient, 15%";
#endif
            chance = 15;
        }

        if (currentFaction != null && currentFaction.HasRelationshipStatusWith(FACTION_RELATIONSHIP_STATUS.Hostile, faction)) {
            chance = Mathf.RoundToInt(chance * 0.5f);
#if DEBUG_LOG
            debugLog += "\nCharacter's current faction is hostile with the new faction, half the chance";
            debugLog += "\nNew chance: " + chance;
#endif
        }

        if (GameUtilities.RollChance(chance)) {
#if DEBUG_LOG
            debugLog += "\nCharacter will join new faction";
#endif
            interruptComponent.TriggerInterrupt(INTERRUPT.Join_Faction, factionLeader, "join_faction_decision");
        } else {
#if DEBUG_LOG
            debugLog += "\nCharacter will NOT join new faction";
#endif
        }
#if DEBUG_LOG
        logComponent.PrintLogIfActive(debugLog);
#endif
    }
    public void ChangeToDefaultFaction() {
        if (isNormalCharacter) {
            ChangeFactionTo(FactionManager.Instance.vagrantFaction, true);
        } else if (minion != null) {
            ChangeFactionTo(PlayerManager.Instance.player.playerFaction, true);
        } else if (IsUndead() || necromancerTrait != null) {
            ChangeFactionTo(FactionManager.Instance.undeadFaction, true);
        } else {
            ChangeFactionTo(FactionManager.Instance.neutralFaction, true);
        }
    }
    #endregion

    #region Carry Component
    /*
        Create a new Party with this character as the leader.
            */
    //public virtual Party CreateOwnParty() {
    //    //if (_ownParty != null) {
    //    //    _ownParty.RemoveCharacter(this);
    //    //}
    //    Party newParty = new Party(this);
    //    SetOwnedParty(newParty);
    //    SetCurrentParty(newParty);
    //    //newParty.AddCharacter(this, true);
    //    //newParty.CreateCharacterObject();
    //    return newParty;
    //}
    //public virtual void SetOwnedParty(Party party) {
    //    ownParty = party;
    //}
    //public virtual void SetCurrentParty(Party party) {
    //    currentParty = party;
    //}
    //public void OnRemovedFromParty() {
    //    SetCurrentParty(ownParty); //set the character's party to it's own party
    //    //if (ownParty is CharacterParty) {
    //    //    if ((ownParty as CharacterParty).actionData.currentAction != null) {
    //    //        (ownParty as CharacterParty).actionData.currentAction.EndAction(ownParty, (ownParty as CharacterParty).actionData.currentTargetObject);
    //    //    }
    //    //}
    //    if (marker) {
    //        marker.visionTrigger.SetAllCollidersState(true);
    //        marker.UpdateAnimation();
    //    }

    //    //if (this.minion != null) {
    //    //    this.minion.SetEnabledState(true); //reenable this minion, since it could've been disabled because it was part of another party
    //    //}
    //}
    //public void OnAddedToParty() {
    //    if (currentParty.id != ownParty.id) {
    //        //currentRegion.RemoveCharacterFromLocation(this); //Why are we removing the character from location if it is added to a party
    //        //ownParty.specificLocation.RemoveCharacterFromLocation(this);
    //        //ownParty.icon.SetVisualState(false);
    //        marker.visionTrigger.SetAllCollidersState(false);
    //        marker.UpdateAnimation();
    //    }
    //}
    //public bool IsInParty() {
    //    return currentParty.isCarryingAnyPOI;
    //    //if (currentParty.characters.Count > 1) {
    //    //    return true; //if the character is in a party that has more than 1 characters
    //    //}
    //    //return false;
    //}
    public void CarryPOI(IPointOfInterest poi, bool changeOwnership = false, bool setOwnership = true) {
        if (poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            carryComponent.CarryPOI(poi);
        } else if (poi.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
            PickUpItem(poi as TileObject, changeOwnership, setOwnership);
        }
    }
    public bool IsPOICarriedOrInInventory(IPointOfInterest poi) {
        if (poi.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
            return HasItem(poi as TileObject);
        }
        return carryComponent.IsPOICarried(poi);
    }
    public bool IsPOICarriedOrInInventory(string poiName) {
        return HasItem(poiName) || carryComponent.IsPOICarried(poiName);
    }
    public void UncarryPOI(IPointOfInterest poi, bool bringBackToInventory = false, bool addToLocation = true, LocationGridTile dropLocation = null) {
        if (poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            carryComponent.UncarryPOI(poi, addToLocation, dropLocation);
        } else if (poi.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
            TileObject item = poi as TileObject;
            //Add to location in uncarry is false because we are going to add to it to location after
            carryComponent.UncarryPOI(poi, false);
            if (!bringBackToInventory) {
                if (addToLocation) {
                    DropItem(item, dropLocation);
                } else {
                    UnobtainItem(item);
                }
            }
        }
    }
    public void UncarryPOI(bool bringBackToInventory = false, bool addToLocation = true, LocationGridTile dropLocation = null) {
        if (carryComponent.isCarryingAnyPOI) {
            IPointOfInterest poi = carryComponent.carriedPOI;
            UncarryPOI(poi, bringBackToInventory, addToLocation, dropLocation);
        }
    }
    public void ShowItemVisualCarryingPOI(TileObject item) {
        if (HasItem(item)) {
            carryComponent.CarryPOI(item);
        }
    }
    //public bool HasOtherCharacterInParty() {
    //    return ownParty.characters.Count > 1;
    //}
    #endregion

    #region Location
    public void SetCurrentStructureLocation(LocationStructure newStructure, bool broadcast = true) {
        if (newStructure == _currentStructure) {
            return; //ignore change;
        }
        LocationStructure previousStructure = _currentStructure;
        _currentStructure = newStructure;
        if (broadcast) {
            if (newStructure != null) {
                Messenger.Broadcast(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE, this, newStructure);
                eventDispatcher.ExecuteCharacterArrivedAtStructure(this, newStructure);
                LocationAwarenessUtility.RemoveFromAwarenessList(this);
                LocationAwarenessUtility.AddToAwarenessList(this, gridTileLocation);
            }
            if (previousStructure != null) {
                eventDispatcher.ExecuteCharacterLeftStructure(this, previousStructure);
                Messenger.Broadcast(CharacterSignals.CHARACTER_LEFT_STRUCTURE, this, previousStructure);
                if (newStructure == null && currentLocationAwareness == previousStructure.locationAwareness) {
                    LocationAwarenessUtility.RemoveFromAwarenessList(this);
                }
            }
        }
        // Debug.Log($"Set current structure location of {name} to {newStructure}");
    }
    public void SetGridTileLocation(LocationGridTile tile) {
        //NOTE: Tile location is being computed every time.
        //this.tile = tile;
        //string summary = string.Empty;
        //if (tile == null) {
        //    summary = GameManager.Instance.TodayLogString() + "Set tile location to null";
        //} else {
        //    summary = GameManager.Instance.TodayLogString() + "Set tile location to " + tile.localPlace.ToString();
        //}
        //locationHistory.Add(summary);
        //if (locationHistory.Count > 80) {
        //    locationHistory.RemoveAt(0);
        //}
    }
    public LocationGridTile GetNearestUnoccupiedTileFromThis() {
        LocationGridTile chosenTile = null;
        if (!isDead && gridTileLocation != null) {
            List<LocationGridTile> unoccupiedNeighbours = RuinarchListPool<LocationGridTile>.Claim();
            gridTileLocation.PopulateUnoccupiedNeighbours(unoccupiedNeighbours, true);
            if (unoccupiedNeighbours.Count > 0) {
                chosenTile = unoccupiedNeighbours[GameUtilities.RandomBetweenTwoNumbers(0, unoccupiedNeighbours.Count - 1)];
            }
            RuinarchListPool<LocationGridTile>.Release(unoccupiedNeighbours);
        }
        return chosenTile;
    }
    public LocationGridTile GetNearestUnoccupiedEdgeTileFromThis() {
        LocationGridTile currentGridTile = gridTileLocation;
        if (currentGridTile.IsAtEdgeOfWalkableMap() && currentGridTile.structure != null) {
            return currentGridTile;
        }

        LocationGridTile nearestEdgeTile = null;
        List<LocationGridTile> neighbours = gridTileLocation.neighbourList;
        for (int i = 0; i < neighbours.Count; i++) {
            if (neighbours[i].IsAtEdgeOfWalkableMap() && neighbours[i].structure != null /*&& !neighbours[i].isOccupied*/) {
                nearestEdgeTile = neighbours[i];
                break;
            }
        }
        if (nearestEdgeTile == null) {
            float nearestDist = -999f;
            for (int i = 0; i < gridTileLocation.parentMap.allEdgeTiles.Count; i++) {
                LocationGridTile currTile = gridTileLocation.parentMap.allEdgeTiles[i];
                float dist = Vector2.Distance(currTile.localLocation, currentGridTile.localLocation);
                if (nearestDist == -999f || dist < nearestDist) {
                    if (currTile.structure != null) {
                        nearestEdgeTile = currTile;
                        nearestDist = dist;
                    }
                }
            }
        }
        return nearestEdgeTile;
    }
    private void OnLeaveArea(Character travellingCharacter) {
        if (carryComponent.IsCurrentlyPartOf(travellingCharacter)) {
            //CheckApprehendRelatedJobsOnLeaveLocatio1n();
            //CancelOrUnassignRemoveTraitRelatedJobs();
            Messenger.Broadcast(CharacterSignals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI_EXCEPT_SELF, this as IPointOfInterest, "");
            CancelAllJobsExceptForCurrent();
            //marker.ClearTerrifyingObjects();
            needsComponent.OnCharacterLeftLocation(currentRegion);
        } else {
            //if (marker.terrifyingObjects.Count > 0) {
            //    marker.RemoveTerrifyingObject(party.owner);
            //    if (party.isCarryingAnyPOI) {
            //        marker.RemoveTerrifyingObject(party.carriedPOI);
            //    }
            //}
        }
    }
    private void OnArrivedAtArea(Character travellingCharacter) {
        if (carryComponent.IsCurrentlyPartOf(travellingCharacter)) {
            //if (isAtHomeArea) {
            //    if (HasTraitOf(TRAIT_TYPE.CRIMINAL)) {
            //        CreateApprehendJob();
            //    }
            //    //for (int i = 0; i < traits.Count; i++) {
            //    //    if (traits[i].name == "Cursed" || traits[i].name == "Sick"
            //    //        || traits[i].name == "Injured" || traits[i].name == "Unconscious") {
            //    //        CreateRemoveTraitJob(traits[i].name);
            //    //    }
            //    //}
            //}
            needsComponent.OnCharacterArrivedAtLocation(currentRegion);
        }
    }
    public void SetRegionLocation(Region region) {
        _currentRegion = region;
        // Debug.Log($"Set region location of {name} to {_currentRegion?.name ?? "Null"}");
    }
    public bool IsInHomeSettlement() {
        if (homeSettlement != null) {
            return currentSettlement == homeSettlement;
        }
        return false;
    }
    public bool HasHome() {
        return homeSettlement != null || (homeStructure != null && !homeStructure.hasBeenDestroyed) || HasTerritory();
    }
    public bool IsAtHome() {
        if (homeStructure != null) {
            return isAtHomeStructure;
        } else if (homeSettlement != null) {
            return IsInHomeSettlement();
        } else if (territory != null) {
            return IsInTerritory();
        }
        return false;
    }
    public int GetAliveResidentsCountInHome() {
        int residentCount = 0;
        if (homeSettlement != null) {
            residentCount = homeSettlement.residents.Count(x => x.isDead == false);
        } else if (homeStructure != null) {
            residentCount = homeStructure.residents.Count(x => x.isDead == false);
        } else if (HasTerritory()) {
            residentCount = territory.region.GetCountOfAliveCharacterWithSameTerritory(this);
        }
        return residentCount;
    }
    #endregion

    #region Utilities
    public bool IsUndead() {
        if (characterClass.IsZombie() || (this is Summon summon &&
            (summon.summonType == SUMMON_TYPE.Ghost || summon.summonType == SUMMON_TYPE.Skeleton ||
             summon.summonType == SUMMON_TYPE.Vengeful_Ghost || summon.summonType == SUMMON_TYPE.Revenant))) {
            return true;
        }
        return false;
    }
    private bool AssignRace(RACE race, bool isInitial = false) {
        if (_raceSetting == null || _raceSetting.race != race) {
            if (_raceSetting != null) {
                if (_raceSetting.race == race) {
                    return false; //current race is already the new race, no change
                }
                for (int i = 0; i < _raceSetting.traitNames.Length; i++) {
                    traitContainer.RemoveTrait(this, _raceSetting.traitNames[i]); //Remove traits from race
                }
            }
            _raceSetting = RaceManager.Instance.GetRaceData(race);
            if (!isInitial) {
                OnUpdateRace();
                Messenger.Broadcast(CharacterSignals.CHARACTER_CHANGED_RACE, this);
            }
            return true;
        }
        return false;
    }
    protected void OnUpdateRace() {
        combatComponent.UpdateBasicData(false);
        needsComponent.UpdateBaseStaminaDecreaseRate();
        for (int i = 0; i < _raceSetting.traitNames.Length; i++) {
            traitContainer.AddTrait(this, _raceSetting.traitNames[i]);
        }
        //Update Portrait to use new race
        visuals.UpdateAllVisuals(this, true);
        //update goap interactions that should no longer be valid
        if (race == RACE.SKELETON) {
            RemoveAdvertisedAction(INTERACTION_TYPE.DRINK_BLOOD);
            RemoveAdvertisedAction(INTERACTION_TYPE.SHARE_INFORMATION);
            RemoveAdvertisedAction(INTERACTION_TYPE.REPORT_CRIME);
        } else {
            AddAdvertisedAction(INTERACTION_TYPE.DRINK_BLOOD);
            AddAdvertisedAction(INTERACTION_TYPE.SHARE_INFORMATION);
            AddAdvertisedAction(INTERACTION_TYPE.REPORT_CRIME);
        }
        if (race.IsSapient()) {
            AddAdvertisedAction(INTERACTION_TYPE.REPORT_CORRUPTED_STRUCTURE);
        } else {
            RemoveAdvertisedAction(INTERACTION_TYPE.REPORT_CORRUPTED_STRUCTURE);
        }
    }
    public void SetRandomName() {
        string newName = RandomNameGenerator.GenerateRandomName(race, gender);
        string[] split = newName.Split(' ');
        string firstName = split[0];
        string surName = string.Empty;
        if (split.Length > 1) {
            surName = split[1];
        }
        SetFirstAndLastName(firstName, surName);
    }
    public void SetFirstAndLastName(string p_firstName, string p_lastName) {
        _firstName = p_firstName;
        _surName = p_lastName;
        RandomNameGenerator.RemoveNameAsAvailable(gender, race, fullName);
    }
    public void RenameCharacter(string p_firstName, string p_lastName) {
        SetFirstAndLastName(p_firstName, p_lastName);
        UpdateCurrentLogsBasedOnUpdatedCharacter(this); //had to do this since signal order can be inconsistent and the UI Update could happen before the actual logs were updated.
        bookmarkEventDispatcher.ExecuteBookmarkChangedNameOrElementsEvent(this);
        if (lycanData?.limboForm != null) {
            lycanData.limboForm.SetFirstAndLastName(firstName, surName);
            lycanData.limboForm.bookmarkEventDispatcher.ExecuteBookmarkChangedNameOrElementsEvent(lycanData.limboForm);
        }
        Messenger.Broadcast(CharacterSignals.CHARACTER_CHANGED_NAME, this);
    }
    private string GetDefaultRaceClassName() {
        if (race == RACE.DEMON) {
            return $"{characterClass.className} {GameUtilities.GetNormalizedRaceAdjective(race)}";
        } else if (race == RACE.RATMAN) {
            return $"{characterClass.className}";
        } else if (characterClass.className == "Necromancer") {
            return $"{characterClass.className}";
        }
        return $"{GameUtilities.GetNormalizedRaceAdjective(race)} {characterClass.className}";
    }
    public void CenterOnCharacter() {
        // if (GameManager.Instance.gameHasStarted == false) {
        //     return;
        // }
        if (isInLimbo) {
            if (isLycanthrope && lycanData.activeForm != this) {
                lycanData.activeForm.CenterOnCharacter();
            }
        } else {
            if (marker) {
                if (carryComponent.masterCharacter.gridTileLocation != null) {
                    bool instantCenter = !InnerMapManager.Instance.IsShowingInnerMap(currentRegion);
                    if (instantCenter) {
                        InnerMapManager.Instance.ShowInnerMap(carryComponent.masterCharacter.gridTileLocation.structure.region, false);
                    }
                    InnerMapCameraMove.Instance.CenterCameraOn(marker.gameObject, instantCenter);
                }
            } else if (grave != null && grave.mapObjectVisual != null && (grave.gridTileLocation != null || grave.isBeingCarriedBy != null)) {
                Region region = null;
                if (grave.isBeingCarriedBy != null) {
                    region = grave.isBeingCarriedBy.currentRegion;
                } else if (grave.gridTileLocation != null) {
                    region = grave.gridTileLocation.parentMap.region;
                }
                if (region != null) {
                    if (!InnerMapManager.Instance.IsShowingInnerMap(region)) {
                        InnerMapManager.Instance.ShowInnerMap(region, false);
                    }
                    InnerMapCameraMove.Instance.CenterCameraOn(grave.mapObjectVisual.gameObject);
                }
            }
            //Removed connectedFoodPile because the character reference will be removed from the master list once the body is destroyed
            //else if (connectedFoodPile != null && connectedFoodPile.mapObjectVisual != null && (connectedFoodPile.gridTileLocation != null || connectedFoodPile.isBeingCarriedBy != null)) {
            //    Region region = null;
            //    if (connectedFoodPile.isBeingCarriedBy != null) {
            //        region = connectedFoodPile.isBeingCarriedBy.currentRegion;
            //    } else if (connectedFoodPile.gridTileLocation != null){
            //        region = connectedFoodPile.gridTileLocation.parentMap.region;
            //    }
            //    if (region != null) {
            //        if (!InnerMapManager.Instance.IsShowingInnerMap(region)) {
            //            InnerMapManager.Instance.ShowInnerMap(region, false);
            //        }
            //        InnerMapCameraMove.Instance.CenterCameraOn(connectedFoodPile.mapObjectVisual.gameObject);    
            //    }
            //}
        }

        // else {
        //     if (InnerMapManager.Instance.isAnInnerMapShowing) {
        //         InnerMapManager.Instance.HideAreaMap();
        //     }
        //     CameraMove.Instance.CenterCameraOn(currentRegion.coreTile.gameObject);
        // }
    }
    private void OnCharacterDied(Character characterThatDied) {
        if (!characterThatDied.persistentID.Equals(persistentID)) {
            if (isDead) {
                return;
            }
            ////Non villagers should not feel griefstricken
            ////https://trello.com/c/U0gnV2Rs/1116-zombies-should-no-longer-become-griefstricken-betrayed-etc

            ////No more griefstricken feeling to a target that is not a villager anymore
            ////https://trello.com/c/CxFjFHtv/1121-no-more-griefstricken-if-a-zombie-died-unlike-its-first-death
            //if (isNormalCharacter && characterThatDied.isNormalCharacter) {
            //    string opinionLabel = relationshipContainer.GetOpinionLabel(characterThatDied);
            //    if (opinionLabel == RelationshipManager.Close_Friend
            //        || (relationshipContainer.HasSpecialPositiveRelationshipWith(characterThatDied)
            //            && relationshipContainer.IsEnemiesWith(characterThatDied) == false)) {
            //        needsComponent.AdjustHope(-10f);
            //        if (!traitContainer.HasTrait("Psychopath")) {
            //            traitContainer.AddTrait(this, "Griefstricken", characterThatDied);
            //        }
            //    } else if (opinionLabel == RelationshipManager.Friend) {
            //        needsComponent.AdjustHope(-5f);
            //    }
            //}

            if (characterThatDied.currentRegion == homeRegion) {
                //if a hostile character has been killed within the character's home npcSettlement, Hope increases by XX amount.
                if (IsHostileWith(characterThatDied)) {
                    needsComponent.AdjustHope(5f);
                }
            }
            //RemoveRelationship(characterThatDied); //do not remove relationships when dying
            if (marker) {
                marker.OnOtherCharacterDied(characterThatDied);
            }
        } else {
            crimeComponent.OnCharacterDied();
        }
    }
    private void OnBeforeSeizingPOI(IPointOfInterest poi) {
        if (poi is Character character) {
            OnBeforeSeizingCharacter(character);
        } else if (poi is TileObject tileObject) {
            OnBeforeSeizingTileObject(tileObject);
        }
    }
    private void OnSeizePOI(IPointOfInterest poi) {
        if (poi != this) {
            if (hasMarker && marker.IsTargetPOIInPathfinding(poi)) {
                if (marker.hasFleePath) {
                    marker.SetHasFleePath(false);
                    marker.pathfindingAI.ClearAllCurrentPathData();
                    combatComponent.SetWillProcessCombat(true);
                }
            }
        }
        if (poi is Character character) {
            OnSeizeCharacter(character);
        } else if (poi is TileObject tileObject) {
            OnSeizeTileObject(tileObject);
        }
    }
    private void OnBeforeSeizingCharacter(Character character) {
        if (this == character) {
            marker.OnBeforeSeizingThisCharacter();
        } else {
            marker.OnBeforeSeizingOtherCharacter(character);
        }
        //if (character.id != id) {
        //    //RemoveRelationship(characterThatDied); //do not remove relationships when dying
        //    marker.OnBeforeSeizingOtherCharacter(character);
        //}
    }
    private void OnSeizeCharacter(Character character) {
        if (character != this) {
            OnSeizeOtherCharacter(character);
        }
    }
    private void OnSeizeOtherCharacter(Character character) {
        //RemoveRelationship(characterThatDied); //do not remove relationships when dying
        combatComponent.RemoveHostileInRange(character);
        combatComponent.RemoveAvoidInRange(character);
        //if (character is Summon) {
        //    currentSettlement?.SettlementResources?.RemoveAnimalFromSettlement(character as Summon);
        //} else {
        //    currentSettlement?.SettlementResources?.RemoveCharacterFromSettlement(this);
        //}
    }

    private void OnBeforeSeizingTileObject(TileObject tileObject) {
        //if(faction != null && faction.isMajorNonPlayerFriendlyNeutral && marker) {
        //    if (marker.IsPOIInVision(tileObject)) {
        //        PlayerManager.Instance.player.threatComponent.AdjustThreat(5);
        //    }
        //}
    }
    private void OnSeizeTileObject(TileObject tileObject) {
        if (currentActionNode != null && currentActionNode.poiTarget == tileObject) {
            StopCurrentActionNode();
        }
        Character[] currentUsers = tileObject.users;
        if (currentUsers != null && currentUsers.Length > 0) {
            for (int i = 0; i < currentUsers.Length; i++) {
                Character currentUser = currentUsers[i];
                if (currentUser != null) {
                    currentUser.StopCurrentActionNode();
                    if (tileObject.RemoveUser(currentUser)) {
                        i--;
                    }
                }
                //i--;
            }
        }
    }
    public void AdjustDoNotRecoverHP(int amount) {
        doNotRecoverHP += amount;
        doNotRecoverHP = Math.Max(doNotRecoverHP, 0);
    }
    public override string ToString() {
        return name;
    }
    private LocationGridTile GetLocationGridTileByXY(int x, int y, bool throwOnException = true) {
        if (currentRegion != null) {
            if (UtilityScripts.Utilities.IsInRange(x, 0, currentRegion.innerMap.width)
                && UtilityScripts.Utilities.IsInRange(y, 0, currentRegion.innerMap.height)) {
                return currentRegion.innerMap.map[x, y];
            }
            int xCoordinate = Mathf.Clamp(x, 0, currentRegion.innerMap.width - 1);
            int yCoordinate = Mathf.Clamp(y, 0, currentRegion.innerMap.height - 1);
            return currentRegion.innerMap.map[xCoordinate, yCoordinate];
        }
        return null;
    }
    public void UpdateCanCombatState() {
        bool combatState = traitContainer.HasTrait("Combatant") && !traitContainer.HasTrait("Injured");
        if (canCombat != combatState) {
            canCombat = combatState;
            if (canCombat == false) {
                Messenger.Broadcast(CharacterSignals.CHARACTER_CAN_NO_LONGER_COMBAT, this);
            }
            //if (canCombat && marker) {
            //    marker.ClearTerrifyingObjects();
            //}
        }
    }
    private bool CanCharacterReact(IPointOfInterest targetPOI = null) {
        if (!limiterComponent.canWitness || !limiterComponent.canPerform) {
            return false; //this character cannot witness
        }
        if (interruptComponent.isInterrupted) {
            //Cannot react if interrupted
            return false;
        }
        if (!isNormalCharacter) {
            //Cannot react if summon or minion
            return false;
        }
        //if (defaultCharacterTrait.hasSurvivedApprehension && !isAtHomeRegion) {
        //    return false; //Must not react because he will only have one thing to do and that is to return home
        //}
        if (stateComponent.currentState != null && !stateComponent.currentState.isDone) {
            if (stateComponent.currentState.characterState == CHARACTER_STATE.COMBAT) {
                //Character must not react if he/she is in flee or engage state
                return false;
            }
            if (stateComponent.currentState.characterState == CHARACTER_STATE.DOUSE_FIRE) {
                //Character must not react if he/she is in douse fire state
                return false;
            }
        }
        //if (traitContainer.GetNormalTrait<Trait>("Berserked") != null) { //|| (stateComponent.stateToDo != null && stateComponent.stateToDo.characterState == CHARACTER_STATE.BERSERKED && !stateComponent.stateToDo.isDone)
        //    //Character must not react if he/she is in berserked state
        //    //Returns true so that it will create an impression that the character actually created a job even if he/she didn't, so that the character will not chat, etc.
        //    return false;
        //}
        //if (traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE)) {
        //    return false;
        //}
        if (targetPOI != null && targetPOI is Character) {
            Character target = targetPOI as Character;
            if (target.faction != null && target.faction.IsHostileWith(faction)) {
                //Cannot react if target charcter is from a hostile faction
                //Only combat those characters that's why they cannot react to their traits, actions, etc.
                return false;
            }
        }
        return true;
    }
    public bool IsAble() {
        return !isDead && !characterClass.IsZombie(); //!traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE)
    }
    public void SetTileObjectLocation(TileObject tileObject) {
        tileObjectLocation = tileObject;
    }
    public void AdjustNumOfActionsBeingPerformedOnThis(int amount) {
        numOfActionsBeingPerformedOnThis += amount;
        numOfActionsBeingPerformedOnThis = Mathf.Max(0, numOfActionsBeingPerformedOnThis);
        if (marker) {
            marker.UpdateAnimation();
        }
    }
    public virtual bool IsValidCombatTargetFor(IPointOfInterest source) {
        return isDead == false /*&& (limiterComponent.canPerform || canMove)*/ && hasMarker
                && gridTileLocation != null && source.gridTileLocation != null && (source is Character character && character.movementComponent.HasPathToEvenIfDiffRegion(gridTileLocation)); //traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE) == false
    }
    public void SetHasUnresolvedCrime(bool state) {
        hasUnresolvedCrime = state;
    }
    public void SetIsInLimbo(bool state) {
        isInLimbo = state;
    }
    public void SetIsLimboCharacter(bool state) {
        isLimboCharacter = state;
    }
    public void SetHasSeenFire(bool state) {
        hasSeenFire = state;
    }
    public void SetHasSeenWet(bool state) {
        hasSeenWet = state;
    }
    public void SetHasSeenPoisoned(bool state) {
        hasSeenPoisoned = state;
    }
    public void SetDestroyMarkerOnDeath(bool state) {
        destroyMarkerOnDeath = state;
    }
    public void SetIsWanderer(bool state) {
        if (isWanderer != state) {
            isWanderer = state;
            if (isWanderer) {
                //Note: I put this in the DefaultWanderer behaviour instead because there will be issues if we add territory here immediately
                //Sometimes when a character migrates to another home settlement he/she becomes a temporary wanderer for a split second because this is triggered every time his home settlement is set to null
                //Due to it, he now adds a territory, then when the new home settlement is set, he already has a job to go to the territory, which must not happen
                //if (!HasTerritory() && currentRegion != null) {
                //    HexTile initialTerritory = currentRegion.GetRandomNoStructureUncorruptedPlainHex();
                //    if(initialTerritory != null) {
                //        AddTerritory(initialTerritory);
                //    } else {
                //        logComponent.PrintLogIfActive(name + " is a wanderer but could not set temporary territory");
                //    }
                //}
                behaviourComponent.ChangeDefaultBehaviourSet(CharacterManager.Default_Wanderer_Behaviour);
            } else {
                if (HasTerritory()) {
                    ClearTerritory();
                }
                behaviourComponent.ChangeDefaultBehaviourSet(CharacterManager.Default_Resident_Behaviour);
            }
        }
    }
    public void SetHasBeenRaisedFromDead(bool state) {
        hasBeenRaisedFromDead = state;
    }
    public bool IsConsideredInDangerBy(Character character) {
        if (traitContainer.HasTrait("Enslaved") && faction != character.faction) {
            return true;
        }
        if (traitContainer.HasTrait("Restrained", "Ensnared", "Frozen")) {
            return !IsAtHome();
        }
        if (HasHome()) {
            if (!IsAtHome()) {
                //If cannot return home, consider in danger
                return !movementComponent.CanReturnHome();
            }
        }
        return false;
    }
    public void SetIsPreplaced(bool state) {
        isPreplaced = state;
    }
    private string GetFirstNameWithColor() {
        if (CharacterManager.Instance != null) {
            string color = CharacterManager.Instance.GetCharacterNameColorHex(this);
            return $"<color=#{color}>{_firstName}</color>";
        }
        return firstName;
    }
    public bool IsRatmanThatIsPartOfMajorFaction() {
        return race == RACE.RATMAN && faction != null && faction.isMajorNonPlayer;
    }
    public string GetCultistUnableToDoJobReason(GoapPlanJob job, Precondition failedPrecondition, INTERACTION_TYPE failedPreconditionActionType) {
        string reason = job.GetJobDetailString();
        if (failedPrecondition != null && failedPrecondition.goapEffect.conditionType == GOAP_EFFECT_CONDITION.TAKE_POI) {
            reason = $"{reason}, could not find a {failedPrecondition.goapEffect.conditionKey} to do action {UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(failedPreconditionActionType.ToString())}";
        }
        return reason;
    }
    public void LogUnableToDoJob(string reason) {
        Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "cancel_job_no_plan", providedTags: LOG_TAG.Work);
        log.AddToFillers(this, name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(null, reason, LOG_IDENTIFIER.STRING_1);
        logComponent.RegisterLog(log, true);
    }
    public void SetDeployedAtStructure(LocationStructure p_structure) {
        deployedAtStructure = p_structure;
    }
    #endregion

    #region History/Logs
    public virtual void OnActionPerformed(ActualGoapNode node) {
        ///Moved all needed checking <see cref="CharacterManager.OnActionStateSet(GoapAction, GoapActionState)"/>
        if (isDead || !limiterComponent.canWitness) {
            return;
        }
        //if (node.action.goapType == INTERACTION_TYPE.WATCH) {
        //    //Cannot witness/watch a watch action
        //    return;
        //}
        if (node.actor == this) {
            //Cannot witness if character is part of the action
            return;
        }
        //if (!node.action.shouldAddLogs) {
        //    return;
        //}

        //Instead of witnessing the action immediately, it needs to be pooled to avoid duplicates, so add the supposed to be witnessed action to the list and let ProcessAllUnprocessedVisionPOIs in CharacterMarker do its thing
        if (marker) { //&& !marker.actionsToWitness.Contains(node)
            if (marker.IsPOIInVision(node.actor)) {
                //marker.actionsToWitness.Add(node);
                //This is done so that the character will react again
                marker.AddUnprocessedAction(node);
            } else if (marker.IsPOIInVision(node.poiTarget)) {
                //marker.actionsToWitness.Add(node);
                //This is done so that the character will react again
                marker.AddUnprocessedAction(node);
            }
        }

        //ThisCharacterWitnessedEvent(action);
        //ThisCharacterWatchEvent(null, action, state);
    }
    public virtual void OnInterruptStarted(InterruptHolder interruptHolder) {
        if (isDead || !limiterComponent.canWitness) {
            return;
        }
        if (interruptHolder.actor == this) {
            return;
        }
        if (marker) {
            if (marker.IsPOIInVision(interruptHolder.actor)) {
                //This is done so that the character will react again
                marker.AddUnprocessedPOI(interruptHolder.actor, true);
            }
            //else if (marker.IsPOIInVision(target)) {
            //    //This is done so that the character will react again
            //    marker.unprocessedVisionPOIs.Add(target);
            //}
        }
    }
    //public void AddOverrideThought(string log) {
    //    _overrideThoughts.Add(log);
    //}
    //public void RemoveOverrideThought(string log) {
    //    _overrideThoughts.Remove(log);
    //}
    //Returns the list of goap actions to be witnessed by this character
    public void ThisCharacterSaw(IPointOfInterest target, bool reactToActionOnly = false) {
        //if (isDead) {
        //    return;
        //}
        IPointOfInterest poiTarget = target;
        Character copiedTarget = null;
        Character targetCharacter = null;
        if (target is Character targetChar) {
            targetCharacter = targetChar;
            if (targetCharacter.reactionComponent.disguisedCharacter != null) {
                //Whenever a disguised character is being processed, process the original instead
                copiedTarget = targetCharacter.reactionComponent.disguisedCharacter;
            }
        }
        if (copiedTarget != null) {
            poiTarget = copiedTarget;
        }

        List<Trait> traitOverrideFunctions = traitContainer.GetTraitOverrideFunctions(TraitManager.See_Poi_Cannot_Witness_Trait);
        if (traitOverrideFunctions != null) {
            for (int i = 0; i < traitOverrideFunctions.Count; i++) {
                Trait trait = traitOverrideFunctions[i];
                trait.OnSeePOIEvenCannotWitness(poiTarget, this);
            }
        }

        //This is a temporary fix for berserk behaviour where in the berserked character can add hostiles even when cannot witness
        //I did this because the cannot witness part affects all traits that has cannot witness, like Frozen
        //Ex: Even when Frozen, the character can add hostiles/combat job which is not suppose to happen
        // if (limiterComponent.canPerform && traitContainer.HasTrait("Berserked")) {
        //     Berserked berserked = traitContainer.GetNormalTrait<Berserked>("Berserked");
        //     berserked.BerserkCombat(target, this);
        // }
        //for (int i = 0; i < traitContainer.statuses.Count; i++) {
        //    traitContainer.statuses[i].OnSeePOIEvenCannotWitness(target, this);
        //}

        if (!limiterComponent.canWitness) {
            return;
        }
        //if (currentActionNode != null && currentActionNode.actionStatus == ACTION_STATUS.STARTED && currentActionNode.isStealth) {
        //    if (currentActionNode.poiTarget == poiTarget) {
        //        //Upon seeing the target while performing a stealth job action, check if it can do the action
        //        if (!marker.CanDoStealthActionToTarget(poiTarget)) {
        //            bool shouldDoAfterEffect = currentActionNode.action.goapType != INTERACTION_TYPE.REMOVE_BUFF;
        //            currentJob.CancelJob(reason: "There is a witness around", shouldDoAfterEffect: shouldDoAfterEffect);
        //        }
        //    } else {
        //        //Upon seeing other characters while target of stealth action is already in vision, automatically cancel job
        //        if (poiTarget is Character seenCharacter && seenCharacter.isNormalCharacter) {
        //            if (marker.IsPOIInVision(currentActionNode.poiTarget)) {
        //                bool shouldDoAfterEffect = currentActionNode.action.goapType != INTERACTION_TYPE.REMOVE_BUFF;
        //                currentJob.CancelJob(reason: "There is a witness around", shouldDoAfterEffect: shouldDoAfterEffect);
        //            }
        //        }
        //    }
        //}

        //React To Actions
        ActualGoapNode targetCharacterCurrentActionNode = null;
        //Character targetCharacter = null;
        if (targetCharacter != null) {
            //targetCharacter = target as Character;
            //React To Interrupt
            if (targetCharacter.interruptComponent.isInterrupted) {
                reactionComponent.ReactTo(targetCharacter.interruptComponent.currentInterrupt, REACTION_STATUS.WITNESSED);
            } else {
                //targetCharacter.OnSeenBy(this); //trigger that the target character was seen by this character.
                targetCharacterCurrentActionNode = targetCharacter.currentActionNode;
                if (targetCharacterCurrentActionNode != null /*&& node.action.shouldAddLogs*/ && targetCharacterCurrentActionNode.actionStatus != ACTION_STATUS.STARTED &&
                    targetCharacterCurrentActionNode.actionStatus != ACTION_STATUS.NONE && targetCharacterCurrentActionNode.actor != this) {
                    reactionComponent.ReactTo(targetCharacterCurrentActionNode, REACTION_STATUS.WITNESSED);
                }
                //else if (targetCharacter.combatComponent.isInCombat) {
                //    if (targetCharacter.stateComponent.currentState is CombatState combatState) {
                //        targetCharacterCurrentActionNode = combatState.actionThatTriggeredThisState;
                //        if (targetCharacterCurrentActionNode != null) {
                //            reactionComponent.ReactTo(targetCharacterCurrentActionNode, REACTION_STATUS.WITNESSED);
                //        }
                //    }
                //}
            }
        }
        if (target.allJobsTargetingThis.Count > 0) {
            //We get the actions targeting the target character because a character must also witness an action even if he only sees the target and not the actor
            for (int i = 0; i < target.allJobsTargetingThis.Count; i++) {
                if (target.allJobsTargetingThis[i] is GoapPlanJob) {
                    GoapPlanJob job = target.allJobsTargetingThis[i] as GoapPlanJob;
                    GoapPlan plan = job.assignedPlan;
                    if (plan != null /*&& plan.currentActualNode.action.shouldAddLogs*/
                        && plan.currentActualNode.actionStatus != ACTION_STATUS.STARTED && plan.currentActualNode.actionStatus != ACTION_STATUS.NONE
                        && plan.currentActualNode != targetCharacterCurrentActionNode && plan.currentActualNode.actor != this) {
                        reactionComponent.ReactTo(plan.currentActualNode, REACTION_STATUS.WITNESSED);
                    }
                }
            }
        }
        if (!reactToActionOnly) {
            //React To Character, Object, and Item
            string debugLog = string.Empty;
            if (copiedTarget != null) {
                reactionComponent.ReactToDisguised(target as Character, copiedTarget, ref debugLog);
            } else {
                reactionComponent.ReactTo(target, ref debugLog);
            }
#if DEBUG_LOG
            if (string.IsNullOrEmpty(debugLog) == false) {
                logComponent.PrintLogIfActive(debugLog);
            }
#endif
            //if(targetCharacter != null) {
            //    ThisCharacterWatchEvent(targetCharacter, null, null);
            //}
        }
    }
    public void ThisCharacterSawAction(ActualGoapNode action) {
        if (!limiterComponent.canWitness) {
            return;
        }
        reactionComponent.ReactTo(action, REACTION_STATUS.WITNESSED);
    }
    //public List<Log> GetWitnessOrInformedMemories(int dayFrom, int dayTo, Character involvedCharacter = null) {
    //    List<Log> memories = new List<Log>();
    //    for (int i = 0; i < _history.Count; i++) {
    //        Log historyLog = _history[i];
    //        if (historyLog.goapAction != null && (historyLog.key == "witness_event" || historyLog.key == "informed_event")) {
    //            if (historyLog.day >= dayFrom && historyLog.day <= dayTo) {
    //                if(involvedCharacter != null) {
    //                    if (historyLog.goapAction.actor == involvedCharacter || historyLog.goapAction.IsTarget(involvedCharacter)) {
    //                        memories.Add(historyLog);
    //                    }
    //                } else {
    //                    memories.Add(historyLog);
    //                }
    //            }
    //        }
    //    }
    //    return memories;
    //}
    //public List<Log> GetCrimeMemories(int dayFrom, int dayTo, Character involvedCharacter = null) {
    //    List<Log> memories = new List<Log>();
    //    for (int i = 0; i < _history.Count; i++) {
    //        Log historyLog = _history[i];
    //        if (historyLog.goapAction != null && historyLog.goapAction.awareCharactersOfThisAction.Contains(this) && historyLog.goapAction.committedCrime != CRIME.NONE) {
    //            if (historyLog.day >= dayFrom && historyLog.day <= dayTo) {
    //                if(involvedCharacter != null) {
    //                    for (int j = 0; j < historyLog.goapAction.crimeCommitters.Length; j++) {
    //                        Character criminal = historyLog.goapAction.crimeCommitters[j];
    //                        if (criminal == involvedCharacter) {
    //                            memories.Add(historyLog);
    //                            break;
    //                        }
    //                    }
    //                } else {
    //                    memories.Add(historyLog);
    //                }
    //            }
    //        }
    //    }
    //    return memories;
    //}
    //public void ThisCharacterWitnessedEvent(ActualGoapNode witnessedEvent) {
    //    //if (isDead || !limiterComponent.canWitness) {
    //    //    return;
    //    //}
    //    if (faction != witnessedEvent.actor.faction && //only check faction relationship if involved characters are of different factions
    //        faction.IsHostileWith(witnessedEvent.actor.faction)) {
    //        //Must not react if the faction of the actor of witnessed action is hostile with the faction of the witness
    //        return;
    //    }


    //    if (witnessedEvent.currentStateName == null) {
    //        throw new System.Exception(GameManager.Instance.TodayLogString() + this.name + " witnessed event " + witnessedEvent.action.goapName + " by " + witnessedEvent.actor.name + " but it does not have a current state!");
    //    }
    //    if (witnessedEvent.descriptionLog == null) {
    //        throw new Exception(GameManager.Instance.TodayLogString() + this.name + " witnessed event " + witnessedEvent.action.goapName + " by " + witnessedEvent.actor.name + " with state " + witnessedEvent.currentStateName + " but it does not have a description log!");
    //    }
    //    Log witnessLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "Generic", "witness_event", witnessedEvent);
    //    witnessLog.AddToFillers(this, name, LOG_IDENTIFIER.OTHER);
    //    witnessLog.AddToFillers(null, Utilities.LogDontReplace(witnessedEvent.descriptionLog), LOG_IDENTIFIER.APPEND);
    //    witnessLog.AddToFillers(witnessedEvent.descriptionLog.fillers);
    //    AddHistory(witnessLog);

    //    CRIME_TYPE crimeType = CrimeManager.Instance.GetCrimeTypeConsideringAction(witnessedEvent);
    //    if (crimeType != CRIME_TYPE.NONE) {
    //        CrimeManager.Instance.ReactToCrime(this, witnessedEvent, witnessedEvent.associatedJobType, crimeType);
    //    }

    //    //if (faction.isPlayerFaction) {
    //    //    //Player characters cannot react to witnessed events
    //    //    return;
    //    //}
    //    //if (witnessedEvent.currentState.shareIntelReaction != null && !isFactionless) { //Characters with no faction cannot witness react
    //    //    List<string> reactions = witnessedEvent.currentState.shareIntelReaction.Invoke(this, null, SHARE_INTEL_STATUS.WITNESSED);
    //    //    if(reactions != null) {
    //    //        string reactionLog = name + " witnessed event: " + witnessedEvent.goapName;
    //    //        reactionLog += "\nREACTION:";
    //    //        for (int i = 0; i < reactions.Count; i++) {
    //    //            reactionLog += "\n" + reactions[i];
    //    //        }
    //    //        PrintLogIfActive(reactionLog);
    //    //    }
    //    //}
    //    //witnessedEvent.AddAwareCharacter(this);

    //    //If a character sees or informed about a lover performing Making Love or Ask to Make Love, they will feel Betrayed
    //    //if (witnessedEvent.actor != this && !witnessedEvent.IsTarget(this)) {
    //    //    Character target = witnessedEvent.poiTarget as Character;
    //    //    if (witnessedEvent.goapType == INTERACTION_TYPE.MAKE_LOVE) {
    //    //        target = (witnessedEvent as MakeLove).targetCharacter;
    //    //        if (relationshipContainer.HasRelationshipWith(witnessedEvent.actor.currentAlterEgo, RELATIONSHIP_TRAIT.LOVER) || relationshipContainer.HasRelationshipWith(target.currentAlterEgo, RELATIONSHIP_TRAIT.LOVER)) {
    //    //            Betrayed betrayed = new Betrayed();
    //    //            traitContainer.AddTrait(this, betrayed);
    //    //            //RelationshipManager.Instance.RelationshipDegradation(witnessedEvent.actor, this, witnessedEvent);
    //    //            //RelationshipManager.Instance.RelationshipDegradation(target, this, witnessedEvent);
    //    //        } 
    //    //    } else if (witnessedEvent.goapType == INTERACTION_TYPE.INVITE) {
    //    //        if (relationshipContainer.HasRelationshipWith(witnessedEvent.actor.currentAlterEgo, RELATIONSHIP_TRAIT.LOVER)) {
    //    //            Betrayed betrayed = new Betrayed();
    //    //            traitContainer.AddTrait(this, betrayed);
    //    //            //RelationshipManager.Instance.RelationshipDegradation(witnessedEvent.actor, this, witnessedEvent);
    //    //            //RelationshipManager.Instance.RelationshipDegradation(target, this, witnessedEvent);
    //    //        } else if (relationshipContainer.HasRelationshipWith(target.currentAlterEgo, RELATIONSHIP_TRAIT.LOVER)) {
    //    //            if (witnessedEvent.currentState.name == "Invite Success") {
    //    //                Betrayed betrayed = new Betrayed();
    //    //                traitContainer.AddTrait(this, betrayed);
    //    //                //RelationshipManager.Instance.RelationshipDegradation(witnessedEvent.actor, this, witnessedEvent);
    //    //                //RelationshipManager.Instance.RelationshipDegradation(target, this, witnessedEvent);
    //    //            }
    //    //        }
    //    //    }
    //    //}
    //}
    #endregion

    #region Combat Handlers
    /// <summary>
    /// This character was hit by an attack.
    /// </summary>
    /// <param name="characterThatAttacked">The character that attacked this.</param>
    /// <param name="combatStateOfAttacker">The combat state that the attacker is in.</param>
    /// <param name="attackSummary">reference log of what happened.</param>
    public void OnHitByAttackFrom(Character characterThatAttacked, CombatState combatStateOfAttacker, ref string attackSummary) {
        // CombatManager.Instance.CreateHitEffectAt(this, elementalType);
        if (characterThatAttacked == null) {
            return;
        }
        if (!HasHealth()) {
            return; //if hp is already 0, do not deal damage
        }
        bool isPlayerSource = false;
        if (characterThatAttacked.faction != null && characterThatAttacked.faction.isPlayerFaction) {
            //If responsible character is part of player faction, tag this as Player Source also
            isPlayerSource = true;
        }
        //If target is cannot perform/cannot move, 100% chance to knockout, reason: for abducting resting characters
        //Put this here so that we will have the chance to knockout before applying damage, since once the character receives damage they will automatically wake up from sleeping
        //So we need to check the knockout chance before applying damage
        int chanceToKnockout = 0;
        if (!limiterComponent.canPerform || !limiterComponent.canMove) {
            //https://trello.com/c/QTYC0Rb4/3161-non-lethal-attack-of-sleeping-characters-should-only-have-50-instant-knockout-rate
            chanceToKnockout = 50;
#if DEBUG_LOG
            attackSummary += $"\nTarget Cannot Perform/Move Knockout Chance: 50%";
#endif
        } else {
            chanceToKnockout = GetChanceToBeKnockedOutBy(characterThatAttacked, ref attackSummary);
        }
        if (characterThatAttacked.combatComponent.combatBehaviourParent.IsCombatBehaviour(CHARACTER_COMBAT_BEHAVIOUR.Snatcher)) {
            //Temporary additional chance to knockout for snatcher combat behaviour
            chanceToKnockout += 20;
        }
        if (race == RACE.ANGEL) {
            chanceToKnockout = Mathf.RoundToInt(chanceToKnockout * 0.35f);
        }

        chanceToKnockout = ProcessKnockoutChanceThroughPiercingAndResistances(chanceToKnockout, characterThatAttacked, this, ref attackSummary);
        
        ELEMENTAL_TYPE elementalType = characterThatAttacked.combatComponent.elementalDamage.type;
        AdjustHP(-characterThatAttacked.combatComponent.GetAttackWithCritRateBonus(), elementalType, source: characterThatAttacked, showHPBar: true, piercingPower: characterThatAttacked.piercingAndResistancesComponent.piercingPower, isPlayerSource: isPlayerSource);
#if DEBUG_LOG
        attackSummary += $"\nDealt damage {stateComponent.owner.combatComponent.GetAttackWithCritRateBonus().ToString()}";
#endif

        //If the hostile reaches 0 hp, evaluate if he/she dies, get knock out, or get injured
        if (!HasHealth()) {
#if DEBUG_LOG
            attackSummary += $"\n{name}'s hp has reached 0.";
#endif
            if (!characterThatAttacked.combatComponent.IsLethalCombatForTarget(this) && !traitContainer.HasTrait("Sturdy")) {
                //If combat is non lethal and target has no sturdy trait, knockout
                //However, even if the combat is non lethal, if the target has sturdy trait, target dies
                traitContainer.AddTrait(this, "Unconscious", GetCharacterResponsibleForUnconsciousness(characterThatAttacked, combatStateOfAttacker));
            } else {
                if (!isDead) {
                    string deathReason = "attacked";
                    if (!characterThatAttacked.combatComponent.IsLethalCombatForTarget(this)) {
                        deathReason = "accidental_attacked";
                    }
                    Death(deathReason, responsibleCharacter: characterThatAttacked, isPlayerSource: isPlayerSource);
                }
            }
        } else {
#if DEBUG_LOG
            attackSummary += $"\nRolling for knockout..";
#endif
            //Each non lethal attack has a 15% chance of unconscious
            //https://trello.com/c/qxXVulZl/1126-each-non-lethal-attack-has-a-15-chance-of-making-target-unconscious
            if (GameUtilities.RollChance(chanceToKnockout, ref attackSummary)) {
                if (!characterThatAttacked.combatComponent.IsLethalCombatForTarget(this) && !traitContainer.HasTrait("Sturdy")) {
                    traitContainer.AddTrait(this, "Unconscious", GetCharacterResponsibleForUnconsciousness(characterThatAttacked, combatStateOfAttacker));
                }
            }
        }
        if (characterThatAttacked.marker) {
            for (int i = 0; i < characterThatAttacked.marker.inVisionCharacters.Count; i++) {
                Character inVision = characterThatAttacked.marker.inVisionCharacters[i];
                inVision.reactionComponent.ReactToCombat(combatStateOfAttacker, this);
                inVision.needsComponent.WakeUpFromNoise();
            }
        }
        if (characterThatAttacked.traitContainer.HasTrait("Plagued")) {
            CombatRateTransmission.Instance.Transmit(characterThatAttacked, this, PlagueDisease.Instance.GetTransmissionLevel(PLAGUE_TRANSMISSION.Combat));
        }
        Messenger.Broadcast(CharacterSignals.CHARACTER_WAS_HIT, this, characterThatAttacked);
    }
    private int GetChanceToBeKnockedOutBy(Character p_attacker, ref string p_attackSummary) {
        int finalChance = 0;
        float attackerPercentHP = (p_attacker.currentHP / (float) p_attacker.maxHP) * 100f;
        float defenderPercentHP = (currentHP / (float) maxHP) * 100f;

        p_attackSummary += $"\nAttacker Percent HP: {attackerPercentHP}%, Defender Percent HP: {defenderPercentHP}%";

        float attackerHPRatio = (100 - attackerPercentHP) * 0.2f;
        float defenderHPRatio = (100 - defenderPercentHP) * 0.25f;

        float rawChance = (defenderHPRatio - attackerHPRatio) + 1f;
        if (!characterClass.IsCombatant()) {
            rawChance += 10f;
        }

        if (rawChance > 0f) {
            finalChance = Mathf.RoundToInt(rawChance);
        }
        p_attackSummary += $"\nKnockout Chance: {finalChance}%";
        return finalChance;
    }
    private int ProcessKnockoutChanceThroughPiercingAndResistances(int p_currentKnockoutChance, Character p_attacker, Character p_target, ref string attackSummary) {
        attackSummary = $"{attackSummary}\nWill process knockout chance of combat between attacker:{p_attacker.name} and defender:{p_target.name} through piercing";
        //Reduce Knockout by 1 per 5 Mental and Physical Resistance of target
        float mentalResistance = p_target.piercingAndResistancesComponent.GetResistanceValue(RESISTANCE.Mental);
        float physicalResistance = p_target.piercingAndResistancesComponent.GetResistanceValue(RESISTANCE.Physical);
        float totalResistance = mentalResistance + physicalResistance;
        int chanceReduction = Mathf.RoundToInt(totalResistance / 5f);
        attackSummary = $"{attackSummary}\n\tTotal resistance of {p_target.name} is {totalResistance.ToString()}. Chance Reduction is {chanceReduction.ToString()}";
        
        //Inc Knockout by 1 per 4 Piercing
        float totalPiercing = p_attacker.piercingAndResistancesComponent.piercingPower;
        int chanceIncrease = Mathf.RoundToInt(totalPiercing / 4f);
        attackSummary = $"{attackSummary}\n\tTotal piercing of {p_attacker.name} is {totalPiercing.ToString()}. Chance Increase is {chanceIncrease.ToString()}";

        int newChance = p_currentKnockoutChance;
        newChance += chanceIncrease;
        newChance -= chanceReduction;
        attackSummary = $"{attackSummary}\n\tComputed chance is {newChance}";

        //Min 1, Max 50
        newChance = Mathf.Clamp(newChance, 1, 50);
        attackSummary = $"{attackSummary}\n\tFinal value after Min/Max is {newChance}";
        
        return newChance;
    }
    private Character GetCharacterResponsibleForUnconsciousness(Character characterThatAttacked, CombatState combatStateOfAttacker) {
        Character responsibleCharacter = null;
        if (combatStateOfAttacker != null) {
            if (combatStateOfAttacker.currentClosestHostile == this) {
                //attacker will only be responsible for unconscious trait, if he/she attacked this character on purpose.
                //this is so that the attacker can still try to take a Remove Status Unconscious job targeting this character
                responsibleCharacter = characterThatAttacked;
            }
        }
        return responsibleCharacter;
    }
    #endregion

    #region RPG
    public void ResetToFullHP() {
        SetHP(maxHP);
    }
    public void SetHP(int amount) {
        currentHP = amount;
    }
    //Adjust current HP based on specified parameter, but HP must not go below 0
    public virtual void AdjustHP(int amount, ELEMENTAL_TYPE elementalDamageType, bool triggerDeath = false,
        object source = null, CombatManager.ElementalTraitProcessor elementalTraitProcessor = null, bool showHPBar = false, float piercingPower = 0f, bool isPlayerSource = false) {
        bool ignoreIndestructibleTrait = source is BeingDrained;
        Character responsibleCharacter = source as Character;
        if (responsibleCharacter != null && responsibleCharacter.faction != null && responsibleCharacter.faction.isPlayerFaction) {
            //If responsible character is part of player faction, tag this as Player Source also
            isPlayerSource = true;
        }

        CombatManager.Instance.ModifyDamage(ref amount, elementalDamageType, piercingPower, this);

        //weapon ward and slayer bonus calculator
        float increaseDamage = EquipmentBonusProcessor.GetSlayerBonusDamage(responsibleCharacter, this, amount);
        float reducedDamage = EquipmentBonusProcessor.GetWardBonusDamage(responsibleCharacter, this, amount);
        int processedAmount = amount;
        processedAmount += (int) increaseDamage;
        processedAmount -= (int) reducedDamage;
#if DEBUG_LOG
        if (responsibleCharacter != null) {
            Debug.Log("Damage Receiver: " + this.name + " !Raw Damage: " + amount + " !Damage Reduction(Ward Bonus): " + reducedDamage + " !Damage Increase(Slayer Bonus): " + increaseDamage + " Attacker: " + responsibleCharacter + " !After Process Damage: " + processedAmount);
        }
#endif
        //weapon ward and slayer bonus calculator
        amount = processedAmount;
        if ((amount < 0 && (ignoreIndestructibleTrait || CanBeDamaged())) || amount > 0) {
            if (hasMarker) {
                if (responsibleCharacter == null || responsibleCharacter.combatComponent == null) {
                    marker.ShowHealthAdjustmentEffect(amount, null);
                } else {
                    marker.ShowHealthAdjustmentEffect(amount, responsibleCharacter.combatComponent);
                }
            }
            //only added checking here because even if objects cannot be damaged,
            //they should still be able to react to the elements
            int prevHP = currentHP;
            currentHP += amount;
            currentHP = Mathf.Clamp(currentHP, 0, maxHP);
            Messenger.Broadcast(CharacterSignals.CHARACTER_ADJUSTED_HP, this, amount, source);
            if (marker && showHPBar) {
                if (marker.hpBarGO.activeSelf) {
                    marker.UpdateHP(this);
                } else {
                    if (amount < 0 && HasHealth()) {
                        //only show hp bar if hp was reduced and hp is greater than 0
                        marker.QuickShowHPBar(this);
                    }
                }
            }
            if (source is Character character) {
                if (character.partyComponent.hasParty && character.partyComponent.currentParty.isPlayerParty && character.partyComponent.currentParty.isActive) {
                    if (character.partyComponent.currentParty.currentQuest.partyQuestType == PARTY_QUEST_TYPE.Demon_Raid) {
                        int damageDone = amount;
                        if (currentHP == 0) {
                            damageDone = prevHP;
                        }
                        character.partyComponent.currentParty.damageAccumulator.AccumulateDamage(damageDone, character);
                    }
                }
            }
            if (amount < 0 && isPlayerSource) {
                int accumulatedDamage = amount;
                if (currentHP == 0) {
                    accumulatedDamage = prevHP;
                }
                PlayerManager.Instance.player.damageAccumulator.AccumulateDamage(accumulatedDamage, gridTileLocation, this);
            }
        }

        if (amount < 0) {
            //hp was reduced
            jobComponent.OnHPReduced();
            CombatManager.Instance.ApplyElementalDamage(amount, elementalDamageType, this, responsibleCharacter, elementalTraitProcessor, setAsPlayerSource: isPlayerSource);
            if (responsibleCharacter is Character attackerCharacter) {
                if (attackerCharacter.race.IsSapient()) {
                    if (attackerCharacter.characterClass.attackType == ATTACK_TYPE.PHYSICAL) {
                        responsibleCharacter?.talentComponent.GetTalent(CHARACTER_TALENT.Martial_Arts).AdjustExperience(2, responsibleCharacter);
                    } else {
                        responsibleCharacter?.talentComponent.GetTalent(CHARACTER_TALENT.Combat_Magic).AdjustExperience(2, responsibleCharacter);
                    }
                }
            }
        } else {
            //hp was increased
            Messenger.Broadcast(JobSignals.CHECK_JOB_APPLICABILITY, JOB_TYPE.RECOVER_HP, this as IPointOfInterest);
        }
        if (!HasHealth()) {
            if (responsibleCharacter != null && responsibleCharacter.race.IsSapient()) {
                if (responsibleCharacter.characterClass.attackType == ATTACK_TYPE.PHYSICAL) {
                    responsibleCharacter?.talentComponent.GetTalent(CHARACTER_TALENT.Martial_Arts).AdjustExperience(8, responsibleCharacter);
                } else {
                    responsibleCharacter?.talentComponent.GetTalent(CHARACTER_TALENT.Combat_Magic).AdjustExperience(8, responsibleCharacter);
                }
            }
        }
        if (!HasHealth()) { //triggerDeath && 
            if (triggerDeath) {
                if (source != null && source != this) {
                    if (responsibleCharacter != null) {
                        Death("attacked", responsibleCharacter: responsibleCharacter, isPlayerSource: isPlayerSource);
                        
                    } else {
                        string cause = "attacked";
                        if (source is SkillData skillData) {
                            cause += $"_spell";
                            Death(cause, isPlayerSource: isPlayerSource, deathLogFillers: new[] {
                                new LogFillerStruct(null, skillData.name, LOG_IDENTIFIER.STRING_1)
                            });
                        } else if (source is TileObject tileObject) {
                            cause += $"_spell";
                            Death(cause, isPlayerSource: isPlayerSource, deathLogFillers: new[] {
                                new LogFillerStruct(null, tileObject.name, LOG_IDENTIFIER.STRING_1)
                            });
                        } else {
                            cause += $"_{source}";
                            Death(cause, isPlayerSource: isPlayerSource);
                        }
                    }
                } else {
                    Death(isPlayerSource: isPlayerSource);
                }
            }
        } else if (amount < 0) {
            if (IsHealthCriticallyLow()) {
                Messenger.Broadcast(CharacterSignals.HEALTH_CRITICALLY_LOW, this);
                if (traitContainer.HasTrait("Coward", "Vampire") && traitContainer.HasTrait("Berserked") == false && !characterClass.IsZombie()) {  //do not make berserked characters trigger flight
                    bool willflight = true;
                    if (traitContainer.HasTrait("Vampire") && crimeComponent.HasNonHostileVillagerInRangeThatConsidersCrimeTypeACrime(CRIME_TYPE.Vampire)) {
                        willflight = false;
                    }
                    if (willflight) {
                        combatComponent.FlightAll("critically low health");
                    }
                }
            }
        }
    }
    public void HPRecovery(float maxHPPercentage) {
        if (doNotRecoverHP <= 0 && !IsHealthFull() && HasHealth()) {
            AdjustHP(Mathf.CeilToInt(maxHPPercentage * maxHP), ELEMENTAL_TYPE.Normal);
        }
    }
    public void HPRecovery(int p_amount) {
        if (doNotRecoverHP <= 0 && !IsHealthFull() && HasHealth()) {
            AdjustHP(p_amount, ELEMENTAL_TYPE.Normal);
        }
    }
    public bool IsHealthFull() {
        return currentHP >= maxHP;
    }
    public bool HasHealth() {
        return currentHP > 0;
    }
    public bool IsHealthCriticallyLow() {
        //chance based dependent on the character
        return currentHP < (maxHP * 0.2f);
    }
#endregion

#region Home
    /// <summary>
    /// Set this character's home npcSettlement data.(Does nothing else)
    /// </summary>
    /// <param name="newHome">The character's new home</param>
    public void SetHomeRegion(Region newHome) {
        Region previousHome = homeRegion;
        homeRegion = newHome;

        //If a character sets his home, add his faction to the factions in the region
        //Subsequently, if character loses his home, remove his faction from the region only if there are no co faction resident in the region anymore
        if (faction != null) {
            if (newHome == null) {
                //If character loses home and he has previous home, remove him faction
                if (previousHome != null) {
                    bool removeFaction = true;
                    for (int i = 0; i < previousHome.residents.Count; i++) {
                        Character resident = previousHome.residents[i];
                        if (resident != this && resident.faction == faction) {
                            removeFaction = false;
                            break;
                        }
                    }
                    if (removeFaction) {
                        previousHome.RemoveFactionHere(faction);
                    }
                }
            } else {
                newHome.AddFactionHere(faction);
            }
        }
    }
    public void SetHomeStructure(LocationStructure p_homeStructure) {
        if (homeStructure != null) {
            previousCharacterDataComponent.SetPreviousHomeStructure(homeStructure);
        }
        homeStructure = p_homeStructure;
        if (p_homeStructure != null) {
            if (tileObjectComponent.primaryBed != null) {
                if (tileObjectComponent.primaryBed.gridTileLocation == null || tileObjectComponent.primaryBed.gridTileLocation.structure != p_homeStructure) {
                    tileObjectComponent.SetPrimaryBed(p_homeStructure.GetRandomTileObjectOfTypeThatHasTileLocationAndIsBuilt<Bed>());
                }
            } else {
                tileObjectComponent.SetPrimaryBed(p_homeStructure.GetRandomTileObjectOfTypeThatHasTileLocationAndIsBuilt<Bed>());
            }
        }
        eventDispatcher.ExecuteCharacterSetHomeStructure(this, p_homeStructure);
    }
    public bool MigrateHomeTo(BaseSettlement newHomeSettlement, LocationStructure homeStructure = null, bool broadcast = true, bool addToRegionResidents = true) {
        BaseSettlement previousHome = null;
        bool sameRegionLocationAlready = false;
        if (homeSettlement != null) {
            previousHome = homeSettlement;
            if (previousHome != newHomeSettlement) {
                if (previousHome.RemoveResident(this)) {
                    //If previous home and new home is just the same, do not remove as settlement ruler, but if it is, then remove the character as settlement ruler
                    if (previousHome is NPCSettlement previousHomeSettlement && previousHomeSettlement.ruler == this) {
                        previousHomeSettlement.SetRuler(null);
                    }
                }
            } else {
                //If migrating to the same settlement, do not process anymore, just process the home structure migration not the settlement because the settlement will not change
                if (this.homeStructure != homeStructure) {
                    newHomeSettlement.AssignCharacterToDwellingInArea(this, homeStructure);
                }
                return true;
            }
        }
        if (homeRegion != null) {
            if (newHomeSettlement != null && newHomeSettlement is NPCSettlement newNPCSettlement && homeRegion == newNPCSettlement.region) {
                sameRegionLocationAlready = true;
            } else {
                //Only remove from previous home region if character has a new home settlement, if it doesn't, for example, if the character only left the settlement but did not have a new home settlement assigned, he should not be removed from his home region because he only left the settlement
                if (newHomeSettlement != null) {
                    homeRegion.RemoveResident(this);
                }
            }
        }
        if (newHomeSettlement != null && newHomeSettlement.AddResident(this, homeStructure)) {
            if (addToRegionResidents && newHomeSettlement is NPCSettlement newNPCSettlement) {
                if (!sameRegionLocationAlready) {
                    newNPCSettlement.region.AddResident(this);
                }
            }
            if (broadcast) {
                Messenger.Broadcast(CharacterSignals.CHARACTER_MIGRATED_HOME, this, previousHome, newHomeSettlement);
            }
            return true;
        }
        return false;
    }
    public void ClearTerritoryAndMigrateHomeStructureTo(LocationStructure dwelling, bool broadcast = true, bool addToRegionResidents = true, bool affectSettlement = true) {
        MigrateHomeStructureTo(dwelling, broadcast, addToRegionResidents, affectSettlement);
        ClearTerritory();
    }
    public void ClearTerritoryAndMigrateHomeSettlementTo(BaseSettlement newHomeSettlement, LocationStructure homeStructure = null, bool broadcast = true, bool addToRegionResidents = true) {
        MigrateHomeTo(newHomeSettlement, homeStructure, broadcast, addToRegionResidents);
        ClearTerritory();
    }
    public void MigrateHomeStructureTo(LocationStructure dwelling, bool broadcast = true, bool addToRegionResidents = true, bool affectSettlement = true) {
        if (dwelling == null) {
            if (affectSettlement) {
                if (homeSettlement != null) {
                    MigrateHomeTo(null);
                } else {
                    ChangeHomeStructure(dwelling);
                }
            } else {
                ChangeHomeStructure(dwelling);
            }
        } else {
            if (dwelling.settlementLocation != null) {
                if (affectSettlement) {
                    MigrateHomeTo(dwelling.settlementLocation, dwelling, broadcast, addToRegionResidents);
                } else {
                    ChangeHomeStructure(dwelling);
                }
            } else {
                bool sameLocationAlready = false;
                if (homeStructure != null && homeStructure.region != null) {
                    if (homeStructure.region == dwelling.region) {
                        sameLocationAlready = true;
                    } else {
                        homeStructure.region.RemoveResident(this);
                    }
                }
                if (!sameLocationAlready) {
                    dwelling.region.AddResident(this);
                }
                ChangeHomeStructure(dwelling);
            }
        }
    }
    public bool ChangeHomeStructure(LocationStructure dwelling) {
        if (homeStructure != null) {
            if (homeStructure == dwelling) {
                return true; //ignore change
            }
            //remove character from his/her old home
            homeStructure.RemoveResident(this);
        }
        //Added checking, because character can sometimes change home from dwelling to nothing.
        if (dwelling != null && dwelling.AddResident(this) && GameManager.Instance.gameHasStarted) {
            if (isNormalCharacter) {
                jobComponent.PlanReturnHome(JOB_TYPE.RETURN_HOME_URGENT);
            }
            return true;
        }
        return false;
    }
    public void SetHomeSettlement(NPCSettlement settlement) {
        if (homeSettlement != settlement) {
            if (settlement == null) {
                if (partyComponent.hasParty && !isDead) {
                    interruptComponent.TriggerInterrupt(INTERRUPT.Leave_Party, this, "Left home settlement");
                }
            }
            if (homeSettlement != null) {
                //make sure that previous data only stores non null values, this is so that when a character leaves a settlement,
                //then becomes homeless, the stored value is still the first actual settlement that it left.
                previousCharacterDataComponent.SetPreviousHomeSettlement(homeSettlement);
            }

            homeSettlement = settlement;
#if DEBUG_LOG
            logComponent.PrintLogIfActive($"Set home settlement of {name} to {homeSettlement?.name}");
#endif
            if (isNormalCharacter) {
                behaviourComponent.UpdateDefaultBehaviourSet();
            }
            if (homeSettlement != null && gridTileLocation != null && areaLocation?.settlementOnArea == homeSettlement) {
                stateAwarenessComponent.StopMissingTimer();
            } else if (homeSettlement == null) {
                stateAwarenessComponent.StartMissingTimer();
            } else if (homeSettlement != null && gridTileLocation != null && areaLocation?.settlementOnArea != homeSettlement) {
                stateAwarenessComponent.StartMissingTimer();
            }
            if (faction != null) {
                faction.ProcessFactionLeaderAsSettlementRuler();
            }
        }
    }
    private void OnStructureDestroyed(LocationStructure structure) {
        //character's home was destroyed.
        if (structure == homeStructure) {
            //affect settlement if home settlement structures have been reduced to 0
            bool affectSettlement = homeSettlement != null && homeSettlement.structures.Count == 0;
            MigrateHomeStructureTo(null, affectSettlement: affectSettlement);
            //removed this because we expect that villagers should purchase their dwellings. See FreeTimeBehaviour
            // interruptComponent.TriggerInterrupt(INTERRUPT.Set_Home, null); 
        }
    }
    /// <summary>
    /// Compare the homes of this character and another character.
    /// This takes into account the homeSetttlement, then the homeStructure, then finally the territories.
    /// </summary>
    /// <param name="otherCharacter">The other character</param>
    /// <returns>If the two characters share a home/home settlement/territory</returns>
    public bool HasSameHomeAs(Character otherCharacter) {
        if (homeSettlement != null) { //check home settlement first
            return otherCharacter.homeSettlement == homeSettlement;
        }
        if (homeStructure != null) { //if no home settlement, check home structure
            return otherCharacter.homeStructure == homeStructure;
        }
        if (territory != null && otherCharacter.territory != null) { //if no home settlement and home structure check territtories
            return territory == otherCharacter.territory;
        }
        return false;
    }
#endregion

#region Traits
    public ITraitContainer traitContainer { get; private set; }
    public TraitProcessor traitProcessor => TraitManager.characterTraitProcessor;
    public void CreateTraitContainer() {
        traitContainer = new TraitContainer();
    }
    public void CreateDefaultTraits() {
        traitContainer.AddTrait(this, "Character Trait");
        traitContainer.AddTrait(this, "Flammable");
        defaultCharacterTrait = traitContainer.GetTraitOrStatus<CharacterTrait>("Character Trait");
    }
    public void CreateRandomInitialTraits(List<string> buffPool = null, List<string> neutralPool = null, List<string> flawPool = null) {
        if (minion == null && race != RACE.DEMON && !(this is Summon) && race != RACE.RATMAN) { //only generate buffs and flaws for non minion characters. Reference: https://trello.com/c/pC9hBih0/2781-demonic-minions-should-not-have-pregenerated-buff-and-flaw-traits

            List<string> buffTraits;
            List<string> neutralTraits;
            List<string> flawTraits;
            bool hasBorrowedBuffPool = false;
            bool hasBorrowedNeutralPool = false;
            bool hasBorrowedFlawPool = false;

            if (buffPool != null) {
                buffTraits = buffPool;
            } else {
                buffTraits = RuinarchListPool<string>.Claim();
                buffTraits.AddRange(TraitManager.Instance.buffTraitPool);
                hasBorrowedBuffPool = true;
            }
            if (neutralPool != null) {
                neutralTraits = neutralPool;
            } else {
                neutralTraits = RuinarchListPool<string>.Claim();
                neutralTraits.AddRange(TraitManager.Instance.neutralTraitPool);
                hasBorrowedNeutralPool = true;
            }
            if (flawPool != null) {
                flawTraits = flawPool;
            } else {
                flawTraits = RuinarchListPool<string>.Claim();
                flawTraits.AddRange(TraitManager.Instance.flawTraitPool);
                hasBorrowedFlawPool = true;
            }

            //remove character's existing traits from pool, as well as any mutual exclusive traits because of said existing trait
            for (int i = 0; i < traitContainer.traits.Count; i++) {
                Trait trait = traitContainer.traits[i];
                buffTraits.Remove(trait.name);
                neutralTraits.Remove(trait.name);
                flawTraits.Remove(trait.name);
                if (trait.mutuallyExclusive != null) {
                    CollectionUtilities.RemoveElements(buffTraits, trait.mutuallyExclusive); //update buff traits pool to accomodate new trait
                    CollectionUtilities.RemoveElements(neutralTraits, trait.mutuallyExclusive); //update neutral traits pool to accomodate new trait
                    CollectionUtilities.RemoveElements(flawTraits, trait.mutuallyExclusive); //update flaw traits pool to accomodate new trait
                }
            }


            //Up to three traits
            //100% Trait 1: Buff List
            string chosenBuffTraitName;
            if (buffTraits.Count > 0) {
                chosenBuffTraitName = CollectionUtilities.GetRandomElement(buffTraits);
                buffTraits.Remove(chosenBuffTraitName);
            } else {
                throw new Exception("There are no buff traits!");
            }
            traitContainer.AddTrait(this, chosenBuffTraitName);
            Trait buffTrait = traitContainer.GetTraitOrStatus<Trait>(chosenBuffTraitName);
            if (buffTrait.mutuallyExclusive != null) {
                CollectionUtilities.RemoveElements(buffTraits, buffTrait.mutuallyExclusive); //update buff traits pool to accomodate new trait
                CollectionUtilities.RemoveElements(neutralTraits, buffTrait.mutuallyExclusive); //update neutral traits pool to accomodate new trait
                CollectionUtilities.RemoveElements(flawTraits, buffTrait.mutuallyExclusive); //update flaw traits pool to accomodate new trait
            }

            List<string> choices = RuinarchListPool<string>.Claim();
            //80% Trait 2: Buff + Neutral List
            if (GameUtilities.RollChance(80)) {
                choices.AddRange(buffTraits);
                choices.AddRange(neutralTraits);
                string chosenBuffOrNeutralTraitName;

                if (choices.Count > 0) {
                    chosenBuffOrNeutralTraitName = CollectionUtilities.GetRandomElement(choices);
                    buffTraits.Remove(chosenBuffOrNeutralTraitName);
                    neutralTraits.Remove(chosenBuffOrNeutralTraitName);
                } else {
                    throw new Exception("No more buff or neutral traits!");
                }

                traitContainer.AddTrait(this, chosenBuffOrNeutralTraitName);
                Trait buffOrNeutralTrait = traitContainer.GetTraitOrStatus<Trait>(chosenBuffOrNeutralTraitName);
                if (buffOrNeutralTrait.mutuallyExclusive != null) {
                    CollectionUtilities.RemoveElements(buffTraits, buffOrNeutralTrait.mutuallyExclusive); //update buff traits pool to accomodate new trait
                    CollectionUtilities.RemoveElements(neutralTraits, buffOrNeutralTrait.mutuallyExclusive); //update neutral traits pool to accomodate new trait
                    CollectionUtilities.RemoveElements(flawTraits, buffOrNeutralTrait.mutuallyExclusive); //update flaw traits pool to accomodate new trait
                }
            }


            //40% Trait 3: Buff + Neutral + Flaw List
            if (GameUtilities.RollChance(40)) {
                choices.Clear();
                choices.AddRange(buffTraits);
                choices.AddRange(neutralTraits);
                choices.AddRange(flawTraits);
                string chosenTrait;
                if (choices.Count > 0) {
                    chosenTrait = CollectionUtilities.GetRandomElement(choices);
                } else {
                    throw new Exception("No more buff, neutral or flaw traits!");
                }
                traitContainer.AddTrait(this, chosenTrait);
            }
            RuinarchListPool<string>.Release(choices);
            if (hasBorrowedBuffPool) {
                RuinarchListPool<string>.Release(buffTraits);
            }
            if (hasBorrowedNeutralPool) {
                RuinarchListPool<string>.Release(neutralTraits);
            }
            if (hasBorrowedFlawPool) {
                RuinarchListPool<string>.Release(flawTraits);
            }
        }
    }
    public void AddTraitNeededToBeRemoved(Trait trait) {
        traitsNeededToBeRemoved.Add(trait);
    }
    public void RemoveTraitNeededToBeRemoved(Trait trait) {
        traitsNeededToBeRemoved.Remove(trait);
    }
    public void ProcessTraitsOnTickStarted() {
        if (!interruptComponent.isInterrupted) {
            traitContainer.ProcessOnTickStarted(this);
        }
    }
    public void ProcessTraitsOnTickEnded() {
        if (!interruptComponent.isInterrupted) {
            traitContainer.ProcessOnTickEnded(this);
        }
    }
    public void ProcessTraitsOnHourStarted() {
        if (!interruptComponent.isInterrupted) {
            traitContainer.ProcessOnHourStarted(this);
        }
    }
    public void TryProcessTraitsOnTickEndedWhileStationaryOrUnoccupied() {
        if (!interruptComponent.isInterrupted && (currentActionNode == null || currentActionNode.actionStatus != ACTION_STATUS.PERFORMING) &&
            !traitContainer.HasTrait("Unconscious") && !traitContainer.HasTrait("Resting") && !traitContainer.HasTrait("Frozen")) {
            List<Trait> traitOverrideFunctions = traitContainer.GetTraitOverrideFunctions(TraitManager.Per_Tick_While_Stationary_Unoccupied);
            if (traitOverrideFunctions != null) {
                for (int i = 0; i < traitOverrideFunctions.Count; i++) {
                    Trait trait = traitOverrideFunctions[i];
                    if (trait.PerTickWhileStationaryOrUnoccupied(this)) {
                        break;
                    }
                }
            }
        }
    }
#endregion

#region Minion
    public void SetMinion(Minion minion) {
        if (_minion != null && minion == null) {
            Messenger.Broadcast(CharacterSignals.CHARACTER_BECOMES_NON_MINION_OR_SUMMON, this);
            moodComponent.OnCharacterNoLongerMinionOrSummon();
            Assert.IsTrue(moodComponent.executeMoodChangeEffects);
        } else if (_minion == null && minion != null) {
            Messenger.Broadcast(CharacterSignals.CHARACTER_BECOMES_MINION_OR_SUMMON, this);
            moodComponent.OnCharacterBecomeMinionOrSummon();
            Assert.IsFalse(moodComponent.executeMoodChangeEffects);
        }
        _minion = minion;
        visuals.CreateWholeImageMaterial(visuals.portraitSettings);
    }
#endregion

#region Interaction
    //public void AddInteractionType(INTERACTION_TYPE type) {
    //    if (!currentInteractionTypes.Contains(type)) {
    //        currentInteractionTypes.Add(type);
    //    }
    //}
    //public void RemoveInteractionType(INTERACTION_TYPE type) {
    //    currentInteractionTypes.Remove(type);
    //}
#endregion

#region Action Planning and Job Processing
    private void DailyGoapProcesses() {
        needsComponent.DailyGoapProcesses();
    }
    public virtual void OnTickStartedWhileSeizedOrIsInLimbo() {
#if DEBUG_PROFILER
        Profiler.BeginSample($"{name} OnTickStartedWhileSeized");
#endif
        stateAwarenessComponent.PerTick();
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif
    }
    protected virtual void OnTickStarted() {
#if DEBUG_PROFILER
        Profiler.BeginSample($"{name} OnTickStarted");
#endif
        //What happens every start of tick

        //Check Trap Structure
#if DEBUG_PROFILER
        Profiler.BeginSample($"{name} OnTickStarted - Increment Trap Structure");
#endif
        trapStructure.IncrementCurrentDuration(1);
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif

        //Out of combat hp recovery
        //if (!isDead && !isInCombat) {
        //    HPRecovery(0.0025f);
        //}
#if DEBUG_PROFILER
        Profiler.BeginSample($"{name} OnTickStarted - State Component Awareness");
#endif
        stateAwarenessComponent.PerTick();
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif

#if DEBUG_PROFILER
        Profiler.BeginSample($"{name} OnTickStarted - Needs Component");
#endif
        needsComponent.PerTick();
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif

#if DEBUG_PROFILER
        Profiler.BeginSample($"{name} OnTickStarted - Process Traits");
#endif
        ProcessTraitsOnTickStarted();
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif

#if DEBUG_PROFILER
        Profiler.BeginSample($"{name} OnTickStarted - GoapPlanGeneration");
#endif
        StartTickGoapPlanGeneration();
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif

#if DEBUG_PROFILER
        Profiler.EndSample();
#endif
    }
    protected virtual void OnTickEnded() {
#if DEBUG_PROFILER
        Profiler.BeginSample($"{name} ProcessForcedCancelJobsOnTickEnded");
#endif
        ProcessForcedCancelJobsOnTickEnded();
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif

#if DEBUG_PROFILER
        Profiler.BeginSample($"{name} Mood Component - On Tick Ended");
#endif
        moodComponent.OnTickEnded();
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif

#if DEBUG_PROFILER
        Profiler.BeginSample($"{name} InterruptComponent - On Tick Ended");
#endif
        interruptComponent.OnTickEnded();
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif
        stateComponent.OnTickEnded();
#if DEBUG_PROFILER
        Profiler.BeginSample($"{name} Process Traits On Tick Ended");
#endif
        ProcessTraitsOnTickEnded();
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif

#if DEBUG_PROFILER
        Profiler.BeginSample($"{name} TryProcessTraitsOnTickEndedWhileStationaryOrUnoccupied");
#endif
        TryProcessTraitsOnTickEndedWhileStationaryOrUnoccupied();
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif

#if DEBUG_PROFILER
        Profiler.BeginSample($"{name} EndTickPerformJobs");
#endif
        EndTickPerformJobs();
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif
    }
    protected virtual void OnHourStarted() {
#if DEBUG_PROFILER
        Profiler.BeginSample($"{name} On Hour Started");
#endif
        ProcessTraitsOnHourStarted();
        dailyScheduleComponent.OnHourStarted(this);
        // if (needsComponent.HasNeeds()) {
        // needsComponent.PlanScheduledFullnessRecovery();
        // needsComponent.PlanScheduledTirednessRecovery();
        // needsComponent.PlanScheduledSecondHappinessRecovery();
        // }
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif
    }
    protected void StartTickGoapPlanGeneration() {
        //This is to ensure that this character will not be idle forever
        //If at the start of the tick, the character is not currently doing any action, and is not waiting for any new plans, it means that the character will no longer perform any actions
        //so start doing actions again
        //SetHasAlreadyAskedForPlan(false);
        //if (needsComponent.HasNeeds()) {
        //    needsComponent.PlanScheduledFullnessRecovery();
        //    //needsComponent.PlanScheduledTirednessRecovery(this);
        //    needsComponent.PlanScheduledSecondHappinessRecovery();
        //}
        if (isNormalCharacter) {
            //try to take settlement job that this character can see the target of.
            if (CanTryToTakeSettlementJobInVision(out var invalidReason)) {
#if DEBUG_LOG
                string debugLog = $"{GameManager.Instance.TodayLogString()}{name} will try to take settlement job in vision";
                debugLog = $"{debugLog}\n{name} Can take settlement job in vision.";
#endif
                JobQueueItem jobToAssign = homeSettlement?.GetFirstJobBasedOnVisionExcept(this, JOB_TYPE.CRAFT_OBJECT);
#if DEBUG_LOG
                debugLog = $"{debugLog}\nJob to assign is:{jobToAssign?.ToString() ?? "None"}";
#endif
                if (jobToAssign != null && ((jobQueue.jobsInQueue.Count <= 0 && behaviourComponent.GetHighestBehaviourPriority() < jobToAssign.priority) ||
                    (jobQueue.jobsInQueue.Count > 0 && jobToAssign.priority > jobQueue.jobsInQueue[0].priority))) {
                    jobQueue.AddJobInQueue(jobToAssign);
#if DEBUG_LOG
                    debugLog = $"{debugLog}\nJob was added to queue!";
                    logComponent.PrintLogIfActive(debugLog);
#endif
                }
                // else {
                //     debugLog = $"{debugLog}\nCouldn't assign job!";
                // }    
            }
            // else {
            //     debugLog = $"{debugLog}\n{name} Cannot take settlement job in vision because \n{invalidReason}";
            // }
        }
        if (CanPlanGoap()) {
            PerStartTickActionPlanning();
        } else {
            if (currentActionNode != null) {
                if (currentActionNode.hasStartedPerTickEffect) {
                    currentActionNode.PerTickEffect();
                }
            }
        }
        //Always set this to false here even if the character cannot do goap planning
        //The reason is that there is a bug in Wurm that after combat, if the Wurm is restrained, it will not go through the behaviour, hence, the value of this is still true
        //Now, once the Wurm is released, it will immediately teleport and leave a wurm hole which is weird since the Wurm does not satisfy the "just exited combat" rule already since it's been restrained for quite a while now
        behaviourComponent.SetSubterraneanJustExitedCombat(false);
    }
    public bool CanPlanGoap() {
        //If there is no npcSettlement, it means that there is no inner map, so character must not do goap actions, jobs, and plans
        //characters that cannot witness, cannot plan actions.
        //minion == null &&
        return !isDead && numOfActionsBeingPerformedOnThis <= 0 && limiterComponent.canPerform
            && currentActionNode == null && planner.status == GOAP_PLANNING_STATUS.NONE
            && (jobQueue.jobsInQueue.Count <= 0 || behaviourComponent.GetHighestBehaviourPriority() > jobQueue.jobsInQueue[0].priority)
            && (carryComponent.masterCharacter.movementComponent.isTravellingInWorld == false)
            && (marker && !marker.hasFleePath) && stateComponent.currentState == null && carryComponent.IsNotBeingCarried() && !interruptComponent.isInterrupted
            && !partyComponent.isFollowingBeacon;
    }
    private bool CanTryToTakeSettlementJobInVision(out string invalidReason) {
        if (isDead) {
            invalidReason = "Character is dead.";
            return false;
        }
        if (numOfActionsBeingPerformedOnThis > 0) {
            invalidReason = $"Actions being performed on this is {numOfActionsBeingPerformedOnThis.ToString()}.";
            return false;
        }
        if (!limiterComponent.canPerform) {
            invalidReason = "Character cannot perform";
            return false;
        }
        if (currentActionNode != null) {
            invalidReason = "Character has current action";
            return false;
        }
        if (currentJob != null) {
            invalidReason = "Character is in the middle of a job";
            return false;
        }
        // if (planner.status != GOAP_PLANNING_STATUS.NONE) {
        //     invalidReason = "Character is planning";
        //     return false;
        // }
        if (marker == null || marker.hasFleePath) {
            invalidReason = "Character has no marker or is fleeing";
            return false;
        }
        if (stateComponent.currentState != null) {
            invalidReason = "Character is in a state";
            return false;
        }
        if (carryComponent.IsNotBeingCarried() == false) {
            invalidReason = "Character is being carried";
            return false;
        }
        if (interruptComponent.isInterrupted) {
            invalidReason = "Character is interrupted";
            return false;
        }
        if (partyComponent.isActiveMember) {
            invalidReason = "Character is in an active party";
            return false;
        }
        //NOTE: ONLY ADDED FACTION CHECKING BECAUSE OF BUG THAT VAGRANTS ARE STILL PART OF A VILLAGE
        if (homeSettlement != null && homeSettlement.owner != faction) {
            invalidReason = "Character is not part of settlement faction!";
            return false;
        }
        invalidReason = "No reason";
        return true;
    }
    public void EndTickPerformJobs() {
        if (shouldDoActionOnFirstTickUponLoadGame) {
            shouldDoActionOnFirstTickUponLoadGame = false;
            if (currentActionNode != null) {
                currentActionNode.DoActionUponLoadingSavedGame();
            }
        } else {
#if DEBUG_PROFILER
            Profiler.BeginSample($"{name} End Tick Perform Jobs");
#endif
            if (CanPerformEndTickJobs() && HasSameOrHigherPriorityJobThanBehaviour()) {
                JobQueueItem job = null;
                if (jobQueue.jobsInQueue.Count > 0) {
                    job = jobQueue.jobsInQueue[0];
                }
                if (job != null) {
#if DEBUG_PROFILER
                    Profiler.BeginSample($"{name} - Process Job");
#endif
                    bool processJob = job.ProcessJob();
#if DEBUG_PROFILER
                    Profiler.EndSample();
#endif
                    if (!processJob) {
#if DEBUG_PROFILER
                        Profiler.BeginSample($"{name} - Perform Job");
#endif
                        PerformJob(job);
#if DEBUG_PROFILER
                        Profiler.EndSample();
#endif
                    }
                }
            }
#if DEBUG_PROFILER
            Profiler.EndSample();
#endif
        }
    }
    public bool CanPerformEndTickJobs() {
        bool canPerformEndTickJobs = !isDead && numOfActionsBeingPerformedOnThis <= 0 /*&& limiterComponent.canWitness*/
         && currentActionNode == null && planner.status == GOAP_PLANNING_STATUS.NONE && jobQueue.jobsInQueue.Count > 0
         && carryComponent.masterCharacter.movementComponent.isTravellingInWorld == false && (marker && !marker.hasFleePath)
         && stateComponent.currentState == null && carryComponent.IsNotBeingCarried() && !interruptComponent.isInterrupted
         && !partyComponent.isFollowingBeacon; //minion == null && doNotDisturb <= 0 
        return canPerformEndTickJobs;
    }
    /// <summary>
    /// Does this character have a job that is same or higher priority than it's highest priority behaviour?
    /// </summary>
    /// <returns>True or false</returns>
    public bool HasSameOrHigherPriorityJobThanBehaviour() {
        return jobQueue.jobsInQueue[0].priority >= behaviourComponent.GetHighestBehaviourPriority();
    }
    //public void PlanGoapActions() {
    //    if (!IsInOwnParty() || ownParty.icon.isTravelling || _doNotDisturb > 0 /*|| isWaitingForInteraction > 0 */ || isDead || marker.hasFleePath) {
    //        return; //if this character is not in own party, is a defender or is travelling or cannot be disturbed, do not generate interaction
    //    }
    //    if (stateComponent.currentState != null) {
    //        //Debug.LogWarning(name + " is currently in " + stateComponent.currentState.stateName + " state, can't plan actions!");
    //        return;
    //    }
    //    //if (stateComponent.stateToDo != null) {
    //    //    Debug.LogWarning("Will about to do " + stateComponent.stateToDo.stateName + " state, can't plan actions!");
    //    //    return;
    //    //}
    //    //if(specificAction != null) {
    //    //    WillAboutToDoAction(specificAction);
    //    //    return;
    //    //}
    //    if (allGoapPlans.Count > 0) {
    //        //StopDailyGoapPlanGeneration();
    //        PerformGoapPlans();
    //        //SchedulePerformGoapPlans();
    //    } else {
    //        PlanPerTickGoap();
    //    }
    //}
    public void PerStartTickActionPlanning() {
        //if (_hasAlreadyAskedForPlan || isDead) {
        //    return;
        //}
        //SetHasAlreadyAskedForPlan(true);
        //if(carryComponent.masterCharacter.avatar && carryComponent.masterCharacter.avatar.isTravellingOutside) {
        //    return;
        //}
        if (interruptComponent.NecromanticTransform()) {
            return;
        }
        string idleLog = OtherIdlePlans();
        logComponent.PrintLogIfActive(idleLog);

        //perform created jobs if any.
        //EndTickPerformJobs();

        //if (!PlanJobQueueFirst()) {
        //    if (!PlanFullnessRecoveryActions()) {
        //        if (!PlanTirednessRecoveryActions()) {
        //            if (!PlanHappinessRecoveryActions()) {
        //                if (!PlanWorkActions()) {
        //                    string idleLog = OtherIdlePlans();
        //                    PrintLogIfActive(idleLog);
        //                }
        //            }
        //        }
        //    }
        //}
    }
    //private bool OtherPlanCreations() {
    //    int chance = UnityEngine.Random.Range(0, 100);
    //    if (traitContainer.GetNormalTrait<Trait>("Berserked") != null) {
    //        if (chance < 15) {
    //            Character target = specificLocation.GetRandomCharacterAtLocationExcept(this);
    //            if (target != null) {
    //                StartGOAP(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_NON_POSITIVE_TRAIT, conditionKey = "Disabler", targetPOI = target }, target, GOAP_CATEGORY.NONE);
    //                return true;
    //            }
    //        } else {
    //            chance = UnityEngine.Random.Range(0, 100);
    //            if (chance < 15) {
    //                IPointOfInterest target = specificLocation.GetRandomTileObject();
    //                if (target != null) {
    //                    StartGOAP(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.DESTROY, conditionKey = target, targetPOI = target }, target, GOAP_CATEGORY.NONE);
    //                    return true;
    //                }
    //            }
    //        }
    //    }
    //    return false;
    //}
    private string OtherIdlePlans() {
        string log = string.Empty;
#if DEBUG_LOG
        log = $" IDLE PLAN FOR {name}";
#endif
        if (isDead) {
#if DEBUG_LOG
            log = $"{log}{name} is already dead not planning other idle plans.";
#endif
            return log;
        }
        //if (!isFactionless) { }
        string classIdlePlanLog = behaviourComponent.RunBehaviour();
#if DEBUG_LOG
        log = $"{log}\n{classIdlePlanLog}";
#endif
        return log;

        //if (homeStructure != null) {
        //    if(currentStructure.structureType == STRUCTURE_TYPE.DWELLING) {
        //        if(currentStructure != homeStructure) {
        //            PlanIdleReturnHome();
        //        } else {
        //            PlanIdleStroll(currentStructure);
        //        }
        //    } else {
        //        int chance = UnityEngine.Random.Range(0, 100);
        //        int returnHomeChance = 0;
        //        if (specificLocation == homeNpcSettlement && currentStructure.structureType == STRUCTURE_TYPE.WORK_AREA) {
        //            returnHomeChance = 25;
        //        } else {
        //            returnHomeChance = 80;
        //        }
        //        if (chance < returnHomeChance) {
        //            PlanIdleReturnHome();
        //        } else {
        //            PlanIdleStroll(currentStructure);
        //        }
        //    }
        //} else {
        //    PlanIdleStroll(currentStructure);
        //}
    }
    public void PlanIdle(JOB_TYPE jobType, INTERACTION_TYPE type, IPointOfInterest target, OtherData[] otherData = null) {
        ActualGoapNode node = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[type], this, target, otherData, 0);
        GoapPlan goapPlan = ObjectPoolManager.Instance.CreateNewGoapPlan(node, target);
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, type, target, this);
        goapPlan.SetDoNotRecalculate(true);
        job.SetCannotBePushedBack(true);
        job.SetAssignedPlan(goapPlan);
        jobQueue.AddJobInQueue(job);

        //GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(type, this, target);
        //GoapNode goalNode = new GoapNode(null, goapAction.cost, goapAction);
        //GoapPlan goapPlan = new GoapPlan(goalNode, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.NONE }, GOAP_CATEGORY.IDLE);
        //goapPlan.ConstructAllNodes();
        //AddPlan(goapPlan);
        //PlanGoapActions(goapAction);
    }
    public void PlanFixedJob(JOB_TYPE jobType, INTERACTION_TYPE type, IPointOfInterest target, out JobQueueItem producedJob, OtherData[] otherData = null) {
        ActualGoapNode node = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[type], this, target, otherData, 0);
        GoapPlan goapPlan = ObjectPoolManager.Instance.CreateNewGoapPlan(node, target);
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, type, target, this);
        goapPlan.SetDoNotRecalculate(true);
        job.SetCannotBePushedBack(true);
        job.SetAssignedPlan(goapPlan);
        producedJob = job;
    }
    public void PlanFixedJob(JOB_TYPE jobType, INTERACTION_TYPE type, IPointOfInterest target, OtherData[] otherData = null) {
        ActualGoapNode node = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[type], this, target, otherData, 0);
        GoapPlan goapPlan = ObjectPoolManager.Instance.CreateNewGoapPlan(node, target);
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, type, target, this);
        goapPlan.SetDoNotRecalculate(true);
        job.SetCannotBePushedBack(true);
        job.SetAssignedPlan(goapPlan);
        jobQueue.AddJobInQueue(job);
    }
    public Character GetDisabledCharacterToCheckOutThatHasIsInHomeSettlementOfThisCharacter() {
        //List<Character> charactersWithRel = relationshipContainer.relationships.Keys.Where(x => x is AlterEgoData).Select(x => (x as AlterEgoData).owner).ToList();
        Character chosenCharacter = null;
        List<Character> charactersWithRel = relationshipContainer.charactersWithOpinion;
        if (charactersWithRel.Count > 0) {
            List<Character> positiveCharacters = RuinarchListPool<Character>.Claim();
            for (int i = 0; i < charactersWithRel.Count; i++) {
                Character character = charactersWithRel[i];
                if (character.isDead /*|| character.isMissing*/ || homeStructure == character.homeStructure) {
                    continue;
                }
                if ((homeSettlement != null && character.currentSettlement != null &&
                                character.currentSettlement == homeSettlement) == false) {
                    //character is invalid, because of given 
                    continue;
                }
                if (relationshipContainer.HasOpinionLabelWithCharacter(character, RelationshipManager.Acquaintance,
                    RelationshipManager.Friend, RelationshipManager.Close_Friend)) {
                    if (character.traitContainer.HasTrait("Paralyzed", "Catatonic")) {
                        positiveCharacters.Add(character);
                    }
                }
            }
            if (positiveCharacters.Count > 0) {
                chosenCharacter = CollectionUtilities.GetRandomElement(positiveCharacters);
            }
            RuinarchListPool<Character>.Release(positiveCharacters);
        }
        return chosenCharacter;
    }
    //private void SetLastAssaultedCharacter(Character character) {
    //    lastAssaultedCharacter = character;
    //    if (character != null) {
    //        //cooldown
    //        GameDate dueDate = GameManager.Instance.Today().AddTicks(GameManager.ticksPerHour);
    //        SchedulingManager.Instance.AddEntry(dueDate, () => RemoveLastAssaultedCharacter(character), this);
    //    }
    //}
    //private void RemoveLastAssaultedCharacter(Character characterToRemove) {
    //    if (lastAssaultedCharacter == characterToRemove) {
    //        SetLastAssaultedCharacter(null);
    //    }
    //}
    public void SetIsConversing(bool state) {
        isConversing = state;
        if (marker) {
            marker.UpdateActionIcon();
        }
        if (isConversing == false) {
            //stopped conversing, set last conversation date
            nonActionEventsComponent.SetLastConversationDate(GameManager.Instance.Today());
        }
    }
    //public void SetIsFlirting(bool state) {
    //    _isFlirting = state;
    //}
    public void AddAdvertisedAction(INTERACTION_TYPE type, bool allowDuplicates = false) {
        if (allowDuplicates || advertisedActions.Contains(type) == false) {
            advertisedActions.Add(type);
            LocationAwarenessUtility.AddToAwarenessList(type, this);
        }
        //advertisedActions.Add(type);
    }
    public void RemoveAdvertisedAction(INTERACTION_TYPE type) {
        if (advertisedActions.Remove(type)) {
            LocationAwarenessUtility.RemoveFromAwarenessList(type, this);
        }
    }
#endregion

#region Inventory
    //public bool ObtainTokenFrom(Character target, SpecialToken token, bool changeCharacterOwnership = true) {
    //    if (target.UnobtainToken(token)) {
    //        ObtainToken(token, changeCharacterOwnership);
    //        return true;
    //    }
    //    return false;
    //}
    public bool AddOwnedItem(TileObject item) {
        if (!ownedItems.Contains(item)) {
            ownedItems.Add(item);
            return true;
        }
        return false;
    }
    public bool RemoveOwnedItem(TileObject item) {
        return ownedItems.Remove(item);
    }
    public bool HasOwnedItemThatIsOnGroundInSameRegion() {
        for (int i = 0; i < ownedItems.Count; i++) {
            TileObject item = ownedItems[i];
            if (item.gridTileLocation != null && item.gridTileLocation.structure.region == currentRegion) {
                return true;
            }
        }
        return false;
    }
    public bool HasOwnedItemInHomeStructure(string itemName) {
        if (homeStructure == null) {
            return false;
        }
        for (int i = 0; i < ownedItems.Count; i++) {
            TileObject item = ownedItems[i];
            if (itemName == item.name && item.gridTileLocation != null && item.gridTileLocation.structure == homeStructure) {
                return true;
            }
        }
        return false;
    }
    public int GetNumOfOwnedItemsInHomeStructure(string itemName) {
        int count = 0;
        if (homeStructure == null) {
            return count;
        }
        for (int i = 0; i < ownedItems.Count; i++) {
            TileObject item = ownedItems[i];
            if (itemName == item.name && item.gridTileLocation != null && item.gridTileLocation.structure == homeStructure) {
                count++;
            }
        }
        return count;
    }
    public bool ObtainItem(TileObject item, bool changeCharacterOwnership = false, bool setOwnership = true) {
        if (AddItem(item)) {
            // item.SetFactionOwner(this.faction);
            item.SetInventoryOwner(this);
            if (changeCharacterOwnership) {
                item.SetCharacterOwner(this);
            } else {
                if (setOwnership) {
                    if (item.characterOwner == null) {
                        item.SetCharacterOwner(this);
                    }
                }
            }
            if (item is EquipmentItem equipment) {
                equipmentComponent.SetEquipment(equipment, this);
            }
            return true;
        }
        return false;
        //token.AdjustQuantity(-1);
    }
    public bool UnobtainItem(TileObject item) {
        if (RemoveItem(item)) {
            RemoveBonusForUnobtainedEquipment(item);
            item.SetInventoryOwner(null);
            return true;
        }
        return false;
    }
    /// <summary>
    /// Function to use if an equip will be replaced by another equipment.
    /// NOTE: This will not call unequipped signals.
    /// </summary>
    /// <param name="item">The equipment to replace</param>
    /// <returns></returns>
    public bool UnobtainItemForEquipmentReplacement(EquipmentItem item) {
        if (RemoveItem(item)) {
            RemoveBonusForUnobtainedEquipment(item, true);
            item.SetInventoryOwner(null);
            return true;
        }
        return false;
    }
    public bool UnobtainItem(TILE_OBJECT_TYPE itemType) {
        TileObject removedItem = RemoveItem(itemType);
        if (removedItem != null) {
            RemoveBonusForUnobtainedEquipment(removedItem);
            removedItem.SetInventoryOwner(null);
            return true;
        }
        return false;
    }
    public bool UnobtainItem(string name) {
        TileObject removedItem = RemoveItem(name);
        if (removedItem != null) {
            RemoveBonusForUnobtainedEquipment(removedItem);
            removedItem.SetInventoryOwner(null);
            return true;
        }
        return false;
    }

    void RemoveBonusForUnobtainedEquipment(TileObject p_tileObject, bool willReplaceEquipment = false) {
        if (p_tileObject is EquipmentItem equipItem) {
            equipItem.SetInventoryOwner(null);
            equipmentComponent.RemoveEquipment(equipItem, this);
            if (!willReplaceEquipment) {
                if (equipItem is WeaponItem) {
                    eventDispatcher.ExecuteWeaponUnequipped(this, equipItem);
                } else if (equipItem is ArmorItem) {
                    eventDispatcher.ExecuteArmorUnequipped(this, equipItem);
                } else if (equipItem is AccessoryItem) {
                    eventDispatcher.ExecuteAccessoryUnequipped(this, equipItem);
                }
            }
        }
    }
    //public bool ConsumeToken(SpecialToken token) {
    //    token.OnConsumeToken(this);
    //    if (token.uses <= 0) {
    //        return RemoveToken(token);
    //    }
    //    return false;
    //}
    private bool AddItem(TileObject item) {
        if (item is EquipmentItem) {
            if (!equipmentInventory.Contains(item)) {
                equipmentInventory.Add(item);
                //item.OnTileObjectAddedToInventoryOf(this);
                Messenger.Broadcast(CharacterSignals.CHARACTER_OBTAINED_ITEM, item, this);
                eventDispatcher.ExecuteItemObtained(this, item);
                return true;
            }
        } else {
            if (!items.Contains(item)) {
                items.Add(item);
                //item.OnTileObjectAddedToInventoryOf(this);
                Messenger.Broadcast(CharacterSignals.CHARACTER_OBTAINED_ITEM, item, this);
                eventDispatcher.ExecuteItemObtained(this, item);
                return true;
            }
        }

        return false;
    }
    private bool RemoveItem(TileObject item) {
        if (item is EquipmentItem) {
            if (equipmentInventory.Remove(item)) {
                Messenger.Broadcast(CharacterSignals.CHARACTER_LOST_ITEM, item, this);
                eventDispatcher.ExecuteItemLost(this, item);
                return true;
            }
        } else {
            if (items.Remove(item)) {
                Messenger.Broadcast(CharacterSignals.CHARACTER_LOST_ITEM, item, this);
                eventDispatcher.ExecuteItemLost(this, item);
                return true;
            }
        }

        return false;
    }
    private TileObject RemoveItem(TILE_OBJECT_TYPE itemType) {
        TileObject removedItem = null;
        for (int i = 0; i < items.Count; i++) {
            if (items[i].tileObjectType == itemType) {
                removedItem = items[i];
                if (RemoveItem(i)) {
                    break;
                }
            }
        }
        return removedItem;
    }
    private TileObject RemoveItem(string name) {
        TileObject removedItem = null;
        for (int i = 0; i < items.Count; i++) {
            if (items[i].name == name) {
                removedItem = items[i];
                if (RemoveItem(i)) {
                    break;
                }
            }
        }
        return removedItem;
    }
    private bool RemoveItem(int index) {
        TileObject item = items[index];
        if (item != null) {
            items.RemoveAt(index);
            Messenger.Broadcast(CharacterSignals.CHARACTER_LOST_ITEM, item, this);
            eventDispatcher.ExecuteItemLost(this, item);
            return true;
        }
        return false;
    }
    public bool DropItem(TileObject item, LocationGridTile gridTile = null) {
        if (UnobtainItem(item)) {
            //if (item.specialTokenType.CreatesObjectWhenDropped()) {
            //    structure.AddItem(item, gridTile);
            //    //location.AddSpecialTokenToLocation(token, structure, gridTile);
            //}
            LocationGridTile targetTile = gridTile;
            if (targetTile == null) {
                targetTile = gridTileLocation;
            }
            if (targetTile == null) {
                return true; //if there is no tile to drop the item, just discard it
            }
            if (targetTile.tileObjectComponent.objHere != null) {
                targetTile = targetTile.GetFirstNearestTileFromThisWithNoObject(true);
            }
            if (targetTile == null) {
                return true; //if there is STILL no tile to drop the item, just discard it
            }
            targetTile.structure.AddPOI(item, targetTile);
            item.OnTileObjectDroppedBy(this, targetTile);
            return true;
        }
        return false;
    }
    public void DropAllItems(LocationGridTile tile) { //, bool removeFactionOwner = false
        for (int i = 0; i < items.Count; i++) {
            TileObject item = items[i];
            if (DropItem(item, tile)) {
                i--;
            }
        }
    }
    public void UnownOrTransferOwnershipOfAllItems() {
        //https://trello.com/c/LbfWIBBh/1866-item-ownership-dead
        //All owned items of character must be unowned when he dies
        //The items must either be unowned or transfered to another resident in his home structure, depending on where the item is currently
        //If it is in home structure transfer it to another random resident, otherwise, unown only

        List<Character> potentialOwners = RuinarchListPool<Character>.Claim();
        if (homeStructure != null) {
            for (int i = 0; i < homeStructure.residents.Count; i++) {
                Character resident = homeStructure.residents[i];
                if (resident != this && resident.faction != null && resident.faction.isMajorFaction) {
                    potentialOwners.Add(resident);
                }
            }
        }
        for (int i = 0; i < ownedItems.Count; i++) {
            TileObject item = ownedItems[i];
            if (item.gridTileLocation == null || item.gridTileLocation.structure != homeStructure) {
                item.SetCharacterOwner(null);
            } else {
                if (potentialOwners != null && potentialOwners.Count > 0) {
                    Character newOwner = CollectionUtilities.GetRandomElement(potentialOwners);
                    item.SetCharacterOwner(newOwner);
                } else {
                    item.SetCharacterOwner(null);
                }
            }
            i--;
        }
        RuinarchListPool<Character>.Release(potentialOwners);
    }
    public void UnownOrTransferOwnershipOfItemsIn(LocationStructure structure) {
        //https://trello.com/c/LbfWIBBh/1866-item-ownership-dead
        //All owned items of character must be unowned when he dies
        //The items must either be unowned or transfered to another resident in his home structure, depending on where the item is currently
        //If it is in home structure transfer it to another random resident, otherwise, unown only

        List<Character> potentialOwners = RuinarchListPool<Character>.Claim();
        if (structure != null) {
            for (int i = 0; i < structure.residents.Count; i++) {
                Character resident = structure.residents[i];
                if (resident != this && resident.faction != null && resident.faction.isMajorFaction) {
                    potentialOwners.Add(resident);
                }
            }
        }
        for (int i = 0; i < ownedItems.Count; i++) {
            TileObject item = ownedItems[i];
            if (item.gridTileLocation != null && item.gridTileLocation.structure == structure) {
                if (potentialOwners != null && potentialOwners.Count > 0) {
                    Character newOwner = CollectionUtilities.GetRandomElement(potentialOwners);
                    item.SetCharacterOwner(newOwner);
                } else {
                    item.SetCharacterOwner(null);
                }
                i--;
            }
        }
        RuinarchListPool<Character>.Release(potentialOwners);
    }
    public void PickUpItem(TileObject item, bool changeCharacterOwnership = false, bool setOwnership = true) {
        item.isBeingCarriedBy?.UnobtainItem(item);
        if (ObtainItem(item, changeCharacterOwnership, setOwnership)) {
            item.gridTileLocation?.structure.RemovePOIDestroyVisualOnly(item, this);
            item.SetPOIState(POI_STATE.ACTIVE);
        }
    }
    public void DestroyItem(TileObject item) {
        item.structureLocation.RemovePOI(item);
        //token.gridTileLocation.structure.location.RemoveSpecialTokenFromLocation(token);
    }
    // private void UpdateItemFactionOwner() {
    //     for (int i = 0; i < items.Count; i++) {
    //         TileObject item = items[i];
    //         item.SetFactionOwner(this.faction);
    //     }
    // }
    public TileObject GetItem(TileObject item) {
        for (int i = 0; i < items.Count; i++) {
            if (items[i] == item) {
                return items[i];
            }
        }
        return null;
    }
    public TileObject GetItem(TILE_OBJECT_TYPE itemType) {
        for (int i = 0; i < items.Count; i++) {
            if (items[i].tileObjectType == itemType) {
                return items[i];
            }
        }
        return null;
    }
    public TileObject GetItem(string itemName) {
        for (int i = 0; i < items.Count; i++) {
            if (items[i].name == itemName) {
                return items[i];
            }
        }
        return null;
    }
    public T GetItem<T>() where T : TileObject {
        for (int i = 0; i < items.Count; i++) {
            if (items[i] is T converted) {
                return converted;
            }
        }
        return null;
    }
    public void PopulateItemsOfType<T>(List<T> p_list) where T : TileObject {
        for (int i = 0; i < items.Count; i++) {
            if (items[i] is T converted) {
                p_list.Add(converted);
            }
        }
    }
    public TileObject GetRandomItem() {
        if (items.Count > 0) {
            return items[UnityEngine.Random.Range(0, items.Count)];
        }
        return null;
    }
    public TileObject GetRandomItemThatIsNotOfType(TILE_OBJECT_TYPE p_type) {
        List<TileObject> choices = RuinarchListPool<TileObject>.Claim();
        for (int i = 0; i < items.Count; i++) {
            TileObject item = items[i];
            if (item.tileObjectType != p_type) {
                choices.Add(item);
            }
        }
        TileObject chosen = null;
        if (choices.Count > 0) {
            chosen = CollectionUtilities.GetRandomElement(choices);
        }
        RuinarchListPool<TileObject>.Release(choices);
        return chosen;
    }
    public bool HasItem(TileObject item) {
        return GetItem(item) != null;
    }
    public bool HasItem(TILE_OBJECT_TYPE itemType) {
        return GetItem(itemType) != null;
    }
    public bool HasItem(string itemName) {
        return GetItem(itemName) != null;
    }
    public bool HasItem<T>() where T : TileObject {
        return GetItem<T>() != null;
    }
    public bool HasItem() {
        return items.Count > 0;
    }
    public bool HasItemOtherThan(TILE_OBJECT_TYPE p_type) {
        for (int i = 0; i < items.Count; i++) {
            if (items[i].tileObjectType != p_type) {
                return true;
            }
        }
        return false;
    }
    public int GetItemCount(string name) {
        int count = 0;
        for (int i = 0; i < items.Count; i++) {
            if (items[i].name == name) {
                count++;
            }
        }
        return count;
    }
    public bool IsInventoryAtFullCapacity() {
        return items.Count >= characterClass.inventoryCapacity;
    }
    public bool IsItemInteresting(string itemName) {
        return interestedItemNames != null && interestedItemNames.Contains(itemName);
    }
    public void AddItemAsInteresting(string itemName) {
        if (interestedItemNames.Contains(itemName) == false) {
            interestedItemNames.Add(itemName);
        }
    }
    public void AddItemAsInteresting(params string[] itemNames) {
        for (int i = 0; i < itemNames.Length; i++) {
            AddItemAsInteresting(itemNames[i]);
        }
    }
    public void RemoveItemAsInteresting(params string[] itemNames) {
        for (int i = 0; i < itemNames.Length; i++) {
            RemoveItemAsInteresting(itemNames[i]);
        }
    }
    public void RemoveItemAsInteresting(string itemName) {
        interestedItemNames.Remove(itemName);
    }
#endregion

#region Awareness
    public void LogAwarenessList() {
        if (currentLocationAwareness != null) {
            string log = $"--------------AWARENESS LIST OF {name}-----------------";
            foreach (KeyValuePair<INTERACTION_TYPE, List<IPointOfInterest>> kvp in currentLocationAwareness.awareness) {
                log += $"\n{kvp.Key}: ";
                for (int i = 0; i < kvp.Value.Count; i++) {
                    if (i > 0) {
                        log += ", ";
                    }
                    log += kvp.Value[i].ToString();
                }
            }
            logComponent.PrintLogIfActive(log);
        }
    }
#endregion

#region Point Of Interest
    //Returns the chosen action for the plan
    public GoapAction AdvertiseActionsToActor(Character actor, GoapEffect precondition, GoapPlanJob job, ref int cost, ref string log) {
        GoapAction chosenAction = null;
        if (advertisedActions != null && advertisedActions.Count > 0) {//&& IsAvailable()
            bool isCharacterAvailable = IsAvailable();
            GoapAction lowestCostAction = null;
            int currentLowestCost = 0;
            log += $"\n--Choices for {precondition}";
            log += "\n--";
            for (int i = 0; i < advertisedActions.Count; i++) {
                INTERACTION_TYPE currType = advertisedActions[i];
                GoapAction action = InteractionManager.Instance.goapActionData[currType];
                if (!isCharacterAvailable && !action.canBeAdvertisedEvenIfTargetIsUnavailable) {
                    //if this character is not available, check if the current action type can be advertised even when the character is inactive.
                    continue; //skip
                }
                if ((action.canBePerformedEvenIfPathImpossible || actor.movementComponent.HasPathToEvenIfDiffRegion(gridTileLocation)) && RaceManager.Instance.CanCharacterDoGoapAction(actor, currType)) {
                    OtherData[] data = job.GetOtherDataFor(currType);
                    if (action.CanSatisfyRequirements(actor, this, data, job)
                        && action.WillEffectsSatisfyPrecondition(precondition, actor, this, job)) { //&& InteractionManager.Instance.CanSatisfyGoapActionRequirementsOnBuildTree(currType, actor, this, data)
                        int actionCost = action.GetCost(actor, this, job);
                        log += $"({actionCost}){action.goapName}-{nameWithID}, ";
                        if (lowestCostAction == null || actionCost < currentLowestCost) {
                            lowestCostAction = action;
                            currentLowestCost = actionCost;
                        }
                    }
                }
            }
            cost = currentLowestCost;
            chosenAction = lowestCostAction;
            //return usableActions;
        }
        return chosenAction;
    }
    public bool CanAdvertiseActionToActor(Character actor, GoapAction action, GoapPlanJob job) {
        if ((IsAvailable() || action.canBeAdvertisedEvenIfTargetIsUnavailable)
            //&& advertisedActions != null && advertisedActions.Contains(action.goapType)
            && actor.trapStructure.SatisfiesForcedStructure(this)
            && actor.trapStructure.SatisfiesForcedArea(this)
            && RaceManager.Instance.CanCharacterDoGoapAction(actor, action.goapType)
            && (action.canBePerformedEvenIfPathImpossible || actor.movementComponent.HasPathToEvenIfDiffRegion(gridTileLocation))) {
            OtherData[] data = job.GetOtherDataFor(action.goapType);
            if (action.CanSatisfyRequirements(actor, this, data, job)) {
                return true;
            }
        }
        return false;
    }
    public void SetPOIState(POI_STATE state) {
        this.state = state;
    }
    public bool IsAvailable() {
        return state != POI_STATE.INACTIVE;
    }
    public void OnPlacePOI() { /*FOR INTERFACE ONLY*/ }
    public void OnLoadPlacePOI() { /*FOR INTERFACE ONLY*/ }
    public void OnDestroyPOI() { /*FOR INTERFACE ONLY*/ }
    public virtual bool IsStillConsideredPartOfAwarenessByCharacter(Character character) {
        if (character.currentRegion == currentRegion && !isBeingSeized/* && !isMissing*/) {
            if (!isDead && carryComponent.masterCharacter.movementComponent.isTravellingInWorld) {
                return false;
            }
            if (isDead && !marker) {
                return false;
            }
            if (isInVampireBatForm) {
                Vampire vampireTrait = traitContainer.GetTraitOrStatus<Vampire>("Vampire");
                if (!vampireTrait.DoesCharacterKnowThisVampire(character)) {
                    return false;
                }
            } else if (isInWerewolfForm) {
                if (!lycanData.DoesCharacterKnowThisLycan(character)) {
                    return false;
                }
            }

            if (character.isInVampireBatForm) {
                Vampire vampireTrait = character.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
                if (!vampireTrait.DoesCharacterKnowThisVampire(this)) {
                    return false;
                }
            } else if (character.isInWerewolfForm) {
                if (!character.lycanData.DoesCharacterKnowThisLycan(this)) {
                    return false;
                }
            }
            return true;
        }
        return false;
    }
    //Characters cannot be owned by other characters
    public bool IsOwnedBy(Character character) { return false; }
    public bool Advertises(INTERACTION_TYPE type) {
        return advertisedActions != null && advertisedActions.Contains(type);
    }
#endregion

#region Goap
    //public void SetNumWaitingForGoapThread(int amount) {
    //    _numOfWaitingForGoapThread = amount;
    //}
    public virtual void ConstructInitialGoapAdvertisementActions() {
        //poiGoapActions = new List<INTERACTION_TYPE>();
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        AddAdvertisedAction(INTERACTION_TYPE.SLEEP_OUTSIDE);
        AddAdvertisedAction(INTERACTION_TYPE.GO_TO);
        AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
        AddAdvertisedAction(INTERACTION_TYPE.RETURN_HOME);
        AddAdvertisedAction(INTERACTION_TYPE.CARRY);
        AddAdvertisedAction(INTERACTION_TYPE.DROP);
        AddAdvertisedAction(INTERACTION_TYPE.CARRY_CORPSE);
        AddAdvertisedAction(INTERACTION_TYPE.DROP_CORPSE);
        AddAdvertisedAction(INTERACTION_TYPE.CARRY_RESTRAINED);
        AddAdvertisedAction(INTERACTION_TYPE.DROP_RESTRAINED);
        AddAdvertisedAction(INTERACTION_TYPE.KNOCKOUT_CHARACTER);
        AddAdvertisedAction(INTERACTION_TYPE.RESTRAIN_CHARACTER);
        AddAdvertisedAction(INTERACTION_TYPE.JUDGE_CHARACTER);
        AddAdvertisedAction(INTERACTION_TYPE.SLAY_CHARACTER);
        AddAdvertisedAction(INTERACTION_TYPE.DOUSE_FIRE);
        AddAdvertisedAction(INTERACTION_TYPE.BURY_CHARACTER);
        AddAdvertisedAction(INTERACTION_TYPE.POISON);
        AddAdvertisedAction(INTERACTION_TYPE.EXILE);
        AddAdvertisedAction(INTERACTION_TYPE.WHIP);
        AddAdvertisedAction(INTERACTION_TYPE.EXECUTE);
        AddAdvertisedAction(INTERACTION_TYPE.ABSOLVE);
        AddAdvertisedAction(INTERACTION_TYPE.BURN_AT_STAKE);
        AddAdvertisedAction(INTERACTION_TYPE.START_TEND);
        AddAdvertisedAction(INTERACTION_TYPE.START_DOUSE);
        AddAdvertisedAction(INTERACTION_TYPE.START_CLEANSE);
        AddAdvertisedAction(INTERACTION_TYPE.START_DRY);
        AddAdvertisedAction(INTERACTION_TYPE.START_PATROL);
        AddAdvertisedAction(INTERACTION_TYPE.PATROL);
        AddAdvertisedAction(INTERACTION_TYPE.BEGIN_MINE);
        AddAdvertisedAction(INTERACTION_TYPE.EAT_ALIVE);
        AddAdvertisedAction(INTERACTION_TYPE.DECREASE_MOOD);
        AddAdvertisedAction(INTERACTION_TYPE.DISABLE);
        AddAdvertisedAction(INTERACTION_TYPE.TORTURE);
        AddAdvertisedAction(INTERACTION_TYPE.BIRTH_RATMAN);
        AddAdvertisedAction(INTERACTION_TYPE.CARRY_PATIENT);
        AddAdvertisedAction(INTERACTION_TYPE.QUARANTINE);
        AddAdvertisedAction(INTERACTION_TYPE.START_PLAGUE_CARE);
        AddAdvertisedAction(INTERACTION_TYPE.CARE);
        AddAdvertisedAction(INTERACTION_TYPE.LONG_STAND_STILL);
        AddAdvertisedAction(INTERACTION_TYPE.COOK);
        AddAdvertisedAction(INTERACTION_TYPE.MAKE_LOVE);
        AddAdvertisedAction(INTERACTION_TYPE.INVITE);

        if (this is Summon) {
            AddAdvertisedAction(INTERACTION_TYPE.PLAY);
            if (this is GiantSpider) {
                AddAdvertisedAction(INTERACTION_TYPE.LAY_EGG);
                AddAdvertisedAction(INTERACTION_TYPE.BUTCHER);
            } else if (this is Wolf) {
                AddAdvertisedAction(INTERACTION_TYPE.BUTCHER);
            } else if (this is SmallSpider) {
                AddAdvertisedAction(INTERACTION_TYPE.BUTCHER);
            } else if (this is Wurm) {
                AddAdvertisedAction(INTERACTION_TYPE.BURROW);
            }
        }
        if (this is Animal) {
            AddAdvertisedAction(INTERACTION_TYPE.BUTCHER);
            AddAdvertisedAction(INTERACTION_TYPE.EAT_CORPSE);
            AddAdvertisedAction(INTERACTION_TYPE.DRINK_BLOOD);
        }
        if (isNormalCharacter) {
            AddAdvertisedAction(INTERACTION_TYPE.DAYDREAM);
            AddAdvertisedAction(INTERACTION_TYPE.PRAY);
            AddAdvertisedAction(INTERACTION_TYPE.ASK_FOR_HELP_SAVE_CHARACTER);
            AddAdvertisedAction(INTERACTION_TYPE.ASK_FOR_HELP_REMOVE_POISON_TABLE);
            AddAdvertisedAction(INTERACTION_TYPE.TANTRUM);
            AddAdvertisedAction(INTERACTION_TYPE.ASK_TO_STOP_JOB);
            AddAdvertisedAction(INTERACTION_TYPE.STRANGLE);
            AddAdvertisedAction(INTERACTION_TYPE.CRY);
            AddAdvertisedAction(INTERACTION_TYPE.TEASE);
            AddAdvertisedAction(INTERACTION_TYPE.DANCE);
            AddAdvertisedAction(INTERACTION_TYPE.SING);
            AddAdvertisedAction(INTERACTION_TYPE.SCREAM_FOR_HELP);
            //AddAdvertisedAction(INTERACTION_TYPE.CHANGE_CLASS);
            AddAdvertisedAction(INTERACTION_TYPE.STUDY_MONSTER);
            AddAdvertisedAction(INTERACTION_TYPE.PICKPOCKET);
            AddAdvertisedAction(INTERACTION_TYPE.STEAL_COINS);
            AddAdvertisedAction(INTERACTION_TYPE.CHANGE_CLASS);
            AddAdvertisedAction(INTERACTION_TYPE.STOCKPILE_FOOD);

            //NOTE: Removed the creation of healing potion, etc. on the fly because it conflicts with the current crafting of objects
            //It is confusing to have a crafting then another one the creates them in the inventory without any crafting
            //Also, we already decided to not have a very deep branching of plans when goap planning
            //Example: When a character wants to heal himself he needs healing potion, if he does not have one, he must get a healing potion, but if there is none, the plan should be discarded
            //It should not continue further like getting ingredients to create a healing potion
            //AddAdvertisedAction(INTERACTION_TYPE.CREATE_HEALING_POTION);
            //AddAdvertisedAction(INTERACTION_TYPE.CREATE_ANTIDOTE);
            //AddAdvertisedAction(INTERACTION_TYPE.CREATE_POISON_FLASK);

            //AddAdvertisedAction(INTERACTION_TYPE.REMOVE_POISON);
            //AddAdvertisedAction(INTERACTION_TYPE.REMOVE_FREEZING);
            AddAdvertisedAction(INTERACTION_TYPE.SHARE_INFORMATION);
            AddAdvertisedAction(INTERACTION_TYPE.REPORT_CRIME);
            AddAdvertisedAction(INTERACTION_TYPE.DRINK_BLOOD);
            AddAdvertisedAction(INTERACTION_TYPE.BUTCHER);
            AddAdvertisedAction(INTERACTION_TYPE.HAVE_AFFAIR);
            //AddAdvertisedAction(INTERACTION_TYPE.OPEN);
            AddAdvertisedAction(INTERACTION_TYPE.CREATE_CULTIST_KIT);
            AddAdvertisedAction(INTERACTION_TYPE.REMOVE_BUFF);
            //AddAdvertisedAction(INTERACTION_TYPE.EXTERMINATE);
            //AddAdvertisedAction(INTERACTION_TYPE.RAID);
            //AddAdvertisedAction(INTERACTION_TYPE.COUNTERATTACK_ACTION);
            AddAdvertisedAction(INTERACTION_TYPE.EVANGELIZE);
            AddAdvertisedAction(INTERACTION_TYPE.BUILD_CAMPFIRE);
            AddAdvertisedAction(INTERACTION_TYPE.VAMPIRIC_EMBRACE);
            AddAdvertisedAction(INTERACTION_TYPE.EAT_CORPSE);
        }
        if (race.IsSapient()) {
            AddAdvertisedAction(INTERACTION_TYPE.REPORT_CORRUPTED_STRUCTURE);
            AddAdvertisedAction(INTERACTION_TYPE.HEAL_SELF);
        }
    }
    /// <summary>
    /// This should only be used for plans that come/constructed from the outside.
    /// </summary>
    /// <param name="plan">Plan to be added</param>
    //public void AddPlan(GoapPlan plan, bool isPriority = false, bool processPlanSpecialCases = true) {
    //    if (!allGoapPlans.Contains(plan)) {
    //        plan.SetPriorityState(isPriority);
    //        if (isPriority) {
    //            allGoapPlans.Insert(0, plan);
    //        } else {
    //            bool hasBeenInserted = false;
    //            if (plan.job != null) {
    //                for (int i = 0; i < allGoapPlans.Count; i++) {
    //                    GoapPlan currentPlan = allGoapPlans[i];
    //                    if (currentPlan.isPriority) {
    //                        continue;
    //                    }
    //                    if (currentPlan.job == null || plan.job.priority < currentPlan.job.priority) {
    //                        allGoapPlans.Insert(i, plan);
    //                        hasBeenInserted = true;
    //                        break;
    //                    }
    //                }re
    //            }
    //            if (!hasBeenInserted) {
    //                allGoapPlans.Add(plan);
    //            }
    //        }

    //        if (processPlanSpecialCases) {
    //            ////If a character is strolling or idly returning home and a plan is added to this character, end the action/state
    //            //if (stateComponent.currentState != null && (stateComponent.currentState.characterState == CHARACTER_STATE.STROLL
    //            //    || stateComponent.currentState.characterState == CHARACTER_STATE.STROLL_OUTSIDE
    //            //    || stateComponent.currentState.characterState == CHARACTER_STATE.PATROL)) {
    //            //    stateComponent.currentState.OnExitThisState();
    //            //} else if (stateComponent.stateToDo != null && (stateComponent.stateToDo.characterState == CHARACTER_STATE.STROLL
    //            //     || stateComponent.stateToDo.characterState == CHARACTER_STATE.STROLL_OUTSIDE
    //            //     || stateComponent.stateToDo.characterState == CHARACTER_STATE.PATROL)) {
    //            //    stateComponent.SetStateToDo(null);
    //            //} else if (currentAction != null && currentAction.goapType == INTERACTION_TYPE.RETURN_HOME) {
    //            //    if (currentAction.parentPlan == null || currentAction.parentPlan.category == GOAP_CATEGORY.IDLE) {
    //            //        currentAction.StopAction();
    //            //    }
    //            //}

    //            if (plan.job != null && (plan.job.jobType.IsNeedsTypeJob() || plan.job.jobType.IsEmergencyTypeJob())) {
    //                //Unassign Location Job if character decides to rest, eat or have fun.
    //                homeNpcSettlement.jobQueue.UnassignAllJobsTakenBy(this);
    //                faction.activeQuest?.jobQueue.UnassignAllJobsTakenBy(this);
    //            }
    //        }
    //    }
    //}
    //public void RecalculateJob(GoapPlanJob job) {
    //    if(job.assignedPlan != null) {
    //        job.assignedPlan.SetIsBeingRecalculated(true);
    //        MultiThreadPool.Instance.AddToThreadPool(new GoapThread(this, job.assignedPlan, job));
    //    }
    //}
    //public bool IsPOIInCharacterAwarenessList(IPointOfInterest poi, List<IPointOfInterest> awarenesses) {
    //    if (awarenesses != null && awarenesses.Count > 0) {
    //        for (int i = 0; i < awarenesses.Count; i++) {
    //            if (awarenesses[i] == poi) {
    //                return true;
    //            }
    //        }
    //    }
    //    return false;
    //}
    public void PerformJob(JobQueueItem job) {
        string log = string.Empty;
#if DEBUG_LOG
        log = $"PERFORMING GOAP PLANS OF {name}";
#endif
        if (currentActionNode != null) {
#if DEBUG_LOG
            log = $"{log}\n{name} can't perform another action because he/she is currently performing {currentActionNode.action.goapName}";
            logComponent.PrintLogIfActive(log);
#endif
            return;
        }

        GoapPlanJob currentTopPrioJob = job as GoapPlanJob;
        if (currentTopPrioJob?.assignedPlan != null) {
            GoapPlan plan = currentTopPrioJob.assignedPlan;
#if DEBUG_LOG
            if (plan.startingNode != null && plan.startingNode.singleNode != null && plan.startingNode.singleNode.actor != this) {
                Debug.LogError($"{name} has a finished job from another character {currentTopPrioJob.jobType.ToString()} {currentTopPrioJob.ToString()} of {plan.startingNode.singleNode.actor?.name}");
                // jobQueue.RemoveJobInQueue(currentTopPrioJob, false, string.Empty);
                return;
            }
#endif
            ActualGoapNode currentNode = plan.currentActualNode;
#if DEBUG_PROFILER
            Profiler.BeginSample($"{name} - {currentNode.action.name} - Can Do Goap Action");
#endif
            bool canCharacterDoGoapAction = RaceManager.Instance.CanCharacterDoGoapAction(this, currentNode.action.goapType);
#if DEBUG_PROFILER
            Profiler.EndSample();
#endif

#if DEBUG_PROFILER
            Profiler.BeginSample($"{name} - {currentNode.action.name} - Can Satisfy Goap Action Requirements");
#endif
            bool canSatisfyGoapActionRequirements = InteractionManager.Instance.CanSatisfyGoapActionRequirements(currentNode.action.goapType, currentNode.actor, currentNode.poiTarget, currentNode.otherData, currentTopPrioJob);
#if DEBUG_PROFILER
            Profiler.EndSample();
#endif

            if (canCharacterDoGoapAction && canSatisfyGoapActionRequirements) {
#if DEBUG_PROFILER
                Profiler.BeginSample($"{name} - {currentNode.action.name} - Can Satisfy All Preconditions");
#endif
                bool preconditionsSatisfied = currentNode.action.CanSatisfyAllPreconditions(currentNode.actor, currentNode.poiTarget, currentNode.otherData, currentTopPrioJob.jobType, out Precondition failedPrecondition);
#if DEBUG_PROFILER
                Profiler.EndSample();
#endif

                if (!preconditionsSatisfied) {
#if DEBUG_PROFILER
                    Profiler.BeginSample($"{name} - {currentNode.action.name} - Preconditions not satisfied");
#endif
#if DEBUG_LOG
                    log = $"{log}\n - {currentNode} Action's preconditions are not all satisfied, trying to recalculate plan...";
#endif
                    if (plan.doNotRecalculate) {
#if DEBUG_LOG
                        log = $"{log}\n - {currentNode} Action's plan has doNotRecalculate state set to true, dropping plan...";
                        logComponent.PrintLogIfActive(log);
#endif
                        string reason = string.Empty;
                        if (currentTopPrioJob.jobType.IsCultistJob()) {
                            reason = GetCultistUnableToDoJobReason(currentTopPrioJob, failedPrecondition, currentNode.goapType);
                        }
                        currentNode.action.OnStopWhileStarted(currentNode);
                        currentTopPrioJob.CancelJob(reason);
                    } else {
#if DEBUG_LOG
                        logComponent.PrintLogIfActive(log);
#endif
                        planner.RecalculateJob(currentTopPrioJob);
                    }
#if DEBUG_PROFILER
                    Profiler.EndSample();
#endif
                } else {
                    //If character is Troll and job is Move Character, do not perform if target is not in vision, or target is outside and it is not Night time
#if DEBUG_PROFILER
                    Profiler.BeginSample($"{name} Troll Checking");
#endif
                    if (this is Troll && currentTopPrioJob.jobType == JOB_TYPE.CAPTURE_CHARACTER) {
                        bool shouldCancelJob = false;
                        if (!marker || (!marker.IsPOIInVision(currentTopPrioJob.targetPOI as Character) && !carryComponent.IsPOICarried(currentTopPrioJob.targetPOI) && !isAtHomeStructure && !IsInHomeSettlement())) {
                            shouldCancelJob = true;
                        }
                        if (!shouldCancelJob) {
                            if (currentTopPrioJob.targetPOI.gridTileLocation == null) {
                                shouldCancelJob = true;
                            } else {
                                TIME_IN_WORDS timeInWords = GameManager.Instance.GetCurrentTimeInWordsOfTick(null);
                                if (timeInWords != TIME_IN_WORDS.EARLY_NIGHT && timeInWords != TIME_IN_WORDS.LATE_NIGHT && timeInWords != TIME_IN_WORDS.AFTER_MIDNIGHT && !currentTopPrioJob.targetPOI.gridTileLocation.structure.isInterior) {
                                    shouldCancelJob = true;
                                }
                            }
                        }
                        if (shouldCancelJob) {
#if DEBUG_LOG
                            log = $"{log}\n-Character is troll and job is Move Character, cancel job";
                            logComponent.PrintLogIfActive(log);
#endif
                            currentNode.action.OnStopWhileStarted(currentNode);
                            currentTopPrioJob.CancelJob();
                            return;
                        }
                    }
#if DEBUG_PROFILER
                    Profiler.EndSample();
#endif

#if DEBUG_PROFILER
                    Profiler.BeginSample($"{name} Carry check");
#endif
                    //Do not perform action if the target character is still in another character's party, this means that he/she is probably being abducted
                    //Wait for the character to be in its own party before doing the action
                    if (currentNode.poiTarget.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
                        Character targetCharacter = currentNode.poiTarget as Character;
                        if (!targetCharacter.carryComponent.IsNotBeingCarried() && targetCharacter.isBeingCarriedBy != this) {
#if DEBUG_LOG
                            log = $"{log}\n - {targetCharacter.name} is not in its own party, waiting and skipping...";
                            logComponent.PrintLogIfActive(log);
#endif
                            return;
                        }
                    }
#if DEBUG_PROFILER
                    Profiler.EndSample();
#endif
                    //if (currentNode.poiTarget != this && currentNode.isStealth) {
                    //    //When performing a stealth job action to a character check if that character is already in vision range, if it is, check if the character doesn't have anyone other than this character in vision, if it is, skip it
                    //    if (marker.IsPOIInVision(currentNode.poiTarget) && !marker.CanDoStealthActionToTarget(currentNode.poiTarget)) {
                    //        log = $"{log}\n - Action is stealth and character cannot do stealth action right now...";
                    //        logComponent.PrintLogIfActive(log);
                    //        return;
                    //    }
                    //}
#if DEBUG_PROFILER
                    Profiler.BeginSample($"{name} Lazy checking");
#endif
                    if (traitContainer.HasTrait("Lazy")) {
                        Lazy lazy = traitContainer.GetTraitOrStatus<Lazy>("Lazy");
                        float chance = lazy.GetTriggerChance(this);
#if DEBUG_LOG
                        log = $"{log}\n - Character is lazy, has {chance.ToString("F2")} chance to not perform job if it is a settlement job...";
#endif
                        //Note: Changed the checker from "Just settlement jobs" to "anything other than personal jobs", because non personal jobs are treated as work jobs
                        if (currentTopPrioJob.originalOwner != null && currentTopPrioJob.originalOwner.ownerType != JOB_OWNER.CHARACTER) { //currentTopPrioJob.originalOwner.ownerType == JOB_OWNER.SETTLEMENT
                            if (GameUtilities.RollChance(chance, ref log)) {
                                if (lazy.TriggerLazy()) {
#if DEBUG_LOG
                                    log = $"{log}\n - Character triggered lazy, not going to do job, and cancel it";
                                    logComponent.PrintLogIfActive(log);
#endif
                                    currentNode.action.OnStopWhileStarted(currentNode);
                                    currentTopPrioJob.CancelJob();
                                    return;
                                } else {
#if DEBUG_LOG
                                    log = $"{log}\n - Character did not trigger lazy, continue to do action";
#endif
                                }
                            }
                        } else {
#if DEBUG_LOG
                            log = $"{log}\n - Job is not a needs type job, continue to do job";
#endif
                        }
                    }
#if DEBUG_PROFILER
                    Profiler.EndSample();
#endif

#if DEBUG_LOG
                    log = $"{log}\n - Action's preconditions are all satisfied, doing action...";
                    logComponent.PrintLogIfActive(log);
#endif
#if DEBUG_PROFILER
                    Profiler.BeginSample($"{name} - {currentNode.action.name} - Character Will Do Job Signal");
#endif
                    Messenger.Broadcast(JobSignals.CHARACTER_WILL_DO_JOB, this, currentTopPrioJob);
#if DEBUG_PROFILER
                    Profiler.EndSample();
#endif

#if DEBUG_PROFILER
                    Profiler.BeginSample($"{name} - {currentNode.action.name} - Do Action Call");
#endif
                    currentNode.DoAction(currentTopPrioJob, plan);
#if DEBUG_PROFILER
                    Profiler.EndSample();
#endif
                }
            } else {
#if DEBUG_PROFILER
                Profiler.BeginSample($"{name} - {currentNode.action.name} - Did not meet requirements");
#endif
#if DEBUG_LOG
                log = $"{log}\n - {plan.currentActualNode} Action did not meet current requirements and allowed actions, dropping plan...";
                logComponent.PrintLogIfActive(log);
#endif
                string reason = string.Empty;
                if (currentNode.associatedJob != null && currentNode.associatedJob.jobType.IsCultistJob()) {
                    Precondition failedPrecondition = currentNode.action.GetPrecondition(this, currentNode.poiTarget, currentNode.otherData, currentTopPrioJob.jobType, out bool isOverridden);
                    if (failedPrecondition != null) {
                        reason = GetCultistUnableToDoJobReason(currentTopPrioJob, failedPrecondition, currentNode.action.goapType);
                    }
                }
                currentNode.action.OnStopWhileStarted(currentNode);
                currentTopPrioJob.CancelJob(reason);
#if DEBUG_PROFILER
                Profiler.EndSample();
#endif
            }
        }
    }
    public void PerformGoapAction() {
        string log = string.Empty;
        if (currentActionNode == null) {
#if DEBUG_LOG
            log = $"{name} cannot PerformGoapAction because there is no current action!";
            logComponent.PrintLogIfActive(log);
#endif
            return;
        }
#if DEBUG_LOG
        log = $"{name} is performing goap action: {currentActionNode.action.goapName}";
#endif
        InnerMapManager.Instance.FaceTarget(this, currentActionNode.poiTarget);
        bool willStillContinueAction = true;
        OnStartPerformGoapAction(currentActionNode, ref willStillContinueAction);
        if (!willStillContinueAction) {
            return;
        }
        if (InteractionManager.Instance.CanSatisfyGoapActionRequirements(currentActionNode.action.goapType, currentActionNode.actor, currentActionNode.poiTarget, currentActionNode.otherData, currentActionNode.associatedJob)
            && currentActionNode.action.CanSatisfyAllPreconditions(currentActionNode.actor, currentActionNode.poiTarget, currentActionNode.otherData, currentActionNode.associatedJobType)) {
#if DEBUG_LOG
            log = $"{log}\nAction satisfies all requirements and preconditions, proceeding to perform actual action: {currentActionNode.action.goapName} to {currentActionNode.poiTarget.name} at {currentActionNode.poiTarget.gridTileLocation}";
            logComponent.PrintLogIfActive(log);
#endif
            currentActionNode.PerformAction();
        } else {
#if DEBUG_LOG
            log = $"{log}\nAction did not meet all requirements and preconditions. Will try to recalculate plan...";
#endif
            GoapPlan plan = currentPlan;
            if (plan.doNotRecalculate) {
#if DEBUG_LOG
                log = $"{log}\n - Action's plan has doNotRecalculate state set to true, dropping plan...";
                logComponent.PrintLogIfActive(log);
#endif
                currentJob.CancelJob();
            } else {
#if DEBUG_LOG
                logComponent.PrintLogIfActive(log);
#endif
                Assert.IsNotNull(currentJob);
                Assert.IsTrue(currentJob is GoapPlanJob);
                planner.RecalculateJob(currentJob as GoapPlanJob);

                //Moved this here, because this must not be called if plan cannot be recalculated since, the CancelJob already calls this function, it will double the call
                SetCurrentActionNode(null, null, null);
            }
        }
    }
    public void GoapActionResult(string result, ActualGoapNode actionNode) {
        string log = string.Empty;
#if DEBUG_LOG
        log = $"{name} is done performing goap action: {actionNode.action.goapName}";
#endif
        Assert.IsNotNull(currentPlan, $"{name} has finished action {actionNode.action.name} with result {result} but currentPlan is null!");
        GoapPlan plan = currentPlan;
        GoapPlanJob job = currentJob as GoapPlanJob;
        Assert.IsNotNull(job, $"Current job of {name} is null or not GoapPlanJob {currentJob?.ToString() ?? "Null"}");

        if (actionNode == currentActionNode) {
            SetCurrentActionNode(null, null, null);
        }

        if (isDead || !limiterComponent.canPerform) {
#if DEBUG_LOG
            log = $"{log}\n{name} is dead or cannot perform! Do not do GoapActionResult, automatically CancelJob";
            logComponent.PrintLogIfActive(log);
#endif
            job.CancelJob();
            return;
        }

        if (result == InteractionManager.Goap_State_Success) {
#if DEBUG_LOG
            log = log + "\nPlan is setting next action to be done...";
#endif
            OnCharacterFinishedActionSuccessfully(actionNode);
            Messenger.Broadcast(JobSignals.CHARACTER_DID_ACTION_SUCCESSFULLY, this, actionNode);
            plan.SetNextNode();
            if (plan.currentNode == null) {
#if DEBUG_LOG
                log = $"{log}\nThis action is the end of plan.";
                //if (job.originalOwner.ownerType != JOB_OWNER.CHARACTER && traitContainer.GetNormalTrait<Trait>("Hardworking") != null) {
                //    log += "\nFinished a npcSettlement job and character is hardworking, increase happiness by 3000...";
                //    needsComponent.AdjustHappiness(3000);
                //}
                logComponent.PrintLogIfActive(log);
#endif
                //bool forceRemoveJobInQueue = true;
                ////If an action is stopped as current action (meaning it was cancelled) and it is a npcSettlement/faction job, do not remove it from the queue
                //if (actionNode.isStoppedAsCurrentAction && plan != null && plan.job != null && plan.job.jobQueueParent.isAreaOrQuestJobQueue) {
                //    forceRemoveJobInQueue = false;
                //}
                job.SetFinishedSuccessfully(true);
                OnCharacterFinishedJobSuccessfully(job);
                Messenger.Broadcast(CharacterSignals.CHARACTER_FINISHED_JOB_SUCCESSFULLY, this, job);
                //this means that this is the end goal so end this plan now
                job.ForceCancelJob();
            } else {
                SetCurrentJob(job);
#if DEBUG_LOG
                log = $"{log}\nNext action for this plan: {plan.currentActualNode.goapName}";
                //if (plan.job != null && plan.job.assignedCharacter != this) {
                //    log += "\nPlan has a job: " + plan.job.name + ". Assigned character " + (plan.job.assignedCharacter != null ? plan.job.assignedCharacter.name : "None") + " does not match with " + name + ".";
                //    log += "Drop plan because this character is no longer the one assigned";
                //    DropPlan(plan);
                //}
                logComponent.PrintLogIfActive(log);
#endif
                //PlanGoapActions();
            }
        }
        //Reason: https://trello.com/c/58aGENsO/1867-attempt-to-find-another-nearby-chair-first-instead-of-dropping-drink-eat-sit-down-actions
        else if (result == InteractionManager.Goap_State_Fail) {
            if (plan.doNotRecalculate) {
                job.CancelJob();
            } else {
                planner.RecalculateJob(job);
            }
        }
    }
    public void SetCurrentActionNode(ActualGoapNode actionNode, JobQueueItem job, GoapPlan plan) {
#if DEBUG_LOG
        Debug.Log($"Set current node of {name} to - Action: {actionNode?.name ?? "Null"}.\nJob: {job?.ToString() ?? "Null"}.\nPlan: {plan?.LogPlan() ?? "Null"}");
#endif
        if (currentActionNode != null) {
            previousCharacterDataComponent.SetPreviousActionType(currentActionNode.goapType);
            previousCharacterDataComponent.SetPreviousJobType(currentActionNode.associatedJobType);
        }
        currentActionNode = actionNode;
#if DEBUG_LOG
        if (currentActionNode != null) {
            logComponent.PrintLogIfActive($"{name} will do action {actionNode.action.name} to {actionNode.poiTarget}");
            //Current Job must always be the job in the top prio, if there is inconsistency with the currentActionNode, then the problem lies on what you set as the currentActionNode
        }
#endif
        SetCurrentJob(job);
        SetCurrentPlan(plan);

        if (marker) {
            marker.UpdateActionIcon();
        }
    }
    private void SetCurrentPlan(GoapPlan plan) {
        currentPlan = plan;
    }
    //Only stop an action node if it is the current action node
    ///Stopping action node does not mean that the job will be cancelled, if you want to cancel job at the same time call <see cref="StopCurrentActionNodeAndCancelItsJob">
    public bool StopCurrentActionNode(string reason = "") {
        if (currentActionNode == null) {
            return false;
        }
        bool shouldLogReason = true;
        if (reason != "" && currentActionNode.poiTarget != this/* && currentJob.jobType != JOB_TYPE.WATCH*/) {
            //if(currentActionNode.poiTarget is Character) {
            //    Trait targetDeadTrait = currentActionNode.poiTarget.traitContainer.GetNormalTrait<Trait>("Dead");
            //    if(targetDeadTrait.gainedFromDoing == currentActionNode) {
            //        shouldLogReason = false;
            //    }
            //}
        } else {
            shouldLogReason = false;
        }
        if (shouldLogReason) {
            Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "current_action_abandoned_reason", providedTags: LOG_TAG.Social);
            log.AddToFillers(this, name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            string jobName = currentActionNode.action.goapName;
            if (currentActionNode.associatedJobType.IsCultistJob()) {
                jobName = $"{jobName} for {UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(currentActionNode.associatedJobType.ToString())}";
            }
            log.AddToFillers(null, jobName, LOG_IDENTIFIER.STRING_1);
            log.AddToFillers(null, reason, LOG_IDENTIFIER.STRING_2);
            logComponent.RegisterLog(log, true);
        }
        //if (actor.currentAction != null && actor.currentAction.parentPlan != null && actor.currentAction.parentPlan.job != null && actor.currentAction == this) {
        //    if (reason != "") {
        //        Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "current_action_abandoned_reason");
        //        log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        //        log.AddToFillers(null, actor.currentAction.goapName, LOG_IDENTIFIER.STRING_1);
        //        log.AddToFillers(null, reason, LOG_IDENTIFIER.STRING_2);
        //        actor.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
        //    }
        //}


        if (ReferenceEquals(marker, null) == false) {
            //This means that the actor currently travelling to another tile in tilemap
            marker.StopMovement();
        }

        //if (carryComponent.masterCharacter.avatar.isTravelling) {
        //    if (ReferenceEquals(carryComponent.masterCharacter.avatar.travelLine, null)) {
        //        //This means that the actor currently travelling to another tile in tilemap
        //        marker.StopMovement();
        //    } else {
        //        //This means that the actor is currently travelling to another npcSettlement
        //        carryComponent.masterCharacter.avatar.SetOnArriveAction(() => OnArriveAtAreaStopMovement());
        //    }
        //}
        //if (poiTarget.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
        //    Messenger.RemoveListener<TileObject, Character, LocationGridTile>(Signals.TILE_OBJECT_REMOVED, OnTileObjectRemoved);
        //    Messenger.RemoveListener<TileObject, Character>(Signals.TILE_OBJECT_DISABLED, OnTileObjectDisabled);
        //}

        //SetIsStopped(true);
        if (currentActionNode.avoidCombat) {
            if (marker) {
                marker.SetVisionColliderSize(CharacterManager.VISION_RANGE);
            }
        }
#if DEBUG_LOG
        logComponent.PrintLogIfActive($"Stopped action of {name} which is {currentActionNode.action.goapName} targetting {currentActionNode.poiTarget.name}!");
#endif
        currentActionNode.StopActionNode();

        SetCurrentActionNode(null, null, null);

        //Every time current node is stopped, drop carried poi
        if (carryComponent.IsNotBeingCarried()) {
            if (carryComponent.isCarryingAnyPOI) {
                // IPointOfInterest carriedPOI = carryComponent.carriedPOI;
                // string log = $"Dropping carried POI: {carriedPOI.name} because current action is stopped!";
                // log += "\nAdditional Info:";
                // if (carriedPOI is ResourcePile) {
                //     ResourcePile pile = carriedPOI as ResourcePile;
                //     log += $"\n-Stored resources on drop: {pile.resourceInPile} {pile.providedResource}";
                // } else if (carriedPOI is Table) {
                //     Table table = carriedPOI as Table;
                //     log += $"\n-Stored resources on drop: {table.food} Food.";
                // }
                //
                // logComponent.PrintLogIfActive(log);
                UncarryPOI();
            }
        }
        //JobQueueItem job = parentPlan.job;

        //Remove job in queue if job is personal job and removeJobInQueue value is true
        //if (removeJobInQueue && job != null && !job.jobQueueParent.isAreaOrQuestJobQueue) {
        //    job.jobQueueParent.RemoveJobInQueue(job);
        //}
        if (UIManager.Instance.characterInfoUI.isShowing) {
            UIManager.Instance.characterInfoUI.UpdateBasicInfo();
        }
        //Messenger.Broadcast<GoapAction>(Signals.STOP_ACTION, this);
        return true;
    }
    //public void SetHasAlreadyAskedForPlan(bool state) {
    //    _hasAlreadyAskedForPlan = state;
    //}
    //public void AddTargettedByAction(GoapAction action) {
    //    if (this != action.actor) { // && !isDead
    //        targettedByAction.Add(action);
    //        if (marker) {
    //            marker.OnCharacterTargettedByAction(action);
    //        }
    //    }
    //}
    //public void RemoveTargettedByAction(GoapAction action) {
    //    if (targettedByAction.Remove(action)) {
    //        if (marker) {
    //            marker.OnCharacterRemovedTargettedByAction(action);
    //        }
    //    }
    //}
    //This will only stop the current action of this character, this is different from StopAction because this will not drop the plan if the actor is not performing it but is on the way
    //This does not stop the movement of this character, call StopMovement separately to stop movement
    //public void StopCurrentAction(bool shouldDoAfterEffect = true, string reason = "") {
    //    if(currentActionNode != null) {
    //        //Debug.Log("Stopped action of " + name + " which is " + currentAction.goapName);
    //        if (currentActionNode.parentPlan != null && currentActionNode.parentPlan.job != null && reason != "") {
    //            Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "current_action_abandoned_reason");
    //            log.AddToFillers(this, name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
    //            log.AddToFillers(null, currentActionNode.goapName, LOG_IDENTIFIER.STRING_1);
    //            log.AddToFillers(null, reason, LOG_IDENTIFIER.STRING_2);
    //            RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
    //        }
    //        if (currentActionNode.isPerformingActualAction && !currentActionNode.isDone) {
    //            if (!shouldDoAfterEffect) {
    //                currentActionNode.OnStopActionDuringCurrentState();
    //            }
    //            if(currentActionNode.currentState != null) {
    //                currentActionNode.SetIsStoppedAsCurrentAction(true);
    //                currentActionNode.currentState.EndPerTickEffect(shouldDoAfterEffect);
    //            } else {
    //                SetCurrentActionNode(null);
    //            }
    //        } else {
    //            currentActionNode.OnStopActionWhileTravelling();
    //            SetCurrentActionNode(null);
    //        }
    //    }
    //}
    private void OnStartPerformGoapAction(ActualGoapNode node, ref bool willStillContinueAction) {
        bool stillContinueCurrentAction = true;

        List<Trait> traitOverrideFunctions = traitContainer.GetTraitOverrideFunctions(TraitManager.Start_Perform_Trait);
        if (traitOverrideFunctions != null) {
            for (int i = 0; i < traitOverrideFunctions.Count; i++) {
                Trait trait = traitOverrideFunctions[i];
                if (trait.OnStartPerformGoapAction(node, ref stillContinueCurrentAction)) {
                    willStillContinueAction = stillContinueCurrentAction;
                    break;
                } else {
                    stillContinueCurrentAction = true;
                }
            }
        }
    }
    protected void OnCharacterFinishedActionSuccessfully(ActualGoapNode p_action) {
        moneyComponent.GainCoinsAfterDoingAction(p_action);
    }
    protected void OnCharacterFinishedJobSuccessfully(JobQueueItem p_job) {
        needsComponent.OnCharacterFinishedJob(p_job);
        behaviourComponent.OnCharacterFinishedJob(p_job);
        traitComponent.OnCharacterFinishedJob(p_job);
        moneyComponent.GainCoinsAfterDoingJob(p_job);
    }

    private void HeardAScream(Character characterThatScreamed) {
        if (!limiterComponent.canPerform || !limiterComponent.canWitness) {
            //Do not react to scream if character has disabler trait
            return;
        }
        if (currentRegion != characterThatScreamed.currentRegion) {
            return;
        }
        if (gridTileLocation != null && characterThatScreamed.gridTileLocation != null) {
            float dist = gridTileLocation.GetDistanceTo(characterThatScreamed.gridTileLocation);
#if DEBUG_LOG
            logComponent.PrintLogIfActive($"{name} distance to {characterThatScreamed.name} is {dist}");
#endif
            float distanceChecker = 10f;
            //if (currentStructure != characterThatScreamed.currentStructure) {
            //    distanceChecker = 2f;
            //}
            if (dist > distanceChecker) {
                //Do not react to scream if character is too far
                return;
            }
        }
        if (jobQueue.HasJob(JOB_TYPE.GO_TO, characterThatScreamed)) {
            //Do not react if character will already react to a scream;
            return;
        }
        if (!CanCharacterReact(characterThatScreamed)) {
            return;
        }
        ReactToScream(characterThatScreamed);
    }
    private void ReactToScream(Character characterThatScreamed) {
        //If you are wondering why the job is not just simply added to the job queue and let the job queue processing work if the job can override current job, is because
        //in this situation, we want the job not to be added to the job queue if it cannot override the current job
        //If we let the AddJobInQueue simply process the job, it will still be added regardless if it cannot override the current job, it means that it will just be pushed back in queue and will be done by the character when the time comes
        //We don't want that because we want to have a spontaneous reaction from this character, so the only way that the character will react is if he can do it immediately
        string log = string.Empty;
#if DEBUG_LOG
        log = $"{name} heard the scream of {characterThatScreamed.name}, reacting...";
#endif

        bool canReact = true;
        int reactJobPriority = JOB_TYPE.GO_TO.GetJobTypePriority();
        if (stateComponent.currentState != null && stateComponent.currentState.job != null && stateComponent.currentState.job.priority >= reactJobPriority) {
            canReact = false;
        }
        //else if (stateComponent.stateToDo != null && stateComponent.stateToDo.job != null && stateComponent.stateToDo.job.priority <= reactJobPriority) {
        //    canReact = false;
        //} 
        else if (currentJob != null && currentJob.priority >= reactJobPriority) {
            canReact = false;
        }
        if (canReact) {
            jobComponent.CreateGoToJob(characterThatScreamed);
            //GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.GO_TO, INTERACTION_TYPE.GO_TO, characterThatScreamed, this);
            //jobQueue.AddJobInQueue(job);
            //if (CanCurrentJobBeOverriddenByJob(job)) {
            //    jobQueue.AddJobInQueue(job, false);
            //    jobQueue.CurrentTopPriorityIsPushedBackBy(job, this);
            //    log += "\n" + name + " will go to " + characterThatScreamed.name;
            //} else {
            //    log += "\n" + name + " cannot react because there is still something important that he/she will do.";
            //}
        } else {
#if DEBUG_LOG
            log += $"\n{name} cannot react because there is still something important that he/she will do.";
#endif
        }
#if DEBUG_LOG
        logComponent.PrintLogIfActive(log);
#endif
    }

    //Can only be seized if poi has tile location
    public virtual void OnSeizePOI() {
        if (UIManager.Instance.characterInfoUI.isShowing && UIManager.Instance.characterInfoUI.activeCharacter == this) {
            UIManager.Instance.characterInfoUI.CloseMenu();
        } else if (UIManager.Instance.monsterInfoUI.isShowing && UIManager.Instance.monsterInfoUI.activeMonster == this) {
            UIManager.Instance.monsterInfoUI.CloseMenu();
        }

        if (trapStructure.IsTrapped()) {
            trapStructure.ResetAllTrapStructures();
        }
        if (trapStructure.IsTrappedInArea()) {
            trapStructure.ResetTrapArea();
        }
        //if(partyComponent.hasParty && partyComponent.currentParty.partyType != PARTY_QUEST_TYPE.Counterattack) {
        //    //Once a character is seized, leave party also - except counter attack
        //    partyComponent.currentParty.RemoveMember(this);
        //}
        RevertFromVampireBatForm();
        RevertFromWerewolfForm();

        minion?.OnSeize();
        Messenger.Broadcast(CharacterSignals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, this as IPointOfInterest, "");
        Messenger.Broadcast(CharacterSignals.FORCE_CANCEL_ALL_ACTIONS_TARGETING_POI, this as IPointOfInterest, "");
        //ForceCancelAllJobsTargettingThisCharacter();
        //marker.ClearTerrifyingObjects();
        needsComponent.OnCharacterLeftLocation(currentRegion);

        jobQueue.CancelAllJobs();
        interruptComponent.OnSeizedOwner();
        tileObjectLocation?.RemoveUser(this);

        UnsubscribeSignals();
        SetIsConversing(false);
        SetPOIState(POI_STATE.INACTIVE);
        SchedulingManager.Instance.ClearAllSchedulesBy(this);

        partyComponent.UnfollowBeacon();
        if (hasMarker) {
            marker.StopMovement();
            //DestroyMarker();
            //marker.collisionTrigger.SetCollidersState(false);
            marker.OnSeize();
            DisableMarker();
            Messenger.Broadcast(JobSignals.CHECK_APPLICABILITY_OF_ALL_JOBS_TARGETING, this as IPointOfInterest);
        }
        Messenger.AddListener(Signals.TICK_STARTED, OnTickStartedWhileSeizedOrIsInLimbo);
        //List<Trait> traitOverrideFunctions = traitContainer.GetTraitOverrideFunctions(TraitManager.Destroy_Map_Visual_Trait);
        //if (traitOverrideFunctions != null) {
        //    for (int i = 0; i < traitOverrideFunctions.Count; i++) {
        //        Trait trait = traitOverrideFunctions[i];
        //        trait.OnDestroyMapObjectVisual(this);
        //    }
        //}
        //Messenger.Broadcast(Signals.ON_SEIZE_CHARACTER, this);
    }
    public virtual void OnUnseizePOI(LocationGridTile tileLocation) {
        Messenger.RemoveListener(Signals.TICK_STARTED, OnTickStartedWhileSeizedOrIsInLimbo);
        needsComponent.OnCharacterArrivedAtLocation(tileLocation.structure.region);
        if (minion == null) {
            if (!isDead) {
                SubscribeToSignals();
            }
        }
        SetPOIState(POI_STATE.ACTIVE);
        if (!marker) {
            CreateMarker();
        } else {
            marker.SetCharacter(this);
        }
        //Note: This is added because, there is a bug wherein after unseizing the character, it does not animate anymore, the reason is that while character is being seized, the "UpdateAnimationSpeed" function is not called
        //Refer to this: https://trello.com/c/oFbPZlmT/3521-character-not-animating-after-seizing
        marker.UpdatePauseAnimationSpeed();
        //marker.SetAllColliderStates(true);
        EnableMarker();
        marker.OnUnseize();
        minion?.OnUnseize();
        if (tileLocation.structure.region != currentRegion) {
            currentRegion.RemoveCharacterFromLocation(this);
        }
        marker.InitialPlaceMarkerAt(tileLocation);
        tileLocation.structure.OnCharacterUnSeizedHere(this);
        needsComponent.CheckExtremeNeeds();
        if (isDead) {
            Messenger.Broadcast(JobSignals.CHECK_JOB_APPLICABILITY, JOB_TYPE.BURY, this as IPointOfInterest);
            jobComponent.TriggerBuryMe();
        }

        if (traitContainer.HasTrait("Berserked")) {
            if (marker) {
                marker.BerserkedMarker();
            }
        }
        if (isNormalCharacter && !traitContainer.HasTrait("Burning")) {
            if (tileLocation.tileObjectComponent.genericTileObject.traitContainer.HasTrait("Burning")) {
                Burning burningSource = tileLocation.tileObjectComponent.genericTileObject.traitContainer.GetTraitOrStatus<Burning>("Burning");
                traitContainer.AddTrait(this, "Burning", bypassElementalChance: true);
                Burning burning = traitContainer.GetTraitOrStatus<Burning>("Burning");
                burning?.SetIsPlayerSource(burningSource.isPlayerSource);
            } else if (tileLocation.tileObjectComponent.objHere != null && tileLocation.tileObjectComponent.objHere.traitContainer.HasTrait("Burning")) {
                Burning burningSource = tileLocation.tileObjectComponent.objHere.traitContainer.GetTraitOrStatus<Burning>("Burning");
                traitContainer.AddTrait(this, "Burning", bypassElementalChance: true);
                Burning burning = traitContainer.GetTraitOrStatus<Burning>("Burning");
                burning?.SetIsPlayerSource(burningSource.isPlayerSource);
            }
        }

        //Every time a character is unseized on a non demonic structure, if the character is a prisoner of player faction, remove prisoner trait.
        //Reason: so that if the character becomes a snatch target again, the snatch behaviour will also treat the target as a new snatch target
        if (!tileLocation.structure.structureType.IsPlayerStructure()) {
            Prisoner prisoner = traitContainer.GetTraitOrStatus<Prisoner>("Prisoner");
            if (prisoner != null && prisoner.IsFactionPrisonerOf(PlayerManager.Instance.player.playerFaction)) {
                traitContainer.RemoveRestrainAndImprison(this);
            }
        }
        //List<Trait> traitOverrideFunctions = traitContainer.GetTraitOverrideFunctions(TraitManager.Initiate_Map_Visual_Trait);
        //if (traitOverrideFunctions != null) {
        //    for (int i = 0; i < traitOverrideFunctions.Count; i++) {
        //        Trait trait = traitOverrideFunctions[i];
        //        trait.OnInitiateMapObjectVisual(this);
        //    }
        //}
        //Messenger.Broadcast(Signals.ON_UNSEIZE_CHARACTER, this);
    }
#endregion

#region Hostility
    /// <summary>
    /// Function to encapsulate, whether or not this character treats another character as hostile.
    /// </summary>
    /// <param name="otherCharacter">Character in question.</param>
    public bool IsHostileWith(Character otherCharacter) {
        //Removed this checker because when checking hostility there is no need to check if either character is dead anymore, the only thing that matters is the faction
        //if (character.isDead || isDead) {
        //    return false;
        //}

        if (faction != null && faction.isPlayerFaction) {
            //if this character is part of the player faction and the other character is allied with the player, then do not consider as hostile
            if (otherCharacter.isAlliedWithPlayer) {
                return false;
            }
        }

        if (isAlliedWithPlayer) {
            //if this character is allied with the player and the other character is part of the player faction, then do not consider as hostile
            if (otherCharacter.faction != null && otherCharacter.faction.isPlayerFaction) {
                return false;
            }
        }

        if (traitContainer.HasTrait("Cultist") && otherCharacter.traitContainer.HasTrait("Cultist")) {
            //if both characters are cultists, do not be hostile with each other, NOTE: Did not use allied with player because if both characters are friendly with the player faction
            //but are hostile with each other, they should still be hostile with each other
            return false;
        }

        if (faction == null || otherCharacter.faction == null) {
            //if either character does not have a faction, do not consider them as hostile
            //This should almost never happen since we expect that all characters should have a faction.
            return false;
        }
        if (((race == RACE.RATMAN || faction.factionType.type == FACTION_TYPE.Ratmen) && otherCharacter.race == RACE.RAT)
            || (race == RACE.RAT && (otherCharacter.race == RACE.RATMAN || otherCharacter.faction.factionType.type == FACTION_TYPE.Ratmen))) {
            //Ratmen does not consider rats as hostile and vice versa
            return false;
        }
        //if (isInVampireBatForm) {
        //    Vampire vampireTrait = traitContainer.GetTraitOrStatus<Vampire>("Vampire");
        //    if (!vampireTrait.DoesCharacterKnowThisVampire(otherCharacter)) {
        //        return true;
        //    }
        //} else if (isInWerewolfForm) {
        //    if (!lycanData.DoesCharacterKnowThisLycan(otherCharacter)) {
        //        return true;
        //    }
        //}
        bool isUndead = faction?.factionType.type == FACTION_TYPE.Undead;
        if (!isUndead) {
            if (otherCharacter.isInVampireBatForm) {
                Vampire vampireTrait = otherCharacter.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
                if (!vampireTrait.DoesCharacterKnowThisVampire(this)) {
                    return true;
                }
            } else if (otherCharacter.isInWerewolfForm) {
                if (!otherCharacter.lycanData.DoesCharacterKnowThisLycan(this)) {
                    return true;
                }
            }
        }

        if (faction != otherCharacter.faction) {
            if (faction != null && faction.isMajorOrVagrant && otherCharacter.traitContainer.HasTrait("Transitioning") && otherCharacter.faction?.factionType.type != FACTION_TYPE.Wild_Monsters) {
                //Non transitioning characters will not attack transitioning characters as long as the transitioning character is not from monster faction, if they are, non transitioning characters will still attack
                return false;
            }
            if (otherCharacter.faction != null && otherCharacter.faction.isMajorOrVagrant && traitContainer.HasTrait("Transitioning")) {
                //Transitioning characters will not attack vagrant characters or characters that are in a hostile major faction, but will still attack character from monster/player/undead/etc faction
                return false;
            }
        }

        return faction.IsHostileWith(otherCharacter.faction);

        // if (isFactionless || otherCharacter.isFactionless) {
        //     //this character is unaligned
        //     //if unaligned, hostile to all other characters, except those of same race
        //     return otherCharacter.race != race;
        // } else {
        //     return faction.IsHostileWith(otherCharacter.faction);
        // }
    }
    public bool IsLycanHostileWith(Character targetCharacter) {
        if ((isLycanthrope && targetCharacter.race == RACE.WOLF) || (targetCharacter.isLycanthrope && race == RACE.WOLF)) {
            return false;
        }
        if (this.race == RACE.WOLF && targetCharacter.faction != null && targetCharacter.faction.factionType.HasIdeology(FACTION_IDEOLOGY.Reveres_Werewolves)) {
            //If this character is a wolf and the other character is part of a faction that reveres werewolves, then do not attack that character
            //Reference: https://trello.com/c/NPXg3GZs/2828-restrained-wolves-should-be-freed-by-reveres-werewolves-faction-members
            return false;
        }
        if (targetCharacter.race == RACE.WOLF || (targetCharacter.isLycanthrope && targetCharacter.lycanData.isInWerewolfForm)) {
            if (faction != null && faction.factionType.HasIdeology(FACTION_IDEOLOGY.Reveres_Werewolves)) {
                //Reveres Werewolf factions will not initiate combat with Wolves and Werewolves
                return false;
            }
        }
        return true;
    }
#endregion

#region Pathfinding
    public void PopulateTilesInRadius(List<LocationGridTile> tiles, int radius, int radiusLimit = 0, bool includeCenterTile = false, bool includeTilesInDifferentStructure = false) {
        if (currentRegion == null) { return; }
        int mapSizeX = currentRegion.innerMap.map.GetUpperBound(0);
        int mapSizeY = currentRegion.innerMap.map.GetUpperBound(1);
        int x = gridTileLocation.localPlace.x;
        int y = gridTileLocation.localPlace.y;
        if (includeCenterTile) {
            tiles.Add(gridTileLocation);
        }
        int xLimitLower = x - radiusLimit;
        int xLimitUpper = x + radiusLimit;
        int yLimitLower = y - radiusLimit;
        int yLimitUpper = y + radiusLimit;


        for (int dx = x - radius; dx <= x + radius; dx++) {
            for (int dy = y - radius; dy <= y + radius; dy++) {
                if (dx >= 0 && dx <= mapSizeX && dy >= 0 && dy <= mapSizeY) {
                    if (dx == x && dy == y) {
                        continue;
                    }
                    if (radiusLimit > 0 && dx > xLimitLower && dx < xLimitUpper && dy > yLimitLower && dy < yLimitUpper) {
                        continue;
                    }
                    LocationGridTile result = currentRegion.innerMap.map[dx, dy];
                    if (!includeTilesInDifferentStructure && result.structure != gridTileLocation.structure) { continue; }
                    tiles.Add(result);
                }
            }
        }
    }
#endregion

#region Player Alliance
    /// <summary>
    /// Get whether this character is allied with the player outside the faction system.
    /// i.e. when we want that character to be considered as an ally to the player, but don't want to
    /// change his/her faction to prevent other villagers from attacking him or her.
    /// </summary>
    /// <param name="state">Should this character be allied with the player.</param>
    private bool IsAlliedWithPlayer() {
        if (traitContainer.HasTrait("Cultist")) {
            return true;
        }
        if (faction != null) {
            if (faction.isPlayerFaction) {
                return true;
            }
            Faction playerFaction = null;
            if (PlayerManager.Instance != null && PlayerManager.Instance.player != null) {
                playerFaction = PlayerManager.Instance.player.playerFaction;
            }
            if (playerFaction != null) {
                if (faction.IsFriendlyWith(playerFaction)) {
                    return true;
                }
            }
        }
        return false;
    }
    private bool IsNotHostileWithPlayer() {
        if (traitContainer.HasTrait("Cultist")) {
            return true;
        }
        if (faction != null) {
            if (faction.isPlayerFaction) {
                return true;
            }
            Faction playerFaction = null;
            if (PlayerManager.Instance != null && PlayerManager.Instance.player != null) {
                playerFaction = PlayerManager.Instance.player.playerFaction;
            }
            if (playerFaction != null) {
                if (faction == playerFaction || !faction.IsHostileWith(playerFaction)) {
                    return true;
                }
            }
        }
        return false;
    }
#endregion

#region IJobOwner
    public virtual void OnJobAddedToCharacterJobQueue(JobQueueItem job, Character character) { }
    public virtual void OnJobRemovedFromCharacterJobQueue(JobQueueItem job, Character character) {
        //if(this == character && job == jobComponent.finalJobAssignment) {
        //    jobComponent.SetFinalJobAssignment(null);
        //    Messenger.AddListener(Signals.TICK_STARTED, DissipateAfterFinalJobAssignment);
        //}

        //Only return to job pool if the job is personal, because if not, the control should be in the owner
        if (job.originalOwner == this) {
            JobManager.Instance.OnFinishJob(job);
        }
    }
    //private void DissipateAfterFinalJobAssignment() {
    //    Messenger.RemoveListener(Signals.TICK_STARTED, DissipateAfterFinalJobAssignment);
    //    LocationGridTile deathTile = gridTileLocation;
    //    Death();
    //    if (deathTile != null && this is Summon) {
    //        GameManager.Instance.CreateParticleEffectAt(deathTile, PARTICLE_EFFECT.Minion_Dissipate);
    //    }
    //}
    public bool ForceCancelJob(JobQueueItem job) {
        //JobManager.Instance.OnFinishGoapPlanJob(job);
        return true;
    }
    public void AddForcedCancelJobsOnTickEnded(JobQueueItem job) {
        if (!forcedCancelJobsOnTickEnded.Contains(job)) {
            forcedCancelJobsOnTickEnded.Add(job);
        }
    }
    public bool WillCancelJobOnTickEnded(JobQueueItem job) {
        return forcedCancelJobsOnTickEnded.Contains(job);
    }
    public void ProcessForcedCancelJobsOnTickEnded() {
        if (forcedCancelJobsOnTickEnded.Count > 0) {
            for (int i = 0; i < forcedCancelJobsOnTickEnded.Count; i++) {
                forcedCancelJobsOnTickEnded[i].ForceCancelJob();
            }
            forcedCancelJobsOnTickEnded.Clear();
        }
    }
    public void ForceCancelJobTypesTargetingPOI(JOB_TYPE jobType, IPointOfInterest target) {
        //NA
    }
#endregion

#region IDamageable
    public bool CanBeDamaged() {
        return traitContainer.HasTrait("Indestructible") == false;
    }
#endregion

#region Lycanthropy
    //NOTE: This might a bad practice since we have a special case here for lycanthrope, but I see no other way to easily know if the character is a lycan or not
    //This way we can easily know and access the lycan data
    public void SetLycanthropeData(LycanthropeData data) {
        lycanData = data;
    }
    public void SetIsInWerewolfForm(bool state) {
        if (isLycanthrope) {
            lycanData.SetIsInWerewolfForm(state);
        }
    }
    public void TransformToWerewolfForm() {
        if (isLycanthrope && !lycanData.isInWerewolfForm) {
            lycanData.SetIsInWerewolfForm(true);
            classComponent.AssignClass("Werewolf");
            if (visuals != null) {
                visuals.UpdateAllVisuals(this);
            }
        }
    }
    public void RevertFromWerewolfForm() {
        if (isLycanthrope && lycanData.isInWerewolfForm) {
            lycanData.SetIsInWerewolfForm(false);
            if (classComponent.previousClassName != "Werewolf") {
                //Reverting back from werewolf form should mean that the class to be assigned must not be werewolf
                classComponent.AssignClass(classComponent.previousClassName);
            }
            if (visuals != null) {
                visuals.UpdateAllVisuals(this);
            }
        }
    }
#endregion

#region Player Action Target
    public List<PLAYER_SKILL_TYPE> actions { get; protected set; }
    //public List<string> overrideThoughts {
    //    get { return _overrideThoughts; }
    //}
    public virtual void ConstructDefaultActions() {
        if (actions == null) {
            actions = new List<PLAYER_SKILL_TYPE>();
        } else {
            actions.Clear();
        }

        if (race == RACE.DEMON) {
            //AddPlayerAction(PLAYER_SKILL_TYPE.UNSUMMON);
        } else {
            if (isNormalCharacter) {
                AddPlayerAction(PLAYER_SKILL_TYPE.AFFLICT);
                AddPlayerAction(PLAYER_SKILL_TYPE.ZAP);
                AddPlayerAction(PLAYER_SKILL_TYPE.TRIGGER_FLAW);
                AddPlayerAction(PLAYER_SKILL_TYPE.RAISE_DEAD);
            }
            AddPlayerAction(PLAYER_SKILL_TYPE.SEIZE_CHARACTER);
            // AddPlayerAction(PLAYER_SKILL_TYPE.SNATCH);
            AddPlayerAction(PLAYER_SKILL_TYPE.SCHEME);
            AddPlayerAction(PLAYER_SKILL_TYPE.TORTURE);
            AddPlayerAction(PLAYER_SKILL_TYPE.BRAINWASH);
            AddPlayerAction(PLAYER_SKILL_TYPE.EXPEL);
        }
        AddPlayerAction(PLAYER_SKILL_TYPE.RELEASE);
        AddPlayerAction(PLAYER_SKILL_TYPE.HEAL);
        AddPlayerAction(PLAYER_SKILL_TYPE.REMOVE_BUFF);
        AddPlayerAction(PLAYER_SKILL_TYPE.REMOVE_FLAW);
        AddPlayerAction(PLAYER_SKILL_TYPE.CULTIST_JOIN_FACTION);
        AddPlayerAction(PLAYER_SKILL_TYPE.DRAIN_SPIRIT);
        AddPlayerAction(PLAYER_SKILL_TYPE.LET_GO);
        AddPlayerAction(PLAYER_SKILL_TYPE.FULL_HEAL);
        AddPlayerAction(PLAYER_SKILL_TYPE.CREATE_BLACKMAIL);
        AddPlayerAction(PLAYER_SKILL_TYPE.EMPOWER);
    }
    public void AddPlayerAction(PLAYER_SKILL_TYPE action) {
        if (actions.Contains(action) == false) {
            actions.Add(action);
            Messenger.Broadcast(PlayerSkillSignals.PLAYER_ACTION_ADDED_TO_TARGET, action, this as IPlayerActionTarget);
        }
    }
    public void RemovePlayerAction(PLAYER_SKILL_TYPE action) {
        if (actions.Remove(action)) {
            Messenger.Broadcast(PlayerSkillSignals.PLAYER_ACTION_REMOVED_FROM_TARGET, action, this as IPlayerActionTarget);
        }
    }
    public void ClearPlayerActions() {
        actions.Clear();
    }
#endregion

#region Selectable
    public virtual bool IsCurrentlySelected() {
        Character characterToSelect = this;
        if (isLycanthrope) {
            characterToSelect = lycanData.activeForm;
        }
        if (characterToSelect.isNormalCharacter) {
            return UIManager.Instance.characterInfoUI.isShowing &&
                   UIManager.Instance.characterInfoUI.activeCharacter == characterToSelect;
        } else {
            return UIManager.Instance.monsterInfoUI.isShowing &&
                   UIManager.Instance.monsterInfoUI.activeMonster == characterToSelect;
        }

    }
    public void LeftSelectAction() {
        if (mapObjectVisual != null) {
            mapObjectVisual.ExecuteClickAction(PointerEventData.InputButton.Left);
        } else {
            UIManager.Instance.ShowCharacterInfo(this, true);
        }
    }
    public void RightSelectAction() {
        mapObjectVisual.ExecuteClickAction(PointerEventData.InputButton.Right);
    }
    public void MiddleSelectAction() {
        mapObjectVisual.ExecuteClickAction(PointerEventData.InputButton.Middle);
    }
    public bool CanBeSelected() {
        if (hasMarker && marker.IsShowingVisuals() == false) {
            return false;
        }
        return true;
    }
#endregion

#region Territorries
    public void SetTerritory([NotNull] Area p_area, bool returnHome = true) {
        if (territory != p_area) {
            territory = p_area;
            if (territory.region != homeRegion) {
                if (homeRegion != null) {
                    homeRegion.RemoveResident(this);
                }
                territory.region.AddResident(this);
            }
            if (homeStructure != null && homeStructure.hasBeenDestroyed) {
                MigrateHomeStructureTo(null, affectSettlement: false);
            }
            if (returnHome) {
                jobComponent.PlanReturnHome(JOB_TYPE.RETURN_HOME_URGENT);
            }
        }
    }
    public void ClearTerritory() {
        //QUESTION: Should a character be removed as region resident if it does not have a territory, home structure, home settlement there?
        territory = null;
    }
    public bool HasTerritory() {
        return territory != null;
    }
    public bool IsTerritory(Area p_area) {
        if (HasTerritory()) {
            return territory == p_area;
        }
        return false;
    }
    public bool IsInTerritory() {
        Area area = areaLocation;
        return area != null && IsTerritory(area);
    }
    public bool IsInTerritoryOf(Character character) {
        Area area = areaLocation;
        return area != null && character.IsTerritory(area);
    }
    public LocationGridTile GetRandomLocationGridTileWithPath() {
        LocationGridTile chosenTile = null;
        if (HasTerritory()) {
            //while (chosenTile == null) {
            LocationGridTile chosenGridTile = territory.gridTileComponent.gridTiles[UnityEngine.Random.Range(0, territory.gridTileComponent.gridTiles.Count)];
            if (movementComponent.HasPathToEvenIfDiffRegion(chosenGridTile)) {
                chosenTile = chosenGridTile;
            }
            //}
        }
        return chosenTile;
    }
#endregion

#region Death
    //Changes character's side
    //public void SetSide(SIDES side) {
    //    this._currentSide = side;
    //}
    //Character's death
    public void SetIsDead(bool isDead) {
        if (_isDead != isDead) {
            _isDead = isDead;
            //if (_isDead) {
            //    if (race == RACE.HUMANS || race == RACE.ELVES) {
            //        //PlayerAction raiseAction = new PlayerAction(PlayerDB.Raise_Skeleton_Action
            //        //    , () => PlayerManager.Instance.allSpellsData[SPELL_TYPE.RAISE_DEAD].CanPerformAbilityTowards(this)
            //        //    , null
            //        //    , () => PlayerManager.Instance.allSpellsData[SPELL_TYPE.RAISE_DEAD].ActivateAbility(this));
            //        AddPlayerAction(SPELL_TYPE.RAISE_DEAD);
            //    }
            //} else {
            //    RemovePlayerAction(SPELL_TYPE.RAISE_DEAD);
            //}
        }
    }
    public void ReturnToLife(Faction faction, RACE race, string className) {
        if (_isDead) {
            //SetRaisedFromDeadAsSkeleton(true);
            AssignRace(race);
            classComponent.AssignClass(className);

            ReturnToLife();

            //Change faction should be called last so that the character's nameplate in the Faction UI is updated when he transfers faction
            ChangeFactionTo(faction);
        }
    }
    public bool ReturnToLife() {
        if (_isDead) {
            SetIsDead(false);
            SubscribeToSignals();
            ResetToFullHP();
            SetPOIState(POI_STATE.ACTIVE);
            needsComponent.ResetFullnessMeter();
            needsComponent.ResetTirednessMeter();
            needsComponent.ResetHappinessMeter();
            marker.OnReturnToLife();
            if (grave != null) {
                Tombstone tombstone = grave;
                grave.gridTileLocation.structure.RemovePOI(grave);
                SetGrave(null);
                marker.PlaceMarkerAt(tombstone.previousTile);
            }
            traitContainer.RemoveTrait(this, "Dead");
            // needsComponent.SetTirednessForcedTick();
            // needsComponent.SetFullnessForcedTick();
            // needsComponent.SetHappinessForcedTick();
            // needsComponent.SetHasCancelledSleepSchedule(false);
            // needsComponent.ResetSleepTicks();
            visuals.UpdateAllVisuals(this);
            ConstructDefaultActions();
            Messenger.Broadcast(CharacterSignals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, this as IPointOfInterest, "");
            Messenger.Broadcast(CharacterSignals.FORCE_CANCEL_ALL_ACTIONS_TARGETING_POI, this as IPointOfInterest, "");
            Messenger.Broadcast(CharacterSignals.CHARACTER_RETURNED_TO_LIFE, this);
            return true;
        }
        return false;
    }

    public void ResetNeeds() {
        needsComponent.ResetFullnessMeter();
        needsComponent.ResetTirednessMeter();
        needsComponent.ResetHappinessMeter();
    }

    public virtual void Death(string cause = "normal", ActualGoapNode deathFromAction = null, Character responsibleCharacter = null, Log _deathLog = null, LogFillerStruct[] deathLogFillers = null, Interrupt interrupt = null, bool isPlayerSource = false) {
        if (minion != null) {
            minion.Death(cause, deathFromAction, responsibleCharacter, _deathLog, deathLogFillers);
            return;
        }
        if (!_isDead) {
            //if (currentAlterEgoName != CharacterManager.Original_Alter_Ego) {
            //    SwitchAlterEgo(CharacterManager.Original_Alter_Ego); //revert the character to his/her original alter ego
            //}

            //Unseize first before processing death
            if (isBeingSeized) {
                PlayerManager.Instance.player.seizeComponent.UnseizePOIOnDeath();
            }
            SetDeathLocation(gridTileLocation);

            SetIsConversing(false);
            //SetIsFlirting(false);
            Region deathLocation = currentRegion;
            LocationStructure deathStructure = currentStructure;

            if (isPlayerSource) {
                PlayerManager.Instance?.player?.retaliationComponent.CharacterDeathRetaliation(this);
            }

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
            if (isLycanthrope) {
                lycanData.LycanDies(this, cause, deathFromAction, responsibleCharacter, _deathLog, deathLogFillers);
            }
            //------------------------ Things that are above this line are called before letting the character die so that if we need things done before actually setting the death of character we can do it here like cleaning up necessary things, etc.
            SetIsDead(true);
            if (isLimboCharacter && isInLimbo) {
                //If a limbo character dies while in limbo, that character should not process death, instead he/she will be removed from the list
                CharacterManager.Instance.RemoveLimboCharacter(this);
                return;
            }

            //Remove disguise first before processing death
            reactionComponent.SetDisguisedCharacter(null);

            UnsubscribeSignals();
            SetPOIState(POI_STATE.INACTIVE);
            ProcessBeforeDeath(cause, responsibleCharacter);
            traitContainer.RemoveTrait(this, "Necromancer"); //Necromancer trait must be removed when the necromancer dies so that another necromancer can take its place
            if (currentRegion == null) {
                throw new Exception(
                    $"Current Region Location of {name} is null! Please use command /l_character_location_history [Character Name/ID] in console menu to log character's location history. (Use '~' to show console menu)");
            }
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
            //StopCurrentActionNode();
            //ForceCancelAllJobsTargettingCharacter(false, "target is already dead");
            ////Messenger.Broadcast(Signals.CANCEL_CURRENT_ACTION, this, "target is already dead");
            //if (jobQueue.jobsInQueue.Count > 0) {
            //    jobQueue.CancelAllJobs();
            //}
            Messenger.Broadcast(CharacterSignals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, this as IPointOfInterest, GoapPlanJob.Target_Already_Dead_Reason);
            Messenger.Broadcast(CharacterSignals.FORCE_CANCEL_ALL_ACTIONS_TARGETING_POI, this as IPointOfInterest, GoapPlanJob.Target_Already_Dead_Reason);

            behaviourComponent.OnDeath();
            jobQueue.CancelAllJobs();

            DropAllItems(deathTilePosition);
            UnownOrTransferOwnershipOfAllItems();

            reactionComponent.SetIsHidden(false);

            //if (currentSettlement != null && isHoldingItem) {
            //    DropAllItems(deathTile);
            //} else {
            //    for (int i = 0; i < items.Count; i++) {
            //        if (RemoveItem(i)) {
            //            i--;
            //        }
            //    }
            //}
            //if (currentRegion == null) {
            //    if (currentArea != null && isHoldingItem) {
            //        DropAllTokens(currentArea, currentStructure, deathTile, true);
            //    } else {
            //        for (int i = 0; i < items.Count; i++) {
            //            if (RemoveToken(i)) {
            //                i--;
            //            }
            //        }
            //    }
            //} else {
            //    List<SpecialToken> all = new List<SpecialToken>(items);
            //    for (int i = 0; i < all.Count; i++) {
            //        RemoveToken(all[i]);
            //    }
            //}


            //clear traits that need to be removed
            traitsNeededToBeRemoved.Clear();

            bool wasOutsideSettlement = currentSettlement == null;

            //bool wasOutsideSettlement = false;
            //if (currentRegion != null) {
            //    wasOutsideSettlement = true;
            //    currentRegion.RemoveCharacterFromLocation(this);
            //}
            UncarryPOI();
            Character carrier = isBeingCarriedBy;
            if (carrier != null) {
                carrier.UncarryPOI(this);
            }
            //avatar.gameObject.SetActive(false);

            //No longer remove from region list even if character died to prevent inconsistency in data because if a dead character is picked up and dropped, he will be added in the structure location list again but wont be in region list
            //https://trello.com/c/WTiGxjrK/1786-inconsistent-characters-at-location-list-in-region-with-characters-at-structure
            //currentRegion?.RemoveCharacterFromLocation(this);
            //SetRegionLocation(deathLocation); //set the specific location of this party, to the location it died at
            //SetCurrentStructureLocation(deathStructure, false);

            //if (this.race != RACE.SKELETON) {
            //    deathLocation.AddCorpse(this, deathStructure, deathTile);
            //}

            //if (faction != null) {
            //    faction.LeaveFaction(this); //remove this character from it's factions list of characters
            //}
            //if (faction != null && faction.leader == this) {
            //    faction.SetNewLeader();
            //}

            // if (_role != null) {
            //     _role.OnDeath(this);
            // }
            marker?.OnDeath(deathTilePosition);

            if (destroyMarkerOnDeath) {
                //If death is destroy marker, this will leave no corpse, so remove it from the list of characters at location in region
                if (currentRegion != null) {
                    currentRegion.RemoveCharacterFromLocation(this);
                }
            }
            previousCharacterDataComponent.SetHomeSettlementOnDeath(homeSettlement);
            if (homeRegion != null) {
                Region home = homeRegion;
                LocationStructure homeStructure = this.homeStructure;
                homeRegion.RemoveResident(this);
                MigrateHomeStructureTo(null, addToRegionResidents: false);
                SetHomeRegion(home); //keep this data with character to prevent errors
                //SetHomeStructure(homeStructure); //keep this data with character to prevent errors
            }
            //if (homeSettlement != null) {
            //    homeSettlement.jobPriorityComponent.UnassignResidentToPrimaryJob(this);
            //}
            //if (homeNpcSettlement != null) {
            //    NPCSettlement home = homeNpcSettlement;
            //    Dwelling homeStructure = this.homeStructure;
            //    homeNpcSettlement.RemoveResident(this);
            //    SetHome(home); //keep this data with character to prevent errors
            //    SetHomeStructure(homeStructure); //keep this data with character to prevent errors
            //}

            //List<Character> characterRels = new List<Character>(this.relationships.Keys.ToList());
            //for (int i = 0; i < characterRels.Count; i++) {
            //    RemoveRelationship(characterRels[i]);
            //}

            //if (_minion != null) {
            //    PlayerManager.Instance.player.RemoveMinion(_minion);
            //}

            //ObjectPoolManager.Instance.DestroyObject(marker.gameObject);
            //deathTile.RemoveCharacterHere(this);

            //RemoveAllTraitsByType(TRAIT_TYPE.CRIMINAL); //remove all criminal type traits

            //RemoveAllNonPersistentTraits();
            if (partyComponent.hasParty) {
                partyComponent.currentParty.RemoveMember(this);
                //if (partyComponent.currentParty.IsLeader(this)) {
                //    partyComponent.currentParty.DisbandParty();
                //} else {
                //    partyComponent.currentParty.RemoveMember(this);
                //}
            }

            SetHP(0);
            //currentSettlement?.SettlementResources?.RemoveCharacterFromSettlement(this);
            if (structureComponent.HasWorkPlaceStructure() && structureComponent.workPlaceStructure.DoesCharacterWorkHere(this)) {
                structureComponent.workPlaceStructure.RemoveAssignedWorker(this);
            }

            if (interruptComponent.isInterrupted && interruptComponent.currentInterrupt.interrupt != interrupt) {
                interruptComponent.ForceEndNonSimultaneousInterrupt();
            }
            traitContainer.AddTrait(this, "Dead", responsibleCharacter);
            if (deathFromAction != null) {
                traitContainer.GetTraitOrStatus<Trait>("Dead")?.SetGainedFromDoingAction(deathFromAction.action.goapType, deathFromAction.isStealth);
            }

            if (cause == "attacked" && responsibleCharacter != null) {
                if (responsibleCharacter.isInWerewolfForm) {
                    traitContainer.AddTrait(this, "Mangled", responsibleCharacter);
                    if (deathFromAction != null) {
                        traitContainer.GetTraitOrStatus<Trait>("Mangled")?.SetGainedFromDoingAction(deathFromAction.action.goapType, deathFromAction.isStealth);
                    }
                }
                FactionType ft = faction?.factionType;
                if (ft != null && ft.type == FACTION_TYPE.Undead || ft.type == FACTION_TYPE.Wild_Monsters) {
                    if (responsibleCharacter.partyComponent.isMemberThatJoinedQuest) {
                        if (!responsibleCharacter.partyComponent.currentParty.isPlayerParty) {
                            responsibleCharacter.partyComponent.currentParty.AllMembersThatJoinedQuestGainsRandomCoinAmount(5, 10);
                        }
                    }
                }
            }

#if DEBUG_LOG
            logComponent.PrintLogIfActive($"{name} died of {cause}");
#endif
            if (_deathLog == null) {
                if (cause == "attacked" && responsibleCharacter == null) {
#if DEBUG_LOG
                    logComponent.PrintLogErrorIfActive($"{name} died, and reason was attacked, but no responsible character was provided!");
#endif
                }
                Log localDeathLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "Generic", $"death_{cause}", providedTags: LOG_TAG.Life_Changes);
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
                PlayerManager.Instance.player?.ShowNotificationFrom(this, localDeathLog);
                SetDeathLog(localDeathLog);
                LogPool.Release(localDeathLog);
            } else {
                SetDeathLog(_deathLog);
            }
            Messenger.Broadcast(CharacterSignals.CHARACTER_DEATH, this);
            eventDispatcher.ExecuteCharacterDied(this);

            List<Trait> afterDeathTraitOverrideFunctions = traitContainer.GetTraitOverrideFunctions(TraitManager.After_Death);
            if (afterDeathTraitOverrideFunctions != null) {
                for (int i = 0; i < afterDeathTraitOverrideFunctions.Count; i++) {
                    Trait trait = afterDeathTraitOverrideFunctions[i];
                    trait.AfterDeath(this);
                }
            }

            if (hasMarker) {
                marker.UpdateName();
            }

            //         if(responsibleCharacter != null) {
            //             if (responsibleCharacter.faction.factionType.type == FACTION_TYPE.Demons && faction.factionType.type != FACTION_TYPE.Demons) {
            //                 //Messenger.Broadcast(PlayerSignals.CREATE_SPIRIT_ENERGY, deathTile.worldLocation, 1, deathTile.parentMap);
            //                 Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, deathTilePosition.worldLocation, 1, deathTilePosition.parentMap);
            //             }
            //}

            Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, this as IPlayerActionTarget);
        }
    }
    public void SetDeathLog(Log log) {
        if (deathLog != null) {
            LogPool.Release(deathLog);
        }
        deathLog = GameManager.CreateNewLog();
        deathLog.Copy(log);
        deathStr = deathLog.logText;
    }
    public void SetGrave(Tombstone grave) {
        this.grave = grave;
    }
#endregion

#region Necromancer
    public void SetNecromancerTrait(Necromancer necromancer) {
        necromancerTrait = necromancer;
    }
#endregion

#region Emotions
    public bool CanFeelEmotion(EMOTION emotion) {
        switch (emotion) {
            case EMOTION.Anger:
            case EMOTION.Rage:
                return characterClass.className != "Hero";
            default:
                return true;
        }
    }
#endregion

#region Prison
    public bool IsInPrison() {
        BaseSettlement currSettlement = currentSettlement;
        return currSettlement != null && currSettlement is NPCSettlement settlement && currentStructure == settlement.prison;
    }
    public bool IsInPrisonOf(NPCSettlement settlement) {
        BaseSettlement currSettlement = currentSettlement;
        return currSettlement != null && currSettlement == settlement && currentStructure == settlement.prison;
    }
    public bool IsInPrisonOf(BaseSettlement settlement) {
        if (settlement is NPCSettlement npcSettlement) {
            return IsInPrisonOf(npcSettlement);
        }
        return false;
    }
    public LocationStructure GetSettlementPrisonFor(Character character) {
        //Right now the character parameter is irrelevant, but in the future we might need it to know what prison we should put that character in
        NPCSettlement settlement = null;
        if (homeSettlement != null) {
            if (currentSettlement != null) {
                if (currentSettlement == homeSettlement) {
                    //if current settlement is home settlement
                    settlement = homeSettlement;
                } else if (currentSettlement is NPCSettlement npcSettlement && npcSettlement.owner != null && faction != null && npcSettlement.owner == faction) {
                    //if current settlement is owned by same faction as this character
                    settlement = npcSettlement;
                } else {
                    settlement = homeSettlement;
                }
            } else {
                settlement = homeSettlement;
            }
        }
        if (settlement != null) {
            return settlement.prison;
        }
        return null;
    }
#endregion

#region Vampire
    private bool IsInVampireBatForm() {
        Vampire vampireTrait = traitContainer.GetTraitOrStatus<Vampire>("Vampire");
        return vampireTrait != null && vampireTrait.isInVampireBatForm;
    }
    public void TransformToVampireBatForm() {
        Vampire vampireTrait = traitContainer.GetTraitOrStatus<Vampire>("Vampire");
        if (vampireTrait != null && !vampireTrait.isInVampireBatForm) {
            vampireTrait.SetIsInVampireBatForm(true);
            movementComponent.AdjustSpeedModifier(0.20f);
            movementComponent.SetToFlying();
            if (visuals != null) {
                visuals.UpdateAllVisuals(this);
            }
        }
    }
    public void RevertFromVampireBatForm() {
        Vampire vampireTrait = traitContainer.GetTraitOrStatus<Vampire>("Vampire");
        if (vampireTrait != null && vampireTrait.isInVampireBatForm) {
            vampireTrait.SetIsInVampireBatForm(false);
            movementComponent.AdjustSpeedModifier(-0.20f);
            movementComponent.SetToNonFlying();
            if (visuals != null) {
                visuals.UpdateAllVisuals(this);
            }
        }
    }
#endregion

#region Cultist
    public void PopulateListOfCultistTargets(List<Character> choices, Func<Character, bool> criteria) {
        for (int i = 0; i < relationshipContainer.charactersWithOpinion.Count; i++) {
            Character target = relationshipContainer.charactersWithOpinion[i];
            if (criteria.Invoke(target)) {
                choices.Add(target);
            }
        }
    }
#endregion

#region IPointOfInterest
    public bool IsPOICurrentlyTargetedByAPerformingAction() {
        for (int i = 0; i < allJobsTargetingThis.Count; i++) {
            if (allJobsTargetingThis[i] is GoapPlanJob) {
                GoapPlanJob planJob = allJobsTargetingThis[i] as GoapPlanJob;
                if (planJob.assignedPlan != null && planJob.assignedPlan.currentActualNode.actionStatus == ACTION_STATUS.PERFORMING) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool IsPOICurrentlyTargetedByAPerformingAction(params JOB_TYPE[] jobType) {
        for (int i = 0; i < allJobsTargetingThis.Count; i++) {
            JobQueueItem job = allJobsTargetingThis[i];
            for (int j = 0; j < jobType.Length; j++) {
                if (jobType[j] == job.jobType) {
                    if (job is GoapPlanJob planJob) {
                        if (planJob.assignedPlan != null && planJob.assignedPlan.currentActualNode.actionStatus == ACTION_STATUS.PERFORMING) {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }
    public void SetCurrentLocationAwareness(ILocationAwareness locationAwareness) {
        currentLocationAwareness = locationAwareness;
    }
    //public void SetIsInPendingAwarenessList(bool state) {
    //    isInPendingAwarenessList = state;
    //}
    public bool IsUnpassable() {
        return false;
    }
    public virtual bool CanBeSeenBy(Character p_character) {
        if (p_character.hasMarker) {
            return p_character.marker.inVisionCharacters.Contains(this);
        }
        return false;
    }
#endregion

#region IStoredTarget
    public bool CanBeStoredAsTarget() {
        return !isDead && PlayerManager.Instance.player != null && faction != PlayerManager.Instance.player.playerFaction;
    }
    public void SetAsStoredTarget(bool p_state) {
        isStoredAsTarget = p_state;
    }
#endregion

#region Afflictions By Player
    public bool HasAfflictedByPlayerWith(PLAYER_SKILL_TYPE p_afflictionType) {
        return afflictionsSkillsInflictedByPlayer.Contains(p_afflictionType);
    }
    public bool HasAfflictedByPlayerWith(string p_traitName) {
        PLAYER_SKILL_TYPE afflictionType = PlayerSkillManager.Instance.GetAfflictionTypeByTraitName(p_traitName);
        return afflictionType != PLAYER_SKILL_TYPE.NONE && afflictionsSkillsInflictedByPlayer.Contains(afflictionType);
    }
    public bool HasAfflictedByPlayerWith(Trait p_trait) {
        return HasAfflictedByPlayerWith(p_trait.name);
    }
    public void AddAfflictionByPlayer(string p_traitName) {
        PLAYER_SKILL_TYPE afflictionType = PlayerSkillManager.Instance.GetAfflictionTypeByTraitName(p_traitName);
        if (afflictionType != PLAYER_SKILL_TYPE.NONE) {
            if (!afflictionsSkillsInflictedByPlayer.Contains(afflictionType)) {
                afflictionsSkillsInflictedByPlayer.Add(afflictionType);
            }
        }
    }
#endregion

#region Loading
    public virtual void LoadReferences(SaveDataCharacter data) {
        isInfoUnlocked = data.isInfoUnlocked;
        ConstructDefaultActions();
        if (data.hasLycan && lycanData == null) {
            LycanthropeData lycanData = data.lycanData.Load();
        }
        if (!string.IsNullOrEmpty(data.grave)) {
            grave = DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentID(data.grave) as Tombstone;
        }
        if (data.deathLog != null) {
            deathLog = data.deathLog;
            // deathLog = DatabaseManager.Instance.logDatabase.GetLogByPersistentID(data.deathLog);
        }
        if (!string.IsNullOrEmpty(data.homeRegion)) {
            homeRegion = DatabaseManager.Instance.regionDatabase.GetRegionByPersistentID(data.homeRegion);
        }
        if (!string.IsNullOrEmpty(data.homeSettlement)) {
            homeSettlement = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentID(data.homeSettlement) as NPCSettlement;
        }
        if (!string.IsNullOrEmpty(data.homeStructure)) {
            homeStructure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(data.homeStructure);
        }
        if (!string.IsNullOrEmpty(data.currentRegion)) {
            _currentRegion = DatabaseManager.Instance.regionDatabase.GetRegionByPersistentID(data.currentRegion);
        }
        if (!string.IsNullOrEmpty(data.currentStructure)) {
            _currentStructure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(data.currentStructure);
        }
        if (!string.IsNullOrEmpty(data.faction)) {
            _faction = FactionManager.Instance.GetFactionByPersistentID(data.faction);
        }
        if (!string.IsNullOrEmpty(data.currentJob)) {
            currentJob = DatabaseManager.Instance.jobDatabase.GetJobWithPersistentID(data.currentJob);
            if (currentJob != null && currentJob is GoapPlanJob job && job.assignedPlan != null) {
                currentPlan = job.assignedPlan;
            }
        }
        if (!string.IsNullOrEmpty(data.currentActionNode)) {
            currentActionNode = DatabaseManager.Instance.actionDatabase.GetActionByPersistentID(data.currentActionNode);
        }
        if (!string.IsNullOrEmpty(data.territory)) {
            territory = DatabaseManager.Instance.areaDatabase.GetAreaByPersistentID(data.territory);
        }
        if (!string.IsNullOrEmpty(data.deployedAtStructure)) {
            deployedAtStructure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(data.deployedAtStructure);
        }
        for (int i = 0; i < data.items.Count; i++) {
            TileObject obj = DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentID(data.items[i]);
            if (obj != null) {
                items.Add(obj);
            }
        }

        for (int i = 0; i < data.ownedItems.Count; i++) {
            TileObject obj = DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentID(data.ownedItems[i]);
            if (obj != null) {
                ownedItems.Add(obj);
            }
        }

        jobQueue.LoadReferences(data);
        for (int i = 0; i < data.forceCancelJobsOnTickEnded.Count; i++) {
            string forceCanceledJob = data.forceCancelJobsOnTickEnded[i];

            JobQueueItem job = DatabaseManager.Instance.jobDatabase.GetJobWithPersistentID(forceCanceledJob);
            if (job != null && !forcedCancelJobsOnTickEnded.Contains(job)) {
                forcedCancelJobsOnTickEnded.Add(job);
            }
        }

        trapStructure.LoadReferences(data.trapStructure);
        needsComponent.LoadReferences(data.needsComponent);
        structureComponent.LoadReferences(data.structureComponent);
        stateComponent.LoadReferences(data.stateComponent);
        nonActionEventsComponent.LoadReferences(data.nonActionEventsComponent);
        interruptComponent.LoadReferences(data.interruptComponent);
        behaviourComponent.LoadReferences(data.behaviourComponent);
        moodComponent.LoadReferences(data.moodComponent);
        jobComponent.LoadReferences(data.jobComponent);
        reactionComponent.LoadReferences(data.reactionComponent);
        // logComponent.LoadReferences(data.logComponent);
        combatComponent.LoadReferences(data.combatComponent);
        rumorComponent.LoadReferences(data.rumorComponent);
        assumptionComponent.LoadReferences(data.assumptionComponent);
        movementComponent.LoadReferences(data.movementComponent);
        stateAwarenessComponent.LoadReferences(data.stateAwarenessComponent);
        carryComponent.LoadReferences(data.carryComponent);
        partyComponent.LoadReferences(data.partyComponent);
        gatheringComponent.LoadReferences(data.gatheringComponent);
        tileObjectComponent.LoadReferences(data.tileObjectComponent);
        crimeComponent.LoadReferences(data.crimeComponent);
        previousCharacterDataComponent.LoadReferences(data.previousCharacterDataComponent);
        traitComponent.LoadReferences(data.traitComponent);
        dailyScheduleComponent.LoadReferences(data.dailyScheduleComponent);
        villagerWantsComponent?.LoadReferences(data.villagerWantsComponent, this);
        //Place marker after loading references
        if (data.hasMarker) {
            if (!marker) {
                CreateMarker();
            }
            marker.LoadMarkerPlacement(data, _currentRegion);

            //Loading carried object should be after creating marker because we need the character marker in order for the eobject to be carried
            carryComponent.LoadCarryReference(data.carryComponent);

            // if (currentActionNode != null && currentActionNode.actionStatus == ACTION_STATUS.PERFORMING) {
            //     if(currentActionNode.poiTarget is TileObject target) {
            //         target.OnDoActionToObject(currentActionNode);
            //     }
            // }
        }
        if (data.deathTileLocation.hasValue) {
            deathTilePosition = DatabaseManager.Instance.locationGridTileDatabase.GetTileBySavedData(data.deathTileLocation);
        }

        //Load character traits after all references and visuals and objects of character has been placed since
        LoadCharacterTraitsFromSave(data);
        for (int i = 0; i < data.equipmentInventory.Count; i++) {
            TileObject obj = DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentID(data.equipmentInventory[i]);
            if (obj != null) {
                equipmentInventory.Add(obj);
            }
        }
        equipmentComponent = new EquipmentComponent();
        ApplyStackCountForTraits();
        SetRelationshipContainer(data.saveDataBaseRelationshipContainer.Load());

        visuals.UpdateAllVisuals(this);
        //Do updating hidden state here because the marker must be created first and visuals must be updated
        OnSetIsHidden();
        reactionComponent.UpdateHiddenState();
        if (marker) {
            marker.UpdateAnimation();
        }
        if (!isDead && minion == null && !isInLimbo) {
            //only subscribe to listeners if character is not dead, this is because we expect that dead characters are not listening to any of the normal signals
            SubscribeToSignals();
        }
        if (minion == null) {
            SubscribeToPermanentSignals();
        }
    }
    public void LoadCurrentlyDoingAction() {
        if (marker) {
            marker.UpdatePosition();
            marker.UpdateAnimation();
        }
        if (currentActionNode != null) {
            if (currentActionNode.action.goapType == INTERACTION_TYPE.DIG && currentActionNode.poiTarget is ThinWall) {
                //TODO: quick fix for null error when a character that is trying to dig a thin wall is loaded and will cause error.
                SetCurrentActionNode(null, null, null);
            } else if (currentActionNode.actionStatus == ACTION_STATUS.STARTED) {
                SetCurrentActionNode(null, null, null);
            } else if (currentActionNode.actionStatus == ACTION_STATUS.PERFORMING) {
                if (currentActionNode.poiTarget is TileObject target) {
                    target.OnDoActionToObject(currentActionNode);
                }
                if (currentActionNode.goapType == INTERACTION_TYPE.MAKE_LOVE) {
                    Character actor = currentActionNode.actor;
                    Character targetCharacter = currentActionNode.poiTarget as Character;
                    Bed bed = null;
                    if (actor.tileObjectComponent.primaryBed != null) {
                        if (actor.tileObjectComponent.primaryBed.gridTileLocation != null
                            && (actor.gridTileLocation == actor.tileObjectComponent.primaryBed.gridTileLocation || actor.gridTileLocation.IsNeighbour(actor.tileObjectComponent.primaryBed.gridTileLocation, true))) {
                            bed = actor.tileObjectComponent.primaryBed;
                        }
                    } else if (targetCharacter.tileObjectComponent.primaryBed != null) {
                        if (targetCharacter.tileObjectComponent.primaryBed.gridTileLocation != null
                            && (actor.gridTileLocation == targetCharacter.tileObjectComponent.primaryBed.gridTileLocation || actor.gridTileLocation.IsNeighbour(targetCharacter.tileObjectComponent.primaryBed.gridTileLocation, true))) {
                            bed = targetCharacter.tileObjectComponent.primaryBed;
                        }
                    }
                    if (bed != null) {
                        bed.OnDoActionToObject(currentActionNode);
                    } else {
                        //If there is no bed to make love, just remove the action itself so that there will be no errors
                        SetCurrentActionNode(null, null, null);
                    }
                }
            }
        }
        if (CanPerformEndTickJobs()) {
            JobQueueItem job = null;
            if (jobQueue.jobsInQueue.Count > 0) {
                job = jobQueue.jobsInQueue[0];
            }
            if (job != null && job.ProcessJob() == false) {
                PerformJob(job);
            }
        }
    }
    protected void LoadCharacterTraitsFromSave(SaveDataCharacter data) {
        traitContainer.Load(this, data.saveDataTraitContainer);

        //This must be reapplied after loading traits because when a trait is loaded the values will also be adjusted
        //Example: perform value in saved data = 3, when a trait is loaded and it increases perform value the value will become 4
        //Now it is already inconsistent since the saved value is not the same as the loaded value now
        //So we must bring back the value to the saved one so that the character state when loaded is the same
        limiterComponent.ApplyDataFromSave(data.limiterComponent);
        moodComponent.SetSaveDataMoodComponent(data.moodComponent);

        if (traitContainer.HasTrait("Character Trait")) {
            defaultCharacterTrait = traitContainer.GetTraitOrStatus<CharacterTrait>("Character Trait");
        }
        if (traitContainer.HasTrait("Necromancer")) {
            necromancerTrait = traitContainer.GetTraitOrStatus<Necromancer>("Necromancer");
        }
    }
#endregion

#region apply equipment effects upon load
    void ApplyStackCountForTraits() {
        equipmentInventory.ForEach((eachEquip) => {
            (eachEquip as EquipmentItem).AssignData();
            equipmentComponent.SetEquipment(eachEquip as EquipmentItem, this, true);
        });
    }
#endregion

#region IBookmarkable
    public void OnSelectBookmark() {
        LeftSelectAction();
    }
    public void RemoveBookmark() {
        PlayerManager.Instance.player.bookmarkComponent.RemoveBookmark(this);
    }
    public void OnHoverOverBookmarkItem(UIHoverPosition p_pos) {
        UIManager.Instance.ShowCharacterNameplateTooltip(this, position: p_pos);
    }
    public void OnHoverOutBookmarkItem() {
        UIManager.Instance.HideCharacterNameplateTooltip();
    }
#endregion

#region Death Tile
    public void SetDeathLocation(LocationGridTile p_tile) {
        deathTilePosition = p_tile;
    }
    #endregion

    #region villager progression HUMANS = skill levelup, Elven = Power Crystal absorbed
    public void RecomputeResistanceInitialChangeClass(Character p_character, string p_previousClass) {
        if (p_previousClass != string.Empty && talentComponent != null && p_character.race == RACE.HUMANS) {
            int getLevelCount = 0;
            talentComponent.allTalents.ForEach((eachTalent) => {
                if(eachTalent.level > 1) {
                    getLevelCount += eachTalent.level - 1;
                }
            });
            for (int x = 0; x < getLevelCount; ++x) {
                ApplyClassBonusOnLevelUp(p_previousClass, -1f);
			}
            for (int x = 0; x < getLevelCount; ++x) {
                ApplyClassBonusOnLevelUp(classComponent.characterClass.className, 1f);
            }
        }
    }
    // private void ApplyInitialClassBonus(string p_className) {
    //     CharacterClassData classData = CharacterManager.Instance.GetOrCreateCharacterClassData(p_className);
    //     piercingAndResistancesComponent.AdjustBasePiercing(classData.characterSkillUpdateData.GetPiercingBonus());
    //
    //     RESISTANCE[] resistances = CollectionUtilities.GetEnumValues<RESISTANCE>();
    //     for (int i = 0; i < resistances.Length; i++) {
    //         RESISTANCE res = resistances[i];
    //         if (res != RESISTANCE.None) {
    //             if (res.IsElemental()) {
    //                 piercingAndResistancesComponent.AdjustResistance(res, classData.characterSkillUpdateData.GetAllElementalResistanceBonus());
    //             } else if (res.IsSecondary()) {
    //                 piercingAndResistancesComponent.AdjustResistance(res, classData.characterSkillUpdateData.GetAllSecondaryResistanceBonus());
    //             }
    //             piercingAndResistancesComponent.AdjustResistance(res, classData.characterSkillUpdateData.GetBonusBaseOnElement(res));
    //         }
    //     }
    // }
    public void RecomputePiercingAndResistanceForGameStart(Character p_character, string p_className) {
        CharacterClassData classData = CharacterManager.Instance.GetOrCreateCharacterClassData(p_className);
        float piercing = classData.initialVillagerPiercing;
        float physicalResistance = 0f;
        float mentalResistance = 0f;
        if (classData.initialVillagerPhysicalResistances != null && classData.initialVillagerPhysicalResistances.Length > 0) {
            physicalResistance = CollectionUtilities.GetRandomElement(classData.initialVillagerPhysicalResistances);
        }
        if (classData.initialVillagerMentalResistances != null && classData.initialVillagerMentalResistances.Length > 0) {
            mentalResistance = CollectionUtilities.GetRandomElement(classData.initialVillagerMentalResistances);
        }
        p_character.piercingAndResistancesComponent.SetBasePiercing(piercing);
        p_character.piercingAndResistancesComponent.SetResistance(RESISTANCE.Physical, physicalResistance);
        p_character.piercingAndResistancesComponent.SetResistance(RESISTANCE.Mental, mentalResistance);
        
        RESISTANCE[] resistances = CollectionUtilities.GetEnumValues<RESISTANCE>();
        for (int i = 0; i < resistances.Length; i++) {
            RESISTANCE res = resistances[i];
            if (res != RESISTANCE.None) {
                float randomResistanceValue = 0f;
                if (res.IsElemental()) {
                    if (classData.initialVillagerElementalResistances != null && classData.initialVillagerElementalResistances.Length > 0) {
                        randomResistanceValue = CollectionUtilities.GetRandomElement(classData.initialVillagerElementalResistances);
                    }
                    p_character.piercingAndResistancesComponent.SetResistance(res, randomResistanceValue);
                } else if (res.IsSecondary()) {
                    if (classData.initialVillagerSecondaryResistances != null && classData.initialVillagerSecondaryResistances.Length > 0) {
                        randomResistanceValue = CollectionUtilities.GetRandomElement(classData.initialVillagerSecondaryResistances);
                    }
                    p_character.piercingAndResistancesComponent.SetResistance(res, randomResistanceValue);
                }
            }
        }

    }
    //function to be attached on skill levelup NOTE!! - only humans should gain from this call
    public void ApplyClassBonusOnLevelUp(string p_className, float factor = 1f) {
        CharacterClassData classData = CharacterManager.Instance.GetOrCreateCharacterClassData(p_className);
        piercingAndResistancesComponent.AdjustBasePiercing(classData.characterSkillUpdateData.GetPiercingBonus() * factor);

        RESISTANCE[] resistances = CollectionUtilities.GetEnumValues<RESISTANCE>();
        for (int i = 0; i < resistances.Length; i++) {
            RESISTANCE res = resistances[i];
            if (res != RESISTANCE.None) {
                if (res.IsElemental()) {
                    piercingAndResistancesComponent.AdjustResistance(res, classData.characterSkillUpdateData.GetAllElementalResistanceBonus() * factor);
                } else if (res.IsSecondary()) {
                    piercingAndResistancesComponent.AdjustResistance(res, classData.characterSkillUpdateData.GetAllSecondaryResistanceBonus() * factor);
                }
                piercingAndResistancesComponent.AdjustResistance(res, classData.characterSkillUpdateData.GetBonusBaseOnElement(res) * factor);
            }
        }
    }

    #region levelup bonus
    // public void InitialBonusForCharacterBaseOnClass(Character p_character) {
    //     if (!p_character.characterClass.IsCombatant()) {
    //         InitialLevelUpNonCombatantCharacter(p_character);
    //     } else if (p_character.characterClass.className == "Druid") {
    //         InitialLevelUpDruidCharacter(p_character);
    //     } else if (p_character.characterClass.className == "Shaman") {
    //         InitialLevelUpShamanCharacter(p_character);
    //     } else if (p_character.characterClass.className == "Archer") {
    //         InitialLevelUpArcherCharacter(p_character);
    //     } else if (p_character.characterClass.className == "Stalker") {
    //         InitialLevelUpStalkerCharacter(p_character);
    //     } else if (p_character.characterClass.className == "Marauder") {
    //         InitialLevelUpMarauderCharacter(p_character);
    //     } else if (p_character.characterClass.className == "Barbarian") {
    //         InitialLevelUpBarbarianCharacter(p_character);
    //     }
    // }

    // public void InitialLevelUpNonCombatantCharacter(Character p_targetCharacter) {
    //     List<float> bonusNumbers = new List<float>() { 0f, 5f, 10f };
    //     LevelUpresistancesWithSetOfNumbers(p_targetCharacter, bonusNumbers);
    // }

    // private void LevelUpresistancesWithSetOfNumbers(Character p_targetCharacter, List<float> bonusNumbers) {
    //     RESISTANCE[] resistances = CollectionUtilities.GetEnumValues<RESISTANCE>();
    //     for (int i = 0; i < resistances.Length; i++) {
    //         RESISTANCE res = resistances[i];
    //         if (res != RESISTANCE.None) {
    //             if (res.IsElemental()) {
    //                 p_targetCharacter.piercingAndResistancesComponent.AdjustResistance(res, bonusNumbers[GameUtilities.RandomBetweenTwoNumbers(0, bonusNumbers.Count - 1)]);
    //             } else if (res.IsSecondary()) {
    //                 p_targetCharacter.piercingAndResistancesComponent.AdjustResistance(res, bonusNumbers[GameUtilities.RandomBetweenTwoNumbers(0, bonusNumbers.Count - 1)]);
    //             }
    //             p_targetCharacter.piercingAndResistancesComponent.AdjustResistance(res, bonusNumbers[GameUtilities.RandomBetweenTwoNumbers(0, bonusNumbers.Count - 1)]);
    //         }
    //     }
    // }

    // public void InitialLevelUpDruidCharacter(Character p_targetCharacter) {
    //     List<float> bonusNumbers = new List<float>() { 5f, 10f, 15f };
    //     LevelUpresistancesWithSetOfNumbers(p_targetCharacter, bonusNumbers);
    // }
    //
    // public void InitialLevelUpShamanCharacter(Character p_targetCharacter) {
    //     List<float> bonusNumbers = new List<float>() { 5f, 10f, 15f };
    //     p_targetCharacter.piercingAndResistancesComponent.AdjustBasePiercing(5f);
    //     LevelUpresistancesWithSetOfNumbers(p_targetCharacter, bonusNumbers);
    // }
    //
    // public void InitialLevelUpArcherCharacter(Character p_targetCharacter) {
    //     List<float> bonusNumbers = new List<float>() { 5f, 10f, 15f };
    //     p_targetCharacter.piercingAndResistancesComponent.AdjustBasePiercing(10f);
    //     LevelUpresistancesWithSetOfNumbers(p_targetCharacter, bonusNumbers);
    // }
    //
    // public void InitialLevelUpStalkerCharacter(Character p_targetCharacter) {
    //     List<float> bonusNumbers = new List<float>() { 5f, 10f, 15f };
    //     p_targetCharacter.piercingAndResistancesComponent.AdjustBasePiercing(15f);
    //     LevelUpresistancesWithSetOfNumbers(p_targetCharacter, bonusNumbers);
    // }
    //
    // public void InitialLevelUpMarauderCharacter(Character p_targetCharacter) {
    //     List<float> bonusNumbers = new List<float>() { 5f, 10f, 15f };
    //     p_targetCharacter.piercingAndResistancesComponent.AdjustBasePiercing(5f);
    //     LevelUpresistancesWithSetOfNumbers(p_targetCharacter, bonusNumbers);
    // }
    //
    // public void InitialLevelUpBarbarianCharacter(Character p_targetCharacter) {
    //     List<float> bonusNumbers = new List<float>() { 5f, 10f, 15f };
    //     p_targetCharacter.piercingAndResistancesComponent.AdjustBasePiercing(10f);
    //     LevelUpresistancesWithSetOfNumbers(p_targetCharacter, bonusNumbers);
    // }
    #endregion

    // public void ApplyInitialClassBonusLevelUp() {
    //     InitialBonusForCharacterBaseOnClass(this);
    // }

    //function to be attached on signal that 1 elven got a power crystal
    //check if they are on the same village
    //NOTE!! - only elves should gain from this call
    void OnPowerCrystalAbsorbedByAnElf(Character p_powerCrystalAbsorber, PowerCrystal p_crystal) {
        if (this != p_powerCrystalAbsorber && race == RACE.ELVES && homeSettlement == p_powerCrystalAbsorber.homeSettlement) {
            ApplyCrystalBonus(p_crystal);
        }
    }

    //the signal should be called here and also the absorber will gain the bonus from this function
    //NOTE!! - only elves should gain from this call
    public void AbsorbCrystal(PowerCrystal p_crystal) {
        ApplyCrystalBonus(p_crystal);

        Log log;
        string crystalBonus = string.Empty;
        if (homeSettlement == null || homeSettlement.locationType != LOCATION_TYPE.VILLAGE) {
            if (p_crystal.amountBonusPiercing > 0) {
                crystalBonus = name + " gained " + p_crystal.amountBonusPiercing + "% Piercing";
            } else {
                crystalBonus = name + " gained " + p_crystal.amountBonusResistance + "% " + p_crystal.resistanceBonuses[0] + " Resistance";
            }
            log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "Power Crystal", "absorb_crystal_homeless", null, LOG_TAG.Major);
            log.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(null, crystalBonus, LOG_IDENTIFIER.STRING_1);
            log.AddLogToDatabase();
            PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
        } else {
            if (p_crystal.amountBonusPiercing > 0) {
                crystalBonus = " gained " + p_crystal.amountBonusPiercing + "% Piercing";
            } else {
                crystalBonus = " gained " + p_crystal.amountBonusResistance + "% " + p_crystal.resistanceBonuses[0] + " Resistance";
            }
            Messenger.Broadcast(CharacterSignals.ON_ELF_ABSORB_CRYSTAL, this, p_crystal);
            log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "Power Crystal", "absorb_crystal_villager", null, LOG_TAG.Major);
            log.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(homeSettlement, homeSettlement.name, LOG_IDENTIFIER.LANDMARK_1);
            log.AddToFillers(null, crystalBonus, LOG_IDENTIFIER.STRING_1);
            log.AddLogToDatabase();
            PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
        }
    }

    public void ApplyCrystalBonus(PowerCrystal p_crystal) {
        p_crystal.resistanceBonuses.ForEach((eachResistance) => {
            piercingAndResistancesComponent.AdjustResistance(eachResistance, p_crystal.amountBonusResistance);
        });
        piercingAndResistancesComponent.AdjustBasePiercing(p_crystal.amountBonusPiercing);
    }
#endregion

#region Talents
    public bool HasTalents() {
        return talentComponent != null;
    }
#endregion

    //Remove this temporarily since we do not yet object pool characters
    //Only do this when we object pool characters
    //#region Operators
    //public static bool operator ==(Character left, IPointOfInterest right) {
    //    return left?.persistentID == right?.persistentID;
    //}
    //public static bool operator !=(Character left, IPointOfInterest right) {
    //    return left?.persistentID != right?.persistentID;
    //}
    //public override bool Equals(object obj) {
    //    if (obj is Character c) {
    //        return persistentID.Equals(c.persistentID);
    //    }
    //    return false;
    //}
    //public override int GetHashCode() {
    //    return base.GetHashCode();
    //}
    //#endregion

#region IGCollectable
    public void SetIsDeadReference(bool p_state) {
        isDeadReference = p_state;
    }
#endregion

    public void CleanUp() {
        _minion?.CleanUp();
        _raceSetting = null;
        _faction = null;
        _minion = null;
        _currentStructure = null;
        _currentRegion = null;

        visuals?.CleanUp();
        traitContainer?.CleanUp();
        trapStructure = null;
        planner = null;
        jobComponent = null;
        logComponent = null;
        eventDispatcher = null;
        bookmarkEventDispatcher = null;
        equipmentComponent = null;
        resourceStorageComponent = null;

        classComponent.CleanUp();
        needsComponent.CleanUp();
        structureComponent.CleanUp();
        stateComponent.CleanUp();
        nonActionEventsComponent.CleanUp();
        interruptComponent.CleanUp();
        behaviourComponent.CleanUp();
        moodComponent.CleanUp();
        reactionComponent.CleanUp();
        combatComponent.CleanUp();
        assumptionComponent.CleanUp();
        movementComponent.CleanUp();
        stateAwarenessComponent.CleanUp();
        carryComponent.CleanUp();
        partyComponent.CleanUp();
        gatheringComponent.CleanUp();
        tileObjectComponent.CleanUp();
        crimeComponent.CleanUp();
        religionComponent.CleanUp();
        limiterComponent.CleanUp();
        piercingAndResistancesComponent.CleanUp();
        previousCharacterDataComponent.CleanUp();
        traitComponent.CleanUp();
        moneyComponent.CleanUp();
        dailyScheduleComponent.CleanUp();
        talentComponent?.CleanUp();
        villagerWantsComponent?.CleanUp();

        classComponent = null;
        needsComponent = null;
        structureComponent = null;
        stateComponent = null;
        nonActionEventsComponent = null;
        interruptComponent = null;
        behaviourComponent = null;
        moodComponent = null;
        reactionComponent = null;
        combatComponent = null;
        assumptionComponent = null;
        movementComponent = null;
        stateAwarenessComponent = null;
        carryComponent = null;
        partyComponent = null;
        gatheringComponent = null;
        tileObjectComponent = null;
        crimeComponent = null;
        religionComponent = null;
        limiterComponent = null;
        piercingAndResistancesComponent = null;
        previousCharacterDataComponent = null;
        traitComponent = null;
        moneyComponent = null;
        dailyScheduleComponent = null;
        talentComponent = null;
        villagerWantsComponent = null;
    }
}