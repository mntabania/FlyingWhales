using System;
using Coffee.UIExtensions;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SpecialBookmarkTextItemUI : BookmarkTextItemUI {
    [SerializeField] private UIShiny borderShineEffect;
    [SerializeField] private Image glowEffect;
    private void OnEnable() {
        glowEffect.DOFade(0f, 2f).SetEase(Ease.InQuart).SetLoops(-1, LoopType.Yoyo);
    }
    private void OnDisable() {
        glowEffect.DOKill(true);
        Color color = glowEffect.color;
        color.a = 1f;
        glowEffect.color = color;
    }
}