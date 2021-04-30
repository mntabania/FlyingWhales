using System.Collections.Generic;
using Inner_Maps;
using Interrupts;
using Traits;
using UnityEngine;
using UtilityScripts;

public class Sludge : Summon {
    public override string raceClassName => characterClass.className;
    public Sludge() : base(SUMMON_TYPE.Sludge, "Sludge", RACE.SLUDGE, UtilityScripts.Utilities.GetRandomGender()) { }
    public Sludge(string className) : base(SUMMON_TYPE.Sludge, className, RACE.SLUDGE, UtilityScripts.Utilities.GetRandomGender()) { }
    public Sludge(SaveDataSummon data) : base(data) { }

    #region Overrides
    public override void Initialize() {
        base.Initialize();
        behaviourComponent.ChangeDefaultBehaviourSet(CharacterManager.Sludge_Behaviour);
    }
    public override void Death(string cause = "normal", ActualGoapNode deathFromAction = null, Character responsibleCharacter = null,
        Log _deathLog = default, LogFillerStruct[] deathLogFillers = null, Interrupt interrupt = null, bool isPlayerSource = false) {
        if (isDead) {
            return;
        }
        LocationGridTile deathTile = gridTileLocation;
        base.Death(cause, deathFromAction, responsibleCharacter, _deathLog, deathLogFillers, interrupt, isPlayerSource);
        List<LocationGridTile> affectedTiles = RuinarchListPool<LocationGridTile>.Claim();
        deathTile.PopulateTilesInRadius(affectedTiles, 1, includeCenterTile: true, includeTilesInDifferentStructure: true);
        for (int i = 0; i < affectedTiles.Count; i++) {
            LocationGridTile tile = affectedTiles[i];
            tile.PerformActionOnTraitables(ApplyDamageTo);
        }
        RuinarchListPool<LocationGridTile>.Release(affectedTiles);
    }
    #endregion

    private void ApplyDamageTo(ITraitable traitable) {
        if (traitable is Character) { return; } //ignore characters
        traitable.AdjustHP(-50, combatComponent.elementalDamage.type, true, this);
    }
}