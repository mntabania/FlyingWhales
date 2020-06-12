using System.Collections;
using System.Collections.Generic;
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
        CharacterManager.Instance.RaiseFromDeath(target, FactionManager.Instance.undeadFaction, className: target.characterClass.className);
        //target.RaiseFromDeath(1, faction: PlayerManager.Instance.player.playerFaction, className: target.characterClass.className);

        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "player_raise_dead");
        log.AddToFillers(target, target.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
        if (UIManager.Instance.characterInfoUI.isShowing) {
            UIManager.Instance.characterInfoUI.CloseMenu();
        }
        base.ActivateAbility(targetPOI);
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        if (!targetCharacter.isDead || !targetCharacter.IsInOwnParty() || targetCharacter.traitContainer.HasTrait("Infected")) {
            return false;
        }
        return base.CanPerformAbilityTowards(targetCharacter);
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter); 
        if (targetCharacter.traitContainer.HasTrait("Infected")) {
            reasons += $"Cannot use Raise Dead on Infected {UtilityScripts.Utilities.VillagerIcon()}Villagers,";
        }
        return reasons;
    }
    public override bool CanPerformAbilityTowards(TileObject tileObject) {
        if(tileObject is Tombstone tombstone) {
            return CanPerformAbilityTowards(tombstone.character);
        }
        return false;
    }
    #endregion
}