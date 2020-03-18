using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Necronomicon : Artifact {

    public Necronomicon() : base(ARTIFACT_TYPE.Necronomicon) {
    }
    public Necronomicon(SaveDataArtifact data) : base(data) {
    }

    #region Overrides
    public override void ActivateArtifactEffect() {
    }
    #endregion
}
