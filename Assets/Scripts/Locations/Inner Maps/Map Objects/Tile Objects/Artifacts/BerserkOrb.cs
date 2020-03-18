using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BerserkOrb : Artifact {

    public BerserkOrb() : base(ARTIFACT_TYPE.Berserk_Orb) {
    }
    public BerserkOrb(SaveDataArtifact data) : base(data) {
    }

    #region Overrides
    public override void ActivateArtifactEffect() {
    }
    #endregion
}
