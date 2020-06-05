using System;
using System.Collections;
using System.Collections.Generic;
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
        Reposition(character);
        rectTransform.SetAsLastSibling();
    }
    private void Update() {
        UpdateText(activeCharacter);
        Reposition(activeCharacter);
    }
    public void Hide() {
        activeCharacter = null;
        gameObject.SetActive(false);
    }
    private void Reposition(Character character) {
        Vector3 screenPoint =
            InnerMapCameraMove.Instance.innerMapsCamera.WorldToScreenPoint(character.marker.transform.position);
        float fovDiff = InnerMapCameraMove.Instance.currentFOV - InnerMapCameraMove.Instance.minFOV;
        float diff = fovDiff / 3.5f;
        screenPoint.y -= (125f - (diff * 12f));
        rectTransform.position = screenPoint;
    }
    private void UpdateText(Character character) {
        thoughtLbl.text = character.visuals.GetThoughtBubble(out var log);
    }
}
