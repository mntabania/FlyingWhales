using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;
using Random = UnityEngine.Random;

public class LocationJobManager {
    private NPCSettlement location { get; set; }

    private int _createJobsTriggerTick;
    private int currentExistingJobsCount;
    private List<string> _jobNames;

    public LocationJobManager(NPCSettlement location) {
        this.location = location;
        _jobNames = new List<string>();
        _createJobsTriggerTick = GameManager.Instance.GetTicksBasedOnHour(9); //150
        Messenger.AddListener(Signals.TICK_STARTED, ProcessJobs);
    }


    private void ProcessJobs() {
        if(GameManager.Instance.tick == _createJobsTriggerTick && currentExistingJobsCount < 2) {
            // if (!JobsPart1()) {
                if (!JobsPart2()) {
                    // JobsPart3();
                }
            // }
        }else if (GameManager.Instance.IsEndOfDay()) {
            OnEndOfDay();
        }
    }
    private void OnEndOfDay() {
        int chance = Random.Range(0, 2);
        if(currentExistingJobsCount > 0) {
            for (int i = 0; i < location.availableJobs.Count; i++) {
                JobQueueItem job = location.availableJobs[i];
                if(job.assignedCharacter == null && chance == 0 && IsJobAnOutsideJob(job)) {
                    job.ForceCancelJob(false);
                }
            }
        }

    }
    public void OnAddToAvailableJobs(JobQueueItem addedJob) {
        if(IsJobAnOutsideJob(addedJob)) {
            currentExistingJobsCount++;
        }
    }
    public void OnRemoveFromAvailableJobs(JobQueueItem removedJob) {
        if (IsJobAnOutsideJob(removedJob)) {
            currentExistingJobsCount--;
        }
    }
    private bool IsJobAnOutsideJob(JobQueueItem job) {
        if (job.jobType == JOB_TYPE.CLEANSE_CORRUPTION || job.jobType == JOB_TYPE.CLAIM_REGION) {
            return true;
        }
        return false;
    }

    #region Part 1
    private bool JobsPart1() {
        _jobNames.Clear();
        _jobNames.Add("Cleanse");
        _jobNames.Add("Claim");
        _jobNames.Add("Invade");

        bool hasCreateJob = false;
        while (!hasCreateJob && _jobNames.Count > 0) {
            var index = Random.Range(0, _jobNames.Count);
            var chosenJobName = _jobNames[index];
            if(chosenJobName == "Cleanse") {
                hasCreateJob = CreateCleanseRegionJob();
            }else if (chosenJobName == "Claim") {
                hasCreateJob = CreateClaimRegionJob();
            } 
            // else if (chosenJobName == "Invade") {
            //     hasCreateJob = CreateInvadeRegionJob();
            // }
            _jobNames.RemoveAt(index);
        }
        return hasCreateJob;
    }
    private bool CreateCleanseRegionJob() {
        if (Random.Range(0, 2) <= 0) {
            Region region;
            if (TryGetCorruptedRegionWithoutLandmark(out region)) {
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CLEANSE_CORRUPTION, INTERACTION_TYPE.CLEANSE_REGION, region.regionTileObject, location);
                job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanDoCleanseRegionJob);
                location.AddToAvailableJobs(job);
                return true;
            }
        }
        return false;
    }
    private bool CreateClaimRegionJob() {
        if (Random.Range(0, 2) == 0) {
            Region region;
            if (TryGetAdjacentRegionWithoutFactionOwner(out region)) {
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CLAIM_REGION, INTERACTION_TYPE.CLAIM_REGION, region.regionTileObject, location);
                job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanDoClaimRegionJob);
                location.AddToAvailableJobs(job);
                return true;
            }
        }
        return false;
    }
    // private bool CreateInvadeRegionJob() {
    //     if (Random.Range(0, 2) == 0) {
    //         Region region;
    //         if (TryGetAdjacentRegionAtWarWithThisLocation(out region)) {
    //             GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.INVADE_REGION, INTERACTION_TYPE.INVADE_REGION, region.regionTileObject, location);
    //             job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanDoInvadeRegionJob);
    //             location.AddToAvailableJobs(job);
    //             return true;
    //         }
    //     }
    //     return false;
    // }
    private bool TryGetCorruptedRegionWithoutLandmark(out Region validRegion) {
        //TODO:
        // List<Region> regions = PlayerManager.Instance.player.playerFaction.ownedSettlements;
        // for (int i = 0; i < regions.Count; i++) {
        //     Region region = regions[i];
        //     if(region.mainLandmark.specificLandmarkType == LANDMARK_TYPE.NONE) {
        //         validRegion = region;
        //         return true;
        //     }
        // }
        validRegion = null;
        return false;
    }
    private bool TryGetAdjacentRegionWithoutFactionOwner(out Region validRegion) {
        //TODO:
        // List<RegionConnectionData> connectedRegions = location.region.connections;
        // for (int i = 0; i < connectedRegions.Count; i++) {
        //     Region region = connectedRegions[i].region;
        //     if (!region.coreTile.isCorrupted && region.owner == null) {
        //         validRegion = region;
        //         return true;
        //     }
        // }
        validRegion = null;
        return false;
    }
    private bool TryGetAdjacentRegionAtWarWithThisLocation(out Region validRegion) {
        //TODO:
        // List<RegionConnectionData> connectedRegions = location.region.connections;
        // for (int i = 0; i < connectedRegions.Count; i++) {
        //     Region region = connectedRegions[i].region;
        //     if (!region.coreTile.isCorrupted && region.npcSettlement == null && region.owner != null
        //         && region.residents.Count <= 0 && region.owner.HasRelationshipStatusWith(FACTION_RELATIONSHIP_STATUS.HOSTILE, location.region.owner)) {
        //         validRegion = region;
        //         return true;
        //     }
        // }
        validRegion = null;
        return false;
    }
    #endregion

    #region Part 2
    private bool JobsPart2() {
        _jobNames.Clear();
        // _jobNames.Add("AttackNonDemonic");
        _jobNames.Add("AttackDemonic");

        bool hasCreateJob = false;
        while (!hasCreateJob && _jobNames.Count > 0) {
            var index = Random.Range(0, _jobNames.Count);
            var chosenJobName = _jobNames[index];
            // if (chosenJobName == "AttackNonDemonic") {
            //     hasCreateJob = CreateAttackNonDemonicRegionJobPart2();
            // } else 
            if (chosenJobName == "AttackDemonic") {
                hasCreateJob = CreateAttackDemonicRegionJobPart2();
            }
            _jobNames.RemoveAt(index);
        }
        return hasCreateJob;
    }
    // private bool CreateAttackNonDemonicRegionJobPart2() {
    //     if (Random.Range(0, 2) == 0) {
    //         Region validRegion;
    //         if (TryGetOccupiedNonSettlementTileAtWarWithThisLocation(out validRegion)) {
    //             validRegion = AttackNonDemonicRegionRegionGetter();
    //             if (validRegion.regionTileObject == null) {
    //                 throw new Exception($"Valid region's tile object is null! Valid region is {validRegion.name}");
    //             }
    //             GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.ATTACK_NON_DEMONIC_REGION, INTERACTION_TYPE.ATTACK_REGION, validRegion.regionTileObject, location);
    //             job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanDoAttackNonDemonicRegionJob);
    //             location.AddToAvailableJobs(job);
    //             return true;
    //         }
    //     }
    //     return false;
    // }
    private bool CreateAttackDemonicRegionJobPart2() {
        // if (Random.Range(0, 2) == 0) {
            Region region;
            if (TryGetCorruptedRegionWithoutLandmark(out region) == false) {
                region = AttackDemonicRegionRegionGetter();
                CreateAttackDemonicRegionJob(region);
                return true;
            }
        // }
        return false;
    }
    private bool TryGetOccupiedNonSettlementTileAtWarWithThisLocation(out Region validRegion) {
        //TODO:
        // Region[] regions = GridMap.Instance.allRegions;
        // for (int i = 0; i < regions.Length; i++) {
        //     Region region = regions[i];
        //     if (region.residents.Count > 0 && region.npcSettlement == null && region.owner.HasRelationshipStatusWith(FACTION_RELATIONSHIP_STATUS.HOSTILE, location.region.owner)) {
        //         validRegion = region;
        //         return true;
        //     }
        // }
        validRegion = null;
        return false;
    }
    #endregion

    #region Part 3
    private void JobsPart3() {
        _jobNames.Clear();
        _jobNames.Add("AttackNonDemonic");
        _jobNames.Add("AttackDemonic");

        bool hasCreateJob = false;
        while (!hasCreateJob && _jobNames.Count > 0) {
            var index = Random.Range(0, _jobNames.Count);
            var chosenJobName = _jobNames[index];
            // if (chosenJobName == "AttackNonDemonic") {
            //     hasCreateJob = CreateAttackNonDemonicRegionJobPart3();
            // } else 
            if (chosenJobName == "AttackDemonic") {
                hasCreateJob = CreateAttackDemonicRegionJobPart3();
            }
            _jobNames.RemoveAt(index);
        }
    }
    // private bool CreateAttackNonDemonicRegionJobPart3() {
    //     if (Random.Range(0, 4) == 0) {
    //         if (IsAtWarWithFactionThatSettlementNotBlockedByGarrison()) {
    //             Region target = AttackNonDemonicRegionRegionGetter();
    //             GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.ATTACK_NON_DEMONIC_REGION,
    //                 INTERACTION_TYPE.ATTACK_REGION, target.regionTileObject, location);
    //             job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanDoAttackNonDemonicRegionJob);
    //             location.AddToAvailableJobs(job);
    //             return true;
    //         }
    //     }
    //     return false;
    // }
    private bool CreateAttackDemonicRegionJobPart3() {
        if (Random.Range(0, 2) == 0) {
            if (IsPlayerOnlyRemainingStructureIsPortal()) {
                Region target = AttackDemonicRegionRegionGetter();
                CreateAttackDemonicRegionJob(target);
                return true;
            }
        }
        return false;
    }
    private bool IsAtWarWithFactionThatSettlementNotBlockedByGarrison() {
        //TODO: Is blocked by garrison checker
        // foreach (KeyValuePair<Faction, FactionRelationship> kvp in location.region.owner.relationships) {
        //     if (kvp.Key.isPlayerFaction) {
        //         continue; //exclude player faction
        //     }
        //     // if (kvp.Value.relationshipStatus == FACTION_RELATIONSHIP_STATUS.HOSTILE && kvp.Key.HasOwnedRegionWithSettlement()) {
        //     //     return true;
        //     // }
        // }
        return false;
    }
    private bool IsPlayerOnlyRemainingStructureIsPortal() {
        //TODO:
        // List<Region> regions = PlayerManager.Instance.player.playerFaction.ownedSettlements;
        // for (int i = 0; i < regions.Count; i++) {
        //     Region region = regions[i];
        //     if (region.mainLandmark != null && region.mainLandmark.specificLandmarkType != LANDMARK_TYPE.THE_PORTAL) {
        //         return false;
        //     }
        // }
        return false;
    }
    #endregion

    #region Region Getters
    private Region AttackNonDemonicRegionRegionGetter() {
        List<Region> choices = new List<Region>();
        // for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
        //     Region region = GridMap.Instance.allRegions[i];
        //     if (region.mainLandmark.specificLandmarkType != LANDMARK_TYPE.NONE 
        //         && region.locationType.IsSettlementType() == false
        //         && region.owner != null && region.owner != PlayerManager.Instance.player.playerFaction) {
        //         FactionRelationship rel = region.owner.GetRelationshipWith(location.owner);
        //         //TODO:
        //         // if (rel != null && rel.relationshipStatus == FACTION_RELATIONSHIP_STATUS.HOSTILE 
        //         //     && region.IsConnectedToRegionOwnedBy(location.owner)) {
        //         //     choices.Add(region);
        //         // }
        //     }
        // }

        if (choices.Count == 0) {
            //TODO:
            // for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
            //     Region region = GridMap.Instance.allRegions[i];
            //     if (region.mainLandmark.specificLandmarkType != LANDMARK_TYPE.NONE 
            //         && region.locationType.IsSettlementType() == false
            //         && region.owner != null && region.owner != PlayerManager.Instance.player.playerFaction) {
            //         FactionRelationship rel = region.owner.GetRelationshipWith(location.owner);
            //         if (rel != null && rel.relationshipStatus == FACTION_RELATIONSHIP_STATUS.HOSTILE) {
            //             choices.Add(region);
            //         }
            //     }
            // }    
        }

        if (choices.Count == 0) {
            //TODO:
            // for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
            //     Region region = GridMap.Instance.allRegions[i];
            //     if (region.locationType.IsSettlementType() == false
            //         && region.owner != null && region.owner != PlayerManager.Instance.player.playerFaction) {
            //         FactionRelationship rel = region.owner.GetRelationshipWith(location.owner);
            //         if (rel != null && rel.relationshipStatus == FACTION_RELATIONSHIP_STATUS.HOSTILE) {
            //             choices.Add(region);
            //         }
            //     }
            // }    
        }

        if (choices.Count > 0) {
            return CollectionUtilities.GetRandomElement(choices);
        }
        
        return null;
    }
    private Region AttackDemonicRegionRegionGetter() {
        List<Region> choices = new List<Region>();
        for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
            Region region = GridMap.Instance.allRegions[i];
            //TODO:
            // if (region.mainLandmark.specificLandmarkType != LANDMARK_TYPE.NONE 
            //     && region.owner == PlayerManager.Instance.player.playerFaction 
            //     && region.IsConnectedToRegionOwnedBy(location.owner)) {
            //     choices.Add(region);
            // }
        }

        if (choices.Count == 0) {
            for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
                Region region = GridMap.Instance.allRegions[i];
                //TODO:
                // if (region.mainLandmark.specificLandmarkType != LANDMARK_TYPE.NONE 
                //     && region.mainLandmark.specificLandmarkType != LANDMARK_TYPE.THE_PORTAL
                //     && region.owner == PlayerManager.Instance.player.playerFaction 
                //     && region.IsConnectedToRegionThatSatisfies(IsRegionConnectedToNonCorruptedRegion)) {
                //     choices.Add(region);
                // }
            }
        }

        // if (choices.Count == 0) {
        //     return PlayerManager.Instance.player.playerNpcSettlement.region;
        // }
        if (choices.Count > 0) {
            return CollectionUtilities.GetRandomElement(choices);    
        }
        return null;
    }
    #endregion

    #region Job Creators
    private void CreateAttackDemonicRegionJob(Region targetRegion) {
        // GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.ATTACK_DEMONIC_REGION, INTERACTION_TYPE.ATTACK_REGION, targetRegion.regionTileObject, location);
        // job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanDoAttackDemonicRegionJob);
        // location.AddToAvailableJobs(job);
    }
    #endregion
}
