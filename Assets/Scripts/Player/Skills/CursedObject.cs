using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;

public class CursedObject : PlayerSpell {
    public CursedObject() : base(SPELL_TYPE.CURSED_OBJECT) {
        tier = 2;
        SetDefaultCooldownTime(24);
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE_OBJECT };
        //abilityTags.Add(ABILITY_TAG.NONE);
    }

    #region Overrides
    public override void ActivateAction(IPointOfInterest targetPOI) {
        if (targetPOI is TileObject) {
            TileObject to = targetPOI as TileObject;
            Trait newTrait = new Cursed();
            newTrait.SetLevel(level);
            targetPOI.traitContainer.AddTrait(targetPOI, newTrait);
            Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "cursed_object");
            log.AddToFillers(to, to.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddLogToInvolvedObjects();
            PlayerManager.Instance.player.ShowNotificationFromPlayer(log);

            base.ActivateAction(targetPOI);
        }
    }
    protected virtual bool CanPerformActionTowards(IPointOfInterest targetPOI) {
        if (targetPOI is TileObject) {
            TileObject to = targetPOI as TileObject;
            if(!to.traitContainer.HasTrait("Cursed")){
                return true;
            }
        }
        return false;
    }
    public override bool CanTarget(IPointOfInterest targetPOI, ref string hoverText) {
        if (targetPOI is TileObject) {
            TileObject to = targetPOI as TileObject;
            if (!to.traitContainer.HasTrait("Cursed")) {
                return true;
            }
        }
        return false;
    }
    #endregion
}

public class CursedObjectData : SpellData {
    public override SPELL_TYPE type => SPELL_TYPE.CURSED_OBJECT;
    public override string name { get { return "Cursed Object"; } }
    public override string description { get { return "Put a curse on an object"; } }
    public override SPELL_CATEGORY category { get { return SPELL_CATEGORY.PLAYER_ACTION; } }

    public CursedObjectData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE_OBJECT };
    }
}