using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public class Sacrifice : CombatAbility {

    private int _explosionRadius;
    public Sacrifice() : base(COMBAT_ABILITY.SACRIFICE) {
        abilityTags.Add(ABILITY_TAG.MAGIC);
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
    protected override void OnLevelUp() {
        base.OnLevelUp();
        if (lvl == 1) {
            _explosionRadius = 3;
        } else if (lvl == 2) {
            _explosionRadius = 4;
        } else if (lvl == 3) {
            _explosionRadius = 5;
        }
    }
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if (targetPOI is Character) {
            Character character = targetPOI as Character;

            GameManager.Instance.CreateAOEEffectAt(character.gridTileLocation, _explosionRadius, true);

            List<LocationGridTile> tilesInRadius = character.gridTileLocation.GetTilesInRadius(_explosionRadius, includeCenterTile: true, includeTilesInDifferentStructure: true);
            List<Character> affectedByExplosion = new List<Character>();
            for (int i = 0; i < character.currentRegion.charactersAtLocation.Count; i++) {
                Character currCharacter = character.currentRegion.charactersAtLocation[i];
                if(currCharacter != character) {
                    if (tilesInRadius.Contains(currCharacter.gridTileLocation) && character.marker.IsCharacterInLineOfSightWith(currCharacter)) {
                        affectedByExplosion.Add(currCharacter);
                    }
                }
            }

            for (int i = 0; i < affectedByExplosion.Count; i++) {
                affectedByExplosion[i].AdjustHP(-1000, ELEMENTAL_TYPE.Normal, true, source: this);
            }

            character.Death("sacrifice");
        }
        base.ActivateAbility(targetPOI);
    }
    #endregion
}
