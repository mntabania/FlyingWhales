using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Allows 1 additional Artifact slot. If destroyed and there is an occupant Artifact, player must choose which Artifact to discard.
/// Ref: https://trello.com/c/EaUHzTaK/2690-the-crypt
/// </summary>
[Obsolete("Player landmarks should no longer be used, use the LocationStructure version instead.")]
public class TheCrypt : BaseLandmark {
    public TheCrypt(HexTile location, LANDMARK_TYPE specificLandmarkType) : base(location, specificLandmarkType) { }
    public TheCrypt(HexTile location, SaveDataLandmark data) : base(location, data) { }

    #region Overrides
    public override void OnFinishedBuilding() {
        base.OnFinishedBuilding();
        // for (int i = 0; i < tileLocation.featureComponent.features.Count; i++) {
        //     TileFeature feature = tileLocation.featureComponent.features[i];
        //     if(feature is ArtifactFeature) {
        //         ArtifactFeature artifactFeature = feature as ArtifactFeature;
        //         if(artifactFeature.artifact != null) {
        //             PlayerManager.Instance.player.GainArtifact(artifactFeature.artifact);
        //         }
        //     }
        // }
        //PlayerManager.Instance.player.IncreaseArtifactSlot();
    }
    //public override void DestroyLandmark() {
    //    base.DestroyLandmark();
    //    PlayerManager.Instance.player.DecreaseArtifactSlot();
    //}
    #endregion
}
