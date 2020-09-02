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
        yield return MapGenerator.Instance.StartCoroutine(LoadCharacters(saveData));

        //Load Tile Objects
        yield return MapGenerator.Instance.StartCoroutine(LoadTileObjects(saveData));

        yield return MapGenerator.Instance.StartCoroutine(LoadActions(saveData));
        yield return MapGenerator.Instance.StartCoroutine(LoadInterrupts(saveData));
        yield return MapGenerator.Instance.StartCoroutine(LoadLogs(saveData));
        yield return MapGenerator.Instance.StartCoroutine(LoadParties(saveData));
        yield return MapGenerator.Instance.StartCoroutine(LoadCrimes(saveData));
    }

    private IEnumerator LoadFactions(SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading Factions...");
        saveData.LoadFactions();
        yield return null;
    }
    private IEnumerator LoadCharacters(SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading Characters..."); //The loading can't say "Loading Interrupts..." so it is classified as an action
        saveData.LoadCharacters();
        yield return null;
    }

    private IEnumerator LoadTileObjects(SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading Objects...");
        saveData.LoadTileObjects();
        yield return null;
    }

    private IEnumerator LoadLogs(SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading Logs...");
        saveData.LoadLogs();
        yield return null;
    }

    private IEnumerator LoadActions(SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading Actions...");
        saveData.LoadActions();
        yield return null;
    }

    private IEnumerator LoadInterrupts(SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading Actions..."); //The loading can't say "Loading Interrupts..." so it is classified as an action
        saveData.LoadInterrupts();
        yield return null;
    }

    private IEnumerator LoadParties(SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading Parties..."); //The loading can't say "Loading Interrupts..." so it is classified as an action
        saveData.LoadParties();
        yield return null;
    }

    private IEnumerator LoadCrimes(SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading Crimes..."); //The loading can't say "Loading Interrupts..." so it is classified as an action
        saveData.LoadCrimes();
        yield return null;
    }
}
