using System.Collections.Generic;
namespace Locations.Region_Features {
    public class RegionFeatureComponent {

        public List<RegionFeature> features { get; }
        
        public RegionFeatureComponent() {
            features = new List<RegionFeature>();
        }
        public void AddFeature(string featureName) {
            RegionFeature regionFeature = LandmarkManager.Instance.CreateRegionFeature<RegionFeature>(featureName);
            AddFeature(regionFeature);
        }
        public void AddFeature(RegionFeature feature) {
            features.Add(feature);
        }
        public bool HasFeature<T>() {
            for (int i = 0; i < features.Count; i++) {
                RegionFeature regionFeature = features[i];
                if (regionFeature is T) {
                    return true;
                }
            }
            return false;
        }

    }
}