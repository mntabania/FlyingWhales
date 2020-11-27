using System.Collections;
using System.Collections.Generic;
using Logs;
using UnityEngine;

public class RaiseDeadData : PlayerAction {
    public override SPELL_TYPE type => SPELL_TYPE.RAISE_DEAD;
    public override string name => "Raise Dead";
    public override string description => "This Action can be used on a corpse of a Resident to spawn a Skeleton.";
    public RaiseDeadData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER, SPELL_TARGET.TILE_OBJECT };
    }
    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        //IncreaseThreatForEveryCharacterThatSeesPOI(targetPOI, 5);
        Character target = null;
        if (targetPOI is Character) {
            target = targetPOI as Character;
        } else if (targetPOI is Tombstone) {
            target = (targetPOI as Tombstone).character;
        }
        CharacterManager.Instance.RaiseFromDeath(target, PlayerManager.Instance.player.playerFaction, className: target.characterClass.className);
        //target.RaiseFromDeath(1, faction: PlayerManager.Instance.player.playerFaction, className: target.characterClass.className);

        Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "player_raise_dead", null, LOG_TAG.Player, LOG_TAG.Life_Changes);
        log.AddToFillers(target, target.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddLogToDatabase();
        PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
        if (UIManager.Instance.characterInfoUI.isShowing) {
            UIManager.Instance.characterInfoUI.CloseMenu();
        }
        base.ActivateAbility(targetPOI);
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        if (!targetCharacter.isDead || !targetCharacter.carryComponent.IsNotBeingCarried() || targetCharacter.marker == null || targetCharacter.characterClass.IsZombie()) {
            return false;
        }
        return base.CanPerformAbilityTowards(targetCharacter);
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
        if (targetCharacter.characterClass.IsZombie()) {
            reasons += $"Cannot use Raise Dead on Zombie Villagers,";
        }
        return reasons;
    }
    public override bool CanPerformAbilityTowards(TileObject tileObject) {
        if(tileObject is Tombstone tombstone) {
            return CanPerformAbilityTowards(tombstone.character);
        }
        return false;
    }
    public override bool IsValid(IPlayerActionTarget target) {
        Character character = null;
        if (target is Tombstone tombstone) {
            character = tombstone.character;
        } else if (target is Character dead) {
            character = dead;
        }
        if(character != null) {
            if (!character.isDead) {
                return false;
            } else if (!character.race.IsSapient()) {
                return false;
            }
        }
        return base.IsValid(target);
    }
    #endregion
}