using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class YesNoConfirmation : PopupMenuBase{
    public GameObject yesNoGO;
    [SerializeField] private CanvasGroup yesNoCanvasGroup;
    [SerializeField] private GameObject yesNoCover;
    [SerializeField] private TextMeshProUGUI yesNoHeaderLbl;
    [SerializeField] private TextMeshProUGUI yesNoDescriptionLbl;
    [SerializeField] private Button yesBtn;
    [SerializeField] private Button noBtn;
    [SerializeField] private Button closeBtn;
    [SerializeField] private TextMeshProUGUI yesBtnLbl;
    [SerializeField] private TextMeshProUGUI noBtnLbl;
    [SerializeField] private HoverHandler yesBtnUnInteractableHoverHandler;

    private System.Action _onHideUIAction;
    
    /// <summary>
    /// Show a yes/no pop up window
    /// </summary>
    /// <param name="header">The title of the window.</param>
    /// <param name="question">The question answerable by yes/no.</param>
    /// <param name="onClickYesAction">The action to perform once the user clicks yes. NOTE: Closing of this window is added by default</param>
    /// <param name="onClickNoAction">The action to perform once the user clicks no. NOTE: Closing of this window is added by default</param>
    /// <param name="showCover">Should this popup also show a cover that covers the game.</param>
    /// <param name="layer">The sorting layer order of this window.</param>
    /// <param name="yesBtnText">The yes button text.</param>
    /// <param name="noBtnText">The no button text.</param>
    /// <param name="yesBtnInteractable">Should the yes button be clickable?</param>
    /// <param name="noBtnInteractable">Should the no button be clickable?</param>
    /// <param name="yesBtnActive">Should the yes button be visible?</param>
    /// <param name="noBtnActive">Should the no button be visible?</param>
    /// <param name="yesBtnInactiveHoverAction">Action to execute when user hover over an un-clickable yes button</param>
    /// <param name="yesBtnInactiveHoverExitAction">Action to execute when user hover over an un-clickable no button</param>
    /// <param name="onClickCloseAction">Action to execute when clicking on close btn. NOTE: Hide action is added by default</param>
    /// <param name="onHideUIAction">Action to execute when popup is closed.</param>
    public void ShowYesNoConfirmation(string header, string question, System.Action onClickYesAction = null, System.Action onClickNoAction = null,
        bool showCover = false, int layer = 21, string yesBtnText = "Yes", string noBtnText = "No", bool yesBtnInteractable = true, bool noBtnInteractable = true, 
        bool yesBtnActive = true, bool noBtnActive = true, System.Action yesBtnInactiveHoverAction = null, System.Action yesBtnInactiveHoverExitAction = null, System.Action onClickCloseAction = null,
        System.Action onHideUIAction = null) {
        
        yesNoHeaderLbl.text = header;
        yesNoDescriptionLbl.text = question;

        yesBtnLbl.text = yesBtnText;
        noBtnLbl.text = noBtnText;

        yesBtn.gameObject.SetActive(yesBtnActive);
        noBtn.gameObject.SetActive(noBtnActive);

        yesBtn.interactable = yesBtnInteractable;
        noBtn.interactable = noBtnInteractable;

        //clear all listeners
        yesBtn.onClick.RemoveAllListeners();
        noBtn.onClick.RemoveAllListeners();
        closeBtn.onClick.RemoveAllListeners();

        //hide confirmation menu on click
        if (UIManager.Instance != null) {
            yesBtn.onClick.AddListener(UIManager.Instance.HideYesNoConfirmation);
            noBtn.onClick.AddListener(UIManager.Instance.HideYesNoConfirmation);
            closeBtn.onClick.AddListener(UIManager.Instance.HideYesNoConfirmation);    
        } else {
            yesBtn.onClick.AddListener(Close);
            noBtn.onClick.AddListener(Close);
            closeBtn.onClick.AddListener(Close);
        }
        

        //specific actions
        if (onClickYesAction != null) {
            yesBtn.onClick.AddListener(onClickYesAction.Invoke);
        }
        if (onClickNoAction != null) {
            noBtn.onClick.AddListener(onClickNoAction.Invoke);
            //closeBtn.onClick.AddListener(onClickNoAction.Invoke);
        }
        if (onClickCloseAction != null) {
            closeBtn.onClick.AddListener(onClickCloseAction.Invoke);
        }

        yesBtnUnInteractableHoverHandler.gameObject.SetActive(!yesBtn.interactable);
        if (yesBtnInactiveHoverAction != null) {
            yesBtnUnInteractableHoverHandler.SetOnHoverOverAction(yesBtnInactiveHoverAction.Invoke);
        }
        if (yesBtnInactiveHoverExitAction != null) {
            yesBtnUnInteractableHoverHandler.SetOnHoverOutAction(yesBtnInactiveHoverExitAction.Invoke);
        }

        _onHideUIAction = onHideUIAction;
        
        yesNoGO.SetActive(true);
        yesNoGO.transform.SetSiblingIndex(layer);
        yesNoCover.SetActive(showCover);
        TweenIn(yesNoCanvasGroup);
    }
    public override void Close() {
        base.Close();
        yesNoGO.SetActive(false);
        _onHideUIAction?.Invoke();
    }
    private void TweenIn(CanvasGroup canvasGroup) {
        canvasGroup.alpha = 0;
        RectTransform rectTransform = canvasGroup.transform as RectTransform; 
        rectTransform.anchoredPosition = new Vector2(0f, -30f);
        
        Sequence sequence = DOTween.Sequence();
        sequence.Append(rectTransform.DOAnchorPos(Vector2.zero, 0.5f).SetEase(Ease.OutBack));
        sequence.Join(DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 1f, 0.5f)
            .SetEase(Ease.InSine));
        sequence.PrependInterval(0.2f);
        sequence.Play();
    }    
}
