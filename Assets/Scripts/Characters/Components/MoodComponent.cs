using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Traits;
using UnityEngine;
using Random = UnityEngine.Random;

public class MoodComponent {
	
	private readonly Character _owner;
	public int moodValue { get; private set; }

	private bool _isInNormalMood;
	private bool _isInLowMood;
	private bool _isInCriticalMood;
	private float _currentLowMoodEffectChance;
	
	private float _currentCriticalMoodEffectChance;
	private bool _executeMoodChangeEffects;
	private bool _isInMajorMentalBreak;
	private bool _isInMinorMentalBreak;

    private bool _hasMoodChanged;
	
	public Dictionary<string, int> moodModificationsSummary { get; }
	public string mentalBreakName { get; private set; }
	public MOOD_STATE moodState {
		get {
			if (_isInNormalMood) {
				return MOOD_STATE.NORMAL;	
			} else if (_isInLowMood) {
				return MOOD_STATE.LOW;
			} else if (_isInCriticalMood) {
				return MOOD_STATE.CRITICAL;
			} else {
				throw new Exception($"Problem determining {_owner.name}'s mood. Because all switches are set to false.");
			}
		}
	}
	public float currentCriticalMoodEffectChance => _currentCriticalMoodEffectChance;
	public float currentLowMoodEffectChance => _currentLowMoodEffectChance;
	public bool executeMoodChangeEffects => _executeMoodChangeEffects;
	
	public MoodComponent(Character owner) {
		_owner = owner;
		EnableMoodEffects();
		moodModificationsSummary = new Dictionary<string, int>();
		_isInNormalMood = true; //set as initially in normal mood
	}

	#region Events
	public void OnCharacterBecomeMinionOrSummon() {
		DisableMoodEffects();
		StopCheckingForMinorMentalBreak();
		StopCheckingForMajorMentalBreak();
	}
	public void OnCharacterNoLongerMinionOrSummon() {
		EnableMoodEffects();
	}
	#endregion

	private void EnableMoodEffects() {
		_executeMoodChangeEffects = true;
	}
	private void DisableMoodEffects() {
		_executeMoodChangeEffects = false;
	}
	public void SetMoodValue(int amount) {
		moodValue = amount;
        _hasMoodChanged = true;
        //OnMoodChanged();
	}
	public void AddMoodEffect(int amount, IMoodModifier modifier) {
		if (amount == 0) {
			return; //ignore
		}
		moodValue += amount;
		AddModificationToSummary(modifier.moodModificationDescription, amount);
        _hasMoodChanged = true;
        //OnMoodChanged();
	}
	public void RemoveMoodEffect(int amount, IMoodModifier modifier) {
		if (amount == 0) {
			return; //ignore
		}
		moodValue += amount;
		RemoveModificationFromSummary(modifier.moodModificationDescription, amount);
        _hasMoodChanged = true;
        //OnMoodChanged();
	}
    public void OnTickEnded() {
        if (_hasMoodChanged) {
            _hasMoodChanged = false;
            OnMoodChanged();
        }
    }

	#region Loading
	public void Load(SaveDataCharacter saveDataCharacter) {
		SetMoodValue(saveDataCharacter.moodValue);
	}
	#endregion

	#region Events
	private void OnMoodChanged() {
		if (moodValue >= EditableValuesManager.Instance.normalMoodMinThreshold) {
			if (_isInNormalMood == false) {
				EnterNormalMood();	
			}
		} else if (moodValue >= EditableValuesManager.Instance.lowMoodMinThreshold 
		           && moodValue <= EditableValuesManager.Instance.lowMoodHighThreshold) {
			if (_isInLowMood == false) {
				EnterLowMood();	
			}
		} else if (moodValue <= EditableValuesManager.Instance.criticalMoodHighThreshold) {
			if (_isInCriticalMood == false) {
				EnterCriticalMood();	
			}
		}
	}
	#endregion

	#region Normal Mood
	private void EnterNormalMood() {
		SwitchMoodStates(MOOD_STATE.NORMAL);
		Debug.Log(
			$"{GameManager.Instance.TodayLogString()} {_owner.name} is <color=green>entering</color> <b>normal</b> mood state");
	}
	private void ExitNormalMood() {
		_isInNormalMood = false;
		Debug.Log(
			$"{GameManager.Instance.TodayLogString()} {_owner.name} is <color=red>exiting</color> <b>normal</b> mood state");
	}
	#endregion
	
	#region Low Mood
	private void EnterLowMood() {
		SwitchMoodStates(MOOD_STATE.LOW);
		Debug.Log(
			$"{GameManager.Instance.TodayLogString()} {_owner.name} is <color=green>entering</color> <b>Low</b> mood state");
		if (executeMoodChangeEffects) {
			StartCheckingForMinorMentalBreak();
			if (currentLowMoodEffectChance > 0f) {
				Messenger.RemoveListener(Signals.HOUR_STARTED, DecreaseMinorMentalBreakChance);
			}	
		}
		
	}
	private void ExitLowMood() {
		_isInLowMood = false;
		Debug.Log(
			$"{GameManager.Instance.TodayLogString()} {_owner.name} is <color=red>exiting</color> <b>low</b> mood state");
		if (executeMoodChangeEffects) {
			StopCheckingForMinorMentalBreak();
			if (currentLowMoodEffectChance > 0f) {
				Messenger.AddListener(Signals.HOUR_STARTED, DecreaseMinorMentalBreakChance);
			}
		}
	}
	private void StartCheckingForMinorMentalBreak() {
		Debug.Log($"<color=blue>{GameManager.Instance.TodayLogString()}{_owner.name} has started checking for minor mental break.</color>");
		Messenger.AddListener(Signals.HOUR_STARTED, CheckForMinorMentalBreak);
	}
	private void StopCheckingForMinorMentalBreak() {
		Debug.Log($"<color=red>{GameManager.Instance.TodayLogString()}{_owner.name} has stopped checking for minor mental break.</color>");
		Messenger.RemoveListener(Signals.HOUR_STARTED, CheckForMinorMentalBreak);
	}
	#endregion

	#region Critical Mood
	private void EnterCriticalMood() {
		SwitchMoodStates(MOOD_STATE.CRITICAL);
		Debug.Log(
			$"{GameManager.Instance.TodayLogString()} {_owner.name} is <color=green>entering</color> <b>critical</b> mood state");
		if (executeMoodChangeEffects) {
			//start checking for major mental breaks
			if (_isInMajorMentalBreak) {
				Debug.Log(
					$"{GameManager.Instance.TodayLogString()}{_owner.name} is currently in a major mental break. So not starting hourly check for major mental break.");
			} else {
				StartCheckingForMajorMentalBreak();
			}
			if (currentCriticalMoodEffectChance > 0f) {
				Messenger.RemoveListener(Signals.HOUR_STARTED, DecreaseMajorMentalBreakChance);
			}
		}
	}
	private void ExitCriticalMood() {
		_isInCriticalMood = false;
		Debug.Log(
			$"{GameManager.Instance.TodayLogString()} {_owner.name} is <color=red>exiting</color> <b>critical</b> mood state");
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
		if (_owner.canPerform && _isInMinorMentalBreak == false && _isInMajorMentalBreak == false) {
			float roll = Random.Range(0f, 100f);
			Debug.Log(
				$"<color=green>{GameManager.Instance.TodayLogString()}{_owner.name} is checking for <b>MAJOR</b> mental break. Roll is <b>{roll.ToString(CultureInfo.InvariantCulture)}</b>. Chance is <b>{currentCriticalMoodEffectChance.ToString(CultureInfo.InvariantCulture)}</b></color>");
			if (roll <= currentCriticalMoodEffectChance) {
				//Trigger Major Mental Break.
				TriggerMajorMentalBreak();
			}	
		}
	}
	private void AdjustMajorMentalBreakChance(float amount) {
		_currentCriticalMoodEffectChance = currentCriticalMoodEffectChance + amount;
		_currentCriticalMoodEffectChance = Mathf.Clamp(currentCriticalMoodEffectChance, 0, 100f);
		if (currentCriticalMoodEffectChance <= 0f) {
			Messenger.RemoveListener(Signals.HOUR_STARTED, DecreaseMajorMentalBreakChance);
		}
	}
	private void SetMajorMentalBreakChance(float amount) {
		_currentCriticalMoodEffectChance = amount;
		_currentCriticalMoodEffectChance = Mathf.Clamp(currentCriticalMoodEffectChance, 0, 100f);
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
		Debug.Log($"<color=blue>{GameManager.Instance.TodayLogString()}{_owner.name} reset major mental break chance.</color>");
		SetMajorMentalBreakChance(0f);
	}
	private void StopCheckingForMajorMentalBreak() {
		Debug.Log($"<color=red>{GameManager.Instance.TodayLogString()}{_owner.name} has stopped checking for major mental break.</color>");
		Messenger.RemoveListener(Signals.TICK_STARTED, CheckForMajorMentalBreak);
	}
	private void StartCheckingForMajorMentalBreak() {
		Debug.Log($"<color=blue>{GameManager.Instance.TodayLogString()}{_owner.name} has started checking for major mental break.</color>");
		Messenger.AddListener(Signals.TICK_STARTED, CheckForMajorMentalBreak);
	}
	#endregion

	#region Major Mental Break
	private void TriggerMajorMentalBreak() {
		if (_isInMajorMentalBreak) {
			throw new Exception($"{GameManager.Instance.TodayLogString()}{_owner.name} is already in a major mental break, but is trying to trigger another one!");
		}
		int roll = Random.Range(0, 3);
		string summary = $"{GameManager.Instance.TodayLogString()}{_owner.name} triggered major mental break.";
		_isInMajorMentalBreak = true;
		if (roll == 0) {
			//Berserk
			summary += "Chosen break is <b>berserk</b>";
			TriggerBerserk();
		} else if (roll == 1) {
			//catatonic
			summary += "Chosen break is <b>catatonic</b>";
			TriggerCatatonic();
		} else if (roll == 2) {
			//suicidal
			summary += "Chosen break is <b>suicidal</b>";
			TriggerSuicidal();
		}
		_owner.interruptComponent.TriggerInterrupt(INTERRUPT.Mental_Break, _owner);
		Debug.Log($"<color=red>{summary}</color>");
		StopCheckingForMajorMentalBreak();
	}
	private void TriggerBerserk() {
		if (_owner.traitContainer.AddTrait(_owner, "Berserked")) {
			mentalBreakName = "Berserked";
			Messenger.AddListener<ITraitable, Trait, Character>(Signals.TRAITABLE_LOST_TRAIT, CheckIfBerserkLost);	
		} else {
			Debug.LogWarning($"{_owner.name} triggered berserk mental break but could not add berserk trait to its traits!");
		}
	}
	private void CheckIfBerserkLost(ITraitable traitable, Trait trait, Character removedBy) {
		if (traitable == _owner && trait is Berserked) {
			//gain catharsis
			_owner.traitContainer.AddTrait(_owner, "Catharsis");
			Messenger.RemoveListener<ITraitable, Trait, Character>(Signals.TRAITABLE_LOST_TRAIT, CheckIfBerserkLost);
			OnMentalBreakDone();
		}
	}
	private void TriggerCatatonic() {
		if (_owner.traitContainer.AddTrait(_owner, "Catatonic")) {
			mentalBreakName = "Catatonia";
			Messenger.AddListener<ITraitable, Trait, Character>(Signals.TRAITABLE_LOST_TRAIT, CheckIfCatatonicLost);
		} else {
			Debug.LogWarning($"{_owner.name} triggered catatonic mental break but could not add catatonic trait to its traits!");
		}
	}
	private void CheckIfCatatonicLost(ITraitable traitable, Trait trait, Character removedBy) {
		if (traitable == _owner && trait is Catatonic) {
			//gain catharsis
			_owner.traitContainer.AddTrait(_owner, "Catharsis");
			Messenger.RemoveListener<ITraitable, Trait, Character>(Signals.TRAITABLE_LOST_TRAIT, CheckIfCatatonicLost);	
			OnMentalBreakDone();
		}
	}
	private void TriggerSuicidal() {
		if (_owner.traitContainer.AddTrait(_owner, "Suicidal")) {
			mentalBreakName = "Suicidal";
			Messenger.AddListener<ITraitable, Trait, Character>(Signals.TRAITABLE_LOST_TRAIT, CheckIfSuicidalLost);
		} else {
			Debug.LogWarning($"{_owner.name} triggered suicidal mental break but could not add suicidal trait to its traits!");
		}
	}
	private void CheckIfSuicidalLost(ITraitable traitable, Trait trait, Character removedBy) {
		if (traitable == _owner && trait is Suicidal) {
			//gain catharsis
			_owner.traitContainer.AddTrait(_owner, "Catharsis");
			Messenger.RemoveListener<ITraitable, Trait, Character>(Signals.TRAITABLE_LOST_TRAIT, CheckIfSuicidalLost);
			OnMentalBreakDone();
		}
	}
	#endregion

	#region Minor Mental Break
	private void CheckForMinorMentalBreak() {
		IncreaseMinorMentalBreakChance();
		if (_owner.canPerform && _isInMinorMentalBreak == false && _isInMajorMentalBreak == false) {
			float roll = Random.Range(0f, 100f);
			Debug.Log(
				$"<color=green>{GameManager.Instance.TodayLogString()}{_owner.name} is checking for <b>MINOR</b> mental break. Roll is <b>{roll.ToString(CultureInfo.InvariantCulture)}</b>. Chance is <b>{currentLowMoodEffectChance.ToString(CultureInfo.InvariantCulture)}</b></color>");
			if (roll <= currentLowMoodEffectChance) {
				//Trigger Minor Mental Break.
				TriggerMinorMentalBreak();
			}	
		}
	}
	private void AdjustMinorMentalBreakChance(float amount) {
		_currentLowMoodEffectChance = currentLowMoodEffectChance + amount;
		_currentLowMoodEffectChance = Mathf.Clamp(currentLowMoodEffectChance, 0, 100f);
		if (currentLowMoodEffectChance <= 0f) {
			Messenger.RemoveListener(Signals.HOUR_STARTED, DecreaseMinorMentalBreakChance);
		}
	}
	private void SetMinorMentalBreakChance(float amount) {
		_currentLowMoodEffectChance = amount;
		_currentLowMoodEffectChance = Mathf.Clamp(currentLowMoodEffectChance, 0, 100f);
		if (currentLowMoodEffectChance <= 0f) {
			Messenger.RemoveListener(Signals.HOUR_STARTED, DecreaseMinorMentalBreakChance);
		}
	}
	private void IncreaseMinorMentalBreakChance() {
		AdjustMinorMentalBreakChance(GetMinorMentalBreakChanceIncrease());
	}
	private void DecreaseMinorMentalBreakChance() {
		AdjustMinorMentalBreakChance(GetMinorMentalBreakChanceDecrease());
	}
	private float GetMinorMentalBreakChanceIncrease() {
		return 100f / (EditableValuesManager.Instance.minorMentalBreakDayThreshold * 24f); //because there are 24 hours in a day
	}
	private float GetMinorMentalBreakChanceDecrease() {
		return (100f / (EditableValuesManager.Instance.minorMentalBreakDayThreshold * 24f)) * -1f; //because there are 24 hours in a day
	}
	private void ResetMinorMentalBreakChance() {
		Debug.Log($"<color=blue>{GameManager.Instance.TodayLogString()}{_owner.name} reset minor mental break chance.</color>");
		SetMinorMentalBreakChance(0f);
	}
	private void TriggerMinorMentalBreak() {
		if (_isInMinorMentalBreak) {
			throw new Exception($"{GameManager.Instance.TodayLogString()}{_owner.name} is already in a minor mental break, but is trying to trigger another one!");
		}
		int roll = Random.Range(0, 2);
		string summary = $"{GameManager.Instance.TodayLogString()}{_owner.name} triggered minor mental break.";
		_isInMinorMentalBreak = true;
		if (roll == 0) {
			summary += "Chosen break is <b>Hide at Home</b>";
			TriggerHideAtHome();	
		} else if (roll == 1) {
			summary += "Chosen break is <b>dazed</b>";
			TriggerDazed();
		}
		_owner.interruptComponent.TriggerInterrupt(INTERRUPT.Mental_Break, _owner);
		Debug.Log($"<color=red>{summary}</color>");
		StopCheckingForMinorMentalBreak();
	}
	private void TriggerHideAtHome() {
		if (_owner.traitContainer.AddTrait(_owner, "Hiding")) {
			mentalBreakName = "Desires Isolation";
			Messenger.AddListener<ITraitable, Trait, Character>(Signals.TRAITABLE_LOST_TRAIT, CheckIfHidingLost);	
		} else {
			Debug.LogWarning($"{_owner.name} triggered hide at home mental break but could not add hiding trait to its traits!");
		}
	}
	private void CheckIfHidingLost(ITraitable traitable, Trait trait, Character removedBy) {
		if (traitable == _owner && trait is Hiding) {
			//gain catharsis
			_owner.traitContainer.AddTrait(_owner, "Catharsis");
			Messenger.RemoveListener<ITraitable, Trait, Character>(Signals.TRAITABLE_LOST_TRAIT, CheckIfHidingLost);
			OnMentalBreakDone();
		}
	}
	private void TriggerDazed() {
		if (_owner.traitContainer.AddTrait(_owner, "Dazed")) {
			mentalBreakName = "Dazed";
			Messenger.AddListener<ITraitable, Trait, Character>(Signals.TRAITABLE_LOST_TRAIT, CheckIfDazedLost);	
		} else {
			Debug.LogWarning($"{_owner.name} triggered berserk mental break but could not add berserk trait to its traits!");
		}
	}
	private void CheckIfDazedLost(ITraitable traitable, Trait trait, Character removedBy) {
		if (traitable == _owner && trait is Dazed) {
			//gain catharsis
			_owner.traitContainer.AddTrait(_owner, "Catharsis");
			Messenger.RemoveListener<ITraitable, Trait, Character>(Signals.TRAITABLE_LOST_TRAIT, CheckIfDazedLost);
			OnMentalBreakDone();
		}
	}
	#endregion

	#region Mental Break Shared
	private void OnMentalBreakDone() {
		_isInMinorMentalBreak = false;
		_isInMajorMentalBreak = false;
		ResetMajorMentalBreakChance();
		ResetMinorMentalBreakChance();
		if (_isInLowMood) {
			Debug.Log($"{GameManager.Instance.TodayLogString()}{_owner.name} is still in low mood state after mental break, starting check for minor mental break again...");
			StartCheckingForMinorMentalBreak();
		}else if (_isInCriticalMood) {
			Debug.Log($"{GameManager.Instance.TodayLogString()}{_owner.name} is still in critical mood state after mental break, starting check for major mental break again...");
			StartCheckingForMajorMentalBreak();
		}
	}
	#endregion
	
	#region Utilities
	private void SwitchMoodStates(MOOD_STATE moodToEnter) {
		MOOD_STATE lastMoodState = moodState;
		switch (moodToEnter) {
			case MOOD_STATE.LOW:
				_isInLowMood = true;
				break;
			case MOOD_STATE.NORMAL:
				_isInNormalMood = true;
				break;
			case MOOD_STATE.CRITICAL:
				_isInCriticalMood = true;
				break;
		}
		switch (lastMoodState) {
			case MOOD_STATE.LOW:
				ExitLowMood();
				break;
			case MOOD_STATE.NORMAL:
				ExitNormalMood();
				break;
			case MOOD_STATE.CRITICAL:
				ExitCriticalMood();
				break;
		}
	}
	#endregion

	#region Summary
	private void AddModificationToSummary(string modificationKey, int modificationValue) {
		if (moodModificationsSummary.ContainsKey(modificationKey) == false) {
			moodModificationsSummary.Add(modificationKey, 0);
		}
		Debug.Log($"<color=blue>{_owner.name} Added mood modification {modificationKey} {modificationValue.ToString()}</color>");
		moodModificationsSummary[modificationKey] += modificationValue;
		Messenger.Broadcast(Signals.MOOD_SUMMARY_MODIFIED, this);
	}
	private void RemoveModificationFromSummary(string modificationKey, int modificationValue) {
		if (moodModificationsSummary.ContainsKey(modificationKey)) {
			Debug.Log($"<color=red>{_owner.name} Removed mood modification {modificationKey} {modificationValue.ToString()}</color>");
			moodModificationsSummary[modificationKey] += modificationValue;
			if (moodModificationsSummary[modificationKey] == 0) {
				moodModificationsSummary.Remove(modificationKey);
			}
			Messenger.Broadcast(Signals.MOOD_SUMMARY_MODIFIED, this);
		}
	}
	#endregion
}