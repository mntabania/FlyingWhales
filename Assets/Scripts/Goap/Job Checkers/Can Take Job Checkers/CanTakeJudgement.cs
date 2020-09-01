namespace Goap.Job_Checkers {
    public class CanTakeJudgement : CanTakeJobChecker {
        public override string key => JobManager.Can_Take_Judgement;
        public override bool CanTakeJob(Character character, JobQueueItem jobQueueItem) {
            return character.isSettlementRuler || character.isFactionLeader;
        }
    }
}