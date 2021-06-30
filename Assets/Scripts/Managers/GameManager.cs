using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Inner_Maps;
using Object_Pools;
using Ruinarch;
using Traits;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Serialization;
using UtilityScripts;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class GameManager : BaseMonoBehaviour {

	public static GameManager Instance;

    public static string[] daysInWords = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

    [FormerlySerializedAs("month")] public int startMonth;
    [FormerlySerializedAs("days")] public int startDay;
	[FormerlySerializedAs("year")] public int startYear;
    [FormerlySerializedAs("tick")] public int startTick;
    public int continuousDays;
    public const int daysPerMonth = 30;
    public const int ticksPerDay = 480;
    public const int ticksPerHour = 20;
    private const int minutesPerTick = 3;
    
    public PROGRESSION_SPEED currProgressionSpeed;

	public float progressionSpeed;
	public bool isPaused { get; private set; }
    public bool displayFPS = true;
    public bool showFullDebug;
    public static bool showAllTilesTooltip = false;

    [Header("Particle Effects")]
    [SerializeField] private GameObject aoeParticlesPrefab;
    [SerializeField] private GameObject aoeParticlesAutoDestroyPrefab;
    [SerializeField] private ParticleEffectAssetDictionary particleEffectsDictionary;

    private const float X1_SPEED = 0.8f;
    private const float X2_SPEED = 0.55f;
    private const float X4_SPEED = 0.3f;

    private float timeElapsed;
    private bool _gameHasStarted;
    public string lastProgressionBeforePausing; //what was the last progression speed before the player paused the game. NOTE: This includes paused state
    private static GameDate today;

    private List<BaseParticleEffect> _activeEffects;
    public static Stopwatch stopwatch;
    
    
    #region getters/setters
    public bool gameHasStarted => _gameHasStarted;
    public int currentTick => today.tick;
    #endregion

    #region Monobehaviours
    private void Awake() {
        Instance = this;
        timeElapsed = 0f;
        _gameHasStarted = false;
        InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Default);
        _activeEffects = new List<BaseParticleEffect>();
        stopwatch = new Stopwatch();
    }
    protected override void OnDestroy() {
        base.OnDestroy();
        _activeEffects?.Clear();
        _activeEffects = null;
    }
    private void OnKeyDown(KeyCode keyCode) {
        if (keyCode == KeyCode.BackQuote) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UIManager.Instance.ToggleConsole();
#endif
        } else if (keyCode == KeyCode.Space) {
            if (!UIManager.Instance.IsConsoleShowing() && UIManager.Instance.pauseBtn.IsInteractable() && !InputManager.Instance.HasSelectedUIObject()) {
                if (isPaused) {
                    UIManager.Instance.Unpause();
                } else {
                    UIManager.Instance.PauseByPlayer();
                }
            }
        } else if (keyCode == KeyCode.Minus || keyCode == KeyCode.KeypadMinus) {
            if (!UIManager.Instance.IsConsoleShowing() && !InputManager.Instance.HasSelectedUIObject()) {
                if (!isPaused) {
                    switch (currProgressionSpeed) {
                        case PROGRESSION_SPEED.X1:
                            if (UIManager.Instance.pauseBtn.IsInteractable()) {
                                UIManager.Instance.PauseByPlayer();    
                            }
                            break;
                        case PROGRESSION_SPEED.X2:
                            UIManager.Instance.SetProgressionSpeed1X();
                            break;
                        case PROGRESSION_SPEED.X4:
                            UIManager.Instance.SetProgressionSpeed2X();
                            break;
                    }
                }
            }
        } else if (keyCode == KeyCode.Plus || keyCode == KeyCode.Equals || keyCode == KeyCode.KeypadPlus) {
            if (isPaused) {
                if (UIManager.Instance.pauseBtn.IsInteractable()) {
                    UIManager.Instance.Unpause();    
                }
            } else {
                switch (currProgressionSpeed) {
                    case PROGRESSION_SPEED.X1:
                        UIManager.Instance.SetProgressionSpeed2X();
                        break;
                    case PROGRESSION_SPEED.X2:
                        UIManager.Instance.SetProgressionSpeed4X();
                        break;
                    case PROGRESSION_SPEED.X4:
                        //do nothing, since 4x is the max speed
                        break;
                }
            }
        }
        
        // else if (keyCode == KeyCode.Alpha1) {
        //     if (!UIManager.Instance.IsConsoleShowing() && UIManager.Instance.x1Btn.IsInteractable() && !InputManager.Instance.HasSelectedUIObject()) {
        //         UIManager.Instance.SetProgressionSpeed1X();
        //     }
        // } else if (keyCode == KeyCode.Alpha2) {
        //     if (!UIManager.Instance.IsConsoleShowing() && UIManager.Instance.x2Btn.IsInteractable() && !InputManager.Instance.HasSelectedUIObject()) {
        //         UIManager.Instance.SetProgressionSpeed2X();
        //     }
        // } else if (keyCode == KeyCode.Alpha3) {
        //     if (!UIManager.Instance.IsConsoleShowing() && UIManager.Instance.x4Btn.IsInteractable() && !InputManager.Instance.HasSelectedUIObject()) {
        //         UIManager.Instance.SetProgressionSpeed4X();
        //     }
        // }
    }
    
    private void Update() {
        if (_gameHasStarted && !isPaused) {
#if DEBUG_PROFILER
            Profiler.BeginSample("Tick Started Call");
#endif
            if (Math.Abs(timeElapsed) <= 0f) {
                TickStarted();
            }
#if DEBUG_PROFILER
            Profiler.EndSample();
#endif
            timeElapsed += Time.deltaTime;
            if (timeElapsed >= progressionSpeed) {
                timeElapsed = 0f;
#if DEBUG_PROFILER
                Profiler.BeginSample("Tick Ended Call");
#endif
                TickEnded();
#if DEBUG_PROFILER
                Profiler.EndSample();
#endif
            }
        }
    }
#endregion
    public void Initialize() {
        today = new GameDate(startMonth, startDay, startYear, startTick);
        BaseParticleEffect.particleEffectActivated = AddActiveEffect;
        BaseParticleEffect.particleEffectDeactivated = RemoveActiveEffect;
    }
    
	public void StartProgression(){
        WorldConfigManager.Instance.mapGenerationData?.CleanUpAfterMapGeneration();
        _gameHasStarted = true;
        UIManager.Instance.Pause();
        lastProgressionBeforePausing = "paused";
        Messenger.Broadcast(Signals.GAME_STARTED);
        SchedulingManager.Instance.StartScheduleCalls ();
        Messenger.Broadcast(Signals.DAY_STARTED); //for the first day
        Messenger.Broadcast(Signals.MONTH_START); //for the first month
        Messenger.AddListener<KeyCode>(ControlsSignals.KEY_DOWN, OnKeyDown);
        if (WorldSettings.Instance.worldSettingsData.IsScenarioMap() && !SaveManager.Instance.useSaveData) {
            string message = LocalizationManager.Instance.GetLocalizedValue("Scenarios", $"{WorldSettings.Instance.worldSettingsData.worldType.ToString()}_Text", "popup_text");
            if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Pangat_Loo) {
                message = UtilityScripts.Utilities.StringReplacer(message, new List<LogFillerStruct>() {
                    new LogFillerStruct(null, WipeOutAllUntilDayWinConditionTracker.DueDay.ToString(), LOG_IDENTIFIER.STRING_1)
                });
            }
            if (!string.IsNullOrEmpty(message)) {
                message = message.Replace("\\n", "\n");
                UIManager.Instance.ShowStartScenario(message);    
            }
        } else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Custom && !SaveManager.Instance.useSaveData) {
            string message = "The key to winning the game is in knowing how to produce <color=#FB6F37>Chaos Orbs</color>. In early game, try using various <color=#FB6F37>traps</color> and damaging <color=#FB6F37>Spells</color>. " +
                             "As you upgrade your <color=#FB6F37>Portal</color>, learn which Powers can provide Chaos Orbs by going through their tooltip descriptions.";
            UIManager.Instance.ShowStartScenario(message);
        }
        Canvas.ForceUpdateCanvases();
    }
    public void LoadProgression(){
        _gameHasStarted = true;
        UIManager.Instance.Pause();
        lastProgressionBeforePausing = "paused";
        SchedulingManager.Instance.StartScheduleCalls ();
        Messenger.AddListener<KeyCode>(ControlsSignals.KEY_DOWN, OnKeyDown);
        Canvas.ForceUpdateCanvases();
        Messenger.Broadcast(Signals.PROGRESSION_LOADED);
    }
    public GameDate Today() {
        return new GameDate(today.month, today.day, today.year, today.tick);
    }
    public void SetToday(GameDate date) {
        today = date;
    }
    public string TodayLogString() {
        return $"[{continuousDays.ToString()} - {ConvertTickToTime(today.tick)}] ";
    }
    public bool IsEndOfDay() {
        return today.tick == ticksPerDay;
    }
    public void SetPausedState(bool isPaused){
        if (isPaused) {
            StoreLastProgressionBeforePausing();
        }
        if (this.isPaused != isPaused) {
            this.isPaused = isPaused;
            UpdateActiveEffectsOnPauseChanged(isPaused);
            Messenger.Broadcast(UISignals.PAUSED, isPaused);
        }
	}
    private void StoreLastProgressionBeforePausing() {
        //the player paused the game
        if (isPaused) {
            lastProgressionBeforePausing = "paused";
        } else {
            switch (currProgressionSpeed) {
                case PROGRESSION_SPEED.X1:
                    lastProgressionBeforePausing = "1";
                    break;
                case PROGRESSION_SPEED.X2:
                    lastProgressionBeforePausing = "2";
                    break;
                case PROGRESSION_SPEED.X4:
                    lastProgressionBeforePausing = "4";
                    break;
            }
        }
    }
    public void SetDelayedPausedState(bool state) {
        StartCoroutine(DelayedPausedState(state));
    }
    private IEnumerator DelayedPausedState(bool state) {
        yield return null;
        SetPausedState(state);
    }
    public void SetProgressionSpeed(PROGRESSION_SPEED progressionSpeed){
        currProgressionSpeed = progressionSpeed;
        float speed = X1_SPEED;
        if (progressionSpeed == PROGRESSION_SPEED.X2) {
            speed = X2_SPEED;
        } else if(progressionSpeed == PROGRESSION_SPEED.X4){
            speed = X4_SPEED;
        }
		this.progressionSpeed = speed;
        //CombatManager.Instance.updateIntervals = this.progressionSpeed / (float) CombatManager.Instance.numOfCombatActionPerDay;
        Messenger.Broadcast(UISignals.PROGRESSION_SPEED_CHANGED, progressionSpeed);
	}
    /// <summary>
    /// Get how many seconds in realtime a tick is.
    /// <param name="progressionSpeed">The progression speed to factor in.</param>
    /// </summary>
    /// <returns>Float value representing ticks in realtime seconds.</returns>
    public float GetTickSpeed(PROGRESSION_SPEED progressionSpeed) {
        switch (progressionSpeed) {
            case PROGRESSION_SPEED.X1:
                return X1_SPEED;
            case PROGRESSION_SPEED.X2:
                return X2_SPEED;
            case PROGRESSION_SPEED.X4:
                return X4_SPEED;
        }
        throw new Exception($"Could not get tick speed from {currProgressionSpeed.ToString()}");
    }
    private void TickStarted() {
        if (today.tick % ticksPerHour == 0 && !IsStartOfGame()) {
            //hour reached
#if DEBUG_PROFILER
            Profiler.BeginSample("Hour Started");
#endif
            Messenger.Broadcast(Signals.HOUR_STARTED);
#if DEBUG_PROFILER
            Profiler.EndSample();
#endif
        }
#if DEBUG_PROFILER
        Profiler.BeginSample("Tick Started Signal");
#endif

        ////Note: When the GC.Collect command is after the Debug.Log, the wr.Target does not get collected by garbage if we do the "wr.Target.ToString()"
        ////The reason is because, when the supposed dead object (which is the Target) is accessed on the same scope the GC.Collect is called, the GC will skip the collection of it because it knows that it was just accessed
        //if (DatabaseManager.Instance.tileObjectDatabase.destroyedTileObjects.Count > 0) {
        //    GC.Collect();
        //    string test = "Destroyed Tile Objects: " + DatabaseManager.Instance.tileObjectDatabase.destroyedTileObjects.Count;
        //    for (int i = 0; i < DatabaseManager.Instance.tileObjectDatabase.destroyedTileObjects.Count; i++) {
        //        WeakReference wr = DatabaseManager.Instance.tileObjectDatabase.destroyedTileObjects[i];
        //        if (wr.IsAlive) {
        //            test += "\n" + wr.Target.ToString() + " -> " + wr.IsAlive;
        //        } else {
        //            test += "\n" + wr.IsAlive;
        //        }
        //    }
        //    Debug.Log(test);
        //}

        Messenger.Broadcast(Signals.TICK_STARTED);
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif

#if DEBUG_PROFILER
        Profiler.BeginSample("Tick Started - Update UI");
#endif
        Messenger.Broadcast(UISignals.UPDATE_UI);
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif
    }
    private void TickEnded() {
#if DEBUG_PROFILER
        Profiler.BeginSample("Check Schedules");
#endif
        Messenger.Broadcast(Signals.CHECK_SCHEDULES);
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif

#if DEBUG_PROFILER
        Profiler.BeginSample("Character Tick Ended Movement");
#endif
        Messenger.Broadcast(CharacterSignals.CHARACTER_TICK_ENDED_MOVEMENT);
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif

#if DEBUG_PROFILER
        Profiler.BeginSample("Process All Unprocessed POIS");
#endif
        Messenger.Broadcast(CharacterSignals.PROCESS_ALL_UNPOROCESSED_POIS);
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif

#if DEBUG_PROFILER
        Profiler.BeginSample("Character Tick Ended");
#endif
        Messenger.Broadcast(CharacterSignals.CHARACTER_TICK_ENDED);
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif

#if DEBUG_PROFILER
        Profiler.BeginSample("Generic Tick Ended Signal");
#endif
        Messenger.Broadcast(Signals.TICK_ENDED);
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif

        today.tick += 1;
        if (today.tick > ticksPerDay) {
            today.tick = 1;
#if DEBUG_PROFILER
            Profiler.BeginSample("Tick Ended - Day Started");
#endif
            DayStarted(false);
#if DEBUG_PROFILER
            Profiler.EndSample();
#endif
        }
#if DEBUG_PROFILER
        Profiler.BeginSample("Tick Ended - Update UI");
#endif
        Messenger.Broadcast(UISignals.UPDATE_UI);
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif
    }
    public void SetTick(int amount) {
        today.tick = amount;
        Messenger.Broadcast(UISignals.UPDATE_UI);
    }
    private void DayStarted(bool broadcastUI = true) {
        today.day += 1;
        continuousDays += 1;
        Messenger.Broadcast(Signals.DAY_STARTED);
        if (today.day > daysPerMonth) {
            today.day = 1;
            today.month += 1;
            if (today.month > 12) {
                today.month = 1;
                today.year += 1;
            }
            Messenger.Broadcast(Signals.MONTH_START);
        }
        if (broadcastUI) {
            Messenger.Broadcast(UISignals.UPDATE_UI);
        }
    }
    public string ConvertTickToTime(int tick, string timeSeparator = ":") {
        float floatConversion = tick / (float) ticksPerHour;
        int hour = (int) floatConversion;
        int minutes = Mathf.RoundToInt((floatConversion - hour) * ticksPerHour * minutesPerTick);
        string timeOfDay = "AM";
        if(hour >= 12) {
            if(hour < 24) {
                timeOfDay = "PM";
            }
            hour -= 12;
        }
        if(hour == 0) {
            hour = 12;
        }
        return $"{hour}{timeSeparator}{minutes:D2} {timeOfDay}";
    }
    public TIME_IN_WORDS GetTimeInWordsOfTick(int tick) {
        //We use float instead of integer so that the decimal places will not be truncated
        //Example: AFTER MIDNIGHT is only up to 5:00 AM Sharp. If we use integer, tick 61 or 5:03 AM will still be AFTER MIDNIGHT instead of MORNING because the decimal places are truncated
        float currentHourInFloat = GetHoursBasedOnTicksInFloat(tick); 
        //MILITARY TIME
        if ((currentHourInFloat > 22 && currentHourInFloat <= 24) || (currentHourInFloat >= 0 && currentHourInFloat <= 5)) {
            return TIME_IN_WORDS.AFTER_MIDNIGHT;
        }
        if (currentHourInFloat > 5 && currentHourInFloat <= 11) {
            return TIME_IN_WORDS.MORNING;
        }
        if (currentHourInFloat > 11 && currentHourInFloat <= 13) {
            return TIME_IN_WORDS.LUNCH_TIME;
        }
        if (currentHourInFloat > 13 && currentHourInFloat <= 17) {
            return TIME_IN_WORDS.AFTERNOON;
        }
        if (currentHourInFloat > 17 && currentHourInFloat <= 20) {
            return TIME_IN_WORDS.EARLY_NIGHT;
        }
        if (currentHourInFloat > 20 && currentHourInFloat <= 22) {
            return TIME_IN_WORDS.LATE_NIGHT;
        }
        return TIME_IN_WORDS.NONE;
    }

    //Note: If there is a character parameter, it means that the current time in words might not be the actual one because we will get the time in words relative to the character
    //Example: If the character is Nocturnal, MORNING will become LATE_NIGHT
    public TIME_IN_WORDS GetCurrentTimeInWordsOfTick(Character relativeTo = null) {
        int currentTick = today.tick;
        TIME_IN_WORDS time = GetTimeInWordsOfTick(currentTick);
        if(relativeTo != null && relativeTo.traitContainer.HasTrait("Nocturnal")) {
            time = ConvertTimeInWordsWhenNocturnal(time);
        }
        return time;
        //float time = GameManager.Instance.tick / (float) ticksPerTimeInWords;
        //int intTime = (int) time;
        //if (time == intTime && intTime > 0) {
        //    //This will make sure that the 12th tick is still part of the previous time in words
        //    //Example: In ticks 1 - 11, the intTime is 0 (AFTER_MIDNIGHT_1), however, in tick 12, intTime is already 1 (AFTER_MIDNIGHT_2), but we still want it to be part of AFTER_MIDNIGHT_1
        //    //Hence, this checker ensures that tick 12's intTime is 0
        //    intTime -= 1;
        //}
        //return timeInWords[intTime];
    }
    public int GetRandomTickFromTimeInWords(TIME_IN_WORDS timeInWords) {
        //NOTE: The passed parameter value in GetTicksBasedOnHour must be military time, i.e., 6PM = 18
        if (timeInWords == TIME_IN_WORDS.AFTER_MIDNIGHT) {
            //After Midnight has special processing because it goes beyond the max tick, its 10:00PM to 5:00AM 
            int maxRange = ticksPerDay + GetTicksBasedOnHour(5); //60;
            //Min value is GetTicksBasedOnHour(22) + 1 because it should start on 10:03PM, 10:00PM is still part of LATE NIGHT 
            int chosenTick = GameUtilities.RandomBetweenTwoNumbers(GetTicksBasedOnHour(22) + 1, maxRange);
            if(chosenTick > ticksPerDay) {
                chosenTick -= ticksPerDay;
            }
            return chosenTick;
        }
        if (timeInWords == TIME_IN_WORDS.MORNING) {
            return GameUtilities.RandomBetweenTwoNumbers(GetTicksBasedOnHour(5) + 1, GetTicksBasedOnHour(11));
        }
        if (timeInWords == TIME_IN_WORDS.LUNCH_TIME) {
            return GameUtilities.RandomBetweenTwoNumbers(GetTicksBasedOnHour(11) + 1, GetTicksBasedOnHour(13));
        }
        if (timeInWords == TIME_IN_WORDS.AFTERNOON) {
            return GameUtilities.RandomBetweenTwoNumbers(GetTicksBasedOnHour(13) + 1, GetTicksBasedOnHour(17));
        }
        if (timeInWords == TIME_IN_WORDS.EARLY_NIGHT) {
            return GameUtilities.RandomBetweenTwoNumbers(GetTicksBasedOnHour(17) + 1, GetTicksBasedOnHour(20));
        }
        if (timeInWords == TIME_IN_WORDS.LATE_NIGHT) {
            return GameUtilities.RandomBetweenTwoNumbers(GetTicksBasedOnHour(20) + 1, GetTicksBasedOnHour(22));
        }
        throw new Exception($"{timeInWords} time in words has no tick!");
    }
    public int GetRandomTickFromTimeInWords(TIME_IN_WORDS timeInWords, int minimumThreshold) {
        //NOTE: The passed parameter value in GetTicksBasedOnHour must be military time, i.e., 6PM = 18
        if (timeInWords == TIME_IN_WORDS.AFTER_MIDNIGHT) {
            //After Midnight has special processing because it goes beyond the max tick, its 10:00PM to 5:00AM 
            int maxRange = ticksPerDay + GetTicksBasedOnHour(5); //60;
            //Min value is GetTicksBasedOnHour(22) + 1 because it should start on 10:03PM, 10:00PM is still part of LATE NIGHT 
            int chosenTick = GameUtilities.RandomBetweenTwoNumbers(minimumThreshold, maxRange);
            if (chosenTick > ticksPerDay) {
                chosenTick -= ticksPerDay;
            }
            return chosenTick;
        }
        if (timeInWords == TIME_IN_WORDS.MORNING) {
            return GameUtilities.RandomBetweenTwoNumbers(minimumThreshold, GetTicksBasedOnHour(11));
        }
        if (timeInWords == TIME_IN_WORDS.LUNCH_TIME) {
            return GameUtilities.RandomBetweenTwoNumbers(minimumThreshold, GetTicksBasedOnHour(13));
        }
        if (timeInWords == TIME_IN_WORDS.AFTERNOON) {
            return GameUtilities.RandomBetweenTwoNumbers(minimumThreshold, GetTicksBasedOnHour(17));
        }
        if (timeInWords == TIME_IN_WORDS.EARLY_NIGHT) {
            return GameUtilities.RandomBetweenTwoNumbers(minimumThreshold, GetTicksBasedOnHour(20));
        }
        if (timeInWords == TIME_IN_WORDS.LATE_NIGHT) {
            return GameUtilities.RandomBetweenTwoNumbers(minimumThreshold, GetTicksBasedOnHour(22));
        }
        throw new Exception($"{timeInWords} time in words has no tick!");
    }
    public TIME_IN_WORDS[] ConvertTimeInWordsWhenNocturnal(TIME_IN_WORDS[] currentTimeInWords) {
        TIME_IN_WORDS[] convertedTimeInWords = new TIME_IN_WORDS[currentTimeInWords.Length];
        for (int i = 0; i < currentTimeInWords.Length; i++) {
            convertedTimeInWords[i] = ConvertTimeInWordsWhenNocturnal(currentTimeInWords[i]);
        }
        return convertedTimeInWords;
    }
    public TIME_IN_WORDS ConvertTimeInWordsWhenNocturnal(TIME_IN_WORDS currentTimeInWords) {
        if (currentTimeInWords == TIME_IN_WORDS.MORNING) {
            return TIME_IN_WORDS.LATE_NIGHT;
        }
        if (currentTimeInWords == TIME_IN_WORDS.LUNCH_TIME) {
            return TIME_IN_WORDS.AFTER_MIDNIGHT;
        }
        if (currentTimeInWords == TIME_IN_WORDS.AFTERNOON) {
            return TIME_IN_WORDS.AFTER_MIDNIGHT;
        }
        if (currentTimeInWords == TIME_IN_WORDS.EARLY_NIGHT) {
            return TIME_IN_WORDS.MORNING;
        }
        if (currentTimeInWords == TIME_IN_WORDS.LATE_NIGHT) {
            return TIME_IN_WORDS.MORNING;
        }
        if (currentTimeInWords == TIME_IN_WORDS.AFTER_MIDNIGHT) {
            return TIME_IN_WORDS.AFTERNOON;
        }
        return TIME_IN_WORDS.NONE;
    }
    public int GetTicksBasedOnHour(int hours) {
        return ticksPerHour * hours;
    }
    public int GetTicksBasedOnMinutes(int minutes) {
        float percent = minutes/60f;
        return Mathf.FloorToInt(ticksPerHour * percent);
    }
    public int GetHoursBasedOnTicks(int ticks) {
        return ticks / ticksPerHour;
    }
    private float GetHoursBasedOnTicksInFloat(int ticks) {
        return ticks / (float) ticksPerHour;
    }
    public int GetMinutesBasedOnTicks(int ticks) {
        return ticks * minutesPerTick;
    }
    public int GetCeilingHoursBasedOnTicks(int ticks) {
        return Mathf.CeilToInt(ticks / (float) ticksPerHour);
    }
    public int GetCeilingDaysBasedOnTicks(int ticks) {
        return Mathf.CeilToInt(ticks / (float) ticksPerDay);
    }
    public int GetCeilingMinsBasedOnTicks(int ticks) {
        return ticks * minutesPerTick;
    }
    public static string ConvertTicksToWholeTime(int ticks) {
        string converted = string.Empty;
        List<string> times = RuinarchListPool<string>.Claim();
        int ticksRemaining = ticks;
        if (ticksRemaining >= ticksPerDay) {
            int days = Mathf.FloorToInt(ticksRemaining / (float) ticksPerDay);
            // converted = $"{converted}{days.ToString()}";
            string formatted = days == 1 ? $"{days.ToString()} day" : $"{days.ToString()} days";
            times.Add(formatted);
            // converted = formatted;
            ticksRemaining -= days * ticksPerDay;
        }
        if (ticksRemaining >= ticksPerHour) {
            int hours = Mathf.FloorToInt(ticksRemaining / (float) ticksPerHour);
            // converted = $"{converted}{hours.ToString()}";
            string formatted = hours == 1 ? $"{hours.ToString()} hour" : $"{hours.ToString()} hours";
            times.Add(formatted);
            // converted = formatted;
            ticksRemaining -= hours * ticksPerHour;
        }
        if (ticksRemaining > 0) {
            int minutes = ticksRemaining * minutesPerTick;
            // converted = $"{converted}{minutes.ToString()}";
            string formatted = minutes == 1 ? $"{minutes.ToString()} min" : $"{minutes.ToString()} mins";
            times.Add(formatted);
            // converted = formatted;
        }
        converted = times.ComafyList();
        RuinarchListPool<string>.Release(times);
        return converted;
    }
    public static int GetTimeAsWholeDuration(int ticks) {
        //Returns duration not as ticks but as time
        //If ticks exceeds a day, returns time as day
        //If ticks exceeds an hour, returns time as hour
        //If ticks exceeds a mins, returns time as mins

        if (ticks >= ticksPerDay) {
            return Mathf.CeilToInt(ticks / (float) ticksPerDay);
        } else if(ticks >= ticksPerHour) {
            return Mathf.CeilToInt(ticks / (float) ticksPerHour);
        } else {
            return ticks * minutesPerTick;
        }
    }
    public static string GetTimeIdentifierAsWholeDuration(int ticks) {
        //Returns duration not as ticks but as time
        //If ticks exceeds a day, returns time as day
        //If ticks exceeds an hour, returns time as hour
        //If ticks exceeds a mins, returns time as mins
        if (ticks > ticksPerDay) {
            return "days";
        } else if (ticks == ticksPerDay) {
            return "day";
        } else if (ticks > ticksPerHour) {
            return "hrs";
        } else if (ticks == ticksPerHour) {
            return "hr";
        } else {
            return "mins";
        }
    }

#region Particle Effects
    public GameObject CreateParticleEffectAt(Vector3 worldLocation, InnerTileMap innerTileMap, PARTICLE_EFFECT particle, int sortingOrder = -1) {
        GameObject prefab = null;
        GameObject go = null;
        if (particleEffectsDictionary.ContainsKey(particle)) {
            prefab = particleEffectsDictionary[particle];
        } else {
            Debug.LogError("No prefab for particle effect: " + particle.ToString());
            return null;
        }
        go = ObjectPoolManager.Instance.InstantiateObjectFromPool(prefab.name, Vector3.zero, Quaternion.identity, innerTileMap.objectsParent);
        go.transform.position = worldLocation;
        go.SetActive(true);
        BaseParticleEffect particleEffectScript = go.GetComponent<BaseParticleEffect>();
        if (particleEffectScript) {
            if(sortingOrder != -1) {
                particleEffectScript.SetSortingOrder(sortingOrder);
            }
            particleEffectScript.PlayParticleEffect();
        }
        return go;
    }
    public GameObject CreateParticleEffectAt(LocationGridTile tile, PARTICLE_EFFECT particle, int sortingOrder = -1) {
        GameObject prefab = null;
        GameObject go = null;
        if (particleEffectsDictionary.ContainsKey(particle)) {
            prefab = particleEffectsDictionary[particle];
        } else {
            Debug.LogError("No prefab for particle effect: " + particle.ToString());
            return null;
        }
        go = ObjectPoolManager.Instance.InstantiateObjectFromPool(prefab.name, Vector3.zero, Quaternion.identity, tile.parentMap.objectsParent);
        go.transform.localPosition = tile.centeredLocalLocation;
        go.SetActive(true);
        BaseParticleEffect particleEffectScript = go.GetComponent<BaseParticleEffect>();
        if (particleEffectScript) {
            if(sortingOrder != -1) {
                particleEffectScript.SetSortingOrder(sortingOrder);
            }
            particleEffectScript.SetTargetTile(tile);
            particleEffectScript.PlayParticleEffect();
        }
        return go;
    }
    public GameObject CreateParticleEffectAt(ThinWall wallObject, PARTICLE_EFFECT particle) {
        GameObject prefab = null;
        GameObject go = null;
        if (particleEffectsDictionary.ContainsKey(particle)) {
            prefab = particleEffectsDictionary[particle];
        } else {
            Debug.LogError("No prefab for particle effect: " + particle.ToString());
            return null;
        }
        go = ObjectPoolManager.Instance.InstantiateObjectFromPool(prefab.name, Vector3.zero, Quaternion.identity, wallObject.mapVisual.transform);
        go.transform.localPosition = wallObject.mapVisual.objectSpriteRenderer.transform.localPosition;
        go.SetActive(true);
        // BaseParticleEffect particleEffectScript = go.GetComponent<BaseParticleEffect>();
        // if (particleEffectScript) {
        //     particleEffectScript.SetTargetTile(wallObject);
        //     particleEffectScript.PlayParticleEffect();
        // }
        return go;
    }
    public GameObject CreateParticleEffectAt(IPointOfInterest poi, PARTICLE_EFFECT particle, bool allowRotation = true) {
        GameObject prefab = null;
        GameObject go = null;
        if (particleEffectsDictionary.ContainsKey(particle)) {
            prefab = particleEffectsDictionary[particle];
        } else {
            Debug.LogError("No prefab for particle effect: " + particle.ToString());
            return null;
        }
        if (poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            Character character = poi as Character;
            if (!character.marker) {
                return null;
            }
            Transform parent = character.marker.particleEffectParentAllowRotation;
            if (!allowRotation) {
                parent = character.marker.particleEffectParent;
            }
            go = ObjectPoolManager.Instance.InstantiateObjectFromPool(prefab.name, Vector3.zero, Quaternion.identity, parent);
            go.transform.localPosition = Vector3.zero;
            go.SetActive(true);
        } else {
            if (poi.mapObjectVisual) {
                go = ObjectPoolManager.Instance.InstantiateObjectFromPool(prefab.name, Vector3.zero, Quaternion.identity, poi.mapObjectVisual.particleEffectParent.transform);
                go.transform.localPosition = Vector3.zero;
            } else {
                if (poi.gridTileLocation == null) { // || poi.mapObjectVisual == null
                    return null;
                }
                go = ObjectPoolManager.Instance.InstantiateObjectFromPool(prefab.name, Vector3.zero, Quaternion.identity, poi.gridTileLocation.parentMap.objectsParent);
                go.transform.localPosition = poi.gridTileLocation.centeredLocalLocation;
            }
            go.SetActive(true);
        }
        return go;
    }
    public GameObject CreateParticleEffectAtWithScale(LocationGridTile tile, PARTICLE_EFFECT particle, float p_scaleFactor = 1f, int sortingOrder = -1) {
        GameObject prefab = null;
        GameObject go = null;
        if (particleEffectsDictionary.ContainsKey(particle)) {
            prefab = particleEffectsDictionary[particle];
        } else {
            Debug.LogError("No prefab for particle effect: " + particle.ToString());
            return null;
        }
        go = ObjectPoolManager.Instance.InstantiateObjectFromPool(prefab.name, Vector3.zero, Quaternion.identity, tile.parentMap.objectsParent);
        go.transform.localPosition = tile.centeredLocalLocation;
        go.transform.GetChild(0).localScale = new Vector3(go.transform.GetChild(0).localScale.x * p_scaleFactor, go.transform.GetChild(0).localScale.y * p_scaleFactor, go.transform.GetChild(0).localScale.z);
        go.SetActive(true);
        BaseParticleEffect particleEffectScript = go.GetComponent<BaseParticleEffect>();
        if (particleEffectScript) {
            if (sortingOrder != -1) {
                particleEffectScript.SetSortingOrder(sortingOrder);
            }
            particleEffectScript.SetTargetTile(tile);
            particleEffectScript.PlayParticleEffect();
        }
        return go;
    }
    public AOEParticle CreateAOEEffectAt(LocationGridTile tile, int range, bool autoDestroy = false) {
        GameObject go;
        if (autoDestroy) {
            go = ObjectPoolManager.Instance.InstantiateObjectFromPool(aoeParticlesAutoDestroyPrefab.name, Vector3.zero, Quaternion.identity, tile.parentMap.objectsParent);
        } else {
            go = ObjectPoolManager.Instance.InstantiateObjectFromPool(aoeParticlesPrefab.name, Vector3.zero, Quaternion.identity, tile.parentMap.objectsParent);
        }
        AOEParticle particle = go.GetComponent<AOEParticle>();
        particle.PlaceParticleEffect(tile, range, autoDestroy);
        return particle;
    }
    private void AddActiveEffect(BaseParticleEffect p_effect) {
        _activeEffects?.Add(p_effect);
    }
    private void RemoveActiveEffect(BaseParticleEffect p_effect) {
        _activeEffects?.Remove(p_effect);
    }
    private void UpdateActiveEffectsOnPauseChanged(bool state) {
        for (int i = 0; i < _activeEffects.Count; i++) {
            BaseParticleEffect effect = _activeEffects[i];
            if (effect.pauseOnGamePaused) {
                effect.OnGamePaused(state);
            }
        }
    }
#endregion

#region For Testing
    [ContextMenu("Print Event Table")]
    public void PrintEventTable() {
        Messenger.PrintEventTable();
    }
#endregion

#region Utilities
    private bool IsStartOfGame() {
        if (today.year == startYear && today.month == startMonth && today.day == startDay && today.tick == startTick) {
            return true;
        }
        return false;
    }
#endregion

    public static Log CreateNewLog() {
        return LogPool.Claim();
    }
    public static Log CreateNewLog(GameDate date, string category, string file, string key, ActualGoapNode node = null, params LOG_TAG[] providedTags) {
        Log log = CreateNewLog();
        log.SetPersistentID(UtilityScripts.Utilities.GetNewUniqueID());
        log.SetDate(date);
        log.SetCategory(category);
        log.SetFile(file);
        log.SetKey(key);
        log.SetConnectedAction(node);
        log.AddTag(providedTags);
        log.DetermineInitialLogText();
        return log;
    }
    public static Log CreateNewLog(GameDate date, string category, string file, string key, List<LOG_TAG> providedTags, ActualGoapNode node = null) {
        Log log = CreateNewLog();
        log.SetPersistentID(UtilityScripts.Utilities.GetNewUniqueID());
        log.SetDate(date);
        log.SetCategory(category);
        log.SetFile(file);
        log.SetKey(key);
        log.SetConnectedAction(node);
        log.AddTag(providedTags);
        log.DetermineInitialLogText();
        return log;
    }
    public static Log CreateNewLog(GameDate date, string category, string file, string key, ActualGoapNode node = null, LOG_TAG providedTags = LOG_TAG.Work) {
        Log log = CreateNewLog();
        log.SetPersistentID(UtilityScripts.Utilities.GetNewUniqueID());
        log.SetDate(date);
        log.SetCategory(category);
        log.SetFile(file);
        log.SetKey(key);
        log.SetConnectedAction(node);
        log.AddTag(providedTags);
        log.DetermineInitialLogText();
        return log;
    }
    public static Log CreateNewLog(string id, GameDate date, string logText, string category, string key, string file, string involvedObjects, List<LOG_TAG> providedTags, string rawText, List<LogFillerStruct> fillers = null) {
        Log log = CreateNewLog();
        log.SetPersistentID(id);
        log.SetDate(date);
        log.SetLogText(logText);
        log.SetCategory(category);
        log.SetFile(file);
        log.SetKey(key);
        log.SetInvolvedObjects(involvedObjects);
        log.AddTag(providedTags);
        log.SetRawText(rawText);
        if (fillers != null) {
            log.SetFillers(fillers);    
        }
        return log;
    }
}
