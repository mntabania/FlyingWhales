using System;
using System.Collections.Generic;
using Plague.Fatality;
using Plague.Symptom;
using Plague.Death_Effect;
using UnityEngine.Assertions;

public class PlagueDisease : ISingletonPattern {
    private static PlagueDisease _Instance;

    private PlagueLifespan _lifespan;
    private List<Fatality> _activeFatalities;
    private List<PlagueSymptom> _activeSymptoms;
    private PlagueDeathEffect _activeDeathEffect;

    #region getters
    public PlagueLifespan lifespan => _lifespan;
    public List<Fatality> activeFatalities => _activeFatalities;
    public List<PlagueSymptom> activeSymptoms => _activeSymptoms;
    public PlagueDeathEffect activeDeathEffect => _activeDeathEffect;
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
        _activeSymptoms = new List<PlagueSymptom>();
        _activeDeathEffect = null;
    }
    public void AddCleanupListener() {
        Messenger.AddListener(Signals.CLEAN_UP_MEMORY, CleanUpAndRemoveCleanUpListener);
    }
    public void CleanUpAndRemoveCleanUpListener() {
        _activeFatalities.Clear();
        _activeFatalities = null;
        _activeSymptoms.Clear();
        _activeSymptoms = null;
        _activeDeathEffect = null;
        _Instance = null;
        Messenger.RemoveListener(Signals.CLEAN_UP_MEMORY, CleanUpAndRemoveCleanUpListener);
    }
    #endregion

    #region Fatalities
    public void AddAndInitializeFatality(PLAGUE_FATALITY p_fatalityType) {
        Fatality fatality = CreateNewFatalityInstance(p_fatalityType);
        _activeFatalities.Add(fatality);
        Messenger.Broadcast(PlayerSignals.ADDED_PLAGUE_DISEASE_FATALITY, fatality);
    }
    private Fatality CreateNewFatalityInstance(PLAGUE_FATALITY fatality) {
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

    #region Symptoms
    public void AddAndInitializeSymptom(PLAGUE_SYMPTOM p_symptomType) {
        PlagueSymptom symptom = CreateNewSymptomInstance(p_symptomType);
        _activeSymptoms.Add(symptom);
        Messenger.Broadcast(PlayerSignals.ADDED_PLAGUE_DISEASE_SYMPTOM, symptom);
    }
    private PlagueSymptom CreateNewSymptomInstance(PLAGUE_SYMPTOM p_symptomType) {
        string typeString = p_symptomType.ToString();
        var typeName = $"Plague.Symptom.{UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(typeString)}, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        Type type = Type.GetType(typeName);
        if (type != null) {
            PlagueSymptom data = Activator.CreateInstance(type) as PlagueSymptom;
            Assert.IsNotNull(data);
            return data;
        } else {
            throw new Exception($"No plague symptom class of type {p_symptomType}");
        }
    }
    #endregion

    #region Death Effect
    public void SetNewPlagueDeathEffectAndUnsetPrev(PLAGUE_DEATH_EFFECT p_deathEffectType) {
        UnseteDeathEffect();
        PlagueDeathEffect deathEffect = CreateNewPlagueDeathEffectInstance(p_deathEffectType);
        _activeDeathEffect = deathEffect;
        Messenger.Broadcast(PlayerSignals.SET_PLAGUE_DEATH_EFFECT, _activeDeathEffect);
    }
    private void UnseteDeathEffect() {
        if(_activeDeathEffect != null) {
            Messenger.Broadcast(PlayerSignals.UNSET_PLAGUE_DEATH_EFFECT, _activeDeathEffect);
            _activeDeathEffect = null;
        }
    }
    private PlagueDeathEffect CreateNewPlagueDeathEffectInstance(PLAGUE_DEATH_EFFECT p_deathEffectType) {
        string typeString = p_deathEffectType.ToString();
        var typeName = $"Plague.Death_Effect.{UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(typeString)}, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        Type type = Type.GetType(typeName);
        if (type != null) {
            PlagueDeathEffect data = Activator.CreateInstance(type) as PlagueDeathEffect;
            Assert.IsNotNull(data);
            return data;
        } else {
            throw new Exception($"No plague death effect class of type {p_deathEffectType}");
        }
    }
    #endregion
}
