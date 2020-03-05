using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using EZObjectPools;
using System;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class CharacterAvatar : MonoBehaviour {

    private Action onPathFinished;
    private Action onPathStarted;
    private Action onPathCancelled;
    private Action onArriveAction;

    private PathFindingThread _currPathfindingRequest; //the current pathfinding request this avatar is waiting for

	[SerializeField] protected SmoothMovement smoothMovement;
	[SerializeField] protected DIRECTION direction;
    [SerializeField] protected GameObject _avatarHighlight;
    [SerializeField] protected GameObject _avatarVisual;
    [SerializeField] protected SpriteRenderer _avatarSpriteRenderer;
    [SerializeField] protected SpriteRenderer _frameSpriteRenderer;
    [SerializeField] protected SpriteRenderer _centerSpriteRenderer;

    protected Party _party;

    public Region targetLocation { get; protected set; }
    public LocationStructure targetStructure { get; protected set; }
    public LocationGridTile targetTile { get; protected set; }
    public IPointOfInterest targetPOI { get; protected set; }
    public bool placeCharacterAsTileObject { get; private set; }

    [SerializeField] protected List<HexTile> path;

	[SerializeField] private bool _hasArrived = false;
    [SerializeField] private bool _isInitialized = false;
    [SerializeField] private bool _isMovementPaused = false;
    [SerializeField] private bool _isTravelling = false;
    [SerializeField] private bool _isTravellingOutside = false;
    private int _distanceToTarget;
    private bool _isVisualShowing;
    private bool _isTravelCancelled;
    private PATHFINDING_MODE _pathfindingMode;
    private TravelLine _travelLine;
    private Action queuedAction = null;

    public CharacterPortrait characterPortrait { get; private set; }
    #region getters/setters
    public Party party {
        get { return _party; }
    }
    public bool isTravelling {
        get { return _isTravelling; }
    }
    public bool isTravellingOutside {
        get { return _isTravellingOutside; } //if the character is travelling from settlement to settlement, as oppose to only travelling inside settlement map
    }
    public bool isVisualShowing {
        get {
            //if (_isVisualShowing) {
            //    return _isVisualShowing;
            //} else {
            //    //check if this characters current location settlement is being tracked
            //    if (party.specificLocation != null ) { //&& party.specificLocation.isBeingTracked
            //        return true;
            //    }
            //}
            return _isVisualShowing;
        }
    }
    public TravelLine travelLine {
        get { return _travelLine; }
    }
    #endregion

    public virtual void Init(Party party) {
        _party = party;
        //SetPosition(_party.specificLocation.tileLocation.transform.position);
        smoothMovement.avatarGO = gameObject;
        smoothMovement.onMoveFinished += OnMoveFinished;
        _isInitialized = true;
        _hasArrived = true;
        SetVisualState(true);
        // SetSprite(_party.owner.role.roleType);
        SetIsPlaceCharacterAsTileObject(true);

        name = $"{party.owner.name}'s Avatar";
        
        GameObject portraitGO = UIManager.Instance.InstantiateUIObject(CharacterManager.Instance.characterPortraitPrefab.name, transform);
        characterPortrait = portraitGO.GetComponent<CharacterPortrait>();
        characterPortrait.GeneratePortrait(_party.owner);
        portraitGO.SetActive(false);

        CharacterManager.Instance.AddCharacterAvatar(this);
    }

    #region Monobehaviour
    private void OnDestroy() {
        //Messenger.RemoveListener(Signals.INSPECT_ALL, OnInspectAll);
        //if (Messenger.eventTable.ContainsKey(Signals.CHARACTER_TOKEN_ADDED)) {
            //Messenger.RemoveListener<CharacterToken>(Signals.CHARACTER_TOKEN_ADDED, OnCharacterTokenObtained);
        //}
        //Messenger.RemoveListener(Signals.TOGGLE_CHARACTERS_VISIBILITY, OnToggleCharactersVisibility);
        if (_isTravelling) {
            CancelledDeparture();
        }
        CharacterManager.Instance.RemoveCharacterAvatar(this);
    }
    #endregion

    #region Pathfinding
    public void SetTarget(Region target, LocationStructure structure, IPointOfInterest poi, LocationGridTile tile) {
        targetLocation = target;
        targetStructure = structure;
        targetPOI = poi;
        targetTile = tile;
    }
    public void SetOnPathFinished(Action action) {
        onPathFinished = action;
    }
    public void SetOnArriveAction(Action action) {
        onArriveAction = action;
    }
    public void StartPath(PATHFINDING_MODE pathFindingMode, Action actionOnPathFinished = null, Action actionOnPathStart = null) {
        Reset();
        if (targetLocation != null) {
            SetOnPathFinished(actionOnPathFinished);
            StartTravelling();
            actionOnPathStart?.Invoke();
        }
    }
    public void CancelTravel(Action onCancelTravel = null) {
        if (_isTravelling && !_isTravelCancelled) {
            _isTravelCancelled = true;
            onPathCancelled = onCancelTravel;
            Messenger.RemoveListener(Signals.TICK_STARTED, TraverseCurveLine);
            Messenger.AddListener(Signals.TICK_STARTED, ReduceCurveLine);
        }
    }
    private void StartTravelling() {
        SetIsTravellingOutside(true);
        _party.owner.SetPOIState(POI_STATE.INACTIVE);
        if (_party.isCarryingAnyPOI) {
            _party.carriedPOI.SetPOIState(POI_STATE.INACTIVE);
        }
        
        Log arriveLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "left_location");
        arriveLog.AddToFillers(_party.owner, _party.owner.name, LOG_IDENTIFIER.CHARACTER_LIST_1, false);
        if (_party.isCarryingAnyPOI) {
            arriveLog.AddToFillers(_party.carriedPOI, _party.carriedPOI.name, LOG_IDENTIFIER.CHARACTER_LIST_1, false);
        }
        arriveLog.AddToFillers(_party.owner.currentRegion, _party.owner.currentRegion.name, LOG_IDENTIFIER.LANDMARK_1);
        arriveLog.AddLogToInvolvedObjects();
        
        _distanceToTarget = 1;
        Debug.Log($"{_party.owner.name} is travelling from {_party.owner.currentRegion.name} to {targetLocation.name}. Travel time in ticks is: {_distanceToTarget.ToString()}");
        _travelLine = _party.owner.currentRegion.coreTile.CreateTravelLine(targetLocation.coreTile, _distanceToTarget, _party.owner);
        _travelLine.SetActiveMeter(isVisualShowing);
        _party.owner.marker.gameObject.SetActive(false);
        Messenger.AddListener(Signals.TICK_STARTED, TraverseCurveLine);
        Messenger.Broadcast(Signals.PARTY_STARTED_TRAVELLING, party);
    }
    private void TraverseCurveLine() {
        if (_travelLine == null) {
            Messenger.RemoveListener(Signals.TICK_STARTED, TraverseCurveLine);
            return;
        }
        if (_travelLine.isDone) {
            Messenger.RemoveListener(Signals.TICK_STARTED, TraverseCurveLine);
            ArriveAtLocation();
            return;
        }
        _travelLine.AddProgress();
    }
    private void ReduceCurveLine() {
        if (_travelLine.isDone) {
            Messenger.RemoveListener(Signals.TICK_STARTED, ReduceCurveLine);
            CancelTravelDeparture();
            return;
        }
        _travelLine.ReduceProgress();
    }
    private void CancelTravelDeparture() {
        CancelledDeparture();
        onPathCancelled?.Invoke();
    }
    private void CancelledDeparture() {
        if(_travelLine != null) {
            SetIsTravelling(false);
            _isTravelCancelled = false;
            _travelLine.travelLineParent.RemoveChild(_travelLine);
            Destroy(_travelLine.gameObject);
            _travelLine = null;
        }
    }
    private void ArriveAtLocation() {
        SetIsTravelling(false);
        SetIsTravellingOutside(false);
        _travelLine.travelLineParent.RemoveChild(_travelLine);
        Destroy(_travelLine.gameObject);
        _travelLine = null;
        SetHasArrivedState(true);
        
        Region fromRegion = _party.owner.currentRegion; 
        
        fromRegion.RemoveCharacterFromLocation(_party.owner);
        targetLocation.AddCharacterToLocation(_party.owner);

        _party.owner.combatComponent.ClearHostilesInRange();
        _party.owner.combatComponent.ClearAvoidInRange();
        _party.owner.marker.ClearPOIsInVisionRange();

        //character must arrive at the direction that it came from.
        LocationGridTile entrance = (targetLocation.innerMap as RegionInnerTileMap).GetTileToGoToRegion(fromRegion);//targetLocation.innerMap.GetRandomUnoccupiedEdgeTile();
        _party.owner.marker.PlaceMarkerAt(entrance);

        _party.owner.marker.pathfindingAI.SetIsStopMovement(true);
        
        Log arriveLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "arrive_location");
        _party.owner.SetPOIState(POI_STATE.ACTIVE);
        arriveLog.AddToFillers(_party.owner, _party.owner.name, LOG_IDENTIFIER.CHARACTER_LIST_1, false);
        if (_party.isCarryingAnyPOI) {
            arriveLog.AddToFillers(_party.carriedPOI, _party.carriedPOI.name, LOG_IDENTIFIER.CHARACTER_LIST_1, false);
        }
        arriveLog.AddToFillers(targetLocation, targetLocation.name, LOG_IDENTIFIER.LANDMARK_1);
        arriveLog.AddLogToInvolvedObjects();

        if (UtilityScripts.GameUtilities.IsRaceBeast(_party.owner.race) == false || (_party.carriedPOI is Character carriedCharacter 
            && UtilityScripts.GameUtilities.IsRaceBeast(carriedCharacter.race) == false )) {
            PlayerManager.Instance.player.ShowNotificationFrom(_party.owner, arriveLog);    
        }

        Messenger.Broadcast(Signals.PARTY_DONE_TRAVELLING, party);
        if(onArriveAction != null) {
            onArriveAction();
            SetOnArriveAction(null);
        }
        if (targetStructure != null) {
            _party.owner.MoveToAnotherStructure(targetStructure, targetTile, targetPOI, onPathFinished);
        } else {
            if(onPathFinished != null) {
                onPathFinished();
                SetOnPathFinished(null);
            }
        }
    }
    public virtual void ReceivePath(List<HexTile> path, PathFindingThread fromThread) {
        if (!_isInitialized) {
            return;
        }
        if (_currPathfindingRequest == null) {
            return; //this avatar currently has no pathfinding request
        } else {
            if (_currPathfindingRequest != fromThread) {
                return; //the current pathfinding request and the thread that returned the path are not the same
            }
        }
        if (path == null) {
            Debug.LogError(
                $"{_party.name}. There is no path from {_party.owner.currentRegion.name} to {targetLocation.name}", this);
            return;
        }
        if (path.Count > 0) {
            this.path = path;
            _currPathfindingRequest = null;
            SetIsTravelling(true);
            //if(_party.specificLocation.locIdentifier == LOCATION_IDENTIFIER.LANDMARK) {
            //    _party.specificLocation.coreTile.landmarkOnTile.landmarkVisual.OnCharacterExitedLandmark(_party);
            //}
            NewMove();
            if(onPathStarted != null) {
                onPathStarted();
            }
        }
    }
    public virtual void NewMove() {
        if (targetLocation != null && path != null) {
            if (path.Count > 0) {
				//this.MakeCitizenMove(_party.specificLocation.tileLocation, this.path[0]);
    //            if(_party.specificLocation.locIdentifier == LOCATION_IDENTIFIER.LANDMARK) {
    //                RemoveCharactersFromLocation(_party.specificLocation);
    //            }
                //AddCharactersToLocation(this.path[0]);
                //this.path.RemoveAt(0);
            }
        }
    }
    /*
     This is called each time the avatar traverses a node in the
     saved path.
         */
    public virtual void OnMoveFinished() {
		if(path == null){
			Debug.LogError (GameManager.Instance.Today ().ToStringDate());
			Debug.LogError ($"Location: {_party.owner.currentRegion.name}");
		}
        //if (_trackTarget != null) {
        //    if(_trackTarget.currentParty.specificLocation.id != targetLocation.id) {
        //        _party.GoToLocation(_trackTarget.currentParty.specificLocation.coreTile, _pathfindingMode, onPathFinished, _trackTarget, onPathReceived);
        //        return;
        //    }
        //}
        if (path.Count > 0) {
            //if(_party.specificLocation.locIdentifier == LOCATION_IDENTIFIER.HEXTILE) {
            //    RemoveCharactersFromLocation(_party.specificLocation);
            //}
            //AddCharactersToLocation(this.path[0]);
            path.RemoveAt(0);
        }
        HasArrivedAtTargetLocation();
    }
    public virtual void HasArrivedAtTargetLocation() {
		if (_party.owner.currentRegion == targetLocation) {
            if (!_hasArrived) {
                SetIsTravelling(false);
                //_trackTarget = null;
                SetHasArrivedState(true);
                targetLocation.AddCharacterToLocation(_party.owner);
                //Debug.Log(_party.name + " has arrived at " + targetLocation.name + " on " + GameManager.Instance.continuousDays);
                ////Every time the party arrives at home, check if it still not ruined
                //if(_party.mainCharacter.homeLandmark.specificLandmarkType == LANDMARK_TYPE.CAMP) {
                //    //Check if the location the character arrived at is the character's home landmark
                //    if (targetLocation.tileLocation.id == _party.mainCharacter.homeLandmark.tileLocation.id) {
                //        //Check if the current landmark in the location is a camp and it is not yet ruined
                //        if (targetLocation.tileLocation.landmarkOnTile.specificLandmarkType == LANDMARK_TYPE.CAMP) {
                //            Character character = _party.mainCharacter;
                //            if (targetLocation.tileLocation.landmarkOnTile.landmarkObj.currentState.stateName != "Ruined") {
                //                //Make it the character's new home landmark
                //                _party.mainCharacter.homeLandmark.RemoveCharacterHomeOnLandmark(character);
                //                targetLocation.tileLocation.landmarkOnTile.AddCharacterHomeOnLandmark(character);
                //            } else {
                //                //Create new camp
                //                BaseLandmark newCamp = targetLocation.tileLocation.settlementOfTile.CreateCampOnTile(targetLocation.tileLocation);
                //                _party.mainCharacter.homeLandmark.RemoveCharacterHomeOnLandmark(character);
                //                newCamp.AddCharacterHomeOnLandmark(character);
                //            }
                //        }
                //    }
                //}
                onPathFinished?.Invoke();
            }
			if(queuedAction != null){
				queuedAction ();
				queuedAction = null;
				return;
			}
		}else{
			if(queuedAction != null){
				queuedAction ();
				queuedAction = null;
				return;
			}
            if (!_isMovementPaused) {
                NewMove();
            }
		}
    }
    public void SetHasArrivedState(bool state) {
        _hasArrived = state;
    }
    public void SetIsTravelling(bool state) {
        _isTravelling = state;
    }
    public void SetIsTravellingOutside(bool state) {
        _isTravellingOutside = state;
    }
    public void SetIsPlaceCharacterAsTileObject(bool state) {
        placeCharacterAsTileObject = state;
    }
    #endregion

    #region Utilities
    public void SetVisualState(bool state) {
        _isVisualShowing = state;
        if(_travelLine != null) {
            _travelLine.SetActiveMeter(isVisualShowing);
        }
    }
    public void SetHighlightState(bool state) {
        _avatarHighlight.SetActive(state);
    }
    public void SetPosition(Vector3 position) {
        transform.position = position;
    }
    public void SetFrameOrderLayer(int layer) {
        _frameSpriteRenderer.sortingOrder = layer;
    }
    public void SetCenterOrderLayer(int layer) {
        _centerSpriteRenderer.sortingOrder = layer;
    }
    #endregion

    #region overrides
    public void Reset() {
        //base.Reset();
        smoothMovement.Reset();
        SetOnPathFinished(null);
        onPathStarted = null;
        direction = DIRECTION.LEFT;
        //targetLocation = null;
        path = null;
        _isMovementPaused = false;
        _hasArrived = false;
        _isTravelCancelled = false;
        //_trackTarget = null;
        //_isInitialized = false;
        _currPathfindingRequest = null;
        SetHighlightState(false);
    }
    #endregion


}
