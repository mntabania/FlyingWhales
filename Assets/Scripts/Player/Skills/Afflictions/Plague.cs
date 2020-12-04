using System.Collections;
using Logs;
using UnityEngine;

public class PlagueData : SpellData {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.PLAGUE;
    public override string name => "Plague";
    public override string description => GetDescription();
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.AFFLICTION;
    //public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.AFFLICTION;

    public PlagueData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER, SPELL_TARGET.TILE_OBJECT };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "player_afflicted", null, LOG_TAG.Player, LOG_TAG.Life_Changes);
        log.AddToFillers(targetPOI, targetPOI.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(null, "Plagued", LOG_IDENTIFIER.STRING_1);
        log.AddLogToDatabase();
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

    private string GetDescription() {
        string modifiedDescription = $"This Affliction will afflict a Villager with a virulent disease. The Plague may start spreading to others before eventually killing the Villager.";
        if (GameManager.Instance.gameHasStarted) {
            modifiedDescription = $"{modifiedDescription}\n\n{PlagueDisease.Instance.GetPlagueEffectsSummary()}";
        }
        return modifiedDescription;
    }
}