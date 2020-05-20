using Inner_Maps;

namespace Locations.Features {
    public class PoisonBloomFeature : TileFeature {

        private string expirationKey;
        private HexTile owner;
    
        public PoisonBloomFeature() {
            name = "Poison Emitting";
            description = "This location is naturally emitting poison clouds.";
        }
        public override void OnAddFeature(HexTile tile) {
            base.OnAddFeature(tile);
            owner = tile;
            Messenger.AddListener(Signals.TICK_ENDED, EmitPoisonCloudsPerTick);
            ScheduleExpiry();
        }
        public override void OnRemoveFeature(HexTile tile) {
            base.OnRemoveFeature(tile);
            Messenger.RemoveListener(Signals.TICK_ENDED, EmitPoisonCloudsPerTick);
            owner = null;
        }
        public void ResetDuration() {
            if (owner != null) {
                SchedulingManager.Instance.RemoveSpecificEntry(expirationKey);
                ScheduleExpiry();    
            }
        }
        private void ScheduleExpiry() {
            GameDate dueDate = GameManager.Instance.Today().AddTicks(GameManager.ticksPerHour);
            expirationKey = SchedulingManager.Instance.AddEntry(dueDate, () => owner.featureComponent.RemoveFeature(this, owner), this);
        }
        private void EmitPoisonCloudsPerTick() {
            if (UnityEngine.Random.Range(0, 100) < 60 && owner != null) {
                LocationGridTile chosenTile = UtilityScripts.CollectionUtilities.GetRandomElement(owner.locationGridTiles);
                PoisonCloudTileObject poisonCloudTileObject = new PoisonCloudTileObject();
                poisonCloudTileObject.SetGridTileLocation(chosenTile);
                poisonCloudTileObject.OnPlacePOI();
                poisonCloudTileObject.SetStacks(3);
            }
        }
    }
}
