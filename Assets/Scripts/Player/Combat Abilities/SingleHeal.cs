using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleHeal : CombatAbility {

    private float _healPercentage;

	public SingleHeal() : base (COMBAT_ABILITY.SINGLE_HEAL) {
        abilityTags.Add(ABILITY_TAG.MAGIC);
        abilityTags.Add(ABILITY_TAG.SUPPORT);
        cooldown = 10;
        _currentCooldown = 10;
    }

    #region Overrides
    public override bool CanTarget(IPointOfInterest targetPOI) {
        if(targetPOI is Character) {
            Character character = targetPOI as Character;
            if (character.minion != null) {
                return true;
            }
        }
        return false;
    }
    protected override void OnLevelUp() {
        base.OnLevelUp();
        if (lvl == 1) {
            _healPercentage = 25f;
        } else if (lvl == 2) {
            _healPercentage = 35f;
        } else if (lvl == 3) {
            _healPercentage = 45f;
        }
    }
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if(targetPOI is Character) {
            Character character = targetPOI as Character;
            int heal = Mathf.CeilToInt((_healPercentage / 100f) * character.maxHP);
            character.AdjustHP(heal, ELEMENTAL_TYPE.Normal);
        }
        base.ActivateAbility(targetPOI);
    }
    #endregion
}
