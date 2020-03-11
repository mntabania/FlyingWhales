using UnityEngine;
using Traits;

public class WorkBehaviour : CharacterBehaviourComponent {
    public WorkBehaviour() {
        priority = 40;
        //attributes = new[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.WITHIN_HOME_SETTLEMENT_ONLY };
    }
    
    public override bool TryDoBehaviour(Character character, ref string log) {
        log += $"\n-{character.name} is going to work or will do needs recovery...";
        if (!PlanJobQueueFirst(character)) {
            if (!PlanWorkActions(character)) {
                return false;
            }
            // if (!character.needsComponent.PlanFullnessRecoveryActions(character)) {
            //     if (!character.needsComponent.PlanTirednessRecoveryActions(character)) {
            //         if (!character.needsComponent.PlanHappinessRecoveryActions(character)) {
            //             
            //         }
            //     }
            // }
        }
        return true;
    }
    
    private bool PlanJobQueueFirst(Character character) {
        if (!character.needsComponent.isStarving && !character.needsComponent.isExhausted && !character.needsComponent.isSulking) {
            return PlanWorkActions(character);
        }
        return false;
    }
    private bool PlanWorkActions(Character character) {
        if (character.isAtHomeRegion && character.homeSettlement != null && character.canTakeJobs) { //&& this.faction.id != FactionManager.Instance.neutralFaction.id
            //check npcSettlement job queue, if it has any jobs that target an object that is in view of the character
            JobQueueItem jobToAssign = character.homeSettlement.GetFirstJobBasedOnVision(character);
            if (jobToAssign == null) {
                //if there are none, check the characters faction job queue under the same conditions.
                if (character.faction?.activeQuest != null) {
                    jobToAssign = character.faction.activeQuest.GetFirstJobBasedOnVision(character);
                }
            }
            if (jobToAssign != null) {
                //bool triggerLazy = false;
                //Lazy lazy = character.traitContainer.GetNormalTrait<Trait>("Lazy") as Lazy;
                //if (lazy != null) {
                //    triggerLazy = Random.Range(0, 100) < 20;
                //}
                //if (triggerLazy) {
                //    if (lazy.TriggerLazy()) {
                //        return true;
                //    } else {
                //        character.PrintLogIfActive($"Triggered LAZY happiness recovery but {character.name} already has that job type in queue and will not do it anymore!");
                //    }
                //}
                character.jobQueue.AddJobInQueue(jobToAssign);
                //took job based from vision
                return true;
            } else {
                //if none of the jobs targets can be seen by the character, try and get a job from the npcSettlement or faction
                //regardless of vision instead.
                jobToAssign = character.homeSettlement.GetFirstUnassignedJobToCharacterJob(character);
                if (jobToAssign == null) {
                    if (character.faction?.activeQuest != null) {
                        jobToAssign = character.faction.activeQuest.GetFirstUnassignedJobToCharacterJob(character);
                    }
                }
                if (jobToAssign != null) {
                    //bool triggerLazy = false;
                    //Lazy lazy = character.traitContainer.GetNormalTrait<Trait>("Lazy") as Lazy;
                    //if (lazy != null) {
                    //    triggerLazy = Random.Range(0, 100) < 20;
                    //}
                    //if (triggerLazy) {
                    //    if (lazy.TriggerLazy()) {
                    //        return true;
                    //    } else {
                    //        character.PrintLogIfActive($"Triggered LAZY happiness recovery but {character.name} already has that job type in queue and will not do it anymore!");
                    //    }
                    //}
                    character.jobQueue.AddJobInQueue(jobToAssign);
                    return true;
                }    
            }
        }
        return false;
    }
}
