public class DailyScheduleSection {
    public TickRange time;
    public DAILY_SCHEDULE scheduleType;

    public DailyScheduleSection(TickRange p_range, DAILY_SCHEDULE p_schedule) {
        time = p_range;
        scheduleType = p_schedule;
    }
    public override string ToString() {
        return $"{time.ToString()}: {scheduleType.ToString()}";
    }
}