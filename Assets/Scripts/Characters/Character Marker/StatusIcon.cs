using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusIcon : MonoBehaviour {
    [SerializeField] private SpriteRenderer _icon;

    public void SetIcon(Sprite icon) {
        _icon.sprite = icon;
    }
}
