using Inner_Maps;
using UnityEngine.Assertions;
namespace Locations.Tile_Features {
    public class PoisonBloomFeature : TileFeature {

        private string expirationKey;
        private HexTile owner;
        
        public int expiryInTicks { get; private set; }
        public GameDate expiryDate { get; private set; }
    
        public PoisonBloomFeature() {
            name = "Poison Emitting";
            description = "This location is naturally emitting poison clouds.";
            expiryInTicks = GameManager.ticksPerHour;
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
            expiryDate = GameManager.Instance.Today().AddTicks(expiryInTicks);
            expirationKey = SchedulingManager.Instance.AddEntry(expiryDate, () => owner.featureComponent.RemoveFeature(this, owner), this);
        }
        private void EmitPoisonCloudsPerTick() {
            if (UnityEngine.Random.Range(0, 100) < 60 && owner != null) {
                LocationGridTile chosenTile = UtilityScripts.CollectionUtilities.GetRandomElement(owner.locationGridTiles);
                PoisonCloud poisonCloud = new PoisonCloud();
                poisonCloud.SetGridTileLocation(chosenTile);
                poisonCloud.OnPlacePOI();
                poisonCloud.SetStacks(3);
            }
        }
        
        #region Expiry
        public void SetExpiryInTicks(int ticks) {
            expiryInTicks = ticks;
        }
        #endregion
    }
    
    [System.Serializable]
    public class SaveDataPoisonBloomFeature : SaveDataTileFeature {

        public int expiryInTicks;
        public override void Save(TileFeature tileFeature) {
            base.Save(tileFeature);
            PoisonBloomFeature poisonBloomFeature = tileFeature as PoisonBloomFeature;
            Assert.IsNotNull(poisonBloomFeature, $"Passed feature is not Poison Bloom! {tileFeature?.ToString() ?? "Null"}");
            expiryInTicks = GameManager.Instance.Today().GetTickDifference(poisonBloomFeature.expiryDate);
        }
        public override TileFeature Load() {
            PoisonBloomFeature poisonBloomFeature = base.Load() as PoisonBloomFeature;
            Assert.IsNotNull(poisonBloomFeature, $"Passed feature is not Poison Bloom! {poisonBloomFeature?.ToString() ?? "Null"}");
            poisonBloomFeature.SetExpiryInTicks(expiryInTicks);
            return poisonBloomFeature;
        }
    } 
}
