﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class Token {
    protected bool _isObtainedByPlayer;
    protected TOKEN_TYPE _tokenType;

    #region getters/setters
    public virtual string tokenName {
        get { return string.Empty; }
    }
    public string nameInBold {
        get { return "<b>" + tokenName + "</b>"; }
    }
    public bool isObtainedByPlayer {
        get { return _isObtainedByPlayer; }
    }
    public TOKEN_TYPE tokenType {
        get { return _tokenType; }
    }
    #endregion

    public Token() {
        _isObtainedByPlayer = false;
    }
    public void SetObtainedState(bool state) {
        _isObtainedByPlayer = state;
    }
    /*
     NOTE: Only use this when the player consumes this token.
     If a character consumes this token, use ConsumeToken(Token) in that characters instance.
         */
    public void PlayerConsumeToken() {
        SetObtainedState(false);
        Messenger.Broadcast(Signals.TOKEN_CONSUMED, this);
    }
    #region Virtuals
    public virtual void CreateJointInteractionStates(Interaction interaction, Character user, object target) { }
    public virtual bool CanBeUsedBy(Character character) { return false; }
    #endregion
    //public int id;
    //public string name;
    //public string description;

    //#region getters/setters
    //public string thisName {
    //    get { return name; }
    //}
    //#endregion
    //public void SetData(IntelComponent intelComponent) {
    //    id = intelComponent.id;
    //    name = intelComponent.thisName;
    //    description = intelComponent.description;
    //}

    //public override string ToString() {
    //    return "[" + id.ToString() + "] " + name + " - " + description; 
    //}
}

public class FactionToken : Token{
    public Faction faction;

    #region getters/setters
    public override string tokenName {
        get { return faction.name; }
    }
    #endregion

    public FactionToken(Faction faction) : base() {
        _tokenType = TOKEN_TYPE.FACTION;
        this.faction = faction;
    }

    public override string ToString() {
        return faction.name + "'s Token";
    }
}

public class LocationToken : Token {
    public Area location;

    #region getters/setters
    public override string tokenName {
        get { return location.name; }
    }
    #endregion

    public LocationToken(Area location) : base() {
        _tokenType = TOKEN_TYPE.LOCATION;
        this.location = location;
    }

    public override string ToString() {
        return location.name + " Token";
    }
}

public class CharacterToken : Token {
    public Character character;

    #region getters/setters
    public override string tokenName {
        get { return character.name; }
    }
    #endregion

    public CharacterToken(Character character) : base() {
        _tokenType = TOKEN_TYPE.CHARACTER;
        this.character = character;
    }
    public override string ToString() {
        return character.name + "'s Token";
    }
}

public class SpecialToken : Token, IPointOfInterest {
    public string name { get; private set; }
    public SPECIAL_TOKEN specialTokenType;
    public INTERACTION_TYPE npcAssociatedInteractionType;
    //public int quantity;
    public int weight;
    public Faction owner;
    public LocationStructure structureLocation { get; private set; }
    public InteractionAttributes interactionAttributes { get; protected set; }
    public List<INTERACTION_TYPE> poiGoapActions { get; private set; }
    public int supplyValue { get { return ItemManager.Instance.itemData[specialTokenType].supplyValue; } }
    public int craftCost { get { return ItemManager.Instance.itemData[specialTokenType].craftCost; } }
    public int purchaseCost { get { return ItemManager.Instance.itemData[specialTokenType].purchaseCost; } }
    protected List<Trait> _traits;
    private LocationGridTile tile;
    private POI_STATE _state;
    private POICollisionTrigger _collisionTrigger;

    #region getters/setters
    public override string tokenName {
        get { return name; }
    }
    public virtual string Item_Used {
        get { return "Item Used"; }
    }
    public virtual string Stop_Fail {
        get { return "Stop Fail"; }
    }
    public string ownerName {
        get {
            if (owner == null) {
                return "no one";
            } else {
                return owner.name;
            }
        }
    }
    public POINT_OF_INTEREST_TYPE poiType {
        get { return POINT_OF_INTEREST_TYPE.ITEM; }
    }
    public LocationGridTile gridTileLocation {
        get { return tile; }
    }
    public POI_STATE state {
        get { return _state; }
    }
    public List<Trait> traits {
        get { return _traits; }
    }
    public Faction factionOwner {
        get { return owner; }
    }
    public POICollisionTrigger collisionTrigger {
        get { return _collisionTrigger; }
    }
    #endregion

    public SpecialToken(SPECIAL_TOKEN specialTokenType, int appearanceRate) : base() {
        _tokenType = TOKEN_TYPE.SPECIAL;
        this.specialTokenType = specialTokenType;
        this.name = Utilities.NormalizeStringUpperCaseFirstLetters(this.specialTokenType.ToString());
        weight = appearanceRate;
        npcAssociatedInteractionType = INTERACTION_TYPE.NONE;
        poiGoapActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.PICK_ITEM, INTERACTION_TYPE.STEAL, INTERACTION_TYPE.SCRAP, INTERACTION_TYPE.ITEM_DESTROY, };
        _traits = new List<Trait>();
        InitializeCollisionTrigger();
    }
    //public void AdjustQuantity(int amount) {
    //    quantity += amount;
    //    if (quantity <= 0) {
    //        Messenger.Broadcast(Signals.SPECIAL_TOKEN_RAN_OUT, this);
    //    }
    //}

    #region Virtuals
    public virtual Character GetTargetCharacterFor(Character sourceCharacter) {
        return null;
    }
    public virtual bool CanBeUsedForTarget(Character sourceCharacter, Character targetCharacter) { return false; }
    public virtual void OnObtainToken(Character character) { }
    public virtual void OnUnobtainToken(Character character) { }
    public virtual void OnConsumeToken(Character character) { }
    public virtual void StartTokenInteractionState(Character user, Character target) {
        user.MoveToAnotherStructure(target.currentStructure, target.GetNearestUnoccupiedTileFromThis());
    }
    #endregion

    public void SetOwner(Faction owner) {
        this.owner = owner;
    }
    public void SetStructureLocation(LocationStructure structureLocation) {
        this.structureLocation = structureLocation;
    }
    public override string ToString() {
        return name;
    }

    #region Area Map
    public void SetGridTileLocation(LocationGridTile tile) {
        this.tile = tile;
        if (tile == null) {
            DisableCollisionTrigger();
        } else {
            PlaceCollisionTriggerAt(tile);
        }
    }
    public LocationGridTile GetNearestUnoccupiedTileFromThis() {
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
    #endregion

    #region Point Of Interest
    public List<GoapAction> AdvertiseActionsToActor(Character actor, List<INTERACTION_TYPE> actorAllowedInteractions) {
        if (poiGoapActions != null && poiGoapActions.Count > 0) {
            List<GoapAction> usableActions = new List<GoapAction>();
            for (int i = 0; i < poiGoapActions.Count; i++) {
                if (actorAllowedInteractions.Contains(poiGoapActions[i])) {
                    GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(poiGoapActions[i], actor, this);
                    if (goapAction.CanSatisfyRequirements()) {
                        usableActions.Add(goapAction);
                    }
                }
            }
            return usableActions;
        }
        return null;
    }
    public void SetPOIState(POI_STATE state) {
        _state = state;
    }
    #endregion

    #region Traits
    public bool AddTrait(string traitName) {
        return AddTrait(AttributeManager.Instance.allTraits[traitName]);
    }
    public bool AddTrait(Trait trait, Character characterResponsible = null, System.Action onRemoveAction = null) {
        if (trait.IsUnique() && GetTrait(trait.name) != null) {
            trait.SetCharacterResponsibleForTrait(characterResponsible);
            return false;
        }
        _traits.Add(trait);
        trait.SetOnRemoveAction(onRemoveAction);
        trait.SetCharacterResponsibleForTrait(characterResponsible);
        //ApplyTraitEffects(trait);
        //ApplyPOITraitInteractions(trait);
        if (trait.daysDuration > 0) {
            GameDate removeDate = GameManager.Instance.Today();
            removeDate.AddTicks(trait.daysDuration);
            SchedulingManager.Instance.AddEntry(removeDate, () => RemoveTrait(trait));
        }
        trait.OnAddTrait(this);
        return true;
    }
    public bool RemoveTrait(Trait trait, bool triggerOnRemove = true) {
        if (_traits.Remove(trait)) {
            //UnapplyTraitEffects(trait);
            //UnapplyPOITraitInteractions(trait);
            if (triggerOnRemove) {
                trait.OnRemoveTrait(this);
            }
            return true;
        }
        return false;
    }
    public bool RemoveTrait(string traitName, bool triggerOnRemove = true) {
        Trait trait = GetTrait(traitName);
        if (trait != null) {
            return RemoveTrait(trait, triggerOnRemove);
        }
        return false;
    }
    public void RemoveTrait(List<Trait> traits) {
        for (int i = 0; i < traits.Count; i++) {
            RemoveTrait(traits[i]);
        }
    }
    public Trait GetTrait(string traitName) {
        for (int i = 0; i < _traits.Count; i++) {
            if (_traits[i].name == traitName) {
                return _traits[i];
            }
        }
        return null;
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
    public void DisableCollisionTrigger() {
        _collisionTrigger.gameObject.SetActive(false);
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
}

public class DefenderToken : Token {
    public Area owner;

    public DefenderToken(Area owner) : base() {
        this.owner = owner;
    }
    public override string ToString() {
        return owner.name + "'s Defenders";
    }
}