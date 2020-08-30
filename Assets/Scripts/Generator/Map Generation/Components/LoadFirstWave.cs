using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadFirstWave : MapGenerationComponent {

    public override IEnumerator ExecuteRandomGeneration(MapGenerationData data) {
        yield return null;
    }

    #region Saved World
    public override IEnumerator LoadSavedData(MapGenerationData data, SaveDataCurrentProgress saveData) {
        yield return MapGenerator.Instance.StartCoroutine(Load(saveData));
    }
    #endregion

    private IEnumerator Load(SaveDataCurrentProgress saveData) {
        //Load Factions
        yield return MapGenerator.Instance.StartCoroutine(LoadFactions(saveData));

        //Load Characters

        //Load Tile Objects
    }

    private IEnumerator LoadFactions(SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading Factions...");
        saveData.LoadFactions();
        yield return null;
    }
}
