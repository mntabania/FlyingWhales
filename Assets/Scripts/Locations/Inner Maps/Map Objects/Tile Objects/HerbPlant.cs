using System.Collections.Generic;
using Locations.Settlements;
public class HerbPlant : TileObject{

    //public BaseSettlement parentSettlement { get; private set; }
    public HerbPlant() {
        Initialize(TILE_OBJECT_TYPE.HERB_PLANT, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
        AddAdvertisedAction(INTERACTION_TYPE.PICK_UP);
        AddAdvertisedAction(INTERACTION_TYPE.DROP_ITEM);
        AddAdvertisedAction(INTERACTION_TYPE.STEAL_ANYTHING);
        AddAdvertisedAction(INTERACTION_TYPE.GATHER_HERB);
        AddAdvertisedAction(INTERACTION_TYPE.CREATE_HOSPICE_POTION);
        AddAdvertisedAction(INTERACTION_TYPE.CREATE_HOSPICE_ANTIDOTE);
        //BaseSettlement.onSettlementBuilt += UpdateSettlementResourcesParent;
    }
    public HerbPlant(SaveDataTileObject data) : base(data) { }

    //protected override void UpdateSettlementResourcesParent() {
    //    if (gridTileLocation != null) {
    //       if (gridTileLocation.area.settlementOnArea != null) {
    //            gridTileLocation.area.settlementOnArea.SettlementResources?.AddToListBasedOnRequirement(SettlementResources.StructureRequirement.HERB_PLANT, this);
    //        }
    //        gridTileLocation.area.neighbourComponent.neighbours.ForEach((eachNeighbor) => {
    //            if (eachNeighbor.settlementOnArea != null) {
    //                //UnityEngine.Debug.LogError(gridTileLocation.area.settlementOnArea + " Added herb count B");
    //                //eachNeighbor.settlementOnArea.SettlementResources?.AddToListBasedOnRequirement(SettlementResources.StructureRequirement.HERB_PLANT, this);
    //                parentSettlement = eachNeighbor.settlementOnArea;
    //            }
    //        });
    //    }
    //}
    //protected override void RemoveFromSettlementResourcesParent() {
    //    if (parentSettlement != null && parentSettlement.SettlementResources != null) {
    //        if (parentSettlement.SettlementResources.herbPlants.Remove(this)) {
    //            parentSettlement = null;
    //        }
    //    }
    //}
    //public override void OnPlacePOI() {
    //    base.OnPlacePOI();
    //    UpdateSettlementResourcesParent();
    //}
    //public override void OnDestroyPOI() {
    //    base.OnDestroyPOI();
    //    //BaseSettlement.onSettlementBuilt -= UpdateSettlementResourcesParent;
    //}
}
