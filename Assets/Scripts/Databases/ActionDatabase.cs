using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionDatabase {
    public Dictionary<string, ActualGoapNode> allActions { get; }

    public ActionDatabase() {
        allActions = new Dictionary<string, ActualGoapNode>();
    }

    public void AddAction(ActualGoapNode action) {
        if (!allActions.ContainsKey(action.persistentID)) {
            allActions.Add(action.persistentID, action);
        }
    }
    public ActualGoapNode GetActionByPersistentID(string id) {
        if (allActions.ContainsKey(id)) {
            return allActions[id];
        } else {
            throw new System.NullReferenceException("Trying to get an action from the database with id " + id + " but the action is not loaded");
        }
    }
}
