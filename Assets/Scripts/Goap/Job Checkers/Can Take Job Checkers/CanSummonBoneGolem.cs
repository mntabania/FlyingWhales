namespace Goap.Job_Checkers {
    public class CanSummonBoneGolem : CanTakeJobChecker {
        public override string key => JobManager.Can_Summon_Bone_Golem;
        public override bool CanTakeJob(Character character, JobQueueItem jobQueueItem) {
            return character.isFactionLeader || character.isSettlementRuler;
        }
    }
}