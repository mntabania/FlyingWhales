using System.Collections.Generic;
using Inner_Maps;
using Interrupts;
using Traits;
using UnityEngine;

public abstract class Wisp : Summon {
    public override string raceClassName => characterClass.className;
    public override COMBAT_MODE defaultCombatMode => COMBAT_MODE.Passive;
    
    protected Wisp(SUMMON_TYPE summonType, string className) : base(summonType, className, RACE.WISP, UtilityScripts.Utilities.GetRandomGender()) {
        //combatComponent.SetCombatMode(COMBAT_MODE.Passive);
        visuals.SetHasBlood(false);
    }
    protected Wisp(SaveDataSummon data) : base(data) {
        //combatComponent.SetCombatMode(COMBAT_MODE.Passive);
        visuals.SetHasBlood(false);
    }

    #region Overrides
    public override void Death(string cause = "normal", ActualGoapNode deathFromAction = null, Character responsibleCharacter = null,
        Log _deathLog = default, LogFillerStruct[] deathLogFillers = null, Interrupt interrupt = null) {
        if (isDead) {
            return;
        }
        LocationGridTile deathTile = gridTileLocation;
        base.Death(cause, deathFromAction, responsibleCharacter, _deathLog, deathLogFillers, interrupt);
        List<LocationGridTile> affectedTiles =
            deathTile.GetTilesInRadius(1, includeCenterTile: true, includeTilesInDifferentStructure: true);
        for (int i = 0; i < affectedTiles.Count; i++) {
            LocationGridTile tile = affectedTiles[i];
            tile.PerformActionOnTraitables(ApplyDamageTo);
        }
        
    }
    #endregion

    private void ApplyDamageTo(ITraitable traitable) {
        if (traitable != this) {
            traitable.AdjustHP(-50, combatComponent.elementalDamage.type, true, this);    
        }
    }
}
