using System.Collections;
using Scenario_Maps;

public class LoadPlayerQuests : MapGenerationComponent {
    public override IEnumerator ExecuteRandomGeneration(MapGenerationData data) {
        yield return null;
    }

    #region Scenario Maps
    public override IEnumerator LoadScenarioData(MapGenerationData data, ScenarioMapData scenarioMapData) {
        yield return null;
    }
    #endregion

    #region Saved World
    public override IEnumerator LoadSavedData(MapGenerationData data, SaveDataCurrentProgress saveData) {
        yield return MapGenerator.Instance.StartCoroutine(LoadReactionQuests(saveData));
    }
    #endregion

    private IEnumerator LoadReactionQuests(SaveDataCurrentProgress saveData) {
        saveData.LoadWinConditionTracker();
        // saveData.LoadReactionQuests();
        yield return null;
    }
    
    
}