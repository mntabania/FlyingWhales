using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Traits;
using UnityEngine;
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

    public Dictionary<string, int> moodModificationsSummary { get; private set; }
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
		moodModificationsSummary = new Dictionary<string, int>();
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
        moodModificationsSummary = data.moodModificationsSummary;
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
        //OnMoodChanged();
	}
	public void AddMoodEffect(int amount, IMoodModifier modifier, GameDate expiryDate) {
		// if (amount == 0) {
		// 	return; //ignore
		// }
		//NOTE: Allowed addition of zero values because of the expiry dates.
		//statuses, even if they have no mood effects, can extend the expiry of that status, and so the mood summary needs to be updated
		//normal traits on the other hand usually don't have mood effects so moved checking of 0 values there.
		moodValue += amount;
		AddModificationToSummary(modifier.moodModificationDescription, amount, expiryDate);
        hasMoodChanged = true;
        //OnMoodChanged();
	}
	public void RemoveMoodEffect(int amount, IMoodModifier modifier) {
		// if (amount == 0) {
		// 	return; //ignore
		// }
		moodValue += amount;
		RemoveModificationFromSummary(modifier.moodModificationDescription, amount);
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
		Debug.Log(
			$"{GameManager.Instance.TodayLogString()} {owner.name} is <color=green>entering</color> <b>normal</b> mood state");
	}
	private void ExitNormalMood() {
		isInNormalMood = false;
		Debug.Log(
			$"{GameManager.Instance.TodayLogString()} {owner.name} is <color=red>exiting</color> <b>normal</b> mood state");
	}
	#endregion
	
	#region Low Mood
	private void EnterLowMood() {
		SwitchMoodStates(MOOD_STATE.Bad);
		Debug.Log(
			$"{GameManager.Instance.TodayLogString()} {owner.name} is <color=green>entering</color> <b>Low</b> mood state");
		
	}
	private void ExitLowMood() {
		isInLowMood = false;
		Debug.Log(
			$"{GameManager.Instance.TodayLogString()} {owner.name} is <color=red>exiting</color> <b>low</b> mood state");
	}
	#endregion

	#region Critical Mood
	private void EnterCriticalMood() {
		SwitchMoodStates(MOOD_STATE.Critical);
		Debug.Log(
			$"{GameManager.Instance.TodayLogString()} {owner.name} is <color=green>entering</color> <b>critical</b> mood state");
		if (executeMoodChangeEffects) {
			//start checking for major mental breaks
			if (isInMajorMentalBreak) {
				Debug.Log(
					$"{GameManager.Instance.TodayLogString()}{owner.name} is currently in a major mental break. So not starting hourly check for major mental break.");
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
		Debug.Log(
			$"{GameManager.Instance.TodayLogString()} {owner.name} is <color=red>exiting</color> <b>critical</b> mood state");
		if (executeMoodChangeEffects) {
			//stop checking for major mental breaks
			StopCheckingForMajorMentalBreak();
			if (currentCriticalMoodEffectChance > 0f) {
				Messenger.AddListener(Signals.HOUR_STARTED, DecreaseMajorMentalBreakChance);
			}
		}
	}
	private void CheckForMajorMentalBreak() {
		IncreaseMajorMentalBreakChance();
		if (owner.limiterComponent.canPerform && isInMinorMentalBreak == false && isInMajorMentalBreak == false) {
			float roll = Random.Range(0f, 100f);
			Debug.Log(
				$"<color=green>{GameManager.Instance.TodayLogString()}{owner.name} is checking for <b>MAJOR</b> mental break. Roll is <b>{roll.ToString(CultureInfo.InvariantCulture)}</b>. Chance is <b>{currentCriticalMoodEffectChance.ToString(CultureInfo.InvariantCulture)}</b></color>");
			if (roll <= currentCriticalMoodEffectChance) {
				//Trigger Major Mental Break.
				TriggerMajorMentalBreak();
			}	
		}
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
		
		string summary = $"{GameManager.Instance.TodayLogString()}{owner.name} triggered major mental break.";
		isInMajorMentalBreak = true;
		if (owner.characterClass.className.Equals("Peasant") || owner.characterClass.className.Equals("Miner") 
		    || owner.characterClass.className.Equals("Craftsman")) {
			int roll = Random.Range(0, 2);
			if (roll == 0) {
				//catatonic
				summary += "Chosen break is <b>catatonic</b>";
				TriggerCatatonic();
				owner.interruptComponent.TriggerInterrupt(INTERRUPT.Mental_Break, owner);
			} else if (roll == 1) {
				//suicidal
				summary += "Chosen break is <b>suicidal</b>";
				TriggerSuicidal();
				owner.interruptComponent.TriggerInterrupt(INTERRUPT.Mental_Break, owner);
			}
		} else if (owner.characterClass.className.Equals("Druid") || owner.characterClass.className.Equals("Shaman") 
					|| owner.characterClass.className.Equals("Mage")) {
			mentalBreakName = "Loss of Control";
			// owner.interruptComponent.TriggerInterrupt(INTERRUPT.Mental_Break, owner);
			TriggerLossOfControl();
		} else {
			summary += "Chosen break is <b>Berserked</b>";
			TriggerBerserk();
			owner.interruptComponent.TriggerInterrupt(INTERRUPT.Mental_Break, owner);
		}
		
		Debug.Log($"<color=red>{summary}</color>");
		StopCheckingForMajorMentalBreak();
	}
	private void TriggerBerserk() {
		if (owner.traitContainer.AddTrait(owner, "Berserked")) {
			mentalBreakName = "Berserked";
			Messenger.AddListener<ITraitable, Trait, Character>(TraitSignals.TRAITABLE_LOST_TRAIT, CheckIfBerserkLost);	
		} else {
			Debug.LogWarning($"{owner.name} triggered berserk mental break but could not add berserk trait to its traits!");
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
			mentalBreakName = "Catatonia";
			Messenger.AddListener<ITraitable, Trait, Character>(TraitSignals.TRAITABLE_LOST_TRAIT, CheckIfCatatonicLost);
		} else {
			Debug.LogWarning($"{owner.name} triggered catatonic mental break but could not add catatonic trait to its traits!");
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
			mentalBreakName = "Suicidal";
			Messenger.AddListener<ITraitable, Trait, Character>(TraitSignals.TRAITABLE_LOST_TRAIT, CheckIfSuicidalLost);
		} else {
			Debug.LogWarning($"{owner.name} triggered suicidal mental break but could not add suicidal trait to its traits!");
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
	// private void CheckForMinorMentalBreak() {
	// 	IncreaseMinorMentalBreakChance();
	// 	if (owner.limiterComponent.canPerform && _isInMinorMentalBreak == false && _isInMajorMentalBreak == false) {
	// 		float roll = Random.Range(0f, 100f);
	// 		Debug.Log(
	// 			$"<color=green>{GameManager.Instance.TodayLogString()}{owner.name} is checking for <b>MINOR</b> mental break. Roll is <b>{roll.ToString(CultureInfo.InvariantCulture)}</b>. Chance is <b>{currentLowMoodEffectChance.ToString(CultureInfo.InvariantCulture)}</b></color>");
	// 		if (roll <= currentLowMoodEffectChance) {
	// 			//Trigger Minor Mental Break.
	// 			TriggerMinorMentalBreak();
	// 		}	
	// 	}
	// }
	private void AdjustMinorMentalBreakChance(float amount) {
		currentLowMoodEffectChance = currentLowMoodEffectChance + amount;
		currentLowMoodEffectChance = Mathf.Clamp(currentLowMoodEffectChance, 0, 100f);
		if (currentLowMoodEffectChance <= 0f) {
			Messenger.RemoveListener(Signals.HOUR_STARTED, DecreaseMinorMentalBreakChance);
		}
	}
	// private void SetMinorMentalBreakChance(float amount) {
	// 	_currentLowMoodEffectChance = amount;
	// 	_currentLowMoodEffectChance = Mathf.Clamp(currentLowMoodEffectChance, 0, 100f);
	// 	if (currentLowMoodEffectChance <= 0f) {
	// 		Messenger.RemoveListener(Signals.HOUR_STARTED, DecreaseMinorMentalBreakChance);
	// 	}
	// }
	// private void IncreaseMinorMentalBreakChance() {
	// 	AdjustMinorMentalBreakChance(GetMinorMentalBreakChanceIncrease());
	// }
	private void DecreaseMinorMentalBreakChance() {
		AdjustMinorMentalBreakChance(GetMinorMentalBreakChanceDecrease());
	}
	// private float GetMinorMentalBreakChanceIncrease() {
	// 	return 100f / (EditableValuesManager.Instance.minorMentalBreakDayThreshold * 24f); //because there are 24 hours in a day
	// }
	private float GetMinorMentalBreakChanceDecrease() {
		return (100f / (EditableValuesManager.Instance.minorMentalBreakDayThreshold * 24f)) * -1f; //because there are 24 hours in a day
	}
	// private void ResetMinorMentalBreakChance() {
	// 	Debug.Log($"<color=blue>{GameManager.Instance.TodayLogString()}{owner.name} reset minor mental break chance.</color>");
	// 	SetMinorMentalBreakChance(0f);
	// }
	// private void TriggerMinorMentalBreak() {
	// 	if (_isInMinorMentalBreak) {
	// 		throw new Exception($"{GameManager.Instance.TodayLogString()}{owner.name} is already in a minor mental break, but is trying to trigger another one!");
	// 	}
	// 	int roll = Random.Range(0, 2);
	// 	string summary = $"{GameManager.Instance.TodayLogString()}{owner.name} triggered minor mental break.";
	// 	_isInMinorMentalBreak = true;
	// 	if (roll == 0) {
	// 		summary += "Chosen break is <b>Hide at Home</b>";
	// 		TriggerHideAtHome();	
	// 	} else if (roll == 1) {
	// 		summary += "Chosen break is <b>dazed</b>";
	// 		TriggerDazed();
	// 	}
	// 	owner.interruptComponent.TriggerInterrupt(INTERRUPT.Mental_Break, owner);
	// 	Debug.Log($"<color=red>{summary}</color>");
	// 	//StopCheckingForMinorMentalBreak();
	// }
	// private void TriggerHideAtHome() {
	// 	if (owner.traitContainer.AddTrait(owner, "Hiding")) {
	// 		mentalBreakName = "Desires Isolation";
	// 		Messenger.AddListener<ITraitable, Trait, Character>(Signals.TRAITABLE_LOST_TRAIT, CheckIfHidingLost);	
	// 	} else {
	// 		Debug.LogWarning($"{owner.name} triggered hide at home mental break but could not add hiding trait to its traits!");
	// 	}
	// }
	// private void CheckIfHidingLost(ITraitable traitable, Trait trait, Character removedBy) {
	// 	if (traitable == owner && trait is Hiding) {
	// 		//gain catharsis
	// 		owner.traitContainer.AddTrait(owner, "Catharsis");
	// 		Messenger.RemoveListener<ITraitable, Trait, Character>(Signals.TRAITABLE_LOST_TRAIT, CheckIfHidingLost);
	// 		OnMentalBreakDone();
	// 	}
	// }
	// private void TriggerDazed() {
	// 	if (owner.traitContainer.AddTrait(owner, "Dazed")) {
	// 		mentalBreakName = "Dazed";
	// 		Messenger.AddListener<ITraitable, Trait, Character>(Signals.TRAITABLE_LOST_TRAIT, CheckIfDazedLost);	
	// 	} else {
	// 		Debug.LogWarning($"{owner.name} triggered berserk mental break but could not add berserk trait to its traits!");
	// 	}
	// }
	// private void CheckIfDazedLost(ITraitable traitable, Trait trait, Character removedBy) {
	// 	if (traitable == owner && trait is Dazed) {
	// 		//gain catharsis
	// 		owner.traitContainer.AddTrait(owner, "Catharsis");
	// 		Messenger.RemoveListener<ITraitable, Trait, Character>(Signals.TRAITABLE_LOST_TRAIT, CheckIfDazedLost);
	// 		OnMentalBreakDone();
	// 	}
	// }
	#endregion

	#region Mental Break Shared
	private void OnMentalBreakDone() {
		//_isInMinorMentalBreak = false;
		isInMajorMentalBreak = false;
		ResetMajorMentalBreakChance();
		mentalBreakName = string.Empty;
		//ResetMinorMentalBreakChance();
		// if (_isInLowMood) {
		// 	Debug.Log($"{GameManager.Instance.TodayLogString()}{owner.name} is still in low mood state after mental break, starting check for minor mental break again...");
		// 	StartCheckingForMinorMentalBreak();
		// }else 
		if (isInCriticalMood) {
			Debug.Log($"{GameManager.Instance.TodayLogString()}{owner.name} is still in critical mood state after mental break, starting check for major mental break again...");
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
	private void AddModificationToSummary(string modificationKey, int modificationValue, GameDate expiryDate) {
		if (!moodModificationsSummary.ContainsKey(modificationKey)) {
			moodModificationsSummary.Add(modificationKey, 0);
		}
		if (!allMoodModifications.ContainsKey(modificationKey)) {
			allMoodModifications.Add(modificationKey, new MoodModification(modificationValue, expiryDate));
		} else {
			allMoodModifications[modificationKey].AddModification(modificationValue, expiryDate);
		}
		Debug.Log($"<color=blue>{owner.name} Added mood modification {modificationKey} {modificationValue.ToString()}</color>");
		moodModificationsSummary[modificationKey] += modificationValue;
		Messenger.Broadcast(CharacterSignals.MOOD_SUMMARY_MODIFIED, this);
	}
	private void RemoveModificationFromSummary(string modificationKey, int modificationValue) {
		if (moodModificationsSummary.ContainsKey(modificationKey)) {
			Debug.Log($"<color=red>{owner.name} Removed mood modification {modificationKey} {modificationValue.ToString()}</color>");
			moodModificationsSummary[modificationKey] += modificationValue;
			if (moodModificationsSummary[modificationKey] == 0) {
				moodModificationsSummary.Remove(modificationKey);
			}
			
			if (allMoodModifications.ContainsKey(modificationKey)) {
				MoodModification modification = allMoodModifications[modificationKey];
				modification.expiryDates.RemoveAt(0); //remove oldest date.
				modification.modifications.RemoveAt(modification.modifications.Count - 1); //remove latest modification. Because stacking statuses are first in last out.
				if (allMoodModifications[modificationKey].IsEmpty()) {
					allMoodModifications.Remove(modificationKey);	
				}
			}
			
			Messenger.Broadcast(CharacterSignals.MOOD_SUMMARY_MODIFIED, this);
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
        moodModificationsSummary = data.moodModificationsSummary;
        allMoodModifications = data.allMoodModifications;
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
	public MoodModification(int modification, GameDate expiryDate) {
		modifications = new List<int>();
		expiryDates = new List<GameDate>();
		AddModification(modification, expiryDate);
	}

	public void AddModification(int modification, GameDate expiryDate) {
		modifications.Add(modification);
		expiryDates.Add(expiryDate);
	}

	public bool IsEmpty() {
		return expiryDates.Count == 0 && modifications.Count == 0;
	}
}