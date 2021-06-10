using System;
using Inner_Maps;
using UnityEngine.Assertions;
using UnityEngine.Profiling;
namespace Locations.Area_Features {
    public class PoisonBloomFeature : AreaFeature {

        private string expirationKey;
        private Area owner;
        
        public int expiryInTicks { get; private set; }
        public GameDate expiryDate { get; private set; }
        public bool isPlayerSource { get; private set; }
        public override Type serializedData => typeof(SaveDataPoisonBloomFeature);
    
        public PoisonBloomFeature() {
            name = "Poison Emitting";
            description = "This location is naturally emitting poison clouds.";
            expiryInTicks = GameManager.ticksPerHour;
        }
        public override void OnAddFeature(Area p_area) {
            base.OnAddFeature(p_area);
            owner = p_area;
            Messenger.AddListener(Signals.TICK_ENDED, EmitPoisonCloudsPerTick);
            ScheduleExpiry();
        }
        public override void OnRemoveFeature(Area p_area) {
            base.OnRemoveFeature(p_area);
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
#if DEBUG_PROFILER
            Profiler.BeginSample($"Emit Poison Cloud Per Tick");
#endif
            if (UnityEngine.Random.Range(0, 100) < 60 && owner != null) {
                LocationGridTile chosenTile = UtilityScripts.CollectionUtilities.GetRandomElement(owner.gridTileComponent.gridTiles);
                PoisonCloud cloud = InnerMapManager.Instance.SpawnPoisonCloud(chosenTile, 3);
                cloud.SetIsPlayerSource(isPlayerSource);
            }
#if DEBUG_PROFILER
            Profiler.EndSample();
#endif
        }
        public void SetIsPlayerSource(bool p_state) {
            isPlayerSource = p_state;
        }
        
#region Expiry
        public void SetExpiryInTicks(int ticks) {
            expiryInTicks = ticks;
        }
#endregion
    }
    
    [System.Serializable]
    public class SaveDataPoisonBloomFeature : SaveDataAreaFeature {

        public int expiryInTicks;
        public bool isPlayerSource;

        public override void Save(AreaFeature tileFeature) {
            base.Save(tileFeature);
            PoisonBloomFeature poisonBloomFeature = tileFeature as PoisonBloomFeature;
            Assert.IsNotNull(poisonBloomFeature, $"Passed feature is not Poison Bloom! {tileFeature?.ToString() ?? "Null"}");
            expiryInTicks = GameManager.Instance.Today().GetTickDifference(poisonBloomFeature.expiryDate);
            isPlayerSource = poisonBloomFeature.isPlayerSource;
        }
        public override AreaFeature Load() {
            PoisonBloomFeature poisonBloomFeature = base.Load() as PoisonBloomFeature;
            Assert.IsNotNull(poisonBloomFeature, $"Passed feature is not Poison Bloom! {poisonBloomFeature?.ToString() ?? "Null"}");
            poisonBloomFeature.SetExpiryInTicks(expiryInTicks);
            poisonBloomFeature.SetIsPlayerSource(isPlayerSource);
            return poisonBloomFeature;
        }
    } 
}
