using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public class Flamestrike : CombatAbility {

    private int _damage;
    public Flamestrike() : base(COMBAT_ABILITY.FLAMESTRIKE) {
        abilityTags.Add(ABILITY_TAG.MAGIC);
        abilityRadius = 3;
        cooldown = 100;
        _currentCooldown = 100;
        _damage = 1000;
    }

    #region Overrides
    protected override void OnLevelUp() {
        base.OnLevelUp();
        if (lvl == 1) {
            abilityRadius = 3;
        } else if (lvl == 2) {
            abilityRadius = 4;
        } else if (lvl == 3) {
            abilityRadius = 5;
        }
    }
    public override void ActivateAbility(List<IPointOfInterest> targetPOIs) {
        GameManager.Instance.CreateAOEEffectAt(InnerMapManager.Instance.GetTileFromMousePosition(), abilityRadius, true);
        for (int i = 0; i < targetPOIs.Count; i++) {
            if (targetPOIs[i] is Character) {
                Character character = targetPOIs[i] as Character;
                character.AdjustHP(-_damage, ELEMENTAL_TYPE.Normal, true, source: this, showHPBar: true);
            }
        }
        base.ActivateAbility(targetPOIs);
    }
    #endregion
}
