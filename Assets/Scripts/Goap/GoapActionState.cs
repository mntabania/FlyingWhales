using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoapActionState {

    public GoapAction parentAction { get; private set; }
	public string name { get; private set; }
    public int duration { get; private set; } //if 0, go instantly to after effect, if -1, endless (can only be ended manually)
    public Action<ActualGoapNode> preEffect { get; private set; }
    public Action<ActualGoapNode> perTickEffect { get; private set; }
    public Action<ActualGoapNode> afterEffect { get; private set; }
    public Func<Character, ActionIntel, REACTION_STATUS, List<string>> shareIntelReaction { get; private set; }
    public string status { get; private set; }
    //public bool isDone { get; private set; }
    public string animationName { get; private set; } //specific animation per action state

    public bool hasPerTickEffect { get { return perTickEffect != null; } }
    public int currentDuration { get; private set; }

    //public List<ActionLog> arrangedLogs { get; protected set; }

    public GoapActionState(string name, GoapAction parentAction, Action<ActualGoapNode> preEffect, Action<ActualGoapNode> perTickEffect, Action<ActualGoapNode> afterEffect, int duration, string status, string animationName) {
        this.name = name;
        this.preEffect = preEffect;
        this.perTickEffect = perTickEffect;
        this.afterEffect = afterEffect;
        this.parentAction = parentAction;
        this.duration = duration;
        this.status = status;
        this.animationName = animationName;
        //this.shouldAddLogs = true;
        //this.isDone = false;
        //this.arrangedLogs = new List<ActionLog>();
        //CreateLog();
    }

    public void SetIntelReaction(Func<Character, ActionIntel, REACTION_STATUS, List<string>> intelReaction) {
        shareIntelReaction = intelReaction;
    }

    //#region Duration
    //public void OverrideDuration(int newDuration) {
    //    this.duration = newDuration;
    //}
    //#endregion

    #region Logs
    public Log CreateDescriptionLog(Character actor, IPointOfInterest poiTarget, ActualGoapNode goapNode) {
        string actionName = parentAction.goapName;
        string stateNameLowercase = name.ToLower();
        if (LocalizationManager.Instance.HasLocalizedValue("GoapAction", actionName,
            $"{stateNameLowercase}_description")) {
            Log _descriptionLog = new Log(GameManager.Instance.Today(), "GoapAction", actionName,
                $"{stateNameLowercase}_description", goapNode);
            if (goapNode != null) {
                _descriptionLog.SetLogType(LOG_TYPE.Action);
            }
            goapNode.action.AddFillersToLog(_descriptionLog, goapNode);
            return _descriptionLog;
        } else {
            Debug.LogWarning($"{this.name} had problems creating it's description log");
        }
        return null;
    }
    //public void AddLogFiller(object obj, string value, LOG_IDENTIFIER identifier) {
    //    descriptionLog.AddToFillers(obj, value, identifier);
    //}
    //public void AddArrangedLog(string priorityID, Log log, System.Action notifAction) {
    //    int index = parentAction.GetArrangedLogPriorityIndex(priorityID);
    //    if(index == -1 || arrangedLogs.Count <= index) {
    //        arrangedLogs.Add(new ActionLog() { log = log, notifAction = notifAction });
    //    } else {
    //        arrangedLogs.Insert(index, new ActionLog() { log = log, notifAction = notifAction });
    //    }
    //}
    #endregion

    //public void Execute(Character actor, Character target, object[] otherData) {
    //    preEffect?.Invoke(actor, target, otherData);
    //    //parentAction.SetExecutionDate(GameManager.Instance.Today());

    //    if(duration > 0) {
    //        currentDuration = 0;
    //        StartPerTickEffect();
    //    } else if (duration != -1){
    //        EndPerTickEffect();
    //    }
    //}
    //private void StartPerTickEffect() {
    //    Messenger.AddListener(Signals.TICK_STARTED, PerTickEffect);
    //}
    //public void EndPerTickEffect(bool shouldDoAfterEffect = true) {
    //    //if (isDone) {
    //    //    return;
    //    //}
    //    //isDone = true;
    //    parentAction.ReturnToActorTheActionResult(status);
    //    if (shouldDoAfterEffect) {
    //        if (afterEffect != null) {
    //            afterEffect();
    //        }
    //        if (parentAction.shouldAddLogs && this.shouldAddLogs) { //only add logs if both the parent action and this state should add logs
    //            if (descriptionLog != null) {
    //                AddArrangedLog("description", descriptionLog, null);
    //            }
    //            for (int i = 0; i < arrangedLogs.Count; i++) {
    //                arrangedLogs[i].log.SetDate(GameManager.Instance.Today());
    //                arrangedLogs[i].log.AddLogToInvolvedObjects();
    //            }
    //        }
    //    } else {
    //        parentAction.SetShowIntelNotification(false);
    //    }
    //    parentAction.actor.OnCharacterDoAction(parentAction); //Moved this here to fix intel not being shown, because arranged logs are not added until after the ReturnToActorTheActionResult() call.
    //    if (shouldDoAfterEffect) {
    //        parentAction.AfterAfterEffect();
    //    }
    //}
    //public void StopPerTickEffect() {
    //    Messenger.RemoveListener(Signals.TICK_STARTED, PerTickEffect);
    //}
    //private void PerTickEffect() {
    //    currentDuration++;
    //    if (perTickEffect != null) {
    //        perTickEffect();
    //    }
    //    if(currentDuration >= duration) {
    //        EndPerTickEffect();
    //    }
    //}

    public override string ToString() {
        return $"{name} {parentAction}";
    }
}

public struct ActionLog {
    public Log log;
    public System.Action notifAction;
}