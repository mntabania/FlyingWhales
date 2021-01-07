using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Traits;
using UnityEngine.Assertions;
using UtilityScripts;
using UnityEngine.EventSystems;
using Locations.Settlements;
using Logs;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public abstract class TileObject : MapObject<TileObject>, IPointOfInterest, IPlayerActionTarget, IPartyQuestTarget, IGatheringTarget, ISavable {
    public string persistentID { get; protected set; }
    public string name { get; protected set; }
    public int id { get; private set; }
    public TILE_OBJECT_TYPE tileObjectType { get; private set; }
    public Character characterOwner { get; protected set; }
    public List<INTERACTION_TYPE> advertisedActions { get; protected set; }
    public Region currentRegion => gridTileLocation.structure.region.coreTile.region;
    public LocationStructure structureLocation => gridTileLocation?.structure;
    public bool isPreplaced { get; private set; }
    /// <summary>
    /// All currently in progress jobs targeting this.
    /// </summary>
    public List<JobQueueItem> allJobsTargetingThis { get; private set; }
    /// <summary>
    /// All instantiated jobs that are targeting this object.
    /// </summary>
    public List<JobQueueItem> allExistingJobsTargetingThis { get; private set; }
    public List<Character> charactersThatAlreadyAssumed { get; private set; }
    public Character isBeingCarriedBy { get; private set; }

    //hp
    public int maxHP { get; protected set; }
    public int currentHP { get; protected set; }

    //tile slots
    private TileObjectSlotItem[] slots { get; set; } //for users
    private GameObject slotsParent;
    protected bool hasCreatedSlots;

    public virtual LocationGridTile gridTileLocation { get; protected set; }
    public POI_STATE state { get; private set; }
    public LocationGridTile previousTile { get; private set; }
    public Dictionary<RESOURCE, int> storedResources { get; private set; }
    protected Dictionary<RESOURCE, int> maxResourceValues { get; set; }
    public List<PLAYER_SKILL_TYPE> actions { get; protected set; }
    public int repairCounter { get; protected set; } //If greater than zero, this tile object cannot be repaired
    public int numOfActionsBeingPerformedOnThis { get; private set; } //this is increased, when the action of another character stops this characters movement

    private bool hasSubscribedToListeners;

    public LogComponent logComponent { get; protected set; }
    
    #region getters
    public OBJECT_TYPE objectType => OBJECT_TYPE.Tile_Object;
    public virtual Type serializedData => typeof(SaveDataTileObject);
    public POINT_OF_INTEREST_TYPE poiType => POINT_OF_INTEREST_TYPE.TILE_OBJECT;
    public virtual Vector3 worldPosition => mapVisual.transform.position;
    public virtual Vector2 selectableSize => Vector2Int.one;
    public bool isDead => gridTileLocation == null; //Consider the object as dead if it no longer has a tile location (has been removed)
    public ProjectileReceiver projectileReceiver => mapVisual.visionTrigger.projectileReceiver;
    public Transform worldObject => mapVisual != null ? mapVisual.transform : null;
    public string nameWithID => ToString();
    public GameObject visualGO => mapVisual.gameObject;
    //public Character isBeingCarriedBy => carriedByCharacter;
    public Faction factionOwner => characterOwner?.faction;
    public bool canBeRepaired => repairCounter <= 0;
    public bool isBeingSeized => PlayerManager.Instance.player != null && PlayerManager.Instance.player.seizeComponent.seizedPOI == this;
    public bool isHidden => false;
    public LocationStructure currentStructure => gridTileLocation?.structure;
    public BaseSettlement currentSettlement {
        get {
            BaseSettlement settlement = null;
            gridTileLocation?.IsPartOfSettlement(out settlement);
            return settlement;
        }
    }
    public BaseMapObjectVisual mapObjectVisual => mapVisual;
    public virtual string neutralizer => string.Empty;
    public virtual Character[] users { //array of characters, currently using the tile object
        get {
            return slots?.Where(x => x != null && x.user != null).Select(x => x.user).ToArray() ?? null;
        }
    }
    #endregion

    public TileObject() { }
    public TileObject(SaveDataTileObject data) { }


    protected virtual void Initialize(TILE_OBJECT_TYPE tileObjectType, bool shouldAddCommonAdvertisements = true) {
        persistentID = UtilityScripts.Utilities.GetNewUniqueID();
        id = UtilityScripts.Utilities.SetID(this);
        this.tileObjectType = tileObjectType;
        name = GenerateName();
        allJobsTargetingThis = new List<JobQueueItem>();
        allExistingJobsTargetingThis = new List<JobQueueItem>();
        charactersThatAlreadyAssumed = new List<Character>();
        hasCreatedSlots = false;
        maxHP = TileObjectDB.GetTileObjectData(tileObjectType).maxHP;
        currentHP = maxHP;
        CreateTraitContainer();
        traitContainer.AddTrait(this, "Flammable");
        ConstructResources();
        if (shouldAddCommonAdvertisements) {
            AddCommonAdvertisements();
        }
        ConstructDefaultActions();
        logComponent = new LogComponent(); logComponent.SetOwner(this);
        DatabaseManager.Instance.tileObjectDatabase.RegisterTileObject(this);
        SubscribeListeners();
    }
    public void Initialize(SaveDataTileObject data) {
        persistentID = data.persistentID;
        id = UtilityScripts.Utilities.SetID(this, data.id);
        tileObjectType = data.tileObjectType;
        name = data.name;
        allJobsTargetingThis = new List<JobQueueItem>();
        allExistingJobsTargetingThis = new List<JobQueueItem>();
        charactersThatAlreadyAssumed = new List<Character>();
        hasCreatedSlots = false;
        maxHP = TileObjectDB.GetTileObjectData(tileObjectType).maxHP;
        currentHP = data.currentHP;
        isPreplaced = data.isPreplaced;
        SetPOIState(data.poiState);
        CreateTraitContainer();
        LoadResources(data);
        advertisedActions = new List<INTERACTION_TYPE>(data.advertisedActions);
        ConstructDefaultActions();

        logComponent = data.logComponent.Load();
        logComponent.SetOwner(this);

        DatabaseManager.Instance.tileObjectDatabase.RegisterTileObject(this);
        SubscribeListeners();
    }

    #region Loading
    /// <summary>
    /// Load data from second wave. NOTE: This is called after the tile object is placed.
    /// </summary>
    /// <param name="data">Saved data</param>
    public virtual void LoadSecondWave(SaveDataTileObject data) {
        // logComponent.LoadReferences(data.logComponent);
        for (int i = 0; i < data.jobsTargetingThis.Count; i++) {
            string jobID = data.jobsTargetingThis[i];
            if (!string.IsNullOrEmpty(jobID)) {
                JobQueueItem job = DatabaseManager.Instance.jobDatabase.GetJobWithPersistentID(jobID);
                AddJobTargetingThis(job);    
            }
        }
        for (int i = 0; i < data.existingJobsTargetingThis.Count; i++) {
            string jobID = data.existingJobsTargetingThis[i];
            if (!string.IsNullOrEmpty(jobID)) {
                JobQueueItem job = DatabaseManager.Instance.jobDatabase.GetJobWithPersistentID(jobID);
                AddExistingJobTargetingThis(job);
            }
        }
        SetMapObjectState(data.mapObjectState);
        if (!string.IsNullOrEmpty(data.characterOwnerID)) {
            characterOwner = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(data.characterOwnerID);    
        }
        if (!string.IsNullOrEmpty(data.isBeingCarriedByID)) {
            isBeingCarriedBy = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(data.isBeingCarriedByID);
        }
    }
    /// <summary>
    /// Load more info, this is called after character markers have been created.
    /// So this is a good place to load data that relies on that.
    /// </summary>
    public virtual void LoadAdditionalInfo(SaveDataTileObject data){}
    #endregion
    
    private void AddCommonAdvertisements() {
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
        AddAdvertisedAction(INTERACTION_TYPE.REPAIR);
        AddAdvertisedAction(INTERACTION_TYPE.BOOBY_TRAP);
    }
    protected void RemoveCommonAdvertisements() {
        RemoveAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        RemoveAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
        RemoveAdvertisedAction(INTERACTION_TYPE.REPAIR);
        RemoveAdvertisedAction(INTERACTION_TYPE.BOOBY_TRAP);
    }

    #region Listeners
    protected void SubscribeListeners() {
        //Messenger.AddListener(Signals.TICK_STARTED, ProcessTraitsOnTickStarted);
    }
    protected void UnsubscribeListeners() {
        //Messenger.RemoveListener(Signals.TICK_STARTED, ProcessTraitsOnTickStarted);
    }
    #endregion

    #region Virtuals
    /// <summary>
    /// Called when a character starts to do an action towards this object.
    /// </summary>
    /// <param name="action">The current action</param>
    public virtual void OnDoActionToObject(ActualGoapNode action) { }
    /// <summary>
    /// Called when a character finished doing an action towards this object.
    /// </summary>
    /// <param name="action">The finished action</param>
    public virtual void OnDoneActionToObject(ActualGoapNode action) { }
    /// <summary>
    /// Called when a character cancelled doing an action towards this object.
    /// </summary>
    /// <param name="action">The finished action</param>
    public virtual void OnCancelActionTowardsObject(ActualGoapNode action) { }
    /// <summary>
    /// When this tile object is placed on a tile, should it be set as that tile's object.
    /// <see cref="LocationGridTile.objHere"/>
    /// </summary>
    /// <returns></returns>
    public virtual bool OccupiesTile() { return true;}
    public virtual void OnDestroyPOI() {
        //DisableGameObject();
        if (previousTile != null) {
            LocationAwarenessUtility.RemoveFromAwarenessList(this, previousTile);
        }
        //removed by aaron aranas awareness update previousTile?.parentMap.region.RemovePendingAwareness(this);
        Messenger.Broadcast(CharacterSignals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, this as IPointOfInterest, "");
        Messenger.Broadcast(CharacterSignals.FORCE_CANCEL_ALL_ACTIONS_TARGETING_POI, this as IPointOfInterest, "");
        OnRemoveTileObject(null, previousTile);
        DestroyMapVisualGameObject();
        SetPOIState(POI_STATE.INACTIVE);
        if (TileObjectDB.TryGetTileObjectData(tileObjectType, out var objData)) {
            if (objData.occupiedSize.X > 1 || objData.occupiedSize.Y > 1) {
                UnoccupyTiles(objData.occupiedSize, previousTile);
            }
        }
        Messenger.Broadcast(JobSignals.CHECK_APPLICABILITY_OF_ALL_JOBS_TARGETING, this as IPointOfInterest);
        UnsubscribeListeners();
    }
    public void OnDiscardCarriedObject() {
        //DisableGameObject();
        Messenger.Broadcast(CharacterSignals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, this as IPointOfInterest, "");
        Messenger.Broadcast(CharacterSignals.FORCE_CANCEL_ALL_ACTIONS_TARGETING_POI, this as IPointOfInterest, "");
        DestroyMapVisualGameObject();
        SetPOIState(POI_STATE.INACTIVE);
        Messenger.Broadcast(JobSignals.CHECK_APPLICABILITY_OF_ALL_JOBS_TARGETING, this as IPointOfInterest);
        UnsubscribeListeners();
    }
    public virtual void OnPlacePOI() {
        DefaultProcessOnPlacePOI();
    }
    public virtual void OnLoadPlacePOI() {
        OnPlacePOI(); //just defaulted to normal on place poi behaviour, since usually this is okay, override function as needed.
    }
    /// <summary>
    /// Convenience function for executing expected behaviour when placing a tile object.
    /// This is mostly used for when you want to override <see cref="OnLoadPlacePOI"/>
    /// and use the default functionality without using any of the overridden <see cref="OnPlacePOI"/>
    /// </summary>
    protected void DefaultProcessOnPlacePOI() {
        SetPOIState(POI_STATE.ACTIVE);
        if (mapVisual == null) {
            InitializeMapObject(this);
            OnMapObjectStateChanged(); //update visuals based on map object state
            LocationAwarenessUtility.AddToAwarenessList(this, gridTileLocation);
            //removed by aaron for awareness update gridTileLocation.parentMap.region.AddPendingAwareness(this);
        }
        PlaceMapObjectAt(gridTileLocation);
        mapVisual.UpdateSortingOrders(this);
        OnPlaceTileObjectAtTile(gridTileLocation);
        if (TileObjectDB.TryGetTileObjectData(tileObjectType, out var objData)) {
            if (objData.occupiedSize.X > 1 || objData.occupiedSize.Y > 1) {
                OccupyTiles(objData.occupiedSize, gridTileLocation);
            }
        }
        if (gridTileLocation.collectionOwner.isPartOfParentRegionMap) {
            gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.OnPlacePOIInHex(this);
        }
        SubscribeListeners();
        if (gridTileLocation.genericTileObject.traitContainer.HasTrait("Poisoned")) {
            //add poisoned to floor
            //Reference: https://trello.com/c/mzPmP1Qv/1933-if-you-drop-food-on-a-poisoned-tile-it-should-also-get-poisoned
            traitContainer.AddTrait(this, "Poisoned");
        }
    }
    public virtual void RemoveTileObject(Character removedBy) {
        SetGridTileLocation(null);
        //DisableGameObject();
        //DestroyGameObject();
        //OnRemoveTileObject(removedBy, previousTile);
        //SetPOIState(POI_STATE.INACTIVE);
        OnDestroyPOI();
        if (previousTile != null && previousTile.collectionOwner.isPartOfParentRegionMap 
                                 && previousTile.collectionOwner.partOfHextile.hexTileOwner) {
            previousTile.collectionOwner.partOfHextile.hexTileOwner.OnRemovePOIInHex(this);
        }
    }
    public virtual LocationGridTile GetNearestUnoccupiedTileFromThis() {
        if (gridTileLocation != null) {
            List<LocationGridTile> unoccupiedNeighbours = gridTileLocation.UnoccupiedNeighbours;
            if (unoccupiedNeighbours.Count == 0) {
                return null;
            } else {
                return unoccupiedNeighbours[Random.Range(0, unoccupiedNeighbours.Count)];
            }
        }
        return null;
    }
    //Returns the chosen action for the plan
    public GoapAction AdvertiseActionsToActor(Character actor, GoapEffect precondition, GoapPlanJob job, ref int cost, ref string log) {
        GoapAction chosenAction = null;
        if (advertisedActions != null && advertisedActions.Count > 0) {//&& IsAvailable()
            bool isCharacterAvailable = IsAvailable();
            //List<GoapAction> usableActions = new List<GoapAction>();
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
                LocationGridTile tileLocation = gridTileLocation;
                if(isBeingCarriedBy != null) {
                    tileLocation = isBeingCarriedBy.gridTileLocation;
                }
                if ((action.canBePerformedEvenIfPathImpossible || actor.movementComponent.HasPathToEvenIfDiffRegion(tileLocation)) && RaceManager.Instance.CanCharacterDoGoapAction(actor, currType)) {
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
            && actor.trapStructure.SatisfiesForcedHex(this)
            && RaceManager.Instance.CanCharacterDoGoapAction(actor, action.goapType)) {
            LocationGridTile tileLocation = gridTileLocation;
            if (isBeingCarriedBy != null) {
                tileLocation = isBeingCarriedBy.gridTileLocation;
            }
            if (action.canBePerformedEvenIfPathImpossible || actor.movementComponent.HasPathToEvenIfDiffRegion(tileLocation)) {
                OtherData[] data = job.GetOtherDataFor(action.goapType);
                if (action.CanSatisfyRequirements(actor, this, data, job)) {
                    return true;
                }
            }
        }
        return false;
    }
    public virtual void SetPOIState(POI_STATE state) {
        this.state = state;
    }
    /// <summary>
    /// Triggered when the grid tile location of this object is set to null.
    /// </summary>
    public virtual void OnRemoveTileObject(Character removedBy, LocationGridTile removedFrom, bool removeTraits = true, bool destroyTileSlots = true) {
        // Debug.Log(GameManager.Instance.TodayLogString() + "Tile Object " + this.name + " has been removed");
        Messenger.Broadcast(GridTileSignals.TILE_OBJECT_REMOVED, this, removedBy, removedFrom);
        if (mapObjectState == MAP_OBJECT_STATE.UNBUILT) {
            //if object is unbuilt, and it was removed, stop checking for invalidity.
            Messenger.RemoveListener(TileObjectSignals.CHECK_UNBUILT_OBJECT_VALIDITY, CheckUnbuiltObjectValidity);
        } else if (mapObjectState == MAP_OBJECT_STATE.BUILT) {
            removedFrom?.parentMap.region.RemoveTileObjectInRegion(this);
        }
        if (hasCreatedSlots && destroyTileSlots) {
            DestroyTileSlots();
        }
        if (removeTraits) {
            traitContainer.RemoveAllTraitsAndStatuses(this);
        }
    }
    public virtual void OnTileObjectGainedTrait(Trait trait) {
        if (trait is Status status && status.isTangible && mapObjectVisual != null) {
            mapObjectVisual.visionTrigger.VoteToMakeVisibleToCharacters();
        }
    }
    public virtual void OnTileObjectLostTrait(Trait trait) {
        if (trait is Status status && status.isTangible && mapObjectVisual != null) {
            mapObjectVisual.visionTrigger.VoteToMakeInvisibleToCharacters();
        }
    }
    public virtual bool IsValidCombatTargetFor(IPointOfInterest source) {
        return gridTileLocation != null && source.gridTileLocation != null && (this is BlockWall || (source is Character character && character.movementComponent.HasPathToEvenIfDiffRegion(gridTileLocation)));
    }
    public virtual bool IsStillConsideredPartOfAwarenessByCharacter(Character character) {
        if(mapVisual == null) {
            return false;
        }
        if(gridTileLocation != null && currentRegion == character.currentRegion) {
            return true;
        }else if (isBeingCarriedBy != null && isBeingCarriedBy.currentRegion == character.currentRegion) {
            return true;
        }
        return false;
    }
    protected virtual string GenerateName() { return UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(tileObjectType.ToString()); }
    public virtual void Neutralize() { }
    public virtual void ConstructDefaultActions() {
        actions = new List<PLAYER_SKILL_TYPE>();

        if (tileObjectType == TILE_OBJECT_TYPE.RAVENOUS_SPIRIT || tileObjectType == TILE_OBJECT_TYPE.FEEBLE_SPIRIT ||
            tileObjectType == TILE_OBJECT_TYPE.FORLORN_SPIRIT) {
            return;
        }

        AddPlayerAction(PLAYER_SKILL_TYPE.DESTROY);
        AddPlayerAction(PLAYER_SKILL_TYPE.IGNITE);
        AddPlayerAction(PLAYER_SKILL_TYPE.POISON);
        AddPlayerAction(PLAYER_SKILL_TYPE.SEIZE_OBJECT);
    }
    public virtual void ActivateTileObject() {
        //Messenger.Broadcast(Signals.INCREASE_THREAT_THAT_SEES_POI, this as IPointOfInterest, 5);
        Messenger.Broadcast(TileObjectSignals.TILE_OBJECT_ACTIVATED, this);
    }
    //public virtual void OnTileObjectAddedToInventoryOf(Character inventoryOwner) { }
    public virtual void OnTileObjectDroppedBy(Character inventoryOwner, LocationGridTile tile) { }
    #endregion

    #region IPointOfInterest
    public bool IsAvailable() {
        return state != POI_STATE.INACTIVE;
    }
    public void AddAdvertisedAction(INTERACTION_TYPE type, bool allowDuplicates = false) {
        if (advertisedActions == null) {
            advertisedActions = new List<INTERACTION_TYPE>(); //{ INTERACTION_TYPE.ASSAULT }
        }
        if (allowDuplicates || advertisedActions.Contains(type) == false) {
            advertisedActions.Add(type);
        }
    }
    public void RemoveAdvertisedAction(INTERACTION_TYPE type) {
        advertisedActions.Remove(type);
    }
    public void AddJobTargetingThis(JobQueueItem job) {
        allJobsTargetingThis.Add(job);
        // ReSharper disable once Unity.NoNullPropagation
        mapObjectVisual?.visionTrigger.VoteToMakeVisibleToCharacters();
    }
    public bool RemoveJobTargetingThis(JobQueueItem job) {
        if (allJobsTargetingThis.Remove(job)) {
            // ReSharper disable once Unity.NoNullPropagation
            mapObjectVisual?.visionTrigger.VoteToMakeInvisibleToCharacters();
            return true;
        }
        return false;
    }
    public virtual void AddExistingJobTargetingThis(JobQueueItem job) {
        allExistingJobsTargetingThis.Add(job);
    }
    public bool RemoveExistingJobTargetingThis(JobQueueItem job) {
        return allExistingJobsTargetingThis.Remove(job);
    }
    public bool HasJobTargetingThis(params JOB_TYPE[] jobTypes) {
        for (int i = 0; i < allJobsTargetingThis.Count; i++) {
            JobQueueItem job = allJobsTargetingThis[i];
            for (int j = 0; j < jobTypes.Length; j++) {
                if (job.jobType == jobTypes[j]) {
                    return true;
                }
            }
        }
        return false;
    }
    public GoapPlanJob GetJobTargetingThisCharacter(JOB_TYPE jobType) {
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
    public virtual void AdjustHP(int amount, ELEMENTAL_TYPE elementalDamageType, bool triggerDeath = false,
        object source = null, CombatManager.ElementalTraitProcessor elementalTraitProcessor = null, bool showHPBar = false) {
        if (currentHP == 0 && amount < 0) { return; } //hp is already at minimum, do not allow any more negative adjustments
        CombatManager.Instance.DamageModifierByElements(ref amount, elementalDamageType, this);

        if ((amount < 0  && CanBeDamaged()) || amount > 0) {
            //only added checking here because even if objects cannot be damaged,
            //they should still be able to react to the elements
            if (amount < 0 &&  Mathf.Abs(amount) > currentHP) {
                //if the damage amount is greater than this object's hp, set the damage to this object's
                //hp instead, this is so that if this object contributes to a structure's hp, it will not deal the excess damage
                //to the structure
                amount = -currentHP;
            }
            currentHP += amount;
            currentHP = Mathf.Clamp(currentHP, 0, maxHP);
            if (mapVisual && mapVisual.hpBarGO && showHPBar) {
                if (mapVisual.hpBarGO.activeSelf) {
                    mapVisual.UpdateHP(this);
                } else {
                    if (amount < 0 && currentHP > 0) {
                        //only show hp bar if hp was reduced and hp is greater than 0
                        mapVisual.QuickShowHPBar(this);
                    }
                }
            }    
        }
        
        if (amount < 0) {
            Character responsibleCharacter = null;
            if (source is Character character) {
                responsibleCharacter = character;
            }
            CombatManager.ElementalTraitProcessor etp = elementalTraitProcessor ?? CombatManager.Instance.DefaultElementalTraitProcessor;
            CombatManager.Instance.ApplyElementalDamage(amount, elementalDamageType, this, responsibleCharacter, etp);
            //CancelRemoveStatusFeedAndRepairJobsTargetingThis();
        }
        LocationGridTile tile = gridTileLocation;
        if (currentHP <= 0) {
            //object has been destroyed
            Character removed = null;
            if (source is Character character) {
                removed = character;
            }
            if (tile != null && tile.structure != null) {
                tile.structure.RemovePOI(this, removed);    
            } else if (isBeingCarriedBy != null) {
                isBeingCarriedBy.UncarryPOI(this, addToLocation: false);
            }
        }
        if (amount < 0) {
            Messenger.Broadcast(TileObjectSignals.TILE_OBJECT_DAMAGED, this, amount);
        } else if (amount > 0) {
            Messenger.Broadcast(TileObjectSignals.TILE_OBJECT_REPAIRED, this, amount);
        }
        if (currentHP == maxHP) {
            Messenger.Broadcast(TileObjectSignals.TILE_OBJECT_FULLY_REPAIRED, this);
        }
    }
    public void OnHitByAttackFrom(Character characterThatAttacked, CombatState combatStateOfAttacker, ref string attackSummary) {
        if (characterThatAttacked == null) {
            return;
        }
        ELEMENTAL_TYPE elementalType = characterThatAttacked.combatComponent.elementalDamage.type;
        //GameManager.Instance.CreateHitEffectAt(this, elementalType);
        if (currentHP <= 0) {
            return; //if hp is already 0, do not deal damage
        }
        AdjustHP(-characterThatAttacked.combatComponent.attack, elementalType, source: characterThatAttacked, showHPBar: true);
        attackSummary = $"{attackSummary}\nDealt damage {characterThatAttacked.combatComponent.attack.ToString()}";
        if (currentHP <= 0) {
            attackSummary = $"{attackSummary}\n{name}'s hp has reached 0.";
        }
        if (characterThatAttacked.marker) {
            for (int i = 0; i < characterThatAttacked.marker.inVisionCharacters.Count; i++) {
                Character inVision = characterThatAttacked.marker.inVisionCharacters[i];
                inVision.reactionComponent.ReactToCombat(combatStateOfAttacker, this);
                inVision.needsComponent.WakeUpFromNoise();
            }
        }
        if(characterThatAttacked is Dragon && this is GenericTileObject) {
            characterThatAttacked.combatComponent.RemoveHostileInRange(this);
        }
        //Messenger.Broadcast(Signals.CHARACTER_WAS_HIT, this, characterThatAttacked);
    }
    public void SetGridTileLocation(LocationGridTile tile) {
        previousTile = gridTileLocation;
        gridTileLocation = tile;
    }
    public void OnSeizePOI() {
        if (UIManager.Instance.tileObjectInfoUI.isShowing && UIManager.Instance.tileObjectInfoUI.activeTileObject == this) {
            UIManager.Instance.tileObjectInfoUI.CloseMenu();
        }
        Messenger.Broadcast(CharacterSignals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, this as IPointOfInterest, "");
        Messenger.Broadcast(CharacterSignals.FORCE_CANCEL_ALL_ACTIONS_TARGETING_POI, this as IPointOfInterest, "");
        //Messenger.Broadcast(Signals.ON_SEIZE_TILE_OBJECT, this);
        //OnRemoveTileObject(null, gridTileLocation, false, false);
        gridTileLocation.structure.RemovePOIWithoutDestroying(this);
        //DestroyGameObject();
        //SetPOIState(POI_STATE.INACTIVE);
        if (TileObjectDB.TryGetTileObjectData(tileObjectType, out var objData)) {
            if (objData.occupiedSize.X > 1 || objData.occupiedSize.Y > 1) {
                UnoccupyTiles(objData.occupiedSize, previousTile);
            }
        }
        Messenger.Broadcast(JobSignals.CHECK_APPLICABILITY_OF_ALL_JOBS_TARGETING, this as IPointOfInterest);
        UnsubscribeListeners();
    }
    public void OnUnseizePOI(LocationGridTile tileLocation) {
        DestroyMapVisualGameObject();
        tileLocation.structure.AddPOI(this, tileLocation);
        if (!traitContainer.HasTrait("Burning")) {
            if (tileLocation.genericTileObject.traitContainer.HasTrait("Burning")) {
                traitContainer.AddTrait(this, "Burning", bypassElementalChance: true);
            }
            //Commented out because this should not happen since you can only unseize a tile object on a tile that has no object
            //else if (tileLocation.objHere != null && tileLocation.objHere.traitContainer.HasTrait("Burning")) {
            //    traitContainer.AddTrait(this, "Burning", bypassElementalChance: true);
            //}
        }
    }
    public virtual bool CollectsLogs() {
        return true;
    }
    public void CancelRemoveStatusFeedAndRepairJobsTargetingThis() {
        for (int i = 0; i < allJobsTargetingThis.Count; i++) {
            JobQueueItem job = allJobsTargetingThis[i];
            if(job.jobType == JOB_TYPE.REMOVE_STATUS || job.jobType == JOB_TYPE.REPAIR || job.jobType == JOB_TYPE.FEED) {
                if (job.CancelJob(false)){
                    i--;
                }
            }
        }
    }
    public void AdjustNumOfActionsBeingPerformedOnThis(int amount) {
        numOfActionsBeingPerformedOnThis += amount;
        numOfActionsBeingPerformedOnThis = Mathf.Max(0, numOfActionsBeingPerformedOnThis);
    }
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
    #endregion

    #region Traits
    public ITraitContainer traitContainer { get; private set; }
    public TraitProcessor traitProcessor => TraitManager.tileObjectTraitProcessor;
    public void CreateTraitContainer() {
        traitContainer = new TraitContainer();
    }
    public virtual bool CanBeAffectedByElementalStatus(string traitName) {
        return true;
    }
    protected void ProcessTraitsOnTickStarted() {
        traitContainer.ProcessOnTickStarted(this);
    }
    #endregion

    #region GOAP
    /// <summary>
    /// Does this tile object advertise a given action type.
    /// </summary>
    /// <param name="type">The action type that need to be advertised.</param>
    /// <returns>If this tile object advertises the given action.</returns>
    public bool Advertises(INTERACTION_TYPE type) {
        return advertisedActions != null && advertisedActions.Contains(type);
    }
    /// <summary>
    /// Does this tile object advertise all of the given actions.
    /// </summary>
    /// <param name="types">The action types that need to be advertised.</param>
    /// <returns>If this tile object meets all the requirements.</returns>
    public bool AdvertisesAll(params INTERACTION_TYPE[] types) {
        for (int i = 0; i < types.Length; i++) {
            if (!(Advertises(types[i]))) {
                return false;
            }
        }
        return true;
    }
    #endregion

    #region Tile Object Slots
    protected virtual void OnPlaceTileObjectAtTile(LocationGridTile tile) {
        if (hasCreatedSlots) {
            RepositionTileSlots(tile);
        } else {
            CreateTileObjectSlots();
        }
        //Moved this to Settlement Generation:
        // if (GameManager.Instance.gameHasStarted == false) { //only update owners on initialization.
        //     UpdateOwners();
        // }
        Messenger.Broadcast(GridTileSignals.TILE_OBJECT_PLACED, this, tile);
    }
    private bool HasSlotSettings() {
        return ReferenceEquals(mapVisual.usedSprite, null) == false
               && InnerMapManager.Instance.HasSettingForTileObjectAsset(mapVisual.usedSprite);
    }
    private void CreateTileObjectSlots() {
        if (tileObjectType != TILE_OBJECT_TYPE.GENERIC_TILE_OBJECT && HasSlotSettings()) {
            Sprite usedAsset = mapVisual.usedSprite;
            List<TileObjectSlotSetting> slotSettings = InnerMapManager.Instance.GetTileObjectSlotSettings(usedAsset);
            if(slotsParent == null) {
                slotsParent = Object.Instantiate(InnerMapManager.Instance.tileObjectSlotsParentPrefab, mapVisual.transform);
                slotsParent.transform.localPosition = Vector3.zero;
                slotsParent.name = $"{ToString()} Slots";
            }
            slots = new TileObjectSlotItem[slotSettings.Count];
            for (int i = 0; i < slotSettings.Count; i++) {
                TileObjectSlotSetting currSetting = slotSettings[i];
                GameObject currSlot = Object.Instantiate(InnerMapManager.Instance.tileObjectSlotPrefab, Vector3.zero, Quaternion.identity, slotsParent.transform);
                TileObjectSlotItem currSlotItem = currSlot.GetComponent<TileObjectSlotItem>();
                currSlotItem.ApplySettings(this, currSetting);
                slots[i] = currSlotItem;
            }
        }
        hasCreatedSlots = true;
    }
    private void RepositionTileSlots(LocationGridTile tile) {
        if (ReferenceEquals(slotsParent, null) == false) {
            slotsParent.transform.localPosition = Vector3.zero;
        }
    }
    protected void DestroyTileSlots() {
        if (slots == null) {
            return;
        }
        for (int i = 0; i < slots.Length; i++) {
            Object.Destroy(slots[i].gameObject);
        }
        slots = null;
        hasCreatedSlots = false;
    }
    private TileObjectSlotItem GetNearestUnoccupiedSlot(Character character) {
        float nearest = 9999f;
        TileObjectSlotItem nearestSlot = null;
        for (int i = 0; i < slots.Length; i++) {
            TileObjectSlotItem slot = slots[i];
            if (slot.user == null) {
                float distance = Vector2.Distance(character.marker.transform.position, slot.transform.position);
                if (distance < nearest) {
                    nearest = distance;
                    nearestSlot = slot;
                }
            }
        }
        return nearestSlot;
    }
    private bool HasUnoccupiedSlot() {
        for (int i = 0; i < slots.Length; i++) {
            TileObjectSlotItem slot = slots[i];
            if (slot.user == null) {
                return true;
            }
        }
        return false;
    }
    private TileObjectSlotItem GetSlotUsedBy(Character character) {
        if (slots != null) {
            for (int i = 0; i < slots.Length; i++) {
                TileObjectSlotItem slot = slots[i];
                if (slot.user == character) {
                    return slot;
                }
            }
        }
        return null;
    }
    public void SetSlotColor(Color color) {
        if (slots != null) {
            for (int i = 0; i < slots.Length; i++) {
                TileObjectSlotItem slot = slots[i];
                slot.SetSlotColor(color);
            }
        }
    }
    public void SetSlotAlpha(float alpha) {
        if (slots != null) {
            for (int i = 0; i < slots.Length; i++) {
                TileObjectSlotItem slot = slots[i];
                Color color = slot.spriteRenderer.color;
                color.a = alpha;
                slot.SetSlotColor(color);
            }
        }
    }
    public void RevalidateTileObjectSlots() {
        if (hasCreatedSlots) {
            DestroyTileSlots();
            CreateTileObjectSlots();
        }
    }
    #endregion

    #region Users
    public virtual bool AddUser(Character newUser) {
        if (users.Contains(newUser)) {
            return true;
        }
        TileObjectSlotItem availableSlot = GetNearestUnoccupiedSlot(newUser);
        if (availableSlot != null) {
            newUser.SetTileObjectLocation(this);
            availableSlot.Use(newUser);
            if (!HasUnoccupiedSlot()) {
                SetPOIState(POI_STATE.INACTIVE);
            }
            Messenger.Broadcast(TileObjectSignals.ADD_TILE_OBJECT_USER, this, newUser);
        }
        return true;
    }
    public virtual bool RemoveUser(Character user) {
        TileObjectSlotItem slot = GetSlotUsedBy(user);
        if (slot != null) {
            user.SetTileObjectLocation(null);
            slot.StopUsing();
            SetPOIState(POI_STATE.ACTIVE);
            Messenger.Broadcast(TileObjectSignals.REMOVE_TILE_OBJECT_USER, this, user);
            return true;
        }
        return false;
    }
    #endregion

    #region Utilities
    public void DoCleanup() {
        traitContainer?.RemoveAllTraitsAndStatuses(this);
    }
    public void UpdateOwners() {
        if (gridTileLocation.structure is Dwelling || gridTileLocation.structure is VampireCastle) {
            LocationStructure dwelling = gridTileLocation.structure;
            //update character owner if object's current character owner is null or is not a resident of the dwelling that it is currently in.
            if (dwelling.residents.Count > 0 && (characterOwner == null || (characterOwner != null && dwelling.residents.Contains(characterOwner) == false))) {
                //Characters that are not part of major factions should not own items inside houses
                //To avoid this we need to check if the resident is part of a major faction
                //The easiest way to do this is to create a separate list containing residents of major factions and then randomize between them
                //But this would mean that we will have to create a new list every time UpdateOwners is called
                //This will probably consume memory and will result in memory fragmentation
                //So I decided to create a randomization using loop
                //This is not a perfect random, but I think it will do the trick
                Character randomFinalResident = null;
                Character chosenResident = null;
                for (int i = 0; i < dwelling.residents.Count; i++) {
                    Character resident = dwelling.residents[i];
                    if(resident.faction != null && resident.faction.isMajorFaction) {
                        if (GameUtilities.RollChance(50)) {
                            chosenResident = resident;
                            break;
                        } else if (randomFinalResident == null) {
                            randomFinalResident = resident;
                        }
                    }
                }
                if(chosenResident == null) {
                    if(randomFinalResident != null) {
                        SetCharacterOwner(randomFinalResident);
                    } else {
                        SetCharacterOwner(null);
                    }
                } else {
                    SetCharacterOwner(chosenResident);
                }
            }
        }    
        
    }
    // public void SetIsBeingCarriedBy(Character carrier) {
    //     isBeingCarriedBy = carrier;
    // }
    public virtual bool CanBeDamaged() {
        return mapObjectState != MAP_OBJECT_STATE.UNBUILT && traitContainer.HasTrait("Indestructible") == false;
    }
    private void OccupyTiles(Point size, LocationGridTile tile) {
        List<LocationGridTile> overlappedTiles = tile.parentMap.GetTiles(size, tile);
        for (int i = 0; i < overlappedTiles.Count; i++) {
            LocationGridTile currTile = overlappedTiles[i];
            currTile.SetTileState(LocationGridTile.Tile_State.Occupied);
        }
    }
    private void UnoccupyTiles(Point size, LocationGridTile tile) {
        List<LocationGridTile> overlappedTiles = tile.parentMap.GetTiles(size, tile);
        for (int i = 0; i < overlappedTiles.Count; i++) {
            LocationGridTile currTile = overlappedTiles[i];
            currTile.SetTileState(LocationGridTile.Tile_State.Empty);
        }
    }
    public bool IsOwnedBy(Character character) {
        //return owners != null && owners.Contains(character);
        return characterOwner == character;
    }
    //public List<Character> GetOwners() {
    //    //if(gridTileLocation != null && gridTileLocation.structure is Dwelling) {
    //    //    return (gridTileLocation.structure as Dwelling).residents;
    //    //}
    //    //return null;
    //    return owners;
    //}
    // public void SetFactionOwner(Faction factionOwner) {
    //     this.factionOwner = factionOwner;
    // }
    /// <summary>
    /// Set which character officially owns this item.
    /// </summary>
    /// <param name="characterOwner">The character that should own this item.</param>
    public virtual void SetCharacterOwner(Character characterOwner) {
        if(this.characterOwner != characterOwner) {
            if(characterOwner != null && this is Heirloom) {
                //Cannot own heirlooms
                return;
            }
            Character prevOwner = this.characterOwner;
            this.characterOwner = characterOwner;
            if (prevOwner != null) {
                prevOwner.RemoveOwnedItem(this);
            }
            if (this.characterOwner == null) {
                RemoveAdvertisedAction(INTERACTION_TYPE.STEAL);
            } else {
                this.characterOwner.AddOwnedItem(this);
                AddAdvertisedAction(INTERACTION_TYPE.STEAL);
            }
        }
    }
    /// <summary>
    /// Set who currently has this item.
    /// This can be any character, and is not limited to just the owner of this object (i.e. if the object was stolen)
    /// </summary>
    /// <param name="character"></param>
    public virtual void SetInventoryOwner(Character character) {
        this.isBeingCarriedBy = character;
        Debug.Log($"Set Carried by character of item {this.ToString()} to {(isBeingCarriedBy?.name ?? "null")}");
    }
    public bool CanBePickedUpNormallyUponVisionBy(Character character) {
        // if (tileObjectType != TILE_OBJECT_TYPE.HEALING_POTION && tileObjectType != TILE_OBJECT_TYPE.TOOL) {
        //     return false;
        // }
        if (tileObjectType.IsTileObjectAnItem() == false) {
            return false;
        }
        if (!character.isNormalCharacter) {
            return false;
        }
        //if (GameUtilities.IsRaceBeast(character.race)) {
        //    return false;
        //}
        //if (character.race == RACE.SKELETON) {
        //    return false;
        //}
        //if (character.characterClass.className.Equals("Zombie")) {
        //    return false;
        //}
        if (mapObjectState != MAP_OBJECT_STATE.BUILT) {
            return false;
        }
        if (numOfActionsBeingPerformedOnThis > 0) {
            return false;
        }
        //characters should not pick up items if that item is the target of it's current action
        if (character.currentActionNode != null && character.currentActionNode.poiTarget == this) {
            return false;
        }
        if (Advertises(INTERACTION_TYPE.PICK_UP) == false) {
            return false;
        }
        bool cannotBePickedUp = IsOwnedBy(character) && gridTileLocation != null && gridTileLocation.structure == character.homeStructure;
        if (cannotBePickedUp) {
            return false;
        }
        if (characterOwner == null || IsOwnedBy(character)) {
            //if the item is at a tile that is part of a npcSettlement and that tile is part of that settlements main storage, do not allow pick up
            //Temporarily removed this because when the player unseize a necronomicon in the storage area, it does not get picked up
            //if (gridTileLocation != null && gridTileLocation.IsPartOfSettlement(out var settlement) 
            //    && settlement is NPCSettlement npcSettlement
            //    && gridTileLocation.structure == npcSettlement.mainStorage) {
            //    return false;
            //}
            return true;
        } else {
            //https://www.notion.so/ruinarch/c818a20f153d4b6ea49e823a04515394?v=5767c0da06f347549ce48dd37384af59&p=7e7a8a17d989411eaec3ebe4ba4b5460
            //Can be picked up even if item has owner if character is Evil, Treacherous or Kleptomaniac
            if(character.traitContainer.HasTrait("Evil", "Treacherous", "Kleptomaniac")) {
                return true;
            }
        }
        return false;
    }
    protected TileObject GetBase() {
        return this;
    }
    public void SetIsPreplaced(bool state) {
        isPreplaced = state;
    }
    public void AdjustRepairCounter(int amount) {
        repairCounter += amount;
    }
    public void AddCharacterThatAlreadyAssumed(Character character) {
        charactersThatAlreadyAssumed.Add(character);
    }
    public bool HasCharacterAlreadyAssumed(Character character) {
        return charactersThatAlreadyAssumed.Contains(character);
    }
    public bool IsInHomeStructureOfCharacterWithOpinion(Character character, params string[] opinion) {
        if(gridTileLocation != null && structureLocation != null) {
            for (int i = 0; i < structureLocation.residents.Count; i++) {
                Character resident = structureLocation.residents[i];
                string opinionLabel = character.relationshipContainer.GetOpinionLabel(resident);
                for (int j = 0; j < opinion.Length; j++) {
                    if (opinionLabel == opinion[j]) {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    #endregion

    #region Inspect
    public virtual void OnInspect(Character inspector) { //, out Log log
        //if (LocalizationManager.Instance.HasLocalizedValue("TileObject", this.GetType().ToString(), "on_inspect")) {
        //    log = GameManager.CreateNewLog(GameManager.Instance.Today(), "TileObject", this.GetType().ToString(), "on_inspect");
        //} else {
        //    log = null;
        //}
        
    }
    #endregion

    #region Map Object
    protected override void CreateMapObjectVisual() {
        GameObject obj = InnerMapManager.Instance.mapObjectFactory.CreateNewTileObjectMapVisual(tileObjectType);
        mapVisual = obj.GetComponent<TileObjectGameObject>();
    }
    protected override void OnMapObjectStateChanged() {
        if (mapVisual == null) { return; }
        if (mapObjectState == MAP_OBJECT_STATE.UNBUILT) {
            OnSetObjectAsUnbuilt();
        } else if (mapObjectState == MAP_OBJECT_STATE.BUILDING) {
            OnSetObjectAsBuilding();
        } else {
            OnSetObjectAsBuilt();
        }
    }
    protected virtual void OnSetObjectAsUnbuilt() {
        mapVisual.SetVisualAlpha(0f / 255f);
        SetSlotAlpha(0f / 255f);
        SetPOIState(POI_STATE.INACTIVE);
        AddAdvertisedAction(INTERACTION_TYPE.CRAFT_TILE_OBJECT);
        UnsubscribeListeners();
        Messenger.AddListener(TileObjectSignals.CHECK_UNBUILT_OBJECT_VALIDITY, CheckUnbuiltObjectValidity);
        if (gridTileLocation != null) {
            //remove tile object from region count.
            gridTileLocation.parentMap.region.RemoveTileObjectInRegion(this);    
        }
    }
    protected virtual void OnSetObjectAsBuilding() {
        mapVisual.SetVisualAlpha(128f / 255f);
        SetSlotAlpha(128f / 255f);
        Messenger.RemoveListener(TileObjectSignals.CHECK_UNBUILT_OBJECT_VALIDITY, CheckUnbuiltObjectValidity);
    }
    protected virtual void OnSetObjectAsBuilt(){
        Messenger.RemoveListener(TileObjectSignals.CHECK_UNBUILT_OBJECT_VALIDITY, CheckUnbuiltObjectValidity);
        mapVisual.SetVisualAlpha(255f / 255f);
        SetSlotAlpha(255f / 255f);
        SetPOIState(POI_STATE.ACTIVE);
        if (advertisedActions != null && advertisedActions.Count > 0) {
            RemoveAdvertisedAction(INTERACTION_TYPE.CRAFT_TILE_OBJECT);
        }
        SubscribeListeners();
        if (gridTileLocation != null) {
            //add tile object to region count. This will be called at DefaultProcessOnPlacePOI
            gridTileLocation.parentMap.region.AddTileObjectInRegion(this);    
        }
    }
    private void CheckUnbuiltObjectValidity() {
        if (allExistingJobsTargetingThis.Count <= 0) {
            //unbuilt object is no longer valid, remove it
            Messenger.RemoveListener(TileObjectSignals.CHECK_UNBUILT_OBJECT_VALIDITY, CheckUnbuiltObjectValidity);
            Messenger.Broadcast(JobSignals.CHECK_APPLICABILITY_OF_ALL_JOBS_TARGETING,  this as IPointOfInterest);
            List<JobQueueItem> jobs = new List<JobQueueItem>(allJobsTargetingThis);
            jobs.AddRange(allExistingJobsTargetingThis);
            for (int i = 0; i < jobs.Count; i++) {
                JobQueueItem jobQueueItem = jobs[i];
                jobQueueItem.CancelJob(false);
            }
            gridTileLocation?.structure.RemovePOI(this);
            Debug.Log($"{GameManager.Instance.TodayLogString()}Unbuilt object {this} was removed!");
        }
    }
    #endregion

    #region Resources
    public void ConstructResources() {
        storedResources = new Dictionary<RESOURCE, int>() {
            { RESOURCE.FOOD, 0 },
            { RESOURCE.WOOD, 0 },
            { RESOURCE.STONE, 0 },
            { RESOURCE.METAL, 0 },
        };
        ConstructMaxResources();
    }
    public void LoadResources(SaveDataTileObject saveDataTileObject) {
        // storedResources = saveDataTileObject.storedResources;
        Assert.IsTrue(saveDataTileObject.resourceValues.Length == 4, $"Resource values in {this} save data is inconsistent with actual resource dictionary");
        storedResources = new Dictionary<RESOURCE, int>() {
            { RESOURCE.FOOD, saveDataTileObject.resourceValues[0] },
            { RESOURCE.WOOD, saveDataTileObject.resourceValues[1] },
            { RESOURCE.STONE, saveDataTileObject.resourceValues[2] },
            { RESOURCE.METAL, saveDataTileObject.resourceValues[3] },
        };
        ConstructMaxResources();
    }
    protected virtual void ConstructMaxResources() {
        maxResourceValues = new Dictionary<RESOURCE, int>();
        RESOURCE[] resourceTypes = CollectionUtilities.GetEnumValues<RESOURCE>();
        for (int i = 0; i < resourceTypes.Length; i++) {
            RESOURCE resourceType = resourceTypes[i];
            maxResourceValues.Add(resourceType, 1000);
        }
    }
    public void SetResource(RESOURCE resourceType, int amount) {
        storedResources[resourceType] = amount;
        storedResources[resourceType] = Mathf.Max(storedResources[resourceType], 0);
    }
    public void AdjustResource(RESOURCE resourceType, int amount) {
        storedResources[resourceType] += amount;
        storedResources[resourceType] = Mathf.Max(storedResources[resourceType], 0);
    }
    public bool HasResourceAmount(RESOURCE resourceType, int amount) {
        return storedResources[resourceType] >= amount;
    }
    public bool IsAtMaxResource(RESOURCE resource) {
        return storedResources[resource] >= maxResourceValues[resource];
    }
    public int GetMaxResourceValue(RESOURCE resource) {
        return maxResourceValues[resource];
    }
    public bool HasEnoughSpaceFor(RESOURCE resource, int amount) {
        int newAmount = storedResources[resource] + amount;
        return newAmount <= maxResourceValues[resource];
    }
    #endregion

    public override string ToString() {
        return $"{name} {id.ToString()}";
    }

    #region Player Action Target
    public void AddPlayerAction(PLAYER_SKILL_TYPE action) {
        if (actions.Contains(action) == false) {
            actions.Add(action);
            Messenger.Broadcast(SpellSignals.PLAYER_ACTION_ADDED_TO_TARGET, action, this as IPlayerActionTarget);    
        }
    }
    public void RemovePlayerAction(PLAYER_SKILL_TYPE action) {
        if (actions.Remove(action)) {
            Messenger.Broadcast(SpellSignals.PLAYER_ACTION_REMOVED_FROM_TARGET, action, this as IPlayerActionTarget);
        }
    }
    public void ClearPlayerActions() {
        actions.Clear();
    }
    #endregion

    #region Selectable
    public bool IsCurrentlySelected() {
        return UIManager.Instance.tileObjectInfoUI.isShowing &&
               UIManager.Instance.tileObjectInfoUI.activeTileObject == this;
    }
    public void LeftSelectAction() {
        if (mapObjectVisual != null) {
            mapObjectVisual.ExecuteClickAction(PointerEventData.InputButton.Left);    
        } else {
            UIManager.Instance.ShowTileObjectInfo(this);    
        }
    }
    public void RightSelectAction() {
        mapObjectVisual.ExecuteClickAction(PointerEventData.InputButton.Right);
    }
    public void MiddleSelectAction() {
        mapObjectVisual.ExecuteClickAction(PointerEventData.InputButton.Middle);
    }
    public virtual bool CanBeSelected() {
        return true;
    }
    #endregion

    #region Logs
    /// <summary>
    /// Called when this object has been added as a filler in a log.
    /// </summary>
    public virtual void OnReferencedInALog() { }
    #endregion
}

[System.Serializable]
public struct TileObjectSlotSetting {
    public string slotName;
    public Vector3 characterPosition;
    public Vector3 usedPosition;
    public Vector3 unusedPosition;
    public Vector3 assetRotation;
    public Sprite slotAsset;
}

[System.Serializable]
public struct TileObjectSerializableData {
    public int id;
    public TILE_OBJECT_TYPE type;
}