using System.Collections;
using System.Collections.Generic;
using Logs;
using Traits;
using UnityEngine;

public class HotheadedData : SpellData {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.HOTHEADED;
    public override string name { get { return "Hotheaded"; } }
    public override string description { get { return "Hotheaded"; } }
    public override PLAYER_SKILL_CATEGORY category { get { return PLAYER_SKILL_CATEGORY.AFFLICTION; } }

    public HotheadedData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }
    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        targetPOI.traitContainer.AddTrait(targetPOI, "Hothead");
        Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "player_afflicted", null, LOG_TAG.Player, LOG_TAG.Life_Changes);
        log.AddToFillers(targetPOI, targetPOI.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(null, name, LOG_IDENTIFIER.STRING_1);
        log.AddLogToDatabase();
        PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
        base.ActivateAbility(targetPOI);
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        if (targetCharacter.isDead || targetCharacter.traitContainer.HasTrait("Hothead")) {
            return false;
        }
        return base.CanPerformAbilityTowards(targetCharacter);
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
        if (targetCharacter.traitContainer.HasTrait("Hothead")) {
            reasons += $"{targetCharacter.name} already has this Flaw,";
        }
        return reasons;
    }
    #endregion
}