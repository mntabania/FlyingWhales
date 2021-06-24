using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;
public class GoapPlan: IObjectPoolTester {

    //public string name { get; private set; }
    // public string id { get; private set; }
    public IPointOfInterest target { get; private set; }
    public JobNode startingNode { get; private set; }
    public JobNode endNode { get; private set; } //IMPORTANT! End node must always be Single Job Node
    public JobNode currentNode { get; private set; }
    public JobNode previousNode { get; private set; }
    public ActualGoapNode currentActualNode { get { return GetCurrentActualNode(); } }
    public List<JobNode> allNodes { get; private set; }
    public int currentNodeIndex { get; private set; }
    public bool isEnd { get; private set; }
    public bool isBeingRecalculated { get; private set; }
    public bool isPersonalPlan { get; private set; }
    public bool doNotRecalculate { get; private set; }
    public GOAP_PLAN_STATE state { get; private set; }
    public bool resetPlanOnFinishRecalculation { get; private set; }
    public bool isAssigned { get; set; }

    public GoapPlan() {
        // id = UtilityScripts.Utilities.SetID(this).ToString();
    }
    public GoapPlan(SaveDataGoapPlan data) {
        if (!string.IsNullOrEmpty(data.poiTargetID)) {
            if (data.targetObjectType == OBJECT_TYPE.Character) {
                target = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(data.poiTargetID);
            } else {
                //it is assumed that the target of the plan is a tile object if it is not a character.
                target = DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentID(data.poiTargetID);
            }
        }
        //NOTE: Did not save isBeingRecalculated because, when loaded, it doesn't matter if the plan is being recalculated or not.
        currentNodeIndex = data.currentNodeIndex;
        isEnd = data.isEnd;
        isPersonalPlan = data.isPersonalPlan;
        doNotRecalculate = data.doNotRecalculate;
        state = data.state;
        isAssigned = data.isAssigned;

        allNodes = RuinarchListPool<JobNode>.Claim();
        for (int i = 0; i < data.allNodes.Count; i++) {
            SaveDataJobNode saveDataJobNode = data.allNodes[i];
            JobNode node = saveDataJobNode.Load();
            if (node.singleNode != null) {
                allNodes.Add(node);
            }
        }
        if (allNodes.Count > 0) {
            startingNode = GetJobNodeWithPersistentID(data.startingNodeID);
            endNode = GetJobNodeWithPersistentID(data.endNodeID);
            currentNode = GetJobNodeWithPersistentID(data.currentNodeID);
            previousNode = GetJobNodeWithPersistentID(data.previousNodeID);
        }
    }
    public void SetTarget(IPointOfInterest p_target) {
        target = p_target;
    }
    public void SetIsPersonalPlan(bool p_state) {
        isPersonalPlan = p_state;
    }
    private JobNode GetJobNodeWithPersistentID(string id) {
        //Assert.IsTrue(allNodes != null && allNodes.Count > 0);
        for (int i = 0; i < allNodes.Count; i++) {
            JobNode jobNode = allNodes[i];
            if (jobNode.persistentID == id) {
                return jobNode;
            }
        }
        return null;
    }
    public void SetActionNodes(ActualGoapNode p_action) {
        if (allNodes == null) {
            allNodes = RuinarchListPool<JobNode>.Claim();
        }
        SingleJobNode sj = ObjectPoolManager.Instance.CreateNewSingleJobNode();
        sj.SetActionNode(p_action);
        allNodes.Add(sj);
        InitializeNodes();
    }
    public void SetActionNodes(ActualGoapNode p_action1, ActualGoapNode p_action2) {
        if (allNodes == null) {
            allNodes = RuinarchListPool<JobNode>.Claim();
        }
        SingleJobNode sj1 = ObjectPoolManager.Instance.CreateNewSingleJobNode();
        SingleJobNode sj2 = ObjectPoolManager.Instance.CreateNewSingleJobNode();
        sj1.SetActionNode(p_action1);
        sj2.SetActionNode(p_action2);
        allNodes.Add(sj1);
        allNodes.Add(sj2);
        InitializeNodes();
    }
    public void SetNodes(List<JobNode> nodes) {
        allNodes = nodes;
        InitializeNodes();
    }
    private void InitializeNodes() {
        this.startingNode = allNodes[0];
        this.endNode = allNodes[allNodes.Count - 1];
        this.currentNode = startingNode;
        currentNodeIndex = 0;
    }
    public void SetNextNode() {
        if (currentNode == null) {
            return;
        }
        previousNode = currentNode;
        int nextNodeIndex = currentNodeIndex + 1;
        if (nextNodeIndex < allNodes.Count) {
            currentNode = allNodes[nextNodeIndex];
            currentNodeIndex = nextNodeIndex;
        } else {
#if DEBUG_LOG
            Debug.Log($"{GameManager.Instance.TodayLogString()} current node of Plan was set to null {GetPlanSummary()}");
#endif
            currentNode = null;
        }
    }
    private ActualGoapNode GetCurrentActualNode() {
        // if (currentNode == null) {
        //     return startingNode.singleNode;
        // }
        return currentNode.singleNode;
    }
    public bool HasNodeWithAction(INTERACTION_TYPE actionType) {
        if (allNodes != null) {
            for (int i = 0; i < allNodes.Count; i++) {
                JobNode jobNode = allNodes[i];
                if (jobNode.singleNode.goapType == actionType) {
                    return true;
                } 
            }    
        }
        return false;
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
    public void SetResetPlanOnFinishRecalculation(bool p_state) {
        resetPlanOnFinishRecalculation = p_state;
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
            ActualGoapNode node = jobNode.singleNode;
            log += $" ({node.cost}){node.action.goapName} - Actor: {node.actor.name}, Target: {node.poiTarget.name}";
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

#region Object Pool
    public void Reset() {
        for (int i = 0; i < allNodes.Count; i++) {
            JobNode jn = allNodes[i];
            if (jn is SingleJobNode sj) {
                ObjectPoolManager.Instance.ReturnSingleJobNodeToPool(sj);
            }
        }
        RuinarchListPool<JobNode>.Release(allNodes);
        allNodes = null;
        state = GOAP_PLAN_STATE.CANCELLED;
        target = null;
        startingNode = null;
        endNode = null;
        currentNode = null;
        previousNode = null;
        currentNodeIndex = 0;
        isEnd = false;
        isBeingRecalculated = false;
        doNotRecalculate = false;
        resetPlanOnFinishRecalculation = false;
    }
#endregion
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
