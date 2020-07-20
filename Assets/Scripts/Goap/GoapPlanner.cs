using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GoapPlanner {
    public Character owner { get; private set; }
    public GOAP_PLANNING_STATUS status { get; private set; }

    public GoapPlanner(Character owner) {
        this.owner = owner;
    }
    public void StartGOAP(GoapEffect goal, IPointOfInterest target, GoapPlanJob job, bool isPersonalPlan = true) {
        if (status == GOAP_PLANNING_STATUS.RUNNING) {
            //If already processing, do not throw another process to the multithread
            return;
        }
        //List<IPointOfInterest> characterTargetsAwareness = new List<IPointOfInterest>();
        //if (target.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
        //    actor.AddAwareness(target);
        //    //characterTargetsAwareness.Add(target);
        //}

        //if (otherCharactePOIs != null) {
        //    for (int i = 0; i < otherCharactePOIs.Count; i++) {
        //        AddAwareness(otherCharactePOIs[i]);
        //        characterTargetsAwareness.Add(otherCharactePOIs[i]);
        //    }
        //}
        if (job != null) {
            job.SetAssignedPlan(null);
        }
        status = GOAP_PLANNING_STATUS.RUNNING;

        //_numOfWaitingForGoapThread++;
        //Debug.LogWarning(name + " sent a plan to other thread(" + _numOfWaitingForGoapThread + ")");
        Character actor = owner;
        MultiThreadPool.Instance.AddToThreadPool(new GoapThread(actor, target, goal, isPersonalPlan, job));
    }
    public void StartGOAP(GoapAction goal, IPointOfInterest target, GoapPlanJob job, bool isPersonalPlan = true) {
        if (status == GOAP_PLANNING_STATUS.RUNNING) {
            //If already processing, do not throw another process to the multithread
            return;
        }
        //List<IPointOfInterest> characterTargetsAwareness = new List<IPointOfInterest>();
        //if (target.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
        //    actor.AddAwareness(target);
        //    //characterTargetsAwareness.Add(target);
        //}

        //if (otherCharactePOIs != null) {
        //    for (int i = 0; i < otherCharactePOIs.Count; i++) {
        //        AddAwareness(otherCharactePOIs[i]);
        //        characterTargetsAwareness.Add(otherCharactePOIs[i]);
        //    }
        //}
        if (job != null) {
            job.SetAssignedPlan(null);
        }
        //_numOfWaitingForGoapThread++;
        status = GOAP_PLANNING_STATUS.RUNNING;
        MultiThreadPool.Instance.AddToThreadPool(new GoapThread(owner, target, goal, isPersonalPlan, job));
    }
    public void StartGOAP(INTERACTION_TYPE goalType, IPointOfInterest target, GoapPlanJob job, bool isPersonalPlan = true) {
        if (status == GOAP_PLANNING_STATUS.RUNNING) {
            //If already processing, do not throw another process to the multithread
            return;
        }
        //List<IPointOfInterest> characterTargetsAwareness = new List<IPointOfInterest>();
        //if (target != null && target.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
        //    actor.AddAwareness(target);
        //    //characterTargetsAwareness.Add(target);
        //}
        if (job != null) {
            job.SetAssignedPlan(null);
        }
        //_numOfWaitingForGoapThread++;
        status = GOAP_PLANNING_STATUS.RUNNING;
        MultiThreadPool.Instance.AddToThreadPool(new GoapThread(owner, goalType, target, isPersonalPlan, job));
    }
    public void RecalculateJob(GoapPlanJob job) {
        if (status == GOAP_PLANNING_STATUS.RUNNING) {
            //If already processing, do not throw another process to the multithread
            return;
        }
        if (job.assignedPlan != null) {
            job.assignedPlan.SetIsBeingRecalculated(true);
            status = GOAP_PLANNING_STATUS.RUNNING;
            MultiThreadPool.Instance.AddToThreadPool(new GoapThread(owner, job.assignedPlan, job));
        }
    }
    public void ReceivePlanFromGoapThread(GoapThread goapThread) {
        if (owner.isDead || !owner.marker) {
            status = GOAP_PLANNING_STATUS.NONE;
            return;
        }
        if (goapThread.recalculationPlan != null && goapThread.recalculationPlan.isEnd) {
            status = GOAP_PLANNING_STATUS.NONE;
            return;
        }

        //status = GOAP_PLANNING_STATUS.PROCESSING_RESULT;
        status = GOAP_PLANNING_STATUS.NONE;

        owner.ExecutePendingActionsAfterMultithread();
        string additionalLog = string.Empty;
        if (goapThread.job.originalOwner == null) {
            //This means that the job is already in the object pool, meaning that the received plan for the job is no longer applicable since the job is already deleted/cancelled
            additionalLog += "\nJOB NO LONGER APPLICABLE, DISCARD PLAN IF THERE'S ANY";
            owner.logComponent.PrintLogIfActive(goapThread.log + additionalLog);
            return;
        }
        owner.logComponent.PrintLogIfActive(goapThread.log + additionalLog);
        if (goapThread.createdPlan != null) {
            if (goapThread.recalculationPlan != null) {
                //This means that the created plan is a recalculated plan
                goapThread.createdPlan.SetIsBeingRecalculated(false);
            }
            if (!owner.canPerform) {
                int canPerformValue = owner.GetCanPerformValue();
                if(canPerformValue == -1 && owner.traitContainer.HasTrait("Paralyzed")) {
                    //If the owner is paralyzed and the only reason he cannot perform is because of that paralyzed, the plan must not be scrapped
                } else {
                    owner.logComponent.PrintLogIfActive($"{owner.name} is scrapping plan since {owner.name} cannot perform. {goapThread.job.name} is the job.");
                    goapThread.job.CancelJob(false);
                    return;
                }

            }
            int jobIndex = owner.jobQueue.GetJobQueueIndex(goapThread.job);
            if(jobIndex != -1) {
                //Only set assigned plan if job is still in character job queue because if not, it means that the job is no longer taken
                goapThread.job.SetAssignedPlan(goapThread.createdPlan);
                if (jobIndex != 0) {
                    //If the job of the receive plan is no longer the top priority, process the top most job because it means that while the goap planner is running, the top most priority has been replaced
                    //This means that the top most priority was not processed since the goap planner is still running
                    owner.jobQueue.ProcessFirstJobInQueue();
                }
            }
        } else {
            JOB_TYPE jobType = goapThread.job.jobType;
            if (jobType.IsNeedsTypeJob()) {
                //If unable to do a Need while in a Trapped Structure, remove Trap Structure.
                if (owner.trapStructure.IsTrapped()) {
                    owner.trapStructure.SetStructureAndDuration(null, 0);
                }
            }
            if (goapThread.recalculationPlan == null) {
                //This means that the planner cannot create a new plan
                bool logCancelJobNoPlan = !(jobType == JOB_TYPE.DOUSE_FIRE && goapThread.job.targetPOI.gridTileLocation == null);
                if (logCancelJobNoPlan && !CharacterManager.Instance.lessenCharacterLogs) {
                    Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "cancel_job_no_plan");
                    log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    log.AddToFillers(null, goapThread.job.GetJobDetailString(), LOG_IDENTIFIER.STRING_1);
                    owner.logComponent.RegisterLog(log);
                }
                if (goapThread.job.originalOwner.ownerType != JOB_OWNER.CHARACTER) {
                    goapThread.job.AddBlacklistedCharacter(owner);
                }
            }
            //Every time no plan is generated for the job, remove carried poi because this means that the carried poi is part of that job that has no plan, so the character needs to let go of the poi now
            if (owner.carryComponent.IsNotBeingCarried()) {
                if (owner.carryComponent.isCarryingAnyPOI) {
                    IPointOfInterest carriedPOI = owner.carryComponent.carriedPOI;
                    string log = $"Dropping carried POI: {carriedPOI.name} because no plan was generated.";
                    log += "\nAdditional Info:";
                    if(carriedPOI is ResourcePile) {
                        ResourcePile pile = carriedPOI as ResourcePile;
                        log += $"\n-Stored resources on drop: {pile.resourceInPile} {pile.providedResource}";
                    }else if (carriedPOI is Table) {
                        Table table = carriedPOI as Table;
                        log += $"\n-Stored resources on drop: {table.food} Food.";
                    }
                    owner.logComponent.PrintLogIfActive(log);
                }
                owner.UncarryPOI();
            }
            goapThread.job.CancelJob(false);

            if(jobType == JOB_TYPE.FULLNESS_RECOVERY_URGENT || jobType == JOB_TYPE.FULLNESS_RECOVERY_NORMAL) {
                //Special case for when a character cannot do hunger recovery, he/she must produce food instead
                owner.jobComponent.CreateProduceFoodJob();
            }
        }
        //if (goapThread.createdPlan != null) {
        //    if (goapThread.recalculationPlan == null) {
        //        int count = traitContainer.GetAllTraitsOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE).Count;
        //        if (count >= 2 || (count == 1 && traitContainer.GetNormalTrait<Trait>("Paralyzed") == null)) {
        //            PrintLogIfActive(GameManager.Instance.TodayLogString() + name + " is scrapping plan since " + name + " has a negative disabler trait. " + goapThread.job.name + " is the job.");
        //            if (goapThread.job != null) {
        //                if (goapThread.job.assignedCharacter == this) {
        //                    goapThread.job.SetAssignedCharacter(null);
        //                    goapThread.job.SetAssignedPlan(null);
        //                    //Only remove job in queue if it is a personal job, cause if it is not, it must be returned to the queue
        //                    if (!goapThread.job.currentOwner.isAreaOrQuestJobQueue) {
        //                        goapThread.job.currentOwner.RemoveJobInQueue(goapThread.job);
        //                    }
        //                }
        //            }
        //            return;
        //        }
        //        if (goapThread.job != null) {
        //            if (goapThread.job.assignedCharacter != this) {
        //                PrintLogIfActive(GameManager.Instance.TodayLogString() + name + " is scrapping plan since " + goapThread.job.name + " job's assigned character is no longer him/her. New assigned character is " + (goapThread.job.assignedCharacter != null ? goapThread.job.assignedCharacter.name : "None"));
        //                return;
        //            }
        //            goapThread.job.SetAssignedPlan(goapThread.createdPlan);

        //            //If the created plan contains a carry component, that plan cannot be overridden
        //            //for (int i = 0; i < goapThread.createdPlan.allNodes.Count; i++) {
        //            //    if (goapThread.createdPlan.allNodes[i].action.goapType == INTERACTION_TYPE.CARRY_CHARACTER
        //            //        || goapThread.createdPlan.allNodes[i].action.goapType == INTERACTION_TYPE.CARRY_CORPSE
        //            //        || goapThread.createdPlan.allNodes[i].action.goapType == INTERACTION_TYPE.INVITE_TO_MAKE_LOVE) {
        //            //        goapThread.createdPlan.job.SetCannotOverrideJob(true);
        //            //        break;
        //            //    }
        //            //}
        //        }
        //        AddPlan(goapThread.createdPlan);
        //        if (CanCurrentJobBeOverriddenByJob(goapThread.job)) {
        //            //AddPlan(goapThread.createdPlan, true);

        //            if (stateComponent.currentState != null) {
        //                stateComponent.currentState.OnExitThisState();
        //                //This call is doubled so that it will also exit the previous major state if there's any
        //                if (stateComponent.currentState != null) {
        //                    stateComponent.currentState.OnExitThisState();
        //                }
        //                ////- berserk, flee, and engage are the highest priority, they cannot be overridden. character must finish the state before doing anything else.
        //                //if (stateComponent.currentState.characterState != CHARACTER_STATE.ENGAGE && stateComponent.currentState.characterState != CHARACTER_STATE.FLEE && stateComponent.currentState.characterState != CHARACTER_STATE.BERSERKED) {
        //                //    stateComponent.currentState.OnExitThisState();
        //                //}
        //            }
        //            //else if (stateComponent.currentState != null) {
        //            //    stateComponent.SetStateToDo(null);
        //            //} 
        //            else {
        //                if (currentParty.icon.isTravelling) {
        //                    if (currentParty.icon.travelLine == null) {
        //                        marker.StopMovement();
        //                    } else {
        //                        currentParty.icon.SetOnArriveAction(() => OnArriveAtAreaStopMovement());
        //                    }
        //                }
        //                AdjustIsWaitingForInteraction(1);
        //                StopCurrentAction(false, "Have something important to do");
        //                AdjustIsWaitingForInteraction(-1);
        //            }
        //            //return;
        //        }
        //    } else {
        //        //Receive plan recalculation
        //        goapThread.createdPlan.SetIsBeingRecalculated(false);
        //        int count = traitContainer.GetAllTraitsOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE).Count;
        //        if (count >= 2 || (count == 1 && traitContainer.GetNormalTrait<Trait>("Paralyzed") == null)) {
        //            PrintLogIfActive(GameManager.Instance.TodayLogString() + name + " is scrapping recalculated plan since " + name + " has a negative disabler trait. " + goapThread.job.name + " is the job.");
        //            DropPlan(goapThread.recalculationPlan, true);
        //            return;
        //        }
        //        if (goapThread.createdPlan.job != null) {
        //            if (goapThread.createdPlan.job.assignedCharacter != this) {
        //                PrintLogIfActive(GameManager.Instance.TodayLogString() + name + " is scrapping recalculated plan since " + goapThread.createdPlan.job.name + " job's assigned character is no longer him/her. New assigned character is " + (goapThread.createdPlan.job.assignedCharacter != null ? goapThread.createdPlan.job.assignedCharacter.name : "None"));
        //                DropPlan(goapThread.recalculationPlan, true);
        //                return;
        //            }
        //        }
        //    }
        //} else {
        //    if (goapThread.job != null && goapThread.job.jobType.IsNeedsTypeJob()) {
        //        //If unable to do a Need while in a Trapped Structure, remove Trap Structure.
        //        if (trapStructure.structure != null) {
        //            trapStructure.SetStructureAndDuration(null, 0);
        //        }
        //    }
        //    if (goapThread.recalculationPlan != null) {
        //        //This means that the recalculation has failed
        //        DropPlan(goapThread.recalculationPlan);
        //    } else {
        //        if (goapThread.job != null) {
        //            goapThread.job.SetAssignedCharacter(null);
        //            if (!goapThread.job.currentOwner.isAreaOrQuestJobQueue) {
        //                //If no plan was generated, automatically remove job from queue if it is a personal job
        //                goapThread.job.currentOwner.RemoveJobInQueue(goapThread.job);
        //                if (goapThread.job.jobType == JOB_TYPE.REMOVE_FIRE) {
        //                    if (goapThread.job.targetPOI.gridTileLocation != null) { //this happens because sometimes the target that was burning is now put out.
        //                        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "cancel_job_no_plan");
        //                        log.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        //                        log.AddToFillers(null, goapThread.job.GetJobDetailString(), LOG_IDENTIFIER.STRING_1);
        //                        RegisterLogAndShowNotifToThisCharacterOnly(log);
        //                    }
        //                } else {
        //                    Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "cancel_job_no_plan");
        //                    log.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        //                    log.AddToFillers(null, goapThread.job.GetJobDetailString(), LOG_IDENTIFIER.STRING_1);
        //                    RegisterLogAndShowNotifToThisCharacterOnly(log);
        //                }
        //                //if (goapThread.job.canBeDoneInLocation) {
        //                //    //If a personal job can be done in location job queue, add it once no plan is generated
        //                //    specificLocation.jobQueue.AddJobInQueue(goapThread.job);
        //                //}
        //            } else {
        //                goapThread.job.AddBlacklistedCharacter(this);
        //            }
        //        }
        //    }
        //}
    }
    //public GoapPlan PlanActions(IPointOfInterest target, GoapAction goalAction, List<GoapAction> usableActions, GOAP_CATEGORY category, bool isPersonalPlan, ref string log, GoapPlanJob job = null) {
    //    //List of all starting nodes that can do the goal
    //    List<GoapNode> startingNodes = new List<GoapNode>();

    //    GoapNode goalNode = ObjectPoolManager.Instance.CreateNewGoapPlanJob(null, goalAction.cost, goalAction);
    //    bool success = BuildGoapTree(goalNode, startingNodes, usableActions, ref log, job);
    //    if (!success) {
    //        return null;
    //    }

    //    GoapNode cheapestStartingNode = null;
    //    for (int i = 0; i < startingNodes.Count; i++) {
    //        if(cheapestStartingNode == null) {
    //            cheapestStartingNode = startingNodes[i];
    //        } else {
    //            if(startingNodes[i].cost < cheapestStartingNode.cost) {
    //                cheapestStartingNode = startingNodes[i];
    //            }
    //        }
    //    }
    //    GOAP_EFFECT_CONDITION[] goalEffects = new GOAP_EFFECT_CONDITION[goalAction.expectedEffects.Count];
    //    for (int i = 0; i < goalAction.expectedEffects.Count; i++) {
    //        goalEffects[i] = goalAction.expectedEffects[i].conditionType;
    //    }
    //    GoapPlan plan = new GoapPlan(cheapestStartingNode, goalEffects, category, isPersonalPlan);
    //    return plan;
    //}
    public GoapPlan PlanActions(IPointOfInterest target, GoapEffect goalEffect, bool isPersonalPlan, ref string log, GoapPlanJob job) {
        //Cache all needed data
        Dictionary<POINT_OF_INTEREST_TYPE, List<GoapAction>> allGoapActionAdvertisements = InteractionManager.Instance.allGoapActionAdvertisements;
        Dictionary<POINT_OF_INTEREST_TYPE, List<IPointOfInterest>> awareness = owner.currentRegion.awareness;
        //Dictionary<POINT_OF_INTEREST_TYPE, List<IPointOfInterest>> awareness = actor.gridTileLocation.parentMap.location.coreTile.region.awareness;
        Dictionary<INTERACTION_TYPE, object[]> otherData = job.otherData;
        List<GoapNode> rawPlan = null; //The plan that will be created will be stored here
        owner.logComponent.ClearCostLog();
        owner.logComponent.AppendCostLog($"BASE COSTS OF {owner.name} ACTIONS ON {job.name} PLANNING");
        log += $"\n--Searching plan for target: {target.name}";
        if (goalEffect.target == GOAP_EFFECT_TARGET.TARGET) {
            //if precondition's target is TARGET, then the one who will advertise must be the target only
            int cost = 0;
            //Get action with the lowest cost that the actor can do that satisfies the goal effect
            if (target == job.targetPOI || target.IsStillConsideredPartOfAwarenessByCharacter(owner)) { //POI must either be the job's target or the actor is still aware of it
                GoapAction currentAction = target.AdvertiseActionsToActor(owner, goalEffect, job, otherData, ref cost, ref log);
                if (currentAction != null) {
                    //If an action is found, make it the goal node and start building the plan
                    GoapNode goalNode = ObjectPoolManager.Instance.CreateNewGoapPlanJob(cost, 0, currentAction, target);
                    rawPlan = new List<GoapNode>();
                    BuildGoapTree(goalNode, owner, job, rawPlan, allGoapActionAdvertisements, awareness, ref log); //, ref log
                }
            }
        } else if (goalEffect.target == GOAP_EFFECT_TARGET.ACTOR) {
            //If precondition's target is ACTOR, get the lowest action that the actor can do that will satisfy the goal effect
            GoapAction lowestCostAction = null;
            IPointOfInterest lowestCostTarget = null;
            int lowestCost = 0;
            log += $"\n--Choices for {goalEffect}";
            log += "\n--";
            foreach (KeyValuePair<POINT_OF_INTEREST_TYPE, List<GoapAction>> kvp in allGoapActionAdvertisements) {
                //First loop through all actions that can be advertised (it is grouped by the poi type that can advertise the actions)
                if (awareness.ContainsKey(kvp.Key)) {
                    List<GoapAction> actionList = kvp.Value;
                    for (int j = 0; j < actionList.Count; j++) {
                        GoapAction currentAction = actionList[j];
                        if (currentAction.WillEffectsMatchPreconditionTypeAndTarget(goalEffect)) {
                            //get other data for current action.
                            object[] otherActionData = null;
                            if (job.otherData != null) {
                                if (job.otherData.ContainsKey(currentAction.goapType)) {
                                    otherActionData = job.otherData[currentAction.goapType];
                                } else if (job.otherData.ContainsKey(INTERACTION_TYPE.NONE)) {
                                    otherActionData = job.otherData[INTERACTION_TYPE.NONE];
                                }
                            }
                            //Further optimize this by creating a list of all poi that currently advertises this action, and loop that
                            bool isJobTargetEvaluated = false;
                            List<IPointOfInterest> poisThatAdvertisesCurrentAction = awareness[kvp.Key];
                            for (int k = 0; k < poisThatAdvertisesCurrentAction.Count; k++) {
                                IPointOfInterest poiTarget = poisThatAdvertisesCurrentAction[k];
                                if(poiTarget == target) { isJobTargetEvaluated = true; }
                                if (poiTarget == job.targetPOI || poiTarget.IsStillConsideredPartOfAwarenessByCharacter(owner)) { //POI must either be the job's target or the actor is still aware of it
                                    int cost = 0;
                                    bool canDoAction = poiTarget.CanAdvertiseActionToActor(owner, currentAction, job, otherData, ref cost)
                                        && currentAction.WillEffectsSatisfyPrecondition(goalEffect, owner, poiTarget, otherActionData);
                                    if (canDoAction) {
                                        log += $"({cost}){currentAction.goapName}-{poiTarget.nameWithID}, ";
                                        if (lowestCostAction == null || cost < lowestCost) {
                                            lowestCostAction = currentAction;
                                            lowestCostTarget = poiTarget;
                                            lowestCost = cost;
                                        }
                                    }
                                }
                            }
                            if(!isJobTargetEvaluated && kvp.Key == target.poiType) {
                                IPointOfInterest poiTarget = target;
                                if (poiTarget == job.targetPOI || poiTarget.IsStillConsideredPartOfAwarenessByCharacter(owner)) { //POI must either be the job's target or the actor is still aware of it
                                    int cost = 0;
                                    bool canDoAction = poiTarget.CanAdvertiseActionToActor(owner, currentAction, job, otherData, ref cost)
                                        && currentAction.WillEffectsSatisfyPrecondition(goalEffect, owner, poiTarget, otherActionData);
                                    if (canDoAction) {
                                        log += $"({cost}){currentAction.goapName}-{poiTarget.nameWithID}, ";
                                        if (lowestCostAction == null || cost < lowestCost) {
                                            lowestCostAction = currentAction;
                                            lowestCostTarget = poiTarget;
                                            lowestCost = cost;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (lowestCostAction != null) {
                GoapNode leafNode = ObjectPoolManager.Instance.CreateNewGoapPlanJob(lowestCost, 0, lowestCostAction, lowestCostTarget);
                rawPlan = new List<GoapNode>();
                BuildGoapTree(leafNode, owner, job, rawPlan, allGoapActionAdvertisements, awareness, ref log); //, ref log
            }
        }
        if(rawPlan != null && rawPlan.Count > 0) {
            //has a created plan
            string rawPlanSummary = $"Generated raw plan for job { job.name } { owner.name }";
            for (int i = 0; i < rawPlan.Count; i++) {
                GoapNode currNode = rawPlan[i];
                rawPlanSummary += $"\n - {currNode.action.goapName }";
            }
            Debug.Log(rawPlanSummary);
            owner.logComponent.PrintCostLog();
            List<JobNode> actualNodes = TransformRawPlanToActualNodes(rawPlan, otherData);
            GoapPlan plan = new GoapPlan(actualNodes, target, isPersonalPlan);
            return plan;
        }
        return null;
    }
    public GoapPlan PlanActions(IPointOfInterest target, GoapAction goalAction, bool isPersonalPlan, ref string log, GoapPlanJob job) {
        Dictionary<POINT_OF_INTEREST_TYPE, List<GoapAction>> allGoapActionAdvertisements = InteractionManager.Instance.allGoapActionAdvertisements;
        Dictionary<POINT_OF_INTEREST_TYPE, List<IPointOfInterest>> awareness = owner.currentRegion.awareness;
        Dictionary<INTERACTION_TYPE, object[]> otherData = job.otherData;
        List<GoapNode> rawPlan = new List<GoapNode>();
        owner.logComponent.ClearCostLog();
        owner.logComponent.AppendCostLog($"BASE COSTS OF {owner.name} ACTIONS ON {job.name} PLANNING");
        if(target != job.targetPOI && !target.IsStillConsideredPartOfAwarenessByCharacter(owner)) {
            //POI must either be the job's target or the actor is still aware of it
            return null;
        }
        object[] data = null;
        if (otherData != null) {
            if (otherData.ContainsKey(goalAction.goapType)) {
                data = otherData[goalAction.goapType];
            } else if (otherData.ContainsKey(INTERACTION_TYPE.NONE)) {
                data = otherData[INTERACTION_TYPE.NONE];
            }
        }
        int cost = goalAction.GetCost(owner, target, job, data);
        log += $"\n--Searching plan for target: {target.name} with goal action ({cost}){goalAction.goapName}";
        GoapNode goalNode = ObjectPoolManager.Instance.CreateNewGoapPlanJob(cost, 0, goalAction, target);
        BuildGoapTree(goalNode, owner, job, rawPlan, allGoapActionAdvertisements, awareness, ref log); //, ref log
        if (rawPlan != null && rawPlan.Count > 0) {
            string rawPlanSummary = $"Generated raw plan for job { job.name } { owner.name }";
            for (int i = 0; i < rawPlan.Count; i++) {
                GoapNode currNode = rawPlan[i];
                rawPlanSummary += $"\n - {currNode.action.goapName }";
            }
            Debug.Log(rawPlanSummary);
            owner.logComponent.PrintCostLog();
            //has a created plan
            List<JobNode> actualNodes = TransformRawPlanToActualNodes(rawPlan, job.otherData);
            GoapPlan plan = new GoapPlan(actualNodes, target, isPersonalPlan);
            return plan;
        }
        return null;
    }
    public bool RecalculatePathForPlan(GoapPlan currentPlan, GoapPlanJob job, ref string log) {
        //In plan recalculation, only recalculate nodes starting from the previous node, because this means that the current node does not satisfy all preconditions, which in turn, means that somewhere in the previous nodes, the character failed to do the action
        //That is why we recalculate from the previous node up to the starting node
        Dictionary<POINT_OF_INTEREST_TYPE, List<GoapAction>> allGoapActionAdvertisements = InteractionManager.Instance.allGoapActionAdvertisements;
        Dictionary<POINT_OF_INTEREST_TYPE, List<IPointOfInterest>> awareness = owner.currentRegion.awareness;
        List<GoapNode> rawPlan = new List<GoapNode>();

        JobNode currentJobNode = currentPlan.currentNode;
        if (currentJobNode.singleNode != null) {
            //Recalculate for single node
            ActualGoapNode actualNode = currentJobNode.singleNode;
            GoapAction goalAction = actualNode.action;
            IPointOfInterest target = actualNode.poiTarget;
            object[] otherData = actualNode.otherData;
            if (target == job.targetPOI || target.IsStillConsideredPartOfAwarenessByCharacter(owner)) {
                //POI must either be the job's target or the actor is still aware of it
                int cost = 0;
                if (target.CanAdvertiseActionToActor(owner, goalAction, job, job.otherData, ref cost)) {
                    GoapNode goalNode = ObjectPoolManager.Instance.CreateNewGoapPlanJob(actualNode.cost, currentPlan.currentNodeIndex, goalAction, target);
                    BuildGoapTree(goalNode, owner, job, rawPlan, allGoapActionAdvertisements, awareness, ref log); //
                    if (rawPlan != null && rawPlan.Count > 0) {
                        //has a created plan
                        string rawPlanSummary = $"Recalculated raw plan for job { job.name } { owner.name }";
                        for (int i = 0; i < rawPlan.Count; i++) {
                            GoapNode currNode = rawPlan[i];
                            rawPlanSummary += $"\n - {currNode.action.goapName }";
                        }
                        Debug.Log(rawPlanSummary);
                        List<JobNode> plannedNodes = TransformRawPlanToActualNodes(rawPlan, job.otherData, currentPlan);
                        currentPlan.Reset(plannedNodes);
                        return true;
                    }
                }
            }
        } else {
            //Recalculate for multi node
            ActualGoapNode[] actualNodes = currentJobNode.multiNode;
            for (int i = 0; i < actualNodes.Length; i++) {
                ActualGoapNode actualNode = actualNodes[i];
                GoapAction goalAction = actualNode.action;
                IPointOfInterest target = actualNode.poiTarget;
                object[] otherData = actualNode.otherData;
                if (target != job.targetPOI && !target.IsStillConsideredPartOfAwarenessByCharacter(owner)) {
                    rawPlan.Clear();
                    break;
                }
                int cost = 0;
                if (!target.CanAdvertiseActionToActor(owner, goalAction, job, job.otherData, ref cost)) {
                    rawPlan.Clear();
                    break;
                } else {
                    GoapNode goalNode = ObjectPoolManager.Instance.CreateNewGoapPlanJob(actualNode.cost, currentPlan.currentNodeIndex, goalAction, target);
                    BuildGoapTree(goalNode, owner, job, rawPlan, allGoapActionAdvertisements, awareness, ref log);
                }
            }
            if (rawPlan != null && rawPlan.Count > 0) {
                //has a created plan
                string rawPlanSummary = $"Recalculated raw plan for job { job.name } { owner.name }";
                for (int i = 0; i < rawPlan.Count; i++) {
                    GoapNode currNode = rawPlan[i];
                    rawPlanSummary += $"\n - {currNode.action.goapName }";
                }
                owner.logComponent.PrintCostLog();
                List<JobNode> plannedNodes = TransformRawPlanToActualNodes(rawPlan, job.otherData, currentPlan);
                currentPlan.Reset(plannedNodes);
                return true;
            }
        }
        return false;
        //List of all starting nodes that can do the goal
        //List<GoapNode> startingNodes = new List<GoapNode>();
        //bool success = false;
        //if (currentPlan.isPersonalPlan) {
        //    string log = string.Empty;
        //    success = BuildGoapTree(currentPlan.endNode, startingNodes, usableActions, ref log, currentPlan.job);
        //} else {
        //    GoapNode currentLeafNode = null;
        //    bool hasUsableAction = false;
        //    success = true;
        //    for (int i = currentPlan.endNode.index; i >= 0; i--) {
        //        GoapNode failedNode = currentPlan.allNodes[i];
        //        hasUsableAction = false;
        //        for (int j = 0; j < usableActions.Count; j++) {
        //            GoapAction currentUsableAction = usableActions[j];
        //            if(failedNode.action.goapType == currentUsableAction.goapType && failedNode.action.poiTarget == currentUsableAction.poiTarget) {
        //                hasUsableAction = true;
        //                if(currentLeafNode == null) {
        //                    currentLeafNode = ObjectPoolManager.Instance.CreateNewGoapPlanJob(failedNode.parent, failedNode.parent.runningCost + currentUsableAction.cost, currentUsableAction);
        //                } else {
        //                    GoapNode leafNode = ObjectPoolManager.Instance.CreateNewGoapPlanJob(currentLeafNode.parent, currentLeafNode.parent.runningCost + currentUsableAction.cost, currentUsableAction);
        //                    currentLeafNode = leafNode;
        //                }
        //                break;
        //            }
        //        }
        //        if (!hasUsableAction) {
        //            //No usable action for the current failed node, fail recalculation
        //            success = false;
        //            break;
        //        }
        //    }
        //    if(currentLeafNode != null) {
        //        startingNodes.Add(currentLeafNode);
        //    } else {
        //        success = false;
        //    }
        //}
        //if (!success) {
        //    return false;
        //}

        //GoapNode cheapestStartingNode = null;
        //for (int i = 0; i < startingNodes.Count; i++) {
        //    if (cheapestStartingNode == null) {
        //        cheapestStartingNode = startingNodes[i];
        //    } else {
        //        if (startingNodes[i].cost < cheapestStartingNode.cost) {
        //            cheapestStartingNode = startingNodes[i];
        //        }
        //    }
        //}
        //currentPlan.Reset(cheapestStartingNode);
        //return true;
    }
    //Note: The target specified here is the target for the precondition not the job itself
    private void BuildGoapTree(GoapNode node, Character actor, GoapPlanJob job, List<GoapNode> rawPlan, Dictionary<POINT_OF_INTEREST_TYPE, List<GoapAction>> allGoapActionAdvertisements
        , Dictionary<POINT_OF_INTEREST_TYPE, List<IPointOfInterest>> awareness, ref string log) { //
        GoapAction action = node.action;
        IPointOfInterest target = node.target;
        log += $"\n--Adding node to raw plan: ({node.cost}){node.action.goapName}-{target.nameWithID}";
        rawPlan.Add(node);
        int sumCostSoFar = rawPlan.Sum(x => x.cost);
        log += $"\n--Cost so far: {sumCostSoFar}";
        if (sumCostSoFar > 1000) {
            log += "\n--Cost exceeded 1000, discard plan";
            rawPlan.Clear();
            return;
        }
        List<Precondition> preconditions = null;
        if (job.otherData != null) {
            if (job.otherData.ContainsKey(action.goapType)) {
                preconditions = action.GetPreconditions(actor, target, job.otherData[action.goapType]);
            } else if (job.otherData.ContainsKey(INTERACTION_TYPE.NONE)) {
                preconditions = action.GetPreconditions(actor, target, job.otherData[INTERACTION_TYPE.NONE]);
            } else {
                preconditions = action.GetPreconditions(actor, target, null);
            }
        } else {
            preconditions = action.GetPreconditions(actor, target, null);
        }
        if (preconditions.Count > 0) {
            log += $"\n--Node {node.action.goapName} has preconditions: {preconditions.Count}";
            //get other data for current action
            object[] preconditionActionData = null;
            if (job.otherData != null) {
                if (job.otherData.ContainsKey(action.goapType)) {
                    preconditionActionData = job.otherData[action.goapType];
                } else if (job.otherData.ContainsKey(INTERACTION_TYPE.NONE)) {
                    preconditionActionData = job.otherData[INTERACTION_TYPE.NONE];
                }
            }

            for (int i = 0; i < preconditions.Count; i++) {
                Precondition precondition = preconditions[i];
                if (!precondition.CanSatisfyCondition(actor, target, preconditionActionData, job.jobType)) {
                    GoapEffect preconditionEffect = precondition.goapEffect;
                    log +=
                        $"\n--Could not satisfy condition {preconditionEffect}, will look for action to satisfy it...";
                    if (preconditionEffect.target == GOAP_EFFECT_TARGET.TARGET) {
                        //if precondition's target is the target, then the one who will advertise must be the target only
                        int cost = 0;
                        GoapAction currentAction = null;
                        if (target == job.targetPOI || target.IsStillConsideredPartOfAwarenessByCharacter(actor)) { //POI must either be the job's target or the actor is still aware of it
                            currentAction = target.AdvertiseActionsToActor(actor, preconditionEffect, job, job.otherData, ref cost, ref log);
                        } else {
                            log += $"\n--{target.name} is not the job's target and the actor is not aware of it";
                        }
                        if (currentAction != null) {
                            log += $"\n--Found action: {currentAction.goapName}, creating new node...";
                            GoapNode leafNode = ObjectPoolManager.Instance.CreateNewGoapPlanJob(cost, node.level + 1, currentAction, target);
                            BuildGoapTree(leafNode, actor, job, rawPlan, allGoapActionAdvertisements, awareness, ref log); //
                        } else {
                            //Fail - rawPlan must be set to null so the plan will fail
                            rawPlan.Clear();
                            //rawPlan = null;
                            log += "\n--Could not find action to satisfy precondition, setting raw plan to null and exiting goap tree...";
                            return;
                        }
                    } else if (preconditionEffect.target == GOAP_EFFECT_TARGET.ACTOR) {
                        GoapAction lowestCostAction = null;
                        IPointOfInterest lowestCostTarget = null;
                        int lowestCost = 0;
                        log += $"\n--Choices for {preconditionEffect}";
                        log += "\n--";
                        foreach (KeyValuePair<POINT_OF_INTEREST_TYPE, List<GoapAction>> kvp in allGoapActionAdvertisements) {
                            if (awareness.ContainsKey(kvp.Key)) {
                                List<GoapAction> actionList = kvp.Value;
                                for (int j = 0; j < actionList.Count; j++) {
                                    GoapAction currentAction = actionList[j];

                                    if (currentAction.WillEffectsMatchPreconditionTypeAndTarget(preconditionEffect)) {
                                        //get other data for current action.
                                        object[] otherActionData = null;
                                        if (job.otherData != null) {
                                            if (job.otherData.ContainsKey(currentAction.goapType)) {
                                                otherActionData = job.otherData[currentAction.goapType];
                                            } else if (job.otherData.ContainsKey(INTERACTION_TYPE.NONE)) {
                                                otherActionData = job.otherData[INTERACTION_TYPE.NONE];
                                            }
                                        }

                                        //Further optimize this by creating a list of all poi that currently advertises this action, and loop that
                                        bool isJobTargetEvaluated = false;
                                        List<IPointOfInterest> poisThatAdvertisesCurrentAction = awareness[kvp.Key];
                                        for (int k = 0; k < poisThatAdvertisesCurrentAction.Count; k++) {
                                            IPointOfInterest poiTarget = poisThatAdvertisesCurrentAction[k];
                                            if (poiTarget == target) { isJobTargetEvaluated = true; }
                                            if (poiTarget == job.targetPOI || poiTarget.IsStillConsideredPartOfAwarenessByCharacter(actor)) { //POI must either be the job's target or the actor is still aware of it
                                                int cost = 0;
                                                bool canDoAction = poiTarget.CanAdvertiseActionToActor(actor, currentAction, job, job.otherData, ref cost)
                                                    && currentAction.WillEffectsSatisfyPrecondition(preconditionEffect, actor, poiTarget, otherActionData);
                                                if (canDoAction) {
                                                    log += $"({cost}){currentAction.goapName}-{poiTarget.nameWithID}, ";
                                                    if (lowestCostAction == null || cost < lowestCost) {
                                                        lowestCostAction = currentAction;
                                                        lowestCostTarget = poiTarget;
                                                        lowestCost = cost;
                                                    }
                                                }
                                            }
                                        }
                                        if (!isJobTargetEvaluated && kvp.Key == target.poiType) {
                                            IPointOfInterest poiTarget = target;
                                            if (poiTarget == job.targetPOI || poiTarget.IsStillConsideredPartOfAwarenessByCharacter(actor)) { //POI must either be the job's target or the actor is still aware of it
                                                int cost = 0;
                                                bool canDoAction = poiTarget.CanAdvertiseActionToActor(actor, currentAction, job, job.otherData, ref cost)
                                                    && currentAction.WillEffectsSatisfyPrecondition(preconditionEffect, actor, poiTarget, otherActionData);
                                                if (canDoAction) {
                                                    log += $"({cost}){currentAction.goapName}-{poiTarget.nameWithID}, ";
                                                    if (lowestCostAction == null || cost < lowestCost) {
                                                        lowestCostAction = currentAction;
                                                        lowestCostTarget = poiTarget;
                                                        lowestCost = cost;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if(lowestCostAction != null) {
                            log += $"\n--Found action: {lowestCostAction.goapName}, creating new node...";
                            GoapNode leafNode = ObjectPoolManager.Instance.CreateNewGoapPlanJob(lowestCost, node.level + 1, lowestCostAction, lowestCostTarget);
                            BuildGoapTree(leafNode, actor, job, rawPlan, allGoapActionAdvertisements, awareness, ref log); //, ref log
                        } else {
                            //Fail - rawPlan must be set to null so the plan will fail
                            rawPlan.Clear();
                            //rawPlan = null;
                            log += "\n--Could not find action to satisfy precondition, setting raw plan to null and exiting goap tree...";
                            return;
                        }
                        //foreach (KeyValuePair<POINT_OF_INTEREST_TYPE, List<IPointOfInterest>> awareness in actor.awareness) {
                        //    List<IPointOfInterest> awarenessList = awareness.Value;
                        //    for (int i = 0; i < awarenessList.Count; i++) {
                        //        IPointOfInterest poiTarget = awarenessList[i];
                        //        int cost = 0;
                        //        INTERACTION_TYPE actionType = poiTarget.AdvertiseActionsToActor(actor, preconditionEffect, job.otherData, ref cost);
                        //        if (actionType != INTERACTION_TYPE.NONE) {
                        //            GoapAction leafAction = InteractionManager.Instance.goapActionData[actionType];
                        //            GoapNode leafNode = ObjectPoolManager.Instance.CreateNewGoapPlanJob(cost, goalNode.level + 1, leafAction, poiTarget);
                        //            BuildGoapTree(leafNode, rawPlan, usableActions, ref log, job);
                        //        }
                        //    }
                        //}
                    }
                } else {
                    log += $"\n--Condition {i} satisfied, checking next precondition...";
                }
            }
        } else {
            log += $"\n--Node {node.action.goapName} has no preconditions, exiting goap tree...";
        }
        //if (parent == null) {
        //    return false;
        //}
        //log += "\nBuilding goap tree with parent " + parent.actionType.goapName;
        //if(precondition.actionType.preconditions.Count > 0) {
        //    List<Precondition> unsatisfiedPreconditions = new List<Precondition>();
        //    for (int i = 0; i < precondition.actionType.preconditions.Count; i++) {
        //        Precondition precondition = precondition.actionType.preconditions[i];
        //        if (!precondition.CanSatisfyCondition(actor, target)) {
        //            //Pool all unsatisfied preconditions
        //            unsatisfiedPreconditions.Add(precondition);
        //        }
        //    }
        //    if (unsatisfiedPreconditions.Count > 0) {
        //        log += "\nChecking unsatisfied preconditions: ";
        //        for (int j = 0; j < unsatisfiedPreconditions.Count; j++) {
        //            Precondition precon = unsatisfiedPreconditions[j];
        //            log += "\n\t" + precon.goapEffect.conditionType.ToString() + " " + precon.goapEffect.conditionKey?.ToString() + " " + precon.goapEffect.targetPOI?.ToString() + " - ";
        //        }
        //        //Look for an action that can satisfy all unsatisfied preconditions
        //        //if one precondition cannot be satisfied, skip that action
        //        //if all preconditions can be satisfied, create a new goap tree for it
        //        log += "\nActions: ";
        //        List<GoapAction> shuffledActions = Utilities.Shuffle(usableActions);
        //        for (int i = 0; i < shuffledActions.Count; i++) {
        //            GoapAction usableAction = shuffledActions[i];
        //            bool canSatisfyAllPreconditions = true;
        //            log += "\n\t" + usableAction.goapName;
        //            for (int j = 0; j < unsatisfiedPreconditions.Count; j++) {
        //                Precondition precon = unsatisfiedPreconditions[j];
        //                if (!usableAction.WillEffectsSatisfyPrecondition(precon.goapEffect)) {
        //                    canSatisfyAllPreconditions = false;
        //                    break;
        //                } else {
        //                    //if there is a provided job, check if it has any forced actions
        //                    //if it does, only allow the action that it allows
        //                    if (job != null && job.forcedActions.Count > 0) {
        //                        bool satisfiedForcedActions = true;
        //                        ForcedActionsComparer comparer = new ForcedActionsComparer();
        //                        foreach (KeyValuePair<GoapEffect, INTERACTION_TYPE> kvp in job.forcedActions) {
        //                            if(comparer.Equals(kvp.Key, precon.goapEffect)) {
        //                                if(kvp.Value != usableAction.goapType) {
        //                                    satisfiedForcedActions = false;
        //                                    break;
        //                                }
        //                            }
        //                        }
        //                        if (!satisfiedForcedActions) {
        //                            canSatisfyAllPreconditions = false;
        //                            break;
        //                        }
        //                    }
        //                }
        //            }
        //            if (canSatisfyAllPreconditions) {
        //                GoapNode leafNode = ObjectPoolManager.Instance.CreateNewGoapPlanJob(precondition, precondition.runningCost + usableAction.cost, usableAction);
        //                log += " - Satisfied"; 
        //                bool success = BuildGoapTree(leafNode, startingNodes, usableActions, ref log, job);
        //                return success;
        //            }
        //        }
        //        return false;
        //    } else {
        //        log += "\nNo unsatisfied preconditions";
        //        startingNodes.Add(precondition);
        //        return true;
        //    }
        //} else {
        //    log += "\nNo preconditions.";
        //    startingNodes.Add(precondition);
        //    return true;
        //}
        //return false;
    }

    private List<JobNode> TransformRawPlanToActualNodes(List<GoapNode> rawPlan, Dictionary<INTERACTION_TYPE, object[]> otherData, GoapPlan currentPlan = null) { //actualPlan is for recalculation only, so that it will no longer create a new list, since in recalculation we already have a list of job nodes
        List<JobNode> actualPlan = null;
        int index = 0;
        if (currentPlan == null) {
            actualPlan = new List<JobNode>();
        } else {
            actualPlan = currentPlan.allNodes;
            actualPlan.RemoveRange(0, currentPlan.currentNodeIndex + 1); //It's +1 because we want to remove also the current node of the actual plan since it is already in the rawPlan
            index = currentPlan.currentNodeIndex;
        }
        List<int> tempNodeIndexHolder = new List<int>();
        List<GoapNode> discardedNodes = new List<GoapNode>();
        while (rawPlan.Count > 0) {
            tempNodeIndexHolder.Clear();
            for (int i = 0; i < rawPlan.Count; i++) {
                GoapNode rawNode = rawPlan[i];
                if(rawNode.level == index) {
                    tempNodeIndexHolder.Add(i);
                }
            }
            if(tempNodeIndexHolder.Count > 0) {
                if(tempNodeIndexHolder.Count == 1) {
                    //Single Job Node
                    int nodeIndex = tempNodeIndexHolder[0];
                    GoapNode rawNode = rawPlan[nodeIndex];
                    object[] data = null;
                    if (otherData != null) {
                        if (otherData.ContainsKey(rawNode.action.goapType)) {
                            data = otherData[rawNode.action.goapType];
                        } else if (otherData.ContainsKey(INTERACTION_TYPE.NONE)) {
                            data = otherData[INTERACTION_TYPE.NONE];
                        }
                    }
                    ActualGoapNode actualNode = new ActualGoapNode(rawNode.action, owner, rawNode.target, data, rawNode.cost);
                    SingleJobNode singleJobNode = new SingleJobNode(actualNode);
                    actualPlan.Insert(0, singleJobNode);
                    GoapNode node = rawPlan[nodeIndex];
                    rawPlan.RemoveAt(nodeIndex);
                    ObjectPoolManager.Instance.ReturnGoapNodeToPool(node);
                } else {
                    //Multi Job Node
                    discardedNodes.Clear();
                    ActualGoapNode[] actualNodes = new ActualGoapNode[tempNodeIndexHolder.Count];
                    for (int i = 0; i < tempNodeIndexHolder.Count; i++) {
                        int nodeIndex = tempNodeIndexHolder[i];
                        GoapNode rawNode = rawPlan[nodeIndex];
                        object[] data = null;
                        if (otherData != null) {
                            if (otherData.ContainsKey(rawNode.action.goapType)) {
                                data = otherData[rawNode.action.goapType];
                            } else if (otherData.ContainsKey(INTERACTION_TYPE.NONE)) {
                                data = otherData[INTERACTION_TYPE.NONE];
                            }
                        }
                        ActualGoapNode actualNode = new ActualGoapNode(rawNode.action, owner, rawNode.target, data, rawNode.cost);
                        actualNodes[i] = actualNode;
                        discardedNodes.Add(rawNode);
                    }
                    for (int i = 0; i < discardedNodes.Count; i++) {
                        rawPlan.Remove(discardedNodes[i]);
                        ObjectPoolManager.Instance.ReturnGoapNodeToPool(discardedNodes[i]);
                    }
                    MultiJobNode multiJobNode = new MultiJobNode(actualNodes);
                    actualPlan.Insert(0, multiJobNode);
                }
            }
            index++;
        }
        return actualPlan;
    }
}