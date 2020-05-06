using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettlementJobPriorityComponent
{
    public NPCSettlement settlement { get; private set; }
    public Dictionary<JOB_TYPE, int> primaryJobTracker { get; private set; }

    public SettlementJobPriorityComponent(NPCSettlement settlement) {
        this.settlement = settlement;
        ConstructPrimaryJobTracker();
    }

    #region Utilities
    private void ConstructPrimaryJobTracker() {
        primaryJobTracker = new Dictionary<JOB_TYPE, int>() {
            { JOB_TYPE.PRODUCE_FOOD, 0 },
            { JOB_TYPE.PRODUCE_WOOD, 0 },
            { JOB_TYPE.HAUL, 0 },
            { JOB_TYPE.DOUSE_FIRE, 0 },
            { JOB_TYPE.REPAIR, 0 },
            { JOB_TYPE.CRAFT_OBJECT, 0 },
            { JOB_TYPE.PATROL, 0 },
            { JOB_TYPE.RESTRAIN, 0 },
            { JOB_TYPE.REMOVE_STATUS, 0 },
            { JOB_TYPE.TEND_FARM, 0 },
        };
    }
    public void OnAddResident(Character character) {
        AssignResidentToPrimaryJob(character);
    }
    public void OnRemoveResident(Character character) {
        UnassignResidentToPrimaryJob(character);
    }
    private void AssignResidentToPrimaryJob(Character character) {
        JOB_TYPE[] priorityJobs = character.characterClass.priorityJobs;
        if (priorityJobs != null && priorityJobs.Length > 0) {
            bool hasSetPrimaryJob = false;
            character.jobComponent.primaryJobCandidates.Clear();
            for (int i = 0; i < priorityJobs.Length; i++) {
                JOB_TYPE currPrioJob = priorityJobs[i];
                if (primaryJobTracker.ContainsKey(currPrioJob)) {
                    character.jobComponent.primaryJobCandidates.Add(currPrioJob);
                    if (primaryJobTracker[currPrioJob] == 0) {
                        character.jobComponent.SetPrimaryJob(currPrioJob);
                        primaryJobTracker[currPrioJob]++;
                        hasSetPrimaryJob = true;
                        break;
                    }
                }
            }
            if (!hasSetPrimaryJob) {
                for (int i = 0; i < character.jobComponent.priorityJobs.Count; i++) {
                    JOB_TYPE currPrioJob = character.jobComponent.priorityJobs[i];
                    if (primaryJobTracker.ContainsKey(currPrioJob)) {
                        character.jobComponent.primaryJobCandidates.Add(currPrioJob);
                        if (primaryJobTracker[currPrioJob] == 0) {
                            character.jobComponent.SetPrimaryJob(currPrioJob);
                            primaryJobTracker[currPrioJob]++;
                            hasSetPrimaryJob = true;
                            break;
                        }
                    }
                }
            }
            if (!hasSetPrimaryJob && character.jobComponent.primaryJobCandidates.Count > 0) {
                JOB_TYPE chosenJob = character.jobComponent.primaryJobCandidates[UnityEngine.Random.Range(0, character.jobComponent.primaryJobCandidates.Count)];
                character.jobComponent.SetPrimaryJob(chosenJob);
                primaryJobTracker[chosenJob]++;
            }
        }
    }
    public void UnassignResidentToPrimaryJob(Character character) {
        JOB_TYPE characterPrimaryJob = character.jobComponent.primaryJob;
        if (primaryJobTracker.ContainsKey(characterPrimaryJob)) {
            character.jobComponent.SetPrimaryJob(JOB_TYPE.NONE);
            primaryJobTracker[characterPrimaryJob]--;
        }
    }
    public string GetJobAssignments() {
        string assignments = string.Empty;
        if(primaryJobTracker != null) {
            foreach (KeyValuePair<JOB_TYPE, int> item in primaryJobTracker) {
                assignments += "\n" + item.Key.ToString() + " - " + item.Value;
            }
        }
        return assignments;
    }
    #endregion
}
