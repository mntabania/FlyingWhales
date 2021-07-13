using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class GoapThread : Multithread {
    public Character actor { get; private set; }
    public GoapEffect goalEffect { get; private set; }
    public INTERACTION_TYPE goalType { get; private set; }
    public IPointOfInterest target { get; private set; }
    public bool isPersonalPlan { get; private set; }
    public GoapPlanJob job { get; private set; }
    public string log { get; private set; }

    //For recalculation
    public GoapPlan recalculationPlan;
    public bool isRecalculationSuccess { get; private set; }

    private Character owner;

    public void Initialize(Character actor, IPointOfInterest target, GoapEffect goalEffect, bool isPersonalPlan, GoapPlanJob job) {//, List<INTERACTION_TYPE> actorAllowedActions, List<GoapAction> usableActions
        this.recalculationPlan = null;
        this.actor = actor;
        this.target = target;
        this.goalEffect = goalEffect;
        this.isPersonalPlan = isPersonalPlan;
        this.job = job;
        owner = actor;
        isRecalculationSuccess = false;
    }
    public void Initialize(Character actor, INTERACTION_TYPE goalType, IPointOfInterest target, bool isPersonalPlan, GoapPlanJob job) {//, List<INTERACTION_TYPE> actorAllowedActions, List<GoapAction> usableActions
        this.recalculationPlan = null;
        this.actor = actor;
        this.target = target;
        this.goalType = goalType;
        this.isPersonalPlan = isPersonalPlan;
        this.job = job;
        owner = actor;
        isRecalculationSuccess = false;
    }
    public void InitializeForRecalculation(Character actor, GoapPlan currentPlan, GoapPlanJob job) {//, List<GoapAction> usableActions
        this.actor = actor;
        this.recalculationPlan = currentPlan;
        this.job = job;
        owner = actor;
        isRecalculationSuccess = false;
    }

    #region Overrides
    public override void DoMultithread() {
        base.DoMultithread();
        try {
            CreatePlan();
        } catch(System.Exception e) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            throw new Exception($"Problem with {actor?.name}'s GoapThread! \nJob is {(job?.jobType.ToString() ?? "None")}\nTarget is {target?.name}\n{e.Message}\n{e.StackTrace}");
#else
            Debug.unityLogger.LogError("Error", $"Problem with {actor.name}'s GoapThread! \nJob is {(job?.jobType.ToString() ?? "None")}\nTarget is {target.name}\n{e.Message}\n{e.StackTrace}");
#endif
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
        //UtilityScripts.LocationAwarenessUtility.UpdateAllPendingAwareness();
        //removed by aaron for awareness update GridMap.Instance.UpdateAwarenessInAllRegions();
    }
    private void CreateNewPlan() {
#if DEBUG_LOG
        log = $"-----------------RECEIVING NEW PLAN FROM OTHER THREAD OF {actor.name} WITH TARGET {target?.nameWithID}" ??
              $"None ({actor.currentRegion.name})-----------------------";
        if (goalType != INTERACTION_TYPE.NONE) {
            log += $"\nGOAL: {goalType}";
        } else {
            log += $"\nGOAL: {goalEffect}";
        }
        if(job.priorityLocations != null) {
            log += $"\nPRIORITY LOCATIONS:";
            foreach (KeyValuePair<INTERACTION_TYPE, List<ILocation>> kvp in job.priorityLocations) {
                log += $"\n{kvp.Key}: ";
                for (int i = 0; i < kvp.Value.Count; i++) {
                    if (i > 0) {
                        log += ", ";
                    }
                    log += kvp.Value[i].locationName;
                }
            }
        }
#endif

        string planLog = string.Empty;
        GoapPlan plan = null;
        if (goalType != INTERACTION_TYPE.NONE) {
            //provided goal type
            GoapAction action = InteractionManager.Instance.goapActionData[goalType];
            if (target.CanAdvertiseActionToActor(actor, action, job) && target.Advertises(goalType)) {
#if DEBUG_LOG
                log += $"\n{target.name} Can advertise actions to {actor.name}";
#endif
                actor.planner.PlanActions(target, action, isPersonalPlan, ref planLog, job, this);
            } else {
#if DEBUG_LOG
                log += $"\n{target.name} Cannot advertise actions to {actor.name}";
#endif
            }
        } else {
            //default
            actor.planner.PlanActions(target, goalEffect, isPersonalPlan, ref planLog, job, this);
        }
#if DEBUG_LOG
        log += $"\nGOAP TREE LOG: {planLog}";
#endif
    }
    private void RecalculatePlan() {
#if DEBUG_LOG
        log =
            $"-----------------RECALCULATING PLAN OF {actor.name} WITH TARGET {recalculationPlan.target.name} ({actor.currentRegion.name})-----------------------";
#endif
        if (recalculationPlan.isEnd) {
#if DEBUG_LOG
            log += "\nPlan has already ended! Cannot recalculate!";
#endif
            return;
        }
#if DEBUG_LOG
        log +=
            $"\nGOAL ACTION: {recalculationPlan.endNode.singleNode.action.goapName} - {recalculationPlan.target.name}";
#endif
        string planLog = string.Empty;
        isRecalculationSuccess = actor.planner.RecalculatePathForPlan(recalculationPlan, job, ref planLog);
#if DEBUG_LOG
        log += $"\nGOAP TREE LOG: {planLog}";
#endif
    }

    public void ReturnPlanFromGoapThread() {
        GoapPlan createdPlan = actor.planner.TransformRawPlanToActualPlan();
        actor.planner.ReceivePlanFromGoapThread(createdPlan);
    }

#region Object Pool
    public void Reset() {
        actor = null;
        goalEffect = default;
        goalType = INTERACTION_TYPE.NONE;
        target = null;
        isPersonalPlan = false;
        job = null;
        log = null;
        
        //For recalculation
        recalculationPlan = null;
        isRecalculationSuccess = false;
        owner = null;
    }
#endregion
}
