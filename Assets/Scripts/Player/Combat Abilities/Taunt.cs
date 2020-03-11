using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public class Taunt : CombatAbility {

    //private int _hpGain;
    public Taunt() : base(COMBAT_ABILITY.TAUNT) {
        cooldown = 10;
        _currentCooldown = 10;
    }

    #region Overrides
    public override bool CanTarget(IPointOfInterest targetPOI) {
        if (targetPOI is Character) {
            Character character = targetPOI as Character;
            if (character.faction.isPlayerFaction) {
                return true;
            }
        }
        return false;
    }
    //protected override void OnLevelUp() {
    //    base.OnLevelUp();
    //    if (lvl == 1) {
    //        _hpGain = 1000;
    //    } else if (lvl == 2) {
    //        _hpGain = 1500;
    //    } else if (lvl == 3) {
    //        _hpGain = 2000;
    //    }
    //}
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if (targetPOI is Character) {
            Character character = targetPOI as Character;

            character.AdjustHP(1000, ELEMENTAL_TYPE.Normal);
            List<LocationGridTile> tilesInRadius = character.gridTileLocation.GetTilesInRadius(3, includeCenterTile: true, includeTilesInDifferentStructure: true);
            List<Character> affectedByTaunt = new List<Character>();
            for (int i = 0; i < character.currentRegion.charactersAtLocation.Count; i++) {
                Character currCharacter = character.currentRegion.charactersAtLocation[i];
                if (currCharacter != character && !currCharacter.faction.isPlayerFaction) {
                    if (tilesInRadius.Contains(currCharacter.gridTileLocation) && character.marker.IsCharacterInLineOfSightWith(currCharacter)) {
                        affectedByTaunt.Add(currCharacter);
                    }
                }
            }

            //for (int i = 0; i < affectedByTaunt.Count; i++) {
            //    Character affected = affectedByTaunt[i];
            //    if(affected.isInCombat) {
            //        CombatState combatState = affected.stateComponent.currentState as CombatState;
            //        combatState.SwitchTarget(character);
            //    } else {
            //        affected.combatComponent.Fight(character);
            //    }
            //}
        }
        base.ActivateAbility(targetPOI);
    }
    #endregion
}
