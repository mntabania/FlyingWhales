﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Guitar : TileObject, IPointOfInterest {
    public string name { get { return ToString(); } }
    public LocationStructure location { get; private set; }
    public List<INTERACTION_TYPE> poiGoapActions { get; private set; }

    private LocationGridTile tile;
    private POI_STATE _state;

    #region getters/setters
    public POINT_OF_INTEREST_TYPE poiType {
        get { return POINT_OF_INTEREST_TYPE.TILE_OBJECT; }
    }
    public LocationGridTile gridTileLocation {
        get { return tile; }
    }
    public POI_STATE state {
        get { return _state; }
    }
    #endregion

    public Guitar(LocationStructure location) {
        this.location = location;
        poiGoapActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.PLAY_GUITAR };
        Initialize(this, TILE_OBJECT_TYPE.GUITAR);
    }

    public override string ToString() {
        return "Guitar";
    }

    #region Interface
    public void SetGridTileLocation(LocationGridTile tile) {
        if (this.tile == null) {
            tile.SetTileAccess(LocationGridTile.Tile_Access.Passable);
        } else {
            tile.SetTileAccess(LocationGridTile.Tile_Access.Impassable);
        }
        this.tile = tile;
    }
    public LocationGridTile GetNearestUnoccupiedTileFromThis(LocationStructure structure, Character otherCharacter) {
        if (gridTileLocation != null && location == structure) {
            List<LocationGridTile> choices = location.unoccupiedTiles.Where(x => x != gridTileLocation).OrderBy(x => Vector2.Distance(gridTileLocation.localLocation, x.localLocation)).ToList();
            if (choices.Count > 0) {
                LocationGridTile nearestTile = choices[0];
                if (otherCharacter.currentStructure == structure && otherCharacter.gridTileLocation != null) {
                    float ogDistance = Vector2.Distance(this.gridTileLocation.localLocation, otherCharacter.gridTileLocation.localLocation);
                    float newDistance = Vector2.Distance(this.gridTileLocation.localLocation, nearestTile.localLocation);
                    if (newDistance > ogDistance) {
                        return otherCharacter.gridTileLocation; //keep the other character's current tile
                    }
                }
                return nearestTile;
            }
        }
        return null;
    }
    #endregion

    #region Point Of Interest
    public List<GoapAction> AdvertiseActionsToActor(Character actor, List<INTERACTION_TYPE> actorAllowedInteractions) {
        if (poiGoapActions != null && poiGoapActions.Count > 0  && state == POI_STATE.ACTIVE) {
            List<GoapAction> usableActions = new List<GoapAction>();
            for (int i = 0; i < poiGoapActions.Count; i++) {
                if (actorAllowedInteractions.Contains(poiGoapActions[i])) {
                    GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(poiGoapActions[i], actor, this);
                    if (goapAction.CanSatisfyRequirements()) {
                        usableActions.Add(goapAction);
                    }
                }
            }
            return usableActions;
        }

        return null;
    }
    public void SetPOIState(POI_STATE state) {
        _state = state;
    }
    #endregion
}
