using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;

public class Alcoholic : PlayerSpell {

    public Alcoholic() : base(SPELL_TYPE.ALCOHOLIC) {
        tier = 3;
        SetDefaultCooldownTime(24);
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER, SPELL_TARGET.TILE_OBJECT };
    }

    #region Overrides
    public override void ActivateAction(IPointOfInterest poi) {
        List<Character> targets = new List<Character>();
        if (poi is Character) {
            targets.Add(poi as Character);
        } else if (poi is TileObject) {
            TileObject to = poi as TileObject;
            if (to.users != null) { targets.AddRange(to.users); }
        } else {
            return;
        }
        if (targets.Count > 0) {
            for (int i = 0; i < targets.Count; i++) {
                Character currTarget = targets[i];
                if (CanPerformActionTowards(currTarget)) {
                    currTarget.traitContainer.AddTrait(currTarget, "Drunkard");
                    Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "player_afflicted");
                    log.AddToFillers(currTarget, currTarget.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    log.AddToFillers(null, "Drunkard", LOG_IDENTIFIER.STRING_1);
                    log.AddLogToInvolvedObjects();
                    PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
                }
            }
            base.ActivateAction(targets[0]);
        }        
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
    private bool CanTarget(Character targetCharacter, ref string hoverText) {
        if (targetCharacter.isDead) { //|| (!targetCharacter.isTracked && !GameManager.Instance.inspectAll)
            return false;
        }
        if (targetCharacter.traitContainer.HasTrait("Drunkard")) {
            return false;
        }
        return base.CanTarget(targetCharacter, ref hoverText);
    }
    protected override bool CanPerformActionTowards(Character targetPOI) {
        if (targetPOI.isDead) {
            return false;
        }
        if (!(targetPOI is Character) || targetPOI.traitContainer.HasTrait("Drunkard")) {
            return false;
        }
        return base.CanPerformActionTowards(targetPOI);
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
    #endregion
}

public class AlcoholicData : SpellData {
    public override SPELL_TYPE ability => SPELL_TYPE.ALCOHOLIC;
    public override string name { get { return "Alcoholic"; } }
    public override string description { get { return "Makes a character often want to drink."; } }
    public override SPELL_CATEGORY category { get { return SPELL_CATEGORY.AFFLICTION; } }
    //public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.AFFLICTION;

    public AlcoholicData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER, SPELL_TARGET.TILE_OBJECT };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        targetPOI.traitContainer.AddTrait(targetPOI, "Drunkard");
        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "player_afflicted");
        log.AddToFillers(targetPOI, targetPOI.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(null, "Drunkard", LOG_IDENTIFIER.STRING_1);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        if (targetCharacter.isDead || targetCharacter.traitContainer.HasTrait("Drunkard")) {
            return false;
        }
        return base.CanPerformAbilityTowards(targetCharacter);
    }
    #endregion
}