using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;

public class StrollOutsideState : CharacterState {

    private STRUCTURE_TYPE[] _notAllowedStructures;
    //private int _planDuration;

    public StrollOutsideState(CharacterStateComponent characterComp) : base(characterComp) {
        stateName = "Stroll Outside State";
        characterState = CHARACTER_STATE.STROLL_OUTSIDE;
        //stateCategory = CHARACTER_STATE_CATEGORY.MAJOR;
        duration = GameManager.ticksPerHour;
        //_planDuration = 0;
        _notAllowedStructures = new STRUCTURE_TYPE[] { STRUCTURE_TYPE.TAVERN, STRUCTURE_TYPE.DWELLING, STRUCTURE_TYPE.WAREHOUSE, STRUCTURE_TYPE.PRISON };
    }

    #region Overrides
    protected override void DoMovementBehavior() {
        base.DoMovementBehavior();
        StartStrollMovement();
    }
    #endregion
    
    private void StrollAgain() {
        DoMovementBehavior();
    }

    private void StartStrollMovement() {
        LocationGridTile target = PickRandomTileToGoTo();
        if(target != null) {
            stateComponent.character.marker.GoTo(target, StartStrollMovement, null); //_notAllowedStructures
        }
        //Debug.Log(stateComponent.character.name + " will stroll to " + target.ToString());
    }
    private LocationGridTile PickRandomTileToGoTo() {
        LocationGridTile targetTile = null;
        //List<LocationGridTile> choices = stateComponent.character.gridTileLocation.GetTilesInRadius(3, includeImpassable: false);
        //if (choices.Count > 0) {
        //    targetTile = choices[UtilityScripts.Utilities.Rng.Next(0, choices.Count)];
        //} else {
        //    targetTile = stateComponent.character.gridTileLocation;
        //}
        if (stateComponent.character.homeSettlement != null) {
            //only stroll around surrounding areas
            HexTile chosenHex = stateComponent.character.homeSettlement.GetAPlainAdjacentHextile();
            if (chosenHex != null) {
                return CollectionUtilities.GetRandomElement(chosenHex.locationGridTiles.Where(t => stateComponent.character.movementComponent.HasPathTo(t)));
            }
        }
        if (stateComponent.character.hexTileLocation != null) {
            //stroll around surrounding area of current hextile
            List<HexTile> choices = new List<HexTile>();
            for (int i = 0; i < stateComponent.character.hexTileLocation.ValidTilesWithinRegion.Count; i++) {
                HexTile hexTile = stateComponent.character.hexTileLocation.ValidTilesWithinRegion[i];
                if (stateComponent.character.movementComponent.HasPathTo(hexTile)) {
                    choices.Add(hexTile);
                }
            }
            if (choices != null && choices.Count > 0) {
                HexTile chosenHex = CollectionUtilities.GetRandomElement(choices);
                return CollectionUtilities.GetRandomElement(chosenHex.locationGridTiles);
            }
        }

        HexTile hex = stateComponent.character.gridTileLocation.GetNearestHexTileWithinRegion();
        targetTile = CollectionUtilities.GetRandomElement(hex.locationGridTiles);
        return targetTile;
    }
}
