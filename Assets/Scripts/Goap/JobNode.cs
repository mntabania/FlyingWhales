public abstract class JobNode {
    public string persistentID { get; } 
    public abstract ActualGoapNode singleNode { get; }
    public abstract ActualGoapNode[] multiNode { get; }
    public abstract int currentNodeIndex { get; }

    public JobNode() { 
        persistentID = UtilityScripts.Utilities.GetNewUniqueID();
    }
    protected JobNode(SaveDataJobNode saveDataJobNode) {
        persistentID = saveDataJobNode.persistentID;
    }
    
    public abstract void OnAttachPlanToJob(GoapPlanJob job);
    public abstract void OnUnattachPlanToJob(GoapPlanJob job);
    public abstract void SetNextActualNode();
    public abstract bool IsCurrentActionNode(ActualGoapNode node);
}

#region Save Data
public abstract class SaveDataJobNode : SaveData<JobNode> {
    public string persistentID;
    public string singleNodeID;
    public string[] multiNodeIDs;
    public int currentNodeIndex;
    public override void Save(JobNode data) {
        base.Save(data);
        persistentID = data.persistentID;
        singleNodeID = string.Empty; //TODO: Connect single job node id
        if (data.multiNode != null) {
            multiNodeIDs = new string[data.multiNode.Length];
            for (int i = 0; i < multiNodeIDs.Length; i++) {
                ActualGoapNode actualGoapNode = data.multiNode[i];
                multiNodeIDs[i] = actualGoapNode.name; //TODO: Use persistent ID
            }    
        }
        currentNodeIndex = data.currentNodeIndex;
    }
}
#endregion