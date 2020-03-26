using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Inner_Maps.Location_Structures;

public class PlayerAction : SpellData {
    public override SPELL_CATEGORY category { get { return SPELL_CATEGORY.PLAYER_ACTION; } }

    public PlayerAction() {
	}
    public virtual bool IsValid(IPlayerActionTarget target) {
        return true;
    }
    public virtual string GetLabelName(IPlayerActionTarget target) {
        return name;
    }
	public void Activate(IPlayerActionTarget target) {
        if(target is IPointOfInterest targetPOI) {
            ActivateAbility(targetPOI);
        } else if (target is HexTile targetHex) {
            ActivateAbility(targetHex);
        } else if (target is LocationStructure targetStructure) {
            ActivateAbility(targetStructure);
        }
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