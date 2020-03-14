
using System;
using Inner_Maps;

public class PoisonBloomFeature : TileFeature {

    private string expirationKey;
    
    public PoisonBloomFeature() {
        name = "Poison Emitting";
        description = "This location is naturally emitting poison clouds.";
    }
    public override void OnAddFeature(HexTile tile) {
        base.OnAddFeature(tile);
        Messenger.AddListener(Signals.TICK_ENDED, () => EmitPoisonCloudsPerTick(tile));
        ScheduleExpiry(tile);
    }
    public override void OnRemoveFeature(HexTile tile) {
        base.OnRemoveFeature(tile);
        Messenger.RemoveListener(Signals.TICK_ENDED, () => EmitPoisonCloudsPerTick(tile));
    }
    public void ResetDuration(HexTile tile) {
        SchedulingManager.Instance.RemoveSpecificEntry(expirationKey);
        ScheduleExpiry(tile);
    }
    private void ScheduleExpiry(HexTile tile) {
        GameDate dueDate = GameManager.Instance.Today().AddTicks(GameManager.ticksPerHour);
        expirationKey = SchedulingManager.Instance.AddEntry(dueDate, () => tile.featureComponent.RemoveFeature(this, tile), this);
    }
    private void EmitPoisonCloudsPerTick(HexTile tile) {
        if (UnityEngine.Random.Range(0, 100) < 60) {
            LocationGridTile chosenTile = UtilityScripts.CollectionUtilities.GetRandomElement(tile.locationGridTiles);
            PoisonCloudTileObject poisonCloudTileObject = new PoisonCloudTileObject();
            poisonCloudTileObject.SetGridTileLocation(chosenTile);
            poisonCloudTileObject.OnPlacePOI();
            //add poisoned status so size of cloud is updated.
            for (int i = 0; i < 3; i++) {
                poisonCloudTileObject.traitContainer.AddTrait(poisonCloudTileObject, "Poisoned", overrideDuration: 0);
            }    
        }
    }
}
