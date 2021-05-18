using System.Collections;
using System.Diagnostics;
using System.Globalization;
using Locations.Region_Features;
using Locations.Area_Features;
using Scenario_Maps;
namespace Generator.Map_Generation.Components {
    public class FeaturesActivation : MapGenerationComponent {
        public override IEnumerator ExecuteRandomGeneration(MapGenerationData data) {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            for (int j = 0; j < GridMap.Instance.mainRegion.regionFeatureComponent.features.Count; j++) {
                RegionFeature feature = GridMap.Instance.mainRegion.regionFeatureComponent.features[j];
                feature.ActivateFeatureInWorldGen(GridMap.Instance.mainRegion);
            }
            stopwatch.Stop();
            AddLog($"{GridMap.Instance.mainRegion.name} Region FeaturesActivation took {stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds to complete.");
            yield return MapGenerator.Instance.StartCoroutine(ExecuteFeatureInitialActions(stopwatch));
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
        
        private IEnumerator ExecuteFeatureInitialActions(Stopwatch stopwatch) {
            stopwatch.Reset();
            stopwatch.Start();
            for (int i = 0; i < GridMap.Instance.allAreas.Count; i++) {
                Area tile = GridMap.Instance.allAreas[i];
                for (int j = 0; j < tile.featureComponent.features.Count; j++) {
                    AreaFeature feature = tile.featureComponent.features[j];
                    feature.GameStartActions(tile);
                }
                yield return null;
            }
            stopwatch.Stop();
            AddLog($"{GridMap.Instance.mainRegion.name} ExecuteFeatureInitialActions took {stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds to complete.");
        }
    }
}