using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoapPlan {

    //public string name { get; private set; }
    public IPointOfInterest target { get; private set; }
    public JobNode startingNode { get; private set; }
    public JobNode endNode { get; private set; } //IMPORTANT! End node must always be Single Job Node
    public JobNode currentNode { get; private set; }
    public JobNode previousNode { get; private set; }
    public ActualGoapNode currentActualNode { get { return GetCurrentActualNode(); } }
    public List<JobNode> allNodes { get; private set; }
    public int currentNodeIndex { get; private set; }
    //public ActualGoapNode currentActualNode { get; private set; }
    //public GOAP_EFFECT_CONDITION[] goalEffects { get; private set; }
    //public List<IPointOfInterest> goalCharacterTargets { get; private set; } ////Only characters in the same structure and characters in this list are allowed to advertise actions even if they are part of the awareness list of the actor
    public bool isEnd { get; private set; }
    public bool isBeingRecalculated { get; private set; }
    public bool isPersonalPlan { get; private set; }
    public bool doNotRecalculate { get; private set; }
    //public bool hasShownNotification { get; private set; }
    public GOAP_PLAN_STATE state { get; private set; }
    //public GOAP_CATEGORY category { get; private set; }
    //public GoapPlanJob job { get; private set; }
    //public bool isPriority { get; private set; }
    public GoapPlan(List<JobNode> nodes, IPointOfInterest target, bool isPersonalPlan = true) {
        this.startingNode = nodes[0];
        this.endNode = nodes[nodes.Count - 1];
        this.currentNode = startingNode;
        currentNodeIndex = 0;
        this.target = target;
        //this.goalEffects = goalEffects;
        this.isPersonalPlan = isPersonalPlan;
        //this.category = category;
        this.doNotRecalculate = false;
        //hasShownNotification = false;
        allNodes = nodes;
        //ConstructAllNodes();
    }

    public void Reset(List<JobNode> nodes) {
        this.startingNode = nodes[0];
        this.endNode = nodes[nodes.Count - 1];
        this.currentNode = startingNode;
        currentNodeIndex = 0;
        allNodes = nodes;
    }
    public void SetNextNode() {
        if (currentNode == null) {
            return;
        }
        if(currentNode.singleNode != null || currentNode.currentNodeIndex >= (currentNode.multiNode.Length - 1)) {
            previousNode = currentNode;
            int nextNodeIndex = currentNodeIndex + 1;
            if(nextNodeIndex < allNodes.Count) {
                currentNode = allNodes[nextNodeIndex];
                currentNodeIndex = nextNodeIndex;
            } else {
                currentNode = null;
            }
        } else {
            currentNode.SetNextActualNode();
        }
    }
    private ActualGoapNode GetCurrentActualNode() {
        if(currentNode.singleNode != null) {
            return currentNode.singleNode;
        } else {
            return currentNode.multiNode[currentNode.currentNodeIndex];
        }
    }
    public void EndPlan() {
        isEnd = true;
        startingNode = null;
        currentNode = null;
        previousNode = null;
        endNode = null;
        allNodes.Clear();
        //if this plan was ended, and it's state has not been set to failed or success, this means that this plan was not completed.
        if (state == GOAP_PLAN_STATE.IN_PROGRESS) SetPlanState(GOAP_PLAN_STATE.CANCELLED);
        //Messenger.RemoveListener<Character, GoapAction, string>(Signals.CHARACTER_FINISHED_ACTION, OnActionInPlanFinished);
        //dropPlanCallStack = StackTraceUtility.ExtractStackTrace();
    }
    //public void InsertAction(GoapAction action) {
    //    if (currentNode != null) {
    //        GoapNode nextNode = currentNode.parent;
    //        GoapNode newNode = new GoapNode(nextNode, action.cost, action);
    //        currentNode.parent = newNode;
    //        newNode.index = currentNode.index;
    //        newNode.actionType.SetParentPlan(this);
    //        //Debug.Log(action.actor.name + " inserted new action " + action.goapName + " to replace action that returned fail. New plan is\n" + GetPlanSummary());
    //    }
    //}

    //public void ConstructAllNodes() {
    //    allNodes.Clear();
    //    ActualGoapNode node = startingNode;
    //    //node.actionType.SetParentPlan(this);
    //    //node.index = allNodes.Count;
    //    allNodes.Add(node);
    //    while (node.parent != null) {
    //        node = node.parent;
    //        node.actionType.SetParentPlan(this);
    //        node.index = allNodes.Count;
    //        allNodes.Add(node);
    //    }
    //    endNode = node;
    //    name = "Plan of " +  endNode.action.actor.name + " to do " + endNode.action.goapName + " targetting " + target.name;
    //}
    //public int GetNumOfNodes() {
    //    if(allNodes.Count > 0) {
    //        return allNodes.Count;
    //    }
    //    int count = 1;
    //    GoapNode node = startingNode;
    //    while (node.parent != null) {
    //        node = node.parent;
    //        count++;
    //    }
    //    return count;
    //}

    //public void SetListOfCharacterAwareness(List<IPointOfInterest> list) {
    //    goalCharacterTargets = list;
    //}
    public void SetIsBeingRecalculated(bool state) {
        isBeingRecalculated = state;
    }
    public void SetDoNotRecalculate(bool state) {
        doNotRecalculate = state;
    }
    //public void SetHasShownNotification(bool state) {
    //    hasShownNotification = state;
    //}
    public void SetPlanState(GOAP_PLAN_STATE state) {
        this.state = state;
    }
    public void OnAttachPlanToJob(GoapPlanJob job) {
        for (int i = 0; i < allNodes.Count; i++) {
            allNodes[i].OnAttachPlanToJob(job);
        }
    }
    public void OnUnattachPlanToJob(GoapPlanJob job) {
        for (int i = 0; i < allNodes.Count; i++) {
            allNodes[i].OnUnattachPlanToJob(job);
        }
    }
    public string LogPlan() {
        //string log = "\n--------------------- PLAN OF " + endNode.singleNode.actor.name + " FOR " + endNode.singleNode.action.goapName + " WITH TARGET " + target.name + " (" + endNode.singleNode.actor.specificLocation.name + ")--------------------------";
        string log = string.Empty;
        for (int i = 0; i < allNodes.Count; i++) {
            JobNode jobNode = allNodes[i];
            if(i > 0) {
                log += "\n";
            }
            log += $"{(i + 1)}.";
            if (jobNode.singleNode != null) {
                ActualGoapNode node = jobNode.singleNode;
                log += $" ({node.cost}) {node.action.goapName} - {node.poiTarget.name}";
            } else {
                for (int j = 0; j < jobNode.multiNode.Length; j++) {
                    ActualGoapNode node = jobNode.multiNode[j];
                    if (j > 0) {
                        log += ",";
                    }
                    log += $" ({node.cost}) {node.action.goapName} - {node.poiTarget.name}";
                }
            }
        }
        return log;
    }
    public string GetPlanSummary() {
        string summary = GetGoalSummary();
        summary += "\nPlanned Actions are: ";
        summary += LogPlan();
        return summary;
    }
    public string GetGoalSummary() {
        string summary = "Goal: ";
        for (int i = 0; i < endNode.singleNode.action.baseExpectedEffects.Count; i++) {
            GoapEffect effect = endNode.singleNode.action.baseExpectedEffects[i];
            summary += $"{effect}, ";
        }
        return summary;
    }
    //public void OnActionInPlanFinished(Character actor, GoapAction action, string result) {
    //    if (endNode == null || action == endNode.action) {
    //        if (result == InteractionManager.Goap_State_Success) {
    //            SetPlanState(GOAP_PLAN_STATE.SUCCESS);
    //        } else if (result == InteractionManager.Goap_State_Fail) {
    //            SetPlanState(GOAP_PLAN_STATE.FAILED);
    //        }
    //    }
    //}
    //public void SetPriorityState(bool state) {
    //    isPriority = state;
    //}
}
