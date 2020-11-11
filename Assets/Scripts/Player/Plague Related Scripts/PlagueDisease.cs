using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlagueDisease : ISingletonPattern {
    private static PlagueDisease _Instance;

    private PlagueLifespan _lifespan;

    #region getters
    public PlagueLifespan lifespan => _lifespan;
    #endregion

    //Singleton pattern
    public static PlagueDisease Instance {
        get {
            if(_Instance == null) { _Instance = new PlagueDisease(); }

            return _Instance;
        }
    }
    public PlagueDisease() {
        Initialize();
    }

    #region ISingletonPattern
    public void Initialize() {
        AddCleanupListener();
        _lifespan = new PlagueLifespan();
    }
    public void AddCleanupListener() {
        Messenger.AddListener(Signals.CLEAN_UP_MEMORY, CleanUpAndRemoveCleanUpListener);
    }
    public void CleanUpAndRemoveCleanUpListener() {
        _Instance = null;
        Messenger.RemoveListener(Signals.CLEAN_UP_MEMORY, CleanUpAndRemoveCleanUpListener);
    }
    #endregion
}
