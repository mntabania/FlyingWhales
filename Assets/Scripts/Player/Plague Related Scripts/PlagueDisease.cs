using System;
using System.Collections.Generic;
using Plague.Fatality;
using UnityEngine.Assertions;

public class PlagueDisease : ISingletonPattern {
    private static PlagueDisease _Instance;

    private PlagueLifespan _lifespan;
    private List<Fatality> _activeFatalities;

    #region getters
    public PlagueLifespan lifespan => _lifespan;
    public List<Fatality> activeFatalities => _activeFatalities;
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
        _activeFatalities = new List<Fatality>();
    }
    public void AddCleanupListener() {
        Messenger.AddListener(Signals.CLEAN_UP_MEMORY, CleanUpAndRemoveCleanUpListener);
    }
    public void CleanUpAndRemoveCleanUpListener() {
        _Instance = null;
        Messenger.RemoveListener(Signals.CLEAN_UP_MEMORY, CleanUpAndRemoveCleanUpListener);
    }
    #endregion

    #region Fatalities
    public void AddAndInitializeFatality(FATALITY p_fatalityType) {
        Fatality fatality = CreateNewFatalityInstance(p_fatalityType);
        activeFatalities.Add(fatality);
        Messenger.Broadcast(PlayerSignals.ADDED_PLAGUED_DISEASE_FATALITY, fatality);
    }
    private Fatality CreateNewFatalityInstance(FATALITY fatality) {
        string typeString = fatality.ToString();
        var typeName = $"Plague.Fatality.{UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(typeString)}, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        Type type = Type.GetType(typeName);
        if (type != null) {
            Fatality data = Activator.CreateInstance(type) as Fatality;
            Assert.IsNotNull(data);
            return data;
        } else {
            throw new Exception($"No fatality class of type {fatality}");
        }
    }
    #endregion
}
