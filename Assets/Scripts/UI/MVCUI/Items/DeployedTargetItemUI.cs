using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;

public class DeployedTargetItemUI : MonoBehaviour {
    public Action<DeployedTargetItemUI> onClicked;
    public Button myButton;

    public RuinarchText txtName;

    public Image imgIcon;
    public IStoredTarget target;

    public bool isDeployed;
    public bool isReadyForDeploy;

    public void InitializeItem(IStoredTarget p_target, bool p_isDeployed = false) {
        txtName.text = p_target.name;
        target = p_target;
        if (p_isDeployed) {
            HideRemoveButton();
            Deploy();
        } else {
            isDeployed = false;
            isReadyForDeploy = true;
        }
    }

    public void UndeployCharacter() {
        target = null;
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