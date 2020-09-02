using UnityEngine.Assertions;

public class MultiJobNode : JobNode {
    public override ActualGoapNode singleNode { get { return null; } }
    public override ActualGoapNode[] multiNode { get { return nodes; } }
    public override int currentNodeIndex { get { return currentIndex; } }

    public ActualGoapNode[] nodes { get; private set; }
    public int currentIndex { get; private set; }
    public MultiJobNode(ActualGoapNode[] nodes) : base() {
        this.nodes = nodes;
        currentIndex = 0;
    }
    public MultiJobNode(SaveDataJobNode saveDataJobNode) : base(saveDataJobNode) {
        //TODO:
    }

    #region Overrides
    public override void OnAttachPlanToJob(GoapPlanJob job) {
        for (int i = 0; i < nodes.Length; i++) {
            nodes[i].OnAttachPlanToJob(job);
        }
    }
    public override void OnUnattachPlanToJob(GoapPlanJob job) {
        for (int i = 0; i < nodes.Length; i++) {
            nodes[i].OnUnattachPlanToJob(job);
        }
    }
    public override void SetNextActualNode() {
        currentIndex += 1;
    }
    public override bool IsCurrentActionNode(ActualGoapNode node) {
        for (int i = 0; i < nodes.Length; i++) {
            ActualGoapNode currNode = nodes[i];
            if(currNode == node) {
                return true;
            }
        }
        return false;
    }
    #endregion
}

#region Save Data
public class SaveDataMultiJobNode : SaveDataJobNode {
    public string[] nodeIDs;
    public int currentIndex;
    public override void Save(JobNode data) {
        base.Save(data);
        MultiJobNode multiJobNode = data as MultiJobNode;
        Assert.IsNotNull(multiJobNode);
        currentIndex = multiJobNode.currentIndex;
    }
}
#endregion