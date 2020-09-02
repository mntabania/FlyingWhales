public class SingleJobNode : JobNode {
    public override ActualGoapNode singleNode { get { return node; } }
    public override ActualGoapNode[] multiNode { get { return null;} }
    public override int currentNodeIndex { get { return -1; } }
    public ActualGoapNode node { get; }
    
    public SingleJobNode(ActualGoapNode node) : base (){
        this.node = node;
        
    }
    public SingleJobNode(SaveDataJobNode saveDataJobNode) : base (saveDataJobNode){
        //TODO:
        
    }

    #region Overrides
    public override void OnAttachPlanToJob(GoapPlanJob job) {
        node.OnAttachPlanToJob(job);
    }
    public override void OnUnattachPlanToJob(GoapPlanJob job) {
        node.OnUnattachPlanToJob(job);
    }
    public override void SetNextActualNode() {
        //Not Applicable
    }
    public override bool IsCurrentActionNode(ActualGoapNode node) {
        return this.node == node;
    }
    #endregion
}

#region Save Data
public class SaveDataSingleJobNode : SaveDataJobNode {
    public string nodeID;
    public override void Save(JobNode data) {
        base.Save(data);
        nodeID = string.Empty; //TODO: Connect node id
    }
}
#endregion