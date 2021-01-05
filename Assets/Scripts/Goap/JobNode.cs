public abstract class JobNode {
    public string persistentID { get; } 
    public abstract ActualGoapNode singleNode { get; }
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
    public override void Save(JobNode data) {
        base.Save(data);
        persistentID = data.persistentID;
    }
}
#endregion