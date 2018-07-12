﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnrollAction : CharacterAction {

    private ECS.Character mentor;

    public EnrollAction() : base(ACTION_TYPE.ENROLL) {
        
    }

    #region Overrides
    public override void PerformAction(CharacterParty party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        if (mentor == null) {
            if (targetObject is ICharacterObject) {
                ICharacterObject owner = targetObject as ICharacterObject;
                if (owner.iparty.icharacters[0] is ECS.Character) {
                    mentor = (owner.iparty.icharacters[0] as ECS.Character);
                }
            }
        }

        int enrollChance = 100;
        int studentCount = mentor.GetCharactersWithRelationshipStatus(CHARACTER_RELATIONSHIP.STUDENT).Count;
        for (int i = 0; i < studentCount; i++) {
            enrollChance -= 20;
            if (enrollChance <= 0) {
                break;
            }
        }

        if (Random.Range(0, 100) < enrollChance) {
            ECS.Character student = party.mainCharacter as ECS.Character;
            //success
            Relationship mentorRel = mentor.GetRelationshipWith(student);
            Relationship studentRel = student.GetRelationshipWith(mentor);
            if (mentorRel == null) {
                mentorRel = CharacterManager.Instance.CreateNewRelationshipTowards(mentor, student);
            }
            if (studentRel == null) {
                studentRel = CharacterManager.Instance.CreateNewRelationshipTowards(student, mentor);
            }
            mentorRel.AddRelationshipStatus(CHARACTER_RELATIONSHIP.STUDENT);
            studentRel.AddRelationshipStatus(CHARACTER_RELATIONSHIP.MENTOR);
        }

        //ActionSuccess();
        GiveAllReward(party);
    }
    public override CharacterAction Clone() {
        EnrollAction action = new EnrollAction();
        SetCommonData(action);
        action.Initialize();
        return action;
    }
    public override bool CanBeDoneBy(CharacterParty party, IObject targetObject) {
        if (targetObject is ICharacterObject) {
            ICharacterObject owner = targetObject as ICharacterObject;
            if (owner.iparty.icharacters[0] is ECS.Character) {
                mentor = (owner.iparty.icharacters[0] as ECS.Character);
            }
        }
        ICharacter currCharacter = party.mainCharacter;
        if (currCharacter is ECS.Character) {
            ECS.Character character = currCharacter as ECS.Character;
            Relationship rel = mentor.GetRelationshipWith(character);
            if (rel == null || !rel.HasStatus(CHARACTER_RELATIONSHIP.STUDENT)) {
                return true;
            }
        }
        return base.CanBeDoneBy(party, targetObject);
    }
    public override bool CanBeDone(IObject targetObject) {
        return false; //Change this to something more elegant, this is to prevent other characters that don't have the release character quest from releasing this character.
    }
    #endregion
}
