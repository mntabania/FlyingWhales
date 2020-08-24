using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;

public class LazinessData : SpellData {
    public override SPELL_TYPE type => SPELL_TYPE.LAZINESS;
    public override string name => "Laziness";
    public override string description => "This Affliction will make a Villager Lazy. Lazy villagers may sometimes refuse to do work.";
    public override SPELL_CATEGORY category => SPELL_CATEGORY.AFFLICTION;
    public LazinessData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }
    
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        targetPOI.traitContainer.AddTrait(targetPOI, "Lazy");
        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "player_afflicted");
        log.AddToFillers(targetPOI, targetPOI.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(null, "Lazy", LOG_IDENTIFIER.STRING_1);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
        base.ActivateAbility(targetPOI);
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        if (targetCharacter.isDead || targetCharacter.traitContainer.HasTrait("Lazy")) {
            return false;
        }
        return base.CanPerformAbilityTowards(targetCharacter);
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
        if (targetCharacter.traitContainer.HasTrait("Lazy")) {
            reasons += $"{targetCharacter.name} already has this Flaw,";
        }
        return reasons;
    }
}