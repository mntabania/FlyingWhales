﻿using System;

/// <summary>
/// Base class for all daily schedules.
/// Daily schedules are composed of sections called DailyScheduleSection.
/// To avoid complications the schedule array should be in chronological order. 
/// </summary>
public abstract class DailySchedule {
    public abstract DailyScheduleSection[] schedule { get; }

    public DAILY_SCHEDULE GetScheduleType(int p_tick) {
        for (int i = 0; i < schedule.Length; i++) {
            DailyScheduleSection section = schedule[i];
            if (section.time.IsInRange(p_tick)) {
                return section.scheduleType;
            }
        }
        throw new Exception($"Could not find schedule type for tick {p_tick.ToString()} on schedule {this.GetType()}");
    }
    public int GetStartingTickOfScheduleType(DAILY_SCHEDULE p_scheduleType) {
        for (int i = 0; i < schedule.Length; i++) {
            DailyScheduleSection section = schedule[i];
            if (section.scheduleType == p_scheduleType) {
                return section.time.GetStartTick();
            }
        }
        return -1;
    }
    
    public string GetScheduleSummary() {
        string summary = GetType().ToString();
        for (int i = 0; i < schedule.Length; i++) {
            DailyScheduleSection section = schedule[i];
            summary = $"{summary}\n\t{section}";
        }
        return summary;
    }
}