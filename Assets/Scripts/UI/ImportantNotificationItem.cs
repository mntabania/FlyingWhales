using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ImportantNotificationItem : PooledObject {

    [SerializeField] private TextMeshProUGUI messageLbl;
    [SerializeField] private EasyTween tween;
    [SerializeField] private Image bgImage;

    private System.Action onClickAction;

    public void Initialize(GameDate date, string message, System.Action onClickAction) {
        messageLbl.text = $"[{GameManager.ConvertTickToTime(date.tick)}] {message}";
        this.onClickAction = onClickAction;
        tween.OpenCloseObjectAnimation();
    }

    public void OnPointerClick(BaseEventData eventData) {
        onClickAction?.Invoke();
        //destroy on click
        ObjectPoolManager.Instance.DestroyObject(this);
    }

    public override void Reset() {
        base.Reset();
        tween.enabled = true;
    }

    public void OnPointerEnter(BaseEventData data) {
        tween.enabled = false;
        Color color = bgImage.color;
        color.a = 255f / 255f;
        bgImage.color = color;
    }
    public void OnPointerExit(BaseEventData data) {
        tween.enabled = true;
    }
}
