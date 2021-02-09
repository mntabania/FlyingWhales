using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    #endregion
}
