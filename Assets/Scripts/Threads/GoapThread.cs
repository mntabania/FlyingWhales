using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoapThread : Multithread {
    public Character actor { get; private set; }
    public GoapPlan createdPlan { get; private set; }
    public GoapEffect goalEffect { get; private set; }
    public GoapAction goalAction { get; private set; }
    public INTERACTION_TYPE goalType { get; private set; }
    public IPointOfInterest target { get; private set; }
    //public bool isPriority { get; private set; }
    public bool isPersonalPlan { get; private set; }
    //public GOAP_CATEGORY category { get; private set; }
    //public List<IPointOfInterest> characterTargetsAwareness { get; private set; }
    public GoapPlanJob job { get; private set; }
    //public bool allowDeadTargets { get; private set; }
    //public List<INTERACTION_TYPE> actorAllowedActions { get; private set; }
    //public List<GoapAction> usableActions { get; private set; }
    public string log { get; private set; }
    //private Dictionary<INTERACTION_TYPE, object[]> otherData;

    //For recalculation
    public GoapPlan recalculationPlan;

    private Character owner;

    public GoapThread(Character actor, IPointOfInterest target, GoapEffect goalEffect, bool isPersonalPlan, GoapPlanJob job) {//, List<INTERACTION_TYPE> actorAllowedActions, List<GoapAction> usableActions
        this.createdPlan = null;
        this.recalculationPlan = null;
        this.actor = actor;
        this.target = target;
        this.goalEffect = goalEffect;
        //this.isPriority = isPriority;
        //this.characterTargetsAwareness = characterTargetsAwareness;
        //this.actorAllowedActions = actorAllowedActions;
        //this.usableActions = usableActions;
        this.isPersonalPlan = isPersonalPlan;
        //this.category = category;
        this.job = job;
        owner = actor;
        //if (actor.traitContainer.HasTrait("Disguised")) {
        //    actor = actor.traitContainer.GetNormalTrait<Traits.Disguised>("Disguised").disguisedCharacter;
        //}
        //this.allowDeadTargets = allowDeadTargets;
    }
    //public GoapThread(Character actor, IPointOfInterest target, GoapEffect goalEffect, GOAP_CATEGORY category, bool isPriority
    //    , bool isPersonalPlan, GoapPlanJob job, Dictionary<INTERACTION_TYPE, object[]> otherData = null) {//, List<INTERACTION_TYPE> actorAllowedActions, List<GoapAction> usableActions
    //    this.createdPlan = null;
    //    this.recalculationPlan = null;
    //    this.actor = actor;
    //    this.target = target;
    //    this.goalEffect = goalEffect;
    //    this.isPriority = isPriority;
    //    //this.characterTargetsAwareness = characterTargetsAwareness;
    //    //this.actorAllowedActions = actorAllowedActions;
    //    //this.usableActions = usableActions;
    //    this.isPersonalPlan = isPersonalPlan;
    //    this.category = category;
    //    this.job = job;
    //    //this.allowDeadTargets = allowDeadTargets;
    //    this.otherData = otherData;
    //}
    public GoapThread(Character actor, IPointOfInterest target, GoapAction goalAction
        , bool isPersonalPlan, GoapPlanJob job) {//, List<INTERACTION_TYPE> actorAllowedActions, List<GoapAction> usableActions
        this.createdPlan = null;
        this.recalculationPlan = null;
        this.actor = actor;
        this.target = target;
        this.goalAction = goalAction;
        //this.isPriority = isPriority;
        //this.characterTargetsAwareness = characterTargetsAwareness;
        //this.actorAllowedActions = actorAllowedActions;
        //this.usableActions = usableActions;
        this.isPersonalPlan = isPersonalPlan;
        //this.category = category;
        this.job = job;
        owner = actor;
        //if (actor.traitContainer.HasTrait("Disguised")) {
        //    actor = actor.traitContainer.GetNormalTrait<Traits.Disguised>("Disguised").disguisedCharacter;
        //}
        //this.allowDeadTargets = allowDeadTargets;
    }
    //public GoapThread(Character actor, INTERACTION_TYPE goalType, GOAP_CATEGORY category, bool isPriority
    //    , bool isPersonalPlan, GoapPlanJob job, Dictionary<INTERACTION_TYPE, object[]> otherData = null) {//, List<INTERACTION_TYPE> actorAllowedActions, List<GoapAction> usableActions
    //    this.createdPlan = null;
    //    this.recalculationPlan = null;
    //    this.actor = actor;
    //    this.goalType = goalType;
    //    this.isPriority = isPriority;
    //    //this.characterTargetsAwareness = characterTargetsAwareness;
    //    //this.actorAllowedActions = actorAllowedActions;
    //    //this.usableActions = usableActions;
    //    this.isPersonalPlan = isPersonalPlan;
    //    this.category = category;
    //    this.job = job;
    //    //this.allowDeadTargets = allowDeadTargets;
    //    this.otherData = otherData;
    //}
    public GoapThread(Character actor, INTERACTION_TYPE goalType, IPointOfInterest target
        , bool isPersonalPlan, GoapPlanJob job) {//, List<INTERACTION_TYPE> actorAllowedActions, List<GoapAction> usableActions
        this.createdPlan = null;
        this.recalculationPlan = null;
        this.actor = actor;
        this.target = target;
        this.goalType = goalType;
        //this.isPriority = isPriority;
        //this.characterTargetsAwareness = characterTargetsAwareness;
        this.isPersonalPlan = isPersonalPlan;
        //this.category = category;
        this.job = job;

        owner = actor;
        //if (actor.traitContainer.HasTrait("Disguised")) {
        //    actor = actor.traitContainer.GetNormalTrait<Traits.Disguised>("Disguised").disguisedCharacter;
        //}
        //this.allowDeadTargets = allowDeadTargets;
        //this.otherData = otherData;
    }
    public GoapThread(Character actor, GoapPlan currentPlan, GoapPlanJob job) {//, List<GoapAction> usableActions
        this.createdPlan = null;
        this.actor = actor;
        this.recalculationPlan = currentPlan;
        this.job = job;
        owner = actor;
        //if (actor.traitContainer.HasTrait("Disguised")) {
        //    actor = actor.traitContainer.GetNormalTrait<Traits.Disguised>("Disguised").disguisedCharacter;
        //}
        //this.actorAllowedActions = actorAllowedActions;
        //this.usableActions = usableActions;
    }
    
    #region Overrides
    public override void DoMultithread() {
        base.DoMultithread();
        try {
            CreatePlan();
        }catch(System.Exception e) {
            Debug.LogError(
                $"Problem with {actor.name}'s GoapThread! \nJob is {(job?.jobType.ToString() ?? "None")}\nTarget is {target.name}\n{e.Message}\n{e.StackTrace}");
        }
    }
    public override void FinishMultithread() {
        base.FinishMultithread();
        ReturnPlanFromGoapThread();
    }
    #endregion

    public void CreatePlan() {
        if(recalculationPlan != null) {
            RecalculatePlan();
        } else {
            CreateNewPlan();
        }
        GridMap.Instance.UpdateAwarenessInAllRegions();
    }
    private void CreateNewPlan() {
        log = $"-----------------RECEIVING NEW PLAN FROM OTHER THREAD OF {actor.name} WITH TARGET {target?.name}" ??
              $"None ({actor.currentRegion.name})-----------------------";
        if (goalType != INTERACTION_TYPE.NONE) {
            log += $"\nGOAL: {goalType}";
        } else {
            log += $"\nGOAL: {goalEffect}";
        }

        string planLog = string.Empty;
        GoapPlan plan = null;
        if (goalType != INTERACTION_TYPE.NONE) {
            //provided goal type
            GoapAction action = InteractionManager.Instance.goapActionData[goalType];
            int cost = 0;
            if (target.CanAdvertiseActionToActor(actor, action, job, job.otherData, ref cost)) {
                log += $"\n{target.name} Can advertise actions to {actor.name}";
                plan = actor.planner.PlanActions(target, action, isPersonalPlan, ref planLog, job);
            } else {
                log += $"\n{target.name} Cannot advertise actions to {actor.name}";
            }
        } else if (goalAction != null) {
            //provided goal action
            int cost = 0;
            if (target.CanAdvertiseActionToActor(actor, goalAction, job, job.otherData, ref cost)) {
                plan = actor.planner.PlanActions(target, goalAction, isPersonalPlan, ref planLog, job);
            }
        } else {
            //default
            plan = actor.planner.PlanActions(target, goalEffect, isPersonalPlan, ref planLog, job);
        }
        log += $"\nGOAP TREE LOG: {planLog}";
        if(plan != null) {
            log += "\n\nGENERATED PLAN: ";
            log += plan.LogPlan();
            createdPlan = plan;
        } else {
            log += "\n\nNO PLAN WAS GENERATED! End goap...";
        }
        //Dictionary<IPointOfInterest, List<GoapAction>> usableActions = new Dictionary<IPointOfInterest, List<GoapAction>>();
        //Dictionary<POINT_OF_INTEREST_TYPE, List<IPointOfInterest>> awareness = actor.awareness;
        //foreach (KeyValuePair<POINT_OF_INTEREST_TYPE, List<IPointOfInterest>> kvp in awareness) {
        //    if (kvp.Key == POINT_OF_INTEREST_TYPE.CHARACTER) {
        //        for (int i = 0; i < kvp.Value.Count; i++) {
        //            Character character = kvp.Value[i] as Character;
        //            if (character.isDead) {
        //                //kvp.Value.RemoveAt(i);
        //                //i--;
        //                if (allowDeadTargets) {
        //                    //if dead targets are allowed, advertise actions from dead targets
        //                    List<GoapAction> awarenessActions = character.AdvertiseActionsToActorFromDeadCharacter(actor, otherData);
        //                    if (awarenessActions != null && awarenessActions.Count > 0) {
        //                        usableActions.AddRange(awarenessActions);
        //                    }
        //                }
        //            } else {
        //                if (character.specificLocation == actor.specificLocation || character == actor) { //|| actor.IsPOIInCharacterAwarenessList(character, characterTargetsAwareness)
        //                    List<GoapAction> awarenessActions = character.AdvertiseActionsToActor(actor, otherData);
        //                    if (awarenessActions != null && awarenessActions.Count > 0) {
        //                        usableActions.AddRange(awarenessActions);
        //                    }
        //                }
        //            }
        //        }
        //    } else if (kvp.Key == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
        //        List<IPointOfInterest> shuffledPOIs = Utilities.Shuffle(kvp.Value);
        //        for (int i = 0; i < shuffledPOIs.Count; i++) {
        //            IPointOfInterest currAwareness = shuffledPOIs[i];
        //            if (currAwareness.gridTileLocation != null && currAwareness.gridTileLocation.structure != null) {
        //                List<GoapAction> awarenessActions = currAwareness.AdvertiseActionsToActor(actor, otherData);
        //                if (awarenessActions != null && awarenessActions.Count > 0) {
        //                    usableActions.AddRange(awarenessActions);
        //                }
        //            }
        //        }
        //    } else {
        //        for (int i = 0; i < kvp.Value.Count; i++) {
        //            List<GoapAction> awarenessActions = kvp.Value[i].AdvertiseActionsToActor(actor, otherData);
        //            if (awarenessActions != null && awarenessActions.Count > 0) {
        //                AddToListOfActionsToUsable(target, awarenessActions, usableActions);
        //            }
        //        }
        //    }
        //}

        ////other data handling
        //if (otherData != null) {
        //    log += "\nOTHER DATA: ";
        //    for (int i = 0; i < usableActions.Count; i++) {
        //        GoapAction currAction = usableActions[i];
        //        if (otherData.ContainsKey(currAction.goapType)) {
        //            log += currAction.goapName + " (" + currAction.poiTarget.name + ")";
        //            if (currAction.InitializeOtherData(otherData[currAction.goapType])) {
        //                log += ": Initialized!";
        //                //if other data was initialized, check if the action still meets the needed requirements
        //                if (!currAction.CanSatisfyRequirements() || !currAction.CanSatisfyRequirementOnBuildGoapTree()) {
        //                    //if it no longer does, add as invalid
        //                    log += " Removing!, ";
        //                    usableActions.RemoveAt(i);
        //                    i--;
        //                    continue;
        //                }
        //            }
        //            log += ", ";
        //        }
        //        if (otherData.ContainsKey(INTERACTION_TYPE.NONE)) {
        //            log += "Universal " + currAction.goapName + " (" + currAction.poiTarget.name + ")";
        //            if (currAction.InitializeOtherData(otherData[INTERACTION_TYPE.NONE])) {
        //                log += ": Initialized!";
        //                //if other data was initialized, check if the action still meets the needed requirements
        //                if (!currAction.CanSatisfyRequirements() || !currAction.CanSatisfyRequirementOnBuildGoapTree()) {
        //                    //if it no longer does, add as invalid
        //                    log += " Removing!";
        //                    usableActions.RemoveAt(i);
        //                    i--;
        //                }
        //            }
        //            log += ", ";
        //        }
        //    }
        //} else {
        //    log += "\nNO OTHER DATA";
        //}

        //string usableLog = "\nUSABLE ACTIONS: ";
        //List<GoapPlan> allPlans = new List<GoapPlan>();
        ////if (job != null && job.planConstructor != null) {
        ////    GoapPlan plan = job.planConstructor.Invoke();
        ////    if (plan != null) {
        ////        allPlans.Add(plan);
        ////        plan.SetListOfCharacterAwareness(characterTargetsAwareness);
        ////    }
        ////} else 
        //if (goalType != INTERACTION_TYPE.NONE) {
        //    //provided goal type
        //    if (target == null) {
        //        foreach (KeyValuePair<IPointOfInterest, List<GoapAction>> kvp in usableActions) {
        //            IPointOfInterest poiTarget = kvp.Key;
        //            for (int i = 0; i < kvp.Value.Count; i++) {
        //                GoapAction action = kvp.Value[i];
        //                if (i > 0) {
        //                    usableLog += ", ";
        //                }
        //                usableLog += action.goapName + " (" + poiTarget.name + ")";
        //                string planLog = string.Empty;
        //                if (action.goapType == goalType) {
        //                    GoapPlan plan = actor.planner.PlanActions(poiTarget, action, usableActions, category, isPersonalPlan, ref planLog, job);
        //                    if (plan != null) {
        //                        allPlans.Add(plan);
        //                        //plan.SetListOfCharacterAwareness(characterTargetsAwareness);
        //                    }
        //                }
        //                //log += planLog;
        //            }
        //        }
        //        //for (int i = 0; i < usableActions.Count; i++) {
        //        //    if (i > 0) {
        //        //        usableLog += ", ";
        //        //    }
        //        //    usableLog += usableActions[i].goapName + " (" + usableActions[i].poiTarget.name + ")";
        //        //    string planLog = string.Empty;
        //        //    if (usableActions[i].goapType == goalType) {
        //        //        GoapPlan plan = actor.planner.PlanActions(usableActions[i].poiTarget, usableActions[i], usableActions, category, isPersonalPlan, ref planLog, job);
        //        //        if (plan != null) {
        //        //            allPlans.Add(plan);
        //        //            //plan.SetListOfCharacterAwareness(characterTargetsAwareness);
        //        //        }
        //        //    }
        //        //    //log += planLog;
        //        //}
        //    } else {
        //        foreach (KeyValuePair<IPointOfInterest, List<GoapAction>> kvp in usableActions) {
        //            IPointOfInterest poiTarget = kvp.Key;
        //            if(target == poiTarget) {
        //                for (int i = 0; i < kvp.Value.Count; i++) {
        //                    GoapAction action = kvp.Value[i];
        //                    if (i > 0) {
        //                        usableLog += ", ";
        //                    }
        //                    usableLog += action.goapName + " (" + poiTarget.name + ")";
        //                    string planLog = string.Empty;
        //                    if (action.goapType == goalType) {
        //                        GoapPlan plan = actor.planner.PlanActions(target, action, usableActions, category, isPersonalPlan, ref planLog, job);
        //                        if (plan != null) {
        //                            allPlans.Add(plan);
        //                            //plan.SetListOfCharacterAwareness(characterTargetsAwareness);
        //                        }
        //                    }
        //                    //log += planLog;
        //                }
        //            }
        //        }
        //        //for (int i = 0; i < usableActions.Count; i++) {
        //        //    if (i > 0) {
        //        //        usableLog += ", ";
        //        //    }
        //        //    usableLog += usableActions[i].goapName + " (" + usableActions[i].poiTarget.name + ")";
        //        //    if (usableActions[i].goapType == goalType && usableActions[i].poiTarget == target) {
        //        //        string planLog = string.Empty;
        //        //        GoapPlan plan = actor.planner.PlanActions(usableActions[i].poiTarget, usableActions[i], usableActions, category, isPersonalPlan, ref planLog, job);
        //        //        if (plan != null) {
        //        //            allPlans.Add(plan);
        //        //            //plan.SetListOfCharacterAwareness(characterTargetsAwareness);
        //        //        }
        //        //        //log += planLog;
        //        //    }
        //        //}
        //    }
        //} else if (goalAction != null) {
        //    //provided goal action
        //    foreach (KeyValuePair<IPointOfInterest, List<GoapAction>> kvp in usableActions) { //For testing only, remove this
        //        IPointOfInterest poiTarget = kvp.Key;
        //        for (int i = 0; i < kvp.Value.Count; i++) {
        //            GoapAction action = kvp.Value[i];
        //            if (i > 0) {
        //                usableLog += ", ";

        //            }
        //            usableLog += action.goapName + " (" + poiTarget.name + ")";
        //        }
        //    }
        //    //for (int i = 0; i < usableActions.Count; i++) {
        //    //    if (i > 0) {
        //    //        usableLog += ", ";
        //    //    }
        //    //    usableLog += usableActions[i].goapName + " (" + usableActions[i].poiTarget.name + ")";
        //    //}
        //    string planLog = string.Empty;
        //    GoapPlan plan = actor.planner.PlanActions(target, goalAction, usableActions, category, isPersonalPlan, ref planLog, job);
        //    if (plan != null) {
        //        allPlans.Add(plan);
        //        //plan.SetListOfCharacterAwareness(characterTargetsAwareness);
        //    }
        //    //log += planLog;
        //} else {
        //    //default
        //    foreach (KeyValuePair<IPointOfInterest, List<GoapAction>> kvp in usableActions) {
        //        IPointOfInterest poiTarget = kvp.Key;
        //        for (int i = 0; i < kvp.Value.Count; i++) {
        //            GoapAction action = kvp.Value[i];
        //            if (i > 0) {
        //                usableLog += ", ";
        //            }
        //            usableLog += action.goapName + " (" + poiTarget.name + ")";
        //            if (action.WillEffectsSatisfyPrecondition(goalEffect)) {
        //                usableLog += "(GOAL EFFECT SATISFIED!)";
        //                string planLog = string.Empty;
        //                GoapPlan plan = actor.planner.PlanActions(target, action, usableActions, category, isPersonalPlan, ref planLog, job);
        //                if (plan != null) {
        //                    allPlans.Add(plan);
        //                    //plan.SetListOfCharacterAwareness(characterTargetsAwareness);
        //                }
        //            }
        //        }
        //    }
        //    //for (int i = 0; i < usableActions.Count; i++) {
        //    //    if (i > 0) {
        //    //        usableLog += ", ";
        //    //    }
        //    //    usableLog += usableActions[i].goapName + " (" + usableActions[i].poiTarget.name + ")";
        //    //    if (usableActions[i].WillEffectsSatisfyPrecondition(goalEffects)) {
        //    //        usableLog += "(GOAL EFFECT SATISFIED!)";
        //    //        if (job != null && job.forcedActions.Count > 0) {
        //    //            bool satisfiedForcedActions = true;
        //    //            ForcedActionsComparer comparer = new ForcedActionsComparer();
        //    //            foreach (KeyValuePair<GoapEffect, INTERACTION_TYPE> kvp in job.forcedActions) {
        //    //                if (comparer.Equals(kvp.Key, goalEffects)) {
        //    //                    if (kvp.Value != usableActions[i].goapType) {
        //    //                        satisfiedForcedActions = false;
        //    //                        break;
        //    //                    }
        //    //                }
        //    //            }
        //    //            if (satisfiedForcedActions) {
        //    //                string planLog = string.Empty;
        //    //                GoapPlan plan = actor.planner.PlanActions(target, usableActions[i], usableActions, category, isPersonalPlan, ref planLog, job);
        //    //                if (plan != null) {
        //    //                    allPlans.Add(plan);
        //    //                    plan.SetListOfCharacterAwareness(characterTargetsAwareness);
        //    //                }
        //    //                //log += planLog;
        //    //            }
        //    //        } else {
        //    //            string planLog = string.Empty;
        //    //            GoapPlan plan = actor.planner.PlanActions(target, usableActions[i], usableActions, category, isPersonalPlan, ref planLog, job);
        //    //            if (plan != null) {
        //    //                allPlans.Add(plan);
        //    //                plan.SetListOfCharacterAwareness(characterTargetsAwareness);
        //    //            }
        //    //            //log += planLog;
        //    //            //Debug.Log(planLog);
        //    //        }
        //    //    }
        //    //}
        //}

        //log += "\nALL GENERATED PLANS: ";
        //if (allPlans.Count > 0) {
        //    GoapPlan shortestPathToGoal = null;
        //    for (int i = 0; i < allPlans.Count; i++) {
        //        log += "\n" + (i + 1) + ". (" + allPlans[i].startingNode.runningCost + ") " + allPlans[i].startingNode.action.goapName + " - " + allPlans[i].startingNode.action.poiTarget.name + "[number of nodes = " + allPlans[i].GetNumOfNodes() + "]";
        //        //for (int j = 0; j < allPlans[i].allNodes.Count; j++) {
        //        //    if (j > 0) {
        //        //        log += ", ";
        //        //    }
        //        //    log += allPlans[i].allNodes[j].action.goapName + " - " + allPlans[i].allNodes[j].action.poiTarget.name;
        //        //}
        //        if (shortestPathToGoal == null) {
        //            shortestPathToGoal = allPlans[i];
        //        } else {
        //            if (allPlans[i].startingNode.runningCost < shortestPathToGoal.startingNode.runningCost) {
        //                shortestPathToGoal = allPlans[i];
        //            }
        //        }
        //    }
        //    //shortestPathToGoal.SetListOfCharacterAwareness(characterTargetsAwareness);
        //    shortestPathToGoal.ConstructAllNodes();
        //    log += shortestPathToGoal.LogPlan();
        //    createdPlan = shortestPathToGoal;
        //} else {
        //    log += "\nNO PLAN WAS GENERATED! End goap...";
        //}
        //log += usableLog;
    }
    private void RecalculatePlan() {
        log =
            $"-----------------RECALCULATING PLAN OF {actor.name} WITH TARGET {recalculationPlan.target.name} ({actor.currentRegion.name})-----------------------";
        if (recalculationPlan.isEnd) {
            log += "\nPlan has already ended! Cannot recalculate!";
            return;
        }
        log +=
            $"\nGOAL ACTION: {recalculationPlan.endNode.singleNode.action.goapName} - {recalculationPlan.target.name}";
        string planLog = string.Empty;
        bool success = actor.planner.RecalculatePathForPlan(recalculationPlan, job, ref planLog);
        log += $"\nGOAP TREE LOG: {planLog}";
        if (success) {
            log += "\nGENERATED PLAN: ";
            log += recalculationPlan.LogPlan();
            createdPlan = recalculationPlan;
        } else {
            log += "\nFAILED TO RECALCULATE PLAN!";
        }

        //List<GoapAction> usableActions = new List<GoapAction>();
        //List<INTERACTION_TYPE> actorAllowedActions = RaceManager.Instance.GetNPCInteractionsOfCharacter(actor);
        //if(recalculationPlan.job != null) {
        //    otherData = recalculationPlan.job.otherData;
        //}
        //foreach (KeyValuePair<POINT_OF_INTEREST_TYPE, List<IPointOfInterest>> kvp in actor.awareness) {
        //    if (kvp.Key == POINT_OF_INTEREST_TYPE.CHARACTER) {
        //        for (int i = 0; i < kvp.Value.Count; i++) {
        //            Character character = kvp.Value[i] as Character;
        //            if (character.isDead) {
        //                //kvp.Value.RemoveAt(i);
        //                //i--;
        //                if (recalculationPlan.job != null && recalculationPlan.job.allowDeadTargets) {
        //                    //if dead targets are allowed, advertise actions from dead targets
        //                    List<GoapAction> awarenessActions = character.AdvertiseActionsToActorFromDeadCharacter(actor, otherData);
        //                    if (awarenessActions != null && awarenessActions.Count > 0) {
        //                        usableActions.AddRange(awarenessActions);
        //                    }
        //                }
        //            } else {
        //                if (character.specificLocation == actor.specificLocation || character == actor || actor.IsPOIInCharacterAwarenessList(character, characterTargetsAwareness)) {
        //                    List<GoapAction> awarenessActions = character.AdvertiseActionsToActor(actor, otherData);
        //                    if (awarenessActions != null && awarenessActions.Count > 0) {
        //                        usableActions.AddRange(awarenessActions);
        //                    }
        //                }
        //            }
        //        }
        //    } else if (kvp.Key == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
        //        List<IPointOfInterest> shuffledPOIs = Utilities.Shuffle(kvp.Value);
        //        for (int i = 0; i < shuffledPOIs.Count; i++) {
        //            IPointOfInterest currAwareness = shuffledPOIs[i];
        //            if (currAwareness.gridTileLocation != null && currAwareness.gridTileLocation.structure != null) {
        //                List<GoapAction> awarenessActions = currAwareness.AdvertiseActionsToActor(actor, otherData);
        //                if (awarenessActions != null && awarenessActions.Count > 0) {
        //                    usableActions.AddRange(awarenessActions);
        //                }
        //            }
        //        }
        //    } else {
        //        for (int i = 0; i < kvp.Value.Count; i++) {
        //            List<GoapAction> awarenessActions = kvp.Value[i].AdvertiseActionsToActor(actor, otherData);
        //            if (awarenessActions != null && awarenessActions.Count > 0) {
        //                for (int j = 0; j < awarenessActions.Count; j++) {
        //                    usableActions.AddRange(awarenessActions);
        //                }
        //            }
        //        }
        //    }
        //}

        //log += "\nUSABLE ACTIONS: ";
        //if (usableActions.Count > 0) {
        //    for (int i = 0; i < usableActions.Count; i++) {
        //        if (i > 0) {
        //            log += ", ";
        //        }
        //        log += usableActions[i].goapName + " (" + usableActions[i].poiTarget.name + ")";
        //    }
        //    bool success = actor.planner.RecalculatePathForPlan(recalculationPlan, usableActions);
        //    if (success) {
        //        recalculationPlan.ConstructAllNodes();
        //        log += "\nSUCCESSFULLY RECALCULATED PLAN!";
        //        log += recalculationPlan.LogPlan();
        //        createdPlan = recalculationPlan;
        //    } else {
        //        log += "\nFAILED TO RECALCULATE PLAN!";
        //    }
        //} else {
        //    log += "\nNO USABLE ACTIONS! FAILED TO RECALCULATE PLAN!";
        //}
    }
    //private GoapAction CreateGoapActionAndInitializeOtherData(INTERACTION_TYPE type, IPointOfInterest poiTarget) {
    //    object[] data = null;
    //    if (otherData != null) {
    //        if (otherData.ContainsKey(type)) {
    //            data = otherData[type];
    //        } else if (otherData.ContainsKey(INTERACTION_TYPE.NONE)) {
    //            data = otherData[INTERACTION_TYPE.NONE];
    //        }
    //    }

    //    GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(type, actor, poiTarget);
    //    if(goapAction != null) {
    //        if(data != null) {
    //            goapAction.InitializeOtherData(data);
    //        }
    //        return goapAction;
    //    } else {
    //        throw new System.Exception("Goap action " + type.ToString() + " is null!");
    //    }
    //}

    public void ReturnPlanFromGoapThread() {
        actor.planner.ReceivePlanFromGoapThread(this);
    }
}
