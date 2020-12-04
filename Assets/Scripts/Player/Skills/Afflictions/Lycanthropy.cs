using System.Collections;
using Logs;
using Traits;
using UnityEngine;

public class LycanthropyData : SpellData {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.LYCANTHROPY;
    public override string name => "Lycanthropy";
    public override string description => "This Affliction will turn a Villager into a Werewolf. A Werewolf sometimes switches from normal form to wolf form and vice-versa whenever it sleeps.";
    public override PLAYER_SKILL_CATEGORY category { get { return PLAYER_SKILL_CATEGORY.AFFLICTION; } }
    //public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.AFFLICTION;

    public LycanthropyData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER, SPELL_TARGET.TILE_OBJECT };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        //targetPOI.traitContainer.AddTrait(targetPOI, "Lycanthrope");
        LycanthropeData lycanthropeData = new LycanthropeData(targetPOI as Character);
        Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "player_afflicted", null, LOG_TAG.Player, LOG_TAG.Life_Changes);
        log.AddToFillers(targetPOI, targetPOI.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(null, "Lycanthrope", LOG_IDENTIFIER.STRING_1);
        log.AddLogToDatabase();
        PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
        base.ActivateAbility(targetPOI);
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