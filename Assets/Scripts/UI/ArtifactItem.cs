using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZObjectPools;
using TMPro;
using UnityEngine.UI;

public class ArtifactItem : PooledObject {
    public TextMeshProUGUI artifactButtonText;
    public Toggle artifactToggle;

    public ARTIFACT_TYPE artifact { get; private set; }

    public void SetArtifact(ARTIFACT_TYPE artifact) {
        this.artifact = artifact;
        UpdateData();
        Messenger.AddListener<ARTIFACT_TYPE>(Signals.PLAYER_NO_ACTIVE_ARTIFACT, OnPlayerNoActiveArtifact);
    }

    private void UpdateData() {
        artifactButtonText.text = UtilityScripts.Utilities.NotNormalizedConversionEnumToString(artifact.ToString());
    }
    private void OnPlayerNoActiveArtifact(ARTIFACT_TYPE artifact) {
        if(this.artifact == artifact) {
            if (artifactToggle.isOn) {
                artifactToggle.isOn = false;
            }
        }
    }
    public void OnToggleArtifact(bool state) {
        PlayerManager.Instance.player.SetCurrentlyActiveArtifact(ARTIFACT_TYPE.None);
        if (state) {
            PlayerManager.Instance.player.SetCurrentlyActiveArtifact(artifact);
        } 
    }

    public override void Reset() {
        base.Reset();
        artifact = ARTIFACT_TYPE.None;
        Messenger.RemoveListener<ARTIFACT_TYPE>(Signals.PLAYER_NO_ACTIVE_ARTIFACT, OnPlayerNoActiveArtifact);
    }
}
