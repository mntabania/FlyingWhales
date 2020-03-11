using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EZObjectPools;

public class QuestJobNameplate : PooledObject {

    public JobQueueItem job { get; private set; }
    public Quest quest { get; private set; }

    public TextMeshProUGUI jobNameLbl;
    public Image jobIconImg;
    public Button mainBtn;
    public GameObject coverGO;

    public System.Action onClickAction { get; private set; }

    public void Initialize(Quest quest, JobQueueItem job) {
        this.job = job;
        this.quest = quest;
        UpdateInfo();
    }

    private void UpdateInfo() {
        if(job == null) { return; }
        jobNameLbl.text = job.name;
        //TODO: eventIconImg
    }
    public void SetClickAction(System.Action onClickAction) {
        this.onClickAction = onClickAction;
    }

    #region Object Pool
    public override void Reset() {
        base.Reset();
        job = null;
        quest = null;
    }
    #endregion

    #region Mouse Actions
    public void OnClickThis() {
        if (onClickAction != null) {
            onClickAction();
        }
    }
    public void OnHover() {
        string hoverText = string.Empty;
        if (job != null) {
            if (job.jobType == JOB_TYPE.CRAFT_OBJECT) {
                hoverText =
                    $"This quest aims to build a new Goddess Statue at {quest.region.name}. A Goddess Statue allows any resident to assist in speeding up the ritual by offering their own sincere prayer.";
            } 
            // else if (job.jobType == JOB_TYPE.DESTROY_PROFANE_LANDMARK) {
            //     hoverText = "This quest aims to destroy one of Ruinarch's Profane structures.";
            // }
            // else if (job.jobType == JOB_TYPE.PERFORM_HOLY_INCANTATION) {
            //     hoverText = "This quest aims to perform a holy incantation at a Hallowed Grounds. If successful, it will significantly speed up the ritual.";
            // }
            if (job.assignedCharacter != null) {
                hoverText += $" {job.assignedCharacter.name} is currently undertaking this quest.";
            }
        }

        if (hoverText != string.Empty) {
            UIManager.Instance.ShowSmallInfo(hoverText);
        }
    }
    public void OnHoverOut() {
        if (UIManager.Instance.IsSmallInfoShowing()) {
            UIManager.Instance.HideSmallInfo();
        }
    }
    #endregion
}
