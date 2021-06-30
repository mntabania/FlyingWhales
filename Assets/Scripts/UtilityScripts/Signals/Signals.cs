using System.Collections.Generic;
using System;
using Traits;

public static class Signals {

    public static string TICK_STARTED = "OnTickStart";
    public static string TICK_ENDED = "OnTickEnd";
    public static string HOUR_STARTED = "OnHourStart";
    public static string DAY_STARTED = "OnDayStart";
    public static string MONTH_START = "OnMonthStart";
    public static string GAME_LOADED = "OnGameLoaded";
    public static string CLEAN_UP_MEMORY = "OnCleanUpMemory";
    public static string CHECK_SCHEDULES = "CheckSchedules";
    public static string GAME_STARTED = "GameStarted";
    public static string TILE_OBJECT_GENERATION_FINISHED = "OnTileObjectGenerationFinished";
    public static string PROGRESSION_LOADED = "OnProgressionLoaded";

    // public static readonly Dictionary<string, SignalMethod[]> orderedSignalExecution = new Dictionary<string, SignalMethod[]>() {
    //     // { TICK_STARTED, new[] {
    //     //     // new SignalMethod() { methodName = string.Empty, objectType = typeof(Trait) },
    //     //     new SignalMethod() { methodName = "PerTickCooldown", objectType = typeof(SkillData) },
    //     //     new SignalMethod() { methodName = "UnsummonedHPRecovery", objectType = typeof(Minion) },
    //     // }},
    //     // { TICK_ENDED, new[] {
    //     //     // new SignalMethod() { methodName = "PerTickMovement", objectType = typeof(CharacterMarker) },
    //     //     // new SignalMethod() { methodName = "ProcessAllUnprocessedVisionPOIs", objectType = typeof(CharacterMarker) },
    //     //     // new SignalMethod() { methodName = "OnTickEnded", objectType = typeof(Character) },
    //     // }},
    //     // {
    //     //     UISignals.START_GAME_AFTER_LOADOUT_SELECT, new[] {
    //     //     // new SignalMethod() { methodName = "OnStartGameAfterLoadoutSelect", objectType = typeof(PlayerSkillLoadoutUI) },
    //     //     new SignalMethod() { methodName = "OnLoadoutSelected", objectType = typeof(StartupManager) },
    //     // }},
    // };
    // public static bool TryGetMatchingSignalMethod(string eventType, Callback method, out SignalMethod matching) {
    //     for (int i = 0; i < orderedSignalExecution[eventType].Length; i++) {
    //         SignalMethod sm = orderedSignalExecution[eventType][i];
    //         if (sm.Equals(method)) {
    //             matching = sm;
    //             return true;
    //         }
    //     }
    //     matching = default(SignalMethod);
    //     return false;
    // }
}

public struct SignalMethod {
    public string methodName;
    public System.Type objectType;

    public bool Equals(Delegate d) {
        if (d.Method.Name.Contains(methodName) && (d.Target.GetType() == objectType || d.Target.GetType().BaseType == objectType)) {
            return true;
        }
        if (string.IsNullOrEmpty(methodName) && (d.Target.GetType() == objectType || d.Target.GetType().BaseType == objectType)) {
            //if the required method name is null, and the provided object is of the same type, consider it a match
            return true;
        }

        return false;
    }
}