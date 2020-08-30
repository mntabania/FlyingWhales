using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadSecondWave : MapGenerationComponent {

    public override IEnumerator ExecuteRandomGeneration(MapGenerationData data) {
        yield return null;
    }

    #region Saved World
    public override IEnumerator LoadSavedData(MapGenerationData data, SaveDataCurrentProgress saveData) {
        yield return MapGenerator.Instance.StartCoroutine(Load(saveData));
    }
    #endregion

    private IEnumerator Load(SaveDataCurrentProgress saveData) {
        //Load Faction Related Extra Data
        yield return MapGenerator.Instance.StartCoroutine(LoadFactionRelationships(saveData));
        yield return MapGenerator.Instance.StartCoroutine(LoadFactionCharacters(saveData));
        yield return MapGenerator.Instance.StartCoroutine(LoadFactionLogs(saveData));

        //Load Characters

        //Load Tile Objects
    }

    private IEnumerator LoadFactionRelationships(SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading Faction Relationships...");
        saveData.LoadFactionRelationships();
        yield return null;
    }
    private IEnumerator LoadFactionCharacters(SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading Faction Members...");
        saveData.LoadFactionCharacters();
        yield return null;
    }
    private IEnumerator LoadFactionLogs(SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading Faction Logs...");
        saveData.LoadFactionLogs();
        yield return null;
    }
}
