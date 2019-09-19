﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileObject : IPointOfInterest {

    public string name { get { return ToString(); } }
    public int id { get; private set; }
    public TILE_OBJECT_TYPE tileObjectType { get; private set; }
    public Faction factionOwner { get { return null; } }
    public List<INTERACTION_TYPE> poiGoapActions { get; protected set; }
    public Area specificLocation { get { return gridTileLocation.structure.location; } }
    protected List<Trait> _traits;
    public virtual List<Trait> normalTraits {
        get { return _traits; }
    }
    public List<Character> awareCharacters { get; private set; } //characters that are aware of this object (Used for checking if a ghost trigger should be destroyed)
    public List<string> actionHistory { get; private set; } //list of actions that was done to this object
    public LocationStructure structureLocation { get; protected set; }
    public bool isDisabledByPlayer { get; protected set; }
    public bool isSummonedByPlayer { get; protected set; }
    public List<JobQueueItem> allJobsTargettingThis { get; protected set; }
    public List<GoapAction> targettedByAction { get; protected set; }
    public List<Character> owners { get; private set; }

    public virtual Character[] users {
        get {
            if (slots == null) {
                return null;
            }
            return slots.Where(x => x != null && x.user != null).Select(x => x.user).ToArray();
        }
    }//array of characters, currently using the tile object

    //tile slots
    public TileObjectSlotItem[] slots { get; protected set; } //for users
    private GameObject slotsParent;
    protected bool hasCreatedSlots;
    private UnityEngine.Tilemaps.TileBase usedAsset;

    protected LocationGridTile tile;
    private POI_STATE _state;
    protected POICollisionTrigger _collisionTrigger;
    protected LocationGridTile previousTile;
    #region getters/setters
    public POINT_OF_INTEREST_TYPE poiType {
        get { return POINT_OF_INTEREST_TYPE.TILE_OBJECT; }
    }
    public LocationGridTile gridTileLocation {
        get { return tile; }
    }
    public POI_STATE state {
        get { return _state; }
    }
    public POICollisionTrigger collisionTrigger {
        get { return _collisionTrigger; }
    }
    #endregion

    protected void Initialize(TILE_OBJECT_TYPE tileObjectType) {
        id = Utilities.SetID(this);
        this.tileObjectType = tileObjectType;
        _traits = new List<Trait>();
        actionHistory = new List<string>();
        awareCharacters = new List<Character>();
        allJobsTargettingThis = new List<JobQueueItem>();
        targettedByAction = new List<GoapAction>();
        owners = new List<Character>();
        hasCreatedSlots = false;
        AddTrait("Flammable");
        InitializeCollisionTrigger();
        InteriorMapManager.Instance.AddTileObject(this);
    }
    protected void Initialize(SaveDataArtifactSlot data, TILE_OBJECT_TYPE tileObjectType) {
        id = Utilities.SetID(this, data.id);
        this.tileObjectType = tileObjectType;
        _traits = new List<Trait>();
        actionHistory = new List<string>();
        awareCharacters = new List<Character>();
        allJobsTargettingThis = new List<JobQueueItem>();
        owners = new List<Character>();
        hasCreatedSlots = false;
        InitializeCollisionTrigger();
        InteriorMapManager.Instance.AddTileObject(this);
    }
    protected void Initialize(SaveDataArtifact data, TILE_OBJECT_TYPE tileObjectType) {
        id = Utilities.SetID(this, data.id);
        this.tileObjectType = tileObjectType;
        _traits = new List<Trait>();
        actionHistory = new List<string>();
        awareCharacters = new List<Character>();
        allJobsTargettingThis = new List<JobQueueItem>();
        owners = new List<Character>();
        hasCreatedSlots = false;
        InitializeCollisionTrigger();
        InteriorMapManager.Instance.AddTileObject(this);
    }

    #region Virtuals
    /// <summary>
    /// Called when a character starts to do an action towards this object.
    /// </summary>
    /// <param name="action">The current action</param>
    public virtual void OnDoActionToObject(GoapAction action) {
        //owner.SetPOIState(POI_STATE.INACTIVE);
        AddActionToHistory(action);
    }
    /// <summary>
    /// Called when a character finished doing an action towards this object.
    /// </summary>
    /// <param name="action">The finished action</param>
    public virtual void OnDoneActionToObject(GoapAction action) {

    }
    /// <summary>
    /// Called when a character cancelled doing an action towards this object.
    /// </summary>
    /// <param name="action">The finished action</param>
    public virtual void OnCancelActionTowardsObject(GoapAction action) {

    }
    public virtual void SetGridTileLocation(LocationGridTile tile) {
        previousTile = this.tile;
        this.tile = tile;
        if (_collisionTrigger == null) {
            InitializeCollisionTrigger();
        }
        if (tile == null) {
            DisableCollisionTrigger();
            OnRemoveTileObject(null, previousTile);
            if (previousTile != null) {
                PlaceGhostCollisionTriggerAt(previousTile);
            }
            SetPOIState(POI_STATE.INACTIVE);
        } else {
            PlaceCollisionTriggerAt(tile);
            OnPlaceObjectAtTile(tile);
            SetPOIState(POI_STATE.ACTIVE);
        }
    }
    public virtual void RemoveTileObject(Character removedBy) {
        LocationGridTile previousTile = this.tile;
        this.tile = null;
        DisableCollisionTrigger();
        OnRemoveTileObject(removedBy, previousTile);
        if (previousTile != null) {
            PlaceGhostCollisionTriggerAt(previousTile);
        }
        SetPOIState(POI_STATE.INACTIVE);
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
    public virtual List<GoapAction> AdvertiseActionsToActor(Character actor, Dictionary<INTERACTION_TYPE, object[]> otherData) {
        if (poiGoapActions != null && poiGoapActions.Count > 0 && gridTileLocation != null) {
            List<GoapAction> usableActions = new List<GoapAction>();
            for (int i = 0; i < poiGoapActions.Count; i++) {
                INTERACTION_TYPE currType = poiGoapActions[i];
                if (RaceManager.Instance.CanCharacterDoGoapAction(actor, currType)) {
                    object[] data = null;
                    if (otherData != null) {
                        if (otherData.ContainsKey(currType)) {
                            data = otherData[currType];
                        } else if (otherData.ContainsKey(INTERACTION_TYPE.NONE)) {
                            data = otherData[INTERACTION_TYPE.NONE];
                        }
                    }
                    //GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(poiGoapActions[i], actor, this);
                    if (InteractionManager.Instance.CanSatisfyGoapActionRequirements(currType, actor, this, data)
                        && InteractionManager.Instance.CanSatisfyGoapActionRequirementsOnBuildTree(currType, actor, this, data)) {
                        GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(currType, actor, this);
                        if (goapAction != null) {
                            if (data != null) {
                                goapAction.InitializeOtherData(data);
                            }
                            usableActions.Add(goapAction);
                        } else {
                            throw new System.Exception("Goap action " + currType.ToString() + " is null!");
                        }
                    }
                }
            }
            return usableActions;
        }

        return null;
    }
    public virtual void SetPOIState(POI_STATE state) {
        _state = state;
    }
    public virtual bool IsOwnedBy(Character character) {
        return owners.Contains(character);
    }
    /// <summary>
    /// Action to do when the player clicks this object on the map.
    /// </summary>
    public virtual void OnClickAction() { Messenger.Broadcast(Signals.HIDE_MENUS); }
    /// <summary>
    /// Triggered when the grid tile location of this object is set to null.
    /// </summary>
    protected virtual void OnRemoveTileObject(Character removedBy, LocationGridTile removedFrom) {
        Messenger.Broadcast(Signals.TILE_OBJECT_REMOVED, this, removedBy, removedFrom);
        if (hasCreatedSlots) {
            DestroyTileSlots();
        }
        RemoveAllTraits();
    }
    public virtual bool CanBeReplaced() {
        return false;
    }
    protected virtual void OnTileObjectGainedTrait(Trait trait) { }
    #endregion

    #region IPointOfInterest
    public bool IsAvailable() {
        return _state != POI_STATE.INACTIVE && !isDisabledByPlayer;
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
                if(poiGoapActions == null) {
                    poiGoapActions = new List<INTERACTION_TYPE>();
                }
                if (!poiGoapActions.Contains(INTERACTION_TYPE.INSPECT)) {
                    poiGoapActions.Add(INTERACTION_TYPE.INSPECT);
                }
            } else {
                if (poiGoapActions != null) {
                    poiGoapActions.Remove(INTERACTION_TYPE.INSPECT);
                }
            }
        }
    }
    public void AddAdvertisedAction(INTERACTION_TYPE type) {
        if (poiGoapActions == null) {
            poiGoapActions = new List<INTERACTION_TYPE>();
        }
        poiGoapActions.Add(type);
    }
    public void RemoveAdvertisedAction(INTERACTION_TYPE type) {
        poiGoapActions.Add(type);
    }
    public void AddJobTargettingThis(JobQueueItem job) {
        allJobsTargettingThis.Add(job);
    }
    public bool RemoveJobTargettingThis(JobQueueItem job) {
        return allJobsTargettingThis.Remove(job);
    }
    public bool HasJobTargettingThis(JOB_TYPE jobType) {
        for (int i = 0; i < allJobsTargettingThis.Count; i++) {
            JobQueueItem job = allJobsTargettingThis[i];
            if (job.jobType == jobType) {
                return true;
            }
        }
        return false;
    }
    public void AddTargettedByAction(GoapAction action) {
        targettedByAction.Add(action);
    }
    public void RemoveTargettedByAction(GoapAction action) {
        targettedByAction.Remove(action);
    }
    #endregion

    #region Traits
    public bool AddTrait(string traitName, Character characterResponsible = null, System.Action onRemoveAction = null, GoapAction gainedFromDoing = null, bool triggerOnAdd = true) {
        if (AttributeManager.Instance.IsInstancedTrait(traitName)) {
            return AddTrait(AttributeManager.Instance.CreateNewInstancedTraitClass(traitName), characterResponsible, onRemoveAction, gainedFromDoing, triggerOnAdd);
        } else {
            return AddTrait(AttributeManager.Instance.allTraits[traitName], characterResponsible, onRemoveAction, gainedFromDoing, triggerOnAdd);
        }
    }
    public virtual bool AddTrait(Trait trait, Character characterResponsible = null, System.Action onRemoveAction = null, GoapAction gainedFromDoing = null, bool triggerOnAdd = true) {
        if (trait.IsUnique()) {
            Trait oldTrait = GetNormalTrait(trait.name);
            if (oldTrait != null) {
                oldTrait.SetCharacterResponsibleForTrait(characterResponsible);
                oldTrait.AddCharacterResponsibleForTrait(characterResponsible);
                return false;
            }
            //return false;
        }
        _traits.Add(trait);
        trait.SetGainedFromDoing(gainedFromDoing);
        trait.SetOnRemoveAction(onRemoveAction);
        trait.SetCharacterResponsibleForTrait(characterResponsible);
        trait.AddCharacterResponsibleForTrait(characterResponsible);
        //ApplyTraitEffects(trait);
        //ApplyPOITraitInteractions(trait);
        if (trait.daysDuration > 0) {
            GameDate removeDate = GameManager.Instance.Today();
            removeDate.AddTicks(trait.daysDuration);
            string ticket = SchedulingManager.Instance.AddEntry(removeDate, () => RemoveTrait(trait), this);
            trait.SetExpiryTicket(this, ticket);
        }
        if (triggerOnAdd) {
            trait.OnAddTrait(this);
        }
        OnTileObjectGainedTrait(trait);
        return true;
    }
    public virtual bool RemoveTrait(Trait trait, bool triggerOnRemove = true, Character removedBy = null, bool includeAlterEgo = true) {
        if (_traits.Remove(trait)) {
            trait.RemoveExpiryTicket(this);
            if (triggerOnRemove) {
                trait.OnRemoveTrait(this, removedBy);
            }
            return true;
        }
        return false;
    }
    public bool RemoveTrait(string traitName, bool triggerOnRemove = true, Character removedBy = null) {
        Trait trait = GetNormalTrait(traitName);
        if (trait != null) {
            return RemoveTrait(trait, triggerOnRemove, removedBy);
        }
        return false;
    }
    public void RemoveTrait(List<Trait> traits) {
        for (int i = 0; i < traits.Count; i++) {
            RemoveTrait(traits[i]);
        }
    }
    public virtual List<Trait> RemoveAllTraitsByType(TRAIT_TYPE traitType) {
        List<Trait> removedTraits = new List<Trait>();
        for (int i = 0; i < _traits.Count; i++) {
            if (_traits[i].type == traitType) {
                removedTraits.Add(_traits[i]);
                _traits.RemoveAt(i);
                i--;
            }
        }
        return removedTraits;
    }
    public Trait GetNormalTrait(params string[] traitNames) {
        for (int i = 0; i < normalTraits.Count; i++) {
            Trait trait = normalTraits[i];

            for (int j = 0; j < traitNames.Length; j++) {
                if (trait.name == traitNames[j] && !trait.isDisabled) {
                    return trait;
                }
            }
        }
        return null;
    }
    public void RefreshTraitExpiry(Trait trait) {
        if (trait.expiryTickets.ContainsKey(this)) {
            SchedulingManager.Instance.RemoveSpecificEntry(trait.expiryTickets[this]);
        }
        if (trait.daysDuration > 0) {
            GameDate removeDate = GameManager.Instance.Today();
            removeDate.AddTicks(trait.daysDuration);
            string ticket = SchedulingManager.Instance.AddEntry(removeDate, () => RemoveTrait(trait), this);
            trait.SetExpiryTicket(this, ticket);
        }

    }
    private void RemoveAllTraits() {
        List<Trait> allTraits = new List<Trait>(normalTraits);
        for (int i = 0; i < allTraits.Count; i++) {
            RemoveTrait(allTraits[i]);
        }
    }
    #endregion

    #region Collision
    public void InitializeCollisionTrigger() {
        GameObject collisionGO = GameObject.Instantiate(InteriorMapManager.Instance.poiCollisionTriggerPrefab, InteriorMapManager.Instance.transform);
        SetCollisionTrigger(collisionGO.GetComponent<POICollisionTrigger>());
        collisionGO.SetActive(false);
        _collisionTrigger.Initialize(this);
        RectTransform rt = collisionGO.transform as RectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.zero;
    }
    public void PlaceCollisionTriggerAt(LocationGridTile tile) {
        _collisionTrigger.transform.SetParent(tile.parentAreaMap.objectsParent);
        (_collisionTrigger.transform as RectTransform).anchoredPosition = tile.centeredLocalLocation;
        _collisionTrigger.gameObject.SetActive(true);
        _collisionTrigger.SetLocation(tile);
    }
    public virtual void DisableCollisionTrigger() {
        _collisionTrigger.gameObject.SetActive(false);
    }
    public virtual void EnableCollisionTrigger() {
        _collisionTrigger.gameObject.SetActive(true);
    }
    public void SetCollisionTrigger(POICollisionTrigger trigger) {
        _collisionTrigger = trigger;
    }
    public void PlaceGhostCollisionTriggerAt(LocationGridTile tile) {
        GameObject ghostGO = GameObject.Instantiate(InteriorMapManager.Instance.ghostCollisionTriggerPrefab, tile.parentAreaMap.objectsParent);
        RectTransform rt = ghostGO.transform as RectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.zero;
        (ghostGO.transform as RectTransform).anchoredPosition = tile.centeredLocalLocation;
        GhostCollisionTrigger gct = ghostGO.GetComponent<GhostCollisionTrigger>();
        gct.Initialize(this);
        gct.SetLocation(tile);
    }
    #endregion

    #region Awareness
    public void AddAwareCharacter(Character character) {
        if (!awareCharacters.Contains(character)) {
            awareCharacters.Add(character);
        }
    }
    public void RemoveAwareCharacter(Character character) {
        //anytime a character aware of this object is removed
        //check if any ghost colliders need to be destroyed
        //to do that:
        if (awareCharacters.Remove(character)) {
            List<LocationGridTile> knownLocations = new List<LocationGridTile>();
            //loop through all the characters that are currently aware of this object
            for (int i = 0; i < awareCharacters.Count; i++) {
                Character currCharacter = awareCharacters[i];
                //IAwareness awareness = currCharacter.HasAwareness(this);
                if (gridTileLocation != null && !knownLocations.Contains(gridTileLocation)) {
                    //for each character, store in a list all their known locations of this object
                    knownLocations.Add(gridTileLocation);
                }
            }
            //then broadcast that list to all ghost colliders of this object
            Messenger.Broadcast(Signals.CHECK_GHOST_COLLIDER_VALIDITY, this as IPointOfInterest, knownLocations);
        }
        //each ghost collider will then check if it's current location is part of the broadcasted list
        //if not, it is safe to destroy that ghost object
        //else keep it
    }
    #endregion

    #region For Testing
    protected void AddActionToHistory(GoapAction action) {
        string summary = GameManager.Instance.ConvertDayToLogString(action.executionDate) + action.actor.name + " performed " + action.goapName;
        actionHistory.Add(summary);
        if (actionHistory.Count > 50) {
            actionHistory.RemoveAt(0);
        }
    }
    public void LogActionHistory() {
        string summary = this.ToString() + "'s action history:";
        for (int i = 0; i < actionHistory.Count; i++) {
            summary += "\n" + (i + 1).ToString() + " - " + actionHistory[i];
        }
        Debug.Log(summary);
    }
    #endregion

    #region GOAP
    private void ConstructInitialGoapAdvertisements() {
        poiGoapActions.Add(INTERACTION_TYPE.INSPECT);
    }
    /// <summary>
    /// Does this tile object advertise a given action type.
    /// </summary>
    /// <param name="type">The action type that need to be advertised.</param>
    /// <returns>If this tile object advertises the given action.</returns>
    public bool Advertises(INTERACTION_TYPE type) {
        return poiGoapActions.Contains(type);
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
    /// <summary>
    /// Does this tile object advertise a number of types from the given list.
    /// </summary>
    /// <param name="count">The number of valid types to consider.</param>
    /// <param name="types">The list of types to check.</param>
    /// <returns>If this tile object can meet the needed requirements.</returns>
    public bool AdvertisesAny(int count = 1, params INTERACTION_TYPE[] types) {
        int validCount = 0;
        for (int i = 0; i < types.Length; i++) {
            if ((Advertises(types[i]))) {
                validCount++;
                if (validCount >= count) {
                    return true;
                }
            }
        }
        return false;
    }
    public GoapAction Advertise(INTERACTION_TYPE type, Character actor) {
        GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(type, actor, this);
        if (InteractionManager.Instance.CanSatisfyGoapActionRequirements(type, actor, this, null)
            && InteractionManager.Instance.CanSatisfyGoapActionRequirementsOnBuildTree(type, actor, this, null)) {
            return goapAction;
        }
        return null;
    }
    #endregion

    #region Tile Object Slots
    protected virtual void OnPlaceObjectAtTile(LocationGridTile tile) {
        if (hasCreatedSlots) {
            RepositionTileSlots(tile);
        } else {
            CreateTileObjectSlots();
        }
        UpdateOwners();
        Messenger.Broadcast(Signals.TILE_OBJECT_PLACED, this, tile);
    }
    private void CreateTileObjectSlots() {
        UnityEngine.Tilemaps.TileBase usedAsset = tile.parentAreaMap.objectsTilemap.GetTile(tile.localPlace);
        if (tileObjectType != TILE_OBJECT_TYPE.GENERIC && usedAsset != null && InteriorMapManager.Instance.HasSettingForTileObjectAsset(usedAsset)) {
            List<TileObjectSlotSetting> slotSettings = InteriorMapManager.Instance.GetTileObjectSlotSettings(usedAsset);
            slotsParent = GameObject.Instantiate(InteriorMapManager.Instance.tileObjectSlotsParentPrefab, tile.parentAreaMap.objectsTilemap.transform);
            slotsParent.transform.localPosition = tile.centeredLocalLocation;
            slotsParent.transform.localRotation = tile.parentAreaMap.objectsTilemap.GetTransformMatrix(tile.localPlace).rotation;
            slotsParent.name = this.ToString() + " Slots";
            slots = new TileObjectSlotItem[slotSettings.Count];
            for (int i = 0; i < slotSettings.Count; i++) {
                TileObjectSlotSetting currSetting = slotSettings[i];
                GameObject currSlot = GameObject.Instantiate(InteriorMapManager.Instance.tileObjectSlotPrefab, Vector3.zero, Quaternion.identity, slotsParent.transform);
                TileObjectSlotItem currSlotItem = currSlot.GetComponent<TileObjectSlotItem>();
                currSlotItem.ApplySettings(this, currSetting);
                slots[i] = currSlotItem;
            }
        }
        hasCreatedSlots = true;
    }
    private void RepositionTileSlots(LocationGridTile tile) {
        if (slotsParent != null) {
            slotsParent.transform.localPosition = tile.centeredLocalLocation;
        }
    }
    protected void DestroyTileSlots() {
        if (slots == null) {
            return;
        }
        for (int i = 0; i < slots.Length; i++) {
            GameObject.Destroy(slots[i].gameObject);
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
        for (int i = 0; i < slots.Length; i++) {
            TileObjectSlotItem slot = slots[i];
            if (slot.user == character) {
                return slot;
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
    #endregion

    #region Users
    public virtual void AddUser(Character newUser) {
        TileObjectSlotItem availableSlot = GetNearestUnoccupiedSlot(newUser);
        if (availableSlot != null) {
            newUser.SetTileObjectLocation(this);
            availableSlot.Use(newUser);
            if (!HasUnoccupiedSlot()) {
                SetPOIState(POI_STATE.INACTIVE);
            }
        }
    }
    protected virtual void RemoveUser(Character user) {
        TileObjectSlotItem slot = GetSlotUsedBy(user);
        if (slot != null) {
            user.SetTileObjectLocation(null);
            slot.StopUsing();
            SetPOIState(POI_STATE.ACTIVE);
        }
    }
    #endregion

    #region Utilities
    public void DoCleanup() {
        RemoveAllTraits();
    }
    public void UpdateOwners() {
        if (tile.structure is Dwelling) {
            owners.Clear();
            owners.AddRange((tile.structure as Dwelling).residents);
        }
    }
    #endregion

    #region Inspect
    public virtual void OnInspect(Character inspector, out Log log) {
        if (LocalizationManager.Instance.HasLocalizedValue("TileObject", this.GetType().ToString(), "on_inspect")) {
            log = new Log(GameManager.Instance.Today(), "TileObject", this.GetType().ToString(), "on_inspect");
        } else {
            log = null;
        }
        
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