using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GeneralConfirmation : PopupMenuBase {

    [SerializeField] protected TextMeshProUGUI generalConfirmationTitleText;
    [SerializeField] protected TextMeshProUGUI generalConfirmationBodyText;
    [SerializeField] protected Button generalConfirmationButton;
    [SerializeField] protected TextMeshProUGUI generalConfirmationButtonText;
    
    public virtual void ShowGeneralConfirmation(string header, string body, string buttonText = "OK", System.Action onClickOK = null) {
        if (PlayerUI.Instance.IsMajorUIShowing()) {
            PlayerUI.Instance.AddPendingUI(() => ShowGeneralConfirmation(header, body, buttonText, onClickOK));
            return;
        }
        if (!GameManager.Instance.isPaused) {
            UIManager.Instance.Pause();
            UIManager.Instance.SetSpeedTogglesState(false);
        }
        generalConfirmationTitleText.text = header.ToUpper();
        generalConfirmationBodyText.text = body;
        generalConfirmationButtonText.text = buttonText;
        generalConfirmationButton.onClick.RemoveAllListeners();
        generalConfirmationButton.onClick.AddListener(OnClickOKGeneralConfirmation);
        if (onClickOK != null) {
            generalConfirmationButton.onClick.AddListener(onClickOK.Invoke);
        }
        base.Open();
    }
    public void OnClickOKGeneralConfirmation() {
        Close();    
    }
    public override void Close() {
        base.Close();
        if (!PlayerUI.Instance.TryShowPendingUI()) {
            UIManager.Instance.ResumeLastProgressionSpeed(); //if no other UI was shown, unpause game
        }
    }
}
