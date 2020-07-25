namespace Locations.Region_Features {
    public class DragonFeature : RegionFeature {
        public override void ActivateFeatureInWorldGen(Region region) {
            base.ActivateFeatureInWorldGen(region);
            if (region.structures.ContainsKey(STRUCTURE_TYPE.CAVE)) {
                //TODO: Spawn Dragon    
            }
        }
    }
}