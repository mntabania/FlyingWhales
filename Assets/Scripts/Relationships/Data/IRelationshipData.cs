using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRelationshipData {
    List<RELATIONSHIP_TYPE> relationships { get; }
    OpinionData opinions { get; }
    AwarenessData awareness { get; }
    string targetName { get; }
    GENDER targetGender { get; }
    void AddRelationship(RELATIONSHIP_TYPE relType);
    void RemoveRelationship(RELATIONSHIP_TYPE relType);
    RELATIONSHIP_TYPE GetFirstMajorRelationship();
    /// <summary>
    /// Check if this relationship has any of the relationship types provided.
    /// Will return true as long as 1 of the provided types were found.
    /// </summary>
    /// <param name="rels">The relationships to watch out for.</param>
    /// <returns>True or false</returns>
    bool HasRelationship(params RELATIONSHIP_TYPE[] rels);
    void SetTargetName(string name);
    void SetTargetGender(GENDER gender);
}
