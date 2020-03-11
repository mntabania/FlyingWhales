using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Relatable : BaseRelatable{

    public IRelationshipContainer relationshipContainer { get; }
    public IRelationshipValidator relationshipValidator { get; }
    public IRelationshipProcessor relationshipProcessor { get; }
    
    public Relatable() {
        relationshipContainer = RelationshipManager.Instance.CreateRelationshipContainer(this);
        relationshipValidator = RelationshipManager.Instance.GetValidator(this);
        relationshipProcessor = RelationshipManager.Instance.GetProcessor(this);
    }

}
