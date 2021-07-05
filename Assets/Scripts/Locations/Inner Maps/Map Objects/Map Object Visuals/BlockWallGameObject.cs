using System.Collections;
using System.Collections.Generic;
using EZObjectPools;
using Inner_Maps;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UtilityScripts;
using System;

public class BlockWallGameObject : TileObjectGameObject {

    public GameObject leftBotImpassable, leftTopImpassable, topLeftImpassable, topRightImpassable, rightTopImpassable, rightBotImpassable, botRightImpassable, botLeftImpassable;

    public void EvaluateImpassables() {
        LocationGridTile currentGridTile = obj.gridTileLocation;
        LocationGridTile gridTile = currentGridTile;
        if(gridTile == null) {
            gridTile = obj.previousTile;
        }
        if(gridTile != null) {
            LocationGridTile northWestNeighbour = gridTile.GetNeighbourAtDirection(GridNeighbourDirection.North_West);
            LocationGridTile northEastNeighbour = gridTile.GetNeighbourAtDirection(GridNeighbourDirection.North_East);
            LocationGridTile southWestNeighbour = gridTile.GetNeighbourAtDirection(GridNeighbourDirection.South_West);
            LocationGridTile southEastNeighbour = gridTile.GetNeighbourAtDirection(GridNeighbourDirection.South_East);
            LocationGridTile northNeighbour = gridTile.GetNeighbourAtDirection(GridNeighbourDirection.North);
            LocationGridTile westNeighbour = gridTile.GetNeighbourAtDirection(GridNeighbourDirection.West);
            LocationGridTile eastNeighbour = gridTile.GetNeighbourAtDirection(GridNeighbourDirection.East);
            LocationGridTile southNeighbour = gridTile.GetNeighbourAtDirection(GridNeighbourDirection.South);

            ActivateDeactivateImpassables(northEastNeighbour, northNeighbour, topRightImpassable, eastNeighbour, rightTopImpassable);
            ActivateDeactivateImpassables(northWestNeighbour, northNeighbour, topLeftImpassable, westNeighbour, leftTopImpassable);
            ActivateDeactivateImpassables(southEastNeighbour, southNeighbour, botRightImpassable, eastNeighbour, rightBotImpassable);
            ActivateDeactivateImpassables(southWestNeighbour, southNeighbour, botLeftImpassable, westNeighbour, leftBotImpassable);

            ApplyGraphUpdate();
        }
    }

    private void ActivateDeactivateImpassables(LocationGridTile p_diagonalNeighbourToCheck, LocationGridTile p_firstNeighbour, GameObject p_firstNeighbourImpassable, LocationGridTile p_secondNeighbour, GameObject p_secondNeighbourImpassable) {
        if (p_diagonalNeighbourToCheck != null) {
            if (!p_diagonalNeighbourToCheck.IsPassable()) {
                if (p_firstNeighbour != null) {
                    bool isPassable = p_secondNeighbour != null && p_secondNeighbour.IsPassable();
                    p_firstNeighbourImpassable.SetActive(isPassable);
                }
                if (p_secondNeighbour != null) {
                    bool isPassable = p_firstNeighbour != null && p_firstNeighbour.IsPassable();
                    p_secondNeighbourImpassable.SetActive(isPassable);
                }
            } else {
                if (p_firstNeighbour != null) {
                    p_firstNeighbourImpassable.SetActive(false);
                }
                if (p_secondNeighbour != null) {
                    p_secondNeighbourImpassable.SetActive(false);
                }
            }
        }
    }

    public override void Reset() {
        base.Reset();
        leftBotImpassable.SetActive(false);
        leftTopImpassable.SetActive(false);
        topLeftImpassable.SetActive(false);
        topRightImpassable.SetActive(false);
        rightTopImpassable.SetActive(false);
        rightBotImpassable.SetActive(false);
        botRightImpassable.SetActive(false);
        botLeftImpassable.SetActive(false);
    }
}