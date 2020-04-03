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
using Traits;
using UnityEngine.Profiling;
using UnityEngine.Serialization;
using UtilityScripts;

public class CharacterMarker : MapObjectVisual<Character> {
    public Character character { get; private set; }

    public Transform visualsParent;
    public TextMeshPro nameLbl;
    [SerializeField] private SpriteRenderer mainImg;
    [SerializeField] private SpriteRenderer hairImg;
    [SerializeField] private SpriteRenderer knockedOutHairImg;
    [SerializeField] private SpriteRenderer hoveredImg;
    [SerializeField] private SpriteRenderer clickedImg;
    [SerializeField] private SpriteRenderer actionIcon;
    [SerializeField] private BoxCollider2D buttonCollider;
    [SerializeField] private ParticleSystem bloodSplatterEffect;
    [SerializeField] private ParticleSystemRenderer bloodSplatterEffectRenderer;

    [Header("Animation")]
    public Animator animator;
    [SerializeField] private CharacterMarkerAnimationListener animationListener;
    [SerializeField] private string currentAnimation;

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


    //vision colliders
    public List<IPointOfInterest> inVisionPOIs { get; private set; } //POI's in this characters vision collider
    public List<IPointOfInterest> unprocessedVisionPOIs { get; private set; } //POI's in this characters vision collider
    public List<IPointOfInterest> unprocessedVisionPOIsForActionOnly { get; private set; } //POI's in this characters vision collider
    public List<Character> inVisionCharacters { get; private set; } //POI's in this characters vision collider
    public List<TileObject> inVisionTileObjects { get; private set; } //POI's in this characters vision collider
    //public List<IPointOfInterest> hostilesInRange { get; private set; } //POI's in this characters hostility collider
    //public List<IPointOfInterest> avoidInRange { get; private set; } //POI's in this characters hostility collider
    //// public List<ActualGoapNode> alreadyWitnessedActions { get; private set; } //List of actions this character can witness, and has not been processed yet. Will be cleared after processing
    //public Dictionary<Character, bool> lethalCharacters { get; private set; }
    //public string avoidReason { get; private set; }
    //public bool willProcessCombat { get; private set; }
    public Action arrivalAction { get; private set; }
    public Action failedToComputePathAction { get; private set; }

    //movement
    public IPointOfInterest targetPOI { get; private set; }
    //public CharacterCollisionTrigger collisionTrigger { get; private set; }
    public Vector2 anchoredPos { get; private set; }
    public Vector3 centeredWorldPos { get; private set; }
    public LocationGridTile destinationTile { get; private set; }
    public int useWalkSpeed { get; private set; }
    public int targettedByRemoveNegativeTraitActionsCounter { get; private set; }
    //public List<IPointOfInterest> terrifyingObjects { get; private set; } //list of objects that this character is terrified of and must avoid
    public bool isMoving { get; private set; }
    public LocationGridTile previousGridTile {
        get => _previousGridTile;
        set {
            _previousGridTile = value;
            if (_previousGridTile == null) {
                Debug.Log($"Previous grid tile was set to null");
            }
        } 
    }
    private LocationGridTile _previousGridTile;
    private float progressionSpeedMultiplier;
    public bool useCanTraverse;

    public float attackSpeedMeter { get; private set; }
    private AnimatorOverrideController animatorOverrideController; //this is the controller that is made per character
    private HexTile _previousHexTileLocation;
    
    public void SetCharacter(Character character) {
        base.Initialize(character);
        this.name = $"{character.name}'s Marker";
        nameLbl.SetText(character.name);
        this.character = character;
        UpdateSortingOrder();
        UpdateMarkerVisuals();
        UpdateActionIcon();
        ForceUpdateMarkerVisualsBasedOnAnimation();

        unprocessedVisionPOIs = new List<IPointOfInterest>();
        unprocessedVisionPOIsForActionOnly = new List<IPointOfInterest>();
        inVisionPOIs = new List<IPointOfInterest>();
        inVisionCharacters = new List<Character>();
        inVisionTileObjects = new List<TileObject>();
        //hostilesInRange = new List<IPointOfInterest>();
        //terrifyingObjects = new List<IPointOfInterest>();
        //avoidInRange = new List<IPointOfInterest>();
        //lethalCharacters = new Dictionary<Character, bool>();
        // alreadyWitnessedActions = new List<ActualGoapNode>();
        //avoidReason = string.Empty;
        attackSpeedMeter = 0f;
        OnProgressionSpeedChanged(GameManager.Instance.currProgressionSpeed);
        UpdateHairState();

        AddListeners();
        PathfindingManager.Instance.AddAgent(pathfindingAI);
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
            if (attackSpeedMeter < character.attackSpeed) {
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
        if (activeCharacter != null) {
            if (activeCharacter.minion == null) {
#if UNITY_EDITOR
                UIManager.Instance.poiTestingUI.ShowUI(character);
#endif
            } else {
                UIManager.Instance.minionCommandsUI.ShowUI(character);
            }
        }
    }
    protected override void OnPointerEnter(Character poi) {
        base.OnPointerEnter(poi);
        InnerMapManager.Instance.SetCurrentlyHoveredPOI(poi);
        InnerMapManager.Instance.ShowTileData(character.gridTileLocation, character);
    }
    protected override void OnPointerExit(Character poi) {
        base.OnPointerExit(poi);
        if (InnerMapManager.Instance.currentlyHoveredPoi == poi) {
            InnerMapManager.Instance.SetCurrentlyHoveredPOI(null);
        }
        UIManager.Instance.HideSmallInfo();
    }
    #endregion

    #region Listeners
    private void AddListeners() {
        Messenger.AddListener<InfoUIBase>(Signals.MENU_OPENED, OnMenuOpened);
        Messenger.AddListener<InfoUIBase>(Signals.MENU_CLOSED, OnMenuClosed);
        Messenger.AddListener<PROGRESSION_SPEED>(Signals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);
        Messenger.AddListener<Character, Trait>(Signals.CHARACTER_TRAIT_ADDED, OnCharacterGainedTrait);
        Messenger.AddListener<Character, Trait>(Signals.CHARACTER_TRAIT_REMOVED, OnCharacterLostTrait);
        //Messenger.AddListener<Character, string>(Signals.TRANSFER_ENGAGE_TO_FLEE_LIST, TransferEngageToFleeList);
        Messenger.AddListener<Party>(Signals.PARTY_STARTED_TRAVELLING, OnCharacterAreaTravelling);
        Messenger.AddListener(Signals.TICK_ENDED, ProcessAllUnprocessedVisionPOIs);
        // Messenger.AddListener<SpecialToken, LocationGridTile>(Signals.ITEM_REMOVED_FROM_TILE, OnItemRemovedFromTile);
        Messenger.AddListener<TileObject, Character, LocationGridTile>(Signals.TILE_OBJECT_REMOVED,
            OnTileObjectRemovedFromTile);
        Messenger.AddListener<IPointOfInterest>(Signals.REPROCESS_POI, ReprocessPOI);
        Messenger.AddListener(Signals.TICK_ENDED, PerTickMovement);
    }
    private void RemoveListeners() {
        Messenger.RemoveListener<InfoUIBase>(Signals.MENU_OPENED, OnMenuOpened);
        Messenger.RemoveListener<InfoUIBase>(Signals.MENU_CLOSED, OnMenuClosed);
        Messenger.RemoveListener<PROGRESSION_SPEED>(Signals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);
        Messenger.RemoveListener<Character, Trait>(Signals.CHARACTER_TRAIT_ADDED, OnCharacterGainedTrait);
        Messenger.RemoveListener<Character, Trait>(Signals.CHARACTER_TRAIT_REMOVED, OnCharacterLostTrait);
        Messenger.RemoveListener<Party>(Signals.PARTY_STARTED_TRAVELLING, OnCharacterAreaTravelling);
        //Messenger.RemoveListener<Character, string>(Signals.TRANSFER_ENGAGE_TO_FLEE_LIST, TransferEngageToFleeList);
        Messenger.RemoveListener(Signals.TICK_ENDED, ProcessAllUnprocessedVisionPOIs);
        // Messenger.RemoveListener<SpecialToken, LocationGridTile>(Signals.ITEM_REMOVED_FROM_TILE, OnItemRemovedFromTile);
        Messenger.RemoveListener<TileObject, Character, LocationGridTile>(Signals.TILE_OBJECT_REMOVED, OnTileObjectRemovedFromTile);
        Messenger.RemoveListener<IPointOfInterest>(Signals.REPROCESS_POI, ReprocessPOI);
        Messenger.RemoveListener(Signals.TICK_ENDED, PerTickMovement);
    }
    public void OnCharacterGainedTrait(Character characterThatGainedTrait, Trait trait) {
        //this will make this character flee when he/she gains an injured trait
        if (characterThatGainedTrait == this.character) {
            SelfGainedTrait(characterThatGainedTrait, trait);
        } else {
            OtherCharacterGainedTrait(characterThatGainedTrait, trait);
        }
        //if(trait.type == TRAIT_TYPE.DISABLER && terrifyingObjects.Count > 0) {
        //    RemoveTerrifyingObject(characterThatGainedTrait);
        //}
    }
    public void OnCharacterLostTrait(Character character, Trait trait) {
        if (character == this.character) {
            string lostTraitSummary =
                $"{character.name} has <color=red>lost</color> trait <b>{trait.name}</b>";
            //if the character does not have any other negative disabler trait
            //check for reactions.
            //switch (trait.name) {
            //    case "Unconscious":
            //    case "Resting":
            //        lostTraitSummary += "\n" + character.name + " is checking for reactions towards characters in vision...";
            //        for (int i = 0; i < inVisionCharacters.Count; i++) {
            //            Character currCharacter = inVisionCharacters[i];
            //            if (!AddHostileInRange(currCharacter)) {
            //                //If not hostile, try to react to character's action
            //                AddUnprocessedPOI(currCharacter);
            //            }
            //        }
            //        break;
            //}
            character.logComponent.PrintLogIfActive(lostTraitSummary);
            UpdateAnimation();
            UpdateActionIcon();
        } 
        //else if (inVisionCharacters.Contains(character)) {
        //    //if the character that lost a trait is not this character and that character is in this character's hostility range
        //    //and the trait that was lost is a negative disabler trait, react to them.
        //    AddHostileInRange(character);
        //}
    }
    /// <summary>
    /// Listener for when a party starts travelling towards another npcSettlement.
    /// </summary>
    /// <param name="travellingParty">The travelling party.</param>
    private void OnCharacterAreaTravelling(Party travellingParty) {
        //if (targetPOI is Character) {
        //    Character targetCharacter = targetPOI as Character;
        //    if (travellingParty.IsPOICarried(targetCharacter)) {
        //        Action action = failedToComputePathAction;
        //        if(action != null) {
        //            if (character.currentParty.icon.isTravelling) {
        //                if(character.currentParty.icon.travelLine != null) {
        //                    character.currentParty.icon.SetOnArriveAction(() => character.OnArriveAtAreaStopMovement());
        //                } else {
        //                    StopMovement();
        //                }
        //            }
        //        }
        //        //set arrival action to null, because some arrival actions set it when executed
        //        ClearArrivalAction();
        //        action?.Invoke();
        //    }
        //}
        if (travellingParty.isCarryingAnyPOI) {
            if (travellingParty.IsPOICarried(targetPOI)) {
                //If the travelling party is travelling outside and is carrying a poi that is being targetted by this marker, this marker should fail to compute path
                Action action = failedToComputePathAction;
                if (action != null) {
                    //if (character.currentParty.icon.isTravellingOutside) {
                    //    if (character.currentParty.icon.travelLine != null) {
                    //        character.currentParty.icon.SetOnArriveAction(() => character.OnArriveAtAreaStopMovement());
                    //    } else {
                    //        StopMovement();
                    //    }
                    //}
                    if (character.currentParty.icon.isTravellingOutside) {
                        character.currentParty.icon.SetOnArriveAction(() => character.OnArriveAtAreaStopMovement());
                    } else {
                        StopMovement();
                    }
                }
                //set arrival action to null, because some arrival actions set it when executed
                failedToComputePathAction = null;
                action?.Invoke();
            }
            if(travellingParty.carriedPOI is Character) {
                Character carriedCharacter = travellingParty.carriedPOI as Character;
                character.combatComponent.RemoveHostileInRange(carriedCharacter); //removed hostile because he/she left the npcSettlement.
                character.combatComponent.RemoveAvoidInRange(carriedCharacter);
                RemovePOIFromInVisionRange(carriedCharacter);
                visionCollider.RemovePOIAsInRangeButDifferentStructure(carriedCharacter);
            }
        }
        //for (int i = 0; i < travellingParty.characters.Count; i++) {
        //    Character traveller = travellingParty.characters[i];
        //    if(traveller != character) {
        //        RemoveHostileInRange(traveller); //removed hostile because he/she left the npcSettlement.
        //        RemoveAvoidInRange(traveller);
        //        RemovePOIFromInVisionRange(traveller);
        //        visionCollision.RemovePOIAsInRangeButDifferentStructure(traveller);
        //    }
        //}

    }
    private void SelfGainedTrait(Character characterThatGainedTrait, Trait trait) {
        string gainTraitSummary =
            $"{GameManager.Instance.TodayLogString()}{characterThatGainedTrait.name} has <color=green>gained</color> trait <b>{trait.name}</b>";
        //if (trait.type == TRAIT_TYPE.DISABLER) { //if the character gained a disabler trait, hinder movement
        //    pathfindingAI.ClearAllCurrentPathData();
        //    pathfindingAI.canSearch = false;
        //    pathfindingAI.AdjustDoNotMove(1);
        //    gainTraitSummary += "\nGained trait is a disabler trait, adjusting do not move value.";
        //}
        if (!characterThatGainedTrait.canPerform) { //trait.type == TRAIT_TYPE.DISABLER && trait.effect == TRAIT_EFFECT.NEGATIVE
            //if the character gained an unconscious trait, exit current state if it is flee
            if (character.isInCombat) {
                characterThatGainedTrait.stateComponent.ExitCurrentState();
                gainTraitSummary += "\nGained trait is unconscious, and characters current state is combat, exiting combat state.";
            }

            //Once a character has a negative disabler trait, clear hostile and avoid list
            character.combatComponent.ClearHostilesInRange(false);
            character.combatComponent.ClearAvoidInRange(false);
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
            //if (inVisionCharacters.Contains(otherCharacter)) {
            //    character.CreateJobsOnEnterVisionWith(otherCharacter);
            //}
            if (inVisionCharacters.Contains(otherCharacter)) {
                character.CreateJobsOnTargetGainTrait(otherCharacter, trait);
            }

            //Only remove hostile in range from non lethal combat if target specifically becomes: Unconscious, Zapped or Restrained.
            //if (!otherCharacter.canPerform) {
            if (character.combatComponent.IsLethalCombatForTarget(otherCharacter) == false) {
                if (otherCharacter.traitContainer.HasTrait("Unconscious", "Zapped", "Restrained")) {
                    if (character.combatComponent.hostilesInRange.Contains(otherCharacter)) {
                        character.combatComponent.RemoveHostileInRange(otherCharacter);
                    }
                    character.combatComponent.RemoveAvoidInRange(otherCharacter);
                }
            }
            
        }
    }
    // private void OnItemRemovedFromTile(SpecialToken token, LocationGridTile removedFrom) {
    //     character.combatComponent.RemoveHostileInRange(token);
    //     //if (hostilesInRange.Contains(token)) {
    //     //    RemoveHostileInRange(token);
    //     //}
    // }
    private void OnTileObjectRemovedFromTile(TileObject obj, Character removedBy, LocationGridTile removedFrom) {
        character.combatComponent.RemoveHostileInRange(obj);
        character.combatComponent.RemoveAvoidInRange(obj);
        RemovePOIFromInVisionRange(obj);
        visionCollider.RemovePOIAsInRangeButDifferentStructure(obj);
    }
    #endregion

    #region UI
    private void OnMenuOpened(InfoUIBase @base) {
        // if (menu is CharacterInfoUI) {
        //     if ((menu as CharacterInfoUI).activeCharacter.id == character.id) {
        //         clickedImg.gameObject.SetActive(true);
        //     } else {
        //         clickedImg.gameObject.SetActive(false);
        //     }
        //
        // }
    }
    private void OnMenuClosed(InfoUIBase @base) {
        // if (menu is CharacterInfoUI) {
        //     clickedImg.gameObject.SetActive(false);
        //     //UnhighlightMarker();
        // }
    }
    private void OnProgressionSpeedChanged(PROGRESSION_SPEED progSpeed) {
        if (progSpeed == PROGRESSION_SPEED.X1) {
            progressionSpeedMultiplier = 1f;
        } else if (progSpeed == PROGRESSION_SPEED.X2) {
            progressionSpeedMultiplier = 1.5f;
        } else if (progSpeed == PROGRESSION_SPEED.X4) {
            progressionSpeedMultiplier = 2f;
        }
        UpdateSpeed();
        UpdateAnimationSpeed();
    }
    #endregion

    #region Action Icon
    public void UpdateActionIcon() {
        if (character == null) {
            return;
        }
        //int negativeDisablerCount = character.traitContainer.GetAllTraitsOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE).Count;
        //if ((negativeDisablerCount >= 2 || (negativeDisablerCount == 1 && character.traitContainer.GetNormalTrait<Trait>("Paralyzed") == null)) || character.isDead) {
        //    actionIcon.gameObject.SetActive(false);
        //    return;
        //}
        if (character.isConversing && !character.isInCombat) {
            actionIcon.sprite = InteractionManager.Instance.actionIconDictionary[GoapActionStateDB.Social_Icon];
            //if (character.isFlirting) {
            //    actionIcon.sprite = actionIconDictionary[GoapActionStateDB.Flirt_Icon];
            //} else {
            //    actionIcon.sprite = actionIconDictionary[GoapActionStateDB.Social_Icon];
            //}
            actionIcon.gameObject.SetActive(true);
            return;
        }
        
        if (character.interruptComponent.isInterrupted) {
            if (character.interruptComponent.currentInterrupt.interruptIconString != GoapActionStateDB.No_Icon) {
                actionIcon.sprite = InteractionManager.Instance.actionIconDictionary[character.interruptComponent.currentInterrupt.interruptIconString];
                actionIcon.gameObject.SetActive(true);
            } else {
                actionIcon.gameObject.SetActive(false);
            }
            return;
        } else if (character.interruptComponent.hasTriggeredSimultaneousInterrupt) {
            if (character.interruptComponent.triggeredSimultaneousInterrupt.interruptIconString != GoapActionStateDB.No_Icon) {
                actionIcon.sprite = InteractionManager.Instance.actionIconDictionary[character.interruptComponent.triggeredSimultaneousInterrupt.interruptIconString];
                actionIcon.gameObject.SetActive(true);
            } else {
                actionIcon.gameObject.SetActive(false);
            }
            return;
        } else {
            actionIcon.gameObject.SetActive(false);
        }
        
        if (character.currentActionNode != null) {
            if (character.currentActionNode.action.actionIconString != GoapActionStateDB.No_Icon) {
                actionIcon.sprite = InteractionManager.Instance.actionIconDictionary[character.currentActionNode.action.actionIconString];
                actionIcon.gameObject.SetActive(true);
            } else {
                actionIcon.gameObject.SetActive(false);
            }
        } else if (character.stateComponent.currentState != null) {
            if (character.stateComponent.currentState.actionIconString != GoapActionStateDB.No_Icon) {
                actionIcon.sprite = InteractionManager.Instance.actionIconDictionary[character.stateComponent.currentState.actionIconString];
                actionIcon.gameObject.SetActive(true);
            } else {
                actionIcon.gameObject.SetActive(false);
            }
        } else if (hasFleePath) {
            actionIcon.sprite = InteractionManager.Instance.actionIconDictionary[GoapActionStateDB.Flee_Icon];
            actionIcon.gameObject.SetActive(true);
        } else {
            //no action or state
            actionIcon.gameObject.SetActive(false);
        }
    }
    #endregion

    #region Object Pool
    public override void Reset() {
        base.Reset();
        destinationTile = null;
        //onProcessCombat = null;
        character.combatComponent.SetOnProcessCombatAction(null);
        SetMarkerColor(Color.white);
        actionIcon.gameObject.SetActive(false);
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
        //If any time a character goes to a structure outside the trap structure, the trap structure data will be cleared out
        if (character.trapStructure.structure != null && character.trapStructure.structure != destinationTile.structure) {
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
            SetDestination(destinationTile.centeredWorldLocation);
            StartMovement();
        }
        
    }
    public void GoToPOI(IPointOfInterest targetPOI, Action arrivalAction = null, Action failedToComputePathAction = null, STRUCTURE_TYPE[] notAllowedStructures = null) {
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
                if (targetCharacter.currentParty != null && targetCharacter.currentParty.icon != null && targetCharacter.currentParty.icon.isTravellingOutside) {
                    OnCharacterAreaTravelling(targetCharacter.currentParty);
                } 
                break;
            default:
                if(targetPOI is MovingTileObject) {
                    SetTargetTransform(targetPOI.mapObjectVisual.transform);
                } else {
                    if (targetPOI.gridTileLocation == null) {
                        throw new Exception($"{character.name} is trying to go to a {targetPOI.ToString()} but its tile location is null");
                    }
                    SetDestination(targetPOI.gridTileLocation.centeredWorldLocation);
                }
                break;
        }
        StartMovement();
    }
    public void GoTo(ITraitable target, Action arrivalAction = null, Action failedToComputePathAction = null, STRUCTURE_TYPE[] notAllowedStructures = null) {
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
    public void ArrivedAtTarget() {
        if (character.isInCombat) {
            if((character.stateComponent.currentState as CombatState).isAttacking){
                return;
            }
        }
        StopMovement();
        Action action = arrivalAction;
        //set arrival action to null, because some arrival actions set it
        ClearArrivalAction();
        action?.Invoke();

        targetPOI = null;
    }
    private void StartMovement() {
        isMoving = true;
        UpdateSpeed();
        pathfindingAI.SetIsStopMovement(false);
        character.currentParty.icon.SetIsTravelling(true);
        UpdateAnimation();
        // Messenger.AddListener(Signals.TICK_ENDED, PerTickMovement);
    }
    public void StopMovement() {
        isMoving = false;
        string log = $"{character.name} StopMovement function is called!";
        character.logComponent.PrintLogIfActive(log);
        if (ReferenceEquals(character.currentParty.icon, null) == false) {
            character.currentParty.icon.SetIsTravelling(false);
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
        Profiler.BeginSample($"{character.name} PerTickMovement");
        if (isMoving) {
            character.PerTickDuringMovement();    
        }
        Profiler.EndSample();
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
    public void SetDestination(Vector3 destination) {
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
    private void UpdateVisualBasedOnCurrentAnimationFrame() {
        string currSpriteName = mainImg.sprite.name;
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
        if (!character.IsInOwnParty()) {
            PlaySleepGround();
            ResetBlood();
            //if (character.traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE)) {
            //    PlaySleepGround();
            //}
            return; //if not in own party do not update any other animations
        }
        if (character.isDead) {
            PlaySleepGround();
            StartCoroutine(StartBlood());
        } else {
            ResetBlood();
            if (character.isStoppedByOtherCharacter > 0) {
                if (character.canMove == false || (!character.canPerform && !character.canWitness)) {
                    PlaySleepGround();
                } else {
                    PlayIdle();
                }
            } else if (character.canMove == false || (!character.canPerform && !character.canWitness) /*|| character.traitContainer.GetNormalTrait<Trait>("Resting") != null || character.traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE)*/) {
                PlaySleepGround();
            } else if (ReferenceEquals(character.currentParty.icon, null) == false && character.currentParty.icon.isTravelling) {
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
            animationListener.StartAttackExecution();
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
        var sortingOrder = InnerMapManager.DefaultCharacterSortingOrder + character.id;
        mainImg.sortingOrder = sortingOrder;
        hairImg.sortingOrder = sortingOrder + 1;
        knockedOutHairImg.sortingOrder = sortingOrder + 1;
        nameLbl.sortingOrder = sortingOrder;
        actionIcon.sortingOrder = sortingOrder;
        hoveredImg.sortingOrder = sortingOrder - 1;
        clickedImg.sortingOrder = sortingOrder - 1;
        colorHighlight.sortingOrder = sortingOrder - 1;
        bloodSplatterEffectRenderer.sortingOrder = InnerMapManager.GroundTilemapSortingOrder + 5;
        hpBarGO.GetComponent<Canvas>().sortingOrder = sortingOrder;
    }
    private float GetSpeed() {
        float speed = GetSpeedWithoutProgressionMultiplier();
        speed *= progressionSpeedMultiplier;
        return speed;
    }
    private float GetSpeedWithoutProgressionMultiplier() {
        float speed = character.runSpeed;
        if (targettedByRemoveNegativeTraitActionsCounter > 0) {
            speed = character.walkSpeed;
        } else {
            if (useWalkSpeed > 0) {
                speed = character.walkSpeed;
            } else {
                if (character.stateComponent.currentState != null) {
                    if (character.stateComponent.currentState.characterState == CHARACTER_STATE.PATROL
                        || character.stateComponent.currentState.characterState == CHARACTER_STATE.STROLL
                        || character.stateComponent.currentState.characterState == CHARACTER_STATE.STROLL_OUTSIDE) {
                        //Walk
                        speed = character.walkSpeed;
                    }else if (character.stateComponent.currentState.characterState == CHARACTER_STATE.DOUSE_FIRE) {
                        //Run
                        speed = character.runSpeed;
                    }
                }
                if (character.currentActionNode != null) {
                    if (character.currentActionNode.action.goapType == INTERACTION_TYPE.RETURN_HOME) {
                        //Run
                        speed = character.runSpeed;
                    }
                }
            }
        }
        speed += (speed * character.speedModifier);
        if (speed <= 0f) {
            speed = 0.5f;
        }
        return speed;
    }
    public void UpdateSpeed() {
        pathfindingAI.speed = GetSpeed();
        //Debug.Log("Updated speed of " + character.name + ". New speed is: " + pathfindingAI.speed.ToString());
    }
    public void AdjustUseWalkSpeed(int amount) {
        useWalkSpeed += amount;
        useWalkSpeed = Mathf.Max(0, useWalkSpeed);
    }
    public new void SetActiveState(bool state) {
        Debug.Log($"Set active state of {this.name} to {state.ToString()}");
        this.gameObject.SetActive(state);
    }
    /// <summary>
    /// Set the state of all visual aspects of this marker.
    /// </summary>
    /// <param name="state">The state the visuals should be in (active or inactive)</param>
    public void SetVisualState(bool state) {
        mainImg.gameObject.SetActive(state);
        nameLbl.gameObject.SetActive(state);
        actionIcon.enabled = state;
        hoveredImg.enabled = state;
        // clickedImg.enabled = state;
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
    public void UpdateMarkerVisuals() {
        UpdateHairVisuals();
    }
    public void UpdatePosition() {
        //This is checked per update, stress test this for performance
        //I'm keeping a separate field called anchoredPos instead of using the rect transform anchoredPosition directly because the multithread cannot access transform components
        anchoredPos = transform.localPosition;

        if (previousGridTile != character.gridTileLocation) {
            character.gridTileLocation.parentMap.region.innerMap.OnCharacterMovedTo(character, character.gridTileLocation, previousGridTile);
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
        CreateCollisionTrigger();
        UpdateSpeed();
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
    public void InitialPlaceMarkerAt(Vector3 worldPosition, Region region, bool addToLocation = true) {
        PlaceMarkerAt(worldPosition, region, addToLocation);
        pathfindingAI.UpdateMe();
        SetCollidersState(true);
        visionCollider.Initialize();
        CreateCollisionTrigger();
        UpdateSpeed();
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
        if (character.race == RACE.SKELETON || character is Summon || character.minion != null || character.destroyMarkerOnDeath) {
            character.DestroyMarker();
        } else {
            SetCollidersState(false);
            //onProcessCombat = null;
            character.combatComponent.SetOnProcessCombatAction(null);
            pathfindingAI.ClearAllCurrentPathData();
            UpdateAnimation();
            UpdateActionIcon();
            gameObject.transform.SetParent(deathTileLocation.parentMap.objectsParent);
            LocationGridTile placeMarkerAt = deathTileLocation;
            if (deathTileLocation.isOccupied) {
                placeMarkerAt = deathTileLocation.GetNearestUnoccupiedTileFromThis();
            }
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
    public void UpdateCenteredWorldPos() {
        centeredWorldPos = character.gridTileLocation.centeredWorldLocation;
    }
    public void SetTargetPOI(IPointOfInterest poi) {
        this.targetPOI = poi;
    }
    private bool CanDoStealthActionToTarget(Character target) {
        if (!target.isDead) {
            if (target.marker.inVisionCharacters.Count > 1) {
                return false; //if there are 2 or more in vision of target character it means he is not alone anymore
            } else if (target.marker.inVisionCharacters.Count == 1) {
                if (!target.marker.inVisionCharacters.Contains(character)) {
                    return false; //if there is only one in vision of target character and it is not this character, it means he is not alone
                }
            }
        } else {
            if (inVisionCharacters.Count > 1) {
                return false;
            }
        }
        return true;
    }
    public bool CanDoStealthActionToTarget(IPointOfInterest target) {
        if(target is Character) {
            return CanDoStealthActionToTarget(target as Character);
        }
        if (inVisionCharacters.Count > 0) {
            return false;
        }
        return true;
    }
    public void SetMarkerColor(Color color) {
        mainImg.color = color;
    }
    private void UpdateHairState() {
        //TODO: Find another way to unify this
        if (character.characterClass.className == "Mage" || character.visuals.portraitSettings.hair == -1 || 
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
    public void RemovePOIFromInVisionRange(IPointOfInterest poi) {
        if (inVisionPOIs.Remove(poi)) {
            RemoveUnprocessedPOI(poi);
            character.combatComponent.RemoveAvoidInRangeSchedule(poi);
            if (poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
                Character target = poi as Character;
                inVisionCharacters.Remove(target);
                Messenger.Broadcast(Signals.CHARACTER_REMOVED_FROM_VISION, character, target);
            } else if (poi.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
                inVisionTileObjects.Remove(poi as TileObject);
            }
        }
    }
    public void ClearPOIsInVisionRange() {
        inVisionPOIs.Clear();
        inVisionCharacters.Clear();
        inVisionTileObjects.Clear();
        ClearUnprocessedPOI();
    }
    public void LogPOIsInVisionRange() {
        string summary = $"{character.name}'s POIs in range: ";
        for (int i = 0; i < inVisionPOIs.Count; i++) {
            summary += $"\n- {inVisionPOIs[i]}";
        }
        Debug.Log(summary);
    }
    private void OnAddPOIAsInVisionRange(IPointOfInterest poi) {
        if (character.currentActionNode != null && character.currentActionNode.action.actionLocationType == ACTION_LOCATION_TYPE.TARGET_IN_VISION && character.currentActionNode.poiTarget == poi) {
            StopMovement();
            character.PerformGoapAction();
        }
    }
    public void AddUnprocessedPOI(IPointOfInterest poi, bool reactToActionOnly = false) {
        // if (character.minion != null || character is Summon) {
        //     //Minion or Summon cannot process pois
        //     return;
        // }
        if (reactToActionOnly) {
            if (!unprocessedVisionPOIsForActionOnly.Contains(poi)) {
                unprocessedVisionPOIsForActionOnly.Add(poi);
            }
        }
        if (!unprocessedVisionPOIs.Contains(poi)) {
            unprocessedVisionPOIs.Add(poi);
        }
        // character.logComponent.PrintLogIfActive(character.name + " added unprocessed poi " + poi.nameWithID);
    }
    public void RemoveUnprocessedPOI(IPointOfInterest poi) {
        unprocessedVisionPOIs.Remove(poi);
        unprocessedVisionPOIsForActionOnly.Remove(poi);
    }
    public void ClearUnprocessedPOI() {
        unprocessedVisionPOIs.Clear();
        unprocessedVisionPOIsForActionOnly.Clear();
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
        Profiler.BeginSample($"{character.name} ProcessAllUnprocessedVisionPOIs");
        if(unprocessedVisionPOIs.Count > 0) {
            string log = $"{character.name} tick ended! Processing all unprocessed in visions...";
            if (!character.isDead/* && character.canWitness*/) { //character.traitContainer.GetNormalTrait<Trait>("Unconscious", "Resting", "Zapped") == null
                for (int i = 0; i < unprocessedVisionPOIs.Count; i++) {
                    IPointOfInterest poi = unprocessedVisionPOIs[i];
                    if (poi.mapObjectVisual == null) {
                        log += $"\n-{poi.nameWithID}'s map visual has been destroyed. Skipping...";
                        continue;
                    }
                    log += $"\n-{poi.nameWithID}";
                    bool reactToActionOnly = false;
                    if (unprocessedVisionPOIsForActionOnly.Count > 0) {
                        reactToActionOnly = unprocessedVisionPOIsForActionOnly.Contains(poi);
                    }
                    character.ThisCharacterSaw(poi, reactToActionOnly);
                }
            } else {
                log += "\n - Character is either dead or cannot witness, not processing...";
            }
            ClearUnprocessedPOI();
            character.logComponent.PrintLogIfActive(log);
        }
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
    //        if (processCombatBehavior && character.isInCombat) {
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
    //            if (character.isInCombat) {
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
        if (character.faction != null && character.faction.isMajorNonPlayerFriendlyNeutral) {
            if (inVisionCharacters.Contains(otherCharacter)) {
                PlayerManager.Instance.player.threatComponent.AdjustThreat(10);
            }
        }
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
    //            if (character.isInCombat) {
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
    //            if (character.isInCombat) {
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
        LocationGridTile chosenTile;
        if(character is Summon summon) {
            chosenTile = summon.GetRandomLocationGridTileWithPath();
        } else {
            chosenTile = character.homeStructure.GetLocationStructure().GetRandomTile();
            
        }
        if(chosenTile == null) {
            chosenTile = character.currentStructure.GetRandomTile();
        }
        GoTo(chosenTile, OnFinishedTraversingFleePath);
    }
    public void OnFleePathComputed(Path path) {
        if (character == null || !character.canPerform || !character.canMove) {
            return; //this is for cases that the character is no longer in a combat state, but the pathfinding thread returns a flee path
        }
        arrivalAction = OnFinishedTraversingFleePath;
        StartMovement();
    }
    public void OnFinishedTraversingFleePath() {
        if (character.isInCombat) {
            (character.stateComponent.currentState as CombatState).FinishedTravellingFleePath();
        }
    }
    public void SetHasFleePath(bool state) {
        hasFleePath = state;
    }
    #endregion

    #region Combat
    public void UpdateAttackSpeedMeter() {
        if (hpBarGO.activeSelf) {
            aspeedFill.fillAmount = attackSpeedMeter / character.attackSpeed;
        }
    }
    public void ResetAttackSpeed() {
        attackSpeedMeter = 0f;
        UpdateAttackSpeedMeter();
    }
    public bool CanAttackByAttackSpeed() {
        return attackSpeedMeter >= character.attackSpeed;
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
        //do the ray test
        var size = Physics2D.RaycastNonAlloc(start, direction, linOfSightHitObjects, rayDistance, 
            GameUtilities.Line_Of_Sight_Layer_Mask);
        Profiler.EndSample();
        
        Profiler.BeginSample($"{character.name} Raycast result loop");
        for (int i = 0; i < size; i++) {
            RaycastHit2D hit = linOfSightHitObjects[i];
            if(hit.collider.gameObject.layer == LayerMask.NameToLayer("Unpassable")) {
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
        visionTrigger.SetCollidersState(state);
    }
    #endregion

    #region Map Object Visual
    public override void UpdateTileObjectVisual(Character obj) { }
    public override void ApplyFurnitureSettings(FurnitureSetting furnitureSetting) { }
    public virtual bool IsMapObjectMenuVisible() {
        if (UIManager.Instance.characterInfoUI.isShowing) {
            return UIManager.Instance.characterInfoUI.activeCharacter == this.character;
        }
        return false;
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
    }
    #endregion

}
