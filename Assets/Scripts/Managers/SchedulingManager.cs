using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class SchedulingManager : MonoBehaviour {
	public static SchedulingManager Instance;

	private Dictionary<GameDate, List<ScheduledAction>> schedules = new Dictionary<GameDate, List<ScheduledAction>> (new GameDateComparer());
	private GameDate checkGameDate;

	void Awake(){
		Instance = this;
	}
	public void StartScheduleCalls() {
        this.checkGameDate = GameManager.Instance.Today();
		Messenger.AddListener(Signals.TICK_ENDED, CheckSchedule);
        //Messenger.AddListener(Signals.DAY_STARTED, CheckSchedule);
    }
	private void CheckSchedule(){
        this.checkGameDate = GameManager.Instance.Today();
        if (this.schedules.ContainsKey(this.checkGameDate)){
			DoAsScheduled (this.schedules [this.checkGameDate]);
			RemoveEntry (this.checkGameDate);
		}
	}
	internal string AddEntry(GameDate gameDate, Action act, object adder){
        if (!this.schedules.ContainsKey(gameDate)) {
            this.schedules.Add(gameDate, new List<ScheduledAction>());
        }
        string newID = GenerateScheduleID();
        this.schedules[gameDate].Add(new ScheduledAction() { scheduleID = newID, action = act, scheduler = adder });
        Debug.Log(
            $"{GameManager.Instance.TodayLogString()}Created new schedule on {gameDate.ConvertToContinuousDaysWithTime()}. Action is {act.Method.Name}, by {adder}");
        return newID;
	}
	internal void RemoveEntry(GameDate gameDate){
		this.schedules.Remove (gameDate);
	}
    internal void RemoveSpecificEntry(int month, int day, int year, int hour, int continuousDays, Action act) {
        GameDate gameDate = new GameDate(month, day, year, hour);
        if (this.schedules.ContainsKey(gameDate)) {
            List<ScheduledAction> acts = this.schedules[gameDate];
            for (int i = 0; i < acts.Count; i++) {
                if (acts[i].action.Target == act.Target) {
                    this.schedules[gameDate].RemoveAt(i);
                    break;
                }
            }
        }
    }
    internal void RemoveSpecificEntry(GameDate date, Action act) {
        if (this.schedules.ContainsKey(date)) {
            List<ScheduledAction> acts = this.schedules[date];
            for (int i = 0; i < acts.Count; i++) {
                if (acts[i].action.Target == act.Target) {
                    this.schedules[date].RemoveAt(i);
                    break;
                }
            }
        }
    }
    private bool RemoveSpecificEntry(GameDate date, string id) {
        if (this.schedules.ContainsKey(date)) {
            List<ScheduledAction> acts = this.schedules[date];
            for (int i = 0; i < acts.Count; i++) {
                ScheduledAction action = acts[i];
                if (action.scheduleID == id) {
                    this.schedules[date].RemoveAt(i);
                    Debug.Log($"Removed scheduled item {action}");
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
    private void DoAsScheduled(List<ScheduledAction> acts){
		for (int i = 0; i < acts.Count; i++) {
			if(acts[i].IsScheduleStillValid() && acts[i].action.Target != null){
				acts [i].action ();
			}
		}
	}
    public void ClearAllSchedulesBy(Character character) {
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
        if (scheduler is Character) {
            Character character = scheduler as Character;
            return !character.isDead;
        } else if (scheduler is TileObject) {
            TileObject tileObject = scheduler as TileObject;
            return tileObject.gridTileLocation != null;
        } 
        // else if (scheduler is SpecialToken) {
        //     SpecialToken token = scheduler as SpecialToken;
        //     return token.gridTileLocation != null;
        // }
        return true;
    }

    public override string ToString() {
        return $"{scheduleID} - {action.Method.Name} by {scheduler}";
    }
}
