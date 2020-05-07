using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using Traits;

public class Party {
    protected int _id;
    protected string _partyName;
    protected int _numOfAttackers;
    protected bool _isDead;
    protected bool _isAttacking;
    protected bool _isDefending;
    protected CharacterAvatar _icon;
    protected Faction _attackedByFaction;
    //protected Combat _currentCombat;
    //protected NPCSettlement _specificLocation;
    protected Character _owner;
    //protected int _maxCharacters;

    public IPointOfInterest carriedPOI { get; protected set; }
    public EmblemBG emblemBG { get; private set; }
    public Sprite emblem { get; private set; }
    public Color partyColor { get; private set; }

    //public List<string> specificLocationHistory { get; private set; } //limited to only 50 items

    #region getters/setters
    public int id {
        get { return _id; }
    }
    public string partyName {
        get { return _partyName; }
    }
    public virtual string name {
        get {
            return _partyName;
        }
    }
    //public float computedPower {
    //    get { return _characters.Sum(x => x.computedPower); }
    //}
    public bool isDead {
        get { return _isDead; }
    }
    //public List<Character> characters {
    //    get { return _characters; }
    //}
    public CharacterAvatar icon {
        get { return _icon; }
    }
    //public Character mainCharacter {
    //    get { return _characters[0]; }
    //}
    //public NPCSettlement specificLocation {
    //    get { return _specificLocation; }
    //}
    public virtual Character owner {
        get { return _owner; }
    }
    public virtual int currentDay {
        get { return 0; }
    }
    public bool isCarryingAnyPOI {
        get { return carriedPOI != null; }
    }
    //public COMBATANT_TYPE combatantType {
    //    get {
    //        if (characters.Count > 1) {
    //            return COMBATANT_TYPE.ARMY; //if the party consists of 2 or more characters, it is considered an army
    //        } else {
    //            return COMBATANT_TYPE.CHARACTER;
    //        }
    //    }
    //}
    //public int maxCharacters {
    //    get { return _maxCharacters; }
    //}
    //public bool isFull {
    //    get { return characters.Count >= maxCharacters; }
    //}
    #endregion

    public Party(Character owner) {
        _owner = owner;
        if (owner != null) {
            _partyName = $"{owner.name}'s Party";
        }
        _id = UtilityScripts.Utilities.SetID(this);
        _isDead = false;
        //_characters = new List<Character>();
        //specificLocationHistory = new List<string>();
        //SetMaxCharacters(4);
        //if (owner.specificLocation != null) {
        //    owner.specificLocation.AddCharacterToLocation(owner);
        //}
    }

    //public void SetMaxCharacters(int max) {
    //    _maxCharacters = max;
    //}

    #region Virtuals
    public virtual void CreateIcon() {
        GameObject characterIconGO = GameObject.Instantiate(CharacterManager.Instance.characterIconPrefab,
        Vector3.zero, Quaternion.identity, CharacterManager.Instance.characterIconsParent);

        _icon = characterIconGO.GetComponent<CharacterAvatar>();
        _icon.Init(this);
    }
    public virtual void ReturnToLife() {
        if (_isDead) {
            _isDead = false;
            CreateIcon();
            //this.specificLocation.AddCharacterToLocation(this);
        }
    }
    public virtual void PartyDeath() {
        if (_isDead) {
            return;
        }
        _isDead = true;
        //For now, when a party dies and there still members besides the owner of this party, kick them out of the party first before applying death
        owner.UncarryPOI();
        //NPCSettlement deathLocation = this.specificLocation;
        //LocationStructure deathStructure = owner.currentStructure;
        //this.specificLocation?.RemoveCharacterFromLocation(this);
        //SetSpecificLocation(deathLocation); //set the specific location of this party, to the location it died at
        //owner.SetCurrentStructureLocation(deathStructure, false);
        RemoveListeners();
        // if (_icon.party.owner.race == RACE.SKELETON) {
        //     GameObject.Destroy(_icon.gameObject);
        //     _icon = null;
        // } else {
            _icon.gameObject.SetActive(false);
        // }        

        //_currentCombat = null;

        //Messenger.Broadcast<Party>(Signals.PARTY_DIED, this);
    }
    public virtual void RemoveListeners() { }
    #endregion

    #region Interface
    //public void SetSpecificLocation(NPCSettlement location) {
    //    if (_specificLocation == location) {
    //        return; //ignore change
    //    }
    //    _specificLocation = location;
    //    if (specificLocationHistory.Count >= 50) {
    //        specificLocationHistory.RemoveAt(0);
    //    }
    //}
    public bool AddPOI(IPointOfInterest poi, bool isOwner = false) {
        if(poi is Character) {
            return AddCharacter(poi as Character, isOwner);
        }else if(poi is TileObject) {
            return AddTileObject(poi as TileObject);
        }
        return false;
    }
    private bool AddTileObject(TileObject tileObject) {
        if (carriedPOI == null) {
            carriedPOI = tileObject;
            // tileObject.SetIsBeingCarriedBy(owner);
            if (tileObject.gridTileLocation != null) {
                tileObject.gridTileLocation.structure.RemovePOIWithoutDestroying(tileObject);
            }
            if (tileObject.mapVisual == null) {
                tileObject.InitializeMapObject(tileObject);
            }
            //tileObject.SetGridTileLocation(owner.gridTileLocation);
            tileObject.visionTrigger.SetCollidersState(false);
            tileObject.mapVisual.transform.SetParent(_owner.marker.visualsParent);
            tileObject.mapVisual.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            tileObject.mapVisual.transform.eulerAngles = Vector3.zero;
            return true;
        }
        return false;
    }
    private bool AddCharacter(Character character, bool isOwner) {
        if (carriedPOI == null) {
            carriedPOI = character;
            character.SetCurrentParty(this);
            character.OnAddedToParty(); //this will remove character from his/her location

            character.SetGridTileLocation(owner.gridTileLocation);
            character.SetCurrentStructureLocation(owner.currentStructure);
            character.marker.transform.SetParent(_owner.marker.visualsParent);
            character.marker.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            character.marker.visualsParent.eulerAngles = Vector3.zero;
            character.marker.transform.eulerAngles = Vector3.zero;
            character.marker.nameLbl.gameObject.SetActive(false);

            Plagued targetPlagued = character.traitContainer.GetNormalTrait<Plagued>("Plagued");
            if (targetPlagued != null) {
                string plaguedSummary = $"{owner.name} carried a plagued character. Rolling for infection.";
                int roll = UnityEngine.Random.Range(0, 100);
                int carryInfectChance = targetPlagued.GetCarryInfectChance();
                plaguedSummary += $"\nRoll is: {roll}, Chance is: {carryInfectChance}";
                if (roll < carryInfectChance) {
                    //carrier will be infected with plague
                    plaguedSummary += $"\nWill infect {owner.name} with plague!";
                    owner.interruptComponent.TriggerInterrupt(INTERRUPT.Plagued, owner);
                    // if (owner.traitContainer.AddTrait(owner, "Plagued", character)) {
                    //     Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "contracted_plague");
                    //     log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    //     log.AddToFillers(character, character.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                    //     log.AddLogToInvolvedObjects();
                    // }
                }
                Debug.Log(GameManager.Instance.TodayLogString() + plaguedSummary);
            }
            Messenger.Broadcast(Signals.CHARACTER_JOINED_PARTY, character, this);
            return true;
        }
        return false;
    }
    public void RemovePOI(IPointOfInterest poi, bool addToLocation = true, LocationGridTile dropLocation = null) {
        if (IsPOICarried(poi)) {
            if (poi is Character) {
                RemoveCharacter(poi as Character, addToLocation, dropLocation);
            } else if (poi is TileObject) {
                RemoveTileObject(poi as TileObject, addToLocation, dropLocation);
            }
        }
    }
    private void RemoveTileObject(TileObject tileObject, bool addToLocation, LocationGridTile dropLocation) {
        carriedPOI = null;
        // tileObject.SetIsBeingCarriedBy(null);
        if (addToLocation) {
            //tileObject.areaMapVisual.collisionTrigger.SetMainColliderState(true);
            if (dropLocation == null) {
                if (_owner.gridTileLocation.isOccupied) {
                    LocationGridTile chosenTile = _owner.gridTileLocation.GetRandomUnoccupiedNeighbor();
                    if (chosenTile != null) {
                        _owner.gridTileLocation.structure.AddPOI(tileObject, chosenTile);
                    } else {
                        Debug.LogWarning(
                            $"{GameManager.Instance.TodayLogString()}{tileObject.name} is being dropped by {_owner.name} but there is no unoccupied neighbor tile including the tile he/she is standing on. Default behavior is to drop character on the tile he/she is standing on regardless if it is unoccupied or not.");
                        _owner.gridTileLocation.structure.AddPOI(tileObject);
                    }
                } else {
                    _owner.gridTileLocation.structure.AddPOI(tileObject, _owner.gridTileLocation);
                }
            } else {
                _owner.gridTileLocation.structure.AddPOI(tileObject, dropLocation);
            }
        } else {
            if (tileObject.gridTileLocation != null) {
                tileObject.gridTileLocation.structure.RemovePOIDestroyVisualOnly(tileObject, owner);
            } else if (tileObject.mapVisual != null) {
                tileObject.DestroyMapVisualGameObject();
            }
        }
        if(tileObject.mapVisual != null) {
            tileObject.mapVisual.transform.eulerAngles = Vector3.zero;
        }
        //character.ownParty.icon.transform.position = this.specificLocation.coreTile.transform.position;
        //Messenger.Broadcast(Signals.CHARACTER_LEFT_PARTY, character, this);
    }
    private void RemoveCharacter(Character character, bool addToLocation, LocationGridTile dropLocation) {
        if(_owner == character) {
            return;
        }
        //LocationGridTile gridTile = _owner.gridTileLocation.GetNearestUnoccupiedTileFromThis();
        //_owner.specificLocation.AddCharacterToLocation(character);
        carriedPOI = null;
        character.OnRemovedFromParty();
        if (dropLocation == null) {
            if (_owner.gridTileLocation.isOccupied) {
                LocationGridTile chosenTile = _owner.gridTileLocation.GetRandomUnoccupiedNeighbor();
                if (chosenTile != null) {
                    character.marker.PlaceMarkerAt(chosenTile, addToLocation);
                } else {
                    Debug.LogWarning(
                        $"{GameManager.Instance.TodayLogString()}{character.name} is being dropped by {_owner.name} but there is no unoccupied neighbor tile including the tile he/she is standing on. Default behavior is to drop character on the tile he/she is standing on regardless if it is unoccupied or not.");
                    character.marker.PlaceMarkerAt(_owner.gridTileLocation, addToLocation);
                }
            } else {
                character.marker.PlaceMarkerAt(_owner.gridTileLocation, addToLocation);
            }
        } else {
            character.marker.PlaceMarkerAt(dropLocation, addToLocation);
        }

        character.marker.transform.eulerAngles = Vector3.zero;
        character.marker.nameLbl.gameObject.SetActive(true);

        character.ownParty.icon.transform.position = owner.currentRegion.coreTile.transform.position;
        Messenger.Broadcast(Signals.CHARACTER_LEFT_PARTY, character, this);
    }
    public bool IsPOICarried(IPointOfInterest poi) {
        return carriedPOI == poi;
    }
    public bool IsPOICarried(string name) {
        return carriedPOI != null && carriedPOI.name == name;
    }
    #endregion

    #region Utilities
    public void SetPartyName(string name) {
        _partyName = name;
    }
    public bool GoToLocation(Region targetLocation, PATHFINDING_MODE pathfindingMode, LocationStructure targetStructure = null,
        Action doneAction = null, Action actionOnStartOfMovement = null, IPointOfInterest targetPOI = null, LocationGridTile targetTile = null) {
        if (_icon.isTravelling && _icon.travelLine != null) {
            return true;
        }
        if (owner.currentRegion.IsSameCoreLocationAs(targetLocation)) {
            //action doer is already at the target location
            doneAction?.Invoke();
            return true;
        } else {
            //_icon.SetActionOnTargetReached(doneAction);
            LocationGridTile exitTile = owner.GetTargetTileToGoToRegion(targetLocation.coreTile.region);
            if (PathfindingManager.Instance.HasPath(owner.gridTileLocation, exitTile)) {
                //check first if character has path toward the exit tile.
                owner.marker.GoTo(exitTile, () => MoveToAnotherLocation(targetLocation.coreTile.region, pathfindingMode, targetStructure, doneAction, actionOnStartOfMovement, targetPOI, targetTile));
                return true;
            } else {
                return false;
            }
        }
    }
    private void MoveToAnotherLocation(Region targetLocation, PATHFINDING_MODE pathfindingMode, LocationStructure targetStructure = null,
        Action doneAction = null, Action actionOnStartOfMovement = null, IPointOfInterest targetPOI = null, LocationGridTile targetTile = null) {
        _icon.SetTarget(targetLocation, targetStructure, targetPOI, targetTile);
        _icon.StartPath(PATHFINDING_MODE.PASSABLE, doneAction, actionOnStartOfMovement);
    }
    #endregion
}
