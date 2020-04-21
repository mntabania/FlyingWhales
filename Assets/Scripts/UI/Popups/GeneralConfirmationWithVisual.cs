using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class GeneralConfirmationWithVisual : GeneralConfirmation {
    [SerializeField] private RawImage picture;
    
    public void ShowGeneralConfirmation(string header, string body, Texture sprite = null, string buttonText = "OK", System.Action onClickOK = null) {
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
        if (sprite == null) {
            picture.gameObject.SetActive(false);
        } else {
            picture.gameObject.SetActive(true);
            picture.texture = sprite;    
        }

        generalConfirmationButton.onClick.RemoveAllListeners();
        generalConfirmationButton.onClick.AddListener(OnClickOKGeneralConfirmation);
        if (onClickOK != null) {
            generalConfirmationButton.onClick.AddListener(onClickOK.Invoke);
        }
        base.Open();
    }
}
