using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;

[System.Serializable]
public class SaveDataRelationship {
    public int targetCharacterID;
    public string targetCharacterAlterEgo;

    public List<RELATIONSHIP_TYPE> rels;
    public bool isDisabled;
    public int flirtationCount;
    

    //public void Save(CharacterRelationshipData relationship) {
    //    targetCharacterID = relationship.targetCharacter.id;
    //    targetCharacterAlterEgo = relationship.targetCharacterAlterEgo.name;

    //    rels = new List<RELATIONSHIP_TRAIT>();
    //    for (int i = 0; i < relationship.rels.Count; i++) {
    //        rels.Add(relationship.rels[i].relType);
    //    }

    //    isDisabled = relationship.isDisabled;
    //    flirtationCount = relationship.flirtationCount;
    //}

    // public void Load(AlterEgoData ownerAlterEgo) {
        //TODO:
        //Character targetCharacter = CharacterManager.Instance.GetCharacterByID(targetCharacterID);
        //AlterEgoData targetAlterEgoData = targetCharacter.GetAlterEgoData(targetCharacterAlterEgo);
        //CharacterRelationshipData relationshipData = new CharacterRelationshipData(ownerAlterEgo.owner, targetCharacter, targetAlterEgoData);
        //relationshipData.SetIsDisabled(isDisabled);
        //relationshipData.SetFlirtationCount(flirtationCount);
        //for (int i = 0; i < rels.Count; i++) {
        //    RelationshipTrait relTrait = CharacterManager.Instance.CreateRelationshipTrait(rels[i], targetCharacter);
        //    relationshipData.AddRelationship(relTrait);
        //}

        //ownerAlterEgo.AddRelationship(relationshipData);
    // }
}
