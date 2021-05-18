using System;
using System.Collections.Generic;
using Plague.Fatality;
using Plague.Symptom;
using Plague.Death_Effect;
using UnityEngine;
using UnityEngine.Assertions;
using System.Linq;
using UtilityScripts;

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
        if (p_data.activeFatalities != null && p_data.activeFatalities.Count > 0) {
            for (int i = 0; i < p_data.activeFatalities.Count; i++) {
                Fatality fatality = CreateNewFatalityInstance(p_data.activeFatalities[i]);
                _activeFatalities.Add(fatality);
            }
        }

        _activeSymptoms = new List<PlagueSymptom>();
        if (p_data.activeSymptoms != null &&  p_data.activeSymptoms.Count > 0) {
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
        _deaths = 0;
        _activeCases = 0;
        _recoveries = 0;
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
    public void UpdateRecoveriesOnPOILostPlagued(IPointOfInterest p_poi) {
        if (p_poi is Character character && !character.traitContainer.HasTrait("Plague Reservoir")) {
            AdjustRecoveries(1);
        }
    }
    public void UpdateActiveCasesOnPOILostPlagued(IPointOfInterest p_poi) {
        if (p_poi is Character character && !character.traitContainer.HasTrait("Plague Reservoir")) {
            AdjustActiveCases(-1);
        }
    }
    public void UpdateActiveCasesOnPOIGainedPlagued(IPointOfInterest p_poi) {
        if (p_poi is Character character && !character.traitContainer.HasTrait("Plague Reservoir")) {
            AdjustActiveCases(1);
        }
    }
    public void UpdateDeathsOnCharacterDied(Character p_character) {
        if (!p_character.traitContainer.HasTrait("Plague Reservoir")) {
            AdjustDeaths(1);
        }
    }
    public void UpdateActiveCasesOnCharacterDied(Character p_character) {
        if (!p_character.traitContainer.HasTrait("Plague Reservoir")) {
            AdjustActiveCases(-1);
        }
    }
    private void AdjustDeaths(int p_adjustment) {
        _deaths += p_adjustment;
        _deaths = Mathf.Max(0, _deaths);
    }
    private void AdjustRecoveries(int p_adjustment) {
        _recoveries += p_adjustment;
        _recoveries = Mathf.Max(0, _recoveries);
    }
    private void AdjustActiveCases(int p_adjustment) {
        _activeCases += p_adjustment;
        _activeCases = Mathf.Max(0, _activeCases);
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
#if DEBUG_LOG
            Debug.Log($"Upgraded {p_transmissionType.ToString()} to level {_transmissionLevels[p_transmissionType].ToString()}");
#endif
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
        return _activeDeathEffect != null && _activeDeathEffect.deathEffectType == p_deathEffect;
    }
#endregion

#region Plagued Status
    public bool AddPlaguedStatusOnPOIWithLifespanDuration(IPointOfInterest poi) {
        //Made this a function because we still need an extra process to know that the character will be affected by plague which is the lifespan
        //If the lifespan is -1, then the character will not be affected by plague
        int lifespanInTicks = lifespan.GetLifespanInTicksOfPlagueOn(poi);
        if(lifespanInTicks != -1) {
            return poi.traitContainer.AddTrait(poi, "Plagued", overrideDuration: lifespanInTicks);
        }
        return false;
    }
#endregion

#region Randomization
    public void OnLoadoutPicked() {
        // if (!PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.BIOLAB).isInUse) {
        //     RandomizePlague();
        // }
    }
    private void RandomizePlague() {
#if DEBUG_LOG
        string randomizeSummary = $"Randomizing Plague Effects:";
#endif
        List<PLAGUE_TRANSMISSION> transmissionChoices = CollectionUtilities.GetEnumValues<PLAGUE_TRANSMISSION>().ToList();
        for (int i = 0; i < 2; i++) {
            if (transmissionChoices.Count == 0) { break; }
            PLAGUE_TRANSMISSION transmissionTypeToUpgrade = CollectionUtilities.GetRandomElement(transmissionChoices);
            int level = i == 0 ? 1 : 2;
            _transmissionLevels[transmissionTypeToUpgrade] = level;
            transmissionChoices.Remove(transmissionTypeToUpgrade);
#if DEBUG_LOG
            randomizeSummary = $"{randomizeSummary}\nUpgraded {transmissionTypeToUpgrade.ToString()} to Level {level}";
#endif
        }
        List<string> lifespanChoices = new List<string>() { "Tile Object", "Monster", "Undead", "Human", "Elf" };
        for (int i = 0; i < 2; i++) {
            if (lifespanChoices.Count == 0) { break; }
            string chosenLifespanToUpgrade = CollectionUtilities.GetRandomElement(lifespanChoices);
            switch (chosenLifespanToUpgrade) {
                case "Tile Object": _lifespan.UpgradeTileObjectInfectionTime(); break;
                case "Monster": _lifespan.UpgradeMonsterInfectionTime(); break;
                case "Undead": _lifespan.UpgradeUndeadInfectionTime(); break;
                case "Human": _lifespan.UpgradeSapientInfectionTime(RACE.HUMANS); break;
                case "Elf": _lifespan.UpgradeSapientInfectionTime(RACE.ELVES); break;
            }
            lifespanChoices.Remove(chosenLifespanToUpgrade);
#if DEBUG_LOG
            randomizeSummary = $"{randomizeSummary}\nUpgraded {chosenLifespanToUpgrade} lifespan by 1 level.";
#endif
        }
        PLAGUE_FATALITY[] fatalityChoices = CollectionUtilities.GetEnumValues<PLAGUE_FATALITY>();
        PLAGUE_FATALITY chosenFatality = CollectionUtilities.GetRandomElement(fatalityChoices);
        AddAndInitializeFatality(chosenFatality);
#if DEBUG_LOG
        randomizeSummary = $"{randomizeSummary}\nUnlocked {chosenFatality.ToString()} Fatality";
#endif

        List<PLAGUE_SYMPTOM> symptomChoices = CollectionUtilities.GetEnumValues<PLAGUE_SYMPTOM>().ToList();
        for (int i = 0; i < 1; i++) {
            if (symptomChoices.Count == 0) { break; }
            PLAGUE_SYMPTOM symptomToUpgrade = CollectionUtilities.GetRandomElement(symptomChoices);
            AddAndInitializeSymptom(symptomToUpgrade);
            symptomChoices.Remove(symptomToUpgrade);
#if DEBUG_LOG
            randomizeSummary = $"{randomizeSummary}\nUnlocked {symptomToUpgrade.ToString()} Symptom";
#endif
        }

        PLAGUE_DEATH_EFFECT[] deathEffectChoices = CollectionUtilities.GetEnumValues<PLAGUE_DEATH_EFFECT>();
        PLAGUE_DEATH_EFFECT deathEffect = CollectionUtilities.GetRandomElement(deathEffectChoices);
        SetNewPlagueDeathEffectAndUnsetPrev(deathEffect);
        _activeDeathEffect.AdjustLevel(1);
#if DEBUG_LOG
        randomizeSummary = $"{randomizeSummary}\nUnlocked and Upgraded {deathEffect.ToString()} to Level 2";
        Debug.Log(randomizeSummary);
#endif
    }
#endregion

#region Utilities
    public string GetPlagueEffectsSummary() {
        string tooltip = $"<b>Effects:</b>";
        int airborneLevel = GetTransmissionLevel(PLAGUE_TRANSMISSION.Airborne);
        int combatLevel = GetTransmissionLevel(PLAGUE_TRANSMISSION.Combat);
        int consumptionLevel = GetTransmissionLevel(PLAGUE_TRANSMISSION.Consumption);
        int physicalContactLevel = GetTransmissionLevel(PLAGUE_TRANSMISSION.Physical_Contact);
        if (airborneLevel > 0) { tooltip = $"{tooltip}\nAirborne Rate: {GetTransmissionRateDescription(airborneLevel)}"; }
        if (combatLevel > 0) { tooltip = $"{tooltip}\nCombat Rate: {GetTransmissionRateDescription(combatLevel)}"; }
        if (consumptionLevel > 0) { tooltip = $"{tooltip}\nConsumption Rate: {GetTransmissionRateDescription(consumptionLevel)}"; }
        if (physicalContactLevel > 0) { tooltip = $"{tooltip}\nPhysical Contact Rate: {GetTransmissionRateDescription(physicalContactLevel)}"; }
        tooltip = $"{tooltip}\nObject Lifespan: {lifespan.GetInfectionTimeString(lifespan.tileObjectInfectionTimeInHours)}";
        tooltip = $"{tooltip}\nHuman Lifespan: {lifespan.GetInfectionTimeString(lifespan.GetSapientLifespanOfPlagueInHours(RACE.HUMANS))}";
        tooltip = $"{tooltip}\nElves Lifespan: {lifespan.GetInfectionTimeString(lifespan.GetSapientLifespanOfPlagueInHours(RACE.ELVES))}";
        tooltip = $"{tooltip}\nMonster Lifespan: {lifespan.GetInfectionTimeString(lifespan.monsterInfectionTimeInHours)}";
        tooltip = $"{tooltip}\nUndead Lifespan: {lifespan.GetInfectionTimeString(lifespan.undeadInfectionTimeInHours)}";
        tooltip = $"{tooltip}\nFatalities: ";
        if (activeFatalities.Count > 0) {
            for (int i = 0; i < activeFatalities.Count; i++) {
                Fatality fatality = activeFatalities[i];
                tooltip = $"{tooltip}{UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(fatality.fatalityType.ToString())}";
                if (i + 1 < activeFatalities.Count) {
                    tooltip = $"{tooltip}, ";
                }
            }    
        } else {
            tooltip = $"{tooltip}-";
        }
        
        tooltip = $"{tooltip}\nSymptoms: ";
        if (activeSymptoms.Count > 0) {
            for (int i = 0; i < activeSymptoms.Count; i++) {
                PlagueSymptom symptom = activeSymptoms[i];
                tooltip = $"{tooltip}{UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(symptom.symptomType.ToString())}";
                if (i + 1 < activeSymptoms.Count) {
                    tooltip = $"{tooltip}, ";
                }
            }    
        } else {
            tooltip = $"{tooltip}-";
        }
        tooltip = $"{tooltip}\nOn Death: {activeDeathEffect?.GetCurrentEffectDescription() ?? "-"}";
        return tooltip;
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
        lifespan.Save(p_data.lifespan);

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