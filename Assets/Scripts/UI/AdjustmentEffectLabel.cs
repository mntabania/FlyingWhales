using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using EZObjectPools;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class AdjustmentEffectLabel : PooledObject {

    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private GameObject bg;

    public void PlayEffect(string text, Vector2 animationDirection, bool showBG = false) {
        label.text = text;
        label.alpha = 1f;
        label.rectTransform.anchoredPosition = new Vector2(label.rectTransform.anchoredPosition.x + animationDirection.x, label.rectTransform.anchoredPosition.y);
        
        Sequence sequence = DOTween.Sequence();
        sequence.Append(label.rectTransform.DOAnchorPosY(label.rectTransform.anchoredPosition.y + animationDirection.y, 1f));
        sequence.Join(label.DOFade(0f, 1.2f).SetEase(Ease.InQuint));
        sequence.OnComplete(OnComplete);
        sequence.Play();
        
        bg.SetActive(showBG);
    }

    private void OnComplete() {
        ObjectPoolManager.Instance.DestroyObject(this);
    }
}
