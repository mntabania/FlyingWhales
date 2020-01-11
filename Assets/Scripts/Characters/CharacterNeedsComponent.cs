﻿using System;
using Traits;
using UnityEngine;

public class CharacterNeedsComponent {

    private readonly Character _character;
    public int doNotGetHungry { get; private set; }
    public int doNotGetTired{ get; private set; }
    public int doNotGetLonely{ get; private set; }
    public int doNotGetUncomfortable { get; private set; }
    public int doNotGetGloomy { get; private set; }

    public bool isStarving => fullness >= 0f && fullness <= 20f;
    public bool isExhausted => tiredness >= 0f && tiredness <= EXHAUSTED_UPPER_LIMIT;
    public bool isForlorn => happiness >= 0f && happiness <= FORLORN_UPPER_LIMIT;
    public bool isAgonizing => comfort >= 0f && comfort <= 20f;
    public bool isDisillusioned => hope >= 0f && hope <= 20f;

    public bool isHungry => fullness > 20f && fullness <= 40f;
    public bool isTired => tiredness > EXHAUSTED_UPPER_LIMIT && tiredness <= 40f;
    public bool isLonely => happiness > FORLORN_UPPER_LIMIT && happiness <= 40f;
    public bool isUncomfortable => comfort > 20f && comfort <= 40f;
    public bool isGloomy => hope > 20f && hope <= 40f;

    public bool isFull => fullness >= 91f && fullness <= 100f;
    public bool isEnergized => tiredness >= 91f && tiredness <= 100f;
    public bool isHappy => happiness >= 91f && happiness <= 100f;
    public bool isRelaxed => comfort >= 91f && comfort <= 100f;
    public bool isSanguine => hope >= 91f && hope <= 100f;


    //Tiredness
    public float tiredness { get; private set; }
    public float tirednessDecreaseRate { get; private set; }
    public int tirednessForcedTick { get; private set; }
    public int currentSleepTicks { get; private set; }
    public int sleepScheduleJobID { get; set; }
    public bool hasCancelledSleepSchedule { get; private set; }
    private float tirednessLowerBound; //how low can this characters tiredness go
    public const float TIREDNESS_DEFAULT = 100f;
    public const float EXHAUSTED_UPPER_LIMIT = 20f;
    //public const int TIREDNESS_THRESHOLD_1 = 10000;
    //public const int TIREDNESS_THRESHOLD_2 = 5000;

    //Fullness
    public float fullness { get; private set; }
    public float fullnessDecreaseRate { get; private set; }
    public int fullnessForcedTick { get; private set; }
    private float fullnessLowerBound; //how low can this characters fullness go
    public const float FULLNESS_DEFAULT = 100f;
    //public const int FULLNESS_THRESHOLD_1 = 10000;
    //public const int FULLNESS_THRESHOLD_2 = 5000;

    //Happiness
    public float happiness { get; private set; }
    public float happinessDecreaseRate { get; private set; }
    private float happinessLowerBound; //how low can this characters happiness go
    public const float HAPPINESS_DEFAULT = 100f;
    public const float FORLORN_UPPER_LIMIT = 20f;
    //public const int HAPPINESS_THRESHOLD_1 = 10000;
    //public const int HAPPINESS_THRESHOLD_2 = 5000;

    //Comfort
    public float comfort { get; private set; }
    public float comfortDecreaseRate { get; private set; }
    private float comfortLowerBound; //how low can this characters happiness go
    public const float COMFORT_DEFAULT = 100f;

    //Hope
    public float hope { get; private set; }
    private float hopeLowerBound; //how low can this characters happiness go
    public const float HOPE_DEFAULT = 100f;

    public bool hasForcedFullness { get; set; }
    public bool hasForcedTiredness { get; set; }
    public TIME_IN_WORDS forcedFullnessRecoveryTimeInWords { get; private set; }
    public TIME_IN_WORDS forcedTirednessRecoveryTimeInWords { get; private set; }

    public CharacterNeedsComponent(Character character) {
        this._character = character;
        SetTirednessLowerBound(0f);
        SetFullnessLowerBound(0f);
        SetHappinessLowerBound(0f);
        SetComfortLowerBound(0f);
        SetHopeLowerBound(0f);
        SetForcedFullnessRecoveryTimeInWords(TIME_IN_WORDS.LUNCH_TIME);
        SetForcedTirednessRecoveryTimeInWords(TIME_IN_WORDS.LATE_NIGHT);
        SetFullnessForcedTick();
        SetTirednessForcedTick();
        
    }
    
    #region Initialization
    public void SubscribeToSignals() {
        Messenger.AddListener(Signals.TICK_STARTED, DecreaseNeeds);
        Messenger.AddListener<Character, GoapPlanJob>(Signals.CHARACTER_FINISHED_JOB, OnCharacterFinishedJob);
    }
    public void UnsubscribeToSignals() {
        Messenger.RemoveListener(Signals.TICK_STARTED, DecreaseNeeds);
        Messenger.RemoveListener<Character, GoapPlanJob>(Signals.CHARACTER_FINISHED_JOB, OnCharacterFinishedJob);
    }
    public void DailyGoapProcesses() {
        hasForcedFullness = false;
        hasForcedTiredness = false;
    }
    public void Initialize() {
        //NOTE: These values will be randomized when this character is placed in his/her settlement map.
        //tiredness = TIREDNESS_DEFAULT;
        //fullness = FULLNESS_DEFAULT;
        //happiness = HAPPINESS_DEFAULT;
        ResetTirednessMeter();
        ResetFullnessMeter();
        ResetHappinessMeter();
        ResetComfortMeter();
        ResetHopeMeter();
    }
    public void InitialCharacterPlacement() {
        ResetTirednessMeter();
        ResetFullnessMeter();
        ResetHappinessMeter();
        ResetComfortMeter();
        ResetHopeMeter();

        //tiredness = TIREDNESS_DEFAULT;
        //if (_character.role.roleType != CHARACTER_ROLE.MINION) {
        //    ////Fullness value between 2600 and full.
        //    //SetFullness(UnityEngine.Random.Range(2600, FULLNESS_DEFAULT + 1));
        //    ////Happiness value between 2600 and full.
        //    //SetHappiness(UnityEngine.Random.Range(2600, HAPPINESS_DEFAULT + 1));
        //    fullness = FULLNESS_DEFAULT;
        //    happiness = HAPPINESS_DEFAULT;
        //} else {
        //    fullness = FULLNESS_DEFAULT;
        //    happiness = HAPPINESS_DEFAULT;
        //}
    }
    #endregion

    #region Loading
    public void LoadAllStatsOfCharacter(SaveDataCharacter data) {
        tiredness = data.tiredness;
        fullness = data.fullness;
        happiness = data.happiness;
        fullnessDecreaseRate = data.fullnessDecreaseRate;
        tirednessDecreaseRate = data.tirednessDecreaseRate;
        happinessDecreaseRate = data.happinessDecreaseRate;
        SetForcedFullnessRecoveryTimeInWords(data.forcedFullnessRecoveryTimeInWords);
        SetForcedTirednessRecoveryTimeInWords(data.forcedTirednessRecoveryTimeInWords);
        SetFullnessForcedTick(data.fullnessForcedTick);
        SetTirednessForcedTick(data.tirednessForcedTick);
        currentSleepTicks = data.currentSleepTicks;
        sleepScheduleJobID = data.sleepScheduleJobID;
        hasCancelledSleepSchedule = data.hasCancelledSleepSchedule;
    }
    #endregion

    public void CheckExtremeNeeds() {
        string summary = GameManager.Instance.TodayLogString() + _character.name + " will check his/her needs.";
        if (isStarving) {
            summary += "\n" + _character.name + " is starving. Planning fullness recovery actions...";
            PlanFullnessRecoveryActions(_character);
        }
        if (isExhausted) {
            summary += "\n" + _character.name + " is exhausted. Planning tiredness recovery actions...";
            PlanTirednessRecoveryActions(_character);
        }
        if (isForlorn) {
            summary += "\n" + _character.name + " is forlorn. Planning happiness recovery actions...";
            PlanHappinessRecoveryActions(_character);
        }
        Debug.Log(summary);
    }

    private bool HasNeeds() {
        return _character.race != RACE.SKELETON && _character.characterClass.className != "Zombie" && !_character.returnedToLife && _character.isAtHomeRegion && 
               _character.homeSettlement != null; //Characters living on a region without a settlement must not decrease needs
    }
    public void DecreaseNeeds() {
        if (HasNeeds() == false) {
            return;
        }
        if (doNotGetHungry <= 0) {
            AdjustFullness(-(CharacterManager.DEFAULT_FULLNESS_DECREASE_RATE + fullnessDecreaseRate));
        }
        if (doNotGetTired <= 0) {
            AdjustTiredness(-(CharacterManager.DEFAULT_TIREDNESS_DECREASE_RATE + tirednessDecreaseRate));
        }
        if (doNotGetLonely <= 0) {
            AdjustHappiness(-(CharacterManager.DEFAULT_HAPPINESS_DECREASE_RATE + happinessDecreaseRate));
        }
        if (doNotGetUncomfortable <= 0) {
            AdjustComfort(-(CharacterManager.DEFAULT_COMFORT_DECREASE_RATE + comfortDecreaseRate));
        }
    }
    public string GetNeedsSummary() {
        string summary = "Fullness: " + fullness + "/" + FULLNESS_DEFAULT;
        summary += "\nTiredness: " + tiredness + "/" + TIREDNESS_DEFAULT;
        summary += "\nHappiness: " + happiness + "/" + HAPPINESS_DEFAULT;
        summary += "\nComfort: " + comfort + "/" + COMFORT_DEFAULT;
        summary += "\nHope: " + hope + "/" + HOPE_DEFAULT;
        return summary;
    }
    public void AdjustFullnessDecreaseRate(float amount) {
        fullnessDecreaseRate += amount;
    }
    public void AdjustTirednessDecreaseRate(float amount) {
        tirednessDecreaseRate += amount;
    }
    public void AdjustHappinessDecreaseRate(float amount) {
        happinessDecreaseRate += amount;
    }
    public void SetTirednessLowerBound(float amount) {
        tirednessLowerBound = amount;
    }
    public void SetFullnessLowerBound(float amount) {
        fullnessLowerBound = amount;
    }
    public void SetHappinessLowerBound(float amount) {
        happinessLowerBound = amount;
    }
    public void SetComfortLowerBound(float amount) {
        comfortLowerBound = amount;
    }
    public void SetHopeLowerBound(float amount) {
        hopeLowerBound = amount;
    }

    #region Tiredness
    public void ResetTirednessMeter() {
        bool wasTired = isTired;
        bool wasExhausted = isExhausted;
        bool wasEnergized = isEnergized;

        tiredness = TIREDNESS_DEFAULT;
        //RemoveTiredOrExhausted();
        OnEnergized(wasEnergized, wasTired, wasExhausted);
    }
    public void AdjustTiredness(float adjustment) {
        bool wasTired = isTired;
        bool wasExhausted = isExhausted;
        bool wasEnergized = isEnergized;

        tiredness += adjustment;
        tiredness = Mathf.Clamp(tiredness, tirednessLowerBound, TIREDNESS_DEFAULT);
        if (tiredness == 0f) {
            _character.traitContainer.AddTrait(_character, "Unconscious");
            OnExhausted(wasEnergized, wasTired, wasExhausted);
            return;
        }
        if (isEnergized) {
            OnEnergized(wasEnergized, wasTired, wasExhausted);
        } else if (isTired) {
            OnTired(wasEnergized, wasTired, wasExhausted);
        } else if (isExhausted) {
            OnExhausted(wasEnergized, wasTired, wasExhausted);
        } else {
            OnNormalEnergy(wasEnergized, wasTired, wasExhausted);
        }
        //if (isExhausted) {
        //    _character.traitContainer.RemoveTrait(_character, "Tired");
        //    if (_character.traitContainer.AddTrait(_character, "Exhausted")) {
        //        Messenger.Broadcast<Character, string>(Signals.TRANSFER_ENGAGE_TO_FLEE_LIST, _character, "exhausted");
        //    }
        //} else if (isTired) {
        //    _character.traitContainer.RemoveTrait(_character, "Exhausted");
        //    _character.traitContainer.AddTrait(_character, "Tired");
        //    //PlanTirednessRecoveryActions();
        //} else {
        //    //tiredness is higher than both thresholds
        //    RemoveTiredOrExhausted();
        //}
    }
    public void SetTiredness(float amount) {
        bool wasTired = isTired;
        bool wasExhausted = isExhausted;
        bool wasEnergized = isEnergized;

        tiredness = amount;
        tiredness = Mathf.Clamp(tiredness, tirednessLowerBound, TIREDNESS_DEFAULT);
        if (tiredness == 0f) {
            _character.traitContainer.AddTrait(_character, "Unconscious");
            OnExhausted(wasEnergized, wasTired, wasExhausted);
            return;
        }
        if (isEnergized) {
            OnEnergized(wasEnergized, wasTired, wasExhausted);
        } else if (isTired) {
            OnTired(wasEnergized, wasTired, wasExhausted);
        } else if (isExhausted) {
            OnExhausted(wasEnergized, wasTired, wasExhausted);
        } else {
            OnNormalEnergy(wasEnergized, wasTired, wasExhausted);
        }
        //if (tiredness == 0f) {
        //    _character.traitContainer.AddTrait(_character, "Unconscious");
        //    return;
        //}
        //if (isExhausted) {
        //    _character.traitContainer.RemoveTrait(_character, "Tired");
        //    _character.traitContainer.AddTrait(_character, "Exhausted");
        //} else if (isTired) {
        //    _character.traitContainer.RemoveTrait(_character, "Exhausted");
        //    _character.traitContainer.AddTrait(_character, "Tired");
        //} else {
        //    //tiredness is higher than both thresholds
        //    _character.needsComponent.RemoveTiredOrExhausted();
        //}
    }
    private void OnEnergized(bool wasEnergized, bool wasTired, bool wasExhausted) {
        if (!wasEnergized) {
            _character.traitContainer.AddTrait(_character, "Energized");
        }
        if (wasExhausted) {
            _character.traitContainer.RemoveTrait(_character, "Exhausted");
        }
        if (wasTired) {
            _character.traitContainer.RemoveTrait(_character, "Tired");
        }
    }
    private void OnTired(bool wasEnergized, bool wasTired, bool wasExhausted) {
        if (!wasTired) {
            _character.traitContainer.AddTrait(_character, "Tired");
        }
        if (wasExhausted) {
            _character.traitContainer.RemoveTrait(_character, "Exhausted");
        }
        if (wasEnergized) {
            _character.traitContainer.RemoveTrait(_character, "Energized");
        }
    }
    private void OnExhausted(bool wasEnergized, bool wasTired, bool wasExhausted) {
        if (!wasExhausted) {
            _character.traitContainer.AddTrait(_character, "Exhausted");
            Messenger.Broadcast<Character, string>(Signals.TRANSFER_ENGAGE_TO_FLEE_LIST, _character, "exhausted");
        }
        if (wasTired) {
            _character.traitContainer.RemoveTrait(_character, "Tired");
        }
        if (wasEnergized) {
            _character.traitContainer.RemoveTrait(_character, "Energized");
        }
    }
    private void OnNormalEnergy(bool wasEnergized, bool wasTired, bool wasExhausted) {
        if (wasExhausted) {
            _character.traitContainer.RemoveTrait(_character, "Exhausted");
        }
        if (wasTired) {
            _character.traitContainer.RemoveTrait(_character, "Tired");
        }
        if (wasEnergized) {
            _character.traitContainer.RemoveTrait(_character, "Energized");
        }
    }
    private void RemoveTiredOrExhausted() {
        if (_character.traitContainer.RemoveTrait(_character, "Tired") == false) {
            _character.traitContainer.RemoveTrait(_character, "Exhausted");
        }
    }
    public void SetTirednessForcedTick() {
        if (!hasForcedTiredness) {
            if (forcedTirednessRecoveryTimeInWords == GameManager.GetCurrentTimeInWordsOfTick()) {
                //If the forced recovery job has not been done yet and the character is already on the time of day when it is supposed to be done,
                //the tick that will be assigned will be ensured that the character will not miss it
                //Example if the time of day is Afternoon, the supposed tick range for it is 145 - 204
                //So if the current tick of the game is already in 160, the range must be adjusted to 161 - 204, so as to ensure that the character will hit it
                //But if the current tick of the game is already in 204, it cannot be 204 - 204, so, it will revert back to 145 - 204 
                int newTick = GameManager.GetRandomTickFromTimeInWords(forcedTirednessRecoveryTimeInWords, GameManager.Instance.tick + 1);
                TIME_IN_WORDS timeInWords = GameManager.GetTimeInWordsOfTick(newTick);
                if(timeInWords != forcedTirednessRecoveryTimeInWords) {
                    newTick = GameManager.GetRandomTickFromTimeInWords(forcedTirednessRecoveryTimeInWords);
                }
                tirednessForcedTick = newTick;
                return;
            }
        }
        tirednessForcedTick = GameManager.GetRandomTickFromTimeInWords(forcedTirednessRecoveryTimeInWords);
    }
    public void SetTirednessForcedTick(int tick) {
        tirednessForcedTick = tick;
    }
    public void SetForcedTirednessRecoveryTimeInWords(TIME_IN_WORDS timeInWords) {
        forcedTirednessRecoveryTimeInWords = timeInWords;
    }
    public void AdjustDoNotGetTired(int amount) {
        doNotGetTired += amount;
        doNotGetTired = Math.Max(doNotGetTired, 0);
    }
    public bool PlanTirednessRecoveryActions(Character character) {
        if (character.doNotDisturb || !character.canWitness) { //character.doNotDisturb > 0 || !character.canWitness
            return false;
        }
        if (this.isExhausted) {
            if (!character.jobQueue.HasJob(JOB_TYPE.TIREDNESS_RECOVERY_EXHAUSTED)) {
                //If there is already a TIREDNESS_RECOVERY JOB and the character becomes Exhausted, replace TIREDNESS_RECOVERY with TIREDNESS_RECOVERY_STARVING only if that character is not doing the job already
                JobQueueItem tirednessRecoveryJob = character.jobQueue.GetJob(JOB_TYPE.TIREDNESS_RECOVERY);
                if (tirednessRecoveryJob != null) {
                    //Replace this with Tiredness Recovery Exhausted only if the character is not doing the Tiredness Recovery Job already
                    JobQueueItem currJob = character.currentJob;
                    if (currJob == tirednessRecoveryJob) {
                        return false;
                    } else {
                        tirednessRecoveryJob.CancelJob();
                    }
                }
                JOB_TYPE jobType = JOB_TYPE.TIREDNESS_RECOVERY_EXHAUSTED;
                bool triggerSpooked = false;
                Spooked spooked = character.traitContainer.GetNormalTrait<Trait>("Spooked") as Spooked;
                if (spooked != null) {
                    triggerSpooked = UnityEngine.Random.Range(0, 100) < 20;
                }
                if (!triggerSpooked) {
                    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, new GoapEffect(GOAP_EFFECT_CONDITION.TIREDNESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR), _character, _character);
                    //job.SetCancelOnFail(true);
                    character.jobQueue.AddJobInQueue(job);
                } else {
                    spooked.TriggerFeelingSpooked();
                }
                return true;
            }
        }
        return false;
    }
    public void PlanScheduledTirednessRecovery(Character character) {
        if (!hasForcedTiredness && tirednessForcedTick != 0 && GameManager.Instance.tick >= tirednessForcedTick && !character.doNotDisturb && doNotGetTired <= 0) {
            if (!character.jobQueue.HasJob(JOB_TYPE.TIREDNESS_RECOVERY, JOB_TYPE.TIREDNESS_RECOVERY_EXHAUSTED)) {
                JOB_TYPE jobType = JOB_TYPE.TIREDNESS_RECOVERY;
                if (isExhausted) {
                    jobType = JOB_TYPE.TIREDNESS_RECOVERY_EXHAUSTED;
                }

                bool triggerSpooked = false;
                Spooked spooked = character.traitContainer.GetNormalTrait<Trait>("Spooked") as Spooked;
                if (spooked != null) {
                    triggerSpooked = UnityEngine.Random.Range(0, 100) < 20;
                }
                if (!triggerSpooked) {
                    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, new GoapEffect(GOAP_EFFECT_CONDITION.TIREDNESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR), _character, _character);
                    //GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TIREDNESS_RECOVERY, conditionKey = null, targetPOI = this });
                    //job.SetCancelOnFail(true);
                    sleepScheduleJobID = job.id;
                    //bool willNotProcess = _numOfWaitingForGoapThread > 0 || !IsInOwnParty() || isDefender || isWaitingForInteraction > 0 /*|| stateComponent.stateToDo != null*/;
                    character.jobQueue.AddJobInQueue(job); //, !willNotProcess
                } else {
                    spooked.TriggerFeelingSpooked();
                }
            }
            hasForcedTiredness = true;
            SetTirednessForcedTick();
        }
        //If a character current sleep ticks is less than the default, this means that the character already started sleeping but was awaken midway that is why he/she did not finish the allotted sleeping time
        //When this happens, make sure to queue tiredness recovery again so he can finish the sleeping time
        else if ((hasCancelledSleepSchedule || currentSleepTicks < CharacterManager.Instance.defaultSleepTicks) && !character.doNotDisturb) {
            if (!character.jobQueue.HasJob(JOB_TYPE.TIREDNESS_RECOVERY, JOB_TYPE.TIREDNESS_RECOVERY_EXHAUSTED)) {
                JOB_TYPE jobType = JOB_TYPE.TIREDNESS_RECOVERY;
                if (isExhausted) {
                    jobType = JOB_TYPE.TIREDNESS_RECOVERY_EXHAUSTED;
                }
                bool triggerSpooked = false;
                Spooked spooked = character.traitContainer.GetNormalTrait<Trait>("Spooked") as Spooked;
                if (spooked != null) {
                    triggerSpooked = UnityEngine.Random.Range(0, 100) < 20;
                }
                if (!triggerSpooked) {
                    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, new GoapEffect(GOAP_EFFECT_CONDITION.TIREDNESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR), _character, _character);
                    //job.SetCancelOnFail(true);
                    sleepScheduleJobID = job.id;
                    //bool willNotProcess = _numOfWaitingForGoapThread > 0 || !IsInOwnParty() || isDefender || isWaitingForInteraction > 0
                    //    || stateComponent.currentState != null || stateComponent.stateToDo != null;
                    character.jobQueue.AddJobInQueue(job); //!willNotProcess
                } else {
                    spooked.TriggerFeelingSpooked();
                }
            }
            SetHasCancelledSleepSchedule(false);
        }
    }
    public void SetHasCancelledSleepSchedule(bool state) {
        hasCancelledSleepSchedule = state;
    }
    public void ResetSleepTicks() {
        currentSleepTicks = CharacterManager.Instance.defaultSleepTicks;
    }
    public void AdjustSleepTicks(int amount) {
        currentSleepTicks += amount;
        if(currentSleepTicks <= 0) {
            this.ResetSleepTicks();
        }
    }
    public void ExhaustCharacter(Character character) {
        if (!isExhausted) {
            SetTiredness(EXHAUSTED_UPPER_LIMIT);
        }
    }
    #endregion

    #region Happiness
    public void ResetHappinessMeter() {
        bool wasLonely = isLonely;
        bool wasForlorn = isForlorn;
        bool wasHappy = isHappy;

        happiness = HAPPINESS_DEFAULT;

        OnHappy(wasHappy, wasLonely, wasForlorn);
        //OnHappinessAdjusted();
    }
    public void AdjustHappiness(float adjustment) {
        bool wasLonely = isLonely;
        bool wasForlorn = isForlorn;
        bool wasHappy = isHappy;

        happiness += adjustment;
        happiness = Mathf.Clamp(happiness, happinessLowerBound, HAPPINESS_DEFAULT);

        if (isHappy) {
            OnHappy(wasHappy, wasLonely, wasForlorn);
        } else if (isLonely) {
            OnLonely(wasHappy, wasLonely, wasForlorn);
        } else if (isForlorn) {
            OnForlorn(wasHappy, wasLonely, wasForlorn);
        } else {
            OnNormalHappiness(wasHappy, wasLonely, wasForlorn);
        }
        //OnHappinessAdjusted();
    }
    public void SetHappiness(float amount) {
        bool wasLonely = isLonely;
        bool wasForlorn = isForlorn;
        bool wasHappy = isHappy;

        happiness = amount;
        happiness = Mathf.Clamp(happiness, happinessLowerBound, HAPPINESS_DEFAULT);

        if (isHappy) {
            OnHappy(wasHappy, wasLonely, wasForlorn);
        } else if (isLonely) {
            OnLonely(wasHappy, wasLonely, wasForlorn);
        } else if (isForlorn) {
            OnForlorn(wasHappy, wasLonely, wasForlorn);
        } else {
            OnNormalHappiness(wasHappy, wasLonely, wasForlorn);
        }
        //OnHappinessAdjusted();
    }
    private void OnHappy(bool wasHappy, bool wasLonely, bool wasForlorn) {
        if (!wasHappy) {
            _character.traitContainer.AddTrait(_character, "Happy");
        }
        if (wasLonely) {
            _character.traitContainer.RemoveTrait(_character, "Lonely");
        }
        if (wasForlorn) {
            _character.traitContainer.RemoveTrait(_character, "Forlorn");
        }
    }
    private void OnLonely(bool wasHappy, bool wasLonely, bool wasForlorn) {
        if (!wasLonely) {
            _character.traitContainer.AddTrait(_character, "Lonely");
        }
        if (wasHappy) {
            _character.traitContainer.RemoveTrait(_character, "Happy");
        }
        if (wasForlorn) {
            _character.traitContainer.RemoveTrait(_character, "Forlorn");
        }
    }
    private void OnForlorn(bool wasHappy, bool wasLonely, bool wasForlorn) {
        if (!wasForlorn) {
            _character.traitContainer.AddTrait(_character, "Forlorn");
        }
        if (wasHappy) {
            _character.traitContainer.RemoveTrait(_character, "Happy");
        }
        if (wasLonely) {
            _character.traitContainer.RemoveTrait(_character, "Lonely");
        }
    }
    private void OnNormalHappiness(bool wasHappy, bool wasLonely, bool wasForlorn) {
        if (wasForlorn) {
            _character.traitContainer.RemoveTrait(_character, "Forlorn");
        }
        if (wasHappy) {
            _character.traitContainer.RemoveTrait(_character, "Happy");
        }
        if (wasLonely) {
            _character.traitContainer.RemoveTrait(_character, "Lonely");
        }
    }
    private void RemoveLonelyOrForlorn() {
        if (_character.traitContainer.RemoveTrait(_character, "Lonely") == false) {
            _character.traitContainer.RemoveTrait(_character, "Forlorn");
        }
    }
    private void OnHappinessAdjusted() {
        //if (isForlorn) {
        //    _character.traitContainer.RemoveTrait(_character, "Lonely");
        //    _character.traitContainer.AddTrait(_character, "Forlorn");
        //} else if (isLonely) {
        //    _character.traitContainer.RemoveTrait(_character, "Forlorn");
        //    _character.traitContainer.AddTrait(_character, "Lonely");
        //} else {
        //    RemoveLonelyOrForlorn();
        //}
        JobQueueItem suicideJob = _character.jobQueue.GetJob(JOB_TYPE.SUICIDE);
        if (happiness <= 0f && suicideJob == null) {
            //When Happiness meter is reduced to 0, the character will create a Commit Suicide Job.
            Debug.Log(GameManager.Instance.TodayLogString() + _character.name + "'s happiness is " + happiness.ToString() + ", creating suicide job");
            _character.CreateSuicideJob();
        } else if (happiness > FORLORN_UPPER_LIMIT && suicideJob != null) {
            Debug.Log(GameManager.Instance.TodayLogString() + _character.name + "'s happiness is " + happiness.ToString() + ", canceling suicide job");
            suicideJob.CancelJob(false, reason: "no longer forlorn");
        }
    }
    public void AdjustDoNotGetLonely(int amount) {
        doNotGetLonely += amount;
        doNotGetLonely = Math.Max(doNotGetLonely, 0);
    }
    public bool PlanHappinessRecoveryActions(Character character) {
        if (character.doNotDisturb || !character.canWitness) { //character.doNotDisturb > 0 || !character.canWitness
            return false;
        }
        if (this.isForlorn) {
            if (!character.jobQueue.HasJob(JOB_TYPE.HAPPINESS_RECOVERY_FORLORN)) {
                //If there is already a HUNGER_RECOVERY JOB and the character becomes Starving, replace HUNGER_RECOVERY with HUNGER_RECOVERY_STARVING only if that character is not doing the job already
                JobQueueItem happinessRecoveryJob = character.jobQueue.GetJob(JOB_TYPE.HAPPINESS_RECOVERY);
                if (happinessRecoveryJob != null) {
                    //Replace this with Hunger Recovery Starving only if the character is not doing the Hunger Recovery Job already
                    JobQueueItem currJob = character.currentJob;
                    if (currJob == happinessRecoveryJob) {
                        return false;
                    } else {
                        happinessRecoveryJob.CancelJob();
                    }
                }
                bool triggerBrokenhearted = false;
                Heartbroken heartbroken = character.traitContainer.GetNormalTrait<Trait>("Heartbroken") as Heartbroken;
                if (heartbroken != null) {
                    triggerBrokenhearted = UnityEngine.Random.Range(0, 100) < 20;
                }
                if (!triggerBrokenhearted) {
                    Hardworking hardworking = character.traitContainer.GetNormalTrait<Trait>("Hardworking") as Hardworking;
                    if (hardworking != null) {
                        bool isPlanningRecoveryProcessed = false;
                        if (hardworking.ProcessHardworkingTrait(character, ref isPlanningRecoveryProcessed)) {
                            return isPlanningRecoveryProcessed;
                        }
                    }
                    JOB_TYPE jobType = JOB_TYPE.HAPPINESS_RECOVERY_FORLORN;
                    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, new GoapEffect(GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR), _character, _character);
                    //job.SetCancelOnFail(true);
                    character.jobQueue.AddJobInQueue(job);
                } else {
                    heartbroken.TriggerBrokenhearted();
                }
                return true;
            }
        } else if (this.isLonely) {
            if (!character.jobQueue.HasJob(JOB_TYPE.HAPPINESS_RECOVERY, JOB_TYPE.HAPPINESS_RECOVERY_FORLORN)) {
                JOB_TYPE jobType = JOB_TYPE.HAPPINESS_RECOVERY;
                int chance = UnityEngine.Random.Range(0, 100);
                int value = 0;
                TIME_IN_WORDS currentTimeInWords = GameManager.GetCurrentTimeInWordsOfTick(character);
                if (currentTimeInWords == TIME_IN_WORDS.MORNING) {
                    value = 30;
                } else if (currentTimeInWords == TIME_IN_WORDS.LUNCH_TIME) {
                    value = 45;
                } else if (currentTimeInWords == TIME_IN_WORDS.AFTERNOON) {
                    value = 45;
                } else if (currentTimeInWords == TIME_IN_WORDS.EARLY_NIGHT) {
                    value = 45;
                } else if (currentTimeInWords == TIME_IN_WORDS.LATE_NIGHT) {
                    value = 30;
                }
                if (chance < value) {
                    bool triggerBrokenhearted = false;
                    Heartbroken heartbroken = character.traitContainer.GetNormalTrait<Trait>("Heartbroken") as Heartbroken;
                    if (heartbroken != null) {
                        triggerBrokenhearted = UnityEngine.Random.Range(0, 100) < 20;
                    }
                    if (!triggerBrokenhearted) {
                        Hardworking hardworking = character.traitContainer.GetNormalTrait<Trait>("Hardworking") as Hardworking;
                        if (hardworking != null) {
                            bool isPlanningRecoveryProcessed = false;
                            if (hardworking.ProcessHardworkingTrait(character, ref isPlanningRecoveryProcessed)) {
                                return isPlanningRecoveryProcessed;
                            }
                        }
                        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, new GoapEffect(GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR), _character, _character);
                        //job.SetCancelOnFail(true);
                        character.jobQueue.AddJobInQueue(job);
                    } else {
                        heartbroken.TriggerBrokenhearted();
                    }
                    return true;
                }
            }
        }
        return false;
    }
    #endregion

    #region Fullness
    public void ResetFullnessMeter() {
        bool wasHungry = isHungry;
        bool wasStarving = isStarving;
        bool wasFull = isFull;

        fullness = FULLNESS_DEFAULT;

        OnFull(wasFull, wasHungry, wasStarving);
        //RemoveHungryOrStarving();
    }
    public void AdjustFullness(float adjustment) {
        bool wasHungry = isHungry;
        bool wasStarving = isStarving;
        bool wasFull = isFull;

        fullness += adjustment;
        fullness = Mathf.Clamp(fullness, fullnessLowerBound, FULLNESS_DEFAULT);
        if(adjustment > 0) {
            _character.HPRecovery(0.02f);
        }
        if (fullness == 0f) {
            _character.traitContainer.AddTrait(_character, "Malnourished");
            OnStarving(wasFull, wasHungry, wasStarving);
            return;
        }
        if (isFull) {
            OnFull(wasFull, wasHungry, wasStarving);
        } else if (isHungry) {
            OnHungry(wasFull, wasHungry, wasStarving);
        } else if (isStarving) {
            OnStarving(wasFull, wasHungry, wasStarving);
        } else {
            OnNormalFullness(wasFull, wasHungry, wasStarving);
        }

        //if (fullness == 0) {
        //    _character.Death("starvation");
        //} else if (isStarving) {
        //    _character.traitContainer.RemoveTrait(_character, "Hungry");
        //    if (_character.traitContainer.AddTrait(_character, "Starving") && _character.traitContainer.GetNormalTrait<Trait>("Vampiric") == null) { //only characters that are not vampires will flee when they are starving
        //        Messenger.Broadcast(Signals.TRANSFER_ENGAGE_TO_FLEE_LIST, _character, "starving");
        //    }
        //} else if (isHungry) {
        //    _character.traitContainer.RemoveTrait(_character, "Starving");
        //    _character.traitContainer.AddTrait(_character, "Hungry");
        //} else {
        //    //fullness is higher than both thresholds
        //    RemoveHungryOrStarving();
        //}
    }
    public void SetFullness(float amount) {
        bool wasHungry = isHungry;
        bool wasStarving = isStarving;
        bool wasFull = isFull;

        fullness = amount;
        fullness = Mathf.Clamp(fullness, fullnessLowerBound, FULLNESS_DEFAULT);

        if (fullness == 0f) {
            _character.traitContainer.AddTrait(_character, "Malnourished");
            OnStarving(wasFull, wasHungry, wasStarving);
            return;
        }
        if (isFull) {
            OnFull(wasFull, wasHungry, wasStarving);
        } else if (isHungry) {
            OnHungry(wasFull, wasHungry, wasStarving);
        } else if (isStarving) {
            OnStarving(wasFull, wasHungry, wasStarving);
        } else {
            OnNormalFullness(wasFull, wasHungry, wasStarving);
        }

        //if (fullness == 0) {
        //    _character.Death("starvation");
        //} else if (isStarving) {
        //    _character.traitContainer.RemoveTrait(_character, "Hungry");
        //    _character.traitContainer.AddTrait(_character, "Starving");
        //} else if (isHungry) {
        //    _character.traitContainer.RemoveTrait(_character, "Starving");
        //    _character.traitContainer.AddTrait(_character, "Hungry");
        //} else {
        //    //fullness is higher than both thresholds
        //    RemoveHungryOrStarving();
        //}
    }
    private void OnFull(bool wasFull, bool wasHungry, bool wasStarving) {
        if (!wasFull) {
            _character.traitContainer.AddTrait(_character, "Full");
        }
        if (wasHungry) {
            _character.traitContainer.RemoveTrait(_character, "Hungry");
        }
        if (wasStarving) {
            _character.traitContainer.RemoveTrait(_character, "Starving");
        }
    }
    private void OnHungry(bool wasFull, bool wasHungry, bool wasStarving) {
        if (!wasHungry) {
            _character.traitContainer.AddTrait(_character, "Hungry");
        }
        if (wasFull) {
            _character.traitContainer.RemoveTrait(_character, "Full");
        }
        if (wasStarving) {
            _character.traitContainer.RemoveTrait(_character, "Starving");
        }
    }
    private void OnStarving(bool wasFull, bool wasHungry, bool wasStarving) {
        if (!wasStarving) {
            _character.traitContainer.AddTrait(_character, "Starving");
        }
        if (wasFull) {
            _character.traitContainer.RemoveTrait(_character, "Full");
        }
        if (wasHungry) {
            _character.traitContainer.RemoveTrait(_character, "Hungry");
        }
    }
    private void OnNormalFullness(bool wasFull, bool wasHungry, bool wasStarving) {
        if (wasStarving) {
            _character.traitContainer.RemoveTrait(_character, "Starving");
        }
        if (wasFull) {
            _character.traitContainer.RemoveTrait(_character, "Full");
        }
        if (wasHungry) {
            _character.traitContainer.RemoveTrait(_character, "Hungry");
        }
    }
    private void RemoveHungryOrStarving() {
        if (_character.traitContainer.RemoveTrait(_character, "Hungry") == false) {
            _character.traitContainer.RemoveTrait(_character, "Starving");
        }
    }
    public void SetFullnessForcedTick() {
        if (!hasForcedFullness) {
            if (forcedFullnessRecoveryTimeInWords == GameManager.GetCurrentTimeInWordsOfTick()) {
                //If the forced recovery job has not been done yet and the character is already on the time of day when it is supposed to be done,
                //the tick that will be assigned will be ensured that the character will not miss it
                //Example if the time of day is Afternoon, the supposed tick range for it is 145 - 204
                //So if the current tick of the game is already in 160, the range must be adjusted to 161 - 204, so as to ensure that the character will hit it
                //But if the current tick of the game is already in 204, it cannot be 204 - 204, so, it will revert back to 145 - 204 
                int newTick = GameManager.GetRandomTickFromTimeInWords(forcedFullnessRecoveryTimeInWords, GameManager.Instance.tick + 1);
                TIME_IN_WORDS timeInWords = GameManager.GetTimeInWordsOfTick(newTick);
                if (timeInWords != forcedFullnessRecoveryTimeInWords) {
                    newTick = GameManager.GetRandomTickFromTimeInWords(forcedFullnessRecoveryTimeInWords);
                }
                fullnessForcedTick = newTick;
                return;
            }
        }
        fullnessForcedTick = GameManager.GetRandomTickFromTimeInWords(forcedFullnessRecoveryTimeInWords);
    }
    public void SetFullnessForcedTick(int tick) {
        fullnessForcedTick = tick;
    }
    public void SetForcedFullnessRecoveryTimeInWords(TIME_IN_WORDS timeInWords) {
        forcedFullnessRecoveryTimeInWords = timeInWords;
    }
    public void AdjustDoNotGetHungry(int amount) {
        doNotGetHungry += amount;
        doNotGetHungry = Math.Max(doNotGetHungry, 0);
    }
    public void PlanScheduledFullnessRecovery(Character character) {
        if (!hasForcedFullness && fullnessForcedTick != 0 && GameManager.Instance.tick >= fullnessForcedTick && !character.doNotDisturb && doNotGetHungry <= 0) {
            if (!character.jobQueue.HasJob(JOB_TYPE.HUNGER_RECOVERY, JOB_TYPE.HUNGER_RECOVERY_STARVING)) {
                JOB_TYPE jobType = JOB_TYPE.HUNGER_RECOVERY;
                if (isStarving) {
                    jobType = JOB_TYPE.HUNGER_RECOVERY_STARVING;
                }
                bool triggerGrieving = false;
                Griefstricken griefstricken = character.traitContainer.GetNormalTrait<Trait>("Griefstricken") as Griefstricken;
                if (griefstricken != null) {
                    triggerGrieving = UnityEngine.Random.Range(0, 100) < 20;
                }
                if (!triggerGrieving) {
                    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, new GoapEffect(GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR), _character, _character);
                    //if (traitContainer.GetNormalTrait<Trait>("Vampiric") != null) {
                    //    job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = this }, INTERACTION_TYPE.HUNTING_TO_DRINK_BLOOD);
                    //}
                    //else if (GetNormalTrait<Trait>("Cannibal") != null) {
                    //    job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = this }, INTERACTION_TYPE.EAT_CHARACTER);
                    //}
                    //job.SetCancelOnFail(true);
                    //bool willNotProcess = _numOfWaitingForGoapThread > 0 || !IsInOwnParty() || isDefender || isWaitingForInteraction > 0 /*|| stateComponent.stateToDo != null*/;
                    character.jobQueue.AddJobInQueue(job); //, !willNotProcess
                } else {
                    griefstricken.TriggerGrieving();
                }
            }
            hasForcedFullness = true;
            SetFullnessForcedTick();
        }
    }
    public bool PlanFullnessRecoveryActions(Character character) {
        if (character.doNotDisturb || !character.canWitness) { //character.doNotDisturb > 0 || !character.canWitness
            return false;
        }
        if (this.isStarving) {
            if (!character.jobQueue.HasJob(JOB_TYPE.HUNGER_RECOVERY_STARVING)) {
                //If there is already a HUNGER_RECOVERY JOB and the character becomes Starving, replace HUNGER_RECOVERY with HUNGER_RECOVERY_STARVING only if that character is not doing the job already
                JobQueueItem hungerRecoveryJob = character.jobQueue.GetJob(JOB_TYPE.HUNGER_RECOVERY);
                if (hungerRecoveryJob != null) {
                    //Replace this with Hunger Recovery Starving only if the character is not doing the Hunger Recovery Job already
                    JobQueueItem currJob = character.currentJob;
                    if (currJob == hungerRecoveryJob) {
                        return false;
                    } else {
                        hungerRecoveryJob.CancelJob();
                    }
                }
                JOB_TYPE jobType = JOB_TYPE.HUNGER_RECOVERY_STARVING;
                bool triggerGrieving = false;
                Griefstricken griefstricken = character.traitContainer.GetNormalTrait<Trait>("Griefstricken") as Griefstricken;
                if (griefstricken != null) {
                    triggerGrieving = UnityEngine.Random.Range(0, 100) < 100;
                }
                if (!triggerGrieving) {
                    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, new GoapEffect(GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR), _character, _character);
                    //if (traitContainer.GetNormalTrait<Trait>("Vampiric") != null) {
                    //    job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = this }, INTERACTION_TYPE.HUNTING_TO_DRINK_BLOOD);
                    //}
                    //else if (GetNormalTrait<Trait>("Cannibal") != null) {
                    //    job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = this }, INTERACTION_TYPE.EAT_CHARACTER);
                    //}
                    //job.SetCancelOnFail(true);
                    character.jobQueue.AddJobInQueue(job);
                } else {
                    griefstricken.TriggerGrieving();
                }
                return true;
            }
        } else if (this.isHungry) {
            if (UnityEngine.Random.Range(0, 2) == 0 && character.traitContainer.GetNormalTrait<Trait>("Glutton") != null) {
                if (!character.jobQueue.HasJob(JOB_TYPE.HUNGER_RECOVERY)) {
                    JOB_TYPE jobType = JOB_TYPE.HUNGER_RECOVERY;
                    bool triggerGrieving = false;
                    Griefstricken griefstricken = character.traitContainer.GetNormalTrait<Trait>("Griefstricken") as Griefstricken;
                    if (griefstricken != null) {
                        triggerGrieving = UnityEngine.Random.Range(0, 100) < 20;
                    }
                    if (!triggerGrieving) {
                        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, new GoapEffect(GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR), _character, _character);
                        //if (traitContainer.GetNormalTrait<Trait>("Vampiric") != null) {
                        //    job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = this }, INTERACTION_TYPE.HUNTING_TO_DRINK_BLOOD);
                        //}
                        //else if (GetNormalTrait<Trait>("Cannibal") != null) {
                        //    job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = this }, INTERACTION_TYPE.EAT_CHARACTER);
                        //}
                        //job.SetCancelOnFail(true);
                        character.jobQueue.AddJobInQueue(job);
                    } else {
                        griefstricken.TriggerGrieving();
                    }
                    return true;
                }
            }
        }
        return false;
    }
    #endregion

    #region Comfort
    public void ResetComfortMeter() {
        bool wasUncomfortable = isUncomfortable;
        bool wasAgonizing = isAgonizing;
        bool wasRelaxed = isRelaxed;

        comfort = COMFORT_DEFAULT;

        OnRelaxed(wasRelaxed, wasUncomfortable, wasAgonizing);
    }
    public void AdjustComfort(float amount) {
        bool wasUncomfortable = isUncomfortable;
        bool wasAgonizing = isAgonizing;
        bool wasRelaxed = isRelaxed;

        comfort += amount;
        comfort = Mathf.Clamp(comfort, comfortLowerBound, COMFORT_DEFAULT);

        if (isRelaxed) {
            OnRelaxed(wasRelaxed, wasUncomfortable, wasAgonizing);
        } else if (isUncomfortable) {
            OnUncomfortable(wasRelaxed, wasUncomfortable, wasAgonizing);
        } else if (isAgonizing) {
            OnAgonizing(wasRelaxed, wasUncomfortable, wasAgonizing);
        } else {
            OnNormalComfort(wasRelaxed, wasUncomfortable, wasAgonizing);
        }
    }
    public void SetComfort(float amount) {
        bool wasUncomfortable = isUncomfortable;
        bool wasAgonizing = isAgonizing;
        bool wasRelaxed = isRelaxed;

        comfort = amount;
        comfort = Mathf.Clamp(comfort, comfortLowerBound, COMFORT_DEFAULT);

        if (isRelaxed) {
            OnRelaxed(wasRelaxed, wasUncomfortable, wasAgonizing);
        } else if (isUncomfortable) {
            OnUncomfortable(wasRelaxed, wasUncomfortable, wasAgonizing);
        } else if (isAgonizing) {
            OnAgonizing(wasRelaxed, wasUncomfortable, wasAgonizing);
        } else {
            OnNormalComfort(wasRelaxed, wasUncomfortable, wasAgonizing);
        }
    }
    private void OnRelaxed(bool wasRelaxed, bool wasUncomfortable, bool wasAgonizing) {
        if (!wasRelaxed) {
            _character.traitContainer.AddTrait(_character, "Relaxed");
        }
        if (wasUncomfortable) {
            _character.traitContainer.RemoveTrait(_character, "Uncomfortable");
        }
        if (wasAgonizing) {
            _character.traitContainer.RemoveTrait(_character, "Agonizing");
        }
    }
    private void OnUncomfortable(bool wasRelaxed, bool wasUncomfortable, bool wasAgonizing) {
        if (!wasUncomfortable) {
            _character.traitContainer.AddTrait(_character, "Uncomfortable");
        }
        if (wasRelaxed) {
            _character.traitContainer.RemoveTrait(_character, "Relaxed");
        }
        if (wasAgonizing) {
            _character.traitContainer.RemoveTrait(_character, "Agonizing");
        }
    }
    private void OnAgonizing(bool wasRelaxed, bool wasUncomfortable, bool wasAgonizing) {
        if (!wasAgonizing) {
            _character.traitContainer.AddTrait(_character, "Agonizing");
        }
        if (wasRelaxed) {
            _character.traitContainer.RemoveTrait(_character, "Relaxed");
        }
        if (wasUncomfortable) {
            _character.traitContainer.RemoveTrait(_character, "Uncomfortable");
        }
    }
    private void OnNormalComfort(bool wasRelaxed, bool wasUncomfortable, bool wasAgonizing) {
        if (wasAgonizing) {
            _character.traitContainer.RemoveTrait(_character, "Agonizing");
        }
        if (wasRelaxed) {
            _character.traitContainer.RemoveTrait(_character, "Relaxed");
        }
        if (wasUncomfortable) {
            _character.traitContainer.RemoveTrait(_character, "Uncomfortable");
        }
    }
    public void AdjustDoNotGetUncomfortable(int amount) {
        doNotGetUncomfortable += amount;
        doNotGetUncomfortable = Math.Max(doNotGetUncomfortable, 0);
    }
    #endregion

    #region Hope
    public void ResetHopeMeter() {
        bool wasGloomy = isGloomy;
        bool wasDisillusioned = isDisillusioned;
        bool wasSanguine = isSanguine;

        hope = HOPE_DEFAULT;

        OnSanguine(wasSanguine, wasGloomy, wasDisillusioned);
    }
    public void AdjustHope(float amount) {
        bool wasGloomy = isGloomy;
        bool wasDisillusioned = isDisillusioned;
        bool wasSanguine = isSanguine;

        hope += amount;
        hope = Mathf.Clamp(hope, hopeLowerBound, HOPE_DEFAULT);

        if (isSanguine) {
            OnSanguine(wasSanguine, wasGloomy, wasDisillusioned);
        } else if (isGloomy) {
            OnGloomy(wasSanguine, wasGloomy, wasDisillusioned);
        } else if (isDisillusioned) {
            OnDisillusioned(wasSanguine, wasGloomy, wasDisillusioned);
        } else {
            OnNormalHope(wasSanguine, wasGloomy, wasDisillusioned);
        }
    }
    public void SetHope(float amount) {
        bool wasGloomy = isGloomy;
        bool wasDisillusioned = isDisillusioned;
        bool wasSanguine = isSanguine;

        hope = amount;
        hope = Mathf.Clamp(hope, hopeLowerBound, HOPE_DEFAULT);

        if (isSanguine) {
            OnSanguine(wasSanguine, wasGloomy, wasDisillusioned);
        } else if (isGloomy) {
            OnGloomy(wasSanguine, wasGloomy, wasDisillusioned);
        } else if (isDisillusioned) {
            OnDisillusioned(wasSanguine, wasGloomy, wasDisillusioned);
        } else {
            OnNormalHope(wasSanguine, wasGloomy, wasDisillusioned);
        }
    }
    private void OnSanguine(bool wasSanguine, bool wasGloomy, bool wasDisillusioned) {
        if (!wasSanguine) {
            _character.traitContainer.AddTrait(_character, "Sanguine");
        }
        if (wasGloomy) {
            _character.traitContainer.RemoveTrait(_character, "Gloomy");
        }
        if (wasDisillusioned) {
            _character.traitContainer.RemoveTrait(_character, "Disillusioned");
        }
    }
    private void OnGloomy(bool wasSanguine, bool wasGloomy, bool wasDisillusioned) {
        if (!wasGloomy) {
            _character.traitContainer.AddTrait(_character, "Gloomy");
        }
        if (wasSanguine) {
            _character.traitContainer.RemoveTrait(_character, "Sanguine");
        }
        if (wasDisillusioned) {
            _character.traitContainer.RemoveTrait(_character, "Disillusioned");
        }
    }
    private void OnDisillusioned(bool wasSanguine, bool wasGloomy, bool wasDisillusioned) {
        if (!wasDisillusioned) {
            _character.traitContainer.AddTrait(_character, "Disillusioned");
        }
        if (wasSanguine) {
            _character.traitContainer.RemoveTrait(_character, "Sanguine");
        }
        if (wasGloomy) {
            _character.traitContainer.RemoveTrait(_character, "Gloomy");
        }
    }
    private void OnNormalHope(bool wasSanguine, bool wasGloomy, bool wasDisillusioned) {
        if (wasDisillusioned) {
            _character.traitContainer.RemoveTrait(_character, "Disillusioned");
        }
        if (wasSanguine) {
            _character.traitContainer.RemoveTrait(_character, "Sanguine");
        }
        if (wasGloomy) {
            _character.traitContainer.RemoveTrait(_character, "Gloomy");
        }
    }
    public void AdjustDoNotGetGloomy(int amount) {
        doNotGetGloomy += amount;
        doNotGetGloomy = Math.Max(doNotGetGloomy, 0);
    }
    #endregion

    #region Events
    public void OnCharacterLeftLocation(ILocation location) {
        // if (location == _character.homeRegion) {
        //     //character left home region
        //     AdjustDoNotGetHungry(1);
        //     AdjustDoNotGetLonely(1);
        //     AdjustDoNotGetTired(1);
        // }
    }
    public void OnCharacterArrivedAtLocation(ILocation location) {
        // if (location == _character.homeRegion) {
        //     //character arrived at home region
        //     AdjustDoNotGetHungry(-1);
        //     AdjustDoNotGetLonely(-1);
        //     AdjustDoNotGetTired(-1);
        // }
    }
    private void OnCharacterFinishedJob(Character character, GoapPlanJob job) {
        if (_character == character) {
            Debug.Log($"{GameManager.Instance.TodayLogString()}{character.name} has finished job {job.ToString()}");
            //after doing an extreme needs type job, check again if the character needs to recover more of that need.
            if (job.jobType == JOB_TYPE.HAPPINESS_RECOVERY_FORLORN) {
                PlanHappinessRecoveryActions(_character);
            } else if (job.jobType == JOB_TYPE.HUNGER_RECOVERY_STARVING) {
                PlanFullnessRecoveryActions(_character);
            } else if (job.jobType == JOB_TYPE.TIREDNESS_RECOVERY_EXHAUSTED) {
                PlanTirednessRecoveryActions(_character);
            }
        }
    }
    /// <summary>
    /// Make this character plan a starving fullness recovery job, regardless of actual
    /// fullness level. NOTE: This will also cancel any existing fullness recovery jobs
    /// </summary>
    public void TriggerFlawFullnessRecovery(Character character) {
        //if (jobQueue.HasJob(JOB_TYPE.HUNGER_RECOVERY, JOB_TYPE.HUNGER_RECOVERY_STARVING)) {
        //    jobQueue.CancelAllJobs(JOB_TYPE.HUNGER_RECOVERY, JOB_TYPE.HUNGER_RECOVERY_STARVING);
        //}
        bool triggerGrieving = false;
        Griefstricken griefstricken = character.traitContainer.GetNormalTrait<Trait>("Griefstricken") as Griefstricken;
        if (griefstricken != null) {
            triggerGrieving = UnityEngine.Random.Range(0, 100) < 20;
        }
        if (!triggerGrieving) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.TRIGGER_FLAW, new GoapEffect(GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR), _character, _character);
            //if (traitContainer.GetNormalTrait<Trait>("Vampiric") != null) {
            //    job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = this }, INTERACTION_TYPE.HUNTING_TO_DRINK_BLOOD);
            //}
            //else if (GetNormalTrait<Trait>("Cannibal") != null) {
            //    job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = this }, INTERACTION_TYPE.EAT_CHARACTER);
            //}
            character.jobQueue.AddJobInQueue(job);
        } else {
            griefstricken.TriggerGrieving();
        }
    }
    #endregion
}
