using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

public class EmpowerData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.EMPOWER;
    public override string name => "Empower";
    public override string description => "This Ability will significantly increase a character's combat prowess for 12 hours.";
    public EmpowerData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if (targetPOI is Character targetCharacter) {
            if (targetCharacter.traitContainer.AddTrait(targetCharacter, "Empowered")) {
                Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "InterventionAbility", "Empower", "activated", null, LOG_TAG.Player);
                log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddLogToDatabase();
                PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
            }
            base.ActivateAbility(targetPOI);
        }
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        if (targetCharacter.isDead || targetCharacter.traitContainer.HasTrait("Empowered")) {
            return false;
        }
        return base.CanPerformAbilityTowards(targetCharacter);
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
        if (targetCharacter.traitContainer.HasTrait("Agoraphobic")) {
            reasons += $"{targetCharacter.name} already has this Flaw,";
        }
        return reasons;
    }
}
