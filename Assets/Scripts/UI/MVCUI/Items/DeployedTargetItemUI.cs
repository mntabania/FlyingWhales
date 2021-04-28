using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;

public class DeployedTargetItemUI : MonoBehaviour {
    public Action<DeployedTargetItemUI> onDeleteClick;
    public Action<DeployedTargetItemUI> onHoverOver;
    public Action<DeployedTargetItemUI> onHoverOut;
    public Button btnDelete;
    public Button btnItem;
    public RuinarchText txtName;

    public Image imgIcon;
    public IStoredTarget target;

    public bool isDeployed;
    public bool isReadyForDeploy;
    public HoverHandler hoverHandler;
    
    public Sprite[] icons;

    public void InitializeItem(IStoredTarget p_target, bool p_isDeployed = false) {
        txtName.text = $"{p_target.iconRichText} {p_target.name}";
        target = p_target;
        if (p_isDeployed) {
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
        HideRemoveButton();
    }

    private void OnEnable() {
        btnDelete.onClick.AddListener(OnDeleteClick);
        btnItem.onClick.AddListener(OnItemClicked);
        hoverHandler.AddOnHoverOverAction(OnHoverOver);
        hoverHandler.AddOnHoverOutAction(OnHoverOut);
    }

    private void OnDisable() {
        btnDelete.onClick.RemoveListener(OnDeleteClick);
        btnItem.onClick.RemoveListener(OnItemClicked);
        hoverHandler.RemoveOnHoverOverAction(OnHoverOver);
        hoverHandler.RemoveOnHoverOutAction(OnHoverOut);
    }

    public void HideRemoveButton() {
        btnDelete.gameObject.SetActive(false);
    }

    public void ShowRemoveButton() {
        btnDelete.gameObject.SetActive(true);
    }

    public void ResetButton() {
        isDeployed = false;
        isReadyForDeploy = false;
    }

    void OnDeleteClick() {
        onDeleteClick?.Invoke(this);
    }

    void OnItemClicked() {
        UIManager.Instance.OpenObjectUI(target);
    }
    private void OnHoverOver() {
        onHoverOver?.Invoke(this);
    }
    private void OnHoverOut() {
        onHoverOut?.Invoke(this);
    }
}