using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IntelItem : MonoBehaviour {

    public Intel intel { get; private set; }

    public delegate void OnClickAction(Intel intel);
    private OnClickAction onClickAction;

    private List<System.Action> otherClickActions;

    [SerializeField] private TextMeshProUGUI infoLbl;
    [SerializeField] private Button mainBtn;
    [SerializeField] private Image iconImg;
    [SerializeField] private Image clickedImg;

    [SerializeField] private Sprite eventIntelIcon;
    [SerializeField] private Sprite objectIntelIcon;

    public void SetIntel(Intel intel) {
        this.intel = intel;
        otherClickActions = new List<System.Action>();
        ClearClickActions();
        SetClickedState(false);
        if (intel != null) {
            iconImg.sprite = objectIntelIcon;
            //if (intel is EventIntel) {
            //    iconImg.sprite = eventIntelIcon;
            //} 
            //infoLbl.text = Utilities.LogReplacer(intel.intelLog);
            infoLbl.text = UtilityScripts.Utilities.LogReplacer(intel.node.descriptionLog);
            mainBtn.interactable = true;
            iconImg.gameObject.SetActive(true);
        } else {
            infoLbl.text = "";
            mainBtn.interactable = false;
            iconImg.gameObject.SetActive(false);
        }
    }
    public void ShowLogDebugInfo() {
        if (intel != null) {
            //string text = intel.GetDebugInfo();
            //UIManager.Instance.ShowSmallInfo(text);
        }
    }
    public void HideLogDebugInfo() {
        UIManager.Instance.HideSmallInfo();
    }
    public void SetClickAction(OnClickAction clickAction) {
        onClickAction = clickAction;
    }
    public void AddOtherClickAction(System.Action clickAction) {
        if (otherClickActions != null) {
            otherClickActions.Add(clickAction);
        }
    }

    public void OnClick() {
        onClickAction?.Invoke(intel);
        for (int i = 0; i < otherClickActions.Count; i++) {
            otherClickActions[i]();
        }
    }

    public void ClearClickActions() {
        onClickAction = null;
        otherClickActions.Clear();
    }

    public void SetClickedState(bool isClicked) {
        clickedImg.gameObject.SetActive(isClicked);
    }
}
