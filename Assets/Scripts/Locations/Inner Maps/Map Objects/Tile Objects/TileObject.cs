using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BayatGames.SaveGameFree.Types;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Pathfinding;
using Traits;
using UnityEngine.Assertions;
using UnityEngine.Experimental.U2D;
using UtilityScripts;
using UnityEngine.EventSystems;

public abstract class TileObject : MapObject<TileObject>, IPointOfInterest, IPlayerActionTarget {
    public string name { get; protected set; }
    public int id { get; private set; }
    public TILE_OBJECT_TYPE tileObjectType { get; private set; }
    public Character characterOwner { get; private set; }
    public List<INTERACTION_TYPE> advertisedActions { get; protected set; }
    public Region currentRegion => gridTileLocation.structure.location.coreTile.region;
    public LocationStructure structureLocation => gridTileLocation.structure;
    public bool isDisabledByPlayer { get; private set; }
    public bool isSummonedByPlayer { get; private set; }
    public bool isPreplaced { get; private set; }
    public List<JobQueueItem> allJobsTargetingThis { get; private set; }
    private List<Character> owners { get; set; }
    public Character isBeingCarriedBy { get; private set; }
    public virtual Character[] users { //array of characters, currently using the tile object
        get {
            return slots?.Where(x => x != null && x.user != null).Select(x => x.user).ToArray() ?? null;
        }
    }
    private Character removedBy { get; set; }
    public BaseMapObjectVisual mapObjectVisual => mapVisual;

    //hp
    public int maxHP { get; private set; }
    public int currentHP { get; protected set; }

    ///this is null by default. This is responsible for updating the pathfinding graph when a tileobject that should be unapassable is placed
    /// <see cref="LocationGridTileGUS.Initialize(Vector2,Vector2,IPointOfInterest)"/>,
    /// this should also destroyed when the object is removed. <see cref="LocationGridTileGUS.Destroy"/>
    private LocationGridTileGUS graphUpdateScene { get; set; } 

    //tile slots
    private TileObjectSlotItem[] slots { get; set; } //for users
    private GameObject slotsParent;
    protected bool hasCreatedSlots;

    public virtual LocationGridTile gridTileLocation { get; protected set; }
    public POI_STATE state { get; private set; }
    public LocationGridTile previousTile { get; private set; }
    public Dictionary<RESOURCE, int> storedResources { get; private set; }
    protected Dictionary<RESOURCE, int> maxResourceValues { get; set; }
    public List<SPELL_TYPE> actions { get; protected set; }

    private bool hasSubscribedToListeners;

    public LogComponent logComponent { get; protected set; }
    
    #region getters
    public POINT_OF_INTEREST_TYPE poiType => POINT_OF_INTEREST_TYPE.TILE_OBJECT;
    public Vector3 worldPosition => mapVisual.transform.position;
    public virtual Vector2 selectableSize => Vector2Int.one;
    public bool isDead => gridTileLocation == null; //Consider the object as dead if it no longer has a tile location (has been removed)
    public ProjectileReceiver projectileReceiver => mapVisual.visionTrigger.projectileReceiver;
    public Transform worldObject => mapVisual != null ? mapVisual.transform : null;
    public string nameWithID => ToString();
    public GameObject visualGO => mapVisual.gameObject;
    //public Character isBeingCarriedBy => carriedByCharacter;
    public Faction factionOwner => characterOwner?.faction;
    #endregion

    protected void Initialize(TILE_OBJECT_TYPE tileObjectType, bool shouldAddCommonAdvertisements = true) {
        id = UtilityScripts.Utilities.SetID(this);
        this.tileObjectType = tileObjectType;
        name = GenerateName();
        allJobsTargetingThis = new List<JobQueueItem>();
        owners = new List<Character>();
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
        logComponent = new LogComponent(this);
        InnerMapManager.Instance.AddTileObject(this);
        SubscribeListeners();
    }
    protected void Initialize(SaveDataTileObject data, bool shouldAddCommonAdvertisements = true) {
        id = UtilityScripts.Utilities.SetID(this, data.id);
        tileObjectType = data.tileObjectType;
        new List<string>();
        allJobsTargetingThis = new List<JobQueueItem>();
        owners = new List<Character>();
        hasCreatedSlots = false;
        CreateTraitContainer();
        if (shouldAddCommonAdvertisements) {
            AddCommonAdvertisements();
        }
        ConstructResources();
        ConstructDefaultActions();
        logComponent = new LogComponent(this);
        InnerMapManager.Instance.AddTileObject(this);
        SubscribeListeners();
    }
    private void AddCommonAdvertisements() {
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        //AddAdvertisedAction(INTERACTION_TYPE.POISON);
        //AddAdvertisedAction(INTERACTION_TYPE.REMOVE_POISON);
        AddAdvertisedAction(INTERACTION_TYPE.REPAIR);
        AddAdvertisedAction(INTERACTION_TYPE.BOOBY_TRAP);
        //AddAdvertisedAction(INTERACTION_TYPE.SCRAP);
        //AddAdvertisedAction(INTERACTION_TYPE.DROP_ITEM);
        //AddAdvertisedAction(INTERACTION_TYPE.PICK_UP);
    }
    protected void RemoveCommonAdvertisements() {
        RemoveAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        //RemoveAdvertisedAction(INTERACTION_TYPE.POISON);
        //RemoveAdvertisedAction(INTERACTION_TYPE.REMOVE_POISON);
        RemoveAdvertisedAction(INTERACTION_TYPE.REPAIR);
        RemoveAdvertisedAction(INTERACTION_TYPE.BOOBY_TRAP);
        //RemoveAdvertisedAction(INTERACTION_TYPE.SCRAP);
        //RemoveAdvertisedAction(INTERACTION_TYPE.DROP_ITEM);
        //RemoveAdvertisedAction(INTERACTION_TYPE.PICK_UP);
    }

    #region Listeners
    protected void SubscribeListeners() {
        // if (hasSubscribedToListeners == false) {
        //     hasSubscribedToListeners = true;
        //     Messenger.AddListener(Signals.TICK_ENDED, () => traitContainer.ProcessOnTickEnded(this));    
        // }
    }
    protected void UnsubscribeListeners() {
        // if (hasSubscribedToListeners) {
        //     hasSubscribedToListeners = false;
        //     Messenger.RemoveListener(Signals.TICK_ENDED, () => traitContainer.ProcessOnTickEnded(this));    
        // }
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

    public virtual void OnDestroyPOI() {
        //DisableGameObject();
        OnRemoveTileObject(null, previousTile);
        DestroyMapVisualGameObject();
        SetPOIState(POI_STATE.INACTIVE);
        if (TileObjectDB.TryGetTileObjectData(tileObjectType, out var objData)) {
            if (objData.occupiedSize.X > 1 || objData.occupiedSize.Y > 1) {
                UnoccupyTiles(objData.occupiedSize, previousTile);
            }
        }
        Messenger.Broadcast(Signals.CHECK_APPLICABILITY_OF_ALL_JOBS_TARGETING, this as IPointOfInterest);
        UnsubscribeListeners();
    }
    public virtual void OnPlacePOI() {
        // Assert.IsNull(gridTileLocation, $"Grid tile location of {this.ToString()} is null, but OnPlacePOI was called!");
        SetPOIState(POI_STATE.ACTIVE);
        if (mapVisual == null) {
            InitializeMapObject(this);
            OnMapObjectStateChanged(); //update visuals based on map object state
            gridTileLocation.parentMap.region.AddPendingAwareness(this);
        }
        PlaceMapObjectAt(gridTileLocation);
        OnPlaceTileObjectAtTile(gridTileLocation);
        TileObjectData objData;
        if (TileObjectDB.TryGetTileObjectData(tileObjectType, out objData)) {
            if (objData.occupiedSize.X > 1 || objData.occupiedSize.Y > 1) {
                OccupyTiles(objData.occupiedSize, gridTileLocation);
            }
        }
        if (gridTileLocation.collectionOwner.isPartOfParentRegionMap) {
            gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.OnPlacePOIInHex(this);
        }
        SubscribeListeners();
        // Assert.IsTrue((this is MovingTileObject) == false && gridTileLocation.structure.pointsOfInterest.Contains(this), 
        //     $"{this} was placed at {gridTileLocation.structure} but was not included in the list of POI's");
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
    public GoapAction AdvertiseActionsToActor(Character actor, GoapEffect precondition, JobQueueItem job,
        Dictionary<INTERACTION_TYPE, object[]> otherData, ref int cost, ref string log) {
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
                if (!isCharacterAvailable && !action.canBeAdvertisedEvenIfActorIsUnavailable) {
                    //if this character is not available, check if the current action type can be advertised even when the character is inactive.
                    continue; //skip
                }
                if (PathfindingManager.Instance.HasPathEvenDiffRegion(actor.gridTileLocation, gridTileLocation) && RaceManager.Instance.CanCharacterDoGoapAction(actor, currType)) {
                    object[] data = null;
                    if (otherData != null) {
                        if (otherData.ContainsKey(currType)) {
                            data = otherData[currType];
                        } else if (otherData.ContainsKey(INTERACTION_TYPE.NONE)) {
                            data = otherData[INTERACTION_TYPE.NONE];
                        }
                    }
                    //object[] otherActionData = null;
                    //if (otherData.ContainsKey(currType)) {
                    //    otherActionData = otherData[currType];
                    //}
                    if (action.CanSatisfyRequirements(actor, this, data)
                        && action.WillEffectsSatisfyPrecondition(precondition, actor, this, data)) { //&& InteractionManager.Instance.CanSatisfyGoapActionRequirementsOnBuildTree(currType, actor, this, data)
                        int actionCost = action.GetCost(actor, this, job, data);
                        log += $"({actionCost}){action.goapName}-{nameWithID}, ";
                        if (lowestCostAction == null || actionCost < currentLowestCost) {
                            lowestCostAction = action;
                            currentLowestCost = actionCost;
                        }
                        //GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(currType, actor, this);
                        //if (goapAction != null) {
                        //    if (data != null) {
                        //        goapAction.InitializeOtherData(data);
                        //    }
                        //    usableActions.Add(goapAction);
                        //} else {
                        //    throw new System.Exception("Goap action " + currType.ToString() + " is null!");
                        //}
                    }
                }
            }
            cost = currentLowestCost;
            chosenAction = lowestCostAction;
            //return usableActions;
        }
        return chosenAction;
    }
    public bool CanAdvertiseActionToActor(Character actor, GoapAction action, JobQueueItem job,
        Dictionary<INTERACTION_TYPE, object[]> otherData, ref int cost) {
        if ((IsAvailable() || action.canBeAdvertisedEvenIfActorIsUnavailable)
            && advertisedActions != null && advertisedActions.Contains(action.goapType)
            && actor.trapStructure.SatisfiesForcedStructure(this)
            && RaceManager.Instance.CanCharacterDoGoapAction(actor, action.goapType)
            && PathfindingManager.Instance.HasPathEvenDiffRegion(actor.gridTileLocation, gridTileLocation)) {
            object[] data = null;
            if (otherData != null) {
                if (otherData.ContainsKey(action.goapType)) {
                    data = otherData[action.goapType];
                } else if (otherData.ContainsKey(INTERACTION_TYPE.NONE)) {
                    data = otherData[INTERACTION_TYPE.NONE];
                }
            }
            if (action.CanSatisfyRequirements(actor, this, data)) {
                cost = action.GetCost(actor, this, job, data);
                return true;
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
        this.removedBy = removedBy;
        Messenger.Broadcast(Signals.TILE_OBJECT_REMOVED, this, removedBy, removedFrom);
        if (hasCreatedSlots && destroyTileSlots) {
            DestroyTileSlots();
        }
        if (removeTraits) {
            traitContainer.RemoveAllTraitsAndStatuses(this);
        }
    }
    public virtual bool CanBeReplaced() {
        return false;
    }
    public virtual void OnTileObjectGainedTrait(Trait trait) {
        if (trait is Status status && status.IsTangible() && mapObjectVisual != null) {
            mapObjectVisual.visionTrigger.VoteToMakeVisibleToCharacters();
        }
    }
    public virtual void OnTileObjectLostTrait(Trait trait) {
        if (trait is Status status && status.IsTangible() && mapObjectVisual != null) {
            mapObjectVisual.visionTrigger.VoteToMakeInvisibleToCharacters();
        }
    }
    public virtual bool IsValidCombatTarget() {
        return gridTileLocation != null;
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
        actions = new List<SPELL_TYPE>();

        if (tileObjectType == TILE_OBJECT_TYPE.RAVENOUS_SPIRIT || tileObjectType == TILE_OBJECT_TYPE.FEEBLE_SPIRIT ||
            tileObjectType == TILE_OBJECT_TYPE.FORLORN_SPIRIT) {
            return;
        }
        //PlayerAction destroyAction = new PlayerAction(PlayerDB.Destroy_Action,
        //    () => PlayerManager.Instance.allSpellsData[SPELL_TYPE.DESTROY].CanPerformAbilityTowards(this),
        //    null,
        //    () => PlayerManager.Instance.allSpellsData[SPELL_TYPE.DESTROY].ActivateAbility(this));
        //PlayerAction igniteAction = new PlayerAction(PlayerDB.Ignite_Action,
        //    () => PlayerManager.Instance.allSpellsData[SPELL_TYPE.IGNITE].CanPerformAbilityTowards(this),
        //    null,
        //    () => PlayerManager.Instance.allSpellsData[SPELL_TYPE.IGNITE].ActivateAbility(this));
        //PlayerAction poisonAction = new PlayerAction(PlayerDB.Poison_Action,
        //    () => PlayerManager.Instance.allSpellsData[SPELL_TYPE.POISON].CanPerformAbilityTowards(this),
        //    null,
        //    () => PlayerManager.Instance.allSpellsData[SPELL_TYPE.POISON].ActivateAbility(this));
        //PlayerAction seizeAction = new PlayerAction(PlayerDB.Seize_Object_Action,
        //    () => !PlayerManager.Instance.player.seizeComponent.hasSeizedPOI && this.mapVisual != null && (this.isBeingCarriedBy != null || this.gridTileLocation != null),
        //    null,
        //    () => PlayerManager.Instance.player.seizeComponent.SeizePOI(this));

        AddPlayerAction(SPELL_TYPE.DESTROY);
        AddPlayerAction(SPELL_TYPE.IGNITE);
        AddPlayerAction(SPELL_TYPE.POISON);
        AddPlayerAction(SPELL_TYPE.SEIZE_OBJECT);
    }
    public virtual void ActivateTileObject() {
        Messenger.Broadcast(Signals.INCREASE_THREAT_THAT_SEES_POI, this as IPointOfInterest, 5);
    }
    #endregion

    #region IPointOfInterest
    public bool IsAvailable() {
        return state != POI_STATE.INACTIVE && !isDisabledByPlayer;
    }
    public void SetIsDisabledByPlayer(bool state) {
        if(isDisabledByPlayer != state) {
            isDisabledByPlayer = state;
            if (isDisabledByPlayer) {
                Character character = null;
                Messenger.Broadcast(Signals.TILE_OBJECT_DISABLED, this, character);
            }
        }
    }
    public void SetIsSummonedByPlayer(bool state) {
        if(isSummonedByPlayer != state) {
            isSummonedByPlayer = state;
            if (isSummonedByPlayer) {
                if(advertisedActions == null) {
                    advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
                }
                if (!advertisedActions.Contains(INTERACTION_TYPE.INSPECT)) {
                    advertisedActions.Add(INTERACTION_TYPE.INSPECT);
                }
            } else {
                if (advertisedActions != null) {
                    advertisedActions.Remove(INTERACTION_TYPE.INSPECT);
                }
            }
        }
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
    }
    public bool RemoveJobTargetingThis(JobQueueItem job) {
        return allJobsTargetingThis.Remove(job);
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
        if (CanBeDamaged() == false) { return; }
        if (currentHP == 0 && amount < 0) { return; } //hp is already at minimum, do not allow any more negative adjustments
        CombatManager.Instance.DamageModifierByElements(ref amount, elementalDamageType, this);
        //int supposedHP = currentHP + amount;
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
        if (amount < 0) {
            Character responsibleCharacter = null;
            if (source is Character character) {
                responsibleCharacter = character;
            }
            CombatManager.ElementalTraitProcessor etp = elementalTraitProcessor ?? 
                                                        CombatManager.Instance.DefaultElementalTraitProcessor;
            CombatManager.Instance.ApplyElementalDamage(amount, elementalDamageType, this, 
                responsibleCharacter, etp);
        }
        LocationGridTile tile = gridTileLocation;
        if (currentHP <= 0) {
            //object has been destroyed
            Character removed = null;
            if (source is Character character) {
                removed = character;
            }
            gridTileLocation?.structure.RemovePOI(this, removed);
        }
        if (amount < 0) {
            Messenger.Broadcast(Signals.OBJECT_DAMAGED, this as IPointOfInterest);
        } else if (currentHP == maxHP) {
            Messenger.Broadcast(Signals.OBJECT_REPAIRED, this as IPointOfInterest);
        }
        if (isPreplaced && tile != null && tile.structure is DemonicStructure demonicStructure) {
            demonicStructure.AdjustHP(amount);
        }
    }
    public void OnHitByAttackFrom(Character characterThatAttacked, CombatState state, ref string attackSummary) {
        ELEMENTAL_TYPE elementalType = characterThatAttacked.combatComponent.elementalDamage.type;
        //GameManager.Instance.CreateHitEffectAt(this, elementalType);
        if (currentHP <= 0) {
            return; //if hp is already 0, do not deal damage
        }
        AdjustHP(-characterThatAttacked.attackPower, elementalType, source: characterThatAttacked, showHPBar: true);
        attackSummary = $"{attackSummary}\nDealt damage {characterThatAttacked.attackPower.ToString()}";
        if (currentHP <= 0) {
            attackSummary = $"{attackSummary}\n{name}'s hp has reached 0.";
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
        Messenger.Broadcast(Signals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, this as IPointOfInterest, "");
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
        Messenger.Broadcast(Signals.CHECK_APPLICABILITY_OF_ALL_JOBS_TARGETING, this as IPointOfInterest);
        UnsubscribeListeners();
    }
    public void OnUnseizePOI(LocationGridTile tileLocation) {
        DestroyMapVisualGameObject();
        tileLocation.structure.AddPOI(this, tileLocation);
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
    #endregion

    #region GOAP
    private void ConstructInitialGoapAdvertisements() {
        advertisedActions.Add(INTERACTION_TYPE.INSPECT);
    }
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
        removedBy = null;
        CheckFurnitureSettings();
        if (hasCreatedSlots) {
            RepositionTileSlots(tile);
        } else {
            CreateTileObjectSlots();
        }
        UpdateOwners();
        Messenger.Broadcast(Signals.TILE_OBJECT_PLACED, this, tile);
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
    public virtual void AddUser(Character newUser) {
        if (users.Contains(newUser)) {
            return;
        }
        TileObjectSlotItem availableSlot = GetNearestUnoccupiedSlot(newUser);
        if (availableSlot != null) {
            newUser.SetTileObjectLocation(this);
            availableSlot.Use(newUser);
            if (!HasUnoccupiedSlot()) {
                SetPOIState(POI_STATE.INACTIVE);
            }
            Messenger.Broadcast(Signals.ADD_TILE_OBJECT_USER, this, newUser);
        }
    }
    public virtual bool RemoveUser(Character user) {
        TileObjectSlotItem slot = GetSlotUsedBy(user);
        if (slot != null) {
            user.SetTileObjectLocation(null);
            slot.StopUsing();
            SetPOIState(POI_STATE.ACTIVE);
            Messenger.Broadcast(Signals.REMOVE_TILE_OBJECT_USER, this, user);
            return true;
        }
        return false;
    }
    #endregion

    #region Utilities
    public void DoCleanup() {
        traitContainer.RemoveAllTraitsAndStatuses(this);
    }
    public void UpdateOwners() {
        if (gridTileLocation.structure is Dwelling dwelling) {
            owners.Clear();
            owners.AddRange(dwelling.residents);
            //update character owner if object's current character owner is null or is not a resident of the dwelling that it is currently in.
            if (dwelling.residents.Count > 0 && dwelling.residents.Contains(characterOwner) == false) {
                SetCharacterOwner(CollectionUtilities.GetRandomElement(dwelling.residents));    
            }
        }
    }
    // public void SetIsBeingCarriedBy(Character carrier) {
    //     isBeingCarriedBy = carrier;
    // }
    public virtual bool CanBeDamaged() {
        return mapObjectState != MAP_OBJECT_STATE.UNBUILT;
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
        return owners != null && owners.Contains(character);
        //return gridTileLocation != null && character.homeStructure == gridTileLocation.structure;
        //return this.characterOwner == character;
    }
    public List<Character> GetOwners() {
        //if(gridTileLocation != null && gridTileLocation.structure is Dwelling) {
        //    return (gridTileLocation.structure as Dwelling).residents;
        //}
        //return null;
        return owners;
    }
    // public void SetFactionOwner(Faction factionOwner) {
    //     this.factionOwner = factionOwner;
    // }
    public void SetCharacterOwner(Character characterOwner) {
        this.characterOwner = characterOwner;
    }
    public void SetInventoryOwner(Character character) {
        Debug.Log($"Set Carried by character of item {this.ToString()} to {(isBeingCarriedBy?.name ?? "null")}");
        this.isBeingCarriedBy = character;
    }
    public bool CanBePickedUpNormallyUponVisionBy(Character character) {
        // if (tileObjectType != TILE_OBJECT_TYPE.HEALING_POTION && tileObjectType != TILE_OBJECT_TYPE.TOOL) {
        //     return false;
        // }
        if (tileObjectType.IsTileObjectAnItem() == false) {
            return false;
        }
        if (UtilityScripts.GameUtilities.IsRaceBeast(character.race)) {
            return false;
        }
        if (character.race == RACE.SKELETON) {
            return false;
        }
        if (character.characterClass.className.Equals("Zombie")) {
            return false;
        }
        if (characterOwner == null) {
            //if the item is at a tile that is part of a npcSettlement and that tile is part of that settlements main storage, do not allow pick up
            if (gridTileLocation != null && gridTileLocation.IsPartOfSettlement(out var settlement) 
                && settlement is NPCSettlement npcSettlement
                && gridTileLocation.structure == npcSettlement.mainStorage) {
                return false;
            }
            
            //characters should not pick up items if that item is the target of it's current action
            if (character.currentActionNode != null && character.currentActionNode.poiTarget == this) {
                return false;
            }
            if (Advertises(INTERACTION_TYPE.PICK_UP) == false) {
                return false;
            }
            return true;
        }
        return false;
    }
    protected TileObject GetBase() {
        return this;
    }
    public void SetIsPreplaced(bool state) {
        isPreplaced = state;
    }
    #endregion

    #region Inspect
    public virtual void OnInspect(Character inspector) { //, out Log log
        //if (LocalizationManager.Instance.HasLocalizedValue("TileObject", this.GetType().ToString(), "on_inspect")) {
        //    log = new Log(GameManager.Instance.Today(), "TileObject", this.GetType().ToString(), "on_inspect");
        //} else {
        //    log = null;
        //}
        
    }
    #endregion

    #region Graph Updates
    protected void InitializeGUS(Vector2 offset, Vector2 size) {
        if (graphUpdateScene == null) {
            GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool("LocationGridTileGUS", Vector3.zero, Quaternion.identity, mapVisual.transform);//gridTileLocation.parentAreaMap.graphUpdateScenesParent
            LocationGridTileGUS gus = go.GetComponent<LocationGridTileGUS>();
            graphUpdateScene = gus;
        }
        graphUpdateScene.Initialize(offset, size, this);
    }
    public void DestroyExistingGUS() {
        if (graphUpdateScene == null) return;
        graphUpdateScene.Destroy();
        graphUpdateScene = null;
    }
    #endregion

    #region Visuals
    private void CheckFurnitureSettings() {
        if (gridTileLocation.hasFurnitureSpot) {
            if (gridTileLocation.furnitureSpot.TryGetFurnitureSettings(tileObjectType.ConvertTileObjectToFurniture(), out var furnitureSetting)) {
                mapVisual.ApplyFurnitureSettings(furnitureSetting);
            }
        }
    }
    #endregion

    #region Map Object
    protected override void CreateMapObjectVisual() {
        GameObject obj = InnerMapManager.Instance.mapObjectFactory.CreateNewTileObjectMapVisual(tileObjectType);
        mapVisual = obj.GetComponent<TileObjectGameObject>();
    }
    private INTERACTION_TYPE[] storedActions;
    protected override void OnMapObjectStateChanged() {
        if (mapVisual == null) { return; }
        if (mapObjectState == MAP_OBJECT_STATE.UNBUILT) {
            mapVisual.SetVisualAlpha(0f / 255f);
            SetSlotAlpha(0f / 255f);
            //store advertised actions
            if(advertisedActions != null && advertisedActions.Count > 0) {
                storedActions = new INTERACTION_TYPE[advertisedActions.Count];
                for (int i = 0; i < advertisedActions.Count; i++) {
                    storedActions[i] = advertisedActions[i];
                }
                advertisedActions.Clear();
            }
            AddAdvertisedAction(INTERACTION_TYPE.CRAFT_TILE_OBJECT);
            UnsubscribeListeners();
        } else if (mapObjectState == MAP_OBJECT_STATE.BUILDING) {
            mapVisual.SetVisualAlpha(128f / 255f);
            SetSlotAlpha(128f / 255f);
        } else {
            mapVisual.SetVisualAlpha(255f / 255f);
            SetSlotAlpha(255f / 255f);
            if (advertisedActions != null && advertisedActions.Count > 0) {
                RemoveAdvertisedAction(INTERACTION_TYPE.CRAFT_TILE_OBJECT);
            }
            if (storedActions != null) {
                for (int i = 0; i < storedActions.Length; i++) {
                    AddAdvertisedAction(storedActions[i]);
                }
            }
            storedActions = null;
            SubscribeListeners();
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
    public void AddPlayerAction(SPELL_TYPE action) {
        if (actions.Contains(action) == false) {
            actions.Add(action);
            Messenger.Broadcast(Signals.PLAYER_ACTION_ADDED_TO_TARGET, action, this as IPlayerActionTarget);    
        }
    }
    public void RemovePlayerAction(SPELL_TYPE action) {
        if (actions.Remove(action)) {
            Messenger.Broadcast(Signals.PLAYER_ACTION_REMOVED_FROM_TARGET, action, this as IPlayerActionTarget);
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
        mapObjectVisual.ExecuteClickAction(PointerEventData.InputButton.Left);
        // UIManager.Instance.ShowTileObjectInfo(this);
    }
    public void RightSelectAction() {
        mapObjectVisual.ExecuteClickAction(PointerEventData.InputButton.Right);
    }
    public virtual bool CanBeSelected() {
        return true;
    }
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

[System.Serializable]
public class SaveDataTileObject {
    public int id;
    public TILE_OBJECT_TYPE tileObjectType;
    public List<SaveDataTrait> traits;
    public List<int> awareCharactersIDs;
    //public LocationStructure structureLocation { get; protected set; }
    public bool isDisabledByPlayer;
    public bool isSummonedByPlayer;
    //public List<JobQueueItem> allJobsTargettingThis { get; protected set; }
    //public List<Character> owners;

    //public Vector3Save tileID;
    public POI_STATE state;
    public Vector3Save previousTileID;
    public int previousTileAreaID;
    public bool hasCurrentTile;

    public int structureLocationAreaID;
    public int structureLocationID;
    public STRUCTURE_TYPE structureLocationType;

    protected TileObject loadedTileObject;

    public virtual void Save(TileObject tileObject) {
        id = tileObject.id;
        tileObjectType = tileObject.tileObjectType;
        isDisabledByPlayer = tileObject.isDisabledByPlayer;
        isSummonedByPlayer = tileObject.isSummonedByPlayer;
        state = tileObject.state;

        hasCurrentTile = tileObject.gridTileLocation != null;

        if(tileObject.structureLocation != null) {
            structureLocationID = tileObject.structureLocation.id;
            structureLocationAreaID = tileObject.structureLocation.location.id; //TODO: Refactor, because location is no longer guaranteed to be an npcSettlement.
            structureLocationType = tileObject.structureLocation.structureType;
        } else {
            structureLocationID = -1;
            structureLocationAreaID = -1;
        }

        if (tileObject.previousTile != null) {
            previousTileID = new Vector3Save(tileObject.previousTile.localPlace);
            previousTileAreaID = tileObject.previousTile.structure.location.id;
        } else {
            previousTileID = new Vector3Save(0, 0, -1);
            previousTileAreaID = -1;
        }

        traits = new List<SaveDataTrait>();
        for (int i = 0; i < tileObject.traitContainer.allTraitsAndStatuses.Count; i++) {
            SaveDataTrait saveDataTrait = SaveManager.ConvertTraitToSaveDataTrait(tileObject.traitContainer.allTraitsAndStatuses[i]);
            if (saveDataTrait != null) {
                saveDataTrait.Save(tileObject.traitContainer.allTraitsAndStatuses[i]);
                traits.Add(saveDataTrait);
            }
        }

        //awareCharactersIDs = new List<int>();
        //for (int i = 0; i < tileObject.awareCharacters.Count; i++) {
        //    awareCharactersIDs.Add(tileObject.awareCharacters[i].id);
        //}
    }

    public virtual TileObject Load() {
        string tileObjectName = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(tileObjectType.ToString());
        TileObject tileObject = System.Activator.CreateInstance(System.Type.GetType(tileObjectName), this) as TileObject;

        //if(structureLocationID != -1 && structureLocationAreaID != -1) {
        //    NPCSettlement npcSettlement = LandmarkManager.Instance.GetAreaByID(structureLocationAreaID);
        //    tileObject.SetStructureLocation(npcSettlement.GetStructureByID(structureLocationType, structureLocationID));
        //}
        //for (int i = 0; i < awareCharactersIDs.Count; i++) {
        //    tileObject.AddAwareCharacter(CharacterManager.Instance.GetCharacterByID(awareCharactersIDs[i]));
        //}

        tileObject.SetIsDisabledByPlayer(isDisabledByPlayer);
        tileObject.SetIsSummonedByPlayer(isSummonedByPlayer);
        tileObject.SetPOIState(state);

        loadedTileObject = tileObject;
        return loadedTileObject;
    }

    //This is the last to be loaded in SaveDataTileObject, so release loadedTileObject reference
    public virtual void LoadAfterLoadingAreaMap() {
        loadedTileObject = null;
    }

    public void LoadPreviousTileAndCurrentTile() {
        if (previousTileAreaID != -1 && previousTileID.z != -1) {
            // NPCSettlement npcSettlement = LandmarkManager.Instance.GetAreaByID(previousTileAreaID);
            // LocationGridTile tile = npcSettlement.innerMap.map[(int)previousTileID.x, (int)previousTileID.y];
            // tile.structure.AddPOI(loadedTileObject, tile);
            // if (!hasCurrentTile) {
            //     tile.structure.RemovePOI(loadedTileObject);
            // }
        }
    }

    public void LoadTraits() {
        for (int i = 0; i < traits.Count; i++) {
            Character responsibleCharacter = null;
            Trait trait = traits[i].Load(ref responsibleCharacter);
            loadedTileObject.traitContainer.AddTrait(loadedTileObject, trait, responsibleCharacter);
        }
    }
}