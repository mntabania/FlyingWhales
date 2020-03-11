using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Allows 1 additional Summon slot. If destroyed and there is an occupant Summon, player must choose which Summon to discard.
/// Ref: https://trello.com/c/Mcy1gAfq/2691-the-kennel
/// </summary>
[Obsolete("Player landmarks should no longer be used, use the LocationStructure version instead.")]
public class TheKennel : BaseLandmark {

    public TheKennel(HexTile location, LANDMARK_TYPE specificLandmarkType) : base(location, specificLandmarkType) { }
    public TheKennel(HexTile location, SaveDataLandmark data) : base(location, data) { }

    #region Overrides
    public override void OnFinishedBuilding() {
        base.OnFinishedBuilding();
        // for (int i = 0; i < tileLocation.region.charactersAtLocation.Count; i++) {
        //     Character character = tileLocation.region.charactersAtLocation[i];
        //     if (character is Summon) {
        //         character.ChangeFactionTo(PlayerManager.Instance.player.playerFaction);
        //     }
        // }
        //PlayerManager.Instance.player.IncreaseSummonSlot();
    }
    //public override void DestroyLandmark() {
    //    base.DestroyLandmark();
    //    //PlayerManager.Instance.player.DecreaseSummonSlot();
    //}
    #endregion
}
