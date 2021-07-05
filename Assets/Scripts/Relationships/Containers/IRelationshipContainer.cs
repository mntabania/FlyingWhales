using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface IRelationshipContainer {

    Dictionary<int, IRelationshipData> relationships { get; }
    List<Character> charactersWithOpinion { get; }

    #region Adding
    void AddRelationship(Relatable owner, Relatable relatable, RELATIONSHIP_TYPE rel);
    IRelationshipData CreateNewRelationship(Relatable owner, Relatable target);
    IRelationshipData CreateNewRelationship(Relatable owner, int id, string name, GENDER gender);
    #endregion

    #region Removing
    void RemoveRelationship(Relatable relatable, RELATIONSHIP_TYPE rel);
    #endregion

    #region Inquiry
    bool HasRelationshipWith(int id);
    bool HasRelationshipWith(Relatable relatable);
    bool HasRelationshipWith(Relatable relatable, RELATIONSHIP_TYPE relType);
    bool HasRelationshipWith(Relatable relatable, RELATIONSHIP_TYPE relType1, RELATIONSHIP_TYPE relType2);
    bool HasRelationshipWith(Relatable relatable, RELATIONSHIP_TYPE relType1, RELATIONSHIP_TYPE relType2, RELATIONSHIP_TYPE relType3);
    bool HasRelationshipWith(Relatable relatable, RELATIONSHIP_TYPE relType1, RELATIONSHIP_TYPE relType2, RELATIONSHIP_TYPE relType3, RELATIONSHIP_TYPE relType4, RELATIONSHIP_TYPE relType5);
    bool HasSpecialRelationshipWith(Relatable relatable);
    bool HasRelationship(RELATIONSHIP_TYPE type);
    bool HasActiveRelationship(RELATIONSHIP_TYPE type1, RELATIONSHIP_TYPE type2);
    #endregion

    #region Getting
    void PopulateAllRelatableIDWithRelationship(List<int> ids, RELATIONSHIP_TYPE type);
    int GetFirstRelatableIDWithRelationship(RELATIONSHIP_TYPE type);
    int GetRelatablesWithRelationshipCount(RELATIONSHIP_TYPE type);
    int GetRelatablesWithRelationshipCount(RELATIONSHIP_TYPE type1, RELATIONSHIP_TYPE type2);
    IRelationshipData GetRelationshipDataWith(Relatable relatable);
    //Returns the relationship where the choices are the relationships that are passed to the function
    //Example: If we want to know if the character is lover or affair of another character we will use this function because this will return if their relationship is lover or affair
    RELATIONSHIP_TYPE GetRelationshipFromParametersWith(Relatable relatable, RELATIONSHIP_TYPE relType1, RELATIONSHIP_TYPE relType2);
    bool IsFamilyMember(Character target);
    Character GetRandomMissingCharacterWithOpinion(string opinionLabel);
    Character GetRandomMissingCharacterThatIsFamilyMemberOrLoverAffairOf(Character p_character);
    Character GetFirstCharacterWithRelationship(RELATIONSHIP_TYPE type1, RELATIONSHIP_TYPE type2);
    Character GetFirstCharacterWithRelationship(RELATIONSHIP_TYPE type);
    bool IsRelativeLoverOrAffairAndNotRival(Character character);
    #endregion

    #region Opinions
    void AdjustOpinion(Character owner, Character target, string opinionText, int opinionValue, string lastStrawReason = "");
    void SetOpinion(Character owner, Character target, string opinionText, int opinionValue, string lastStrawReason = "");
    void SetOpinion(Character owner, int targetID, string targetName, GENDER gender, string opinionText, int opinionValue, bool isInitial, string lastStrawReason = "");
    void RemoveOpinion(Character target, string opinionText);
    void PopulateEnemyCharacters(List<Character> characters);
    void PopulateFriendCharacters(List<Character> characters);
    bool HasOpinion(Character target, string opinionText);
    bool HasOpinion(int id, string opinionText);
    int GetTotalOpinion(Character target);
    OpinionData GetOpinionData(Character target);
    string GetOpinionLabel(Character target);
    bool IsFriendsWith(Character character);
    bool IsEnemiesWith(Character character);
    Character GetFirstEnemyCharacter();
    Character GetRandomEnemyCharacter();
    bool HasOpinionLabelWithCharacter(Character character, string opinion);
    bool HasOpinionLabelWithCharacter(Character character, string opinion1, string opinion2);
    bool HasOpinionLabelWithCharacter(Character character, string opinion1, string opinion2, string opinion3);
    bool HasOpinionLabelWithCharacter(Character character, List<OPINIONS> opinions);

    bool HasEnemyCharacter();
    int GetNumberOfFriendCharacters();
    RELATIONSHIP_EFFECT GetRelationshipEffectWith(Character character);
    int GetCompatibility(Character target);
    string GetRelationshipNameWith(Character target);
    string GetRelationshipNameWith(int target);
    #endregion

    #region Awareness
    void SetAwarenessState(Character source, Character target, AWARENESS_STATE state);
    AWARENESS_STATE GetAwarenessState(Character character);
    #endregion

    OpinionData GetOpinionData(int id);
    IRelationshipData GetRelationshipDataWith(int id);
    int GetTotalOpinion(int id);
    IRelationshipData GetOrCreateRelationshipDataWith(Relatable owner, int id, string name, GENDER gender);
    IRelationshipData GetOrCreateRelationshipDataWith(Relatable owner, Relatable relatable);
    int GetCompatibility(int targetID);
    bool HasSpecialPositiveRelationshipWith(Character characterThatDied);
    bool BreakUp(Character owner, Character targetCharacter, string reason);
}
