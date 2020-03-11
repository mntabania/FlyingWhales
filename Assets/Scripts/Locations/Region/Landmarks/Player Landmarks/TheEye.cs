using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Obsolete("Player landmarks should no longer be used, use the LocationStructure version instead.")]
public class TheEye : BaseLandmark {

    // public int currentCooldownTick { get; private set; }
    // public int cooldownDuration { get; private set; }
    //
    // public static readonly int interfereManaCost = 100;
    //
    // public bool isInCooldown {
    //     get { return currentCooldownTick < cooldownDuration; }
    // }

    public TheEye(HexTile location, LANDMARK_TYPE specificLandmarkType) : base(location, specificLandmarkType) {
        // cooldownDuration = GameManager.Instance.GetTicksBasedOnHour(4);
        // currentCooldownTick = cooldownDuration;
    }

    public TheEye(HexTile location, SaveDataLandmark data) : base(location, data) {
        // cooldownDuration = GameManager.Instance.GetTicksBasedOnHour(4);
    }

    public void LoadSavedData(SaveDataTheEye data) {
        // if (data.currentCooldownTick < cooldownDuration) {
        //     StartCooldown();
        // }
        // currentCooldownTick = data.currentCooldownTick;
    }

    public void StartInterference(Character targetCharacter, Character interferingCharacter) {
        interferingCharacter.minion.SetAssignedRegion(targetCharacter.currentRegion);  //only set assigned region to minion.
        GoapPlanJob job = null;
        for (int i = 0; i < targetCharacter.jobQueue.jobsInQueue.Count; i++) {
            JobQueueItem jqi = targetCharacter.jobQueue.jobsInQueue[i];
            if (jqi is GoapPlanJob) {
                GoapPlanJob gpj = jqi as GoapPlanJob;
                if (gpj.targetPOI is TileObject && (gpj.targetPOI as TileObject).tileObjectType == TILE_OBJECT_TYPE.REGION_TILE_OBJECT) {
                    job = gpj;
                }
            }
        }
        job?.ForceCancelJob(false, $"Interfered by {interferingCharacter.name}");
        // PlayerManager.Instance.player.AdjustMana(-interfereManaCost);
        //Start cooldown.
        // StartCooldown();
    }

    // #region Override
    // public override void DestroyLandmark() {
    //     StopCooldown();
    //     base.DestroyLandmark();
    // }
    // #endregion

    // #region Cooldown
    // private void StartCooldown() {
    //     currentCooldownTick = 0;
    //     Messenger.AddListener(Signals.TICK_ENDED, PerTickCooldown);
    //     Messenger.Broadcast(Signals.REGION_INFO_UI_UPDATE_APPROPRIATE_CONTENT, tileLocation.region);
    // }
    // private void PerTickCooldown() {
    //     currentCooldownTick++;
    //     if (currentCooldownTick == cooldownDuration) {
    //         //coodlown done
    //         StopCooldown();
    //     }
    // }
    // private void StopCooldown() {
    //     Messenger.RemoveListener(Signals.TICK_ENDED, PerTickCooldown);
    //     Messenger.Broadcast(Signals.REGION_INFO_UI_UPDATE_APPROPRIATE_CONTENT, tileLocation.region);
    // }
    // #endregion
}

public class SaveDataTheEye : SaveDataLandmark {
    public int currentCooldownTick;

    public override void Save(BaseLandmark landmark) {
        base.Save(landmark);
        //TheEye eye = landmark as TheEye;
        // currentCooldownTick = eye.currentCooldownTick;
    }
    public override void LoadSpecificLandmarkData(BaseLandmark landmark) {
        base.LoadSpecificLandmarkData(landmark);
        //TheEye eye = landmark as TheEye;
        //eye.LoadSavedData(this);
    }
}

