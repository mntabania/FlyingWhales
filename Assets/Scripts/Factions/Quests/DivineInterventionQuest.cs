using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;

public class DivineInterventionQuest : Quest {

    public DivineInterventionQuest(Faction factionOwner, Region region) : base(factionOwner, region) {
        name = "Divine Intervention Quest";
        description =
            $"{factionOwner.name} has begun the ritual that will call for the Goddess's Return. If the ritual finishes, this Divine Intervention will cleanse the world of all the Ruinarch's corruption and banish the demons back to the Nether Realm.";
        SetDuration(PlayerDB.DIVINE_INTERVENTION_DURATION);
        SetCurrentDuration(0);
    }

    public DivineInterventionQuest(SaveDataQuest data) : base(data) {
    }

    #region Overrides
    public override void ActivateQuest() {
        base.ActivateQuest();
        TimerHubUI.Instance.AddItem("Until Divine Intervention", duration, () => UIManager.Instance.ShowQuestInfo(this));
    }
    public override void FinishQuest() {
        base.FinishQuest();
        TimerHubUI.Instance.RemoveItem("Until Divine Intervention");
        PlayerUI.Instance.GameOver("The gods have arrived and wiped you off from the face of this planet!");
    }
    protected override void PerTickOnQuest() {
        //Remove jobs at the end of the day if it hasn't been taken
        //if (GameManager.Instance.tick == GameManager.ticksPerDay) {
        //    for (int i = 0; i < jobQueue.jobsInQueue.Count; i++) {
        //        JobQueueItem job = jobQueue.jobsInQueue[i];
        //        if(job.assignedCharacter == null) {
        //            jobQueue.CancelJob(job);
        //            i--;
        //        }
        //    }
        //}

        //Cancel jobs that are no longer applicable
        //Might have performance issue, but for now this will do
        //Will improve later
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem job = availableJobs[i];
            // if(job.jobType == JOB_TYPE.CRAFT_OBJECT && !CanStillCreateGoddessStatues()) {
            //     job.CancelJob();
            //     i--;
            // }
            // else if (job.jobType == JOB_TYPE.DESTROY_PROFANE_LANDMARK && !AreThereProfaneLandmarks()) {
            //     job.CancelJob();
            //     i--;
            // }
            // else if (job.jobType == JOB_TYPE.PERFORM_HOLY_INCANTATION && !AreThereHallowedGrounds()) {
            //     job.CancelJob();
            //     i--;
            // }
        }

        // TryCreateBuildGoddessStatueJob();
        // TryCreateDestroyProfaneLandmarkJob();
        // TryCreatePerformHolyIncantationJob();
        base.PerTickOnQuest();
    }
    public override void AdjustCurrentDuration(int amount) {
        base.AdjustCurrentDuration(amount);
        if(currentDuration < duration) {
            TimerHubUI.Instance.SetItemTimeDuration("Until Divine Intervention", duration - currentDuration);
        }
    }
    #endregion

    // private void TryCreateBuildGoddessStatueJob() {
    //     if (GameManager.Instance.tick == 72 || GameManager.Instance.tick == 132) { //72 = 6:00AM, 132 = 11:00AM
    //         string summary = GameManager.Instance.TodayLogString() + " Will try to create build goddess statue job";
    //         int roll = UnityEngine.Random.Range(0, 100);
    //         bool hasExistingJob = HasJob(JOB_TYPE.CRAFT_OBJECT);
    //         bool canStillCreateGoddessStatue = CanStillCreateGoddessStatues();
    //         summary += "\nRoll is: " + roll.ToString();
    //         summary += "\nHas Existing Job?: " + hasExistingJob.ToString();
    //         summary += "\nHas empty goddess statue?: " + canStillCreateGoddessStatue.ToString();
    //         if (roll < 20 && !hasExistingJob && canStillCreateGoddessStatue) {
    //             summary += "\nMet all requirements, creating build goddess statue job.";
    //             //Create Job Here
    //             CreateBuildGoddessStatueJob();
    //         }
    //         Debug.Log(summary);
    //     }
    // }
    private bool CanStillCreateGoddessStatues() {
        return region.GetTileObjectsOfType(TILE_OBJECT_TYPE.GODDESS_STATUE).Count < 4;
    }

    // private void TryCreateDestroyProfaneLandmarkJob() {
    //     if (GameManager.Instance.tick == 72 || GameManager.Instance.tick == 132) { //72 = 6:00AM, 132 = 11:00AM
    //         string summary = GameManager.Instance.TodayLogString() + " Will try to create build goddess statue job";
    //         int roll = UnityEngine.Random.Range(0, 100);
    //         bool hasExistingJob = HasJob(JOB_TYPE.DESTROY_PROFANE_LANDMARK);
    //         bool hasProfaneLandmarks = AreThereProfaneLandmarks();
    //         summary += "\nRoll is: " + roll.ToString();
    //         summary += "\nHas Existing Job?: " + hasExistingJob.ToString();
    //         summary += "\nHas profane landmarks?: " + hasProfaneLandmarks.ToString();
    //         if (roll < 20 && !hasExistingJob && hasProfaneLandmarks) {
    //             summary += "\nMet all requirements, creating destroy profane job.";
    //             //Create Job Here
    //             CreateDestroyProfaneJob();
    //         }
    //         Debug.Log(summary);
    //     }
    // }
    private bool AreThereProfaneLandmarks() {
        return false;
    }
    // private void TryCreatePerformHolyIncantationJob() {
    //     if (GameManager.Instance.tick == 72 || GameManager.Instance.tick == 132) { //72 = 6:00AM, 132 = 11:00AM
    //         string summary = GameManager.Instance.TodayLogString() + " Will try to create build goddess statue job";
    //         int roll = UnityEngine.Random.Range(0, 100);
    //         bool hasExistingJob = HasJob(JOB_TYPE.PERFORM_HOLY_INCANTATION);
    //         bool hasHallowedGrounds = AreThereHallowedGrounds();
    //         summary += "\nRoll is: " + roll.ToString();
    //         summary += "\nHas Existing Job?: " + hasExistingJob.ToString();
    //         summary += "\nHas hallowed grounds?: " + hasHallowedGrounds.ToString();
    //         if (roll < 20 && !hasExistingJob && hasHallowedGrounds) {
    //             summary += "\nMet all requirements, creating holy incantation job.";
    //             //Create Job Here
    //             CreateHolyIncantationJob();
    //         }
    //         Debug.Log(summary);
    //     }
    // }
    private bool AreThereHallowedGrounds() {
        for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
            Region currRegion = GridMap.Instance.allRegions[i];
            if (currRegion.HasTileWithFeature(TileFeatureDB.Hallowed_Ground_Feature)) {
                return true;
            }
        }
        return false;
    }

    #region Build Goddess Statue
    // private void CreateBuildGoddessStatueJob() {
    //     LocationStructure structure = region.GetRandomStructureOfType(STRUCTURE_TYPE.WORK_AREA);
    //     GoddessStatue goddessStatue = InnerMapManager.Instance.CreateNewTileObject<GoddessStatue>(TILE_OBJECT_TYPE.GODDESS_STATUE);
    //     structure.AddPOI(goddessStatue);
    //     goddessStatue.SetMapObjectState(MAP_OBJECT_STATE.UNBUILT);
    //     GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CRAFT_OBJECT, INTERACTION_TYPE.CRAFT_TILE_OBJECT, goddessStatue, this);
    //     job.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[] { TileObjectDB.GetTileObjectData(goddessStatue.tileObjectType).constructionCost });
    //     job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeBuildGoddessStatueJob);
    //     AddToAvailableJobs(job);
    //
    //     //expires at end of day
    //     GameDate expiryDate = GameManager.Instance.Today();
    //     expiryDate.SetTicks(GameManager.ticksPerDay);
    //     SchedulingManager.Instance.AddEntry(expiryDate, () => CheckIfJobWillExpire(job), this);
    // }
    #endregion

    // #region Destroy Profane
    // private void CreateDestroyProfaneJob() {
    //     Region target = LandmarkManager.Instance.GetLandmarkOfType(LANDMARK_TYPE.THE_PROFANE).tileLocation.region;
    //     GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DESTROY_PROFANE_LANDMARK, INTERACTION_TYPE.ATTACK_REGION, target.regionTileObject, this);
    //     job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanDoDestroyProfaneJob);
    //     AddToAvailableJobs(job);
    //
    //     //expires at end of day
    //     GameDate expiryDate = GameManager.Instance.Today();
    //     expiryDate.SetTicks(GameManager.ticksPerDay);
    //     SchedulingManager.Instance.AddEntry(expiryDate, () => CheckIfJobWillExpire(job), this);
    // }
    // #endregion

    // #region Holy Incantation
    // private void CreateHolyIncantationJob() {
    //     Region target = LandmarkManager.Instance.GetRandomRegionWithFeature(TileFeatureDB.Hallowed_Ground_Feature);
    //     GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PERFORM_HOLY_INCANTATION, INTERACTION_TYPE.HOLY_INCANTATION, target.regionTileObject, this);
    //     job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanDoHolyIncantationJob);
    //     AddToAvailableJobs(job);
    //
    //     //expires at end of day
    //     GameDate expiryDate = GameManager.Instance.Today();
    //     expiryDate.SetTicks(GameManager.ticksPerDay);
    //     SchedulingManager.Instance.AddEntry(expiryDate, () => CheckIfJobWillExpire(job), this);
    // }
    // #endregion

    #region Sabotage Faction
    public void CreateSabotageFactionJob() {
        if (!HasJob(JOB_TYPE.CORRUPT_CULTIST_SABOTAGE_FACTION)) {
            Region target = LandmarkManager.Instance.GetRandomRegionWithFeature(TileFeatureDB.Hallowed_Ground_Feature);
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CORRUPT_CULTIST_SABOTAGE_FACTION, INTERACTION_TYPE.DEMONIC_INCANTATION, target.regionTileObject, this);
            job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanDoSabotageFactionJob);
            AddToAvailableJobs(job);
        }
    }
    #endregion

    #region Utilities
    private void CheckIfJobWillExpire(JobQueueItem item) {
        if (item.assignedCharacter == null && item.assignedCharacter != null && item.assignedCharacter.jobQueue.jobsInQueue.Contains(item)) {
            Debug.Log($"{GameManager.Instance.TodayLogString()}{item.jobType} expired.");
            item.assignedCharacter.jobQueue.RemoveJobInQueue(item);
        }
    }
    #endregion
}
