using System.Collections.Generic;

public class SaveDataGoapPlan : SaveData<GoapPlan> {

    public string poiTargetID;
    public OBJECT_TYPE targetObjectType;

    public List<SaveDataJobNode> allNodes;
    public string startingNodeID;
    public string endNodeID;
    public string currentNodeID;
    public string previousNodeID;

    public int currentNodeIndex;
    public bool isEnd;
    public bool isPersonalPlan;
    public bool doNotRecalculate;
    public bool isAssigned;
    public GOAP_PLAN_STATE state;
    public override void Save(GoapPlan data) {
        base.Save(data);
        if (data.target != null) {
            poiTargetID = data.target.persistentID;
            targetObjectType = data.target.objectType;    
        } else {
            poiTargetID = string.Empty;
            targetObjectType = OBJECT_TYPE.Character;
        }
        
        
        allNodes = new List<SaveDataJobNode>();
        if (data.allNodes != null) {
            for (int i = 0; i < data.allNodes.Count; i++) {
                JobNode jobNode = data.allNodes[i];
                SaveDataJobNode saveDataJobNode = SaveUtilities.createSaveDataJobNode(jobNode);
                saveDataJobNode.Save(jobNode);
                allNodes.Add(saveDataJobNode);
            }    
        }
        
        startingNodeID = data.startingNode == null ? string.Empty : data.startingNode.persistentID;
        endNodeID = data.endNode == null ? string.Empty : data.endNode.persistentID;
        currentNodeID = data.currentNode == null ? string.Empty : data.currentNode.persistentID;
        previousNodeID = data.previousNode == null ? string.Empty : data.previousNode.persistentID;

        currentNodeIndex = data.currentNodeIndex;
        isEnd = data.isEnd;
        isPersonalPlan = data.isPersonalPlan;
        doNotRecalculate = data.doNotRecalculate;
        state = data.state;
        isAssigned = data.isAssigned;
    }
    public override GoapPlan Load() {
        return new GoapPlan(this);
    }
}