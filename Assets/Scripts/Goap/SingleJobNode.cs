using UnityEngine.Assertions;
using UnityEngine;

public class SingleJobNode : JobNode {
    public override ActualGoapNode singleNode { get { return node; } }
    public ActualGoapNode node { get; private set; }
    
    public SingleJobNode() : base() { }
    public SingleJobNode(SaveDataSingleJobNode saveDataJobNode) : base (saveDataJobNode) {
        this.node = DatabaseManager.Instance.actionDatabase.GetActionByPersistentID(saveDataJobNode.nodeID);
    }

    public void SetActionNode(ActualGoapNode p_action) {
        node = p_action;
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
    public override void Reset() {
        if (node != null) {
            if (!node.ProcessReturnToPool()) {
                node.SetIsSupposedToBeInPool(true);
            }
        }
        node = null;
    }
    #endregion
}

#region Save Data
public class SaveDataSingleJobNode : SaveDataJobNode {
    public string nodeID;
    public override void Save(JobNode data) {
        base.Save(data);
        SingleJobNode singleJobNode = data as SingleJobNode;
        Assert.IsNotNull(singleJobNode);
        nodeID = singleJobNode.node.persistentID;
        SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(singleJobNode.node);
    }
    public override JobNode Load() {
        return new SingleJobNode(this);
    }
}
#endregion