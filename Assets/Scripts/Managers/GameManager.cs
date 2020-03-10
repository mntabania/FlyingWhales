using System;
using System.Collections;
using Inner_Maps;
using Ruinarch;
using Traits;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour {

	public static GameManager Instance;

    public static string[] daysInWords = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
    //public static TIME_IN_WORDS[] timeInWords = new TIME_IN_WORDS[] {
    //    TIME_IN_WORDS.AFTER_MIDNIGHT
    //    , TIME_IN_WORDS.MORNING
    //    , TIME_IN_WORDS.LUNCH_TIME
    //    , TIME_IN_WORDS.AFTERNOON
    //    , TIME_IN_WORDS.EARLY_NIGHT
    //    , TIME_IN_WORDS.LATE_NIGHT };


    public int month;
    public static int days;
	public int year;
    public int tick;
    public int continuousDays;
    public const int daysPerMonth = 30;
    public const int ticksPerDay = 288;
    public const int ticksPerHour = 12;

    public int startYear;

    public PROGRESSION_SPEED currProgressionSpeed;

	public float progressionSpeed;
	public bool isPaused { get; private set; }
    public bool displayFPS = true;
    public bool showFullDebug;
    public static bool showAllTilesTooltip = false;
    
    public GameObject travelLineParentPrefab;
    public GameObject travelLinePrefab;

    [Header("Particle Effects")]
    //[SerializeField] private GameObject electricEffectPrefab;
    //[SerializeField] private GameObject explodeEffectPrefab;
    //[SerializeField] private GameObject fireEffectPrefab;
    [SerializeField] private GameObject aoeParticlesPrefab;
    [SerializeField] private GameObject aoeParticlesAutoDestroyPrefab;
    //[SerializeField] private GameObject burningEffectPrefab;
    [SerializeField] private GameObject bloodPuddleEffectPrefab;
    //[SerializeField] private GameObject freezingEffectPrefab;
    //[SerializeField] private GameObject poisonEffectPrefab;
    [SerializeField] private ParticleEffectAssetDictionary particleEffectsDictionary;

    private const float X1_SPEED = 0.75f;
    private const float X2_SPEED = 0.5f;
    private const float X4_SPEED = 0.25f;

    private float timeElapsed;
    private bool _gameHasStarted;

    public bool pauseTickEnded2;

    public string lastProgressionBeforePausing; //what was the last progression speed before the player paused the game. NOTE: This includes paused state

    #region getters/setters
    public bool gameHasStarted {
        get { return _gameHasStarted; }
    }
    #endregion

    #region Monobehaviours
    private void Awake() {
        // Debug.unityLogger.logEnabled = false;
        Instance = this;
        timeElapsed = 0f;
        _gameHasStarted = false;
        InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Default);
    }

    private void OnKeyDown(KeyCode keyCode) {
        if (keyCode == KeyCode.BackQuote) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UIManager.Instance.ToggleConsole();
#endif
        } else if (keyCode == KeyCode.Space) {
            if (!UIManager.Instance.IsConsoleShowing() && UIManager.Instance.pauseBtn.IsInteractable()) {
                if (isPaused) {
                    UIManager.Instance.Unpause();
                } else {
                    UIManager.Instance.Pause();
                }
            }
        } else if (keyCode == KeyCode.Alpha1) {
            if (!UIManager.Instance.IsConsoleShowing() && UIManager.Instance.x1Btn.IsInteractable()) {
                UIManager.Instance.SetProgressionSpeed1X();
            }
        } else if (keyCode == KeyCode.Alpha2) {
            if (!UIManager.Instance.IsConsoleShowing() && UIManager.Instance.x2Btn.IsInteractable()) {
                UIManager.Instance.SetProgressionSpeed2X();
            }
        } else if (keyCode == KeyCode.Alpha3) {
            if (!UIManager.Instance.IsConsoleShowing() && UIManager.Instance.x4Btn.IsInteractable()) {
                UIManager.Instance.SetProgressionSpeed4X();
            }
        }
    }
    
    private void Update() {
        if (_gameHasStarted && !isPaused) {
            if (Math.Abs(timeElapsed) <= 0f) {
                TickStarted();
            }
            timeElapsed += Time.deltaTime;
            if (timeElapsed >= progressionSpeed) {
                timeElapsed = 0f;
                TickEnded();
            }
        }
    }
    #endregion

    [ContextMenu("Start Progression")]
	public void StartProgression(){
        _gameHasStarted = true;
        days = 1;
        UIManager.Instance.Pause();
        lastProgressionBeforePausing = "paused";
        SchedulingManager.Instance.StartScheduleCalls ();
        Messenger.Broadcast(Signals.DAY_STARTED); //for the first day
        Messenger.Broadcast(Signals.MONTH_START); //for the first month
        Messenger.AddListener<KeyCode>(Signals.KEY_DOWN, OnKeyDown);
        //TimerHubUI.Instance.AddItem("Until Divine Intervention", 4320, null);
        Messenger.Broadcast(Signals.UPDATE_INNER_MAP_LIGHT, GetCurrentTimeInWordsOfTick());
    }
    public GameDate Today() {
        return new GameDate(month, days, year, tick);
    }
    public string TodayLogString() {
        return $"[{continuousDays.ToString()} - {ConvertTickToTime(tick)}] ";
    }
    public GameDate EndOfTheMonth() {
        return new GameDate(month, daysPerMonth, year, ticksPerDay);
    }
    public int GetNextMonth() {
        int currMonth = month;
        currMonth++;
        if(currMonth > 12) {
            currMonth = 1;
        }
        return currMonth;
    }
    public int GetTicksDifferenceOfTwoDates(GameDate fromDate, GameDate toDate) {
        int date1DaysDiff = fromDate.day;
        for (int i = 1; i < fromDate.month; i++) {
            date1DaysDiff += daysPerMonth;
        }
        int date2DaysDiff = toDate.day;
        for (int i = 1; i < toDate.month; i++) {
            date2DaysDiff += daysPerMonth;
        }
        int daysDiff = date2DaysDiff - date1DaysDiff;
        int yearDiff = toDate.year - fromDate.year;
        if(fromDate.year > toDate.year) {
            yearDiff = fromDate.year - toDate.year;
        }

        int daysDifference = (daysDiff + (yearDiff * 365)) * daysPerMonth;
        int tickDifference = fromDate.tick - toDate.tick;
        if (daysDifference > 0) {
            tickDifference = daysDifference * ticksPerDay;
            tickDifference = (tickDifference - toDate.tick) + fromDate.tick;
        }
        return tickDifference;
    }
    public GameDate FirstDayOfTheMonth() {
		return new GameDate(month, 1, year, 1);
	}
    public bool IsEndOfDay() {
        return tick == ticksPerDay;
    }
    public void SetPausedState(bool isPaused){
        if (isPaused) {
            StoreLastProgressionBeforePausing();
        }
        if (this.isPaused != isPaused) {
            this.isPaused = isPaused;
            Messenger.Broadcast(Signals.PAUSED, isPaused);
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
    public void SetProgressionSpeed(PROGRESSION_SPEED progSpeed){
        //if (!isPaused && currProgressionSpeed == progSpeed) {
        //    return; //ignore change
        //}
        currProgressionSpeed = progSpeed;
        //Debug.Log("Set progression speed to " + progSpeed.ToString());
        float speed = X1_SPEED;
        if (progSpeed == PROGRESSION_SPEED.X2) {
            speed = X2_SPEED;
        } else if(progSpeed == PROGRESSION_SPEED.X4){
            speed = X4_SPEED;
        }
		progressionSpeed = speed;
        //CombatManager.Instance.updateIntervals = this.progressionSpeed / (float) CombatManager.Instance.numOfCombatActionPerDay;
        Messenger.Broadcast(Signals.PROGRESSION_SPEED_CHANGED, progSpeed);
	}
    private void TickStarted() {
        if (tick % ticksPerHour == 0 && !IsStartOfGame()) {
            //hour reached
            Messenger.Broadcast(Signals.HOUR_STARTED);
        }
        Messenger.Broadcast(Signals.TICK_STARTED);
        Messenger.Broadcast(Signals.UPDATE_UI);
    }
    private void TickEnded(){
        Messenger.Broadcast(Signals.TICK_ENDED);

        tick += 1;
        if (tick > ticksPerDay) {
            //int difference = this.tick - ticksPerDay; //Added this for cases when the ticks to be added per tick is greater than 1, so it is possible that the excess ticks over ticksPerDay can also be greater than 1
            tick = 1;
            DayStarted(false);
        }
        Messenger.Broadcast(Signals.UPDATE_UI);
    }
    public void SetTick(int amount) {
        tick = amount;
        Messenger.Broadcast(Signals.UPDATE_UI);
    }
    private void DayStarted(bool broadcastUI = true) {
        days += 1;
        continuousDays += 1;
        Messenger.Broadcast(Signals.DAY_STARTED);
        if (days > daysPerMonth) {
            days = 1;
            month += 1;
            if (month > 12) {
                month = 1;
                year += 1;
            }
            Messenger.Broadcast(Signals.MONTH_START);
        }
        if (broadcastUI) {
            Messenger.Broadcast(Signals.UPDATE_UI);
        }
    }
    public static string ConvertTickToTime(int tick) {
        float floatConversion = tick / (float) ticksPerHour;
        int hour = (int) floatConversion;
        int minutes = Mathf.RoundToInt(((floatConversion - hour) * 12) * 5);
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
        return $"{hour}:{minutes:D2} {timeOfDay}";
    }
    public static TIME_IN_WORDS GetTimeInWordsOfTick(int tick) {
        if ((tick >= 265 && tick <= 288) || (tick >= 1 && tick <= 60)) {
            return TIME_IN_WORDS.AFTER_MIDNIGHT;
        }
        if (tick >= 61 && tick <= 132) {
            return TIME_IN_WORDS.MORNING;
        }
        if (tick >= 133 && tick <= 156) {
            return TIME_IN_WORDS.LUNCH_TIME;
        }
        if (tick >= 157 && tick <= 204) {
            return TIME_IN_WORDS.AFTERNOON;
        }
        if (tick >= 205 && tick <= 240) {
            return TIME_IN_WORDS.EARLY_NIGHT;
        }
        if (tick >= 241 && tick <= 264) {
            return TIME_IN_WORDS.LATE_NIGHT;
        }
        return TIME_IN_WORDS.NONE;
    }

    //Note: If there is a character parameter, it means that the current time in words might not be the actual one because we will get the time in words relative to the character
    //Example: If the character is Nocturnal, MORNING will become LATE_NIGHT
    public static TIME_IN_WORDS GetCurrentTimeInWordsOfTick(Character relativeTo = null) {
        TIME_IN_WORDS time = TIME_IN_WORDS.NONE;
        if ((Instance.tick >= 265 && Instance.tick <= 288) || (Instance.tick >= 1 && Instance.tick <= 60)) {
            time = TIME_IN_WORDS.AFTER_MIDNIGHT;
        } else if (Instance.tick >= 61 && Instance.tick <= 132) {
            time = TIME_IN_WORDS.MORNING;
        } else if (Instance.tick >= 133 && Instance.tick <= 156) {
            time = TIME_IN_WORDS.LUNCH_TIME;
        } else if (Instance.tick >= 157 && Instance.tick <= 204) {
            time = TIME_IN_WORDS.AFTERNOON;
        } else if (Instance.tick >= 205 && Instance.tick <= 240) {
            time = TIME_IN_WORDS.EARLY_NIGHT;
        } else if (Instance.tick >= 241 && Instance.tick <= 264) {
            time = TIME_IN_WORDS.LATE_NIGHT;
        }
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
    public static int GetRandomTickFromTimeInWords(TIME_IN_WORDS timeInWords) {
        if (timeInWords == TIME_IN_WORDS.AFTER_MIDNIGHT) {
            //After Midnight has special processing because it goes beyond the max tick, its 10:00PM to 5:00AM 
            int maxRange = ticksPerDay + 60;
            int chosenTick = Random.Range(265, maxRange + 1);
            if(chosenTick > ticksPerDay) {
                chosenTick -= ticksPerDay;
            }
            return chosenTick;
        }
        if (timeInWords == TIME_IN_WORDS.MORNING) {
            return Random.Range(61, 133);
        }
        if (timeInWords == TIME_IN_WORDS.LUNCH_TIME) {
            return Random.Range(133, 157);
        }
        if (timeInWords == TIME_IN_WORDS.AFTERNOON) {
            return Random.Range(157, 205);
        }
        if (timeInWords == TIME_IN_WORDS.EARLY_NIGHT) {
            return Random.Range(205, 241);
        }
        if (timeInWords == TIME_IN_WORDS.LATE_NIGHT) {
            return Random.Range(241, 265);
        }
        throw new Exception($"{timeInWords} time in words has no tick!");
    }
    public static int GetRandomTickFromTimeInWords(TIME_IN_WORDS timeInWords, int minimumThreshold) {
        if (timeInWords == TIME_IN_WORDS.AFTER_MIDNIGHT) {
            int maxRange = ticksPerDay + 60;
            int chosenTick = Random.Range(minimumThreshold, maxRange + 1);
            if (chosenTick > ticksPerDay) {
                chosenTick -= ticksPerDay;
            }
            return chosenTick;
        }
        if (timeInWords == TIME_IN_WORDS.MORNING) {
            return Random.Range(minimumThreshold, 133);
        }
        if (timeInWords == TIME_IN_WORDS.LUNCH_TIME) {
            return Random.Range(minimumThreshold, 157);
        }
        if (timeInWords == TIME_IN_WORDS.AFTERNOON) {
            return Random.Range(minimumThreshold, 205);
        }
        if (timeInWords == TIME_IN_WORDS.EARLY_NIGHT) {
            return Random.Range(minimumThreshold, 241);
        }
        if (timeInWords == TIME_IN_WORDS.LATE_NIGHT) {
            return Random.Range(minimumThreshold, 265);
        }
        throw new Exception($"{timeInWords} time in words has no tick!");
    }
    public static TIME_IN_WORDS[] ConvertTimeInWordsWhenNocturnal(TIME_IN_WORDS[] currentTimeInWords) {
        TIME_IN_WORDS[] convertedTimeInWords = new TIME_IN_WORDS[currentTimeInWords.Length];
        for (int i = 0; i < currentTimeInWords.Length; i++) {
            convertedTimeInWords[i] = ConvertTimeInWordsWhenNocturnal(currentTimeInWords[i]);
        }
        return convertedTimeInWords;
    }
    public static TIME_IN_WORDS ConvertTimeInWordsWhenNocturnal(TIME_IN_WORDS currentTimeInWords) {
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
    public int GetCeilingHoursBasedOnTicks(int ticks) {
        return Mathf.CeilToInt(ticks / (float) ticksPerHour);
    }
    public int GetCeilingDaysBasedOnTicks(int ticks) {
        return Mathf.CeilToInt(ticks / (float) ticksPerDay);
    }
    //public void SetTicksToAddPerTick(int amount) {
    //    ticksToAddPerTick = amount;
    //}

    #region Particle Effects
    public GameObject CreateParticleEffectAt(LocationGridTile tile, PARTICLE_EFFECT particle) {
        GameObject prefab = null;
        GameObject go = null;
        if (particleEffectsDictionary.ContainsKey(particle)) {
            prefab = particleEffectsDictionary[particle];
        } else {
            Debug.LogError("No prefab for particle effect: " + particle.ToString());
            return prefab;
        }
        go = ObjectPoolManager.Instance.InstantiateObjectFromPool(prefab.name, Vector3.zero, Quaternion.identity, tile.parentMap.objectsParent);
        go.transform.localPosition = tile.centeredLocalLocation;
        go.SetActive(true);
        BaseParticleEffect particleEffectScript = go.GetComponent<BaseParticleEffect>();
        if (particleEffectScript) {
            particleEffectScript.SetTargetTile(tile);
            particleEffectScript.PlayParticleEffect();
        }
        return go;
    }
    public GameObject CreateParticleEffectAt(StructureWallObject wallObject, PARTICLE_EFFECT particle) {
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
            Transform parent = character.marker.visualsParent.transform;
            if (!allowRotation) {
                parent = character.marker.transform;
            }
            go = ObjectPoolManager.Instance.InstantiateObjectFromPool(prefab.name, Vector3.zero, Quaternion.identity, parent);
            go.transform.localPosition = Vector3.zero;
            go.SetActive(true);
        } else {
            if (poi.mapObjectVisual) {
                go = ObjectPoolManager.Instance.InstantiateObjectFromPool(prefab.name, Vector3.zero, Quaternion.identity, poi.mapObjectVisual.transform);
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

    //public GameObject CreateElectricEffectAt(IPointOfInterest poi) {
    //    GameObject go = null;
    //    if (poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
    //        go = CreateElectricEffectAt(poi as Character);
    //    } else {
    //        if (poi.gridTileLocation == null) {
    //            return go;
    //        }
    //        go = ObjectPoolManager.Instance.InstantiateObjectFromPool(electricEffectPrefab.name, Vector3.zero, Quaternion.identity, poi.gridTileLocation.parentMap.objectsParent);
    //        go.transform.localPosition = poi.gridTileLocation.centeredLocalLocation;
    //        go.SetActive(true);
    //    }
    //    return go;
    //}
    //private GameObject CreateElectricEffectAt(Character character) {
    //    //StartCoroutine(ElectricEffect(character));
    //    GameObject go = null;
    //    if (!character.marker) {
    //        return go;
    //    }
    //    go = ObjectPoolManager.Instance.InstantiateObjectFromPool(electricEffectPrefab.name, Vector3.zero, Quaternion.identity, character.marker.transform);
    //    go.transform.localPosition = Vector3.zero;
    //    go.SetActive(true);
    //    return go;
    //}
    //public void CreateFireEffectAt(LocationGridTile tile) {
    //    GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(fireEffectPrefab.name, Vector3.zero, Quaternion.identity, tile.parentMap.objectsParent);
    //    go.transform.localPosition = tile.centeredLocalLocation;
    //    go.SetActive(true);
    //}
    //public void CreateFireEffectAt(IPointOfInterest poi) {
    //    if (poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
    //        CreateFireEffectAt(poi as Character);
    //    } else {
    //        if (poi.gridTileLocation == null) {
    //            return;
    //        }
    //        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(fireEffectPrefab.name, Vector3.zero, Quaternion.identity, poi.gridTileLocation.parentMap.objectsParent);
    //        go.transform.localPosition = poi.gridTileLocation.centeredLocalLocation;
    //        go.SetActive(true);
    //    }
    //}
    //private void CreateFireEffectAt(Character character) {
    //    if (!character.marker) {
    //        return;
    //    }
    //    GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(fireEffectPrefab.name, Vector3.zero, Quaternion.identity, character.marker.transform);
    //    go.transform.localPosition = Vector3.zero;
    //    go.SetActive(true);
    //    //StartCoroutine(FireEffect(character));
    //}
    //public GameObject CreateFreezingEffectAt(LocationGridTile tile) {
    //    GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(freezingEffectPrefab.name, Vector3.zero, Quaternion.identity, tile.parentMap.objectsParent);
    //    go.transform.localPosition = tile.centeredLocalLocation;
    //    go.SetActive(true);
    //    return go;
    //}
    //public GameObject CreateFreezingEffectAt(IPointOfInterest poi) {
    //    GameObject go = null;
    //    if (poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
    //        go = CreateFreezingEffectAt(poi as Character);
    //    } else {
    //        if (poi.gridTileLocation == null) {
    //            return go;
    //        }
    //        go = ObjectPoolManager.Instance.InstantiateObjectFromPool(freezingEffectPrefab.name, Vector3.zero, Quaternion.identity, poi.gridTileLocation.parentMap.objectsParent);
    //        go.transform.localPosition = poi.gridTileLocation.centeredLocalLocation;
    //        go.SetActive(true);
    //    }
    //    return go;
    //}
    //private GameObject CreateFreezingEffectAt(Character character) {
    //    GameObject go = null;
    //    if (!character.marker) {
    //        return go;
    //    }
    //    go = ObjectPoolManager.Instance.InstantiateObjectFromPool(freezingEffectPrefab.name, Vector3.zero, Quaternion.identity, character.marker.visualsParent);
    //    go.transform.localPosition = Vector3.zero;
    //    go.SetActive(true);
    //    return go;
    //    //StartCoroutine(FireEffect(character));
    //}
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
    //public GameObject CreateBurningEffectAt(LocationGridTile tile) {
    //    GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(burningEffectPrefab.name, Vector3.zero, Quaternion.identity, tile.parentMap.objectsParent);
    //    go.transform.localPosition = tile.centeredLocalLocation;
    //    go.SetActive(true);
    //    return go;
    //}
    //public GameObject CreateBurningEffectAt(ITraitable obj) {
    //    GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(burningEffectPrefab.name, Vector3.zero, Quaternion.identity, obj.worldObject);
    //    go.transform.position = obj.worldObject.position;
    //    go.SetActive(true);
    //    return go;
    //}
    //public void CreateExplodeEffectAt(Character character) {
    //    if (!character.marker) {
    //        return;
    //    }
    //    GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(hitEffectPrefab.name, Vector3.zero, Quaternion.identity, character.marker.transform);
    //    go.transform.localPosition = Vector3.zero;
    //    go.SetActive(true);
    //}
    //public void CreateExplodeEffectAt(LocationGridTile tile) {
    //    GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(hitEffectPrefab.name, Vector3.zero, Quaternion.identity, tile.parentMap.objectsParent);
    //    go.transform.localPosition = tile.centeredLocalLocation;
    //    go.SetActive(true);
    //}
    public void CreateBloodEffectAt(Character character) {
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(bloodPuddleEffectPrefab.name, Vector3.zero, Quaternion.identity, InnerMapManager.Instance.transform);
        go.transform.position = character.marker.transform.position;
        go.SetActive(true);
    }
    #endregion

    //#region Cursor
    //public void SetCursorToDefault() {
    //    isDraggingItem = false;
    //    Cursor.SetCursor(defaultCursorTexture, hotSpot, cursorMode);
    //}
    //public void SetCursorToTarget() {
    //    Cursor.SetCursor(targetCursorTexture, hotSpot, cursorMode);
    //}
    //public void SetCursorToDrag() {
    //    Cursor.SetCursor(dragWorldCursorTexture, new Vector2(16f, 16f), cursorMode);
    //}
    //public void SetCursorToItemDragHover() {
    //    isDraggingItem = false;
    //    Cursor.SetCursor(dragItemHoverCursorTexture, hotSpot, cursorMode);
    //}
    //public void SetCursorToItemDragClicked() {
    //    isDraggingItem = true;
    //    Cursor.SetCursor(dragItemClickedCursorTexture, hotSpot, cursorMode);
    //}
    //#endregion

    #region For Testing
    [ContextMenu("Print Event Table")]
    public void PrintEventTable() {
        Messenger.PrintEventTable();
    }
    #endregion

    #region Utilities
    private bool IsStartOfGame() {
        if (year == startYear && month == 1 && days == 1 && tick == 24) {
            return true;
        }
        return false;
    }
    public void SetStartDate(GameDate date) {
        month = date.month;
        days = date.day;
        tick = date.tick;
        year = date.year;
        continuousDays = date.ConvertToContinuousDays();
    }
    #endregion
}
