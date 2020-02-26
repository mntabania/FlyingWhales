using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;

public class Jolt : PlayerSpell {

    private int _durationInMinutes;
    public Jolt() : base(SPELL_TYPE.JOLT) {
        //description = "Temporarily speeds up the movement of a character.";
        SetDefaultCooldownTime(24);
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER, SPELL_TARGET.TILE_OBJECT };
        //abilityTags.Add(ABILITY_TAG.MAGIC);
    }

    #region Overrides
    public override void ActivateAction(IPointOfInterest targetPOI) {
        List<Character> targets = new List<Character>();
        if (targetPOI is Character) {
            targets.Add(targetPOI as Character);
        } else if (targetPOI is TileObject) {
            TileObject to = targetPOI as TileObject;
            if (to.users != null) { targets.AddRange(to.users); }
        } else {
            return;
        }
        if (targets.Count > 0) {
            for (int i = 0; i < targets.Count; i++) {
                Character currTarget = targets[i];
                if (CanPerformActionTowards(currTarget)) {
                    Trait newTrait = new Jolted();
                    newTrait.OverrideDuration(GameManager.Instance.GetTicksBasedOnMinutes(_durationInMinutes));
                    currTarget.traitContainer.AddTrait(currTarget, newTrait);
                    if (UIManager.Instance.characterInfoUI.isShowing) {
                        UIManager.Instance.characterInfoUI.UpdateThoughtBubble();
                    }
                    Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "player_intervention");
                    log.AddToFillers(currTarget, currTarget.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    log.AddToFillers(null, "jolted", LOG_IDENTIFIER.STRING_1);
                    log.AddLogToInvolvedObjects();
                    PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
                }
            }
            base.ActivateAction(targets[0]);
        }
    }
    protected virtual bool CanPerformActionTowards(IPointOfInterest targetPOI) {
        if (targetPOI is TileObject) {
            TileObject to = targetPOI as TileObject;
            if (to.users != null) {
                for (int i = 0; i < to.users.Length; i++) {
                    Character currUser = to.users[i];
                    bool canTarget = CanPerformActionTowards(currUser);
                    if (canTarget) { return true; }
                }
            }
        }
        return false;
    }
    protected override bool CanPerformActionTowards(Character targetCharacter) {
        if (targetCharacter.isDead) {
            return false;
        }
        if (targetCharacter.traitContainer.HasTrait("Jolted")) {
            return false;
        }
        //if (targetCharacter.traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE)) {
        //    return false;
        //}
        return base.CanPerformActionTowards(targetCharacter);
    }
    public override bool CanTarget(IPointOfInterest targetPOI, ref string hoverText) {
        if (targetPOI is Character) {
            return CanTarget(targetPOI as Character, ref hoverText);
        } else if (targetPOI is TileObject) {
            TileObject to = targetPOI as TileObject;
            if (to.users != null) {
                for (int i = 0; i < to.users.Length; i++) {
                    Character currUser = to.users[i];
                    if (currUser != null) {
                        bool canTarget = CanTarget(currUser, ref hoverText);
                        if (canTarget) { return true; }
                    }
                }
            }
        }
        return false;
    }
    protected override void OnLevelUp() {
        base.OnLevelUp();
        if (level == 1) {
            _durationInMinutes = 30;
        } else if (level == 2) {
            _durationInMinutes = 60;
        } else if (level == 3) {
            _durationInMinutes = 90;
        }
    }
    #endregion

    private bool CanTarget(Character targetCharacter, ref string hoverText) {
        if (targetCharacter.isDead) {
            return false;
        }
        if (targetCharacter.traitContainer.HasTrait("Jolted")) {
            return false;
        }
        //if (targetCharacter.traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE)) {
        //    return false;
        //}
        return base.CanTarget(targetCharacter, ref hoverText);
    }
}
