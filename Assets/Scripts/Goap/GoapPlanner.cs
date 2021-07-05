using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Inner_Maps.Location_Structures;
using Inner_Maps;
using Locations;
using Locations.Settlements;
using UtilityScripts;
public class GoapPlanner {
    private const int CACHED_GOAP_NODE_CAPACITY = 10;
    public Character owner { get; private set; }
    public GOAP_PLANNING_STATUS status { get; private set; }
    private Precondition failedPrecondition { get; set; }
    public INTERACTION_TYPE failedPreconditionActionType { get; private set; }

    private List<GoapNode> _rawPlan;

    private GoapThread _goapThreadInProcess;

    private List<GoapNode> _cachedGoapNodes;

    public GoapPlanner(Character owner) {
        this.owner = owner;
        _rawPlan = new List<GoapNode>();
        _cachedGoapNodes = new List<GoapNode>();
    }
    public void StartGOAP(GoapEffect goal, IPointOfInterest target, GoapPlanJob job, bool isPersonalPlan = true) {
        if (status == GOAP_PLANNING_STATUS.RUNNING) {
            //If already processing, do not throw another process to the multithread
            return;
        }
        if (job != null) {
            job.SetAssignedPlan(null);
        }
        status = GOAP_PLANNING_STATUS.RUNNING;

#if DEBUG_LOG
        Debug.Log($"{GameManager.Instance.TodayLogString()}{owner.name} has started planning {job}");
#endif
        Character actor = owner;
        CreateGoapNodeCache();
        _goapThreadInProcess = ObjectPoolManager.Instance.CreateNewGoapThread();
        _goapThreadInProcess.Initialize(actor, target, goal, isPersonalPlan, job);
        MultiThreadPool.Instance.AddToThreadPool(_goapThreadInProcess);
        job.SetIsInMultithread(true);
    }
    public void StartGOAP(INTERACTION_TYPE goalType, IPointOfInterest target, GoapPlanJob job, bool isPersonalPlan = true) {
        if (status == GOAP_PLANNING_STATUS.RUNNING) {
            //If already processing, do not throw another process to the multithread
            return;
        }
        if (job != null) {
            job.SetAssignedPlan(null);
        }
        status = GOAP_PLANNING_STATUS.RUNNING;
#if DEBUG_LOG
        Debug.Log($"{GameManager.Instance.TodayLogString()}{owner.name} has started planning {job}");
#endif
        CreateGoapNodeCache();
        _goapThreadInProcess = ObjectPoolManager.Instance.CreateNewGoapThread();
        _goapThreadInProcess.Initialize(owner, goalType, target, isPersonalPlan, job);
        MultiThreadPool.Instance.AddToThreadPool(_goapThreadInProcess);
        job.SetIsInMultithread(true);
    }
    public void RecalculateJob(GoapPlanJob job) {
        if (status == GOAP_PLANNING_STATUS.RUNNING) {
            //If already processing, do not throw another process to the multithread
            return;
        }
        if (job.assignedPlan != null) {
            job.assignedPlan.SetIsBeingRecalculated(true);
            status = GOAP_PLANNING_STATUS.RUNNING;
            CreateGoapNodeCache();
            _goapThreadInProcess = ObjectPoolManager.Instance.CreateNewGoapThread();
            _goapThreadInProcess.InitializeForRecalculation(owner, job.assignedPlan, job);
            MultiThreadPool.Instance.AddToThreadPool(_goapThreadInProcess);
            job.SetIsInMultithread(true);
        }
    }
    public void ReceivePlanFromGoapThread(GoapPlan createdPlan) {
        status = GOAP_PLANNING_STATUS.NONE;
        if (_goapThreadInProcess.job != null) {
            _goapThreadInProcess.job.SetIsInMultithread(false);
            if (_goapThreadInProcess.job.shouldForceCancelUponReceiving) {
                ForceCancelJobAndReturnToObjectPool(_goapThreadInProcess.job);
                if (_goapThreadInProcess.recalculationPlan != null && _goapThreadInProcess.recalculationPlan.resetPlanOnFinishRecalculation) {
                    ObjectPoolManager.Instance.ReturnGoapPlanToPool(_goapThreadInProcess.recalculationPlan);
                }
                ObjectPoolManager.Instance.ReturnGoapThreadToPool(_goapThreadInProcess);
                _goapThreadInProcess = null;
                return;
            }
        }
        if (owner.isDead || !owner.marker) {
            ForceCancelJobAndReturnToObjectPool(_goapThreadInProcess.job);
            ObjectPoolManager.Instance.ReturnGoapThreadToPool(_goapThreadInProcess);
            _goapThreadInProcess = null;
            return;
        }
        if (_goapThreadInProcess.recalculationPlan != null) {
            // owner.logComponent.PrintLogIfActive(goapThread.log);
            if (_goapThreadInProcess.recalculationPlan.isEnd) {
                ForceCancelJobAndReturnToObjectPool(_goapThreadInProcess.job);
                if (_goapThreadInProcess.recalculationPlan.resetPlanOnFinishRecalculation) {
                    ObjectPoolManager.Instance.ReturnGoapPlanToPool(_goapThreadInProcess.recalculationPlan);
                }
                ObjectPoolManager.Instance.ReturnGoapThreadToPool(_goapThreadInProcess);
                _goapThreadInProcess = null;
                return;
            } else {
                if (_goapThreadInProcess.recalculationPlan.resetPlanOnFinishRecalculation) {
                    ObjectPoolManager.Instance.ReturnGoapPlanToPool(_goapThreadInProcess.recalculationPlan);
                    ObjectPoolManager.Instance.ReturnGoapThreadToPool(_goapThreadInProcess);
                    _goapThreadInProcess = null;
                    return;
                }
            }
        }
#if DEBUG_LOG
        string additionalLog = string.Empty;
#endif
        if (_goapThreadInProcess.job.originalOwner == null) {
            //This means that the job is already in the object pool, meaning that the received plan for the job is no longer applicable since the job is already deleted/cancelled
#if DEBUG_LOG
            additionalLog += "\nJOB NO LONGER APPLICABLE, DISCARD PLAN IF THERE'S ANY";
            owner.logComponent.PrintLogIfActive(_goapThreadInProcess.log + additionalLog);
#endif
            ForceCancelJobAndReturnToObjectPool(_goapThreadInProcess.job);
            ObjectPoolManager.Instance.ReturnGoapThreadToPool(_goapThreadInProcess);
            _goapThreadInProcess = null;
            return;
        }
#if DEBUG_LOG
        owner.logComponent.PrintLogIfActive(_goapThreadInProcess.log + additionalLog);
#endif
        if (createdPlan != null) {
            JOB_TYPE jobType = _goapThreadInProcess.job.jobType;
            if (jobType == JOB_TYPE.PRODUCE_FOOD && owner.traitContainer.HasTrait("Abstain Fullness")) {
                owner.traitContainer.RemoveTrait(owner, "Abstain Fullness");
            }
            createdPlan.SetDoNotRecalculate(_goapThreadInProcess.job.doNotRecalculate);
            if (_goapThreadInProcess.recalculationPlan != null) {
                //This means that the created plan is a recalculated plan
                createdPlan.SetIsBeingRecalculated(false);
            }
            if (!owner.limiterComponent.canPerform) {
                int canPerformValue = owner.limiterComponent.canPerformValue;
                if (canPerformValue == -1 && (owner.traitContainer.HasTrait("Paralyzed") || owner.traitContainer.HasTrait("Quarantined"))) {
                    //If the owner is paralyzed or quarantined and the only reason he cannot perform is because of that paralyzed, the plan must not be scrapped
                } else {
#if DEBUG_LOG
                    owner.logComponent.PrintLogIfActive($"{owner.name} is scrapping plan since {owner.name} cannot perform. {_goapThreadInProcess.job.name} is the job.");
#endif
                    _goapThreadInProcess.job.CancelJob();
                    ObjectPoolManager.Instance.ReturnGoapThreadToPool(_goapThreadInProcess);
                    _goapThreadInProcess = null;
                    return;
                }
            }
            int jobIndex = owner.jobQueue.GetJobQueueIndex(_goapThreadInProcess.job);
            if (jobIndex != -1) {
                //Only set assigned plan if job is still in character job queue because if not, it means that the job is no longer taken
                _goapThreadInProcess.job.SetAssignedPlan(createdPlan);
                if (jobIndex != 0) {
                    ObjectPoolManager.Instance.ReturnGoapThreadToPool(_goapThreadInProcess);
                    _goapThreadInProcess = null;
                    //If the job of the receive plan is no longer the top priority, process the top most job because it means that while the goap planner is running, the top most priority has been replaced
                    //This means that the top most priority was not processed since the goap planner is still running
                    owner.jobQueue.ProcessFirstJobInQueue();
                    return;
                }
            }
        } else {
            JOB_TYPE jobType = _goapThreadInProcess.job.jobType;
            //If unable to do a Need while in a Trapped Structure, remove Trap Structure.
            if (jobType.IsFullnessRecoveryTypeJob()) {
                owner.trapStructure.ResetAllTrappedValues();
                if (jobType != JOB_TYPE.FULLNESS_RECOVERY_ON_SIGHT) {
                    owner.traitContainer.AddTrait(owner, "Abstain Fullness");
                }
            } else if (jobType.IsTirednessRecoveryTypeJob()) {
                owner.trapStructure.ResetAllTrappedValues();
                owner.traitContainer.AddTrait(owner, "Abstain Tiredness");
            } else if (jobType.IsHappinessRecoveryTypeJob()) {
                owner.trapStructure.ResetAllTrappedValues();
                owner.traitContainer.AddTrait(owner, "Abstain Happiness");
            }
            if (_goapThreadInProcess.recalculationPlan == null) {
                //This means that the planner cannot create a new plan
                if (_goapThreadInProcess.job.targetPOI != null) {
                    //Note: Added checking for target POI because there are times that a job has no target POI (defaults to target actor when planning)
                    bool logCancelJobNoPlan = !(jobType == JOB_TYPE.DOUSE_FIRE && _goapThreadInProcess.job.targetPOI.gridTileLocation == null);
                    if (logCancelJobNoPlan && !CharacterManager.Instance.lessenCharacterLogs) {
                        Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "cancel_job_no_plan", providedTags: LOG_TAG.Work);
                        log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                        log.AddToFillers(null, _goapThreadInProcess.job.GetJobDetailString(), LOG_IDENTIFIER.STRING_1);
                        owner.logComponent.RegisterLog(log, true);
                    }
                }
                if (_goapThreadInProcess.job.originalOwner.ownerType != JOB_OWNER.CHARACTER) {
                    _goapThreadInProcess.job.AddBlacklistedCharacter(owner);
                }
            }
            //Every time no plan is generated for the job, remove carried poi because this means that the carried poi is part of that job that has no plan, so the character needs to let go of the poi now
            if (owner.carryComponent.IsNotBeingCarried()) {
                if (owner.carryComponent.isCarryingAnyPOI) {
                    IPointOfInterest carriedPOI = owner.carryComponent.carriedPOI;
#if DEBUG_LOG
                    string log = $"Dropping carried POI: {carriedPOI.name} because no plan was generated.";
                    log += "\nAdditional Info:";
#endif
                    if (carriedPOI is ResourcePile) {
                        ResourcePile pile = carriedPOI as ResourcePile;
#if DEBUG_LOG
                        log += $"\n-Stored resources on drop: {pile.resourceInPile} {pile.providedResource}";
#endif
                    } else if (carriedPOI is Table) {
                        Table table = carriedPOI as Table;
#if DEBUG_LOG
                        log += $"\n-Stored resources on drop: {table.food} Food.";
#endif
                    }
#if DEBUG_LOG
                    owner.logComponent.PrintLogIfActive(log);
#endif
                }
                owner.UncarryPOI();
            }
            if (_goapThreadInProcess.job != null && _goapThreadInProcess.job.jobType.IsCultistJob()) {
                string reason = owner.GetCultistUnableToDoJobReason(_goapThreadInProcess.job, failedPrecondition, failedPreconditionActionType);
                owner.LogUnableToDoJob(reason);
            }
            _goapThreadInProcess.job.CancelJob();

            if (jobType == JOB_TYPE.FULLNESS_RECOVERY_URGENT || jobType == JOB_TYPE.FULLNESS_RECOVERY_NORMAL) {
                //Do not produce food anymore personally, since it is already handled in character wants
                if (!owner.traitContainer.HasTrait("Vampire") && owner.isNormalCharacter && !owner.isConsideredRatman) {
                    //Special case for when a character cannot do hunger recovery, he/she must produce food instead
                    //NOTE: Excluded vampires because we don't want vampires to produce food when they fail to drink blood.
                    if (!owner.partyComponent.isMemberThatJoinedQuest) {
                        //If character is currently in an active party with a quest and it is one of the members that joined the quest
                        //It should not produce food personally because the produce food while in a party that has quest is controlled by the party itseld, the Produce Food For Camp
                        owner.jobComponent.CreateProduceFoodJob();
                    }
                }
                if (owner.traitContainer.HasTrait("Pest") || owner is Rat) {
                    owner.behaviourComponent.SetPestHasFailedEat(true);
                }
            } else if (jobType == JOB_TYPE.RECOVER_HP) {
                owner.jobComponent.SetDoNotDoRecoverHPJob(true);
            }
        }

        ObjectPoolManager.Instance.ReturnGoapThreadToPool(_goapThreadInProcess);
        _goapThreadInProcess = null;
    }
    private void ForceCancelJobAndReturnToObjectPool(JobQueueItem job) {
        if (job == null) {
            return;
        }
        job.ForceCancelJob();
        if (!string.IsNullOrEmpty(job.persistentID)) {
            JobManager.Instance.OnFinishJob(job);
        }
    }
    public void PlanActions(IPointOfInterest target, GoapEffect goalEffect, bool isPersonalPlan, ref string log, GoapPlanJob job, GoapThread goapThread) {
        //Cache all needed data
        Dictionary<GOAP_EFFECT_CONDITION, List<GoapAction>> actionsCategorizedByEffect = InteractionManager.Instance.actionsCategorizedByEffect;
        _rawPlan.Clear();
        failedPrecondition = null;
        failedPreconditionActionType = INTERACTION_TYPE.NONE;

        owner.logComponent.ClearCostLog();

#if DEBUG_LOG
        owner.logComponent.AppendCostLog($"BASE COSTS OF {owner.name} ACTIONS ON {job.name} PLANNING");
        log = $"{log}\n--Searching plan for target: {target.name}";
#endif
        if (goalEffect.target == GOAP_EFFECT_TARGET.TARGET) {
            //if precondition's target is TARGET, then the one who will advertise must be the target only
            int cost = 0;
            //Get action with the lowest cost that the actor can do that satisfies the goal effect
            if (target == job.targetPOI || target.IsStillConsideredPartOfAwarenessByCharacter(owner)) { //POI must either be the job's target or the actor is still aware of it
                GoapAction currentAction = target.AdvertiseActionsToActor(owner, goalEffect, job, ref cost, ref log);
                if (currentAction != null) {
                    //If an action is found, make it the goal node and start building the plan
                    GoapNode goalNode = SetGoapNodeCacheData(cost, 0, currentAction, target);
                    BuildGoapTree(goalNode, owner, job, _rawPlan, actionsCategorizedByEffect, ref log); //, ref log
                }
            }
        } else if (goalEffect.target == GOAP_EFFECT_TARGET.ACTOR) {
            //If precondition's target is ACTOR, get the lowest action that the actor can do that will satisfy the goal effect
            GoapAction lowestCostAction = null;
            IPointOfInterest lowestCostTarget = null;
            int lowestCost = 0;

#if DEBUG_LOG
            log = $"{log}\n--Choices for {goalEffect.ToString()}";
            log = $"{log}\n--";
#endif

            ProcessFindingLowestCostActionAndTarget(job, goalEffect, target, actionsCategorizedByEffect, ref lowestCost, ref lowestCostAction, ref lowestCostTarget, ref log);
            if (lowestCostAction != null) {
                GoapNode leafNode = SetGoapNodeCacheData(lowestCost, 0, lowestCostAction, lowestCostTarget);
                BuildGoapTree(leafNode, owner, job, _rawPlan, actionsCategorizedByEffect, ref log); //, ref log
            }
        }
        if (_rawPlan.Count > 0) {
            //has a created plan
#if DEBUG_LOG
            string rawPlanSummary = $"Generated raw plan for job { job.name } { owner.name }";
            for (int i = 0; i < _rawPlan.Count; i++) {
                GoapNode currNode = _rawPlan[i];
                rawPlanSummary = $"{rawPlanSummary}\n - {currNode.action.goapName}";
            }
            Debug.Log(rawPlanSummary);
#endif
            //List<JobNode> actualNodes = TransformRawPlanToActualNodes(_rawPlan, job);
            //// GoapPlan plan = ObjectPoolManager.Instance.CreateNewGoapPlan(actualNodes, target);
            //GoapPlan plan = goapThread.cachedPlan;
            //plan.SetNodes(actualNodes);
            //plan.SetTarget(target);
            //plan.SetIsPersonalPlan(isPersonalPlan);
            //return plan;
        }

#if DEBUG_LOG
        owner.logComponent.PrintCostLog();
#endif
    }
    public void PlanActions(IPointOfInterest target, GoapAction goalAction, bool isPersonalPlan, ref string log, GoapPlanJob job, GoapThread goapThread) {
        Dictionary<GOAP_EFFECT_CONDITION, List<GoapAction>> actionsCategorizedByEffect = InteractionManager.Instance.actionsCategorizedByEffect;
        _rawPlan.Clear();
        failedPrecondition = null;
        failedPreconditionActionType = INTERACTION_TYPE.NONE;

        owner.logComponent.ClearCostLog();

#if DEBUG_LOG
        owner.logComponent.AppendCostLog($"BASE COSTS OF {owner.name} ACTIONS ON {job.name} PLANNING");
#endif
        if (target != job.targetPOI && !target.IsStillConsideredPartOfAwarenessByCharacter(owner)) {
            //POI must either be the job's target or the actor is still aware of it
            //return null;
            return;
        }
        int cost = goalAction.GetCost(owner, target, job);

#if DEBUG_LOG
        log += $"\n--Searching plan for target: {target.name} with goal action ({cost}){goalAction.goapName}";
#endif
        GoapNode goalNode = SetGoapNodeCacheData(cost, 0, goalAction, target);
        BuildGoapTree(goalNode, owner, job, _rawPlan, actionsCategorizedByEffect, ref log); //, ref log
        if (_rawPlan.Count > 0) {
#if DEBUG_LOG
            string rawPlanSummary = $"Generated raw plan for job { job.name } { owner.name }";
            for (int i = 0; i < _rawPlan.Count; i++) {
                GoapNode currNode = _rawPlan[i];
                rawPlanSummary += $"\n - {currNode.action.goapName }";
            }
            Debug.Log(rawPlanSummary);
#endif

            //Move this to Main Thread so that there will be no race conditions
            //has a created plan
            //List<JobNode> actualNodes = TransformRawPlanToActualNodes(_rawPlan, job);
            //// GoapPlan plan = ObjectPoolManager.Instance.CreateNewGoapPlan(actualNodes, target);
            //GoapPlan plan = goapThread.cachedPlan;
            //plan.SetNodes(actualNodes);
            //plan.SetTarget(target);
            //plan.SetIsPersonalPlan(isPersonalPlan);
            //return plan;
        }

#if DEBUG_LOG
        owner.logComponent.PrintCostLog();
#endif
        //return null;
    }
    public bool RecalculatePathForPlan(GoapPlan currentPlan, GoapPlanJob job, ref string log) {
        //In plan recalculation, only recalculate nodes starting from the previous node, because this means that the current node does not satisfy all preconditions, which in turn, means that somewhere in the previous nodes, the character failed to do the action
        //That is why we recalculate from the previous node up to the starting node
        Dictionary<GOAP_EFFECT_CONDITION, List<GoapAction>> actionsCategorizedByEffect = InteractionManager.Instance.actionsCategorizedByEffect;
        _rawPlan.Clear();
        failedPrecondition = null;
        failedPreconditionActionType = INTERACTION_TYPE.NONE;

        JobNode currentJobNode = currentPlan.currentNode;
        ActualGoapNode actualNode = currentJobNode.singleNode;
        GoapAction goalAction = actualNode.action;
        IPointOfInterest target = actualNode.poiTarget;
        OtherData[] otherData = actualNode.otherData;
        if (target == job.targetPOI || target.IsStillConsideredPartOfAwarenessByCharacter(owner)) {
            //POI must either be the job's target or the actor is still aware of it
            if (target.CanAdvertiseActionToActor(owner, goalAction, job) && target.Advertises(goalAction.goapType)) {
                int cost = goalAction.GetCost(owner, target, job);
                GoapNode goalNode = SetGoapNodeCacheData(actualNode.cost, currentPlan.currentNodeIndex, goalAction, target);
                BuildGoapTree(goalNode, owner, job, _rawPlan, actionsCategorizedByEffect, ref log); //
                if (_rawPlan.Count > 0) {
                    //has a created plan
#if DEBUG_LOG
                    string rawPlanSummary = $"Recalculated raw plan for job { job.name } { owner.name }";
                    for (int i = 0; i < _rawPlan.Count; i++) {
                        GoapNode currNode = _rawPlan[i];
                        rawPlanSummary += $"\n - {currNode.action.goapName }";
                    }
                    Debug.Log(rawPlanSummary);
#endif
                    //List<JobNode> plannedNodes = TransformRawPlanToActualNodes(_rawPlan, job, currentPlan);
                    //currentPlan.SetNodes(plannedNodes);
                    return true;
                }
            }
        }
        return false;
    }
    public GoapPlan TransformRawPlanToActualPlan() {
        GoapPlan createdPlan = null;
        if (_rawPlan.Count > 0) {
            string log = string.Empty;
            if (_goapThreadInProcess.recalculationPlan != null) {
                GoapPlan recalculatedPlan = _goapThreadInProcess.recalculationPlan;
                if (_goapThreadInProcess.isRecalculationSuccess) {
                    List<JobNode> plannedNodes = TransformRawPlanToActualNodes(_rawPlan, _goapThreadInProcess.job, recalculatedPlan);
                    recalculatedPlan.SetNodes(plannedNodes);
#if DEBUG_LOG
                    log += "\nGENERATED RECALCULATED PLAN: ";
                    log += recalculatedPlan.LogPlan();
#endif
                    createdPlan = recalculatedPlan;
                } else {
#if DEBUG_LOG
                    log += "\nFAILED TO RECALCULATE PLAN!";
#endif
                }
            } else {
                List<JobNode> actualNodes = TransformRawPlanToActualNodes(_rawPlan, _goapThreadInProcess.job);
                GoapPlan plan = ObjectPoolManager.Instance.CreateNewGoapPlan(actualNodes, _goapThreadInProcess.target);
                plan.SetNodes(actualNodes);
                plan.SetTarget(_goapThreadInProcess.target);
                plan.SetIsPersonalPlan(_goapThreadInProcess.isPersonalPlan);

                if (plan != null) {
#if DEBUG_LOG
                    log += "GENERATED PLAN: ";
                    log += plan.LogPlan();
#endif
                    createdPlan = plan;
                } else {
#if DEBUG_LOG
                    log += "\n\nNO PLAN WAS GENERATED! End goap...";
#endif
                }
            }
#if DEBUG_LOG
            owner.logComponent.PrintLogIfActive(log);
#endif
        }
        ResetGoapNodeCache();
        return createdPlan;
    }
    //Note: The target specified here is the target for the precondition not the job itself
    private void BuildGoapTree(GoapNode node, Character actor, GoapPlanJob job, List<GoapNode> rawPlan, Dictionary<GOAP_EFFECT_CONDITION, List<GoapAction>> actionsCategorizedByEffect, ref string log) { //
        GoapAction action = node.action;
        IPointOfInterest target = node.target;
#if DEBUG_LOG
        log += $"\n--Adding node to raw plan: ({node.cost}){node.action.goapName}-{target.nameWithID}";
#endif
        rawPlan.Add(node);
        int sumCostSoFar = rawPlan.Sum(x => x.cost);
#if DEBUG_LOG
        log += $"\n--Cost so far: {sumCostSoFar}";
#endif
        if (sumCostSoFar > 1000) {
#if DEBUG_LOG
            log += "\n--Cost exceeded 1000, discard plan";
#endif
            rawPlan.Clear();
            return;
        }
        Precondition precondition = null;
        OtherData[] otherData = job.GetOtherDataFor(action.goapType);
        bool isOverriden = false;
        precondition = action.GetPrecondition(actor, target, otherData, job.jobType, out isOverriden);
        if (precondition != null) {
#if DEBUG_LOG
            log += $"\n--Node {node.action.goapName} has precondition";
#endif
            if (!precondition.CanSatisfyCondition(actor, target, otherData, job.jobType)) {
                GoapEffect preconditionEffect = precondition.goapEffect;
#if DEBUG_LOG
                log +=
                    $"\n--Could not satisfy condition {preconditionEffect}, will look for action to satisfy it...";
#endif
                if (preconditionEffect.target == GOAP_EFFECT_TARGET.TARGET) {
                    //if precondition's target is the target, then the one who will advertise must be the target only
                    int cost = 0;
                    GoapAction currentAction = null;
                    if (target == job.targetPOI || target.IsStillConsideredPartOfAwarenessByCharacter(actor)) { //POI must either be the job's target or the actor is still aware of it
                        currentAction = target.AdvertiseActionsToActor(actor, preconditionEffect, job, ref cost, ref log);
                    } else {
#if DEBUG_LOG
                        log += $"\n--{target.name} is not the job's target and the actor is not aware of it";
#endif
                    }
                    if (currentAction != null) {
#if DEBUG_LOG
                        log += $"\n--Found action: {currentAction.goapName}, creating new node...";
#endif
                        GoapNode leafNode = SetGoapNodeCacheData(cost, node.level + 1, currentAction, target);
                        BuildGoapTree(leafNode, actor, job, rawPlan, actionsCategorizedByEffect, ref log); //
                    } else {
                        //Fail - rawPlan must be set to null so the plan will fail
                        rawPlan.Clear();
#if DEBUG_LOG
                        log += "\n--Could not find action to satisfy precondition, setting raw plan to null and exiting goap tree...";
#endif
                        failedPrecondition = precondition;
                        failedPreconditionActionType = action.goapType;
                        return;
                    }
                } else if (preconditionEffect.target == GOAP_EFFECT_TARGET.ACTOR) {
                    GoapAction lowestCostAction = null;
                    IPointOfInterest lowestCostTarget = null;
                    int lowestCost = 0;
#if DEBUG_LOG
                    log += $"\n--Choices for {preconditionEffect}";
                    log += "\n--";
#endif
                    ProcessFindingLowestCostActionAndTarget(job, preconditionEffect, target, actionsCategorizedByEffect, ref lowestCost, ref lowestCostAction, ref lowestCostTarget, ref log);
                    if (lowestCostAction != null) {
#if DEBUG_LOG
                        log += $"\n--Found action: {lowestCostAction.goapName}, creating new node...";
#endif
                        GoapNode leafNode = SetGoapNodeCacheData(lowestCost, node.level + 1, lowestCostAction, lowestCostTarget);
                        BuildGoapTree(leafNode, actor, job, rawPlan, actionsCategorizedByEffect, ref log); //, ref log
                    } else {
                        //Fail - rawPlan must be set to null so the plan will fail
                        rawPlan.Clear();
#if DEBUG_LOG
                        log += "\n--Could not find action to satisfy precondition, setting raw plan to null and exiting goap tree...";
#endif
                        failedPrecondition = precondition;
                        failedPreconditionActionType = action.goapType;
                        return;
                    }
                }
            } else {
#if DEBUG_LOG
                log += $"\n--Precondition satisfied, exit goap tree...";
#endif
            }
        } else {
#if DEBUG_LOG
            log += $"\n--Node {node.action.goapName} has no preconditions, exiting goap tree...";
#endif
        }
    }
    private void ProcessFindingLowestCostActionAndTarget(GoapPlanJob job, GoapEffect goalEffect, IPointOfInterest target, Dictionary<GOAP_EFFECT_CONDITION, List<GoapAction>> actionsCategorizedByEffect, ref int lowestCost, ref GoapAction lowestCostAction, ref IPointOfInterest lowestCostTarget, ref string log) {
        if (!actionsCategorizedByEffect.ContainsKey(goalEffect.conditionType)) {
            return;
        }

        List<GoapAction> actionsFilteredByEffect = actionsCategorizedByEffect[goalEffect.conditionType];
        for (int i = 0; i < actionsFilteredByEffect.Count; i++) {
            GoapAction currentAction = actionsFilteredByEffect[i];
            IPointOfInterest poiTarget = target;
            bool isJobTargetEvaluated = false;

            SetLowestCostActionBasedOnLocationAwareness(job, currentAction, goalEffect, ref isJobTargetEvaluated, ref lowestCost, ref lowestCostAction, ref lowestCostTarget, ref log);

            if (!isJobTargetEvaluated) {
                if (poiTarget == job.targetPOI || poiTarget.IsStillConsideredPartOfAwarenessByCharacter(owner)) { //POI must either be the job's target or the actor is still aware of it
                    if (poiTarget.Advertises(currentAction.goapType)) {
                        PopulateLowestCostAction(poiTarget, currentAction, job, goalEffect, ref lowestCost, ref lowestCostAction, ref lowestCostTarget, ref log);
                    }
                }
            }
        }
    }
    private void SetLowestCostActionBasedOnLocationAwareness(GoapPlanJob job, GoapAction action, GoapEffect goalEffect, ref bool isJobTargetEvaluated, ref int lowestCost, ref GoapAction lowestCostAction, ref IPointOfInterest lowestCostTarget, ref string log) {
        //TODO: Job Location Parameter
        List<ILocation> priorityLocations = job.GetPriorityLocationsFor(action.goapType);
        if (priorityLocations != null) {
            bool hasSet = false;
            for (int i = 0; i < priorityLocations.Count; i++) {
                ILocation location = priorityLocations[i];
                if (location is BaseSettlement settlement) {
                    for (int j = 0; j < settlement.allStructures.Count; j++) {
                        LocationStructure structure = settlement.allStructures[j];
                        if (SetLowestCostActionGivenLocationAwareness(structure.locationAwareness, job, action, goalEffect, ref isJobTargetEvaluated, ref lowestCost, ref lowestCostAction, ref lowestCostTarget, ref log)) {
                            hasSet = true;
                        }
                    }
                    for (int j = 0; j < settlement.areas.Count; j++) {
                        Area area = settlement.areas[j];
                        if (SetLowestCostActionGivenLocationAwareness(area.locationAwareness, job, action, goalEffect, ref isJobTargetEvaluated, ref lowestCost, ref lowestCostAction, ref lowestCostTarget, ref log)) {
                            hasSet = true;
                        }
                    }
                } else if (location is LocationStructure structure) {
                    if (SetLowestCostActionGivenLocationAwareness(structure.locationAwareness, job, action, goalEffect, ref isJobTargetEvaluated, ref lowestCost, ref lowestCostAction, ref lowestCostTarget, ref log)) {
                        hasSet = true;
                    }
                } else if (location is Area area) {
                    if (SetLowestCostActionGivenLocationAwareness(area.locationAwareness, job, action, goalEffect, ref isJobTargetEvaluated, ref lowestCost, ref lowestCostAction, ref lowestCostTarget, ref log)) {
                        hasSet = true;
                    }
                }
            }
            if (hasSet) {
                return;
            }
        }

        LocationGridTile currentGridTile = owner.gridTileLocation;
        if (currentGridTile != null) {
            LocationStructure currentStructure = currentGridTile.structure;
            //BaseSettlement currentSettlement = null;
            //currentGridTile.IsPartOfSettlement(out currentSettlement);

            //if(currentSettlement != null && currentSettlement == owner.homeSettlement) {

            //}

            //First step: Process current structure, if there is an action, skip next processing
            if (currentStructure.structureType != STRUCTURE_TYPE.WILDERNESS && currentStructure.structureType != STRUCTURE_TYPE.OCEAN) {
                bool hasSet = SetLowestCostActionGivenLocationAwareness(currentStructure.locationAwareness, job, action, goalEffect, ref isJobTargetEvaluated, ref lowestCost, ref lowestCostAction, ref lowestCostTarget, ref log);
                if (hasSet) {
                    return;
                }
            }

            Area currentArea = currentGridTile.area;

            if (currentArea != null) {
                //Second step: Process current hex, if there is an action, skip next processing
                bool hasSet = SetLowestCostActionGivenLocationAwareness(currentArea.locationAwareness, job, action, goalEffect, ref isJobTargetEvaluated, ref lowestCost, ref lowestCostAction, ref lowestCostTarget, ref log);
                if (hasSet) {
                    return;
                }

                //Second step: Process adjacent hexes, if there is an action, skip next processing
                List<Area> neighbours = currentArea.neighbourComponent.neighbours;
                for (int i = 0; i < neighbours.Count; i++) {
                    Area neighbour = neighbours[i];
                    SetLowestCostActionGivenLocationAwareness(neighbour.locationAwareness, job, action, goalEffect, ref isJobTargetEvaluated, ref lowestCost, ref lowestCostAction, ref lowestCostTarget, ref log);
                }
            }
        }
    }
    private bool SetLowestCostActionGivenLocationAwareness(ILocationAwareness locationAwareness, GoapPlanJob job, GoapAction action, GoapEffect goalEffect, ref bool isJobTargetEvaluated, ref int lowestCost, ref GoapAction lowestCostAction, ref IPointOfInterest lowestCostTarget, ref string log) {
        bool hasSetAsLowest = false;
        lock (MultiThreadPool.THREAD_LOCKER) {
            List<IPointOfInterest> pois = locationAwareness.GetListOfPOIBasedOnActionType(action.goapType);
            if (pois != null) {
                for (int i = 0; i < pois.Count; i++) {
                    IPointOfInterest poi = pois[i];
                    if (poi == job.targetPOI) {
                        isJobTargetEvaluated = true;
                    }
                    if (poi.IsStillConsideredPartOfAwarenessByCharacter(owner)) { //POI must either be the job's target or the actor is still aware of it
                        if (PopulateLowestCostAction(poi, action, job, goalEffect, ref lowestCost, ref lowestCostAction, ref lowestCostTarget, ref log)) {
                            hasSetAsLowest = true;
                        }
                    }
                }
            }
        }
        return hasSetAsLowest;
    }
    private bool PopulateLowestCostAction(IPointOfInterest poiTarget, GoapAction action, GoapPlanJob job, GoapEffect goalEffect, ref int lowestCost, ref GoapAction lowestCostAction, ref IPointOfInterest lowestCostTarget, ref string log) {
        bool canActionBeDone = CanActionBeDoneOn(poiTarget, action, job, goalEffect);
        if (canActionBeDone) {
            int cost = action.GetCost(owner, poiTarget, job);
#if DEBUG_LOG
            log = $"{log}({cost}){action.goapName}-{poiTarget.nameWithID}, ";
#endif
            if (lowestCostAction == null || cost < lowestCost) {
                lowestCostAction = action;
                lowestCostTarget = poiTarget;
                lowestCost = cost;
                return true;
            }
        }
        return false;
    }
    private bool CanActionBeDoneOn(IPointOfInterest poiTarget, GoapAction action, GoapPlanJob job, GoapEffect goalEffect) {
        bool state = poiTarget.CanAdvertiseActionToActor(owner, action, job) && action.WillEffectsSatisfyPrecondition(goalEffect, owner, poiTarget, job);
        return state;
    }
    private List<JobNode> TransformRawPlanToActualNodes(List<GoapNode> rawPlan, GoapPlanJob job, GoapPlan currentPlan = null) { //actualPlan is for recalculation only, so that it will no longer create a new list, since in recalculation we already have a list of job nodes
        List<JobNode> actualPlan;
        int index = 0;
        if (currentPlan == null) {
            actualPlan = RuinarchListPool<JobNode>.Claim();
        } else {
            actualPlan = currentPlan.allNodes;
            actualPlan.RemoveRange(0, currentPlan.currentNodeIndex + 1); //It's +1 because we want to remove also the current node of the actual plan since it is already in the rawPlan
            index = currentPlan.currentNodeIndex;
        }
        List<int> tempNodeIndexHolder = RuinarchListPool<int>.Claim();
        List<GoapNode> discardedNodes = RuinarchListPool<GoapNode>.Claim();
        while (rawPlan.Count > 0) {
            tempNodeIndexHolder.Clear();
            for (int i = 0; i < rawPlan.Count; i++) {
                GoapNode rawNode = rawPlan[i];
                if (rawNode.level == index) {
                    tempNodeIndexHolder.Add(i);
                }
            }
            if (tempNodeIndexHolder.Count > 0) {
                int nodeIndex = tempNodeIndexHolder[0];
                GoapNode rawNode = rawPlan[nodeIndex];
                if (rawNode.action == null) {
                    Debug.LogError("Null action in raw plan");
                }
                OtherData[] data = job.GetOtherDataFor(rawNode.action.goapType);
                ActualGoapNode actualNode = ObjectPoolManager.Instance.CreateNewAction(rawNode.action, owner, rawNode.target, data, rawNode.cost);
                //SingleJobNode singleJobNode = new SingleJobNode(actualNode);
                SingleJobNode singleJobNode = ObjectPoolManager.Instance.CreateNewSingleJobNode();
                singleJobNode.SetActionNode(actualNode);
                actualPlan.Insert(0, singleJobNode);
                rawPlan.RemoveAt(nodeIndex);
            }
            index++;
        }
        RuinarchListPool<int>.Release(tempNodeIndexHolder);
        RuinarchListPool<GoapNode>.Release(discardedNodes);
        return actualPlan;
    }

    #region Goap Node Cache
    private void CreateGoapNodeCache() {
        if (_cachedGoapNodes.Count > 0) {
            Debug.LogError("Creating cache but there is still cached data");
        } else {
            for (int i = 0; i < CACHED_GOAP_NODE_CAPACITY; i++) {
                _cachedGoapNodes.Add(ObjectPoolManager.Instance.CreateNewGoapNode());
            }
        }
    }
    private void ResetGoapNodeCache() {
        for (int i = 0; i < _cachedGoapNodes.Count; i++) {
            ObjectPoolManager.Instance.ReturnGoapNodeToPool(_cachedGoapNodes[i]);
        }
        _cachedGoapNodes.Clear();
    }
    private GoapNode SetGoapNodeCacheData(int cost, int level, GoapAction action, IPointOfInterest target) {
        for (int i = 0; i < _cachedGoapNodes.Count; i++) {
            GoapNode gn = _cachedGoapNodes[i];
            if (gn.action == null) {
                gn.Initialize(cost, level, action, target);
                return gn;
            }
        }
        throw new System.Exception("Cached goap nodes are fully filled up, need to adjust max capacity");
    }
    #endregion
}