using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Pathfinding;
using System.Linq;
using Inner_Maps;
using Traits;
using UnityEngine.Profiling;
using UnityEngine.Serialization;
using UtilityScripts;
using Locations.Settlements;
using Necromancy.UI;
using UnityEngine.Assertions;

public class CharacterMarker : MapObjectVisual<Character> {
    public Character character { get; private set; }

    [Header("Character Marker Assets")]
    public Transform visualsParent;
    [SerializeField] private SpriteRenderer mainImg;
    [SerializeField] private SpriteRenderer hairImg;
    [SerializeField] private SpriteRenderer knockedOutHairImg;
    [SerializeField] private SpriteRenderer hoveredImg;
    [SerializeField] private SpriteRenderer clickedImg;
    [SerializeField] private BoxCollider2D buttonCollider;
    [SerializeField] private ParticleSystem bloodSplatterEffect;
    [SerializeField] private ParticleSystemRenderer bloodSplatterEffectRenderer;
    [SerializeField] private SpriteRenderer additionalEffectsImg;
    [SerializeField] private TextRendererParticleSystem textRendererParticleSystem;
    public Transform particleEffectParentAllowRotation;

    [Header("Animation")]
    public Animator animator;
    [FormerlySerializedAs("animationListener")] [SerializeField] private CharacterMarkerAnimationListener _animationListener;
    [SerializeField] private string currentAnimation;
    [SerializeField] private RuntimeAnimatorController defaultController;
    [SerializeField] private RuntimeAnimatorController monsterController;
    [SerializeField] private int _pauseAnimationCounter;
    
    [Header("Pathfinding")]
    public CharacterAIPath pathfindingAI;    
    public AIDestinationSetter destinationSetter;
    public Seeker seeker;
    public BoxCollider2D collider;
    [FormerlySerializedAs("visionCollision")] public CharacterMarkerVisionCollider visionCollider;

    [Header("Combat")]
    public Transform projectileParent;

    [Header("For Testing")]
    [SerializeField] private SpriteRenderer colorHighlight;

    
    //vision colliders
    public List<IPointOfInterest> inVisionPOIs { get; private set; } //POI's in this characters vision collider
    public List<IPointOfInterest> inVisionPOIsButDiffStructure { get; private set; } //POI's in this characters vision collider
    public List<IPointOfInterest> unprocessedVisionPOIs { get; private set; } //POI's in this characters vision collider
    private List<IPointOfInterest> unprocessedVisionPOIInterruptsOnly { get; set; } //POI's in this characters vision collider
    private List<ActualGoapNode> unprocessedActionsOnly { get; set; } //POI's in this characters vision collider
    public List<Character> inVisionCharacters { get; private set; } //POI's in this characters vision collider
    public List<TileObject> inVisionTileObjects { get; private set; } //POI's in this characters vision collider
    public Action arrivalAction { get; private set; }
    public Action arrivalActionBeforeDigging { get; private set; }
    //private Action failedToComputePathAction { get; set; }

    //movement
    public IPointOfInterest targetPOI { get; private set; }
    public LocationGridTile destinationTile { get; private set; }
    public float progressionSpeedMultiplier { get; private set; }
    public bool isMoving { get; private set; }
    public bool hasFleePath { get; private set; }
    private float attackSpeedMeter { get; set; }
    public LocationGridTile previousGridTile {
        get => _previousGridTile;
        set {
            _previousGridTile = value;
#if DEBUG_LOG
            if (_previousGridTile == null) {
                Debug.Log($"Previous grid tile was set to null");
            }
#endif
        } 
    }

    private LocationGridTile _previousGridTile;
    private Area _previousAreaLocation;
    private CharacterMarkerNameplate _nameplate;
    private LocationGridTile _destinationTile;
    private string _destroySchedule;
    private GameDate _destroyDate;
    private List<Area> areasInWildernessForFlee;
    private List<Vector3> avoidThisPositions;
    private int _currentColliderSize;

    public bool useCanTraverse;
    public float endReachedDistance;

#region Getters
    public GameDate destroyDate => _destroyDate;
    public bool hasExpiry => !string.IsNullOrEmpty(_destroySchedule);
    public bool isMainVisualActive => mainImg.gameObject.activeSelf;
    public CharacterMarkerAnimationListener animationListener => _animationListener;
    public int sortingOrder => mainImg.sortingOrder;
    public CharacterMarkerNameplate nameplate => _nameplate;
#endregion
    
    public void SetCharacter(Character character) {
        base.Initialize(character);
        name = $"{character.name}'s Marker";
        this.character = character;
        CreateNameplate();
        UpdateName();
        UpdateSortingOrder();
        UpdateMarkerVisuals();
        UpdateAnimatorController();
        UpdateActionIcon();
        ForceUpdateMarkerVisualsBasedOnAnimation();
        CreateCollisionTrigger();
        SetVisualState(true);

        unprocessedVisionPOIs = new List<IPointOfInterest>();
        unprocessedVisionPOIInterruptsOnly = new List<IPointOfInterest>();
        unprocessedActionsOnly = new List<ActualGoapNode>();
        inVisionPOIs = new List<IPointOfInterest>();
        inVisionPOIsButDiffStructure = new List<IPointOfInterest>();
        inVisionCharacters = new List<Character>();
        inVisionTileObjects = new List<TileObject>();
        attackSpeedMeter = 0f;
        OnProgressionSpeedChanged(GameManager.Instance.currProgressionSpeed);
        UpdateHairState();

        if(areasInWildernessForFlee == null) {
            areasInWildernessForFlee = new List<Area>();
        }
        if (textRendererParticleSystem != null) {
            textRendererParticleSystem.Stop();    
        }

        AddListeners();
        PathfindingManager.Instance.AddAgent(pathfindingAI);

        if (GameManager.Instance.gameHasStarted) {
            if (GameManager.Instance.isPaused) {
                PauseAnimation();
            }
        }

        List<Trait> traitOverrideFunctions = character.traitContainer.GetTraitOverrideFunctions(TraitManager.Initiate_Map_Visual_Trait);
        if (traitOverrideFunctions != null) {
            for (int i = 0; i < traitOverrideFunctions.Count; i++) {
                Trait trait = traitOverrideFunctions[i];
                trait.OnInitiateMapObjectVisual(character);
            }
        }
        UpdateTraversableTags();
        UpdateTagPenalties();
        seeker.graphMask = InnerMapManager.Instance.mainGraphMask;
        // UpdateNameplatePosition();
    }

#region Monobehavior
    private void OnDisable() {
        if (LevelLoaderManager.Instance.isLoadingNewScene) { return; }
        if (UIManager.Instance == null) { return; }
        if(UIManager.Instance.characterInfoUI.isShowing && UIManager.Instance.characterInfoUI.activeCharacter == character) {
            UIManager.Instance.characterInfoUI.CloseMenu();
        }
        if (UIManager.Instance.monsterInfoUI.isShowing && UIManager.Instance.monsterInfoUI.activeMonster == character) {
            UIManager.Instance.monsterInfoUI.CloseMenu();
        }
        if (character != null) {
            if (character != null && InnerMapCameraMove.Instance != null && InnerMapCameraMove.Instance.target == this.transform) {
                InnerMapCameraMove.Instance.CenterCameraOn(null);
            }
        }
    }
    private void OnEnable() {
        if (character != null) {
            //if (character.isBeingSeized) { return; }
            UpdateAnimation();
        }
    }
    public void ManualUpdate() {
        if (GameManager.Instance.gameHasStarted && !GameManager.Instance.isPaused) {
            if (character.isBeingSeized) { return; }
#if DEBUG_PROFILER
            Profiler.BeginSample($"{character.name} - Attack Speed Meter");
#endif
            if (attackSpeedMeter < character.combatComponent.attackSpeed) {
                attackSpeedMeter += ((Time.deltaTime * 1000f) * progressionSpeedMultiplier);
                UpdateAttackSpeedMeter();
            }
#if DEBUG_PROFILER
            Profiler.EndSample();

            Profiler.BeginSample($"{character.name} - Pathfinding Update Me Call");
#endif
            pathfindingAI.UpdateMe();
#if DEBUG_PROFILER
            Profiler.EndSample();
#endif
        }
    }
    private void LateUpdate() {
        UpdateVisualBasedOnCurrentAnimationFrame();
        if (character.stateComponent.currentState is CombatState combatState) {
            combatState.LateUpdate();
        }
    }
    private void ForceUpdateMarkerVisualsBasedOnAnimation() {
        string currSpriteName = mainImg.sprite.name;
        if (character.visuals.markerAnimations.ContainsKey(currSpriteName)) {
            Sprite newSprite = character.visuals.markerAnimations[currSpriteName];
            mainImg.sprite = newSprite;
        }
    }
#endregion

#region Placement
    /// <summary>
    /// Used for placing a character for the first time.
    /// </summary>
    /// <param name="tile">The tile the character should be placed at.</param>
    public void InitialPlaceMarkerAt(LocationGridTile tile) {
        visionCollider.Initialize();
        PlaceMarkerAt(tile);
        pathfindingAI.UpdateMe();
        character.movementComponent.UpdateSpeed();
    }
    /// <summary>
    /// Used for placing the character for the first time after being loaded from
    /// saved game.
    /// </summary>
    /// <param name="data">The saved data of this character</param>
    /// <param name="region">The region that this character should be at.
    /// Cannot access <see cref="Character.currentRegion"/> here because that takes into account if the character is carried or not,
    /// which can be a problem if the data of this characters carrier hasn't been loaded yet.</param>
    public void LoadMarkerPlacement(SaveDataCharacter data, Region region) {
        visionCollider.Initialize();
        SetCollidersState(true);
        Transform thisTransform = transform;
        thisTransform.SetParent(region.innerMap.objectsParent);
        thisTransform.position = data.worldPos;
        visualsParent.transform.localRotation = data.rotation;
        UpdateActionIcon();
        UpdateAnimation();
        character.SetGridTilePosition(transform.localPosition);
        if (character.gridTileLocation != null) {
            LocationAwarenessUtility.AddToAwarenessList(character, character.gridTileLocation);    
        }
        //removed by aaron for awareness update region.AddPendingAwareness(character);
        if (data.hasExpiry) {
            ScheduleExpiry(data.markerExpiryDate);
        }
    }
    /// <summary>
    /// Place this marker at a given tile location. 
    /// </summary>
    public void PlaceMarkerAt(LocationGridTile tile) {
        gameObject.transform.SetParent(tile.parentMap.objectsParent);
        //Always add to region characters now because in UpdatePosition, the character will be added to the structure characters at location list
        //It will be inconsistent if the character is added to that list but will not be added to the region's characters at location list
        tile.structure.region.AddCharacterToLocation(character);

        //if (addToLocation) {
        //    tile.structure.location.AddCharacterToLocation(character);
        //}
        SetActiveState(true);
        UpdateAnimation();
        pathfindingAI.Teleport(tile.centeredWorldLocation);
        UpdatePosition();
        if (!(tile.tileObjectComponent.objHere is WurmHole)) {
            //NOTE: SPECIAL CASE only set structure if character the target tile does not have a wurm hole. Because this will cause conflicts in structure location when the character is teleported
            //but this will set the structure location of the character to th target tile instead
            //added checker to prevent duplicate calls to structure add and remove, since we expect that normally UpdatePosition will handle any changes in structure location
            if (character.currentStructure != tile.structure) {
                character.currentStructure?.RemoveCharacterAtLocation(character);
                tile.structure.AddCharacterAtLocation(character);    
            }
        }
//#if UNITY_EDITOR || DEVELOPMENT_BUILD
//        Assert.IsTrue(character.currentStructure == tile.structure,
//            $"{character.name} updated its position but the structure is not the same as the tile's structure. Current structure: { character.currentStructure?.name }, Tile structure: { tile.structure.name }");
//#else
//        character.currentStructure?.RemoveCharacterAtLocation(character);
//        tile.structure.AddCharacterAtLocation(character);
//#endif
        //Removed this because character will be added already in the structure characters at location list in UpdatePosition
        //if (addToLocation) {
        //    tile.structure.AddCharacterAtLocation(character, tile);
        //}
        UpdateActionIcon();
        SetCollidersState(true);
        LocationAwarenessUtility.AddToAwarenessList(character, character.gridTileLocation);
        //removed by aaron for awareness update tile.structure.region.AddPendingAwareness(character);
        character.reactionComponent.UpdateHiddenState();
        if (_nameplate) {
            _nameplate.UpdateNameActiveState();
        }
    }
#endregion
    
#region Pointer Functions
    protected override void OnPointerLeftClick(Character poi) {
        base.OnPointerLeftClick(poi);
        UIManager.Instance.ShowCharacterInfo(character, true);
    }
    protected override void OnPointerRightClick(Character poi) {
        base.OnPointerRightClick(poi);
        UIManager.Instance.ShowPlayerActionContextMenu(poi, poi.worldPosition, false);
    }
    protected override void OnPointerMiddleClick(Character poi) {
        base.OnPointerMiddleClick(poi);
        Character activeCharacter = UIManager.Instance.characterInfoUI.activeCharacter ?? UIManager.Instance.monsterInfoUI.activeMonster;
        if (activeCharacter != null) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UIManager.Instance.poiTestingUI.ShowUI(character, activeCharacter);
#endif
//            if (activeCharacter.minion == null) {
//#if UNITY_EDITOR || DEVELOPMENT_BUILD
//                UIManager.Instance.poiTestingUI.ShowUI(character, activeCharacter);
//#endif
//            } else {
//                UIManager.Instance.minionCommandsUI.ShowUI(character);
//            }
        }
    }
    protected override void OnPointerEnter(Character character) {
        base.OnPointerEnter(character);
        InnerMapManager.Instance.SetCurrentlyHoveredPOI(character);
        InnerMapManager.Instance.ShowTileData(this.character.gridTileLocation, this.character);
        if (UIManager.Instance.GetCurrentlySelectedCharacter() != character) {
            //only process hover tooltips if character is not the currently selected character
            ShowThoughtsAndNameplate();    
        }
        if (PlayerManager.Instance.player != null && PlayerManager.Instance.player.currentActiveIntel != null) {
            string message = string.Empty;
            if (PlayerManager.Instance.player.currentActiveIntel.actor != character) {
                if (character.relationshipContainer.HasRelationshipWith(PlayerManager.Instance.player.currentActiveIntel.actor)) {
                    string relationshipName = character.relationshipContainer.GetRelationshipNameWith(PlayerManager.Instance.player.currentActiveIntel.actor);
                    message = $"{message}{PlayerManager.Instance.player.currentActiveIntel.actor.visuals.GetCharacterNameWithIconAndColor()} - {relationshipName} of {character.visuals.GetCharacterNameWithIconAndColor()}\n";
                }
            } else {
                message = $"{message}{character.visuals.GetCharacterNameWithIconAndColor()} is the actor of this intel.\n";
            }
            //NOTE: Added checking for target character and actor to prevent duplicates.
            if (PlayerManager.Instance.player.currentActiveIntel.target is Character targetCharacter && targetCharacter != PlayerManager.Instance.player.currentActiveIntel.actor) {
                if (targetCharacter != character) {
                    if (character.relationshipContainer.HasRelationshipWith(targetCharacter)) {
                        string relationshipName = character.relationshipContainer.GetRelationshipNameWith(targetCharacter);
                        message = $"{message}{targetCharacter.visuals.GetCharacterNameWithIconAndColor()} - {relationshipName} of {character.visuals.GetCharacterNameWithIconAndColor()}\n";
                    }
                } else {
                    message = $"{message}{character.visuals.GetCharacterNameWithIconAndColor()} is the target of this intel.\n";
                }  
            }
            if (!string.IsNullOrEmpty(message) && _nameplate != null) {
                _nameplate.ShowIntelHelper(message);
            }
        }
        if (_nameplate) {
            _nameplate.UpdateNameActiveState();
        }
    }
    private bool HasRelationshipWithIntel(IIntel intel) {
        if (intel.actor != character) {
            if (character.relationshipContainer.HasRelationshipWith(intel.actor)) {
                return true;
            }
            if (intel.target is Character targetCharacter) {
                if (targetCharacter != character) {
                    if (character.relationshipContainer.HasRelationshipWith(targetCharacter)) {
                        return true;
                    }
                } else {
                    return true;
                }
            }
        }
        return false;
    }
    protected override void OnPointerExit(Character poi) {
        base.OnPointerExit(poi);
        if (InnerMapManager.Instance.currentlyHoveredPoi == poi) {
            InnerMapManager.Instance.SetCurrentlyHoveredPOI(null);
        }
        UIManager.Instance.HideSmallInfo();
        if (UIManager.Instance.GetCurrentlySelectedCharacter() != character) {
            //only process hover tooltips if character is not the currently selected character
            HideThoughtsAndNameplate();
        }
        if (_nameplate != null) {
            _nameplate.HideIntelHelper();
            _nameplate.UpdateNameActiveState();
        }
    }
#endregion

#region Listeners
    private void AddListeners() {
        Messenger.AddListener<PROGRESSION_SPEED>(UISignals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);
        Messenger.AddListener<Character, Trait>(CharacterSignals.CHARACTER_TRAIT_ADDED, OnCharacterGainedTrait);
        Messenger.AddListener<Character, Trait>(CharacterSignals.CHARACTER_TRAIT_REMOVED, OnCharacterLostTrait);
        Messenger.AddListener<Character>(CharacterSignals.STARTED_TRAVELLING_IN_WORLD, OnCharacterAreaTravelling);
        Messenger.AddListener(CharacterSignals.PROCESS_ALL_UNPOROCESSED_POIS, ProcessAllUnprocessedVisionPOIs);
        Messenger.AddListener<TileObject, Character, LocationGridTile>(GridTileSignals.TILE_OBJECT_REMOVED, OnTileObjectRemovedFromTile);
        Messenger.AddListener<IPointOfInterest>(CharacterSignals.REPROCESS_POI, ReprocessPOI);
        Messenger.AddListener(CharacterSignals.CHARACTER_TICK_ENDED_MOVEMENT, PerTickMovement);
        Messenger.AddListener<IIntel>(PlayerSignals.ACTIVE_INTEL_SET, OnActiveIntelSet);
        Messenger.AddListener(PlayerSignals.ACTIVE_INTEL_REMOVED, OnActiveIntelRemoved);
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CHANGED_NAME, OnCharacterChangedName);
        Messenger.AddListener<bool>(CharacterSignals.TOGGLE_CHARACTER_MARKER_NAMEPLATE, OnToggleCharacterMarkerNameplate);
        Messenger.AddListener<Character>(PlayerSignals.PLAYER_STORED_CHARACTER, OnPlayerStoredCharacterAsTarget);
        Messenger.AddListener<Character>(PlayerSignals.PLAYER_REMOVED_STORED_CHARACTER, OnPlayerRemoveStoredCharacterAsTarget);
        Messenger.AddListener<MovingTileObject>(TileObjectSignals.MOVING_TILE_OBJECT_EXPIRED, OnMovingTileObjectExpired);
    }
    private void RemoveListeners() {
        Messenger.RemoveListener<PROGRESSION_SPEED>(UISignals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);
        Messenger.RemoveListener<Character, Trait>(CharacterSignals.CHARACTER_TRAIT_ADDED, OnCharacterGainedTrait);
        Messenger.RemoveListener<Character, Trait>(CharacterSignals.CHARACTER_TRAIT_REMOVED, OnCharacterLostTrait);
        Messenger.RemoveListener<Character>(CharacterSignals.STARTED_TRAVELLING_IN_WORLD, OnCharacterAreaTravelling);
        Messenger.RemoveListener(CharacterSignals.PROCESS_ALL_UNPOROCESSED_POIS, ProcessAllUnprocessedVisionPOIs);
        Messenger.RemoveListener<TileObject, Character, LocationGridTile>(GridTileSignals.TILE_OBJECT_REMOVED, OnTileObjectRemovedFromTile);
        Messenger.RemoveListener<IPointOfInterest>(CharacterSignals.REPROCESS_POI, ReprocessPOI);
        Messenger.RemoveListener(CharacterSignals.CHARACTER_TICK_ENDED_MOVEMENT, PerTickMovement);
        Messenger.RemoveListener<IIntel>(PlayerSignals.ACTIVE_INTEL_SET, OnActiveIntelSet);
        Messenger.RemoveListener(PlayerSignals.ACTIVE_INTEL_REMOVED, OnActiveIntelRemoved);
        Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_CHANGED_NAME, OnCharacterChangedName);
        Messenger.RemoveListener<bool>(CharacterSignals.TOGGLE_CHARACTER_MARKER_NAMEPLATE, OnToggleCharacterMarkerNameplate);
        Messenger.RemoveListener<Character>(PlayerSignals.PLAYER_STORED_CHARACTER, OnPlayerStoredCharacterAsTarget);
        Messenger.RemoveListener<Character>(PlayerSignals.PLAYER_REMOVED_STORED_CHARACTER, OnPlayerRemoveStoredCharacterAsTarget);
        Messenger.RemoveListener<MovingTileObject>(TileObjectSignals.MOVING_TILE_OBJECT_EXPIRED, OnMovingTileObjectExpired);
    }

    private void OnCharacterChangedName(Character p_character) {
        if (p_character == character) {
            UpdateName();
        }
    }
    private void OnCharacterGainedTrait(Character characterThatGainedTrait, Trait trait) {
        if (characterThatGainedTrait == this.character) {
            SelfGainedTrait(characterThatGainedTrait, trait);
        } else {
            OtherCharacterGainedTrait(characterThatGainedTrait, trait);
        }
    }
    private void OnCharacterLostTrait(Character character, Trait trait) {
        if (character == this.character) {
#if DEBUG_LOG
            string lostTraitSummary =
                $"{character.name} has <color=red>lost</color> trait <b>{trait.name}</b>";
            character.logComponent.PrintLogIfActive(lostTraitSummary);
#endif
            UpdateAnimation();
            UpdateActionIcon();
        }
    }
    /// <summary>
    /// Listener for when a party starts travelling towards another npcSettlement.
    /// </summary>
    /// <param name="travellingCharacter">The travelling party.</param>
    private void OnCharacterAreaTravelling(Character travellingCharacter) {
        if (travellingCharacter.carryComponent.isCarryingAnyPOI) {
            //if (travellingCharacter.carryComponent.IsPOICarried(targetPOI)) {
            //    //If the travelling party is travelling outside and is carrying a poi that is being targetted by this marker, this marker should fail to compute path
            //    Action action = failedToComputePathAction;
            //    if (action != null) {
            //        if (character.carryComponent.masterCharacter.avatar.isTravellingOutside) {
            //            character.carryComponent.masterCharacter.avatar.SetOnArriveAction(() => character.OnArriveAtAreaStopMovement());
            //        } else {
            //            StopMovement();
            //        }
            //    }
            //    //set arrival action to null, because some arrival actions set it when executed
            //    failedToComputePathAction = null;
            //    action?.Invoke();
            //}
            if(travellingCharacter.carryComponent.carriedPOI is Character carriedCharacter && character != carriedCharacter) {
                character.combatComponent.RemoveHostileInRange(carriedCharacter); //removed hostile because he/she left the npcSettlement.
                character.combatComponent.RemoveAvoidInRange(carriedCharacter);
                RemovePOIFromInVisionRange(carriedCharacter);
                RemovePOIAsInRangeButDifferentStructure(carriedCharacter);
            }
        }
    }
    private void OnToggleCharacterMarkerNameplate(bool state) {
        if (_nameplate) {
            _nameplate.UpdateNameActiveState();
        }
    }
    private void SelfGainedTrait(Character characterThatGainedTrait, Trait trait) {
#if DEBUG_LOG
        string gainTraitSummary = $"{GameManager.Instance.TodayLogString()}{characterThatGainedTrait.name} has <color=green>gained</color> trait <b>{trait.name}</b>";
#endif
        if (!characterThatGainedTrait.limiterComponent.canPerform) {
            if (character.combatComponent.isInCombat) {
                characterThatGainedTrait.stateComponent.ExitCurrentState();
#if DEBUG_LOG
                gainTraitSummary += "\nGained trait hinders performance, and characters current state is combat, exiting combat state.";
#endif
            }

            //Once a character has a negative disabler trait, clear hostile and avoid list
            character.combatComponent.ClearHostilesInRange(false);
            character.combatComponent.ClearAvoidInRange(false);
        }
        UpdateAnimation();
        UpdateActionIcon();

#if DEBUG_LOG
        character.logComponent.PrintLogIfActive(gainTraitSummary);
#endif
    }
    private void OtherCharacterGainedTrait(Character otherCharacter, Trait trait) {
        if (trait.name == "Invisible") {
            character.combatComponent.RemoveHostileInRange(otherCharacter);
            character.combatComponent.RemoveAvoidInRange(otherCharacter);
            RemovePOIFromInVisionRange(otherCharacter);
        } else {
            if (IsPOIInVision(otherCharacter)) {
                character.CreateJobsOnTargetGainTrait(otherCharacter, trait);
            }

            //Only remove hostile in range from non lethal combat if target specifically becomes: Unconscious, Zapped or Restrained.
            //if (!otherCharacter.limiterComponent.canPerform) {
            if (character.combatComponent.IsLethalCombatForTarget(otherCharacter) == false) {
                if (otherCharacter.traitContainer.HasTrait("Unconscious", "Paralyzed", "Restrained")) {
                    character.combatComponent.RemoveHostileInRange(otherCharacter);
                    character.combatComponent.RemoveAvoidInRange(otherCharacter);
                }
            }
            
        }
    }
    private void OnTileObjectRemovedFromTile(TileObject obj, Character removedBy, LocationGridTile removedFrom) {
        character.combatComponent.RemoveHostileInRange(obj);
        character.combatComponent.RemoveAvoidInRange(obj);
        RemovePOIFromInVisionRange(obj);
        RemovePOIAsInRangeButDifferentStructure(obj);
    }
    private void OnActiveIntelSet(IIntel intel) {
        if (PlayerManager.Instance.player.CanShareIntelTo(character, intel) && HasRelationshipWithIntel(intel)) {
            _nameplate.SetHighlighterState(true);    
        }
    }
    private void OnActiveIntelRemoved() {
        _nameplate.HideIntelHelper();
        _nameplate.SetHighlighterState(false);
    }
    private void OnPlayerStoredCharacterAsTarget(Character p_character) {
        if (character != null && _nameplate) {
            _nameplate.UpdateNameActiveState();
        }
    }
    private void OnPlayerRemoveStoredCharacterAsTarget(Character p_character) {
        if (character != null && _nameplate) {
            _nameplate.UpdateNameActiveState();
        }
    }
    private void OnMovingTileObjectExpired(MovingTileObject p_tileObject) {
        if (inVisionPOIs.Contains(p_tileObject)) {
            RemovePOIFromInVisionRange(p_tileObject);
        }
    }
#endregion

#region UI
    private void OnProgressionSpeedChanged(PROGRESSION_SPEED progSpeed) {
        if (progSpeed == PROGRESSION_SPEED.X1) {
            progressionSpeedMultiplier = 1f;
        } else if (progSpeed == PROGRESSION_SPEED.X2) {
            progressionSpeedMultiplier = 1.5f;
        } else if (progSpeed == PROGRESSION_SPEED.X4) {
            progressionSpeedMultiplier = 2f;
        }
        character.movementComponent.UpdateSpeed();
        UpdateAnimationSpeed();
    }
    public void UpdateName() {
        _nameplate.UpdateName();
    }
    //public void SetNameState(bool state) {
    //    _nameplate.SetNameState(state);
    //}
    private void CreateNameplate() {
        GameObject nameplateGO = ObjectPoolManager.Instance.InstantiateObjectFromPool("CharacterMarkerNameplate", transform.position,
            Quaternion.identity, UIManager.Instance.characterMarkerNameplateParent);
        _nameplate = nameplateGO.GetComponent<CharacterMarkerNameplate>();
        _nameplate.Initialize(this);
    }
#endregion

#region Action Icon
    public void UpdateActionIcon() {
        //Do not show action icon if character is invisible
        if (_nameplate && isMainVisualActive) {
            _nameplate.UpdateActionIcon();    
        }
    }
#endregion

#region Object Pool
    public override void Reset() {
        TryCancelExpiry();
        destinationTile = null;
        SetVisionColliderSize(CharacterManager.VISION_RANGE);
        //onProcessCombat = null;
        _pauseAnimationCounter = 0;
        //character.combatComponent.SetOnProcessCombatAction(null);
        SetMarkerColor(Color.white);
        if (_nameplate != null) {
            ObjectPoolManager.Instance.DestroyObject(_nameplate);    
        }
        _nameplate = null;
        PathfindingManager.Instance.RemoveAgent(pathfindingAI);
        RemoveListeners();
        HideHPBar();
        HideAdditionalEffect();
        _previousAreaLocation?.locationCharacterTracker.RemoveCharacterFromLocation(character, _previousAreaLocation);
        Messenger.Broadcast(CharacterSignals.CHARACTER_EXITED_AREA, character, _previousAreaLocation);
        visionCollider.Reset();
        GameObject.Destroy(visionTrigger.gameObject);
        visionTrigger = null;
        SetCollidersState(false);
        pathfindingAI.ResetThis();
        character = null;
        // previousGridTile = null;
        _previousAreaLocation = null;
        areasInWildernessForFlee.Clear();
        if (textRendererParticleSystem != null) {
            textRendererParticleSystem.Stop();    
        }
        if (animationListener != null) {
            animationListener.Reset();
        }
        base.Reset();
    }
    protected override void OnDestroy() {
        pathfindingAI = null;    
        destinationSetter = null;
        seeker = null;
        collider = null;
        character = null;
        base.OnDestroy();
    }
    protected override void DestroyAllParticleEffects() {
        Transform[] particleGOs = GameUtilities.GetComponentsInDirectChildren<Transform>(particleEffectParent.gameObject);
        if (particleGOs != null) {
            for (int i = 0; i < particleGOs.Length; i++) {
                ObjectPoolManager.Instance.DestroyObject(particleGOs[i].gameObject);
            }
        }
        particleGOs = GameUtilities.GetComponentsInDirectChildren<Transform>(particleEffectParentAllowRotation.gameObject);
        if (particleGOs != null) {
            for (int i = 0; i < particleGOs.Length; i++) {
                ObjectPoolManager.Instance.DestroyObject(particleGOs[i].gameObject);
            }
        }
    }
#endregion

#region Pathfinding Movement
    public void GoTo(LocationGridTile destinationTile, Action arrivalAction = null) {
        if (character.movementComponent.isStationary) {
            return;
        }
        //If any time a character goes to a structure outside the trap structure, the trap structure data will be cleared out
        if (character.trapStructure.IsTrappedAndTrapStructureIsNot(destinationTile.structure)) {
            character.trapStructure.ResetAllTrapStructures();
        }
        if (character.trapStructure.IsTrappedAndTrapAreaIsNot(destinationTile.area)) {
            character.trapStructure.ResetTrapArea();
        }
        pathfindingAI.ClearAllCurrentPathData();
        //pathfindingAI.SetNotAllowedStructures(notAllowedStructures);
        this.destinationTile = destinationTile;
        this.arrivalAction = arrivalAction;
        //this.failedToComputePathAction = failedToComputePathAction;
        this.targetPOI = null;
        if (destinationTile == character.gridTileLocation) {
            Action action = this.arrivalAction;
            ClearArrivalAction();
            action?.Invoke();
        } else {
            SetDestination(destinationTile.GetPositionWithinTileThatIsOnAWalkableNode(), destinationTile);
            StartMovement();
        }
    }
    public void GoToPOI(IPointOfInterest targetPOI, Action arrivalAction = null, Action failedToComputePathAction = null, STRUCTURE_TYPE[] notAllowedStructures = null, Action p_arrivalActionBeforeDigging = null) {
        if (character.movementComponent.isStationary) {
            return;
        }
        pathfindingAI.ClearAllCurrentPathData();
        pathfindingAI.SetNotAllowedStructures(notAllowedStructures);
        this.arrivalAction = arrivalAction;
        arrivalActionBeforeDigging = p_arrivalActionBeforeDigging;
        //this.failedToComputePathAction = failedToComputePathAction;
        this.targetPOI = targetPOI;
        switch (targetPOI.poiType) {
            case POINT_OF_INTEREST_TYPE.CHARACTER:
                Character targetCharacter = targetPOI as Character;
                if (targetCharacter.marker && !targetCharacter.carryComponent.masterCharacter.movementComponent.isTravellingInWorld) {
                    LocationGridTile characterTileLocation = targetCharacter.gridTileLocation;
                    if (characterTileLocation != null) {
                        //check if target character is temporarily unable to move for a long amount of time
                        //and is located at a tile that has unwalkable nodes, go to a rile that is walkable.
                        //Only checked specific traits since we do not want characters that are targeting Zapped characters to use this, since
                        //we expect that once a zapped character can move again, this character will follow that villager.
                        //Related issue: https://trello.com/c/1a1pneeJ/4869-feed-judge-failed-due-to-wall-line-of-sight
                        if (characterTileLocation.HasUnwalkableNodes() && (targetCharacter.traitContainer.HasTrait("Unconscious") || 
                                                                           targetCharacter.traitContainer.HasTrait("Paralyzed") || 
                                                                           targetCharacter.traitContainer.HasTrait("Restrained"))) {
                            SetDestination(characterTileLocation.GetPositionWithinTileThatIsOnAWalkableNode(), characterTileLocation);
                        } else {
                            SetTargetTransform(targetCharacter.marker.transform);    
                        }
                    } else {
                        SetTargetTransform(targetCharacter.marker.transform);    
                    }
                }
                break;
            default:
                if(targetPOI is MovingTileObject) {
                    SetTargetTransform(targetPOI.mapObjectVisual.transform);
                } else {
                    if (targetPOI.gridTileLocation == null) {
                        throw new Exception($"{character.name} is trying to go to a {targetPOI.ToString()} but its tile location is null");
                    }
                    LocationGridTile targetTile;
                    if (targetPOI is TileObject tileObject && tileObject.mapObjectVisual != null && tileObject.gridTileLocation != null && TileObjectDB.OccupiesMoreThan1Tile(tileObject.tileObjectType)) {
                        //added this checking for Demonic Structure Tile Objects, since they are technically located at the bottom left tile of where they are, but we want characters targeting them to
                        //go to the center of the structure instead
                        LocationGridTile tileLocationBasedOnWorldPosition = targetPOI.gridTileLocation.parentMap.GetTileFromWorldPos(targetPOI.worldPosition);
                        targetTile = tileLocationBasedOnWorldPosition;
                    } else {
                        targetTile = targetPOI.gridTileLocation;
                    }
                    Assert.IsNotNull(targetTile);
                    SetDestination(targetTile.GetPositionWithinTileThatIsOnAWalkableNode(), targetTile);
                }
                break;
        }
        StartMovement();
    }
    public void GoTo(ITraitable target, Action arrivalAction = null, Action failedToComputePathAction = null, STRUCTURE_TYPE[] notAllowedStructures = null) {
        if (character.movementComponent.isStationary) {
            return;
        }
        if (target is IPointOfInterest poi) {
            GoToPOI(poi, arrivalAction, failedToComputePathAction, notAllowedStructures);
        } else {
            pathfindingAI.ClearAllCurrentPathData();
            pathfindingAI.SetNotAllowedStructures(notAllowedStructures);
            this.arrivalAction = arrivalAction;
            this.targetPOI = null;
            SetTargetTransform(target.worldObject);
            StartMovement();
        }
    }
    public void GoTo(Vector3 destination, Action arrivalAction = null, STRUCTURE_TYPE[] notAllowedStructures = null) {
        pathfindingAI.ClearAllCurrentPathData();
        pathfindingAI.SetNotAllowedStructures(notAllowedStructures);
        this.destinationTile = destinationTile;
        this.arrivalAction = arrivalAction;
        SetTargetTransform(null);
        SetDestination(destination);
        StartMovement();
    }
    public void ArrivedAtTarget(ref bool shouldRecomputePath) {
        StopMovement();

        LocationGridTile actualDestinationTile = null;
        LocationGridTile attainedDestinationTile = null;
        ProcessDestinationAndAttainedDestinationTile(ref actualDestinationTile, ref attainedDestinationTile);

        Action actionBeforeDigging = arrivalActionBeforeDigging;
        //set arrival action to null, because some arrival actions set it
        arrivalActionBeforeDigging = null;
        actionBeforeDigging?.Invoke();

        if (character.traitContainer.HasTrait("Vampire")) {
            if (attainedDestinationTile != null && actualDestinationTile != null && actualDestinationTile != attainedDestinationTile) {
                //When path is completed and the distance between the actor and the target is still more than 1 tile, we need to assume the the path is blocked
                //Transform to bat so the character can traverse the tile
                Vampire vampireTrait = character.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
                if (vampireTrait.CanTransformIntoBat()) {
                    if (!vampireTrait.isInVampireBatForm && !vampireTrait.isTraversingUnwalkableAsBat && !character.crimeComponent.HasNonHostileVillagerInRangeThatConsidersCrimeTypeACrime(CRIME_TYPE.Vampire)) {
                        if (!PathfindingManager.Instance.HasPathEvenDiffRegion(attainedDestinationTile, actualDestinationTile)) {
                            //Only transform to bat if there is really no path between the current location and destination tile
                            //If it has, even if the destination reached is not really the destination tile, do not transform
                            if (character.interruptComponent.TriggerInterrupt(INTERRUPT.Transform_To_Bat, character)) {
                                vampireTrait.SetIsTraversingUnwalkableAsBat(true);
                                shouldRecomputePath = true;
                                return;
                            }
                        }
                    } else if (vampireTrait.isTraversingUnwalkableAsBat) {
                        //If character is still traversing unwalkable path do not do arrive action
                        shouldRecomputePath = true;
                        return;
                    }
                }
            }
        }

        if (character.combatComponent.isInCombat) {
            CombatState combatState = character.stateComponent.currentState as CombatState;
            if (combatState.isAttacking){
                if (combatState.currentClosestHostile != null && !character.movementComponent.HasPathToEvenIfDiffRegion(combatState.currentClosestHostile.gridTileLocation)) {
                    if (attainedDestinationTile != null && actualDestinationTile != null && actualDestinationTile != attainedDestinationTile) {
                        //When path is completed and the distance between the actor and the target is still more than 1 tile, we need to assume the the path is blocked
                        if (character.movementComponent.AttackBlockersOnReachEndPath(pathfindingAI.currentPath, attainedDestinationTile, actualDestinationTile)) {
                            targetPOI = null;
                            return;
                        }
                    }

                    if (character.combatComponent.RemoveHostileInRange(combatState.currentClosestHostile)) {
                        targetPOI = null;
                    }
                    return;
                }
            }
        }

        if (character.movementComponent.CanDig()) {
            if (attainedDestinationTile != null && actualDestinationTile != null && actualDestinationTile != attainedDestinationTile) {
                //Only really dig if the character has really no path towards target
                if(!PathfindingManager.Instance.HasPathEvenDiffRegion(attainedDestinationTile, actualDestinationTile)) {
                    //When path is completed and the distance between the actor and the target is still more than 1 tile, we need to assume the the path is blocked
                    if (character.movementComponent.DigOnReachEndPath(pathfindingAI.currentPath, attainedDestinationTile, actualDestinationTile)) {
                        targetPOI = null;
                        return;
                    }
                }
            }
        }

        
        Action action = arrivalAction;
        //set arrival action to null, because some arrival actions set it
        ClearArrivalAction();
#if DEBUG_PROFILER
        Profiler.BeginSample($"{character.name} - Arrived At Target - Action Invoke");
#endif
        action?.Invoke();
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif
        
        // if (actualDestinationTile == attainedDestinationTile || (attainedDestinationTile != null && actualDestinationTile != null && attainedDestinationTile.IsNeighbour(actualDestinationTile))) {
        //     Action action = arrivalAction;
        //     //set arrival action to null, because some arrival actions set it
        //     ClearArrivalAction();
        //     action?.Invoke();    
        // } else {
        //     ClearArrivalAction();
        //     if (character.currentJob != null && character.currentActionNode != null) {
        //         character.NoPathToDoJobOrAction(character.currentJob, character.currentActionNode);    
        //     }
        // }
        
        targetPOI = null;
    }
    private void ProcessDestinationAndAttainedDestinationTile(ref LocationGridTile destinationTile, ref LocationGridTile attainedDestinationTile) {
        destinationTile = GetDestinationTile();

        if (character.gridTileLocation == destinationTile || character.gridTileLocation.IsNeighbour(destinationTile)) {
            attainedDestinationTile = destinationTile;
        } else {
            attainedDestinationTile = character.gridTileLocation;
            //List<Vector3> vectorPath = pathfindingAI.currentPath.vectorPath;
            //if (vectorPath != null && vectorPath.Count > 0) {
            //    Vector3 lastPositionInPath = vectorPath.Last();
            //    //lastPositionInPath = new Vector3(Mathf.Round(lastPositionInPath.x), Mathf.Round(lastPositionInPath.y), lastPositionInPath.z);
            //    attainedDestinationTile = character.currentRegion.innerMap.GetTileFromWorldPos(lastPositionInPath);
            //} else {
            //    attainedDestinationTile = character.gridTileLocation;
            //}
        }
    }
    public LocationGridTile GetDestinationTile() {
        if (targetPOI != null) {
            return targetPOI.gridTileLocation;
        } else if (destinationTile != null) {
            return destinationTile;
        }
        return null;
    }
    public void StartMovement() {
        if (character.movementComponent.isStationary) {
            return;
        }
        isMoving = true;
        character.movementComponent.UpdateSpeed();
        pathfindingAI.SetIsStopMovement(false);
        UpdateAnimation();
        // Messenger.AddListener(Signals.TICK_ENDED, PerTickMovement);
    }
    public void StopMovement() {
        isMoving = false;
        // string log = $"{character.name} StopMovement function is called!";
        // character.logComponent.PrintLogIfActive(log);
        pathfindingAI.SetIsStopMovement(true);
        UpdateAnimation();
    }
    private void PerTickMovement() {
        if (isMoving) {
#if DEBUG_PROFILER
            Profiler.BeginSample($"{character.name} PerTickMovement");
#endif
            character.PerTickDuringMovement();
#if DEBUG_PROFILER
            Profiler.EndSample();
#endif
        }
    }
    /// <summary>
    /// Make this marker look at a specific point (In World Space).
    /// </summary>
    /// <param name="target">The target point in world space</param>
    /// <param name="force">Should this object be forced to rotate?</param>
    public override void LookAt(Vector3 target, bool force = false) {
        if (!force) {
            if (!character.limiterComponent.canPerform || !character.limiterComponent.canMove) { //character.traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE)
                return;
            }
        }
        
        Vector3 diff = target - transform.position;
        diff.Normalize();
        float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        Rotate(Quaternion.Euler(0f, 0f, rot_z - 90), force);
    }
    /// <summary>
    /// Rotate this marker to a specific angle.
    /// </summary>
    /// <param name="target">The angle this character must rotate to.</param>
    /// <param name="force">Should this object be forced to rotate?</param>
    public override void Rotate(Quaternion target, bool force = false) {
        if (!force) {
            if (!character.limiterComponent.canPerform || !character.limiterComponent.canMove) { //character.traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE)
                return;
            }
        }
        visualsParent.rotation = target;
    }
    public void SetDestination(Vector3 destination, LocationGridTile destinationTile) {
        this.destinationTile = destinationTile;
        pathfindingAI.destination = destination;
        pathfindingAI.canSearch = true;
    }
    public void SetDestination(Vector3 destination) {
        this.destinationTile = null;
        pathfindingAI.destination = destination;
        pathfindingAI.canSearch = true;
    }
    public void SetTargetTransform(Transform target) {
        destinationSetter.target = target;
        pathfindingAI.canSearch = true;
    }
    public bool IsTargetPOIInPathfinding(IPointOfInterest poi) {
        return destinationSetter.target == poi.mapObjectVisual.transform;
    }
    public void ClearArrivalAction() {
         arrivalAction = null;
    }
#endregion

#region For Testing
    public void BerserkedMarker() {
        if(mainImg.color == Color.white) {
            SetMarkerColor(Color.red);
            hairImg.materials = new Material[] { CharacterManager.Instance.spriteLightingMaterial };
            knockedOutHairImg.materials = new Material[] { CharacterManager.Instance.spriteLightingMaterial };
            hairImg.color = Color.red;
            knockedOutHairImg.color = Color.red;
        }
    }
    public void UnberserkedMarker() {
        if (mainImg.color == Color.red) {
            SetMarkerColor(Color.white);
            hairImg.materials = new Material[] { CharacterManager.Instance.spriteLightingMaterial, character.visuals.hairMaterial };
            knockedOutHairImg.materials = new Material[] { CharacterManager.Instance.spriteLightingMaterial, character.visuals.hairMaterial };
            hairImg.color = Color.white;
            knockedOutHairImg.color = Color.white;
        }
    }
    [ContextMenu("Force Update Visuals")]
    public void ForceUpdateVisuals() {
        character.visuals.UpdateAllVisuals(character);
    }
#endregion

#region Animation
    private void UpdateAnimatorController() {
        animator.runtimeAnimatorController = character.isNormalCharacter || character.characterClass.rangeType == RANGE_TYPE.RANGED ? defaultController : monsterController;
    }
    private void UpdateVisualBasedOnCurrentAnimationFrame() {
        string currSpriteName = mainImg.sprite.name;
        //Temporary fix for Wurm, if the sprite name is has "hidden", change it to "idle"
        //Ex: hidden_1 -> idle_1
        //if (currSpriteName.Contains("hidden")) {
        //    currSpriteName = currSpriteName.Replace("hidden", "idle");
        //}
        if(currSpriteName == "hidden_1") {
            currSpriteName = "idle_1";
        } else if (currSpriteName == "hidden_2") {
            currSpriteName = "idle_2";
        } else if (currSpriteName == "hidden_3") {
            currSpriteName = "idle_3";
        } else if (currSpriteName == "hidden_4") {
            currSpriteName = "idle_4";
        }
        if (character.visuals.markerAnimations.ContainsKey(currSpriteName)) {
            Sprite newSprite = character.visuals.markerAnimations[currSpriteName];
            mainImg.sprite = newSprite;
        } else {
            mainImg.sprite = character.visuals.defaultSprite;
        }
    }
    private void PlayWalkingAnimation() {
        PlayAnimation("Walk");
    }
    private void PlayIdle() {
        PlayAnimation("Idle");
    }
    private void PlaySleepGround() {
        PlayAnimation("Sleep Ground");
    }
    public void PlayAnimation(string animation) {
        if (gameObject.activeSelf == false || animator.gameObject.activeSelf == false) {
            return;
        }
        currentAnimation = animation;
        animator.Play(animation, 0, 0.5f);
    }
    public void UpdateAnimation() {
        if (gameObject.activeSelf == false) { return; }
        if (!character.carryComponent.IsNotBeingCarried()) {
            PlaySleepGround();
            ResetBlood();
            return; //if not in own party do not update any other animations
        }
        if (character.isDead) {
            PlaySleepGround();
            if (character.visuals.HasBlood()) {
                StartCoroutine(StartBlood());    
            }
        } else {
            ResetBlood();
            if (character.numOfActionsBeingPerformedOnThis > 0) {
                if ((character.limiterComponent.canMove == false || (!character.limiterComponent.canPerform && !character.limiterComponent.canWitness)) && (!character.traitContainer.HasTrait("Hibernating", "Stoned") || (!(character is Golem) && !(character is Troll)))) {
                    PlaySleepGround();
                } else {
                    PlayIdle();
                }
            } else if ((character.limiterComponent.canMove == false || (!character.limiterComponent.canPerform && !character.limiterComponent.canWitness)) && (!character.traitContainer.HasTrait("Hibernating", "Stoned") || (!(character is Golem) && !(character is Troll)))) {
                PlaySleepGround();
            } else if (isMoving) {
                PlayWalkingAnimation();
            } else if (character.currentActionNode != null && string.IsNullOrEmpty(character.currentActionNode.currentStateName) == false 
                                                           && string.IsNullOrEmpty(character.currentActionNode.currentState.animationName) == false) {
                PlayAnimation(character.currentActionNode.currentState.animationName);
            } else if (character.currentActionNode != null && !string.IsNullOrEmpty(character.currentActionNode.action.animationName)) {
                PlayAnimation(character.currentActionNode.action.animationName);
            } else {
                PlayIdle();
            }
        } 
        
        
        UpdateHairState();
    }
    private IEnumerator StartBlood() {
        bloodSplatterEffect.gameObject.SetActive(true);
        yield return GameUtilities.waitFor5Seconds;
        bloodSplatterEffect.Pause();
    }
    private void ResetBlood() {
        if (bloodSplatterEffect.gameObject.activeSelf) {
            bloodSplatterEffect.gameObject.SetActive(false);
            bloodSplatterEffect.Clear();    
        }
    }
    public void PauseAnimation() {
        _pauseAnimationCounter++;
        UpdatePauseAnimationSpeed();
    }
    public void UnpauseAnimation() {
        _pauseAnimationCounter--;
        UpdatePauseAnimationSpeed();
    }
    public void UpdatePauseAnimationSpeed() {
        if(_pauseAnimationCounter > 0) {
            animator.speed = 0;
        } else {
            animator.speed = 1;
        }
    }
    public void SetAnimationTrigger(string triggerName) {
        if (triggerName == "Attack" && (character.stateComponent.currentState is CombatState) == false) {
            return; //because sometime trigger is set even though character is no longer in combat state.
        }
        if (ReferenceEquals(animator.runtimeAnimatorController, null) == false) {
            animator.SetTrigger(triggerName);
        }
        if (triggerName == "Attack") {
            //start coroutine to call
            _animationListener.StartAttackExecution();
        }
    }
    public void SetAnimationBool(string name, bool value) {
        animator.SetBool(name, value);
    }
    private void UpdateAnimationSpeed() {
        if(animator.speed != 0) {
            animator.speed = 1f * progressionSpeedMultiplier;
        }
    }
#endregion

#region Utilities
    private void UpdateSortingOrder() {
        var characterSortingOrder = InnerMapManager.DefaultCharacterSortingOrder + character.id;
        mainImg.sortingOrder = characterSortingOrder;
        hairImg.sortingOrder = characterSortingOrder + 1;
        knockedOutHairImg.sortingOrder = characterSortingOrder + 1;
        hoveredImg.sortingOrder = characterSortingOrder - 1;
        clickedImg.sortingOrder = characterSortingOrder - 1;
        colorHighlight.sortingOrder = characterSortingOrder - 1;
        additionalEffectsImg.sortingOrder = characterSortingOrder + 2;
        bloodSplatterEffectRenderer.sortingOrder = InnerMapManager.DetailsTilemapSortingOrder + 5;
        hpBarGO.GetComponent<Canvas>().sortingOrder = InnerMapManager.DefaultCharacterSortingOrder - 1;
    }
    private new void SetActiveState(bool state) {
#if DEBUG_LOG
        Debug.Log($"Set active state of {this.name} to {state}");
#endif
        this.gameObject.SetActive(state);
    }
    /// <summary>
    /// Set the state of all visual aspects of this marker.
    /// </summary>
    /// <param name="state">The state the visuals should be in (active or inactive)</param>
    public void SetVisualState(bool state) {
        mainImg.gameObject.SetActive(state);
        _nameplate.SetVisualsState(state);
        // actionIcon.enabled = state;
        hoveredImg.enabled = state;
        particleEffectParent.gameObject.SetActive(state);
        particleEffectParentAllowRotation.gameObject.SetActive(state);
        if (!state) {
            HideHPBar();
        }
        // clickedImg.enabled = state;
    }
    public bool IsShowingVisuals() {
        return mainImg.gameObject.activeSelf;
    }
    private void UpdateHairVisuals() {
        Character character = this.character;
        if (character.reactionComponent.disguisedCharacter != null) {
            character = character.reactionComponent.disguisedCharacter;
        }
        Sprite hair = CharacterManager.Instance.GetMarkerHairSprite(character.gender);
        hairImg.sprite = hair;
        hairImg.color = Color.white;
        hairImg.material = character.visuals.hairMaterial;

        Sprite knockoutHair = CharacterManager.Instance.GetMarkerKnockedOutHairSprite(character.gender);
        knockedOutHairImg.sprite = knockoutHair;
        knockedOutHairImg.color = Color.white;
        knockedOutHairImg.material = character.visuals.hairMaterial;
    }
    public void ShowAdditionalEffect(Sprite sprite) {
        additionalEffectsImg.sprite = sprite;
        additionalEffectsImg.gameObject.SetActive(true);
    }
    public void HideAdditionalEffect() {
        additionalEffectsImg.gameObject.SetActive(false);
    }
    public bool IsShowingAdditionEffectImage(Sprite sprite) {
        return additionalEffectsImg.sprite == sprite && additionalEffectsImg.gameObject.activeSelf;
    }
    public void UpdateMarkerVisuals() {
        UpdateHairVisuals();
    }
    public void UpdatePosition() {
        //This is checked per update, stress test this for performance
        //I'm keeping a separate field called anchoredPos instead of using the rect transform anchoredPosition directly because the multithread cannot access transform components
#if DEBUG_PROFILER
        Profiler.BeginSample($"{character.name} Set Grid Tile Position");
#endif
        character.SetGridTilePosition(transform.localPosition);
        character.SetGridTileWorldPosition(transform.position);
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif
        LocationGridTile prevTile = previousGridTile;
        if (previousGridTile != character.gridTileLocation && character.gridTileLocation != null) {
            if(character != null) {
                previousGridTile = character.gridTileLocation;
                if (_previousAreaLocation == null || (_previousAreaLocation != character.areaLocation)) {
                    if (_previousAreaLocation != null) {
                        _previousAreaLocation.locationCharacterTracker.RemoveCharacterFromLocation(character, _previousAreaLocation);

#if DEBUG_PROFILER
                        Profiler.BeginSample($"{character.name} Character Exited Hextile Broadcast");
#endif
                        Messenger.Broadcast(CharacterSignals.CHARACTER_EXITED_AREA, character, _previousAreaLocation);
#if DEBUG_PROFILER
                        Profiler.EndSample();
#endif

#if DEBUG_PROFILER
                        Profiler.BeginSample($"{character.name} Remove From Awareness List");
#endif
                        if(character.currentLocationAwareness == _previousAreaLocation.locationAwareness) {
                            LocationAwarenessUtility.RemoveFromAwarenessList(character);
                        }
#if DEBUG_PROFILER
                        Profiler.EndSample();
#endif
                    }
                    
                    //When character enters new hex tile it becomes the previous hex tile altogether
                    _previousAreaLocation = character.areaLocation;
                    
                    _previousAreaLocation.locationCharacterTracker.AddCharacterAtLocation(character, _previousAreaLocation);

#if DEBUG_PROFILER
                    Profiler.BeginSample($"{character.name} Character Entered Hextile Broadcast");
#endif
                    Messenger.Broadcast(CharacterSignals.CHARACTER_ENTERED_AREA, character, _previousAreaLocation); //character.gridTileLocation.hexTileOwner
#if DEBUG_PROFILER
                    Profiler.EndSample();
#endif

#if DEBUG_PROFILER
                    Profiler.BeginSample($"{character.name} Add To Awareness List");
#endif
                    LocationAwarenessUtility.AddToAwarenessList(character, character.gridTileLocation);
#if DEBUG_PROFILER
                    Profiler.EndSample();
#endif
                }
            }
#if DEBUG_PROFILER
            Profiler.BeginSample($"{character.name} On Character Moved To");
#endif
            character.gridTileLocation.parentMap.OnCharacterMovedTo(character, character.gridTileLocation, prevTile);
#if DEBUG_PROFILER
            Profiler.EndSample();
#endif
        }
    }
    public void OnDeath(LocationGridTile deathTileLocation) {
        HideHPBar();
        if (character.minion != null || character.destroyMarkerOnDeath) {
            character.DestroyMarker(deathTileLocation);
        } else {
            ScheduleExpiry();
            SetCollidersState(false);
            //onProcessCombat = null;
            //character.combatComponent.SetOnProcessCombatAction(null);
            pathfindingAI.ClearAllCurrentPathData();
            UpdateAnimation();
            UpdateActionIcon();
            gameObject.transform.SetParent(deathTileLocation.parentMap.objectsParent);
            LocationGridTile placeMarkerAt = deathTileLocation;
            //if (deathTileLocation.isOccupied) {
            //    placeMarkerAt = deathTileLocation.GetNearestUnoccupiedTileFromThis();
            //}
            transform.position = placeMarkerAt.centeredWorldLocation;
            character.combatComponent.ClearHostilesInRange();
            ClearPOIsInVisionRange();
            character.combatComponent.ClearAvoidInRange();
            visionCollider.OnDeath();
            StartCoroutine(UpdatePositionNextFrame());
        }
    }
    private IEnumerator UpdatePositionNextFrame() {
        yield return null;
        UpdatePosition();
    }
    public void OnReturnToLife() {
        TryCancelExpiry();
        gameObject.SetActive(true);
        SetCollidersState(true);
        UpdateAnimation();
        UpdateActionIcon();
    }
    public void SetTargetPOI(IPointOfInterest poi) {
        this.targetPOI = poi;
    }
    private bool CanDoStealthCrimeToTarget(Character target, CRIME_TYPE crimeType) {
        if (!target.isDead) {
            if(target.crimeComponent.HasNonHostileVillagerInRangeThatConsidersCrimeTypeACrime(crimeType, character)) {
                return false;
            }
        } else {
            if (character.crimeComponent.HasNonHostileVillagerInRangeThatConsidersCrimeTypeACrime(crimeType, target)) {
                return false;
            }
        }
        return true;
        //if (!target.isDead) {
        //    if (target.marker.inVisionCharacters.Count > 1) {
        //        return false; //if there are 2 or more in vision of target character it means he is not alone anymore
        //    } else if (target.marker.inVisionCharacters.Count == 1) {
        //        if (!target.marker.IsPOIInVision(character)) {
        //            return false; //if there is only one in vision of target character and it is not this character, it means he is not alone
        //        }
        //    }
        //} else {
        //    if (inVisionCharacters.Count > 1) {
        //        return false;
        //    }
        //}
        //return true;
    }
    public bool CanDoStealthCrimeToTarget(IPointOfInterest target, CRIME_TYPE crimeType) {
        //If action is stealth and there is a character in vision that can witness and considers the action as a crime, then return false, this means that the actor must not do the action because there are witnesses
        if(crimeType != CRIME_TYPE.Unset && crimeType != CRIME_TYPE.None) {
            if (target is Character targetCharacter) {
                return CanDoStealthCrimeToTarget(targetCharacter, crimeType);
            }
            if (character.crimeComponent.HasNonHostileVillagerInRangeThatConsidersCrimeTypeACrime(crimeType)) {
                return false;
            }
        }
        return true;
    }
    public void SetMarkerColor(Color color) {
        mainImg.color = color;
    }
    private void UpdateHairState() {
        Character character = this.character;
        if(character.reactionComponent.disguisedCharacter != null) {
            character = character.reactionComponent.disguisedCharacter;
        }
        if (character.visuals.HasHeadHair()) {
            if (currentAnimation == "Sleep Ground") {
                knockedOutHairImg.gameObject.SetActive(true);
                hairImg.gameObject.SetActive(false);
            } else {
                knockedOutHairImg.gameObject.SetActive(false);
                hairImg.gameObject.SetActive(true);
            }
        } else {
            hairImg.gameObject.SetActive(false);
            knockedOutHairImg.gameObject.SetActive(false);
        }
    }
    public void PopulateCharactersThatIsNotDeadVillagerAndNotConversedInMinutes(List<Character> characters, int minutes) {
        for (int i = 0; i < inVisionCharacters.Count; i++) {
            Character c = inVisionCharacters[i];
            if (CharacterManager.Instance.HasCharacterNotConversedInMinutes(c, minutes) && c.isNormalCharacter && !c.isDead) {
                characters.Add(c);
            }
        }
    }
    public bool IsStillInRange(IPointOfInterest poi) {
        //I added checking for poisInRangeButDiffStructure beacuse characters are being removed from the character's avoid range when they exit a structure. (Myk)
        return IsPOIInVision(poi) || inVisionPOIsButDiffStructure.Contains(poi);
    }
    public bool HasEnemyOrRivalInVision() {
        for (int i = 0; i < inVisionCharacters.Count; i++) {
            Character otherCharacter = inVisionCharacters[i];
            if (character.relationshipContainer.IsEnemiesWith(otherCharacter)) {
                return true;
            }
        }
        return false;
    }
#endregion

#region Vision Collision
    private void CreateCollisionTrigger() {
        visionTrigger = InnerMapManager.Instance.mapObjectFactory.CreateAndInittializeCharacterVisionTrigger(character);
    }
    public void AddPOIAsInVisionRange(IPointOfInterest poi) {
        if (!IsPOIInVision(poi)) {
            inVisionPOIs.Add(poi);
            AddUnprocessedPOI(poi);
            if (poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
                inVisionCharacters.Add(poi as Character);
            } else if (poi.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
                inVisionTileObjects.Add(poi as TileObject);
            }
            //character.AddAwareness(poi);
            OnAddPOIAsInVisionRange(poi);
            Messenger.Broadcast(CharacterSignals.CHARACTER_SAW, character, poi);
        }
    }
    public bool RemovePOIFromInVisionRange(IPointOfInterest poi) {
        if (inVisionPOIs.Remove(poi)) {
            RemoveUnprocessedPOI(poi);
            if (!inVisionPOIsButDiffStructure.Contains(poi)) {
                character.combatComponent.RemoveHostileInRangeSchedule(poi);
                character.combatComponent.RemoveAvoidInRangeSchedule(poi);
            }
            if (poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
                Character target = poi as Character;
                inVisionCharacters.Remove(target);
                target.defaultCharacterTrait.RemoveCharacterThatHasReactedToThis(character);
                Messenger.Broadcast(CharacterSignals.CHARACTER_REMOVED_FROM_VISION, character, target);
            } else if (poi.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
                inVisionTileObjects.Remove(poi as TileObject);
            }
            
            
            return true;
        }
        return false;
    }
    public void AddPOIAsInRangeButDifferentStructure(IPointOfInterest poi) {
        if (inVisionPOIsButDiffStructure.Contains(poi) == false) {
            inVisionPOIsButDiffStructure.Add(poi);
        }
    }
    public bool RemovePOIAsInRangeButDifferentStructure(IPointOfInterest poi) {
        return inVisionPOIsButDiffStructure.Remove(poi);
    }
    public void ClearPOIsInVisionRangeButDiffStructure() {
        inVisionPOIsButDiffStructure.Clear();
    }
    public void ClearPOIsInVisionRange() {
        inVisionPOIs.Clear();
        inVisionCharacters.Clear();
        inVisionTileObjects.Clear();
        ClearUnprocessedPOI();
        ClearUnprocessedActions();
    }
    public void LogPOIsInVisionRange() {
#if DEBUG_LOG
        string summary = $"{character.name}'s POIs in range: ";
        for (int i = 0; i < inVisionPOIs.Count; i++) {
            summary += $"\n- {inVisionPOIs[i]}";
        }
        Debug.Log(summary);
#endif
    }
    private void OnAddPOIAsInVisionRange(IPointOfInterest poi) {
        if (character.currentActionNode != null && character.currentActionNode.target == poi && character.currentActionNode.action.IsInvalidOnVision(character.currentActionNode, out var reason)) {
            if (!string.IsNullOrEmpty(reason)) {
                GoapActionInvalidity goapActionInvalidity = new GoapActionInvalidity(true, "Target Missing", reason);
                character.currentActionNode.action.LogActionInvalid(goapActionInvalidity, character.currentActionNode, false);
            }
            character.currentActionNode.associatedJob?.CancelJob();
        }
        if (character.currentActionNode != null && character.currentActionNode.action.actionLocationType == ACTION_LOCATION_TYPE.TARGET_IN_VISION && character.currentActionNode.poiTarget == poi) {
            pathfindingAI.ClearAllCurrentPathData();
            character.PerformGoapAction();
        }
        if(poi is Character target) {
            target.stateAwarenessComponent.OnCharacterWasSeenBy(character);
        }
    }
    public void AddUnprocessedPOI(IPointOfInterest poi, bool reactToInterruptOnly = false) {
        // if (character.minion != null || character is Summon) {
        //     //Minion or Summon cannot process pois
        //     return;
        // }

        //if poi is already set as unprocessed, do not set it to check the action only, since the poi was just seen this tick.
        //so this character should react to both the character and the action/interrupt.
        if (reactToInterruptOnly && unprocessedVisionPOIs.Contains(poi) == false) {
            if (!unprocessedVisionPOIInterruptsOnly.Contains(poi)) {
                unprocessedVisionPOIInterruptsOnly.Add(poi);
            }
        }
        if (!unprocessedVisionPOIs.Contains(poi)) {
            unprocessedVisionPOIs.Add(poi);
        }
        // character.logComponent.PrintLogIfActive(character.name + " added unprocessed poi " + poi.nameWithID);
    }
    public void AddUnprocessedAction(ActualGoapNode action) {
        if (!unprocessedActionsOnly.Contains(action)) {
            unprocessedActionsOnly.Add(action);
            action.IncreaseReactionCounter();
        }
    }
    public void RemoveUnprocessedAction(ActualGoapNode action) {
        if (unprocessedActionsOnly.Remove(action)) {
            action.DecreaseReactionCounter();
        }
    }
    public void RemoveUnprocessedPOI(IPointOfInterest poi) {
        unprocessedVisionPOIs.Remove(poi);
        unprocessedVisionPOIInterruptsOnly.Remove(poi);
    }
    public void ClearUnprocessedPOI() {
        unprocessedVisionPOIs.Clear();
        unprocessedVisionPOIInterruptsOnly.Clear();
    }
    public void ClearUnprocessedActions() {
        for (int i = 0; i < unprocessedActionsOnly.Count; i++) {
            unprocessedActionsOnly[i].DecreaseReactionCounter();
        }
        unprocessedActionsOnly.Clear();
    }
    public bool HasUnprocessedPOI(IPointOfInterest poi) {
        return unprocessedVisionPOIs.Contains(poi);
    }
    private void ReprocessPOI(IPointOfInterest poi) {
        if (HasUnprocessedPOI(poi) == false && IsPOIInVision(poi)) {
            AddUnprocessedPOI(poi);
        }
    }
    private void ProcessAllUnprocessedVisionPOIs() {
        if (character == null) { return; }
        // string log = $"{character.name} tick ended! Processing all unprocessed in visions...";
        if (unprocessedVisionPOIs.Count > 0) {
            if (!character.isDead && character.reactionComponent.disguisedCharacter == null /* && character.limiterComponent.canWitness*/) { //character.traitContainer.GetNormalTrait<Trait>("Unconscious", "Resting", "Zapped") == null
#if DEBUG_PROFILER
                Profiler.BeginSample($"{character.name} ProcessAllUnprocessedVisionPOIs - Objects");
#endif
                for (int i = 0; i < unprocessedVisionPOIs.Count; i++) {
                    IPointOfInterest poi = unprocessedVisionPOIs[i];
                    if (poi.mapObjectVisual == null) {
                        // log = $"{log}\n-{poi.nameWithID}'s map visual has been destroyed. Skipping...";
                        continue;
                    }
                    if (poi.isHidden) {
                        // log = $"{log}\n-{poi.nameWithID} is hidden. Skipping...";
                        continue;
                    }
                    if(poi is Character target) {
                        //After dropping a character, the carrier should not immediately react to the recently dropped character
                        if(target.carryComponent.prevCarriedBy != null && target.carryComponent.prevCarriedBy == character) {
                            // log = $"{log}\n-{poi.nameWithID} is just got dropped. Skipping...";
                            target.carryComponent.SetPrevCarriedBy(null);
                            continue;
                        }
                    }
                    if(!visionCollider.IsTheSameStructureOrSameOpenSpaceWithPOI(poi)) {
                        //Before reacting to a character check first if he is in vision list, if he is not and he is not in line of sight, do not react
                        if (!IsCharacterInLineOfSightWith(poi)) {
                            // log = $"{log}\n-{poi.nameWithID} is not in same space and no longer in line of sight with actor. Skipping...";
                            continue;
                        }
                    }
                    // log = $"{log}\n-{poi.nameWithID}";
                    bool reactToActionOnly = false;
                    if (unprocessedVisionPOIInterruptsOnly.Count > 0) {
                        reactToActionOnly = unprocessedVisionPOIInterruptsOnly.Contains(poi);
                    }
                    character.ThisCharacterSaw(poi, reactToActionOnly);
                }
#if DEBUG_PROFILER
                Profiler.EndSample();
#endif
            } else {
                // log = $"{log}\n - Character is either dead, not processing...";
            }
            ClearUnprocessedPOI();
        }
        if (unprocessedActionsOnly.Count > 0) {
            if (!character.isDead) {
#if DEBUG_PROFILER
                Profiler.BeginSample($"{character.name} ProcessAllUnprocessedVisionPOIs - Actions");
#endif
                for (int i = 0; i < unprocessedActionsOnly.Count; i++) {
                    ActualGoapNode action = unprocessedActionsOnly[i];
                    Character actor = action.actor;
                    // log = $"{log}\n-{action.goapName} of {actor.name} towards {action.poiTarget.name}";
                    if (!visionCollider.IsTheSameStructureOrSameOpenSpaceWithPOI(actor)) {
                        //Before reacting to a character check first if he is in vision list, if he is not and he is not in line of sight, do not react
                        if (!IsCharacterInLineOfSightWith(actor)) {
                            // log = $"{log}\n-{actor.nameWithID} is not in same space and no longer in line of sight with actor. Skipping...";
                            continue;
                        }
                    }
                    character.ThisCharacterSawAction(action);
                }
#if DEBUG_PROFILER
                Profiler.EndSample();
#endif
            } else {
                // log = $"{log}\n - Character is either dead, not processing...";
            }
            ClearUnprocessedActions();
        }
        // character.logComponent.PrintLogIfActive(log);
        character.SetHasSeenFire(false);
        character.SetHasSeenWet(false);
        character.SetHasSeenPoisoned(false);
        character.combatComponent.CheckCombatPerTickEnded();
    }
    public bool IsPOIInVision(IPointOfInterest poi) {
        if (character == null) { return false; }
        return poi.CanBeSeenBy(character); //(poi is Character character && inVisionCharacters.Contains(character)) || (poi is TileObject tileObject && inVisionTileObjects.Contains(tileObject));
    }
#endregion

#region Hosility Collision
    //public bool AddHostileInRange(IPointOfInterest poi, bool checkHostility = true, bool processCombatBehavior = true, bool isLethal = true, bool gotHit = false) {
    //    if (!IsHostileInRange(poi)) {
    //        //&& !this.character.traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE) 
    //        if (this.character.traitContainer.GetNormalTrait<Trait>("Zapped") == null && !this.character.isFollowingPlayerInstruction && CanAddPOIAsHostile(poi, checkHostility, isLethal)) {
    //            string transferReason = string.Empty;
    //            if (!WillCharacterTransferEngageToFleeList(isLethal, ref transferReason, gotHit)) {
    //                hostilesInRange.Add(poi);
    //                if (poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
    //                    lethalCharacters.Add(poi as Character, isLethal);
    //                }
    //                this.character.logComponent.PrintLogIfActive(poi.name + " was added to " + this.character.name + "'s hostile range!");
    //                //When adding hostile in range, check if character is already in combat state, if it is, only reevaluate combat behavior, if not, enter combat state
    //                //if (processCombatBehavior) {
    //                //    ProcessCombatBehavior();
    //                //}

    //                willProcessCombat = true;
    //            } else {
    //                //Transfer to flee list
    //                return AddAvoidInRange(poi, processCombatBehavior, transferReason);
    //            }
    //            return true;
    //        }
    //    } else {
    //        if (gotHit) {
    //            //When a poi hits this character, the behavior would be to add that poi to this character's hostile list so he can attack back
    //            //However, there are times that the attacker is already in the hostile list
    //            //If that happens, the behavior would be to evaluate the situation if the character will avoid or continue attacking
    //            string transferReason = string.Empty;
    //            if (WillCharacterTransferEngageToFleeList(isLethal, ref transferReason, gotHit)) {
    //                Messenger.Broadcast(Signals.TRANSFER_ENGAGE_TO_FLEE_LIST, character, transferReason);
    //                willProcessCombat = true;
    //            }

    //        }
    //    }
    //    return false;
    //}
    //private bool CanAddPOIAsHostile(IPointOfInterest poi, bool checkHostility, bool isLethal) {
    //    if (poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
    //        Character character = poi as Character;
    //        if (isLethal == false && character.canBeAtttacked == false) {
    //            //if combat intent is not lethal and the target cannot be attacked, then do not allow target to be added as a hostile,
    //            //otherwise ignore limiterComponent.canBeAttacked value
    //            return false;
    //        }
    //        return !character.isDead && !this.character.isFollowingPlayerInstruction &&
    //            (!checkHostility || this.character.IsHostileWith(character));
    //    } else {
    //        return true; //allow any other object types
    //    }
    //}
    //public void RemoveHostileInRange(IPointOfInterest poi, bool processCombatBehavior = true) {
    //    if (hostilesInRange.Remove(poi)) {
    //        if (poi is Character) {
    //            lethalCharacters.Remove(poi as Character);
    //        }
    //        string removeHostileSummary = poi.name + " was removed from " + character.name + "'s hostile range.";
    //        character.logComponent.PrintLogIfActive(removeHostileSummary);
    //        //When removing hostile in range, check if character is still in combat state, if it is, reevaluate combat behavior, if not, do nothing
    //        if (processCombatBehavior && character.combatComponent.isInCombat) {
    //            CombatState combatState = character.stateComponent.currentState as CombatState;
    //            if (combatState.forcedTarget == poi) {
    //                combatState.SetForcedTarget(null);
    //            }
    //            if (combatState.currentClosestHostile == poi) {
    //                combatState.ResetClosestHostile();
    //            }
    //            Messenger.Broadcast(Signals.DETERMINE_COMBAT_REACTION, this.character);
    //        }
    //    }
    //}
    //public void ClearHostilesInRange(bool processCombatBehavior = true) {
    //    if(hostilesInRange.Count > 0) {
    //        hostilesInRange.Clear();
    //        lethalCharacters.Clear();
    //        //When adding hostile in range, check if character is already in combat state, if it is, only reevaluate combat behavior, if not, enter combat state
    //        if (processCombatBehavior) {
    //            if (character.combatComponent.isInCombat) {
    //                Messenger.Broadcast(Signals.DETERMINE_COMBAT_REACTION, this.character);
    //            } 
    //            //else {
    //            //    character.stateComponent.SwitchToState(CHARACTER_STATE.COMBAT);
    //            //}
    //        }
    //    }
    //}
    public void OnOtherCharacterDied(Character otherCharacter) {
        //NOTE: This is no longer needed since this will only cause duplicates because CreateJobsOnEnterVisionWith will also be called upon adding the Dead trait
        //if (IsPOIInVision(otherCharacter)) {
        //    character.CreateJobsOnEnterVisionWith(otherCharacter); //this is used to create jobs that involve characters that died within the character's range of vision
        //}


        //RemovePOIFromInVisionRange(otherCharacter);
        //visionCollision.RemovePOIAsInRangeButDifferentStructure(otherCharacter);

        character.combatComponent.RemoveHostileInRange(otherCharacter); //removed hostile because he/she died.
        character.combatComponent.RemoveAvoidInRange(otherCharacter);

        if (targetPOI == otherCharacter) {
            //if (this.arrivalAction != null) {
            //    Debug.Log(otherCharacter.name + " died, executing arrival action " + this.arrivalAction.Method.Name);
            //} else {
            //    Debug.Log(otherCharacter.name + " died, executing arrival action None");
            //}
            //execute the arrival action, the arrival action should handle the cases for when the target is missing
            Action action = arrivalAction;
            //set arrival action to null, because some arrival actions set it when executed
            ClearArrivalAction();
            action?.Invoke();
        }
    }
    public void OnBeforeSeizingOtherCharacter(Character otherCharacter) {
        //if (character.faction != null && character.faction.isMajorNonPlayerFriendlyNeutral) {
        //    if (IsPOIInVision(otherCharacter)) {
        //        PlayerManager.Instance.player.threatComponent.AdjustThreat(10);
        //    }
        //}
    }
    public void OnBeforeSeizingThisCharacter() {
        //for (int i = 0; i < inVisionCharacters.Count; i++) {
        //    Character inVision = inVisionCharacters[i];
        //    if (inVision.faction != null && inVision.faction.isMajorNonPlayerFriendlyNeutral) {
        //        PlayerManager.Instance.player.threatComponent.AdjustThreat(10);
        //    }
        //}
    }
    //public bool IsLethalCombatForTarget(Character character) {
    //    if (lethalCharacters.ContainsKey(character)) {
    //        return lethalCharacters[character];
    //    }
    //    return true;
    //}
    //public bool HasLethalCombatTarget() {
    //    for (int i = 0; i < hostilesInRange.Count; i++) {
    //        IPointOfInterest poi = hostilesInRange[i];
    //        if (poi is Character) {
    //            Character hostile = poi as Character;
    //            if (IsLethalCombatForTarget(hostile)) {
    //                return true;
    //            }
    //        }

    //    }
    //    return false;
    //}
    #endregion

    #region Avoid In Range
    //public bool AddAvoidInRange(IPointOfInterest poi, bool processCombatBehavior = true, string reason = "") {
    //    if (poi is Character) {
    //        return AddAvoidInRange(poi as Character, processCombatBehavior, reason);
    //    } else {
    //        if (character.traitContainer.GetNormalTrait<Trait>("Berserked") == null) {
    //            if (!IsAvoidInRange(poi)) {
    //                avoidInRange.Add(poi);
    //                willProcessCombat = true;
    //                avoidReason = reason;
    //                return true;
    //            }
    //        }
    //    }
    //    return false;
    //}
    //private bool AddAvoidInRange(Character poi, bool processCombatBehavior = true, string reason = "") {
    //    if (!poi.isDead && !poi.traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE) && character.traitContainer.GetNormalTrait<Trait>("Berserked") == null) { //, "Resting"
    //        if (!IsAvoidInRange(poi)) {
    //            avoidInRange.Add(poi);
    //            //NormalReactToHostileCharacter(poi, CHARACTER_STATE.FLEE);
    //            //When adding hostile in range, check if character is already in combat state, if it is, only reevaluate combat behavior, if not, enter combat state
    //            //if (processCombatBehavior) {
    //            //    ProcessCombatBehavior();
    //            //}
    //            willProcessCombat = true;
    //            avoidReason = reason;
    //            return true;
    //        }
    //    }
    //    return false;
    //}
    //public bool AddAvoidsInRange(List<IPointOfInterest> pois, bool processCombatBehavior = true, string reason = "") {
    //    //Only react to the first hostile that is added
    //    IPointOfInterest otherPOI = null;
    //    for (int i = 0; i < pois.Count; i++) {
    //        IPointOfInterest poi = pois[i];
    //        if (poi is Character) {
    //            Character characterToAvoid = poi as Character;
    //            if (characterToAvoid.isDead || characterToAvoid.traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE) || characterToAvoid.traitContainer.GetNormalTrait<Trait>("Berserked") != null) {
    //                continue; //skip
    //            }
    //        }
    //        if (!IsAvoidInRange(poi)) {
    //            avoidInRange.Add(poi);
    //            if (otherPOI == null) {
    //                otherPOI = poi;
    //            }
    //        }

    //    }
    //    if (otherPOI != null) {
    //        willProcessCombat = true;
    //        avoidReason = reason;
    //        return true;
    //    }
    //    return false;
    //}
    //public bool AddAvoidsInRange(List<Character> pois, bool processCombatBehavior = true, string reason = "") {
    //    //Only react to the first hostile that is added
    //    Character otherPOI = null;
    //    for (int i = 0; i < pois.Count; i++) {
    //        Character poi = pois[i];
    //        if (!poi.isDead && !poi.traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE) && poi.traitContainer.GetNormalTrait<Trait>("Berserked") == null) {
    //            if (!IsAvoidInRange(poi)) {
    //                avoidInRange.Add(poi);
    //                if (otherPOI == null) {
    //                    otherPOI = poi;
    //                }
    //                //return true;
    //            }
    //        }
    //    }
    //    if (otherPOI != null) {
    //        willProcessCombat = true;
    //        avoidReason = reason;
    //        return true;
    //    }
    //    return false;
    //}
    //public void RemoveAvoidInRange(IPointOfInterest poi, bool processCombatBehavior = true) {
    //    if (avoidInRange.Remove(poi)) {
    //        //Debug.Log("Removed avoid in range " + poi.name + " from " + this.character.name);
    //        //When adding hostile in range, check if character is already in combat state, if it is, only reevaluate combat behavior, if not, enter combat state
    //        if (processCombatBehavior) {
    //            if (character.combatComponent.isInCombat) {
    //                Messenger.Broadcast(Signals.DETERMINE_COMBAT_REACTION, this.character);
    //            }
    //        }
    //    }
    //}
    //public void ClearAvoidInRange(bool processCombatBehavior = true) {
    //    if(avoidInRange.Count > 0) {
    //        avoidInRange.Clear();

    //        //When adding hostile in range, check if character is already in combat state, if it is, only reevaluate combat behavior, if not, enter combat state
    //        if (processCombatBehavior) {
    //            if (character.combatComponent.isInCombat) {
    //                Messenger.Broadcast(Signals.DETERMINE_COMBAT_REACTION, this.character);
    //            } 
    //            //else {
    //            //    character.stateComponent.SwitchToState(CHARACTER_STATE.COMBAT);
    //            //}
    //        }
    //    }
    //}
    #endregion

    #region Flee
    public void OnStartFlee() {
        if (!hasFleePath) {
            //Only clear path on the first start of flee, once the character is already in flee path and decided to flee again do not clear path anymore
            pathfindingAI.ClearAllCurrentPathData();
        }
        SetHasFleePath(true);
        pathfindingAI.canSearch = false; //set to false, because if this is true and a destination has been set in the ai path, the ai will still try and go to that point instead of the computed flee path

        List<Area> playerAreas = PlayerManager.Instance.player.playerSettlement.areas;
        if (avoidThisPositions == null) {
            avoidThisPositions = new List<Vector3>();
        } else {
            avoidThisPositions.Clear();
        }
        if (character.combatComponent.avoidInRange.Count > 0) {
            for (int i = 0; i < character.combatComponent.avoidInRange.Count; i++) {
                IPointOfInterest poi = character.combatComponent.avoidInRange[i];
                if(IsStillInRange(poi)) {
                    avoidThisPositions.Add(poi.gridTileLocation.worldLocation);
                }
            }
        }

        //TODO: Must be on see only because the flee path will be messed up if they always avoid the hexes
        //Corrupted hexes should also be avoided
        //https://trello.com/c/6WJtivlY/1274-fleeing-should-not-go-to-corrupted-structures
        if (character.isNormalCharacter) {
            if (playerAreas.Count > 0) {
                for (int i = 0; i < playerAreas.Count; i++) {
                    Area corruptedArea = playerAreas[i];
                    if (corruptedArea.region == character.currentRegion) {
                        if (character.gridTileLocation != null) {
                            if (character.areaLocation == corruptedArea) {
                                avoidThisPositions.Add(corruptedArea.gridTileComponent.centerGridTile.worldLocation);
                            }
                        }
                    }
                }
            }
        }
        ReconstructFleePath();
    }
    public void OnStartFleeToPartyMate() {
        Character chosenCharacter = null;
        Character secondChosenCharacter = null;

        LocationGridTile currentTileLocation = character.gridTileLocation;
        if (character.partyComponent.isMemberThatJoinedQuest && currentTileLocation != null) {
            for (int i = 0; i < character.partyComponent.currentParty.membersThatJoinedQuest.Count; i++) {
                Character member = character.partyComponent.currentParty.membersThatJoinedQuest[i];
                LocationGridTile memberTileLocation = member.gridTileLocation;
                if (character != member && member.limiterComponent.canPerform && member.limiterComponent.canMove && member.hasMarker && !member.isBeingSeized
                    && member.carryComponent.IsNotBeingCarried() && memberTileLocation != null) {
                    if (character.movementComponent.HasPathToEvenIfDiffRegion(memberTileLocation)) {
                        float dist = currentTileLocation.GetDistanceTo(memberTileLocation);
                        if(dist <= 20f) {
                            if (member.combatComponent.combatBehaviourParent.IsCombatBehaviour(CHARACTER_COMBAT_BEHAVIOUR.Tank)) {
                                chosenCharacter = member;
                                break;
                            } else if(secondChosenCharacter == null) {
                                secondChosenCharacter = member;
                            }
                        }
                    }
                }
            }
        }
        if(chosenCharacter != null || secondChosenCharacter != null) {
            if (!hasFleePath) {
                //Only clear path on the first start of flee, once the character is already in flee path and decided to flee again do not clear path anymore
                pathfindingAI.ClearAllCurrentPathData();
            }
            SetHasFleePath(true);
            pathfindingAI.canSearch = false; //set to false, because if this is true and a destination has been set in the ai path, the ai will still try and go to that point instead of the computed flee path
            if (chosenCharacter != null) {
                GoTo(chosenCharacter, OnFinishedTraversingFleePath);
            } else if (secondChosenCharacter != null) {
                GoTo(secondChosenCharacter, OnFinishedTraversingFleePath);
            }
        } else {
            OnStartFlee();
        }
    }
    public void OnStartFleeToHome() {
        if (!hasFleePath) {
            //Only clear path on the first start of flee, once the character is already in flee path and decided to flee again do not clear path anymore
            pathfindingAI.ClearAllCurrentPathData();
        }
        SetHasFleePath(true);
        LocationGridTile chosenTile = null;
        if(character.homeStructure != null) {
            chosenTile = character.homeStructure.GetRandomTile();
        } else if (character.HasTerritory()) {
            chosenTile = character.GetRandomLocationGridTileWithPath();
        }
        if (chosenTile != null) {
            if (character.currentRegion != chosenTile.structure.region) {
                if (character.movementComponent.MoveToAnotherRegion(chosenTile.structure.region, () => GoTo(chosenTile, OnFinishedTraversingFleePath)) == false) {
                    OnStartFlee();
                }
            } else {
                GoTo(chosenTile, OnFinishedTraversingFleePath);
            }
        } else {
            OnStartFlee();
        }
    }
    public void OnStartFleeToOutside() {
        if (!hasFleePath) {
            //Only clear path on the first start of flee, once the character is already in flee path and decided to flee again do not clear path anymore
            pathfindingAI.ClearAllCurrentPathData();
        }
        SetHasFleePath(true);
        LocationGridTile chosenTile = null;
        if(character.currentStructure != null && character.currentStructure.structureType.IsSpecialStructure()) {
            Area area = character.areaLocation;
            areasInWildernessForFlee.Clear();
            for (int i = 0; i < area.neighbourComponent.neighbours.Count; i++) {
                Area neighbour = area.neighbourComponent.neighbours[i];
                LocationGridTile neighbourCenter = neighbour.gridTileComponent.centerGridTile;
                if (character.movementComponent.HasPathTo(neighbourCenter)) {
                    if(neighbourCenter.structure.structureType == STRUCTURE_TYPE.WILDERNESS) {
                        BaseSettlement settlement = null;
                        character.gridTileLocation?.IsPartOfSettlement(out settlement);
                        if(settlement == null || neighbour.settlementOnArea == null || neighbour.settlementOnArea != settlement) {
                            areasInWildernessForFlee.Add(neighbour);
                        }
                    }
                }
            }
            if(areasInWildernessForFlee.Count > 0) {
                Area randomArea = CollectionUtilities.GetRandomElement(areasInWildernessForFlee);
                chosenTile = randomArea.gridTileComponent.centerGridTile;
            }
        }
        if (chosenTile != null) {
            GoTo(chosenTile, OnFinishedTraversingFleePath);
        } else {
            OnStartFlee();
        }
    }
    public void ReconstructFleePath() {
        if(avoidThisPositions.Count > 0) {
            FleeMultiplePath fleePath = FleeMultiplePath.Construct(this.transform.position, avoidThisPositions, CombatManager.Instance.searchLength);
            fleePath.aimStrength = CombatManager.Instance.aimStrength;
            fleePath.spread = CombatManager.Instance.spread;
            seeker.StartPath(fleePath);
        } else {
            //No positions to avoid, should redetermine combat action, or if there is no combat state anymore, just finish path
            OnFinishedTraversingFleePath();
        }
    }
    public void OnFleePathComputed(Path path) {
        if (character == null || !character.limiterComponent.canPerform || !character.limiterComponent.canMove) {
            return; //this is for cases that the character is no longer in a combat state, but the pathfinding thread returns a flee path
        }
        arrivalAction = OnFinishedTraversingFleePath;
        StartMovement();
    }
    public void OnFinishedTraversingFleePath() {
        if (character.combatComponent.isInCombat) {
            (character.stateComponent.currentState as CombatState).FinishedTravellingFleePath();
        } else {
            SetHasFleePath(false);
        }
    }
    public void SetHasFleePath(bool state) {
        hasFleePath = state;
    }
    public void AddAvoidPositions(Vector3 avoid) {
        if (avoidThisPositions == null) {
            avoidThisPositions = new List<Vector3>();
        }
        avoidThisPositions.Add(avoid);
        ReconstructFleePath();
    }
#endregion

#region Combat
    public void UpdateAttackSpeedMeter() {
        if (hpBarGO.activeSelf) {
            aspeedFill.fillAmount = attackSpeedMeter / character.combatComponent.attackSpeed;
        }
    }
    public void ResetAttackSpeed() {
        attackSpeedMeter = 0f;
        UpdateAttackSpeedMeter();
    }
    public bool CanAttackByAttackSpeed() {
        return attackSpeedMeter >= character.combatComponent.attackSpeed;
    }
    //private readonly RaycastHit2D[] linOfSightHitObjects = new RaycastHit2D[5];
    private RaycastHit2D[] lineOfSightHitObjects;
    public bool IsCharacterInLineOfSightWith(IPointOfInterest target, float rayDistance = 5f) {
        //No longer checks if target is in vision, rather, it should check if target has a map visual object, if it does not, there will be no line of sight
        //Also, there is no line of sight if actor and target is in a different region
        if (target.mapObjectVisual == null) {
            return false;
        }
        if (character == null || character.currentRegion == null || target.gridTileLocation == null || character.currentRegion != target.gridTileLocation.structure.region) {
            return false;
        }
        //precompute our ray settings
        Vector3 start = transform.position;
        Vector3 direction = GameUtilities.VectorSubtraction(target.worldPosition, start).normalized;
        float distance = rayDistance;
        if (target.IsUnpassable()) {
            distance += 1.5f;
        }
        //do the ray test
        //int size = Physics2D.RaycastNonAlloc(start, direction, linOfSightHitObjects, distance, 
        //    GameUtilities.Line_Of_Sight_Layer_Mask);
        lineOfSightHitObjects = Physics2D.RaycastAll(start, direction, distance, GameUtilities.Line_Of_Sight_Layer_Mask);
        
        if(lineOfSightHitObjects != null) {
            for (int i = 0; i < lineOfSightHitObjects.Length; i++) {
                RaycastHit2D hit = lineOfSightHitObjects[i];
                if (!target.IsUnpassable() && hit.collider.gameObject.layer == LayerMask.NameToLayer("Unpassable")) {
                    return false;
                } else if (hit.transform.IsChildOf(target.mapObjectVisual.transform)) {
                    return true;
                }
            }
        }
        return false;
    }
#endregion

#region Colliders
    public void SetCollidersState(bool state) {
        //for (int i = 0; i < collider.Length; i++) {
        //    collider[i].enabled = state;
        //}
        collider.enabled = state;
    }
    public void SetAllColliderStates(bool state) {
        SetCollidersState(state);
        visionTrigger.SetAllCollidersState(state);
    }
    public void SetVisionColliderSize(int size) {
        if (_currentColliderSize != size) {
            _currentColliderSize = size;
            collider.size = new Vector2(size, size);    
        }
    }
#endregion

#region Map Object Visual
    public override void UpdateTileObjectVisual(Character obj) { }
    public virtual void ApplyFurnitureSettings(FurnitureSetting furnitureSetting) { }
    public override void SetVisualAlpha(float alpha) {
        base.SetVisualAlpha(alpha);
        
        Color color = hairImg.color;
        color.a = alpha;
        hairImg.color = color;
        
        color = knockedOutHairImg.color;
        color.a = alpha;
        knockedOutHairImg.color = color;
    }
#endregion

#region Seize
    public void OnSeize() {
        Character _character = character;
        //TODO: Change logic of this, only quick fix for webbbed characters that are seized
        bool isAdditionalEffectActive = additionalEffectsImg.gameObject.activeSelf;
        Reset();
        if (isAdditionalEffectActive) {
            ShowAdditionalEffect(additionalEffectsImg.sprite);
        }
        character = _character;
        buttonCollider.enabled = false;
    }
    public void OnUnseize() {
        buttonCollider.enabled = true;
    }
#endregion

#region Expiry
    public void TryCancelExpiry() {
        if (String.IsNullOrEmpty(_destroySchedule) == false) {
            SchedulingManager.Instance.RemoveSpecificEntry(_destroySchedule);
            _destroySchedule = string.Empty;
        }
    }
    public void ScheduleExpiry() {
        if (String.IsNullOrEmpty(_destroySchedule)) {
            _destroyDate = GameManager.Instance.Today();
            _destroyDate.AddDays(3);
            // _destroyDate.AddTicks(3);
#if DEBUG_LOG
            Debug.Log($"{character.name}'s marker will expire at {_destroyDate.ConvertToContinuousDaysWithTime()}");
#endif
            _destroySchedule = SchedulingManager.Instance.AddEntry(_destroyDate, TryExpire, character);    
        }
    }
    private void ScheduleExpiry(GameDate gameDate) {
        if (String.IsNullOrEmpty(_destroySchedule)) {
            _destroyDate = gameDate;
            _destroySchedule = SchedulingManager.Instance.AddEntry(_destroyDate, TryExpire, character);    
        }
    }
    private void TryExpire() {
        bool canExpire = character.numOfActionsBeingPerformedOnThis <= 0;
        if (character.isBeingCarriedBy != null) {
            canExpire = false;
        }
        if (character.isBeingSeized) {
            canExpire = false;
        }
        if (canExpire) {
            Expire();    
        } else {
            //reschedule expiry to next hour.
            _destroyDate = GameManager.Instance.Today();
            _destroyDate.AddTicks(GameManager.ticksPerHour);
            _destroySchedule = SchedulingManager.Instance.AddEntry(_destroyDate, TryExpire, character);    
        }
        
    }
    private void Expire() {
#if DEBUG_LOG
        Debug.Log($"{character.name}'s marker has expired.");
#endif
        character.ForceCancelAllJobsTargetingThisCharacter(false);
        Messenger.Broadcast(CharacterSignals.CHARACTER_MARKER_EXPIRED, character);
        character.DestroyMarker();
    }
#endregion

#region Nameplate
    public void ShowThoughtsAndNameplate() {
        if (!_nameplate) {
            return;
        }
        _nameplate.ShowThoughts();
    }
    public void HideThoughtsAndNameplate() {
        if (!_nameplate) {
            return;
        }
        _nameplate.HideThoughts();
    }
    public void UpdateNameplateElementsState() {
        if (!_nameplate) {
            return;
        }
        _nameplate.UpdateElementsStateBasedOnActiveCharacter();
    }
#endregion

#region Stroll
    public void DoStrollMovement() {
#if DEBUG_PROFILER
        Profiler.BeginSample($"{character.name} - Do Stroll Movement");
#endif
        StopMovement();
        pathfindingAI.ClearAllCurrentPathData();
        ConstantPath constantPath = ConstantPath.Construct(transform.position, 10000, OnStrollPathComputed);
        AstarPath.StartPath(constantPath);
        // constantPath.BlockUntilCalculated();
        // if (constantPath.allNodes != null && constantPath.allNodes.Count > 0) {
        //     GoTo(PathUtilities.GetPointsOnNodes(constantPath.allNodes, 1, 5).Last(), arrivalAction);    
        // } else {
        //     if (character.stateComponent.currentState is StrollOutsideState) {
        //         //could not find nodes to stroll to. Just exit stroll state.
        //         character.stateComponent.ExitCurrentState();    
        //     }
        // }
        UpdateAnimation();
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif
    }
    public void OnStrollPathComputed(Path path) {
        if (character == null) {
            return;
        }
        if (path is ConstantPath constantPath && character.stateComponent.currentState is StrollOutsideState strollOutsideState) {
            if (character.jobQueue.jobsInQueue.Count > 1 && character.jobQueue.jobsInQueue[0] != strollOutsideState.job) {
                character.stateComponent.ExitCurrentState();
            } else {
                if (constantPath.allNodes != null && constantPath.allNodes.Count > 0) {
                    GoTo(PathUtilities.GetPointsOnNodes(constantPath.allNodes, 1, 5).Last(), strollOutsideState.StartStrollMovement);    
                } else {
                    //could not find nodes to stroll to. Just exit stroll state.
                    character.stateComponent.ExitCurrentState();
                }    
            }
                
        }
    }
#endregion

#region Tags
    public void UpdateTraversableTags() {
        seeker.traversableTags = character.movementComponent.traversableTags;
    }
    public void UpdateTagPenalties() {
        seeker.tagPenalties = character.movementComponent.tagPenalties;
    }
#endregion

#region Effects
    public void ShowHealthAdjustmentEffect(int damage, CombatComponent p_combatComponent) {
        Color color = Color.green;
        float startSize = 1.5f;
        if(p_combatComponent == null) {
            color = damage > 0 ? Color.green : Color.red;
            textRendererParticleSystem.SpawnParticle(transform.position, damage, color, startSize);
        } else {
            switch (p_combatComponent.damageDone.damageType) {
                case CombatComponent.DamageDoneType.DamageType.Normal:
                color = damage > 0 ? Color.green : Color.red;
                break;
                case CombatComponent.DamageDoneType.DamageType.Crit:
                color = Color.yellow;
                startSize = 2.5f;
                break;
            }
            textRendererParticleSystem.SpawnParticle(transform.position, damage, color, startSize);
        }
        
    }
    public void ShowHealthAdjustmentEffect(float damage) {
        Color color = damage > 0 ? Color.green : Color.red;
        float startSize = 1.5f;
        textRendererParticleSystem.SpawnParticle(transform.position, damage, color, startSize);
    }
    public void ShowTextEffect(string p_text, Color p_color) {
        float startSize = 1.5f;
        textRendererParticleSystem.SpawnParticle(transform.position, p_text, p_color, startSize);
    }
    #endregion
}
