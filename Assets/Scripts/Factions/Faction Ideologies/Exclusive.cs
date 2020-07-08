using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;

public class Exclusive : FactionIdeology {
    public EXCLUSIVE_IDEOLOGY_CATEGORIES category { get; private set; }
    public RACE raceRequirement { get; private set; }
    public GENDER genderRequirement { get; private set; }

    public Exclusive() : base(FACTION_IDEOLOGY.Exclusive) { }

    #region Overrides
    public override bool DoesCharacterFitIdeology(Character character) {
        if(category == EXCLUSIVE_IDEOLOGY_CATEGORIES.GENDER) {
            return character.gender == genderRequirement;
        } else if (category == EXCLUSIVE_IDEOLOGY_CATEGORIES.RACE) {
            return character.race == raceRequirement;
        }
        return true;
    }
    public override string GetRequirementsForJoiningAsString() {
        return $"{category.ToString()}: {GetRequirementAsString()}";
    }
    #endregion

    #region Requirements
    public void SetRequirement(RACE race) {
        category = EXCLUSIVE_IDEOLOGY_CATEGORIES.RACE;
        raceRequirement = race;
    }
    public void SetRequirement(GENDER gender) {
        category = EXCLUSIVE_IDEOLOGY_CATEGORIES.GENDER;
        genderRequirement = gender;
    }
    #endregion

    private string GetRequirementAsString() {
        if (category == EXCLUSIVE_IDEOLOGY_CATEGORIES.GENDER) {
            return genderRequirement.ToString();
        } else if (category == EXCLUSIVE_IDEOLOGY_CATEGORIES.RACE) {
            return raceRequirement.ToString();
        }
        return string.Empty;
    }
}
