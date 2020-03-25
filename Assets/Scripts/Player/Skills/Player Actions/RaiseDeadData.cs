using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaiseDeadData : PlayerAction {
    public override SPELL_TYPE type => SPELL_TYPE.RAISE_DEAD;
    public override string name { get { return "Raise Dead"; } }
    public override string description { get { return "Returns a character to life."; } }

    public RaiseDeadData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER, SPELL_TARGET.TILE_OBJECT };
    }
    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        IncreaseThreatForEveryCharacterThatSeesPOI(targetPOI, 5);
        Character target = null;
        if (targetPOI is Character) {
            target = targetPOI as Character;
        } else if (targetPOI is Tombstone) {
            target = (targetPOI as Tombstone).character;
        }
        target.RaiseFromDeath(1, faction: PlayerManager.Instance.player.playerFaction, className: target.characterClass.className);

        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "player_raise_dead");
        log.AddToFillers(target, target.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        if (!targetCharacter.isDead || !targetCharacter.IsInOwnParty()) {
            return false;
        }
        return base.CanPerformAbilityTowards(targetCharacter);
    }
    public override bool CanPerformAbilityTowards(TileObject tileObject) {
        if(tileObject is Tombstone tombstone) {
            return CanPerformAbilityTowards(tombstone.character);
        }
        return false;
    }
    #endregion
}