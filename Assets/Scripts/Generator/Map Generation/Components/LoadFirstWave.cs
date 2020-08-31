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
        yield return MapGenerator.Instance.StartCoroutine(LoadTileObjects(saveData));
        
        //Load Traits
        yield return MapGenerator.Instance.StartCoroutine(LoadTraits(saveData));
    }

    private IEnumerator LoadFactions(SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading Factions...");
        saveData.LoadFactions();
        yield return null;
    }
    private IEnumerator LoadTileObjects(SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading Objects...");
        saveData.LoadTileObjects();
        yield return null;
    }
    private IEnumerator LoadTraits(SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Load Traits...");
        saveData.LoadTraits();
        yield return null;
    }
}
