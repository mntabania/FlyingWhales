using DG.Tweening;
using TMPro;
using UnityEngine;

public class BottomNotification : MonoBehaviour {

    [SerializeField] private RectTransform thisRect;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI messageLbl;

    public void ShowMessage(string message) {
        messageLbl.text = message;
        thisRect.anchoredPosition = new Vector2(0f, -110);
        thisRect.DOAnchorPosY(50f, 0.5f).SetEase(Ease.OutBack);
    }
    public void HideMessage() {
        thisRect.DOAnchorPosY(-110f, 0.5f).SetEase(Ease.InBack);
    }

}
