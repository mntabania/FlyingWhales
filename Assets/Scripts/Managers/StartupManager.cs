using UnityEngine;
using System.Collections;

public class StartupManager : MonoBehaviour {
	public MapGenerator mapGenerator;
    public Initializer initializer;

    void Awake() {
        Messenger.Cleanup();
    }
    void Start(){
        Messenger.AddListener(Signals.GAME_LOADED, OnGameLoaded);
        Messenger.AddListener(Signals.START_GAME_AFTER_LOADOUT_SELECT, OnLoadoutSelected);
        StartCoroutine(PerformStartup());
    }

    private IEnumerator PerformStartup() {
        LevelLoaderManager.Instance.SetLoadingState(true);
        LevelLoaderManager.Instance.UpdateLoadingInfo("Initializing Data...");
        yield return StartCoroutine(initializer.InitializeDataBeforeWorldCreation());

        LevelLoaderManager.Instance.UpdateLoadingInfo("Initializing World...");
        Debug.Log("Generating random world...");
        yield return StartCoroutine(this.mapGenerator.InitializeWorld());
    }

    private void OnGameLoaded() {
        Messenger.RemoveListener(Signals.GAME_LOADED, OnGameLoaded);
        initializer.InitializeDataAfterWorldCreation();
    }
    private void OnLoadoutSelected() {
        Messenger.RemoveListener(Signals.START_GAME_AFTER_LOADOUT_SELECT, OnLoadoutSelected);
        initializer.InitializeDataAfterLoadoutSelection();
    }
}
