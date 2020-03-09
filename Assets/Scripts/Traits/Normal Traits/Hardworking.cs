using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Hardworking : Trait {

        public int jobID { get; private set; } //The job that is currently being done in place of happiness recovery

        public Hardworking() {
            name = "Hardworking";
            description = "Hardworking characters enjoy taking on npcSettlement tasks.";
            type = TRAIT_TYPE.BUFF;
            effect = TRAIT_EFFECT.NEUTRAL;
            
            
            
            ticksDuration = 0;
            SetJobIDReplacementForHappinessRecovery(0);
            mutuallyExclusive = new string[] { "Lazy" };
        }
        public void SetJobIDReplacementForHappinessRecovery(int id) {
            jobID = id;
        }
        //Returns true if processing for happiness recovery will not continue, otherwise return false
        //Ref bool - true or false if the process for planning happiness recovery is considered as processed or not
        public bool ProcessHardworkingTrait(Character character, ref bool isPlanningHappinessRecoveryProcessed) {
            if (jobID != 0) {
                //If there is a replacement job for happiness recovery, check if the job is still in the job queue and the character assign is still this character, if not set job id to zero again
                //TODO:
                //JobQueueItem job = character.specificLocation.jobQueue.GetJobByID(jobID);
                //if (job == null) {
                //    SetJobIDReplacementForHappinessRecovery(0);
                //} else if (job.assignedCharacter != character) {
                //    SetJobIDReplacementForHappinessRecovery(0);
                //}
            }
            if (jobID == 0) {
                if (UnityEngine.Random.Range(0, 100) < 35) { //20
                    //string log = GameManager.Instance.TodayLogString() + character.name + " will do a task instead of happiness recovery because he is hardworking!";
                    //If there is no replacement npcSettlement job for happiness recovery, process one
                    //Only get the first npcSettlement job if it will be the highest priority in your personal jobs and it can override the current job
                    //TODO:
                    //JobQueueItem jobToReplace = character.specificLocation.jobQueue.GetFirstUnassignedJobInQueue(character);
                    //if (jobToReplace != null) {
                    //    log += "\nJob to be replaced: " + jobToReplace.jobType.ToString();
                    //    if (character.CanCurrentJobBeOverriddenByJob(jobToReplace)) {
                    //        if (character.jobQueue.jobsInQueue.Count > 0) {
                    //            log += "\nThere are personal jobs, highest prio in personal jobs is "
                    //                + character.jobQueue.jobsInQueue[0].jobType.ToString() + ":" + character.jobQueue.jobsInQueue[0].priority
                    //                + " VS " + jobToReplace.jobType.ToString() + ":" + jobToReplace.priority;
                    //            character.PrintLogIfActive(log);
                    //            //This checks if the npcSettlement job will become the highest priority job than any personal job this character currently has
                    //            //If it is, get it as replacement for happiness recovery job
                    //            if (jobToReplace.priority < character.jobQueue.jobsInQueue[0].priority) {
                    //                character.specificLocation.jobQueue.ForceAssignCharacterToJob(jobToReplace, character);
                    //                SetJobIDReplacementForHappinessRecovery(jobToReplace.id);
                    //                isPlanningHappinessRecoveryProcessed = true;
                    //                return true;
                    //            }
                    //        } else {
                    //            log += "\nNo personal jobs, automatically get npcSettlement job...";
                    //            character.PrintLogIfActive(log);
                    //            //Since there are no personal jobs, automatically get the npcSettlement job
                    //            character.specificLocation.jobQueue.ForceAssignCharacterToJob(jobToReplace, character);
                    //            SetJobIDReplacementForHappinessRecovery(jobToReplace.id);
                    //            isPlanningHappinessRecoveryProcessed = true;
                    //            return true;
                    //        }
                    //    }
                    //} else {
                    //    log += "\nNo personal jobs, automatically get npcSettlement job...";
                    //    character.PrintLogIfActive(log);
                    //    isPlanningHappinessRecoveryProcessed = false;
                    //    return false;
                    //}
                }
            } else {
                //If there is still job replacement for happiness recovery do not process happiness recovery
                isPlanningHappinessRecoveryProcessed = false;
                return true;
            }
            return false;
        }
    }

    public class SaveDataHardworking : SaveDataTrait {
        public int jobID;

        public override void Save(Trait trait) {
            base.Save(trait);
            Hardworking hardworking = trait as Hardworking;
            jobID = hardworking.jobID;
        }

        public override Trait Load(ref Character responsibleCharacter) {
            Trait trait = base.Load(ref responsibleCharacter);
            Hardworking hardworking = trait as Hardworking;
            hardworking.SetJobIDReplacementForHappinessRecovery(jobID);
            return trait;
        }
    }
}


