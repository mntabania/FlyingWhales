﻿using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Traits;
using UnityEngine;

public interface IDamageable {
    string name { get; }
	int currentHP { get; }
    int maxHP { get; }
    ProjectileReceiver projectileReceiver { get; } //the object to be targeted when a character decides to do damage to this object
    LocationGridTile gridTileLocation { get; }
    BaseMapObjectVisual mapObjectVisual { get; }

    void AdjustHP(int amount, ELEMENTAL_TYPE elementalDamageType, bool triggerDeath = false, object source = null,
        CombatManager.ElementalTraitProcessor elementalTraitProcessor = null, bool showHPBar = false, float piercingPower = 0f, bool isPlayerSource = false);
    void OnHitByAttackFrom(Character characterThatAttacked, CombatState combatStateOfAttacker, ref string attackSummary);
    bool CanBeDamaged();
}
