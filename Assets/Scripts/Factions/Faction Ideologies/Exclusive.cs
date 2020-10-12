using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;
using Factions.Faction_Types;

[System.Serializable]
public class Exclusive : FactionIdeology {
    public EXCLUSIVE_IDEOLOGY_CATEGORIES category { get; private set; }
    public RACE raceRequirement { get; private set; }
    public GENDER genderRequirement { get; private set; }
    public string traitRequirement { get; private set; }

    public Exclusive() : base(FACTION_IDEOLOGY.Exclusive) { }

    #region Overrides
    public override bool DoesCharacterFitIdeology(Character character) {
        if(category == EXCLUSIVE_IDEOLOGY_CATEGORIES.GENDER) {
            return character.gender == genderRequirement;
        } else if (category == EXCLUSIVE_IDEOLOGY_CATEGORIES.RACE) {
            return character.race == raceRequirement;
        } else if (category == EXCLUSIVE_IDEOLOGY_CATEGORIES.TRAIT) {
            return character.traitContainer.HasTrait(traitRequirement);
        }
        return true;
    }
    public override bool DoesCharacterFitIdeology(PreCharacterData character) {
        if(category == EXCLUSIVE_IDEOLOGY_CATEGORIES.GENDER) {
            return character.gender == genderRequirement;
        } else if (category == EXCLUSIVE_IDEOLOGY_CATEGORIES.RACE) {
            return character.race == raceRequirement;
        } else if (category == EXCLUSIVE_IDEOLOGY_CATEGORIES.TRAIT) {
            return false; //Default to false since PreCharacterData does not have traits
        }
        return true;
    }
    public override string GetRequirementsForJoiningAsString() {
        return $"{category.ToString()}: {GetRequirementAsString()}";
    }
    public override string GetIdeologyDescription() {
        return $"Only allows {GetRequirementAsString()} to join them.";
    }
    public override string GetIdeologyName() {
        return $"{name}: {GetRequirementAsString()}";
    }
    public override void OnAddIdeology(FactionType factionType) {
        factionType.RemoveIdeology(FACTION_IDEOLOGY.Inclusive);
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
    public void SetRequirement(string trait) {
        category = EXCLUSIVE_IDEOLOGY_CATEGORIES.TRAIT;
        traitRequirement = trait;
    }
    #endregion

    private string GetRequirementAsString() {
        if (category == EXCLUSIVE_IDEOLOGY_CATEGORIES.GENDER) {
            return UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(genderRequirement.ToString());
        } else if (category == EXCLUSIVE_IDEOLOGY_CATEGORIES.RACE) {
            return UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(raceRequirement.ToString());
        } else if (category == EXCLUSIVE_IDEOLOGY_CATEGORIES.RACE) {
            return traitRequirement;
        }
        return string.Empty;
    }
}
