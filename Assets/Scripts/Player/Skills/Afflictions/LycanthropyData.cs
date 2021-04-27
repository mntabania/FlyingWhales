using System.Collections;
using Logs;
using Object_Pools;
using Traits;
using UnityEngine;
using UtilityScripts;

public class LycanthropyData : AfflictData {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.LYCANTHROPY;
    public override string name => "Lycanthropy";
    public override string description => "This Affliction will turn a Villager into a Werewolf. A Werewolf sometimes switches from normal form to wolf form and vice-versa whenever it sleeps." +
        "\nA Lycanthrope produces 2 Chaos Orbs whenever it sheds a Wolf Pelt. It also produces 2 Chaos Orbs each time it kills a Villager.";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.AFFLICTION;
    //public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.AFFLICTION;

    public LycanthropyData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER, SPELL_TARGET.TILE_OBJECT };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        //targetPOI.traitContainer.AddTrait(targetPOI, "Lycanthrope");
        OnAfflictPOIWith("Lycanthrope", targetPOI, "Lycanthrope");
        new LycanthropeData(targetPOI as Character);
        OnExecutePlayerSkill();
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        if (targetCharacter.isDead || targetCharacter.race == RACE.SKELETON || targetCharacter.traitContainer.HasTrait("Lycanthrope", "Beast")) {
            return false;
        }
        if (targetCharacter.HasItem(TILE_OBJECT_TYPE.PHYLACTERY)) {
            return false;
        }
        return base.CanPerformAbilityTowards(targetCharacter);
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
        if (targetCharacter.traitContainer.HasTrait("Lycanthrope")) {
            reasons += $"{targetCharacter.name} already has this Flaw,";
        }
        if (targetCharacter.HasItem(TILE_OBJECT_TYPE.PHYLACTERY)) {
            reasons += $"{targetCharacter.name} has a Phylactery,";
        }
        return reasons;
    }
    #endregion
}