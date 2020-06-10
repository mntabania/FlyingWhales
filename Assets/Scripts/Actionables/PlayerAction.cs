using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Inner_Maps.Location_Structures;

public class PlayerAction : SpellData {
    public override SPELL_CATEGORY category { get { return SPELL_CATEGORY.PLAYER_ACTION; } }

    #region Virtuals
    public virtual bool IsValid(IPlayerActionTarget target) {
        if (target is Character character) {
            return character.marker != null;
        } else if (target is TileObject tileObject) {
            return tileObject.mapObjectVisual != null;
        }
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
        } else if (target is StructureRoom room) {
            ActivateAbility(room);
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
        } else if (target is StructureRoom room) {
            return CanPerformAbilityTowards(room);
        }
        return CanPerformAbility();
    }
}