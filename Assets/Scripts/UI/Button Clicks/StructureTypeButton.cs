using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StructureTypeButton : MonoBehaviour {
    public Text buttonText;

    public STRUCTURE_TYPE structureType { get; private set; }


    public void SetCurrentlySelectedButton() {
        ClassPanelUI.Instance.currentSelectedRelatedStructuresButton = this;
    }
    public void SetStructureType(STRUCTURE_TYPE structure) {
        structureType = structure;
        buttonText.text = structureType.ToString();
    }
}
