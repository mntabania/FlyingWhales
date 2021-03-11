using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;

public class DeployedTargetItemUI : MonoBehaviour {
    public Action<DeployedTargetItemUI> onClicked;
    public Button myButton;

    public RuinarchText txtName;

    public Image imgIcon;
    public IPointOfInterest poi;

    public bool isDeployed;
    public bool isReadyForDeploy;

    public void InitializeItem(IPointOfInterest p_poi, bool p_isDeployed = false) {
        poi = p_poi;
        if (p_isDeployed) {
            Deploy();
        } else {
            isDeployed = false;
            isReadyForDeploy = true;
        }
    }

    public void Deploy() {
        isDeployed = true;
        isReadyForDeploy = false;
    }

    private void OnEnable() {
        myButton.onClick.AddListener(Click);
    }

    private void OnDisable() {
        myButton.onClick.RemoveListener(Click);
    }

    public void HideRemoveButton() {
        myButton.gameObject.SetActive(false);
    }

    public void ShowRemoveButton() {
        myButton.gameObject.SetActive(true);
    }

    public void ResetButton() {
        isDeployed = false;
        isReadyForDeploy = false;
    }

    void Click() {
        onClicked?.Invoke(this);
    }
}