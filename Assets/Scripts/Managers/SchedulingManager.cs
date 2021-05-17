using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Assertions;
using UnityEngine.Profiling;
using UtilityScripts;
public class SchedulingManager : BaseMonoBehaviour {
	public static SchedulingManager Instance;

	private Dictionary<GameDate, List<ScheduledAction>> schedules = new Dictionary<GameDate, List<ScheduledAction>> (new GameDateComparer());
	private GameDate checkGameDate;
    private List<ScheduledAction> _actionsToDo;
    
    
	void Awake(){
		Instance = this;
        _actionsToDo = new List<ScheduledAction>();
	}
    protected override void OnDestroy() {
        //Do not return to object pool scheduled action here since this is only done when game has ended or has changed scene, we can do garbage collection here
        schedules.Clear();
        schedules = null;
        _actionsToDo.Clear();
        _actionsToDo = null;
        base.OnDestroy();
        Instance = null;
    }
    public void StartScheduleCalls() {
        checkGameDate = GameManager.Instance.Today();
		Messenger.AddListener(Signals.CHECK_SCHEDULES, CheckSchedule);
    }
	private void CheckSchedule(){
        checkGameDate = GameManager.Instance.Today();
        if (schedules.ContainsKey(checkGameDate)) {
            _actionsToDo.Clear();
            _actionsToDo.AddRange(schedules[checkGameDate]);
            DoAsScheduled(_actionsToDo);
			RemoveEntry(checkGameDate);
		}
	}
	internal string AddEntry(GameDate gameDate, Action act, object adder){
        if (!schedules.ContainsKey(gameDate)) {
            schedules.Add(gameDate, RuinarchListPool<ScheduledAction>.Claim());
        }
        string newID = GenerateScheduleID();
        ScheduledAction sa = ObjectPoolManager.Instance.CreateNewScheduledAction();
        sa.SetData(newID, act, adder);
        schedules[gameDate].Add(sa);
        // Debug.Log($"{GameManager.Instance.TodayLogString()}Created new schedule on {gameDate.ConvertToContinuousDaysWithTime()}. Action is {act.Method.Name}, by {adder}");
        return newID;
	}
	internal void RemoveEntry(GameDate gameDate){
        if (schedules.ContainsKey(gameDate)) {
            List<ScheduledAction> saList = schedules[gameDate];
            if (saList != null) {
                for (int i = 0; i < saList.Count; i++) {
                    OnRemoveScheduledAction(saList[i]);
                }
                RuinarchListPool<ScheduledAction>.Release(saList);
            }
        }
		schedules.Remove(gameDate);
	}
    internal void RemoveSpecificEntry(int month, int day, int year, int hour, int continuousDays, Action act) {
        GameDate gameDate = new GameDate(month, day, year, hour);
        if (schedules.ContainsKey(gameDate)) {
            List<ScheduledAction> acts = schedules[gameDate];
            for (int i = 0; i < acts.Count; i++) {
                ScheduledAction sa = acts[i];
                if (sa.action.Target == act.Target) {
                    OnRemoveScheduledAction(sa);
                    schedules[gameDate].RemoveAt(i);
                    break;
                }
            }
        }
    }
    internal void RemoveSpecificEntry(GameDate date, Action act) {
        if (schedules.ContainsKey(date)) {
            List<ScheduledAction> acts = schedules[date];
            for (int i = 0; i < acts.Count; i++) {
                ScheduledAction sa = acts[i];
                if (sa.action.Target == act.Target) {
                    OnRemoveScheduledAction(sa);
                    schedules[date].RemoveAt(i);
                    break;
                }
            }
        }
    }
    private bool RemoveSpecificEntry(GameDate date, string id) {
        if (schedules.ContainsKey(date)) {
            List<ScheduledAction> acts = schedules[date];
            for (int i = 0; i < acts.Count; i++) {
                ScheduledAction action = acts[i];
                if (action.scheduleID == id) {
#if DEBUG_LOG
                    Debug.Log($"Removed scheduled item {action.ToString()} for {action.scheduler?.ToString()}. ID is {id}");
#endif
                    OnRemoveScheduledAction(action);
                    schedules[date].RemoveAt(i);
                    return true;
                }
            }
        }
        return false;
    }
    public bool RemoveSpecificEntry(string id) {
        foreach (KeyValuePair<GameDate, List<ScheduledAction>> keyValuePair in schedules) {
            if (RemoveSpecificEntry(keyValuePair.Key, id)) {
                return true;
            }
        }
        return false;
    }
    private void OnRemoveScheduledAction(ScheduledAction p_sa) {
        ObjectPoolManager.Instance.ReturnScheduledActionToPool(p_sa);
    }
    private void EvaluateRemovingOfSchedules() {

    }
    private void DoAsScheduled(List<ScheduledAction> acts) {
        int expectedIterations = acts.Count;
        int actualIterations = 0;
		for (int i = 0; i < acts.Count; i++) {
            ScheduledAction action = acts[i];
            if (schedules[checkGameDate].Contains(action)) {
                //only perform scheduled action, if it still present in the original actions list.
#if DEBUG_PROFILER
                Profiler.BeginSample($"Is Action Still Valid");
#endif
                bool isScheduleStillValid = action.IsScheduleStillValid();
#if DEBUG_PROFILER
                Profiler.EndSample();
#endif
                if(isScheduleStillValid && action.action.Target != null) {
#if DEBUG_PROFILER
                    Profiler.BeginSample($"{action.ToString()} Invoke");
#endif
                    action.action();
#if DEBUG_PROFILER
                    Profiler.EndSample();
#endif
                }
            }
			
            actualIterations++;
        }
        Assert.IsTrue(expectedIterations == actualIterations, $"Scheduling Manager inconsistency with performing scheduled actions! Performed actions were {actualIterations} but expected actions were {expectedIterations.ToString()}");
	}
    public void ClearAllSchedulesBy(Character character) {
#if DEBUG_LOG
        Debug.Log($"Clearing all schedules by {character.name}");
#endif
        Dictionary<GameDate, List<ScheduledAction>> temp = new Dictionary<GameDate, List<ScheduledAction>>(schedules);
        foreach (KeyValuePair<GameDate, List<ScheduledAction>> kvp in temp) {
            //List<ScheduledAction> newList = RuinarchListPool<ScheduledAction>.Claim();
            //newList.AddRange(kvp.Value);
            List<ScheduledAction> saList = kvp.Value;
            for (int i = 0; i < saList.Count; i++) {
                ScheduledAction sa = saList[i];
                if (sa.scheduler == character) {
                    OnRemoveScheduledAction(sa);
                    saList.RemoveAt(i);
                    i--;
                }
            }
            //schedules[kvp.Key] = newList;
        }
    }
    public void ClearAllSchedulesBy(object obj) {
#if DEBUG_LOG
        Debug.Log($"Clearing all schedules by {obj.ToString()}");
#endif
        Dictionary<GameDate, List<ScheduledAction>> temp = new Dictionary<GameDate, List<ScheduledAction>>(schedules);
        foreach (KeyValuePair<GameDate, List<ScheduledAction>> kvp in temp) {
            List<ScheduledAction> saList = kvp.Value;
            for (int i = 0; i < saList.Count; i++) {
                ScheduledAction sa = saList[i];
                if (sa.scheduler == obj) {
                    OnRemoveScheduledAction(sa);
                    saList.RemoveAt(i);
                    i--;
                }
            }
            //List<ScheduledAction> newList = new List<ScheduledAction>(kvp.Value);
            //for (int i = 0; i < kvp.Value.Count; i++) {
            //    if (kvp.Value[i].scheduler == obj) {
            //        newList.Remove(kvp.Value[i]);
            //    }
            //}
            //schedules[kvp.Key] = newList;
        }
    }
    public string GenerateScheduleID() {
        //Reference: https://stackoverflow.com/questions/11313205/generate-a-unique-id
        return Guid.NewGuid().ToString("N");
    }
}

public class GameDateComparer : IEqualityComparer<GameDate> {
    public bool Equals(GameDate x, GameDate y) {
        if (x.year == y.year && x.month == y.month && x.day == y.day && x.tick == y.tick) {
            return true;
        }
        return false;
    }

    public int GetHashCode(GameDate obj) {
        return obj.GetHashCode();
    }
}

public class ScheduledAction {
    public string scheduleID;
    public Action action;
    public object scheduler; //the object that scheduled this action

    public void SetData(string p_scheduleID, Action p_action, object p_scheduler) {
        scheduleID = p_scheduleID;
        action = p_action;
        scheduler = p_scheduler;
    }
    public bool IsScheduleStillValid() {
        if(scheduler != null) {
            if (scheduler is Character character) {
                return character.gridTileLocation != null;
            } else if (scheduler is TileObject tileObject) {
                return tileObject.gridTileLocation != null;
            }
        }
        return true;
    }

    public override string ToString() {
        return $"{scheduleID} - {action.Method.Name} by {scheduler}";
    }

    public void Reset() {
        scheduleID = null;
        action = null;
        scheduler = null;
    }
}
