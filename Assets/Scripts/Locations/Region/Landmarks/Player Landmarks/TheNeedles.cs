using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Obsolete("Player landmarks should no longer be used, use the LocationStructure version instead.")]
public class TheNeedles : BaseLandmark {

    // public int currentCooldownTick { get; private set; }
    // public int cooldownDuration { get; private set; }
    // public bool isInCooldown {
    //     get { return currentCooldownTick < cooldownDuration; }
    // }

    public TheNeedles(HexTile location, LANDMARK_TYPE specificLandmarkType) : base(location, specificLandmarkType) {
        // cooldownDuration = GameManager.Instance.GetTicksBasedOnHour(4);
        // currentCooldownTick = cooldownDuration;
    }

    public TheNeedles(HexTile location, SaveDataLandmark data) : base(location, data) {
        // cooldownDuration = GameManager.Instance.GetTicksBasedOnHour(4);
    }

    public void LoadSavedData(SaveDataTheNeedles data) {
        // if (data.currentCooldownTick < cooldownDuration) {
        //     StartCooldown();
        // }
        // currentCooldownTick = data.currentCooldownTick;
    }

    // public void Activate() {
    //     Minion minion = tileLocation.region.assignedMinion;
    //     int gainedMana = GetManaValue(minion);
    //     PlayerManager.Instance.player.AdjustMana(gainedMana);
    //     tileLocation.region.SetAssignedMinion(null);
    //     minion.SetAssignedRegion(null);
    //     minion.character.Death("ConvertedMana");
    //     StartCooldown();
    //     Messenger.Broadcast(Signals.REGION_INFO_UI_UPDATE_APPROPRIATE_CONTENT, tileLocation.region);
    // }

    // #region Overrides
    // public override void DestroyLandmark() {
    //     StopCooldown();
    //     base.DestroyLandmark();
    // }
    // #endregion
    //
    // private void StartCooldown() {
    //     currentCooldownTick = 0;
    //     Messenger.AddListener(Signals.TICK_ENDED, PerTickCooldown);
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
    //
    // public int GetManaValue(Minion minion) {
    //    return minion.character.level * 100;
    // }
}

public class SaveDataTheNeedles : SaveDataLandmark {
    public int currentCooldownTick;

    public override void Save(BaseLandmark landmark) {
        base.Save(landmark);
        //TheNeedles needles = landmark as TheNeedles;
        // currentCooldownTick = needles.currentCooldownTick;
    }
    public override void LoadSpecificLandmarkData(BaseLandmark landmark) {
        base.LoadSpecificLandmarkData(landmark);
        //TheNeedles needles = landmark as TheNeedles;
        //needles.LoadSavedData(this);
    }
}