using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Assertions;

public class SchedulingManager : MonoBehaviour {
	public static SchedulingManager Instance;

	private Dictionary<GameDate, List<ScheduledAction>> schedules = new Dictionary<GameDate, List<ScheduledAction>> (new GameDateComparer());
	private GameDate checkGameDate;
    private List<ScheduledAction> _actionsToDo;
    
    
	void Awake(){
		Instance = this;
        _actionsToDo = new List<ScheduledAction>();
	}
	public void StartScheduleCalls() {
        checkGameDate = GameManager.Instance.Today();
		Messenger.AddListener(Signals.TICK_ENDED, CheckSchedule);
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
            schedules.Add(gameDate, new List<ScheduledAction>());
        }
        string newID = GenerateScheduleID();
        schedules[gameDate].Add(new ScheduledAction() { scheduleID = newID, action = act, scheduler = adder });
        Debug.Log(
            $"{GameManager.Instance.TodayLogString()}Created new schedule on {gameDate.ConvertToContinuousDaysWithTime()}. Action is {act.Method.Name}, by {adder}");
        return newID;
	}
	internal void RemoveEntry(GameDate gameDate){
		schedules.Remove (gameDate);
	}
    internal void RemoveSpecificEntry(int month, int day, int year, int hour, int continuousDays, Action act) {
        GameDate gameDate = new GameDate(month, day, year, hour);
        if (schedules.ContainsKey(gameDate)) {
            List<ScheduledAction> acts = schedules[gameDate];
            for (int i = 0; i < acts.Count; i++) {
                if (acts[i].action.Target == act.Target) {
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
                if (acts[i].action.Target == act.Target) {
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
                    schedules[date].RemoveAt(i);
                    Debug.Log($"Removed scheduled item {action.ToString()} for {action.scheduler.ToString()}. ID is {id}");
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
    private void DoAsScheduled(List<ScheduledAction> acts) {
        int expectedIterations = acts.Count;
        int actualIterations = 0;
		for (int i = 0; i < acts.Count; i++) {
            ScheduledAction action = acts[i];
            if (schedules[checkGameDate].Contains(action)) {
                //only perform scheduled action, if it still present in the original actions list.
                if(action.IsScheduleStillValid() && action.action.Target != null){
                    action.action ();
                }    
            }
			
            actualIterations++;
        }
        Assert.IsTrue(expectedIterations == actualIterations, $"Scheduling Manager inconsistency with performing scheduled actions! Performed actions were {actualIterations} but expected actions were {expectedIterations.ToString()}");
	}
    public void ClearAllSchedulesBy(Character character) {
        Debug.Log($"Clearing all schedules by {character.name}");
        Dictionary<GameDate, List<ScheduledAction>> temp = new Dictionary<GameDate, List<ScheduledAction>>(schedules);
        foreach (KeyValuePair<GameDate, List<ScheduledAction>> kvp in temp) {
            List<ScheduledAction> newList = new List<ScheduledAction>(kvp.Value);
            for (int i = 0; i < kvp.Value.Count; i++) {
                if (kvp.Value[i].scheduler == character) {
                    newList.Remove(kvp.Value[i]);
                }
            }
            schedules[kvp.Key] = newList;
        }
    }
    public void ClearAllSchedulesBy(object obj) {
        Debug.Log($"Clearing all schedules by {obj.ToString()}");
        Dictionary<GameDate, List<ScheduledAction>> temp = new Dictionary<GameDate, List<ScheduledAction>>(schedules);
        foreach (KeyValuePair<GameDate, List<ScheduledAction>> kvp in temp) {
            List<ScheduledAction> newList = new List<ScheduledAction>(kvp.Value);
            for (int i = 0; i < kvp.Value.Count; i++) {
                if (kvp.Value[i].scheduler == obj) {
                    newList.Remove(kvp.Value[i]);
                }
            }
            schedules[kvp.Key] = newList;
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

public struct ScheduledAction {
    public string scheduleID;
    public Action action;
    public object scheduler; //the object that scheduled this action
    
    public bool IsScheduleStillValid() {
        if (scheduler is Character character) {
            return character.gridTileLocation != null;
        } else if (scheduler is TileObject tileObject) {
            return tileObject.gridTileLocation != null;
        }
        return true;
    }

    public override string ToString() {
        return $"{scheduleID} - {action.Method.Name} by {scheduler}";
    }
}
