using System.Collections;
using System.Collections.Generic;
using Logs;
using Traits;
using UnityEngine;

public class PoisonData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.POISON;
    public override string name => "Poison";
    public override string description => "This Ability applies Poison on an object.";
    public PoisonData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE_OBJECT };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        //IncreaseThreatForEveryCharacterThatSeesPOI(targetPOI, 5);
        //int duration = TraitManager.Instance.allTraits["Poisoned"].ticksDuration + PlayerSkillManager.Instance.GetDurationBonusPerLevel(PLAYER_SKILL_TYPE.POISON);
        int duration = PlayerSkillManager.Instance.GetDurationBonusPerLevel(PLAYER_SKILL_TYPE.POISON); 
        targetPOI.traitContainer.AddTrait(targetPOI, "Poisoned", overrideDuration: duration);
        Poisoned poisoned = targetPOI.traitContainer.GetTraitOrStatus<Poisoned>("Poisoned");
        poisoned?.SetIsPlayerSource(true);
        Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "InterventionAbility", name, "activated", null, LOG_TAG.Player);
        log.AddToFillers(targetPOI, targetPOI.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddLogToDatabase();
        PlayerManager.Instance.player.ShowNotificationFromPlayer(log, true);
        base.ActivateAbility(targetPOI);
    }
    public override bool CanPerformAbilityTowards(TileObject tileObject) {
        if ((tileObject.gridTileLocation == null && tileObject.isBeingCarriedBy == null) || 
            tileObject.traitContainer.HasTrait("Poisoned", "Robust")) {
            return false;
        }
        return base.CanPerformAbilityTowards(tileObject);
    }
    public override bool IsValid(IPlayerActionTarget target) {
        bool isValid = base.IsValid(target);
        if (isValid || (target is TileObject targetTileObject && targetTileObject.isBeingCarriedBy != null)) {
            return true; //allow carried objects to be poisoned
        }
        return false;
    }
    #endregion
}