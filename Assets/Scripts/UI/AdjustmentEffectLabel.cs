using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using EZObjectPools;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class AdjustmentEffectLabel : PooledObject {

    [SerializeField] private TextMeshProUGUI label;

    public void PlayEffect(string text, Vector2 animationDirection) {
        label.text = text;
        
        label.alpha = 1f;
        
        Sequence sequence = DOTween.Sequence();
        sequence.Append(label.rectTransform.DOAnchorPos(
            label.rectTransform.anchoredPosition + animationDirection, 1f));
        sequence.Join(label.DOFade(0f, 1.2f).SetEase(Ease.InQuint));
        sequence.OnComplete(OnComplete);
        sequence.Play();
    }

    private void OnComplete() {
        ObjectPoolManager.Instance.DestroyObject(this);
    }
}
