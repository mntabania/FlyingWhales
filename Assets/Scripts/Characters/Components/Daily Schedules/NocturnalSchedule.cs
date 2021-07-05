public class NocturnalSchedule : DailySchedule {
    private DailyScheduleSection[] m_schedule;
    
    public override DailyScheduleSection[] schedule => m_schedule;

    public NocturnalSchedule() {
        m_schedule = new[] {
            new DailyScheduleSection(new TickRange(
                    GameManager.Instance.GetTicksBasedOnHour(9), 
                    GameManager.Instance.GetTicksBasedOnHour(12)), 
                DAILY_SCHEDULE.Free_Time
            ),
            new DailyScheduleSection(new TickRange(
                    GameManager.Instance.GetTicksBasedOnHour(12), 
                    GameManager.Instance.GetTicksBasedOnHour(19)), 
                DAILY_SCHEDULE.Sleep
            ),
            new DailyScheduleSection(new TickRange(
                    GameManager.Instance.GetTicksBasedOnHour(19), 
                    GameManager.Instance.GetTicksBasedOnHour(22)), 
                DAILY_SCHEDULE.Free_Time
            ),
            new DailyScheduleSection(new TickRange(
                    GameManager.Instance.GetTicksBasedOnHour(22), 
                    GameManager.Instance.GetTicksBasedOnHour(9)), 
                DAILY_SCHEDULE.Work
            ),
        };
    }
}