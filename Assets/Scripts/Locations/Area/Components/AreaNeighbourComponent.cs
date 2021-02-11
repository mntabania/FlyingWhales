﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

public class AreaNeighbourComponent : AreaComponent {
    public List<Area> neighbours { get; private set; }

    public AreaNeighbourComponent() {
        neighbours = new List<Area>();
    }

    #region Utilities
    //Removed this because Func creates so much garbage
    //public bool HasNeighbourThatMeetCriteria(Func<Area, bool> criteria) {
    //    for (int i = 0; i < neighbours.Count; i++) {
    //        Area neighbour = neighbours[i];
    //        if (criteria.Invoke(neighbour)) {
    //            return true;
    //        }
    //    }
    //    return false;
    //}
    public void AddNeighbour(Area p_area) {
        neighbours.Add(p_area);
    }
    public bool HasNeighbourWithElevation(ELEVATION elevation) {
        for (int i = 0; i < neighbours.Count; i++) {
            Area neighbour = neighbours[i];
            if (neighbour.elevationType == elevation) {
                return true;
            }
        }
        return false;
    }
    public bool HasNeighbourWithFeature(string feature) {
        for (int i = 0; i < neighbours.Count; i++) {
            Area neighbour = neighbours[i];
            if (neighbour.featureComponent.HasFeature(feature)) {
                return true;
            }
        }
        return false;
    }
    public bool HasOwnedSettlementNeighbour() {
        for (int i = 0; i < neighbours.Count; i++) {
            Area neighbour = neighbours[i];
            if (neighbour.settlementOnArea?.owner != null) {
                return true;
            }
        }
        return false;
    }
    private bool HasSettlementNeighbour() {
        for (int i = 0; i < neighbours.Count; i++) {
            Area neighbour = neighbours[i];
            if (neighbour.settlementOnArea != null && neighbour.settlementOnArea.locationType == LOCATION_TYPE.VILLAGE) {
                return true;
            }
        }
        return false;
    }
    public bool IsNextToVillage() {
        for (int i = 0; i < neighbours.Count; i++) {
            Area neighbour = neighbours[i];
            if (neighbour.region == owner.region && neighbour.IsPartOfVillage()) {
                return true;
            }
        }
        return false;
    }
    public Area GetRandomAdjacentHextileWithinRegion(bool includeSelf = false) {
        if (includeSelf && GameUtilities.RollChance(15)) {
            return owner;
        } else {
            List<Area> neighbours = ObjectPoolManager.Instance.CreateNewAreaList();
            PopulatePlainNeighboursWithinRegion(neighbours);
            Area chosenArea = null;
            if (neighbours.Count > 0) {
                chosenArea = neighbours[UnityEngine.Random.Range(0, neighbours.Count)];
            }
            ObjectPoolManager.Instance.ReturnAreaListToPool(neighbours);
            return chosenArea;
        }
    }
    public Area GetRandomAdjacentNoSettlementHextileWithinRegion(bool includeSelf = false) {
        if (includeSelf && GameUtilities.RollChance(15)) {
            return owner;
        } else {
            List<Area> neighbours = ObjectPoolManager.Instance.CreateNewAreaList();
            PopulatePlainNoSettlementNeighboursWithinRegion(neighbours);
            Area chosenArea = null;
            if (neighbours.Count > 0) {
                chosenArea = neighbours[UnityEngine.Random.Range(0, neighbours.Count)];
            }
            ObjectPoolManager.Instance.ReturnAreaListToPool(neighbours);
            return chosenArea;
        }
    }
    public Area GetNearestPlainAreaWithNoResident() {
        if (owner.areaData.elevationType != ELEVATION.WATER && owner.areaData.elevationType != ELEVATION.MOUNTAIN) {
            if (!owner.HasAliveVillagerResident()) {
                return owner;
            }
        }
        for (int i = 0; i < neighbours.Count; i++) {
            Area neighbour = neighbours[i];
            if (neighbour.elevationType != ELEVATION.WATER && neighbour.elevationType != ELEVATION.MOUNTAIN) {
                if (!neighbour.HasAliveVillagerResident()) {
                    return neighbour;
                }
            }
        }

        //Stopped this first because this can cause infinite loop since we do not flag neighbour that is already checked
        //for (int i = 0; i < neighbours.Count; i++) {
        //    Area neighbour = neighbours[i];
        //    Area nearestArea = neighbour.neighbourComponent.GetNearestPlainAreaWithNoResident();
        //    if (nearestArea != null) {
        //        return nearestArea;
        //    }
        //}
        return null;
    }
    public void PopulatePlainNeighbours(List<Area> areas) {
        for (int i = 0; i < neighbours.Count; i++) {
            Area area = neighbours[i];
            if(area.elevationType != ELEVATION.WATER && area.elevationType != ELEVATION.MOUNTAIN) {
                areas.Add(area);
            }
        }
    }
    public void PopulatePlainNeighboursWithinRegion(List<Area> areas) {
        for (int i = 0; i < neighbours.Count; i++) {
            Area area = neighbours[i];
            if (owner.region == area.region && area.elevationType != ELEVATION.WATER && area.elevationType != ELEVATION.MOUNTAIN) {
                areas.Add(area);
            }
        }
    }
    public void PopulatePlainNoSettlementNeighboursWithinRegion(List<Area> areas) {
        for (int i = 0; i < neighbours.Count; i++) {
            Area area = neighbours[i];
            if (owner.region == area.region && area.settlementOnArea == null && area.elevationType != ELEVATION.WATER && area.elevationType != ELEVATION.MOUNTAIN) {
                areas.Add(area);
            }
        }
    }
    #endregion
}
