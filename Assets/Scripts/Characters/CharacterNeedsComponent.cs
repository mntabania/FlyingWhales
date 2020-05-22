using System;
using System.Globalization;
using Interrupts;
using Traits;
using UnityEngine;
using Random = System.Random;

public class CharacterNeedsComponent {

    private readonly Character _character;
    public int doNotGetHungry { get; private set; }
    public int doNotGetTired{ get; private set; }
    public int doNotGetBored{ get; private set; }
    public int doNotGetDrained { get; private set; }
    public int doNotGetDiscouraged { get; private set; }

    public bool isStarving => fullness >= 0f && fullness <= STARVING_UPPER_LIMIT;
    public bool isExhausted => tiredness >= 0f && tiredness <= EXHAUSTED_UPPER_LIMIT;
    public bool isSulking => happiness >= 0f && happiness <= SULKING_UPPER_LIMIT;
    public bool isDrained => stamina >= 0f && stamina <= DRAINED_UPPER_LIMIT;
    public bool isHopeless => hope >= 0f && hope <= HOPELESS_UPPER_LIMIT;

    public bool isHungry => fullness > STARVING_UPPER_LIMIT && fullness <= HUNGRY_UPPER_LIMIT;
    public bool isTired => tiredness > EXHAUSTED_UPPER_LIMIT && tiredness <= TIRED_UPPER_LIMIT;
    public bool isBored => happiness > SULKING_UPPER_LIMIT && happiness <= BORED_UPPER_LIMIT;
    public bool isSpent => stamina > DRAINED_UPPER_LIMIT && stamina <= SPENT_UPPER_LIMIT;
    public bool isDiscouraged => hope > HOPELESS_UPPER_LIMIT && hope <= DISCOURAGED_UPPER_LIMIT;

    public bool isFull => fullness >= FULL_LOWER_LIMIT && fullness <= 100f;
    public bool isRefreshed => tiredness >= REFRESHED_LOWER_LIMIT && tiredness <= 100f;
    public bool isEntertained => happiness >= ENTERTAINED_LOWER_LIMIT && happiness <= 100f;
    public bool isSprightly => stamina >= SPRIGHTLY_LOWER_LIMIT && stamina <= 100f;
    public bool isHopeful => hope >= HOPEFUL_LOWER_LIMIT && hope <= 100f;


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
    public const float TIRED_UPPER_LIMIT = 40f;
    public const float REFRESHED_LOWER_LIMIT = 91f;

    //Fullness
    public float fullness { get; private set; }
    public float fullnessDecreaseRate { get; private set; }
    public int fullnessForcedTick { get; private set; }
    private float fullnessLowerBound; //how low can this characters fullness go
    public const float FULLNESS_DEFAULT = 100f;
    public const float STARVING_UPPER_LIMIT = 20f;
    public const float HUNGRY_UPPER_LIMIT = 40f;
    public const float FULL_LOWER_LIMIT = 91f;

    //Happiness
    public float happiness { get; private set; }
    public float happinessDecreaseRate { get; private set; }
    private float happinessLowerBound; //how low can this characters happiness go
    public const float HAPPINESS_DEFAULT = 100f;
    public const float SULKING_UPPER_LIMIT = 20f;
    public const float BORED_UPPER_LIMIT = 40f;
    public const float ENTERTAINED_LOWER_LIMIT = 91f;

    //Comfort
    public float stamina { get; private set; }
    public float staminaDecreaseRate { get; private set; }
    private float staminaLowerBound; //how low can this characters happiness go
    public const float STAMINA_DEFAULT = 100f;
    public const float DRAINED_UPPER_LIMIT = 20f;
    public const float SPENT_UPPER_LIMIT = 40f;
    public const float SPRIGHTLY_LOWER_LIMIT = 91f;

    //Hope
    public float hope { get; private set; }
    private float hopeLowerBound; //how low can this characters happiness go
    public const float HOPE_DEFAULT = 100f;
    public const float HOPELESS_UPPER_LIMIT = 20f;
    public const float DISCOURAGED_UPPER_LIMIT = 40f;
    public const float HOPEFUL_LOWER_LIMIT = 91f;

    public bool hasForcedFullness { get; set; }
    public bool hasForcedTiredness { get; set; }
    public TIME_IN_WORDS forcedFullnessRecoveryTimeInWords { get; private set; }
    public TIME_IN_WORDS forcedTirednessRecoveryTimeInWords { get; private set; }

    public CharacterNeedsComponent(Character character) {
        this._character = character;
        SetTirednessLowerBound(0f);
        SetFullnessLowerBound(0f);
        SetHappinessLowerBound(0f);
        SetStaminaLowerBound(0f);
        SetHopeLowerBound(0f);
        SetForcedFullnessRecoveryTimeInWords(TIME_IN_WORDS.LUNCH_TIME);
        SetForcedTirednessRecoveryTimeInWords(TIME_IN_WORDS.LATE_NIGHT);
        SetFullnessForcedTick();
        SetTirednessForcedTick();
        
    }
    
    #region Initialization
    public void SubscribeToSignals() {
        Messenger.AddListener(Signals.TICK_STARTED, DecreaseNeeds);
        Messenger.AddListener<Character, GoapPlanJob>(Signals.CHARACTER_FINISHED_JOB_SUCCESSFULLY, OnCharacterFinishedJob);
    }
    public void UnsubscribeToSignals() {
        Messenger.RemoveListener(Signals.TICK_STARTED, DecreaseNeeds);
        Messenger.RemoveListener<Character, GoapPlanJob>(Signals.CHARACTER_FINISHED_JOB_SUCCESSFULLY, OnCharacterFinishedJob);
    }
    public void DailyGoapProcesses() {
        hasForcedFullness = false;
        hasForcedTiredness = false;
    }
    public void Initialize() {
        // //NOTE: These values will be randomized when this character is placed in his/her npcSettlement map.
        // ResetTirednessMeter();
        // ResetFullnessMeter();
        // ResetHappinessMeter();
        // ResetComfortMeter();
        // ResetHopeMeter();
    }
    public void InitialCharacterPlacement() {
        ResetHopeMeter();
        SetTiredness(UnityEngine.Random.Range(50, 101));
        SetFullness(UnityEngine.Random.Range(50, 101));
        SetHappiness(UnityEngine.Random.Range(50, 101));
        SetStamina(UnityEngine.Random.Range(50, 101));
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

    public void CheckExtremeNeeds(Interrupt interruptThatTriggered = null) {
        if (HasNeeds() == false) { return; }
        string summary = $"{GameManager.Instance.TodayLogString()}{_character.name} will check his/her needs.";
        if (isStarving && (interruptThatTriggered == null || interruptThatTriggered.interrupt != INTERRUPT.Grieving)) {
            summary += $"\n{_character.name} is starving. Planning fullness recovery actions...";
            PlanFullnessRecoveryActions(_character);
        }
        if (isExhausted && (interruptThatTriggered == null || interruptThatTriggered.interrupt != INTERRUPT.Feeling_Spooked)) {
            summary += $"\n{_character.name} is exhausted. Planning tiredness recovery actions...";
            PlanTirednessRecoveryActions(_character);
        }
        if (isSulking && (interruptThatTriggered == null || interruptThatTriggered.interrupt != INTERRUPT.Feeling_Brokenhearted)) {
            summary += $"\n{_character.name} is sulking. Planning happiness recovery actions...";
            PlanHappinessRecoveryActions(_character);
        }
        Debug.Log(summary);
    }

    public bool HasNeeds() {
        return _character.race != RACE.SKELETON && _character.characterClass.className != "Zombie" && !_character.returnedToLife && _character.minion == null && !(_character is Summon)
            && !_character.traitContainer.HasTrait("Fervor")
            /*&& _character.isAtHomeRegion && _character.homeNpcSettlement != null*/; //Characters living on a region without a npcSettlement must not decrease needs
    }
    public void DecreaseNeeds() {
        if (HasNeeds() == false) {
            return;
        }
        if (doNotGetHungry <= 0) {
            AdjustFullness(-(EditableValuesManager.Instance.baseFullnessDecreaseRate + fullnessDecreaseRate));
        }
        if (doNotGetTired <= 0) {
            AdjustTiredness(-(EditableValuesManager.Instance.baseTirednessDecreaseRate + tirednessDecreaseRate));
        }
        if (doNotGetBored <= 0) {
            AdjustHappiness(-(EditableValuesManager.Instance.baseHappinessDecreaseRate + happinessDecreaseRate));
        }
        if (doNotGetDrained <= 0) {
            if(_character.marker && _character.marker.isMoving) {
                if (_character.movementComponent.isRunning) {
                    AdjustStamina(-(EditableValuesManager.Instance.baseStaminaDecreaseRate + staminaDecreaseRate));
                } else {
                    AdjustStamina(2f);
                }
            } else {
                AdjustStamina(10f);
            }
        }
    }
    public string GetNeedsSummary() {
        string summary = $"Fullness: {fullness.ToString(CultureInfo.InvariantCulture)}/{FULLNESS_DEFAULT.ToString(CultureInfo.InvariantCulture)}";
        summary += $"\nTiredness: {tiredness.ToString(CultureInfo.InvariantCulture)}/{TIREDNESS_DEFAULT.ToString(CultureInfo.InvariantCulture)}";
        summary += $"\nHappiness: {happiness.ToString(CultureInfo.InvariantCulture)}/{HAPPINESS_DEFAULT.ToString(CultureInfo.InvariantCulture)}";
        summary += $"\nStamina: {stamina.ToString(CultureInfo.InvariantCulture)}/{STAMINA_DEFAULT.ToString(CultureInfo.InvariantCulture)}";
        summary += $"\nHope: {hope.ToString(CultureInfo.InvariantCulture)}/{HOPE_DEFAULT.ToString(CultureInfo.InvariantCulture)}";
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
    public void AdjustStaminaDecreaseRate(float amount) {
        staminaDecreaseRate += amount;
    }
    private void SetTirednessLowerBound(float amount) {
        tirednessLowerBound = amount;
    }
    private void SetFullnessLowerBound(float amount) {
        fullnessLowerBound = amount;
    }
    private void SetHappinessLowerBound(float amount) {
        happinessLowerBound = amount;
    }
    private void SetStaminaLowerBound(float amount) {
        staminaLowerBound = amount;
    }
    private void SetHopeLowerBound(float amount) {
        hopeLowerBound = amount;
    }

    #region Tiredness
    public void ResetTirednessMeter() {
        bool wasTired = isTired;
        bool wasExhausted = isExhausted;
        bool wasRefreshed = isRefreshed;

        tiredness = TIREDNESS_DEFAULT;
        //RemoveTiredOrExhausted();
        OnRefreshed(wasRefreshed, wasTired, wasExhausted);
    }
    public void AdjustTiredness(float adjustment) {
        if(adjustment < 0 && _character.traitContainer.HasTrait("Vampiric")) {
            _character.logComponent.PrintLogIfActive("Trying to reduce energy meter but character is a vampire, will ignore reduction.");
            return;
        }
        bool wasTired = isTired;
        bool wasExhausted = isExhausted;
        bool wasRefreshed = isRefreshed;
        bool wasUnconscious = tiredness == 0f;

        tiredness += adjustment;
        tiredness = Mathf.Clamp(tiredness, tirednessLowerBound, TIREDNESS_DEFAULT);
        if (tiredness == 0f) {
            if (!wasUnconscious) {
                _character.traitContainer.AddTrait(_character, "Unconscious");
            }
            OnExhausted(wasRefreshed, wasTired, wasExhausted);
            return;
        }
        if (isRefreshed) {
            OnRefreshed(wasRefreshed, wasTired, wasExhausted);
        } else if (isTired) {
            OnTired(wasRefreshed, wasTired, wasExhausted);
        } else if (isExhausted) {
            OnExhausted(wasRefreshed, wasTired, wasExhausted);
        } else {
            OnNormalEnergy(wasRefreshed, wasTired, wasExhausted);
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
        bool wasRefreshed = isRefreshed;
        bool wasUnconscious = tiredness == 0f;

        tiredness = amount;
        tiredness = Mathf.Clamp(tiredness, tirednessLowerBound, TIREDNESS_DEFAULT);
        if (tiredness == 0f) {
            if (!wasUnconscious) {
                _character.traitContainer.AddTrait(_character, "Unconscious");
            }
            OnExhausted(wasRefreshed, wasTired, wasExhausted);
            return;
        }
        if (isRefreshed) {
            OnRefreshed(wasRefreshed, wasTired, wasExhausted);
        } else if (isTired) {
            OnTired(wasRefreshed, wasTired, wasExhausted);
        } else if (isExhausted) {
            OnExhausted(wasRefreshed, wasTired, wasExhausted);
        } else {
            OnNormalEnergy(wasRefreshed, wasTired, wasExhausted);
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
    private void OnRefreshed(bool wasRefreshed, bool wasTired, bool wasExhausted) {
        if (!wasRefreshed) {
            _character.traitContainer.AddTrait(_character, "Refreshed");
        }
        if (wasExhausted) {
            _character.traitContainer.RemoveTrait(_character, "Exhausted");
        }
        if (wasTired) {
            _character.traitContainer.RemoveTrait(_character, "Tired");
        }
    }
    private void OnTired(bool wasRefreshed, bool wasTired, bool wasExhausted) {
        if (!wasTired) {
            _character.traitContainer.AddTrait(_character, "Tired");
        }
        if (wasExhausted) {
            _character.traitContainer.RemoveTrait(_character, "Exhausted");
        }
        if (wasRefreshed) {
            _character.traitContainer.RemoveTrait(_character, "Refreshed");
        }
    }
    private void OnExhausted(bool wasRefreshed, bool wasTired, bool wasExhausted) {
        if (!wasExhausted) {
            _character.traitContainer.AddTrait(_character, "Exhausted");
            //Messenger.Broadcast<Character, string>(Signals.TRANSFER_ENGAGE_TO_FLEE_LIST, _character, "exhausted");
        }
        if (wasTired) {
            _character.traitContainer.RemoveTrait(_character, "Tired");
        }
        if (wasRefreshed) {
            _character.traitContainer.RemoveTrait(_character, "Refreshed");
        }
    }
    private void OnNormalEnergy(bool wasRefreshed, bool wasTired, bool wasExhausted) {
        if (wasExhausted) {
            _character.traitContainer.RemoveTrait(_character, "Exhausted");
        }
        if (wasTired) {
            _character.traitContainer.RemoveTrait(_character, "Tired");
        }
        if (wasRefreshed) {
            _character.traitContainer.RemoveTrait(_character, "Refreshed");
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
                int newTick = GameManager.GetRandomTickFromTimeInWords(forcedTirednessRecoveryTimeInWords, GameManager.Instance.Today().tick + 1);
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
        if (!character.canPerform) { //character.doNotDisturb > 0 || !character.canWitness
            return false;
        }
        if (this.isExhausted) {
            if (!character.jobQueue.HasJob(JOB_TYPE.ENERGY_RECOVERY_URGENT)) {
                //If there is already a TIREDNESS_RECOVERY JOB and the character becomes Exhausted, replace TIREDNESS_RECOVERY with TIREDNESS_RECOVERY_STARVING only if that character is not doing the job already
                JobQueueItem tirednessRecoveryJob = character.jobQueue.GetJob(JOB_TYPE.ENERGY_RECOVERY_NORMAL);
                if (tirednessRecoveryJob != null) {
                    //Replace this with Tiredness Recovery Exhausted only if the character is not doing the Tiredness Recovery Job already
                    JobQueueItem currJob = character.currentJob;
                    if (currJob == tirednessRecoveryJob) {
                        return false;
                    } else {
                        tirednessRecoveryJob.CancelJob();
                    }
                }
                JOB_TYPE jobType = JOB_TYPE.ENERGY_RECOVERY_URGENT;
                bool triggerSpooked = false;
                Spooked spooked = character.traitContainer.GetNormalTrait<Spooked>("Spooked");
                if (spooked != null) {
                    triggerSpooked = UnityEngine.Random.Range(0, 100) < (25 * character.traitContainer.stacks[spooked.name]);
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
    public bool PlanExtremeTirednessRecoveryActionsForCannotPerform() {
        if (!_character.jobQueue.HasJob(JOB_TYPE.ENERGY_RECOVERY_URGENT)) {
            //If there is already a TIREDNESS_RECOVERY JOB and the character becomes Exhausted, replace TIREDNESS_RECOVERY with TIREDNESS_RECOVERY_STARVING only if that character is not doing the job already
            JobQueueItem tirednessRecoveryJob = _character.jobQueue.GetJob(JOB_TYPE.ENERGY_RECOVERY_NORMAL);
            if (tirednessRecoveryJob != null) {
                //Replace this with Tiredness Recovery Exhausted only if the character is not doing the Tiredness Recovery Job already
                JobQueueItem currJob = _character.currentJob;
                if (currJob == tirednessRecoveryJob) {
                    return false;
                } else {
                    tirednessRecoveryJob.CancelJob();
                }
            }
            JOB_TYPE jobType = JOB_TYPE.ENERGY_RECOVERY_URGENT;
            bool triggerSpooked = false;
            Spooked spooked = _character.traitContainer.GetNormalTrait<Spooked>("Spooked");
            if (spooked != null) {
                triggerSpooked = UnityEngine.Random.Range(0, 100) < (25 * _character.traitContainer.stacks[spooked.name]);
            }
            if (!triggerSpooked) {
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.SLEEP_OUTSIDE, _character, _character);
                //job.SetCancelOnFail(true);
                _character.jobQueue.AddJobInQueue(job);
            } else {
                spooked.TriggerFeelingSpooked();
            }
            return true;
        }
        return false;
    }
    public void PlanScheduledTirednessRecovery(Character character) {
        if (!hasForcedTiredness && tirednessForcedTick != 0 && GameManager.Instance.Today().tick >= tirednessForcedTick && character.canPerform && doNotGetTired <= 0) {
            if (!character.jobQueue.HasJob(JOB_TYPE.ENERGY_RECOVERY_NORMAL, JOB_TYPE.ENERGY_RECOVERY_URGENT)) {
                JOB_TYPE jobType = JOB_TYPE.ENERGY_RECOVERY_NORMAL;
                if (isExhausted) {
                    jobType = JOB_TYPE.ENERGY_RECOVERY_URGENT;
                }

                bool triggerSpooked = false;
                Spooked spooked = character.traitContainer.GetNormalTrait<Spooked>("Spooked");
                if (spooked != null) {
                    triggerSpooked = UnityEngine.Random.Range(0, 100) < (25 * character.traitContainer.stacks[spooked.name]);
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
        else if ((hasCancelledSleepSchedule || currentSleepTicks < CharacterManager.Instance.defaultSleepTicks) && character.canPerform) {
            if (!character.jobQueue.HasJob(JOB_TYPE.ENERGY_RECOVERY_NORMAL, JOB_TYPE.ENERGY_RECOVERY_URGENT)) {
                JOB_TYPE jobType = JOB_TYPE.ENERGY_RECOVERY_NORMAL;
                if (isExhausted) {
                    jobType = JOB_TYPE.ENERGY_RECOVERY_URGENT;
                }
                bool triggerSpooked = false;
                Spooked spooked = character.traitContainer.GetNormalTrait<Spooked>("Spooked");
                if (spooked != null) {
                    triggerSpooked = UnityEngine.Random.Range(0, 100) < (25 * character.traitContainer.stacks[spooked.name]);
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
        if (_character.traitContainer.HasTrait("Psychopath")) {
            //Psychopath's Happiness is always fixed at 50 and is not changed by anything.
            return;
        }
        bool wasBored = isBored;
        bool wasSulking = isSulking;
        bool wasEntertained = isEntertained;

        happiness = HAPPINESS_DEFAULT;

        OnEntertained(wasEntertained, wasBored, wasSulking);
        //OnHappinessAdjusted();
    }
    public void AdjustHappiness(float adjustment) {
        if (_character.traitContainer.HasTrait("Psychopath")) {
            //Psychopath's Happiness is always fixed at 50 and is not changed by anything.
            return;
        }
        bool wasBored = isBored;
        bool wasSulking = isSulking;
        bool wasEntertained = isEntertained;

        happiness += adjustment;
        happiness = Mathf.Clamp(happiness, happinessLowerBound, HAPPINESS_DEFAULT);

        if (isEntertained) {
            OnEntertained(wasEntertained, wasBored, wasSulking);
        } else if (isBored) {
            OnBored(wasEntertained, wasBored, wasSulking);
        } else if (isSulking) {
            OnSulking(wasEntertained, wasBored, wasSulking);
        } else {
            OnNormalHappiness(wasEntertained, wasBored, wasSulking);
        }
        //OnHappinessAdjusted();
    }
    public void SetHappiness(float amount) {
        if (_character.traitContainer.HasTrait("Psychopath")) {
            //Psychopath's Happiness is always fixed at 50 and is not changed by anything.
            return;
        }
        bool wasBored = isBored;
        bool wasSulking = isSulking;
        bool wasEntertained = isEntertained;

        happiness = amount;
        happiness = Mathf.Clamp(happiness, happinessLowerBound, HAPPINESS_DEFAULT);

        if (isEntertained) {
            OnEntertained(wasEntertained, wasBored, wasSulking);
        } else if (isBored) {
            OnBored(wasEntertained, wasBored, wasSulking);
        } else if (isSulking) {
            OnSulking(wasEntertained, wasBored, wasSulking);
        } else {
            OnNormalHappiness(wasEntertained, wasBored, wasSulking);
        }
        //OnHappinessAdjusted();
    }
    private void OnEntertained(bool wasEntertained, bool wasBored, bool wasSulking) {
        if (!wasEntertained) {
            _character.traitContainer.AddTrait(_character, "Entertained");
        }
        if (wasBored) {
            _character.traitContainer.RemoveTrait(_character, "Bored");
        }
        if (wasSulking) {
            _character.traitContainer.RemoveTrait(_character, "Sulking");
        }
    }
    private void OnBored(bool wasEntertained, bool wasBored, bool wasSulking) {
        if (!wasBored) {
            _character.traitContainer.AddTrait(_character, "Bored");
        }
        if (wasEntertained) {
            _character.traitContainer.RemoveTrait(_character, "Entertained");
        }
        if (wasSulking) {
            _character.traitContainer.RemoveTrait(_character, "Sulking");
        }
    }
    private void OnSulking(bool wasEntertained, bool wasBored, bool wasSulking) {
        if (!wasSulking) {
            _character.traitContainer.AddTrait(_character, "Sulking");
        }
        if (wasEntertained) {
            _character.traitContainer.RemoveTrait(_character, "Entertained");
        }
        if (wasBored) {
            _character.traitContainer.RemoveTrait(_character, "Bored");
        }
    }
    private void OnNormalHappiness(bool wasEntertained, bool wasBored, bool wasSulking) {
        if (wasSulking) {
            _character.traitContainer.RemoveTrait(_character, "Sulking");
        }
        if (wasEntertained) {
            _character.traitContainer.RemoveTrait(_character, "Entertained");
        }
        if (wasBored) {
            _character.traitContainer.RemoveTrait(_character, "Bored");
        }
    }
    private void RemoveBoredOrSulking() {
        if (_character.traitContainer.RemoveTrait(_character, "Bored") == false) {
            _character.traitContainer.RemoveTrait(_character, "Sulking");
        }
    }
    public void AdjustDoNotGetBored(int amount) {
        doNotGetBored += amount;
        doNotGetBored = Math.Max(doNotGetBored, 0);
    }
    public bool PlanHappinessRecoveryActions(Character character) {
        if (!character.canPerform) { //character.doNotDisturb > 0 || !character.canWitness
            return false;
        }
        if (this.isBored || this.isSulking) {
            if (!character.jobQueue.HasJob(JOB_TYPE.HAPPINESS_RECOVERY)) {
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
                if (chance < value || isSulking) {
                    bool triggerBrokenhearted = false;
                    Heartbroken heartbroken = character.traitContainer.GetNormalTrait<Heartbroken>("Heartbroken");
                    if (heartbroken != null) {
                        triggerBrokenhearted = UnityEngine.Random.Range(0, 100) < (25 * character.traitContainer.stacks[heartbroken.name]);
                    }
                    if (!triggerBrokenhearted) {
                        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HAPPINESS_RECOVERY, new GoapEffect(GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR), _character, _character);
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
        bool wasMalnourished = fullness == 0f;

        fullness = FULLNESS_DEFAULT;

        OnFull(wasFull, wasHungry, wasStarving, wasMalnourished);
        //RemoveHungryOrStarving();
    }
    public void AdjustFullness(float adjustment) {
        bool wasHungry = isHungry;
        bool wasStarving = isStarving;
        bool wasFull = isFull;
        bool wasMalnourished = fullness == 0f;

        fullness += adjustment;
        fullness = Mathf.Clamp(fullness, fullnessLowerBound, FULLNESS_DEFAULT);
        if(adjustment > 0) {
            _character.HPRecovery(0.02f);
        }
        if (fullness == 0f) {
            if (!wasMalnourished) {
                _character.traitContainer.AddTrait(_character, "Malnourished");
            }
            OnStarving(wasFull, wasHungry, wasStarving);
            return;
        }
        if (isFull) {
            OnFull(wasFull, wasHungry, wasStarving, wasMalnourished);
        } else if (isHungry) {
            OnHungry(wasFull, wasHungry, wasStarving);
        } else if (isStarving) {
            OnStarving(wasFull, wasHungry, wasStarving);
        } else {
            OnNormalFullness(wasFull, wasHungry, wasStarving, wasMalnourished);
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
        bool wasMalnourished = fullness == 0f;

        fullness = amount;
        fullness = Mathf.Clamp(fullness, fullnessLowerBound, FULLNESS_DEFAULT);

        if (fullness == 0f) {
            if (!wasMalnourished) {
                _character.traitContainer.AddTrait(_character, "Malnourished");
            }
            OnStarving(wasFull, wasHungry, wasStarving);
            return;
        }
        if (isFull) {
            OnFull(wasFull, wasHungry, wasStarving, wasMalnourished);
        } else if (isHungry) {
            OnHungry(wasFull, wasHungry, wasStarving);
        } else if (isStarving) {
            OnStarving(wasFull, wasHungry, wasStarving);
        } else {
            OnNormalFullness(wasFull, wasHungry, wasStarving, wasMalnourished);
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
    private void OnFull(bool wasFull, bool wasHungry, bool wasStarving, bool wasMalnourished) {
        if (!wasFull) {
            _character.traitContainer.AddTrait(_character, "Full");
        }
        if (wasHungry) {
            _character.traitContainer.RemoveTrait(_character, "Hungry");
        }
        if (wasStarving) {
            _character.traitContainer.RemoveTrait(_character, "Starving");
        }
        _character.traitContainer.RemoveTrait(_character, "Malnourished");
        // if (wasMalnourished) {
        //     _character.traitContainer.RemoveTrait(_character, "Malnourished");
        // }
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
    private void OnNormalFullness(bool wasFull, bool wasHungry, bool wasStarving, bool wasMalnourished) {
        if (wasStarving) {
            _character.traitContainer.RemoveTrait(_character, "Starving");
        }
        if (wasFull) {
            _character.traitContainer.RemoveTrait(_character, "Full");
        }
        if (wasHungry) {
            _character.traitContainer.RemoveTrait(_character, "Hungry");
        }
        _character.traitContainer.RemoveTrait(_character, "Malnourished");
        // if (wasMalnourished) {
        //     _character.traitContainer.RemoveTrait(_character, "Malnourished");
        // }
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
                int newTick = GameManager.GetRandomTickFromTimeInWords(forcedFullnessRecoveryTimeInWords, GameManager.Instance.Today().tick + 1);
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
        if (!hasForcedFullness && fullnessForcedTick != 0 && GameManager.Instance.Today().tick >= fullnessForcedTick && character.canPerform && doNotGetHungry <= 0) {
            if (!character.jobQueue.HasJob(JOB_TYPE.FULLNESS_RECOVERY_NORMAL, JOB_TYPE.FULLNESS_RECOVERY_URGENT)) {
                JOB_TYPE jobType = JOB_TYPE.FULLNESS_RECOVERY_NORMAL;
                if (isStarving) {
                    jobType = JOB_TYPE.FULLNESS_RECOVERY_URGENT;
                }
                bool triggerGrieving = false;
                Griefstricken griefstricken = character.traitContainer.GetNormalTrait<Griefstricken>("Griefstricken");
                if (griefstricken != null) {
                    triggerGrieving = UnityEngine.Random.Range(0, 100) < (25 * character.traitContainer.stacks[griefstricken.name]);
                }
                if (!triggerGrieving) {
                    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, new GoapEffect(GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR), _character, _character);
                    job.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[] { 12 });
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
        if (!character.canPerform) { //character.doNotDisturb > 0 || !character.canWitness
            return false;
        }
        if (this.isStarving) {
            if (!character.jobQueue.HasJob(JOB_TYPE.FULLNESS_RECOVERY_URGENT)) {
                //If there is already a HUNGER_RECOVERY JOB and the character becomes Starving, replace HUNGER_RECOVERY with HUNGER_RECOVERY_STARVING only if that character is not doing the job already
                JobQueueItem hungerRecoveryJob = character.jobQueue.GetJob(JOB_TYPE.FULLNESS_RECOVERY_NORMAL);
                if (hungerRecoveryJob != null) {
                    //Replace this with Hunger Recovery Starving only if the character is not doing the Hunger Recovery Job already
                    JobQueueItem currJob = character.currentJob;
                    if (currJob == hungerRecoveryJob) {
                        return false;
                    } else {
                        hungerRecoveryJob.CancelJob();
                    }
                }
                JOB_TYPE jobType = JOB_TYPE.FULLNESS_RECOVERY_URGENT;
                bool triggerGrieving = false;
                Griefstricken griefstricken = character.traitContainer.GetNormalTrait<Griefstricken>("Griefstricken");
                if (griefstricken != null) {
                    triggerGrieving = UnityEngine.Random.Range(0, 100) < (25 * character.traitContainer.stacks[griefstricken.name]);
                }
                if (!triggerGrieving) {
                    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, new GoapEffect(GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR), _character, _character);
                    job.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[] { 12 });
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
        //else if (this.isHungry) {
        //    if (UnityEngine.Random.Range(0, 2) == 0 && character.traitContainer.GetNormalTrait<Trait>("Glutton") != null) {
        //        if (!character.jobQueue.HasJob(JOB_TYPE.HUNGER_RECOVERY)) {
        //            JOB_TYPE jobType = JOB_TYPE.HUNGER_RECOVERY;
        //            bool triggerGrieving = false;
        //            Griefstricken griefstricken = character.traitContainer.GetNormalTrait<Trait>("Griefstricken") as Griefstricken;
        //            if (griefstricken != null) {
        //                triggerGrieving = UnityEngine.Random.Range(0, 100) < 20;
        //            }
        //            if (!triggerGrieving) {
        //                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, new GoapEffect(GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR), _character, _character);
        //                //if (traitContainer.GetNormalTrait<Trait>("Vampiric") != null) {
        //                //    job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = this }, INTERACTION_TYPE.HUNTING_TO_DRINK_BLOOD);
        //                //}
        //                //else if (GetNormalTrait<Trait>("Cannibal") != null) {
        //                //    job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = this }, INTERACTION_TYPE.EAT_CHARACTER);
        //                //}
        //                //job.SetCancelOnFail(true);
        //                character.jobQueue.AddJobInQueue(job);
        //            } else {
        //                griefstricken.TriggerGrieving();
        //            }
        //            return true;
        //        }
        //    }
        //}
        return false;
    }
    public void PlanFullnessRecoveryNormal() {
        if (!_character.jobQueue.HasJob(JOB_TYPE.FULLNESS_RECOVERY_NORMAL)) {
            JOB_TYPE jobType = JOB_TYPE.FULLNESS_RECOVERY_NORMAL;
            bool triggerGrieving = false;
            Griefstricken griefstricken = _character.traitContainer.GetNormalTrait<Griefstricken>("Griefstricken");
            if (griefstricken != null) {
                triggerGrieving = UnityEngine.Random.Range(0, 100) < (25 * _character.traitContainer.stacks[griefstricken.name]);
            }
            if (!triggerGrieving) {
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, new GoapEffect(GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR), _character, _character);
                job.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[] { 12 });
                //if (traitContainer.GetNormalTrait<Trait>("Vampiric") != null) {
                //    job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = this }, INTERACTION_TYPE.HUNTING_TO_DRINK_BLOOD);
                //}
                //else if (GetNormalTrait<Trait>("Cannibal") != null) {
                //    job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = this }, INTERACTION_TYPE.EAT_CHARACTER);
                //}
                //job.SetCancelOnFail(true);
                _character.jobQueue.AddJobInQueue(job);
            } else {
                griefstricken.TriggerGrieving();
            }
        }
    }

    #endregion

    #region Stamina
    public void ResetStaminaMeter() {
        bool wasSpent = isSpent;
        bool wasDrained = isDrained;
        bool wasSprightly = isSprightly;

        stamina = STAMINA_DEFAULT;

        OnSprightly(wasSprightly, wasSpent, wasDrained);
    }
    public void AdjustStamina(float amount) {
        bool wasSpent = isSpent;
        bool wassDrained = isDrained;
        bool wasSprightly = isSprightly;

        stamina += amount;
        stamina = Mathf.Clamp(stamina, staminaLowerBound, STAMINA_DEFAULT);

        if (isSprightly) {
            OnSprightly(wasSprightly, wasSpent, wassDrained);
        } else if (isSpent) {
            OnSpent(wasSprightly, wasSpent, wassDrained);
        } else if (isDrained) {
            OnDrained(wasSprightly, wasSpent, wassDrained);
        } else {
            OnNormalStamina(wasSprightly, wasSpent, wassDrained);
        }
    }
    public void SetStamina(float amount) {
        bool wasSpent = isSpent;
        bool wasDrained = isDrained;
        bool wasSprightly = isSprightly;

        stamina = amount;
        stamina = Mathf.Clamp(stamina, staminaLowerBound, STAMINA_DEFAULT);

        if (isSprightly) {
            OnSprightly(wasSprightly, wasSpent, wasDrained);
        } else if (isSpent) {
            OnSpent(wasSprightly, wasSpent, wasDrained);
        } else if (isDrained) {
            OnDrained(wasSprightly, wasSpent, wasDrained);
        } else {
            OnNormalStamina(wasSprightly, wasSpent, wasDrained);
        }
    }
    private void OnSprightly(bool wasSprightly, bool wasSpent, bool wasDrained) {
        if (!wasSprightly) {
            _character.traitContainer.AddTrait(_character, "Sprightly");
            _character.movementComponent.SetNoRunExceptCombat(false);
            _character.movementComponent.SetNoRunWithoutException(false);
        }
        if (wasSpent) {
            _character.traitContainer.RemoveTrait(_character, "Spent");
        }
        if (wasDrained) {
            _character.traitContainer.RemoveTrait(_character, "Drained");
        }
    }
    private void OnSpent(bool wasSprightly, bool wasSpent, bool wasDrained) {
        if (!wasSpent) {
            _character.traitContainer.AddTrait(_character, "Spent");
            _character.movementComponent.SetNoRunExceptCombat(true);
            _character.movementComponent.SetNoRunWithoutException(false);
        }
        if (wasSprightly) {
            _character.traitContainer.RemoveTrait(_character, "Sprightly");
        }
        if (wasDrained) {
            _character.traitContainer.RemoveTrait(_character, "Drained");
        }
    }
    private void OnDrained(bool wasSprightly, bool wasSpent, bool wasDrained) {
        if (!wasDrained) {
            _character.traitContainer.AddTrait(_character, "Drained");
            _character.movementComponent.SetNoRunExceptCombat(false);
            _character.movementComponent.SetNoRunWithoutException(true);
        }
        if (wasSprightly) {
            _character.traitContainer.RemoveTrait(_character, "Sprightly");
        }
        if (wasSpent) {
            _character.traitContainer.RemoveTrait(_character, "Spent");
        }
    }
    private void OnNormalStamina(bool wasSprightly, bool wasSpent, bool wasDrained) {
        if (wasDrained) {
            _character.traitContainer.RemoveTrait(_character, "Drained");
        }
        if (wasSprightly) {
            _character.traitContainer.RemoveTrait(_character, "Sprightly");
        }
        if (wasSpent) {
            _character.traitContainer.RemoveTrait(_character, "Spent");
        }
    }
    public void AdjustDoNotGetDrained(int amount) {
        doNotGetDrained += amount;
        doNotGetDrained = Math.Max(doNotGetDrained, 0);
    }
    #endregion

    #region Hope
    public void ResetHopeMeter() {
        bool wasDiscouraged = isDiscouraged;
        bool wasHopeless = isHopeless;
        bool wasHopeful = isHopeful;

        hope = HOPE_DEFAULT;

        OnHopeful(wasHopeful, wasDiscouraged, wasHopeless);
    }
    public void AdjustHope(float amount) {
        bool wasDiscouraged = isDiscouraged;
        bool wasHopeless = isHopeless;
        bool wasHopeful = isHopeful;

        hope += amount;
        hope = Mathf.Clamp(hope, hopeLowerBound, HOPE_DEFAULT);

        if (isHopeful) {
            OnHopeful(wasHopeful, wasDiscouraged, wasHopeless);
        } else if (isDiscouraged) {
            OnDiscouraged(wasHopeful, wasDiscouraged, wasHopeless);
        } else if (isHopeless) {
            OnHopeless(wasHopeful, wasDiscouraged, wasHopeless);
        } else {
            OnNormalHope(wasHopeful, wasDiscouraged, wasHopeless);
        }
    }
    public void SetHope(float amount) {
        bool wasDiscouraged = isDiscouraged;
        bool wasHopeless = isHopeless;
        bool wasHopeful = isHopeful;

        hope = amount;
        hope = Mathf.Clamp(hope, hopeLowerBound, HOPE_DEFAULT);

        if (isHopeful) {
            OnHopeful(wasHopeful, wasDiscouraged, wasHopeless);
        } else if (isDiscouraged) {
            OnDiscouraged(wasHopeful, wasDiscouraged, wasHopeless);
        } else if (isHopeless) {
            OnHopeless(wasHopeful, wasDiscouraged, wasHopeless);
        } else {
            OnNormalHope(wasHopeful, wasDiscouraged, wasHopeless);
        }
    }
    private void OnHopeful(bool wasHopeful, bool wasDiscouraged, bool wasHopeless) {
        if (!wasHopeful) {
            _character.traitContainer.AddTrait(_character, "Hopeful");
        }
        if (wasDiscouraged) {
            _character.traitContainer.RemoveTrait(_character, "Discouraged");
        }
        if (wasHopeless) {
            _character.traitContainer.RemoveTrait(_character, "Hopeless");
        }
    }
    private void OnDiscouraged(bool wasHopeful, bool wasDiscouraged, bool wasHopeless) {
        if (!wasDiscouraged) {
            _character.traitContainer.AddTrait(_character, "Discouraged");
        }
        if (wasHopeful) {
            _character.traitContainer.RemoveTrait(_character, "Hopeful");
        }
        if (wasHopeless) {
            _character.traitContainer.RemoveTrait(_character, "Hopeless");
        }
    }
    private void OnHopeless(bool wasHopeful, bool wasDiscouraged, bool wasHopeless) {
        if (!wasHopeless) {
            _character.traitContainer.AddTrait(_character, "Hopeless");
        }
        if (wasHopeful) {
            _character.traitContainer.RemoveTrait(_character, "Hopeful");
        }
        if (wasDiscouraged) {
            _character.traitContainer.RemoveTrait(_character, "Discouraged");
        }
    }
    private void OnNormalHope(bool wasHopeful, bool wasDiscouraged, bool wasHopeless) {
        if (wasHopeless) {
            _character.traitContainer.RemoveTrait(_character, "Hopeless");
        }
        if (wasHopeful) {
            _character.traitContainer.RemoveTrait(_character, "Hopeful");
        }
        if (wasDiscouraged) {
            _character.traitContainer.RemoveTrait(_character, "Discouraged");
        }
    }
    public void AdjustDoNotGetDiscouraged(int amount) {
        doNotGetDiscouraged += amount;
        doNotGetDiscouraged = Math.Max(doNotGetDiscouraged, 0);
    }
    #endregion

    #region Events
    public void OnCharacterLeftLocation(Region location) {
        // if (location == _character.homeRegion) {
        //     //character left home region
        //     AdjustDoNotGetHungry(1);
        //     AdjustDoNotGetBored(1);
        //     AdjustDoNotGetTired(1);
        // }
    }
    public void OnCharacterArrivedAtLocation(Region location) {
        // if (location == _character.homeRegion) {
        //     //character arrived at home region
        //     AdjustDoNotGetHungry(-1);
        //     AdjustDoNotGetBored(-1);
        //     AdjustDoNotGetTired(-1);
        // }
    }
    private void OnCharacterFinishedJob(Character character, GoapPlanJob job) {
        if (_character == character) {
            Debug.Log($"{GameManager.Instance.TodayLogString()}{character.name} has finished job {job.ToString()}");
            //after doing an extreme needs type job, check again if the character needs to recover more of that need.
            if (job.jobType == JOB_TYPE.FULLNESS_RECOVERY_URGENT) {
                PlanFullnessRecoveryActions(_character);
            } else if (job.jobType == JOB_TYPE.ENERGY_RECOVERY_URGENT) {
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
        Griefstricken griefstricken = character.traitContainer.GetNormalTrait<Griefstricken>("Griefstricken");
        if (griefstricken != null) {
            triggerGrieving = UnityEngine.Random.Range(0, 100) < (25 * character.traitContainer.stacks[griefstricken.name]);
        }
        if (!triggerGrieving) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.TRIGGER_FLAW, new GoapEffect(GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR), _character, _character);
            job.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[] { 12 });
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
