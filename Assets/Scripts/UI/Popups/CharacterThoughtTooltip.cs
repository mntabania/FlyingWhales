using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

public class CharacterThoughtTooltip : MonoBehaviour {

    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private TextMeshProUGUI thoughtLbl;

    public Character activeCharacter { get; private set; }
    
    public void Show(Character character) {
        activeCharacter = character;
        gameObject.SetActive(true);
        UpdateText(character);
        rectTransform.SetAsLastSibling();
        Reposition(character);
    }
    private void Update() {
        if (activeCharacter != null) {
            UpdateText(activeCharacter);
            Reposition(activeCharacter);    
        }
    }
    public void Hide() {
        activeCharacter = null;
        gameObject.SetActive(false);
    }
    private void Reposition([NotNull]Character character) {
        Vector3 screenPoint =
            InnerMapCameraMove.Instance.innerMapsCamera.WorldToScreenPoint(character.marker.transform.position);
        float fovDiff = InnerMapCameraMove.Instance.currentFOV - InnerMapCameraMove.Instance.minFOV;
        float diff = fovDiff / 3.5f;
        screenPoint.y -= (125f - (diff * 12f));
        rectTransform.position = screenPoint;
    }
    private void UpdateText([NotNull]Character character) {
        thoughtLbl.text = character.visuals.GetThoughtBubble(out var log);
    }
}
