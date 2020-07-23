using System.Collections;
using Locations.Region_Features;
namespace Generator.Map_Generation.Components {
    public class RegionFeatureActivation : MapGenerationComponent {
        public override IEnumerator Execute(MapGenerationData data) {
            for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
                Region region = GridMap.Instance.allRegions[i];
                for (int j = 0; j < region.regionFeatureComponent.features.Count; j++) {
                    RegionFeature feature = region.regionFeatureComponent.features[j];
                    feature.ActivateFeatureInWorldGen(region);
                }
            }
            yield return null;
        }
    }
}