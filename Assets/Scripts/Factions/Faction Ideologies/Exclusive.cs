using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;

public class Exclusive : FactionIdeology {
    public EXCLUSIVE_IDEOLOGY_CATEGORIES category { get; private set; }
    public RACE raceRequirement { get; private set; }
    public GENDER genderRequirement { get; private set; }
    public string traitRequirement { get; private set; }

    public Exclusive() : base(FACTION_IDEOLOGY.EXCLUSIVE) {

    }

    #region Overrides
    public override void SetRequirements(Faction faction) {
        if(faction.leader != null) {
            int chance = UnityEngine.Random.Range(0, 3);
            category = EXCLUSIVE_IDEOLOGY_CATEGORIES.TRAIT;
            if (chance == 0) {
                category = EXCLUSIVE_IDEOLOGY_CATEGORIES.GENDER;
                genderRequirement = faction.leader.gender;
            } else if (chance == 1) {
                category = EXCLUSIVE_IDEOLOGY_CATEGORIES.RACE;
                raceRequirement = faction.leader.race;
            } else {
                category = EXCLUSIVE_IDEOLOGY_CATEGORIES.TRAIT;
                traitRequirement = GetRandomTraitRequirement();
            }
        } else {
            category = EXCLUSIVE_IDEOLOGY_CATEGORIES.TRAIT;
            traitRequirement = GetRandomTraitRequirement();
        }
    }
    public override bool DoesCharacterFitIdeology(Character character) {
        if(category == EXCLUSIVE_IDEOLOGY_CATEGORIES.GENDER) {
            return character.gender == genderRequirement;
        } else if (category == EXCLUSIVE_IDEOLOGY_CATEGORIES.RACE) {
            return character.race == raceRequirement;
        }
        return character.traitContainer.HasTrait(traitRequirement);
    }
    public override string GetRequirementsForJoiningAsString() {
        return $"{category}: {GetRequirementAsString()}";
    }
    #endregion

    public void SetIndividualRequirements(EXCLUSIVE_IDEOLOGY_CATEGORIES category, string requirement) {
        if (category == EXCLUSIVE_IDEOLOGY_CATEGORIES.GENDER) {
            genderRequirement = (GENDER) System.Enum.Parse(typeof(GENDER), requirement);
        } else if (category == EXCLUSIVE_IDEOLOGY_CATEGORIES.RACE) {
            raceRequirement = (RACE) System.Enum.Parse(typeof(RACE), requirement);
        } else if (category == EXCLUSIVE_IDEOLOGY_CATEGORIES.TRAIT) {
            traitRequirement = requirement;
        }
    }

    private string GetRequirementAsString() {
        if (category == EXCLUSIVE_IDEOLOGY_CATEGORIES.GENDER) {
            return genderRequirement.ToString();
        } else if (category == EXCLUSIVE_IDEOLOGY_CATEGORIES.RACE) {
            return raceRequirement.ToString();
        }
        return traitRequirement;
    }
    private string GetRandomTraitRequirement() {
        string[] requirements = FactionManager.Instance.exclusiveIdeologyTraitRequirements;
        return requirements[UnityEngine.Random.Range(0, requirements.Length)];
    }
}
