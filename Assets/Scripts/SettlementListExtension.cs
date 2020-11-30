using System.Collections.Generic;
using Locations.Settlements;

public static class SettlementListExtension {
    public static List<BaseSettlement> GetSettlementsThatAreUnownedOrNotFriendlyWithFaction(this List<BaseSettlement> p_settlements, LOCATION_TYPE p_locationType, Faction p_otherFaction) {
        List<BaseSettlement> foundSettlements = null;
        for (int i = 0; i < p_settlements.Count; i++) {
            BaseSettlement settlement = p_settlements[i];
            if (settlement.locationType == p_locationType && (settlement.owner == null || !settlement.owner.IsFriendlyWith(p_otherFaction))) {
                if (foundSettlements == null) { foundSettlements = new List<BaseSettlement>(); }
                foundSettlements.Add(settlement);
            }
        }
        return foundSettlements;
    }
}
