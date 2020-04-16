using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ChangeOpacityOnHoverImage : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    [SerializeField] private Image targetImage;
    [Range(0f, 1f)]
    [SerializeField] private float targetOpacity;
    private float _originalOpacity;
    private void Awake() {
        if (targetImage == null) {
            targetImage = GetComponent<Image>();
        }
        _originalOpacity = targetImage.color.a;
    }
    public void OnPointerEnter(PointerEventData eventData) {
        DOTween.ToAlpha(GetColor, SetColor, targetOpacity, 0.1f);
    }
    public void OnPointerExit(PointerEventData eventData) {
        DOTween.ToAlpha(GetColor, SetColor, _originalOpacity, 0.1f);
    }

    private void SetColor(Color color) {
        targetImage.color = color;
    }
    private Color GetColor() {
        return targetImage.color;
    }
}
