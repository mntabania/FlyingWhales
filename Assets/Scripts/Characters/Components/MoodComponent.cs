using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Traits;
using UnityEngine;
using UnityEngine.Profiling;
using Random = UnityEngine.Random;

public class MoodComponent : CharacterComponent {
	
	public int moodValue { get; private set; }
    public bool isInNormalMood { get; private set; }
    public bool isInLowMood { get; private set; }
    public bool isInCriticalMood { get; private set; }
    public float currentLowMoodEffectChance { get; private set; }
    public float currentCriticalMoodEffectChance { get; private set; }
    public bool executeMoodChangeEffects { get; private set; }
    public bool isInMajorMentalBreak { get; private set; }
    public bool isInMinorMentalBreak { get; private set; }

    public bool hasMoodChanged { get; private set; }
    public string mentalBreakName { get; private set; }

    public Dictionary<string, MoodModification> allMoodModifications { get; private set; }

    #region getters
    public MOOD_STATE moodState {
		get {
			if (isInNormalMood) {
				return MOOD_STATE.Normal;	
			} else if (isInLowMood) {
				return MOOD_STATE.Bad;
			} else if (isInCriticalMood) {
				return MOOD_STATE.Critical;
			} else {
				throw new Exception($"Problem determining {owner.name}'s mood. Because all switches are set to false.");
			}
		}
	}
	public string moodStateName {
		get {
			if (isInNormalMood) {
				return "Normal";	
			} else if (isInLowMood) {
				return "Bad";
			} else if (isInCriticalMood) {
				return "Critical";
			} else {
				return "";
			}
		}
	}
    #endregion

    public MoodComponent() {
		EnableMoodEffects();
		allMoodModifications = new Dictionary<string, MoodModification>();
		isInNormalMood = true; //set as initially in normal mood
	}
    public MoodComponent(SaveDataMoodComponent data) {
        SetSaveDataMoodComponent(data);
    }

    //This is only used for reapplication of mood data from save
    //When mood is loaded we need to reapply the data after the loading of traits so that the data will be consistent from when it was saved
    public void SetSaveDataMoodComponent(SaveDataMoodComponent data) {
        moodValue = data.moodValue;
        isInNormalMood = data.isInNormalMood;
        isInLowMood = data.isInLowMood;
        isInCriticalMood = data.isInCriticalMood;
        currentLowMoodEffectChance = data.currentLowMoodEffectChance;
        currentCriticalMoodEffectChance = data.currentCriticalMoodEffectChance;
        executeMoodChangeEffects = data.executeMoodChangeEffects;
        isInMajorMentalBreak = data.isInMajorMentalBreak;
        isInMinorMentalBreak = data.isInMinorMentalBreak;
        hasMoodChanged = data.hasMoodChanged;
        mentalBreakName = data.mentalBreakName;
        allMoodModifications = data.allMoodModifications;

        if (!string.IsNullOrEmpty(mentalBreakName)) {
	        if (mentalBreakName == "Loss of Control") {
		        Messenger.AddListener<INTERRUPT, Character>(CharacterSignals.INTERRUPT_FINISHED, CheckIfLossOfControlFinished);
	        } else if (mentalBreakName == "Berserked") {
		        Messenger.AddListener<ITraitable, Trait, Character>(TraitSignals.TRAITABLE_LOST_TRAIT, CheckIfBerserkLost);	
	        } else if (mentalBreakName == "Catatonia") {
		        Messenger.AddListener<ITraitable, Trait, Character>(TraitSignals.TRAITABLE_LOST_TRAIT, CheckIfCatatonicLost);
	        } else if (mentalBreakName == "Suicidal") {
		        Messenger.AddListener<ITraitable, Trait, Character>(TraitSignals.TRAITABLE_LOST_TRAIT, CheckIfSuicidalLost);
	        }
        }
    }

    #region Events
    public void OnCharacterBecomeMinionOrSummon() {
		DisableMoodEffects();
		//StopCheckingForMinorMentalBreak();
		StopCheckingForMajorMentalBreak();
	}
	public void OnCharacterNoLongerMinionOrSummon() {
		EnableMoodEffects();
	}
	#endregion

	private void EnableMoodEffects() {
		executeMoodChangeEffects = true;
	}
	private void DisableMoodEffects() {
		executeMoodChangeEffects = false;
	}
	public void SetMoodValue(int amount) {
		moodValue = amount;
        hasMoodChanged = true;
	}
	public void AddMoodEffect(int amount, IMoodModifier modifier, GameDate expiryDate, Character characterResponsible) {
#if DEBUG_LOG
		if (amount == 0) {
			Debug.Log($"Added mood effect with 0 amount! {modifier.modifierName}");
		}
#endif
		//NOTE: Allowed addition of zero values because of the expiry dates.
		//statuses, even if they have no mood effects, can extend the expiry of that status, and so the mood summary needs to be updated
		//normal traits on the other hand usually don't have mood effects so moved checking of 0 values there.
		moodValue += amount;
		AddModificationToSummary(modifier.modifierName, amount, expiryDate, modifier.GetMoodEffectFlavorText(characterResponsible));
        hasMoodChanged = true;
        //OnMoodChanged();
	}
	public void RescheduleMoodEffect(IMoodModifier p_modifier, GameDate p_rescheduledDate) {
		if (allMoodModifications.ContainsKey(p_modifier.modifierName)) {
			if (allMoodModifications[p_modifier.modifierName].expiryDates.Count > 0) {
				allMoodModifications[p_modifier.modifierName].expiryDates.RemoveAt(0);	
			}
			allMoodModifications[p_modifier.modifierName].expiryDates.Add(p_rescheduledDate);
		}
	}
	public void RemoveMoodEffect(int amount, IMoodModifier modifier) {
		// if (amount == 0) {
		// 	return; //ignore
		// }
		moodValue += amount;
		RemoveModificationFromSummary(modifier.modifierName, amount);
        hasMoodChanged = true;
        //OnMoodChanged();
	}
	public void OnTickEnded() {
        if (hasMoodChanged) {
            hasMoodChanged = false;
            OnMoodChanged();
        }
    }

#region Loading
    public void LoadReferences(SaveDataMoodComponent data) {
        //Currently N/A
    }
#endregion

#region Events
    private void OnMoodChanged() {
		if (moodValue >= EditableValuesManager.Instance.normalMoodMinThreshold) {
			if (isInNormalMood == false) {
				EnterNormalMood();	
			}
		} else if (moodValue >= EditableValuesManager.Instance.lowMoodMinThreshold 
		           && moodValue <= EditableValuesManager.Instance.lowMoodHighThreshold) {
			if (isInLowMood == false) {
				EnterLowMood();	
			}
		} else if (moodValue <= EditableValuesManager.Instance.criticalMoodHighThreshold) {
			if (isInCriticalMood == false) {
				EnterCriticalMood();	
			}
		}
	}
#endregion

#region Normal Mood
	private void EnterNormalMood() {
		SwitchMoodStates(MOOD_STATE.Normal);
#if DEBUG_LOG
		Debug.Log(
			$"{GameManager.Instance.TodayLogString()} {owner.name} is <color=green>entering</color> <b>normal</b> mood state");
#endif
	}
	private void ExitNormalMood() {
		isInNormalMood = false;
#if DEBUG_LOG
		Debug.Log(
			$"{GameManager.Instance.TodayLogString()} {owner.name} is <color=red>exiting</color> <b>normal</b> mood state");
#endif
	}
#endregion
	
#region Low Mood
	private void EnterLowMood() {
		SwitchMoodStates(MOOD_STATE.Bad);
#if DEBUG_LOG
		Debug.Log(
			$"{GameManager.Instance.TodayLogString()} {owner.name} is <color=green>entering</color> <b>Low</b> mood state");
#endif
		
	}
	private void ExitLowMood() {
		isInLowMood = false;
#if DEBUG_LOG
		Debug.Log(
			$"{GameManager.Instance.TodayLogString()} {owner.name} is <color=red>exiting</color> <b>low</b> mood state");
#endif
	}
#endregion

#region Critical Mood
	private void EnterCriticalMood() {
		SwitchMoodStates(MOOD_STATE.Critical);
#if DEBUG_LOG
		Debug.Log(
			$"{GameManager.Instance.TodayLogString()} {owner.name} is <color=green>entering</color> <b>critical</b> mood state");
#endif
		if (executeMoodChangeEffects) {
			//start checking for major mental breaks
			if (isInMajorMentalBreak) {
#if DEBUG_LOG
				Debug.Log(
					$"{GameManager.Instance.TodayLogString()}{owner.name} is currently in a major mental break. So not starting hourly check for major mental break.");
#endif
			} else {
				StartCheckingForMajorMentalBreak();
			}
			if (currentCriticalMoodEffectChance > 0f) {
				Messenger.RemoveListener(Signals.HOUR_STARTED, DecreaseMajorMentalBreakChance);
			}
		}
		
	}
	private void ExitCriticalMood() {
		isInCriticalMood = false;
#if DEBUG_LOG
		Debug.Log(
			$"{GameManager.Instance.TodayLogString()} {owner.name} is <color=red>exiting</color> <b>critical</b> mood state");
#endif
		if (executeMoodChangeEffects) {
			//stop checking for major mental breaks
			StopCheckingForMajorMentalBreak();
			if (currentCriticalMoodEffectChance > 0f) {
				Messenger.AddListener(Signals.HOUR_STARTED, DecreaseMajorMentalBreakChance);
			}
		}
	}
	private void CheckForMajorMentalBreak() {
#if DEBUG_PROFILER
		Profiler.BeginSample($"{owner.name} Check For Major Mental Break");
#endif
		IncreaseMajorMentalBreakChance();
		if (owner.limiterComponent.canPerform && isInMinorMentalBreak == false && isInMajorMentalBreak == false) {
			float roll = Random.Range(0f, 100f);
#if DEBUG_LOG
			Debug.Log($"<color=green>{GameManager.Instance.TodayLogString()}{owner.name} is checking for <b>MAJOR</b> mental break. Roll is <b>{roll.ToString(CultureInfo.InvariantCulture)}</b>. Chance is <b>{currentCriticalMoodEffectChance.ToString(CultureInfo.InvariantCulture)}</b></color>");
#endif
			if (roll <= currentCriticalMoodEffectChance) {
				//Trigger Major Mental Break.
				TriggerMajorMentalBreak();
			}
		}
#if DEBUG_PROFILER
		Profiler.EndSample();
#endif
	}
	private void AdjustMajorMentalBreakChance(float amount) {
		currentCriticalMoodEffectChance = currentCriticalMoodEffectChance + amount;
		currentCriticalMoodEffectChance = Mathf.Clamp(currentCriticalMoodEffectChance, 0, 100f);
		if (currentCriticalMoodEffectChance <= 0f) {
			Messenger.RemoveListener(Signals.HOUR_STARTED, DecreaseMajorMentalBreakChance);
		}
	}
	private void SetMajorMentalBreakChance(float amount) {
		currentCriticalMoodEffectChance = amount;
		currentCriticalMoodEffectChance = Mathf.Clamp(currentCriticalMoodEffectChance, 0, 100f);
		if (currentCriticalMoodEffectChance <= 0f) {
			Messenger.RemoveListener(Signals.HOUR_STARTED, DecreaseMajorMentalBreakChance);
		}
	}
	private void IncreaseMajorMentalBreakChance() {
		AdjustMajorMentalBreakChance(GetMajorMentalBreakChanceIncrease());
	}
	private void DecreaseMajorMentalBreakChance() {
		 AdjustMajorMentalBreakChance(GetMajorMentalBreakChanceDecrease());
	}
	private float GetMajorMentalBreakChanceIncrease() {
		return 100f / (EditableValuesManager.Instance.majorMentalBreakHourThreshold * GameManager.ticksPerHour);
	}
	private float GetMajorMentalBreakChanceDecrease() {
		return (100f / (EditableValuesManager.Instance.majorMentalBreakHourThreshold * 24f)) * -1f; //because there are 24 hours in a day
	}
	private void ResetMajorMentalBreakChance() {
		// Debug.Log($"<color=blue>{GameManager.Instance.TodayLogString()}{owner.name} reset major mental break chance.</color>");
		SetMajorMentalBreakChance(0f);
	}
	private void StopCheckingForMajorMentalBreak() {
		// Debug.Log($"<color=red>{GameManager.Instance.TodayLogString()}{owner.name} has stopped checking for major mental break.</color>");
		Messenger.RemoveListener(Signals.TICK_STARTED, CheckForMajorMentalBreak);
	}
	private void StartCheckingForMajorMentalBreak() {
		// Debug.Log($"<color=blue>{GameManager.Instance.TodayLogString()}{owner.name} has started checking for major mental break.</color>");
		Messenger.AddListener(Signals.TICK_STARTED, CheckForMajorMentalBreak);
	}
#endregion

#region Major Mental Break
	private void TriggerMajorMentalBreak() {
		if (isInMajorMentalBreak) {
			throw new Exception($"{GameManager.Instance.TodayLogString()}{owner.name} is already in a major mental break, but is trying to trigger another one!");
		}

#if DEBUG_LOG
		string summary = $"{GameManager.Instance.TodayLogString()}{owner.name} triggered major mental break.";
#endif
		isInMajorMentalBreak = true;
		if (owner.characterClass.className.Equals("Farmer") || owner.characterClass.className.Equals("Miner") 
		    || owner.characterClass.className.Equals("Crafter")) {
			//owner.characterClass.className.Equals("Farmer") || owner.characterClass.className.Equals("Miner") 
			//|| owner.characterClass.className.Equals("Crafter")
			int roll = Random.Range(0, 2);
			if (roll == 0) {
				//catatonic
#if DEBUG_LOG
				summary += "Chosen break is <b>catatonic</b>";
#endif
				TriggerCatatonic();
				mentalBreakName = "Catatonia";
				owner.interruptComponent.TriggerInterrupt(INTERRUPT.Mental_Break, owner);
			} else if (roll == 1) {
				//suicidal
#if DEBUG_LOG
				summary += "Chosen break is <b>suicidal</b>";
#endif
				TriggerSuicidal();
				mentalBreakName = "Suicidal";
				owner.interruptComponent.TriggerInterrupt(INTERRUPT.Mental_Break, owner);
			}
		} else if (owner.characterClass.className.Equals("Druid") || owner.characterClass.className.Equals("Shaman") 
					|| owner.characterClass.className.Equals("Mage")) {
			mentalBreakName = "Loss of Control";
			// owner.interruptComponent.TriggerInterrupt(INTERRUPT.Mental_Break, owner);
			TriggerLossOfControl();
		} else {
#if DEBUG_LOG
			summary += "Chosen break is <b>Berserked</b>";
#endif
			TriggerBerserk();
			mentalBreakName = "Berserked";
			owner.interruptComponent.TriggerInterrupt(INTERRUPT.Mental_Break, owner);
		}

#if DEBUG_LOG
		Debug.Log($"<color=red>{summary}</color>");
#endif
		StopCheckingForMajorMentalBreak();
	}
	private void TriggerBerserk() {
		if (owner.traitContainer.AddTrait(owner, "Berserked")) {
			Messenger.AddListener<ITraitable, Trait, Character>(TraitSignals.TRAITABLE_LOST_TRAIT, CheckIfBerserkLost);	
		} else {
#if DEBUG_LOG
			Debug.LogWarning($"{owner.name} triggered berserk mental break but could not add berserk trait to its traits!");
#endif
		}
	}
	private void CheckIfBerserkLost(ITraitable traitable, Trait trait, Character removedBy) {
		if (traitable == owner && trait is Berserked) {
			//gain catharsis
			owner.traitContainer.AddTrait(owner, "Catharsis");
			Messenger.RemoveListener<ITraitable, Trait, Character>(TraitSignals.TRAITABLE_LOST_TRAIT, CheckIfBerserkLost);
			OnMentalBreakDone();
		}
	}
	private void TriggerCatatonic() {
		if (owner.traitContainer.AddTrait(owner, "Catatonic")) {
			Messenger.AddListener<ITraitable, Trait, Character>(TraitSignals.TRAITABLE_LOST_TRAIT, CheckIfCatatonicLost);
		} else {
#if DEBUG_LOG
			Debug.LogWarning($"{owner.name} triggered catatonic mental break but could not add catatonic trait to its traits!");
#endif
		}
	}
	private void CheckIfCatatonicLost(ITraitable traitable, Trait trait, Character removedBy) {
		if (traitable == owner && trait is Catatonic) {
			//gain catharsis
			owner.traitContainer.AddTrait(owner, "Catharsis");
			Messenger.RemoveListener<ITraitable, Trait, Character>(TraitSignals.TRAITABLE_LOST_TRAIT, CheckIfCatatonicLost);	
			OnMentalBreakDone();
		}
	}
	private void TriggerSuicidal() {
		if (owner.traitContainer.AddTrait(owner, "Suicidal")) {
			Messenger.AddListener<ITraitable, Trait, Character>(TraitSignals.TRAITABLE_LOST_TRAIT, CheckIfSuicidalLost);
		} else {
#if DEBUG_LOG
			Debug.LogWarning($"{owner.name} triggered suicidal mental break but could not add suicidal trait to its traits!");
#endif
		}
	}
	private void CheckIfSuicidalLost(ITraitable traitable, Trait trait, Character removedBy) {
		if (traitable == owner && trait is Suicidal) {
			//gain catharsis
			owner.traitContainer.AddTrait(owner, "Catharsis");
			Messenger.RemoveListener<ITraitable, Trait, Character>(TraitSignals.TRAITABLE_LOST_TRAIT, CheckIfSuicidalLost);
			OnMentalBreakDone();
		}
	}
	private void TriggerLossOfControl() {
		owner.interruptComponent.TriggerInterrupt(INTERRUPT.Loss_Of_Control, owner);
		Messenger.AddListener<INTERRUPT, Character>(CharacterSignals.INTERRUPT_FINISHED, CheckIfLossOfControlFinished);
	}
	private void CheckIfLossOfControlFinished(INTERRUPT interrupt, Character character) {
		if (interrupt == INTERRUPT.Loss_Of_Control && character == owner) {
			Messenger.RemoveListener<INTERRUPT, Character>(CharacterSignals.INTERRUPT_FINISHED, CheckIfLossOfControlFinished);	
			OnMentalBreakDone();
		}
	}
#endregion

#region Minor Mental Break
	private void AdjustMinorMentalBreakChance(float amount) {
		currentLowMoodEffectChance = currentLowMoodEffectChance + amount;
		currentLowMoodEffectChance = Mathf.Clamp(currentLowMoodEffectChance, 0, 100f);
		if (currentLowMoodEffectChance <= 0f) {
			Messenger.RemoveListener(Signals.HOUR_STARTED, DecreaseMinorMentalBreakChance);
		}
	}
	private void DecreaseMinorMentalBreakChance() {
		AdjustMinorMentalBreakChance(GetMinorMentalBreakChanceDecrease());
	}
	private float GetMinorMentalBreakChanceDecrease() {
		return (100f / (EditableValuesManager.Instance.minorMentalBreakDayThreshold * 24f)) * -1f; //because there are 24 hours in a day
	}
#endregion

#region Mental Break Shared
	private void OnMentalBreakDone() {
		isInMajorMentalBreak = false;
		ResetMajorMentalBreakChance();
		mentalBreakName = string.Empty;
		if (isInCriticalMood) {
#if DEBUG_LOG
			Debug.Log($"{GameManager.Instance.TodayLogString()}{owner.name} is still in critical mood state after mental break, starting check for major mental break again...");
#endif
			StartCheckingForMajorMentalBreak();
		}
	}
#endregion
	
#region Utilities
	private void SwitchMoodStates(MOOD_STATE moodToEnter) {
		MOOD_STATE lastMoodState = moodState;
		switch (moodToEnter) {
			case MOOD_STATE.Bad:
				isInLowMood = true;
				break;
			case MOOD_STATE.Normal:
				isInNormalMood = true;
				break;
			case MOOD_STATE.Critical:
				isInCriticalMood = true;
				break;
		}
		switch (lastMoodState) {
			case MOOD_STATE.Bad:
				ExitLowMood();
				break;
			case MOOD_STATE.Normal:
				ExitNormalMood();
				break;
			case MOOD_STATE.Critical:
				ExitCriticalMood();
				break;
		}
	}
#endregion

#region Summary
	private void AddModificationToSummary(string p_modifierName, int p_modificationValue, GameDate p_expiryDate, Log p_modificationFlavorText) {
		if (!allMoodModifications.ContainsKey(p_modifierName)) {
			allMoodModifications.Add(p_modifierName, new MoodModification(p_modificationValue, p_expiryDate, p_modificationFlavorText));
		} else {
			allMoodModifications[p_modifierName].AddModification(p_modificationValue, p_expiryDate, p_modificationFlavorText);
		}
#if DEBUG_LOG
		Debug.Log($"<color=blue>{owner.name} Added mood modification {p_modifierName} {p_modificationValue.ToString()}</color>");
#endif
		Messenger.Broadcast(CharacterSignals.MOOD_SUMMARY_MODIFIED, this);
	}
	private void RemoveModificationFromSummary(string modificationKey, int modificationValue) {
		if (allMoodModifications.ContainsKey(modificationKey)) {
#if DEBUG_LOG
			Debug.Log($"<color=red>{owner.name} Removed mood modification {modificationKey} {modificationValue.ToString()}</color>");
#endif
			allMoodModifications[modificationKey].RemoveLatestModification();
			if (allMoodModifications[modificationKey].IsEmpty()) {
				allMoodModifications.Remove(modificationKey);
			}
			Messenger.Broadcast(CharacterSignals.MOOD_SUMMARY_MODIFIED, this);
		}
	}
	public void UpdateMoodSummaryLogsOnCharacterChangedName(Character p_character) {
		foreach (var moodModification in allMoodModifications) {
			for (int i = 0; i < moodModification.Value.flavorTexts.Count; i++) {
				var flavorText = moodModification.Value.flavorTexts[i];
				if (flavorText != null) {
					flavorText.TryUpdateLogAfterRename(p_character);	
				}
			}
		}
	}
#endregion
}

[System.Serializable]
public class SaveDataMoodComponent : SaveData<MoodComponent> {
    public int moodValue;
    public bool isInNormalMood;
    public bool isInLowMood;
    public bool isInCriticalMood;
    public float currentLowMoodEffectChance;
    public float currentCriticalMoodEffectChance;
    public bool executeMoodChangeEffects;
    public bool isInMajorMentalBreak;
    public bool isInMinorMentalBreak;

    public bool hasMoodChanged;
    public string mentalBreakName;

    public Dictionary<string, int> moodModificationsSummary;
    public Dictionary<string, MoodModification> allMoodModifications;

#region Overrides
    public override void Save(MoodComponent data) {
        moodValue = data.moodValue;
        isInNormalMood = data.isInNormalMood;
        isInLowMood = data.isInLowMood;
        isInCriticalMood = data.isInCriticalMood;
        currentLowMoodEffectChance = data.currentLowMoodEffectChance;
        currentCriticalMoodEffectChance = data.currentCriticalMoodEffectChance;
        executeMoodChangeEffects = data.executeMoodChangeEffects;
        isInMajorMentalBreak = data.isInMajorMentalBreak;
        isInMinorMentalBreak = data.isInMinorMentalBreak;
        hasMoodChanged = data.hasMoodChanged;
        mentalBreakName = data.mentalBreakName;
        // moodModificationsSummary = data.moodModificationsSummary;
        allMoodModifications = new Dictionary<string, MoodModification>();
        foreach (var kvp in data.allMoodModifications) {
	        allMoodModifications.Add(kvp.Key, kvp.Value);
        }
    }

    public override MoodComponent Load() {
        MoodComponent component = new MoodComponent(this);
        return component;
    }
#endregion
}

[System.Serializable]
public struct MoodModification {
	public List<int> modifications;
	public List<GameDate> expiryDates;
	public List<Log> flavorTexts;
	public MoodModification(int modification, GameDate expiryDate, Log flavorText) {
		modifications = new List<int>();
		expiryDates = new List<GameDate>();
		flavorTexts = new List<Log>();
		AddModification(modification, expiryDate, flavorText);
	}

	public void AddModification(int modification, GameDate expiryDate, Log flavorText) {
		modifications.Add(modification);
		expiryDates.Add(expiryDate);
		flavorTexts.Add(flavorText);
	}
	public void RemoveLatestModification() {
		if (modifications.Count > 0) {
			modifications.RemoveAt(modifications.Count - 1); //remove latest modification. Because stacking statuses are first in last out.
			flavorTexts.RemoveAt(flavorTexts.Count - 1); //remove latest flavor text. Because stacking statuses are first in last out.
			expiryDates.RemoveAt(0); //remove oldest date.
		}
	}

	public bool IsEmpty() {
		return expiryDates.Count == 0 && modifications.Count == 0 && flavorTexts.Count == 0;
	}
}