using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using TMPro;
using UnityEngine;

public class MarkerDummy : MonoBehaviour {

    [SerializeField] private SpriteRenderer _fill;
    [SerializeField] private TextMeshPro label;

    public void InitialSetup(Sprite sprite, LocationGridTile positionAt) {
        _fill.sprite = sprite;
        label.text = "0%";
        Color newColor = _fill.color;
        newColor.a = 0f/255f;
        _fill.color = newColor;
        this.transform.position = positionAt.centeredWorldLocation;
    }

    public void SetProgress(float percent) {
        Color newColor = _fill.color;
        newColor.a = (255f * percent)/255f;
        _fill.color = newColor;
        label.text = $"{(percent * 100f):F0}%";
    }

    public void Deactivate() {
        gameObject.SetActive(false);
    }
    public void Activate() {
        gameObject.SetActive(true);
    }
}
