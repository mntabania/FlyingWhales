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
    public void RemoveAction(string action) {
        if (allActions.ContainsKey(action)) {
            allActions.Remove(action);
        }
    }
    public ActualGoapNode GetActionByPersistentID(string id) {
        if (allActions.ContainsKey(id)) {
            return allActions[id];
        } else {
            throw new System.NullReferenceException("Trying to get an action from the database with id " + id + " but the action is not loaded");
        }
        //return null;
        //No longer throws exception because action can be returned as null since corrupted actions are deleted in database
        //Corrupted = meaning the action is no longer viable because the actor/target might be null even if they are not initially null, meaning the actor/target is already deleted in database
        //else {
        //    throw new System.NullReferenceException("Trying to get an action from the database with id " + id + " but the action is not loaded");
        //}
    }
}
