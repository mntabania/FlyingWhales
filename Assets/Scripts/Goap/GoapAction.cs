using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Reflection;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Traits;

public class GoapAction : IReactable {

    public INTERACTION_TYPE goapType { get; private set; }
    public virtual ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.DIRECT; } }
    public string goapName { get; protected set; }
    public List<Precondition> basePreconditions { get; private set; }
    public List<GoapEffect> baseExpectedEffects { get; private set; }
    public List<GoapEffectConditionTypeAndTargetType> possibleExpectedEffectsTypeAndTargetMatching { get; private set; }
    public RACE[] racesThatCanDoAction { get; protected set; }
    public Dictionary<string, GoapActionState> states { get; protected set; }
    public ACTION_LOCATION_TYPE actionLocationType { get; protected set; } //This is set in every action's constructor
    public bool showNotification { get; protected set; } //should this action show a notification when it is done by its actor or when it receives a plan with this action as it's end node?
    public bool shouldAddLogs { get; protected set; } //should this action add logs to it's actor?
    // public bool shouldIntelNotificationOnlyIfActorIsActive { get; protected set; }
    public bool isNotificationAnIntel { get; protected set; }
    public string actionIconString { get; protected set; }
    public string animationName { get; protected set; } //what animation should the character be playing while doing this action
    public bool doesNotStopTargetCharacter { get; protected set; }
    public bool canBeAdvertisedEvenIfActorIsUnavailable { get; protected set; }
    protected TIME_IN_WORDS[] validTimeOfDays;
    public POINT_OF_INTEREST_TYPE[] advertisedBy { get; protected set; } //list of poi types that can advertise this action

    #region getters
    public string name => goapName;
    #endregion

    public GoapAction(INTERACTION_TYPE goapType) { //, INTERACTION_ALIGNMENT alignment, Character actor, IPointOfInterest poiTarget
        this.goapType = goapType;
        this.goapName = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(goapType.ToString());
        showNotification = true;
        shouldAddLogs = true;
        basePreconditions = new List<Precondition>();
        baseExpectedEffects = new List<GoapEffect>();
        possibleExpectedEffectsTypeAndTargetMatching = new List<GoapEffectConditionTypeAndTargetType>();
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
        actionIconString = GoapActionStateDB.No_Icon;
        canBeAdvertisedEvenIfActorIsUnavailable = false;
        animationName = "Interacting";
        ConstructBasePreconditionsAndEffects();
        CreateStates();
    }

    #region States

    public void SetState(string stateName, ActualGoapNode actionNode) {
        actionNode.OnActionStateSet(stateName);
        Messenger.Broadcast(Signals.AFTER_ACTION_STATE_SET, stateName, actionNode);
    }
    #endregion

    #region Virtuals
    private void CreateStates() {
        string summary = $"Creating states for goap action (Dynamic) {goapType}";
        states = new Dictionary<string, GoapActionState>();
        if (GoapActionStateDB.goapActionStates.ContainsKey(this.goapType)) {
            StateNameAndDuration[] statesSetup = GoapActionStateDB.goapActionStates[this.goapType];
            for (int i = 0; i < statesSetup.Length; i++) {
                StateNameAndDuration state = statesSetup[i];
                summary += $"\nCreating {state.name}";
                string trimmedState = UtilityScripts.Utilities.RemoveAllWhiteSpace(state.name);
                Type thisType = this.GetType();
                string estimatedPreMethodName = $"Pre{trimmedState}";
                string estimatedPerTickMethodName = $"PerTick{trimmedState}";
                string estimatedAfterMethodName = $"After{trimmedState}";

                MethodInfo preMethod = thisType.GetMethod(estimatedPreMethodName, new Type[] { typeof(ActualGoapNode) }); //
                MethodInfo perMethod = thisType.GetMethod(estimatedPerTickMethodName, new Type[] { typeof(ActualGoapNode) });
                MethodInfo afterMethod = thisType.GetMethod(estimatedAfterMethodName, new Type[] { typeof(ActualGoapNode) });
                Action<ActualGoapNode> preAction = null;
                Action<ActualGoapNode> perAction = null;
                Action<ActualGoapNode> afterAction = null;
                if (preMethod != null) {
                    preAction = (Action<ActualGoapNode>) Delegate.CreateDelegate(typeof(Action<ActualGoapNode>), this, preMethod, false);
                    summary += $"\n\tPre Method is {preMethod}";
                } else {
                    summary += "\n\tPre Method is null";
                }
                if (perMethod != null) {
                    perAction = (Action<ActualGoapNode>) Delegate.CreateDelegate(typeof(Action<ActualGoapNode>), this, perMethod, false);
                    summary += $"\n\tPer Tick Method is {perAction}";
                } else {
                    summary += "\n\tPer Tick Method is null";
                }
                if (afterMethod != null) {
                    afterAction = (Action<ActualGoapNode>) Delegate.CreateDelegate(typeof(Action<ActualGoapNode>), this, afterMethod, false);
                    summary += $"\n\tAfter Method is {afterAction}";
                } else {
                    summary += "\n\tAfter Method is null";
                }
                GoapActionState newState = new GoapActionState(state.name, this, preAction, perAction, afterAction, state.duration, state.status, state.animationName);
                states.Add(state.name, newState);
                //summary += "\n Creating state " + state.name;
            }
        }
        //Debug.Log(summary);
    }
    protected virtual void ConstructBasePreconditionsAndEffects() { }
    public virtual void Perform(ActualGoapNode actionNode) { }
    protected virtual bool AreRequirementsSatisfied(Character actor, IPointOfInterest target, object[] otherData) { return true; }
    protected virtual int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        return 0;
    }
    public virtual void AddFillersToLog(Log log, ActualGoapNode node) {
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        LocationStructure targetStructure = node.targetStructure;
        log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(poiTarget, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER); //Target character is only the identifier but it doesn't mean that this is a character, it can be item, etc.
        if (targetStructure != null) {
            log.AddToFillers(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
        } else {
            log.AddToFillers(actor.currentRegion, actor.currentRegion.name, LOG_IDENTIFIER.LANDMARK_1);
        }
    }
    public virtual GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        string stateName = "Target Missing";
        bool defaultTargetMissing = IsTargetMissing(node);
        GoapActionInvalidity goapActionInvalidity = new GoapActionInvalidity(defaultTargetMissing, stateName);
        //if (defaultTargetMissing == false) {
        //    //check the target's traits, if any of them can make this action invalid
        //    for (int i = 0; i < poiTarget.traitContainer.allTraits.Count; i++) {
        //        Trait trait = poiTarget.traitContainer.allTraits[i];
        //        if (trait.TryStopAction(goapType, actor, poiTarget, ref goapActionInvalidity)) {
        //            break; //a trait made this action invalid, stop loop
        //        }
        //    }
        //}
        return goapActionInvalidity;
    }
    public virtual void OnInvalidAction(ActualGoapNode node) { }
    public virtual LocationStructure GetTargetStructure(ActualGoapNode node) {
        //if (poiTarget is Character) {
        //    return (poiTarget as Character).currentStructure;
        //}
        IPointOfInterest poiTarget = node.poiTarget;
        if (poiTarget.gridTileLocation == null) {
            return null;
        }
        return poiTarget.gridTileLocation.structure;
    }
    /// <summary>
    /// Function to use when actionLocationType is NEAR_TARGET. <see cref="GoapNode.MoveToDoAction"/>
    /// Will, by default, return the poiTarget, but can be overridden to make actor go somewhere else.
    /// </summary>
    /// <returns></returns>
    public virtual IPointOfInterest GetTargetToGoTo(ActualGoapNode goapNode) {
        return goapNode.poiTarget;
    }
    public virtual LocationGridTile GetTargetTileToGoTo(ActualGoapNode goapNode) {
        return goapNode.poiTarget.gridTileLocation;
    }
    //If this action is being performed and is stopped abruptly, call this
    public virtual void OnStopWhilePerforming(ActualGoapNode node) { }
    /// <summary>
    /// What should happen when an action is stopped while the actor is still travelling towards it's target or when the action has already started?
    /// </summary>
    public virtual void OnStopWhileStarted(ActualGoapNode node) { }
    public virtual LocationGridTile GetOverrideTargetTile(ActualGoapNode goapNode) {
        return null;
    }
    public virtual string ReactionToActor(Character witness, ActualGoapNode node, REACTION_STATUS status) { return string.Empty; }
    public virtual string ReactionToTarget(Character witness, ActualGoapNode node, REACTION_STATUS status) { return string.Empty; }
    public virtual string ReactionOfTarget(ActualGoapNode node, REACTION_STATUS status) { return string.Empty; }
    public virtual void OnActionStarted(ActualGoapNode node) { }
    #endregion

    #region Utilities
    public int GetCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        int baseCost = GetBaseCost(actor, target, job, otherData);
        //modify costs based on actor's and target's traits
        //for (int i = 0; i < actor.traitContainer.allTraits.Count; i++) {
        //    Trait trait = actor.traitContainer.allTraits[i];
        //    trait.ExecuteCostModification(goapType, actor, target, otherData, ref baseCost);
        //}
        //for (int i = 0; i < target.traitContainer.allTraits.Count; i++) {
        //    Trait trait = target.traitContainer.allTraits[i];
        //    trait.ExecuteCostModification(goapType, actor, target, otherData, ref baseCost);
        //}
        return (baseCost * TimeOfDaysCostMultiplier(actor) * PreconditionCostMultiplier()) + GetDistanceCost(actor, target);
    }
    private bool IsTargetMissing(ActualGoapNode node) {
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        if (poiTarget.IsAvailable() == false || poiTarget.gridTileLocation == null) {
            return true;
        }
        if (!actor.currentRegion.IsSameCoreLocationAs(poiTarget.gridTileLocation.structure.location)) {
            return true;
        }
        
        if (actionLocationType == ACTION_LOCATION_TYPE.NEAR_TARGET) {
            //if the action type is NEAR_TARGET, then check if the actor is near the target, if not, this action is invalid.
            if (actor.gridTileLocation != poiTarget.gridTileLocation && actor.gridTileLocation.IsNeighbour(poiTarget.gridTileLocation) == false) {
                return true;
            }
        } else if (actionLocationType == ACTION_LOCATION_TYPE.NEAR_OTHER_TARGET) {
            //if the action type is NEAR_TARGET, then check if the actor is near the target, if not, this action is invalid.
            if (actor.gridTileLocation != node.targetTile && actor.gridTileLocation.IsNeighbour(node.targetTile) == false) {
                return true;
            }
        }
        return false;
    }
    public bool CanSatisfyRequirements(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        bool requirementActionSatisfied = !(poiTarget.poiType != POINT_OF_INTEREST_TYPE.CHARACTER 
                                            && poiTarget.traitContainer.HasTrait("Frozen") 
                                            && (actionCategory == ACTION_CATEGORY.DIRECT || actionCategory == ACTION_CATEGORY.CONSUME));
        if (requirementActionSatisfied) {
            requirementActionSatisfied = AreRequirementsSatisfied(actor, poiTarget, otherData);
            if (requirementActionSatisfied) {
                if (goapType.IsDirectCombatAction()) { //Reference: https://trello.com/c/uxZxcOEo/2343-critical-characters-shouldnt-attempt-hostile-actions
                    requirementActionSatisfied = actor.IsCombatReady();
                }
            }
        }
        return requirementActionSatisfied; //&& (validTimeOfDays == null || validTimeOfDays.Contains(GameManager.GetCurrentTimeInWordsOfTick()));
    }
    public bool DoesCharacterMatchRace(Character character) {
        if (racesThatCanDoAction != null) {
            return racesThatCanDoAction.Contains(character.race);
        }
        return false;
    }
    private int GetDistanceCost(Character actor, IPointOfInterest poiTarget) {
        // if (actor.currentSettlement == null) {
        //     return 1;
        // }
        LocationGridTile tile = poiTarget.gridTileLocation;
        if (actor.gridTileLocation != null && tile != null) {
            int distance = Mathf.RoundToInt(Vector2.Distance(actor.gridTileLocation.centeredWorldLocation, tile.centeredWorldLocation));
            distance = (int) (distance * 0.25f);
            if (!actor.currentRegion.IsSameCoreLocationAs(tile.structure.location)) {
                return distance + 100;
            }
            return distance;
        }
        return 1;
    }
    private int TimeOfDaysCostMultiplier(Character actor) {
        if (validTimeOfDays == null || validTimeOfDays.Contains(GameManager.GetCurrentTimeInWordsOfTick(actor))) {
            return 1;
        }
        return 3;
    }
    private int PreconditionCostMultiplier() {
        return Math.Max(basePreconditions.Count * 2, 1);
    }
    public void LogActionInvalid(GoapActionInvalidity goapActionInvalidity, ActualGoapNode node) {
        Log log = new Log(GameManager.Instance.Today(), "GoapAction", "Generic", "Invalid");
        log.AddToFillers(node.actor, node.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(node.poiTarget, node.poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        log.AddToFillers(null, UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetterOnly(goapType.ToString()), LOG_IDENTIFIER.STRING_1);
        log.AddLogToInvolvedObjects();
        // PlayerManager.Instance.player.ShowNotificationFrom(node.actor, log);
    }
    #endregion

    #region Preconditions
    protected void AddPrecondition(GoapEffect effect, Func<Character, IPointOfInterest, object[], bool> condition) {
        basePreconditions.Add(new Precondition(effect, condition));
    }
    public bool CanSatisfyAllPreconditions(Character actor, IPointOfInterest target, object[] otherData) {
        List<Precondition> preconditions = GetPreconditions(actor, target, otherData);
        for (int i = 0; i < preconditions.Count; i++) {
            if (!preconditions[i].CanSatisfyCondition(actor, target, otherData)) {
                return false;
            }
        }
        return true;
    }
    public virtual List<Precondition> GetPreconditions(Character actor, IPointOfInterest target, object[] otherData) {
        return basePreconditions;
    }
    #endregion

    #region Effects
    protected void AddExpectedEffect(GoapEffect effect) {
        baseExpectedEffects.Add(effect);
        AddPossibleExpectedEffectForTypeAndTargetMatching(new GoapEffectConditionTypeAndTargetType(effect.conditionType, effect.target));
    }
    protected void AddPossibleExpectedEffectForTypeAndTargetMatching(GoapEffectConditionTypeAndTargetType effect) {
        possibleExpectedEffectsTypeAndTargetMatching.Add(effect);
    }
    public bool WillEffectsSatisfyPrecondition(GoapEffect precondition, Character actor, IPointOfInterest target, object[] otherData) {
        List<GoapEffect> effects = GetExpectedEffects(actor, target, otherData);
        for (int i = 0; i < effects.Count; i++) {
            if(EffectPreconditionMatching(effects[i], precondition)) {
                return true;
            }
        }
        return false;
    }
    public bool WillEffectsMatchPreconditionTypeAndTarget(GoapEffect precondition) {
        List<GoapEffectConditionTypeAndTargetType> effects = possibleExpectedEffectsTypeAndTargetMatching;
        for (int i = 0; i < effects.Count; i++) {
            if (effects[i].conditionType == precondition.conditionType && effects[i].target == precondition.target) {
                return true;
            }
        }
        return false;
    }
    private bool EffectPreconditionMatching(GoapEffect effect, GoapEffect precondition) {
        if(effect.conditionType == precondition.conditionType && effect.target == precondition.target) { //&& CharacterManager.Instance.POIValueTypeMatching(effect.targetPOI, precondition.targetPOI)
            if (effect.conditionKey != "" && precondition.conditionKey != "") {
                if(effect.isKeyANumber && precondition.isKeyANumber) {
                    int effectInt = int.Parse(effect.conditionKey);
                    int preconditionInt = int.Parse(precondition.conditionKey);
                    return effectInt >= preconditionInt;
                } else {
                    return effect.conditionKey == precondition.conditionKey;
                }
                //switch (effect.conditionType) {
                //    case GOAP_EFFECT_CONDITION.HAS_SUPPLY:
                //    case GOAP_EFFECT_CONDITION.HAS_FOOD:
                //        int effectInt = (int) effect.conditionKey;
                //        int preconditionInt = (int) precondition.conditionKey;
                //        return effectInt >= preconditionInt;
                //    default:
                //        return effect.conditionKey == precondition.conditionKey;
                //}
            } else {
                return true;
            }
        }
        return false;
    }
    protected virtual List<GoapEffect> GetExpectedEffects(Character actor, IPointOfInterest target, object[] otherData) {
        List<GoapEffect> effects = new List<GoapEffect>(baseExpectedEffects);
        //TODO: Might be a more optimized way to do this
        //modify expected effects depending on actor's traits
        for (int i = 0; i < actor.traitContainer.allTraits.Count; i++) {
            Trait currTrait = actor.traitContainer.allTraits[i];
            currTrait.ExecuteExpectedEffectModification(goapType, actor, target, otherData, ref effects);
        }
        return effects;
    }
    #endregion
}

public struct GoapActionInvalidity {
    public bool isInvalid;
    public string stateName;

    public GoapActionInvalidity(bool isInvalid, string stateName) {
        this.isInvalid = isInvalid;
        this.stateName = stateName;
    }
}
public struct GoapEffectConditionTypeAndTargetType {
    public GOAP_EFFECT_CONDITION conditionType;
    public GOAP_EFFECT_TARGET target;
    
    public GoapEffectConditionTypeAndTargetType(GOAP_EFFECT_CONDITION conditionType, GOAP_EFFECT_TARGET target) {
        this.conditionType = conditionType;
        this.target = target;
    }
}
public struct GoapEffect {
    public GOAP_EFFECT_CONDITION conditionType;
    //public object conditionKey;
    public string conditionKey;
    public bool isKeyANumber;
    public GOAP_EFFECT_TARGET target;
    //public IPointOfInterest targetPOI; //this is the target that will be affected by the condition type and key

    public GoapEffect(GOAP_EFFECT_CONDITION conditionType, string conditionKey, bool isKeyANumber, GOAP_EFFECT_TARGET target) {
        this.conditionType = conditionType;
        this.conditionKey = conditionKey;
        this.isKeyANumber = isKeyANumber;
        this.target = target;
    }

    public override string ToString() {
        return $"{conditionType} - {conditionKey} - {target}";
    }
    //public string conditionString() {
    //    if(conditionKey is string) {
    //        return conditionKey.ToString();
    //    } else if (conditionKey is int) {
    //        return conditionKey.ToString();
    //    } else if (conditionKey is Character) {
    //        return (conditionKey as Character).name;
    //    } else if (conditionKey is Settlement) {
    //        return (conditionKey as Settlement).name;
    //    } else if (conditionKey is Region) {
    //        return (conditionKey as Region).name;
    //    } else if (conditionKey is SpecialToken) {
    //        return (conditionKey as SpecialToken).name;
    //    } else if (conditionKey is IPointOfInterest) {
    //        return (conditionKey as IPointOfInterest).name;
    //    }
    //    return string.Empty;
    //}
    //public string conditionKeyToString() {
    //    if (conditionKey is string) {
    //        return (string)conditionKey;
    //    } else if (conditionKey is int) {
    //        return ((int)conditionKey).ToString();
    //    } else if (conditionKey is Character) {
    //        return (conditionKey as Character).id.ToString();
    //    } else if (conditionKey is Settlement) {
    //        return (conditionKey as Settlement).id.ToString();
    //    } else if (conditionKey is Region) {
    //        return (conditionKey as Region).id.ToString();
    //    } else if (conditionKey is SpecialToken) {
    //        return (conditionKey as SpecialToken).id.ToString();
    //    } else if (conditionKey is IPointOfInterest) {
    //        return (conditionKey as IPointOfInterest).id.ToString();
    //    }
    //    return string.Empty;
    //}
    //public string conditionKeyTypeString() {
    //    if (conditionKey is string) {
    //        return "string";
    //    } else if (conditionKey is int) {
    //        return "int";
    //    } else if (conditionKey is Character) {
    //        return "character";
    //    } else if (conditionKey is Settlement) {
    //        return "settlement";
    //    } else if (conditionKey is Region) {
    //        return "region";
    //    } else if (conditionKey is SpecialToken) {
    //        return "item";
    //    } else if (conditionKey is IPointOfInterest) {
    //        return "poi";
    //    }
    //    return string.Empty;
    //}

    //public override bool Equals(object obj) {
    //    if (obj is GoapEffect) {
    //        GoapEffect otherEffect = (GoapEffect)obj;
    //        if (otherEffect.conditionType == conditionType) {
    //            if (string.IsNullOrEmpty(conditionString())) {
    //                return true;
    //            } else {
    //                return otherEffect.conditionString() == conditionString();
    //            }
    //        }
    //    }
    //    return base.Equals(obj);
    //}
}

[System.Serializable]
public class SaveDataGoapEffect {
    public GOAP_EFFECT_CONDITION conditionType;

    public string conditionKey;
    public string conditionKeyIdentifier;
    public POINT_OF_INTEREST_TYPE conditionKeyPOIType;
    public TILE_OBJECT_TYPE conditionKeyTileObjectType;


    public int targetPOIID;
    public POINT_OF_INTEREST_TYPE targetPOIType;
    public TILE_OBJECT_TYPE targetPOITileObjectType;

    public void Save(GoapEffect goapEffect) {
        conditionType = goapEffect.conditionType;

        //if(goapEffect.conditionKey != null) {
        //    conditionKeyIdentifier = goapEffect.conditionKeyTypeString();
        //    conditionKey = goapEffect.conditionKeyToString();
        //    if(goapEffect.conditionKey is IPointOfInterest) {
        //        conditionKeyPOIType = (goapEffect.conditionKey as IPointOfInterest).poiType;
        //    }
        //    if (goapEffect.conditionKey is TileObject) {
        //        conditionKeyTileObjectType = (goapEffect.conditionKey as TileObject).tileObjectType;
        //    }
        //} else {
        //    conditionKeyIdentifier = string.Empty;
        //}

        //if(goapEffect.targetPOI != null) {
        //    targetPOIID = goapEffect.targetPOI.id;
        //    targetPOIType = goapEffect.targetPOI.poiType;
        //    if(goapEffect.targetPOI is TileObject) {
        //        targetPOITileObjectType = (goapEffect.targetPOI as TileObject).tileObjectType;
        //    }
        //} else {
        //    targetPOIID = -1;
        //}
    }

    public GoapEffect Load() {
        GoapEffect effect = new GoapEffect() {
            conditionType = conditionType,
        };
        //if(targetPOIID != -1) {
        //    GoapEffect tempEffect = effect;
        //    if (targetPOIType == POINT_OF_INTEREST_TYPE.CHARACTER) {
        //        tempEffect.targetPOI = CharacterManager.Instance.GetCharacterByID(targetPOIID);
        //    } else if (targetPOIType == POINT_OF_INTEREST_TYPE.ITEM) {
        //        tempEffect.targetPOI = TokenManager.Instance.GetSpecialTokenByID(targetPOIID);
        //    } else if (targetPOIType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
        //        tempEffect.targetPOI = InteriorMapManager.Instance.GetTileObject(targetPOITileObjectType, targetPOIID);
        //    }
        //    effect = tempEffect;
        //}
        //if(conditionKeyIdentifier != string.Empty) {
        //    GoapEffect tempEffect = effect;
        //    if (conditionKeyIdentifier == "string") {
        //        tempEffect.conditionKey = conditionKey;
        //    } else if (conditionKey == "int") {
        //        tempEffect.conditionKey = int.Parse(conditionKey);
        //    } else if (conditionKey == "character") {
        //        tempEffect.conditionKey = CharacterManager.Instance.GetCharacterByID(int.Parse(conditionKey));
        //    } else if (conditionKey == "settlement") {
        //        tempEffect.conditionKey = LandmarkManager.Instance.GetAreaByID(int.Parse(conditionKey));
        //    } else if (conditionKey == "region") {
        //        tempEffect.conditionKey = GridMap.Instance.GetRegionByID(int.Parse(conditionKey));
        //    } else if (conditionKey == "item") {
        //        tempEffect.conditionKey = TokenManager.Instance.GetSpecialTokenByID(int.Parse(conditionKey));
        //    } else if (conditionKey == "poi") {
        //        if (conditionKeyPOIType == POINT_OF_INTEREST_TYPE.CHARACTER) {
        //            tempEffect.conditionKey = CharacterManager.Instance.GetCharacterByID(int.Parse(conditionKey));
        //        } else if (conditionKeyPOIType == POINT_OF_INTEREST_TYPE.ITEM) {
        //            tempEffect.conditionKey = TokenManager.Instance.GetSpecialTokenByID(int.Parse(conditionKey));
        //        } else if (conditionKeyPOIType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
        //            tempEffect.conditionKey = InteriorMapManager.Instance.GetTileObject(conditionKeyTileObjectType, int.Parse(conditionKey));
        //        }
        //    }
        //    effect = tempEffect;
        //}
        return effect;
    }
}

public class GoapActionData {
    public INTERACTION_TYPE goapType { get; protected set; }
    public RACE[] racesThatCanDoAction { get; protected set; }
    public Func<Character, IPointOfInterest, object[], bool> requirementAction { get; protected set; }
    public Func<Character, IPointOfInterest, object[], bool> requirementOnBuildGoapTreeAction { get; protected set; }

    public GoapActionData(INTERACTION_TYPE goapType) {
        this.goapType = goapType;
    }

    #region Virtuals
    public bool CanSatisfyTimeOfDays() {
        return true;
    }
    #endregion

    public bool CanSatisfyRequirements(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        bool requirementActionSatisfied = true;
        if (requirementAction != null) {
            requirementActionSatisfied = requirementAction.Invoke(actor, poiTarget, otherData);
        }
        if (requirementActionSatisfied) {
            if (goapType.IsDirectCombatAction()) { //Reference: https://trello.com/c/uxZxcOEo/2343-critical-characters-shouldnt-attempt-hostile-actions
                requirementActionSatisfied = actor.IsCombatReady();
            }
        }
        return requirementActionSatisfied;
    }
    public bool CanSatisfyRequirementOnBuildGoapTree(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        bool requirementActionSatisfied = true;
        if (requirementOnBuildGoapTreeAction != null) {
            requirementActionSatisfied = requirementOnBuildGoapTreeAction.Invoke(actor, poiTarget, otherData);
        }
        if (requirementActionSatisfied) {
            if (goapType.IsDirectCombatAction()) { //Reference: https://trello.com/c/uxZxcOEo/2343-critical-characters-shouldnt-attempt-hostile-actions
                requirementActionSatisfied = actor.IsCombatReady();
            }
        }
        return requirementActionSatisfied;
    }
}