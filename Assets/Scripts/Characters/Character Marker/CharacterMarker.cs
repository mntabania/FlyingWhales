using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using TMPro;
using Pathfinding;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Traits;
using UnityEngine.Profiling;
using UnityEngine.Serialization;
using UtilityScripts;

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
    public Transform particleEffectParent;
    public Transform particleEffectParentAllowRotation;

    [Header("Animation")]
    public Animator animator;
    [FormerlySerializedAs("animationListener")] [SerializeField] private CharacterMarkerAnimationListener _animationListener;
    [SerializeField] private string currentAnimation;
    [SerializeField] private RuntimeAnimatorController defaultController;
    [SerializeField] private RuntimeAnimatorController monsterController;
    
    [Header("Pathfinding")]
    public CharacterAIPath pathfindingAI;    
    public AIDestinationSetter destinationSetter;
    public Seeker seeker;
    public Collider2D[] colliders;
    [FormerlySerializedAs("visionCollision")] public CharacterMarkerVisionCollider visionCollider;

    [Header("Combat")]
    public Transform projectileParent;

    [Header("For Testing")]
    [SerializeField] private SpriteRenderer colorHighlight;

    private string _destroySchedule;

    //vision colliders
    public List<IPointOfInterest> inVisionPOIs { get; private set; } //POI's in this characters vision collider
    public List<IPointOfInterest> inVisionPOIsButDiffStructure { get; private set; } //POI's in this characters vision collider
    public List<IPointOfInterest> unprocessedVisionPOIs { get; private set; } //POI's in this characters vision collider
    private List<IPointOfInterest> unprocessedVisionPOIInterruptsOnly { get; set; } //POI's in this characters vision collider
    private List<ActualGoapNode> unprocessedActionsOnly { get; set; } //POI's in this characters vision collider
    public List<Character> inVisionCharacters { get; private set; } //POI's in this characters vision collider
    public List<TileObject> inVisionTileObjects { get; private set; } //POI's in this characters vision collider
    public Action arrivalAction { get; private set; }
    private Action failedToComputePathAction { get; set; }

    //movement
    public IPointOfInterest targetPOI { get; private set; }
    public Vector2 anchoredPos { get; private set; }
    public LocationGridTile destinationTile { get; private set; }
    public float progressionSpeedMultiplier { get; private set; }
    public bool isMoving { get; private set; }
    private LocationGridTile previousGridTile {
        get => _previousGridTile;
        set {
            _previousGridTile = value;
            if (_previousGridTile == null) {
                Debug.Log($"Previous grid tile was set to null");
            }
        } 
    }
    public bool isMainVisualActive => mainImg.gameObject.activeSelf;
    public CharacterMarkerAnimationListener animationListener => _animationListener;
    public int sortingOrder => mainImg.sortingOrder;
    private LocationGridTile _previousGridTile;
    public bool useCanTraverse;
    private float attackSpeedMeter { get; set; }
    private HexTile _previousHexTileLocation;
    private CharacterMarkerNameplate _nameplate;
    private LocationGridTile _destinationTile;

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

        AddListeners();
        PathfindingManager.Instance.AddAgent(pathfindingAI);
        // UpdateNameplatePosition();
    }

    #region Monobehavior
    private void OnDisable() {
        if (character != null && 
            InnerMapCameraMove.Instance.target == this.transform) {
            InnerMapCameraMove.Instance.CenterCameraOn(null);
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
            if (attackSpeedMeter < character.combatComponent.attackSpeed) {
                attackSpeedMeter += ((Time.deltaTime * 1000f) * progressionSpeedMultiplier);
                UpdateAttackSpeedMeter();
            }
            pathfindingAI.UpdateMe();
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

    #region Pointer Functions
    protected override void OnPointerLeftClick(Character poi) {
        base.OnPointerLeftClick(poi);
        UIManager.Instance.ShowCharacterInfo(character, true);
    }
    protected override void OnPointerRightClick(Character poi) {
        base.OnPointerRightClick(poi);
        Character activeCharacter = UIManager.Instance.characterInfoUI.activeCharacter;
        if (activeCharacter == null) {
            activeCharacter = UIManager.Instance.monsterInfoUI.activeMonster;
        }
        if (activeCharacter != null) {
            if (activeCharacter.minion == null) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UIManager.Instance.poiTestingUI.ShowUI(character, activeCharacter);
#endif
            } else {
                UIManager.Instance.minionCommandsUI.ShowUI(character);
            }
        }
    }
    protected override void OnPointerEnter(Character character) {
        base.OnPointerEnter(character);
        InnerMapManager.Instance.SetCurrentlyHoveredPOI(character);
        InnerMapManager.Instance.ShowTileData(this.character.gridTileLocation, this.character);
        if (UIManager.Instance.GetCurrentlySelectedCharacter() != character) {
            //only process hover tooltips if character is not the currently selected character
            ShowThoughts();    
        }
    }
    protected override void OnPointerExit(Character poi) {
        base.OnPointerExit(poi);
        if (InnerMapManager.Instance.currentlyHoveredPoi == poi) {
            InnerMapManager.Instance.SetCurrentlyHoveredPOI(null);
        }
        UIManager.Instance.HideSmallInfo();
        if (UIManager.Instance.GetCurrentlySelectedCharacter() != character) {
            //only process hover tooltips if character is not the currently selected character
            HideThoughts();
        }
    }
    #endregion

    #region Listeners
    private void AddListeners() {
        Messenger.AddListener<PROGRESSION_SPEED>(Signals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);
        Messenger.AddListener<Character, Trait>(Signals.CHARACTER_TRAIT_ADDED, OnCharacterGainedTrait);
        Messenger.AddListener<Character, Trait>(Signals.CHARACTER_TRAIT_REMOVED, OnCharacterLostTrait);
        Messenger.AddListener<Character>(Signals.CHARACTER_STARTED_TRAVELLING_OUTSIDE, OnCharacterAreaTravelling);
        Messenger.AddListener(Signals.TICK_ENDED, ProcessAllUnprocessedVisionPOIs);
        Messenger.AddListener<TileObject, Character, LocationGridTile>(Signals.TILE_OBJECT_REMOVED,
            OnTileObjectRemovedFromTile);
        Messenger.AddListener<IPointOfInterest>(Signals.REPROCESS_POI, ReprocessPOI);
        Messenger.AddListener(Signals.TICK_ENDED, PerTickMovement);
    }
    private void RemoveListeners() {
        Messenger.RemoveListener<PROGRESSION_SPEED>(Signals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);
        Messenger.RemoveListener<Character, Trait>(Signals.CHARACTER_TRAIT_ADDED, OnCharacterGainedTrait);
        Messenger.RemoveListener<Character, Trait>(Signals.CHARACTER_TRAIT_REMOVED, OnCharacterLostTrait);
        Messenger.RemoveListener<Character>(Signals.CHARACTER_STARTED_TRAVELLING_OUTSIDE, OnCharacterAreaTravelling);
        Messenger.RemoveListener(Signals.TICK_ENDED, ProcessAllUnprocessedVisionPOIs);
        Messenger.RemoveListener<TileObject, Character, LocationGridTile>(Signals.TILE_OBJECT_REMOVED, OnTileObjectRemovedFromTile);
        Messenger.RemoveListener<IPointOfInterest>(Signals.REPROCESS_POI, ReprocessPOI);
        Messenger.RemoveListener(Signals.TICK_ENDED, PerTickMovement);
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
            string lostTraitSummary =
                $"{character.name} has <color=red>lost</color> trait <b>{trait.name}</b>";
            character.logComponent.PrintLogIfActive(lostTraitSummary);
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
            if (travellingCharacter.carryComponent.IsPOICarried(targetPOI)) {
                //If the travelling party is travelling outside and is carrying a poi that is being targetted by this marker, this marker should fail to compute path
                Action action = failedToComputePathAction;
                if (action != null) {
                    if (character.carryComponent.masterCharacter.avatar.isTravellingOutside) {
                        character.carryComponent.masterCharacter.avatar.SetOnArriveAction(() => character.OnArriveAtAreaStopMovement());
                    } else {
                        StopMovement();
                    }
                }
                //set arrival action to null, because some arrival actions set it when executed
                failedToComputePathAction = null;
                action?.Invoke();
            }
            if(travellingCharacter.carryComponent.carriedPOI is Character carriedCharacter) {
                character.combatComponent.RemoveHostileInRange(carriedCharacter); //removed hostile because he/she left the npcSettlement.
                character.combatComponent.RemoveAvoidInRange(carriedCharacter);
                RemovePOIFromInVisionRange(carriedCharacter);
                RemovePOIAsInRangeButDifferentStructure(carriedCharacter);
            }
        }
    }
    private void SelfGainedTrait(Character characterThatGainedTrait, Trait trait) {
        string gainTraitSummary =
            $"{GameManager.Instance.TodayLogString()}{characterThatGainedTrait.name} has <color=green>gained</color> trait <b>{trait.name}</b>";
       
        if (!characterThatGainedTrait.canPerform) {
            if (character.combatComponent.isInCombat) {
                characterThatGainedTrait.stateComponent.ExitCurrentState();
                gainTraitSummary += "\nGained trait hinders performance, and characters current state is combat, exiting combat state.";
            }

            //Once a character has a negative disabler trait, clear hostile and avoid list
            character.combatComponent.ClearHostilesInRange(false);
            character.combatComponent.ClearAvoidInRange(false);
        }
        if (trait is Cultist) {
            UpdateName();
        }
        UpdateAnimation();
        UpdateActionIcon();
        character.logComponent.PrintLogIfActive(gainTraitSummary);
    }
    private void OtherCharacterGainedTrait(Character otherCharacter, Trait trait) {
        if (trait.name == "Invisible") {
            character.combatComponent.RemoveHostileInRange(otherCharacter);
            character.combatComponent.RemoveAvoidInRange(otherCharacter);
            RemovePOIFromInVisionRange(otherCharacter);
        } else {
            if (inVisionCharacters.Contains(otherCharacter)) {
                character.CreateJobsOnTargetGainTrait(otherCharacter, trait);
            }

            //Only remove hostile in range from non lethal combat if target specifically becomes: Unconscious, Zapped or Restrained.
            //if (!otherCharacter.canPerform) {
            if (character.combatComponent.IsLethalCombatForTarget(otherCharacter) == false) {
                if (otherCharacter.traitContainer.HasTrait("Unconscious", "Paralyzed", "Restrained")) {
                    if (character.combatComponent.hostilesInRange.Contains(otherCharacter)) {
                        character.combatComponent.RemoveHostileInRange(otherCharacter);
                    }
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
    public void SetNameState(bool state) {
        _nameplate.SetNameState(state);
    }
    private void CreateNameplate() {
        GameObject nameplateGO = ObjectPoolManager.Instance.InstantiateObjectFromPool("CharacterMarkerNameplate", transform.position,
            Quaternion.identity, UIManager.Instance.characterMarkerNameplateParent);
        _nameplate = nameplateGO.GetComponent<CharacterMarkerNameplate>();
        _nameplate.Initialize(this);
    }
    #endregion

    #region Action Icon
    public void UpdateActionIcon() {
        if (_nameplate) {
            _nameplate.UpdateActionIcon();    
        }
    }
    #endregion

    #region Object Pool
    public override void Reset() {
        base.Reset(); 
        TryCancelExpiry();
        destinationTile = null;
        //onProcessCombat = null;
        character.combatComponent.SetOnProcessCombatAction(null);
        SetMarkerColor(Color.white);
        ObjectPoolManager.Instance.DestroyObject(_nameplate);
        _nameplate = null;
        PathfindingManager.Instance.RemoveAgent(pathfindingAI);
        RemoveListeners();
        HideHPBar();
        
        Messenger.Broadcast(Signals.CHARACTER_EXITED_HEXTILE, character, _previousHexTileLocation);
        
        visionCollider.Reset();
        GameObject.Destroy(visionTrigger.gameObject);
        visionTrigger = null;
        SetCollidersState(false);
        pathfindingAI.ResetThis();
        character = null;
        // previousGridTile = null;
        _previousHexTileLocation = null;
    }
    #endregion

    #region Pathfinding Movement
    public void GoTo(LocationGridTile destinationTile, Action arrivalAction = null, Action failedToComputePathAction = null, STRUCTURE_TYPE[] notAllowedStructures = null) {
        if (character.movementComponent.isStationary) {
            return;
        }
        //If any time a character goes to a structure outside the trap structure, the trap structure data will be cleared out
        if (character.trapStructure.IsTrappedAndTrapStructureIsNot(destinationTile.structure)) {
            character.trapStructure.SetStructureAndDuration(null, 0);
        }
        pathfindingAI.ClearAllCurrentPathData();
        pathfindingAI.SetNotAllowedStructures(notAllowedStructures);
        this.destinationTile = destinationTile;
        this.arrivalAction = arrivalAction;
        this.failedToComputePathAction = failedToComputePathAction;
        this.targetPOI = null;
        if (destinationTile == character.gridTileLocation) {
            //if (this.arrivalAction != null) {
            //    Debug.Log(character.name + " is already at " + destinationTile.ToString() + " executing action " + this.arrivalAction.Method.Name);
            //} else {
            //    Debug.Log(character.name + " is already at " + destinationTile.ToString() + " executing action null.");
            //}
            Action action = this.arrivalAction;
            ClearArrivalAction();
            action?.Invoke();
        } else {
            SetDestination(destinationTile.centeredWorldLocation, destinationTile);
            StartMovement();
        }
    }
    public void GoToPOI(IPointOfInterest targetPOI, Action arrivalAction = null, Action failedToComputePathAction = null, STRUCTURE_TYPE[] notAllowedStructures = null) {
        if (character.movementComponent.isStationary) {
            return;
        }
        pathfindingAI.ClearAllCurrentPathData();
        pathfindingAI.SetNotAllowedStructures(notAllowedStructures);
        this.arrivalAction = arrivalAction;
        this.failedToComputePathAction = failedToComputePathAction;
        this.targetPOI = targetPOI;
        switch (targetPOI.poiType) {
            case POINT_OF_INTEREST_TYPE.CHARACTER:
                Character targetCharacter = targetPOI as Character;
                if (!targetCharacter.marker) {
                    this.failedToComputePathAction?.Invoke(); //target character is already dead.
                    this.failedToComputePathAction = null;
                    return;
                }
                SetTargetTransform(targetCharacter.marker.transform);
                if (targetCharacter.carryComponent.masterCharacter.avatar && targetCharacter.carryComponent.masterCharacter.avatar.isTravellingOutside) {
                    OnCharacterAreaTravelling(targetCharacter);
                } 
                break;
            default:
                if(targetPOI is MovingTileObject) {
                    SetTargetTransform(targetPOI.mapObjectVisual.transform);
                } else {
                    if (targetPOI.gridTileLocation == null) {
                        throw new Exception($"{character.name} is trying to go to a {targetPOI.ToString()} but its tile location is null");
                    }
                    SetDestination(targetPOI.gridTileLocation.centeredWorldLocation, targetPOI.gridTileLocation);
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
    //public void GoTo(Vector3 destination, Action arrivalAction = null, STRUCTURE_TYPE[] notAllowedStructures = null) {
    //    pathfindingAI.ClearAllCurrentPathData();
    //    pathfindingAI.SetNotAllowedStructures(notAllowedStructures);
    //    this.destinationTile = destinationTile;
    //    this.arrivalAction = arrivalAction;
    //    SetTargetTransform(null);
    //    SetDestination(destination);
    //    StartMovement();
    //}
    public void ArrivedAtTarget() {
        if (character.combatComponent.isInCombat) {
            CombatState combatState = character.stateComponent.currentState as CombatState;
            if (combatState.isAttacking){
                if(combatState.currentClosestHostile != null && !character.movementComponent.HasPathToEvenIfDiffRegion(combatState.currentClosestHostile.gridTileLocation)) {
                    character.combatComponent.RemoveHostileInRange(combatState.currentClosestHostile);
                }
                return;
            }
        }
        StopMovement();

        if (character.movementComponent.enableDigging) {
            LocationGridTile destinationTile = null;
            if (targetPOI != null) {
                destinationTile = targetPOI.gridTileLocation;
            } else if (this.destinationTile != null) {
                destinationTile = this.destinationTile;
            }
            Vector3 lastPositionInPath = pathfindingAI.currentPath.vectorPath.Last();
            LocationGridTile attainedDestinationTile = character.currentRegion.innerMap.GetTile(lastPositionInPath);

            if (character.gridTileLocation != null && destinationTile != null && destinationTile != attainedDestinationTile) {
                //When path is completed and the distance between the actor and the target is still more than 1 tile, we need to assume the the path is blocked
                if (character.movementComponent.DigOnReachEndPath(pathfindingAI.currentPath)) {
                    targetPOI = null;
                    return;
                }
            }
        }

        Action action = arrivalAction;
        //set arrival action to null, because some arrival actions set it
        ClearArrivalAction();
        action?.Invoke();

        targetPOI = null;
    }
    private void StartMovement() {
        if (character.movementComponent.isStationary) {
            return;
        }
        isMoving = true;
        character.movementComponent.UpdateSpeed();
        pathfindingAI.SetIsStopMovement(false);
        character.carryComponent.masterCharacter.avatar.SetIsTravelling(true);
        UpdateAnimation();
        // Messenger.AddListener(Signals.TICK_ENDED, PerTickMovement);
    }
    public void StopMovement() {
        isMoving = false;
        string log = $"{character.name} StopMovement function is called!";
        character.logComponent.PrintLogIfActive(log);
        if (ReferenceEquals(character.carryComponent.masterCharacter.avatar, null) == false) {
            character.carryComponent.masterCharacter.avatar.SetIsTravelling(false);
        }
        pathfindingAI.SetIsStopMovement(true);
        UpdateAnimation();
        // Messenger.RemoveListener(Signals.TICK_ENDED, PerTickMovement);
    }
    private void PerTickMovement() {
        // if (character == null) {
        //     Messenger.RemoveListener(Signals.TICK_ENDED, PerTickMovement);
        //     return;
        // }
        if (isMoving) {
            Profiler.BeginSample($"{character.name} PerTickMovement");
            character.PerTickDuringMovement();    
            Profiler.EndSample();
        }
    }
    /// <summary>
    /// Make this marker look at a specific point (In World Space).
    /// </summary>
    /// <param name="target">The target point in world space</param>
    /// <param name="force">Should this object be forced to rotate?</param>
    public override void LookAt(Vector3 target, bool force = false) {
        if (!force) {
            if (!character.canPerform || !character.canMove) { //character.traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE)
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
            if (!character.canPerform || !character.canMove) { //character.traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE)
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
    public void SetTargetTransform(Transform target) {
        destinationSetter.target = target;
        pathfindingAI.canSearch = true;
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
        //if (isInCombatTick) {
        //    return;
        //}
        if (gameObject.activeSelf == false) { return; }
        if (!character.carryComponent.IsNotBeingCarried()) {
            PlaySleepGround();
            ResetBlood();
            //if (character.traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE)) {
            //    PlaySleepGround();
            //}
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
                if ((character.canMove == false || (!character.canPerform && !character.canWitness)) && !character.traitContainer.HasTrait("Hibernating")) {
                    PlaySleepGround();
                } else {
                    PlayIdle();
                }
            } else if ((character.canMove == false || (!character.canPerform && !character.canWitness)) && !character.traitContainer.HasTrait("Hibernating")) {
                PlaySleepGround();
            } else if (ReferenceEquals(character.carryComponent.masterCharacter.avatar, null) == false && character.carryComponent.masterCharacter.avatar.isTravelling) {
                //|| character.stateComponent.currentState.characterState == CHARACTER_STATE.STROLL
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
        yield return new WaitForSeconds(5f);
        bloodSplatterEffect.Pause();
    }
    private void ResetBlood() {
        if (bloodSplatterEffect.gameObject.activeSelf) {
            bloodSplatterEffect.gameObject.SetActive(false);
            bloodSplatterEffect.Clear();    
        }
    }
    public void PauseAnimation() {
        animator.speed = 0;
    }
    public void UnpauseAnimation() {
        animator.speed = 1;
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
        animator.speed = 1f * progressionSpeedMultiplier;
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
        hpBarGO.GetComponent<Canvas>().sortingOrder = characterSortingOrder;
    }
    private new void SetActiveState(bool state) {
        Debug.Log($"Set active state of {this.name} to {state.ToString()}");
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
        // clickedImg.enabled = state;
    }
    public bool IsShowingVisuals() {
        return mainImg.gameObject.activeSelf;
    }
    private void UpdateHairVisuals() {
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
    public void UpdateMarkerVisuals() {
        UpdateHairVisuals();
    }
    public void UpdatePosition() {
        //This is checked per update, stress test this for performance
        //I'm keeping a separate field called anchoredPos instead of using the rect transform anchoredPosition directly because the multithread cannot access transform components
        anchoredPos = transform.localPosition;

        if (previousGridTile != character.gridTileLocation) {
            character.gridTileLocation.parentMap.region.innerMap.OnCharacterMovedTo(character, character.gridTileLocation, previousGridTile);
            if(character != null) {
                previousGridTile = character.gridTileLocation;
                if (_previousHexTileLocation == null || (character.gridTileLocation.collectionOwner.isPartOfParentRegionMap &&
                    _previousHexTileLocation != character.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner)) {
                    if (_previousHexTileLocation != null) {
                        Messenger.Broadcast(Signals.CHARACTER_EXITED_HEXTILE, character, _previousHexTileLocation);
                    }
                    if (character.gridTileLocation.collectionOwner.isPartOfParentRegionMap) {
                        _previousHexTileLocation = character.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner;
                        Messenger.Broadcast(Signals.CHARACTER_ENTERED_HEXTILE, character, character.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner);
                    } else {
                        _previousHexTileLocation = null;
                    }
                }
            }
        }
    }
    /// <summary>
    /// Used for placing a character for the first time.
    /// </summary>
    /// <param name="tile">The tile the character should be placed at.</param>
    /// <param name="addToLocation">If the character should be added to the location or not?</param>
    public void InitialPlaceMarkerAt(LocationGridTile tile, bool addToLocation = true) {
        PlaceMarkerAt(tile, addToLocation);
        pathfindingAI.UpdateMe();
        SetCollidersState(true);
        visionCollider.Initialize();
        character.movementComponent.UpdateSpeed();
        _nameplate.UpdateActiveState();
    }
    public void PlaceMarkerAt(LocationGridTile tile, bool addToLocation = true) {
        this.gameObject.transform.SetParent(tile.parentMap.objectsParent);
        if (addToLocation) {
            tile.structure.location.AddCharacterToLocation(character);
        }
        SetActiveState(true);
        UpdateAnimation();
        pathfindingAI.Teleport(tile.centeredWorldLocation);
        UpdatePosition();
        if (addToLocation) {
            tile.structure.AddCharacterAtLocation(character, tile);
        }
        UpdateActionIcon();
        SetCollidersState(true);
        tile.parentMap.region.AddPendingAwareness(character);
    }
    public void PlaceMarkerAt(Vector3 worldPosition, Region region, bool addToLocation = true) {
        Vector3 localPos = region.innerMap.grid.WorldToLocal(worldPosition);
        Vector3Int coordinate = region.innerMap.grid.LocalToCell(localPos);
        LocationGridTile tile = region.innerMap.map[coordinate.x, coordinate.y];
        
        this.gameObject.transform.SetParent(tile.parentMap.objectsParent);
        pathfindingAI.Teleport(worldPosition);
        if (addToLocation) {
            tile.structure.location.AddCharacterToLocation(character);
            tile.structure.AddCharacterAtLocation(character, tile);
        }
        SetActiveState(true);
        UpdateAnimation();
        UpdatePosition();
        UpdateActionIcon();
        SetCollidersState(true);
        tile.parentMap.region.AddPendingAwareness(character);
    }
    public void OnDeath(LocationGridTile deathTileLocation) {
        if (character.minion != null || character.destroyMarkerOnDeath) {
            character.DestroyMarker();
        } else {
            ScheduleExpiry();
            SetCollidersState(false);
            //onProcessCombat = null;
            character.combatComponent.SetOnProcessCombatAction(null);
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
        gameObject.SetActive(true);
        SetCollidersState(true);
        UpdateAnimation();
        UpdateActionIcon();
    }
    public void SetTargetPOI(IPointOfInterest poi) {
        this.targetPOI = poi;
    }
    private bool CanDoStealthActionToTarget(Character target) {
        if (!target.isDead) {
            int inVisionNormalCharacterCount = 0;
            for (int i = 0; i < target.marker.inVisionCharacters.Count; i++) {
                Character inVision = target.marker.inVisionCharacters[i];
                if (inVision.isNormalCharacter) {
                    inVisionNormalCharacterCount++;
                    if (inVisionNormalCharacterCount > 1) {
                        return false; //if there are 2 or more in vision of target character it means he is not alone anymore
                    }
                }
            }
            if (inVisionNormalCharacterCount == 1) {
                if (!target.marker.inVisionCharacters.Contains(character)) {
                    return false; //if there is only one in vision of target character and it is not this character, it means he is not alone
                }
            }
        } else {
            int inVisionNormalCharacterCount = 0;
            for (int i = 0; i < inVisionCharacters.Count; i++) {
                Character inVision = inVisionCharacters[i];
                if (inVision.isNormalCharacter) {
                    inVisionNormalCharacterCount++;
                    if (inVisionNormalCharacterCount > 1) {
                        return false;
                    }
                }
            }
        }
        return true;
        //if (!target.isDead) {
        //    if (target.marker.inVisionCharacters.Count > 1) {
        //        return false; //if there are 2 or more in vision of target character it means he is not alone anymore
        //    } else if (target.marker.inVisionCharacters.Count == 1) {
        //        if (!target.marker.inVisionCharacters.Contains(character)) {
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
    public bool CanDoStealthActionToTarget(IPointOfInterest target) {
        if(target is Character) {
            return CanDoStealthActionToTarget(target as Character);
        }
        for (int i = 0; i < inVisionCharacters.Count; i++) {
            Character inVision = inVisionCharacters[i];
            if (inVision.isNormalCharacter) {
                return false;
            }
        }
        //if (inVisionCharacters.Count > 0) {
        //    return false;
        //}
        return true;
    }
    public void SetMarkerColor(Color color) {
        mainImg.color = color;
    }
    private void UpdateHairState() {
        //TODO: Find another way to unify this
        if (character.characterClass.className == "Mage" || character.characterClass.className == "Necromancer" || character.visuals.portraitSettings.hair == -1 || 
            character.race == RACE.WOLF || character.race == RACE.SKELETON || 
            character.race == RACE.GOLEM || character.race == RACE.ELEMENTAL || character.race == RACE.KOBOLD ||
            character.race == RACE.SPIDER || character.race == RACE.MIMIC || character.race == RACE.ENT || 
            character.race == RACE.PIG || character.race == RACE.CHICKEN || character.race == RACE.SHEEP 
            || character.race == RACE.ABOMINATION) {
            hairImg.gameObject.SetActive(false);
            knockedOutHairImg.gameObject.SetActive(false);
        } else {
            if (currentAnimation == "Sleep Ground") {
                knockedOutHairImg.gameObject.SetActive(true);
                hairImg.gameObject.SetActive(false);
            } else {
                knockedOutHairImg.gameObject.SetActive(false);
                hairImg.gameObject.SetActive(true);
            }
            
        }
    }
    public List<Character> GetInVisionCharactersThatMeetCriteria(System.Func<Character, bool> criteria) {
        List<Character> characters = null;
        for (int i = 0; i < inVisionCharacters.Count; i++) {
            Character c = inVisionCharacters[i];
            if (criteria.Invoke(c)) {
                if (characters == null) {
                    characters = new List<Character>();
                }
                characters.Add(c);
            }
        }
        return characters;
    }
    public bool IsStillInRange(IPointOfInterest poi) {
        //I added checking for poisInRangeButDiffStructure beacuse characters are being removed from the character's avoid range when they exit a structure. (Myk)
        return inVisionPOIs.Contains(poi) || inVisionPOIsButDiffStructure.Contains(poi);
    }
    #endregion

    #region Vision Collision
    private void CreateCollisionTrigger() {
        GameObject collisionTriggerGO = GameObject.Instantiate(InnerMapManager.Instance.characterCollisionTriggerPrefab, this.transform);
        collisionTriggerGO.transform.localPosition = Vector3.zero;
        visionTrigger = collisionTriggerGO.GetComponent<CharacterVisionTrigger>();
        visionTrigger.Initialize(character);
    }
    public void AddPOIAsInVisionRange(IPointOfInterest poi) {
        if (!inVisionPOIs.Contains(poi)) {
            inVisionPOIs.Add(poi);
            AddUnprocessedPOI(poi);
            if (poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
                inVisionCharacters.Add(poi as Character);
            } else if (poi.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
                inVisionTileObjects.Add(poi as TileObject);
            }
            //character.AddAwareness(poi);
            OnAddPOIAsInVisionRange(poi);
            Messenger.Broadcast(Signals.CHARACTER_SAW, character, poi);
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
                Messenger.Broadcast(Signals.CHARACTER_REMOVED_FROM_VISION, character, target);
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
    public void ClearPOIsInVisionRange() {
        inVisionPOIs.Clear();
        inVisionCharacters.Clear();
        inVisionTileObjects.Clear();
        ClearUnprocessedPOI();
        ClearUnprocessedActions();
    }
    public void LogPOIsInVisionRange() {
        string summary = $"{character.name}'s POIs in range: ";
        for (int i = 0; i < inVisionPOIs.Count; i++) {
            summary += $"\n- {inVisionPOIs[i]}";
        }
        Debug.Log(summary);
    }
    private void OnAddPOIAsInVisionRange(IPointOfInterest poi) {
        if (character.currentActionNode != null && character.currentActionNode.target == poi && character.currentActionNode.action.IsInvalidOnVision(character.currentActionNode)) {
            character.currentActionNode.associatedJob?.CancelJob(false);
        }
        if (character.currentActionNode != null && character.currentActionNode.action.actionLocationType == ACTION_LOCATION_TYPE.TARGET_IN_VISION && character.currentActionNode.poiTarget == poi) {
            StopMovement();
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
        }
    }
    public void RemoveUnprocessedAction(ActualGoapNode action) {
        unprocessedActionsOnly.Remove(action);
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
        unprocessedActionsOnly.Clear();
    }
    public bool HasUnprocessedPOI(IPointOfInterest poi) {
        return unprocessedVisionPOIs.Contains(poi);
    }
    private void ReprocessPOI(IPointOfInterest poi) {
        if (HasUnprocessedPOI(poi) == false && inVisionPOIs.Contains(poi)) {
            AddUnprocessedPOI(poi);
        }
    }
    private void ProcessAllUnprocessedVisionPOIs() {
        if (character == null) { return; }
        Profiler.BeginSample($"{character.name} ProcessAllUnprocessedVisionPOIs");
        string log = $"{character.name} tick ended! Processing all unprocessed in visions...";
        if (unprocessedVisionPOIs.Count > 0) {
            if (!character.isDead/* && character.canWitness*/) { //character.traitContainer.GetNormalTrait<Trait>("Unconscious", "Resting", "Zapped") == null
                for (int i = 0; i < unprocessedVisionPOIs.Count; i++) {
                    IPointOfInterest poi = unprocessedVisionPOIs[i];
                    if (poi.mapObjectVisual == null) {
                        log += $"\n-{poi.nameWithID}'s map visual has been destroyed. Skipping...";
                        continue;
                    }
                    if (poi.isHidden) {
                        log += $"\n-{poi.nameWithID} is hidden. Skipping...";
                        continue;
                    }
                    log += $"\n-{poi.nameWithID}";
                    bool reactToActionOnly = false;
                    if (unprocessedVisionPOIInterruptsOnly.Count > 0) {
                        reactToActionOnly = unprocessedVisionPOIInterruptsOnly.Contains(poi);
                    }
                    character.ThisCharacterSaw(poi, reactToActionOnly);
                }
            } else {
                log += "\n - Character is either dead, not processing...";
            }
            ClearUnprocessedPOI();
        }
        if (unprocessedActionsOnly.Count > 0) {
            if (!character.isDead) {
                for (int i = 0; i < unprocessedActionsOnly.Count; i++) {
                    ActualGoapNode action = unprocessedActionsOnly[i];
                    log += $"\n-{action.goapName}";
                    character.ThisCharacterSawAction(action);
                }
            } else {
                log += "\n - Character is either dead, not processing...";
            }
            ClearUnprocessedActions();
        }
        character.logComponent.PrintLogIfActive(log);
        character.SetHasSeenFire(false);
        character.SetHasSeenWet(false);
        character.SetHasSeenPoisoned(false);
        character.combatComponent.CheckCombatPerTickEnded();
        Profiler.EndSample();
    }
    #endregion

    #region Hosility Collision
    //public bool AddHostileInRange(IPointOfInterest poi, bool checkHostility = true, bool processCombatBehavior = true, bool isLethal = true, bool gotHit = false) {
    //    if (!hostilesInRange.Contains(poi)) {
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
    //            //otherwise ignore canBeAttacked value
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
        //if (inVisionCharacters.Contains(otherCharacter)) {
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
    public void OnSeizeOtherCharacter(Character otherCharacter) {
        character.combatComponent.RemoveHostileInRange(otherCharacter);
        character.combatComponent.RemoveAvoidInRange(otherCharacter);
    }
    public void OnBeforeSeizingOtherCharacter(Character otherCharacter) {
        //if (character.faction != null && character.faction.isMajorNonPlayerFriendlyNeutral) {
        //    if (inVisionCharacters.Contains(otherCharacter)) {
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
    //            if (!avoidInRange.Contains(poi)) {
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
    //        if (!avoidInRange.Contains(poi)) {
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
    //        if (!avoidInRange.Contains(poi)) {
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
    //            if (!avoidInRange.Contains(poi)) {
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
    public bool hasFleePath { get; private set; }
    public void OnStartFlee() {
        if (character.combatComponent.avoidInRange.Count == 0) {
            return;
        }
        pathfindingAI.ClearAllCurrentPathData();
        SetHasFleePath(true);
        pathfindingAI.canSearch = false; //set to false, because if this is true and a destination has been set in the ai path, the ai will still try and go to that point instead of the computed flee path

        Vector3[] avoidThisPositions = new Vector3[character.combatComponent.avoidInRange.Count + PlayerManager.Instance.player.playerSettlement.allStructures.Count];
        int lastIndex = 0;
        if (character.combatComponent.avoidInRange.Count > 0) {
            for (int i = 0; i < character.combatComponent.avoidInRange.Count; i++) {
                avoidThisPositions[i] = character.combatComponent.avoidInRange[i].gridTileLocation.worldLocation;
            }
            lastIndex = character.combatComponent.avoidInRange.Count;
        }

        //Corrupted hexes should also be avoided
        //https://trello.com/c/6WJtivlY/1274-fleeing-should-not-go-to-corrupted-structures
        List<HexTile> playerHexes = PlayerManager.Instance.player.playerSettlement.tiles;
        if(playerHexes.Count > 0) {
            for (int i = 0; i < playerHexes.Count; i++) {
                avoidThisPositions[i + lastIndex] = playerHexes[i].GetCenterLocationGridTile().worldLocation;
            }
        }

        FleeMultiplePath fleePath = FleeMultiplePath.Construct(this.transform.position, character.combatComponent.avoidInRange.Select(x => x.gridTileLocation.worldLocation).ToArray(), 20000);
        fleePath.aimStrength = 1;
        fleePath.spread = 4000;
        seeker.StartPath(fleePath);
    }
    public void OnStartFleeToHome() {
        if (character.combatComponent.avoidInRange.Count == 0) {
            return;
        }
        pathfindingAI.ClearAllCurrentPathData();
        SetHasFleePath(true);
        LocationGridTile chosenTile = null;
        if(character.homeStructure != null) {
            chosenTile = character.homeStructure.GetRandomTile();
        } else if (character.HasTerritory()) {
            chosenTile = character.GetRandomLocationGridTileWithPath();
        }
        if (chosenTile != null) {
            if (character.currentRegion != chosenTile.structure.location) {
                if (character.carryComponent.masterCharacter.movementComponent.GoToLocation(destinationTile.structure.location, PATHFINDING_MODE.NORMAL, doneAction: () => GoTo(chosenTile, OnFinishedTraversingFleePath)) == false) {
                    OnStartFlee();
                }
            } else {
                GoTo(chosenTile, OnFinishedTraversingFleePath);
            }
        } else {
            OnStartFlee();
        }
    }
    public void OnFleePathComputed(Path path) {
        if (character == null || !character.canPerform || !character.canMove) {
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
    private readonly RaycastHit2D[] linOfSightHitObjects = new RaycastHit2D[5];
    public bool IsCharacterInLineOfSightWith(IPointOfInterest target, float rayDistance = 5f) {
        Profiler.BeginSample($"{character.name} IsCharacterInLineOfSightWith Pre Check");
        if (inVisionPOIs.Contains(target) == false) { return false; }
        Profiler.EndSample();
        
        Profiler.BeginSample($"{character.name} start set");
        //precompute our ray settings
        Vector3 start = transform.position;
        Profiler.EndSample();
        
        Profiler.BeginSample($"{character.name} Vector3 subtraction");
        Vector3 direction = GameUtilities.VectorSubtraction(target.worldPosition, start).normalized;
        Profiler.EndSample();
        
        Profiler.BeginSample($"{character.name} Raycast");
        float distance = rayDistance;
        if (target is BlockWall) {
            distance += 1.5f;
        }
        //do the ray test
        var size = Physics2D.RaycastNonAlloc(start, direction, linOfSightHitObjects, distance, 
            GameUtilities.Line_Of_Sight_Layer_Mask);
        Profiler.EndSample();
        
        Profiler.BeginSample($"{character.name} Raycast result loop");
        for (int i = 0; i < size; i++) {
            RaycastHit2D hit = linOfSightHitObjects[i];
            if((target is BlockWall) == false && hit.collider.gameObject.layer == LayerMask.NameToLayer("Unpassable")) {
                return false;
            } else if (hit.transform.IsChildOf(target.mapObjectVisual.transform)) {
                return true;
            }
            
        }
        Profiler.EndSample();
        return false;
    }
    #endregion

    #region Colliders
    public void SetCollidersState(bool state) {
        for (int i = 0; i < colliders.Length; i++) {
            colliders[i].enabled = state;
        }
    }
    public void SetAllColliderStates(bool state) {
        SetCollidersState(state);
        visionTrigger.SetAllCollidersState(state);
    }
    #endregion

    #region Map Object Visual
    public override void UpdateTileObjectVisual(Character obj) { }
    public override void ApplyFurnitureSettings(FurnitureSetting furnitureSetting) { }
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
        Reset();
        character = _character;
        buttonCollider.enabled = false;
    }
    public void OnUnseize() {
        buttonCollider.enabled = true;
        if (character.isDead) {
            ScheduleExpiry();
        }
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
            GameDate date = GameManager.Instance.Today();
            date.AddDays(3);
            _destroySchedule = SchedulingManager.Instance.AddEntry(date, Expire, character);    
        }
    }
    private void Expire() {
        character.DestroyMarker();
    }
    #endregion

    #region Nameplate
    public void ShowThoughts() {
        _nameplate.ShowThoughts();
    }
    public void HideThoughts() {
        _nameplate.HideThoughts();
    }
    public void UpdateNameplateElementsState() {
        _nameplate.UpdateElementsStateBasedOnActiveCharacter();
    }
    #endregion

    
}
