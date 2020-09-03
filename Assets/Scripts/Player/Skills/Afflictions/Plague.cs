using System.Collections;
using UnityEngine;

public class PlagueData : SpellData {
    public override SPELL_TYPE type => SPELL_TYPE.PLAGUE;
    public override string name => "Plague";
    public override string description => "This Affliction will afflict a Villager with a virulent disease. The Plague may start spreading to others before eventually killing the Villager.";
    public override SPELL_CATEGORY category => SPELL_CATEGORY.AFFLICTION;
    //public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.AFFLICTION;

    public PlagueData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER, SPELL_TARGET.TILE_OBJECT };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "player_afflicted");
        log.AddToFillers(targetPOI, targetPOI.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(null, "Plagued", LOG_IDENTIFIER.STRING_1);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
        if (targetPOI is Character) {
            (targetPOI as Character).interruptComponent.TriggerInterrupt(INTERRUPT.Plagued, targetPOI);
        }
        // targetPOI.traitContainer.AddTrait(targetPOI, "Plagued");
        base.ActivateAbility(targetPOI);
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        if (targetCharacter.isDead || targetCharacter.race == RACE.SKELETON || targetCharacter.traitContainer.HasTrait("Plagued", "Robust", "Beast")) {
            return false;
        }
        return base.CanPerformAbilityTowards(targetCharacter);
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
        if (targetCharacter.traitContainer.HasTrait("Robust")) {
            reasons += $"Robust Villagers are immune to Plague,";
        }
        if (targetCharacter.traitContainer.HasTrait("Plagued")) {
            reasons += $"{targetCharacter.name} already has this Flaw,";
        }
        return reasons;
    }
    #endregion
}