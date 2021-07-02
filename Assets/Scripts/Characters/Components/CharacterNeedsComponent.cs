using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using Interrupts;
using Traits;
using UnityEngine;
using UtilityScripts;
using Inner_Maps.Location_Structures;
using UnityEngine.Profiling;
using Random = System.Random;

public class CharacterNeedsComponent : CharacterComponent {
    public int doNotGetHungry { get; private set; }
    public int doNotGetTired{ get; private set; }
    public int doNotGetBored{ get; private set; }
    public int doNotGetDrained { get; private set; }
    public int doNotGetDiscouraged { get; private set; }

    public bool doesNotGetHungry => doNotGetHungry > 0 || (owner.currentActionNode != null && owner.currentActionNode.actionStatus == ACTION_STATUS.PERFORMING && owner.currentActionNode.action.IsFullnessRecoveryAction());
    public bool doesNotGetTired => doNotGetTired > 0 || (owner.currentActionNode != null && owner.currentActionNode.actionStatus == ACTION_STATUS.PERFORMING && owner.currentActionNode.action.IsTirednessRecoveryAction());
    public bool doesNotGetBored => doNotGetBored > 0 || (owner.currentActionNode != null && owner.currentActionNode.actionStatus == ACTION_STATUS.PERFORMING && owner.currentActionNode.action.IsHappinessRecoveryAction());

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
    // public int tirednessForcedTick { get; private set; }
    // public int currentSleepTicks { get; private set; }
    // public string sleepScheduleJobID { get; set; }
    // public bool hasCancelledSleepSchedule { get; private set; }
    private float tirednessLowerBound; //how low can this characters tiredness go
    public const float TIREDNESS_DEFAULT = 100f;
    public const float EXHAUSTED_UPPER_LIMIT = 20f;
    public const float TIRED_UPPER_LIMIT = 40f;
    public const float REFRESHED_LOWER_LIMIT = 91f;

    //Fullness
    public float fullness { get; private set; }
    public float fullnessDecreaseRate { get; private set; }
    // public int fullnessForcedTick { get; private set; }
    private float fullnessLowerBound; //how low can this characters fullness go
    public const float FULLNESS_DEFAULT = 100f;
    public const float STARVING_UPPER_LIMIT = 20f;
    public const float HUNGRY_UPPER_LIMIT = 50f;
    public const float FULL_LOWER_LIMIT = 91f;

    //Happiness
    public float happiness { get; private set; }
    public float happinessDecreaseRate { get; private set; }
    // public int happinessSecondForcedTick { get; private set; }
    private float happinessLowerBound; //how low can this characters happiness go
    public const float HAPPINESS_DEFAULT = 100f;
    public const float SULKING_UPPER_LIMIT = 20f;
    public const float BORED_UPPER_LIMIT = 41f;
    public const float ENTERTAINED_LOWER_LIMIT = 42f;

    //Stamina
    public float stamina { get; private set; }
    public float staminaDecreaseRate { get; private set; }
    public float baseStaminaDecreaseRate { get; private set; }
    private float staminaLowerBound; //how low can this characters happiness go
    public const float STAMINA_DEFAULT = 100f;
    public const float DRAINED_UPPER_LIMIT = 20f;
    public const float SPENT_UPPER_LIMIT = 50f;
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
    public bool hasForcedSecondHappiness { get; set; }
    // public TIME_IN_WORDS forcedFullnessRecoveryTimeInWords { get; private set; }
    // public TIME_IN_WORDS forcedTirednessRecoveryTimeInWords { get; private set; }
    // public TIME_IN_WORDS[] forcedHappinessRecoveryChoices { get; private set; }

    private bool _hasTriggeredThisHour;

    public CharacterNeedsComponent() {
        SetTirednessLowerBound(0f);
        SetFullnessLowerBound(0f);
        SetHappinessLowerBound(0f);
        SetStaminaLowerBound(0f);
        SetHopeLowerBound(0f);
        // SetForcedFullnessRecoveryTimeInWords(TIME_IN_WORDS.LUNCH_TIME);
        // SetForcedTirednessRecoveryTimeInWords(TIME_IN_WORDS.LATE_NIGHT);
        // SetForcedHappinessRecoveryTimeChoices(TIME_IN_WORDS.MORNING, TIME_IN_WORDS.AFTERNOON, TIME_IN_WORDS.EARLY_NIGHT);
        // SetFullnessForcedTick();
        // SetTirednessForcedTick();
        // SetHappinessForcedTick();
        //UpdateBaseStaminaDecreaseRate();
    }
    public CharacterNeedsComponent(SaveDataCharacterNeedsComponent data) {
        SetSaveDataCharacterNeedsComponent(data);
    }

    //This is only used for reapplication of mood data from save
    //When mood is loaded we need to reapply the data after the loading of traits so that the data will be consistent from when it was saved
    public void SetSaveDataCharacterNeedsComponent(SaveDataCharacterNeedsComponent data) {
        doNotGetHungry = data.doNotGetHungry;
        doNotGetTired = data.doNotGetTired;
        doNotGetBored = data.doNotGetBored;
        doNotGetDrained = data.doNotGetDrained;
        doNotGetDiscouraged = data.doNotGetDiscouraged;

        tiredness = data.tiredness;
        tirednessDecreaseRate = data.tirednessDecreaseRate;
        // tirednessForcedTick = data.tirednessForcedTick;
        // currentSleepTicks = data.currentSleepTicks;
        // sleepScheduleJobID = data.sleepScheduleJobID;
        // hasCancelledSleepSchedule = data.hasCancelledSleepSchedule;

        fullness = data.fullness;
        fullnessDecreaseRate = data.fullnessDecreaseRate;
        // fullnessForcedTick = data.fullnessForcedTick;

        happiness = data.happiness;
        happinessDecreaseRate = data.happinessDecreaseRate;
        // happinessSecondForcedTick = data.happinessSecondForcedTick;

        stamina = data.stamina;
        staminaDecreaseRate = data.staminaDecreaseRate;
        baseStaminaDecreaseRate = data.baseStaminaDecreaseRate;

        hope = data.hope;

        hasForcedFullness = data.hasForcedFullness;
        hasForcedTiredness = data.hasForcedTiredness;
        hasForcedSecondHappiness = data.hasForcedSecondHappiness;
        // forcedFullnessRecoveryTimeInWords = data.forcedFullnessRecoveryTimeInWords;
        // forcedTirednessRecoveryTimeInWords = data.forcedTirednessRecoveryTimeInWords;
        // forcedHappinessRecoveryChoices = data.forcedHappinessRecoveryChoices;
    }

    #region Initialization
    public void SubscribeToSignals() {
        //if(!(owner is Summon)) {
        //    Messenger.AddListener(Signals.TICK_STARTED, DecreaseNeeds); //do not make summons decrease needs
        //}
        Messenger.AddListener(Signals.HOUR_STARTED, PerHour);
    }
    public void UnsubscribeToSignals() {
        //if (Messenger.eventTable.ContainsKey(Signals.TICK_STARTED)) {
        //    Messenger.RemoveListener(Signals.TICK_STARTED, DecreaseNeeds);
        //}
        Messenger.RemoveListener(Signals.HOUR_STARTED, PerHour);
    }
    public void DailyGoapProcesses() {
        hasForcedFullness = false;
        hasForcedTiredness = false;
        hasForcedSecondHappiness = false;
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
        //ResetHopeMeter();
        SetHope(50);
        SetTiredness(UnityEngine.Random.Range(50, 101));
        SetFullness(UnityEngine.Random.Range(50, 101));
        SetHappiness(UnityEngine.Random.Range(50, 101));
        SetStamina(100);
    }
    #endregion

    #region Loading
    public void LoadAllStatsOfCharacter(SaveDataCharacter data) {
        //tiredness = data.tiredness;
        //fullness = data.fullness;
        //happiness = data.happiness;
        //fullnessDecreaseRate = data.fullnessDecreaseRate;
        //tirednessDecreaseRate = data.tirednessDecreaseRate;
        //happinessDecreaseRate = data.happinessDecreaseRate;
        //SetForcedFullnessRecoveryTimeInWords(data.forcedFullnessRecoveryTimeInWords);
        //SetForcedTirednessRecoveryTimeInWords(data.forcedTirednessRecoveryTimeInWords);
        //SetFullnessForcedTick(data.fullnessForcedTick);
        //SetTirednessForcedTick(data.tirednessForcedTick);
        //currentSleepTicks = data.currentSleepTicks;
        //sleepScheduleJobID = data.sleepScheduleJobID;
        //hasCancelledSleepSchedule = data.hasCancelledSleepSchedule;
    }
    #endregion

    public void PerTick() {
        DecreaseNeeds();
    }
    public void PerTickSummon() {
        StaminaAdjustments();
    }
    private void PerHour() {
#if DEBUG_PROFILER
        Profiler.BeginSample($"{owner.name} Needs Component Hour Started");
#endif
        if (!_hasTriggeredThisHour) {
            _hasTriggeredThisHour = true;
            EveryOtherHour();
        } else {
            _hasTriggeredThisHour = false;
        }
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif
    }
    private void EveryOtherHour() {
        if (HasNeeds() == false) { return; }
        CheckStarving();
    }

    public void CheckExtremeNeeds(Interrupt interruptThatTriggered = null) {
        if (HasNeeds() == false) { return; }
#if DEBUG_LOG
        string summary = $"{GameManager.Instance.TodayLogString()}{owner.name} will check his/her needs.";
#endif
        if (isStarving && (interruptThatTriggered == null || interruptThatTriggered.type != INTERRUPT.Grieving)) {
#if DEBUG_LOG
            summary += $"\n{owner.name} is starving. Planning fullness recovery actions...";
#endif
            PlanFullnessRecoveryActions();
        }
        if (isExhausted && (interruptThatTriggered == null || interruptThatTriggered.type != INTERRUPT.Feeling_Spooked)) {
#if DEBUG_LOG
            summary += $"\n{owner.name} is exhausted. Planning tiredness recovery actions...";
#endif
            PlanTirednessRecoveryActions();
        }
        // if (isSulking && (interruptThatTriggered == null || interruptThatTriggered.type != INTERRUPT.Feeling_Brokenhearted)) {
        //     summary += $"\n{owner.name} is sulking. Planning happiness recovery actions...";
        //     PlanHappinessRecoveryActions();
        // }
#if DEBUG_LOG
        owner.logComponent.PrintLogIfActive(summary);
#endif
    }
    public void CheckExtremeNeedsWhileInActiveParty(Interrupt interruptThatTriggered = null) {
        if (HasNeeds() == false) { return; }
#if DEBUG_LOG
        string summary = $"{GameManager.Instance.TodayLogString()}{owner.name} will check his/her needs.";
#endif
        if ((isStarving || isHungry) && (interruptThatTriggered == null || interruptThatTriggered.type != INTERRUPT.Grieving)) {
#if DEBUG_LOG
            summary += $"\n{owner.name} is starving. Planning fullness recovery actions...";
#endif
            PlanFullnessRecoveryActionsWhileInActiveParty();
        }
        if ((isExhausted || isTired) && (interruptThatTriggered == null || interruptThatTriggered.type != INTERRUPT.Feeling_Spooked)) {
#if DEBUG_LOG
            summary += $"\n{owner.name} is exhausted. Planning tiredness recovery actions...";
#endif
            PlanTirednessRecoveryActionsWhileInActiveParty();
        }
        if (interruptThatTriggered == null || interruptThatTriggered.type != INTERRUPT.Feeling_Brokenhearted) {
#if DEBUG_LOG
            summary += $"\n{owner.name} is sulking. Planning happiness recovery actions...";
#endif
            PlanHappinessRecoveryWhileInActiveParty();
        }
#if DEBUG_LOG
        owner.logComponent.PrintLogIfActive(summary);
#endif
    }
    private void CheckStarving() {
        if (isStarving) {
            PlanFullnessRecoveryActions();
        }
    }

    public bool HasNeeds() {
        return owner.race != RACE.SKELETON && !owner.characterClass.IsZombie() && owner.characterClass.className != "Necromancer" && !owner.hasBeenRaisedFromDead && owner.minion == null && !(owner is Summon)
            && !owner.traitContainer.HasTrait("Fervor")
            /*&& _character.isAtHomeRegion && _character.homeNpcSettlement != null*/; //Characters living on a region without a npcSettlement must not decrease needs
    }
    public void DecreaseNeeds() {
#if DEBUG_PROFILER
        Profiler.BeginSample($"{owner.name} Decrease Needs");
#endif
        StaminaAdjustments();

        if (HasNeeds() == false) {
            return;
        }
        if (!doesNotGetHungry) {
            AdjustFullness(-(EditableValuesManager.Instance.baseFullnessDecreaseRate + fullnessDecreaseRate));
        }
        if (!doesNotGetTired) {
            AdjustTiredness(-(EditableValuesManager.Instance.baseTirednessDecreaseRate + tirednessDecreaseRate));
        }
        if (!doesNotGetBored) {
            AdjustHappiness(-(EditableValuesManager.Instance.baseHappinessDecreaseRate + happinessDecreaseRate));
        }
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif
    }
    private void StaminaAdjustments() {
        //Stamina is not affected by HasNeeds checker, so anyone, even demons will decrease their stamina
        if (doNotGetDrained <= 0) {
            if (owner.marker && owner.marker.isMoving) {
                if (owner.movementComponent.isRunning) {
                    AdjustStamina(-(baseStaminaDecreaseRate + staminaDecreaseRate));
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
        summary += $"\nTrust: {hope.ToString(CultureInfo.InvariantCulture)}/{HOPE_DEFAULT.ToString(CultureInfo.InvariantCulture)}";
        return summary;
    }
    public void AdjustFullnessDecreaseRate(float amount) {
        fullnessDecreaseRate += amount;
#if DEBUG_LOG
        Debug.Log($"{owner.name} adjusted fullness decrease rate by {amount.ToString()}. New value is {fullnessDecreaseRate.ToString()}");
#endif
    }
    public void AdjustTirednessDecreaseRate(float amount) {
        tirednessDecreaseRate += amount;
    }
    public void AdjustHappinessDecreaseRate(float amount) {
        happinessDecreaseRate += amount;
#if DEBUG_LOG
        Debug.Log($"Adjusted happiness decrease rate of {owner.name} by {amount.ToString(CultureInfo.InvariantCulture)} new decrease rate is {happinessDecreaseRate.ToString(CultureInfo.InvariantCulture)}");
#endif
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
        if(adjustment < 0 && owner.traitContainer.HasTrait("Vampire")) {
#if DEBUG_LOG
            owner.logComponent.PrintLogIfActive("Trying to reduce energy meter but character is a vampire, will ignore reduction.");
#endif
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
                owner.traitContainer.AddTrait(owner, "Unconscious");
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
                owner.traitContainer.AddTrait(owner, "Unconscious");
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
            owner.traitContainer.AddTrait(owner, "Refreshed");
        }
        if (wasExhausted) {
            owner.traitContainer.RemoveTrait(owner, "Exhausted");
        }
        if (wasTired) {
            owner.traitContainer.RemoveTrait(owner, "Tired");
        }
    }
    private void OnTired(bool wasRefreshed, bool wasTired, bool wasExhausted) {
        if (!wasTired) {
            owner.traitContainer.AddTrait(owner, "Tired");
        }
        if (wasExhausted) {
            owner.traitContainer.RemoveTrait(owner, "Exhausted");
        }
        if (wasRefreshed) {
            owner.traitContainer.RemoveTrait(owner, "Refreshed");
        }
    }
    private void OnExhausted(bool wasRefreshed, bool wasTired, bool wasExhausted) {
        if (!wasExhausted) {
            owner.traitContainer.AddTrait(owner, "Exhausted");
            //Messenger.Broadcast<Character, string>(Signals.TRANSFER_ENGAGE_TO_FLEE_LIST, _character, "exhausted");
        }
        if (wasTired) {
            owner.traitContainer.RemoveTrait(owner, "Tired");
        }
        if (wasRefreshed) {
            owner.traitContainer.RemoveTrait(owner, "Refreshed");
        }
    }
    private void OnNormalEnergy(bool wasRefreshed, bool wasTired, bool wasExhausted) {
        if (wasExhausted) {
            owner.traitContainer.RemoveTrait(owner, "Exhausted");
        }
        if (wasTired) {
            owner.traitContainer.RemoveTrait(owner, "Tired");
        }
        if (wasRefreshed) {
            owner.traitContainer.RemoveTrait(owner, "Refreshed");
        }
    }
    private void RemoveTiredOrExhausted() {
        if (owner.traitContainer.RemoveTrait(owner, "Tired") == false) {
            owner.traitContainer.RemoveTrait(owner, "Exhausted");
        }
    }
    // public void SetTirednessForcedTick() {
    //     if (!hasForcedTiredness) {
    //         if (forcedTirednessRecoveryTimeInWords == GameManager.Instance.GetCurrentTimeInWordsOfTick()) {
    //             //If the forced recovery job has not been done yet and the character is already on the time of day when it is supposed to be done,
    //             //the tick that will be assigned will be ensured that the character will not miss it
    //             //Example if the time of day is Afternoon, the supposed tick range for it is 145 - 204
    //             //So if the current tick of the game is already in 160, the range must be adjusted to 161 - 204, so as to ensure that the character will hit it
    //             //But if the current tick of the game is already in 204, it cannot be 204 - 204, so, it will revert back to 145 - 204 
    //             int newTick = GameManager.Instance.GetRandomTickFromTimeInWords(forcedTirednessRecoveryTimeInWords, GameManager.Instance.Today().tick + 1);
    //             TIME_IN_WORDS timeInWords = GameManager.Instance.GetTimeInWordsOfTick(newTick);
    //             if(timeInWords != forcedTirednessRecoveryTimeInWords) {
    //                 newTick = GameManager.Instance.GetRandomTickFromTimeInWords(forcedTirednessRecoveryTimeInWords);
    //             }
    //             tirednessForcedTick = newTick;
    //             return;
    //         }
    //     }
    //     tirednessForcedTick = GameManager.Instance.GetRandomTickFromTimeInWords(forcedTirednessRecoveryTimeInWords);
    // }
    // public void SetTirednessForcedTick(int tick) {
    //     tirednessForcedTick = tick;
    // }
    // public void SetForcedTirednessRecoveryTimeInWords(TIME_IN_WORDS timeInWords) {
    //     forcedTirednessRecoveryTimeInWords = timeInWords;
    // }
    public void AdjustDoNotGetTired(int amount) {
        doNotGetTired += amount;
    }
    public bool PlanTirednessRecoveryActions() {
        if (!owner.limiterComponent.canPerform) { //character.doNotDisturb > 0 || !character.limiterComponent.canWitness
            return false;
        }
        if (this.isExhausted) {
            if (!owner.jobQueue.HasJob(JOB_TYPE.ENERGY_RECOVERY_URGENT)) {
                //If there is already a TIREDNESS_RECOVERY JOB and the character becomes Exhausted, replace TIREDNESS_RECOVERY with TIREDNESS_RECOVERY_STARVING only if that character is not doing the job already
                JobQueueItem tirednessRecoveryJob = owner.jobQueue.GetJob(JOB_TYPE.ENERGY_RECOVERY_NORMAL);
                if (tirednessRecoveryJob != null) {
                    //Replace this with Tiredness Recovery Exhausted only if the character is not doing the Tiredness Recovery Job already
                    JobQueueItem currJob = owner.currentJob;
                    if (currJob == tirednessRecoveryJob) {
                        return false;
                    } else {
                        tirednessRecoveryJob.CancelJob();
                    }
                }
                JOB_TYPE jobType = JOB_TYPE.ENERGY_RECOVERY_URGENT;
                PlanTirednessRecovery(jobType, false);
                return true;
            }
        }
        return false;
    }
    public bool PlanTirednessRecoveryActionsForSleepBehaviour(out JobQueueItem producedJob) {
        if (!owner.limiterComponent.canPerform) {
            producedJob = null;
            return false;
        }
        if (!owner.jobQueue.HasJob(JOB_TYPE.ENERGY_RECOVERY_URGENT, JOB_TYPE.ENERGY_RECOVERY_NORMAL)) {
            return PlanTirednessRecovery(JOB_TYPE.ENERGY_RECOVERY_NORMAL, false, out producedJob);
        }
        producedJob = null;
        return false;
    }
    private bool PlanTirednessRecoveryActionsWhileInActiveParty() {
        if (!owner.limiterComponent.canPerform) { //character.doNotDisturb > 0 || !character.limiterComponent.canWitness
            return false;
        }
        if (isExhausted) {
            if (!owner.jobQueue.HasJob(JOB_TYPE.ENERGY_RECOVERY_URGENT)) {
                //If there is already a TIREDNESS_RECOVERY JOB and the character becomes Exhausted, replace TIREDNESS_RECOVERY with TIREDNESS_RECOVERY_STARVING only if that character is not doing the job already
                JobQueueItem tirednessRecoveryJob = owner.jobQueue.GetJob(JOB_TYPE.ENERGY_RECOVERY_NORMAL);
                if (tirednessRecoveryJob != null) {
                    //Replace this with Tiredness Recovery Exhausted only if the character is not doing the Tiredness Recovery Job already
                    JobQueueItem currJob = owner.currentJob;
                    if (currJob == tirednessRecoveryJob) {
                        return false;
                    } else {
                        tirednessRecoveryJob.CancelJob();
                    }
                }
                JOB_TYPE jobType = JOB_TYPE.ENERGY_RECOVERY_URGENT;
                PlanTirednessRecoveryBase(jobType, false);
                return true;
            }
        } else if (isTired) {
            if (!owner.jobQueue.HasJob(JOB_TYPE.ENERGY_RECOVERY_NORMAL)) {
                PlanTirednessRecoveryBase(JOB_TYPE.ENERGY_RECOVERY_NORMAL, false);
                return true;
            }
        }
        return false;
    }
    public bool PlanExtremeTirednessRecoveryActionsForCannotPerform() {
        //This is to prevent the character from creating tiredness recovery when he/she is in an active party
        //because when a character is in an active party the party controls the needs recovery that is why we must be sure that he/she will not create its own needs recovery
        //If the character is in an active party and must create a needs recovery, we bypass this checking, meaning the PlanFullnessRecoveryBase is called directly, see CheckExtremeNeedsWhileInActiveParty
        //Note: This part is outside the normal function of planning tiredness recovery because we force the character to do Sleep Outside action instead of going through the normal planning
        if (owner.partyComponent.isActiveMember) {
            return false;
        }
        if (!owner.jobQueue.HasJob(JOB_TYPE.ENERGY_RECOVERY_URGENT)) {
            //If there is already a TIREDNESS_RECOVERY JOB and the character becomes Exhausted, replace TIREDNESS_RECOVERY with TIREDNESS_RECOVERY_STARVING only if that character is not doing the job already
            JobQueueItem tirednessRecoveryJob = owner.jobQueue.GetJob(JOB_TYPE.ENERGY_RECOVERY_NORMAL);
            if (tirednessRecoveryJob != null) {
                //Replace this with Tiredness Recovery Exhausted only if the character is not doing the Tiredness Recovery Job already
                JobQueueItem currJob = owner.currentJob;
                if (currJob == tirednessRecoveryJob) {
                    return false;
                } else {
                    tirednessRecoveryJob.CancelJob();
                }
            }
            JOB_TYPE jobType = JOB_TYPE.ENERGY_RECOVERY_URGENT;
            bool triggerSpooked = false;
            Spooked spooked = owner.traitContainer.GetTraitOrStatus<Spooked>("Spooked");
            if (spooked != null) {
                triggerSpooked = UnityEngine.Random.Range(0, 100) < (25 * owner.traitContainer.stacks[spooked.name]);
            }
            if (!triggerSpooked) {
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.SLEEP_OUTSIDE, owner, owner);
                //job.SetCancelOnFail(true);
                owner.jobQueue.AddJobInQueue(job);
            } else {
                spooked.TriggerFeelingSpooked();
            }
            return true;
        }
        return false;
    }
    // public void PlanScheduledTirednessRecovery() {
    //     if (!hasForcedTiredness && tirednessForcedTick != 0 && GameManager.Instance.Today().tick >= tirednessForcedTick && owner.limiterComponent.canPerform && !doesNotGetTired) {
    //         if (!owner.jobQueue.HasJob(JOB_TYPE.ENERGY_RECOVERY_NORMAL, JOB_TYPE.ENERGY_RECOVERY_URGENT)) {
    //             JOB_TYPE jobType = JOB_TYPE.ENERGY_RECOVERY_NORMAL;
    //             if (isExhausted) {
    //                 jobType = JOB_TYPE.ENERGY_RECOVERY_URGENT;
    //             }
    //             PlanTirednessRecovery(jobType, true);
    //         }
    //         hasForcedTiredness = true;
    //         SetTirednessForcedTick();
    //     }
    //     //If a character current sleep ticks is less than the default, this means that the character already started sleeping but was awaken midway that is why he/she did not finish the allotted sleeping time
    //     //When this happens, make sure to queue tiredness recovery again so he can finish the sleeping time
    //     else if ((hasCancelledSleepSchedule || currentSleepTicks < CharacterManager.Instance.defaultSleepTicks) && owner.limiterComponent.canPerform) {
    //         if (!owner.jobQueue.HasJob(JOB_TYPE.ENERGY_RECOVERY_NORMAL, JOB_TYPE.ENERGY_RECOVERY_URGENT)) {
    //             JOB_TYPE jobType = JOB_TYPE.ENERGY_RECOVERY_NORMAL;
    //             if (isExhausted) {
    //                 jobType = JOB_TYPE.ENERGY_RECOVERY_URGENT;
    //             }
    //             PlanTirednessRecovery(jobType, true);
    //         }
    //         SetHasCancelledSleepSchedule(false);
    //     }
    // }
    private GoapPlanJob PlanTirednessRecovery(JOB_TYPE jobType, bool shouldSetScheduleJobID) {
        //This is to prevent the character from creating tiredness recovery when he/she is in an active party
        //because when a character is in an active party the party controls the needs recovery that is why we must be sure that he/she will not create its own needs recovery
        //If the character is in an active party and must create a needs recovery, we bypass this checking, meaning the PlanFullnessRecoveryBase is called directly, see CheckExtremeNeedsWhileInActiveParty
        if (owner.partyComponent.isActiveMember) {
            return null;
        }
        return PlanTirednessRecoveryBase(jobType, shouldSetScheduleJobID);
    }
    private bool PlanTirednessRecovery(JOB_TYPE jobType, bool shouldSetScheduleJobID, out JobQueueItem producedJob) {
        //This is to prevent the character from creating tiredness recovery when he/she is in an active party
        //because when a character is in an active party the party controls the needs recovery that is why we must be sure that he/she will not create its own needs recovery
        //If the character is in an active party and must create a needs recovery, we bypass this checking, meaning the PlanFullnessRecoveryBase is called directly, see CheckExtremeNeedsWhileInActiveParty
        if (owner.partyComponent.isActiveMember) {
            producedJob = null;
            return false;
        }
        return PlanTirednessRecoveryBase(jobType, shouldSetScheduleJobID, out producedJob);
    }
    private GoapPlanJob PlanTirednessRecoveryBase(JOB_TYPE jobType, bool shouldSetScheduleJobID) {
        if (!owner.limiterComponent.canDoTirednessRecovery) {
            //No matter what, if character has the limiter "cannot do tiredness recovery", he will not do tiredness recovery
#if DEBUG_LOG
            owner.logComponent.PrintLogIfActive($"\n{owner.name} is cannot do tiredness recovery");
#endif
            return null;
        }
        //No matter what happens, we do not allow characters to sleep if they are burning/poisoned because it does not make sense
        if (owner.traitContainer.HasTrait("Burning", "Poisoned")) {
#if DEBUG_LOG
            owner.logComponent.PrintLogIfActive($"\n{owner.name} is poisoned or burning will not plan tiredness recovery...");
#endif
            return null;
        }
        bool triggerSpooked = false;
        Spooked spooked = owner.traitContainer.GetTraitOrStatus<Spooked>("Spooked");
        if (spooked != null) {
            triggerSpooked = UnityEngine.Random.Range(0, 100) < (25 * owner.traitContainer.stacks[spooked.name]);
        }
        if (!triggerSpooked) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, new GoapEffect(GOAP_EFFECT_CONDITION.TIREDNESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR), owner, owner);
            if (!owner.traitContainer.HasTrait("Travelling")) {
                if(owner.homeStructure != null) {
                    job.AddPriorityLocation(INTERACTION_TYPE.SLEEP, owner.homeStructure);
                }
            }
            if (owner.currentSettlement != null && owner.currentSettlement.HasStructure(STRUCTURE_TYPE.TAVERN)) {
                // if (owner.homeStructure == null || owner.homeStructure.settlementLocation != owner.currentSettlement) {
                    List<LocationStructure> taverns = owner.currentSettlement.GetStructuresOfType(STRUCTURE_TYPE.TAVERN);
                    for (int i = 0; i < taverns.Count; i++) {
                        LocationStructure tavern = taverns[i];
                        job.AddPriorityLocation(INTERACTION_TYPE.SLEEP, tavern);
                    }    
                // }
            }
            owner.jobQueue.AddJobInQueue(job);
            // if (shouldSetScheduleJobID) {
            //     sleepScheduleJobID = job.persistentID;
            // }
        } else {
            spooked.TriggerFeelingSpooked();
        }
        return null;
    }
    private bool PlanTirednessRecoveryBase(JOB_TYPE jobType, bool shouldSetScheduleJobID, out JobQueueItem producedJob) {
        if (!owner.limiterComponent.canDoTirednessRecovery) {
            //No matter what, if character has the limiter "cannot do tiredness recovery", he will not do tiredness recovery
#if DEBUG_LOG
            owner.logComponent.PrintLogIfActive($"\n{owner.name} is cannot do tiredness recovery");
#endif
            producedJob = null;
            return false;
        }
        //No matter what happens, we do not allow characters to sleep if they are burning/poisoned because it does not make sense
        if (owner.traitContainer.HasTrait("Burning", "Poisoned")) {
#if DEBUG_LOG
            owner.logComponent.PrintLogIfActive($"\n{owner.name} is poisoned or burning will not plan tiredness recovery...");
#endif
            producedJob = null;
            return false;
        }
        bool triggerSpooked = false;
        Spooked spooked = owner.traitContainer.GetTraitOrStatus<Spooked>("Spooked");
        if (spooked != null) {
            triggerSpooked = UnityEngine.Random.Range(0, 100) < (25 * owner.traitContainer.stacks[spooked.name]);
        }
        if (!triggerSpooked) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, new GoapEffect(GOAP_EFFECT_CONDITION.TIREDNESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR), owner, owner);
            if (!owner.traitContainer.HasTrait("Travelling")) {
                if(owner.homeStructure != null) {
                    job.AddPriorityLocation(INTERACTION_TYPE.SLEEP, owner.homeStructure);
                }
            }
            if (owner.currentSettlement != null && owner.currentSettlement.HasStructure(STRUCTURE_TYPE.TAVERN)) {
                // if (owner.homeStructure == null || owner.homeStructure.settlementLocation != owner.currentSettlement) {
                    List<LocationStructure> taverns = owner.currentSettlement.GetStructuresOfType(STRUCTURE_TYPE.TAVERN);
                    for (int i = 0; i < taverns.Count; i++) {
                        LocationStructure tavern = taverns[i];
                        job.AddPriorityLocation(INTERACTION_TYPE.SLEEP, tavern);
                    }    
                // }
            }
            // if (shouldSetScheduleJobID) {
            //     sleepScheduleJobID = job.persistentID;
            // }
            producedJob = job;
            return true;
        } else {
            spooked.TriggerFeelingSpooked();
        }
        producedJob = null;
        return false;
    }
    // public void SetHasCancelledSleepSchedule(bool state) {
    //     hasCancelledSleepSchedule = state;
    // }
    // public void ResetSleepTicks() {
    //     currentSleepTicks = CharacterManager.Instance.defaultSleepTicks;
    // }
    // public void AdjustSleepTicks(int amount) {
    //     currentSleepTicks += amount;
    //     if(currentSleepTicks <= 0) {
    //         this.ResetSleepTicks();
    //     }
    // }
    public void ExhaustCharacter(Character character) {
        if (!isExhausted) {
            SetTiredness(EXHAUSTED_UPPER_LIMIT);
        }
    }
    public void WakeUpFromNoise() {
        if (owner.traitContainer.HasTrait("Resting")) {
            if (GameUtilities.RollChance(50)) {
                owner.interruptComponent.TriggerInterrupt(INTERRUPT.Noise_Wake_Up, owner);
            }
        }
    }
    #endregion

    #region Happiness
    public void ResetHappinessMeter() {
        if (owner.traitContainer.HasTrait("Psychopath")) {
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
        if (owner.traitContainer.HasTrait("Psychopath")) {
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
    public void SetHappiness(float amount, bool bypassPsychopathChecking = false) {
        if (!bypassPsychopathChecking) {
            if (owner.traitContainer.HasTrait("Psychopath")) {
                //Psychopath's Happiness is always fixed at 50 and is not changed by anything.
                return;
            }
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
            owner.traitContainer.AddTrait(owner, "Entertained");
        }
        if (wasBored) {
            owner.traitContainer.RemoveTrait(owner, "Bored");
        }
        if (wasSulking) {
            owner.traitContainer.RemoveTrait(owner, "Sulking");
        }
    }
    private void OnBored(bool wasEntertained, bool wasBored, bool wasSulking) {
        if (!wasBored) {
            owner.traitContainer.AddTrait(owner, "Bored");
        }
        if (wasEntertained) {
            owner.traitContainer.RemoveTrait(owner, "Entertained");
        }
        if (wasSulking) {
            owner.traitContainer.RemoveTrait(owner, "Sulking");
        }
    }
    private void OnSulking(bool wasEntertained, bool wasBored, bool wasSulking) {
        if (!wasSulking) {
            owner.traitContainer.AddTrait(owner, "Sulking");
        }
        if (wasEntertained) {
            owner.traitContainer.RemoveTrait(owner, "Entertained");
        }
        if (wasBored) {
            owner.traitContainer.RemoveTrait(owner, "Bored");
        }
    }
    private void OnNormalHappiness(bool wasEntertained, bool wasBored, bool wasSulking) {
        if (wasSulking) {
            owner.traitContainer.RemoveTrait(owner, "Sulking");
        }
        if (wasEntertained) {
            owner.traitContainer.RemoveTrait(owner, "Entertained");
        }
        if (wasBored) {
            owner.traitContainer.RemoveTrait(owner, "Bored");
        }
    }
    private void RemoveBoredOrSulking() {
        if (owner.traitContainer.RemoveTrait(owner, "Bored") == false) {
            owner.traitContainer.RemoveTrait(owner, "Sulking");
        }
    }
    public void AdjustDoNotGetBored(int amount) {
        doNotGetBored += amount;
    }
    // public bool CanPlanScheduledHappinessRecovery() {
    //     if (!hasForcedHappiness && happinessForcedTick != 0 && GameManager.Instance.currentTick >= happinessForcedTick && owner.limiterComponent.canPerform && doNotGetBored <= 0) {
    //         return true;
    //     }
    //     if (!hasForcedSecondHappiness && happinessSecondForcedTick != 0 && GameManager.Instance.currentTick >= happinessSecondForcedTick && owner.limiterComponent.canPerform && doNotGetBored <= 0) {
    //         return true;
    //     }
    //     return false;
    // }
    // public void PlanScheduledSecondHappinessRecovery() {
    //     if (!hasForcedSecondHappiness && happinessSecondForcedTick != 0 && GameManager.Instance.currentTick >= happinessSecondForcedTick && owner.limiterComponent.canPerform && !doesNotGetBored) {
    //         hasForcedSecondHappiness = true;
    //         if (!owner.jobQueue.HasJob(JOB_TYPE.HAPPINESS_RECOVERY)) {
    //             PlanHappinessRecoveryActions();
    //         }
    //         SetHappinessForcedTick();
    //     }
    // }
    public bool PlanHappinessRecoveryActions() {
        //This is to prevent the character from creating happiness recovery when he/she is in an active party
        //because when a character is in an active party the party controls the needs recovery that is why we must be sure that he/she will not create its own needs recovery
        //If the character is in an active party and must create a needs recovery, we bypass this checking, meaning the PlanFullnessRecoveryBase is called directly, see CheckExtremeNeedsWhileInActiveParty
        if (owner.partyComponent.isActiveMember) {
            return false;
        }
        return PlanHappinessRecoveryBase();
    }
    public bool PlanHappinessRecoveryWhileInActiveParty() {
        return PlanHappinessRecoveryBase();
    }
    private bool PlanHappinessRecoveryBase() {
        if (!owner.limiterComponent.canDoHappinessRecovery) {
            //No matter what, if character has the limiter "cannot do happiness recovery", he will not do happiness recovery
#if DEBUG_LOG
            owner.logComponent.PrintLogIfActive($"\n{owner.name} is cannot do happiness recovery");
#endif
            return false;
        }
        if (!owner.limiterComponent.canPerform) { //character.doNotDisturb > 0 || !character.limiterComponent.canWitness
            return false;
        }
        // if (isBored || isSulking) {
            if (!owner.jobQueue.HasJob(JOB_TYPE.HAPPINESS_RECOVERY)) {
                // int chance = UnityEngine.Random.Range(0, 100);
                // int value = 0;
                // TIME_IN_WORDS currentTimeInWords = GameManager.GetCurrentTimeInWordsOfTick(owner);
                // if (currentTimeInWords == TIME_IN_WORDS.MORNING) {
                //     value = 30;
                // } else if (currentTimeInWords == TIME_IN_WORDS.LUNCH_TIME) {
                //     value = 45;
                // } else if (currentTimeInWords == TIME_IN_WORDS.AFTERNOON) {
                //     value = 45;
                // } else if (currentTimeInWords == TIME_IN_WORDS.EARLY_NIGHT) {
                //     value = 45;
                // } else if (currentTimeInWords == TIME_IN_WORDS.LATE_NIGHT) {
                //     value = 30;
                // }
                // if (chance < value || isSulking) {
            bool triggerBrokenhearted = false;
            Heartbroken heartbroken = owner.traitContainer.GetTraitOrStatus<Heartbroken>("Heartbroken");
            if (heartbroken != null) {
                triggerBrokenhearted = UnityEngine.Random.Range(0, 100) < (25 * owner.traitContainer.stacks[heartbroken.name]);
            }
            if (!triggerBrokenhearted) {
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HAPPINESS_RECOVERY, new GoapEffect(GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR), base.owner, base.owner);
                JobUtilities.PopulatePriorityLocationsForHappinessRecovery(owner, job);
                job.SetDoNotRecalculate(true);
                owner.jobQueue.AddJobInQueue(job);
            } else {
                heartbroken.TriggerBrokenhearted();
            }
            return true;
                // }
            }
        // }
        return false;
    }
    // public void SetHappinessForcedTick() {
    //     //reroll happiness recovery range so character can schedule on another time of day
    //     TIME_IN_WORDS chosenTime = CollectionUtilities.GetRandomElement(forcedHappinessRecoveryChoices);
    //     // if (owner != null && !owner.traitContainer.HasTrait("Nocturnal")) {
    //     //     SetForcedHappinessSecondRecoveryTimeInWords(GameUtilities.RollChance(50) ? TIME_IN_WORDS.AFTERNOON : TIME_IN_WORDS.EARLY_NIGHT);
    //     // }
    //    
    //     int newTick = GameManager.Instance.GetRandomTickFromTimeInWords(chosenTime, GameManager.Instance.Today().tick + 1);
    //     TIME_IN_WORDS timeInWords = GameManager.Instance.GetTimeInWordsOfTick(newTick);
    //     if(timeInWords != chosenTime) {
    //         newTick = GameManager.Instance.GetRandomTickFromTimeInWords(chosenTime);
    //     }
    //     happinessSecondForcedTick = newTick;
    // }
    // public void SetHappinessForcedTick(int tick) {
    //     happinessSecondForcedTick = tick;
    // }
    // public void SetForcedHappinessRecoveryTimeChoices(params TIME_IN_WORDS[] timeInWords) {
    //     forcedHappinessRecoveryChoices = timeInWords;
    // }
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
            owner.HPRecovery(0.02f);
        }
        if (fullness == 0f) {
            if (!wasMalnourished) {
                owner.traitContainer.AddTrait(owner, "Malnourished");
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
        //    if (_character.traitContainer.AddTrait(_character, "Starving") && _character.traitContainer.GetNormalTrait<Trait>("Vampire") == null) { //only characters that are not vampires will flee when they are starving
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
                owner.traitContainer.AddTrait(owner, "Malnourished");
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
            owner.traitContainer.AddTrait(owner, "Full");
        }
        if (wasHungry) {
            owner.traitContainer.RemoveTrait(owner, "Hungry");
        }
        if (wasStarving) {
            owner.traitContainer.RemoveTrait(owner, "Starving");
        }
        owner.traitContainer.RemoveTrait(owner, "Malnourished");
        // if (wasMalnourished) {
        //     _character.traitContainer.RemoveTrait(_character, "Malnourished");
        // }
    }
    private void OnHungry(bool wasFull, bool wasHungry, bool wasStarving) {
        if (!wasHungry) {
            owner.traitContainer.AddTrait(owner, "Hungry");
        }
        if (wasFull) {
            owner.traitContainer.RemoveTrait(owner, "Full");
        }
        if (wasStarving) {
            owner.traitContainer.RemoveTrait(owner, "Starving");
        }
    }
    private void OnStarving(bool wasFull, bool wasHungry, bool wasStarving) {
        if (!wasStarving) {
            owner.traitContainer.AddTrait(owner, "Starving");
        }
        if (wasFull) {
            owner.traitContainer.RemoveTrait(owner, "Full");
        }
        if (wasHungry) {
            owner.traitContainer.RemoveTrait(owner, "Hungry");
        }
    }
    private void OnNormalFullness(bool wasFull, bool wasHungry, bool wasStarving, bool wasMalnourished) {
        if (wasStarving) {
            owner.traitContainer.RemoveTrait(owner, "Starving");
        }
        if (wasFull) {
            owner.traitContainer.RemoveTrait(owner, "Full");
        }
        if (wasHungry) {
            owner.traitContainer.RemoveTrait(owner, "Hungry");
        }
        owner.traitContainer.RemoveTrait(owner, "Malnourished");
        // if (wasMalnourished) {
        //     _character.traitContainer.RemoveTrait(_character, "Malnourished");
        // }
    }
    private void RemoveHungryOrStarving() {
        if (owner.traitContainer.RemoveTrait(owner, "Hungry") == false) {
            owner.traitContainer.RemoveTrait(owner, "Starving");
        }
    }
    // public void SetFullnessForcedTick() {
    //     if (!hasForcedFullness) {
    //         if (forcedFullnessRecoveryTimeInWords == GameManager.Instance.GetCurrentTimeInWordsOfTick()) {
    //             //If the forced recovery job has not been done yet and the character is already on the time of day when it is supposed to be done,
    //             //the tick that will be assigned will be ensured that the character will not miss it
    //             //Example if the time of day is Afternoon, the supposed tick range for it is 145 - 204
    //             //So if the current tick of the game is already in 160, the range must be adjusted to 161 - 204, so as to ensure that the character will hit it
    //             //But if the current tick of the game is already in 204, it cannot be 204 - 204, so, it will revert back to 145 - 204 
    //             int newTick = GameManager.Instance.GetRandomTickFromTimeInWords(forcedFullnessRecoveryTimeInWords, GameManager.Instance.Today().tick + 1);
    //             TIME_IN_WORDS timeInWords = GameManager.Instance.GetTimeInWordsOfTick(newTick);
    //             if (timeInWords != forcedFullnessRecoveryTimeInWords) {
    //                 newTick = GameManager.Instance.GetRandomTickFromTimeInWords(forcedFullnessRecoveryTimeInWords);
    //             }
    //             fullnessForcedTick = newTick;
    //             return;
    //         }
    //     }
    //     fullnessForcedTick = GameManager.Instance.GetRandomTickFromTimeInWords(forcedFullnessRecoveryTimeInWords);
    // }
    // public void SetFullnessForcedTick(int tick) {
    //     fullnessForcedTick = tick;
    // }
    // public void SetForcedFullnessRecoveryTimeInWords(TIME_IN_WORDS timeInWords) {
    //     forcedFullnessRecoveryTimeInWords = timeInWords;
    // }
    public void AdjustDoNotGetHungry(int amount) {
        doNotGetHungry += amount;
    }
    // public void PlanScheduledFullnessRecovery() {
    //     if (owner.traitContainer.HasTrait("Vampire")) {
    //         //Note: Instead of just restricting the vampires from doing fullness recovery when it is not After Midnight, we will restrict the vampires from doing normal fullness recovery any time
    //         //because their fullness recovery is now triggered in VampireBehaviour
    //         //We did this to eliminate the conflict between nocturnals and vampires, because when a character becomes nocturnal, the fullnesss recovery sched is switched to Early Night, however, if it becomes a Vampire, it will switch to After Midnight
    //         //Now, when a character becomes Nocturnal, after becoming a Vampire, the schedule will be on Early Night, not After Midnight
    //         return;
    //     }
    //     if (!hasForcedFullness && fullnessForcedTick != 0 && GameManager.Instance.currentTick >= fullnessForcedTick && owner.limiterComponent.canPerform && !doesNotGetHungry) {
    //         hasForcedFullness = true;
    //         //if (owner.traitContainer.HasTrait("Vampire")) {
    //         //    TIME_IN_WORDS currentTime = GameManager.GetCurrentTimeInWordsOfTick();
    //         //    if (currentTime != TIME_IN_WORDS.AFTER_MIDNIGHT) {
    //         //        //Vampires will only recover fullness after midnight, no matter how hungry they are
    //         //        //https://trello.com/c/f0ICyIAj/2475-vampires-only-drink-blood-at-night
    //         //        //The reason why the checking only happens here is because this is the one being called every hour, every time the character becomes starving, etc.
    //         //        //Some fullness recovery actions especially the ones forced by the player (ex. Glutton trigger flaw) must still occur even if it is After Midnight
    //         //        return;
    //         //    }
    //         //}
    //         if (!owner.jobQueue.HasJob(JOB_TYPE.FULLNESS_RECOVERY_NORMAL, JOB_TYPE.FULLNESS_RECOVERY_URGENT, JOB_TYPE.FULLNESS_RECOVERY_URGENT)) {
    //             JOB_TYPE jobType = JOB_TYPE.FULLNESS_RECOVERY_NORMAL;
    //             if (isStarving) {
    //                 jobType = JOB_TYPE.FULLNESS_RECOVERY_URGENT;
    //             }
    //             GoapPlanJob job = PlanFullnessRecovery(jobType);
    //             if(job != null) {
    //                 owner.jobQueue.AddJobInQueue(job);
    //             }
    //         }
    //         SetFullnessForcedTick();
    //     }
    // }
    public bool PlanFullnessRecoveryActions() {
        if (!owner.limiterComponent.canPerform) { //character.doNotDisturb > 0 || !character.limiterComponent.canWitness
            return false;
        }
        if (owner.traitContainer.HasTrait("Vampire")) {
            //Note: Instead of just restricting the vampires from doing fullness recovery when it is not After Midnight, we will restrict the vampires from doing normal fullness recovery any time
            //because their fullness recovery is now triggered in VampireBehaviour
            //We did this to eliminate the conflict between nocturnals and vampires, because when a character becomes nocturnal, the fullnesss recovery sched is switched to Early Night, however, if it becomes a Vampire, it will switch to After Midnight
            //Now, when a character becomes Nocturnal, after becoming a Vampire, the schedule will be on Early Night, not After Midnight
            return false;
        }
        //if (owner.traitContainer.HasTrait("Vampire")) {
        //    TIME_IN_WORDS currentTime = GameManager.GetCurrentTimeInWordsOfTick();
        //    if(currentTime != TIME_IN_WORDS.AFTER_MIDNIGHT) {
        //        //Vampires will only recover fullness after midnight, no matter how hungry they are
        //        //https://trello.com/c/f0ICyIAj/2475-vampires-only-drink-blood-at-night
        //        //The reason why the checking only happens here is because this is the one being called every hour, every time the character becomes starving, etc.
        //        //Some fullness recovery actions especially the ones forced by the player (ex. Glutton trigger flaw) must still occur even if it is After Midnight
        //        return false;
        //    }
        //}
        if (this.isStarving) {
            if (!owner.jobQueue.HasJob(JOB_TYPE.FULLNESS_RECOVERY_URGENT, JOB_TYPE.FULLNESS_RECOVERY_ON_SIGHT)) {
                //If there is already a HUNGER_RECOVERY JOB and the character becomes Starving, replace HUNGER_RECOVERY with HUNGER_RECOVERY_STARVING only if that character is not doing the job already
                JobQueueItem hungerRecoveryJob = owner.jobQueue.GetJob(JOB_TYPE.FULLNESS_RECOVERY_NORMAL);
                if (hungerRecoveryJob != null) {
                    //Replace this with Hunger Recovery Starving only if the character is not doing the Hunger Recovery Job already
                    JobQueueItem currJob = owner.currentJob;
                    if (currJob == hungerRecoveryJob) {
                        return false;
                    } else {
                        hungerRecoveryJob.CancelJob();
                    }
                }
                JOB_TYPE jobType = JOB_TYPE.FULLNESS_RECOVERY_URGENT;
                GoapPlanJob job = PlanFullnessRecovery(jobType);
                if (job != null) {
                    owner.jobQueue.AddJobInQueue(job);
                }
                return true;
            }
        } 
        return false;
    }
    public bool PlanFullnessRecoveryActionsForFreeTime(out JobQueueItem producedJob) {
        if (!owner.limiterComponent.canPerform) {
            producedJob = null;
            return false;
        }
        if (owner.traitContainer.HasTrait("Vampire")) {
            //Note: Instead of just restricting the vampires from doing fullness recovery when it is not After Midnight, we will restrict the vampires from doing normal fullness recovery any time
            //because their fullness recovery is now triggered in VampireBehaviour
            //We did this to eliminate the conflict between nocturnals and vampires, because when a character becomes nocturnal, the fullnesss recovery sched is switched to Early Night, however, if it becomes a Vampire, it will switch to After Midnight
            //Now, when a character becomes Nocturnal, after becoming a Vampire, the schedule will be on Early Night, not After Midnight
            producedJob = null;
            return false;
        }
        if (isStarving || isHungry) {
            if (!owner.jobQueue.HasJob(JOB_TYPE.FULLNESS_RECOVERY_URGENT, JOB_TYPE.FULLNESS_RECOVERY_ON_SIGHT, JOB_TYPE.FULLNESS_RECOVERY_NORMAL)) {
                JOB_TYPE jobType = JOB_TYPE.FULLNESS_RECOVERY_URGENT;
                if (isHungry) { jobType = JOB_TYPE.FULLNESS_RECOVERY_NORMAL; }
                GoapPlanJob job = PlanFullnessRecovery(jobType);
                if (job != null) {
                    producedJob = job;
                    return true;
                }
            }
        }
        producedJob = null;
        return false;
    }
    public bool PlanFullnessRecoveryActionsVampire() {
        if (!owner.limiterComponent.canPerform) { //character.doNotDisturb > 0 || !character.limiterComponent.canWitness
            return false;
        }
        if (isStarving) {
            if (!owner.jobQueue.HasJob(JOB_TYPE.FULLNESS_RECOVERY_URGENT, JOB_TYPE.FULLNESS_RECOVERY_ON_SIGHT)) {
                //If there is already a HUNGER_RECOVERY JOB and the character becomes Starving, replace HUNGER_RECOVERY with HUNGER_RECOVERY_STARVING only if that character is not doing the job already
                JobQueueItem hungerRecoveryJob = owner.jobQueue.GetJob(JOB_TYPE.FULLNESS_RECOVERY_NORMAL);
                if (hungerRecoveryJob != null) {
                    //Replace this with Hunger Recovery Starving only if the character is not doing the Hunger Recovery Job already
                    JobQueueItem currJob = owner.currentJob;
                    if (currJob == hungerRecoveryJob) {
                        return false;
                    } else {
                        hungerRecoveryJob.CancelJob();
                    }
                }
                JOB_TYPE jobType = JOB_TYPE.FULLNESS_RECOVERY_URGENT;
                GoapPlanJob job = PlanFullnessRecovery(jobType);
                if (job != null) {
                    owner.jobQueue.AddJobInQueue(job);
                }
                return true;
            }
        } else if (isHungry) {
            if (!owner.jobQueue.HasJob(JOB_TYPE.FULLNESS_RECOVERY_NORMAL)) {
                GoapPlanJob job = PlanFullnessRecovery(JOB_TYPE.FULLNESS_RECOVERY_NORMAL);
                if (job != null) {
                    owner.jobQueue.AddJobInQueue(job);
                }
                return true;
            }
        }
        return false;
    }
    private bool PlanFullnessRecoveryActionsWhileInActiveParty() {
        if (!owner.limiterComponent.canPerform) { //character.doNotDisturb > 0 || !character.limiterComponent.canWitness
            return false;
        }
        if (owner.traitContainer.HasTrait("Vampire")) {
            //Vampires should only eat in an active party if he is starving or if there is no party members that can see him
            bool shouldEat = false;
            if (!isStarving) {
                if (owner.partyComponent.isMemberThatJoinedQuest) {
                    CRIME_SEVERITY severity = owner.partyComponent.currentParty.partyFaction.GetCrimeSeverity(owner, owner, CRIME_TYPE.Vampire);
                    if (severity == CRIME_SEVERITY.None || severity == CRIME_SEVERITY.Unapplicable) {
                        //If party faction does not consider vampire a crime, vampire can eat
                        shouldEat = true;
                    }
                }
            } else {
                shouldEat = true;
            }
            if (!shouldEat) {
                return false;
            }
        }
        if (isStarving) {
            if (!owner.jobQueue.HasJob(JOB_TYPE.FULLNESS_RECOVERY_URGENT, JOB_TYPE.FULLNESS_RECOVERY_ON_SIGHT)) {
                //If there is already a HUNGER_RECOVERY JOB and the character becomes Starving, replace HUNGER_RECOVERY with HUNGER_RECOVERY_STARVING only if that character is not doing the job already
                JobQueueItem hungerRecoveryJob = owner.jobQueue.GetJob(JOB_TYPE.FULLNESS_RECOVERY_NORMAL);
                if (hungerRecoveryJob != null) {
                    //Replace this with Hunger Recovery Starving only if the character is not doing the Hunger Recovery Job already
                    JobQueueItem currJob = owner.currentJob;
                    if (currJob == hungerRecoveryJob) {
                        return false;
                    } else {
                        hungerRecoveryJob.CancelJob();
                    }
                }
                JOB_TYPE jobType = JOB_TYPE.FULLNESS_RECOVERY_URGENT;
                GoapPlanJob job = PlanFullnessRecoveryBase(jobType);
                if (job != null) {
                    owner.jobQueue.AddJobInQueue(job);
                }
                return true;
            }
        } else if (isHungry) {
            if (!owner.jobQueue.HasJob(JOB_TYPE.FULLNESS_RECOVERY_NORMAL)) {
                GoapPlanJob job = PlanFullnessRecoveryBase(JOB_TYPE.FULLNESS_RECOVERY_NORMAL);
                if (job != null) {
                    owner.jobQueue.AddJobInQueue(job);
                }
                return true;
            }
        }
        return false;
    }
    public void PlanFullnessRecoveryGlutton(out JobQueueItem producedJob) {
        producedJob = null;
        if (!owner.jobQueue.HasJob(JOB_TYPE.FULLNESS_RECOVERY_NORMAL)) {
            JOB_TYPE jobType = JOB_TYPE.FULLNESS_RECOVERY_NORMAL;
            producedJob = PlanFullnessRecovery(jobType);
        }
    }
    private GoapPlanJob PlanFullnessRecovery(JOB_TYPE jobType) {
        //This is to prevent the character from creating fullness recovery when he/she is in an active party
        //because when a character is in an active party the party controls the needs recovery that is why we must be sure that he/she will not create its own needs recovery
        //If the character is in an active party and must create a needs recovery, we bypass this checking, meaning the PlanFullnessRecoveryBase is called directly, see CheckExtremeNeedsWhileInActiveParty
        if (owner.partyComponent.isActiveMember) {
            return null;
        }
        return PlanFullnessRecoveryBase(jobType);
    }
    private GoapPlanJob PlanFullnessRecoveryBase(JOB_TYPE jobType) {
        if (!owner.limiterComponent.canDoFullnessRecovery) {
            //No matter what, if character has the limiter "cannot do fullness recovery", he will not do fullness recovery
#if DEBUG_LOG
            owner.logComponent.PrintLogIfActive($"\n{owner.name} is cannot do fullness recovery");
#endif
            return null;
        }
        //No matter what happens if the character is burning, he/she wil not trigger fullness recovery
        if (owner.traitContainer.HasTrait("Burning")) {
#if DEBUG_LOG
            owner.logComponent.PrintLogIfActive($"\n{owner.name} is burning will not plan fullness recovery...");
#endif
            return null;
        }
        //This base recovery creation function is different from tiredness/happiness because instead of adding the job in the job queue we only return the created job
        //The reason for this is the Glutton behaviour
        //Since our behaviours always had a function that adds the created job in queue after processing, we must not add them prematurely so as to avoid duplicates, hence, the reason we only return the created job
        bool triggerGrieving = false;
        Griefstricken griefstricken = owner.traitContainer.GetTraitOrStatus<Griefstricken>("Griefstricken");
        if (griefstricken != null) {
            triggerGrieving = UnityEngine.Random.Range(0, 100) < (25 * owner.traitContainer.stacks[griefstricken.name]);
        }
        if (!triggerGrieving) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, new GoapEffect(GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR), owner, owner);
            JobUtilities.PopulatePriorityLocationsForFullnessRecovery(owner, job);
            job.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[] { 20 });
            return job;
        } else {
            griefstricken.TriggerGrieving();
        }
        return null;
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
            owner.traitContainer.AddTrait(owner, "Sprightly");
            owner.movementComponent.SetNoRunExceptCombat(false);
            owner.movementComponent.SetNoRunWithoutException(false);
        }
        if (wasSpent) {
            owner.traitContainer.RemoveTrait(owner, "Spent");
        }
        if (wasDrained) {
            owner.traitContainer.RemoveTrait(owner, "Drained");
        }
        owner.movementComponent.UpdateSpeed();
    }
    private void OnSpent(bool wasSprightly, bool wasSpent, bool wasDrained) {
        if (!wasSpent) {
            owner.traitContainer.AddTrait(owner, "Spent");
            owner.movementComponent.SetNoRunExceptCombat(true);
            //owner.movementComponent.SetNoRunWithoutException(false);
        }
        if (wasSprightly) {
            owner.traitContainer.RemoveTrait(owner, "Sprightly");
        }
        if (wasDrained) {
            owner.traitContainer.RemoveTrait(owner, "Drained");
        }
        owner.movementComponent.UpdateSpeed();
    }
    private void OnDrained(bool wasSprightly, bool wasSpent, bool wasDrained) {
        if (!wasDrained) {
            owner.traitContainer.AddTrait(owner, "Drained");
            owner.movementComponent.SetNoRunExceptCombat(true);
            owner.movementComponent.SetNoRunWithoutException(true);
        }
        if (wasSprightly) {
            owner.traitContainer.RemoveTrait(owner, "Sprightly");
        }
        if (wasSpent) {
            owner.traitContainer.RemoveTrait(owner, "Spent");
        }
        owner.movementComponent.UpdateSpeed();
    }
    private void OnNormalStamina(bool wasSprightly, bool wasSpent, bool wasDrained) {
        if (wasDrained) {
            owner.traitContainer.RemoveTrait(owner, "Drained");
        }
        if (wasSprightly) {
            owner.traitContainer.RemoveTrait(owner, "Sprightly");
        }
        if (wasSpent) {
            owner.traitContainer.RemoveTrait(owner, "Spent");
        }
        owner.movementComponent.UpdateSpeed();
    }
    public void AdjustDoNotGetDrained(int amount) {
        doNotGetDrained += amount;
    }
    public void UpdateBaseStaminaDecreaseRate() {
        baseStaminaDecreaseRate = Mathf.RoundToInt(owner.characterClass.staminaReduction * (owner.raceSetting.staminaReductionMultiplier == 0f ? 1f : owner.raceSetting.staminaReductionMultiplier));
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
        // if (!wasHopeful) {
        //     owner.traitContainer.AddTrait(owner, "Hopeful");
        // }
        // if (wasDiscouraged) {
        //     owner.traitContainer.RemoveTrait(owner, "Discouraged");
        // }
        // if (wasHopeless) {
        //     owner.traitContainer.RemoveTrait(owner, "Hopeless");
        // }
    }
    private void OnDiscouraged(bool wasHopeful, bool wasDiscouraged, bool wasHopeless) {
        // if (!wasDiscouraged) {
        //     owner.traitContainer.AddTrait(owner, "Discouraged");
        // }
        // if (wasHopeful) {
        //     owner.traitContainer.RemoveTrait(owner, "Hopeful");
        // }
        // if (wasHopeless) {
        //     owner.traitContainer.RemoveTrait(owner, "Hopeless");
        // }
    }
    private void OnHopeless(bool wasHopeful, bool wasDiscouraged, bool wasHopeless) {
        // if (!wasHopeless) {
        //     owner.traitContainer.AddTrait(owner, "Hopeless");
        // }
        // if (wasHopeful) {
        //     owner.traitContainer.RemoveTrait(owner, "Hopeful");
        // }
        // if (wasDiscouraged) {
        //     owner.traitContainer.RemoveTrait(owner, "Discouraged");
        // }
    }
    private void OnNormalHope(bool wasHopeful, bool wasDiscouraged, bool wasHopeless) {
        // if (wasHopeless) {
        //     owner.traitContainer.RemoveTrait(owner, "Hopeless");
        // }
        // if (wasHopeful) {
        //     owner.traitContainer.RemoveTrait(owner, "Hopeful");
        // }
        // if (wasDiscouraged) {
        //     owner.traitContainer.RemoveTrait(owner, "Discouraged");
        // }
    }
    public void AdjustDoNotGetDiscouraged(int amount) {
        doNotGetDiscouraged += amount;
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
    public void OnCharacterFinishedJob(JobQueueItem job) {
#if DEBUG_LOG
        Debug.Log($"{GameManager.Instance.TodayLogString()}{owner.name} has finished job {job.ToString()}");
#endif
        //after doing an extreme needs type job, check again if the character needs to recover more of that need.
        if (job.jobType == JOB_TYPE.FULLNESS_RECOVERY_URGENT) {
            if (owner.traitContainer.HasTrait("Pest") || owner is Rat) {
                owner.traitContainer.AddTrait(owner, "Abstain Fullness");
            }
            PlanFullnessRecoveryActions();
        } else if (job.jobType == JOB_TYPE.ENERGY_RECOVERY_URGENT) {
            PlanTirednessRecoveryActions();
        } else if (job.jobType == JOB_TYPE.FULLNESS_RECOVERY_NORMAL) {
            if(owner.traitContainer.HasTrait("Pest") || owner is Rat) {
                owner.traitContainer.AddTrait(owner, "Abstain Fullness");
            }
        }
    }
    /// <summary>
    /// Make this character plan a starving fullness recovery job, regardless of actual
    /// fullness level. NOTE: This will also cancel any existing fullness recovery jobs
    /// </summary>
    public void TriggerFlawFullnessRecovery(Character character) {
        //In trigger flaw, we bypass if the character is in an active party checking because whether or not he/she is in an active party, trigger flaw must always happen
        GoapPlanJob job = PlanFullnessRecoveryBase(JOB_TYPE.TRIGGER_FLAW);
        if(job != null){
            character.jobQueue.AddJobInQueue(job);
        }
    }
#endregion

#region Loading
    public void LoadReferences(SaveDataCharacterNeedsComponent data) {
        //Currently N/A
    }
#endregion
}

[System.Serializable]
public class SaveDataCharacterNeedsComponent : SaveData<CharacterNeedsComponent> {
    //Needs Component
    public int doNotGetHungry;
    public int doNotGetTired;
    public int doNotGetBored;
    public int doNotGetDrained;
    public int doNotGetDiscouraged;

    //Tiredness
    public float tiredness;
    public float tirednessDecreaseRate;
    // public int tirednessForcedTick;
    // public int currentSleepTicks;
    // public string sleepScheduleJobID;
    // public bool hasCancelledSleepSchedule;

    //Fullness
    public float fullness;
    public float fullnessDecreaseRate;
    // public int fullnessForcedTick;

    //Happiness
    public float happiness;
    public float happinessDecreaseRate;
    public int happinessSecondForcedTick;

    //Stamina
    public float stamina;
    public float staminaDecreaseRate;
    public float baseStaminaDecreaseRate;

    //Hope
    public float hope;

    public bool hasForcedFullness;
    public bool hasForcedTiredness;
    public bool hasForcedSecondHappiness;
    // public TIME_IN_WORDS forcedFullnessRecoveryTimeInWords;
    // public TIME_IN_WORDS forcedTirednessRecoveryTimeInWords;
    public TIME_IN_WORDS[] forcedHappinessRecoveryChoices;

#region Overrides
    public override void Save(CharacterNeedsComponent data) {
        doNotGetHungry = data.doNotGetHungry;
        doNotGetTired = data.doNotGetTired;
        doNotGetBored = data.doNotGetBored;
        doNotGetDrained = data.doNotGetDrained;
        doNotGetDiscouraged = data.doNotGetDiscouraged;

        tiredness = data.tiredness;
        tirednessDecreaseRate = data.tirednessDecreaseRate;
        // tirednessForcedTick = data.tirednessForcedTick;
        // currentSleepTicks = data.currentSleepTicks;
        // sleepScheduleJobID = data.sleepScheduleJobID;
        // hasCancelledSleepSchedule = data.hasCancelledSleepSchedule;

        fullness = data.fullness;
        fullnessDecreaseRate = data.fullnessDecreaseRate;
        // fullnessForcedTick = data.fullnessForcedTick;

        happiness = data.happiness;
        happinessDecreaseRate = data.happinessDecreaseRate;
        // happinessSecondForcedTick = data.happinessSecondForcedTick;

        stamina = data.stamina;
        staminaDecreaseRate = data.staminaDecreaseRate;
        baseStaminaDecreaseRate = data.baseStaminaDecreaseRate;

        hope = data.hope;

        hasForcedFullness = data.hasForcedFullness;
        hasForcedTiredness = data.hasForcedTiredness;
        hasForcedSecondHappiness = data.hasForcedSecondHappiness;
        // forcedFullnessRecoveryTimeInWords = data.forcedFullnessRecoveryTimeInWords;
        // forcedTirednessRecoveryTimeInWords = data.forcedTirednessRecoveryTimeInWords;
        // forcedHappinessRecoveryChoices = data.forcedHappinessRecoveryChoices;
    }

    public override CharacterNeedsComponent Load() {
        CharacterNeedsComponent component = new CharacterNeedsComponent(this);
        return component;
    }
#endregion
}