public class SleepBehaviour : CharacterBehaviourComponent {
    
    public SleepBehaviour() {
        priority = 9;
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        if (character.dailyScheduleComponent.schedule.GetScheduleType(GameManager.Instance.currentTick) == DAILY_SCHEDULE.Sleep) {
            return character.needsComponent.PlanTirednessRecoveryActionsForSleepBehaviour(out producedJob);
        }
        producedJob = null;
        return false;
    }
}
