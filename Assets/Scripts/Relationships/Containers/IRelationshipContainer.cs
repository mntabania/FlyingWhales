﻿using System.Collections;
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
    bool HasRelationshipWith(Relatable relatable, params RELATIONSHIP_TYPE[] relType);
    bool HasSpecialRelationshipWith(Relatable relatable);
    bool HasRelationship(params RELATIONSHIP_TYPE[] type);
    bool HasActiveRelationship(params RELATIONSHIP_TYPE[] type);
    #endregion

    #region Getting
    List<int> GetAllRelatableIDWithRelationship(params RELATIONSHIP_TYPE[] type);
    int GetFirstRelatableIDWithRelationship(params RELATIONSHIP_TYPE[] type);
    int GetRelatablesWithRelationshipCount(params RELATIONSHIP_TYPE[] type);
    IRelationshipData GetRelationshipDataWith(Relatable relatable);
    //Returns the relationship where the choices are the relationships that are passed to the function
    //Example: If we want to know if the character is lover or affair of another character we will use this function because this will return if their relationship is lover or affair
    RELATIONSHIP_TYPE GetRelationshipFromParametersWith(Relatable relatable, params RELATIONSHIP_TYPE[] relType);
    bool IsFamilyMember(Character target);
    Character GetMissingCharacterWithOpinion(string opinionLabel);
    Character GetMissingCharacterThatMeetCriteria(Func<Character, bool> checker);
    Character GetFirstCharacterWithRelationship(params RELATIONSHIP_TYPE[] type);
    Character GetFirstCharacterWithRelationship(RELATIONSHIP_TYPE type);
    bool IsRelativeLoverOrAffairAndNotRival(Character character);
    #endregion

    #region Opinions
    void AdjustOpinion(Character owner, Character target, string opinionText, int opinionValue, string lastStrawReason = "");
    void SetOpinion(Character owner, Character target, string opinionText, int opinionValue, string lastStrawReason = "");
    void SetOpinion(Character owner, int targetID, string targetName, GENDER gender, string opinionText, int opinionValue, bool isInitial, string lastStrawReason = "");
    void RemoveOpinion(Character target, string opinionText);
    bool HasOpinion(Character target, string opinionText);
    bool HasOpinion(int id, string opinionText);
    int GetTotalOpinion(Character target);
    OpinionData GetOpinionData(Character target);
    string GetOpinionLabel(Character target);
    bool IsFriendsWith(Character character);
    bool IsEnemiesWith(Character character);
    Character GetFirstEnemyCharacter();
    Character GetRandomEnemyCharacter();
    List<Character> GetCharactersWithPositiveOpinion();
    List<Character> GetCharactersWithNeutralOpinion();
    List<Character> GetCharactersWithNegativeOpinion();
    List<Character> GetEnemyCharacters();
    List<Character> GetFriendCharacters();
    List<Character> GetCharactersWithOpinionLabel(params string[] labels);
    bool HasCharacterWithOpinionLabel(params string[] labels);
    bool HasOpinionLabelWithCharacter(Character character, params string[] labels);
    bool HasOpinionLabelWithCharacter(Character character, params OPINIONS[] labels);
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
