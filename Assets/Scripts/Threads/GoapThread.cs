using System;
using System.Threading;
using UnityEngine;

public class GoapThread : Multithread {
    public Character actor { get; private set; }
    public GoapPlan createdPlan { get; private set; }
    public GoapEffect goalEffect { get; private set; }
    public GoapAction goalAction { get; private set; }
    public INTERACTION_TYPE goalType { get; private set; }
    public IPointOfInterest target { get; private set; }
    public bool isPersonalPlan { get; private set; }
    public GoapPlanJob job { get; private set; }
    public string log { get; private set; }

    //For recalculation
    public GoapPlan recalculationPlan;

    private Character owner;

    private Timer timer;

    public void Initialize(Character actor, IPointOfInterest target, GoapEffect goalEffect, bool isPersonalPlan, GoapPlanJob job) {//, List<INTERACTION_TYPE> actorAllowedActions, List<GoapAction> usableActions
        this.createdPlan = null;
        this.recalculationPlan = null;
        this.actor = actor;
        this.target = target;
        this.goalEffect = goalEffect;
        this.isPersonalPlan = isPersonalPlan;
        this.job = job;
        owner = actor;
    }
    public void Initialize(Character actor, IPointOfInterest target, GoapAction goalAction
        , bool isPersonalPlan, GoapPlanJob job) {//, List<INTERACTION_TYPE> actorAllowedActions, List<GoapAction> usableActions
        this.createdPlan = null;
        this.recalculationPlan = null;
        this.actor = actor;
        this.target = target;
        this.goalAction = goalAction;
        this.isPersonalPlan = isPersonalPlan;
        this.job = job;
        owner = actor;
    }
    public void Initialize(Character actor, INTERACTION_TYPE goalType, IPointOfInterest target
        , bool isPersonalPlan, GoapPlanJob job) {//, List<INTERACTION_TYPE> actorAllowedActions, List<GoapAction> usableActions
        this.createdPlan = null;
        this.recalculationPlan = null;
        this.actor = actor;
        this.target = target;
        this.goalType = goalType;
        this.isPersonalPlan = isPersonalPlan;
        this.job = job;

        owner = actor;
    }
    public void Initialize(Character actor, GoapPlan currentPlan, GoapPlanJob job) {//, List<GoapAction> usableActions
        this.createdPlan = null;
        this.actor = actor;
        this.recalculationPlan = currentPlan;
        this.job = job;
        owner = actor;
    }
    
    #region Overrides
    public override void DoMultithread() {
        base.DoMultithread();
        try {
            CreatePlan();
        } catch(System.Exception e) {
            Debug.unityLogger.LogError("Error", $"Problem with {actor.name}'s GoapThread! \nJob is {(job?.jobType.ToString() ?? "None")}\nTarget is {target.name}\n{e.Message}\n{e.StackTrace}");
        }
    }
    public override void FinishMultithread() {
        base.FinishMultithread();
        ReturnPlanFromGoapThread();
    }
    #endregion

    public void CreatePlan() {
        timer = new Timer(TimerCallback, null, 1000, 20000);
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
    }

    public void ReturnPlanFromGoapThread() {
        if (timer != null) {
            timer.Change(Timeout.Infinite, Timeout.Infinite);
            timer.Dispose();
            timer = null;
        }
        actor.planner.ReceivePlanFromGoapThread(this);
    }

    #region Timer
    private void TimerCallback(object state) {
        try {
            timer.Change(Timeout.Infinite, Timeout.Infinite);
            timer.Dispose();
            timer = null;
            if (Application.isPlaying) {
                throw new Exception();    
            }
        } catch {
            //Debug.unityLogger.LogError("Error", $"{actor.name}'s GoapThread has exceeded 20 seconds! " +
            //                                    $"\nJob is {(job?.jobType.ToString() ?? "None")}" +
            //                                    $"\nTarget is {target.name}" +
            //                                    $"\nTarget action is {goalType.ToString()}" +
            //                                    $"\nTarget effect is {goalEffect.ToString()}");
        }
    }
    #endregion

    #region Object Pool
    public void Reset() {
        actor = null;
        createdPlan = null;
        goalEffect = default;
        goalAction = null;
        goalType = INTERACTION_TYPE.NONE;
        target = null;
        isPersonalPlan = false;
        job = null;
        log = null;

        timer?.Dispose();
        timer = null;

        //For recalculation
        recalculationPlan = null;
        owner = null;
    }
    #endregion
}
