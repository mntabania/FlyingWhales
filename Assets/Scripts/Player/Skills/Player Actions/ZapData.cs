using System.Collections;
using System.Collections.Generic;
using Logs;
using Traits;
using UnityEngine;

public class ZapData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.ZAP;
    public override string name => "Zap";
    public override string description => "This Ability can be used to apply Zapped on a character - temporarily preventing movement." +
        "\nZapping a hostile Villager will produce a Chaos Orb.";
    public ZapData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER, SPELL_TARGET.TILE_OBJECT };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        int duration = PlayerSkillManager.Instance.GetDurationBonusPerLevel(PLAYER_SKILL_TYPE.ZAP);
        targetPOI.traitContainer.AddTrait(targetPOI, "Zapped", bypassElementalChance: true, overrideDuration: duration);
        Zapped zapped = targetPOI.traitContainer.GetTraitOrStatus<Zapped>("Zapped");
        if (zapped != null) {
            zapped.SetIsPlayerSource(true);
        }
        if (UIManager.Instance.characterInfoUI.isShowing) {
            UIManager.Instance.characterInfoUI.UpdateThoughtBubble();
        }
        Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "player_intervention", null, LOG_TAG.Player);
        log.AddToFillers(targetPOI, targetPOI.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(null, "zapped", LOG_IDENTIFIER.STRING_1);
        log.AddLogToDatabase();
        PlayerManager.Instance.player.ShowNotificationFromPlayer(log, true);
        Messenger.Broadcast(PlayerSkillSignals.ZAP_ACTIVATED, targetPOI);
        base.ActivateAbility(targetPOI);
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        if (targetCharacter.isDead || !targetCharacter.carryComponent.IsNotBeingCarried() || targetCharacter.traitContainer.HasTrait("Zapped", "Electric")) {
            return false;
        }
        return base.CanPerformAbilityTowards(targetCharacter);
    }
    #endregion
}