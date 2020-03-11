using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class PreCharacterRelationship {
    
    public int compatibility { get; set; }
    public List<RELATIONSHIP_TYPE> relationships { get; set; }
    public int baseOpinion { get; set; }

    public PreCharacterRelationship() {
        relationships = new List<RELATIONSHIP_TYPE>();
        compatibility = -1;
    }

    public void SetCompatibility(int value) {
        compatibility = value;
        Assert.IsTrue(compatibility >= OpinionComponent.MinCompatibility 
                      && compatibility <= OpinionComponent.MaxCompatibility, 
            $"Compatibility value exceeds the min/max compatibility. Set Value is {compatibility.ToString()}");
    }
    public void SetOpinion(int value) {
        baseOpinion = value;
    }

    public void AddRelationship(RELATIONSHIP_TYPE relationshipType) {
        relationships.Add(relationshipType);
    }
}
