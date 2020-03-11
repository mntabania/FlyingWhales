using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ArtifactPickerItem : NameplateItem<Artifact> {

    [Header("Artifact Slot Attributes")]    
    [SerializeField] private Image portrait;
    //public GameObject portraitCover;

    private Artifact artifact;
    public override Artifact obj { get { return artifact; } }

    public override void SetObject(Artifact o) {
        base.SetObject(o);
        this.artifact = o;
        UpdateVisuals();
    }

    //public override void SetButtonState(bool state) {
    //    base.SetButtonState(state);
    //    portraitCover.SetActive(!state);
    //}

    private void UpdateVisuals() {
        portrait.sprite = ScriptableObjectsManager.Instance.GetArtifactData(artifact.type).portrait;
        mainLbl.text = artifact.name;
        subLbl.text = string.Empty;
    }
}

