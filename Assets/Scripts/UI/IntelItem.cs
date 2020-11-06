using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IntelItem : MonoBehaviour {

    public IIntel intel { get; private set; }

    public delegate void OnClickAction(IIntel intel);
    private OnClickAction onClickAction;

    private List<System.Action> otherClickActions;

    [SerializeField] private TextMeshProUGUI infoLbl;
    [SerializeField] private Toggle shareToggle;
    [SerializeField] private LogItem logItem;

    public void SetIntel(IIntel intel) {
        this.intel = intel;
        otherClickActions = new List<System.Action>();
        ClearClickActions();
        SetClickedState(false);
        UpdateIntelText();
        if (intel != null) {
            shareToggle.interactable = true;
            shareToggle.gameObject.SetActive(true);
            // logItem.SetLog(intel.log);
        } else {
            shareToggle.interactable = false;
            shareToggle.gameObject.SetActive(false);
            // logItem.SetLog(null);
        }
        Messenger.AddListener<IIntel>(Signals.INTEL_LOG_UPDATED, OnIntelLogUpdated);
    }

    #region Listeners
    private void OnIntelLogUpdated(IIntel p_intel) {
        if (intel == p_intel) {
            UpdateIntelText();
        }
    }
    #endregion

    #region Text
    private void UpdateIntelText() {
        if (intel != null) {
            infoLbl.text = intel.log.logText;
        } else {
            infoLbl.text = "";
        }
    }
    #endregion
    
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
        shareToggle.SetIsOnWithoutNotify(isClicked);
    }
}
