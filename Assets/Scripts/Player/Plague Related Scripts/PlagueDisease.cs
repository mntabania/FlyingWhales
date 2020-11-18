using System;
using System.Collections.Generic;
using Plague.Fatality;
using Plague.Symptom;
using Plague.Death_Effect;
using UnityEngine;
using UnityEngine.Assertions;
using System.Linq;

public class PlagueDisease : ISingletonPattern, ISavable {
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
    public OBJECT_TYPE objectType => OBJECT_TYPE.Plague_Disease;
    public Type serializedData => typeof(SaveDataPlagueDisease);
    public string persistentID => "-1"; //No ID needed since this is a singleton
    public PlagueLifespan lifespan => _lifespan;
    public List<Fatality> activeFatalities => _activeFatalities;
    public List<PlagueSymptom> activeSymptoms => _activeSymptoms;
    public PlagueDeathEffect activeDeathEffect => _activeDeathEffect;
    public int activeCases => _activeCases;
    public int deaths => _deaths;
    public int recoveries => _recoveries;
    public Dictionary<PLAGUE_TRANSMISSION, int> transmissionLevels => _transmissionLevels;
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
    public PlagueDisease(SaveDataPlagueDisease p_data) {
        _lifespan = p_data.lifespan.Load();

        _activeFatalities = new List<Fatality>();
        if (p_data.activeFatalities.Count > 0) {
            for (int i = 0; i < p_data.activeFatalities.Count; i++) {
                Fatality fatality = CreateNewFatalityInstance(p_data.activeFatalities[i]);
                _activeFatalities.Add(fatality);
            }
        }

        _activeSymptoms = new List<PlagueSymptom>();
        if (p_data.activeSymptoms.Count > 0) {
            for (int i = 0; i < p_data.activeSymptoms.Count; i++) {
                PlagueSymptom symptom = CreateNewSymptomInstance(p_data.activeSymptoms[i]);
                _activeSymptoms.Add(symptom);
            }
        }

        if (p_data.hasDeathEffect) {
            PlagueDeathEffect deathEffect = CreateNewPlagueDeathEffectInstance(p_data.activeDeathEffect);
            _activeDeathEffect = deathEffect;
            _activeDeathEffect.SetLevel(p_data.activeDeathEffectLevel);
        } else {
            _activeDeathEffect = null;
        }

        _activeCases = p_data.activeCases;
        _deaths = p_data.deaths;
        _recoveries = p_data.recoveries;
        _transmissionLevels = p_data.transmissionLevels;

        //Once save data has been loaded, set this as the Instance
        _Instance = this;
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
    public static bool HasInstance() {
        return _Instance != null;
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
    public void UpdateActiveCasesAndRecoveriesOnPOILostPlagued(IPointOfInterest p_poi) {
        if (p_poi is Character character && character.isNotSummonAndDemon) {
            _activeCases--;
            _recoveries++;
        }
    }
    public void UpdateActiveCasesOnPOIGainedPlagued(IPointOfInterest p_poi) {
        if (p_poi is Character character && character.isNotSummonAndDemon) {
            _activeCases++;
        }
    }
    public void UpdateDeathsOnCharacterDied(Character p_character) {
        if (p_character.isNotSummonAndDemon) {
            _activeCases--;
            _deaths++;
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
    public bool HasMaxTransmissions() {
        int activeTransmissions = 0;
        foreach (var transmissions in _transmissionLevels) {
            if (transmissions.Value > 0) {
                activeTransmissions++;
            }
        }
        return activeTransmissions >= 3;
    }
    public bool IsTransmissionActive(PLAGUE_TRANSMISSION p_transmissionType) {
        if (_transmissionLevels.ContainsKey(p_transmissionType)) {
            return _transmissionLevels[p_transmissionType] > 0;
        }
        return false;
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
    public bool HasMaxDeathEffect() {
        return _activeDeathEffect != null;
    }
    public bool IsDeathEffectActive(PLAGUE_DEATH_EFFECT p_deathEffect, out PlagueDeathEffect deathEffect) {
        deathEffect = _activeDeathEffect;
        return _activeDeathEffect.deathEffectType == p_deathEffect;
    }
    #endregion

}

[System.Serializable]
public class SaveDataPlagueDisease : SaveData<PlagueDisease> {
    public SaveDataPlagueLifespan lifespan;
    public List<PLAGUE_FATALITY> activeFatalities;
    public List<PLAGUE_SYMPTOM> activeSymptoms;
    public bool hasDeathEffect;
    public PLAGUE_DEATH_EFFECT activeDeathEffect;
    public int activeDeathEffectLevel;
    public int activeCases;
    public int deaths;
    public int recoveries;
    public Dictionary<PLAGUE_TRANSMISSION, int> transmissionLevels;

    #region Overrides
    public override void Save() {
        base.Save();
        PlagueDisease p_data = PlagueDisease.Instance;
        lifespan = new SaveDataPlagueLifespan();
        lifespan.Save();

        if(p_data.activeFatalities.Count > 0) {
            activeFatalities = new List<PLAGUE_FATALITY>();
            for (int i = 0; i < p_data.activeFatalities.Count; i++) {
                activeFatalities.Add(p_data.activeFatalities[i].fatalityType);
            }
        }

        if (p_data.activeSymptoms.Count > 0) {
            activeSymptoms = new List<PLAGUE_SYMPTOM>();
            for (int i = 0; i < p_data.activeSymptoms.Count; i++) {
                activeSymptoms.Add(p_data.activeSymptoms[i].symptomType);
            }
        }

        hasDeathEffect = p_data.activeDeathEffect != null;
        if (hasDeathEffect) {
            activeDeathEffect = p_data.activeDeathEffect.deathEffectType;
            activeDeathEffectLevel = p_data.activeDeathEffect.level;
        }

        activeCases = p_data.activeCases;
        deaths = p_data.deaths;
        recoveries = p_data.recoveries;
        transmissionLevels = p_data.transmissionLevels;
    }
    public override PlagueDisease Load() {
        PlagueDisease plagueDisease = new PlagueDisease(this);
        return plagueDisease;
    }
    #endregion

    #region Clean Up
    public void CleanUp() {
        lifespan = null;
        activeFatalities.Clear();
        activeFatalities = null;
        activeSymptoms.Clear();
        activeSymptoms = null;
        transmissionLevels.Clear();
        transmissionLevels = null;
    }
    #endregion
}