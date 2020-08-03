using System.Collections;
using System.Collections.Generic;
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
        _notAllowedStructures = new STRUCTURE_TYPE[] { STRUCTURE_TYPE.INN, STRUCTURE_TYPE.DWELLING, STRUCTURE_TYPE.WAREHOUSE, STRUCTURE_TYPE.PRISON };
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
        stateComponent.character.marker.GoTo(target, StartStrollMovement, null); //_notAllowedStructures
        //Debug.Log(stateComponent.character.name + " will stroll to " + target.ToString());
    }
    private LocationGridTile PickRandomTileToGoTo() {
        if (stateComponent.character.homeSettlement != null) {
            //only stroll around surrounding areas
            List<HexTile> surroundingAreas = stateComponent.character.homeSettlement.GetSurroundingAreas();
            List<LocationGridTile> choices = new List<LocationGridTile>();
            for (int i = 0; i < surroundingAreas.Count; i++) {
                HexTile surroundingArea = surroundingAreas[i];
                choices.AddRange(surroundingArea.locationGridTiles);
            }
            return CollectionUtilities.GetRandomElement(choices);
        } else if (stateComponent.character.hexTileLocation != null){
            //stroll around surrounding area of current hextile
            List<LocationGridTile> choices = new List<LocationGridTile>();
            for (int i = 0; i < stateComponent.character.hexTileLocation.AllNeighbours.Count; i++) {
                HexTile hexTile = stateComponent.character.hexTileLocation.AllNeighbours[i];
                if (hexTile.region == stateComponent.character.currentRegion) {
                    choices.AddRange(hexTile.locationGridTiles);
                }
            }
            return CollectionUtilities.GetRandomElement(choices);
        } else {
            LocationStructure structure = stateComponent.character.currentRegion.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
            LocationGridTile tile = structure.GetRandomTile();
            if(tile != null) {
                return tile;
            } else {
                throw new System.Exception(
                    $"No unoccupied tile in wilderness for {stateComponent.character.name} to go to in {stateName}");
            }    
        }
    }
}
