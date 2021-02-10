using System.Collections;
using Locations.Region_Features;
using Locations.Tile_Features;
using Scenario_Maps;
namespace Generator.Map_Generation.Components {
    public class FeaturesActivation : MapGenerationComponent {
        public override IEnumerator ExecuteRandomGeneration(MapGenerationData data) {
            for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
                Region region = GridMap.Instance.allRegions[i];
                for (int j = 0; j < region.regionFeatureComponent.features.Count; j++) {
                    RegionFeature feature = region.regionFeatureComponent.features[j];
                    feature.ActivateFeatureInWorldGen(region);
                }
            }
            yield return MapGenerator.Instance.StartCoroutine(ExecuteFeatureInitialActions());
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
        
        private IEnumerator ExecuteFeatureInitialActions() {
            for (int i = 0; i < GridMap.Instance.allAreas.Count; i++) {
                HexTile tile = GridMap.Instance.allAreas[i];
                for (int j = 0; j < tile.featureComponent.features.Count; j++) {
                    TileFeature feature = tile.featureComponent.features[j];
                    feature.GameStartActions(tile);
                }
                yield return null;
            }
        }
    }
}