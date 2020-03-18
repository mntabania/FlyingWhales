using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnkhOfAnubis : Artifact {

    public AnkhOfAnubis() : base(ARTIFACT_TYPE.Ankh_Of_Anubis) {
    }
    public AnkhOfAnubis(SaveDataArtifact data) : base(data) {
    }

    #region Overrides
    public override void ActivateArtifactEffect() {
    }
    #endregion
}
