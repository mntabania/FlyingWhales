using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Inner_Maps.Location_Structures;

public class PlayerAction : SpellData {
    public override SPELL_CATEGORY category { get { return SPELL_CATEGORY.PLAYER_ACTION; } }

    public PlayerAction() {
	}

    #region Virtuals
    public virtual bool IsValid(IPlayerActionTarget target) {
        return true;
    }
    public virtual string GetLabelName(IPlayerActionTarget target) {
        return name;
    }
    #endregion

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if(targetPOI is TileObject tileObject) {
            IncreaseThreatForEveryCharacterThatSeesPOI(targetPOI, 5);
        }
        base.ActivateAbility(targetPOI);
    }
    #endregion  

    public void Activate(IPlayerActionTarget target) {
        if(target is IPointOfInterest targetPOI) {
            ActivateAbility(targetPOI);
        } else if (target is HexTile targetHex) {
            ActivateAbility(targetHex);
        } else if (target is LocationStructure targetStructure) {
            ActivateAbility(targetStructure);
        }
        Messenger.Broadcast(Signals.PLAYER_ACTION_ACTIVATED, this);
	}
    public bool CanPerformAbilityTo(IPlayerActionTarget target) {
        if (target is IPointOfInterest targetPOI) {
            return CanPerformAbilityTowards(targetPOI);
        } else if (target is HexTile targetHex) {
            return CanPerformAbilityTowards(targetHex);
        } else if (target is LocationStructure targetStructure) {
            return CanPerformAbilityTowards(targetStructure);
        }
        return CanPerformAbility();
    }
}