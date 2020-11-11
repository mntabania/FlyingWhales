using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CleanUpMemoryManager : MonoBehaviour {
    public static CleanUpMemoryManager Instance;

    void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        } else {
            Destroy(this.gameObject);
        }
    }
    void Start()
    {
        SceneManager.sceneUnloaded += OnSceneUnLoaded;
    }

    private void OnSceneUnLoaded(Scene p_scene) {
        //Called every time a scene is unloaded
        BroadcastCleanupMemorySignal();
        CleanupMessenger();
    }
    private void BroadcastCleanupMemorySignal() {
        Messenger.Broadcast(Signals.CLEAN_UP_MEMORY);
    }
    private void CleanupMessenger() {
        Messenger.Cleanup();
    }
}
