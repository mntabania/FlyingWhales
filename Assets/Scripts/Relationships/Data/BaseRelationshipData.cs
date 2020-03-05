using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseRelationshipData : IRelationshipData {
    
    public string targetName { get; private set; }
    public GENDER targetGender { get; private set; }
    public List<RELATIONSHIP_TYPE> relationships { get; }
    public OpinionData opinions { get; }
    
    public BaseRelationshipData() {
        relationships = new List<RELATIONSHIP_TYPE>();
        opinions = ObjectPoolManager.Instance.CreateNewOpinionData();
    }

    #region Utilities
    public void SetTargetName(string name) {
        targetName = name;
    }
    public void SetTargetGender(GENDER gender) {
        targetGender = gender;
    }
    #endregion
    
    #region Adding
    public void AddRelationship(RELATIONSHIP_TYPE relType) {
        relationships.Add(relType);
    }
    #endregion

    #region Removing
    public void RemoveRelationship(RELATIONSHIP_TYPE relType) {
        relationships.Remove(relType);
    }
    #endregion

    #region Inquiry
    public bool HasRelationship(params RELATIONSHIP_TYPE[] rels) {
        for (int i = 0; i < rels.Length; i++) {
            if (relationships.Contains(rels[i])) {
                return true; //as long as the relationship has at least 1 relationship type from the list, consider this as true.
            }
        }
        return false;
    }
    public RELATIONSHIP_TYPE GetFirstMajorRelationship() {
        if(relationships.Count > 0) {
            return relationships[0];
        }
        //for (int i = 0; i < relationships.Count; i++) {
        //    RELATIONSHIP_TYPE rel = relationships[i];
        //    return rel;
        //}
        return RELATIONSHIP_TYPE.NONE;
    }
    #endregion

}
