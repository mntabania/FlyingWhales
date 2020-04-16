using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PriorityJobButton : MonoBehaviour {
    public Text buttonText;

    public JOB_TYPE jobType { get; private set; }
    public string identifier { get; private set; }

    public void SetCurrentlySelectedButton() {
        if(identifier == "priority") {
            ClassPanelUI.Instance.currentSelectedPriorityJobButton = this;
        } else if (identifier == "secondary") {
            ClassPanelUI.Instance.currentSelectedSecondaryJobButton = this;
        } else if (identifier == "able") {
            ClassPanelUI.Instance.currentSelectedAbleJobButton = this;
        }
    }
    public void SetJobType(JOB_TYPE jobType, string identifier) {
        this.jobType = jobType;
        this.identifier = identifier;
        buttonText.text = jobType.ToString();
    }
}
