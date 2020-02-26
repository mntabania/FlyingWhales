using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestInfoUI : MonoBehaviour {
    [Header("General")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI infoText;
    public TextMeshProUGUI jobText;
    public ScrollRect jobScrollRect;
    public GameObject questJobNameplatePrefab;

    public Quest quest { get; private set; }

    public List<QuestJobNameplate> activeQuestJobNameplates { get; private set; }

    public void Initialize() {
        activeQuestJobNameplates = new List<QuestJobNameplate>();
        Messenger.AddListener<Quest, JobQueueItem>(Signals.ADD_QUEST_JOB, OnAddQuestJob);
        Messenger.AddListener<Quest, JobQueueItem>(Signals.REMOVE_QUEST_JOB, OnRemoveQuestJob);
    }
    public void ShowQuestInfoUI(Quest quest) {
        this.quest = quest;
        PopulateQuestInfo();
        UpdateQuestJobs();
        gameObject.SetActive(true);
    }

    public void HideQuestInfoUI() {
        gameObject.SetActive(false);
    }

    public void UpdateQuestInfo() {
        //UpdateQuestJobs();
    }

    private void PopulateQuestInfo() {
        titleText.text = quest.name;
        descriptionText.text = quest.description;

        //string factionText = "<link=" + '"' + quest.factionOwner.id.ToString() + "_faction" + '"' + ">Faction Owner: <b>" + quest.factionOwner.name + "</b></link>";
        //string regionText = "<link=" + '"' + quest.region.coreTile.id.ToString() + "_hextile" + '"' + ">Region: <b>" + (quest.region.settlement != null ? quest.region.settlement.name : quest.region.name) + "</b></link>";

        string remainingDaysText = string.Empty;
        if(quest is DivineInterventionQuest) {
            int currentTime = quest.duration - quest.currentDuration;
            if (currentTime > GameManager.ticksPerDay) {
                remainingDaysText =
                    $"Remaining Days: <b>{GameManager.Instance.GetCeilingDaysBasedOnTicks(currentTime)}</b>";
            } else {
                remainingDaysText =
                    $"Remaining Hours: <b>{GameManager.Instance.GetCeilingHoursBasedOnTicks(currentTime)}</b>";
            }
        }
        infoText.text = remainingDaysText;

        //UpdateQuestJobs();
    }

    private void UpdateJobText() {
        string jobInfo = "Associated Events: ";
        if (quest.availableJobs.Count <= 0) {
            jobInfo = "Associated Events: None";
        }
        //if (quest.availableJobs.Count > 0) {
        //    for (int i = 0; i < quest.availableJobs.Count; i++) {
        //        JobQueueItem job = quest.availableJobs[i];
        //        jobInfo += "\n\t<link=" + '"' + i + "_job" + '"' + ">" + (i + 1) + ". <b>" + job.name + "</b></link>";
        //        //if (job.assignedCharacter != null) {
        //        //    jobInfo += "\n\t\t" + "<link=" + '"' + job.assignedCharacter.id.ToString() + "_character" + '"' + ">Assigned Character: <b>" + job.assignedCharacter.name + "</b></link>";
        //        //} else {
        //        //    jobInfo += "\n\t\tAssigned Character: <b>None</b>";
        //        //}
        //    }
        //} else {
        //    jobInfo += "None";
        //}
        jobText.text = jobInfo;
    }
    private void UpdateQuestJobs() {
        UpdateJobText();
        if(jobScrollRect.content.childCount > 0) {
            UtilityScripts.Utilities.DestroyChildren(jobScrollRect.content);
        }
        for (int i = 0; i < quest.availableJobs.Count; i++) {
            GenerateQuestJobNameplate(quest, quest.availableJobs[i], false);
        }
    }
    private void GenerateQuestJobNameplate(Quest quest, JobQueueItem job, bool updateJobText = true) {
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(questJobNameplatePrefab.name, Vector3.zero, Quaternion.identity, jobScrollRect.content);
        QuestJobNameplate item = go.GetComponent<QuestJobNameplate>();
        item.Initialize(quest, job);
        activeQuestJobNameplates.Add(item);
        if (updateJobText) {
            UpdateJobText();
        }
    }
    private void RemoveQuestJobNameplate(JobQueueItem job) {
        for (int i = 0; i < activeQuestJobNameplates.Count; i++) {
            QuestJobNameplate nameplate = activeQuestJobNameplates[i];
            if (nameplate.job == job) {
                activeQuestJobNameplates.RemoveAt(i);
                ObjectPoolManager.Instance.DestroyObject(nameplate);
                break;
            }
        }
        UpdateJobText();
    }
    #region Listeners
    private void OnAddQuestJob(Quest quest, JobQueueItem job) {
        if(gameObject.activeSelf && this.quest == quest) {
            GenerateQuestJobNameplate(quest, job);
        }
    }
    private void OnRemoveQuestJob(Quest quest, JobQueueItem job) {
        if (gameObject.activeSelf && this.quest == quest) {
            RemoveQuestJobNameplate(job);
        }
    }
    #endregion
}
