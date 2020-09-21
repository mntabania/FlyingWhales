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
        
        //Load Jobs
        yield return MapGenerator.Instance.StartCoroutine(LoadJobs(saveData));

        //Load Characters
        yield return MapGenerator.Instance.StartCoroutine(LoadCharacters(saveData));

        //Load Tile Objects
        yield return MapGenerator.Instance.StartCoroutine(LoadTileObjects(saveData));

        yield return MapGenerator.Instance.StartCoroutine(LoadActions(saveData));
        yield return MapGenerator.Instance.StartCoroutine(LoadInterrupts(saveData));
        yield return MapGenerator.Instance.StartCoroutine(LoadLogs(saveData));
        yield return MapGenerator.Instance.StartCoroutine(LoadParties(saveData));
        yield return MapGenerator.Instance.StartCoroutine(LoadPartyQuests(saveData));
        yield return MapGenerator.Instance.StartCoroutine(LoadCrimes(saveData));
        yield return MapGenerator.Instance.StartCoroutine(LoadGatherings(saveData));

        //Load Traits
        yield return MapGenerator.Instance.StartCoroutine(LoadTraits(saveData));
    }

    private IEnumerator LoadFactions(SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading Factions...");
        saveData.LoadFactions();
        yield return null;
    }
    private IEnumerator LoadCharacters(SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading Characters...");
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
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading Parties...");
        saveData.LoadParties();
        yield return null;
    }

    private IEnumerator LoadPartyQuests(SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading Party Quests...");
        saveData.LoadPartyQuests();
        yield return null;
    }

    private IEnumerator LoadCrimes(SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading Crimes...");
        saveData.LoadCrimes();
        yield return null;
    }
    private IEnumerator LoadTraits(SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading Traits...");
        saveData.LoadTraits();
        yield return null;
    }
    private IEnumerator LoadJobs(SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading Jobs...");
        saveData.LoadJobs();
        yield return null;
    }
    private IEnumerator LoadGatherings(SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading Gatherings...");
        saveData.LoadGatherings();
        yield return null;
    }

}
