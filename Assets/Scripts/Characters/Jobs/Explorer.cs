﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class Explorer : Job {

    public Explorer(Character character) : base(character, JOB.EXPLORER) {
        _actionDuration = 120;
        _hasCaptureEvent = false;
    }

    #region Overrides
    public override void DoJobAction() {
        base.DoJobAction();
        string jobSummary = GameManager.Instance.TodayLogString() + " " + _character.name + " job summary: ";

        int baseSuccessRate = 50;
        int baseFailRate = 40;
        int criticalFailRate = 12;

        //Success Rate +1 per level starting at Level 6
        baseSuccessRate += (Mathf.Max(character.level - 5, 0));
        //Critical Fail Rate -1 per mult of 4 level starting at Level 6
        if (character.level > 6) {
            criticalFailRate -= Mathf.FloorToInt(character.level / 4);
        }

        WeightedDictionary<JOB_RESULT> rateWeights = new WeightedDictionary<JOB_RESULT>();
        rateWeights.AddElement(JOB_RESULT.SUCCESS, baseSuccessRate);
        rateWeights.AddElement(JOB_RESULT.FAIL, baseFailRate);
        rateWeights.AddElement(JOB_RESULT.CRITICAL_FAIL, criticalFailRate);
        jobSummary += "\n" + rateWeights.GetWeightsSummary("Rates summary ");
        if (rateWeights.GetTotalOfWeights() > 0) {
            JOB_RESULT chosenResult = rateWeights.PickRandomElementGivenWeights();
            jobSummary += "\nRate result: " + chosenResult.ToString() + ".";
            switch (chosenResult) {
                case JOB_RESULT.SUCCESS:
                    //TODO: If Success was triggered: spawn an event from Exploration Event of current area
                    SetCreatedInteraction(InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.RAID_SUCCESS, character.specificLocation.tileLocation.landmarkOnTile));
                    _createdInteraction.SetEndInteractionAction(() => StartJobAction());
                    _createdInteraction.ScheduleSecondTimeOut();
                    _createdInteraction.SetOtherData(new object[] { 0 });
                    character.AddInteraction(_createdInteraction);
                    break;
                case JOB_RESULT.FAIL:
                    SetCreatedInteraction(InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MINION_FAILED, character.specificLocation.tileLocation.landmarkOnTile));
                    _createdInteraction.SetEndInteractionAction(() => StartJobAction());
                    _createdInteraction.ScheduleSecondTimeOut();
                    character.specificLocation.tileLocation.areaOfTile.coreTile.landmarkOnTile.AddInteraction(_createdInteraction);
                    break;
                case JOB_RESULT.CRITICAL_FAIL:
                    SetCreatedInteraction(InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MINION_CRITICAL_FAIL, character.specificLocation.tileLocation.landmarkOnTile));
                    _createdInteraction.SetEndInteractionAction(() => StartJobAction());
                    _createdInteraction.ScheduleSecondTimeOut();
                    character.specificLocation.tileLocation.areaOfTile.coreTile.landmarkOnTile.AddInteraction(_createdInteraction);
                    break;
                default:
                    break;
            }
        } else {
            StartJobAction();
        }
        Debug.Log(jobSummary);
    }
    public override void ApplyActionDuration() {
        _actionDuration = 120 - (3 * (Mathf.Max(_character.level - 5, 0)));
    }
    #endregion
}
