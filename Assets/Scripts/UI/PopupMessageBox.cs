using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PopupMessageBox : InfoUIBase{

    public static PopupMessageBox Instance = null;

    [SerializeField] private EnvelopContentUnityUI envelopContent;
    [SerializeField] private TextMeshProUGUI messageLbl;
    [SerializeField] private EasyTween tweener;

    private void Awake() {
        Instance = this;
    }

    internal override void Initialize() {
        Messenger.AddListener<string, bool>(Signals.SHOW_POPUP_MESSAGE, ShowMessage);
        Messenger.AddListener(Signals.HIDE_POPUP_MESSAGE, HideMessage);
    }

    private void ShowMessage(string message, bool autoHide) {
        messageLbl.text = message;
        envelopContent.Execute();

        if (!isShowing) {
            isShowing = true;
            tweener.TriggerOpenClose();
            if (autoHide) {
                StartCoroutine(AutoHideAfterSeconds());
            }
        }
        
    }

    private IEnumerator AutoHideAfterSeconds() {
        yield return new WaitForSeconds(2f);
        HideMessage();
    }

    public void SetYesAction(UnityAction yesAction) {
        //yesBtn.onClick.RemoveAllListeners();
        AddYesAction(yesAction);
    }
    public void AddYesAction(UnityAction yesAction) {
        //yesBtn.onClick.AddListener(yesAction);
    }
    public void SetNoAction(UnityAction noAction) {
        //noBtn.onClick.RemoveAllListeners();
        AddNoAction(noAction);
    }
    public void AddNoAction(UnityAction noAction) {
        //noBtn.onClick.AddListener(noAction);
    }

    private void HideMessage() {
        isShowing = false;
        tweener.TriggerOpenClose();
        //tweenPosition.from = showingPos;
        //tweenPosition.to = hiddenPos;
        //tweenPosition.ResetToBeginning();
        //tweenPosition.PlayForward();
    }
}
