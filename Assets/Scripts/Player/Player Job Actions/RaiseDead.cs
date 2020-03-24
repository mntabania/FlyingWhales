using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaiseDead : PlayerSpell {

    private int _level;

    public RaiseDead() : base(SPELL_TYPE.RAISE_DEAD) {
        tier = 2;
        SetDefaultCooldownTime(24);
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER, SPELL_TARGET.TILE_OBJECT };
        _level = 1;
        //abilityTags.Add(ABILITY_TAG.MAGIC);
    }

    #region Overrides
    public override void ActivateAction(IPointOfInterest targetPOI) {
        Character target;
        if (targetPOI is Character) {
            target = targetPOI as Character;
        } else if (targetPOI is Tombstone) {
            target = (targetPOI as Tombstone).character;
        } else {
            return;
        }
        base.ActivateAction(target);
        target.RaiseFromDeath(_level, faction:PlayerManager.Instance.player.playerFaction);

        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "player_raise_dead");
        log.AddToFillers(target, target.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
    }

    protected override bool CanPerformActionTowards(Character targetCharacter) {
        if (!targetCharacter.isDead) {
            return false;
        }
        if (!targetCharacter.IsInOwnParty()) {
            return false;
        }
        return base.CanPerformActionTowards(targetCharacter);
    }
    public override bool CanTarget(IPointOfInterest targetPOI, ref string hoverText) {
        if (!(targetPOI is Character) && !(targetPOI is Tombstone)) {
            return false;
        }
        Character targetCharacter;
        if (targetPOI is Character) {
            targetCharacter = targetPOI as Character;
        } else {
            targetCharacter = (targetPOI as Tombstone).character;
        }
        if (!targetCharacter.isDead) {
            return false;
        }
        if (!targetCharacter.IsInOwnParty()) {
            return false;
        }
        return base.CanTarget(targetCharacter, ref hoverText);
    }
    protected override void OnLevelUp() {
        base.OnLevelUp();
        if (level == 1) {
            _level = 5;
        } else if (level == 2) {
            _level = 10;
        } else if (level == 3) {
            _level = 15;
        }
    }
    #endregion
}

public class RaiseDeadData : SpellData {
    public override SPELL_TYPE ability => SPELL_TYPE.RAISE_DEAD;
    public override string name { get { return "Raise Dead"; } }
    public override string description { get { return "Returns a character to life."; } }
    public override SPELL_CATEGORY category { get { return SPELL_CATEGORY.MONSTER; } }

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
    #endregion
}