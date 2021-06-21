using System;
using System.Collections;
using System.Collections.Generic;
using Logs;
using UnityEngine;

public class GoapActionState {

    public GoapAction parentAction { get; private set; }
	public string name { get; private set; }
    public int duration { get; private set; } //if 0, go instantly to after effect, if -1, endless (can only be ended manually)
    public Action<ActualGoapNode> preEffect { get; private set; }
    public Action<ActualGoapNode> perTickEffect { get; private set; }
    public Action<ActualGoapNode> afterEffect { get; private set; }
    public string status { get; private set; }
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
    }

    #region Logs
    public Log CreateDescriptionLog(ActualGoapNode goapNode) {
        string actionName = parentAction.goapName;
        string stateNameLowercase = name.ToLower();
        if (LocalizationManager.Instance.HasLocalizedValue("GoapAction", actionName, $"{stateNameLowercase}_description")) {
            Log _descriptionLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "GoapAction", actionName, $"{stateNameLowercase}_description", goapNode.logTags, goapNode);
            goapNode.action.AddFillersToLog(_descriptionLog, goapNode);
            return _descriptionLog;
        } else {
            Debug.LogWarning($"{this.name} had problems creating it's description log");
        }
        return default;
    }
    #endregion

    public override string ToString() {
        return $"{name} {parentAction}";
    }
}

public struct ActionLog {
    public Log log;
    public System.Action notifAction;
}