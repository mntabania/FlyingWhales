public class PartyMemberSchedule : DailySchedule {
    private DailyScheduleSection[] m_schedule;
    
    public override DailyScheduleSection[] schedule => m_schedule;

    public PartyMemberSchedule() {
        m_schedule = new[] {
            new DailyScheduleSection(new TickRange(
                    GameManager.Instance.GetTicksBasedOnHour(5), 
                    GameManager.Instance.GetTicksBasedOnHour(8)), 
                DAILY_SCHEDULE.Free_Time
            ),
            new DailyScheduleSection(new TickRange(
                    GameManager.Instance.GetTicksBasedOnHour(8), 
                    GameManager.Instance.GetTicksBasedOnHour(19)), 
                DAILY_SCHEDULE.Work
            ),
            new DailyScheduleSection(new TickRange(
                    GameManager.Instance.GetTicksBasedOnHour(19), 
                    GameManager.Instance.GetTicksBasedOnHour(22)), 
                DAILY_SCHEDULE.Free_Time
            ),
            new DailyScheduleSection(new TickRange(
                    GameManager.Instance.GetTicksBasedOnHour(22), 
                    GameManager.Instance.GetTicksBasedOnHour(5)), 
                DAILY_SCHEDULE.Sleep
            ),
        };
    }
}