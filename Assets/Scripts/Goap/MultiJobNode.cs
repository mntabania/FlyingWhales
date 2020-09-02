using UnityEngine.Assertions;

public class MultiJobNode : JobNode {
    public override ActualGoapNode singleNode { get { return null; } }
    public override ActualGoapNode[] multiNode { get { return nodes; } }
    public override int currentNodeIndex { get { return currentIndex; } }

    private ActualGoapNode[] nodes { get; }
    public int currentIndex { get; private set; }
    public MultiJobNode(ActualGoapNode[] nodes) : base() {
        this.nodes = nodes;
        currentIndex = 0;
    }
    public MultiJobNode(SaveDataMultiJobNode saveDataJobNode) : base(saveDataJobNode) {
        nodes = new ActualGoapNode[saveDataJobNode.nodeIDs.Length];
        for (int i = 0; i < nodes.Length; i++) {
            ActualGoapNode node = DatabaseManager.Instance.actionDatabase.GetActionByPersistentID(saveDataJobNode.nodeIDs[i]);
            nodes[i] = node;
        }
        currentIndex = saveDataJobNode.currentIndex;
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
        nodeIDs = new string[multiJobNode.multiNode.Length];
        for (int i = 0; i < multiJobNode.multiNode.Length; i++) {
            ActualGoapNode actualGoapNode = multiJobNode.multiNode[i]; 
            string id = actualGoapNode.persistentID;
            nodeIDs[i] = id;
            SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(actualGoapNode);
        }
    }
    public override JobNode Load() {
        return new MultiJobNode(this);
    }
}
#endregion