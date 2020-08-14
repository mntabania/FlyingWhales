using System.Collections;
using Locations.Region_Features;
using Scenario_Maps;
namespace Generator.Map_Generation.Components {
    public class RegionFeatureActivation : MapGenerationComponent {
        public override IEnumerator ExecuteRandomGeneration(MapGenerationData data) {
            for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
                Region region = GridMap.Instance.allRegions[i];
                for (int j = 0; j < region.regionFeatureComponent.features.Count; j++) {
                    RegionFeature feature = region.regionFeatureComponent.features[j];
                    feature.ActivateFeatureInWorldGen(region);
                }
            }
            yield return null;
        }

        #region Scenario Maps
        public override IEnumerator LoadScenarioData(MapGenerationData data, ScenarioMapData scenarioMapData) {
            yield return MapGenerator.Instance.StartCoroutine(ExecuteRandomGeneration(data));
        }
        #endregion
        
        #region Saved World
        public override IEnumerator LoadSavedData(MapGenerationData data, SaveDataCurrentProgress saveData) {
            yield return MapGenerator.Instance.StartCoroutine(ExecuteRandomGeneration(data));
        }
        #endregion
    }
}