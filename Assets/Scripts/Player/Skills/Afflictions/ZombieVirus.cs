using System.Collections;
using Logs;
using UnityEngine;

public class ZombieVirusData : SpellData {
    public override SPELL_TYPE type => SPELL_TYPE.ZOMBIE_VIRUS;
    public override string name => "Zombie Virus";
    public override string description => $"This Affliction will inoculate a Villager with the Zombie Virus. This Virus will cause the Villager to eventually die. Once dead, it will reanimate during nighttime and may spread the Virus to other Villagers that it attacks.";
    public override SPELL_CATEGORY category => SPELL_CATEGORY.AFFLICTION;
    //public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.AFFLICTION;

    public ZombieVirusData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER, SPELL_TARGET.TILE_OBJECT };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "player_afflicted", null, LOG_TAG.Player, LOG_TAG.Life_Changes);
        log.AddToFillers(targetPOI, targetPOI.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(null, "Infected", LOG_IDENTIFIER.STRING_1);
        log.AddLogToDatabase();
        PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
        base.ActivateAbility(targetPOI);
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        if (targetCharacter.isDead || targetCharacter.race == RACE.SKELETON || targetCharacter.traitContainer.HasTrait("Robust")) {
            return false;
        }
        return base.CanPerformAbilityTowards(targetCharacter);
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
        if (targetCharacter.traitContainer.HasTrait("Robust")) {
            reasons += $"Robust Villagers are immune to Zombie Virus,";
        }
        return reasons;
    }
    #endregion
}