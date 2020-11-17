using System;
using System.Collections.Generic;
using Plague.Fatality;
using Plague.Symptom;
using Traits;
using Plague.Death_Effect;
using UnityEngine;
using UnityEngine.Assertions;

public class PlagueDisease : ISingletonPattern {
    private static PlagueDisease _Instance;

    private PlagueLifespan _lifespan;
    private List<Fatality> _activeFatalities;
    private List<PlagueSymptom> _activeSymptoms;
    private PlagueDeathEffect _activeDeathEffect;
    private int _activeCases;
    private int _deaths;
    private int _recoveries;
    private Dictionary<PLAGUE_TRANSMISSION, int> _transmissionLevels;
    
    #region getters
    public PlagueLifespan lifespan => _lifespan;
    public List<Fatality> activeFatalities => _activeFatalities;
    public List<PlagueSymptom> activeSymptoms => _activeSymptoms;
    public PlagueDeathEffect activeDeathEffect => _activeDeathEffect;
    public int activeCases => _activeCases;
    public int deaths => _deaths;
    public int recoveries => _recoveries;
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
        _transmissionLevels = new Dictionary<PLAGUE_TRANSMISSION, int>() {
            {PLAGUE_TRANSMISSION.Airborne, 0},
            {PLAGUE_TRANSMISSION.Consumption, 1},
            {PLAGUE_TRANSMISSION.Physical_Contact, 0},
            {PLAGUE_TRANSMISSION.Combat, 0}
        };
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
    public bool IsFatalityActive(PLAGUE_FATALITY p_fatalityType) {
        for (int i = 0; i < _activeFatalities.Count; i++) {
            Fatality fatality = _activeFatalities[i];
            if (fatality.fatalityType == p_fatalityType) {
                return true;
            }
        }
        return false;
    }
    public bool HasActivatedMaxFatalities() {
        return _activeFatalities.Count == 2;
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
    public bool IsSymptomActive(PLAGUE_SYMPTOM p_symptomType) {
        for (int i = 0; i < _activeSymptoms.Count; i++) {
            PlagueSymptom symptom = _activeSymptoms[i];
            if (symptom.symptomType == p_symptomType) {
                return true;
            }
        }
        return false;
    }
    public bool HasActivatedMaxSymptoms() {
        return _activeSymptoms.Count == 5;
    }
    #endregion

    #region Misc Data
    public void OnPOILostPlagued(IPointOfInterest p_poi) {
        if (p_poi is Character character && character.isNotSummonAndDemon) {
            _activeCases--;
            _recoveries++;
        }
    }
    public void OnPOIGainedPlagued(IPointOfInterest p_poi) {
        if (p_poi is Character character && character.isNotSummonAndDemon) {
            _activeCases++;
        }
    }
    #endregion

    #region Transmission
    public int GetTransmissionLevel(PLAGUE_TRANSMISSION p_transmissionType) {
        if (_transmissionLevels.ContainsKey(p_transmissionType)) {
            return _transmissionLevels[p_transmissionType];    
        }
        Debug.LogError($"Could not find level for transmission type {p_transmissionType.ToString()}");
        return 0;
    } 
    public string GetTransmissionRateDescription(int level) {
        switch (level) {
            case 1:
                return "Low";
            case 2:
                return "Medium";
            case 3:
                return "High";
            default:
                return "N/A";
        }
    }
    public bool IsMaxLevel(PLAGUE_TRANSMISSION p_transmissionType) {
        return GetTransmissionLevel(p_transmissionType) == 3;
    }
    public void UpgradeTransmissionLevel(PLAGUE_TRANSMISSION p_transmissionType) {
        if (_transmissionLevels.ContainsKey(p_transmissionType)) {
            _transmissionLevels[p_transmissionType]++;
            Debug.Log($"Upgraded {p_transmissionType.ToString()} to level {_transmissionLevels[p_transmissionType].ToString()}");
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
