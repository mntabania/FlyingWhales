using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GeneralConfirmation : PopupMenuBase {

    [SerializeField] protected RuinarchText generalConfirmationTitleText;
    [SerializeField] protected RuinarchText generalConfirmationBodyText;
    [SerializeField] protected Button generalConfirmationButton;
    [SerializeField] protected RuinarchText generalConfirmationButtonText;
    [SerializeField] protected CanvasGroup _canvasGroup;
    [SerializeField] protected Button _centerButton;
    
    public virtual void ShowGeneralConfirmation(string header, string body, string buttonText = "OK", System.Action onClickOK = null, System.Action onClickCenter = null) {
        if (PlayerUI.Instance != null && PlayerUI.Instance.IsMajorUIShowing()) {
            PlayerUI.Instance.AddPendingUI(() => ShowGeneralConfirmation(header, body, buttonText, onClickOK, onClickCenter));
            return;
        }

        if (UIManager.Instance != null) {
            if (!UIManager.Instance.IsObjectPickerOpen()) {
                //if object picker is already being shown 
                UIManager.Instance.Pause();
                UIManager.Instance.SetSpeedTogglesState(false);
            }
            UIManager.Instance.HideSmallInfo();    
        }
        
        generalConfirmationTitleText.SetTextAndReplaceWithIcons(header.ToUpper());
        generalConfirmationBodyText.SetTextAndReplaceWithIcons(body);
        generalConfirmationButtonText.SetTextAndReplaceWithIcons(buttonText);
        _centerButton.onClick.RemoveAllListeners();
        if (onClickCenter != null) {
            _centerButton.gameObject.SetActive(true);
            _centerButton.onClick.AddListener(OnClickOKGeneralConfirmation);
            _centerButton.onClick.AddListener(onClickCenter.Invoke);
        } else {
            _centerButton.gameObject.SetActive(false);
        }
        base.Open();
        LayoutRebuilder.ForceRebuildLayoutImmediate(_canvasGroup.transform as RectTransform);
        generalConfirmationButton.onClick.RemoveAllListeners();
        generalConfirmationButton.onClick.AddListener(OnClickOKGeneralConfirmation);
        if (onClickOK != null) {
            generalConfirmationButton.onClick.AddListener(onClickOK.Invoke);
        }
        transform.SetAsLastSibling();
        TweenIn();
    }
    
    private void TweenIn() {
        _canvasGroup.alpha = 0;
        RectTransform rectTransform = _canvasGroup.transform as RectTransform; 
        rectTransform.anchoredPosition = new Vector2(0f, -30f);
        
        Sequence sequence = DOTween.Sequence();
        sequence.Append(rectTransform.DOAnchorPos(Vector2.zero, 0.5f).SetEase(Ease.OutBack));
        sequence.Join(DOTween.To(() => _canvasGroup.alpha, x => _canvasGroup.alpha = x, 1f, 0.5f)
            .SetEase(Ease.InSine));
        sequence.PrependInterval(0.2f);
        sequence.Play();
    }
    
    public void OnClickOKGeneralConfirmation() {
        Close();    
    }
    public override void Close() {
        base.Close();
        if (PlayerUI.Instance != null && UIManager.Instance != null && !PlayerUI.Instance.TryShowPendingUI() && !UIManager.Instance.IsObjectPickerOpen()) {
            UIManager.Instance.ResumeLastProgressionSpeed(); //if no other UI was shown and object picker is not open, unpause game
        }
    }
}
