using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;

public class AvailableTargetItemUI : MonoBehaviour {
    public Action<AvailableTargetItemUI> onClicked;
    public Button myButton;

    public RuinarchText txtName;

    public Image imgIcon;
    public IPointOfInterest poi;

    public void InitializeItem(IPointOfInterest p_poi) {
        poi = p_poi;
    }

    private void OnEnable() {
        myButton.onClick.AddListener(Click);
    }

    private void OnDisable() {
        myButton.onClick.RemoveListener(Click);
    }

    void Click() {
        onClicked?.Invoke(this);
    }
}