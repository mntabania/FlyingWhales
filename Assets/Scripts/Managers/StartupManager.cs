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
        StartCoroutine(PerformStartup());
    }

    private IEnumerator PerformStartup() {
        LevelLoaderManager.Instance.SetLoadingState(true);
        LevelLoaderManager.Instance.UpdateLoadingInfo("Initializing Data...");
        yield return StartCoroutine(initializer.InitializeDataBeforeWorldCreation());

        LevelLoaderManager.Instance.UpdateLoadingInfo("Initializing World...");
        if (SaveManager.Instance.currentSave != null) {
            Debug.Log("Loading world from current saved data...");
            this.mapGenerator.InitializeWorld(SaveManager.Instance.currentSave);
        } else {
            Debug.Log("Generating random world...");
            yield return StartCoroutine(this.mapGenerator.InitializeWorld());
        }
    }

    private void OnGameLoaded() {
        Messenger.RemoveListener(Signals.GAME_LOADED, OnGameLoaded);
        initializer.InitializeDataAfterWorldCreation();
    }
}
